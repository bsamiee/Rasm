use core::{fmt::Write as _, hint::black_box, num::ParseIntError};

// --- [OPERATIONS] ----------------------------------------------------------------------

macro_rules! taken {
    ($value:expr) => {
        $value.unwrap()
    };
}

macro_rules! required {
    ($value:expr) => {
        $value.expect("value is present")
    };
}

macro_rules! ensured {
    ($condition:expr) => {
        if !$condition {
            panic!("broken")
        }
    };
}

macro_rules! pending {
    () => {
        todo!()
    };
}

macro_rules! said {
    ($($argument:tt)*) => {
        println!($($argument)*)
    };
}

macro_rules! traced {
    ($value:expr) => {
        dbg!($value)
    };
}

fn unwrapped(value: Option<u32>) -> u32 { taken!(value) }

fn expected(value: Result<u32, ParseIntError>) -> u32 { required!(value) }

fn checked(value: u32) { ensured!(value > 1) }

fn unfinished() -> u32 { pending!() }

fn printed(value: u32) { said!("{value}") }

fn debugged(value: u32) -> u32 { traced!(value) }

fn total(values: &[u32]) -> Option<u32> { values.iter().copied().max() }

fn direct(value: Option<u32>) -> u32 { value.unwrap() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    checked(2);
    printed(3);
    black_box(&(
        unwrapped(Some(1)),
        expected("2".parse()),
        debugged(4),
        total(&[5]),
        direct(Some(6)),
        unfinished as fn() -> u32,
        (showing(7), ignoring(Ok(8)), keeping(Ok(9)), writing(&mut String::new(), 10)),
    ));
}

macro_rules! shown {
    ($value:expr) => {
        format!("{:?}", $value)
    };
}

macro_rules! written {
    ($out:expr, $value:expr) => {
        write!($out, "{:?}", $value)
    };
}

macro_rules! ignored {
    ($value:expr) => {
        $value.map_err(|_| Refused)
    };
}

macro_rules! kept {
    ($value:expr) => {
        $value.map_err(|error| Unreadable(error))
    };
}

#[derive(Debug, thiserror::Error)]
#[error("count is refused")]
struct Refused;

#[derive(Debug, thiserror::Error)]
#[error("count is unreadable")]
struct Unreadable(#[source] ParseIntError);

fn showing(value: u32) -> String { shown!(value) }

fn writing(out: &mut String, value: u32) -> core::fmt::Result { written!(out, value) }

fn ignoring(value: Result<u32, ParseIntError>) -> Result<u32, Refused> { ignored!(value) }

fn keeping(value: Result<u32, ParseIntError>) -> Result<u32, Unreadable> { kept!(value) }
