#![feature(array_try_map, float_conversions, iter_array_chunks, type_changing_struct_update)]

use core::range::Range;

use itertools::Itertools as _;
use jiff::{SignedDuration, Timestamp, tz::Offset};
use nalgebra::{UnitQuaternion, UnitVector3, Vector3};
use nonempty_collections::NEVec;
use rasm_proximity::ProximityError;
use rasm_site::Place;
use solar_positioning::{Location, PreparedPositions, RefractionCorrection, SolarPosition, SolarPositions, delta_t, time::JulianDate};
use uom::si::{
    angle::{degree, radian},
    f64::{Angle, Length, Ratio, ThermodynamicTemperature},
    length::meter,
    ratio::ratio,
    thermodynamic_temperature::{degree_celsius, kelvin},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, derive_more::Display)]
pub enum Operation {
    #[display("standard atmosphere at {} m", _0.get::<meter>())]
    Atmosphere(Length),
    #[display("ΔT estimate at {_0}")]
    DeltaT(Timestamp),
    #[display("position at {_0}")]
    Position(Timestamp),
    #[display("search from {} to {}", _0.start, _0.end)]
    Search(Range<Timestamp>),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Input {
    #[display("non-finite cell {_0}")]
    Cell(usize),
    #[display("non-finite edge {_0}")]
    Edge(usize),
    #[display("non-finite direction {_0}")]
    Direction(usize),
    #[display("negative or non-finite duration {_0}")]
    Duration(usize),
    #[display("non-finite cap")]
    Cap,
    #[display("non-positive step {_0}")]
    Step(SignedDuration),
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Refraction {
    Standard,
    Stated(RefractionCorrection),
}

#[derive(Debug, Clone, PartialEq, derive_more::AsRef)]
pub struct Sun {
    #[as_ref]
    place: Place,
    #[as_ref]
    location: Location,
    #[as_ref]
    refraction: Option<RefractionCorrection>,
    #[as_ref]
    delta_t: Option<SignedDuration>,
}

impl Sun {
    pub fn new(place: Place, refraction: Option<Refraction>, delta_t: Option<SignedDuration>) -> Result<Self, SolarError> {
        let elevation = *place.elevation;
        let refraction = refraction
            .map(|refraction| match refraction {
                Refraction::Standard => {
                    let sea = RefractionCorrection::standard();
                    let (temperature, pressure) = lapsed(ThermodynamicTemperature::new::<degree_celsius>(sea.temperature()), elevation);
                    RefractionCorrection::new(sea.pressure() * pressure.get::<ratio>(), temperature.get::<degree_celsius>()).map_err(positioning(Operation::Atmosphere(elevation)))
                }
                Refraction::Stated(correction) => Ok(correction),
            })
            .transpose()?;
        let location = Location { latitude: place.position.latitude().get::<degree>(), longitude: place.position.longitude().get::<degree>() };
        Ok(Self { place, location, refraction, delta_t })
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum SolarError {
    #[error("solar-positioning refused the {operation}")]
    Positioning { operation: Operation, source: solar_positioning::Error },
    #[error("refused {}", inputs.iter().format(", "))]
    Refused { inputs: NEVec<Input> },
    #[error(transparent)]
    Time(#[from] jiff::Error),
    #[error(transparent)]
    Proximity(#[from] ProximityError),
}

impl From<NEVec<Input>> for SolarError {
    fn from(value: NEVec<Input>) -> Self { Self::Refused { inputs: value } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn position(sun: &Sun, at: Timestamp) -> Result<(PreparedPositions, SolarPosition), SolarError> { prepared(sun, julian(sun, at)?).map_err(positioning(Operation::Position(at))) }

#[must_use]
pub fn direction(azimuth: Angle, altitude: Angle, north: UnitQuaternion<f64>) -> UnitVector3<f64> {
    north * (UnitQuaternion::from_euler_angles(altitude.get::<radian>(), 0.0, -azimuth.get::<radian>()) * Vector3::y_axis())
}

#[must_use]
pub fn lapsed(temperature: ThermodynamicTemperature, rise: Length) -> (ThermodynamicTemperature, Ratio) {
    const LAPSE_RATE: f64 = 0.0065;
    const BAROMETRIC_EXPONENT: f64 = 5.25588;
    let base = temperature.get::<kelvin>();
    let lifted = (-LAPSE_RATE).mul_add(rise.get::<meter>(), base);
    (ThermodynamicTemperature::new::<kelvin>(lifted), Ratio::new::<ratio>((lifted / base).powf(BAROMETRIC_EXPONENT)))
}

pub fn at(interval: Range<Timestamp>, share: f64) -> Result<Timestamp, SolarError> {
    SignedDuration::try_from_secs_f64(interval.end.duration_since(interval.start).as_secs_f64() * share).and_then(|offset| interval.start.checked_add(offset)).map_err(SolarError::Time)
}

fn prepared(sun: &Sun, date: JulianDate) -> Result<(PreparedPositions, SolarPosition), solar_positioning::Error> {
    let prepared = SolarPositions::new().for_time_from_julian(date)?;
    Ok((prepared, prepared.at(sun.location, sun.place.elevation.get::<meter>(), sun.refraction)?))
}

fn julian(sun: &Sun, at: Timestamp) -> Result<JulianDate, SolarError> {
    let civil = Offset::UTC.to_datetime(at);
    let (year, month) = (i32::from(civil.year()), u32::from(civil.month().unsigned_abs()));
    let delta_t = sun.delta_t.map_or_else(|| delta_t::estimate_from_date(year, month).map_err(positioning(Operation::DeltaT(at))), |stated| Ok(stated.as_secs_f64()))?;
    JulianDate::from_utc(
        year,
        month,
        u32::from(civil.day().unsigned_abs()),
        u32::from(civil.hour().unsigned_abs()),
        u32::from(civil.minute().unsigned_abs()),
        SignedDuration::new(i64::from(civil.second()), civil.subsec_nanosecond()).as_secs_f64(),
        delta_t,
    )
    .map_err(positioning(Operation::Position(at)))
}

const fn positioning(operation: Operation) -> impl Fn(solar_positioning::Error) -> SolarError + Copy { move |source| SolarError::Positioning { operation, source } }

fn finite(values: &[f64]) -> bool { values.iter().all(|value| value.is_finite()) }

fn refused<I>(inputs: I) -> Result<(), SolarError>
where I: IntoIterator<Item = Input> {
    NEVec::try_from_vec(inputs.into_iter().collect()).map_or(Ok(()), |inputs| Err(inputs.into()))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod dome;
pub mod envelope;
pub mod events;
pub mod exposure;
