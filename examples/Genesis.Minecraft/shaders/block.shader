struct BlockVertexInput {
    position: vec3<f32>,
    normal: vec3<f32>,
    uv: vec2<f32>,
    block_color: vec3<f32>,
}

struct BlockVertexOutput {
    position: vec4<f32>,
    normal: vec3<f32>,
    uv: vec2<f32>,
    block_color: vec3<f32>,
    world_pos: vec3<f32>,
}

[Vertex]
micro vs_main(input: BlockVertexInput) -> BlockVertexOutput {
    let mut output: BlockVertexOutput;
    output.position = uniforms.mvp * vec4<f32>(input.position, 1.0);
    output.normal = normalize((uniforms.model * vec4<f32>(input.normal, 0.0)).xyz);
    output.uv = input.uv;
    output.block_color = input.block_color;
    output.world_pos = (uniforms.model * vec4<f32>(input.position, 1.0)).xyz;
    return output;
}

[Fragment]
micro fs_main(input: BlockVertexOutput) -> vec4<f32> {
    let light_dir = normalize(vec3<f32>(0.5, 1.0, 0.3));
    let ambient = 0.3;
    let diffuse = max(dot(input.normal, light_dir), 0.0) * 0.7;
    let lighting = ambient + diffuse;
    return vec4<f32>(input.block_color * lighting, 1.0);
}
