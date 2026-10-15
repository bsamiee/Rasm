#![feature(deref_patterns)]

use core::{fmt, hint::black_box, num::ParseIntError};
use std::{collections::BTreeSet, io};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Failure {
    message: String,
}

impl Failure {
    fn new(message: &str) -> Self { Self { message: message.trim().to_owned() } }
}

#[derive(Debug)]
struct Wrapped(String);

impl From<String> for Wrapped {
    fn from(value: String) -> Self { Self(value) }
}

#[derive(Debug)]
struct Attempt {
    count: u16,
    label: String,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
#[error("{message} after {attempts} attempts")]
struct RefusedError {
    message: String,
    attempts: u8,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn parsed(text: &str) -> Result<u16, ParseIntError> { text.parse() }

fn field(text: &str) -> Result<u16, Failure> { parsed(text).map_err(|error| Failure { message: error.to_string() }) }

fn wrapped(text: &str) -> Result<u16, Wrapped> { parsed(text).map_err(|error| Wrapped(format!("port: {error}"))) }

fn converted(text: &str) -> Result<u16, Wrapped> { parsed(text).map_err(|error| Wrapped(String::from(error.to_string().as_str()))) }

fn constructed(text: &str) -> Result<u16, Failure> { parsed(text).map_err(|error| Failure::new(&error.to_string())) }

fn raised(text: &str) -> Result<u16, Wrapped> {
    let port = parsed(text).map_err(|error| error.to_string())?;
    Ok(port.saturating_add(1))
}

fn assigned(text: &str, attempt: &mut Attempt) -> u16 {
    parsed(text).unwrap_or_else(|error| {
        attempt.label = error.to_string();
        0
    })
}

fn relayed_store(text: &str) -> Result<u16, Failure> {
    parsed(text).map_err(|error| {
        let message = format!("{error:?}");
        Failure { message }
    })
}

fn listed(error: &ParseIntError, messages: &mut Vec<String>) { messages.push(error.to_string()); }

fn closure(text: &str) -> Option<String> { parsed(text).map_err(|error| error.to_string()).err() }

fn errored(error: &ParseIntError) -> Result<u16, String> { Err(error.to_string()) }

fn collected(errors: &[ParseIntError]) -> Vec<String> { errors.iter().map(ToString::to_string).collect() }

fn described(error: &ParseIntError) -> String { error.to_string() }

fn fallback(error: &ParseIntError, label: &str) -> String {
    let mut message = label.to_owned();
    if message.is_empty() {
        message = error.to_string();
    }
    message
}

fn equal(error: &ParseIntError) -> bool { error.to_string() == "invalid digit found in string" }

fn contained(error: &io::Error) -> bool { ToString::to_string(error).contains("denied") }

fn matched(error: &ParseIntError) -> u16 {
    match format!("{error}") {
        "cannot parse integer from empty string" => 0,
        _ => 1,
    }
}

fn tested(error: &ParseIntError) -> usize { if let Some(("cannot", rest)) = error.to_string().split_once(' ') { rest.len() } else { 0 } }

fn prefixed(error: &ParseIntError) -> bool {
    let message = error.to_string();
    message.starts_with("invalid")
}

fn seen(error: &ParseIntError, known: &BTreeSet<String>) -> bool { known.contains(&error.to_string()) }

const fn log(text: &str) -> usize { text.len() }

fn logged(error: &ParseIntError) -> usize { log(&error.to_string()) }

fn printed(error: &ParseIntError) -> usize {
    let message = error.to_string();
    log(&message)
}

fn measured(error: &ParseIntError, label: &str) -> usize {
    let mut message = label.to_owned();
    if message.is_empty() {
        message = error.to_string();
    }
    message.len()
}

fn joined(errors: &[ParseIntError]) -> usize { log(&errors.iter().map(ToString::to_string).collect::<Vec<_>>().join(", ")) }

fn counted(port: u16) -> Attempt { Attempt { count: port, label: port.to_string() } }

fn kind(error: &io::Error) -> String { format!("{}", error.kind()) }

#[derive(Debug, Clone, Copy)]
enum Mode {
    Fast,
}

impl fmt::Display for Mode {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("fast") }
}

fn fast(mode: Mode) -> bool { mode.to_string() == "fast" }

const fn refused(message: String) -> RefusedError { RefusedError { message, attempts: 1 } }

fn rejected(error: &ParseIntError) -> RefusedError { refused(error.to_string()) }

fn handed(error: &ParseIntError) -> io::Error { io::Error::other(error.to_string()) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let text = "8080";
    let error = parsed("port").err();
    let denied = io::Error::from(io::ErrorKind::PermissionDenied);
    let mut attempt = counted(parsed(text).unwrap_or_default());
    let mut messages = Vec::new();
    let ports = [assigned(text, &mut attempt), attempt.count];
    let failures = [
        field(text).map_or_else(|failure| failure.message.len(), usize::from),
        wrapped(text).map_or_else(|wrapped| wrapped.0.len(), usize::from),
        converted(text).map_or_else(|wrapped| wrapped.0.len(), usize::from),
        constructed(text).map_or_else(|failure| failure.message.len(), usize::from),
        raised(text).map_or_else(|wrapped| wrapped.0.len(), usize::from),
        relayed_store(text).map_or_else(|failure| failure.message.len(), usize::from),
    ];
    let errors = error.iter().cloned().collect::<Vec<_>>();
    let lengths = error.as_ref().map(|error| {
        listed(error, &mut messages);
        (described(error), fallback(error, ""), logged(error), printed(error), measured(error, ""), matched(error), tested(error), errored(error))
    });
    black_box((lengths, failures, collected(&errors), joined(&errors)));
    let checks = error.as_ref().is_some_and(|error| equal(error) || prefixed(error) || seen(error, &BTreeSet::new())) || contained(&denied);
    let texts = [closure(text).unwrap_or_default(), kind(&denied), attempt.label, messages.concat()];
    let boundaries = error.as_ref().map(rejected).is_some_and(|refused| refused.attempts > 0) && error.as_ref().map(handed).is_some();
    black_box((ports, checks, boundaries, texts, refused(String::new()).attempts, fast(Mode::Fast), Failure::new(" ").message));
}
