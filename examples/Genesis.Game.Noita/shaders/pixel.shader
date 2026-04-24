struct PixelVertexInput {
    position: vec2<f32>,
    material_id: i32,
    color: vec3<f32>,
}

struct PixelVertexOutput {
    position: vec4<f32>,
    material_id: i32,
    color: vec3<f32>,
    world_pos: vec2<f32>,
}

struct PixelFragmentOutput {
    color: vec4<f32>,
    glow: vec4<f32>,
}

[Vertex]
micro vs_main(input: PixelVertexInput) -> PixelVertexOutput {
    let mut output: PixelVertexOutput;
    output.position = uniforms.mvp * vec4<f32>(input.position, 0.0, 1.0);
    output.material_id = input.material_id;
    output.color = input.color;
    output.world_pos = input.position;
    return output;
}

[Fragment]
micro fs_main(input: PixelVertexOutput) -> PixelFragmentOutput {
    let mut output: PixelFragmentOutput;

    let base_color = input.color;

    if input.material_id == MATERIAL_FIRE {
        let flicker = sin(input.world_pos.x * 10.0 + uniforms.time * 8.0) * 0.15 + 0.85;
        base_color = vec3<f32>(1.0, 0.4 * flicker, 0.0);
        output.glow = vec4<f32>(1.0, 0.3, 0.0, 0.6);
    }
    else if input.material_id == MATERIAL_LAVA {
        let pulse = sin(input.world_pos.x * 5.0 + input.world_pos.y * 3.0 + uniforms.time * 3.0) * 0.1 + 0.9;
        base_color = vec3<f32>(1.0 * pulse, 0.2 * pulse, 0.0);
        output.glow = vec4<f32>(1.0, 0.15, 0.0, 0.4);
    }
    else if input.material_id == MATERIAL_ACID {
        let bubble = sin(input.world_pos.x * 8.0 + uniforms.time * 4.0) * 0.1;
        base_color = vec3<f32>(0.3 + bubble, 1.0, 0.1 + bubble);
        output.glow = vec4<f32>(0.2, 0.8, 0.0, 0.3);
    }
    else if input.material_id == MATERIAL_STEAM {
        let fade = 0.5 + sin(uniforms.time * 2.0) * 0.2;
        base_color = vec3<f32>(0.8 * fade, 0.8 * fade, 0.9 * fade);
        output.glow = vec4<f32>(0.0, 0.0, 0.0, 0.0);
    }
    else if input.material_id == MATERIAL_SMOKE {
        let drift = sin(input.world_pos.x * 3.0 + uniforms.time * 1.5) * 0.1;
        base_color = vec3<f32>(0.3 + drift, 0.3 + drift, 0.3 + drift);
        output.glow = vec4<f32>(0.0, 0.0, 0.0, 0.0);
    }
    else if input.material_id == MATERIAL_WATER {
        let wave = sin(input.world_pos.x * 6.0 + uniforms.time * 2.0) * 0.05;
        base_color = vec3<f32>(0.12 + wave, 0.42 + wave, 0.63 + wave * 2.0);
        output.glow = vec4<f32>(0.0, 0.0, 0.0, 0.0);
    }
    else if input.material_id == MATERIAL_GOLD {
        let sparkle = sin(input.world_pos.x * 20.0 + input.world_pos.y * 20.0 + uniforms.time * 5.0) * 0.1 + 0.9;
        base_color = vec3<f32>(1.0 * sparkle, 0.84 * sparkle, 0.0);
        output.glow = vec4<f32>(0.5, 0.4, 0.0, 0.2);
    }
    else {
        output.glow = vec4<f32>(0.0, 0.0, 0.0, 0.0);
    }

    output.color = vec4<f32>(base_color, 1.0);
    return output;
}
