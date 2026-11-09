use core::{iter, num::NonZero, range::Range};

use gauss_quad::GaussLegendre;
use itertools::{Either, Itertools as _};
use jiff::{SignedDuration, Span, Timestamp, Zoned};
use nalgebra::{UnitQuaternion, UnitVector3};
use nonempty_collections::nev;
use solar_positioning::{ElevationTarget, Events, HorizonState, PreparedPositions, SolarEvents, SolarPosition, SolarPositions, time::JulianDate};
use uom::si::{
    angle::degree,
    f64::{Angle, Time},
    length::meter,
    time::second,
};

use crate::{Input, Operation, SolarError, Sun, at, direction, julian, location, position, positioning, prepared};

// --- [CONSTANTS] -----------------------------------------------------------------------

const DAY: SignedDuration = SignedDuration::from_hours(24);

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn events(sun: &Sun, interval: Range<Timestamp>, elevation: Angle) -> Result<Events<Zoned>, SolarError> {
    let (start, end) = julian_interval(sun, interval)?;
    let zoned = |at: Timestamp| at.to_zoned(sun.place.zone.clone());
    let searched = positioning(Operation::Search(interval));
    let (lit, crossings) = crossings(sun, interval, elevation)?;
    let next = |date: f64| SolarEvents::new().next_transit_from_julian(date, end, location(sun).longitude, start.delta_t());
    let transits = iter::successors(Some(next(start.julian_date())), |previous| previous.as_ref().ok().copied().flatten().map(next))
        .map_while(Result::transpose)
        .map(|transit| transit.map_err(searched).and_then(|date| instant(interval, start, date).map(zoned).map_err(SolarError::Time)))
        .collect::<Result<Vec<_>, _>>()?;
    let (rises, sets) = crossings.into_iter().partition_map(|(at, rising)| if rising { Either::Left(zoned(at)) } else { Either::Right(zoned(at)) });
    Ok(Events { start: zoned(interval.start), end: zoned(interval.end), state_at_start: if lit { HorizonState::Above } else { HorizonState::Below }, rises, transits, sets })
}

pub fn windows(sun: &Sun, interval: Range<Timestamp>, elevation: Angle) -> Result<Vec<Range<Timestamp>>, SolarError> {
    let (lit, crossings) = crossings(sun, interval, elevation)?;
    Ok(iter::chain(lit.then_some(interval.start), crossings.into_iter().map(|(at, _)| at))
        .chain([interval.end])
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

fn crossings(sun: &Sun, interval: Range<Timestamp>, elevation: Angle) -> Result<(bool, Vec<(Timestamp, bool)>), SolarError> {
    let (start, end) = julian_interval(sun, interval)?;
    let searched = positioning(Operation::Search(interval));
    let target = ElevationTarget { angle: elevation.get::<degree>(), height: sun.place.elevation.get::<meter>(), refraction: sun.refraction };
    let (_, initial) = prepared(sun, start).map_err(searched)?;
    let lit = initial.elevation_angle() >= target.angle;
    let positions = SolarPositions::new();
    let step = |(date, rising): (f64, bool)| {
        (if rising { SolarPositions::next_elevation_rise_from_julian } else { SolarPositions::next_elevation_set_from_julian })(&positions, date, end, location(sun), start.delta_t(), target)
            .map(|next| next.map(|next| (next, !rising)))
    };
    iter::successors(Some(step((start.julian_date(), !lit))), |previous| previous.as_ref().ok().copied().flatten().map(step))
        .map_while(Result::transpose)
        .map(|crossing| crossing.map_err(searched).and_then(|(date, rising)| instant(interval, start, date).map(|at| (at, !rising)).map_err(SolarError::Time)))
        .collect::<Result<Vec<_>, _>>()
        .map(|crossings| (lit, crossings))
}

fn julian_interval(sun: &Sun, interval: Range<Timestamp>) -> Result<(JulianDate, f64), SolarError> {
    let start = julian(sun, interval.start)?;
    Ok((start, start.julian_date() + interval.end.duration_since(interval.start).div_duration_f64(DAY)))
}

fn instant(interval: Range<Timestamp>, start: JulianDate, date: f64) -> Result<Timestamp, jiff::Error> {
    SignedDuration::try_from_secs_f64((date - start.julian_date()) * DAY.as_secs_f64()).and_then(|offset| interval.start.checked_add(offset))
}
