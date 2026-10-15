use core::{f64::consts::PI, fmt, hint::black_box};

// --- [TYPES] ---------------------------------------------------------------------------

trait Shape {
    fn area(&self) -> f64;
}

trait Render {
    fn render(&self) -> String;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TIMEOUT: u64 = 30;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Client {
    endpoint: u16,
}

#[derive(Debug)]
struct Policy {
    endpoint: u16,
    attempts: u8,
    timeout: u64,
}

#[derive(Debug)]
struct Page;

#[derive(Debug)]
struct Banner;

#[derive(Debug)]
struct Footer;

#[derive(Debug, Default)]
struct Uptime {
    seconds: u64,
}

#[derive(Debug, Default)]
struct Clock {
    ticks: u64,
}

#[derive(Debug, thiserror::Error)]
struct BadScheme;

#[derive(Debug, thiserror::Error)]
struct TimedOut;

#[derive(Debug)]
struct Square {
    side: f64,
}

#[derive(Debug)]
struct Circle {
    radius: f64,
}

#[derive(Debug, PartialEq, Eq)]
struct Rgb(u8, u8, u8);

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Policy {
    const fn new(client: &Client, attempts: u8, timeout: u64) -> Self { Self { endpoint: client.endpoint, attempts, timeout } }
}

impl Render for Page {
    fn render(&self) -> String { String::from("page") }
}

impl Render for Banner {
    fn render(&self) -> String { String::from("banner") }
}

impl Render for Footer {
    fn render(&self) -> String { String::from("footer") }
}

impl fmt::Display for BadScheme {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("bad scheme") }
}

impl fmt::Display for TimedOut {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("timed out") }
}

impl Shape for Square {
    fn area(&self) -> f64 { self.side * self.side }
}

impl Shape for Circle {
    fn area(&self) -> f64 { PI * self.radius * self.radius }
}

impl Square {
    fn perimeter(&self) -> f64 { self.side * 4.0 }
}

impl Circle {
    fn perimeter(&self) -> f64 { PI * self.radius * 2.0 }
}

const fn retry_reads(client: &Client) -> Policy { Policy::new(client, 3, TIMEOUT) }

const fn retry_writes(client: &Client) -> Policy { Policy::new(client, 5, TIMEOUT) }

fn total_squares(shapes: &[Square]) -> f64 { shapes.iter().map(Shape::area).sum() }

fn total_circles(shapes: &[Circle]) -> f64 { shapes.iter().map(Shape::area).sum() }

fn fence_squares(shapes: &[Square]) -> f64 { shapes.iter().map(|shape| shape.perimeter() * 2.0_f64).sum() }

fn fence_circles(shapes: &[Circle]) -> f64 { shapes.iter().map(|shape| shape.perimeter() * 2.0_f64).sum() }

const fn red() -> Rgb { Rgb(255, 0, 0) }

const fn green() -> Rgb { Rgb(0, 255, 0) }

const fn page_size() -> u32 { 1 }

const fn banner_size() -> u32 { 2 }

fn trimmed_left(text: &str) -> String { text.trim_start().repeat(2) }

fn trimmed_right(text: &str) -> String { text.trim_end().repeat(3) }

const fn is_large(value: u32) -> bool { value > 100 }

const fn is_huge(value: u32) -> bool { value > 1000 }

fn uptime() -> Uptime { Default::default() }

fn clock() -> Clock { Default::default() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let client = Client { endpoint: 80 };
    let (reads, writes) = (retry_reads(&client), retry_writes(&client));
    let squares = [Square { side: 1.0 }];
    let circles = [Circle { radius: 1.0 }];
    let measured = total_squares(&squares) + total_circles(&circles) + fence_squares(&squares) + fence_circles(&circles) + squares[0].perimeter() + circles[0].perimeter();
    black_box((
        (reads.attempts, writes.attempts, reads.timeout, writes.endpoint),
        (Page.render(), Banner.render(), trimmed_left(" a"), trimmed_right("b ")),
        measured,
        (page_size(), banner_size(), red() != green()),
        (Footer.render(), is_large(5), is_huge(6), uptime().seconds, clock().ticks, BadScheme.to_string(), TimedOut.to_string()),
    ));
}
