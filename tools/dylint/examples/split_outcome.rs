use core::{error::Error, hint::black_box, num::ParseIntError};
use std::io;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Attempt {
    value: Option<u16>,
    error: Option<ParseIntError>,
}

#[derive(Debug)]
struct Check {
    passed: bool,
    failure: Option<io::Error>,
}

#[derive(Debug)]
struct Fetched {
    body: Option<String>,
    cause: Box<dyn Error + Send + Sync>,
}

#[derive(Debug)]
struct Probe(bool, Option<io::Error>);

#[derive(Debug)]
struct Outcome {
    result: Result<u16, ParseIntError>,
}

#[derive(Debug)]
struct Job {
    id: u16,
    last: Option<io::Error>,
}

#[derive(Debug)]
struct Settings {
    verbose: bool,
    port: Option<u16>,
}

#[derive(Debug)]
struct Batch {
    done: bool,
    failures: Vec<io::Error>,
}

#[derive(Debug)]
struct Lookup {
    found: bool,
    value: Option<u16>,
}

#[derive(Debug)]
struct Cached {
    missing: bool,
    entry: Option<String>,
}

#[derive(Debug)]
struct Toggle {
    on: bool,
    last: Option<u16>,
}

#[derive(Debug)]
struct Parsed {
    port: Option<u16>,
    failure: Option<ParseIntError>,
}

#[derive(Debug)]
struct Pending {
    ready: bool,
    slot: Option<u16>,
}

#[derive(Debug)]
struct Reply {
    value: Option<u16>,
    refusal: Option<Closed>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, thiserror::Error)]
#[error("port is closed")]
struct Closed;

#[derive(Debug, thiserror::Error)]
#[error("cannot read {path:?}")]
struct ReadError {
    path: Option<String>,
    source: io::Error,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn attempt(text: &str) -> Attempt {
    let parsed = text.parse::<u16>();
    Attempt { value: parsed.as_ref().ok().copied(), error: parsed.err() }
}

fn port(attempt: &Attempt) -> u16 { if attempt.error.is_none() { attempt.value.unwrap_or_default() } else { 0 } }

const fn checked(check: &Check) -> bool { check.passed && check.failure.is_none() }

fn fetched(fetched: &Fetched) -> usize { fetched.body.as_ref().map_or(0, String::len).strict_add(usize::from(fetched.cause.source().is_none())) }

const fn probed(probe: &Probe) -> bool { probe.0 && probe.1.is_none() }

fn outcome(outcome: &Outcome) -> u16 { outcome.result.as_ref().map_or(0, |port| *port) }

const fn job(job: &Job) -> u16 { if job.last.is_none() { job.id } else { 0 } }

fn settings(settings: &Settings) -> u16 { if settings.verbose { settings.port.unwrap_or_default() } else { 0 } }

const fn batch(batch: &Batch) -> bool { batch.done && batch.failures.is_empty() }

fn lookup(table: &[(u16, u16)], key: u16) -> Lookup {
    match table.iter().find(|(held, _)| *held == key) {
        Some(&(_, value)) => Lookup { found: true, value: Some(value) },
        None => Lookup { found: false, value: None },
    }
}

const fn cached(entry: Option<String>) -> Cached { Cached { missing: entry.is_none(), entry } }

const fn toggles() -> [Toggle; 2] { [Toggle { on: true, last: Some(8080) }, Toggle { on: true, last: None }] }

fn pending(ready: bool) -> Pending {
    let mut held = if ready { Pending { ready: true, slot: Some(8080) } } else { Pending { ready: false, slot: None } };
    held.ready.toggle();
    held
}

fn parsed(text: &str) -> Parsed {
    match text.parse::<u16>() {
        Ok(port) => Parsed { port: Some(port), failure: None },
        Err(failure) => Parsed { port: None, failure: Some(failure) },
    }
}

fn reply(outcome: Result<u16, Closed>) -> Reply { Reply { value: outcome.ok(), refusal: outcome.err() } }

fn read(path: &str) -> Result<String, ReadError> { fs_err::read_to_string(path).map_err(|source| ReadError { path: Some(path.to_owned()), source }) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let missing = || io::Error::from(io::ErrorKind::NotFound);
    let ports = [port(&attempt("8080")), outcome(&Outcome { result: "8080".parse() }), job(&Job { id: 8080, last: None }), settings(&Settings { verbose: true, port: Some(8080) })];
    let checks = [
        checked(&Check { passed: true, failure: None }),
        probed(&Probe(true, None)),
        batch(&Batch { done: true, failures: Vec::new() }),
        fetched(&Fetched { body: None, cause: Box::new(missing()) }) == 0,
        lookup(&[(1, 8080)], 1).found && lookup(&[], 1).value.is_none() && !cached(None).missing.eq(&cached(None).entry.is_some()),
        toggles().iter().all(|toggle| toggle.on && toggle.last.is_none_or(|last| last == 8080)) && pending(true).slot.is_some() != pending(true).ready,
    ];
    let parsed = parsed("8080");
    let replied = [reply(Ok(8080)), reply(Err(Closed))].map(|reply| reply.value.is_some() && reply.refusal.is_none());
    black_box((ports, checks, read("port").map(|text| text.len()).unwrap_or_default(), parsed.port, parsed.failure.is_some(), replied));
}
