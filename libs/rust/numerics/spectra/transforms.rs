use core::{convert::identity, f64::consts::PI, iter, num::NonZero};

use nalgebra::convert;
use ndarray::{Array, Array1, ArrayBase, ArrayViewMut1, Axis, CowArray, Data, Dimension};
use ndrustfft::{DctHandler, FftHandler, Normalization, R2cFftHandler, nddct2_par, nddct3_par, ndfft_par, ndfft_r2c_par, ndifft_par, ndifft_r2c_par};
use rustfft::{FftDirection, FftPlanner, num_complex::Complex};
use uom::si::{f64::Frequency, frequency::hertz};

use crate::{Band, Rate, Series, SpectraError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Bins {
    Half,
    Full,
    Cosine,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PLANES]

pub fn fourier<S, D>(input: &ArrayBase<S, D>, axes: impl IntoIterator<Item = Axis>, direction: FftDirection) -> Result<Array<Complex<f64>, D>, SpectraError>
where
    S: Data<Elem = Complex<f64>>,
    D: Dimension,
{
    axes.into_iter()
        .try_fold(CowArray::from(input), |current, axis| {
            shaped(current.raw_dim(), axis, identity).map(|(len, shape)| {
                let (handler, mut output) = (FftHandler::new(len), Array::zeros(shape));
                match direction {
                    FftDirection::Forward => ndfft_par(&current, &mut output, &handler, axis.index()),
                    FftDirection::Inverse => ndifft_par(&current, &mut output, &handler, axis.index()),
                }
                CowArray::from(output)
            })
        })
        .map(ArrayBase::into_owned)
}

pub fn spectrum<S, D>(input: &ArrayBase<S, D>, half: Axis, axes: impl IntoIterator<Item = Axis>) -> Result<Array<Complex<f64>, D>, SpectraError>
where
    S: Data<Elem = f64>,
    D: Dimension,
{
    let (len, shape) = shaped(input.raw_dim(), half, |count| count.strict_div(2).strict_add(1))?;
    let mut output = Array::zeros(shape);
    ndfft_r2c_par(input, &mut output, &R2cFftHandler::new(len), half.index());
    fourier(&output, axes, FftDirection::Forward)
}

pub fn signal<S, D>(input: &ArrayBase<S, D>, half: Axis, len: usize, axes: impl IntoIterator<Item = Axis>) -> Result<Array<f64, D>, SpectraError>
where
    S: Data<Elem = Complex<f64>>,
    D: Dimension,
{
    let (found, shape) = shaped(input.raw_dim(), half, |_| len)?;
    let expected = len.strict_div(2).strict_add(1);
    (found == expected).ok_or(SpectraError::Bins { axis: half.index(), expected, found })?;
    let bins = fourier(input, axes, FftDirection::Inverse)?;
    let mut output = Array::zeros(shape);
    ndifft_r2c_par(&bins, &mut output, &R2cFftHandler::new(len), half.index());
    Ok(output)
}

pub fn cosine<S, D>(input: &ArrayBase<S, D>, axes: impl IntoIterator<Item = Axis>, direction: FftDirection) -> Result<Array<f64, D>, SpectraError>
where
    S: Data<Elem = f64>,
    D: Dimension,
{
    axes.into_iter()
        .try_fold(CowArray::from(input), |current, axis| {
            shaped(current.raw_dim(), axis, identity).map(|(len, shape)| match (len, direction) {
                (0, _) => current,
                (1, FftDirection::Forward) => CowArray::from(current.mapv(|sample| 2.0 * sample)),
                (1, FftDirection::Inverse) => CowArray::from(current.mapv(|sample| sample / 2.0)),
                (2.., _) => {
                    let (handler, mut output) = (DctHandler::new(len), Array::zeros(shape));
                    match direction {
                        FftDirection::Forward => nddct2_par(&current, &mut output, &handler, axis.index()),
                        FftDirection::Inverse => nddct3_par(
                            &current,
                            &mut output,
                            &handler.normalization(Normalization::Custom(|lane: &mut [f64]| {
                                let count = convert::<usize, f64>(lane.len());
                                ArrayViewMut1::from(lane).mapv_inplace(|sample| sample / count);
                            })),
                            axis.index(),
                        ),
                    }
                    CowArray::from(output)
                }
            })
        })
        .map(ArrayBase::into_owned)
}

fn shaped<D>(mut shape: D, Axis(axis): Axis, len: impl FnOnce(usize) -> usize) -> Result<(usize, D), SpectraError>
where D: Dimension {
    let dimensions = shape.ndim();
    let slot = shape.slice_mut().get_mut(axis).ok_or(SpectraError::Axis { axis, dimensions })?;
    let found = *slot;
    *slot = len(found);
    Ok((found, shape))
}

// --- [SERIES]

#[must_use]
pub fn zoom(series: &Series, band: Band, bins: NonZero<usize>) -> Array1<Complex<f64>> {
    let [rate, start, last] = [series.rate.get::<hertz>(), band.start.get::<hertz>(), band.last.get::<hertz>()];
    let step = NonZero::new(bins.get().strict_sub(1)).map_or_default(|intervals| (last - start) / convert::<usize, f64>(intervals.get()));
    let len = series.samples.len().strict_add(bins.get()).strict_sub(1);
    let chirp = |index: usize| Complex::cis(PI * step / rate * convert::<usize, f64>(index).powi(2));
    let mut planner = FftPlanner::new();
    let [forward, inverse] = [FftDirection::Forward, FftDirection::Inverse].map(|direction| planner.plan_fft(len, direction));
    let mut scratch = vec![Complex::ZERO; forward.get_inplace_scratch_len().max(inverse.get_inplace_scratch_len())];
    let mut weighted = Vec::from_fn(len, |index| {
        series.samples.get(index).map_or(Complex::ZERO, |&sample| {
            let position = convert::<usize, f64>(index);
            Complex::cis(-PI / rate * position * step.mul_add(position, 2.0 * start)).scale(sample)
        })
    });
    let mut kernel = Vec::from_fn(len, |index| chirp(if index < bins.get() { index } else { len.strict_sub(index) }));
    forward.process_with_scratch(&mut weighted, &mut scratch);
    forward.process_with_scratch(&mut kernel, &mut scratch);
    iter::zip(&mut weighted, &kernel).for_each(|(value, &weight)| *value *= weight);
    inverse.process_with_scratch(&mut weighted, &mut scratch);
    let scale = convert::<usize, f64>(len);
    weighted.into_iter().take(bins.get()).enumerate().map(|(index, value)| chirp(index).conj() * value / scale).collect()
}

#[must_use]
pub fn frequencies(rate: Rate, len: NonZero<usize>, bins: Bins) -> Array1<Frequency> {
    let (rate, count) = (rate.get::<hertz>(), convert::<usize, f64>(len.get()));
    let (size, spacing, wrap) = match bins {
        Bins::Half => (len.get().strict_div(2).strict_add(1), rate / count, len.get()),
        Bins::Full => (len.get(), rate / count, len.get().div_ceil(2)),
        Bins::Cosine => (len.get(), rate / (2.0 * count), len.get()),
    };
    Array1::from_shape_fn(size, |bin| Frequency::new::<hertz>(spacing * (convert::<usize, f64>(bin) - if bin < wrap { 0.0 } else { count })))
}
