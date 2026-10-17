use core::{cmp::Ordering, fmt, hint::black_box, num::NonZero, ops::Range};
use std::{
    io,
    process::{ExitStatus, Output},
    sync::mpsc,
};

use serde::Serialize;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Origin {
    host: String,
    port: u16,
}

#[derive(Debug)]
struct Endpoint {
    host: String,
    port: u16,
}

#[derive(Debug)]
struct User {
    id: u32,
    name: String,
    email: String,
}

#[derive(Debug)]
struct Group {
    id: u32,
    name: String,
    size: u32,
}

#[derive(Debug)]
struct Users {
    items: Vec<User>,
    cursor: Option<String>,
}

#[derive(Debug)]
struct Groups {
    items: Vec<Group>,
    cursor: Option<String>,
}

#[derive(Debug)]
struct Paged<T> {
    entries: Vec<T>,
    next: Option<String>,
}

#[derive(Debug)]
struct Members {
    entries: Vec<User>,
    next: Option<String>,
}

#[derive(Debug)]
enum Event {
    Moved { x: f64, y: f64 },
    Clicked { x: f64, y: f64 },
}

#[derive(Debug)]
enum Request {
    Fetch { host: String, port: u16 },
    Close,
}

#[derive(Debug)]
struct Link {
    host: String,
    path: String,
    scheme: String,
}

#[derive(Debug)]
struct Location {
    host: String,
    path: String,
}

#[derive(Debug)]
struct Meters(f64);

#[derive(Debug)]
struct Seconds(f64);

#[derive(Debug)]
struct Interval {
    from: u32,
    to: u32,
}

#[derive(Debug)]
struct Address {
    port: NonZero<u16>,
    timeout: NonZero<u32>,
}

#[derive(Debug)]
struct AddressDto {
    port: u16,
    timeout: u32,
}

#[derive(Debug)]
struct Account {
    login: String,
    email: String,
}

#[derive(Debug)]
struct AccountRow {
    user: String,
    address: String,
}

#[derive(Debug)]
struct Celsius(f64);

#[derive(Debug)]
struct Reading(f64);

#[derive(Debug)]
struct Fahrenheit(f64);

#[derive(Debug)]
struct Pixel {
    column: i32,
    row: i32,
}

#[derive(Debug)]
struct Cell {
    column: i32,
    row: i32,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "camelCase")]
struct Contact {
    given: String,
    family: String,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "kebab-case")]
struct Card {
    given: String,
    family: String,
}

#[derive(Debug)]
enum Figure {
    Circle { radius: f64 },
    Rectangle { width: f64, height: f64 },
}

#[derive(Debug)]
enum Shape {
    Circle(f64),
    Square(f64),
}

#[derive(Debug, PartialEq, Eq)]
enum ShapeKind {
    Circle,
    Square,
}

#[derive(Debug)]
enum Token {
    Word(String),
    Number(u32),
}

#[derive(Debug, PartialEq, Eq)]
enum Order {
    Less,
    Equal,
    Greater,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Phase {
    Start,
    Stop,
}

#[derive(Debug, PartialEq, Eq)]
enum Signal {
    Begin,
    End,
}

#[derive(Debug)]
struct Either {
    left: bool,
    right: bool,
}

#[derive(Debug)]
struct Both {
    left: bool,
    right: bool,
}

#[derive(Debug)]
struct Notice<M, P> {
    method: M,
    params: P,
}

#[derive(Debug)]
struct Invocation<M, P> {
    method: M,
    params: Option<P>,
}

#[derive(Debug, PartialEq, Eq)]
enum Trend {
    Falling,
    Flat,
    Rising,
}

#[derive(Debug)]
enum Message {
    Text(String),
    Bytes(Vec<u8>),
}

#[derive(Debug)]
enum Frame {
    Text(String),
    Bytes(Vec<u8>),
}

#[derive(Debug, PartialEq, Eq)]
enum Threshold {
    Trace,
    Debug,
    Info,
    Warn,
}

#[derive(Debug, PartialEq, Eq)]
enum Extreme {
    Min,
    Max,
}

#[derive(Debug)]
struct Bounds {
    start: u32,
    end: u32,
}

#[derive(Debug, Clone, Copy)]
struct Span {
    start: u64,
    end: u64,
}

#[derive(Debug)]
struct Window {
    start: usize,
    end: usize,
}

#[derive(Debug)]
struct Logged {
    status: ExitStatus,
    stdout: Vec<u8>,
    stderr: String,
}

#[derive(Debug)]
struct Captured {
    status: ExitStatus,
    stdout: Vec<u8>,
    stderr: Vec<u8>,
}

#[derive(Debug)]
enum Rejected<T> {
    Full(T),
    Disconnected(T),
}

#[derive(Debug)]
enum Maybe<T> {
    Some(T),
    None,
}

#[derive(Debug)]
enum Outcome<T, E> {
    Ok(T),
    Err(E),
}

#[derive(Debug)]
enum Pending<T> {
    Ok(T),
    Err(String),
    Waiting,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum ManifestError {
    #[error("cannot read the manifest")]
    Read(#[source] io::Error),
    #[error("cannot write the manifest")]
    Write(#[source] io::Error),
    #[error("manifest names no package")]
    Unnamed,
}

#[derive(Debug, thiserror::Error)]
enum ServeError<X> {
    #[error("cannot start the runtime")]
    Runtime(#[source] io::Error),
    #[error("cannot serve the endpoint")]
    Serve(#[source] io::Error),
    #[error("endpoint failed")]
    Endpoint(#[source] X),
}

#[derive(Debug, thiserror::Error)]
enum SchemaError {
    #[error("{path} breaks the schema at line {line}")]
    Invalid { path: String, line: u32 },
    #[error("{path} ends at line {line}")]
    Truncated { path: String, line: u32 },
    #[error("schema holds {0} keys")]
    Oversized(usize),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

impl fmt::Display for Pixel {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{}x{}", self.column, self.row) }
}

impl Shape {
    const fn kind(&self) -> ShapeKind {
        match self {
            Self::Circle(_) => ShapeKind::Circle,
            Self::Square(_) => ShapeKind::Square,
        }
    }

    const fn side(&self) -> f64 {
        match *self {
            Self::Circle(radius) => radius,
            Self::Square(side) => side,
        }
    }
}

impl Window {
    const fn width(&self) -> usize { self.end.saturating_sub(self.start) }
}

fn row(account: &Account) -> (String, String) {
    let row = AccountRow { user: account.login.clone(), address: account.email.clone() };
    (row.user, row.address)
}

const fn reading(celsius: &Celsius) -> Reading { Reading(celsius.0) }

const fn fahrenheit(celsius: &Celsius) -> Fahrenheit { Fahrenheit(celsius.0.mul_add(1.8, 32.0)) }

const fn signal(phase: Phase) -> Signal {
    match phase {
        Phase::Start => Signal::Begin,
        Phase::Stop => Signal::End,
    }
}

trait Gate {
    fn open(&self) -> bool;
}

impl Gate for Either {
    fn open(&self) -> bool { self.left || self.right }
}

impl Gate for Both {
    fn open(&self) -> bool { self.left && self.right }
}

fn decoded(dto: &AddressDto) -> u16 {
    #[derive(Debug)]
    struct Shim {
        port: u16,
        timeout: u32,
    }
    let shim = Shim { port: dto.port, timeout: dto.timeout };
    shim.port.strict_add(u16::try_from(shim.timeout).unwrap_or_default())
}

fn located(link: &Link) -> Location { Location { host: link.host.clone(), path: link.path.clone() } }

const fn order(ordering: Ordering) -> Order {
    match ordering {
        Ordering::Less => Order::Less,
        Ordering::Equal => Order::Equal,
        Ordering::Greater => Order::Greater,
    }
}

const fn trend(ordering: Ordering) -> Trend {
    match ordering {
        Ordering::Less => Trend::Falling,
        Ordering::Equal => Trend::Flat,
        Ordering::Greater => Trend::Rising,
    }
}

fn framed(message: Message) -> Frame {
    match message {
        Message::Text(text) => Frame::Text(text),
        Message::Bytes(bytes) => Frame::Bytes(bytes),
    }
}

const fn threshold(rank: u8) -> Threshold {
    match rank {
        0 => Threshold::Trace,
        1 => Threshold::Debug,
        2 => Threshold::Info,
        _ => Threshold::Warn,
    }
}

const fn extreme(high: bool) -> Extreme { if high { Extreme::Max } else { Extreme::Min } }

fn captured(output: Output) -> Captured { Captured { status: output.status, stdout: output.stdout, stderr: output.stderr } }

fn rejected(sender: &mpsc::SyncSender<u8>) -> Option<Rejected<u8>> {
    match sender.try_send(1) {
        Ok(()) => None,
        Err(mpsc::TrySendError::Full(value)) => Some(Rejected::Full(value)),
        Err(mpsc::TrySendError::Disconnected(value)) => Some(Rejected::Disconnected(value)),
    }
}

fn maybe(value: Option<u8>) -> Maybe<u8> { value.map_or(Maybe::None, Maybe::Some) }

const fn outcome(value: Result<u8, u16>) -> Outcome<u8, u16> {
    match value {
        Ok(held) => Outcome::Ok(held),
        Err(error) => Outcome::Err(error),
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let origin = Origin { host: String::from("local"), port: 80 };
    let endpoint = Endpoint { host: origin.host.clone(), port: origin.port };
    let users = Users { items: vec![User { id: 1, name: String::from("ada"), email: String::from("ada@example.com") }], cursor: None };
    let groups = Groups { items: vec![Group { id: 2, name: String::from("core"), size: 3 }], cursor: Some(String::from("next")) };
    let people = users.items.iter().map(|user| (user.id, user.name.len(), user.email.len())).collect::<Vec<_>>();
    let teams = groups.items.iter().map(|group| (group.id, group.size, group.name.len())).collect::<Vec<_>>();
    let paged = Paged { entries: vec![1_u8], next: None };
    let members = Members { entries: Vec::new(), next: Some(String::from("next")) };
    let moved = [Event::Moved { x: 1.0_f64, y: 2.0_f64 }, Event::Clicked { x: 3.0_f64, y: 4.0_f64 }]
        .iter()
        .map(|event| match *event {
            Event::Moved { x, y } | Event::Clicked { x, y } => x + y,
        })
        .sum::<f64>();
    let fetched = [Request::Fetch { host: endpoint.host, port: endpoint.port }, Request::Close]
        .iter()
        .map(|request| match request {
            Request::Fetch { host, port } => (host.len(), *port),
            Request::Close => (0, 0),
        })
        .collect::<Vec<_>>();
    let location = located(&Link { host: String::from("example.com"), path: String::from("/"), scheme: String::from("https") });
    let link = Link { host: location.host, path: location.path, scheme: String::from("http") };
    let address = Address { port: NonZero::<u16>::MIN, timeout: NonZero::<u32>::MIN };
    let dto = AddressDto { port: u16::from(address.port), timeout: u32::from(address.timeout) };
    let contact = Contact { given: String::from("ada"), family: String::from("lovelace") };
    let card = Card { given: String::from("grace"), family: String::from("hopper") };
    let area = [Figure::Circle { radius: 1.0_f64 }, Figure::Rectangle { width: 1.0_f64, height: 2.0_f64 }]
        .iter()
        .map(|figure| match *figure {
            Figure::Circle { radius } => radius * radius,
            Figure::Rectangle { width, height } => width * height,
        })
        .sum::<f64>();
    let (meters, seconds, interval) = (Meters(1.0), Seconds(2.0), Interval { from: 1, to: 2 });
    let (pixel, cell) = (Pixel { column: 1, row: 2 }, Cell { column: 3, row: 4 });
    let shapes = [Shape::Circle(1.0), Shape::Square(2.0)];
    let kinds = shapes.iter().filter(|shape| shape.kind() == ShapeKind::Circle).count();
    let sides = shapes.iter().map(Shape::side).sum::<f64>();
    let tokens = [Token::Word(String::from("a")), Token::Number(1)]
        .iter()
        .map(|token| match token {
            Token::Word(word) => word.len(),
            Token::Number(number) => usize::from(*number > 0),
        })
        .sum::<usize>();
    let ordered = order(1_u8.cmp(&2)) == Order::Less && trend(2_u8.cmp(&2)) == Trend::Flat;
    let lengths = [framed(Message::Text(String::from("hi"))), framed(Message::Bytes(vec![1]))]
        .iter()
        .map(|frame| match frame {
            Frame::Text(text) => text.len(),
            Frame::Bytes(bytes) => bytes.len(),
        })
        .collect::<Vec<_>>();
    let levels = threshold(3) == Threshold::Warn;
    let extremes = extreme(true) == Extreme::Max && extreme(false) == Extreme::Min && NonZero::<u16>::MAX > NonZero::<u16>::MIN;
    let ranges: (Range<u32>, Bounds, Span, Window) = (0..1, Bounds { start: 0, end: 1 }, Span { start: 2, end: 3 }, Window { start: 4, end: 5 });
    let logged = Logged { status: ExitStatus::default(), stdout: Vec::new(), stderr: String::new() };
    let output = captured(Output { status: ExitStatus::default(), stdout: Vec::new(), stderr: Vec::new() });
    let (sender, _receiver) = mpsc::sync_channel::<u8>(1);
    let pending = [Pending::Ok(1_u8), Pending::Err(String::new()), Pending::Waiting]
        .iter()
        .map(|state| match state {
            Pending::Ok(value) => usize::from(*value),
            Pending::Err(error) => error.len(),
            Pending::Waiting => 0,
        })
        .sum::<usize>();
    black_box((
        [moved, area, meters.0, seconds.0, sides],
        (people, users.cursor, teams, groups.cursor, fetched, paged.entries, paged.next, members.entries, members.next),
        [contact.given, contact.family, card.given, card.family, link.host, link.path, link.scheme],
        [interval.from, interval.to, u32::from(dto.port), dto.timeout, ranges.0.end, ranges.1.start, ranges.1.end],
        (pixel.to_string(), Pixel { column: 5, row: 6 }.to_string(), decoded(&dto), cell.column, cell.row, kinds, tokens, ordered, lengths),
        (levels, extremes, ranges.2.start, ranges.2.end, ranges.3.width(), rejected(&sender)),
        (logged.status, logged.stdout, logged.stderr, output.status, output.stdout, output.stderr),
        (maybe(Some(1)), outcome(Ok(1)), pending, {
            let (notice, invocation) = (Notice { method: 1_u8, params: 2_u8 }, Invocation { method: 1_u8, params: Some(2_u8) });
            (notice.method, notice.params, invocation.method, invocation.params)
        }),
        (row(&Account { login: String::from("ada"), email: String::from("ada@example.com") }), reading(&Celsius(1.0)).0, fahrenheit(&Celsius(2.0)).0),
        (signal(Phase::Start) == Signal::Begin, signal(Phase::Stop) == Signal::End, Either { left: true, right: false }.open(), Both { left: true, right: true }.open()),
        [ManifestError::Read(io::Error::from(io::ErrorKind::NotFound)), ManifestError::Write(io::Error::from(io::ErrorKind::PermissionDenied)), ManifestError::Unnamed].map(|error| error.to_string()),
        [SchemaError::Invalid { path: String::from("a"), line: 1 }, SchemaError::Truncated { path: String::from("b"), line: 2 }, SchemaError::Oversized(3)].map(|error| error.to_string()),
        [ServeError::Runtime(io::Error::from(io::ErrorKind::NotFound)), ServeError::Serve(io::Error::from(io::ErrorKind::AddrInUse)), ServeError::Endpoint(fmt::Error)].map(|error| error.to_string()),
    ));
}
