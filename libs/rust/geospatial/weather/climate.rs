use core::{convert, hash::Hash, marker::PhantomData, num::NonZero};
use std::collections::{BTreeMap, VecDeque};

use itertools::{Either, Itertools as _};
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

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Degree {
    Heating,
    Cooling,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Statistic {
    Mean,
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
    Urban,
    Stated { exponent: f64, thickness: Length },
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Profile {
    Power { site: Terrain, station: Terrain },
    Logarithmic { roughness: Length, displacement: Length },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SUMMARIES]
pub fn summary<K, D>(
    hours: &Hours,
    key: impl Fn(civil::DateTime) -> Option<K>,
    field: impl Fn(&Hour) -> Option<Quantity<D, SI<f64>, f64>>,
    statistic: Statistic,
) -> Result<BTreeMap<K, Quantity<D, SI<f64>, f64>>, WeatherError>
where
    K: Ord + Hash,
    D: si::Dimension + ?Sized,
{
    refused(match statistic {
        Statistic::Quantile(tau) => (!(0.0..=1.0).contains(&tau.get::<ratio>())).then_some(Refusal::Quantile(tau)),
        Statistic::Mean => None,
    })
    .map(|()| {
        hours
            .hours
            .iter()
            .filter_map(|hour| key(local(hours, hour)).zip(field(hour)).map(|(group, quantity)| (group, (quantity.value, duration(hour).value))))
            .into_group_map()
            .into_iter()
            .map(|(group, readings)| {
                let value = match statistic {
                    Statistic::Mean => mean(readings),
                    Statistic::Quantile(tau) => Data::new(readings.into_iter().map(|(value, _)| value).collect::<Vec<_>>()).quantile(tau.get::<ratio>()),
                };
                (group, Quantity { dimension: PhantomData, units: PhantomData, value })
            })
            .collect()
    })
}

pub fn degree_days<K>(hours: &Hours, key: impl Fn(civil::Date) -> Option<K>, base: ThermodynamicTemperature, degree: Degree) -> Result<BTreeMap<K, TemperatureInterval>, WeatherError>
where K: Ord + Hash {
    daily(hours).map(|days| {
        days.into_iter()
            .filter_map(|(date, mean)| {
                key(date).map(|group| {
                    let excess = match degree {
                        Degree::Heating => base.get::<kelvin>() - mean.get::<kelvin>(),
                        Degree::Cooling => mean.get::<kelvin>() - base.get::<kelvin>(),
                    };
                    (group, TemperatureInterval::new::<temperature_interval::kelvin>(excess.max(0.0)))
                })
            })
            .into_grouping_map()
            .sum()
            .into_iter()
            .collect()
    })
}

pub fn running_mean(hours: &Hours, days: NonZero<usize>, options: RunningMeanOutdoorTemperatureOptions) -> Result<Vec<(civil::Date, ThermodynamicTemperature)>, WeatherError> {
    daily(hours).map(|means| {
        means
            .into_iter()
            .scan(VecDeque::with_capacity(days.get()), |recent, (date, mean)| {
                let running = (!recent.is_empty()).then(|| (date, ThermodynamicTemperature::new::<kelvin>(running_mean_outdoor_temperature(recent.make_contiguous(), options).as_kelvin())));
                recent.push_front(Temperature::from_kelvin(mean.get::<kelvin>()));
                recent.truncate(days.get());
                Some(running)
            })
            .flatten()
            .collect()
    })
}

fn daily(hours: &Hours) -> Result<Vec<(civil::Date, ThermodynamicTemperature)>, WeatherError> {
    hours
        .hours
        .iter()
        .enumerate()
        .map(|(index, hour)| hour.dry_bulb.map(|dry_bulb| (local(hours, hour).date(), (dry_bulb.value, duration(hour).value))).ok_or(Refusal::Incomplete(index)))
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map(|readings| {
            readings.into_iter().chunk_by(|&(date, _)| date).into_iter().map(|(date, day)| (date, ThermodynamicTemperature::new::<kelvin>(mean(day.map(|(_, reading)| reading))))).collect()
        })
        .map_err(|refusals| WeatherError::Refused { refusals })
}

fn mean<I>(readings: I) -> f64
where I: IntoIterator<Item = (f64, f64)> {
    let (weighted, total) = readings.into_iter().fold((0.0, 0.0), |(weighted, total), (value, weight)| (value.mul_add(weight, weighted), total + weight));
    weighted / total
}

// --- [WIND]
pub fn rose<K>(hours: &Hours, key: impl Fn(civil::DateTime) -> Option<K>, sectors: NonZero<u16>, edges: &[Velocity]) -> Result<BTreeMap<K, Rose>, WeatherError>
where K: Ord + Hash {
    let rising = edges.first().is_some_and(|first| first.is_normal() && first.is_sign_positive()) && edges.array_windows().all(|[lower, upper]| lower < upper);
    let count = f64::from(sectors.get());
    refused((!rising).then_some(Refusal::Edges)).map(|()| {
        hours
            .hours
            .iter()
            .filter_map(|hour| {
                key(local(hours, hour)).zip(hour.wind_direction.zip(hour.wind_speed)).map(|(group, (direction, speed))| {
                    let time = duration(hour);
                    let sector = direction.get::<revolution>().mul_add(count, 0.5).floor().rem_euclid(count).to_int_saturating::<usize>();
                    (group, edges.partition_point(|edge| *edge <= speed).checked_sub(1).map_or(Either::Left(time), |bin| Either::Right(((sector, bin), time))))
                })
            })
            .into_group_map()
            .into_iter()
            .map(|(group, readings)| {
                let (calm, binned): (Vec<Time>, Vec<((usize, usize), Time)>) = readings.into_iter().partition_map(convert::identity);
                let cells = binned.into_iter().into_grouping_map().sum();
                (group, Rose { calm: calm.into_iter().sum(), frequency: Array2::from_shape_fn((usize::from(sectors.get()), edges.len()), |cell| cells.get(&cell).copied().unwrap_or_default()) })
            })
            .collect()
    })
}

pub fn wind(speed: Velocity, anemometer: Length, height: Length, profile: Profile) -> Result<Velocity, WeatherError> {
    match profile {
        Profile::Power { site, station } => {
            let coefficients = |terrain| match terrain {
                Terrain::Country => (0.14, Length::new::<meter>(270.0)),
                Terrain::Suburbs | Terrain::Urban => (0.22, Length::new::<meter>(370.0)),
                Terrain::City => (0.33, Length::new::<meter>(460.0)),
                Terrain::Ocean => (0.10, Length::new::<meter>(210.0)),
                Terrain::Stated { exponent, thickness } => (exponent, thickness),
            };
            let positive = |lengths: [Length; 2]| lengths.iter().all(|length| length.is_normal() && length.is_sign_positive());
            let ((site_exponent, site_thickness), (station_exponent, station_thickness)) = (coefficients(site), coefficients(station));
            (speed.is_finite() && speed.value >= 0.0)
                .ok_or(ScaleWindSpeedLogError::NegativeWindSpeed)
                .and(positive([anemometer, station_thickness]).ok_or(ScaleWindSpeedLogError::NonPositiveReferenceHeight))
                .and(positive([height, site_thickness]).ok_or(ScaleWindSpeedLogError::NonPositiveTargetHeight))
                .map(|()| speed * ((station_thickness / anemometer).get::<ratio>().powf(station_exponent) * (height / site_thickness).get::<ratio>().powf(site_exponent)))
                .map_err(WeatherError::Wind)
        }
        Profile::Logarithmic { roughness, displacement } => {
            let converted = |length: Length| thermalcomfort::Length::from_meters(length.get::<meter>());
            scale_wind_speed_log(
                Speed::from_meters_per_second(speed.get::<meter_per_second>()),
                converted(height),
                ScaleWindSpeedLogOptions { z1: converted(anemometer), z0: converted(roughness), d: converted(displacement), round_output: false },
            )
            .map(|scaled| Velocity::new::<meter_per_second>(scaled.as_meters_per_second()))
            .map_err(WeatherError::Wind)
        }
    }
}

// --- [READINGS]
fn local(hours: &Hours, hour: &Hour) -> civil::DateTime { AsRef::<Place>::as_ref(&hours.sun).zone.to_datetime(hour.interval.start) }

fn duration(hour: &Hour) -> Time { Time::new::<second>(hour.interval.end.duration_since(hour.interval.start).as_secs_f64()) }
