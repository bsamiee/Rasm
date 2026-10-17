use core::{
    f64::consts::PI,
    fmt::{self, Debug, Display},
    hint::black_box,
    ops::Neg,
};

// --- [TYPES] ---------------------------------------------------------------------------

trait Shape {
    fn area(&self) -> f64;

    fn doubled(&self) -> f64;
}

trait Named {
    fn name(&self) -> &str;
}

trait Bounds {
    fn lower(&self) -> u32;

    fn upper(&self) -> u32;
}

trait Hooks {
    fn enter(&self) -> u32 { 0 }

    fn exit(&self) -> u32 { 0 }
}

trait Weighted {
    fn weight(&self) -> u32;
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Route {
    segments: Vec<String>,
}

#[derive(Debug)]
struct Square {
    side: f64,
}

#[derive(Debug)]
struct Circle {
    radius: f64,
}

#[derive(Debug)]
struct Item {
    name: String,
}

#[derive(Debug)]
struct Catalog {
    items: Vec<Item>,
}

#[derive(Debug)]
struct User {
    name: String,
    email: String,
}

#[derive(Debug)]
struct Team {
    name: String,
    size: usize,
}

#[derive(Debug)]
struct Fixed {
    limit: u32,
}

#[derive(Debug)]
struct Spread {
    floor: u32,
    ceiling: u32,
}

#[derive(Debug)]
struct Counter {
    count: u32,
}

#[derive(Debug)]
struct Labeled<T> {
    name: &'static str,
    value: T,
}

// --- [MACROS] --------------------------------------------------------------------------

macro_rules! getter {
    ($name:ident) => {
        const fn $name(&self) -> u32 { self.count }
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Route {
    fn path(&self) -> String { self.segments.join("/") }

    fn url_path(&self) -> String { self.segments.join("/") }
}

impl Shape for Square {
    fn area(&self) -> f64 { self.side * self.side }

    fn doubled(&self) -> f64 { self.area() * 2.0 }
}

impl Shape for Circle {
    fn area(&self) -> f64 { PI * self.radius * self.radius }

    fn doubled(&self) -> f64 { self.area() * 2.0 }
}

impl Display for Square {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("shape") }
}

impl Display for Circle {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("shape") }
}

impl<C> Extend<C> for Square {
    fn extend<I>(&mut self, items: I)
    where I: IntoIterator<Item = C> {
        items.into_iter().for_each(drop);
    }
}

impl<C> Extend<C> for Circle {
    fn extend<I>(&mut self, items: I)
    where I: IntoIterator<Item = C> {
        items.into_iter().for_each(drop);
    }
}

impl Neg for Box<Square> {
    type Output = f64;

    fn neg(self) -> f64 { -1.0 }
}

impl Neg for Box<Circle> {
    type Output = f64;

    fn neg(self) -> f64 { -1.0 }
}

impl Display for Labeled<u8> {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str(self.name) }
}

impl Display for Labeled<u16> {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str(self.name) }
}

impl Catalog {
    fn names(&self) -> Vec<String> { self.items.iter().map(|item| item.name.clone()).collect() }

    fn labels(&self) -> Vec<String> { self.items.iter().map(|entry| entry.name.clone()).collect() }
}

impl Named for User {
    fn name(&self) -> &str { &self.name }
}

impl Named for Team {
    fn name(&self) -> &str { &self.name }
}

impl Bounds for Fixed {
    fn lower(&self) -> u32 { self.limit }

    fn upper(&self) -> u32 { self.limit }
}

impl Bounds for Spread {
    fn lower(&self) -> u32 { self.floor }

    fn upper(&self) -> u32 { self.ceiling }
}

impl Counter {
    getter!(current);

    getter!(latest);
}

mod decimal {
    pub(super) fn parse(text: &str) -> u32 { text.parse().unwrap_or_default() }

    pub(super) fn read(text: &str) -> u32 { parse(text) }
}

mod hexadecimal {
    pub(super) fn parse(text: &str) -> u32 { u32::from_str_radix(text, 16).unwrap_or_default() }

    pub(super) fn read(text: &str) -> u32 { parse(text) }
}

fn total(values: &[u32]) -> u32 { values.iter().sum() }

fn sum(numbers: &[u32]) -> u32 { numbers.iter().sum() }

fn shown<T: Display>(value: &T) -> String { value.to_string() }

fn rendered<T: Display + Debug>(value: &T) -> String { value.to_string() }

async fn first(value: u32) -> u32 { value.strict_add(1) }

async fn second(value: u32) -> u32 { value.strict_add(1) }

impl Hooks for Route {}

impl Hooks for Catalog {}

impl Weighted for Fixed {
    fn weight(&self) -> u32 { 1 }
}

impl Weighted for Spread {
    fn weight(&self) -> u32 { 1 }
}

impl Weighted for Counter {
    fn weight(&self) -> u32 { 2 }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let route = Route { segments: vec![String::from("api"), String::from("v")] };
    let catalog = Catalog { items: vec![Item { name: String::from("pen") }] };
    let (user, team) = (User { name: String::from("ada"), email: String::from("ada@example.com") }, Team { name: String::from("core"), size: 3 });
    let (fixed, spread, counter) = (Fixed { limit: 4 }, Spread { floor: 1, ceiling: 9 }, Counter { count: 2 });
    let (mut square, mut circle) = (Square { side: 1.0 }, Circle { radius: 1.0 });
    square.extend([1_u8]);
    circle.extend([2_u8]);
    let (small, large) = (Labeled { name: "small", value: 1_u8 }, Labeled { name: "large", value: 2_u16 });
    black_box((
        [Square { side: 2.0 }.doubled(), Circle { radius: 1.0 }.doubled()],
        [Square { side: 3.0 }.to_string(), Circle { radius: 2.0 }.to_string()],
        ([user.name(), team.name(), &user.email].concat(), team.size, route.path(), route.url_path(), catalog.names(), catalog.labels()),
        [fixed.lower(), fixed.upper(), spread.lower(), spread.upper(), counter.current(), counter.latest()],
        [decimal::read("10"), hexadecimal::read("a"), decimal::parse("7"), hexadecimal::parse("b"), total(&[1, 2]), sum(&[3])],
        (shown(&1), rendered(&2), first(3).await, second(4).await, route.enter(), route.exit(), catalog.enter()),
        [fixed.weight(), spread.weight(), counter.weight()],
        (-Box::new(square), -Box::new(circle), small.to_string(), large.to_string(), small.value, large.value),
    ));
}
