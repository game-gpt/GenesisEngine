struct CameraUniforms {
    view: mat4<f32>,
    projection: mat4<f32>,
    position: vec3<f32>,
    yaw: f32,
    pitch: f32,
    fov: f32,
    near_plane: f32,
    far_plane: f32,
}

micro screen_to_world_ray(screen_point: vec2<f32>, viewport_size: vec2<f32>, camera: CameraUniforms) -> vec3<f32> {
    let ndc_x = (2.0 * screen_point.x) / viewport_size.x - 1.0;
    let ndc_y = 1.0 - (2.0 * screen_point.y) / viewport_size.y;
    let ray_clip = vec4<f32>(ndc_x, ndc_y, -1.0, 1.0);
    let ray_eye = inverse(camera.projection) * ray_clip;
    let ray_world = normalize((inverse(camera.view) * vec4<f32>(ray_eye.xy, -1.0, 0.0)).xyz);
    return ray_world;
}
