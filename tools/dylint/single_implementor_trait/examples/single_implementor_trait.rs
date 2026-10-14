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

trait Doubled {
    fn doubled(self) -> Self;
}

impl Doubled for u32 {
    fn doubled(self) -> Self { self * 2 }
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
    fn render(&self) -> String { String::from("banner") }
}

#[derive(Debug)]
struct Grid;

impl Shape for Grid {
    type Output = [usize; 2];

    fn shape(&self) -> Self::Output { [1, 2] }
}

struct Service {
    store: Arc<dyn Send + Store>,
    views: Vec<Box<dyn Render>>,
    on_change: Box<dyn Callback>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn tick_twice<C>(clock: &C) -> u64
where C: Clock {
    clock.tick() + clock.ticks().sum::<u64>()
}

const fn marked<M>(value: &M) -> &M
where M: Marker {
    value
}

fn rank<S>(value: &S) -> <S as Shape>::Output
where S: Shape {
    value.shape()
}

fn service() -> Service { Service { store: Arc::new(Memory::default()), views: vec![Box::new(Page), Box::new(Banner)], on_change: Box::new(|key| key + 1) } }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    let built = service();
    let _memory = marked(&Memory::default());
    let read = built.store.get(1).map_or(0, |value| built.on_change.call(value).doubled());
    let rendered = built.views.iter().map(|view| view.render().len()).sum::<usize>() + rank(&Grid).len();
    ExitCode::from(u8::from(tick_twice(&Fixed(2)) + u64::from(read) == 0 || rendered == 0))
}
