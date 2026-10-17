use core::num::ParseIntError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Exported {
    pub level: String,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
#[error("level is not a number")]
pub struct LevelError(#[from] ParseIntError);

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn exported(exported: &Exported) -> Result<u8, LevelError> { Ok(exported.level.parse()?) }
