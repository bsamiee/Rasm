#![feature(allocator_ext, btreemap_alloc)]

use core::hint::black_box;
use std::{
    alloc::System,
    cell::Cell,
    collections::{BTreeMap, BTreeSet, HashMap, HashSet, hash_map::DefaultHasher},
    ffi::OsString,
    hash::BuildHasher,
    path::PathBuf,
};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
struct CopyDefault;

impl Default for CopyDefault {
    fn default() -> Self {
        eprintln!("constructing the default");
        Self
    }
}

impl PartialEq<CopyDefault> for u32 {
    fn eq(&self, _: &CopyDefault) -> bool { self.is_multiple_of(2) }
}

struct Tracked<'a>(&'a Cell<usize>);

impl Drop for Tracked<'_> {
    fn drop(&mut self) { self.0.set(self.0.get() + 1); }
}

struct HashBuilder;

impl Default for HashBuilder {
    fn default() -> Self {
        eprintln!("constructing the hasher");
        Self
    }
}

impl BuildHasher for HashBuilder {
    type Hasher = DefaultHasher;

    fn build_hasher(&self) -> Self::Hasher { DefaultHasher::new() }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn viewed(value: Option<&String>) -> &str { value.map(String::as_str).unwrap_or_default() }

fn cloned(map: &BTreeMap<u32, String>, key: u32) -> String { map.get(&key).cloned().unwrap_or_default() }

fn counted(value: Option<u32>) -> u32 { value.unwrap_or_default() }

fn flagged(value: Option<bool>) -> bool { value.unwrap_or_default() }

fn initial(value: Option<char>) -> char { value.unwrap_or_default() }

fn listed(value: Option<Vec<u32>>) -> Vec<u32> { value.unwrap_or_default() }

fn mapped(value: Option<BTreeMap<u32, u32>>) -> BTreeMap<u32, u32> { value.unwrap_or_default() }

fn located(value: Option<PathBuf>) -> PathBuf { value.unwrap_or_default() }

fn named(value: Option<OsString>) -> OsString { value.unwrap_or_default() }

fn sliced(value: Option<&[u8]>) -> &[u8] { value.unwrap_or_default() }

fn captured(value: Option<u32>) -> bool {
    let count = value.unwrap_or_default();
    black_box(|| count.saturating_add(1));
    count == 0
}

fn hashed(value: Option<HashMap<u32, u32, HashBuilder>>) -> bool { value.unwrap_or_default().is_empty() }

fn hashed_set(value: Option<HashSet<u32, HashBuilder>>) -> bool { value.unwrap_or_default().is_empty() }

fn allocated(value: Option<BTreeMap<u32, u32, System>>) -> bool { value.unwrap_or(BTreeMap::new_in(System)).is_empty() }

fn allocated_set(value: Option<BTreeSet<u32, System>>) -> bool { value.unwrap_or(BTreeSet::new_in(System)).is_empty() }

fn map_empty(value: Option<BTreeMap<u32, u32>>) -> bool { value.unwrap_or_default().is_empty() }

fn set_empty(value: Option<BTreeSet<u32>>) -> bool { value.unwrap_or_default().is_empty() }

fn numeric(value: Option<u32>) -> bool { value.unwrap_or_default() == 0 }

fn textual(value: Option<&str>) -> bool { value.unwrap_or("").is_empty() }

fn borrowed(value: Option<&[u8]>) -> bool { value.unwrap_or_default().is_empty() }

fn mapped_test(value: Option<u32>) -> bool { value.map_or(0, |value| value.saturating_mul(2)) == 0 }

fn matched(value: Option<char>) -> bool {
    match value.unwrap_or_default() {
        '\0' => false,
        _ => true,
    }
}

fn custom_default(value: Option<CopyDefault>) -> bool { value.unwrap_or_default() == CopyDefault::default() }

fn compared_default(value: Option<u32>) -> bool { value.unwrap_or_default() == CopyDefault::default() }

fn destruction(value: Option<Vec<Tracked<'_>>>, drops: &Cell<usize>) -> (bool, usize) { black_box((value.unwrap_or_default().is_empty(), drops.get())) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let names = BTreeMap::from([(1, String::from("a"))]);
    black_box(&(viewed(None).len(), cloned(&names, 1), counted(None), flagged(None), initial(None), listed(None), mapped(None), located(None), named(None), sliced(None)));
    black_box(&(paged(None, &[1]), pending(None), next(&names), counted_zero(None), listed_empty(None), both(None, Some("a")), changed(None), captured(None)));
    black_box((hashed(None), hashed_set(None), allocated(None), allocated_set(None), map_empty(None), set_empty(None)));
    black_box((numeric(None), textual(None), borrowed(None), mapped_test(None), matched(None), custom_default(None), compared_default(None)));
    let drops = Cell::new(0);
    black_box(destruction(Some(vec![Tracked(&drops)]), &drops));
}

macro_rules! offset {
    ($cursor:expr, $rows:expr) => {
        $cursor.map_or(0, |cursor: u32| $rows.partition_point(|&row| row < cursor))
    };
}

fn paged(cursor: Option<u32>, rows: &[u32]) -> usize { offset!(cursor, rows) }

fn pending(responses: Option<Vec<u32>>) -> u32 { responses.unwrap_or_default().iter().sum() }

fn next(rows: &BTreeMap<u32, String>) -> u32 { rows.last_key_value().map_or(0, |(row, _)| row.saturating_add(1)) }

fn counted_zero(value: Option<u32>) -> &'static str {
    let count = value.unwrap_or(0);
    if count == 0 { "none" } else { "some" }
}

fn listed_empty(value: Option<Vec<u32>>) -> bool { value.unwrap_or_default().is_empty() }

macro_rules! tested {
    ($value:expr) => {
        $value.unwrap_or("") == ""
    };
}

fn both(left: Option<&str>, right: Option<&str>) -> bool { tested!(left) && tested!(right) }

fn changed(value: Option<u32>) -> bool {
    let mut count = value.unwrap_or_default();
    count = count.saturating_add(1);
    count == 0
}
