use crate::world::World;

#[unsafe(no_mangle)]
pub extern "C" fn world_get_width(world: *const World) -> u64 {
    let world = unsafe { &*world };
    world.size.x
}

#[unsafe(no_mangle)]
pub extern "C" fn world_get_height(world: *const World) -> u64 {
    let world = unsafe { &*world };
    world.size.y
}

#[unsafe(no_mangle)]
pub extern "C" fn world_get_depth(world: *const World) -> u64 {
    let world = unsafe { &*world };
    world.size.z
}

#[unsafe(no_mangle)]
pub extern "C" fn world_get_map_ptr(ptr: *mut World) -> *const u16 {
    let world = unsafe { &mut *ptr };
    world.map.as_ptr()
}

#[unsafe(no_mangle)]
pub extern "C" fn world_get_map_len(ptr: *mut World) -> u64 {
    let world = unsafe { &mut *ptr };
    world.map.len() as u64
}
