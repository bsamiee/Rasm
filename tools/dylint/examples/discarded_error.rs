use core::{convert::Infallible, hint::black_box, num::ParseIntError};
use std::{
    process::ExitCode,
    sync::{Mutex, mpsc},
};

use itertools::Itertools as _;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn parse(text: &str) -> Result<u32, ParseIntError> { text.parse() }

fn effect(text: &str) -> Result<(), ParseIntError> { parse(text).map(drop) }

fn infallible(value: u32) -> Result<u32, Infallible> { Ok(value) }

fn reported(error: &ParseIntError) -> u32 { u32::from(error.to_string().is_empty()) }

fn fallbacks(text: &str) -> [u32; 6] {
    [
        parse(text).unwrap_or(0),
        parse(text).unwrap_or_default(),
        parse(text).map_or(0, |value| value.strict_add(1)),
        parse(text).unwrap_or_else(|_| 0),
        parse(text).unwrap_or_else(|_ignored| 0),
        parse(text).map_or_else(|_| 0, |value| value.strict_add(1)),
    ]
}

fn successes(text: &str) -> [Result<(), ParseIntError>; 2] { [effect(text).or(Ok(())), effect(text).or_else(|_| Ok(()))] }

fn absences(text: &str) -> Option<u32> { parse(text).ok()?.checked_add(parse(text).ok().map_or(0, |value| value)) }

fn returned(text: &str) -> Option<u32> { parse(text).ok() }

fn received(receiver: &mpsc::Receiver<u32>) -> Option<u32> { receiver.recv().ok() }

fn closed(receiver: &mpsc::Receiver<u32>) -> [bool; 1] { [receiver.recv().is_ok()] }

fn kept(texts: &[&str]) -> (Vec<u32>, Option<u32>, Option<u32>) {
    let parsed = parse(texts[0]);
    let first = parsed.as_ref().ok().copied();
    (
        texts.iter().map(|text| parse(text)).filter_map(Result::ok).collect(),
        first,
        parse(texts[0])
            .inspect_err(|error| {
                black_box(error);
            })
            .ok()
            .or(parsed.err().map(|_| 0)),
    )
}

fn views(text: &str) -> [bool; 7] {
    [
        parse(text).is_ok_and(|value| value > 1),
        parse(text).is_ok(),
        parse(text).is_err(),
        parse(text).is_err_and(|_| true),
        parse(text).err().is_none(),
        parse(text).err().is_some(),
        matches!(parse(text), Ok(1)),
    ]
}

fn patterns(texts: &[&str]) -> u32 {
    if let Ok(value) = parse(texts[0]) {
        black_box(value);
    }
    if let Err(_) = parse(texts[0]) {}
    let fallback = if let Ok(value) = parse(texts[0]) { value } else { 0 };
    let Ok(start) = parse(texts[0]) else { return fallback };
    match parse(texts[1]) {
        Ok(value) => start.strict_add(value),
        Err(_) => 0,
    }
}

fn worked(texts: &[&str]) -> u32 {
    if let Err(_) = parse(texts[0]) {
        black_box(texts);
    }
    let derived = if let Ok(value) = parse(texts[0]) { value } else { u32::from(texts.is_empty()) };
    let held = parse(texts[0]).ok();
    match parse(texts[1]) {
        Ok(value) => derived.strict_add(value).strict_add(u32::from(held.is_some())),
        Err(error) => reported(&error),
    }
}

fn skipped(texts: &[&str]) -> u32 {
    let mut sum = 0;
    for text in texts {
        match parse(text) {
            Ok(value) => sum = value,
            Err(_) => continue,
        }
    }
    sum
}

fn single(values: &[u32]) -> Option<&u32> { values.iter().exactly_one().ok() }

fn locked(shared: &Mutex<u32>) -> [u32; 2] { [shared.lock().map_or(0, |guard| *guard), shared.try_lock().map_or(0, |guard| *guard)] }

fn compared(values: &[u32]) -> u32 { if let Ok(value) = values.iter().all_equal_value() { *value } else { 0 } }

fn walked(texts: &[&str]) -> (Vec<u32>, Vec<u32>, usize, usize) {
    (texts.iter().map(|text| parse(text)).flatten().collect(), parse(texts[0]).into_iter().collect(), parse(texts[0]).iter().count(), parse(texts[0]).iter_mut().count())
}

fn tested(text: &str) -> Result<bool, ParseIntError> { Ok(parse(text).is_ok_and(|value| value > 1)) }

fn predicate(text: &str) -> bool { parse(text).is_ok_and(|value| value > 1) }

fn statements(text: &str) {
    drop(effect(text));
    _ = effect(text);
    let _ignored = effect(text);
}

macro_rules! defaulted {
    ($text:expr) => {
        parse($text).unwrap_or_default()
    };
}

fn expanded(text: &str) -> u32 { defaulted!(text) }

fn handled(text: &str) -> [u32; 2] { [parse(text).unwrap_or_else(|error| reported(&error)), parse(text).map_or_else(|error| reported(&error), |value| value)] }

fn translated(text: &str) -> ExitCode { parse(text).map_or(ExitCode::FAILURE, |_| ExitCode::SUCCESS) }

fn certain(value: u32) -> u32 { infallible(value).unwrap_or_default() }

fn saturated(value: i64) -> (u8, i8) { (u8::try_from(value).unwrap_or(u8::MAX), i8::try_from(value).unwrap_or_else(|_| if value < 0 { i8::MIN } else { i8::MAX })) }

fn clamped(value: i64) -> u8 { u8::try_from(value).unwrap_or(1) }

fn absent(value: Option<u32>) -> u32 { value.unwrap_or(0) }

fn propagated(text: &str) -> Result<u32, ParseIntError> { parse(text).or_else(|error| if error.to_string().is_empty() { Ok(0) } else { Err(error) }) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    statements("1");
    black_box(&(fallbacks("1"), successes("1"), views("1"), walked(&["1", "x"]), expanded("2"), handled("3")));
    black_box(&(translated("4") == ExitCode::SUCCESS, certain(5), saturated(-5), clamped(-5), absent(None), propagated("6"), tested("7"), predicate("8")));
    black_box(&(absences("9"), returned("10"), received(&mpsc::channel().1), closed(&mpsc::channel().1), kept(&["11"]), patterns(&["12", "13"]), skipped(&["14"]), compared(&[15])));
    black_box(&(single(&[16]), locked(&Mutex::new(17)), worked(&["18", "19"])));
}
