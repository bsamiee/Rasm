use core::{hint::black_box, panic::AssertUnwindSafe};
use std::rc::Rc;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Profile {
    name: Option<String>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn named(name: &str) -> u8 {
    let owned = name.to_owned();
    match owned.as_str() {
        "a" => 1,
        "b" => 2,
        _ => 0,
    }
}

fn counted(values: &[u8]) -> u8 {
    let owned = values.to_vec();
    if let [first, second] = owned.as_slice() { first.strict_add(*second) } else { 0 }
}

fn leading(values: &[u8]) -> u8 {
    let owned = values.to_vec();
    let [first, ..] = owned.as_slice() else { return 0 };
    *first
}

fn optional(profile: &Profile) -> u8 {
    match profile.name.as_deref() {
        Some("b") => 1,
        Some("c") => 2,
        _ => 0,
    }
}

fn shared(value: &Rc<u8>) -> u8 {
    match &**value {
        5 => 1,
        6 => 2,
        _ => 0,
    }
}

fn boxed(value: u8) -> u8 {
    let held = Box::new(value);
    match &*held {
        5 => 1,
        6 => 2,
        _ => 0,
    }
}

fn trimmed(name: &str) -> u8 {
    match name.trim() {
        "c" => 1,
        "d" => 2,
        _ => 0,
    }
}

fn reborrowed(value: &&u8) -> u8 {
    match &*value {
        &&5 => 1,
        &&6 => 2,
        _ => 0,
    }
}

fn unwound(value: &AssertUnwindSafe<u8>) -> u8 {
    match &**value {
        7 => 1,
        8 => 2,
        _ => 0,
    }
}

fn unwound_optional(value: Option<AssertUnwindSafe<u8>>) -> u8 {
    match value.as_deref() {
        Some(&7) => 1,
        Some(&8) => 2,
        _ => 0,
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    black_box([named("a"), counted(&[1, 2]), leading(&[3]), optional(&Profile { name: Some(String::from("b")) }), shared(&Rc::new(5)), boxed(5), trimmed(" c ")]);
    black_box([reborrowed(&&5), unwound(&AssertUnwindSafe(7)), unwound_optional(Some(AssertUnwindSafe(8)))]);
}
