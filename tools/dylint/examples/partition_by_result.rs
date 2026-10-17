use core::{hint::black_box, num::ParseIntError};

// --- [TYPES] ---------------------------------------------------------------------------

trait Small {
    fn is_small(&self) -> bool;
}

impl Small for Result<u8, ParseIntError> {
    fn is_small(&self) -> bool { matches!(self, Ok(value) if *value < 9) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn passed(result: &Result<u8, ParseIntError>) -> bool { result.is_ok() }

const fn large(value: &u8) -> bool { *value > 9 }

fn split(results: Vec<Result<u8, ParseIntError>>) -> (Vec<Result<u8, ParseIntError>>, Vec<Result<u8, ParseIntError>>) { results.into_iter().partition(passed) }

fn small(result: &Result<u8, ParseIntError>) -> bool { result.is_small() }

fn ranked(results: Vec<Result<u8, ParseIntError>>) -> (Vec<Result<u8, ParseIntError>>, Vec<Result<u8, ParseIntError>>) { results.into_iter().partition(small) }

fn sized(values: Vec<u8>) -> (Vec<u8>, Vec<u8>) { values.into_iter().partition(large) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(split(vec!["1".parse()]).0.len(), ranked(vec!["2".parse()]), sized(vec![1]))); }
