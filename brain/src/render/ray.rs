use glam::{I64Vec3, U64Vec3, Vec3};

use crate::{
    render::axis_state::{AxisState, Side},
    world::{World, calc_index},
};

#[repr(C)]
pub struct Ray {
    // pub origin_x: f32,
    // pub origin_y: f32,
    // pub origin_z: f32,
    // pub direction_x: f32,
    // pub direction_y: f32,
    // pub direction_z: f32,
    pub origin: Vec3,
    pub direction: Vec3,
}

#[repr(C)]
pub struct RaycastResult {
    pub hit: u8,
    // pub x: i32,
    // pub y: i32,
    // pub z: i32,
    pub position: I64Vec3,
    pub face: u8,
}

impl Ray {
    pub fn calc_ray(&self, map: &[u16], size: &U64Vec3, max_dist: f32) -> RaycastResult {
        if !self.direction.is_finite() || !self.origin.is_finite() {
            return RaycastResult {
                hit: 0,
                position: I64Vec3::ZERO,
                face: 0,
            };
        }

        let mut ax = AxisState::new(self.origin.x, self.direction.x);
        let mut ay = AxisState::new(self.origin.y, self.direction.y);
        let mut az = AxisState::new(self.origin.z, self.direction.z);
        let mut last_face = Side::NORTH;

        while ax.side_dist.min(ay.side_dist).min(az.side_dist) < max_dist {
            if ax.side_dist < ay.side_dist && ax.side_dist < az.side_dist {
                ax.next();
                last_face = if ax.step > 0 { Side::WEST } else { Side::EAST };
            } else if ay.side_dist < ax.side_dist && ay.side_dist < az.side_dist {
                ay.next();
                last_face = if ay.step > 0 { Side::DOWN } else { Side::UP };
            } else if az.side_dist < ax.side_dist && az.side_dist < ay.side_dist {
                az.next();
                last_face = if az.step > 0 {
                    Side::SOUTH
                } else {
                    Side::NORTH
                };
            }

            if ax.map_pos < 0 || ay.map_pos < 0 || az.map_pos < 0 {
                break;
            }

            let x = ax.map_pos as u64;
            let y = ay.map_pos as u64;
            let z = az.map_pos as u64;

            if let Some(idx) = calc_index(&size, (x, y, z).into()) {
                if map[idx] != 0 {
                    return RaycastResult {
                        hit: 1,
                        position: (ax.map_pos, ay.map_pos, az.map_pos).into(),
                        face: last_face as u8,
                    };
                }
            } else {
                continue;
            }
        }

        RaycastResult {
            hit: 0,
            position: I64Vec3::ZERO,
            face: 0,
        }
    }
}
