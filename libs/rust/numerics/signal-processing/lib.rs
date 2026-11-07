#![feature(array_try_map, control_flow_into_value, default_field_values, deref_patterns, float_conversions)]

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

#[nutype::nutype(
    validate(with = |rate: &Frequency| -> Result<(), SpectraError> { (rate.is_normal() && rate.is_sign_positive()).ok_or(SpectraError::Rate { rate: *rate }) }, error = SpectraError),
    derive(Debug, Clone, Copy, PartialEq, Deref)
)]
pub struct Rate(Frequency);

#[nutype::nutype(validate(with = finite, error = SpectraError), derive(Debug, Clone, PartialEq, Deref))]
pub struct Samples(Array1<f64>);

#[derive(Debug, Clone, PartialEq)]
pub struct Series {
    pub rate: Rate,
    pub samples: Samples,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Lagged {
    pub lag: Time,
    pub series: Series,
}

#[nutype::nutype(
    validate(
        with = |band: &RangeInclusive<Frequency>| -> Result<(), SpectraError> {
            (band.start.is_finite() && band.last.is_finite() && band.start < band.last).ok_or(SpectraError::Band { band: *band })
        },
        error = SpectraError
    ),
    derive(Debug, Clone, Copy, PartialEq, Deref)
)]
pub struct Band(RangeInclusive<Frequency>);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum SpectraError {
    #[error(transparent)]
    Fft(#[from] realfft::FftError),
    #[error("series holds no sample")]
    Empty,
    #[error("sample {index} is not finite")]
    Sample { index: usize },
    #[error("sample rate {} is no positive normal frequency", rate.into_format_args(hertz, Abbreviation))]
    Rate { rate: Frequency },
    #[error("band from {} to {} {}", band.start.into_format_args(hertz, Abbreviation), band.last.into_format_args(hertz, Abbreviation), match band {
        RangeInclusive { start, last } if !start.is_finite() || !last.is_finite() => "is not finite",
        RangeInclusive { start, last } if start >= last => "does not rise",
        RangeInclusive { start: _, last: _ } => "starts at no positive frequency",
    })]
    Band { band: RangeInclusive<Frequency> },
    #[error("series at {} and {} differ in sample rate", rates[0].into_format_args(hertz, Abbreviation), rates[1].into_format_args(hertz, Abbreviation))]
    Rates { rates: [Rate; 2] },
    #[error("axis {axis} lies outside an array of {dimensions} dimensions")]
    Axis { axis: usize, dimensions: usize },
    #[error("axis {axis} holds the half spectrum and takes no complex transform")]
    Half { axis: usize },
    #[error("axis {axis} holds {found} bins where the signal length needs {expected}")]
    Bins { axis: usize, expected: usize, found: usize },
    #[error("a segment of {segment} samples padded by {padding} estimates no power over {samples} samples")]
    Segment { segment: NonZero<usize>, padding: usize, samples: usize },
    #[error("series holds no power at {}", frequency.into_format_args(hertz, Abbreviation))]
    Power { frequency: Frequency },
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
    #[error("series holds no energy")]
    Silent,
    #[error("noise truncation {} is not positive and finite", fmt::from_fn(|formatter| {
        decay::refused(lundeby).enumerate().try_for_each(|(index, (setting, value, unit))| write!(formatter, "{}{setting} {value} {unit}", if index == 0 { "" } else { ", " }))
    }))]
    Lundeby { lundeby: Lundeby },
    #[error("noise truncation finds no decay above the noise floor at iteration {iteration}")]
    Truncation { iteration: usize },
    #[error(
        "early time {} {}",
        early.into_format_args(second, Abbreviation),
        if early.is_finite() && early.is_sign_positive() { "leaves no energy on one side of the split" } else { "is negative or not finite" }
    )]
    Split { early: Time },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn finite(samples: &Array1<f64>) -> Result<(), SpectraError> {
    match samples.iter().position(|sample| !sample.is_finite()) {
        Some(index) => Err(SpectraError::Sample { index }),
        None if samples.is_empty() => Err(SpectraError::Empty),
        None => Ok(()),
    }
}

const fn real_bins(len: usize) -> usize { len.strict_div(2).strict_add(1) }

// --- [COMPOSITION] ---------------------------------------------------------------------

mod convolution;
mod decay;
mod filters;
mod power;
mod transforms;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use convolution::{Boundary, Product, convolve, deconvolve};
pub use decay::{Decay, Lateral, Lundeby, Reverberation, centre, clarity, definition, interaural, lateral, reverberation, strength};
pub use filters::{Direction, Filtered, Pass, Stage, Weighting, bands, bank, filter, response};
pub use power::{Average, Scaling, Spectrogram, Spectrum, Taper, Transfer, Trend, Welch, banded, estimate, spectrogram, transfer};
pub use transforms::{Transform, analytic, cosine, fourier, frequencies, resample, signal, spectrum, zoom};
