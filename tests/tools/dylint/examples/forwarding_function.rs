use core::future::ready;
use core::hint::black_box;

// --- [MODELS] --------------------------------------------------------------------------

struct Gauge {
    level: u32,
}

impl Gauge {
    fn raised(&self, step: u32) -> u32 { self.level.strict_add(step) }

    fn lifted(&self, step: u32) -> u32 { self.raised(step) }

    fn level(&self) -> u32 { self.level }
}

impl From<u32> for Gauge {
    fn from(level: u32) -> Self { Self::with(level) }
}

impl Gauge {
    const fn with(level: u32) -> Self { Self { level } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn scaled(value: u32, factor: u32) -> u32 { value.strict_mul(factor) }

fn forwarded(value: u32, factor: u32) -> u32 { scaled(value, factor) }

fn swapped(value: u32, factor: u32) -> u32 { scaled(factor, value) }

fn fixed(value: u32) -> u32 { scaled(value, 2) }

fn squared(value: u32) -> u32 { scaled(value, value) }

fn borrowed(text: String) -> usize { str::len(&text) }

fn trimmed(text: &str) -> &str { text.trim() }

fn discarded(value: u32) { scaled(value, value); }

fn widened(value: u32) -> u64 { u64::from(scaled(value, 3)) }

fn noted(value: u32) { black_box(value); }

fn relayed(value: u32) { noted(value) }

fn countdown(steps: u32) -> u32 { if steps == 0 { 0 } else { countdown(steps.strict_sub(1)) } }

async fn fetched(value: u32) -> u32 { ready(value).await }

async fn awaited(value: u32) -> u32 { fetched(value).await }

macro_rules! forwarder {
    ($name:ident) => {
        fn $name(value: u32) -> u32 { fixed(value) }
    };
}

forwarder!(expanded);

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let gauge = Gauge::from(1);
    black_box((forwarded(1, 2), swapped(1, 2), expanded(3), squared(4), borrowed(String::new()), trimmed(" "), widened(4), countdown(2), gauge.lifted(1), gauge.level()));
    discarded(5);
    relayed(6);
    black_box(awaited(7).await);
}
