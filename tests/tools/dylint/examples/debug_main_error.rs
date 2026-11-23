use std::{fs, io};

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Failure {
    #[error("cannot read the settings file")]
    Read(#[source] io::Error),
}

// --- [COMPOSITION] ---------------------------------------------------------------------

//~v debug_main_error
fn main() -> Result<(), Failure> {
    fs::read_to_string("settings.toml").map_err(Failure::Read)?;
    Ok(())
}
