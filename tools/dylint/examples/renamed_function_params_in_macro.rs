use core::hint::black_box;

// --- [TYPES] ---------------------------------------------------------------------------

trait Scale {
    fn scale(&self, factor: u32) -> u32;

    fn shift(&self, offset: u32) -> u32;

    fn clamp(&self, limit: u32) -> u32;
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Gauge(u32);

struct Meter(u32);

macro_rules! scaled {
    ($($method:ident($argument:ident) => $body:expr;)*) => {$(
        fn $method(&self, $argument: u32) -> u32 { $body(self.0, $argument) }
    )*};
}

macro_rules! offset {
    () => {
        fn shift(&self, delta: u32) -> u32 { self.0 + delta }
    };
}

impl Scale for Gauge {
    scaled! {
        scale(by) => u32::wrapping_mul;
        clamp(limit) => u32::min;
    }

    offset!();
}

impl Scale for Meter {
    scaled! {
        scale(factor) => u32::wrapping_mul;
        shift(_offset) => u32::saturating_sub;
        clamp(limit) => u32::min;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (gauge, meter) = (Gauge(1), Meter(2));
    black_box([gauge.scale(2), gauge.shift(3), gauge.clamp(4), meter.scale(5), meter.shift(6), meter.clamp(7)]);
}
