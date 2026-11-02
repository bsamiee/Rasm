#![feature(iterator_try_collect)]

use core::range::RangeInclusive;

use datasketches::error::Error;
use nonempty_collections::NEBTreeMap;
use nuts_rs::{LogpError, NutsError};
use statrs::distribution::StudentsTError;

// --- [TYPES] ---------------------------------------------------------------------------

pub type Space<K> = NEBTreeMap<K, Bounds>;

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(
    validate(predicate = |bounds| {
        let span = bounds.last - bounds.start;
        span.is_finite() && span > 0.0_f64
    }),
    derive(Debug, Clone, Copy, PartialEq, Deref)
)]
pub struct Bounds(RangeInclusive<f64>);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum StatsError {
    #[error("log density or its gradient is not finite")]
    NotFinite,
    #[error("chain {chain} failed")]
    Sampler { chain: u64, source: NutsError },
    #[error("sampler settings refused")]
    Settings,
    #[error("{found} outputs for a design of {expected} points")]
    Outputs { expected: usize, found: usize },
    #[error(transparent)]
    Distribution(#[from] StudentsTError),
    #[error(transparent)]
    Sketch(#[from] Error),
}

impl LogpError for StatsError {
    fn is_recoverable(&self) -> bool {
        match self {
            Self::NotFinite => true,
            Self::Sampler { chain: _, source: _ } | Self::Settings | Self::Outputs { expected: _, found: _ } | Self::Distribution(_) | Self::Sketch(_) => false,
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod calibration;
pub mod descriptive;
pub mod inference;
pub mod sampling;
pub mod sensitivity;
pub mod sketches;
