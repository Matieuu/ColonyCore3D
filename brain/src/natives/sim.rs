use std::collections::HashMap;

use glam::{Vec2, Vec3};

use crate::{
    entities::player::Player,
    machines::furnace::Furnace,
    render::ray::{Ray, RaycastResult},
    world::{World, calc_index},
};

#[unsafe(no_mangle)]
pub extern "C" fn sim_init(x: u64, y: u64, z: u64) -> *mut World {
    let world_size = x * y * z;

    let mut world = Box::new(World {
        size: (x as u64, y as u64, z as u64).into(),
        map: vec![0; world_size as usize],
        player: Player::new((50., 3., 50.).into()),
        entities: HashMap::with_capacity((world_size as f32).sqrt().floor() as usize),
    });

    for dx in 0..x {
        for dz in 0..z {
            if let Some(idx) = calc_index(&world.size, (dx, 0, dz).into()) {
                world.map[idx] = 1;
            }
        }
    }

    if let Some(idx) = calc_index(&world.size, (10, 1, 10).into()) {
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
pub extern "C" fn sim_destroy(ptr: *mut World) {
    assert!(!ptr.is_null());
    let _ = unsafe { Box::from_raw(ptr) };
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_tick(ptr: *mut World, dt: f32, input: Vec2, jump: u8) {
    assert!(!ptr.is_null());
    let world = unsafe { &mut *ptr };

    let mut move_dir = Vec3::new(input.x, 0., input.y);
    if move_dir.length_squared() > 0. {
        move_dir = move_dir.normalize();
    }

    let player = &mut world.player;
    player.tick(&world.map, &world.size, move_dir, jump != 0, dt);

    for (_, entity) in world.entities.iter_mut() {
        entity.tick();
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_raycast(ptr: *mut World, ray: Ray, dist: f32) -> RaycastResult {
    assert!(!ptr.is_null());
    let world = unsafe { &*ptr };
    ray.calc_ray(&world.map, &world.size, dist)
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_get_player_pos(ptr: *mut World) -> Vec3 {
    let world = unsafe { &*ptr };
    Vec3::new(
        world.player.position.x,
        world.player.position.y,
        world.player.position.z,
    )
}
