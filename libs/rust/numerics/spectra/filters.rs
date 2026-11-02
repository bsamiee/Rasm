use core::{borrow::Borrow, convert::identity, f64::consts::PI, iter, num::NonZero, range::RangeInclusive};

use biquad::{Biquad as _, Coefficients, DirectForm2Transposed, Errors, Hertz, Type};
use nalgebra::convert;
use ndarray::Array1;
use rustfft::num_complex::Complex;
use uom::si::{f64::Frequency, frequency::hertz};

use crate::{Band, BandError, Samples, Series, SpectraError};

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

pub fn filter<I>(series: &Series, stages: I) -> Result<Series, SpectraError>
where I: IntoIterator<Item: Borrow<Stage>> {
    let rate = series.rate.get::<hertz>();
    let sampling = Hertz::<f64>::from_hz(rate);
    let bilinear = 2.0 * rate;
    let poles = |order: NonZero<u8>| {
        let degree = convert::<u8, f64>(order.get());
        (0..order.get() / 2).map(move |index| Complex::cis(PI * convert::<u8, f64>(index).mul_add(2.0, degree + 1.0) / (2.0 * degree)))
    };
    let mut cascade = stages
        .into_iter()
        .enumerate()
        .map(|(stage, element)| -> Result<Vec<Coefficients<f64>>, SpectraError> {
            let refused = |f0: f64| move |refusal: Errors| SpectraError::Filter { stage, f0: Frequency::new::<hertz>(f0), rate: series.rate, refusal };
            let designed = |filter: Type<f64>, f0: f64, q_value: f64, gain: f64| {
                sampling
                    .and_then(|fs| Coefficients::from_params(filter, fs, Hertz::from_hz(f0)?, q_value))
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
                        [_, last] if last >= rate / 2.0 => Err(refused(last)(Errors::OutsideNyquist)),
                        edges => Ok(edges.map(|edge| bilinear * (PI * edge / rate).tan())),
                    }?;
                    let (width, centre) = (high - low, (low * high).sqrt());
                    poles(order)
                        .flat_map(|pole| {
                            let shift = pole * width / 2.0;
                            let offset = (shift * shift - centre * centre).sqrt();
                            [shift + offset, shift - offset].map(|root| (root.norm(), -0.5 * root.norm() / root.re))
                        })
                        .chain((!order.get().is_multiple_of(2)).then_some((centre, centre / width)))
                        .map(|(omega, q_value)| designed(Type::BandPass, rate / PI * (omega / bilinear).atan(), q_value, width / omega))
                        .collect()
                }
            }
        })
        .collect::<Result<Vec<_>, _>>()?
        .into_iter()
        .flatten()
        .map(DirectForm2Transposed::<f64>::new)
        .collect::<Vec<_>>();
    Samples::try_new(series.samples.iter().map(|sample| cascade.iter_mut().fold(*sample, |signal, section| section.run(signal))).collect::<Array1<f64>>())
        .map(|samples| Series { rate: series.rate, samples })
}

#[must_use]
pub fn bands(fraction: NonZero<u8>, span: Band) -> Vec<Band> {
    let reference = 1000.0;
    let octave = 10.0_f64.powf(0.3);
    let count = convert::<u8, f64>(fraction.get());
    let offset = if fraction.get().is_multiple_of(2) { 0.5 } else { 0.0 };
    let edge = octave.powf(0.5 / count);
    let last = span.last.get::<hertz>();
    iter::successors(Some(count.mul_add((span.start.get::<hertz>().max(f64::MIN_POSITIVE) / reference).log(octave), -offset).ceil()), |index| Some(index + 1.0))
        .map(|index| reference * octave.powf((index + offset) / count))
        .map_while(|midband| match Band::try_new(RangeInclusive { start: Frequency::new::<hertz>(midband / edge), last: Frequency::new::<hertz>(midband * edge) }) {
            Ok(band) if midband <= last => Some(band),
            Ok(_) | Err(BandError::PredicateViolated) => None,
        })
        .collect()
}
