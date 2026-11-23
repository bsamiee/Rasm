use core::hint::black_box;
use std::collections::{BTreeSet, HashSet};
use std::rc::Rc;

// --- [MODELS] --------------------------------------------------------------------------

struct Pair(BTreeSet<u32>, BTreeSet<u32>);

impl Pair {
    fn intersection<'pair>(&'pair self, other: &'pair BTreeSet<u32>) -> impl Iterator<Item = &'pair u32> { self.0.iter().filter(move |item| other.contains(item) || self.1.contains(item)) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn intersected(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> BTreeSet<u32> { held.intersection(stated).copied().collect() } //~ manual_set_operator `held & stated`

fn united(held: BTreeSet<u32>, stated: BTreeSet<u32>) -> BTreeSet<u32> { held.union(&stated).copied().collect() } //~ manual_set_operator `&held | &stated`

fn subtracted(held: &BTreeSet<String>, stated: &BTreeSet<String>) -> BTreeSet<String> { held.difference(stated).cloned().collect() } //~ manual_set_operator `held - stated`

fn toggled(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> BTreeSet<u32> { held.symmetric_difference(stated).copied().collect() } //~ manual_set_operator `held ^ stated`

fn hashed(held: &HashSet<u32>, stated: &HashSet<u32>) -> HashSet<u32> { held.intersection(stated).copied().collect() } //~ manual_set_operator `held & stated`

fn nested(held: &Rc<BTreeSet<u32>>, stated: &&BTreeSet<u32>) -> BTreeSet<u32> { held.intersection(stated).copied().collect() } //~ manual_set_operator `&**held & *stated`

fn qualified(held: BTreeSet<u32>, stated: &BTreeSet<u32>) -> BTreeSet<u32> { Iterator::collect(Iterator::copied(BTreeSet::intersection(&held, stated))) } //~ manual_set_operator `&held & stated`

fn listed(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> Vec<u32> { held.intersection(stated).copied().collect() }

fn measured(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> usize { held.intersection(stated).copied().collect::<BTreeSet<u32>>().len() } //~ manual_set_operator `(held & stated)`

fn crossed(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> HashSet<u32> { held.intersection(stated).copied().collect() }

fn referenced<'set>(held: &'set BTreeSet<u32>, stated: &'set BTreeSet<u32>) -> BTreeSet<&'set u32> { held.intersection(stated).collect() }

fn paired(pair: &Pair, stated: &BTreeSet<u32>) -> BTreeSet<u32> { pair.intersection(stated).copied().collect() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (held, stated) = (BTreeSet::from([1, 2, 3]), BTreeSet::from([2, 3, 4]));
    let (named, titled) = (BTreeSet::from([String::from("a")]), BTreeSet::from([String::from("b")]));
    let (hashed_held, hashed_stated) = (HashSet::from([1, 2, 3]), HashSet::from([2, 3, 4]));
    black_box((intersected(&held, &stated), united(held.clone(), stated.clone()), subtracted(&named, &titled), toggled(&held, &stated)));
    black_box((hashed(&hashed_held, &hashed_stated), nested(&Rc::new(held.clone()), &&stated), qualified(held.clone(), &stated)));
    black_box((listed(&held, &stated), measured(&held, &stated), crossed(&held, &stated), referenced(&held, &stated)));
    black_box(paired(&Pair(held.clone(), stated.clone()), &stated));
}
