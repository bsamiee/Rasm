use faer::{Col, Mat, linalg::solvers::SolveLstsq as _};
use nalgebra::{convert, convert_unchecked};
use ndarray::{Array1, Axis, s};
use uom::si::{
    f64::{Ratio, Time},
    frequency::hertz,
    ratio::ratio,
    time::second,
};

use crate::{Samples, Series, SpectraError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Reverberation {
    EarlyDecay,
    T20,
    T30,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn curve(response: &Series) -> Result<Series, SpectraError> {
    let energy = energy(response)?;
    let total = energy.iter().rev().sum::<f64>();
    let mut decay = Array1::from_iter(energy.iter().rev().copied());
    decay.accumulate_axis_inplace(Axis(0), |later, power| *power += *later);
    decay.invert_axis(Axis(0));
    Ok(Series { rate: response.rate, samples: Samples::try_new(decay.mapv_into(|tail| tail / total))? })
}

pub fn reverberation(response: &Series, evaluation: Reverberation) -> Result<Option<Time>, SpectraError> {
    let decay = curve(response)?;
    let [upper, lower] = match evaluation {
        Reverberation::EarlyDecay => [0.0, 10.0],
        Reverberation::T20 => [5.0, 25.0],
        Reverberation::T30 => [5.0, 35.0],
    }
    .map(|attenuation| 10.0_f64.powf(-attenuation / 10.0));
    let reached = decay.samples.last().is_some_and(|end| *end <= lower);
    Ok(decay
        .samples
        .iter()
        .position(|tail| *tail <= upper)
        .zip(decay.samples.iter().rposition(|tail| *tail >= lower))
        .filter(|(first, last)| reached && first < last)
        .and_then(|(first, last)| {
            let evaluated = decay.samples.slice(s![first..=last]);
            Mat::from_fn(evaluated.len(), 2, |row, power| convert::<usize, f64>(row).powf(convert::<usize, f64>(power)))
                .qr()
                .solve_lstsq(evaluated.iter().map(|tail| level(*tail)).collect::<Col<f64>>())
                .iter()
                .nth(1)
                .copied()
        })
        .filter(|slope| *slope < 0.0)
        .map(|slope| Time::new::<second>(-60.0 / (slope * response.rate.get::<hertz>()))))
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
    let energy = energy(response)?;
    let moment = energy.iter().enumerate().map(|(index, power)| convert::<usize, f64>(index) * power).sum::<f64>();
    Ok(Time::new::<second>(moment / energy.sum() / response.rate.get::<hertz>()))
}

fn energy(response: &Series) -> Result<Array1<f64>, SpectraError> {
    const ONSET: f64 = 1e-2;
    let peak = response.samples.iter().map(|sample| sample.abs()).max_by(f64::total_cmp).filter(|peak| *peak > 0.0).ok_or(SpectraError::Silent)?;
    Ok(response.samples.iter().map(|sample| (sample / peak).powi(2)).skip_while(|power| *power < ONSET).collect())
}

fn split(response: &Series, early: Time) -> Result<[f64; 2], SpectraError> {
    let energy = energy(response)?;
    let boundary = Some(early.get::<second>() * response.rate.get::<hertz>()).filter(|boundary| *boundary >= 0.0).ok_or(SpectraError::Split { early })?;
    let (early_part, late_part) = energy.view().split_at(Axis(0), convert_unchecked::<f64, usize>(boundary.round().min(convert::<usize, f64>(energy.len()))));
    Ok([early_part.sum(), late_part.sum()])
}

fn level(ratio: f64) -> f64 { 10.0 * ratio.log10() }
