use core::hint::black_box;
use std::{collections::BTreeMap, sync::Arc};

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

struct Relay<F>(F);

impl<F> Sink for Relay<F>
where F: Fn(u32) -> u32
{
    fn put(&self, value: u32) -> u32 { (self.0)(value) }
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

fn main() {
    let built = service();
    let _memory = marked(&Memory::default());
    let first = first(&Borrowed(&BTreeMap::new())).unwrap_or(0);
    let read = built.store.get(1).map_or(0, |value| built.on_change.call(value).doubled());
    black_box(&(tick_twice(&Fixed(2)), [read, first, sink!(Relay(|value: u32| value.strict_add(1))).put(1)], built.views.iter().map(|view| view.render()).collect::<Vec<_>>(), rank(&Grid), ran()));
}

trait Handler {
    fn handle(&self, key: u32) -> u32;
}

impl<F> Handler for F
where F: Fn(u32) -> u32
{
    fn handle(&self, key: u32) -> u32 { self(key) }
}

fn run<H>(handler: &H) -> u32
where H: Handler {
    handler.handle(1)
}

fn ran() -> u32 { run(&|key: u32| key.strict_add(1)) }
