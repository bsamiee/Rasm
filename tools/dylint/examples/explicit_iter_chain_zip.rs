use core::{cell::RefCell, hint::black_box, iter, slice};
use std::{
    collections::{BTreeMap, HashMap},
    hash::{BuildHasher, DefaultHasher},
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Ring {
    values: Vec<u32>,
    offset: usize,
}

impl Ring {
    fn iter(&self) -> iter::Skip<slice::Iter<'_, u32>> { self.values.iter().skip(self.offset) }
}

struct Partial {
    values: Vec<u32>,
}

impl Partial {
    fn iter(&self) -> slice::Iter<'_, u32> { self.values[1..].iter() }
}

impl<'a> IntoIterator for &'a Partial {
    type Item = &'a u32;
    type IntoIter = slice::Iter<'a, u32>;

    fn into_iter(self) -> Self::IntoIter { self.values.iter() }
}

struct Dereferenced(Vec<u32>);

impl core::ops::Deref for Dereferenced {
    type Target = [u32];

    fn deref(&self) -> &Self::Target { &self.0 }
}

impl<'a> IntoIterator for &'a Dereferenced {
    type Item = &'a u32;
    type IntoIter = slice::Iter<'a, u32>;

    fn into_iter(self) -> Self::IntoIter { self.0[1..].iter() }
}

struct Observed<'a> {
    events: &'a RefCell<Vec<u8>>,
}

impl IntoIterator for Observed<'_> {
    type Item = u8;
    type IntoIter = core::array::IntoIter<u8, 1>;

    fn into_iter(self) -> Self::IntoIter {
        self.events.borrow_mut().push(1);
        [1].into_iter()
    }
}

struct ObservedHasher<'a> {
    events: &'a RefCell<Vec<u8>>,
}

impl BuildHasher for ObservedHasher<'_> {
    type Hasher = DefaultHasher;

    fn build_hasher(&self) -> Self::Hasher { DefaultHasher::new() }
}

impl Drop for ObservedHasher<'_> {
    fn drop(&mut self) { self.events.borrow_mut().push(1); }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn owned(left: Vec<u32>, right: Vec<u32>) -> Vec<u32> { left.into_iter().chain(right).collect() }

fn borrowed(values: &[u32], weights: &[u32]) -> u32 { values.iter().zip(weights).map(|(value, weight)| value.strict_mul(*weight)).sum() }

fn bound(weights: &[u32]) -> u32 {
    let values = vec![1_u32, 2];
    values.iter().zip(weights).map(|(value, weight)| value.strict_add(*weight)).sum()
}

fn scaled(weights: &[u32]) -> Vec<u32> {
    let mut values = vec![1_u32, 2];
    values.iter_mut().zip(weights).for_each(|(value, weight)| *value = value.strict_mul(*weight));
    values
}

fn keyed(map: &BTreeMap<u32, u32>, extra: &[(u32, u32)]) -> usize { map.iter().chain(extra.iter().map(|(key, value)| (key, value))).count() }

fn ring(ring: &Ring, other: &[u32]) -> usize { ring.iter().zip(other).count() }

fn mapped(values: &[u32], other: &[u32]) -> usize { values.iter().map(|value| value.strict_add(1)).zip(other).count() }

fn direct(values: &[u32], other: &[u32]) -> usize { iter::zip(values, other).count() }

fn partial(values: &Partial, other: &[u32]) -> usize { values.iter().zip(other).count() }

fn dereferenced(values: &Dereferenced, other: &[u32]) -> usize { values.iter().zip(other).count() }

fn ordered_conversion(events: &RefCell<Vec<u8>>) -> Vec<u8> {
    Observed { events }
        .into_iter()
        .zip({
            events.borrow_mut().push(2);
            [2]
        })
        .map(|(left, right)| left.strict_add(right))
        .collect()
}

fn native_conversion(events: &RefCell<Vec<u8>>) -> usize {
    let values = HashMap::<u8, u8, _>::with_hasher(ObservedHasher { events });
    values
        .into_iter()
        .zip({
            events.borrow_mut().push(2);
            [(0, 0)]
        })
        .count()
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (map, turned) = (BTreeMap::from([(1, 2)]), Ring { values: vec![1, 2], offset: 1 });
    black_box(&(
        turned.iter().count(),
        owned(vec![1], vec![2]),
        borrowed(&[1], &[2]),
        bound(&[3, 4]),
        scaled(&[5, 6]),
        keyed(&map, &[(3, 4)]),
        ring(&turned, &[2]),
        mapped(&[1], &[2]),
        direct(&[1], &[2]),
    ));
    black_box(partial(&Partial { values: vec![1, 2] }, &[3]));
    black_box(dereferenced(&Dereferenced(vec![1, 2]), &[3]));
    let events = RefCell::new(Vec::new());
    black_box((ordered_conversion(&events), events.into_inner()));
    let events = RefCell::new(Vec::new());
    black_box((native_conversion(&events), events.into_inner()));
}
