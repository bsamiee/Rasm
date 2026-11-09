#![feature(array_try_map, default_field_values, float_conversions, once_cell_try, type_changing_struct_update)]

use core::{
    fmt, iter,
    marker::PhantomData,
    num::{NonZero, ParseIntError},
    range::Range,
};

use basin::BrentRootError;
use gauss_quad::GaussLegendre;
use grib::GribError;
use itertools::Itertools as _;
use jiff::{SignedDuration, Span, Timestamp, civil, tz::TimeZone};
use nalgebra::Vector2;
use ndarray_interp::BuilderError;
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use rasm_site::Place;
use rasm_solar_geometry::{Refraction, SolarError, Sun, lapsed, sampled};
use solar_positioning::{PreparedPositions, SolarPosition};
use thermalcomfort::{Humidity, Temperature, constants::STEFAN_BOLTZMANN, environment::ScaleWindSpeedLogError};
use uom::{
    ConstZero as _,
    si::{
        self, Quantity, SI,
        angle::radian,
        f64::{Angle, HeatFluxDensity, Illuminance, Length, Luminance, Pressure, Ratio, ThermodynamicTemperature, Time, Velocity},
        heat_flux_density::watt_per_square_meter,
        length::meter,
        pressure::pascal,
        ratio::ratio,
        thermodynamic_temperature::kelvin,
        time::second,
        velocity::meter_per_second,
    },
};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

type Measure<D> = Quantity<D, SI<f64>, f64>;

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
    pub flags: Option<String> = None,
    pub dry_bulb: Option<ThermodynamicTemperature> = None,
    pub dew_point: Option<ThermodynamicTemperature> = None,
    pub relative_humidity: Option<Ratio> = None,
    pub pressure: Option<Pressure> = None,
    pub extraterrestrial_horizontal: Option<HeatFluxDensity> = None,
    pub extraterrestrial_normal: Option<HeatFluxDensity> = None,
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
pub struct Hours<S = Place> {
    #[as_ref]
    site: S,
    #[as_ref([Hour])]
    records: Vec<Hour>,
}

impl<S> Hours<S> {
    pub fn try_new(site: S, hours: Vec<Hour>) -> Result<Self, WeatherError> {
        macro_rules! values {
            ($hour:ident: $($field:ident),*) => {
                [$($hour.$field.map(|quantity| quantity.value)),*]
            };
        }
        refused(
            hours
                .iter()
                .enumerate()
                .flat_map(|(index, hour)| {
                    let values = values!(
                        hour: dry_bulb, dew_point, relative_humidity, pressure, extraterrestrial_horizontal, extraterrestrial_normal, infrared, global_irradiance, direct_irradiance,
                        diffuse_irradiance, global_illuminance, direct_illuminance, diffuse_illuminance, zenith_luminance, wind_direction, wind_speed, total_sky_cover, opaque_sky_cover,
                        visibility, precipitable_water, aerosol_optical_depth, snow_depth, since_snowfall, albedo, precipitation, precipitation_period
                    );
                    let height = match hour.ceiling {
                        Some(Ceiling::Height(height)) => Some(height.value),
                        None | Some(Ceiling::Unlimited | Ceiling::Cirroform) => None,
                    };
                    [
                        (hour.interval.is_empty() || index.checked_sub(1).and_then(|before| hours.get(before)).is_some_and(|before| hour.interval.start < before.interval.end))
                            .then_some(Refusal::Hour { index, defect: Defect::Interval }),
                        iter::chain(values, [height]).flatten().any(|value| !value.is_finite()).then_some(Refusal::Hour { index, defect: Defect::NonFinite }),
                        hour.present_weather.is_some_and(|digits| digits.iter().any(|digit| *digit > 9)).then_some(Refusal::Hour { index, defect: Defect::Weather }),
                    ]
                })
                .flatten(),
        )
        .map(|()| Self { site, records: hours })
    }
}

impl Hours {
    pub fn lit(self, refraction: Option<Refraction>, delta_t: Option<SignedDuration>) -> Result<Hours<Sun>, SolarError> { Sun::new(self.site, refraction, delta_t).map(|site| Hours { site, ..self }) }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Defect {
    #[display("spans an empty or reversed interval or starts before the previous hour ends")]
    Interval,
    #[display("holds a non-finite quantity")]
    NonFinite,
    #[display("holds a present weather digit above 9")]
    Weather,
    #[display("lacks a value the operation reads")]
    Incomplete,
    #[display("spans an interval unequal to the first, dividing no hour or ending between steps")]
    Step,
}

#[derive(Debug, Clone, Copy, PartialEq, thiserror::Error)]
pub enum Refusal {
    #[error("hour {index} {defect}")]
    Hour { index: usize, defect: Defect },
    #[error("step {step} is not positive or leaves a remainder of the run from {start}")]
    Resolution { start: Timestamp, step: SignedDuration },
    #[error("series states the one valid time {0} and no duration")]
    Instant(Timestamp),
    #[error("quantile {} lies outside 0 to 1", .0.get::<ratio>())]
    Quantile(Ratio),
    #[error("running mean alpha {0} lies outside 0 to 1")]
    Alpha(f64),
    #[error("degree-day base {} K is not finite", .0.get::<kelvin>())]
    Base(ThermodynamicTemperature),
    #[error("wind rose speed edges are not positive and strictly rising")]
    Edges,
    #[error("wind speed {} m/s is negative or not finite", .0.get::<meter_per_second>())]
    Speed(Velocity),
    #[error("height {} m is not positive and finite", .0.get::<meter>())]
    Height(Length),
    #[error("power-law exponent {exponent} lies outside 0 to 1 or boundary layer {} m is not positive", .thickness.get::<meter>())]
    Terrain { exponent: f64, thickness: Length },
    #[error("roughness {} m or displacement {} m is not finite", .roughness.get::<meter>(), .displacement.get::<meter>())]
    Logarithmic { roughness: Length, displacement: Length },
    #[error("dry bulb {} K, relative humidity {} or pressure {} Pa lies outside the psychrometric range", .dry_bulb.get::<kelvin>(), .relative_humidity.get::<ratio>(), .pressure.get::<pascal>())]
    Psychrometric { dry_bulb: ThermodynamicTemperature, relative_humidity: Ratio, pressure: Pressure },
    #[error("typical year takes every month, month {0} holds no hours")]
    Typical(i8),
    #[error("{stated} stated years for {hours} hours")]
    Years { stated: usize, hours: usize },
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Record {
    #[display("header {_0}")]
    Header(epw::Keyword),
    #[display("row {_0}")]
    Row(u64),
    #[display("message {message} of file {file}")]
    Message { file: usize, message: usize },
}

#[derive(Debug, thiserror::Error)]
pub enum Malformed {
    #[error(transparent)]
    Csv(#[from] csv::Error),
    #[error(transparent)]
    Time(#[from] jiff::Error),
    #[error(transparent)]
    Grib(#[from] GribError),
    #[error(transparent)]
    Lattice(BuilderError),
    #[error("GRIB decode: {}", .0.iter().format("; "))]
    Decode(NEVec<GribError>),
    #[error("{text} names no {term}")]
    Day { text: String, term: epw::Term },
    #[error("{text} names no ordinal day")]
    Ordinal { text: String, source: ParseIntError },
    #[error("required record is absent")]
    Absent,
    #[error("message states no valid time")]
    Undated,
    #[error("field lies on no grid template this reader samples, or on fewer than two points along an axis")]
    Grid,
    #[error("cannot project the grid: {0}")]
    Projection(&'static str),
    #[error("site lies outside the lattice")]
    Outside,
}

#[derive(Debug, thiserror::Error)]
pub enum WeatherError {
    #[error("refused {}", .refusals.iter().format(", "))]
    Refused { refusals: NEVec<Refusal> },
    #[error("malformed records{}", fmt::from_fn(|formatter| .records.iter().try_for_each(|(record, malformed)| write!(formatter, "\n{record}: {malformed}"))))]
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
    #[error(transparent)]
    Root(#[from] BrentRootError<!>),
}

impl From<NEVec<Refusal>> for WeatherError {
    fn from(value: NEVec<Refusal>) -> Self { Self::Refused { refusals: value } }
}

impl From<NEVec<(Record, Malformed)>> for WeatherError {
    fn from(value: NEVec<(Record, Malformed)>) -> Self { Self::Records { records: value } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RESAMPLING]

pub fn resampled<S>(hours: &Hours<S>, step: SignedDuration, gap: SignedDuration) -> Result<Hours<S>, WeatherError>
where S: Clone {
    fn received<D>(run: &[Hour], gap: SignedDuration, field: impl Fn(&Hour) -> Option<Measure<D>> + Copy) -> impl FnMut(Range<Timestamp>) -> Option<Measure<D>>
    where D: si::Dimension + ?Sized {
        let mut midpoint = instant(run, |hour| hour.interval.start.as_nanosecond().strict_add(hour.interval.end.as_nanosecond()), gap, field);
        move |interval| {
            if length(interval) < length(run[run.partition_point(|hour| hour.interval.end <= interval.start)].interval) {
                midpoint(interval.start.as_nanosecond().strict_add(interval.end.as_nanosecond()))
            } else {
                averaged(run, interval, |hour| hour.interval, gap, |hour| field(hour).map(|quantity| (quantity.value, 1.0))).map(measure)
            }
        }
    }

    macro_rules! resample {
        ($run:ident, $intervals:expr; $($point:ident),*; $($flux:ident),*) => {{
            let ending = |hour: &Hour| hour.interval.end.as_nanosecond().strict_mul(2);
            $(let mut $point = instant($run, ending, gap, |hour| hour.$point);)*
            $(let mut $flux = received($run, gap, |hour| hour.$flux);)*
            let toward = |component: fn(Vector2<f64>) -> f64| instant($run, ending, gap, move |hour| hour.wind_direction.map(|direction| Ratio::new::<ratio>(component(heading(direction)))));
            let (mut east, mut north) = (toward(|vector| vector.x), toward(|vector| vector.y));
            $intervals.map(|(start, end)| {
                let interval = Range { start, end };
                let to = end.as_nanosecond().strict_mul(2);
                let span = Time::new::<second>(length(interval).as_secs_f64());
                let holding = &$run[$run.partition_point(|hour| hour.interval.end < end)];
                let precipitation = averaged($run, interval, |hour| hour.interval, gap, |hour| hour.precipitation.map(|depth| (depth.value, length(hour.interval).as_secs_f64())))
                    .map(|rate| Length::new::<meter>(rate * span.value));
                Hour {
                    interval,
                    flags: holding.flags.clone(),
                    $($point: $point(to),)*
                    $($flux: $flux(interval),)*
                    wind_direction: east(to).zip(north(to)).map(|(east, north)| bearing(Vector2::new(east.get::<ratio>(), north.get::<ratio>()))),
                    ceiling: holding.ceiling,
                    present_weather: holding.present_weather,
                    precipitation,
                    precipitation_period: precipitation.map(|_| span),
                }
            }).collect_vec()
        }};
    }
    let period = Span::try_from(step)?;
    let runs = runs(&hours.records)
        .map(|(run, interval)| {
            (step.is_positive() && aligned(length(interval), step)).ok_or(Refusal::Resolution { start: interval.start, step }).map(|()| {
                resample!(run, interval.start.series(period).take_while(|at| *at < interval.end).chain(iter::once(interval.end)).tuple_windows();
                    dry_bulb, dew_point, relative_humidity, pressure, wind_speed, total_sky_cover, opaque_sky_cover, visibility,
                    precipitable_water, aerosol_optical_depth, snow_depth, since_snowfall, albedo;
                    extraterrestrial_horizontal, extraterrestrial_normal, infrared, global_irradiance, direct_irradiance, diffuse_irradiance, global_illuminance, direct_illuminance,
                    diffuse_illuminance, zenith_luminance
                )
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    Ok(Hours { site: hours.site.clone(), records: runs.into_iter().flatten().collect() })
}

fn instant<D>(run: &[Hour], node: impl Fn(&Hour) -> i128, gap: SignedDuration, field: impl Fn(&Hour) -> Option<Measure<D>>) -> impl FnMut(i128) -> Option<Measure<D>>
where D: si::Dimension + ?Sized {
    let mut following = run.iter().enumerate().filter_map(move |(index, hour)| field(hour).map(|quantity| (index, node(hour), quantity))).peekable();
    let mut earlier = None;
    let gap = gap.as_nanos().strict_mul(2);
    move |at| {
        earlier = following.peeking_take_while(|&(_, time, _)| time < at).last().or(earlier);
        let near = |time: i128| time.strict_sub(at).strict_abs() <= gap;
        match (earlier, following.peek().copied()) {
            (None, None) => None,
            (_, Some((_, time, later))) if time <= at => Some(later),
            (Some((before, earlier_time, earlier)), Some((after, later_time, later))) => (after == before.strict_add(1) || later_time.strict_sub(earlier_time) <= gap).then(|| {
                let share = nalgebra::convert::<i128, f64>(at.strict_sub(earlier_time)) / nalgebra::convert::<i128, f64>(later_time.strict_sub(earlier_time));
                measure((later.value - earlier.value).mul_add(share, earlier.value))
            }),
            (Some((index, time, value)), None) => (index.strict_add(1) == run.len() || near(time)).then_some(value),
            (None, Some((index, time, value))) => (index == 0 || near(time)).then_some(value),
        }
    }
}

fn averaged<T>(items: &[T], interval: Range<Timestamp>, span: impl Fn(&T) -> Range<Timestamp>, gap: SignedDuration, value: impl Fn(&T) -> Option<(f64, f64)>) -> Option<f64> {
    let first = items.partition_point(|item| span(item).end <= interval.start);
    let (sum, covered) = items
        .iter()
        .skip(first)
        .map_while(|item| {
            let part = span(item);
            (part.start < interval.end).then(|| (item, Range { start: part.start.max(interval.start), end: part.end.min(interval.end) }))
        })
        .filter_map(|(item, part)| value(item).map(|(value, divisor)| (value / divisor, length(part).as_secs_f64())))
        .fold((0.0, 0.0), |(sum, covered), (value, weight)| (weight.mul_add(value, sum), covered + weight));
    (covered > 0.0 && length(interval).as_secs_f64() - covered <= gap.as_secs_f64()).then(|| sum / covered)
}

fn weighted(readings: impl IntoIterator<Item = (f64, f64)>) -> Option<f64> {
    let (sum, total) = readings.into_iter().fold((0.0, 0.0), |(sum, total), (value, weight)| (weight.mul_add(value, sum), total + weight));
    (total > 0.0).then(|| sum / total)
}

fn runs(records: &[Hour]) -> impl Iterator<Item = (&[Hour], Range<Timestamp>)> {
    records.chunk_by(|earlier, later| earlier.interval.end == later.interval.start).map(|run| (run, Range { start: run[0].interval.start, end: run[run.len().strict_sub(1)].interval.end }))
}

fn aligned(duration: SignedDuration, step: SignedDuration) -> bool { duration.as_nanos().checked_rem_euclid(step.as_nanos()) == Some(0) }

fn length(interval: Range<Timestamp>) -> SignedDuration { interval.end.duration_since(interval.start) }

// --- [CALENDAR]

fn zone<S>(hours: &Hours<S>) -> &TimeZone
where S: AsRef<Place> {
    &hours.site.as_ref().zone
}

fn local<S>(hours: &Hours<S>, hour: &Hour) -> civil::DateTime
where S: AsRef<Place> {
    zone(hours).to_datetime(hour.interval.start)
}

fn redated(hour: Hour, zone: &TimeZone, year: i16) -> Result<Hour, jiff::Error> {
    let start = zone.to_timestamp(zone.to_datetime(hour.interval.start).with().year(year).build()?)?;
    Ok(Hour { interval: Range { start, end: start.checked_add(length(hour.interval))? }, ..hour })
}

// --- [COMPLETION]

pub fn completed(hours: &Hours<Sun>) -> Result<Hours<Sun>, WeatherError> {
    const ERBS_LOW: f64 = 0.22;
    const ERBS_HIGH: f64 = 0.8;
    let normal = |horizontal: HeatFluxDensity, prepared: &PreparedPositions, solar: &SolarPosition| {
        let cosine = cos_zenith(solar);
        if cosine > 0.0 { (horizontal / cosine).min(extraterrestrial(prepared)) } else { HeatFluxDensity::ZERO }
    };
    let lacking = hours.records.iter().positions(|hour| hour.extraterrestrial_horizontal.is_none() || hour.extraterrestrial_normal.is_none()).collect_vec();
    let incoming = sampled(&hours.site, lacking.iter().map(|&index| hours.records[index].interval), &GaussLegendre::new(const { NonZero::new(3).unwrap() }))?
        .into_iter()
        .into_grouping_map_by(|&(member, _, _, _)| lacking[member])
        .fold((HeatFluxDensity::ZERO, HeatFluxDensity::ZERO), |(on_horizontal, on_normal), &index, (_, prepared, position, duration)| {
            let arriving = extraterrestrial(&prepared) * (duration.get::<second>() / length(hours.records[index].interval).as_secs_f64());
            (on_horizontal + arriving * cos_zenith(&position), on_normal + arriving)
        });
    let records = hours
        .records
        .iter()
        .enumerate()
        .map(|(index, hour)| {
            let incident = lacking.binary_search(&index).is_ok().then(|| incoming.get(&index).copied().unwrap_or_default());
            let relative_humidity = hour.relative_humidity.or_else(|| hour.dry_bulb.zip(hour.dew_point).map(|(dry_bulb, dew_point)| relative_humidity(dry_bulb, dew_point)));
            let dew_point = match (hour.dew_point, hour.dry_bulb, relative_humidity) {
                (None, Some(dry_bulb), Some(humidity)) => dew_point(dry_bulb, humidity)?,
                (stated, _, _) => stated,
            };
            let infrared = hour.infrared.or_else(|| {
                let tenths = hour.opaque_sky_cover.or(hour.total_sky_cover)?.get::<ratio>() * 10.0;
                let clear = 0.764_f64.mul_add((dew_point?.get::<kelvin>() / 273.0).ln(), 0.787);
                let emissivity = clear * 0.000_28_f64.mul_add(tenths, -0.0035).mul_add(tenths, 0.0224).mul_add(tenths, 1.0);
                Some(HeatFluxDensity::new::<watt_per_square_meter>(emissivity * STEFAN_BOLTZMANN * hour.dry_bulb?.get::<kelvin>().powi(4)))
            });
            let (direct_irradiance, diffuse_irradiance) = match (hour.global_irradiance, hour.direct_irradiance, hour.diffuse_irradiance) {
                (Some(global), Some(direct), None) => {
                    let (_, solar) = position(&hours.site, hour.interval)?;
                    (Some(direct), Some((global - direct * cos_zenith(&solar)).max(HeatFluxDensity::ZERO)))
                }
                (Some(global), None, Some(diffuse)) => {
                    let (prepared, solar) = position(&hours.site, hour.interval)?;
                    (Some(normal((global - diffuse).max(HeatFluxDensity::ZERO), &prepared, &solar)), Some(diffuse))
                }
                (Some(global), None, None) => {
                    let (prepared, solar) = position(&hours.site, hour.interval)?;
                    let arriving = extraterrestrial(&prepared) * cos_zenith(&solar);
                    let clearness = if arriving > HeatFluxDensity::ZERO { (global / arriving).get::<ratio>() } else { 0.0 };
                    let fraction = match clearness {
                        ..=ERBS_LOW => 0.09_f64.mul_add(-clearness, 1.0),
                        ..=ERBS_HIGH => 12.336_f64.mul_add(clearness, -16.638).mul_add(clearness, 4.388).mul_add(clearness, -0.1604).mul_add(clearness, 0.9511),
                        _ => 0.165,
                    };
                    let diffuse = global * fraction;
                    (Some(normal(global - diffuse, &prepared, &solar)), Some(diffuse))
                }
                (_, direct, diffuse) => (direct, diffuse),
            };
            Ok(Hour {
                dew_point,
                relative_humidity,
                extraterrestrial_horizontal: hour.extraterrestrial_horizontal.or_else(|| incident.map(|(on_horizontal, _)| on_horizontal)),
                extraterrestrial_normal: hour.extraterrestrial_normal.or_else(|| incident.map(|(_, on_normal)| on_normal)),
                infrared,
                direct_irradiance,
                diffuse_irradiance,
                ..hour.clone()
            })
        })
        .collect::<Result<Vec<_>, WeatherError>>()?;
    Ok(Hours { site: hours.site.clone(), records })
}

pub fn elevated<S>(hours: &Hours<S>, surface: Length) -> Result<Hours<S>, WeatherError>
where S: AsRef<Place> + Clone {
    let rise = *hours.site.as_ref().elevation - surface;
    let records = hours
        .records
        .iter()
        .map(|hour| {
            let lifted = hour.dry_bulb.map(|air| lapsed(air, rise));
            let dry_bulb = lifted.map(|(air, _)| air);
            let dew_point = match (dry_bulb, hour.relative_humidity) {
                (Some(air), Some(humidity)) => dew_point(air, humidity)?,
                (None | Some(_), _) => hour.dew_point,
            };
            Ok(Hour { dry_bulb, dew_point, pressure: hour.pressure.zip(lifted).map(|(stated, (_, scale))| stated * scale), ..hour.clone() })
        })
        .collect::<Result<Vec<_>, WeatherError>>()?;
    Ok(Hours { site: hours.site.clone(), records })
}

// --- [SOLAR]

fn position(sun: &Sun, interval: Range<Timestamp>) -> Result<(PreparedPositions, SolarPosition), WeatherError> {
    rasm_solar_geometry::at(interval, 0.5).and_then(|at| rasm_solar_geometry::position(sun, at)).map_err(WeatherError::Solar)
}

fn extraterrestrial(prepared: &PreparedPositions) -> HeatFluxDensity {
    const SOLAR_CONSTANT: f64 = 1367.0;
    HeatFluxDensity::new::<watt_per_square_meter>(prepared.earth_radius_vector().map_or(SOLAR_CONSTANT, |distance| SOLAR_CONSTANT / distance.powi(2)))
}

fn cos_zenith(solar: &SolarPosition) -> f64 { solar.elevation_angle().to_radians().sin().max(0.0) }

// --- [CODECS]

fn temperature(value: ThermodynamicTemperature) -> Temperature { Temperature::from_kelvin(value.get::<kelvin>()) }

fn quantity(value: Temperature) -> ThermodynamicTemperature { ThermodynamicTemperature::new::<kelvin>(value.as_kelvin()) }

fn humidity(value: Ratio) -> Humidity { Humidity::from_ratio(value.get::<ratio>()) }

fn atmospheric(value: Pressure) -> thermalcomfort::Pressure { thermalcomfort::Pressure::from_pascals(value.get::<pascal>()) }

fn heading(direction: Angle) -> Vector2<f64> {
    let (east, north) = direction.get::<radian>().sin_cos();
    Vector2::new(east, north)
}

fn bearing(toward: Vector2<f64>) -> Angle { Angle::new::<radian>(toward.x.atan2(toward.y).rem_euclid(Angle::FULL_TURN.get::<radian>())) }

const fn measure<D>(value: f64) -> Measure<D>
where D: si::Dimension + ?Sized {
    Quantity { dimension: PhantomData, units: PhantomData, value }
}

fn positive<D>(quantity: Measure<D>) -> bool
where D: si::Dimension + ?Sized {
    quantity.value.is_normal() && quantity.value.is_sign_positive()
}

// --- [REFUSALS]

fn refused<I>(refusals: I) -> Result<(), WeatherError>
where I: IntoIterator<Item = Refusal> {
    refusals.try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>).map_or(Ok(()), |refusals| Err(refusals.into()))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod climate;
mod comfort;
mod design;
pub mod epw;
pub mod gridded;
mod sky;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use climate::{DegreeDayMethod, DegreeDays, Profile, Rose, Statistic, Terrain, degree_days, integral, rose, running_mean, summary, typical, wind};
pub use comfort::{Adaptive, Conditions, Exposure, Index, Limit, Limits, Psychrometrics, Standard, adaptive, dew_point, equivalent, outdoor, pmv, psychrometrics, relative_humidity, utci};
pub use design::{Annual, Cooling, Design, Edition, Extremes, Heating, Humidification, computed, profile};
#[cfg(feature = "proximity")]
pub use sky::irradiance;
pub use sky::{Light, Matrix, Model, Sky, Spectrum, matrix};
