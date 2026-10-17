#![feature(default_field_values)]

use core::hint::black_box;

// --- [CONSTANTS] -----------------------------------------------------------------------

const BASE: Based = Based { attempts: 0, label: String::new() };

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Retry {
    attempts: u8,
    label: String,
}

#[derive(Debug)]
struct Buffered {
    attempts: u8,
    bytes: Vec<u8>,
}

#[derive(Debug)]
struct Blank {
    attempts: u8,
    label: String,
}

impl Default for Retry {
    fn default() -> Self { Self { attempts: 3, label: String::new() } }
}

impl Default for Buffered {
    fn default() -> Self { Self { attempts: 3, bytes: Vec::with_capacity(8) } }
}

impl Default for Blank {
    fn default() -> Self { Self { attempts: 0, label: String::new() } }
}

#[derive(Debug)]
struct Config {
    depth: u8,
    name: String,
    enabled: bool,
    label: &'static str,
    parent: Option<u8>,
}

impl Config {
    fn new() -> Self { Self { depth: 0, name: String::new(), enabled: false, label: "", parent: None } }
}

impl Default for Config {
    fn default() -> Self { Self::new() }
}

#[derive(Debug)]
struct Limits {
    depth: u8,
    name: String,
    enabled: bool,
    label: &'static str,
    parent: Option<u8>,
}

impl Limits {
    fn new() -> Self { Self { depth: 4, name: String::new(), enabled: false, label: "", parent: None } }
}

impl Default for Limits {
    fn default() -> Self { Self::new() }
}

#[derive(Debug)]
struct Attributed {
    attempts: u8,
}

#[automatically_derived]
impl Default for Attributed {
    fn default() -> Self { Self { attempts: 3 } }
}

#[derive(Debug)]
struct Seeded {
    attempts: u8,
}

impl Seeded {
    fn new(attempts: u8) -> Self { Self { attempts } }
}

impl Default for Seeded {
    fn default() -> Self { Self::new(2) }
}

#[derive(Debug)]
enum Mode {
    Retry { attempts: u8 },
}

impl Default for Mode {
    fn default() -> Self { Self::Retry { attempts: 3 } }
}

#[derive(Debug)]
struct Wrapped<T> {
    value: T,
}

impl Default for Wrapped<u8> {
    fn default() -> Self { Self { value: 3 } }
}

#[derive(Debug)]
struct Fixed<const N: usize> {
    attempts: u8,
}

impl Default for Fixed<2> {
    fn default() -> Self { Self { attempts: 3 } }
}

#[derive(Debug)]
struct Based {
    attempts: u8,
    label: String,
}

impl Default for Based {
    fn default() -> Self { Self { attempts: 3, ..BASE } }
}

#[derive(Debug)]
struct Declared {
    attempts: u8 = 3,
    label: String,
}

impl Declared {
    fn fresh() -> Self { Self { attempts: 4, label: String::new() } }
}

impl Default for Declared {
    fn default() -> Self { Self::fresh() }
}

#[derive(Debug)]
struct Cloned {
    attempts: u8,
}

impl Clone for Cloned {
    fn clone(&self) -> Self { Self { attempts: 3 } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (retry, buffered, blank) = (Retry::default(), Buffered::default(), Blank::default());
    let (config, limits, attributed, seeded) = (Config::default(), Limits::default(), Attributed::default(), Seeded::default());
    let (Mode::Retry { attempts }, wrapped, fixed, based) = (Mode::default(), Wrapped::<u8>::default(), Fixed::<2>::default(), Based::default());
    black_box(&([retry.attempts, buffered.attempts, blank.attempts], [retry.label, blank.label], buffered.bytes));
    black_box(&([config.depth, limits.depth, attributed.attempts, seeded.attempts], [config.name, limits.name]));
    let (declared, fresh, cloned) = (Declared::default(), Declared::fresh(), Cloned { attempts: 1 }.clone());
    black_box(&([attempts, wrapped.value, fixed.attempts, based.attempts, BASE.attempts, declared.attempts, fresh.attempts, cloned.attempts], [based.label, declared.label, fresh.label]));
    black_box(&(config.enabled, config.label, config.parent, limits.enabled, limits.label, limits.parent));
}
