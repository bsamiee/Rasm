use core::{cmp::Ordering, f64::consts::TAU, iter, num::NonZero};

use nalgebra::convert;
use ndarray::{Array, Array1, ArrayBase, ArrayViewMut1, Axis, CowArray, Data, Dimension, Zip};
use ndrustfft::{DctHandler, FftHandler, Normalization, R2cFftHandler, nddct2_inplace_par, nddct3_inplace_par, ndfft_inplace_par, ndfft_r2c_par, ndifft_inplace_par, ndifft_r2c_par};
use num_complex::Complex;
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use rustfft::{FftDirection, FftPlanner};
use uom::si::{f64::Frequency, frequency::hertz, ratio::ratio};

use crate::{Band, Rate, Samples, Series, SpectraError, real_bins};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Transform {
    Real,
    Complex,
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
    checked(input.ndim(), None, axes).map(|axes| complex(input.as_standard_layout().into_owned(), &axes, direction))
}

pub fn spectrum<S, D>(input: &ArrayBase<S, D>, half: Axis, axes: impl IntoIterator<Item = Axis>) -> Result<Array<Complex<f64>, D>, SpectraError>
where
    S: Data<Elem = f64>,
    D: Dimension,
{
    let (len, shape) = shaped(input.raw_dim(), half, real_bins)?;
    let axes = checked(input.ndim(), Some(half), axes)?;
    let mut output = Array::zeros(shape);
    ndfft_r2c_par(input, &mut output, &R2cFftHandler::new(len), half.index());
    Ok(complex(output, &axes, FftDirection::Forward))
}

pub fn signal<S, D>(input: &ArrayBase<S, D>, half: Axis, len: usize, axes: impl IntoIterator<Item = Axis>) -> Result<Array<f64, D>, SpectraError>
where
    S: Data<Elem = Complex<f64>>,
    D: Dimension,
{
    let (found, shape) = shaped(input.raw_dim(), half, |_| len)?;
    let axes = checked(input.ndim(), Some(half), axes)?;
    let expected = real_bins(len);
    (found == expected).ok_or(SpectraError::Bins { axis: half.index(), expected, found })?;
    let transformed = match &axes {
        [] => CowArray::from(input.view()),
        axes => CowArray::from(complex(input.as_standard_layout().into_owned(), axes, FftDirection::Inverse)),
    };
    let mut output = Array::zeros(shape);
    ndifft_r2c_par(&transformed, &mut output, &R2cFftHandler::new(len), half.index());
    Ok(output)
}

pub fn cosine<S, D>(input: &ArrayBase<S, D>, axes: impl IntoIterator<Item = Axis>, direction: FftDirection) -> Result<Array<f64, D>, SpectraError>
where
    S: Data<Elem = f64>,
    D: Dimension,
{
    checked(input.ndim(), None, axes).map(|axes| {
        let mut output = input.as_standard_layout().into_owned();
        for axis in axes {
            match (output.len_of(axis), direction) {
                (0, _) => (),
                (1, FftDirection::Forward) => output.mapv_inplace(|sample| 2.0 * sample),
                (1, FftDirection::Inverse) => output.mapv_inplace(|sample| sample / 2.0),
                (len @ 2.., FftDirection::Forward) => nddct2_inplace_par(&mut output, &DctHandler::new(len), axis.index()),
                (len @ 2.., FftDirection::Inverse) => nddct3_inplace_par(
                    &mut output,
                    &DctHandler::new(len).normalization(Normalization::Custom(|lane: &mut [f64]| {
                        let count = convert::<usize, f64>(lane.len());
                        ArrayViewMut1::from(lane).mapv_inplace(|sample| sample / count);
                    })),
                    axis.index(),
                ),
            }
        }
        output
    })
}

pub fn analytic<S, D>(input: &ArrayBase<S, D>, axis: Axis) -> Result<Array<Complex<f64>, D>, SpectraError>
where
    S: Data<Elem = f64>,
    D: Dimension,
{
    let half = spectrum(input, axis, [])?;
    let len = input.len_of(axis);
    let mut output = Array::zeros(input.raw_dim());
    Zip::from(output.lanes_mut(axis)).and(half.lanes(axis)).par_for_each(|lane, bins| {
        iter::zip(lane, bins.iter().enumerate()).for_each(|(value, (bin, &coefficient))| {
            *value = match bin {
                0 => coefficient,
                bin if bin.strict_mul(2) == len => coefficient,
                _ => coefficient * 2.0,
            };
        });
    });
    ndifft_inplace_par(&mut output, &FftHandler::new(len), axis.index());
    Ok(output)
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
    Zip::from(input.lanes(axis)).and(output.lanes_mut(axis)).into_par_iter().for_each_init(
        || (vec![Complex::ZERO; size], vec![Complex::ZERO; scratch_len]),
        |(values, scratch), (lane, out)| {
            iter::zip(values.iter_mut(), iter::zip(lane, &entry).map(|(&sample, &weight)| sample.into() * weight).chain(iter::repeat(Complex::ZERO))).for_each(|(value, weighted)| *value = weighted);
            forward.process_with_scratch(values, scratch);
            iter::zip(values.iter_mut(), &kernel).for_each(|(value, &weight)| *value *= weight);
            inverse.process_with_scratch(values, scratch);
            iter::zip(out, iter::zip(values.iter(), &exit)).for_each(|(value, (&sum, &weight))| *value = sum * weight);
        },
    );
    Ok(output)
}

fn complex<D>(mut array: Array<Complex<f64>, D>, axes: &[Axis], direction: FftDirection) -> Array<Complex<f64>, D>
where D: Dimension {
    for &axis in axes {
        let handler = FftHandler::new(array.len_of(axis));
        match direction {
            FftDirection::Forward => ndfft_inplace_par(&mut array, &handler, axis.index()),
            FftDirection::Inverse => ndifft_inplace_par(&mut array, &handler, axis.index()),
        }
    }
    array
}

fn checked(dimensions: usize, half: Option<Axis>, axes: impl IntoIterator<Item = Axis>) -> Result<Vec<Axis>, SpectraError> {
    axes.into_iter()
        .map(|Axis(axis)| match axis {
            _ if axis >= dimensions => Err(SpectraError::Axis { axis, dimensions }),
            _ if half == Some(Axis(axis)) => Err(SpectraError::Half { axis }),
            _ => Ok(Axis(axis)),
        })
        .collect()
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

pub fn resample(series: &Series, rate: Rate) -> Result<Series, SpectraError> {
    let len = series.samples.len();
    let target = NonZero::new((convert::<usize, f64>(len) * (*rate / *series.rate).get::<ratio>()).round().to_int_saturating::<usize>()).ok_or(SpectraError::Empty)?;
    let (bins, shared) = (spectrum(&*series.samples, Axis(0), [])?, len.min(target.get()));
    let (scale, fold) = (
        convert::<usize, f64>(target.get()) / convert::<usize, f64>(len),
        match target.get().cmp(&len) {
            Ordering::Less => 2.0,
            Ordering::Equal => 1.0,
            Ordering::Greater => 0.5,
        },
    );
    let resized = Array1::from_shape_fn(real_bins(usize::from(target)), |bin| match bins.get(bin) {
        Some(&coefficient) if bin.strict_mul(2) == shared => coefficient * scale * fold,
        Some(&coefficient) => coefficient * scale,
        None => Complex::ZERO,
    });
    let resampled = Rate::try_new(*series.rate * scale)?;
    signal(&resized, Axis(0), target.get(), []).and_then(Samples::try_new).map(|samples| Series { rate: resampled, samples })
}

// --- [BINS]

#[must_use]
pub fn frequencies(rate: Rate, len: NonZero<usize>, transform: Transform) -> Array1<Frequency> {
    let (rate, count) = (rate.get::<hertz>(), convert::<usize, f64>(len.get()));
    let (size, origin, spacing, wrap) = match transform {
        Transform::Real => (real_bins(usize::from(len)), 0.0, rate / count, len),
        Transform::Complex => (len.get(), 0.0, rate / count, len.div_ceil(const { NonZero::new(2).unwrap() })),
        Transform::Cosine => (len.get(), 0.0, rate / (2.0 * count), len),
        Transform::Zoom(band) => (len.get(), band.start.get::<hertz>(), pitch(band, len), len),
    };
    Array1::from_shape_fn(size, |bin| {
        let folded = bin.strict_div(wrap.get()).strict_mul(len.get());
        Frequency::new::<hertz>(spacing.mul_add(convert::<usize, f64>(bin) - convert::<usize, f64>(folded), origin))
    })
}

fn pitch(band: Band, bins: NonZero<usize>) -> f64 {
    NonZero::new(bins.get().strict_sub(1)).map_or_default(|intervals| (band.last.get::<hertz>() - band.start.get::<hertz>()) / convert::<usize, f64>(intervals.get()))
}
