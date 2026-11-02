use core::num::NonZero;

use apodize::{blackman_iter, hamming_iter, hanning_iter, nuttall_iter, triangular_iter};
use nalgebra::convert;
use ndarray::{Array1, Array2, Axis, Zip, s};
use rustfft::num_complex::Complex;
use uom::si::frequency::hertz;

use crate::{Series, SpectraError, transforms::spectrum};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Taper {
    Rectangular,
    Triangular,
    Hann,
    Hamming,
    BlackmanHarris,
    Nuttall,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Scaling {
    Density,
    Spectrum,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Welch {
    pub segment: NonZero<usize>,
    pub hop: Option<NonZero<usize>> = None,
    pub taper: Taper = Taper::Hann,
    pub scaling: Scaling = Scaling::Density,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn estimate(series: &Series, welch: Welch) -> Result<Array1<f64>, SpectraError> {
    let Welch { segment, hop, taper, scaling } = welch;
    let (length, samples) = (segment.get(), series.samples.len());
    (2..=samples).contains(&length).ok_or(SpectraError::Segment { segment, samples })?;
    let hop = hop.unwrap_or(segment.div_ceil(const { NonZero::new(2).unwrap() }));
    let periodic = length.strict_add(1);
    let weights = match taper {
        Taper::Rectangular => Array1::ones(periodic),
        Taper::Triangular => Array1::from_iter(triangular_iter(periodic)),
        Taper::Hann => Array1::from_iter(hanning_iter(periodic)),
        Taper::Hamming => Array1::from_iter(hamming_iter(periodic)),
        Taper::BlackmanHarris => Array1::from_iter(blackman_iter(periodic)),
        Taper::Nuttall => Array1::from_iter(nuttall_iter(periodic)),
    }
    .slice_move(s![..length]);
    let count = samples.strict_sub(length).strict_div(hop.get()).strict_add(1);
    let mut frames = Array2::zeros((count, length));
    let width = convert::<usize, f64>(length);
    Zip::from(frames.rows_mut()).and(series.samples.windows_with_stride(length, hop.get())).par_for_each(|frame, window| {
        let mean = window.sum() / width;
        Zip::from(frame).and(&window).and(&weights).for_each(|value, &sample, &weight| *value = (sample - mean) * weight);
    });
    let power = spectrum(&frames, Axis(1), [])?.map(Complex::norm_sqr).sum_axis(Axis(0));
    let norm = convert::<usize, f64>(count)
        * match scaling {
            Scaling::Density => series.rate.get::<hertz>() * weights.dot(&weights),
            Scaling::Spectrum => weights.sum().powi(2),
        };
    let sides = length.div_ceil(2);
    Ok(Zip::indexed(&power).map_collect(|bin, &value| if (1..sides).contains(&bin) { 2.0 * value / norm } else { value / norm }))
}
