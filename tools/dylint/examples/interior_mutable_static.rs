use core::{
    cell::Cell,
    sync::atomic::{AtomicU32, Ordering},
};
use std::sync::{Arc, LazyLock, Mutex, OnceLock};

// --- [MACROS] --------------------------------------------------------------------------

macro_rules! counter {
    ($name:ident) => {
        static $name: AtomicU32 = AtomicU32::new(0);
    };
}

// --- [SERVICES] ------------------------------------------------------------------------

static HITS: AtomicU32 = AtomicU32::new(0);

static ROOT: OnceLock<String> = OnceLock::new();

static PREFIX: OnceLock<String> = OnceLock::new();

static LABEL: OnceLock<String> = OnceLock::new();

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

fn main() {
    HITS.fetch_add(1, Ordering::Relaxed);
    CALLS.fetch_add(DEPTH.with(|depth| *depth), Ordering::Relaxed);
    VISITS.set(HITS.load(Ordering::Relaxed));
    ROOT.get_or_init(|| GREETING.to_owned());
    let name = GREETING.repeat(2);
    PREFIX.get_or_init(|| format!("{name}/"));
    LABEL.get_or_init(|| GREETING.to_owned());
    LABEL.get_or_init(String::new);
    ORIGIN.get_or_init(|| GREETING.to_owned());
    COUNTER.fetch_add(1, Ordering::Relaxed);
    SHARED.lock().map(|mut bytes| bytes.push(1)).unwrap_or_default();
    for limit in TABLE.iter().chain(&LIMITS) {
        HITS.fetch_add(*limit, Ordering::Relaxed);
    }
}
