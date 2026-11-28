use core::hint::black_box;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Retry {
    attempts: u8,
    label: String,
}

#[derive(Debug)]
struct Blank {
    attempts: u8,
    label: String,
}

impl Default for Retry {
    fn default() -> Self { Self { attempts: 3, label: String::new() } }
}

impl Blank {
    fn new() -> Self { Self { attempts: 0, label: String::new() } }
}

impl Default for Blank {
    fn default() -> Self { Self::new() } //~ manual_default_field_values derive `Default` and delete the impl
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (retry, blank) = (Retry::default(), Blank::default());
    black_box(([retry.attempts, blank.attempts], [retry.label, blank.label]));
}
