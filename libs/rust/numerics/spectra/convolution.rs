use core::iter;

use nalgebra::convert;
use ndarray_conv::{BorderType, ConvFFTExt as _, ConvMode, Error, PaddingMode, ReverseKernel as _};
use uom::si::{
    f64::{Frequency, Time},
    frequency::hertz,
    time::second,
};

use crate::{Rate, RateError, Samples, Series, SpectraError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Product {
    Convolution,
    Correlation,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy)]
pub struct Boundary {
    pub mode: ConvMode<1> = ConvMode::Full,
    pub padding: PaddingMode<1, f64> = PaddingMode::Zeros,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Lagged {
    pub lag: Time,
    pub series: Series,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn convolve(data: &Series, kernel: &Series, product: Product, boundary: Boundary) -> Result<Lagged, SpectraError> {
    let Boundary { mode, padding } = boundary;
    let (len, taps) = (data.samples.len(), kernel.samples.len());
    let refused = || SpectraError::Convolution(Error::MismatchShape(mode, [taps]));
    (data.rate == kernel.rate).ok_or(SpectraError::Rates { data: data.rate, kernel: kernel.rate })?;
    let ([front, back], stride) = match mode {
        ConvMode::Full => ([taps.strict_sub(1); 2], 1),
        ConvMode::Same => ([taps / 2, taps.strict_sub(1) / 2], 1),
        ConvMode::Valid => ([0; 2], 1),
        ConvMode::Custom { padding: [side], strides: [stride] } => ([side; 2], stride),
        ConvMode::Explicit { padding: [sides], strides: [stride] } => (sides, stride),
    };
    let borders = match padding {
        PaddingMode::Zeros => [BorderType::Zeros; 2],
        PaddingMode::Const(value) => [BorderType::Const(value); 2],
        PaddingMode::Reflect => [BorderType::Reflect; 2],
        PaddingMode::Replicate => [BorderType::Replicate; 2],
        PaddingMode::Circular => [BorderType::Circular; 2],
        PaddingMode::Custom([border]) => [border; 2],
        PaddingMode::Explicit([borders]) => borders,
    };
    let extends = iter::zip(borders, [front, back]).all(|(border, side)| match border {
        BorderType::Zeros | BorderType::Const(_) | BorderType::Replicate => true,
        BorderType::Reflect => side < len,
        BorderType::Circular => side <= len,
    });
    (extends && len.checked_add(front).and_then(|padded| padded.checked_add(back)).is_some()).ok_or_else(refused)?;
    let frequency = data.rate.get::<hertz>();
    let rate = Rate::try_new(Frequency::new::<hertz>(frequency / convert::<usize, f64>(stride))).map_err(|RateError::PredicateViolated| refused())?;
    let (oriented, reach) = match product {
        Product::Convolution => (kernel.samples.reverse(), taps.strict_sub(1)),
        Product::Correlation => (kernel.samples.no_reverse(), 0),
    };
    Ok(Lagged {
        lag: Time::new::<second>((convert::<usize, f64>(reach) - convert::<usize, f64>(front)) / frequency),
        series: Series { rate, samples: Samples::try_new(data.samples.conv_fft_par(oriented, mode, padding)?)? },
    })
}
