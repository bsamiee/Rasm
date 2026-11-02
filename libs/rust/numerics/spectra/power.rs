use core::num::NonZero;

use apodize::{blackman_iter, cosine_iter, hamming_iter, hanning_iter, nuttall_iter, triangular_iter};
use nalgebra::convert;
use ndarray::{Array1, Array2, Axis, Slice, Zip};
use ndrustfft::{R2cFftHandler, ndfft_r2c};
use num_complex::Complex;
use uom::si::frequency::hertz;

use crate::{Series, SpectraError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Taper {
    Rectangular,
    Triangular,
    Hann,
    Hamming,
    BlackmanHarris,
    Nuttall,
    FlatTop,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Trend {
    Constant,
    Linear,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Scaling {
    Density,
    Spectrum,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Average {
    Mean,
    Median,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Welch {
    pub segment: NonZero<usize>,
    pub hop: Option<NonZero<usize>> = None,
    pub padding: usize = 0,
    pub taper: Taper = Taper::Hann,
    pub detrend: Option<Trend> = Some(Trend::Constant),
    pub scaling: Scaling = Scaling::Density,
    pub average: Average = Average::Mean,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn estimate(series: &Series, welch: Welch) -> Result<Array1<f64>, SpectraError> {
    let Welch { segment, hop, padding, taper, detrend, scaling, average } = welch;
    let (length, samples) = (segment.get(), series.samples.len());
    (2..=samples).contains(&length).ok_or(SpectraError::Segment { segment, samples })?;
    let (periodic, transform) = (length.strict_add(1), segment.saturating_add(padding));
    let (weights, overlap) = match taper {
        Taper::Rectangular => (Array1::ones(periodic), const { NonZero::new(1).unwrap() }),
        Taper::Triangular => (Array1::from_iter(triangular_iter(periodic)), const { NonZero::new(2).unwrap() }),
        Taper::Hann => (Array1::from_iter(hanning_iter(periodic)), const { NonZero::new(2).unwrap() }),
        Taper::Hamming => (Array1::from_iter(hamming_iter(periodic)), const { NonZero::new(2).unwrap() }),
        Taper::BlackmanHarris => (Array1::from_iter(blackman_iter(periodic)), const { NonZero::new(3).unwrap() }),
        Taper::Nuttall => (Array1::from_iter(nuttall_iter(periodic)), const { NonZero::new(3).unwrap() }),
        Taper::FlatTop => (Array1::from_iter(cosine_iter(0.217_06, 0.421_03, 0.282_94, 0.078_97, periodic)), const { NonZero::new(4).unwrap() }),
    };
    let weights = weights.slice_axis_move(Axis(0), Slice::from(..length));
    let hop = hop.unwrap_or_else(|| segment.div_ceil(overlap));
    let count = samples.strict_sub(length).strict_div(hop.get()).strict_add(1);
    let width = convert::<usize, f64>(length);
    let ramp = Array1::from_shape_fn(length, |index| convert::<usize, f64>(index) - (width - 1.0) / 2.0);
    let (spread, handler) = (ramp.dot(&ramp), R2cFftHandler::<f64>::new(transform.get()));
    let mut periodograms = Array2::zeros((count, transform.get().strict_div(2).strict_add(1)));
    Zip::from(periodograms.rows_mut()).and(series.samples.windows_with_stride(length, hop.get())).par_for_each(|periodogram, window| {
        let (offset, slope) = match detrend {
            None => (0.0, 0.0),
            Some(Trend::Constant) => (window.sum() / width, 0.0),
            Some(Trend::Linear) => (window.sum() / width, window.dot(&ramp) / spread),
        };
        let mut frame = Array1::zeros(transform.get());
        Zip::from(frame.slice_axis_mut(Axis(0), Slice::from(..length))).and(&window).and(&weights).and(&ramp).for_each(|value, &sample, &weight, &position| {
            *value = slope.mul_add(-position, sample - offset) * weight;
        });
        let mut bins = Array1::<Complex<f64>>::zeros(periodogram.len());
        ndfft_r2c(&frame, &mut bins, &handler, 0);
        Zip::from(periodogram).and(&bins).for_each(|power, bin| *power = bin.norm_sqr());
    });
    let (averaged, divisor) = match average {
        Average::Mean => (periodograms.sum_axis(Axis(0)), convert::<usize, f64>(count)),
        Average::Median => (
            Zip::from(periodograms.columns()).par_map_collect(|lane| {
                let mut values = lane.to_vec();
                let (lower, &mut upper, _) = values.select_nth_unstable_by(count.strict_div(2), f64::total_cmp);
                count.is_multiple_of(2).then_some(lower).and_then(|lower| lower.iter().copied().max_by(f64::total_cmp)).map_or(upper, |below| below.midpoint(upper))
            }),
            (1..=count.strict_sub(1).strict_div(2)).map(|term| convert::<usize, f64>(term.strict_mul(2))).map(|even| (even + 1.0).recip() - even.recip()).sum::<f64>() + 1.0,
        ),
    };
    let scale = divisor
        * match scaling {
            Scaling::Density => series.rate.get::<hertz>() * weights.dot(&weights),
            Scaling::Spectrum => weights.sum().powi(2),
        };
    let sides = transform.get().div_ceil(2);
    Ok(Zip::indexed(&averaged).map_collect(|bin, &value| if (1..sides).contains(&bin) { 2.0 * value / scale } else { value / scale }))
}
