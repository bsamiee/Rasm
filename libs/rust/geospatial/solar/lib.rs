#![feature(array_try_map, float_conversions, iter_array_chunks, iterator_try_collect, type_changing_struct_update)]

use core::range::Range;

use itertools::Itertools as _;
use jiff::{SignedDuration, Timestamp, tz::Offset};
use nalgebra::{UnitQuaternion, UnitVector3, Vector3};
use nonempty_collections::NEVec;
use rasm_proximity::ProximityError;
use rasm_site::Place;
use solar_positioning::{Location, PreparedPositions, RefractionCorrection, SolarPosition, SolarPositions, delta_t, time::JulianDate};
use uom::si::{
    angle::degree,
    f64::{Length, ThermodynamicTemperature},
    length::meter,
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
    #[display("non-finite sun {_0}")]
    Sun(usize),
    #[display("non-finite direction {_0}")]
    Direction(usize),
    #[display("negative or non-finite duration {_0}")]
    Duration(usize),
    #[display("non-finite cap")]
    Cap,
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
        const LAPSE_RATE: f64 = 0.0065;
        const BAROMETRIC_EXPONENT: f64 = 5.25588;
        let elevation = *place.elevation;
        let kelvins = |celsius| ThermodynamicTemperature::new::<degree_celsius>(celsius).get::<kelvin>();
        let refraction = refraction
            .map(|refraction| match refraction {
                Refraction::Standard => {
                    let sea = RefractionCorrection::standard();
                    let temperature = (-LAPSE_RATE).mul_add(elevation.get::<meter>(), sea.temperature());
                    RefractionCorrection::new(sea.pressure() * (kelvins(temperature) / kelvins(sea.temperature())).powf(BAROMETRIC_EXPONENT), temperature)
                        .map_err(positioning(Operation::Atmosphere(elevation)))
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
    #[error("sampling step {step} is not positive")]
    Step { step: SignedDuration },
    #[error("refused {}", inputs.iter().format(", "))]
    Refused { inputs: NEVec<Input> },
    #[error(transparent)]
    Time(#[from] jiff::Error),
    #[error(transparent)]
    Proximity(#[from] ProximityError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn position(sun: &Sun, at: Timestamp) -> Result<(PreparedPositions, SolarPosition), SolarError> { prepared(sun, julian(sun, at)?).map_err(positioning(Operation::Position(at))) }

#[must_use]
pub fn direction(position: SolarPosition, north: UnitQuaternion<f64>) -> UnitVector3<f64> { toward(north, position.azimuth().to_radians(), position.elevation_angle().to_radians()) }

fn toward(north: UnitQuaternion<f64>, azimuth: f64, altitude: f64) -> UnitVector3<f64> { north * (UnitQuaternion::from_euler_angles(altitude, 0.0, -azimuth) * Vector3::y_axis()) }

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
    NEVec::try_from_vec(inputs.into_iter().collect()).map_or(Ok(()), |inputs| Err(SolarError::Refused { inputs }))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod dome;
pub mod envelope;
pub mod events;
pub mod exposure;
