use core::{fmt, hint::black_box, write as put};
use std::format as render;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Reading {
    label: &'static str,
    count: u32,
}

impl Reading {
    fn to_string(&self) -> u32 { self.count }
}

impl fmt::Display for Reading {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { write!(formatter, "{} {}", self.label, self.count) }
}

struct Formatter;

impl Formatter {
    fn write_fmt(&mut self, _: fmt::Arguments<'_>) -> bool { false }
}

enum Status {
    Ready,
    Waiting,
}

impl Status {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result {
        black_box(());
        if black_box(true) {
            match self {
                Self::Ready => write!(formatter, "ready"),
                Self::Waiting => write!(formatter, "waiting"),
            }
        } else {
            formatter.write_str("status")
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn displayed(value: &Reading) -> impl IntoIterator<Item = String> { [format!("{value}"), format!("{0}", value), format!("{}", value), render!("{value}"), ToString::to_string(value)] }

fn options(value: &Reading) -> impl IntoIterator<Item = String> { [format!("{value:>9}"), format!("{value:.2}"), format!("{value:?}"), format!("reading: {value}")] }

fn literals() -> impl fmt::Display {
    fmt::from_fn(|formatter| {
        write!(formatter, "plain")?;
        write!(formatter, "{{escaped}}\n")?;
        write!(formatter, r#"raw "{{value}}""#)?;
        writeln!(formatter, "row")?;
        writeln!(formatter)?;
        put!(formatter, "alias")?;
        formatter.write_str("direct")
    })
}

fn ordinary_text(value: &str, owned: &String) -> impl IntoIterator<Item = String> { [format!("{value}"), format!("{owned}"), format!("literal"), String::new()] }

fn literal_arguments() -> impl IntoIterator<Item = String> { [format!("{}", 42_u32), format!("{}", format_args!("{}", 7_u8))] }

fn temporary_owners() -> impl IntoIterator<Item = String> { [format!("{}", Box::new(1)), format!("{}", &Box::new(2)), format!("{}", format_args!("{}", Box::new(3)))] }

fn shadowed() -> bool {
    macro_rules! format {
        ($($tokens:tt)*) => {
            7_u8
        };
    }
    macro_rules! write {
        ($($tokens:tt)*) => {
            true
        };
    }
    black_box(format!("{anything}"));
    write!(unrelated, "literal")
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> fmt::Result {
    let reading = Reading { label: "items", count: 3 };
    black_box((
        displayed(&reading),
        options(&reading),
        reading.to_string(),
        literals().to_string(),
        ordinary_text("text", &String::from("owned")),
        literal_arguments(),
        temporary_owners(),
        shadowed(),
    ));
    black_box(write!(&mut Formatter, "literal"));
    let mut text = String::new();
    black_box((fmt::from_fn(|formatter| Status::Ready.fmt(formatter)).to_string(), fmt::from_fn(|formatter| Status::Waiting.fmt(formatter)).to_string()));
    fmt::Write::write_fmt(&mut text, format_args!("literal"))
}
