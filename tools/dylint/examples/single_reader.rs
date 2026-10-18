use core::hint::black_box;
use std::sync::LazyLock;

use serde::{Serialize, Serializer};

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMIT: usize = 8;

const SHARED: usize = 2;

static BANNER: &str = "ports";

const DEPTH: usize = 2;

const WIDTH: usize = 4;

const SIZE: usize = 2;

static PORTS: LazyLock<Vec<u16>> = LazyLock::new(|| (1..=4).map(|step: u16| step.strict_mul(80)).collect());

#[used]
static KEPT: u8 = 4;

// --- [MODELS] --------------------------------------------------------------------------

struct Grid {
    cells: [u8; WIDTH],
}

mod geometry {
    pub(crate) struct Square {
        side: usize,
    }

    impl Square {
        pub(crate) const fn new(side: usize) -> Self { Self { side } }

        pub(crate) const fn side(&self) -> usize { self.side }
    }
}

struct Window {
    width: usize,
}

impl Window {
    const MULTIPLE: usize = 3;
    const OFFSET: usize = 1;

    fn clamped(&self) -> usize { self.width.min(self.bound()) }

    fn bound(&self) -> usize { self.width.strict_mul(SHARED) }

    fn squared(&self) -> usize { self.width.strict_mul(self.width).strict_add(Self::OFFSET) }

    fn tripled(&self) -> usize { self.width.strict_mul(Self::MULTIPLE).strict_add(Self::OFFSET) }
}

trait Scale {
    const FACTOR: usize;

    fn scale(&self) -> usize;
}

impl Scale for Window {
    const FACTOR: usize = 2;

    fn scale(&self) -> usize { self.width.strict_add(Self::FACTOR) }
}

impl Scale for Grid {
    const FACTOR: usize = 5;

    fn scale(&self) -> usize { self.cells.len().strict_mul(Self::FACTOR) }
}

struct Buffer<T> {
    items: Vec<T>,
}

impl<T> Buffer<T> {
    const ITEM: usize = size_of::<T>();

    fn bytes(&self) -> usize { self.items.len().strict_mul(Self::ITEM) }
}

impl From<usize> for Window {
    fn from(width: usize) -> Self { Self { width } }
}

#[derive(Serialize)]
struct Report {
    #[serde(serialize_with = "Report::rendered")]
    ports: usize,
}

impl Report {
    fn rendered<S: Serializer>(ports: &usize, serializer: S) -> Result<S::Ok, S::Error> { serializer.collect_str(&format_args!("{ports} ports")) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn digits(text: &str) -> usize { text.chars().filter(char::is_ascii_digit).count() }

fn parsed(text: &str) -> usize { digits(text).min(LIMIT) }

fn countdown(steps: usize) -> usize { if steps == 0 { 0 } else { countdown(steps.strict_sub(1)) } }

fn started(steps: usize) -> usize { countdown(steps).strict_add(1) }

fn trimmed(text: &str) -> &str { text.trim() }

fn first(text: &str) -> usize { trimmed(text).len() }

fn second(text: &str) -> usize { trimmed(text).len().strict_add(SHARED) }

fn titled() -> String { format!("{BANNER}:") }

fn direct(text: &str) -> usize { text.len() }

fn expanded(text: &str) -> usize { text.chars().count() }

macro_rules! measured {
    ($text:expr) => {
        expanded($text)
    };
}

fn measuring(text: &str) -> usize { measured!(text) }

fn scaled(width: usize) -> usize { width.strict_add(1) }

macro_rules! reader {
    ($name:ident, $read:expr) => {
        fn $name(width: usize) -> usize { $read(width) }
    };
}

reader!(widened, scaled);

pub fn exported(text: &str) -> usize { text.len().strict_add(SHARED) }

fn relayed(text: &str) -> usize { exported(text).strict_mul(2) }

fn built(width: usize) -> Window { Window::from(width) }

fn hidden(text: &str) -> usize { text.bytes().filter(u8::is_ascii_uppercase).count() }

fn visible(text: &str) -> usize { hidden(text) }

#[cfg(any())]
fn stripped() -> usize { hidden("") }

fn outer(text: &str) -> usize {
    fn inner(text: &str) -> usize { text.split_whitespace().count() }
    inner(text)
}

fn sided(side: usize) -> usize { geometry::Square::new(side).side() }

fn halved(width: usize) -> usize { width.strict_div(2) }

fn padded() -> usize { [1_u8; DEPTH + 1].len() }

fn gridded(grid: &Grid) -> usize { grid.cells.len().strict_add(WIDTH) }

const fn seed() -> u8 { 3 }

fn opened() -> u8 { 1 }

fn closed() -> u8 { 0 }

const SEED: u8 = seed();

const STEPS: [fn() -> u8; 2] = [opened, closed];

fn sized() -> [u8; SIZE] { [0; SIZE] }

fn port(index: usize) -> Option<u16> { PORTS.get(index).copied() }

fn kept() -> u8 { KEPT }

fn vowels(text: &str) -> usize { text.chars().filter(|letter| "aeiou".contains(*letter)).count() }

fn counted(text: &str) -> usize { vowels(text) }

fn graded(score: u8) -> u8 { if score > 4 { if score > 7 { 2 } else { 1 } } else { 0 } }

fn ranked(score: u8) -> u8 { if score > 2 { if score > 5 { if score > 8 { 3 } else { 2 } } else { 1 } } else { 0 } }

fn grade(score: u8) -> u8 { graded(score).strict_add(ranked(score)) }

fn floored(width: usize) -> usize { if width.is_power_of_two() { width } else { width.next_power_of_two() } }

fn wrapped(width: usize) -> usize {
    fn unwrapped(width: usize) -> usize { floored(width).strict_mul(2) }
    unwrapped(width)
}

fn lifted(width: usize) -> usize { if width > 2 { width.strict_sub(2) } else { 0 } }

fn layered(width: usize) -> usize {
    fn middle(width: usize) -> usize {
        fn innermost(width: usize) -> usize { lifted(width).strict_add(1) }
        innermost(width)
    }
    middle(width)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    fn narrowed(width: usize) -> usize { halved(width).strict_add(1) }
    let window = Window { width: 3 };
    let grid = Grid { cells: [0; 4] };
    black_box(&(parsed("a1b2"), started(3), first(" x "), second(" y "), titled(), direct("z"), measuring("w"), window.clamped(), window.squared()));
    black_box(&(built(5).width, relayed("e"), visible("v"), outer("o"), sided(4), Report { ports: 6 }, widened(2), narrowed(8)));
    black_box(&(padded(), gridded(&grid), SEED, STEPS, window.tripled(), window.scale(), grid.scale()));
    black_box(&(Buffer { items: vec![1_u16] }.bytes(), sized(), port(1), kept(), counted("a"), grade(6), wrapped(3), layered(5)));
}
