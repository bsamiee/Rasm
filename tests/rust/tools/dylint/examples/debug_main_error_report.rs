#![feature(error_reporter)]

use std::{error::Report, fs, io};

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Failure {
    #[error("cannot read the settings file")]
    Read(#[source] io::Error),
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> Result<(), Report<Failure>> {
    fs::read_to_string("settings.toml").map_err(Failure::Read)?;
    Ok(())
}
