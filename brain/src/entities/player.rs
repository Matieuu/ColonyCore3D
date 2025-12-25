use glam::{U64Vec3, Vec3};

use crate::{
    constants::{GRAVITY, JUMPING_ACCELERATION, WALKING_ACCELERATION, WALKING_SPEED},
    render::ray::Ray,
    world::World,
};

pub struct Player {
    pub position: Vec3,
    pub velocity: Vec3,
    pub on_ground: bool,
}

impl Player {
    pub fn new(start_pos: Vec3) -> Self {
        Player {
            position: start_pos,
            velocity: Vec3::ZERO,
            on_ground: false,
        }
    }

    pub fn tick(&mut self, map: &[u16], size: &U64Vec3, input_dir: Vec3, jump: bool, dt: f32) {
        let accel_mod = if self.on_ground {
            WALKING_ACCELERATION
        } else {
            WALKING_ACCELERATION * 0.25
        };

        let mut input_dir: Vec3 = input_dir.with_y(0.);

        if input_dir.length_squared() > 0. {
            self.velocity += input_dir * accel_mod * dt;
        } else {
            let damping: Vec3 = (0.9, 1., 0.9).into();
            self.velocity *= damping;
        }

        if jump && self.on_ground {
            self.velocity.y = JUMPING_ACCELERATION;
            self.on_ground = false;
        }

        if !self.on_ground {
            self.velocity.y -= GRAVITY * dt;
        }

        let mut next_position = self.position + self.velocity * dt;
        let ray = Ray {
            origin: next_position,
            direction: (0., -1., 0.).into(),
        };
        let hit = ray.calc_ray(map, size, 100.);

        if hit.hit == 1 {
            let ground_y = hit.position.y as f32 + 1.;
            let diff = ground_y - next_position.y;

            if self.velocity.y <= 0. && -0.1 < diff && diff < 1. {
                self.on_ground = true;
                self.velocity.y = 0.;
                next_position.y = ground_y;
            } else {
                self.on_ground = false;
            }
        } else {
            self.on_ground = false;
        }

        self.position = next_position;
    }
}
