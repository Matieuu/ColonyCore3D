use std::{collections::HashMap, u32};

use glam::U64Vec3;

use crate::{entities::player::Player, render::block_entity::BlockEntity};

pub struct World {
    pub size: U64Vec3,
    pub map: Vec<u16>,

    pub player: Player,
    pub entities: HashMap<usize, Box<dyn BlockEntity>>,
}

pub fn calc_index(size: &U64Vec3, position: U64Vec3) -> Option<usize> {
    if position.x >= size.x || position.y >= size.y || position.z >= size.z {
        return None;
    }

    let idx: usize = position.x as usize
        + position.y as usize * size.x as usize
        + position.z as usize * size.x as usize * size.y as usize;

    Some(idx)
}
