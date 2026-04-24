struct FluidVertexInput {
    position: vec2<f32>,
    material_id: i32,
    color: vec3<f32>,
    velocity: vec2<f32>,
    pressure: f32,
}

struct FluidVertexOutput {
    position: vec4<f32>,
    material_id: i32,
    color: vec3<f32>,
    world_pos: vec2<f32>,
    velocity: vec2<f32>,
    pressure: f32,
}

struct FluidFragmentOutput {
    color: vec4<f32>,
    normal: vec4<f32>,
}

[Vertex]
micro vs_main(input: FluidVertexInput) -> FluidVertexOutput {
    let mut output: FluidVertexOutput;
    output.position = uniforms.mvp * vec4<f32>(input.position, 0.0, 1.0);
    output.material_id = input.material_id;
    output.color = input.color;
    output.world_pos = input.position;
    output.velocity = input.velocity;
    output.pressure = input.pressure;
    return output;
}

[Fragment]
micro fs_main(input: FluidVertexOutput) -> FluidFragmentOutput {
    let mut output: FluidFragmentOutput;

    let base_color = input.color;

    let flow_distortion = vec2<f32>(
        sin(input.world_pos.x * 8.0 + uniforms.time * 3.0 + input.velocity.x * 2.0) * 0.02,
        cos(input.world_pos.y * 8.0 + uniforms.time * 2.0 + input.velocity.y * 2.0) * 0.02
    );

    let distorted_uv = input.world_pos + flow_distortion;

    let flow_intensity = length(input.velocity) * 0.01;

    if input.material_id == MATERIAL_WATER {
        let depth_darken = clamp(input.pressure * 0.1, 0.0, 0.4);
        let wave = sin(distorted_uv.x * 10.0 + uniforms.time * 3.0) * 0.03;
        let r = 0.12 + wave - depth_darken;
        let g = 0.42 + wave - depth_darken;
        let b = 0.63 + wave * 2.0 - depth_darken * 0.5;
        base_color = vec3<f32>(clamp(r, 0.0, 1.0), clamp(g, 0.0, 1.0), clamp(b, 0.0, 1.0));

        let surface_normal = vec3<f32>(
            sin(distorted_uv.x * 15.0 + uniforms.time * 4.0) * 0.1,
            1.0,
            cos(distorted_uv.y * 15.0 + uniforms.time * 3.0) * 0.1
        );
        output.normal = vec4<f32>(normalize(surface_normal), 1.0);
    }
    else if input.material_id == MATERIAL_OIL {
        let sheen = sin(distorted_uv.x * 5.0 + distorted_uv.y * 5.0 + uniforms.time) * 0.05;
        base_color = vec3<f32>(0.31 + sheen, 0.24 + sheen, 0.16 + sheen);
        output.normal = vec4<f32>(0.0, 1.0, 0.0, 1.0);
    }
    else if input.material_id == MATERIAL_ACID {
        let bubble = sin(distorted_uv.x * 12.0 + uniforms.time * 5.0) * 0.08;
        base_color = vec3<f32>(0.3 + bubble, 1.0, 0.1 + bubble);
        output.normal = vec4<f32>(0.0, 1.0, 0.0, 1.0);
    }
    else if input.material_id == MATERIAL_LAVA {
        let pulse = sin(distorted_uv.x * 4.0 + distorted_uv.y * 3.0 + uniforms.time * 2.0) * 0.15 + 0.85;
        let crack = sin(distorted_uv.x * 20.0 + distorted_uv.y * 20.0 + uniforms.time * 0.5);
        if crack > 0.9 {
            base_color = vec3<f32>(1.0, 0.8 * pulse, 0.0);
        }
        else {
            base_color = vec3<f32>(0.9 * pulse, 0.15 * pulse, 0.0);
        }
        output.normal = vec4<f32>(0.0, 1.0, 0.0, 1.0);
    }
    else {
        output.normal = vec4<f32>(0.0, 1.0, 0.0, 1.0);
    }

    let highlight = flow_intensity * 0.3;
    base_color = base_color + vec3<f32>(highlight, highlight, highlight);

    output.color = vec4<f32>(base_color, 0.9);
    return output;
}
