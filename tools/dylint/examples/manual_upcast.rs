use core::{
    any::Any,
    fmt::{self, Display},
    hint::black_box,
};

// --- [TYPES] ---------------------------------------------------------------------------

trait Shape: Display {
    fn area(&self) -> u32;
    fn as_display(&self) -> &dyn Display;
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

trait Defaulted: Display {
    fn as_shown(&self) -> &dyn Display
    where Self: Sized {
        self
    }
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

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let mut square = Square { side: 2, title: String::from("unit") };
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
}
