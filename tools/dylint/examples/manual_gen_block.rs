use core::hint::black_box;

// --- [MODELS] --------------------------------------------------------------------------

struct Countdown {
    left: u8,
}

#[derive(Clone)]
struct Repeated {
    left: u8,
}

struct Restart {
    left: u8,
}

impl Iterator for Countdown {
    type Item = u8;

    fn next(&mut self) -> Option<u8> {
        self.left = self.left.checked_sub(1)?;
        Some(self.left)
    }
}

impl Iterator for Restart {
    type Item = u8;

    fn next(&mut self) -> Option<u8> {
        *self = Self { left: self.left.checked_sub(1)? };
        Some(self.left)
    }
}

impl Iterator for Repeated {
    type Item = u8;

    fn next(&mut self) -> Option<u8> {
        self.left = self.left.checked_sub(1)?;
        Some(self.left)
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let counted: u8 = Countdown { left: 3 }.sum();
    let repeated: u8 = Repeated { left: 2 }.sum();
    let restarted: u8 = Restart { left: 4 }.sum();
    black_box([counted, repeated, restarted]);
}
