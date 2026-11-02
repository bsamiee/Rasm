#![feature(array_try_map, default_field_values, gen_blocks, iter_array_chunks, iterator_try_collect, type_changing_struct_update)]

use jiff::{SignedDuration, Timestamp, tz::Offset};
use nalgebra::{Unit, UnitQuaternion, Vector3};
use rasm_proximity::ProximityError;
use rasm_site::Place;
use solar_positioning::{Location, PreparedPositions, RefractionCorrection, SolarPosition, SolarPositions, delta_t, time::JulianDate};
use uom::si::{
    angle::{degree, radian},
    f64::Angle,
    length::meter,
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Sun {
    pub place: Place,
    pub refraction: Option<RefractionCorrection> = Some(RefractionCorrection::standard()),
    pub delta_t: Option<SignedDuration> = None,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum SolarError {
    #[error(transparent)]
    Positioning(#[from] solar_positioning::Error),
    #[error(transparent)]
    Time(#[from] jiff::Error),
    #[error(transparent)]
    Proximity(#[from] ProximityError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn position(sun: &Sun, at: Timestamp) -> Result<(PreparedPositions, SolarPosition), SolarError> {
    let prepared = SolarPositions::new().for_time_from_julian(julian(sun, at)?)?;
    Ok((prepared, prepared.at(location(&sun.place), sun.place.elevation.get::<meter>(), sun.refraction)?))
}

#[must_use]
pub fn direction(position: SolarPosition, north: UnitQuaternion<f64>) -> Unit<Vector3<f64>> {
    toward(north, Angle::new::<degree>(position.azimuth()), Angle::new::<degree>(position.elevation_angle()))
}

fn toward(north: UnitQuaternion<f64>, azimuth: Angle, altitude: Angle) -> Unit<Vector3<f64>> {
    north * (UnitQuaternion::from_euler_angles(altitude.get::<radian>(), 0.0, -azimuth.get::<radian>()) * Vector3::y_axis())
}

fn julian(sun: &Sun, at: Timestamp) -> Result<JulianDate, SolarError> {
    let civil = Offset::UTC.to_datetime(at);
    let (year, month) = (i32::from(civil.year()), u32::from(civil.month().unsigned_abs()));
    let delta_t = sun.delta_t.map_or_else(|| delta_t::estimate_from_date(year, month), |stated| Ok(stated.as_secs_f64()))?;
    Ok(JulianDate::from_utc(
        year,
        month,
        u32::from(civil.day().unsigned_abs()),
        u32::from(civil.hour().unsigned_abs()),
        u32::from(civil.minute().unsigned_abs()),
        SignedDuration::new(i64::from(civil.second()), civil.subsec_nanosecond()).as_secs_f64(),
        delta_t,
    )?)
}

fn location(place: &Place) -> Location { Location { latitude: place.latitude.get::<degree>(), longitude: place.longitude.get::<degree>() } }

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod dome;
pub mod envelope;
pub mod events;
pub mod exposure;
