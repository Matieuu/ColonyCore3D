use std::ffi::{CStr, c_char};

#[repr(C)]
pub struct Player {
    pub name: *const c_char,
    pub health: u8,
    pub x: f64,
    pub y: f64,
}

#[unsafe(no_mangle)]
pub extern "C" fn player_create(name: *const c_char, x: f64, y: f64) -> *mut Player {
    let player = Box::new(Player {
        name,
        health: 100,
        x,
        y,
    });
    Box::into_raw(player)
}

#[unsafe(no_mangle)]
pub extern "C" fn player_destroy(ptr: *mut Player) {
    if !ptr.is_null() {
        unsafe { let _ = Box::from_raw(ptr); }
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn player_move(ptr: *mut Player, dx: f64, dy: f64) {
    if ptr.is_null() {
        panic!("Player does not exist");
    }

    let player = unsafe { &mut *ptr };

    player.x += dx;
    player.y += dy;
    println!("[RUST] Gracz przesunięty na: {:.2}:{:.2}", player.x, player.y);
}

#[unsafe(no_mangle)]
pub extern "C" fn player_damage(ptr: *mut Player, amount: u8) {
    let player = unsafe { &mut *ptr };
    player.health -= amount;
}
