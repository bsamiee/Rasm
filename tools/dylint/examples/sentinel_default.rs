use core::hint::black_box;
use std::{collections::BTreeMap, ffi::OsString, path::PathBuf};

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

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let names = BTreeMap::from([(1, String::from("a"))]);
    black_box(&(viewed(None).len(), cloned(&names, 1), counted(None), flagged(None), initial(None), listed(None), mapped(None), located(None), named(None), sliced(None)));
    black_box(&(paged(None, &[1]), pending(None), next(&names), counted_zero(None), listed_empty(None), both(None, Some("a"))));
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
