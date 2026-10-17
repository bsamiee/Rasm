use core::{hint::black_box, num::ParseIntError};
use std::io;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Failure {
    #[error("cannot read {path}: {source}")]
    Read { path: String, source: io::Error },
    #[error("cannot parse the port: {0}")]
    Parse(#[from] ParseIntError),
    #[error("{0}")]
    Wrapped(#[source] io::Error),
    #[error("cannot open {path}")]
    Open { path: String, source: io::Error },
    #[error(transparent)]
    Forwarded(io::Error),
    #[error("cannot bind {0}")]
    Bind(u16, #[source] io::Error),
    #[error("cannot write {cause:?}")]
    Write {
        #[source]
        cause: io::Error,
    },
    #[error("cannot sync {}: {}", .path, .source)]
    Sync { path: String, source: io::Error },
    #[error("cannot flush {reason}", reason = .0)]
    Flush(#[source] io::Error),
    #[error("cannot seek {path} at {}", .offset)]
    Seek { path: String, offset: u64, source: io::Error },
    #[error("cannot close {}", .source.kind())]
    Close { source: io::Error },
}

#[derive(Debug, thiserror::Error)]
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
        ],
        LoadError { source: missing() },
    ));
}
