use core::{error::Error, hint::black_box, num::ParseIntError};
use std::io;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Attempt {
    value: Option<u16>,
    error: Option<ParseIntError>,
}

#[derive(Debug)]
struct Fetched {
    body: Option<String>,
    cause: Box<dyn Error + Send + Sync>,
}

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
struct Parsed {
    port: Option<u16>,
    failure: Option<ParseIntError>,
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

fn fetched(fetched: &Fetched) -> usize { fetched.body.as_ref().map_or(0, String::len).strict_add(usize::from(fetched.cause.source().is_none())) }

fn outcome(outcome: &Outcome) -> u16 { outcome.result.as_ref().map_or(0, |port| *port) }

const fn job(job: &Job) -> u16 { if job.last.is_none() { job.id } else { 0 } }

fn parsed(text: &str) -> Parsed {
    match text.parse::<u16>() {
        Ok(port) => Parsed { port: Some(port), failure: None },
        Err(failure) => Parsed { port: None, failure: Some(failure) },
    }
}

fn reply(outcome: Result<u16, Closed>) -> Reply { Reply { value: outcome.ok(), refusal: outcome.err() } }

#[derive(Debug)]
pub struct Exchange {
    pub value: Option<u32>,
    pub error: Option<String>,
}

fn exchange(text: &str) -> Exchange {
    match text.parse::<u32>() {
        Ok(value) => Exchange { value: Some(value), error: None },
        Err(error) => Exchange { value: None, error: Some(error.to_string()) },
    }
}

fn read(path: &str) -> Result<String, ReadError> { fs_err::read_to_string(path).map_err(|source| ReadError { path: Some(path.to_owned()), source }) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let missing = || io::Error::from(io::ErrorKind::NotFound);
    let ports = [port(&attempt("8080")), outcome(&Outcome { result: "8080".parse() }), job(&Job { id: 8080, last: None })];
    let parsed = parsed("8080");
    let replied = [reply(Ok(8080)), reply(Err(Closed))].map(|reply| reply.value.is_some() && reply.refusal.is_none());
    black_box(&(exchange("1").value, exchange("x").error));
    black_box(&(ports, fetched(&Fetched { body: None, cause: Box::new(missing()) }) == 0, read("port").map(|text| text.len()).unwrap_or_default(), parsed.port, parsed.failure.is_some(), replied));
}
