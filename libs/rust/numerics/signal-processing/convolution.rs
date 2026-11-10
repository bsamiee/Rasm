use core::iter;
use core::num::NonZero;

use nalgebra::convert;
use ndarray::{Array1, Array2, Axis, Slice, Zip};
use ndarray_conv::{BorderType, ConvFFTExt as _, ConvMode, Error, PaddingMode, ReverseKernel as _};
use num_complex::Complex;

use crate::transforms::{signal, spectrum};
use crate::{Lagged, Rate, Samples, Series, SpectraError};

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

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn convolve(data: &Series, kernel: &Series, product: Product, boundary: Boundary) -> Result<Lagged, SpectraError> {
    let Boundary { mode, padding } = boundary;
    let (samples, taps) = (data.samples.len(), kernel.samples.len());
    (data.rate == kernel.rate).ok_or(SpectraError::Rates { rates: [data.rate, kernel.rate] })?;
    let ([front, back], stride) = match mode {
        ConvMode::Full => ([taps.strict_sub(1); 2], 1),
        ConvMode::Same => ([taps / 2, taps.strict_sub(1) / 2], 1),
        ConvMode::Valid => ([0; 2], 1),
        ConvMode::Custom { padding: [side], strides: [stride] } => ([side; 2], stride),
        ConvMode::Explicit { padding: [sides], strides: [stride] } => (sides, stride),
    };
    let stride = NonZero::new(stride).ok_or(SpectraError::Stride)?;
    let borders = match padding {
        PaddingMode::Zeros => [BorderType::Zeros; 2],
        PaddingMode::Const(value) => [BorderType::Const(value); 2],
        PaddingMode::Reflect => [BorderType::Reflect; 2],
        PaddingMode::Replicate => [BorderType::Replicate; 2],
        PaddingMode::Circular => [BorderType::Circular; 2],
        PaddingMode::Custom([border]) => [border; 2],
        PaddingMode::Explicit([borders]) => borders,
    };
    let refused = || SpectraError::Boundary { sides: [front, back], samples, taps };
    let extends = iter::zip(borders, [front, back]).all(|(border, side)| match border {
        BorderType::Zeros | BorderType::Const(_) | BorderType::Replicate => true,
        BorderType::Reflect => side < samples,
        BorderType::Circular => side <= samples,
    });
    (extends && samples.checked_add(front).and_then(|padded| padded.checked_add(back)).is_some()).ok_or_else(refused)?;
    let rate = Rate::try_new(*data.rate / convert::<usize, f64>(stride.get()))?;
    let (oriented, reach) = match product {
        Product::Convolution => (kernel.samples.reverse(), taps.strict_sub(1)),
        Product::Correlation => (kernel.samples.no_reverse(), 0),
    };
    let output = data.samples.conv_fft_par(oriented, mode, padding).map_err(|(Error::DataShape(_) | Error::KernelShape(_) | Error::MismatchShape(..))| refused())?;
    Ok(Lagged { lag: (convert::<usize, f64>(reach) - convert::<usize, f64>(front)) / *data.rate, series: Series { rate, samples: Samples::try_new(output)? } })
}

pub fn deconvolve(recording: &Series, excitation: &Series, regularization: f64) -> Result<Lagged, SpectraError> {
    let (samples, taps) = (recording.samples.len(), excitation.samples.len());
    (recording.rate == excitation.rate).ok_or(SpectraError::Rates { rates: [recording.rate, excitation.rate] })?;
    (regularization.is_finite() && regularization >= 0.0).ok_or(SpectraError::Regularization { regularization })?;
    let size = samples.strict_add(taps).strict_sub(1);
    let padded = Array2::from_shape_fn((2, size), |(row, index)| if row == 0 { recording } else { excitation }.samples.get(index).copied().unwrap_or_default());
    let spectra = spectrum(&padded, Axis(1), [])?;
    let (output, input) = (spectra.row(0), spectra.row(1));
    let floor = input.iter().map(Complex::norm_sqr).max_by(f64::total_cmp).filter(|peak| *peak > 0.0).ok_or(SpectraError::Silent)? * regularization;
    input
        .iter()
        .position(|excited| excited.norm_sqr() + floor <= 0.0)
        .map_or(Ok(()), |bin| Err(SpectraError::Power { frequency: *recording.rate * (convert::<usize, f64>(bin) / convert::<usize, f64>(size)) }))?;
    let quotient = Zip::from(&output).and(&input).map_collect(|recorded, excited| recorded * excited.conj() / (excited.norm_sqr() + floor));
    let response = signal(&quotient, Axis(0), size, [])?;
    let rotated = iter::chain(&response.slice_axis(Axis(0), Slice::from(samples..)), &response.slice_axis(Axis(0), Slice::from(..samples))).copied().collect::<Array1<f64>>();
    Ok(Lagged { lag: -convert::<usize, f64>(taps.strict_sub(1)) / *recording.rate, series: Series { rate: recording.rate, samples: Samples::try_new(rotated)? } })
}
