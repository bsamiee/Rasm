use core::{convert::Infallible, fmt, hint::black_box, str::FromStr};
use std::{collections::BTreeMap, process::ExitCode, sync::Arc};

// --- [TYPES] ---------------------------------------------------------------------------

pub(crate) trait Store: Send + Sync {
    fn get(&self, key: u32) -> Option<u32>;
}

trait Clock {
    fn tick(&self) -> u64;

    fn ticks(&self) -> impl Iterator<Item = u64>;
}

trait Render {
    fn render(&self) -> String;
}

trait Callback {
    fn call(&self, key: u32) -> u32;
}

impl<F> Callback for F
where F: Fn(u32) -> u32
{
    fn call(&self, key: u32) -> u32 { self(key) }
}

trait Marker {}

trait Shape {
    type Output;

    fn shape(&self) -> Self::Output;
}

trait Lookup {
    fn lookup(&self, key: u32) -> Option<u32>;
}

trait Sink {
    fn put(&self, value: u32) -> u32;
}

trait Doubled {
    fn doubled(self) -> Self;
}

impl Doubled for u32 {
    fn doubled(self) -> Self { self.strict_mul(2) }
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Default)]
struct Memory(BTreeMap<u32, u32>);

impl Store for Memory {
    fn get(&self, key: u32) -> Option<u32> { self.0.get(&key).copied() }
}

impl Marker for Memory {}

#[derive(Debug)]
struct Fixed(u64);

impl Clock for Fixed {
    fn tick(&self) -> u64 { self.0 }

    fn ticks(&self) -> impl Iterator<Item = u64> { [self.0, self.0].into_iter() }
}

#[derive(Debug)]
struct Page;

impl Render for Page {
    fn render(&self) -> String { String::from("page") }
}

#[derive(Debug)]
struct Banner;

impl Render for Banner {
    fn render(&self) -> String { "banner".to_owned() }
}

#[derive(Debug)]
struct Grid;

impl Shape for Grid {
    type Output = [usize; 2];

    fn shape(&self) -> Self::Output { [1, 2] }
}

#[derive(Debug)]
struct Borrowed<'map>(&'map BTreeMap<u32, u32>);

impl Lookup for Borrowed<'_> {
    fn lookup(&self, key: u32) -> Option<u32> { self.0.get(&key).copied() }
}

#[derive(Debug, Default)]
struct Names(Vec<String>);

impl Names {
    const fn len(&self) -> usize { self.0.len() }

    const fn is_empty(&self) -> bool { self.0.is_empty() }
}

impl AsRef<[String]> for Names {
    fn as_ref(&self) -> &[String] { &self.0 }
}

impl From<Vec<String>> for Names {
    fn from(value: Vec<String>) -> Self { Self(value) }
}

#[derive(Debug)]
struct Label(String);

impl fmt::Display for Label {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { fmt::Display::fmt(&self.0, f) }
}

impl FromStr for Label {
    type Err = Infallible;

    fn from_str(s: &str) -> Result<Self, Self::Err> { s.parse().map(Self) }
}

#[derive(Debug)]
struct Count(u64);

impl Count {
    const fn get(&self) -> u64 { self.0 }

    const fn checked_add(&self, other: u64) -> Option<u64> { self.0.checked_add(other) }
}

#[derive(Debug)]
struct Scores(Vec<u32>);

impl Scores {
    fn get(&self, index: usize) -> Option<&u32> { self.0.get(index.checked_add(1)?) }
}

struct Relay<F>(F);

impl<F> Sink for Relay<F>
where F: Fn(u32) -> u32
{
    fn put(&self, value: u32) -> u32 { (self.0)(value) }
}

#[derive(Debug, Default)]
struct Queue(Vec<u32>);

impl Queue {
    fn push(&mut self, value: u32) { self.0.push(value); }

    const fn len(&self) -> usize { self.0.len() }
}

#[derive(Debug, Default)]
pub struct Public(Vec<u32>);

impl Public {
    #[must_use]
    pub const fn len(&self) -> usize { self.0.len() }
}

#[derive(Debug)]
struct Lines(Vec<String>);

impl Lines {
    const fn count(&self) -> usize { self.0.len() }
}

struct Tagged(String);

impl fmt::Debug for Tagged {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { fmt::Display::fmt(&self.0, f) }
}

#[derive(Debug)]
struct Hex(Vec<u8>);

impl fmt::Display for Hex {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { self.0.iter().try_for_each(|byte| write!(f, "{byte:02x}")) }
}

struct Service {
    store: Arc<dyn Send + Store>,
    views: Vec<Box<dyn Render>>,
    on_change: Box<dyn Callback>,
}

// --- [MACROS] --------------------------------------------------------------------------

macro_rules! sink {
    ($relay:expr) => {{
        let sink: Box<dyn Sink> = Box::new($relay);
        sink
    }};
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn tick_twice<C>(clock: &C) -> u64
where C: Clock {
    clock.tick().strict_add(clock.ticks().sum())
}

const fn marked<M>(value: &M) -> &M
where M: Marker {
    value
}

fn rank<S>(value: &S) -> <S as Shape>::Output
where S: Shape {
    value.shape()
}

fn first(lookup: &dyn Lookup) -> Option<u32> { lookup.lookup(0) }

fn service() -> Service { Service { store: Arc::new(Memory::default()), views: vec![Box::new(Page), Box::new(Banner)], on_change: Box::new(|key: u32| key.strict_add(1)) } }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    let built = service();
    let _memory = marked(&Memory::default());
    let first = first(&Borrowed(&BTreeMap::new())).unwrap_or(0);
    let read = built.store.get(1).map_or(0, |value| built.on_change.call(value).doubled());
    let names = Names::from(vec![Label::from_str("name").map_or_else(|never| match never {}, |label| label.to_string())]);
    let count = Count(u64::from(sink!(Relay(|value: u32| value.strict_add(1))).put(1)));
    let shown = format!("{:?}{}{}", Tagged(String::from("tag")), Hex(vec![1]), Lines(names.as_ref().to_vec()).count());
    black_box((
        tick_twice(&Fixed(2)),
        [read, first],
        built.views.iter().map(|view| view.render()).collect::<Vec<_>>(),
        rank(&Grid),
        Scores(vec![1, 2]).get(0).copied(),
        count.checked_add(count.get()),
        shown,
        names.len() == 1,
        {
            let mut queue = Queue::default();
            queue.push(1);
            [queue.len(), Public::default().len()]
        },
        (format!("{:?}", Tagged(String::new())), Hex(vec![2]).to_string()),
    ));
    ExitCode::from(u8::from(names.is_empty()))
}
