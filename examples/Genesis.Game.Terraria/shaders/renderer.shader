struct SpriteVertexInput {
    position: vec3<f32>,
    uv: vec2<f32>,
    sprite_color: vec3<f32>,
}

struct SpriteVertexOutput {
    position: vec4<f32>,
    uv: vec2<f32>,
    sprite_color: vec3<f32>,
}

[Vertex]
micro vs_main(input: SpriteVertexInput) -> SpriteVertexOutput {
    let mut output: SpriteVertexOutput;
    output.position = uniforms.mvp * vec4<f32>(input.position, 1.0);
    output.uv = input.uv;
    output.sprite_color = input.sprite_color;
    return output;
}

[Fragment]
micro fs_main(input: SpriteVertexOutput) -> vec4<f32> {
    return vec4<f32>(input.sprite_color, 1.0);
}
