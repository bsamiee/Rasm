use core::hint::black_box;

// --- [TYPES] ---------------------------------------------------------------------------

trait Arity {
    const ARITY: usize;
}

trait Width {
    const WIDTH: u32;
}

trait Depth {
    const DEPTH: u8 = 1;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const RETRIES: u32 = 3;
const WORKERS: u32 = 3;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Add;

#[derive(Debug)]
struct Sub;

#[derive(Debug)]
struct Narrow;

#[derive(Debug)]
struct Wide;

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Arity for Add {
    const ARITY: usize = 2;
}

impl Arity for Sub {
    const ARITY: usize = 2;
}

impl Width for Narrow {
    const WIDTH: u32 = 8;
}

impl Width for Wide {
    const WIDTH: u32 = 16;
}

impl Depth for Narrow {
    const DEPTH: u8 = 4;
}

impl Depth for Wide {
    const DEPTH: u8 = 4;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&([Add::ARITY, Sub::ARITY], [Narrow::WIDTH, Wide::WIDTH, RETRIES, WORKERS], [Narrow::DEPTH, Wide::DEPTH])); }
