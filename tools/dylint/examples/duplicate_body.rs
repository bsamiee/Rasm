use core::f64::consts::PI;
use core::fmt::{self, Debug, Display};
use core::hint::black_box;
use core::ops::Neg;

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

macro_rules! stepped {
    () => {
        fn forward(value: u32) -> u32 { value.strict_add(2) }

        fn onward(value: u32) -> u32 { value.strict_add(2) }
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

stepped!();

fn total(values: &[u32]) -> u32 { values.iter().sum() }

fn sum(numbers: &[u32]) -> u32 { numbers.iter().sum() }

fn shown<T: Display>(value: &T) -> String { value.to_string() }

fn rendered<T: Display + Debug>(value: &T) -> String { value.to_string() }

async fn first(value: u32) -> u32 { value.strict_add(1) }

async fn second(value: u32) -> u32 { value.strict_add(1) }

const fn shared() -> u32 { 1 }

const fn support() -> u32 { 1 }

async fn ready() -> u32 { 1 }

async fn settled() -> u32 { 1 }

fn shown_or_empty(value: Option<&str>) -> &str { value.unwrap_or("") }

fn shown_or_missing(value: Option<&str>) -> &str { value.unwrap_or("n/a") }

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
    black_box(&(
        [Square { side: 2.0 }.doubled(), Circle { radius: 1.0 }.doubled()],
        [Square { side: 3.0 }.to_string(), Circle { radius: 2.0 }.to_string()],
        ([user.name(), team.name(), &user.email].concat(), team.size, route.path(), route.url_path(), catalog.names(), catalog.labels()),
        [fixed.lower(), fixed.upper(), spread.lower(), spread.upper(), counter.current(), counter.latest()],
        [decimal::read("10"), hexadecimal::read("a"), decimal::parse("7"), hexadecimal::parse("b"), total(&[1, 2]), sum(&[3])],
        (shown(&1), rendered(&2), first(3).await, second(4).await, route.enter(), route.exit(), catalog.enter()),
        [fixed.weight(), spread.weight(), counter.weight(), shared(), support(), ready().await, settled().await],
        [shown_or_empty(None), shown_or_missing(None)],
        [forward(1), onward(2)],
        (-Box::new(square), -Box::new(circle), small.to_string(), large.to_string(), small.value, large.value),
    ));
    black_box((parameter_patterns::first((1, 2)), parameter_patterns::last((3, 4)), parameter_patterns::firsts(&[(1, 2)]), parameter_patterns::lasts(&[(3, 4)])));
    black_box((typed_closures::byte_input(), typed_closures::word_input(), typed_closures::byte_output(), typed_closures::word_output()));
    black_box((trait_methods::first::value(&1), trait_methods::last::value(&1), parameter_drops::values()));
    let state = core::cell::Cell::new(0);
    black_box((stateful_binary::ordered(&state, 1, 2), stateful_binary::reversed(&state, 1, 2), stateful_binary::repeated(&state, 1, 2)));
    black_box((stateful_binary::ordered_calls(&state, 1, 2), stateful_binary::reversed_calls(&state, 1, 2), stateful_binary::repeated_calls(&state, 1, 2)));
    black_box((stateful_binary::length_forward(core::convert::identity), stateful_binary::length_reversed(core::convert::identity)));
    black_box((
        contracts::runtime(1),
        const { contracts::constant(2) },
        contracts::tracked(3),
        contracts::never(4),
        contracts::static_borrow("static"),
        contracts::borrowed("borrowed"),
        contracts::renamed("renamed"),
        contracts::independent("first", "second"),
        contracts::tied("first", "second"),
        contracts::outlives("first", "second"),
        contracts::renamed_outlives("first", "second"),
    ));
    black_box(default_relocation::values());
}

mod parameter_patterns {
    pub(super) const fn first((value, _): (u8, u8)) -> u8 { value }

    pub(super) const fn last((_, value): (u8, u8)) -> u8 { value }

    pub(super) fn firsts(pairs: &[(u8, u8)]) -> Vec<u8> { pairs.iter().map(|&(value, _)| value).collect() }

    pub(super) fn lasts(pairs: &[(u8, u8)]) -> Vec<u8> { pairs.iter().map(|&(_, value)| value).collect() }
}

mod typed_closures {
    pub(super) fn byte_input() -> Option<u32> { (|value: u8| value.checked_add(1).map(u32::from))(255) }

    pub(super) fn word_input() -> Option<u32> { (|value: u16| value.checked_add(1).map(u32::from))(255) }

    pub(super) fn byte_output() -> u64 { u64::from((|| -> f32 { 1.0 })().to_bits()) }

    pub(super) fn word_output() -> u64 { u64::from((|| -> f64 { 1.0 })().to_bits()) }
}

mod trait_methods {

    // --- [TYPES]

    trait First {
        fn value(&self) -> u32 { 1 }
    }

    trait Last {
        fn value(&self) -> u32 { 2 }
    }

    // --- [OPERATIONS]

    impl First for u8 {}
    impl First for u16 {}
    impl Last for u8 {}
    impl Last for u16 {}

    // --- [COMPOSITION]

    pub(super) mod first {
        use super::First;

        pub(crate) fn value(value: &u8) -> u32 { value.value() }
    }

    pub(super) mod last {
        use super::Last;

        pub(crate) fn value(value: &u8) -> u32 { value.value() }
    }
}

mod contracts {
    pub(super) fn runtime(value: u32) -> u32 { value }

    pub(super) const fn constant(value: u32) -> u32 { value }

    #[track_caller]
    pub(super) fn tracked(value: u32) -> u32 { value }

    #[inline(never)]
    pub(super) fn never(value: u32) -> u32 { value }

    pub(super) fn static_borrow(value: &'static str) -> &'static str { value }

    pub(super) fn borrowed<'a>(value: &'a str) -> &'a str { value }

    pub(super) fn renamed<'other>(value: &'other str) -> &'other str { value }

    pub(super) fn independent<'a, 'b>(first: &'a str, second: &'b str) -> (&'a str, &'b str) { (first, second) }

    pub(super) fn tied<'a>(first: &'a str, second: &'a str) -> (&'a str, &'a str) { (first, second) }

    pub(super) fn outlives<'long: 'short, 'short>(first: &'long str, second: &'short str) -> (&'long str, &'short str) { (first, second) }

    pub(super) fn renamed_outlives<'a: 'b, 'b>(first: &'a str, second: &'b str) -> (&'a str, &'b str) { (first, second) }
}

mod parameter_drops {
    use core::cell::RefCell;
    use core::mem::DropGuard;

    pub(super) fn values() -> Vec<&'static str> {
        struct Fields<T> {
            first: T,
            last: T,
        }

        fn declared<T>(Fields { first: _first, last: _last }: Fields<T>, visit: impl FnOnce()) { visit() }

        fn reordered<T>(Fields { last: _last, first: _first }: Fields<T>, visit: impl FnOnce()) { visit() }

        fn renamed<T>(Fields { first: _head, last: _tail }: Fields<T>, visit: impl FnOnce()) { visit() }

        let events = RefCell::new(Vec::new());
        let fields = || {
            let [first, last] = ["first", "last"].map(|field| DropGuard::new((field, &events), |(field, events)| events.borrow_mut().push(field)));
            Fields { first, last }
        };
        let visit = || {};
        declared(fields(), visit);
        reordered(fields(), visit);
        renamed(fields(), visit);
        events.into_inner()
    }
}

mod stateful_binary {
    use core::cell::Cell;

    pub(super) fn ordered(state: &Cell<u8>, left: u8, right: u8) -> bool { state.replace(left) == state.replace(right) }

    pub(super) fn reversed(state: &Cell<u8>, left: u8, right: u8) -> bool { state.replace(right) == state.replace(left) }

    pub(super) fn repeated(state: &Cell<u8>, left: u8, right: u8) -> bool { state.replace(left) == state.replace(right) }

    pub(super) fn ordered_calls(state: &Cell<u8>, left: u8, right: u8) -> (u8, u8) { (state.replace(left), state.replace(right)) }

    pub(super) fn reversed_calls(state: &Cell<u8>, left: u8, right: u8) -> (u8, u8) { (state.replace(right), state.replace(left)) }

    pub(super) fn repeated_calls(state: &Cell<u8>, left: u8, right: u8) -> (u8, u8) { (state.replace(left), state.replace(right)) }

    pub(super) fn length_forward(visit: impl FnOnce(usize) -> usize) -> usize {
        let items = [0_u8; {
            let mut state = 0;
            let _same = core::mem::replace(&mut state, 1) == core::mem::replace(&mut state, 2);
            state
        }];
        visit(items.len())
    }

    pub(super) fn length_reversed(visit: impl FnOnce(usize) -> usize) -> usize {
        let items = [0_u8; {
            let mut state = 0;
            let _same = core::mem::replace(&mut state, 2) == core::mem::replace(&mut state, 1);
            state
        }];
        visit(items.len())
    }
}

mod default_relocation {
    use core::fmt::{Debug, Display};

    use super::{Circle, Square};

    // --- [TYPES]

    trait Counted {
        fn count(values: &[u8]) -> usize;
    }

    trait Described<T: ToString> {
        fn width(value: &T) -> usize;
    }

    trait Characters<T> {
        fn count(value: &T) -> usize;
    }

    trait FixedInput<T> {
        fn magnitude(value: &T) -> usize;
    }

    trait Repeated<T, U> {
        fn pair(first: T, second: U) -> (T, T);
    }

    trait SendMeasured {
        fn measure(&self, payload: usize) -> usize;
    }

    trait Retained<'a> {
        fn footprint(&self, capacity: usize) -> usize;
    }

    trait BoxedView: Display + 'static {
        fn into_display(self: Box<Self>) -> Box<dyn Display>;
    }

    // --- [MODELS]

    struct SendBound<T: Send + ?Sized>(Box<T>);

    struct LifetimeBound<'a, T: ?Sized + 'a>(&'a T);

    // --- [OPERATIONS]

    impl Counted for Square {
        fn count(values: &[u8]) -> usize { values.len() }
    }

    impl Counted for Circle {
        fn count(values: &[u8]) -> usize { values.len() }
    }

    impl<T: ToString> Described<T> for Square {
        fn width(value: &T) -> usize { value.to_string().len() }
    }

    impl<T: ToString> Described<T> for Circle {
        fn width(value: &T) -> usize { value.to_string().len() }
    }

    impl<T: ToString> Characters<T> for Square {
        fn count(value: &T) -> usize { value.to_string().chars().count() }
    }

    impl<T: ToString> Characters<T> for Circle {
        fn count(value: &T) -> usize { value.to_string().chars().count() }
    }

    impl FixedInput<u8> for Square {
        fn magnitude(value: &u8) -> usize { usize::from(*value) }
    }

    impl FixedInput<u8> for Circle {
        fn magnitude(value: &u8) -> usize { usize::from(*value) }
    }

    impl<T> Repeated<T, T> for Square {
        fn pair(first: T, second: T) -> (T, T) { (first, second) }
    }

    impl<T> Repeated<T, T> for Circle {
        fn pair(first: T, second: T) -> (T, T) { (first, second) }
    }

    impl SendMeasured for Square {
        fn measure(&self, payload: usize) -> usize { payload.saturating_add(size_of::<SendBound<Self>>()) }
    }

    impl SendMeasured for Circle {
        fn measure(&self, payload: usize) -> usize { payload.saturating_add(size_of::<SendBound<Self>>()) }
    }

    impl<'a> Retained<'a> for Square {
        fn footprint(&self, capacity: usize) -> usize { capacity.max(size_of::<LifetimeBound<'a, Self>>()) }
    }

    impl<'a> Retained<'a> for Circle {
        fn footprint(&self, capacity: usize) -> usize { capacity.max(size_of::<LifetimeBound<'a, Self>>()) }
    }

    impl BoxedView for Square {
        fn into_display(self: Box<Self>) -> Box<dyn Display> { self }
    }

    impl BoxedView for Circle {
        fn into_display(self: Box<Self>) -> Box<dyn Display> { self }
    }

    pub(super) fn values() -> impl Debug {
        let (square, circle) = (Square { side: 1.0 }, Circle { radius: 2.0 });
        (
            [<Square as Counted>::count(&[0, 1]), <Circle as Counted>::count(&[2, 0])],
            [<Square as Described<u8>>::width(&3), <Circle as Described<u16>>::width(&4)],
            [<Square as Characters<&str>>::count(&"é"), <Circle as Characters<&str>>::count(&"中")],
            [<Square as FixedInput<u8>>::magnitude(&5), <Circle as FixedInput<u8>>::magnitude(&6)],
            [<Square as Repeated<u8, u8>>::pair(7, 8), <Circle as Repeated<u8, u8>>::pair(9, 10)],
            [square.measure(11), circle.measure(12)],
            [<Square as Retained<'static>>::footprint(&square, 13), <Circle as Retained<'static>>::footprint(&circle, 14)],
            [Box::new(square).into_display().to_string(), Box::new(circle).into_display().to_string()],
        )
    }
}
