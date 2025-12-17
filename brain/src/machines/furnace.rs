use crate::{
    block_entity::BlockEntity,
    constants::{PROP_FUEL_LEVEL, PROP_IS_ACTIVE, PROP_MAX_FUEL},
    utils::is_int_boolean,
};

pub struct Furnace {
    fuel_level: f32,
    max_fuel_level: f32,
}

impl BlockEntity for Furnace {
    fn get_float(&self, id: u16) -> Option<f32> {
        match id {
            PROP_FUEL_LEVEL => Some(self.fuel_level),
            PROP_MAX_FUEL => Some(self.max_fuel_level),
            _ => None,
        }
    }

    fn get_int(&self, id: u16) -> Option<i32> {
        match id {
            PROP_IS_ACTIVE if self.fuel_level != 0.0 => Some(1),
            _ => None,
        }
    }

    fn set_float(&mut self, id: u16, value: f32) {
        match id {
            PROP_FUEL_LEVEL => self.fuel_level = value,
            PROP_MAX_FUEL => self.max_fuel_level = value,
            _ => {}
        }
    }

    fn tick(&mut self) {
        if self.fuel_level > 0.0 {
            self.fuel_level -= 0.1;
        }
    }
}
