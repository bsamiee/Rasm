#![feature(default_field_values, error_reporter, float_conversions)]

use core::{fmt, marker::PhantomData, range::Range};
use std::error::Report;

use grib::GribError;
use itertools::Itertools as _;
use jiff::{SignedDuration, Span, Timestamp};
use nonempty_collections::NEVec;
use rasm_analysis::AnalysisError;
use rasm_solar::{SolarError, Sun};
use solar_positioning::{PreparedPositions, SolarPosition};
use thermalcomfort::environment::ScaleWindSpeedLogError;
use uom::si::{
    self, Quantity, SI,
    angle::radian,
    f64::{Angle, HeatFluxDensity, Illuminance, Length, Luminance, Pressure, Ratio, ThermodynamicTemperature, Time, Velocity},
    heat_flux_density::watt_per_square_meter,
    ratio::ratio,
    thermodynamic_temperature::kelvin,
    time::second,
};
use validated::Validated;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Ceiling {
    Height(Length),
    Unlimited,
    Cirroform,
}

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
    records: Vec<Hour>,
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
                        present_weather,
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
                    [
                        interval.is_empty().then_some(Refusal::Interval(index)),
                        values.into_iter().flatten().any(|value| !value.is_finite()).then_some(Refusal::NonFinite(index)),
                        present_weather.is_some_and(|digits| digits.iter().any(|digit| *digit > 9)).then_some(Refusal::Weather(index)),
                    ]
                })
                .flatten(),
        )
        .map(|()| Self { sun, records: hours })
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, thiserror::Error)]
pub enum Refusal {
    #[error("hour {0} spans an empty or reversed interval")]
    Interval(usize),
    #[error("hour {0} holds a non-finite quantity")]
    NonFinite(usize),
    #[error("hour {0} holds a present weather digit above 9")]
    Weather(usize),
    #[error("hour {0} lacks a value the operation reads")]
    Incomplete(usize),
    #[error("hour {0} spans an interval unequal to the first, dividing no hour or ending between steps")]
    Step(usize),
    #[error("step {step} is not positive or leaves a remainder of the run from {start}")]
    Resolution { start: Timestamp, step: SignedDuration },
    #[error("quantile {} lies outside 0 to 1", .0.get::<ratio>())]
    Quantile(Ratio),
    #[error("running mean alpha {0} lies outside 0 to 1")]
    Alpha(f64),
    #[error("degree-day base {} K is not finite", .0.get::<kelvin>())]
    Base(ThermodynamicTemperature),
    #[error("wind rose speed edges are not positive and strictly rising")]
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
    #[error("field lies on no grid template this reader samples, or on fewer than two points along an axis")]
    Grid,
    #[error("site lies outside the lattice")]
    Outside,
    #[error("{}", .0.iter().format("; "))]
    Analysis(NEVec<AnalysisError>),
}

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
    Time(#[from] jiff::Error),
    #[error("{}", .0.iter().format("; "))]
    Analysis(NEVec<AnalysisError>),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RESAMPLING]
pub fn resampled(hours: &Hours, step: SignedDuration) -> Result<Hours, WeatherError> {
    fn received<D>(run: &[Hour], from: f64, to: f64, field: impl Fn(&Hour) -> Option<Quantity<D, SI<f64>, f64>>) -> Option<Quantity<D, SI<f64>, f64>>
    where D: si::Dimension + ?Sized {
        if run.get(run.partition_point(|hour| ending(hour) <= from)).is_some_and(|hour| to - from < ending(hour) - seconds(hour.interval.start)) {
            instant(run, |hour| seconds(hour.interval.start).midpoint(ending(hour)), from.midpoint(to), field)
        } else {
            mean(run, from, to, field)
        }
    }

    let resample = |run: &[Hour], interval: Range<Timestamp>| -> Hour {
        let (from, to) = (seconds(interval.start), seconds(interval.end));
        let span = Time::new::<second>(to - from);
        let holding = run.get(run.partition_point(|hour| ending(hour) < to));
        let toward = |component: fn(Angle) -> Ratio| instant(run, ending, to, |hour| hour.wind_direction.map(component));
        let precipitation = mean(run, from, to, |hour| hour.precipitation.map(|depth| depth / Time::new::<second>(ending(hour) - seconds(hour.interval.start)))).map(|rate| rate * span);
        Hour {
            interval,
            dry_bulb: instant(run, ending, to, |hour| hour.dry_bulb),
            dew_point: instant(run, ending, to, |hour| hour.dew_point),
            relative_humidity: instant(run, ending, to, |hour| hour.relative_humidity),
            pressure: instant(run, ending, to, |hour| hour.pressure),
            infrared: received(run, from, to, |hour| hour.infrared),
            global_irradiance: received(run, from, to, |hour| hour.global_irradiance),
            direct_irradiance: received(run, from, to, |hour| hour.direct_irradiance),
            diffuse_irradiance: received(run, from, to, |hour| hour.diffuse_irradiance),
            global_illuminance: received(run, from, to, |hour| hour.global_illuminance),
            direct_illuminance: received(run, from, to, |hour| hour.direct_illuminance),
            diffuse_illuminance: received(run, from, to, |hour| hour.diffuse_illuminance),
            zenith_luminance: received(run, from, to, |hour| hour.zenith_luminance),
            wind_direction: toward(Angle::sin)
                .zip(toward(Angle::cos))
                .map(|(eastward, northward)| Angle::new::<radian>(eastward.atan2(northward).get::<radian>().rem_euclid(Angle::FULL_TURN.get::<radian>()))),
            wind_speed: instant(run, ending, to, |hour| hour.wind_speed),
            total_sky_cover: instant(run, ending, to, |hour| hour.total_sky_cover),
            opaque_sky_cover: instant(run, ending, to, |hour| hour.opaque_sky_cover),
            visibility: instant(run, ending, to, |hour| hour.visibility),
            ceiling: holding.and_then(|hour| hour.ceiling),
            present_weather: holding.and_then(|hour| hour.present_weather),
            precipitable_water: instant(run, ending, to, |hour| hour.precipitable_water),
            aerosol_optical_depth: instant(run, ending, to, |hour| hour.aerosol_optical_depth),
            snow_depth: instant(run, ending, to, |hour| hour.snow_depth),
            since_snowfall: instant(run, ending, to, |hour| hour.since_snowfall),
            albedo: instant(run, ending, to, |hour| hour.albedo),
            precipitation,
            precipitation_period: precipitation.map(|_| span),
        }
    };
    let period = Span::try_from(step)?;
    let runs = hours
        .records
        .chunk_by(|earlier, later| earlier.interval.end == later.interval.start)
        .filter_map(|run| {
            run.first().zip(run.last()).map(|(first, last)| {
                let (start, end) = (first.interval.start, last.interval.end);
                (step.is_positive() && end.duration_since(start).as_nanos().checked_rem_euclid(step.as_nanos()) == Some(0))
                    .ok_or(Refusal::Resolution { start, step })
                    .map(|()| start.series(period).take_while(|at| *at <= end).tuple_windows().map(|(from, to)| resample(run, Range { start: from, end: to })).collect_vec())
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(|refusals| WeatherError::Refused { refusals })?;
    Ok(Hours { sun: hours.sun.clone(), records: runs.into_iter().flatten().collect() })
}

fn instant<D>(run: &[Hour], node: fn(&Hour) -> f64, at: f64, field: impl Fn(&Hour) -> Option<Quantity<D, SI<f64>, f64>>) -> Option<Quantity<D, SI<f64>, f64>>
where D: si::Dimension + ?Sized {
    let following = run.partition_point(|hour| node(hour) < at);
    match (following.checked_sub(1).and_then(|index| run.get(index)), run.get(following)) {
        (None, None) => None,
        (_, Some(later)) if node(later) <= at => field(later),
        (Some(hour), None) | (None, Some(hour)) => field(hour),
        (Some(earlier), Some(later)) => {
            let weight = (at - node(earlier)) / (node(later) - node(earlier));
            field(earlier).zip(field(later)).map(|(before, after)| Quantity { dimension: PhantomData, units: PhantomData, value: (after.value - before.value).mul_add(weight, before.value) })
        }
    }
}

fn mean<D>(run: &[Hour], from: f64, to: f64, field: impl Fn(&Hour) -> Option<Quantity<D, SI<f64>, f64>>) -> Option<Quantity<D, SI<f64>, f64>>
where D: si::Dimension + ?Sized {
    run.iter()
        .skip(run.partition_point(|hour| ending(hour) <= from))
        .take_while(|hour| seconds(hour.interval.start) < to)
        .map(|hour| field(hour).map(|quantity| quantity.value * (ending(hour).min(to) - seconds(hour.interval.start).max(from))))
        .sum::<Option<f64>>()
        .map(|total| Quantity { dimension: PhantomData, units: PhantomData, value: total / (to - from) })
}

fn ending(hour: &Hour) -> f64 { seconds(hour.interval.end) }

fn seconds(at: Timestamp) -> f64 { at.as_duration().as_secs_f64() }

// --- [SHARED]
fn position(hours: &Hours, hour: &Hour) -> Result<(PreparedPositions, SolarPosition), WeatherError> {
    let Range { start, end } = hour.interval;
    Ok(rasm_solar::position(&hours.sun, start.checked_add(end.duration_since(start).div_f64(2.0))?)?)
}

fn extraterrestrial(prepared: &PreparedPositions) -> HeatFluxDensity {
    const SOLAR_CONSTANT: f64 = 1367.0;
    HeatFluxDensity::new::<watt_per_square_meter>(prepared.earth_radius_vector().map_or(SOLAR_CONSTANT, |distance| SOLAR_CONSTANT / distance.powi(2)))
}

fn written(value: f64) -> f64 {
    const DECIMALS: f64 = 1.0e9;
    (value * DECIMALS).round() / DECIMALS
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
