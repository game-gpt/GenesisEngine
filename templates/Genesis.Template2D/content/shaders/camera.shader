shader camera {
    type: orthographic,
    width: 800,
    height: 600,
    near: -100.0,
    far: 100.0
}

shader tile {
    vertex: "content/shaders/tile.vert",
    fragment: "content/shaders/tile.frag"
}
