#![cfg_attr(dylint_lib = "rasm_lints", allow(single_reader, reason = "each static is read once by design, single_reader has its own fixture"))]

use core::cell::Cell;
use core::sync::atomic::{AtomicU32, Ordering};
use std::sync::{Arc, LazyLock, Mutex};

use clap::Parser;

// --- [MACROS] --------------------------------------------------------------------------

macro_rules! counter {
    ($name:ident) => {
        static $name: AtomicU32 = AtomicU32::new(0);
    };
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Parser)]
struct Options {
    #[arg(long, default_value_t = 8080)]
    port: u32,
}

// --- [SERVICES] ------------------------------------------------------------------------

//~v interior_mutable_static
static HITS: AtomicU32 = AtomicU32::new(0);

static TABLE: LazyLock<Vec<u32>> = LazyLock::new(|| (1..4).collect());

//~v interior_mutable_static
static SHARED: LazyLock<Arc<Mutex<Vec<u8>>>> = LazyLock::new(|| Arc::new(Mutex::new(Vec::new())));

static GREETING: &str = "hello";

thread_local! {
    //~v interior_mutable_static
    static VISITS: Cell<u32> = const { Cell::new(0) };
    static DEPTH: u32 = const { 1 };
}

//~v interior_mutable_static
counter!(CALLS);

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    //~v interior_mutable_static
    static SEEN: AtomicU32 = AtomicU32::new(0);
    SEEN.fetch_add(1, Ordering::Relaxed);
    HITS.fetch_add(1, Ordering::Relaxed);
    CALLS.fetch_add(DEPTH.with(|depth| *depth), Ordering::Relaxed);
    VISITS.set(HITS.load(Ordering::Relaxed));
    match SHARED.lock() {
        Ok(mut bytes) => bytes.push(1),
        Err(poisoned) => poisoned.into_inner().push(1),
    }
    for limit in TABLE.iter() {
        HITS.fetch_add(*limit, Ordering::Relaxed);
    }
    HITS.fetch_add(Options::parse_from(["fixture"]).port, Ordering::Relaxed);
    core::hint::black_box(GREETING);
}

#[cfg(test)]
mod tests {
    use core::sync::atomic::{AtomicU32, Ordering};

    static TEST_VALUE: AtomicU32 = AtomicU32::new(0);

    #[test]
    fn counted() {
        core::hint::black_box(super::main as fn());
        TEST_VALUE.fetch_add(1, Ordering::Relaxed);
    }
}

#[test]
fn counted_directly() {
    static TEST_VALUE: AtomicU32 = AtomicU32::new(0);
    TEST_VALUE.fetch_add(1, Ordering::Relaxed);
}
