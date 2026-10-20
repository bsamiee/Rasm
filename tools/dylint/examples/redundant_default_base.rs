#![feature(default_field_values)]

use core::{hint::black_box, time::Duration};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Default)]
struct Limits {
    retries: u8 = 3,
    timeout: Duration = Duration::from_secs(30),
    name: String,
}

#[derive(Debug, Default)]
struct Window {
    width: u16 = 80,
    height: u16,
}

#[derive(Debug)]
struct Retry {
    attempts: u8 = 2,
    backoff: u16 = 10,
}

impl Default for Retry {
    fn default() -> Self { Self { .. } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn limits(name: String) -> Limits { Limits { name, ..Default::default() } }

fn window() -> Window { Window { width: 100, ..Default::default() } }

fn retry() -> Retry { Retry { attempts: 4, ..Default::default() } }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (limits, window, retry) = (limits(String::new()), window(), retry());
    black_box(&([window.width, window.height, retry.backoff], limits.timeout, limits.retries, limits.name, retry.attempts, named(String::new()).retries));
}

fn named(name: String) -> Limits { Limits { name, ..Limits::default() } }
