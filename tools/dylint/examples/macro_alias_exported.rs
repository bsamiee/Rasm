#![no_std]

extern crate self as std;

// --- [COMPOSITION] ---------------------------------------------------------------------

#[macro_export]
macro_rules! format {
    ($($tokens:tt)*) => {
        7_u32
    };
}

#[macro_export]
macro_rules! write {
    ($($tokens:tt)*) => {
        9_u32
    };
}

#[macro_export]
macro_rules! matches {
    ($($tokens:tt)*) => {
        false
    };
}

pub const FORMATTED: u32 = ::std::format!("literal");
pub const WRITTEN: u32 = ::std::write!(not_a_writer, "literal");

pub fn alternatives() -> bool { ::std::matches!(not_a_value, A) || ::std::matches!(not_a_value, B) }
