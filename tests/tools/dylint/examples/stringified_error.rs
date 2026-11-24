#![feature(deref_patterns)]
#![cfg_attr(
    dylint_lib = "rasm_lints",
    allow(
        assertions_outside_tests,
        discarded_error,
        empty_string_default,
        forwarding_function,
        single_reader,
        untyped_error,
        reason = "these lints fire incidentally here and are exercised by their own fixtures"
    )
)]

use core::hint::black_box;
use core::marker::PhantomData;
use core::num::ParseIntError;
use core::{assert_matches, debug_assert_matches, fmt};
use std::collections::BTreeSet;
use std::io;

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

struct Renderer {
    format: fn(&ParseIntError) -> String,
}

struct Observed(PhantomData<String>);

impl FromIterator<String> for Observed {
    fn from_iter<I: IntoIterator<Item = String>>(messages: I) -> Self {
        messages.into_iter().for_each(|message| {
            log(&message);
        });
        Self(PhantomData)
    }
}

struct Observer;

impl Observer {
    fn observe(&mut self, message: String) { log(&message); }
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

fn field(text: &str) -> Result<u16, Failure> { parsed(text).map_err(|error| Failure { message: error.to_string() }) } //~ stringified_error

fn wrapped(text: &str) -> Result<u16, Wrapped> { parsed(text).map_err(|error| Wrapped(format!("port: {error}"))) } //~ stringified_error

fn converted(text: &str) -> Result<u16, Wrapped> { parsed(text).map_err(|error| Wrapped(String::from(error.to_string().as_str()))) } //~ stringified_error

fn raised(text: &str) -> Result<u16, Wrapped> {
    let port = parsed(text).map_err(|error| error.to_string())?;
    Ok(port.saturating_add(1))
}

fn assigned(text: &str, attempt: &mut Attempt) -> u16 {
    parsed(text).unwrap_or_else(|error| {
        attempt.label = error.to_string(); //~ stringified_error
        0
    })
}

fn relayed_store(text: &str) -> Result<u16, Failure> {
    parsed(text).map_err(|error| {
        let message = format!("{error:?}"); //~ stringified_error
        Failure { message }
    })
}

fn listed(error: &ParseIntError, messages: &mut Vec<String>) { messages.push(error.to_string()); } //~ stringified_error

fn closure(text: &str) -> Option<String> { parsed(text).map_err(|error| error.to_string()).err() }

fn errored(error: &ParseIntError) -> Result<u16, String> { Err(error.to_string()) }

fn successful(error: &ParseIntError) -> Result<String, itertools::structs::ExactlyOneError<Box<dyn Iterator<Item = u16>>>> { Ok(error.to_string()) } //~ stringified_error

const fn error_text(message: String) -> Result<u16, String> { Err(message) }

fn factory_errored(error: &ParseIntError) -> Result<u16, String> { error_text(error.to_string()) }

const fn success_text(message: String) -> Result<String, ParseIntError> { Ok(message) }

fn factory_view(error: &ParseIntError) -> Result<usize, ParseIntError> { Ok(success_text(error.to_string())?.len()) }

fn optional_view(error: &ParseIntError) -> Option<usize> { Some(Some(error.to_string())?.len()) }

fn collected(errors: &[ParseIntError]) -> Vec<String> { errors.iter().map(ToString::to_string).collect() } //~ stringified_error

fn described(error: &ParseIntError) -> String { error.to_string() } //~ stringified_error

fn fallback(error: &ParseIntError, label: &str) -> String {
    let mut message = label.to_owned();
    if message.is_empty() {
        message = error.to_string(); //~ stringified_error
    }
    message
}

fn equal(error: &ParseIntError) -> bool { error.to_string() == "invalid digit found in string" } //~ stringified_error

fn asserted(error: &ParseIntError) { assert_eq!(error.to_string(), "invalid digit found in string") } //~ stringified_error match the error's variant

fn guarded(error: &ParseIntError) {
    assert_matches!(Some(error), Some(cause) if cause.to_string() == "invalid digit found in string"); //~ stringified_error match the error's variant
    debug_assert_matches!(Some(error), Some(cause) if cause.to_string().contains("digit")); //~ stringified_error match the error's variant
}

fn contained(error: &io::Error) -> bool { ToString::to_string(error).contains("denied") } //~ stringified_error

fn matched(error: &ParseIntError) -> u16 {
    //~v stringified_error
    match format!("{error}") {
        "cannot parse integer from empty string" => 0,
        _ => 1,
    }
}

fn tested(error: &ParseIntError) -> usize { if let Some(("cannot", rest)) = error.to_string().split_once(' ') { rest.len() } else { 0 } } //~ stringified_error

fn prefixed(error: &ParseIntError) -> bool {
    let message = error.to_string(); //~ stringified_error
    message.starts_with("invalid")
}

fn seen(error: &ParseIntError, known: &BTreeSet<String>) -> bool { known.contains(&error.to_string()) } //~ stringified_error

const fn log(text: &str) -> usize { text.len() }

fn logged(error: &ParseIntError) -> usize { log(&error.to_string()) } //~ stringified_error

fn measured(error: &ParseIntError, label: &str) -> usize {
    let mut message = label.to_owned();
    if message.is_empty() {
        message = error.to_string();
    }
    message.len()
}

fn joined(errors: &[ParseIntError]) -> usize { log(&errors.iter().map(ToString::to_string).collect::<Vec<_>>().join(", ")) } //~ stringified_error

fn replaced(message: String) -> String {
    log(&message);
    String::from("unrelated")
}

fn observed(errors: &[ParseIntError]) -> Observed { errors.iter().map(ToString::to_string).collect() }

fn unrelated(error: &ParseIntError) -> bool { replaced(error.to_string()) == "unrelated" } //~ stringified_error

fn observed_once(error: &ParseIntError, observer: &mut Observer) { observer.observe(error.to_string()); } //~ stringified_error

fn counted(port: u16) -> Attempt { Attempt { count: port, label: port.to_string() } }

fn kind(error: &io::Error) -> String { format!("{}", error.kind()) } //~ display_format wraps one `Display` value

#[derive(Debug, Clone, Copy)]
enum Mode {
    Fast,
}

impl fmt::Display for Mode {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("fast") }
}

fn fast(mode: Mode) -> bool { mode.to_string() == "fast" } //~ stringified_error

fn labeled(mode: Mode, named: bool) -> usize { if let Some(label) = if named { Some(format!("mode {mode}")) } else { None } { log(&label) } else { 0 } }

const fn refused(message: String) -> RefusedError { RefusedError { message, attempts: 1 } }

fn rejected(error: &ParseIntError) -> RefusedError { refused(error.to_string()) } //~ stringified_error

fn handed(error: &ParseIntError) -> io::Error { io::Error::other(error.to_string()) }

fn owned_io(error: ParseIntError) -> io::Error { io::Error::other(error.to_string()) } //~ stringified_error

fn bounded_io<E: core::error::Error + Send + Sync + 'static>(error: E) -> io::Error { io::Error::other(error.to_string()) } //~ stringified_error

fn generic_io<E: core::error::Error>(error: E) -> io::Error { io::Error::other(error.to_string()) }

fn nonstatic_io<E: core::error::Error + Send + Sync>(error: E) -> io::Error { io::Error::other(error.to_string()) }

fn reused_io(error: ParseIntError) -> (io::Error, ParseIntError) { (io::Error::other(error.to_string()), error) } //~ stringified_error

fn native_kind(kind: io::ErrorKind) -> io::Error { io::Error::new(kind, "invalid port") }

async fn confirmed(prompt: String) -> bool { prompt.ends_with('?') }

fn checked(prompt: String, fallback: bool) -> bool { prompt.ends_with('?') || fallback }

fn relayed_comparison(error: &ParseIntError) -> bool {
    let first = error.to_string(); //~ stringified_error
    let second = first;
    let third = second;
    third == "invalid digit found in string"
}

fn padded(text: &str, width: usize) -> bool { format!("{text:width$}") == text }

fn returned_formatter() -> fn(&ParseIntError) -> String { ToString::to_string }

fn viewed_formatters(error: &ParseIntError) -> (usize, usize) {
    let renderer = Renderer { format: ToString::to_string };
    (log(&(renderer.format)(error)), log(&returned_formatter()(error)))
}

fn invoked_formatter(error: &ParseIntError) -> Wrapped {
    let render = ToString::to_string; //~ stringified_error
    Wrapped(render(error))
}

fn mixed_error(error: &ParseIntError) -> bool {
    let first = error.to_string(); //~ stringified_error
    let second = first.clone();
    let wrapped = Wrapped(first);
    black_box(wrapped);
    second == "invalid digit found in string"
}

fn boundary(error: &ParseIntError, index: usize) -> bool { error.to_string().is_char_boundary(index) }

fn closure_stored(error: &ParseIntError) -> Wrapped {
    let render = || error.to_string(); //~ stringified_error
    let alias = render;
    Wrapped(alias())
}

fn closure_viewed(error: &ParseIntError) -> usize {
    let render = || error.to_string(); //~ stringified_error
    log(&render())
}

fn overwritten(error: &ParseIntError, label: &str) -> bool {
    let mut message = error.to_string(); //~ stringified_error
    log(&message);
    message = label.to_owned();
    message == "external"
}

fn mutated(error: &ParseIntError, label: &str) -> bool {
    let mut message = error.to_string();
    message.clear();
    message.push_str(label);
    message == "external"
}

fn deferred(error: &ParseIntError) -> impl Fn() -> String + '_ { move || error.to_string() }

fn deferred_factory(error: &ParseIntError) -> impl FnOnce() -> String + '_ {
    let factory = || move || error.to_string();
    factory()
}

fn invoked_factory(error: &ParseIntError) -> Wrapped {
    let factory = || move || error.to_string(); //~ stringified_error
    Wrapped(factory()())
}

fn captured_factory(error: &ParseIntError) -> impl FnOnce() -> String {
    let message = error.to_string(); //~ stringified_error
    let factory = move || move || message;
    factory()
}

fn deferred_future(error: &ParseIntError) -> impl Future<Output = String> + '_ { async move { error.to_string() } }

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
        raised(text).map_or_else(|wrapped| wrapped.0.len(), usize::from),
        relayed_store(text).map_or_else(|failure| failure.message.len(), usize::from),
    ];
    let errors = error.iter().cloned().collect::<Vec<_>>();
    let lengths = error.as_ref().map(|error| {
        listed(error, &mut messages);
        (described(error), fallback(error, ""), logged(error), measured(error, ""), matched(error), tested(error), errored(error))
    });
    black_box(&(lengths, failures, collected(&errors), joined(&errors)));
    black_box(observed(&errors));
    black_box(error.as_ref().map(|error| (unrelated(error), observed_once(error, &mut Observer))));
    let checks = error.as_ref().is_some_and(|error| equal(error) || prefixed(error) || seen(error, &BTreeSet::new())) || contained(&denied);
    let texts = [closure(text).unwrap_or_default(), kind(&denied), attempt.label, messages.concat()];
    let boundaries = error.as_ref().map(rejected).is_some_and(|refused| refused.attempts > 0) && error.as_ref().map(handed).is_some();
    black_box(&(ports, checks, boundaries, texts, refused(String::new()).attempts, fast(Mode::Fast), labeled(Mode::Fast, true), Failure::new(" ").message));
    black_box(&(futures::executor::block_on(confirmed(format!("Delete {text}?"))), checked(format!("Delete {text}?"), false)));
    black_box(error.as_ref().map(relayed_comparison));
    black_box(error.as_ref().map(|error| (asserted(error), guarded(error))));
    black_box((padded(text, 8), error.as_ref().map(|error| (viewed_formatters(error), invoked_formatter(error)))));
    black_box((error.as_ref().map(|error| boundary(error, 0)), checked(format!("Delete {}?", attempt.count), false)));
    black_box(error.as_ref().map(|error| (closure_stored(error), closure_viewed(error), mixed_error(error))));
    black_box(error.as_ref().map(|error| (successful(error), factory_errored(error), optional_view(error), factory_view(error))));
    black_box(error.as_ref().map(|error| (overwritten(error, text), mutated(error, text))));
    black_box(error.as_ref().map(|error| (deferred(error), deferred_factory(error), deferred_future(error))));
    black_box(error.as_ref().map(|error| (invoked_factory(error), captured_factory(error))));
    black_box(error.as_ref().map(|error| (owned_io(error.clone()), bounded_io(error.clone()), generic_io(error.clone()), nonstatic_io(error.clone()), reused_io(error.clone()))));
    black_box(native_kind(io::ErrorKind::InvalidData));
}
