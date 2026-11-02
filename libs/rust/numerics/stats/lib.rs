use core::{num::NonZero, range::RangeInclusive};

use datasketches::error::Error;
use nonempty_collections::NEBTreeMap;
use nuts_rs::{LogpError, NutsError};
use statrs::distribution::StudentsTError;

// --- [TYPES] ---------------------------------------------------------------------------

pub type Space<K> = NEBTreeMap<K, Bounds>;

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Moment {
    Mean,
    Variance,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Sample {
    #[display("resample estimates are not finite")]
    Resamples,
    #[display("resample estimates fall on one side of the full-sample estimate")]
    Bias,
    #[display("groups all hold a non-finite output")]
    Groups,
    #[display("draws per chain split into halves under two")]
    Draws,
}

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

#[derive(Debug, thiserror::Error, derive_more::IsVariant)]
pub enum StatsError {
    #[error("log density or its gradient is not finite")]
    NotFinite,
    #[error("chain {chain} failed")]
    Sampler { chain: u64, source: Option<NutsError> },
    #[error("start of chain {chain} lies outside the open bounds of dimension {dimension}")]
    Start { chain: u64, dimension: usize },
    #[error("sampler settings refused")]
    Settings,
    #[error("{found} entries along an axis of {expected}")]
    Shape { expected: usize, found: usize },
    #[error("{levels} levels over {dimensions} dimensions overflow a factorial design")]
    Factorial { levels: NonZero<u16>, dimensions: NonZero<usize> },
    #[error("{found} observations admit no interval")]
    Observations { found: u64, source: StudentsTError },
    #[error("{found} finite pairs grade no fit of {parameters} parameters")]
    Pairs { found: u64, parameters: u32 },
    #[error("{found} {sample}")]
    Degenerate { found: usize, sample: Sample },
    #[error("{0} is zero")]
    Zero(Moment),
    #[error(transparent)]
    Sketch(#[from] Error),
}

impl LogpError for StatsError {
    fn is_recoverable(&self) -> bool { self.is_not_finite() }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod calibration;
pub mod descriptive;
pub mod inference;
pub mod sampling;
pub mod sensitivity;
pub mod sketches;
