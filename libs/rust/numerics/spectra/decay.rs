use core::iter;

use faer::{Col, Mat, linalg::solvers::SolveLstsq as _};
use nalgebra::{convert, convert_unchecked};
use ndarray::{Array1, ArrayView1, Axis, Slice};
use uom::si::{
    f64::{Ratio, Time},
    frequency::hertz,
    ratio::ratio,
    time::{millisecond, second},
};

use crate::{Samples, Series, SpectraError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Reverberation {
    EarlyDecay,
    T20,
    T30,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEADROOM: f64 = 10.0;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn curve(response: &Series) -> Result<Series, SpectraError> {
    let (mut decay, truncation) = energy(response)?;
    let remainder = tail(truncation, 1.0);
    let total = decay.iter().rev().sum::<f64>() + remainder;
    decay.invert_axis(Axis(0));
    decay.accumulate_axis_inplace(Axis(0), |later, power| *power += *later);
    decay.invert_axis(Axis(0));
    Ok(Series { rate: response.rate, samples: Samples::try_new(decay.mapv_into(|integral| (integral + remainder) / total))? })
}

pub fn reverberation(response: &Series, evaluation: Reverberation) -> Result<Option<Time>, SpectraError> {
    let decay = curve(response)?.samples.mapv(level);
    let start = -5.0;
    let [upper, lower] = match evaluation {
        Reverberation::EarlyDecay => [0.0, -10.0],
        Reverberation::T20 => [start, -25.0],
        Reverberation::T30 => [start, -35.0],
    };
    Ok(decay
        .last()
        .filter(|end| **end <= lower - HEADROOM)
        .and_then(|_| decline(decay.view(), [upper, lower]))
        .map(|(_, [_, slope])| Time::new::<second>(-60.0 / (slope * response.rate.get::<hertz>()))))
}

pub fn clarity(response: &Series, early: Time) -> Result<f64, SpectraError> {
    match split(response, early)? {
        [early_energy, late_energy] if early_energy > 0.0 && late_energy > 0.0 => Ok(level(early_energy / late_energy)),
        [_, _] => Err(SpectraError::Split { early }),
    }
}

pub fn definition(response: &Series, early: Time) -> Result<Ratio, SpectraError> {
    split(response, early).map(|[early_energy, late_energy]| Ratio::new::<ratio>(early_energy / (early_energy + late_energy)))
}

pub fn centre(response: &Series) -> Result<Time, SpectraError> {
    let (energy, truncation @ [_, decline_ratio]) = energy(response)?;
    let remainder = tail(truncation, 1.0);
    let moment = remainder
        .mul_add(convert::<usize, f64>(energy.len()) + decline_ratio / (1.0 - decline_ratio), energy.iter().enumerate().map(|(index, power)| convert::<usize, f64>(index) * power).sum::<f64>());
    Ok(Time::new::<second>(moment / (energy.sum() + remainder) / response.rate.get::<hertz>()))
}

fn energy(response: &Series) -> Result<(Array1<f64>, [f64; 2]), SpectraError> {
    const ONSET: f64 = 1.0e-2;
    const LATE_RANGE: f64 = 20.0;
    const WINDOW_DROP: f64 = 2.0;
    const ITERATIONS: usize = 5;
    let peak = response.samples.iter().map(|sample| sample.abs()).max_by(f64::total_cmp).filter(|peak| *peak > 0.0).ok_or(SpectraError::Silent)?;
    let padding = response.samples.iter().rev().take_while(|sample| **sample == 0.0).count();
    let mut squared = response.samples.iter().take(response.samples.len().strict_sub(padding)).map(|sample| (sample / peak).powi(2)).skip_while(|power| *power < ONSET).collect::<Array1<f64>>();
    let length = convert::<usize, f64>(squared.len());
    let last_tenth = (length * 0.9).floor();
    let floor = |from: f64| squared.slice_axis(Axis(0), Slice::from(convert_unchecked::<f64, usize>(from.min(last_tenth))..)).mean().filter(|noise| *noise > 0.0);
    let line = |width: f64, noise: f64, span: f64| {
        let size = convert_unchecked::<f64, usize>(width.round().max(1.0));
        let levels = squared.exact_chunks(size).into_iter().filter_map(|window| window.mean()).map(level).collect::<Array1<f64>>();
        let (loudest, _) = levels.iter().enumerate().max_by(|(_, left), (_, right)| left.total_cmp(right))?;
        let (first, [intercept, slope]) = decline(levels.slice_axis(Axis(0), Slice::from(loudest..)), [level(noise) + HEADROOM + span, level(noise) + HEADROOM])?;
        let spacing = convert::<usize, f64>(size);
        let origin = (convert::<usize, f64>(loudest) + convert::<usize, f64>(first)).mul_add(spacing, (spacing - 1.0) / 2.0);
        Some([intercept - slope * origin / spacing, slope / spacing])
    };
    let crossing = |noise: f64, [intercept, slope]: [f64; 2]| (level(noise) - intercept) / slope;
    let preliminary = floor(last_tenth).and_then(|noise| line(Time::new::<millisecond>(30.0).get::<second>() * response.rate.get::<hertz>(), noise, f64::INFINITY).map(|decay| (noise, decay)));
    let estimate = iter::successors(preliminary, |&(noise, decay @ [_, slope])| {
        let late_noise = floor(crossing(noise, decay) - HEADROOM / slope)?;
        line(WINDOW_DROP / -slope, late_noise, LATE_RANGE).map(|late| (late_noise, late))
    })
    .take(ITERATIONS)
    .last()
    .map(|(noise, decay)| (crossing(noise, decay), decay))
    .filter(|(crosspoint, _)| *crosspoint < length - 1.0);
    let gain = |decibels: f64| 10.0_f64.powf(decibels / 10.0);
    match estimate {
        Some((crosspoint, [intercept, slope])) => {
            let end = crosspoint.max(0.0).round();
            squared.slice_axis_inplace(Axis(0), Slice::from(..=convert_unchecked::<f64, usize>(end)));
            Ok((squared, [gain(slope.mul_add(end, intercept)), gain(slope)]))
        }
        None => Ok((squared, [0.0, 0.0])),
    }
}

fn split(response: &Series, early: Time) -> Result<[f64; 2], SpectraError> {
    let (energy, truncation) = energy(response)?;
    let boundary = (early.get::<second>() * response.rate.get::<hertz>()).round();
    (boundary >= 0.0).ok_or(SpectraError::Split { early })?;
    let length = convert::<usize, f64>(energy.len());
    let (early_part, late_part) = energy.view().split_at(Axis(0), convert_unchecked::<f64, usize>(boundary.min(length)));
    let [measured, beyond] = [0.0, (boundary - length).max(0.0)].map(|past| tail(truncation, past + 1.0));
    Ok([early_part.sum() + measured - beyond, late_part.sum() + beyond])
}

fn decline(levels: ArrayView1<'_, f64>, [upper, lower]: [f64; 2]) -> Option<(usize, [f64; 2])> {
    let first = levels.iter().position(|level| *level <= upper)?;
    let run = levels.iter().skip(first).take_while(|level| **level >= lower).copied().collect::<Col<f64>>();
    let fit = (run.nrows() > 1).then(|| Mat::from_fn(run.nrows(), 2, |row, power| convert::<usize, f64>(row).powf(convert::<usize, f64>(power))).qr().solve_lstsq(run))?;
    let &[intercept, slope] = fit.try_as_col_major()?.as_slice().first_chunk::<2>()?;
    (slope < 0.0).then_some((first, [intercept, slope]))
}

fn tail([power, decline_ratio]: [f64; 2], offset: f64) -> f64 { power * decline_ratio.powf(offset) / (1.0 - decline_ratio) }

fn level(quotient: f64) -> f64 { 10.0 * quotient.log10() }
