use core::{iter, range::Range};

use jiff::{SignedDuration, Timestamp, Zoned};
use solar_positioning::{ElevationTarget, Events, Horizon, SolarEvents, SolarPositions, time::JulianDate};
use uom::si::{angle::degree, f64::Angle, length::meter};

use crate::{SolarError, Sun, julian, location, position};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn events(sun: &Sun, interval: Range<Timestamp>, horizon: Horizon) -> Result<Events<Zoned>, SolarError> {
    let start = julian(sun, interval.start)?;
    let (end, instant) = span(sun, interval, start);
    let found = SolarEvents::new().for_interval_from_julian(start.julian_date(), end, location(&sun.place), start.delta_t(), horizon)?;
    let [rises, transits, sets] = [found.rises, found.transits, found.sets].try_map(|dates| dates.into_iter().map(&instant).try_collect::<Vec<_>>())?;
    Ok(Events { start: interval.start.to_zoned(sun.place.zone.clone()), end: interval.end.to_zoned(sun.place.zone.clone()), rises, transits, sets, ..found })
}

pub fn above(sun: &Sun, interval: Range<Timestamp>, elevation: Angle) -> Result<Vec<Range<Zoned>>, SolarError> {
    let start = julian(sun, interval.start)?;
    let (end, instant) = span(sun, interval, start);
    let (location, target) = (location(&sun.place), ElevationTarget { angle: elevation.get::<degree>(), height: sun.place.elevation.get::<meter>(), refraction: sun.refraction });
    let (_, initial) = position(sun, interval.start)?;
    let lit = initial.elevation_angle() >= target.angle;
    let crossings = gen move {
        let (mut date, mut rising) = (start.julian_date(), !lit);
        loop {
            let search = if rising { SolarPositions::next_elevation_rise_from_julian } else { SolarPositions::next_elevation_set_from_julian };
            match search(&SolarPositions::new(), date, end, location, start.delta_t(), target) {
                Ok(Some(next)) => {
                    yield Ok(next);
                    (date, rising) = (next, !rising);
                }
                Ok(None) => break,
                Err(error) => {
                    yield Err(error);
                    break;
                }
            }
        }
    }
    .try_collect::<Vec<_>>()?;
    let lit_at_end = lit == crossings.len().is_multiple_of(2);
    let inner = crossings.into_iter().map(instant).try_collect::<Vec<_>>()?;
    let [first, last] = [interval.start, interval.end].map(|at| at.to_zoned(sun.place.zone.clone()));
    Ok(iter::chain(lit.then_some(first), inner)
        .chain(lit_at_end.then_some(last))
        .array_chunks::<2>()
        .map(|[rise, set]| Range { start: rise, end: set })
        .filter(|window| window.start < window.end)
        .collect())
}

fn span(sun: &Sun, interval: Range<Timestamp>, start: JulianDate) -> (f64, impl Fn(f64) -> Result<Zoned, jiff::Error>) {
    let day = SignedDuration::from_hours(24);
    (start.julian_date() + interval.end.duration_since(interval.start).div_duration_f64(day), move |date: f64| {
        SignedDuration::try_from_secs_f64((date - start.julian_date()) * day.as_secs_f64()).and_then(|offset| interval.start.checked_add(offset)).map(|at| at.to_zoned(sun.place.zone.clone()))
    })
}
