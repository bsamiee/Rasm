use core::{hint::black_box, num::ParseIntError};
use std::{
    io::{self, Read},
    path::{Path, PathBuf},
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Magic;

impl Read for Magic {
    fn read(&mut self, buffer: &mut [u8]) -> io::Result<usize> { if buffer.is_empty() { Ok(0) } else { Err(io::Error::new(io::ErrorKind::InvalidData, "bad magic")) } }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum StoreError {
    #[error("cannot read the store")]
    Read(#[from] io::Error),
}

#[derive(Debug, thiserror::Error)]
#[error("cannot open the journal after {attempts} attempts")]
struct JournalError {
    attempts: u8,
    source: io::Error,
}

impl From<io::Error> for JournalError {
    fn from(value: io::Error) -> Self { Self { attempts: 1, source: value } }
}

#[derive(Debug, thiserror::Error)]
#[error("port text {text:?} is not a number")]
struct PortText {
    text: String,
    #[source]
    source: ParseIntError,
}

#[derive(Debug, thiserror::Error)]
enum LoadError {
    #[error("cannot read {}", path.display())]
    Read {
        path: PathBuf,
        #[source]
        source: io::Error,
    },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn io_text(text: &str) -> io::Result<u16> {
    match text {
        "" => Err(io::Error::new(io::ErrorKind::InvalidData, "port text is empty")),
        "-" => Err(io::Error::other(format!("port text {text} is a dash"))),
        "?" => Err(io::Error::from(io::ErrorKind::NotFound)),
        _ => text.parse().map_err(io::Error::other),
    }
}

fn typed_io(text: &str) -> io::Result<u16> { text.parse().map_err(|source| io::Error::new(io::ErrorKind::InvalidData, PortText { text: text.to_owned(), source })) }

fn loaded(path: &Path) -> Result<String, LoadError> {
    if path.is_empty() {
        Err(LoadError::Read { path: path.to_owned(), source: io::Error::other("path is empty") })
    } else {
        fs_err::read_to_string(path).map_err(|source| LoadError::Read { path: path.to_owned(), source })
    }
}

fn stored(path: &Path) -> Result<String, StoreError> { Ok(fs_err::read_to_string(path)?) }

fn journaled(path: &Path) -> Result<String, JournalError> { Ok(fs_err::read_to_string(path)?) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(io_text("8080"), typed_io("8081"), loaded(Path::new("port")), stored(Path::new("port")), journaled(Path::new("port")), Magic.read(&mut [0; 4]))); }
