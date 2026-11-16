use core::num::ParseIntError;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn probed(text: &str) -> Result<u16, ()> { text.parse().map_err(|_error: ParseIntError| ()) }

pub fn described(text: &str) -> Result<u16, String> { text.parse().map_err(|error: ParseIntError| error.to_string()) }
