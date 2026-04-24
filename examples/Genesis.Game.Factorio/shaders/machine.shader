struct MachineVertexInput {
    position: vec2<f32>,
    machine_type: i32,
    progress: f32,
    recipe_id: i32,
    has_power: bool,
}

struct MachineVertexOutput {
    position: vec4<f32>,
    uv: vec2<f32>,
    machine_color: vec3<f32>,
    progress: f32,
    has_power: bool,
    machine_type: i32,
    world_pos: vec2<f32>,
}

let FURNACE_COLOR: vec3<f32> = vec3<f32>(0.70, 0.30, 0.20);
let ASSEMBLER_COLOR: vec3<f32> = vec3<f32>(0.40, 0.60, 0.80);
let BOILER_COLOR: vec3<f32> = vec3<f32>(0.50, 0.50, 0.50);
let STEAM_ENGINE_COLOR: vec3<f32> = vec3<f32>(0.60, 0.65, 0.70);
let NO_POWER_COLOR: vec3<f32> = vec3<f32>(0.30, 0.15, 0.15);

[Vertex]
micro vs_main(input: MachineVertexInput) -> MachineVertexOutput {
    let mut output: MachineVertexOutput;
    let tile_size = 32.0;
    let world_x = input.position.x * tile_size;
    let world_y = input.position.y * tile_size;
    output.position = uniforms.mvp * vec4<f32>(world_x, world_y, 0.0, 1.0);
    output.uv = vec2<f32>(input.position.x, input.position.y);
    output.progress = input.progress;
    output.has_power = input.has_power;
    output.machine_type = input.machine_type;
    output.world_pos = vec2<f32>(world_x, world_y);

    if !input.has_power {
        output.machine_color = NO_POWER_COLOR;
    }
    else if input.machine_type == 0 {
        output.machine_color = FURNACE_COLOR;
    }
    else if input.machine_type == 1 {
        output.machine_color = ASSEMBLER_COLOR;
    }
    else if input.machine_type == 2 {
        output.machine_color = BOILER_COLOR;
    }
    else if input.machine_type == 3 {
        output.machine_color = STEAM_ENGINE_COLOR;
    }
    else {
        output.machine_color = vec3<f32>(0.5, 0.5, 0.5);
    }

    return output;
}

[Fragment]
micro fs_main(input: MachineVertexOutput) -> vec4<f32> {
    let tile_size = 32.0;
    let local_x = fract(input.world_pos.x / tile_size);
    let local_y = fract(input.world_pos.y / tile_size);

    let base_color = input.machine_color;

    let border = step(local_x, 0.08) + step(0.92, local_x) + step(local_y, 0.08) + step(0.92, local_y);
    let bordered_color = mix(base_color, base_color * 0.5, min(border, 1.0));

    let progress_bar_y = step(0.85, local_y) * step(local_y, 0.95);
    let progress_fill = step(local_x, input.progress);
    let progress_color = mix(vec3<f32>(0.2, 0.2, 0.2), vec3<f32>(0.2, 0.9, 0.2), progress_fill);
    let final_color = mix(bordered_color, progress_color, progress_bar_y);

    if input.machine_type == 0 {
        let glow = step(0.3, local_x) * step(local_x, 0.7) * step(0.3, local_y) * step(local_y, 0.6);
        let furnace_glow = mix(vec3<f32>(0.0, 0.0, 0.0), vec3<f32>(1.0, 0.5, 0.1), glow * input.progress * 0.6);
        return vec4<f32>(final_color + furnace_glow, 1.0);
    }

    if input.machine_type == 1 {
        let gear1_center = vec2<f32>(0.35, 0.45);
        let gear2_center = vec2<f32>(0.65, 0.45);
        let d1 = distance(vec2<f32>(local_x, local_y), gear1_center);
        let d2 = distance(vec2<f32>(local_x, local_y), gear2_center);
        let gear = step(d1, 0.12) + step(d2, 0.12);
        let gear_color = mix(vec3<f32>(0.0, 0.0, 0.0), vec3<f32>(0.7, 0.7, 0.75), min(gear, 1.0) * 0.5 * input.progress);
        return vec4<f32>(final_color + gear_color, 1.0);
    }

    return vec4<f32>(final_color, 1.0);
}
