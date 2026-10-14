use std::{fs, io, num::ParseIntError, path::Path};

use thiserror::Error;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
enum PortError {
    #[error("cannot read the port file")]
    Read(#[from] io::Error),
    #[error("port file holds no port number")]
    Parse(#[from] ParseIntError),
}

#[derive(Debug, Error)]
#[error("cannot read the located file")]
struct LocatedError(#[from] io::Error);

#[derive(Debug, Error)]
enum OuterError {
    #[error(transparent)]
    Located(#[from] LocatedError),
    #[error("cannot read the outer file")]
    Read(#[from] io::Error),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn port(path: &Path) -> Result<u16, PortError> {
    let text = fs::read_to_string(path).map_err(Into::<PortError>::into)?;
    let port = text.trim().parse().map_err(PortError::from)?;
    Ok(port)
}

fn fallback(path: &Path) -> Result<u16, PortError> {
    let text = fs::read_to_string(path).map_err(PortError::Read)?;
    text.trim().parse().map_err(Into::into)
}

fn located(path: &Path) -> Result<String, OuterError> {
    let text = fs::read_to_string(path).map_err(LocatedError::from)?;
    Ok(text)
}

fn generic<E>(path: &Path) -> Result<Vec<u8>, E>
where E: From<io::Error> {
    let bytes = fs::read(path).map_err(E::from)?;
    Ok(bytes)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> Result<(), PortError> {
    let parse = |text: &str| -> Result<u16, PortError> {
        let port = text.trim().parse().map_err(<PortError as From<_>>::from)?;
        Ok(port)
    };
    port(Path::new("port"))?;
    fallback(Path::new("fallback"))?;
    located(Path::new("located")).map_or(Ok(0), |text| parse(&text))?;
    generic::<PortError>(Path::new("generic"))?;
    Ok(())
}
