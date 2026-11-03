use core::{iter, num::NonZero, range::Range};

use gauss_quad::GaussLegendre;
use itertools::Itertools as _;
use jiff::{SignedDuration, Span, Timestamp, Zoned};
use nalgebra::{UnitQuaternion, UnitVector3};
use nonempty_collections::nev;
use solar_positioning::{ElevationTarget, Events, Horizon, PreparedPositions, SolarEvents, SolarPosition, SolarPositions, time::JulianDate};
use uom::si::{
    angle::degree,
    f64::{Angle, Time},
    length::meter,
    time::second,
};

use crate::{Input, Operation, SolarError, Sun, at, direction, julian, position, positioning, prepared};

// --- [CONSTANTS] -----------------------------------------------------------------------

const DAY: SignedDuration = SignedDuration::from_hours(24);

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn events(sun: &Sun, interval: Range<Timestamp>, horizon: Horizon) -> Result<Events<Zoned>, SolarError> {
    let (start, end) = julian_interval(sun, interval)?;
    let zoned = |at: Timestamp| at.to_zoned(sun.place.zone.clone());
    let found = SolarEvents::new().for_interval_from_julian(start.julian_date(), end, sun.location, start.delta_t(), horizon).map_err(positioning(Operation::Search(interval)))?;
    let [rises, transits, sets] = [found.rises, found.transits, found.sets].try_map(|dates| dates.into_iter().map(|date| instant(interval, start, date).map(zoned)).collect::<Result<Vec<_>, _>>())?;
    Ok(Events { start: zoned(interval.start), end: zoned(interval.end), rises, transits, sets, ..found })
}

pub fn windows(sun: &Sun, interval: Range<Timestamp>, elevation: Angle) -> Result<Vec<Range<Timestamp>>, SolarError> {
    let (start, end) = julian_interval(sun, interval)?;
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
        .collect::<Result<Vec<_>, _>>()
        .map_err(searched)?;
    let lit_at_end = lit == crossings.len().is_multiple_of(2);
    let inner = crossings.into_iter().map(|(date, _)| instant(interval, start, date)).collect::<Result<Vec<_>, _>>()?;
    Ok(iter::chain(lit.then_some(interval.start), inner)
        .chain(lit_at_end.then_some(interval.end))
        .array_chunks::<2>()
        .map(|[rise, set]| Range { start: rise, end: set })
        .filter(|window| window.start < window.end)
        .collect())
}

pub fn sampled<I>(sun: &Sun, cells: I, rule: &GaussLegendre) -> Result<Vec<(usize, PreparedPositions, SolarPosition, Time)>, SolarError>
where I: IntoIterator<Item = Range<Timestamp>> {
    let cells = cells.into_iter().collect_vec();
    let Some((start, end)) = cells.iter().map(|cell| cell.start).min().zip(cells.iter().map(|cell| cell.end).max()) else {
        return Ok(Vec::new());
    };
    let lit = windows(sun, Range { start, end }, Angle::default())?;
    cells
        .into_iter()
        .enumerate()
        .flat_map(|(index, cell)| {
            lit.iter()
                .skip(lit.partition_point(|window| window.end <= cell.start))
                .take_while(move |window| window.start < cell.end)
                .map(move |window| (index, Range { start: window.start.max(cell.start), end: window.end.min(cell.end) }))
        })
        .cartesian_product(rule.as_node_weight_pairs())
        .map(|((index, part), &(node, weight))| -> Result<_, SolarError> {
            let (prepared, sampled) = position(sun, at(part, node.midpoint(1.0))?)?;
            Ok((index, prepared, sampled, Time::new::<second>(part.end.duration_since(part.start).as_secs_f64() * weight / 2.0)))
        })
        .collect()
}

pub fn path(sun: &Sun, north: UnitQuaternion<f64>, interval: Range<Timestamp>, step: SignedDuration) -> Result<Vec<(UnitVector3<f64>, Time)>, SolarError> {
    step.is_positive().ok_or(nev![Input::Step(step)])?;
    let cells = interval.start.series(Span::try_from(step)?).take_while(|at| *at < interval.end).chain([interval.end]).tuple_windows().map(|(start, end)| Range { start, end });
    Ok(sampled(sun, cells, &GaussLegendre::new(NonZero::<usize>::MIN))?
        .into_iter()
        .map(|(_, _, sampled, duration)| (direction(Angle::new::<degree>(sampled.azimuth()), Angle::new::<degree>(sampled.elevation_angle()), north), duration))
        .collect())
}

fn julian_interval(sun: &Sun, interval: Range<Timestamp>) -> Result<(JulianDate, f64), SolarError> {
    let start = julian(sun, interval.start)?;
    Ok((start, start.julian_date() + interval.end.duration_since(interval.start).div_duration_f64(DAY)))
}

fn instant(interval: Range<Timestamp>, start: JulianDate, date: f64) -> Result<Timestamp, jiff::Error> {
    SignedDuration::try_from_secs_f64((date - start.julian_date()) * DAY.as_secs_f64()).and_then(|offset| interval.start.checked_add(offset))
}
