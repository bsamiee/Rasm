#![feature(default_field_values)]

use core::{hint::black_box, iter::Map, marker::PhantomData, pin::Pin};
use std::{rc::Rc, vec};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Retry {
    attempts: u8,
    label: String,
}

#[derive(Debug)]
struct Buffered {
    attempts: u8,
    bytes: Vec<u8>,
}

#[derive(Debug)]
struct Blank {
    attempts: u8,
    label: String,
}

#[derive(Debug, Default)]
struct Limits {
    retries: u8 = 3,
    timeout: u16 = 30,
    name: String,
}

#[derive(Debug, Default)]
struct Window {
    width: u16 = 80,
    height: u16,
}

#[derive(Debug)]
struct Profile {
    name: Option<String>,
}

#[derive(Debug)]
struct Draft;

#[derive(Debug)]
struct Sent;

#[derive(Debug)]
struct Message<State> {
    state: PhantomData<State>,
    body: String,
    id: u8,
}

#[derive(Debug)]
struct Readings {
    values: Vec<u8>,
}

#[derive(Debug)]
struct Callbacks {
    steps: Vec<fn() -> u8>,
    last: fn() -> u8,
}

impl IntoIterator for Readings {
    type IntoIter = Map<vec::IntoIter<u8>, fn(u8) -> u16>;
    type Item = u16;

    fn into_iter(self) -> Self::IntoIter { self.values.into_iter().map(u16::from) }
}

impl IntoIterator for Callbacks {
    type IntoIter = vec::IntoIter<fn() -> u8>;
    type Item = fn() -> u8;

    fn into_iter(self) -> Self::IntoIter {
        let mut steps = self.steps;
        steps.push(self.last);
        steps.into_iter()
    }
}

const fn step() -> u8 { 4 }

impl Default for Retry {
    fn default() -> Self { Self { attempts: 3, label: String::new() } }
}

impl Default for Buffered {
    fn default() -> Self { Self { attempts: 3, bytes: Vec::with_capacity(8) } }
}

impl Default for Blank {
    fn default() -> Self { Self { attempts: 0, label: String::new() } }
}

impl Message<Draft> {
    fn send(self) -> Message<Sent> { Message { state: PhantomData, body: self.body, id: self.id } }
}

impl Message<Sent> {
    fn resend(self) -> Self { Self { state: PhantomData, body: self.body, id: self.id.strict_add(1) } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn limits(name: String) -> Limits { Limits { name, ..Default::default() } }

fn window() -> Window { Window { width: 100, ..Default::default() } }

fn named(name: &str) -> u8 {
    let owned = name.to_owned();
    match owned.as_str() {
        "a" => 1,
        "b" => 2,
        _ => 0,
    }
}

fn counted(values: &[u8]) -> u8 {
    let owned = values.to_vec();
    if let [first, second] = owned.as_slice() { first.strict_add(*second) } else { 0 }
}

fn leading(values: &[u8]) -> u8 {
    let owned = values.to_vec();
    let [first, ..] = owned.as_slice() else { return 0 };
    *first
}

fn optional(profile: &Profile) -> u8 {
    match profile.name.as_deref() {
        Some("b") => 1,
        Some("c") => 2,
        _ => 0,
    }
}

fn shared(value: &Rc<u8>) -> u8 {
    match &**value {
        5 => 1,
        6 => 2,
        _ => 0,
    }
}

fn boxed(value: u8) -> u8 {
    let held = Box::new(value);
    match &*held {
        5 => 1,
        6 => 2,
        _ => 0,
    }
}

#[derive(Debug)]
struct Echo;

impl IntoFuture for Echo {
    type IntoFuture = Pin<Box<dyn Future<Output = u64> + Send>>;
    type Output = u64;

    fn into_future(self) -> Self::IntoFuture { Box::pin(async { 8 }) }
}

fn trimmed(name: &str) -> u8 {
    match name.trim() {
        "c" => 1,
        "d" => 2,
        _ => 0,
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let draft = Message::<Draft> { state: PhantomData, body: String::from("hello"), id: 1 };
    let sent = draft.send().resend();
    let (retry, buffered, blank, limits, window) = (Retry::default(), Buffered::default(), Blank::default(), limits(String::new()), window());
    black_box((
        (Readings { values: vec![1, 2] }.into_iter().collect::<Vec<_>>(), Callbacks { steps: vec![step], last: step }.into_iter().map(|call| call()).collect::<Vec<_>>()),
        [named("a"), counted(&[1, 2]), leading(&[3]), optional(&Profile { name: Some(String::from("b")) }), shared(&Rc::new(5)), boxed(5), trimmed(" c ")],
        [retry.attempts, buffered.attempts, blank.attempts, limits.retries, sent.id],
        [limits.timeout, window.width, window.height],
        [retry.label, blank.label, limits.name, sent.body],
        buffered.bytes,
        Echo.await,
    ));
}
