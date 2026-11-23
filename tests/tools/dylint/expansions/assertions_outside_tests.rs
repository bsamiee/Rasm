#![cfg_attr(dylint_lib = "rasm_lints", allow(single_reader, reason = "each helper is read once by design, single_reader has its own fixture"))]

use core::hint::black_box;

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMIT: usize = 8;

const POSITIVE: () = assert!(LIMIT > 0);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
enum Reply {
    Accepted(usize),
    Rejected,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn bounded(values: &[usize]) -> usize {
    assert!(values.len() <= LIMIT, "too many values"); //[assertions_outside_tests]~ assertions_outside_tests
    values.iter().sum()
}

fn accepted(reply: &Reply) -> usize {
    core::assert_matches!(reply, Reply::Accepted(_)); //[assertions_outside_tests]~ assertions_outside_tests
    match reply {
        Reply::Accepted(count) => *count,
        Reply::Rejected => 0,
    }
}

fn compiled() -> usize {
    const { assert!(LIMIT.is_power_of_two()) };
    LIMIT
}

macro_rules! ensured {
    ($condition:expr) => {
        //[assertions_outside_tests]~v assertions_outside_tests
        assert!($condition, "broken")
    };
}

fn wrapped(count: usize) -> usize {
    ensured!(count < LIMIT);
    count
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(POSITIVE, bounded(&[1, 2]), accepted(&Reply::Accepted(1)), accepted(&Reply::Rejected), compiled(), wrapped(1))); }

#[cfg(test)]
mod tests {
    fn helper(value: usize) {
        assert!(value > 0);
    }

    #[test]
    fn asserted() {
        core::hint::black_box(super::main as fn());
        helper(1);
        assert_eq!(1, 1);
    }
}

#[test]
fn asserted_directly() {
    assert_ne!(0, 1);
}
