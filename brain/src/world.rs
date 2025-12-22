use std::{collections::HashMap, u32};

use crate::render::block_entity::BlockEntity;

pub struct World {
    pub width: u32,
    pub height: u32,
    pub depth: u32,

    pub map: Vec<u16>,
    pub entities: HashMap<usize, Box<dyn BlockEntity>>,
}

impl World {
    pub fn calc_index(&self, x: u32, y: u32, z: u32) -> Option<usize> {
        if x >= self.width || y >= self.height || z >= self.depth {
            return None;
        }

        let idx: usize = x as usize
            + y as usize * self.width as usize
            + z as usize * self.width as usize * self.height as usize;

        Some(idx)
    }
}
