use std::{borrow::Cow, error::Error, fmt, num::ParseIntError, rc::Rc, sync::Arc};

use anyhow::Result as Outcome;
use eyre::Report;
use tokio::task;
use tower::BoxError;

// --- [TYPES] ---------------------------------------------------------------------------

type Boxed<T> = Result<T, Box<dyn Error + Send + Sync>>;
type Fallible<T, E = String> = Result<T, E>;

trait Source {
    fn read(&self) -> Result<u16, String>;
}

trait Store {
    type Error;

    async fn fetch(&self, key: u16) -> Result<u16, Self::Error>;
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Pending {
    outcome: Result<u16, ()>,
}

#[derive(Debug)]
struct Digits(u16);

impl TryFrom<&str> for Digits {
    type Error = String;

    fn try_from(text: &str) -> Result<Self, Self::Error> { text.parse().map(Self).map_err(|_error: ParseIntError| String::from("text is not a port")) }
}

#[derive(Debug)]
struct Port;

impl Source for Port {
    fn read(&self) -> Result<u16, String> { Ok(8080) }
}

#[derive(Debug)]
struct Memory;

impl Store for Memory {
    type Error = String;

    async fn fetch(&self, key: u16) -> Result<u16, Self::Error> {
        task::yield_now().await;
        Ok(key)
    }
}

#[derive(Debug)]
struct Reply(u16);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug)]
enum Code {
    Invalid,
}

impl fmt::Display for Code {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { f.write_str("port is not a number") }
}

#[derive(Debug)]
enum Relocated {
    Moved,
}

#[derive(Debug, thiserror::Error)]
enum PortError {
    #[error("port is not a number")]
    Parse(#[from] ParseIntError),
}

#[derive(Debug, thiserror::Error)]
#[error("tagged value is not a port")]
struct Tagged<E>(#[source] E);

// --- [OPERATIONS] ----------------------------------------------------------------------

fn spelled(text: &str) -> Result<u16, String> { Digits::try_from(text).map(|digits| digits.0) }

fn borrowed(text: &str) -> Result<u16, &'static str> { text.parse().map_err(|_error: ParseIntError| "port is not a number") }

fn renamed_alias(text: &str) -> Outcome<u16> { Ok(text.parse()?) }

fn imported_report(text: &str) -> Result<u16, Report> { Ok(text.parse()?) }

fn foreign_box_alias(text: &str) -> Result<u16, BoxError> { Ok(text.parse()?) }

fn boxed(text: &str) -> Result<u16, Box<dyn Error + Send + Sync>> { Ok(text.parse()?) }

fn local_alias(text: &str) -> Boxed<u16> { Ok(text.parse()?) }

fn generic_default(text: &str) -> Fallible<u16> { text.parse().map_err(|_error: ParseIntError| String::from("port is not a number")) }

async fn awaited(text: &str) -> Result<u16, String> {
    task::yield_now().await;
    spelled(text)
}

fn coded(text: &str) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| Code::Invalid) }

fn retried(text: &str, code: Code) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| code) }

fn relocated(text: &str) -> Result<u16, Relocated> { text.parse().map_err(|_error: ParseIntError| Relocated::Moved) }

fn boxed_text(text: &str) -> Result<u16, Box<str>> { text.parse().map_err(|_error: ParseIntError| Box::from("port is not a number")) }

fn shared_text(text: &str) -> Result<u16, Arc<str>> { text.parse().map_err(|_error: ParseIntError| Arc::from("port is not a number")) }

fn counted_text(text: &str) -> Result<u16, Rc<str>> { text.parse().map_err(|_error: ParseIntError| Rc::from("port is not a number")) }

fn borrowed_text(text: &str) -> Result<u16, Cow<'static, str>> { text.parse().map_err(|_error: ParseIntError| Cow::Borrowed("port is not a number")) }

fn returned(text: String) -> Result<u16, String> { text.parse().map_err(|_error: ParseIntError| text) }

fn routed(text: &str) -> Result<Reply, Reply> { text.parse().map(Reply).map_err(|_error: ParseIntError| Reply(400)) }

fn tagged<E>(outcome: Result<u16, E>) -> Result<u16, Tagged<E>> { outcome.map_err(Tagged) }

fn typed(text: &str) -> Result<u16, PortError> { Ok(text.parse()?) }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() -> Result<(), Box<dyn Error + Send + Sync>> {
    let text = "8080";
    let source: &dyn Source = &Port;
    let ports = [
        spelled(text)?,
        borrowed(text)?,
        renamed_alias(text)?,
        imported_report(text)?,
        foreign_box_alias(text)?,
        boxed(text)?,
        local_alias(text)?,
        generic_default(text)?,
        awaited(text).await?,
        coded(text).map_err(|Code::Invalid| "port is not a number")?,
        retried(text, Code::Invalid).map_err(|Code::Invalid| "port is not a number")?,
        relocated(text).map_err(|Relocated::Moved| "relocated")?,
        boxed_text(text).map_err(String::from)?,
        shared_text(text).map_err(|failure| String::from(&*failure))?,
        counted_text(text).map_err(|failure| String::from(&*failure))?,
        borrowed_text(text)?,
        returned(text.to_owned())?,
        routed(text).map_err(|failure| format!("{failure:?}"))?.0,
        tagged(text.parse::<u16>())?,
        typed(text)?,
        Digits::try_from(text)?.0,
        source.read()?,
        Memory.fetch(8080).await?,
        Pending { outcome: text.parse().map_err(drop) }.outcome.map_err(|()| "pending outcome holds no port")?,
    ];
    ports.iter().all(|port| *port == 8080).then_some(()).ok_or_else(|| Box::from("ports differ"))
}
