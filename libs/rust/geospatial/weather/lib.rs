#![feature(default_field_values, error_reporter)]

use core::{fmt, range::Range};
use std::error::Report;

use grib::GribError;
use itertools::Itertools as _;
use jiff::Timestamp;
use nonempty_collections::NEVec;
use rasm_site::SiteError;
use rasm_solar::{SolarError, Sun};
use solar_positioning::{PreparedPositions, SolarPosition};
use thermalcomfort::environment::ScaleWindSpeedLogError;
use uom::si::{
    f64::{Angle, HeatFluxDensity, Illuminance, Length, Luminance, Pressure, Ratio, ThermodynamicTemperature, Time, Velocity},
    ratio::ratio,
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Ceiling {
    Height(Length),
    Unlimited,
    Cirroform,
}

#[derive(Debug, Clone, Copy, PartialEq, derive_more::Display)]
pub enum Refusal {
    #[display("hour {_0} spans an empty or reversed interval")]
    Interval(usize),
    #[display("hour {_0} holds a non-finite quantity")]
    NonFinite(usize),
    #[display("hour {_0} lacks a value the operation reads")]
    Incomplete(usize),
    #[display("hour {_0} spans an interval unequal to the first or dividing no hour")]
    Step(usize),
    #[display("quantile {} lies outside 0 to 1", _0.get::<ratio>())]
    Quantile(Ratio),
    #[display("wind rose speed edges are not positive and strictly rising")]
    Edges,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Record {
    #[display("header {_0}")]
    Header(epw::Keyword),
    #[display("row {_0}")]
    Row(u64),
    #[display("message {_0}")]
    Message(usize),
}

#[derive(Debug, thiserror::Error)]
pub enum Malformed {
    #[error(transparent)]
    Csv(#[from] csv::Error),
    #[error(transparent)]
    Time(#[from] jiff::Error),
    #[error(transparent)]
    Grib(#[from] GribError),
    #[error("required record is absent")]
    Absent,
    #[error("message states no valid time")]
    Undated,
    #[error("field lies on no latitude–longitude lattice of at least two points per axis")]
    Grid,
    #[error("site lies outside the lattice")]
    Outside,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Hour {
    pub interval: Range<Timestamp>,
    pub dry_bulb: Option<ThermodynamicTemperature> = None,
    pub dew_point: Option<ThermodynamicTemperature> = None,
    pub relative_humidity: Option<Ratio> = None,
    pub pressure: Option<Pressure> = None,
    pub infrared: Option<HeatFluxDensity> = None,
    pub global_irradiance: Option<HeatFluxDensity> = None,
    pub direct_irradiance: Option<HeatFluxDensity> = None,
    pub diffuse_irradiance: Option<HeatFluxDensity> = None,
    pub global_illuminance: Option<Illuminance> = None,
    pub direct_illuminance: Option<Illuminance> = None,
    pub diffuse_illuminance: Option<Illuminance> = None,
    pub zenith_luminance: Option<Luminance> = None,
    pub wind_direction: Option<Angle> = None,
    pub wind_speed: Option<Velocity> = None,
    pub total_sky_cover: Option<Ratio> = None,
    pub opaque_sky_cover: Option<Ratio> = None,
    pub visibility: Option<Length> = None,
    pub ceiling: Option<Ceiling> = None,
    pub present_weather: Option<[u8; 9]> = None,
    pub precipitable_water: Option<Length> = None,
    pub aerosol_optical_depth: Option<Ratio> = None,
    pub snow_depth: Option<Length> = None,
    pub since_snowfall: Option<Time> = None,
    pub albedo: Option<Ratio> = None,
    pub precipitation: Option<Length> = None,
    pub precipitation_period: Option<Time> = None,
}

#[derive(Debug, Clone, PartialEq, derive_more::AsRef, derive_more::Into)]
pub struct Hours {
    #[as_ref]
    sun: Sun,
    #[as_ref([Hour])]
    hours: Vec<Hour>,
}

impl Hours {
    pub fn try_new(sun: Sun, hours: Vec<Hour>) -> Result<Self, WeatherError> {
        refused(
            hours
                .iter()
                .enumerate()
                .flat_map(|(index, hour)| {
                    let Hour {
                        interval,
                        dry_bulb,
                        dew_point,
                        relative_humidity,
                        pressure,
                        infrared,
                        global_irradiance,
                        direct_irradiance,
                        diffuse_irradiance,
                        global_illuminance,
                        direct_illuminance,
                        diffuse_illuminance,
                        zenith_luminance,
                        wind_direction,
                        wind_speed,
                        total_sky_cover,
                        opaque_sky_cover,
                        visibility,
                        ceiling,
                        present_weather: _,
                        precipitable_water,
                        aerosol_optical_depth,
                        snow_depth,
                        since_snowfall,
                        albedo,
                        precipitation,
                        precipitation_period,
                    } = hour;
                    let height = ceiling.and_then(|ceiling| match ceiling {
                        Ceiling::Height(height) => Some(height),
                        Ceiling::Unlimited | Ceiling::Cirroform => None,
                    });
                    let values = [
                        dry_bulb.map(|quantity| quantity.value),
                        dew_point.map(|quantity| quantity.value),
                        relative_humidity.map(|quantity| quantity.value),
                        pressure.map(|quantity| quantity.value),
                        infrared.map(|quantity| quantity.value),
                        global_irradiance.map(|quantity| quantity.value),
                        direct_irradiance.map(|quantity| quantity.value),
                        diffuse_irradiance.map(|quantity| quantity.value),
                        global_illuminance.map(|quantity| quantity.value),
                        direct_illuminance.map(|quantity| quantity.value),
                        diffuse_illuminance.map(|quantity| quantity.value),
                        zenith_luminance.map(|quantity| quantity.value),
                        wind_direction.map(|quantity| quantity.value),
                        wind_speed.map(|quantity| quantity.value),
                        total_sky_cover.map(|quantity| quantity.value),
                        opaque_sky_cover.map(|quantity| quantity.value),
                        visibility.map(|quantity| quantity.value),
                        height.map(|quantity| quantity.value),
                        precipitable_water.map(|quantity| quantity.value),
                        aerosol_optical_depth.map(|quantity| quantity.value),
                        snow_depth.map(|quantity| quantity.value),
                        since_snowfall.map(|quantity| quantity.value),
                        albedo.map(|quantity| quantity.value),
                        precipitation.map(|quantity| quantity.value),
                        precipitation_period.map(|quantity| quantity.value),
                    ];
                    [(interval.start >= interval.end).then_some(Refusal::Interval(index)), values.into_iter().flatten().any(|value| !value.is_finite()).then_some(Refusal::NonFinite(index))]
                })
                .flatten(),
        )
        .map(|()| Self { sun, hours })
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum WeatherError {
    #[error("refused {}", .refusals.iter().format(", "))]
    Refused { refusals: NEVec<Refusal> },
    #[error("malformed records{}", fmt::from_fn(|formatter| .records.iter().try_for_each(|(record, malformed)| write!(formatter, "\n{record}: {}", Report::new(malformed)))))]
    Records { records: NEVec<(Record, Malformed)> },
    #[error(transparent)]
    Csv(#[from] csv::Error),
    #[error(transparent)]
    Grib(#[from] GribError),
    #[error(transparent)]
    Wind(#[from] ScaleWindSpeedLogError),
    #[error(transparent)]
    Solar(#[from] SolarError),
    #[error(transparent)]
    Site(#[from] SiteError),
    #[error(transparent)]
    Time(#[from] jiff::Error),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn position(hours: &Hours, hour: &Hour) -> Result<(PreparedPositions, SolarPosition), WeatherError> {
    let Range { start, end } = hour.interval;
    start.checked_add(end.duration_since(start).div_f64(2.0)).map_err(WeatherError::Time).and_then(|midpoint| rasm_solar::position(&hours.sun, midpoint).map_err(WeatherError::Solar))
}

fn refused<I>(refusals: I) -> Result<(), WeatherError>
where I: IntoIterator<Item = Refusal> {
    NEVec::try_from_vec(refusals.into_iter().collect()).map_or(Ok(()), |refusals| Err(WeatherError::Refused { refusals }))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod climate;
pub mod comfort;
pub mod design;
pub mod epw;
pub mod gridded;
pub mod sky;
