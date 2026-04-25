shader camera {
    type: perspective,
    fov: 70.0,
    near: 0.1,
    far: 1000.0
}

shader block {
    vertex: "content/shaders/block.vert",
    fragment: "content/shaders/block.frag"
}
