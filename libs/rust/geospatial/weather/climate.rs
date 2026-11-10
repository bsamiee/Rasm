use core::hash::Hash;
use core::iter;
use core::num::NonZero;
use core::ops::Mul;
use core::range::RangeInclusive;
use std::collections::hash_map::Entry;
use std::collections::{BTreeMap, HashMap, VecDeque};

use itertools::Itertools as _;
use jiff::civil;
use ndarray::Array2;
use nonempty_collections::{NEVec, NonEmptyIterator as _};
use rasm_site::Place;
use rasm_solar_geometry::sector;
use statrs::statistics::{Data, OrderStatistics as _, Statistics as _};
use thermalcomfort::environment::{RunningMeanOutdoorTemperatureOptions, ScaleWindSpeedLogOptions, running_mean_outdoor_temperature, scale_wind_speed_log};
use thermalcomfort::{Speed, Temperature};
use uom::si::f64::{Length, Ratio, TemperatureInterval, ThermodynamicTemperature, Time, Velocity};
use uom::si::length::meter;
use uom::si::ratio::ratio;
use uom::si::time::second;
use uom::si::velocity::meter_per_second;
use uom::si::{self, temperature_interval};
use validated::Validated;

use crate::epw::{Day, Kind, Period};
use crate::{Defect, Hour, Hours, Measure, Refusal, WeatherError, length, local, measure, positive, quantity, redated, refused, weighted, zone};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum DegreeDays {
    Heating,
    Cooling,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum DegreeDayMethod {
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
    Power {
        site: Terrain,
        station: Terrain = Terrain::Country,
    },
    Logarithmic {
        roughness: Length,
        displacement: Length,
    },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SUMMARIES]

pub fn summary<S, K, D>(
    hours: &Hours<S>,
    key: impl Fn(civil::DateTime, &Hour) -> Option<K>,
    field: impl Fn(&Hour) -> Option<Measure<D>>,
    statistic: Statistic,
) -> Result<BTreeMap<K, Measure<D>>, WeatherError>
where
    S: AsRef<Place>,
    K: Ord + Hash,
    D: si::Dimension + ?Sized,
{
    refused(match statistic {
        Statistic::Quantile(tau) => (!(0.0..=1.0).contains(&tau.get::<ratio>())).then_some(Refusal::Quantile(tau)),
        Statistic::Mean | Statistic::Sum => None,
    })?;
    let readings = readings(hours, key, field, |_| None);
    match statistic {
        Statistic::Mean => grouped(
            readings,
            |(value, weight)| (weight * value, weight),
            |(sum, total), (value, weight)| {
                *sum = weight.mul_add(value, *sum);
                *total += weight;
            },
            |(sum, total)| Some(measure(sum / total)),
        ),
        Statistic::Sum => grouped(readings, |(value, _)| value, |sum, (value, _)| *sum += value, |sum| Some(measure(sum))),
        Statistic::Quantile(tau) => grouped(readings, NEVec::new, NEVec::push, |readings| {
            let step = readings.nonempty_iter().map(|&(_, weight)| weight).reduce(f64::min).max(1.0);
            let repeated = readings.iter().flat_map(|&(value, weight)| iter::repeat_n(value, (weight / step).round().max(1.0).to_int_saturating::<usize>()));
            Some(measure(Data::new(repeated.collect_vec()).quantile(tau.get::<ratio>())))
        }),
    }
}

pub fn integral<S, K, D, P>(hours: &Hours<S>, key: impl Fn(civil::DateTime, &Hour) -> Option<K>, field: impl Fn(&Hour) -> Option<Measure<D>>) -> Result<BTreeMap<K, Measure<P>>, WeatherError>
where
    S: AsRef<Place>,
    K: Ord + Hash,
    D: si::Dimension + ?Sized,
    P: si::Dimension + ?Sized,
    Measure<D>: Mul<Time, Output = Measure<P>>,
{
    summary(hours, key, |hour| field(hour).map(|quantity| quantity * Time::new::<second>(length(hour.interval).as_secs_f64())), Statistic::Sum)
}

pub fn degree_days<S, K>(
    hours: &Hours<S>,
    key: impl Fn(civil::Date) -> Option<K>,
    base: ThermodynamicTemperature,
    kind: DegreeDays,
    method: DegreeDayMethod,
) -> Result<BTreeMap<K, <TemperatureInterval as Mul<Time>>::Output>, WeatherError>
where
    S: AsRef<Place>,
    K: Ord + Hash,
{
    let excess = |temperature: f64, weight: f64| {
        let difference = match kind {
            DegreeDays::Heating => base.value - temperature,
            DegreeDays::Cooling => temperature - base.value,
        };
        TemperatureInterval::new::<temperature_interval::kelvin>(difference.max(0.0)) * Time::new::<second>(weight)
    };
    refused((!base.is_finite()).then_some(Refusal::Base(base)))?;
    Ok(daily(hours, key, |day| match method {
        DegreeDayMethod::MeanTemperature => weighted(day.iter().copied()).map(|mean| excess(mean, day.iter().map(|&(_, weight)| weight).sum())),
        DegreeDayMethod::Integration => Some(day.iter().map(|&(temperature, weight)| excess(temperature, weight)).sum()),
    })?
    .into_iter()
    .into_grouping_map()
    .sum()
    .into_iter()
    .collect())
}

pub fn running_mean<S>(hours: &Hours<S>, days: NonZero<usize>, options: RunningMeanOutdoorTemperatureOptions) -> Result<Vec<(civil::Date, ThermodynamicTemperature)>, WeatherError>
where S: AsRef<Place> {
    refused((!(0.0..=1.0).contains(&options.alpha)).then_some(Refusal::Alpha(options.alpha)))?;
    Ok(daily(hours, Some, weighted)?
        .into_iter()
        .scan((VecDeque::new(), None), |(recent, previous): &mut (VecDeque<Temperature>, Option<civil::Date>), (date, mean)| {
            if !previous.replace(date).is_some_and(|last| last.tomorrow().is_ok_and(|next| next == date)) {
                recent.clear();
            }
            let running = (!recent.is_empty()).then(|| (date, quantity(running_mean_outdoor_temperature(recent.make_contiguous(), options))));
            recent.push_front(Temperature::from_kelvin(mean));
            recent.truncate(days.get());
            Some(running)
        })
        .flatten()
        .collect())
}

// --- [TYPICAL]

pub fn typical<S>(hours: &Hours<S>, year: i16) -> Result<(Hours<S>, Vec<Period>), WeatherError>
where S: AsRef<Place> + Clone {
    const CANDIDATES: usize = 3;
    const WEEK: usize = 7;
    fn time_weighted<'hour, I>(hours: I, field: fn(&Hour) -> Option<f64>) -> Option<f64>
    where I: IntoIterator<Item = &'hour Hour> {
        weighted(hours.into_iter().filter_map(|hour| field(hour).map(|value| (value, length(hour.interval).as_secs_f64()))))
    }
    let parameters @ [dry_bulb, _, _]: [fn(&Hour) -> Option<f64>; 3] =
        [|hour| hour.dry_bulb.map(|value| value.value), |hour| hour.global_irradiance.map(|value| value.value), |hour| hour.relative_humidity.map(|value| value.value)];
    let records = {
        let months = hours
            .records
            .iter()
            .map(|hour| {
                let date = local(hours, hour).date();
                ((date.year(), date.month()), (date, hour))
            })
            .into_group_map();
        let wind = |hour: &Hour| hour.wind_speed.map(|value| value.value);
        let days = months
            .values()
            .flatten()
            .copied()
            .into_group_map()
            .into_iter()
            .map(|(date, day)| (date, parameters.map(|field| time_weighted(day.iter().copied(), field)), day))
            .sorted_by_key(|&(date, _, _)| date)
            .collect_vec();
        let chosen = (1..=12_i8)
            .map(|month| {
                let dates = days.iter().filter(|(date, _, _)| date.month() == month).collect_vec();
                let years = dates.iter().copied().into_group_map_by(|(date, _, _)| date.year()).into_iter().sorted_by_key(|candidate| candidate.0).collect_vec();
                let ranks = (0..parameters.len())
                    .map(|parameter| {
                        let all = dates.iter().filter_map(|(_, means, _)| means[parameter]).sorted_by(f64::total_cmp).collect_vec();
                        let cumulative = |value: f64| nalgebra::convert::<usize, f64>(all.partition_point(|&stated| stated <= value)) / nalgebra::convert::<usize, f64>(all.len().strict_add(1));
                        let statistics = years
                            .iter()
                            .map(|candidate| {
                                let own = candidate.1.iter().filter_map(|(_, means, _)| means[parameter]).sorted_by(f64::total_cmp).collect_vec();
                                let count = nalgebra::convert::<usize, f64>(own.len().strict_add(1));
                                own.iter().enumerate().map(|(index, &value)| (nalgebra::convert::<usize, f64>(index.strict_add(1)) / count - cumulative(value)).abs()).sum1()
                            })
                            .collect_vec();
                        let ordered = statistics.iter().flatten().copied().sorted_by(f64::total_cmp).collect_vec();
                        statistics.into_iter().map(|own| own.map_or(ordered.len(), |own| ordered.partition_point(|&other| other < own))).collect_vec()
                    })
                    .collect_vec();
                let long_term = time_weighted(dates.iter().flat_map(|(_, _, day)| day.iter().copied()), wind);
                years
                    .iter()
                    .enumerate()
                    .map(|(index, candidate)| {
                        let total = ranks.iter().map(|statistics| statistics[index]).sum::<usize>();
                        let mean = time_weighted(candidate.1.iter().flat_map(|(_, _, day)| day.iter().copied()), wind);
                        (candidate.0, total, mean.zip(long_term).map_or_default(|(own, mean)| (own - mean).abs()))
                    })
                    .sorted_by_key(|&(_, total, _)| total)
                    .take(CANDIDATES)
                    .min_by(|(left_year, left_rank, left), (right_year, right_rank, right)| left.total_cmp(right).then(left_rank.cmp(right_rank)).then(left_year.cmp(right_year)))
                    .map(|(candidate, _, _)| (month, candidate))
                    .ok_or(Refusal::Typical(month))
            })
            .collect::<Validated<Vec<_>, _>>()
            .ok()?;
        drop(days);
        let leap = civil::Date::new(year, 1, 1)?.in_leap_year();
        chosen
            .iter()
            .flat_map(|&(month, source)| months.get(&(source, month)).into_iter().flatten().filter(move |(date, _)| leap || (date.month(), date.day()) != (2, 29)).map(|&(_, hour)| hour))
            .map(|hour| redated(hour.clone(), zone(hours), year))
            .collect::<Result<Vec<_>, _>>()?
    };
    let typical = Hours::try_new(hours.site.clone(), records)?;
    let dated = typical.records.iter().map(|hour| (local(&typical, hour).date(), hour)).chunk_by(|&(date, _)| date);
    let means = (&dated).into_iter().filter_map(|(date, day)| time_weighted(day.map(|(_, hour)| hour), dry_bulb).map(|mean| (date, mean))).collect_vec();
    let southern = hours.site.as_ref().position.latitude().is_sign_negative();
    let seasons = [("Winter", [12, 1, 2], Some(("Min", -1.0))), ("Spring", [3, 4, 5], None), ("Summer", [6, 7, 8], Some(("Max", 1.0))), ("Autumn", [9, 10, 11], None)];
    let periods = seasons
        .into_iter()
        .flat_map(|(season, months, extreme): (&str, [i8; 3], Option<(&str, f64)>)| {
            let months = if southern { months.map(|month| month.strict_add(5).rem_euclid(12).strict_add(1)) } else { months };
            let means = means.iter().copied().filter(|(date, _)| months.contains(&date.month())).collect_vec();
            let average = weighted(means.iter().map(|&(_, mean)| (mean, 1.0)));
            let weeks = means
                .array_windows::<WEEK>()
                .filter(|week| week.array_windows().all(|[(earlier, _), (later, _)]| earlier.tomorrow().is_ok_and(|next| next == *later)))
                .map(|week| (week[0].0, week[WEEK - 1].0, week.iter().map(|&(_, mean)| mean).mean()))
                .collect_vec();
            let period = |name: String, kind: Kind, (first, last, _): (civil::Date, civil::Date, f64)| Period {
                name,
                kind,
                days: RangeInclusive { start: Day::Date { month: first.month(), day: first.day() }, last: Day::Date { month: last.month(), day: last.day() } },
            };
            let nearest = |target: f64| weeks.iter().copied().min_by(|(_, _, left), (_, _, right)| (left - target).abs().total_cmp(&(right - target).abs()));
            let typical_week = average.and_then(nearest).map(|week| period(format!("{season} - Week Nearest Average Temperature For Period"), Kind::Typical, week));
            let extreme_week = extreme.and_then(|(bound, sign)| {
                weeks
                    .iter()
                    .map(|&(_, _, mean)| mean)
                    .max_by(|left, right| (sign * left).total_cmp(&(sign * right)))
                    .and_then(nearest)
                    .map(|week| period(format!("{season} - Week Nearest {bound} Temperature For Period"), Kind::Extreme, week))
            });
            iter::chain(extreme_week, typical_week)
        })
        .collect();
    Ok((typical, periods))
}

// --- [WIND]

pub fn rose<S, K>(hours: &Hours<S>, key: impl Fn(civil::DateTime, &Hour) -> Option<K>, sectors: NonZero<u16>, edges: &[Velocity]) -> Result<BTreeMap<K, Rose>, WeatherError>
where
    S: AsRef<Place>,
    K: Ord + Hash,
{
    let rising = edges.first().is_some_and(|first| positive(*first)) && edges.is_sorted_by(|lower, upper| lower < upper);
    let count = NonZero::<usize>::from(sectors);
    refused((!rising).then_some(Refusal::Edges))?;
    Ok(hours
        .records
        .iter()
        .filter_map(|hour| {
            let speed = hour.wind_speed?;
            let cell = match (edges.partition_point(|edge| *edge <= speed).checked_sub(1), hour.wind_direction) {
                (None, _) => Some(None),
                (Some(bin), Some(direction)) => Some(Some((sector(direction, count), bin))),
                (Some(_), None) => None,
            }?;
            key(local(hours, hour), hour).map(|group| (group, (cell, Time::new::<second>(length(hour.interval).as_secs_f64()))))
        })
        .into_grouping_map()
        .fold_with(
            |_, _| Rose { calm: Time::default(), frequency: Array2::default((count.get(), edges.len())) },
            |mut rose, _, (cell, time)| {
                *match cell {
                    None => &mut rose.calm,
                    Some(cell) => &mut rose.frequency[cell],
                } += time;
                rose
            },
        )
        .into_iter()
        .collect())
}

pub fn wind(speed: Velocity, anemometer: Length, height: Length, profile: Profile) -> Result<Velocity, WeatherError> {
    match profile {
        Profile::Power { site, station } => {
            let coefficients = [site, station].map(|terrain| match terrain {
                Terrain::Country => (0.14, Length::new::<meter>(270.0)),
                Terrain::Suburbs => (0.22, Length::new::<meter>(370.0)),
                Terrain::City => (0.33, Length::new::<meter>(460.0)),
                Terrain::Ocean => (0.10, Length::new::<meter>(210.0)),
                Terrain::Stated { exponent, thickness } => (exponent, thickness),
            });
            refused(
                iter::chain((!(speed.is_finite() && speed.value >= 0.0)).then_some(Refusal::Speed(speed)), [anemometer, height].into_iter().filter(|length| !positive(*length)).map(Refusal::Height))
                    .chain(
                        coefficients
                            .into_iter()
                            .filter(|&(exponent, thickness)| !((0.0..=1.0).contains(&exponent) && positive(thickness)))
                            .map(|(exponent, thickness)| Refusal::Terrain { exponent, thickness }),
                    ),
            )?;
            let [(site_exponent, site_thickness), (station_exponent, station_thickness)] = coefficients;
            Ok(speed * ((station_thickness / anemometer).get::<ratio>().max(1.0).powf(station_exponent) * (height / site_thickness).get::<ratio>().min(1.0).powf(site_exponent)))
        }
        Profile::Logarithmic { roughness, displacement } => {
            refused(
                iter::chain((!speed.is_finite()).then_some(Refusal::Speed(speed)), [anemometer, height].into_iter().filter(|length| !length.is_finite()).map(Refusal::Height))
                    .chain((!(roughness.is_finite() && displacement.is_finite())).then_some(Refusal::Logarithmic { roughness, displacement })),
            )?;
            let converted = |length: Length| thermalcomfort::Length::from_meters(length.get::<meter>());
            let options = ScaleWindSpeedLogOptions { z1: converted(anemometer), z0: converted(roughness), d: converted(displacement), round_output: false };
            Ok(Velocity::new::<meter_per_second>(scale_wind_speed_log(Speed::from_meters_per_second(speed.get::<meter_per_second>()), converted(height), options)?.as_meters_per_second()))
        }
    }
}

// --- [READINGS]

fn readings<S, K, D>(
    hours: &Hours<S>,
    key: impl Fn(civil::DateTime, &Hour) -> Option<K>,
    field: impl Fn(&Hour) -> Option<Measure<D>>,
    absent: impl Fn(usize) -> Option<Refusal>,
) -> impl Iterator<Item = Validated<(K, (f64, f64)), Refusal>>
where
    S: AsRef<Place>,
    D: si::Dimension + ?Sized,
{
    hours.records.iter().enumerate().filter_map(move |(index, hour)| {
        let group = key(local(hours, hour), hour)?;
        match field(hour) {
            Some(quantity) if quantity.is_finite() => Some(Validated::Good((group, (quantity.value, length(hour.interval).as_secs_f64())))),
            Some(_) => Some(Validated::fail(Refusal::Hour { index, defect: Defect::NonFinite })),
            None => absent(index).map(Validated::fail),
        }
    })
}

fn grouped<K, A, T>(
    readings: impl IntoIterator<Item = Validated<(K, (f64, f64)), Refusal>>,
    begin: impl Fn((f64, f64)) -> A,
    append: impl Fn(&mut A, (f64, f64)),
    finish: impl Fn(A) -> Option<T>,
) -> Result<BTreeMap<K, T>, WeatherError>
where
    K: Ord + Hash,
{
    Ok(readings
        .into_iter()
        .fold(Validated::Good(HashMap::<K, A>::new()), |groups, reading| {
            groups.map2(reading, |mut groups, (key, reading)| {
                match groups.entry(key) {
                    Entry::Occupied(mut entry) => append(entry.get_mut(), reading),
                    Entry::Vacant(entry) => {
                        entry.insert(begin(reading));
                    }
                }
                groups
            })
        })
        .ok()?
        .into_iter()
        .filter_map(|(key, readings)| finish(readings).map(|value| (key, value)))
        .collect())
}

fn daily<S, K, T>(hours: &Hours<S>, key: impl Fn(civil::Date) -> Option<K>, reduce: impl Fn(Vec<(f64, f64)>) -> Option<T>) -> Result<Vec<(K, T)>, WeatherError>
where S: AsRef<Place> {
    Ok(grouped(
        readings(hours, |at, _| key(at.date()).map(|_| at.date()), |hour| hour.dry_bulb, |index| Some(Refusal::Hour { index, defect: Defect::Incomplete })),
        |reading| vec![reading],
        Vec::push,
        Some,
    )?
    .into_iter()
    .filter_map(|(date, day)| key(date).zip(reduce(day)))
    .collect())
}
