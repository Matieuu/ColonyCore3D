use crate::{
    constants::PROP_ITEM_COUNT, render::block_entity::BlockEntity, utils::is_int_positive,
};

pub struct Chest {
    pub inv_size: u16,
}

impl BlockEntity for Chest {
    fn get_int(&self, id: u16) -> Option<i32> {
        match id {
            PROP_ITEM_COUNT => Some(self.inv_size as i32),
            _ => None,
        }
    }

    fn set_int(&mut self, id: u16, value: i32) {
        match id {
            PROP_ITEM_COUNT if is_int_positive(value) => self.inv_size = value as u16,
            _ => {}
        }
    }
}
