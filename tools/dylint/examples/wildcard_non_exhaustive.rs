use core::hint::black_box;
use std::collections::HashSet;

use rasm_lints_auxiliary::{Choice, Inner, Outer};

// --- [TYPES] ---------------------------------------------------------------------------

#[non_exhaustive]
enum LocalChoice {
    First,
    Second,
    Third,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn local_choice(value: LocalChoice) -> u8 {
    match value {
        LocalChoice::First => 1,
        _ => 0,
    }
}

const fn known_choice(value: Choice) -> u8 {
    match value {
        Choice::First => 1,
        Choice::Second => 2,
        Choice::Third => 3,
        _ => 0,
    }
}

const fn known_outer(value: Outer<Inner>) -> u8 {
    match value {
        Outer::Known(Inner::Left) => 1,
        Outer::Known(Inner::Right) => 2,
        _ => 0,
    }
}

const fn inner_wildcard(value: Outer<Inner>) -> u8 {
    match value {
        Outer::Known(Inner::Left) => 1,
        Outer::Known(_) => 2,
        _ => 0,
    }
}

const fn separate_tuple(value: (Choice, Inner)) -> u8 {
    match value {
        (Choice::First, Inner::Left) => 1,
        (_, _) => 0,
    }
}

const fn joint_tuple(value: (Choice, Inner)) -> u8 {
    match value {
        (Choice::First, Inner::Left) => 1,
        _ => 0,
    }
}

const fn direct_let_else(value: Choice) -> u8 {
    let Choice::First = value else { return 0 };
    1
}

const fn nested_let_else(value: Outer<Inner>) -> u8 {
    let Outer::Known(Inner::Left) = value else { return 0 };
    1
}

const fn constant_choices(value: Choice) -> u8 {
    const FIRST: Choice = Choice::First;
    const SECOND: Choice = Choice::Second;
    match value {
        FIRST => 1,
        SECOND => 2,
        _ => 0,
    }
}

const fn matched_choice(value: Choice) -> bool { matches!(value, Choice::First) }

fn equal_choice(value: Choice) -> u8 { if value == Choice::First { 1 } else { 0 } }

fn contains_choice(choices: &HashSet<Choice>) -> u8 { if choices.contains(&Choice::First) { 1 } else { 0 } }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    black_box(&(
        local_choice,
        known_choice,
        known_outer,
        inner_wildcard,
        separate_tuple,
        joint_tuple,
        direct_let_else,
        nested_let_else,
        constant_choices,
        matched_choice,
        equal_choice,
        contains_choice,
        [LocalChoice::First, LocalChoice::Second, LocalChoice::Third],
    ));
}
