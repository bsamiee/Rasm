#![feature(default_field_values)]

use core::{fmt, num::NonZero, range::RangeInclusive};

use biquad::Errors;
use ndarray::Array1;
use uom::si::{
    f64::{Frequency, Time},
    frequency::hertz,
    time::second,
};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(predicate = |rate: &Frequency| rate.is_finite() && rate.get::<hertz>() > 0.0), derive(Debug, Clone, Copy, PartialEq, Deref))]
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
    #[error("data at {} Hz and kernel at {} Hz differ in sample rate", data.get::<hertz>(), kernel.get::<hertz>())]
    Rates { data: Rate, kernel: Rate },
    #[error("stage {stage}: {}", fmt::from_fn(|formatter| match refusal {
        Errors::OutsideNyquist => write!(formatter, "{} Hz lies at or above the Nyquist frequency of {} Hz", f0.get::<hertz>(), rate.get::<hertz>() / 2.0),
        Errors::NegativeFrequency => write!(formatter, "{} Hz is no positive frequency", f0.get::<hertz>()),
        Errors::NegativeQ => write!(formatter, "Q at {} Hz is not positive", f0.get::<hertz>()),
    }))]
    Filter { stage: usize, f0: Frequency, rate: Rate, refusal: Errors },
    #[error("stage {stage} designs a section at {} Hz whose coefficients are not finite", f0.get::<hertz>())]
    Section { stage: usize, f0: Frequency },
    #[error("impulse response holds no energy")]
    Silent,
    #[error("early time {} s leaves no energy on one side of the split", early.get::<second>())]
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
