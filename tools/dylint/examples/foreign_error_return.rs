use core::{
    error::Error,
    fmt,
    hint::black_box,
    num::{ParseIntError, TryFromIntError},
    pin::Pin,
    task::{Context, Poll},
};
use std::{io, path::Path, sync::Arc};

use clap::Parser as _;
use tokio::{
    io::{AsyncRead, ReadBuf},
    task::{self, JoinError, JoinHandle},
};

// --- [MACROS] --------------------------------------------------------------------------

macro_rules! reader {
    ($name:ident) => {
        pub fn $name(text: &str) -> Result<u16, ParseIntError> { text.parse() }
    };
}

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Loader {
    fn load(&self) -> Result<u16, io::Error>;
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy)]
pub struct Disk;

impl Loader for Disk {
    fn load(&self) -> Result<u16, io::Error> { Ok(8080) }
}

impl AsyncRead for Disk {
    fn poll_read(self: Pin<&mut Self>, _: &mut Context<'_>, _: &mut ReadBuf<'_>) -> Poll<io::Result<()>> { Poll::Ready(Ok(())) }
}

impl fmt::Display for Disk {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("disk") }
}

impl io::Write for Disk {
    fn write(&mut self, bytes: &[u8]) -> io::Result<usize> { Ok(bytes.len()) }

    fn flush(&mut self) -> io::Result<()> { Ok(()) }
}

#[derive(Debug, Clone, Copy, clap::Parser)]
pub struct Options {
    #[arg(long, default_value_t = 8080)]
    pub port: u16,
}

#[derive(Debug, Clone, Copy)]
pub struct Port(pub u16);

impl TryFrom<u32> for Port {
    type Error = TryFromIntError;

    fn try_from(value: u32) -> Result<Self, Self::Error> { u16::try_from(value).map(Self) }
}

impl Disk {
    pub fn open(self) -> io::Result<u16> { self.load() }

    fn reopen(self) -> io::Result<u16> { self.open() }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum PortError {
    #[error("port is not a number")]
    Parse(#[from] ParseIntError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn parse(text: &str) -> Result<u16, ParseIntError> { text.parse() }

pub async fn fetch(text: &str) -> Result<u16, ParseIntError> {
    task::yield_now().await;
    parse(text)
}

pub fn shared(text: &str) -> Result<u16, Arc<ParseIntError>> { parse(text).map_err(Arc::new) }

pub fn local(text: &str) -> Result<u16, PortError> { Ok(parse(text)?) }

pub const fn generic<E>(outcome: Result<u16, E>) -> Result<u16, E>
where E: Error {
    outcome
}

reader!(expanded);

fn hidden(text: &str) -> Result<u16, ParseIntError> { text.trim().parse() }

pub fn written(formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("port") }

pub fn relay() -> JoinHandle<()> { tokio::spawn(async {}) }

pub fn narrowed(value: u32) -> Result<Port, TryFromIntError> { Port::try_from(value) }

pub async fn joined(port: u16) -> Result<u16, JoinError> { tokio::spawn(async move { port }).await }

pub fn load(path: &Path) -> io::Result<String> { fs_err::read_to_string(path) }

pub fn configured(arguments: &[&str]) -> Result<Options, clap::Error> { Options::try_parse_from(arguments) }

pub fn from_str_radix(text: &str) -> Result<u16, ParseIntError> { u16::from_str_radix(text, 16) }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
pub async fn main() {
    let text = "8080";
    black_box(&(
        Disk.reopen(),
        parse(text),
        fetch(text).await,
        shared(text),
        local(text),
        generic(hidden(text)),
        expanded(text),
        relay().await,
        fmt::from_fn(written).to_string(),
        narrowed(8080),
        joined(8080).await,
        Disk.to_string(),
        load(Path::new("port")),
        configured(&["fixture"]).map(|options| options.port),
        from_str_radix("1f90"),
    ));
}
