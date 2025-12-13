pub struct GameState {
    ticks: u32
}

#[unsafe(no_mangle)]
pub extern "C" fn init_game() -> *mut GameState {
    let gs = Box::new(GameState { ticks: 0 });
    Box::into_raw(gs)
}

#[unsafe(no_mangle)]
pub extern "C" fn destroy_game(ptr: *mut GameState) {
    let _ = unsafe { Box::from_raw(ptr) };
}

#[unsafe(no_mangle)]
pub extern "C" fn add_ticks(ptr: *mut GameState, amount: u32) {
    let mut gs = unsafe { &mut *ptr };
    gs.ticks += amount;
}

#[unsafe(no_mangle)]
pub extern "C" fn get_ticks(ptr: *mut GameState) -> u32 {
    let gs = unsafe { &mut *ptr };
    gs.ticks
}
