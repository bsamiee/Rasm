use core::hint::black_box;
use std::collections::BTreeMap;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn viewed(value: Option<&String>) -> &str { value.map(String::as_str).unwrap_or_default() }

fn cloned(map: &BTreeMap<u32, String>, key: u32) -> String { map.get(&key).cloned().unwrap_or_default() }

fn counted(value: Option<u32>) -> u32 { value.unwrap_or_default() }

fn flagged(value: Option<bool>) -> bool { value.unwrap_or_default() }

fn initial(value: Option<char>) -> char { value.unwrap_or_default() }

fn listed(value: Option<Vec<u32>>) -> Vec<u32> { value.unwrap_or_default() }

fn mapped(value: Option<BTreeMap<u32, u32>>) -> BTreeMap<u32, u32> { value.unwrap_or_default() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let names = BTreeMap::from([(1, String::from("a"))]);
    black_box(&(viewed(None).len(), cloned(&names, 1), counted(None), flagged(None), initial(None), listed(None), mapped(None)));
}
