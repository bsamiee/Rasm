use core::{
    hint::black_box,
    num::{ParseIntError, TryFromIntError},
};

// --- [TYPES] ---------------------------------------------------------------------------

type Parsed = Result<u8, ParseIntError>;

trait Small {
    fn is_small(&self) -> bool;
}

impl Small for Parsed {
    fn is_small(&self) -> bool { matches!(self, Ok(value) if *value < 9) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn passed(result: &Parsed) -> bool { result.is_ok() }

const fn large(value: &u8) -> bool { *value > 9 }

fn small(result: &Parsed) -> bool { result.is_small() }

fn named(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(passed) }

fn pathed(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(Result::is_err) }

fn closed(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| result.is_ok()) }

fn negated(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| !result.as_ref().is_err()) }

fn called(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| Result::is_ok(result)) }

fn matched(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| matches!(result, Ok(_))) }

fn dereferenced(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| (*result).is_err()) }

fn copied(results: Vec<Result<u8, TryFromIntError>>) -> (Vec<Result<u8, TryFromIntError>>, Vec<Result<u8, TryFromIntError>>) { results.into_iter().partition(|&result| result.is_ok()) }

fn armed(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) {
    results.into_iter().partition(|result| match result {
        Ok(_) => true,
        Err(_) => false,
    })
}

fn ranked(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(small) }

fn flagged(results: Vec<Parsed>, ready: bool) -> (Vec<Parsed>, Vec<Parsed>) {
    results.into_iter().partition(|result| match result {
        Ok(_) => ready,
        _ => false,
    })
}

fn guarded(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| matches!(result, Ok(value) if *value > 1)) }

fn sized(values: Vec<u8>) -> (Vec<u8>, Vec<u8>) { values.into_iter().partition(large) }

fn compared(values: Vec<u8>) -> (Vec<u8>, Vec<u8>) { values.into_iter().partition(|value| *value > 1) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let parsed = || vec!["1".parse()];
    black_box(&(named(parsed()), pathed(parsed()), closed(parsed()), negated(parsed()), called(parsed()), matched(parsed()), dereferenced(parsed())));
    black_box(&(copied(vec![u8::try_from(1_u16)]), armed(parsed())));
    black_box(&(ranked(parsed()), flagged(parsed(), true), guarded(parsed()), sized(vec![1]), compared(vec![2])));
}
