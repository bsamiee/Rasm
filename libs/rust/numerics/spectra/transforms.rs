use core::{f64::consts::TAU, iter, num::NonZero};

use nalgebra::convert;
use ndarray::{Array, Array1, ArrayBase, ArrayViewMut1, Axis, CowArray, Data, Dimension, Zip};
use ndrustfft::{DctHandler, FftHandler, Normalization, R2cFftHandler, nddct2_par, nddct3_par, ndfft_par, ndfft_r2c_par, ndifft_par, ndifft_r2c_par};
use rustfft::{FftDirection, FftPlanner, num_complex::Complex};
use uom::si::{f64::Frequency, frequency::hertz};

use crate::{Band, Rate, SpectraError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Bins {
    Half,
    Full,
    Cosine,
    Zoom(Band),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PLANES]

pub fn fourier<S, D>(input: &ArrayBase<S, D>, axes: impl IntoIterator<Item = Axis>, direction: FftDirection) -> Result<Array<Complex<f64>, D>, SpectraError>
where
    S: Data<Elem = Complex<f64>>,
    D: Dimension,
{
    checked(input.ndim(), axes).map(|axes| complex(CowArray::from(input), &axes, direction))
}

pub fn spectrum<S, D>(input: &ArrayBase<S, D>, half: Axis, axes: impl IntoIterator<Item = Axis>) -> Result<Array<Complex<f64>, D>, SpectraError>
where
    S: Data<Elem = f64>,
    D: Dimension,
{
    let (len, shape) = shaped(input.raw_dim(), half, |count| count.strict_div(2).strict_add(1))?;
    let axes = checked(input.ndim(), axes)?;
    let mut output = Array::zeros(shape);
    ndfft_r2c_par(input, &mut output, &R2cFftHandler::new(len), half.index());
    Ok(complex(CowArray::from(output), &axes, FftDirection::Forward))
}

pub fn signal<S, D>(input: &ArrayBase<S, D>, half: Axis, len: usize, axes: impl IntoIterator<Item = Axis>) -> Result<Array<f64, D>, SpectraError>
where
    S: Data<Elem = Complex<f64>>,
    D: Dimension,
{
    let (found, shape) = shaped(input.raw_dim(), half, |_| len)?;
    let axes = checked(input.ndim(), axes)?;
    let expected = len.strict_div(2).strict_add(1);
    (found == expected).ok_or(SpectraError::Bins { axis: half.index(), expected, found })?;
    let mut output = Array::zeros(shape);
    ndifft_r2c_par(&complex(CowArray::from(input), &axes, FftDirection::Inverse), &mut output, &R2cFftHandler::new(len), half.index());
    Ok(output)
}

pub fn cosine<S, D>(input: &ArrayBase<S, D>, axes: impl IntoIterator<Item = Axis>, direction: FftDirection) -> Result<Array<f64, D>, SpectraError>
where
    S: Data<Elem = f64>,
    D: Dimension,
{
    checked(input.ndim(), axes).map(|axes| {
        axes.into_iter()
            .fold(CowArray::from(input), |current, axis| match (current.len_of(axis), direction) {
                (0, _) => current,
                (1, FftDirection::Forward) => CowArray::from(current.mapv(|sample| 2.0 * sample)),
                (1, FftDirection::Inverse) => CowArray::from(current.mapv(|sample| sample / 2.0)),
                (len @ 2.., _) => {
                    let (handler, mut output) = (DctHandler::new(len), Array::zeros(current.raw_dim()));
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
            .into_owned()
    })
}

pub fn zoom<S, D>(input: &ArrayBase<S, D>, axis: Axis, rate: Rate, band: Band, bins: NonZero<usize>) -> Result<Array<Complex<f64>, D>, SpectraError>
where
    S: Data<Elem: Copy + Sync + Into<Complex<f64>>>,
    D: Dimension,
{
    let (count, shape) = shaped(input.raw_dim(), axis, |_| bins.get())?;
    let (rate, start) = (rate.get::<hertz>(), band.start.get::<hertz>());
    let (sweep, size) = (pitch(band, bins) / (2.0 * rate), count.max(1).strict_add(bins.get()).strict_sub(1).next_power_of_two());
    let turns = |coefficient: f64, value: f64| {
        let product = coefficient * value;
        product.rem_euclid(1.0) + coefficient.mul_add(value, -product)
    };
    let square = |index: usize| turns(sweep, convert::<usize, f64>(index.strict_mul(index)));
    let chirp = |index: usize| Complex::cis(TAU * square(index));
    let mut planner = FftPlanner::new();
    let [forward, inverse] = [FftDirection::Forward, FftDirection::Inverse].map(|direction| planner.plan_fft(size, direction));
    let scratch_len = forward.get_inplace_scratch_len().max(inverse.get_inplace_scratch_len());
    let mut kernel = Vec::from_fn(size, |index| match size.strict_sub(index) {
        _ if index < bins.get() => chirp(index),
        back if back < count => chirp(back),
        _ => Complex::ZERO,
    });
    forward.process(&mut kernel);
    let entry = Vec::from_fn(count, |index| Complex::cis(-TAU * (square(index) + turns(start / rate, convert::<usize, f64>(index)))));
    let exit = Vec::from_fn(bins.get(), |index| chirp(index).conj().unscale(convert::<usize, f64>(size)));
    let mut output = Array::zeros(shape);
    Zip::from(input.lanes(axis)).and(output.lanes_mut(axis)).par_for_each(|lane, mut out| {
        let (mut values, mut scratch) = (vec![Complex::ZERO; size], vec![Complex::ZERO; scratch_len]);
        iter::zip(&mut values, iter::zip(lane, &entry)).for_each(|(value, (&sample, &weight))| *value = sample.into() * weight);
        forward.process_with_scratch(&mut values, &mut scratch);
        iter::zip(&mut values, &kernel).for_each(|(value, &weight)| *value *= weight);
        inverse.process_with_scratch(&mut values, &mut scratch);
        iter::zip(&mut out, iter::zip(&values, &exit)).for_each(|(value, (&sum, &weight))| *value = sum * weight);
    });
    Ok(output)
}

fn complex<D>(input: CowArray<'_, Complex<f64>, D>, axes: &[Axis], direction: FftDirection) -> Array<Complex<f64>, D>
where D: Dimension {
    axes.iter()
        .fold(input, |current, &axis| {
            let (handler, mut output) = (FftHandler::new(current.len_of(axis)), Array::zeros(current.raw_dim()));
            match direction {
                FftDirection::Forward => ndfft_par(&current, &mut output, &handler, axis.index()),
                FftDirection::Inverse => ndifft_par(&current, &mut output, &handler, axis.index()),
            }
            CowArray::from(output)
        })
        .into_owned()
}

fn checked(dimensions: usize, axes: impl IntoIterator<Item = Axis>) -> Result<Vec<Axis>, SpectraError> {
    axes.into_iter().map(|Axis(axis)| (axis < dimensions).then_some(Axis(axis)).ok_or(SpectraError::Axis { axis, dimensions })).collect()
}

fn shaped<D>(mut shape: D, Axis(axis): Axis, len: impl FnOnce(usize) -> usize) -> Result<(usize, D), SpectraError>
where D: Dimension {
    let dimensions = shape.ndim();
    let slot = shape.slice_mut().get_mut(axis).ok_or(SpectraError::Axis { axis, dimensions })?;
    let found = *slot;
    *slot = len(found);
    Ok((found, shape))
}

// --- [BINS]

#[must_use]
pub fn frequencies(rate: Rate, len: NonZero<usize>, bins: Bins) -> Array1<Frequency> {
    let (rate, count) = (rate.get::<hertz>(), convert::<usize, f64>(len.get()));
    let (size, origin, spacing, wrap) = match bins {
        Bins::Half => (len.get().strict_div(2).strict_add(1), 0.0, rate / count, len),
        Bins::Full => (len.get(), 0.0, rate / count, len.div_ceil(const { NonZero::new(2).unwrap() })),
        Bins::Cosine => (len.get(), 0.0, rate / (2.0 * count), len),
        Bins::Zoom(band) => (len.get(), band.start.get::<hertz>(), pitch(band, len), len),
    };
    Array1::from_shape_fn(size, |bin| {
        let folded = bin.strict_div(wrap.get()).strict_mul(len.get());
        Frequency::new::<hertz>(spacing.mul_add(convert::<usize, f64>(bin) - convert::<usize, f64>(folded), origin))
    })
}

fn pitch(band: Band, bins: NonZero<usize>) -> f64 {
    NonZero::new(bins.get().strict_sub(1)).map_or_default(|intervals| (band.last.get::<hertz>() - band.start.get::<hertz>()) / convert::<usize, f64>(intervals.get()))
}
