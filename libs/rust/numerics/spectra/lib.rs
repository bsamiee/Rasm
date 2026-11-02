#![feature(default_field_values, float_conversions, iterator_try_collect)]

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

#[nutype::nutype(validate(with = rate, error = SpectraError), derive(Debug, Clone, Copy, PartialEq, Deref))]
pub struct Rate(Frequency);

#[nutype::nutype(validate(with = finite, error = SpectraError), derive(Debug, Clone, PartialEq, Deref))]
pub struct Samples(Array1<f64>);

#[derive(Debug, Clone, PartialEq)]
pub struct Series {
    pub rate: Rate,
    pub samples: Samples,
}

#[nutype::nutype(validate(with = band, error = SpectraError), derive(Debug, Clone, Copy, PartialEq, Deref))]
pub struct Band(RangeInclusive<Frequency>);

#[nutype::nutype(validate(with = passband, error = SpectraError), derive(Debug, Clone, Copy, PartialEq, Deref))]
pub struct Passband(RangeInclusive<Frequency>);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum SpectraError {
    #[error("series holds no sample")]
    Empty,
    #[error("sample {index} is not finite")]
    Sample { index: usize },
    #[error("sample rate {} is no positive normal frequency", rate.into_format_args(hertz, Abbreviation))]
    Rate { rate: Frequency },
    #[error("band from {} to {} rises from no start at or above zero to a finite end", band.start.into_format_args(hertz, Abbreviation), band.last.into_format_args(hertz, Abbreviation))]
    Band { band: RangeInclusive<Frequency> },
    #[error("passband from {} to {} rises from no positive start to a finite end", band.start.into_format_args(hertz, Abbreviation), band.last.into_format_args(hertz, Abbreviation))]
    Passband { band: RangeInclusive<Frequency> },
    #[error("series at {} and {} differ in sample rate", rates[0].into_format_args(hertz, Abbreviation), rates[1].into_format_args(hertz, Abbreviation))]
    Rates { rates: [Rate; 2] },
    #[error("series of {} and {} samples differ in length", lengths[0], lengths[1])]
    Lengths { lengths: [usize; 2] },
    #[error("axis {axis} lies outside an array of {dimensions} dimensions")]
    Axis { axis: usize, dimensions: usize },
    #[error("axis {axis} holds the half spectrum and takes no complex transform")]
    Half { axis: usize },
    #[error("axis {axis} holds {found} bins where the signal length needs {expected}")]
    Bins { axis: usize, expected: usize, found: usize },
    #[error("a segment of {segment} samples padded by {padding} estimates no power over {samples} samples")]
    Segment { segment: NonZero<usize>, padding: usize, samples: usize },
    #[error("coherence at {} is undefined, a series holds no power there", frequency.into_format_args(hertz, Abbreviation))]
    Coherence { frequency: Frequency },
    #[error("padding of {} and {} samples around {samples} samples fits no {taps}-tap kernel or reaches past its border", sides[0], sides[1])]
    Boundary { sides: [usize; 2], samples: usize, taps: usize },
    #[error("stride of zero samples takes no output")]
    Stride,
    #[error("regularization {regularization} is negative or not finite")]
    Regularization { regularization: f64 },
    #[error("stage {stage}: {}", fmt::from_fn(|formatter| match (refusal, f0.into_format_args(hertz, Abbreviation)) {
        (Errors::OutsideNyquist, f0) => write!(formatter, "{f0} lies at or above the Nyquist frequency of a {} sample rate", rate.into_format_args(hertz, Abbreviation)),
        (Errors::NegativeFrequency, f0) => write!(formatter, "{f0} is no positive frequency"),
        (Errors::NegativeQ, f0) => write!(formatter, "Q at {f0} is not positive"),
    }))]
    Filter { stage: usize, f0: Frequency, rate: Rate, refusal: Errors },
    #[error("stage {stage} designs a section at {} whose coefficients are not finite", f0.into_format_args(hertz, Abbreviation))]
    Section { stage: usize, f0: Frequency },
    #[error(
        "stage {stage}: weighting at a {} sample rate deviates {deviation:.1} dB at {}, outside the IEC 61672-1 class 1 limits",
        rate.into_format_args(hertz, Abbreviation),
        frequency.into_format_args(hertz, Abbreviation)
    )]
    Weighting { stage: usize, frequency: Frequency, deviation: f64, rate: Rate },
    #[error("series holds no energy")]
    Silent,
    #[error("noise truncation finds no decay above the noise floor at iteration {iteration}")]
    Truncation { iteration: usize },
    #[error("early time {} is negative or not finite", early.into_format_args(second, Abbreviation))]
    Early { early: Time },
    #[error("early time {} leaves no energy on one side of the split", early.into_format_args(second, Abbreviation))]
    Split { early: Time },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn rate(rate: &Frequency) -> Result<(), SpectraError> { (rate.is_normal() && rate.is_sign_positive()).ok_or(SpectraError::Rate { rate: *rate }) }

fn finite(samples: &Array1<f64>) -> Result<(), SpectraError> {
    match samples.iter().position(|sample| !sample.is_finite()) {
        Some(index) => Err(SpectraError::Sample { index }),
        None if samples.is_empty() => Err(SpectraError::Empty),
        None => Ok(()),
    }
}

fn band(band: &RangeInclusive<Frequency>) -> Result<(), SpectraError> {
    (band.last.is_finite() && (0.0..band.last.get::<hertz>()).contains(&band.start.get::<hertz>())).ok_or(SpectraError::Band { band: *band })
}

fn passband(band: &RangeInclusive<Frequency>) -> Result<(), SpectraError> {
    (band.last.is_finite() && band.start.get::<hertz>() > 0.0 && band.start < band.last).ok_or(SpectraError::Passband { band: *band })
}

const fn real_bins(len: usize) -> usize { len.strict_div(2).strict_add(1) }

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod convolution;
pub mod decay;
pub mod filters;
pub mod power;
pub mod transforms;
