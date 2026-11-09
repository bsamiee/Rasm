use core::fmt;
use std::{fs, io};

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(thiserror::Error)]
enum Failure {
    #[error("cannot read the settings file")]
    Read(#[source] io::Error),
}

impl fmt::Debug for Failure {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { fmt::Display::fmt(self, formatter) }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> Result<(), Failure> {
    fs::read_to_string("settings.toml").map_err(Failure::Read)?;
    Ok(())
}
