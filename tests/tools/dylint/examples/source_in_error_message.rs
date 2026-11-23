#![cfg_attr(dylint_lib = "rasm_lints", allow(duplicate_type, reason = "these lints fire incidentally here and are exercised by their own fixtures"))]

use core::hint::black_box;
use core::num::ParseIntError;
use std::io;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Failure {
    //~v source_in_error_message
    #[error("cannot read {path}: {source}")]
    Read { path: String, source: io::Error },
    //~v source_in_error_message
    #[error("cannot parse the port: {0}")]
    Parse(#[from] ParseIntError),
    //~v source_in_error_message
    #[error("{0}")]
    Wrapped(#[source] io::Error),
    #[error("cannot open {path}")]
    Open { path: String, source: io::Error },
    #[error(transparent)]
    Forwarded(io::Error),
    #[error("cannot bind {0}")]
    Bind(u16, #[source] io::Error),
    //~v source_in_error_message
    #[error("cannot write {cause:?}")]
    Write {
        #[source]
        cause: io::Error,
    },
    //~v source_in_error_message
    #[error("cannot sync {}: {}", .path, .source)]
    Sync { path: String, source: io::Error },
    //~v source_in_error_message
    #[error("cannot flush {reason}", reason = .0)]
    Flush(#[source] io::Error),
    #[error("cannot seek {path} at {}", .offset)]
    Seek { path: String, offset: u64, source: io::Error },
    #[error("cannot close {}", .source.kind())]
    Close { source: io::Error },
    //~v source_in_error_message
    #[error(r"cannot receive: {source}")]
    Receive { source: io::Error },
    //~v source_in_error_message
    #[error("cannot accept: \u{7b}source\u{7d}")]
    Accept { source: io::Error },
}

#[derive(Debug, thiserror::Error)]
//~v source_in_error_message
#[error("cannot load the settings: {source}")]
struct LoadError {
    source: io::Error,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let missing = || io::Error::from(io::ErrorKind::NotFound);
    black_box(&(
        [
            Failure::Read { path: String::from("a"), source: missing() },
            Failure::from("x".parse::<u16>().unwrap_err()),
            Failure::Wrapped(missing()),
            Failure::Open { path: String::from("b"), source: missing() },
            Failure::Forwarded(missing()),
            Failure::Bind(80, missing()),
            Failure::Write { cause: missing() },
            Failure::Sync { path: String::from("c"), source: missing() },
            Failure::Flush(missing()),
            Failure::Seek { path: String::from("d"), offset: 1, source: missing() },
            Failure::Close { source: missing() },
            Failure::Receive { source: missing() },
            Failure::Accept { source: missing() },
        ],
        LoadError { source: missing() },
    ));
}
