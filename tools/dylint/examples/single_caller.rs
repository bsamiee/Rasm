use core::hint::black_box;

use serde::{Serialize, Serializer};

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMIT: usize = 8;

const SHARED: usize = 2;

static BANNER: &str = "ports";

// --- [MODELS] --------------------------------------------------------------------------

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
    fn clamped(&self) -> usize { self.width.min(self.bound()) }

    fn bound(&self) -> usize { self.width.strict_mul(SHARED) }

    fn squared(&self) -> usize { self.width.strict_mul(self.width) }
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

fn started(steps: usize) -> usize { countdown(steps) }

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

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    fn narrowed(width: usize) -> usize { halved(width) }
    let window = Window { width: 3 };
    black_box(&(parsed("a1b2"), started(3), first(" x "), second(" y "), titled(), direct("z"), measuring("w"), window.clamped(), window.squared()));
    black_box(&(built(5).width, relayed("e"), visible("v"), outer("o"), sided(4), Report { ports: 6 }, widened(2), narrowed(8)));
}
