use core::{
    any::Any,
    fmt::{self, Display},
    hint::black_box,
};

// --- [TYPES] ---------------------------------------------------------------------------

trait Shape: Display {
    fn area(&self) -> u32;
    fn as_display(&self) -> &dyn Display;
    fn threaded(&self) -> &(dyn Display + Send);
    fn shared(&self) -> &(dyn Display + Send)
    where Self: Send;
    fn permanent(&self) -> &(dyn Display + 'static);
    fn retained(&self) -> &(dyn Display + 'static)
    where Self: 'static;
    fn supplied(&self, token: String) -> &dyn Display;
}

trait Named: Display {
    fn name(&self) -> String;
    fn as_named_display(&mut self) -> &mut dyn Display;
}

trait Labeled: Display {
    fn label(&self) -> &dyn Display;
}

trait Boxed: Display {
    fn into_display(self: Box<Self>) -> Box<dyn Display>;
    fn into_static(self: Box<Self>) -> Box<dyn Display>
    where Self: 'static;
}

trait Erased: Display {
    fn as_erased(&self) -> &dyn Erased;
}

trait Viewed: Display {
    fn view(&self) -> &dyn Display;
}

trait Plugin {
    fn as_any(&self) -> &dyn Any;
}

trait View {
    fn as_any(&self) -> &dyn Any
    where Self: 'static;
}

trait Defaulted: Display {
    fn as_shown(&self) -> &dyn Display
    where Self: Sized {
        self
    }

    fn sized_any(&self) -> &dyn Any
    where Self: Sized + 'static {
        self
    }
}

trait Tagged<T> {}

trait TypedView<'a>: Tagged<&'a u8> {
    fn narrow(&self) -> &dyn Tagged<&'a u8>;
    fn wide(&self) -> &dyn Tagged<u16>;
    fn static_tag(&self) -> &dyn Tagged<&'static u8>;
}

trait Configured {
    type Item;
}

trait ConfiguredView: Configured {
    fn configured(&self) -> &dyn Configured<Item = Self::Item>;
}

trait FixedView: Configured {
    fn fixed(&self) -> &dyn Configured<Item = u8>;
}

trait Threaded: Display + Send + Any {
    fn display(&self) -> &(dyn Display + Send);
    fn reflected(&self) -> &dyn Any;
}

trait Witness {
    type Value: ?Sized;
}

trait Projected<T: Witness<Value = Self>>: Display {
    fn projected(&self) -> &dyn Display;
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Square {
    side: u32,
    title: String,
}

impl Display for Square {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { write!(formatter, "square {}", self.side) }
}

impl Shape for Square {
    fn area(&self) -> u32 { self.side.saturating_mul(self.side) }

    fn as_display(&self) -> &dyn Display { self }

    fn threaded(&self) -> &(dyn Display + Send) { self }

    fn shared(&self) -> &(dyn Display + Send) { self }

    fn permanent(&self) -> &(dyn Display + 'static) { self }

    fn retained(&self) -> &(dyn Display + 'static) { self }

    fn supplied(&self, _token: String) -> &dyn Display { self }
}

impl Named for Square {
    fn name(&self) -> String { self.title.clone() }

    fn as_named_display(&mut self) -> &mut dyn Display { self }
}

impl Labeled for Square {
    fn label(&self) -> &dyn Display { &self.title }
}

impl Boxed for Square {
    fn into_display(self: Box<Self>) -> Box<dyn Display> { self }

    fn into_static(self: Box<Self>) -> Box<dyn Display> { self }
}

impl Erased for Square {
    fn as_erased(&self) -> &dyn Erased { self }
}

impl Defaulted for Square {}

impl Viewed for Square {
    fn view(&self) -> &dyn Display { self }
}

#[derive(Debug)]
struct Circle {
    radius: u32,
}

impl Display for Circle {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { write!(formatter, "circle {}", self.radius) }
}

impl Shape for Circle {
    fn area(&self) -> u32 { self.radius.saturating_mul(3) }

    fn as_display(&self) -> &dyn Display { self }

    fn threaded(&self) -> &(dyn Display + Send) { self }

    fn shared(&self) -> &(dyn Display + Send) { self }

    fn permanent(&self) -> &(dyn Display + 'static) { self }

    fn retained(&self) -> &(dyn Display + 'static) { self }

    fn supplied(&self, _token: String) -> &dyn Display { self }
}

impl Named for Circle {
    fn name(&self) -> String { String::from("circle") }

    fn as_named_display(&mut self) -> &mut dyn Display { self }
}

impl Labeled for Circle {
    fn label(&self) -> &dyn Display { &self.radius }
}

impl Boxed for Circle {
    fn into_display(self: Box<Self>) -> Box<dyn Display> { self }

    fn into_static(self: Box<Self>) -> Box<dyn Display> { self }
}

impl Erased for Circle {
    fn as_erased(&self) -> &dyn Erased {
        const UNIT: Circle = Circle { radius: 1 };
        &UNIT
    }
}

impl Defaulted for Circle {}

impl Viewed for Circle {
    fn view(&self) -> &dyn Display { &self.radius }
}

impl Plugin for Square {
    fn as_any(&self) -> &dyn Any { self }
}

impl Plugin for Circle {
    fn as_any(&self) -> &dyn Any { self }
}

struct Borrowed<'a>(&'a str);

impl View for Borrowed<'_> {
    fn as_any(&self) -> &dyn Any
    where Self: 'static {
        self
    }
}

impl<T> Tagged<T> for Square {}

impl<T> Tagged<T> for Circle {}

impl<'a> TypedView<'a> for Square {
    fn narrow(&self) -> &dyn Tagged<&'a u8> { self }

    fn wide(&self) -> &dyn Tagged<u16> { self }

    fn static_tag(&self) -> &dyn Tagged<&'static u8> { self }
}

impl<'a> TypedView<'a> for Circle {
    fn narrow(&self) -> &dyn Tagged<&'a u8> { self }

    fn wide(&self) -> &dyn Tagged<u16> { self }

    fn static_tag(&self) -> &dyn Tagged<&'static u8> { self }
}

impl Configured for Square {
    type Item = u8;
}

impl Configured for Circle {
    type Item = u16;
}

impl ConfiguredView for Square {
    fn configured(&self) -> &dyn Configured<Item = Self::Item> { self }
}

impl ConfiguredView for Circle {
    fn configured(&self) -> &dyn Configured<Item = Self::Item> { self }
}

impl FixedView for Square {
    fn fixed(&self) -> &dyn Configured<Item = u8> { self }
}

impl Threaded for Square {
    fn display(&self) -> &(dyn Display + Send) { self }

    fn reflected(&self) -> &dyn Any { self }
}

impl Threaded for Circle {
    fn display(&self) -> &(dyn Display + Send) { self }

    fn reflected(&self) -> &dyn Any { self }
}

impl Witness for () {
    type Value = Square;
}

impl Projected<()> for Square {
    fn projected(&self) -> &dyn Display { self }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn shown<'a>(value: Box<dyn Boxed + 'a>) -> Box<dyn Display> { value.into_display() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let mut square = Square { side: 2, title: String::from("unit") };
    black_box((Borrowed("value").as_any().is::<Borrowed<'static>>(), Borrowed("text").0));
    black_box(&(
        [square.as_display().to_string(), square.label().to_string(), square.as_erased().to_string(), square.as_shown().to_string(), square.as_named_display().to_string()],
        Box::new(Square { side: 3, title: square.name() }).into_display().to_string(),
        square.area(),
        [square.view().to_string(), Circle { radius: 7 }.view().to_string()],
        {
            let mut circle = Circle { radius: 1 };
            [circle.as_display().to_string(), circle.label().to_string(), circle.as_erased().to_string(), circle.as_shown().to_string(), circle.as_named_display().to_string(), circle.name()]
        },
        (Box::new(Circle { radius: 4 }).into_display().to_string(), Circle { radius: 5 }.area(), square.as_any().is::<Square>(), Circle { radius: 6 }.as_any().is::<Circle>()),
    ));
    black_box((square.threaded(), square.shared(), square.permanent(), square.retained(), square.supplied(String::from("token")), square.sized_any().is::<Square>()));
    black_box((square.narrow(), square.wide(), square.static_tag(), square.configured(), Circle { radius: 2 }.configured()));
    black_box((shown(Box::new(Circle { radius: 3 })), Box::new(Circle { radius: 3 }).into_static()));
    let view: &(dyn Shape + Send) = &square;
    let display: &(dyn Display + Send) = view;
    let configured: &dyn ConfiguredView<Item = u8> = &square;
    let source: &dyn Configured<Item = u8> = configured;
    let threaded: &dyn Threaded = &square;
    let inherited: &(dyn Display + Send) = threaded;
    let reflected: &dyn Any = threaded;
    let projected: &dyn Projected<()> = &square;
    let shown: &dyn Display = projected;
    let fixed: &dyn FixedView<Item = u8> = &square;
    black_box((display, source, inherited, threaded.display(), reflected.is::<Square>(), threaded.reflected().is::<Square>(), square.projected(), shown, fixed.fixed()));
}
