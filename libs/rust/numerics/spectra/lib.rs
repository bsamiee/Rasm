#![feature(default_field_values, iterator_try_collect)]

use core::{fmt, num::NonZero, range::RangeInclusive};

use biquad::Errors;
use ndarray::Array1;
use uom::{
    fmt::DisplayStyle::Abbreviation,
    si::{
        f64::{Frequency, Time},
        frequency::hertz,
        time::second,
    },
};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(predicate = |rate: &Frequency| rate.is_normal() && rate.is_sign_positive()), derive(Debug, Clone, Copy, PartialEq, Deref))]
pub struct Rate(Frequency);

#[nutype::nutype(validate(with = finite, error = SpectraError), derive(Debug, Clone, PartialEq, Deref))]
pub struct Samples(Array1<f64>);

#[derive(Debug, Clone, PartialEq)]
pub struct Series {
    pub rate: Rate,
    pub samples: Samples,
}

#[nutype::nutype(
    validate(predicate = |band: &RangeInclusive<Frequency>| band.last.is_finite() && (0.0..band.last.get::<hertz>()).contains(&band.start.get::<hertz>())),
    derive(Debug, Clone, Copy, PartialEq, Deref)
)]
pub struct Band(RangeInclusive<Frequency>);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum SpectraError {
    #[error("series holds no sample")]
    Empty,
    #[error("sample {index} is not finite")]
    Sample { index: usize },
    #[error("axis {axis} lies outside an array of {dimensions} dimensions")]
    Axis { axis: usize, dimensions: usize },
    #[error("axis {axis} holds {found} bins where the signal length needs {expected}")]
    Bins { axis: usize, expected: usize, found: usize },
    #[error("a segment of {segment} samples estimates no power over {samples} samples")]
    Segment { segment: NonZero<usize>, samples: usize },
    #[error("data at {} and kernel at {} differ in sample rate", data.into_format_args(hertz, Abbreviation), kernel.into_format_args(hertz, Abbreviation))]
    Rates { data: Rate, kernel: Rate },
    #[error("stage {stage}: {}", fmt::from_fn(|formatter| match (refusal, f0.into_format_args(hertz, Abbreviation)) {
        (Errors::OutsideNyquist, f0) => write!(formatter, "{f0} lies at or above the Nyquist frequency of a {} sample rate", rate.into_format_args(hertz, Abbreviation)),
        (Errors::NegativeFrequency, f0) => write!(formatter, "{f0} is no positive frequency"),
        (Errors::NegativeQ, f0) => write!(formatter, "Q at {f0} is not positive"),
    }))]
    Filter { stage: usize, f0: Frequency, rate: Rate, refusal: Errors },
    #[error("stage {stage} designs a section at {} whose coefficients are not finite", f0.into_format_args(hertz, Abbreviation))]
    Section { stage: usize, f0: Frequency },
    #[error("impulse response holds no energy")]
    Silent,
    #[error("early time {} leaves no energy on one side of the split", early.into_format_args(second, Abbreviation))]
    Split { early: Time },
    #[error(transparent)]
    Convolution(#[from] ndarray_conv::Error<1>),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn finite(samples: &Array1<f64>) -> Result<(), SpectraError> {
    match samples.iter().position(|sample| !sample.is_finite()) {
        Some(index) => Err(SpectraError::Sample { index }),
        None if samples.is_empty() => Err(SpectraError::Empty),
        None => Ok(()),
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod convolution;
pub mod decay;
pub mod filters;
pub mod power;
pub mod transforms;
