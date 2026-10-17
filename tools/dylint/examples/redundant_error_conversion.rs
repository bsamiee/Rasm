#![feature(try_blocks, try_blocks_heterogeneous)]

use core::{
    error, fmt,
    hint::black_box,
    num::{ParseFloatError, ParseIntError},
    str::{self, Utf8Error},
};
use std::io;

use thiserror::Error;

// --- [MACROS] --------------------------------------------------------------------------

macro_rules! text {
    ($bytes:expr) => {
        str::from_utf8($bytes)
    };
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
enum PortError {
    #[error("port bytes are not text")]
    Text(#[from] Utf8Error),
    #[error("port text holds no port number")]
    Parse(#[from] ParseIntError),
    #[error("retried port bytes are not text")]
    Retry(#[source] Utf8Error),
    #[error("port scale is invalid")]
    Scale(#[source] ScaleError),
}

#[derive(Debug, Error)]
#[error("located bytes are not text")]
struct LocatedError(#[from] Utf8Error);

#[derive(Debug, Error)]
enum OuterError {
    #[error(transparent)]
    Located(#[from] LocatedError),
    #[error("outer bytes are not text")]
    Text(#[from] Utf8Error),
}

#[derive(Debug, Error)]
enum ScaleError {
    #[error("scale is not a number")]
    Parse(#[source] ParseFloatError),
    #[error("scale bytes are not text")]
    Text {
        #[source]
        source: Utf8Error,
    },
    #[error("scale line {line} is not an integer")]
    Line {
        #[source]
        source: ParseIntError,
        line: usize,
    },
    #[error("scale {0} is out of range")]
    Range(u16),
    #[error("scale archive is not text")]
    Archive(#[source] Box<LocatedError>),
}

impl From<ParseFloatError> for ScaleError {
    fn from(value: ParseFloatError) -> Self { Self::Parse(value) }
}

impl From<Utf8Error> for ScaleError {
    fn from(value: Utf8Error) -> Self { Self::Text { source: value } }
}

impl From<ParseIntError> for ScaleError {
    fn from(value: ParseIntError) -> Self { Self::Line { source: value, line: 0 } }
}

impl From<u16> for ScaleError {
    fn from(value: u16) -> Self { Self::Range(value) }
}

impl From<LocatedError> for ScaleError {
    fn from(value: LocatedError) -> Self { Self::Archive(Box::new(value)) }
}

#[derive(Debug, Error)]
enum ReadError {
    #[error("cannot read the port file")]
    Io(#[source] io::Error),
}

impl From<io::Error> for ReadError {
    fn from(value: io::Error) -> Self { Self::Io(value) }
}

#[derive(Debug, Error)]
enum AttributedError {
    #[error("attributed bytes are not text")]
    Text(#[source] Utf8Error),
}

#[automatically_derived]
impl From<Utf8Error> for AttributedError {
    fn from(value: Utf8Error) -> Self { Self::Text(value) }
}

#[derive(Debug)]
struct ManualError(ParseIntError);

impl fmt::Display for ManualError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "manual port text holds no port number") }
}

impl error::Error for ManualError {
    fn source(&self) -> Option<&(dyn error::Error + 'static)> { Some(&self.0) }
}

impl From<ParseIntError> for ManualError {
    fn from(value: ParseIntError) -> Self { Self(value) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn port(bytes: &[u8]) -> Result<u16, PortError> {
    let text = str::from_utf8(bytes).map_err(Into::<PortError>::into)?;
    let port = text.trim().parse().map_err(PortError::from)?;
    Ok(port)
}

fn variant(bytes: &[u8]) -> Result<u16, PortError> {
    let text = str::from_utf8(bytes).map_err(PortError::Text)?;
    text.trim().parse().map_err(Into::into)
}

fn retried(bytes: &[u8]) -> Result<&str, PortError> {
    let text = str::from_utf8(bytes).map_err(PortError::Retry)?;
    Ok(text)
}

fn wrapped(bytes: &[u8]) -> Result<&str, LocatedError> {
    let text = str::from_utf8(bytes).map_err(LocatedError)?;
    Ok(text)
}

fn located(bytes: &[u8]) -> Result<&str, OuterError> {
    let text = str::from_utf8(bytes).map_err(LocatedError::from)?;
    Ok(text)
}

fn expanded(bytes: &[u8]) -> Result<&str, PortError> {
    let text = text!(bytes).map_err(PortError::from)?;
    Ok(text)
}

fn generic<E>(bytes: &[u8]) -> Result<&str, E>
where E: From<Utf8Error> {
    let text = str::from_utf8(bytes).map_err(E::from)?;
    Ok(text)
}

fn collected(lines: &[&str]) -> Result<Vec<u16>, PortError> {
    let ports = lines.iter().map(|line| line.parse()).collect::<Result<Vec<_>, _>>().map_err(PortError::Parse)?;
    Ok(ports)
}

fn manual(text: &str) -> Result<u16, ManualError> {
    let port = text.parse::<u16>().map_err(ManualError)?;
    Ok(port)
}

fn blocked(text: &str) -> Result<u16, PortError> { try { text.trim().parse::<u16>().map_err(PortError::from)? } }

fn bikeshed(text: &str) -> Result<u16, PortError> { try bikeshed Result<u16, PortError> { text.trim().parse::<u16>().map_err(PortError::from)? } }

fn read(path: &str) -> Result<String, ReadError> { Ok(fs_err::read_to_string(path)?) }

fn closured(text: &str) -> Result<u16, PortError> {
    let port = text.trim().parse().map_err(|error| PortError::from(error))?;
    Ok(port)
}

fn variant_closure(bytes: &[u8]) -> Result<&str, PortError> {
    let text = str::from_utf8(bytes).map_err(|error| PortError::Text(error))?;
    Ok(text)
}

fn attributed(bytes: &[u8]) -> Result<&str, AttributedError> { Ok(str::from_utf8(bytes)?) }

fn scale(text: &str) -> Result<f64, ScaleError> {
    let scale = text.trim().parse().map_err(ScaleError::Parse)?;
    Ok(scale)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let run = || -> Result<(), PortError> {
        let parse = |text: &str| -> Result<u16, PortError> {
            let port = text.trim().parse().map_err(<PortError as From<_>>::from)?;
            Ok(port)
        };
        let inferred = |text: &str| {
            let port: u16 = text.trim().parse().map_err(PortError::Parse)?;
            Ok::<_, PortError>(port)
        };
        port(b"8080")?;
        variant(b"8081")?;
        retried(b"8082")?;
        wrapped(b"8083").map_or(Ok(0), parse)?;
        located(b"8084").map_or(Ok(0), inferred)?;
        expanded(b"8085")?;
        generic::<PortError>(b"8086")?;
        collected(&["8087"])?;
        manual("8088").map_err(|error| PortError::Parse(error.0))?;
        scale("1.5").map_err(PortError::Scale)?;
        blocked("8089")?;
        bikeshed("8090")?;
        closured("8091")?;
        variant_closure(b"8092")?;
        attributed(b"8093").map_or(Ok(0), parse)?;
        Ok(())
    };
    black_box((run(), read("port").map(|text| text.len()))).0.unwrap_or_default();
}
