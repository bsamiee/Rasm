use core::{error::Error, fmt, hint::black_box};

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Manual;

impl fmt::Display for Manual {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("manual failure") }
}

impl Error for Manual {}

#[derive(Debug, thiserror::Error)]
#[error("derived failure")]
struct Derived;

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(Manual.to_string(), Manual.source().is_none(), Derived.source().is_none())); }
