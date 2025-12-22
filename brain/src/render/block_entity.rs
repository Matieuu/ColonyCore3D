pub trait BlockEntity {
    #[rustfmt::skip]    fn get_float(&self, id: u16) -> Option<f32> { None }
    #[rustfmt::skip]    fn get_int(&self, id: u16) -> Option<i32> { None }

    fn set_float(&mut self, id: u16, value: f32) {}
    fn set_int(&mut self, id: u16, value: i32) {}

    fn tick(&mut self) {}
}
