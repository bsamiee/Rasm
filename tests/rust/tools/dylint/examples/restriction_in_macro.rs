use core::{fmt::Write as _, future, hint::black_box, num::ParseIntError, pin::pin};

use tokio::pin as pinned;

// --- [OPERATIONS] ----------------------------------------------------------------------

macro_rules! taken {
    ($value:expr) => {
        $value.unwrap()
    };
}

macro_rules! taken_qualified {
    ($value:expr) => {
        Option::unwrap($value)
    };
}

macro_rules! required_qualified {
    ($value:expr) => {
        Result::expect($value, "value is present")
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

fn unwrapped_qualified(value: Option<u32>) -> u32 { taken_qualified!(value) }

fn expected_qualified(value: Result<u32, ParseIntError>) -> u32 { required_qualified!(value) }

fn checked(value: u32) { ensured!(value > 1) }

fn unfinished() -> u32 { pending!() }

fn printed(value: u32) { said!("{value}") }

fn debugged(value: u32) -> u32 { traced!(value) }

fn total(values: &[u32]) -> Option<u32> { values.iter().copied().max() }

fn direct(value: Option<u32>) -> u32 { value.unwrap() }

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

fn showing(value: u32) -> String { shown!(value) }

fn writing(out: &mut String, value: u32) -> core::fmt::Result { written!(out, value) }

fn pinned_values() {
    let first = future::ready(1);
    let second = future::ready(2);
    tokio::pin!(first, second);
    black_box((first, second));
}

fn aliased_pinning() {
    let value = future::ready(3);
    pinned!(value);
    black_box(value);
}

fn nested_pinning() {
    tokio::pin! {
        let outer = async {
            let inner = future::ready(4);
            pinned!(inner);
            black_box(inner);
        };
        let other = future::ready(5);
    }
    black_box((outer, other));
}

fn standard_pinning() { black_box(pin!(future::ready(6))); }

fn local_pinning() {
    macro_rules! pin {
        ($value:ident) => {
            let $value = $value;
        };
    }
    let value = future::ready(7);
    pin!(value);
    black_box(&value);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    checked(2);
    printed(3);
    black_box(&(unwrapped(Some(1)), expected("2".parse()), debugged(4), total(&[5]), direct(Some(6)), unfinished as fn() -> u32, (showing(7), writing(&mut String::new(), 8))));
    black_box((unwrapped_qualified(Some(1)), expected_qualified("2".parse())));
    pinned_values();
    aliased_pinning();
    nested_pinning();
    standard_pinning();
    local_pinning();
}
