use core::{
    borrow::Borrow,
    convert::identity,
    f64::consts::{PI, TAU},
    iter,
    num::NonZero,
    range::RangeInclusive,
};

use biquad::{Biquad as _, Coefficients, DirectForm2Transposed, Errors, Hertz, Type};
use nalgebra::{convert, convert_unchecked};
use ndarray::{Array1, Axis, Slice};
use rustfft::num_complex::Complex;
use uom::si::{f64::Frequency, frequency::hertz};

use crate::{Band, Rate, Samples, Series, SpectraError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Direction {
    Forward,
    Reverse,
    ZeroPhase,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Pass {
    Low(Frequency),
    High(Frequency),
    Band(Band),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Stage {
    Butterworth { pass: Pass, order: NonZero<u8> },
    Section { filter: Type<f64>, f0: Frequency, q_value: f64 },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn filter<I>(series: &Series, stages: I, direction: Direction) -> Result<Series, SpectraError>
where I: IntoIterator<Item: Borrow<Stage>> {
    fn run(sections: &[Coefficients<f64>], level: fn(f64) -> f64, samples: impl Iterator<Item = f64>) -> impl Iterator<Item = f64> {
        samples.scan(None, move |chain: &mut Option<Vec<DirectForm2Transposed<f64>>>, sample| {
            let chain = chain.get_or_insert_with(|| {
                sections
                    .iter()
                    .scan(level(sample), |input, &coefficients @ Coefficients { a1, a2, b0, b1, b2 }| {
                        let output = *input * (b0 + b1 + b2) / (1.0 + a1 + a2);
                        let mut section = DirectForm2Transposed::<f64>::new(coefficients);
                        (section.s1, section.s2) = (b0.mul_add(-*input, output), b2.mul_add(*input, -a2 * output));
                        *input = output;
                        Some(section)
                    })
                    .collect()
            });
            Some(chain.iter_mut().fold(sample, |signal, section| section.run(signal)))
        })
    }
    let sections = cascade(series.rate, stages)?;
    let signal = &*series.samples;
    let length = signal.len();
    let filtered = match direction {
        Direction::Forward => run(&sections, |_| 0.0, signal.iter().copied()).collect::<Array1<f64>>(),
        Direction::Reverse => run(&sections, |_| 0.0, signal.iter().rev().copied()).collect::<Array1<f64>>().slice_axis_move(Axis(0), Slice::from(..).step_by(-1)),
        Direction::ZeroPhase => {
            let radius = |&Coefficients { a1, a2, b0: _, b1: _, b2: _ }: &Coefficients<f64>| match a1.mul_add(a1, -4.0 * a2) {
                discriminant if discriminant < 0.0 => a2.sqrt(),
                discriminant => a1.abs().midpoint(discriminant.sqrt()),
            };
            let decay = match sections.iter().map(radius).max_by(f64::total_cmp) {
                Some(radius) if radius < 1.0 => f64::EPSILON.log(radius).ceil(),
                Some(_) => f64::INFINITY,
                None => 0.0,
            };
            let pad = convert_unchecked::<f64, usize>((decay + convert::<usize, f64>(sections.len().strict_mul(2))).min(convert::<usize, f64>(length.strict_sub(1))));
            let extended = signal
                .first()
                .into_iter()
                .flat_map(|&first| signal.iter().skip(1).take(pad).rev().map(move |sample| 2.0_f64.mul_add(first, -sample)))
                .chain(signal.iter().copied())
                .chain(signal.last().into_iter().flat_map(|&last| signal.iter().rev().skip(1).take(pad).map(move |sample| 2.0_f64.mul_add(last, -sample))));
            let forward = run(&sections, identity, extended).collect::<Array1<f64>>();
            run(&sections, identity, forward.iter().rev().copied()).collect::<Array1<f64>>().slice_axis_move(Axis(0), Slice::from(pad..pad.strict_add(length)).step_by(-1))
        }
    };
    Samples::try_new(filtered).map(|samples| Series { rate: series.rate, samples })
}

pub fn response<F, I>(rate: Rate, frequencies: F, stages: I, direction: Direction) -> Result<Array1<Complex<f64>>, SpectraError>
where
    F: IntoIterator<Item: Borrow<Frequency>>,
    I: IntoIterator<Item: Borrow<Stage>>,
{
    let sections = cascade(rate, stages)?;
    Ok(frequencies
        .into_iter()
        .map(|frequency| {
            let delay = Complex::cis(-TAU * frequency.borrow().get::<hertz>() / rate.get::<hertz>());
            let gain = sections.iter().map(|&Coefficients { a1, a2, b0, b1, b2 }| (delay * (delay * b2 + b1) + b0) / (delay * (delay * a2 + a1) + 1.0)).product::<Complex<f64>>();
            match direction {
                Direction::Forward => gain,
                Direction::Reverse => gain.conj(),
                Direction::ZeroPhase => Complex::from(gain.norm_sqr()),
            }
        })
        .collect())
}

#[must_use]
pub fn bands(fraction: NonZero<u8>, span: Band) -> Vec<Band> {
    let reference = 1000.0;
    let octave = 10.0_f64.powf(0.3);
    let count = convert::<u8, f64>(fraction.get());
    let offset = if fraction.get().is_multiple_of(2) { 0.5 } else { 0.0 };
    let edge = octave.powf(0.5 / count);
    let [lowest, highest] = [span.start, span.last].map(|frequency| count.mul_add((frequency.get::<hertz>().max(f64::MIN_POSITIVE) / reference).log(octave), -offset).round());
    iter::successors(Some(lowest), |index| Some(index + 1.0))
        .take_while(|index| *index <= highest)
        .map(|index| reference * octave.powf((index + offset) / count))
        .map_while(|midband| Band::try_new(RangeInclusive { start: Frequency::new::<hertz>(midband / edge), last: Frequency::new::<hertz>(midband * edge) }).ok())
        .collect()
}

fn cascade<I>(rate: Rate, stages: I) -> Result<Vec<Coefficients<f64>>, SpectraError>
where I: IntoIterator<Item: Borrow<Stage>> {
    let fs = rate.get::<hertz>();
    let sampling = Hertz::<f64>::from_hz(fs);
    let bilinear = 2.0 * fs;
    let poles = |order: NonZero<u8>| {
        let degree = convert::<u8, f64>(order.get());
        (0..order.get() / 2).map(move |index| Complex::cis(PI * convert::<u8, f64>(index).mul_add(2.0, degree + 1.0) / (2.0 * degree)))
    };
    stages
        .into_iter()
        .enumerate()
        .map(|(stage, element)| -> Result<Vec<Coefficients<f64>>, SpectraError> {
            let refused = |f0: f64| move |refusal: Errors| SpectraError::Filter { stage, f0: Frequency::new::<hertz>(f0), rate, refusal };
            let designed = |filter: Type<f64>, f0: f64, q_value: f64, gain: f64| {
                sampling
                    .and_then(|sampling| Coefficients::from_params(filter, sampling, Hertz::from_hz(f0)?, q_value))
                    .map_err(refused(f0))
                    .map(|Coefficients { a1, a2, b0, b1, b2 }| Coefficients { a1, a2, b0: b0 * gain, b1: b1 * gain, b2: b2 * gain })
                    .and_then(|coefficients @ Coefficients { a1, a2, b0, b1, b2 }| {
                        [a1, a2, b0, b1, b2].into_iter().all(f64::is_finite).then_some(coefficients).ok_or_else(|| SpectraError::Section { stage, f0: Frequency::new::<hertz>(f0) })
                    })
            };
            let prototype = |pair: Type<f64>, cutoff: Frequency, order: NonZero<u8>, real: fn(Coefficients<f64>) -> Coefficients<f64>| {
                let f0 = cutoff.get::<hertz>();
                poles(order)
                    .map(|pole| designed(pair, f0, -0.5 / pole.re, 1.0))
                    .chain((!order.get().is_multiple_of(2)).then(|| designed(Type::SinglePoleLowPass, f0, 0.5, 1.0).map(real)))
                    .collect::<Result<Vec<_>, _>>()
            };
            match *element.borrow() {
                Stage::Section { filter, f0, q_value } => designed(filter, f0.get::<hertz>(), q_value, 1.0).map(|coefficients| vec![coefficients]),
                Stage::Butterworth { pass: Pass::Low(cutoff), order } => prototype(Type::LowPass, cutoff, order, identity),
                Stage::Butterworth { pass: Pass::High(cutoff), order } => prototype(Type::HighPass, cutoff, order, |low| Coefficients { b0: 1.0 - low.b0, b1: low.a1 - low.b1, ..low }),
                Stage::Butterworth { pass: Pass::Band(band), order } => {
                    let [low, high] = match [band.start, band.last].map(|edge| edge.get::<hertz>()) {
                        [start, _] if start <= 0.0 => Err(refused(start)(Errors::NegativeFrequency)),
                        [_, last] if last >= fs / 2.0 => Err(refused(last)(Errors::OutsideNyquist)),
                        edges => Ok(edges.map(|edge| bilinear * (PI * edge / fs).tan())),
                    }?;
                    let (width, centre) = (high - low, (low * high).sqrt());
                    poles(order)
                        .flat_map(|pole| {
                            let shift = pole * width / 2.0;
                            let offset = (shift * shift - centre * centre).sqrt();
                            [shift + offset, shift - offset].map(|root| (root.norm(), -0.5 * root.norm() / root.re))
                        })
                        .chain((!order.get().is_multiple_of(2)).then_some((centre, centre / width)))
                        .map(|(omega, q_value)| designed(Type::BandPass, fs / PI * (omega / bilinear).atan(), q_value, width / omega))
                        .collect()
                }
            }
        })
        .collect::<Result<Vec<_>, _>>()
        .map(|cascade| cascade.concat())
}
