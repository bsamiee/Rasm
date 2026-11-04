// --- [TYPES] ---------------------------------------------------------------------------

pub trait Defaults {
    const VALUE: u32 = 1;
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct First;

#[derive(Debug)]
struct Last;

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Defaults for First {
    const VALUE: u32 = 4;
}

impl Defaults for Last {
    const VALUE: u32 = 4;
}

#[must_use]
pub fn values() -> [u32; 2] { [First::VALUE, Last::VALUE] }
