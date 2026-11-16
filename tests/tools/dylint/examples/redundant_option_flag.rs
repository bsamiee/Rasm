use core::hint::black_box;
use std::io;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Check {
    passed: bool,
    failure: Option<io::Error>,
}

#[derive(Debug)]
struct Probe(bool, Option<io::Error>);

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
struct Pending {
    ready: bool,
    slot: Option<u16>,
}

#[derive(Debug)]
pub struct Outcome {
    pub ok: bool,
    pub value: Option<String>,
}

#[derive(Debug)]
struct Parsed {
    parsed: bool,
    port: Option<u16>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn outcome(value: Option<String>) -> Outcome { Outcome { ok: value.is_some(), value } }

fn parsed(text: &str) -> Parsed {
    let result = text.parse::<u16>();
    Parsed { parsed: result.is_ok(), port: result.ok() }
}

const fn checked(check: &Check) -> bool { check.passed && check.failure.is_none() }

const fn probed(probe: &Probe) -> bool { probe.0 && probe.1.is_none() }

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

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    black_box([
        checked(&Check { passed: true, failure: None }),
        probed(&Probe(true, None)),
        batch(&Batch { done: true, failures: Vec::new() }),
        settings(&Settings { verbose: true, port: Some(8080) }) == 8080,
        lookup(&[(1, 8080)], 1).found && lookup(&[], 1).value.is_none() && !cached(None).missing.eq(&cached(None).entry.is_some()),
        toggles().iter().all(|toggle| toggle.on && toggle.last.is_none_or(|last| last == 8080)) && pending(true).slot.is_some() != pending(true).ready,
        outcome(Some(String::from("1"))).ok && outcome(None).value.is_none() && parsed("1").parsed && parsed("x").port.is_none(),
    ]);
}
