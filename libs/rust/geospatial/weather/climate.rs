use core::{hash::Hash, iter, marker::PhantomData, num::NonZero, ops::Mul};
use std::collections::{BTreeMap, VecDeque};

use itertools::Itertools as _;
use jiff::civil;
use ndarray::Array2;
use rasm_site::Place;
use statrs::statistics::{Data, OrderStatistics as _};
use thermalcomfort::{
    Speed, Temperature,
    environment::{RunningMeanOutdoorTemperatureOptions, ScaleWindSpeedLogError, ScaleWindSpeedLogOptions, running_mean_outdoor_temperature, scale_wind_speed_log},
};
use uom::si::{
    self, Quantity, SI,
    angle::revolution,
    f64::{Length, Ratio, TemperatureInterval, ThermodynamicTemperature, Time, Velocity},
    length::meter,
    ratio::ratio,
    temperature_interval,
    thermodynamic_temperature::kelvin,
    time::second,
    velocity::meter_per_second,
};
use validated::Validated;

use crate::{Hour, Hours, Refusal, WeatherError, refused};

// --- [TYPES] ---------------------------------------------------------------------------

type Measure<D> = Quantity<D, SI<f64>, f64>;
type Reading = (f64, f64);

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Degree {
    Heating,
    Cooling,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Method {
    MeanTemperature,
    Integration,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Statistic {
    Mean,
    Sum,
    Quantile(Ratio),
}

#[derive(Debug, Clone, PartialEq)]
pub struct Rose {
    pub calm: Time,
    pub frequency: Array2<Time>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Terrain {
    Country,
    Suburbs,
    City,
    Ocean,
    Stated { exponent: f64, thickness: Length },
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Profile {
    Power { site: Terrain, station: Terrain = Terrain::Country },
    Logarithmic { roughness: Length, displacement: Length },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SUMMARIES]
pub fn summary<K, D>(hours: &Hours, key: impl Fn(civil::DateTime) -> Option<K>, field: impl Fn(&Hour) -> Option<Measure<D>>, statistic: Statistic) -> Result<BTreeMap<K, Measure<D>>, WeatherError>
where
    K: Ord + Hash,
    D: si::Dimension + ?Sized,
{
    refused(match statistic {
        Statistic::Quantile(tau) => (!(0.0..=1.0).contains(&tau.get::<ratio>())).then_some(Refusal::Quantile(tau)),
        Statistic::Mean | Statistic::Sum => None,
    })?;
    Ok(hours
        .records
        .iter()
        .enumerate()
        .filter_map(|(index, hour)| {
            key(local(hours, hour))
                .zip(field(hour))
                .map(|(group, quantity)| if quantity.is_finite() { Validated::Good((group, (quantity.value, duration(hour).value))) } else { Validated::fail(Refusal::NonFinite(index)) })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(|refusals| WeatherError::Refused { refusals })?
        .into_iter()
        .into_group_map()
        .into_iter()
        .map(|(group, readings)| {
            let value = match statistic {
                Statistic::Mean => mean(&readings),
                Statistic::Sum => readings.iter().map(|&(value, _)| value).sum(),
                Statistic::Quantile(tau) => {
                    let step = readings.iter().map(|&(_, weight)| weight).reduce(f64::min).map_or(1.0, |shortest| shortest.max(1.0));
                    Data::new(readings.iter().flat_map(|&(value, weight)| iter::repeat_n(value, (weight / step).round().max(1.0).to_int_saturating::<usize>())).collect::<Vec<_>>())
                        .quantile(tau.get::<ratio>())
                }
            };
            (group, Quantity { dimension: PhantomData, units: PhantomData, value })
        })
        .collect())
}

pub fn integral<K, D, P>(hours: &Hours, key: impl Fn(civil::DateTime) -> Option<K>, field: impl Fn(&Hour) -> Option<Measure<D>>) -> Result<BTreeMap<K, Measure<P>>, WeatherError>
where
    K: Ord + Hash,
    D: si::Dimension + ?Sized,
    P: si::Dimension + ?Sized,
    Measure<D>: Mul<Time, Output = Measure<P>>,
{
    summary(hours, key, |hour| field(hour).map(|quantity| quantity * duration(hour)), Statistic::Sum)
}

pub fn degree_days<K>(
    hours: &Hours,
    key: impl Fn(civil::Date) -> Option<K>,
    base: ThermodynamicTemperature,
    degree: Degree,
    method: Method,
) -> Result<BTreeMap<K, <TemperatureInterval as Mul<Time>>::Output>, WeatherError>
where
    K: Ord + Hash,
{
    let excess = |temperature: f64, weight: f64| {
        let difference = match degree {
            Degree::Heating => base.value - temperature,
            Degree::Cooling => temperature - base.value,
        };
        TemperatureInterval::new::<temperature_interval::kelvin>(difference.max(0.0)) * Time::new::<second>(weight)
    };
    refused((!base.is_finite()).then_some(Refusal::Base(base)))?;
    Ok(daily(hours, key)?
        .into_iter()
        .map(|(group, day)| {
            let accumulated = match method {
                Method::MeanTemperature => excess(mean(&day), day.iter().map(|&(_, weight)| weight).sum()),
                Method::Integration => day.iter().map(|&(temperature, weight)| excess(temperature, weight)).sum(),
            };
            (group, accumulated)
        })
        .into_grouping_map()
        .sum()
        .into_iter()
        .collect())
}

pub fn running_mean(hours: &Hours, days: NonZero<usize>, options: RunningMeanOutdoorTemperatureOptions) -> Result<Vec<(civil::Date, ThermodynamicTemperature)>, WeatherError> {
    refused((!(0.0..=1.0).contains(&options.alpha)).then_some(Refusal::Alpha(options.alpha)))?;
    Ok(daily(hours, Some)?
        .into_iter()
        .scan((VecDeque::new(), None), |(recent, previous): &mut (VecDeque<Temperature>, Option<civil::Date>), (date, day)| {
            if !previous.replace(date).is_some_and(|last| last.tomorrow().is_ok_and(|next| next == date)) {
                recent.clear();
            }
            let running = (!recent.is_empty()).then(|| (date, ThermodynamicTemperature::new::<kelvin>(running_mean_outdoor_temperature(recent.make_contiguous(), options).as_kelvin())));
            recent.push_front(Temperature::from_kelvin(mean(&day)));
            recent.truncate(days.get());
            Some(running)
        })
        .flatten()
        .collect())
}

fn daily<K>(hours: &Hours, key: impl Fn(civil::Date) -> Option<K>) -> Result<Vec<(K, Vec<Reading>)>, WeatherError> {
    Ok(hours
        .records
        .iter()
        .enumerate()
        .filter_map(|(index, hour)| {
            let date = local(hours, hour).date();
            key(date).map(|_| hour.dry_bulb.map_or_else(|| Validated::fail(Refusal::Incomplete(index)), |dry_bulb| Validated::Good((date, (dry_bulb.value, duration(hour).value)))))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(|refusals| WeatherError::Refused { refusals })?
        .into_iter()
        .sorted_by_key(|&(date, _)| date)
        .chunk_by(|&(date, _)| date)
        .into_iter()
        .filter_map(|(date, day)| key(date).map(|group| (group, day.map(|(_, reading)| reading).collect())))
        .collect())
}

fn mean(readings: &[Reading]) -> f64 {
    let (weighted, total) = readings.iter().fold((0.0, 0.0), |(weighted, total), &(value, weight)| (value.mul_add(weight, weighted), total + weight));
    weighted / total
}

// --- [WIND]
pub fn rose<K>(hours: &Hours, key: impl Fn(civil::DateTime) -> Option<K>, sectors: NonZero<u16>, edges: &[Velocity]) -> Result<BTreeMap<K, Rose>, WeatherError>
where K: Ord + Hash {
    let rising = edges.first().is_some_and(|first| first.is_normal() && first.is_sign_positive()) && edges.is_sorted_by(|lower, upper| lower < upper);
    let count = f64::from(sectors.get());
    let shape = (NonZero::<usize>::from(sectors).get(), edges.len());
    refused((!rising).then_some(Refusal::Edges))?;
    Ok(hours
        .records
        .iter()
        .filter_map(|hour| {
            let speed = hour.wind_speed?;
            let cell = edges
                .partition_point(|edge| *edge <= speed)
                .checked_sub(1)
                .map_or(Some(None), |bin| hour.wind_direction.map(|direction| Some((direction.get::<revolution>().mul_add(count, 0.5).floor().rem_euclid(count).to_int_saturating::<usize>(), bin))))?;
            key(local(hours, hour)).map(|group| (group, (cell, duration(hour))))
        })
        .into_grouping_map()
        .fold_with(
            |_, _| Rose { calm: Time::default(), frequency: Array2::default(shape) },
            |mut rose, _, (cell, time)| {
                if let Some(slot) = cell.map_or(Some(&mut rose.calm), |cell| rose.frequency.get_mut(cell)) {
                    *slot += time;
                }
                rose
            },
        )
        .into_iter()
        .collect())
}

pub fn wind(speed: Velocity, anemometer: Length, height: Length, profile: Profile) -> Result<Velocity, WeatherError> {
    let positive = |length: Length| length.is_normal() && length.is_sign_positive();
    (speed.is_finite() && speed.value >= 0.0).ok_or(ScaleWindSpeedLogError::NegativeWindSpeed)?;
    positive(anemometer).ok_or(ScaleWindSpeedLogError::NonPositiveReferenceHeight)?;
    positive(height).ok_or(ScaleWindSpeedLogError::NonPositiveTargetHeight)?;
    match profile {
        Profile::Power { site, station } => {
            let coefficients = [site, station].map(|terrain| match terrain {
                Terrain::Country => (0.14, Length::new::<meter>(270.0)),
                Terrain::Suburbs => (0.22, Length::new::<meter>(370.0)),
                Terrain::City => (0.33, Length::new::<meter>(460.0)),
                Terrain::Ocean => (0.10, Length::new::<meter>(210.0)),
                Terrain::Stated { exponent, thickness } => (exponent, thickness),
            });
            coefficients.iter().all(|&(exponent, thickness): &(f64, Length)| (0.0..=1.0).contains(&exponent) && positive(thickness)).ok_or(ScaleWindSpeedLogError::NonPositiveRoughness)?;
            let [(site_exponent, site_thickness), (station_exponent, station_thickness)] = coefficients;
            Ok(speed * ((station_thickness / anemometer).get::<ratio>().max(1.0).powf(station_exponent) * (height / site_thickness).get::<ratio>().min(1.0).powf(site_exponent)))
        }
        Profile::Logarithmic { roughness, displacement } => {
            let converted = |length: Length| thermalcomfort::Length::from_meters(length.get::<meter>());
            positive(roughness).ok_or(ScaleWindSpeedLogError::NonPositiveRoughness)?;
            displacement.is_finite().ok_or(ScaleWindSpeedLogError::NegativeDisplacement)?;
            let options = ScaleWindSpeedLogOptions { z1: converted(anemometer), z0: converted(roughness), d: converted(displacement), round_output: false };
            Ok(Velocity::new::<meter_per_second>(scale_wind_speed_log(Speed::from_meters_per_second(speed.get::<meter_per_second>()), converted(height), options)?.as_meters_per_second()))
        }
    }
}

// --- [READINGS]
fn local(hours: &Hours, hour: &Hour) -> civil::DateTime { AsRef::<Place>::as_ref(&hours.sun).zone.to_datetime(hour.interval.start) }

fn duration(hour: &Hour) -> Time { Time::new::<second>(hour.interval.end.duration_since(hour.interval.start).as_secs_f64()) }
