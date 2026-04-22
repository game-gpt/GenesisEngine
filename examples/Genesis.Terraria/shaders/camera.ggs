struct CameraUniforms {
    view: mat4<f32>,
    projection: mat4<f32>,
    position: vec2<f32>,
    zoom: f32,
    rotation: f32,
    viewport_size: vec2<f32>,
}

micro screen_to_world(screen_point: vec2<f32>, camera: CameraUniforms) -> vec2<f32> {
    return (screen_point - camera.viewport_size * 0.5) / camera.zoom + camera.position;
}

micro world_to_screen(world_point: vec2<f32>, camera: CameraUniforms) -> vec2<f32> {
    return (world_point - camera.position) * camera.zoom + camera.viewport_size * 0.5;
}
