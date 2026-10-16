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
    assert!(values.len() <= LIMIT, "too many values");
    values.iter().sum()
}

fn paired(left: &[usize], right: &[usize]) -> usize {
    debug_assert_eq!(left.len(), right.len());
    left.iter().zip(right).map(|(left, right)| left.strict_mul(*right)).sum()
}

fn distinct(left: usize, right: usize) -> usize {
    assert_ne!(left, right);
    left.abs_diff(right)
}

fn accepted(reply: &Reply) -> usize {
    core::assert_matches!(reply, Reply::Accepted(_));
    if let Reply::Accepted(count) = reply { *count } else { 0 }
}

fn equal(left: usize, right: usize) -> usize {
    assert_eq!(left, right, "lengths differ");
    left
}

fn checked(values: &[usize]) -> usize {
    debug_assert!(!values.is_empty(), "no values");
    debug_assert_ne!(values.len(), LIMIT, "values fill the limit");
    values.len()
}

fn rejected(reply: &Reply) -> bool {
    core::debug_assert_matches!(reply, Reply::Rejected);
    matches!(reply, Reply::Rejected)
}

fn compiled() -> usize {
    const { assert!(LIMIT.is_power_of_two()) };
    LIMIT
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    black_box(&(
        POSITIVE,
        bounded(&[1, 2]),
        paired(&[1], &[2]),
        distinct(1, 2),
        accepted(&Reply::Accepted(1)),
        accepted(&Reply::Rejected),
        compiled(),
        equal(1, 1),
        checked(&[1]),
        rejected(&Reply::Rejected),
    ));
}
