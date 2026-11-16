#![feature(const_result_trait_fn, const_trait_impl, integer_casts, option_get_or_try_insert_with, option_reduce)]

use core::convert::Infallible;
use core::fmt;
use core::hint::black_box;
use core::num::{ParseIntError, TryFromIntError};
use std::process::ExitCode;
use std::sync::{Mutex, mpsc};

use itertools::Itertools as _;
use rasm_lints_auxiliary::{DroppedError, OpenError, SingleError};
use tokio::sync::{broadcast, oneshot};

// --- [CONSTANTS] -----------------------------------------------------------------------

const PARSED: u32 = u32::from_str_radix("21", 10).ok().unwrap();

// --- [MODELS] --------------------------------------------------------------------------

struct Results(std::vec::IntoIter<Result<u32, ParseIntError>>);

impl Results {
    fn flatten(self) -> Result<Vec<u32>, ParseIntError> { self.0.collect() }
}

impl Iterator for Results {
    type Item = Result<u32, ParseIntError>;

    fn next(&mut self) -> Option<Self::Item> { self.0.next() }
}

#[derive(Debug, Default)]
struct Record {
    closed: bool,
}

impl Record {
    fn step(self) -> Result<Self, Rejected<Self>> {
        let reason = if self.closed { Reason::Closed } else { Reason::Reserved };
        Err(Rejected { value: self, reason })
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Reason {
    #[error("closed")]
    Closed,
    #[error("reserved")]
    Reserved,
}

#[derive(Debug, thiserror::Error)]
enum Bracket {
    #[error("unbracketed between {lower} and {upper}")]
    Unbracketed { lower: u32, upper: u32 },
    #[error("empty")]
    Empty,
}

#[derive(Debug, thiserror::Error)]
enum Refusal {
    #[error("reserved")]
    Reserved,
}

#[derive(Debug, thiserror::Error)]
#[error("{reason}")]
struct Rejected<T: fmt::Debug> {
    value: T,
    reason: Reason,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn parse(text: &str) -> Result<u32, ParseIntError> { text.parse() }

fn effect(text: &str) -> Result<(), ParseIntError> { parse(text).map(drop) }

fn infallible(value: u32) -> Result<u32, Infallible> { Ok(value) }

fn reported(error: &ParseIntError) -> u32 { u32::from(error.to_string().is_empty()) }

fn fallbacks(text: &str) -> [u32; 7] {
    [
        parse(text).unwrap_or(0),
        parse(text).unwrap_or_default(),
        parse(text).map_or(0, |value| value.strict_add(1)),
        parse(text).map_or_default(|value| value.strict_add(1)),
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

fn bracketed(found: Result<u32, Bracket>) -> Result<Option<u32>, Bracket> {
    match found {
        Ok(value) => Ok(Some(value)),
        Err(Bracket::Unbracketed { .. }) => Ok(None),
        Err(error @ Bracket::Empty) => Err(error),
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

fn single_viewed(values: &[u32]) -> bool { values.iter().exactly_one().as_ref().ok().is_some() }

fn single_batch(groups: &[Vec<u32>]) -> (Vec<&u32>, Vec<&u32>) {
    (groups.iter().map(|values| values.iter().exactly_one()).filter_map(Result::ok).collect(), groups.iter().map(|values| values.iter().exactly_one()).flatten().collect())
}

fn single_pattern(values: &[u32]) { let (Ok(_) | Err(_)) = values.iter().exactly_one(); }

fn rejected(record: Record) -> Record { record.step().unwrap_or_default() }

fn rejected_absence(record: Record) -> Option<Record> { record.step().ok() }

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

fn native_saturation(value: i64) -> (u8, i8) { (value.saturating_cast(), value.saturating_cast()) }

fn opaque_saturation(read: impl FnOnce() -> Result<u8, TryFromIntError>) -> u8 { read().unwrap_or(u8::MAX) }

fn reversed_saturation(value: i64) -> i8 { i8::try_from(value).unwrap_or_else(|_| if value < 0 { i8::MAX } else { i8::MIN }) }

fn clamped(value: i64) -> u8 { u8::try_from(value).unwrap_or(1) }

fn narrowed(value: i64) -> u8 { u8::try_from(value).map_or_default(|_| u8::MAX) }

fn absent(value: Option<u32>) -> u32 { value.unwrap_or(0) }

fn propagated(text: &str) -> Result<u32, ParseIntError> { parse(text).or_else(|error| if error.to_string().is_empty() { Ok(0) } else { Err(error) }) }

fn alternatives(text: &str) {
    fn returned_back(values: Vec<u32>) -> Result<(), Vec<u32>> { if values.is_empty() { Ok(()) } else { Err(values) } }
    let (Ok(()) | Err(_)) = effect(text);
    let (Ok(()) | Err(ParseIntError { .. })) = effect(text);
    let (Ok(()) | Err(_)) = returned_back(vec![1]);
    let (Ok(()) | Err(Vec { .. })) = returned_back(vec![2]);
    match effect(text) {
        Ok(()) | Err(_) => (),
    }
}

fn asserted(text: &str) -> [u32; 2] { [const { u32::from_str_radix("22", 10).ok().expect("radix 10 parses decimal digits") }, parse(text).ok().unwrap()] }

fn preserved(text: &str) -> Result<Vec<u32>, ParseIntError> { Results(vec![parse(text)].into_iter()).flatten() }

fn nested(value: Result<Result<(), ParseIntError>, ParseIntError>) {
    match value {
        Ok(Err(_)) | Ok(Ok(())) => (),
        Err(error) => {
            black_box(error);
        }
    }
}

fn partial(value: u64) -> Option<u16> { u16::try_from(value).ok() }

fn partial_mapped(value: u64) -> Option<u32> { Some(u32::from(u16::try_from(value).ok()?)) }

fn conversion_outcomes(value: u64) -> (bool, u16, Result<u16, ParseIntError>) { (u16::try_from(value).is_ok(), u16::try_from(value).unwrap_or_default(), u16::try_from(value).or_else(|_| Ok(0))) }

fn local_singleton(value: Result<u32, Refusal>) -> Option<u32> { value.ok() }

fn foreign_singleton(value: Result<u32, SingleError>) -> Option<u32> { value.ok() }

fn singleton_items(values: [Result<u32, Refusal>; 2]) -> Vec<u32> { values.into_iter().filter_map(Result::ok).collect() }

fn singleton_translation(value: Result<u32, Refusal>) -> Result<u32, Reason> { value.ok().filter(|&value| value > 0).ok_or(Reason::Reserved) }

fn open_error(value: Result<u32, OpenError>) -> Option<u32> { value.ok() }

fn dropped_error(value: Result<u32, DroppedError>) -> Option<u32> { value.ok() }

fn multiple_causes(value: Result<u32, Reason>) -> Option<u32> { value.ok() }

fn singleton_outcomes(read: impl Fn() -> Result<u32, SingleError>) -> (bool, u32, Result<u32, SingleError>) {
    drop(read());
    (read().is_ok(), read().unwrap_or_default(), read().or(Ok(0)))
}

fn optional_fallback(value: Result<u32, SingleError>) -> u32 { value.ok().unwrap_or_default() }

fn optional_view(value: Result<u32, SingleError>) -> [bool; 1] { [value.ok().is_some()] }

fn optional_success(value: Result<u32, SingleError>) -> Option<u32> { value.ok().or(Some(0)) }

fn optional_absence(value: Result<u32, SingleError>) -> Option<u32> { value.ok().or(None) }

fn optional_relays(value: Result<u32, SingleError>) -> u32 { value.ok().map(u32::reverse_bits).or(None).unwrap_or_default() }

fn optional_predicate(value: Result<u32, SingleError>) -> bool { value.ok().is_some() }

fn optional_success_closure(value: Result<u32, SingleError>) -> Option<u32> { value.ok().or_else(|| Some(0)) }

fn optional_absence_closure(value: Result<u32, SingleError>) -> Option<u32> { value.ok().or_else(|| None) }

fn optional_iteration(value: Result<u32, SingleError>) -> Vec<u32> { value.ok().into_iter().collect() }

fn optional_exit(value: Result<u32, SingleError>) -> ExitCode { value.ok().map_or(ExitCode::FAILURE, |_| ExitCode::SUCCESS) }

fn optional_returned_success(value: Result<u32, SingleError>) -> Option<u32> { value.ok().or_else(|| return Some(0)) }

fn optional_returned_absence(value: Result<u32, SingleError>) -> Option<u32> { value.ok().or_else(|| return None) }

fn optional_untyped_translation(value: Result<u32, SingleError>) -> u32 { value.ok().ok_or(()).unwrap_or_default() }

fn optional_exclusive_success(value: Result<u32, SingleError>) -> Option<u32> { value.ok().xor(Some(0)) }

fn optional_exclusive_absence(value: Result<u32, SingleError>) -> Option<u32> { value.ok().xor(None) }

fn optional_reduced_success(value: Result<u32, SingleError>) -> Option<u32> { value.ok().reduce(Some(0), u32::strict_add) }

fn optional_reduced_absence(value: Result<u32, SingleError>) -> Option<u32> { value.ok().reduce(None::<u32>, u32::strict_add) }

fn optional_try_insert_success(value: Result<u32, SingleError>) -> Option<u32> { value.ok().get_or_try_insert_with(|| Some(0)).copied() }

fn optional_try_insert_absence(value: Result<u32, SingleError>) -> Option<u32> { value.ok().get_or_try_insert_with(|| None).copied() }

fn optional_try_insert_translation(value: Result<u32, SingleError>) -> Result<u32, Reason> { value.ok().get_or_try_insert_with(|| Err(Reason::Closed)).copied() }

fn optional_try_insert_return(value: Result<u32, SingleError>, choose: bool) -> Result<u32, Reason> {
    value
        .ok()
        .get_or_try_insert_with(|| {
            Err({
                if choose {
                    return Ok(0);
                }
                Reason::Closed
            })
        })
        .copied()
}

fn optional_try_insert_break(value: Result<u32, SingleError>, choose: bool) -> Result<u32, Reason> {
    value
        .ok()
        .get_or_try_insert_with(|| 'reply: {
            Err({
                if choose {
                    break 'reply Ok(0);
                }
                Reason::Closed
            })
        })
        .copied()
}

fn publications(sender: &broadcast::Sender<String>) {
    drop(sender.send(String::new()));
    sender.send(String::new()).ok();
    let _ignored = sender.send(String::new());
}

fn publication_without_observers() {
    let sender = broadcast::Sender::new(1);
    publications(&sender);
}

fn publication_with_observer() {
    let (sender, _receiver) = broadcast::channel(1);
    publications(&sender);
}

fn publication_absence(sender: &broadcast::Sender<String>) -> Option<usize> { sender.send(String::new()).ok() }

fn publication_fallback(sender: &broadcast::Sender<String>) -> usize { sender.send(String::new()).unwrap_or_default() }

fn publication_view(sender: &broadcast::Sender<String>) -> [bool; 1] { [sender.send(String::new()).is_ok()] }

fn opaque_publication(result: Result<usize, broadcast::error::SendError<String>>) { drop(result); }

fn publication_break(sender: &broadcast::Sender<String>, incoming: Result<usize, broadcast::error::SendError<String>>, choose: bool) {
    drop('publish: { sender.send(if choose { break 'publish incoming } else { String::new() }) });
}

fn other_channels() {
    let (sender, receiver) = mpsc::channel();
    drop(receiver);
    drop(sender.send(String::new()));
    let (sender, receiver) = oneshot::channel();
    drop(receiver);
    drop(sender.send(String::new()));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    statements("1");
    alternatives("20");
    nested(Ok(effect("24")));
    single_pattern(&[16]);
    black_box(&preserved("21"));
    black_box(&(fallbacks("1"), successes("1"), views("1"), walked(&["1", "x"]), expanded("2"), handled("3")));
    black_box(&(translated("4") == ExitCode::SUCCESS, certain(5), saturated(-5), clamped(-5), narrowed(-5), absent(None), propagated("6"), tested("7"), predicate("8")));
    black_box(&(absences("9"), returned("10"), received(&mpsc::channel().1), closed(&mpsc::channel().1), kept(&["11"]), patterns(&["12", "13"]), skipped(&["14"]), compared(&[15])));
    black_box(&(single(&[16]), single_viewed(&[16]), single_batch(&[vec![16]]), locked(&Mutex::new(17)), worked(&["18", "19"]), PARSED, asserted("23")));
    black_box(&(rejected(Record::default()), rejected_absence(Record::default()), Record { closed: true }.step().map_err(|error| error.value)));
    black_box(&(partial(25), partial_mapped(26), conversion_outcomes(27)));
    black_box(&(bracketed(Err(Bracket::Unbracketed { lower: 1, upper: 2 })), bracketed(Err(Bracket::Empty))));
    black_box(&(native_saturation(-28), opaque_saturation(|| u8::try_from(29_i64)), reversed_saturation(-30)));
    black_box(&(local_singleton(Err(Refusal::Reserved)), foreign_singleton(Err(SingleError)), singleton_items([Ok(31), Err(Refusal::Reserved)]), singleton_translation(Err(Refusal::Reserved))));
    black_box(&(open_error(Err(OpenError::default())), dropped_error(Err(DroppedError)), multiple_causes(Err(Reason::Closed)), singleton_outcomes(|| Err(SingleError))));
    black_box(&(optional_fallback(Err(SingleError)), optional_view(Err(SingleError)), optional_success(Err(SingleError)), optional_absence(Err(SingleError)), optional_relays(Err(SingleError))));
    black_box(&(optional_predicate(Err(SingleError)), optional_success_closure(Err(SingleError)), optional_absence_closure(Err(SingleError))));
    black_box(&(optional_iteration(Err(SingleError)), optional_exit(Err(SingleError))));
    black_box(&(optional_returned_success(Err(SingleError)), optional_returned_absence(Err(SingleError)), optional_untyped_translation(Err(SingleError))));
    black_box(&(optional_exclusive_success(Err(SingleError)), optional_exclusive_absence(Err(SingleError))));
    black_box(&(optional_reduced_success(Err(SingleError)), optional_reduced_absence(Err(SingleError))));
    black_box(&(optional_try_insert_success(Err(SingleError)), optional_try_insert_absence(Err(SingleError)), optional_try_insert_translation(Err(SingleError))));
    black_box(&(optional_try_insert_return(Err(SingleError), true), optional_try_insert_break(Err(SingleError), true)));
    publication_without_observers();
    publication_with_observer();
    other_channels();
    let sender = broadcast::Sender::new(1);
    opaque_publication(sender.send(String::new()));
    publication_break(&sender, sender.send(String::new()), true);
    black_box(&(publication_absence(&sender), publication_fallback(&sender), publication_view(&sender)));
}
