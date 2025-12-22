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

pub mod constants;
pub mod machines;
pub mod natives;
pub mod render;
pub mod utils;
pub mod world;
