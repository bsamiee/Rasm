use core::hint::black_box;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum Mode {
    Fast,
    Slow,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ALL: [Mode; 2] = [Mode::Fast, Mode::Slow];
const HOT: [Mode; 1] = [Mode::Fast];
const PRIORITY: [Mode; 2] = [Mode::Slow, Mode::Fast];

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&([ALL.len(), HOT.len(), PRIORITY.len()], [Mode::Fast, Mode::Slow].contains(&Mode::Slow))); }
