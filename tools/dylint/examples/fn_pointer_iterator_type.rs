use core::{hint::black_box, iter::Map, ops::Deref};
use std::vec;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Readings {
    values: Vec<u8>,
}

#[derive(Debug)]
struct Callbacks {
    steps: Vec<fn() -> u8>,
    last: fn() -> u8,
}

impl IntoIterator for Readings {
    type IntoIter = Map<vec::IntoIter<u8>, fn(u8) -> u16>;
    type Item = u16;

    fn into_iter(self) -> Self::IntoIter { self.values.into_iter().map(u16::from) }
}

impl IntoIterator for Callbacks {
    type IntoIter = vec::IntoIter<fn() -> u8>;
    type Item = fn() -> u8;

    fn into_iter(self) -> Self::IntoIter {
        let mut steps = self.steps;
        steps.push(self.last);
        steps.into_iter()
    }
}

impl Deref for Callbacks {
    type Target = Vec<fn() -> u8>;

    fn deref(&self) -> &Self::Target { &self.steps }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn step() -> u8 { 4 }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    black_box(&(
        Readings { values: vec![1, 2] }.into_iter().collect::<Vec<_>>(),
        Callbacks { steps: vec![step], last: step }.into_iter().map(|call| call()).collect::<Vec<_>>(),
        Callbacks { steps: vec![step], last: step }.len(),
    ));
}
