use core::iter;
use core::marker::PhantomData;
use core::num::NonZero;
use core::ops::ControlFlow;
use core::range::{Range, RangeInclusive};

use faer::linalg::solvers::SolveLstsq as _;
use faer::{Col, Mat};
use nalgebra::convert;
use ndarray::{Array1, ArrayView1, Axis, Slice};
use uom::si::f64::{Ratio, Time};
use uom::si::frequency::hertz;
use uom::si::ratio::ratio;
use uom::si::time::second;

use crate::{Lagged, Samples, Series, SpectraError};

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEADROOM: f64 = 10.0;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Lundeby {
    pub interval: Time = seconds(0.030),
    pub range: f64 = 20.0,
    pub drop: f64 = 2.0,
    pub iterations: NonZero<u8> = NonZero::new(5).unwrap(),
}

#[derive(Debug, Clone, PartialEq, derive_more::AsRef)]
pub struct Decay {
    #[as_ref]
    curve: Lagged,
    #[as_ref]
    noise: Option<Ratio>,
    onset: usize,
    energy: Array1<f64>,
    remainder: f64,
    attenuation: f64,
    peak: f64,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Reverberation {
    pub time: Time,
    pub nonlinearity: Ratio,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Lateral {
    pub fraction: Ratio,
    pub cosine: Ratio,
    pub level: f64,
}

impl Decay {
    pub fn new(response: &Series, truncation: Option<Lundeby>) -> Result<Self, SpectraError> {
        const ONSET: f64 = 1.0e-2;
        let peak = response.samples.iter().map(|sample| sample.abs()).max_by(f64::total_cmp).filter(|peak| *peak > 0.0).ok_or(SpectraError::Silent)?;
        let padding = response.samples.iter().rev().take_while(|sample| **sample == 0.0).count();
        let powers = response.samples.iter().take(response.samples.len().strict_sub(padding)).map(|sample| (sample / peak).powi(2));
        let onset = powers.clone().take_while(|power| *power < ONSET).count();
        let squared = powers.skip(onset).collect::<Array1<f64>>();
        let crossing = |noise: f64, [intercept, slope]: [f64; 2]| (level(noise) - intercept) / slope;
        let truncated = |lundeby: Lundeby| -> Result<(f64, [f64; 2]), SpectraError> {
            refused(&lundeby).next().is_none().ok_or(SpectraError::Lundeby { lundeby })?;
            let last_tenth = (convert::<usize, f64>(squared.len()) * 0.9).floor();
            let floor = |from: f64| squared.slice_axis(Axis(0), Slice::from(from.min(last_tenth).to_int_saturating::<usize>()..)).mean().filter(|noise| *noise > 0.0);
            let line = |width: f64, noise: f64, span: f64| {
                let size = width.round().to_int_saturating::<usize>().max(1);
                let levels = squared.exact_chunks(size).into_iter().filter_map(|window| window.mean()).map(level).collect::<Array1<f64>>();
                let (loudest, _) = levels.iter().enumerate().max_by(|(_, left), (_, right)| left.total_cmp(right))?;
                let (first, [intercept, slope], _) =
                    regression(levels.slice_axis(Axis(0), Slice::from(loudest..)), RangeInclusive { start: level(noise) + HEADROOM + span, last: level(noise) + HEADROOM })?;
                let spacing = convert::<usize, f64>(size);
                let origin = (convert::<usize, f64>(loudest) + convert::<usize, f64>(first)).mul_add(spacing, (spacing - 1.0) / 2.0);
                Some([intercept - slope * origin / spacing, slope / spacing])
            };
            let estimate = |iteration: u8, noise: Option<f64>, width: f64, span: f64| {
                noise.and_then(|noise| line(width, noise, span).map(|decay| (noise, decay))).ok_or_else(|| SpectraError::Truncation { iteration: usize::from(iteration) })
            };
            let preliminary = estimate(0, floor(last_tenth), (lundeby.interval * *response.rate).get::<ratio>(), f64::INFINITY)?;
            (1..lundeby.iterations.get())
                .try_fold(preliminary, |(noise, decay @ [_, slope]), iteration| {
                    let width = lundeby.drop / -slope;
                    match estimate(iteration, floor(crossing(noise, decay) - HEADROOM / slope), width, lundeby.range) {
                        Ok(state @ (next, late)) if (crossing(next, late) - crossing(noise, decay)).abs() < width => ControlFlow::Break(Ok(state)),
                        Ok(state) => ControlFlow::Continue(state),
                        Err(error) => ControlFlow::Break(Err(error)),
                    }
                })
                .map_continue(Ok)
                .into_value()
        };
        let fitted = truncation.map(truncated).transpose()?;
        let last = squared.len().strict_sub(1);
        let (end, [power, attenuation]) = fitted.map_or((last, [0.0; 2]), |(noise, decay @ [intercept, slope])| {
            let end = crossing(noise, decay).round().to_int_saturating::<usize>().min(last);
            (end, [slope.mul_add(convert::<usize, f64>(end), intercept), slope].map(|decibels| 10.0_f64.powf(decibels / 10.0)))
        });
        let energy = squared.slice_axis_move(Axis(0), Slice::from(..=end));
        let remainder = power * attenuation / (1.0 - attenuation);
        let total = energy.sum() + remainder;
        let mut integral = energy.clone();
        integral.invert_axis(Axis(0));
        integral.accumulate_axis_inplace(Axis(0), |later, sum| *sum += *later);
        integral.invert_axis(Axis(0));
        Ok(Self {
            curve: Lagged {
                lag: convert::<usize, f64>(onset) / *response.rate,
                series: Series { rate: response.rate, samples: Samples::try_new(integral.mapv_into(|sum| (sum + remainder) / total))? },
            },
            noise: fitted.map(|(noise, _)| Ratio::new::<ratio>(noise)),
            onset,
            energy,
            remainder,
            attenuation,
            peak,
        })
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn reverberation(decay: &Decay, range: RangeInclusive<f64>) -> Option<Reverberation> {
    let (floor, levels) = (range.last - HEADROOM, decay.curve.series.samples.mapv(level));
    (decay.noise.is_none_or(|noise| level(noise.get::<ratio>()) <= floor) && levels.last().is_some_and(|end| *end <= floor))
        .then(|| regression(levels.view(), range))
        .flatten()
        .map(|(_, [_, slope], nonlinearity)| Reverberation { time: -60.0 / slope / *decay.curve.series.rate, nonlinearity: Ratio::new::<ratio>(nonlinearity) })
}

pub fn clarity(decay: &Decay, early: Time) -> Result<f64, SpectraError> {
    match split(decay, early)? {
        [early_energy, late_energy] if early_energy > 0.0 && late_energy > 0.0 => Ok(level(early_energy / late_energy)),
        [_, _] => Err(SpectraError::Split { early }),
    }
}

pub fn definition(decay: &Decay, early: Time) -> Result<Ratio, SpectraError> { split(decay, early).map(|[early_energy, late_energy]| Ratio::new::<ratio>(early_energy / (early_energy + late_energy))) }

#[must_use]
pub fn centre(decay: &Decay) -> Time {
    let Decay { curve, noise: _, onset: _, energy, remainder, attenuation, peak: _ } = decay;
    let moment =
        remainder.mul_add(convert::<usize, f64>(energy.len()) + attenuation / (1.0 - attenuation), energy.iter().enumerate().map(|(index, power)| convert::<usize, f64>(index) * power).sum::<f64>());
    moment / (energy.sum() + remainder) / *curve.series.rate
}

pub fn strength(decay: &Decay, reference: &Series) -> Result<f64, SpectraError> {
    free(reference).map(|free| level(decay.peak.powi(2) * (decay.energy.sum() + decay.remainder) / decay.curve.series.rate.get::<hertz>() / free))
}

pub fn lateral(decay: &Decay, figure: &Series, reference: &Series) -> Result<Lateral, SpectraError> {
    const LATERAL: Time = seconds(0.005);
    const EARLY: Time = seconds(0.080);
    matched(decay, figure)?;
    let free = free(reference)?;
    let [start, end] = [LATERAL, EARLY].map(|time| offset(decay, time));
    let [omni, _] = split(decay, EARLY)?;
    let late = energy(figure.samples.iter().skip(end), figure);
    (omni > 0.0 && late > 0.0).ok_or(SpectraError::Split { early: EARLY })?;
    let window = figure.samples.iter().enumerate().take(end).skip(start);
    Ok(Lateral {
        fraction: Ratio::new::<ratio>(window.clone().map(|(_, sample)| sample.powi(2)).sum::<f64>() / (decay.peak.powi(2) * omni)),
        cosine: Ratio::new::<ratio>(window.filter_map(|(index, sample)| decay.energy.get(index.strict_sub(decay.onset)).map(|power| (sample * power.sqrt()).abs())).sum::<f64>() / (decay.peak * omni)),
        level: level(late / free),
    })
}

pub fn interaural(decay: &Decay, left: &Series, right: &Series, window: Range<Time>) -> Result<Ratio, SpectraError> {
    const INTERAURAL: Time = seconds(0.001);
    [left, right].into_iter().try_for_each(|series| matched(decay, series))?;
    let samples = left.samples.len().min(right.samples.len());
    let [start, end] = [window.start, window.end].map(|time| offset(decay, time).min(samples));
    let reach = (INTERAURAL * *left.rate).get::<ratio>().round().to_int_saturating::<isize>();
    let norm = (energy(left.samples.iter().take(end).skip(start), left) * energy(right.samples.iter().take(end).skip(start), right)).sqrt();
    (norm > 0.0).ok_or(SpectraError::Silent)?;
    let correlation = |lag: isize| {
        (start..end).filter_map(|index| index.checked_add_signed(lag).and_then(|shifted| right.samples.get(shifted)).zip(left.samples.get(index)).map(|(later, earlier)| later * earlier)).sum::<f64>()
    };
    Ok(Ratio::new::<ratio>((reach.strict_neg()..=reach).map(|lag| correlation(lag).abs()).fold(0.0, f64::max) / left.rate.get::<hertz>() / norm))
}

pub(crate) fn refused(lundeby: &Lundeby) -> impl Iterator<Item = (&'static str, f64, &'static str)> {
    let Lundeby { interval, range, drop, iterations: _ } = *lundeby;
    iter::once(("interval", interval.get::<second>(), "s"))
        .chain([("range", range), ("drop", drop)].map(|(setting, value)| (setting, value, "dB")))
        .filter(|&(_, value, _)| !(value.is_finite() && value > 0.0))
}

fn matched(decay: &Decay, series: &Series) -> Result<(), SpectraError> { (series.rate == decay.curve.series.rate).ok_or(SpectraError::Rates { rates: [decay.curve.series.rate, series.rate] }) }

fn offset(decay: &Decay, time: Time) -> usize { decay.onset.saturating_add((time * *decay.curve.series.rate).get::<ratio>().round().to_int_saturating::<usize>()) }

fn energy<'sample>(samples: impl Iterator<Item = &'sample f64>, series: &Series) -> f64 { samples.map(|sample| sample.powi(2)).sum::<f64>() / series.rate.get::<hertz>() }

fn free(reference: &Series) -> Result<f64, SpectraError> {
    let free = energy(reference.samples.iter(), reference);
    (free > 0.0).ok_or(SpectraError::Silent).map(|()| free)
}

fn split(decay: &Decay, early: Time) -> Result<[f64; 2], SpectraError> {
    let boundary = (early * *decay.curve.series.rate).get::<ratio>().round();
    (boundary >= 0.0 && boundary.is_finite()).ok_or(SpectraError::Split { early })?;
    let length = convert::<usize, f64>(decay.energy.len());
    let (early_part, late_part) = decay.energy.view().split_at(Axis(0), boundary.to_int_saturating::<usize>().min(decay.energy.len()));
    let beyond = decay.remainder * decay.attenuation.powf((boundary - length).max(0.0));
    Ok([early_part.sum() + decay.remainder - beyond, late_part.sum() + beyond])
}

fn regression(levels: ArrayView1<'_, f64>, range: RangeInclusive<f64>) -> Option<(usize, [f64; 2], f64)> {
    let first = levels.iter().position(|level| *level <= range.start)?;
    let run = levels.iter().skip(first).take_while(|level| **level >= range.last).copied().collect::<Col<f64>>();
    let design = (run.nrows() > 1).then(|| Mat::from_fn(run.nrows(), 2, |row, power| if power == 0 { 1.0 } else { convert::<usize, f64>(row) }))?;
    let fit = design.qr().solve_lstsq(&run);
    let residual = (&run - &design * &fit).squared_norm_l2();
    let spread = (&run - Col::full(run.nrows(), run.sum() / convert::<usize, f64>(run.nrows()))).squared_norm_l2();
    (fit[1] < 0.0).then_some((first, [fit[0], fit[1]], residual / spread))
}

const fn seconds(value: f64) -> Time { Time { dimension: PhantomData, units: PhantomData, value } }

fn level(quotient: f64) -> f64 { 10.0 * quotient.log10() }
