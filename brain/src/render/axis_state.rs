pub enum Side {
    UP = 0,
    DOWN,
    NORTH,
    EAST,
    SOUTH,
    WEST,
}

pub struct AxisState {
    pub map_pos: i32,
    pub step: i32,
    pub side_dist: f32,
    pub delta_dist: f32,
}

impl AxisState {
    pub fn new(origin: f32, dir: f32) -> Self {
        let map_pos = origin.floor() as i32;
        let delta_dist = (1.0 / dir).abs();

        let (step, side_dist) = if dir < 0.0 {
            (-1, (origin - map_pos as f32) * delta_dist)
        } else {
            (1, (map_pos as f32 + 1.0 - origin) * delta_dist)
        };

        Self {
            map_pos,
            step,
            side_dist,
            delta_dist,
        }
    }

    pub fn next(&mut self) {
        self.side_dist += self.delta_dist;
        self.map_pos += self.step;
    }
}
