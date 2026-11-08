use core::{borrow::Borrow, iter, num::NonZero};

use apodize::{blackman_iter, cosine_iter, hamming_iter, hanning_iter, nuttall_iter, triangular_iter};
use nalgebra::convert;
use ndarray::{Array1, Array2, Axis, Slice, Zip};
use num_complex::Complex;
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use realfft::RealFftPlanner;
use uom::si::{
    f64::{Frequency, Time},
    frequency::hertz,
};

use crate::{
    Band, Rate, Samples, Series, SpectraError,
    transforms::{Transform, frequencies},
};

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

#[derive(Debug, Clone, PartialEq)]
pub struct Spectrum {
    pub frequencies: Array1<Frequency>,
    pub powers: Array1<f64>,
    pub scaling: Scaling,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Spectrogram {
    pub times: Array1<Time>,
    pub frequencies: Array1<Frequency>,
    pub powers: Array2<f64>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Transfer {
    pub coherence: Array1<f64>,
    pub response: Array1<Complex<f64>>,
    pub frequencies: Array1<Frequency>,
    pub cross: Array1<Complex<f64>>,
}

struct Frames<T, const N: usize> {
    values: [Array2<T>; N],
    factors: Array1<f64>,
    frequencies: Array1<Frequency>,
    hop: NonZero<usize>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn estimate(series: &Series, welch: Welch) -> Result<Spectrum, SpectraError> {
    frames(series.rate, [&series.samples], welch, Complex::norm_sqr).map(|Frames { values: [powers], factors, frequencies, hop: _ }| Spectrum {
        frequencies,
        powers: averaged(&powers, welch.average, &factors, |&power| power),
        scaling: welch.scaling,
    })
}

pub fn spectrogram(series: &Series, welch: Welch) -> Result<Spectrogram, SpectraError> {
    let Frames { values: [mut powers], factors, frequencies, hop } = frames(series.rate, [&series.samples], welch, Complex::norm_sqr)?;
    let centre = convert::<usize, f64>(welch.segment.get()) / 2.0;
    powers.zip_mut_with(&factors, |power, &factor| *power *= factor);
    Ok(Spectrogram { times: Array1::from_shape_fn(powers.nrows(), |frame| (convert::<usize, f64>(frame.strict_mul(hop.get())) + centre) / *series.rate), frequencies, powers })
}

pub fn transfer(input: &Series, output: &Series, welch: Welch) -> Result<Transfer, SpectraError> {
    (input.rate == output.rate).ok_or(SpectraError::Rates { rates: [input.rate, output.rate] })?;
    let Frames { values: [source, target], factors, frequencies, hop: _ } = frames(input.rate, [&input.samples, &output.samples], welch, |&bin| bin)?;
    let [inputs, outputs] = [&source, &target].map(|bins| averaged(bins, welch.average, &factors, Complex::norm_sqr));
    iter::zip(&frequencies, iter::zip(&inputs, &outputs)).find(|(_, (from, to))| **from <= 0.0 || **to <= 0.0).map_or(Ok(()), |(&frequency, _)| Err(SpectraError::Power { frequency }))?;
    let mut products = source;
    products.zip_mut_with(&target, |from, to| *from = from.conj() * to);
    drop(target);
    let cross = Zip::from(&averaged(&products, welch.average, &factors, |product| product.re))
        .and(&averaged(&products, welch.average, &factors, |product| product.im))
        .map_collect(|&re, &im| Complex::new(re, im));
    drop(products);
    Ok(Transfer {
        coherence: Zip::from(&cross).and(&inputs).and(&outputs).map_collect(|product, &from, &to| product.norm_sqr() / (from * to)),
        response: Zip::from(&cross).and(&inputs).map_collect(|&product, &from| product / from),
        frequencies,
        cross,
    })
}

#[must_use]
pub fn banded<I>(spectrum: &Spectrum, bands: I) -> Array1<f64>
where I: IntoIterator<Item: Borrow<Band>> {
    let width = match spectrum.scaling {
        Scaling::Density => spectrum.frequencies.get(1).map_or_default(Frequency::get::<hertz>),
        Scaling::Spectrum => 1.0,
    };
    bands.into_iter().map(|band| iter::zip(&spectrum.frequencies, &spectrum.powers).filter(|(frequency, _)| band.borrow().contains(*frequency)).map(|(_, power)| power).sum::<f64>() * width).collect()
}

fn frames<T, const N: usize>(rate: Rate, series: [&Samples; N], welch: Welch, value: impl Fn(&Complex<f64>) -> T + Sync) -> Result<Frames<T, N>, SpectraError>
where T: Default + Clone + Send {
    let Welch { segment, hop, padding, taper, detrend, scaling, average: _ } = welch;
    let (length, samples) = (segment.get(), series.iter().map(|samples| samples.len()).min().unwrap_or_default());
    let transform = segment.checked_add(padding).filter(|_| (2..=samples).contains(&length)).ok_or(SpectraError::Segment { segment, padding, samples })?;
    let periodic = length.strict_add(1);
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
    let ramp = Array1::linspace(-(width - 1.0) / 2.0, (width - 1.0) / 2.0, length);
    let (spread, handler) = (ramp.dot(&ramp), RealFftPlanner::<f64>::new().plan_fft_forward(transform.get()));
    let bins = handler.complex_len();
    let values = series.try_map(|series| {
        let input = series.slice_axis(Axis(0), Slice::from(..samples));
        let mut values = Array2::default((count, bins));
        Zip::from(values.rows_mut())
            .and(input.windows_with_stride(length, hop.get()))
            .into_par_iter()
            .try_for_each_init(
                || (handler.make_input_vec(), handler.make_output_vec(), handler.make_scratch_vec()),
                |(frame, spectrum, scratch), (row, window)| {
                    let (offset, slope) = match detrend {
                        None => (0.0, 0.0),
                        Some(Trend::Constant) => (window.sum() / width, 0.0),
                        Some(Trend::Linear) => (window.sum() / width, window.dot(&ramp) / spread),
                    };
                    frame[length..].fill(0.0);
                    Zip::from(&mut frame[..length]).and(&window).and(&weights).and(&ramp).for_each(|tapered, &sample, &weight, &position| {
                        *tapered = slope.mul_add(-position, sample - offset) * weight;
                    });
                    handler.process_with_scratch(frame, spectrum, scratch).map(|()| Zip::from(row).and(spectrum.as_slice()).for_each(|slot, bin| *slot = value(bin)))
                },
            )
            .map(|()| values)
    })?;
    let scale = match scaling {
        Scaling::Density => rate.get::<hertz>() * weights.dot(&weights),
        Scaling::Spectrum => weights.sum().powi(2),
    };
    let sides = transform.get().div_ceil(2);
    Ok(Frames {
        values,
        factors: Array1::from_shape_fn(bins, |bin| if (1..sides).contains(&bin) { 2.0 / scale } else { scale.recip() }),
        frequencies: frequencies(rate, transform, Transform::Real),
        hop,
    })
}

fn averaged<T>(values: &Array2<T>, average: Average, factors: &Array1<f64>, project: impl Fn(&T) -> f64 + Sync) -> Array1<f64>
where T: Sync {
    let count = values.nrows();
    let lanes = Zip::from(values.columns()).and(factors);
    match average {
        Average::Mean => lanes.par_map_collect(|lane, &factor| factor * (lane.fold(0.0, |sum, entry| sum + project(entry)) / convert::<usize, f64>(count))),
        Average::Median => {
            let bias = (1..=count.strict_sub(1).strict_div(2)).map(|term| convert::<usize, f64>(term.strict_mul(2))).map(|even| (even + 1.0).recip() - even.recip()).sum::<f64>() + 1.0;
            let mut powers = Array1::zeros(factors.len());
            lanes.and(&mut powers).into_par_iter().for_each_init(
                || Vec::with_capacity(count),
                |ordered, (lane, &factor, power)| {
                    ordered.clear();
                    ordered.extend(lane.iter().map(&project));
                    let (lower, &mut upper, _) = ordered.select_nth_unstable_by(count.strict_div(2), f64::total_cmp);
                    let median = count.is_multiple_of(2).then_some(lower).and_then(|lower| lower.iter().copied().max_by(f64::total_cmp)).map_or(upper, |below| below.midpoint(upper));
                    *power = factor * (median / bias);
                },
            );
            powers
        }
    }
}
