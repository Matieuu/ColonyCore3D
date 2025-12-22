use std::collections::HashMap;

use crate::{
    machines::furnace::Furnace,
    render::ray::{Ray, RaycastResult},
    world::World,
};

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
pub extern "C" fn sim_destroy(ptr: *mut World) {
    assert!(!ptr.is_null());
    let _ = unsafe { Box::from_raw(ptr) };
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_tick(ptr: *mut World) {
    assert!(!ptr.is_null());
    let world = unsafe { &mut *ptr };

    for (_, entity) in world.entities.iter_mut() {
        entity.tick();
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn sim_raycast(ptr: *mut World, ray: Ray) -> RaycastResult {
    assert!(!ptr.is_null());
    let world = unsafe { &*ptr };
    ray.calc_ray(world)
}
