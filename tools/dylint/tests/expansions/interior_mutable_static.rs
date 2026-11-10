use core::cell::Cell;
use core::sync::atomic::{AtomicU32, Ordering};
use std::sync::{Arc, LazyLock, Mutex, OnceLock};

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

static HITS: AtomicU32 = AtomicU32::new(0);

static ROOT: OnceLock<String> = OnceLock::new();

pub static ORIGIN: OnceLock<String> = OnceLock::new();

static TABLE: LazyLock<Vec<u32>> = LazyLock::new(|| (1..4).collect());

static COUNTER: LazyLock<AtomicU32> = LazyLock::new(|| AtomicU32::new(1));

static SHARED: LazyLock<Arc<Mutex<Vec<u8>>>> = LazyLock::new(|| Arc::new(Mutex::new(Vec::new())));

static GREETING: &str = "hello";

static LIMITS: [u32; 2] = [1, 2];

thread_local! {
    static VISITS: Cell<u32> = const { Cell::new(0) };
    static DEPTH: u32 = const { 1 };
}

counter!(CALLS);

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    static SEEN: AtomicU32 = AtomicU32::new(0);
    SEEN.fetch_add(1, Ordering::Relaxed);
    HITS.fetch_add(1, Ordering::Relaxed);
    CALLS.fetch_add(DEPTH.with(|depth| *depth), Ordering::Relaxed);
    VISITS.set(HITS.load(Ordering::Relaxed));
    ROOT.get_or_init(|| GREETING.to_owned());
    ORIGIN.get_or_init(|| GREETING.to_owned());
    COUNTER.fetch_add(1, Ordering::Relaxed);
    SHARED.lock().map(|mut bytes| bytes.push(1)).unwrap_or_default();
    for limit in TABLE.iter().chain(&LIMITS) {
        HITS.fetch_add(*limit, Ordering::Relaxed);
    }
    HITS.fetch_add(Options::parse_from(["fixture"]).port, Ordering::Relaxed);
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
