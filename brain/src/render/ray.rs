use crate::{
    render::axis_state::{AxisState, Side},
    world::World,
};

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

impl Ray {
    pub fn calc_ray(&self, world: &World) -> RaycastResult {
        if !self.direction_x.is_finite()
            || !self.direction_y.is_finite()
            || !self.direction_z.is_finite()
            || !self.origin_x.is_finite()
            || !self.origin_y.is_finite()
            || !self.origin_z.is_finite()
        {
            return RaycastResult {
                hit: 0,
                x: 0,
                y: 0,
                z: 0,
                face: 0,
            };
        }

        let mut ax = AxisState::new(self.origin_x, self.direction_x);
        let mut ay = AxisState::new(self.origin_y, self.direction_y);
        let mut az = AxisState::new(self.origin_z, self.direction_z);

        let max_dist = 100.0;
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

            let x = ax.map_pos as u32;
            let y = ay.map_pos as u32;
            let z = az.map_pos as u32;

            if let Some(idx) = world.calc_index(x, y, z) {
                if world.map[idx] != 0 {
                    return RaycastResult {
                        hit: 1,
                        x: ax.map_pos,
                        y: ay.map_pos,
                        z: az.map_pos,
                        face: last_face as u8,
                    };
                }
            } else {
                continue;
            }
        }

        RaycastResult {
            hit: 0,
            x: 0,
            y: 0,
            z: 0,
            face: 0,
        }
    }
}
