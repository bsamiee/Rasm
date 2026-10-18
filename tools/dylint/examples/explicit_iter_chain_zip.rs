use core::{hint::black_box, iter, slice};
use std::collections::BTreeMap;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Ring {
    values: Vec<u32>,
    offset: usize,
}

impl Ring {
    fn iter(&self) -> iter::Skip<slice::Iter<'_, u32>> { self.values.iter().skip(self.offset) }
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
}
