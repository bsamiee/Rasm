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
pub enum Evaluation {
    EarlyDecay,
    T20,
    T30,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEADROOM: f64 = 10.0;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Reverberation {
    pub time: Time,
    pub nonlinearity: Ratio,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn curve(response: &Series) -> Result<Series, SpectraError> { integral(response).and_then(Samples::try_new).map(|samples| Series { rate: response.rate, samples }) }

pub fn reverberation(response: &Series, evaluation: Evaluation) -> Result<Option<Reverberation>, SpectraError> {
    let [upper, lower] = match evaluation {
        Evaluation::EarlyDecay => [0.0, -10.0],
        Evaluation::T20 => [-5.0, -25.0],
        Evaluation::T30 => [-5.0, -35.0],
    };
    let levels = integral(response)?.mapv_into(level);
    Ok(levels
        .last()
        .filter(|end| **end <= lower - HEADROOM)
        .and_then(|_| decline(levels.view(), [upper, lower]))
        .map(|(_, [_, slope], unexplained)| Reverberation { time: Time::new::<second>(-60.0 / (slope * response.rate.get::<hertz>())), nonlinearity: Ratio::new::<ratio>(unexplained) }))
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
    energy(response).map(|(energy, [remainder, attenuation])| {
        let moment = remainder
            .mul_add(convert::<usize, f64>(energy.len()) + attenuation / (1.0 - attenuation), energy.iter().enumerate().map(|(index, power)| convert::<usize, f64>(index) * power).sum::<f64>());
        Time::new::<second>(moment / (energy.sum() + remainder) / response.rate.get::<hertz>())
    })
}

fn energy(response: &Series) -> Result<(Array1<f64>, [f64; 2]), SpectraError> {
    const ONSET: f64 = 1.0e-2;
    const LATE_RANGE: f64 = 20.0;
    const WINDOW_DROP: f64 = 2.0;
    const ITERATIONS: usize = 5;
    let peak = response.samples.iter().map(|sample| sample.abs()).max_by(f64::total_cmp).filter(|peak| *peak > 0.0).ok_or(SpectraError::Silent)?;
    let padding = response.samples.iter().rev().take_while(|sample| **sample == 0.0).count();
    let squared = response.samples.iter().take(response.samples.len().strict_sub(padding)).map(|sample| (sample / peak).powi(2)).skip_while(|power| *power < ONSET).collect::<Array1<f64>>();
    let length = convert::<usize, f64>(squared.len());
    let last_tenth = (length * 0.9).floor();
    let floor = |from: f64| squared.slice_axis(Axis(0), Slice::from(convert_unchecked::<f64, usize>(from.min(last_tenth))..)).mean().filter(|noise| *noise > 0.0);
    let line = |width: f64, noise: f64, span: f64| {
        let size = convert_unchecked::<f64, usize>(width.round().max(1.0));
        let levels = squared.exact_chunks(size).into_iter().filter_map(|window| window.mean()).map(level).collect::<Array1<f64>>();
        let (loudest, _) = levels.iter().enumerate().max_by(|(_, left), (_, right)| left.total_cmp(right))?;
        let (first, [intercept, slope], _) = decline(levels.slice_axis(Axis(0), Slice::from(loudest..)), [level(noise) + HEADROOM + span, level(noise) + HEADROOM])?;
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
    Ok(match estimate {
        Some((crosspoint, [intercept, slope])) => {
            let end = crosspoint.max(0.0).round();
            let [power, attenuation] = [slope.mul_add(end, intercept), slope].map(|decibels| 10.0_f64.powf(decibels / 10.0));
            (squared.slice_axis_move(Axis(0), Slice::from(..=convert_unchecked::<f64, usize>(end))), [power * attenuation / (1.0 - attenuation), attenuation])
        }
        None => (squared, [0.0, 0.0]),
    })
}

fn integral(response: &Series) -> Result<Array1<f64>, SpectraError> {
    energy(response).map(|(mut decay, [remainder, _])| {
        let total = decay.sum() + remainder;
        decay.invert_axis(Axis(0));
        decay.accumulate_axis_inplace(Axis(0), |later, power| *power += *later);
        decay.invert_axis(Axis(0));
        decay.mapv_into(|integral| (integral + remainder) / total)
    })
}

fn split(response: &Series, early: Time) -> Result<[f64; 2], SpectraError> {
    let boundary = (early.get::<second>() * response.rate.get::<hertz>()).round();
    (boundary >= 0.0).ok_or(SpectraError::Split { early })?;
    let (energy, [remainder, attenuation]) = energy(response)?;
    let length = convert::<usize, f64>(energy.len());
    let (early_part, late_part) = energy.view().split_at(Axis(0), convert_unchecked::<f64, usize>(boundary.min(length)));
    let beyond = remainder * attenuation.powf((boundary - length).max(0.0));
    Ok([early_part.sum() + remainder - beyond, late_part.sum() + beyond])
}

fn decline(levels: ArrayView1<'_, f64>, [upper, lower]: [f64; 2]) -> Option<(usize, [f64; 2], f64)> {
    let first = levels.iter().position(|level| *level <= upper)?;
    let run = levels.iter().skip(first).take_while(|level| **level >= lower).copied().collect::<Col<f64>>();
    let fit = (run.nrows() > 1).then(|| Mat::from_fn(run.nrows(), 2, |row, power| convert::<usize, f64>(row).powf(convert::<usize, f64>(power))).qr().solve_lstsq(&run))?;
    let &[intercept, slope] = fit.try_as_col_major()?.as_slice().first_chunk::<2>()?;
    let mean = run.iter().sum::<f64>() / convert::<usize, f64>(run.nrows());
    let residual = run.iter().enumerate().map(|(row, level)| (level - slope.mul_add(convert::<usize, f64>(row), intercept)).powi(2)).sum::<f64>();
    let spread = run.iter().map(|level| (level - mean).powi(2)).sum::<f64>();
    (slope < 0.0).then_some((first, [intercept, slope], residual / spread))
}

fn level(quotient: f64) -> f64 { 10.0 * quotient.log10() }
