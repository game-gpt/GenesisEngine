struct CameraUniforms {
    view: mat4<f32>,
    projection: mat4<f32>,
    position: vec3<f32>,
    zoom: f32,
    near_plane: f32,
    far_plane: f32,
    viewport_width: f32,
    viewport_height: f32,
}

micro screen_to_world(screen_point: vec2<f32>, viewport_size: vec2<f32>, camera: CameraUniforms) -> vec2<f32> {
    let ndc_x = (2.0 * screen_point.x) / viewport_size.x - 1.0;
    let ndc_y = 1.0 - (2.0 * screen_point.y) / viewport_size.y;
    let world_x = ndc_x / camera.zoom + camera.position.x;
    let world_y = ndc_y / camera.zoom + camera.position.y;
    return vec2<f32>(world_x, world_y);
}

micro world_to_screen(world_point: vec2<f32>, viewport_size: vec2<f32>, camera: CameraUniforms) -> vec2<f32> {
    let offset_x = (world_point.x - camera.position.x) * camera.zoom;
    let offset_y = (world_point.y - camera.position.y) * camera.zoom;
    let screen_x = (offset_x + 1.0) * viewport_size.x * 0.5;
    let screen_y = (1.0 - offset_y) * viewport_size.y * 0.5;
    return vec2<f32>(screen_x, screen_y);
}
