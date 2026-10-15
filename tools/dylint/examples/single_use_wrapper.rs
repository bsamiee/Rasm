use core::{fmt, hint::black_box};

// --- [MODELS] --------------------------------------------------------------------------

struct Joined<'items>(&'items [u8]);

struct Stored<'items>(&'items [u8]);

struct Holder<'items> {
    label: Stored<'items>,
}

struct Twice(u8);

#[derive(Debug)]
struct Traced(u8);

struct Countdown {
    left: u8,
}

#[derive(Clone)]
struct Repeated {
    left: u8,
}

impl fmt::Display for Joined<'_> {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { self.0.iter().try_for_each(|item| write!(f, "{item},")) }
}

impl fmt::Display for Stored<'_> {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { self.0.iter().try_for_each(|item| write!(f, "{item};")) }
}

impl fmt::Display for Twice {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{}{}", self.0, self.0) }
}

impl Iterator for Countdown {
    type Item = u8;

    fn next(&mut self) -> Option<u8> {
        self.left = self.left.checked_sub(1)?;
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
    let values = [1, 2, 3];
    let joined = format!("{}", Joined(&values));
    let holder = Holder { label: Stored(&values) };
    let twice = format!("{}{}", Twice(1), Twice(2));
    let traced = usize::from(Traced(4).0);
    let counted: u8 = Countdown { left: 3 }.sum();
    let repeated: u8 = Repeated { left: 2 }.sum();
    black_box((joined, holder.label.to_string(), twice, traced, [counted, repeated]));
}
