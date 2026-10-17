use core::hint::black_box;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Interval {
    start: u32,
    end: u32,
}

struct Label {
    text: String,
}

struct Point {
    x: i32,
    y: i32,
}

struct Measured {
    width: u32,
    height: u32,
}

#[derive(Clone)]
struct Copied {
    left: u8,
    right: u8,
}

struct Settings {
    retries: u8,
    verbose: bool,
}

#[repr(C)]
struct Header {
    tag: u8,
    size: u32,
}

struct Pair(u32, u32);

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Measured {
    const fn area(&self) -> u32 { self.width * self.height }
}

fn interval(start: u32, end: u32) -> u32 {
    let Interval { start, end } = black_box(Interval { start, end });
    end - start
}

fn label(text: &str) -> usize {
    let Label { text } = Label { text: text.to_owned() };
    text.len()
}

fn point(x: i32, y: i32) -> i32 {
    let point = Point { x, y };
    point.x + point.y
}

fn copied(left: u8, right: u8) -> u8 {
    let Copied { left, right } = Copied { left, right }.clone();
    left ^ right
}

fn settings(retries: u8) -> u8 {
    let base = Settings { retries, verbose: false };
    let Settings { retries, verbose } = Settings { verbose: true, ..base };
    retries + u8::from(verbose)
}

fn header(tag: u8, size: u32) -> u32 {
    let Header { tag, size } = Header { tag, size };
    u32::from(tag) + size
}

fn pair(left: u32, right: u32) -> u32 {
    let Pair(left, right) = Pair(left, right);
    left + right
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    black_box((interval(1, 2), label("a"), point(3, 4), Measured { width: 5, height: 6 }.area(), copied(7, 8), settings(11), header(12, 13), pair(14, 15)));
    black_box(format!("{:?}", Interval { start: 16, end: 17 }));
}
