#![warn(clippy::all, clippy::pedantic)]
#![forbid(clippy::unwrap_used, clippy::ok_expect, clippy::err_expect)]
#![deny(
    unused_must_use,
    unreachable_patterns,
    unused_variables,
    rust_2018_idioms
)]
#![allow(clippy::module_name_repetitions)]
#![cfg_attr(
    debug_assertions,
    allow(dead_code, unused_imports, unused_mut, unused_variables)
)]
#![cfg_attr(not(debug_assertions), deny(debug_assertions))]

use std::collections::HashMap;

use crate::{
    axis_state::{AxisState, Side},
    machines::furnace::Furnace,
    ray::{Ray, RaycastResult},
    world::World,
};

pub mod axis_state;
pub mod block_entity;
pub mod constants;
pub mod machines;
pub mod ray;
pub mod utils;
pub mod world;

#[unsafe(no_mangle)]
pub extern "C" fn sim_init(x: u32, y: u32, z: u32) -> *mut World {
    let world_size = x * y * z;

    let mut world = Box::new(World {
        width: x,
        height: y,
        depth: z,
        map: vec![0; world_size as usize],
        entities: HashMap::with_capacity((world_size as f32).sqrt().floor() as usize),
    });

    for dx in 0..x {
        for dz in 0..z {
            if let Some(idx) = world.calc_index(dx, 0, dz) {
                world.map[idx] = 1;
            }
        }
    }

    if let Some(idx) = world.calc_index(10, 1, 10) {
        use crate::machines::furnace;
        let mut furnace = Furnace {
            fuel_level: 50.0,
            max_fuel_level: 100.0,
        };

        world.entities.insert(idx, Box::new(furnace));
    }

    Box::into_raw(world)
}

#[unsafe(no_mangle)]
pub extern "C" fn world_get_width(world: *const World) -> u32 {
    let world = unsafe { &*world };
    world.width
}

#[unsafe(no_mangle)]
pub extern "C" fn world_get_height(world: *const World) -> u32 {
    let world = unsafe { &*world };
    world.height
}

#[unsafe(no_mangle)]
pub extern "C" fn world_get_depth(world: *const World) -> u32 {
    let world = unsafe { &*world };
    world.depth
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_destroy(ptr: *mut World) {
    let _ = unsafe { Box::from_raw(ptr) };
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_get_map_ptr(ptr: *mut World) -> *const u16 {
    let world = unsafe { &mut *ptr };
    world.map.as_ptr()
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_get_map_len(ptr: *mut World) -> u64 {
    let world = unsafe { &mut *ptr };
    world.map.len() as u64
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_entity_get_float(
    ptr: *mut World,
    x: u32,
    y: u32,
    z: u32,
    prop_id: u16,
    out_value: *mut f32,
) -> u8 {
    let world = unsafe { &mut *ptr };

    if let Some(idx) = world.calc_index(x, y, z) {
        if let Some(entity) = world.entities.get(&idx) {
            if let Some(val) = entity.get_float(prop_id) {
                unsafe { *out_value = val }
                return 1;
            }
        }
    }

    0
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_entity_get_int(
    ptr: *mut World,
    x: u32,
    y: u32,
    z: u32,
    prop_id: u16,
    out_value: *mut i32,
) -> u8 {
    let world = unsafe { &mut *ptr };

    if let Some(idx) = world.calc_index(x, y, z) {
        if let Some(entity) = world.entities.get(&idx) {
            if let Some(val) = entity.get_int(prop_id) {
                unsafe { *out_value = val }
                return 1;
            }
        }
    }

    0
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_tick(ptr: *mut World) {
    let world = unsafe { &mut *ptr };

    for (_, entity) in world.entities.iter_mut() {
        entity.tick();
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_raycast(ptr: *mut World, ray: Ray) -> RaycastResult {
    if !ray.direction_x.is_finite()
        || !ray.direction_y.is_finite()
        || !ray.direction_z.is_finite()
        || !ray.origin_x.is_finite()
        || !ray.origin_y.is_finite()
        || !ray.origin_z.is_finite()
    {
        return RaycastResult {
            hit: 0,
            x: 0,
            y: 0,
            z: 0,
            face: 0,
        };
    }

    let world = unsafe { &*ptr };

    let mut ax = AxisState::new(ray.origin_x, ray.direction_x);
    let mut ay = AxisState::new(ray.origin_y, ray.direction_y);
    let mut az = AxisState::new(ray.origin_z, ray.direction_z);

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
