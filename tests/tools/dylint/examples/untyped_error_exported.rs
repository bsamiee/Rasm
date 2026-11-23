#![cfg_attr(dylint_lib = "rasm_lints", allow(discarded_error_source, stringified_error, reason = "these lints fire incidentally here and are exercised by their own fixtures"))]

use core::num::ParseIntError;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn probed(text: &str) -> Result<u16, ()> { text.parse().map_err(|_error: ParseIntError| ()) }

pub fn described(text: &str) -> Result<u16, String> { text.parse().map_err(|error: ParseIntError| error.to_string()) } //~ untyped_error
