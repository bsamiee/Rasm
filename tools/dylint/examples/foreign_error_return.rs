use core::{error::Error, fmt, hint::black_box, num::ParseIntError};
use std::{io, sync::Arc};

use tokio::task::{self, JoinHandle};

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

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let text = "8080";
    black_box(&(Disk.reopen(), parse(text), fetch(text).await, shared(text), local(text), generic(hidden(text)), expanded(text), relay().await, fmt::from_fn(written).to_string()));
}
