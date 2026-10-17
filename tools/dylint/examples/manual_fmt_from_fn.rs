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

struct Scaled(u8);

impl fmt::Display for Joined<'_> {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { self.0.iter().try_for_each(|item| write!(f, "{item},")) }
}

impl fmt::Display for Stored<'_> {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { self.0.iter().try_for_each(|item| write!(f, "{item};")) }
}

impl fmt::Display for Twice {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{}{}", self.0, self.0) }
}

impl Scaled {
    const fn doubled(&self) -> u8 { self.0.saturating_mul(2) }
}

impl fmt::Display for Scaled {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{}", self.doubled()) }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let values = [1, 2, 3];
    let joined = format!("{}", Joined(&values));
    let holder = Holder { label: Stored(&values) };
    let twice = format!("{}{}", Twice(1), Twice(2));
    let traced = usize::from(Traced(4).0);
    let scaled = Scaled(5);
    black_box(&(joined, holder.label.to_string(), twice, traced, scaled.to_string(), scaled.doubled()));
}
