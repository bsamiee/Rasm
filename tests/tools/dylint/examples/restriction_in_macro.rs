#![cfg_attr(dylint_lib = "rasm_lints", allow(forwarding_function, reason = "these lints fire incidentally here and are exercised by their own fixtures"))]

use core::fmt::Write as _;
use core::future;
use core::hint::black_box;
use core::num::ParseIntError;
use core::pin::pin;

use tokio::pin as pinned;

// --- [OPERATIONS] ----------------------------------------------------------------------

macro_rules! taken {
    ($value:expr) => {
        //~v unwrap_used_in_macro
        $value.unwrap()
    };
}

macro_rules! taken_qualified {
    ($value:expr) => {
        //~v unwrap_used_in_macro
        Option::unwrap($value)
    };
}

macro_rules! required_qualified {
    ($value:expr) => {
        //~v expect_used_in_macro
        Result::expect($value, "value is present")
    };
}

macro_rules! required {
    ($value:expr) => {
        //~v expect_used_in_macro
        $value.expect("value is present")
    };
}

macro_rules! ensured {
    ($condition:expr) => {
        if !$condition {
            //~v panic_in_macro
            panic!("broken")
        }
    };
}

macro_rules! pending {
    () => {
        //~v panic_in_macro
        todo!()
    };
}

macro_rules! said {
    ($($argument:tt)*) => {
        //~v print_in_macro
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
        //~v use_debug_in_macro
        write!($out, "{:?}", $value)
    };
}

fn showing(value: u32) -> String { shown!(value) }

fn writing(out: &mut String, value: u32) -> core::fmt::Result { written!(out, value) }

fn pinned_values() {
    let first = future::ready(1);
    let second = future::ready(2);
    tokio::pin!(first, second); //~ tokio_pin
    black_box((first, second));
}

fn aliased_pinning() {
    let value = future::ready(3);
    pinned!(value); //~ tokio_pin
    black_box(value);
}

fn nested_pinning() {
    //~v tokio_pin
    tokio::pin! {
        let outer = async {
            let inner = future::ready(4);
            pinned!(inner); //~ tokio_pin
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
