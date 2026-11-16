// --- [CONSTANTS] -----------------------------------------------------------------------

pub const SHARED: usize = 2;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Window;

impl Window {
    pub const FACTOR: usize = 3;

    #[must_use]
    pub const fn tripled(width: usize) -> usize { width.strict_mul(Self::FACTOR) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn exported(text: &str) -> usize { text.len().strict_add(SHARED) }

#[must_use]
pub fn relayed(text: &str) -> usize { exported(text).strict_mul(2) }
