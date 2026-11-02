use core::{iter, range::Range};

use jiff::{SignedDuration, Timestamp, Zoned};
use nalgebra::{UnitQuaternion, UnitVector3};
use solar_positioning::{ElevationTarget, Events, Horizon, SolarEvents, SolarPositions, time::JulianDate};
use uom::si::{
    angle::degree,
    f64::{Angle, Time},
    length::meter,
    time::second,
};

use crate::{Operation, SolarError, Sun, direction, julian, position, positioning, prepared};

// --- [CONSTANTS] -----------------------------------------------------------------------

const DAY: SignedDuration = SignedDuration::from_hours(24);

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn events(sun: &Sun, interval: Range<Timestamp>, horizon: Horizon) -> Result<Events<Zoned>, SolarError> {
    let (start, end) = julian_interval(sun, interval)?;
    let [first, last] = [interval.start, interval.end].map(|at| at.to_zoned(sun.place.zone.clone()));
    let found = SolarEvents::new().for_interval_from_julian(start.julian_date(), end, sun.location, start.delta_t(), horizon).map_err(positioning(Operation::Search(interval)))?;
    let [rises, transits, sets] = [found.rises, found.transits, found.sets].try_map(|dates| dates.into_iter().map(|date| instant(sun, interval, start, date)).try_collect::<Vec<_>>())?;
    Ok(Events { start: first, end: last, rises, transits, sets, ..found })
}

pub fn windows(sun: &Sun, interval: Range<Timestamp>, elevation: Angle) -> Result<Vec<Range<Zoned>>, SolarError> {
    let (start, end) = julian_interval(sun, interval)?;
    let [first, last] = [interval.start, interval.end].map(|at| at.to_zoned(sun.place.zone.clone()));
    let searched = positioning(Operation::Search(interval));
    let target = ElevationTarget { angle: elevation.get::<degree>(), height: sun.place.elevation.get::<meter>(), refraction: sun.refraction };
    let (_, initial) = prepared(sun, start).map_err(searched)?;
    let lit = initial.elevation_angle() >= target.angle;
    let positions = SolarPositions::new();
    let step = |(date, rising): (f64, bool)| {
        (if rising { SolarPositions::next_elevation_rise_from_julian } else { SolarPositions::next_elevation_set_from_julian })(&positions, date, end, sun.location, start.delta_t(), target)
            .map(|next| next.map(|next| (next, !rising)))
    };
    let crossings = iter::successors(Some(step((start.julian_date(), !lit))), |previous| previous.as_ref().ok().copied().flatten().map(step))
        .map_while(Result::transpose)
        .try_collect::<Vec<_>>()
        .map_err(searched)?;
    let lit_at_end = lit == crossings.len().is_multiple_of(2);
    let inner = crossings.into_iter().map(|(date, _)| instant(sun, interval, start, date)).try_collect::<Vec<_>>()?;
    Ok(iter::chain(lit.then_some(first), inner)
        .chain(lit_at_end.then_some(last))
        .array_chunks::<2>()
        .map(|[rise, set]| Range { start: rise, end: set })
        .filter(|window| window.start < window.end)
        .collect())
}

pub fn path(sun: &Sun, north: UnitQuaternion<f64>, interval: Range<Timestamp>, step: SignedDuration) -> Result<Vec<(UnitVector3<f64>, Time)>, SolarError> {
    let width = step.is_positive().then(|| step.as_secs_f64()).ok_or(SolarError::Step { step })?;
    windows(sun, interval, Angle::new::<degree>(0.0))?
        .into_iter()
        .flat_map(|window| {
            let [lit, dark] = [window.start, window.end].map(|at| at.timestamp().duration_since(interval.start).as_secs_f64());
            ((lit / width).floor().to_int_saturating::<usize>()..(dark / width).ceil().to_int_saturating::<usize>()).map(move |cell| {
                let lower = nalgebra::convert::<usize, f64>(cell) * width;
                [lower.max(lit), (lower + width).min(dark)]
            })
        })
        .filter(|[from, to]| from < to)
        .map(|[from, to]| {
            let at = SignedDuration::try_from_secs_f64(from.midpoint(to)).and_then(|offset| interval.start.checked_add(offset))?;
            let (_, sampled) = position(sun, at)?;
            Ok((direction(sampled, north), Time::new::<second>(to - from)))
        })
        .try_collect()
}

fn julian_interval(sun: &Sun, interval: Range<Timestamp>) -> Result<(JulianDate, f64), SolarError> {
    let start = julian(sun, interval.start)?;
    Ok((start, start.julian_date() + interval.end.duration_since(interval.start).div_duration_f64(DAY)))
}

fn instant(sun: &Sun, interval: Range<Timestamp>, start: JulianDate, date: f64) -> Result<Zoned, jiff::Error> {
    SignedDuration::try_from_secs_f64((date - start.julian_date()) * DAY.as_secs_f64()).and_then(|offset| interval.start.checked_add(offset)).map(|at| at.to_zoned(sun.place.zone.clone()))
}
