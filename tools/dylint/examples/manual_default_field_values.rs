#![feature(const_default, const_trait_impl, default_field_values)]

use core::{
    hint::black_box,
    sync::atomic::{AtomicU8, Ordering},
};

// --- [TYPES] ---------------------------------------------------------------------------

trait New {
    fn new() -> Self;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const BASE: Based = Based { attempts: 0, label: String::new() };
static SEQUENCE: AtomicU8 = AtomicU8::new(0);

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
    fn new() -> Self { Self { depth: 0, name: String::default(), enabled: false, label: "", parent: None } }
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
    fn new() -> Self { Self { depth: 4, name: String::new(), enabled: false, label: "network", parent: None } }
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

#[derive(Debug)]
struct Number(u8);

impl Number {
    const fn new() -> Self { Self(7) }
}

impl Default for Number {
    fn default() -> Self {
        let value = Self::new();
        if value.0 > 0 { Self(0) } else { value }
    }
}

#[derive(Debug)]
struct Custom {
    number: Number,
}

impl Custom {
    fn fresh() -> Self { Self { number: Number::new() } }
}

impl Default for Custom {
    fn default() -> Self { Self::fresh() }
}

#[derive(Debug)]
struct Overridden {
    attempts: u8 = 3,
    enabled: bool,
}

#[expect(default_overrides_default_fields, reason = "Explicit initializer differs from declared field default")]
impl Default for Overridden {
    fn default() -> Self { Self { attempts: 0, enabled: true } }
}

#[derive(Debug)]
struct Nested {
    values: (Number, u8),
}

impl Default for Nested {
    fn default() -> Self { Self { values: (Number::new(), 0) } }
}

#[derive(Debug)]
struct Factory;

impl Factory {
    const LIMIT: u8 = 5;

    fn config() -> Supplied { Supplied { attempts: Self::LIMIT } }
}

#[derive(Debug)]
struct Supplied {
    attempts: u8,
}

impl Default for Supplied {
    fn default() -> Self { Factory::config() }
}

#[derive(Debug)]
struct Measured {
    size: usize,
}

impl Measured {
    fn new<T, const EXTRA: usize>() -> Self { Self { size: size_of::<T>().saturating_add(EXTRA) } }
}

impl Default for Measured {
    fn default() -> Self { Self::new::<u8, 1>() }
}

#[derive(Debug)]
struct Scoped {
    size: usize,
}

impl Scoped {
    fn new<'scope>() -> Self { Self { size: size_of::<&'scope u8>() } }
}

impl Default for Scoped {
    fn default() -> Self { Self::new() }
}

#[derive(Debug)]
struct Erased {
    attempts: u8,
    value: Box<dyn core::fmt::Debug>,
}

impl Default for Erased {
    fn default() -> Self { Self { attempts: 3, value: Box::<u8>::default() } }
}

#[derive(Debug)]
struct Sequence(u8);

impl Default for Sequence {
    fn default() -> Self { Self(SEQUENCE.fetch_add(1, Ordering::Relaxed)) }
}

#[derive(Debug)]
struct Reordered {
    first: Sequence,
    second: Sequence,
}

impl Reordered {
    fn new() -> Self { Self { second: Sequence::default(), first: Sequence::default() } }
}

impl Default for Reordered {
    fn default() -> Self { Self::new() }
}

#[derive(Debug)]
struct Received {
    attempts: u8,
}

impl Received {
    fn fresh(&self) -> Self { Self { attempts: 3 } }
}

impl Default for Received {
    fn default() -> Self { Self { attempts: 1 }.fresh() }
}

impl Drop for Received {
    fn drop(&mut self) { SEQUENCE.fetch_add(1, Ordering::Relaxed); }
}

#[derive(Debug)]
struct Indirect {
    attempts: u8,
}

impl Indirect {
    fn new() -> Self { Self { attempts: 3 } }
}

impl Default for Indirect {
    fn default() -> Self {
        ({
            SEQUENCE.fetch_add(1, Ordering::Relaxed);
            Self::new
        })()
    }
}

impl New for Vec<u8, std::alloc::System> {
    fn new() -> Self {
        let mut bytes = Self::new_in(std::alloc::System);
        bytes.push(1);
        bytes
    }
}

#[derive(Debug)]
struct Allocated {
    bytes: Vec<u8, std::alloc::System>,
    attempts: u8,
}

impl Default for Allocated {
    fn default() -> Self { Self { bytes: Vec::<u8, std::alloc::System>::new(), attempts: 3 } }
}

#[derive(Debug)]
struct Bounded<'a> {
    label: &'a str,
    depth: u8,
}

impl<'a: 'static> Bounded<'a> {
    const DEPTH: u8 = 3;

    fn new() -> Self { Self { label: "", depth: Self::DEPTH } }
}

impl<'a> Default for Bounded<'a> {
    fn default() -> Self { Bounded::<'static>::new() }
}

#[derive(Debug)]
struct Specialized<'a> {
    label: &'a str,
}

impl<'a> Specialized<'a> {
    fn new() -> Self { Self { label: "" } }
}

impl Default for Specialized<'static> {
    fn default() -> Self { Self::new() }
}

#[derive(Debug)]
struct Constant {
    attempts: u8,
}

const impl Default for Constant {
    fn default() -> Self { Self { attempts: 3 } }
}

#[derive(Debug)]
struct Tuple(u8);

impl Default for Tuple {
    fn default() -> Self { Self { 0: 3 } }
}

mod model {
    #[derive(Debug)]
    pub struct Config {
        pub attempts: u8,
    }
}

mod producer {

    // --- [CONSTANTS]

    const PRIVATE_LIMIT: u8 = 6;

    // --- [MODELS]

    impl Default for super::model::Config {
        fn default() -> Self { Self { attempts: PRIVATE_LIMIT } }
    }
}

mod converted {
    #![no_implicit_prelude]

    use ::std::string::String;

    // --- [TYPES]

    trait From {
        fn from(value: &str) -> Self;
    }

    // --- [MODELS]

    impl From for String {
        fn from(_: &str) -> Self {
            let mut text = String::new();
            text.push('x');
            text
        }
    }

    #[derive(::core::fmt::Debug)]
    pub struct Config {
        pub text: String,
        pub attempts: u8,
    }

    impl ::core::default::Default for Config {
        fn default() -> Self { Self { text: String::from(""), attempts: 3 } }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (retry, buffered, blank) = (Retry::default(), Buffered::default(), Blank::default());
    let (config, limits, attributed, seeded) = (Config::default(), Limits::default(), Attributed::default(), Seeded::default());
    let (Mode::Retry { attempts }, wrapped, fixed, based) = (Mode::default(), Wrapped::<u8>::default(), Fixed::<2>::default(), Based::default());
    black_box(&([retry.attempts, buffered.attempts, blank.attempts], [retry.label, blank.label], buffered.bytes));
    black_box(&([config.depth, limits.depth, attributed.attempts, seeded.attempts], [config.name, limits.name]));
    let (declared, fresh, cloned, overridden) = (Declared::default(), Declared::fresh(), Cloned { attempts: 1 }.clone(), Overridden::default());
    black_box(&([attempts, wrapped.value, fixed.attempts, based.attempts, BASE.attempts, declared.attempts, fresh.attempts, cloned.attempts], [based.label, declared.label, fresh.label]));
    black_box(&(config.enabled, config.label, config.parent, limits.enabled, limits.label, limits.parent, overridden.attempts, overridden.enabled));
    black_box(Custom::default().number.0);
    let (nested, supplied, measured, scoped, erased, reordered) = (Nested::default(), Supplied::default(), Measured::default(), Scoped::default(), Erased::default(), Reordered::default());
    black_box((
        nested.values.0.0,
        nested.values.1,
        supplied.attempts,
        measured.size,
        scoped.size,
        erased.attempts,
        erased.value,
        reordered.first.0,
        reordered.second.0,
        model::Config::default().attempts,
    ));
    black_box((Received::default().attempts, Indirect::default().attempts));
    let (allocated, converted) = (Allocated::default(), converted::Config::default());
    black_box((allocated.bytes, allocated.attempts, converted.text, converted.attempts));
    let (bounded, specialized) = (Bounded::default(), Specialized::default());
    black_box((bounded.label, bounded.depth, specialized.label));
    black_box(const { Constant::default().attempts });
    black_box(Tuple::default().0);
}
