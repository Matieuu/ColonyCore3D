use glam::U64Vec3;

use crate::world::{World, calc_index};

#[unsafe(no_mangle)]
pub extern "C" fn entity_get_float(
    ptr: *mut World,
    position: U64Vec3,
    prop_id: u16,
    out_value: *mut f32,
) -> u8 {
    assert!(!ptr.is_null());
    let world = unsafe { &mut *ptr };

    if let Some(idx) = calc_index(&world.size, position) {
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
pub extern "C" fn entity_get_int(
    ptr: *mut World,
    position: U64Vec3,
    prop_id: u16,
    out_value: *mut i32,
) -> u8 {
    assert!(!ptr.is_null());
    let world = unsafe { &mut *ptr };

    if let Some(idx) = calc_index(&world.size, position) {
        if let Some(entity) = world.entities.get(&idx) {
            if let Some(val) = entity.get_int(prop_id) {
                unsafe { *out_value = val }
                return 1;
            }
        }
    }

    0
}
