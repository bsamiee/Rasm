use core::hint::black_box;
use std::alloc::System;
use std::sync::atomic::{AtomicUsize, Ordering};

// --- [MODELS] --------------------------------------------------------------------------

struct Catalog {
    words: Vec<&'static str>,
}

impl Catalog {
    fn split(&self) -> (Vec<&'static str>, Vec<&'static str>) {
        let blank = self.words.iter().filter(|word| word.is_empty()).copied().collect();
        let filled = self.words.iter().filter(|word| !word.is_empty()).copied().collect();
        (blank, filled)
    }
}

impl AsRef<Catalog> for [u8] {
    fn as_ref(&self) -> &Catalog {
        CALLS.fetch_add(1, Ordering::Relaxed);
        &EMPTY
    }
}

impl<'a> FromIterator<&'a Catalog> for Vec<u8> {
    fn from_iter<T: IntoIterator<Item = &'a Catalog>>(items: T) -> Self {
        CALLS.fetch_add(1, Ordering::Relaxed);
        items.into_iter().map(|item| u8::from(item.words.is_empty())).collect()
    }
}

impl<'a> FromIterator<&'a Catalog> for Vec<&'a Catalog, System> {
    fn from_iter<T: IntoIterator<Item = &'a Catalog>>(_: T) -> Self {
        CALLS.fetch_add(1, Ordering::Relaxed);
        Self::new_in(System)
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

static CALLS: AtomicUsize = AtomicUsize::new(0);
static EMPTY: Catalog = Catalog { words: Vec::new() };

// --- [OPERATIONS] ----------------------------------------------------------------------

fn halves(words: &[&'static str]) -> (Vec<&'static str>, Vec<&'static str>) {
    let blank = words.iter().filter(|word| str::is_empty(word)).copied().collect();
    let filled = words.iter().filter(|word| !str::is_empty(word)).copied().collect();
    (blank, filled)
}

fn trimmed(words: &[&'static str]) -> (Vec<&'static str>, Vec<&'static str>) {
    let blank = words.iter().filter(|word| word.trim().is_empty()).copied().collect();
    let filled = words.iter().filter(|word| !word.trim().is_empty()).copied().collect();
    (blank, filled)
}

fn closures(words: &[&'static str]) -> (Vec<&'static str>, Vec<&'static str>) {
    let blank = || words.iter().filter(|word| word.is_empty()).copied().collect::<Vec<_>>();
    let filled = words.iter().filter(|word| !word.is_empty()).copied().collect();
    (blank(), filled)
}

fn closure_halves(words: &[&'static str]) -> (Vec<&'static str>, Vec<&'static str>) {
    let split = || {
        let blank = words.iter().filter(|word| word.is_empty()).copied().collect();
        let filled = words.iter().filter(|word| !word.is_empty()).copied().collect();
        (blank, filled)
    };
    split()
}

fn tuple_members(items: &[(i32, i32)]) -> (Vec<(i32, i32)>, Vec<(i32, i32)>) {
    let left = items.iter().filter(|(left, _)| *left > 0).copied().collect();
    let right = items.iter().filter(|(_, right)| !(*right > 0)).copied().collect();
    (left, right)
}

fn tuple_halves(items: &[(i32, i32)]) -> (Vec<(i32, i32)>, Vec<(i32, i32)>) {
    let positive = items.iter().filter(|(value, _)| *value > 0).copied().collect();
    let negative = items.iter().filter(|(value, _)| !(*value > 0)).copied().collect();
    (positive, negative)
}

fn borrowed(items: &[u8]) -> (Vec<&u8>, Vec<&u8>) {
    let selected = items.iter().filter(|value| **value > 0).collect();
    let rejected = items.iter().filter(|value| !(**value > 0)).collect();
    (selected, rejected)
}

fn mixed(items: &[u8]) -> (Vec<&u8>, Vec<u8>) {
    let selected = items.iter().filter(|value| **value > 0).collect();
    let rejected = items.iter().filter(|value| !(**value > 0)).copied().collect();
    (selected, rejected)
}

fn discarded(items: &[u8]) -> Vec<u8> {
    drop(items.iter().filter(|value| **value > 0).copied().collect::<Vec<_>>());
    let rejected = items.iter().filter(|value| !(**value > 0)).copied().collect();
    rejected
}

fn custom_collection(items: &[Catalog]) -> (Vec<u8>, Vec<u8>, usize) {
    let selected = items.iter().filter(|item| item.words.is_empty()).collect();
    let rejected = items.iter().filter(|item| !item.words.is_empty()).collect();
    (selected, rejected, CALLS.load(Ordering::Relaxed))
}

fn allocated_collection(items: &[Catalog]) -> (Vec<&Catalog, System>, Vec<&Catalog, System>, usize) {
    let selected = items.iter().filter(|item| item.words.is_empty()).collect();
    let rejected = items.iter().filter(|item| !item.words.is_empty()).collect();
    (selected, rejected, CALLS.load(Ordering::Relaxed))
}

fn qualified(items: &[u8]) -> (Vec<u8>, Vec<u8>) {
    let selected = Iterator::collect(Iterator::copied(Iterator::filter(<[u8]>::iter(items), |value| **value > 0)));
    let rejected = Iterator::collect(Iterator::copied(Iterator::filter(<[u8]>::iter(items), |value| !(**value > 0))));
    (selected, rejected)
}

fn predicate_effect(items: &[u8], calls: &AtomicUsize) -> (Vec<u8>, Vec<u8>) {
    let selected = items.iter().filter(|_| calls.fetch_add(1, Ordering::Relaxed) < 2).copied().collect();
    let rejected = items.iter().filter(|_| !(calls.fetch_add(1, Ordering::Relaxed) < 2)).copied().collect();
    (selected, rejected)
}

fn trait_effect(items: &[u8]) -> (Vec<u8>, Vec<u8>, usize) {
    let selected = items.iter().filter(|_| AsRef::<Catalog>::as_ref(&items).words.is_empty()).copied().collect();
    let rejected = items.iter().filter(|_| !AsRef::<Catalog>::as_ref(&items).words.is_empty()).copied().collect();
    (selected, rejected, CALLS.load(Ordering::Relaxed))
}

fn observed(_: &&u8) { CALLS.fetch_add(1, Ordering::Relaxed); }

fn source_effect(items: &[u8]) -> (Vec<u8>, Vec<u8>, usize) {
    let selected = items.iter().inspect(observed).filter(|value| **value > 0).copied().collect();
    let rejected = items.iter().inspect(observed).filter(|value| !(**value > 0)).copied().collect();
    (selected, rejected, CALLS.load(Ordering::Relaxed))
}

fn output_effect(items: &[u8]) -> (Vec<u8>, Vec<u8>, usize) {
    let selected = items.iter().filter(|value| **value > 0).inspect(observed).copied().collect();
    let rejected = items.iter().filter(|value| !(**value > 0)).inspect(observed).copied().collect();
    (selected, rejected, CALLS.load(Ordering::Relaxed))
}

fn changed_predicate(items: &[u8]) -> (Vec<u8>, Vec<u8>) {
    let mut limit = 1;
    let selected = items.iter().filter(|value| **value > limit).copied().collect();
    limit = 2;
    let rejected = items.iter().filter(|value| !(**value > limit)).copied().collect();
    (selected, rejected)
}

fn changed_source(items: &mut [u8]) -> (Vec<u8>, Vec<u8>) {
    let selected = items.iter().filter(|value| **value > 0).copied().collect();
    items.fill(0);
    let rejected = items.iter().filter(|value| !(**value > 0)).copied().collect();
    (selected, rejected)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let words = ["", " ", "item"];
    black_box((Catalog { words: words.to_vec() }.split(), halves(&words), trimmed(&words), closures(&words), closure_halves(&words)));
    black_box((tuple_members(&[(1, -1)]), tuple_halves(&[(1, -1)]), borrowed(&[0, 1]), mixed(&[0, 1]), qualified(&[0, 1])));
    black_box((predicate_effect(&[0, 1], &AtomicUsize::new(0)), source_effect(&[0, 1]), output_effect(&[0, 1]), changed_predicate(&[0, 1]), changed_source(&mut [0, 1])));
    black_box(trait_effect(&[0, 1]));
    black_box((discarded(&[0, 1]), custom_collection(&[Catalog { words: Vec::new() }])));
    black_box(allocated_collection(&[Catalog { words: Vec::new() }]));
}
