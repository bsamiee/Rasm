use core::{error::Error, fmt, hint::black_box, num::ParseIntError};
use std::{borrow::Cow, io, rc::Rc, sync::Arc};

use anyhow::Context as _;
use tokio::task;

// --- [TYPES] ---------------------------------------------------------------------------

type Boxed<T> = Result<T, Box<dyn Error + Send + Sync>>;
type Fallible<T, E = String> = Result<T, E>;

pub trait Source {
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

    fn try_from(value: &str) -> Result<Self, Self::Error> { value.parse().map(Self).map_err(|_error: ParseIntError| String::from("text is not a port")) }
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

#[derive(Debug)]
struct Survey {
    ports: Vec<u16>,
    failures: Vec<PortError>,
}

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
enum PlanError {
    #[error("plan holds {} invalid ports", .0.len())]
    Invalid(Vec<PortError>),
}

#[derive(Debug, thiserror::Error)]
#[error("tagged value is not a port")]
struct Tagged<E>(#[source] E);

#[derive(Debug, thiserror::Error)]
enum ConfigError {
    #[error("{0}")]
    Message(String),
    #[error("{detail} in {context}")]
    Detail { detail: Box<str>, context: Cow<'static, str> },
    #[error("key {key} holds no port")]
    Parse {
        key: String,
        #[source]
        source: ParseIntError,
    },
    #[error("configuration is empty")]
    Empty,
}

#[derive(Debug, thiserror::Error)]
#[error("{0}")]
struct Refusal(Arc<str>);

#[derive(Debug, thiserror::Error)]
enum SyncError {
    #[error("cannot reach the peer")]
    Unreachable(#[source] Box<dyn Error + Send + Sync>),
    #[error("sync stopped at {offset}")]
    Stopped { offset: u64 },
}

#[derive(Debug, thiserror::Error)]
enum LoadError {
    #[error("cannot read {path}")]
    Read {
        path: String,
        #[source]
        source: io::Error,
    },
}

#[derive(Debug, thiserror::Error)]
enum ReportError {
    #[error("cannot open the project")]
    Project(#[source] anyhow::Error),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn spelled(text: &str) -> Result<u16, String> { if text.is_empty() { Err(String::from("port text is empty")) } else { Digits::try_from(text).map(|digits| digits.0) } }

fn borrowed(text: &str) -> Result<u16, &'static str> { text.parse().map_err(|_error: ParseIntError| "port is not a number") }

fn boxed(text: &str) -> Result<u16, Box<dyn Error + Send + Sync>> { Ok(text.trim().parse()?) }

fn local_alias(text: &str) -> Boxed<u16> { Ok(text.trim_start().parse()?) }

fn generic_default(text: &str) -> Fallible<u16> { text.parse().map_err(|_error: ParseIntError| String::from("port is not a number")) }

async fn awaited(text: &str) -> Result<u16, String> {
    task::yield_now().await;
    spelled(text)
}

fn coded(text: &str) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| Code::Invalid) }

fn retried(text: &str, code: Code) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| code) }

fn relocated(text: &str) -> Result<u16, Relocated> { text.parse().map_err(|_error: ParseIntError| Relocated::Moved) }

fn boxed_text(text: &str) -> Result<u16, Box<str>> { text.parse().map_err(|_error: ParseIntError| Box::from("boxed port is not a number")) }

fn shared_text(text: &str) -> Result<u16, Arc<str>> { text.parse().map_err(|_error: ParseIntError| Arc::from("shared port is not a number")) }

fn counted_text(text: &str) -> Result<u16, Rc<str>> { text.parse().map_err(|_error: ParseIntError| Rc::from("counted port is not a number")) }

fn borrowed_text(text: &str) -> Result<u16, Cow<'static, str>> { text.parse().map_err(|_error: ParseIntError| Cow::Borrowed("port is not a number")) }

fn shared_error(text: &str) -> Result<u16, Arc<dyn Error + Send + Sync>> { text.parse().map_err(|error: ParseIntError| -> Arc<dyn Error + Send + Sync> { Arc::new(error) }) }

fn counted_error(text: &str) -> Result<u16, Rc<dyn Error>> { text.parse().map_err(|error: ParseIntError| -> Rc<dyn Error> { Rc::new(error) }) }

fn returned(text: String) -> Result<u16, String> { text.parse().map_err(|_error: ParseIntError| text) }

fn routed(text: &str) -> Result<Reply, Reply> { text.parse().map(Reply).map_err(|_error: ParseIntError| Reply(400)) }

fn planned(texts: &[&str]) -> Result<Vec<u16>, Vec<PortError>> {
    let failures = audited(texts);
    if failures.is_empty() { Ok(texts.iter().filter_map(|text| typed(text).ok()).collect()) } else { Err(failures) }
}

fn audited(texts: &[&str]) -> Vec<PortError> { texts.iter().filter_map(|text| typed(text).err()).collect() }

fn surveyed(texts: &[&str]) -> Survey { Survey { ports: texts.iter().filter_map(|text| typed(text).ok()).collect(), failures: audited(texts) } }

fn header(bytes: Vec<u8>) -> Result<[u8; 4], Vec<u8>> { bytes.try_into() }

fn rejected(texts: &[&str]) -> Result<u16, PlanError> { audited(texts).is_empty().then_some(8080).ok_or_else(|| PlanError::Invalid(audited(texts))) }

fn tagged<E>(outcome: Result<u16, E>) -> Result<u16, Tagged<E>> { outcome.map_err(Tagged) }

fn kept(text: &str) -> Result<u16, Code> { text.trim().parse().map_err(|_error: ParseIntError| Code::Invalid) }

fn defaulted(port: Option<u16>) -> Result<u16, PortError> { port.ok_or_else(|| String::from("port is missing")).or_else(|_text| typed("8080")) }

fn described(outcome: Result<u16, PortError>) -> Result<String, PortError> { outcome.map(|port| port.to_string()) }

fn synced(offset: u64) -> Result<u64, SyncError> {
    match offset {
        0 => Err(SyncError::Stopped { offset }),
        1 => Err(SyncError::Unreachable(Box::new(io::Error::from(io::ErrorKind::NotFound)))),
        _ => Ok(offset),
    }
}

fn typed(text: &str) -> Result<u16, PortError> { Ok(text.parse()?) }

fn checked(text: &str) -> Result<u16, PortError> {
    let present = if text.is_empty() { Err("port text is empty") } else { Ok(text) };
    let spelled = |digits: &str| if digits.is_empty() { Err(String::from("port text is empty")) } else { digits.parse::<u16>().map_err(|_error: ParseIntError| String::from("port is not a number")) };
    let typed = |digits: &str| -> Result<u16, PortError> { Ok(digits.parse()?) };
    present.map_or(Ok(0), |digits| spelled(digits).or_else(|_text| typed(digits)))
}

fn configured(text: &str) -> Result<u16, ConfigError> {
    match text {
        "" => Err(ConfigError::Empty),
        "refused" => Err(ConfigError::Message(String::from("configuration refused the port"))),
        "detail" => Err(ConfigError::Detail { detail: Box::from("port detail"), context: Cow::Borrowed("port context") }),
        _ => text.parse().map_err(|source| ConfigError::Parse { key: String::from("port"), source }),
    }
}

fn refused() -> Result<u16, Refusal> { Err(Refusal(Arc::from("port refused"))) }

fn loaded(path: &str) -> Result<String, LoadError> { fs_err::read_to_string(path).map_err(|source| LoadError::Read { path: path.to_owned(), source }) }

fn reported(path: &str) -> anyhow::Result<String> { Ok(fs_err::read_to_string(path)?) }

fn contexted(text: &str) -> Result<u16, ReportError> { text.parse::<u16>().context("port").map_err(ReportError::Project) }

const fn projected(report: anyhow::Error) -> ReportError { ReportError::Project(report) }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() -> Result<(), Box<dyn Error + Send + Sync>> {
    let text = "8080";
    let source: &dyn Source = &Port;
    let ports = [
        spelled(text)?,
        borrowed(text)?,
        boxed(text)?,
        local_alias(text)?,
        generic_default(text)?,
        awaited(text).await?,
        coded(text).unwrap_or_default(),
        retried(text, Code::Invalid).unwrap_or_default(),
        relocated(text).unwrap_or_default(),
        boxed_text(text).unwrap_or_default(),
        shared_text(text).unwrap_or_default(),
        counted_text(text).unwrap_or_default(),
        borrowed_text(text)?,
        shared_error(text).unwrap_or_default(),
        counted_error(text).unwrap_or_default(),
        returned(text.to_owned())?,
        routed(text).map_or_else(|failure| failure.0, |reply| reply.0),
        tagged(text.parse::<u16>())?,
        kept(text).unwrap_or_default(),
        defaulted(Some(8080))?,
        described(typed(text))?.parse()?,
        synced(3).map(|_offset| 8080)?,
        typed(text)?,
        checked(text)?,
        configured(text)?,
        refused()?,
        Digits::try_from(text)?.0,
        source.read()?,
        Memory.fetch(8080).await?,
        Pending { outcome: text.parse().map_err(drop) }.outcome.unwrap_or_default(),
    ];
    let survey = surveyed(&[text]);
    black_box((
        [planned(&[text]).map_or(0, |found| found.len()), audited(&[text]).len(), survey.ports.len(), survey.failures.len()],
        header(vec![1, 2, 3, 4]).map_or(0, |bytes| bytes.len()),
        rejected(&[text])?,
    ));
    let files = [loaded("port")?, reported("port").map_err(projected)?, contexted(text)?.to_string()];
    ports.iter().all(|port| *port == 8080).then_some(()).filter(|()| files.iter().all(|file| !file.is_empty())).ok_or_else(|| Box::from("ports differ"))
}
