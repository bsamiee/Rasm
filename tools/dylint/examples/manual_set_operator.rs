use core::hint::black_box;
use std::{
    collections::{BTreeSet, HashSet},
    rc::Rc,
};

// --- [MODELS] --------------------------------------------------------------------------

struct Pair(BTreeSet<u32>, BTreeSet<u32>);

impl Pair {
    fn intersection<'pair>(&'pair self, other: &'pair BTreeSet<u32>) -> impl Iterator<Item = &'pair u32> { self.0.iter().filter(move |item| other.contains(item) || self.1.contains(item)) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn intersected(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> BTreeSet<u32> { held.intersection(stated).copied().collect() }

fn united(held: BTreeSet<u32>, stated: BTreeSet<u32>) -> BTreeSet<u32> { held.union(&stated).copied().collect() }

fn subtracted(held: &BTreeSet<String>, stated: &BTreeSet<String>) -> BTreeSet<String> { held.difference(stated).cloned().collect() }

fn toggled(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> BTreeSet<u32> { held.symmetric_difference(stated).copied().collect() }

fn hashed(held: &HashSet<u32>, stated: &HashSet<u32>) -> HashSet<u32> { held.intersection(stated).copied().collect() }

fn joined(held: HashSet<String>, stated: HashSet<String>) -> HashSet<String> { held.union(&stated).cloned().collect() }

fn removed(held: &HashSet<u32>, stated: &HashSet<u32>) -> HashSet<u32> { held.difference(stated).copied().collect() }

fn swapped(held: &HashSet<u32>, stated: &HashSet<u32>) -> HashSet<u32> { held.symmetric_difference(stated).copied().collect() }

fn mutated(held: &mut HashSet<u32>, stated: &HashSet<u32>) -> HashSet<u32> { held.intersection(stated).copied().collect() }

fn nested(held: &Rc<BTreeSet<u32>>, stated: &&BTreeSet<u32>) -> BTreeSet<u32> { held.intersection(stated).copied().collect() }

fn qualified(held: BTreeSet<u32>, stated: &BTreeSet<u32>) -> BTreeSet<u32> { Iterator::collect(Iterator::copied(BTreeSet::intersection(&held, stated))) }

fn listed(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> Vec<u32> { held.intersection(stated).copied().collect() }

fn counted(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> usize { held.intersection(stated).count() }

fn referenced<'set>(held: &'set BTreeSet<u32>, stated: &'set BTreeSet<u32>) -> BTreeSet<&'set u32> { held.intersection(stated).collect() }

fn operated(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> BTreeSet<u32> { held & stated }

fn paired(pair: &Pair, stated: &BTreeSet<u32>) -> BTreeSet<u32> { pair.intersection(stated).copied().collect() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (held, stated) = (BTreeSet::from([1, 2, 3]), BTreeSet::from([2, 3, 4]));
    let (named, titled) = (BTreeSet::from([String::from("a")]), BTreeSet::from([String::from("b")]));
    let (mut hashed_held, hashed_stated) = (HashSet::from([1, 2, 3]), HashSet::from([2, 3, 4]));
    let (keyed, labeled) = (HashSet::from([String::from("a")]), HashSet::from([String::from("b")]));
    black_box((intersected(&held, &stated), united(held.clone(), stated.clone()), subtracted(&named, &titled), toggled(&held, &stated)));
    black_box((hashed(&hashed_held, &hashed_stated), joined(keyed, labeled), removed(&hashed_held, &hashed_stated), swapped(&hashed_held, &hashed_stated)));
    black_box((mutated(&mut hashed_held, &hashed_stated), nested(&Rc::new(held.clone()), &&stated), qualified(held.clone(), &stated)));
    black_box((listed(&held, &stated), counted(&held, &stated), referenced(&held, &stated), operated(&held, &stated)));
    black_box(paired(&Pair(held.clone(), stated.clone()), &stated));
}
