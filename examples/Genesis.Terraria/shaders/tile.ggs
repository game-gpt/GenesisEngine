struct TileVertexInput {
    position: vec3<f32>,
    uv: vec2<f32>,
    tile_color: vec3<f32>,
}

struct TileVertexOutput {
    position: vec4<f32>,
    uv: vec2<f32>,
    tile_color: vec3<f32>,
}

[Vertex]
micro vs_main(input: TileVertexInput) -> TileVertexOutput {
    let mut output: TileVertexOutput;
    output.position = uniforms.mvp * vec4<f32>(input.position, 1.0);
    output.uv = input.uv;
    output.tile_color = input.tile_color;
    return output;
}

[Fragment]
micro fs_main(input: TileVertexOutput) -> vec4<f32> {
    return vec4<f32>(input.tile_color, 1.0);
}
