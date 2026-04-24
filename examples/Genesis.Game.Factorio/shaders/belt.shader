struct BeltVertexInput {
    position: vec2<f32>,
    direction: i32,
    item_0_progress: f32,
    item_1_progress: f32,
    item_2_progress: f32,
    item_3_progress: f32,
    item_count: i32,
}

struct BeltVertexOutput {
    position: vec4<f32>,
    uv: vec2<f32>,
    belt_color: vec3<f32>,
    item_positions: [vec2<f32>],
    world_pos: vec2<f32>,
}

let BELT_COLOR: vec3<f32> = vec3<f32>(0.80, 0.75, 0.30);
let BELT_STRIPE_COLOR: vec3<f32> = vec3<f32>(0.60, 0.55, 0.20);
let ITEM_NONE_COLOR: vec3<f32> = vec3<f32>(0.0, 0.0, 0.0);

[Vertex]
micro vs_main(input: BeltVertexInput) -> BeltVertexOutput {
    let mut output: BeltVertexOutput;
    let tile_size = 32.0;
    let world_x = input.position.x * tile_size;
    let world_y = input.position.y * tile_size;
    output.position = uniforms.mvp * vec4<f32>(world_x, world_y, 0.0, 1.0);
    output.uv = vec2<f32>(input.position.x, input.position.y);
    output.belt_color = BELT_COLOR;
    output.world_pos = vec2<f32>(world_x, world_y);

    let dir_offset = get_direction_vector(input.direction);
    let base_x = world_x;
    let base_y = world_y;

    output.item_positions = [];
    if input.item_count > 0 {
        output.item_positions.push(vec2<f32>(base_x + dir_offset.x * input.item_0_progress * tile_size,
                                              base_y + dir_offset.y * input.item_0_progress * tile_size));
    }
    if input.item_count > 1 {
        output.item_positions.push(vec2<f32>(base_x + dir_offset.x * input.item_1_progress * tile_size,
                                              base_y + dir_offset.y * input.item_1_progress * tile_size));
    }
    if input.item_count > 2 {
        output.item_positions.push(vec2<f32>(base_x + dir_offset.x * input.item_2_progress * tile_size,
                                              base_y + dir_offset.y * input.item_2_progress * tile_size));
    }
    if input.item_count > 3 {
        output.item_positions.push(vec2<f32>(base_x + dir_offset.x * input.item_3_progress * tile_size,
                                              base_y + dir_offset.y * input.item_3_progress * tile_size));
    }

    return output;
}

[Fragment]
micro fs_main(input: BeltVertexOutput) -> vec4<f32> {
    let tile_size = 32.0;
    let local_x = fract(input.world_pos.x / tile_size);
    let local_y = fract(input.world_pos.y / tile_size);

    let stripe = step(0.45, local_x) * step(local_x, 0.55);
    let color = mix(BELT_COLOR, BELT_STRIPE_COLOR, stripe * 0.5);

    let border = step(local_x, 0.05) + step(0.95, local_x) + step(local_y, 0.05) + step(0.95, local_y);
    let final_color = mix(color, vec3<f32>(0.3, 0.3, 0.3), min(border, 1.0) * 0.5);

    return vec4<f32>(final_color, 1.0);
}

micro get_direction_vector(direction: i32) -> vec2<f32> {
    if direction == 0 { return vec2<f32>(0.0, -1.0); }
    if direction == 1 { return vec2<f32>(0.0, 1.0); }
    if direction == 2 { return vec2<f32>(1.0, 0.0); }
    if direction == 3 { return vec2<f32>(-1.0, 0.0); }
    return vec2<f32>(0.0, 0.0);
}
