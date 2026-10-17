// --- [CONSTANTS] -----------------------------------------------------------------------

pub const SHARED: usize = 2;

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn exported(text: &str) -> usize { text.len().strict_add(SHARED) }

#[must_use]
pub fn relayed(text: &str) -> usize { exported(text).strict_mul(2) }
