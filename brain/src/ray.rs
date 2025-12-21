#[repr(C)]
pub struct Ray {
    pub origin_x: f32,
    pub origin_y: f32,
    pub origin_z: f32,
    pub direction_x: f32,
    pub direction_y: f32,
    pub direction_z: f32,
}

#[repr(C)]
pub struct RaycastResult {
    pub hit: u8,
    pub x: i32,
    pub y: i32,
    pub z: i32,
    pub face: u8,
}
