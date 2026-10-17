use core::hint::black_box;
use std::collections::BTreeMap;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord)]
enum Unit {
    Meter,
    Foot,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord)]
enum Level {
    Low,
    High,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let counts = BTreeMap::from([(Unit::Meter, 0_u32), (Unit::Foot, 0)]);
    let collected = [(Unit::Meter, 1_u32), (Unit::Foot, 2)].into_iter().collect::<BTreeMap<_, _>>();
    let partial = BTreeMap::from([(Level::Low, 1_u32)]);
    black_box(&(counts, collected, partial, Level::High));
}
