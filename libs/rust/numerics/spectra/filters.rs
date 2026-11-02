use core::{
    borrow::Borrow,
    convert::identity,
    f64::consts::{PI, TAU},
    iter,
    num::NonZero,
    range::RangeInclusive,
};

use biquad::{Biquad as _, Coefficients, DirectForm2Transposed, Errors, Hertz, ToHertz as _, Type};
use nalgebra::convert;
use ndarray::{Array1, Axis, Slice};
use num_complex::Complex;
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use uom::si::{f64::Frequency, frequency::hertz, ratio::ratio};

use crate::{Band, Rate, Samples, Series, SpectraError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Direction {
    Forward,
    Reverse,
    ZeroPhase,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Weighting {
    A,
    C,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Pass {
    Low(Frequency),
    High(Frequency),
    Band(Band),
    Stop(Band),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Stage {
    Butterworth { pass: Pass, order: NonZero<u8> },
    Section { filter: Type<f64>, f0: Frequency, q_value: f64 },
    Weighting(Weighting),
}

#[derive(Debug, Clone, PartialEq)]
pub struct Filtered {
    pub band: Band,
    pub energy: f64,
    pub series: Series,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn filter<I>(series: &Series, stages: I, direction: Direction) -> Result<Series, SpectraError>
where I: IntoIterator<Item: Borrow<Stage>> {
    fn run(sections: &[Coefficients<f64>], level: fn(f64) -> f64, samples: impl Iterator<Item = f64>) -> impl Iterator<Item = f64> {
        let mut samples = samples.peekable();
        let chain = samples.peek().map_or_default(|&first| {
            sections
                .iter()
                .scan(level(first), |input, &coefficients @ Coefficients { a1, a2, b0, b1, b2 }| {
                    let output = *input * (b0 + b1 + b2) / (1.0 + a1 + a2);
                    let mut section = DirectForm2Transposed::<f64>::new(coefficients);
                    (section.s1, section.s2) = (b0.mul_add(-*input, output), b2.mul_add(*input, -a2 * output));
                    *input = output;
                    Some(section)
                })
                .collect::<Vec<_>>()
        });
        samples.scan(chain, |chain, sample| Some(chain.iter_mut().fold(sample, |signal, section| section.run(signal))))
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
            let pad = (decay + convert::<usize, f64>(sections.len().strict_mul(2))).to_int_saturating::<usize>().min(length.strict_sub(1));
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
            let gain = gain(&sections, (*frequency.borrow() / *rate).get::<ratio>());
            match direction {
                Direction::Forward => gain,
                Direction::Reverse => gain.conj(),
                Direction::ZeroPhase => Complex::from(gain.norm_sqr()),
            }
        })
        .collect())
}

pub fn bands(fraction: NonZero<u8>, span: Band) -> Result<Vec<Band>, SpectraError> {
    let (reference, octave) = (1000.0, 10.0_f64.powf(0.3));
    let count = f64::from(fraction.get());
    let offset = if fraction.get().is_multiple_of(2) { 0.5 } else { 0.0 };
    let edge = octave.powf(0.5 / count);
    (span.start.get::<hertz>() > 0.0).ok_or(SpectraError::Band { band: *span })?;
    let [lowest, highest] = [span.start, span.last].map(|frequency| count.mul_add((frequency.get::<hertz>() / reference).log(octave), -offset).round().to_int_saturating::<i32>());
    (lowest..=highest)
        .map(|index| reference * octave.powf((f64::from(index) + offset) / count))
        .map(|midband| Band::try_new(RangeInclusive { start: Frequency::new::<hertz>(midband / edge), last: Frequency::new::<hertz>(midband * edge) }))
        .collect()
}

pub fn bank(series: &Series, fraction: NonZero<u8>, span: Band, order: NonZero<u8>, direction: Direction) -> Result<Vec<Filtered>, SpectraError> {
    bands(fraction, span)?
        .into_par_iter()
        .map(|band| {
            filter(series, [Stage::Butterworth { pass: Pass::Band(band), order }], direction).map(|series| Filtered {
                band,
                energy: series.samples.dot(&*series.samples) / series.rate.get::<hertz>(),
                series,
            })
        })
        .collect::<Result<Vec<_>, _>>()
}

fn gain<'section, I>(sections: I, cycles: f64) -> Complex<f64>
where I: IntoIterator<Item = &'section Coefficients<f64>> {
    let delay = Complex::cis(-TAU * cycles);
    sections.into_iter().map(|&Coefficients { a1, a2, b0, b1, b2 }| (delay * (delay * b2 + b1) + b0) / (delay * (delay * a2 + a1) + 1.0)).product()
}

fn cascade<I>(rate: Rate, stages: I) -> Result<Vec<Coefficients<f64>>, SpectraError>
where I: IntoIterator<Item: Borrow<Stage>> {
    const REFERENCE: f64 = 1000.0;
    const POLES: [f64; 4] = [20.6, 107.7, 737.9, 12_194.0];
    const LIMITS: [[f64; 2]; 34] = [
        [f64::NEG_INFINITY, 3.5],
        [f64::NEG_INFINITY, 3.0],
        [-4.5, 2.5],
        [-2.5, 2.5],
        [-2.0, 2.5],
        [-2.0, 2.0],
        [-1.5, 1.5],
        [-1.5, 1.5],
        [-1.5, 1.5],
        [-1.5, 1.5],
        [-1.5, 1.5],
        [-1.5, 1.5],
        [-1.5, 1.5],
        [-1.5, 1.5],
        [-1.4, 1.4],
        [-1.4, 1.4],
        [-1.4, 1.4],
        [-1.4, 1.4],
        [-1.4, 1.4],
        [-1.4, 1.4],
        [-1.1, 1.1],
        [-1.4, 1.4],
        [-1.6, 1.6],
        [-1.6, 1.6],
        [-1.6, 1.6],
        [-1.6, 1.6],
        [-1.6, 1.6],
        [-2.1, 2.1],
        [-2.6, 2.1],
        [-3.1, 2.1],
        [-3.6, 2.6],
        [-6.0, 3.0],
        [-17.0, 3.5],
        [f64::NEG_INFINITY, 4.0],
    ];
    let fs = rate.get::<hertz>();
    let sampling: Hertz<f64> = fs.hz();
    let bilinear = 2.0 * fs;
    let poles = |order: NonZero<u8>| {
        let degree = f64::from(order.get());
        (0..order.get() / 2).map(move |index| Complex::cis(PI * f64::from(index).mul_add(2.0, degree + 1.0) / (2.0 * degree)))
    };
    let scaled = |Coefficients { a1, a2, b0, b1, b2 }: Coefficients<f64>, gain: f64| Coefficients { a1, a2, b0: b0 * gain, b1: b1 * gain, b2: b2 * gain };
    let complement = |low: Coefficients<f64>| Coefficients { b0: 1.0 - low.b0, b1: low.a1 - low.b1, ..low };
    let decibels = |amplitude: f64| 20.0 * amplitude.log10();
    stages
        .into_iter()
        .enumerate()
        .map(|(stage, element)| -> Result<Vec<Coefficients<f64>>, SpectraError> {
            let refused = |f0: f64| move |refusal: Errors| SpectraError::Filter { stage, f0: Frequency::new::<hertz>(f0), rate, refusal };
            let finite = |f0: f64, coefficients @ Coefficients { a1, a2, b0, b1, b2 }: Coefficients<f64>| {
                [a1, a2, b0, b1, b2].into_iter().all(f64::is_finite).then_some(coefficients).ok_or_else(|| SpectraError::Section { stage, f0: Frequency::new::<hertz>(f0) })
            };
            let designed = |filter: Type<f64>, f0: f64, q_value: f64, gain: f64| {
                Hertz::from_hz(f0)
                    .and_then(|frequency| Coefficients::from_params(filter, sampling, frequency, q_value))
                    .map_err(refused(f0))
                    .and_then(|coefficients| finite(f0, scaled(coefficients, gain)))
            };
            let odd = |order: NonZero<u8>| !order.get().is_multiple_of(2);
            let prototype = |pair: Type<f64>, cutoff: Frequency, order: NonZero<u8>, real: fn(Coefficients<f64>) -> Coefficients<f64>| {
                let f0 = cutoff.get::<hertz>();
                poles(order).map(|pole| designed(pair, f0, -0.5 / pole.re, 1.0)).chain(odd(order).then(|| designed(Type::SinglePoleLowPass, f0, 0.5, 1.0).map(real))).try_collect::<Vec<_>>()
            };
            let resonances = |band: Band, order: NonZero<u8>, mapped: fn(Complex<f64>) -> Complex<f64>| {
                let [low, high] = [band.start, band.last].map(|edge| bilinear * (PI * edge.get::<hertz>() / fs).tan());
                let (width, square) = (high - low, low * high);
                let centre = square.sqrt();
                let pairs = poles(order)
                    .flat_map(move |pole| {
                        let shift = mapped(pole) * width / 2.0;
                        let offset = (shift * shift - square).sqrt();
                        [shift + offset, shift - offset].map(|root| (root.norm(), -0.5 * root.norm() / root.re))
                    })
                    .chain(odd(order).then_some((centre, centre / width)));
                (width, square, pairs)
            };
            let tuned = |omega: f64| fs / PI * (omega / bilinear).atan();
            match *element.borrow() {
                Stage::Section { filter, f0, q_value } => designed(filter, f0.get::<hertz>(), q_value, 1.0).map(|coefficients| vec![coefficients]),
                Stage::Butterworth { pass: Pass::Low(cutoff), order } => prototype(Type::LowPass, cutoff, order, identity),
                Stage::Butterworth { pass: Pass::High(cutoff), order } => prototype(Type::HighPass, cutoff, order, complement),
                Stage::Butterworth { pass: Pass::Band(band) | Pass::Stop(band), order: _ } if band.last >= *rate / 2.0 => Err(refused(band.last.get::<hertz>())(Errors::OutsideNyquist)),
                Stage::Butterworth { pass: Pass::Band(band) | Pass::Stop(band), order: _ } if band.start.get::<hertz>() <= 0.0 => Err(refused(band.start.get::<hertz>())(Errors::NegativeFrequency)),
                Stage::Butterworth { pass: Pass::Band(band), order } => {
                    let (width, _, pairs) = resonances(band, order, identity);
                    pairs.map(|(omega, q_value)| designed(Type::BandPass, tuned(omega), q_value, width / omega)).collect()
                }
                Stage::Butterworth { pass: Pass::Stop(band), order } => {
                    let (_, square, pairs) = resonances(band, order, |pole| pole.inv());
                    pairs
                        .map(|(omega, q_value)| {
                            let (damping, [warp, natural]) = (omega / q_value * bilinear, [bilinear * bilinear, omega * omega]);
                            let leading = warp + damping + natural;
                            let zero = (warp + square) / leading;
                            finite(
                                tuned(omega),
                                Coefficients { a1: 2.0 * (natural - warp) / leading, a2: (warp - damping + natural) / leading, b0: zero, b1: 2.0 * (square - warp) / leading, b2: zero },
                            )
                        })
                        .collect()
                }
                Stage::Weighting(weighting) => {
                    let [low, second, third, high] = POLES;
                    let singles: &[f64] = match weighting {
                        Weighting::A => &[second, third],
                        Weighting::C => &[],
                    };
                    let base = designed(Type::HighPass, low, 0.5, 1.0)?;
                    let rest = iter::once(designed(Type::LowPass, high, 0.5, 1.0))
                        .chain(singles.iter().map(|&pole| designed(Type::SinglePoleLowPass, pole, 0.5, 1.0).map(complement)))
                        .try_collect::<Vec<_>>()?;
                    let sections = iter::once(scaled(base, gain(iter::once(&base).chain(&rest), REFERENCE / fs).norm().recip())).chain(rest).collect::<Vec<_>>();
                    let analog = |frequency: f64| {
                        frequency.powi(2) / frequency.mul_add(frequency, low * low) * high * high / frequency.mul_add(frequency, high * high)
                            * singles.iter().map(|&pole| frequency / frequency.hypot(pole)).product::<f64>()
                    };
                    iter::zip((10..=43).map(|band| 10.0_f64.powf(f64::from(band) / 10.0)), LIMITS)
                        .take_while(|&(frequency, _)| frequency < fs / 2.0)
                        .try_for_each(|(frequency, [lower, upper])| {
                            let deviation = decibels(gain(&sections, frequency / fs).norm()) - decibels(analog(frequency) / analog(REFERENCE));
                            (lower..=upper).contains(&deviation).ok_or(SpectraError::Weighting { stage, frequency: Frequency::new::<hertz>(frequency), deviation, rate })
                        })
                        .map(|()| sections)
                }
            }
        })
        .try_collect::<Vec<_>>()
        .map(|cascade| cascade.concat())
}
