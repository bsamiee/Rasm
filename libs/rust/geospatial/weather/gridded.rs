use core::{
    cell::OnceCell,
    convert, iter,
    num::NonZero,
    range::{Range, RangeInclusive},
};
use std::{
    collections::{BTreeMap, HashMap, HashSet},
    io,
};

use dashu::base::ConversionError;
use grib::{
    Grib2Read, Grib2SubmessageDecoder, LatLons as _, SubMessage,
    codetables::grib2::Table4_4,
    def::grib2::{
        GridDefinitionTemplate, ProductDefinitionTemplate,
        template::{
            Template3_0, Template3_1, Template3_10, Template3_20, Template3_30, Template3_40, Template3_41, Template4_0, Template4_1, Template4_8, Template4_11,
            param_set::{DateTime, EnsembleForecast, GaussianGrid, LatLonGrid, Statistical, StatisticalProcess, TimeRange},
        },
    },
};
use indexmap::IndexMap;
use itertools::{Either, Itertools as _};
use jiff::{SignedDuration, Span, Timestamp, civil, tz::Offset};
use nalgebra::{Point3, Rotation2, Vector2, Vector3};
use ndarray::{aview1, aview2};
use ndarray_interp::{InterpolateError, interp2d::Interp2D};
use nonempty_collections::{NESlice, nev};
use rasm_numerical_analysis::roots;
use rasm_proximity::{Cloud, Neighbor, nearest};
use rasm_site::Place;
use rasm_solar_geometry::{Sun, lapsed};
use solar_positioning::Location;
use uom::si::{
    acceleration::{meter_per_second_squared, standard_gravity},
    angle::degree,
    f64::{Acceleration, Angle, HeatFluxDensity, Length, Pressure, Ratio, ThermodynamicTemperature, Time, Velocity},
    heat_flux_density::watt_per_square_meter,
    length::{meter, millimeter},
    pressure::pascal,
    ratio::percent,
    thermodynamic_temperature::kelvin,
    time::second,
    velocity::meter_per_second,
};
use validated::Validated;

use crate::{Hour, Hours, Malformed, Record, Refusal, WeatherError, averaged, bearing, comfort, completed, heading, length, normal, position, reduced};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
enum Field {
    DryBulb,
    DewPoint,
    RelativeHumidity,
    Pressure,
    Infrared,
    GlobalIrradiance,
    DirectHorizontal,
    WindDirection,
    WindSpeed,
    Eastward,
    Northward,
    TotalSkyCover,
    Visibility,
    PrecipitableWater,
    SnowDepth,
    Albedo,
    Precipitation,
    Orography,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const RESOLUTION: f64 = 1.0e-6;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy)]
struct Product {
    field: Field,
    member: Option<u8>,
    interval: Range<Timestamp>,
    weight: f64,
}

#[derive(Clone, Copy)]
struct Cell {
    corners: [(usize, usize); 4],
    axes: [[f64; 2]; 2],
    at: (f64, f64),
    rotation: Option<Rotation2<f64>>,
}

struct Grid {
    sites: Vec<Option<Cell>>,
    indices: Vec<(usize, (usize, usize))>,
}

#[derive(Clone, Copy)]
struct Reading {
    site: usize,
    member: Option<u8>,
    field: Field,
    interval: Range<Timestamp>,
    value: Option<f64>,
    rotation: Option<Rotation2<f64>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn read<R, I>(suns: &[Sun], files: I) -> Result<Vec<BTreeMap<Option<u8>, Hours>>, WeatherError>
where
    R: io::Read + io::Seek,
    I: IntoIterator<Item = R>,
{
    fn sampled<R>(grid: &OnceCell<Grid>, locations: &[Location], message: SubMessage<'_, R>) -> Result<Vec<Reading>, Malformed>
    where R: Grib2Read {
        let product = || -> Result<Option<Product>, Malformed> {
            const METEOROLOGY: u8 = 0;
            const AVERAGE: u8 = 0;
            const ACCUMULATION: u8 = 1;
            const GEOPOTENTIAL: (u8, Option<u8>, Option<u8>) = (METEOROLOGY, Some(3), Some(4));
            const PARAMETERS: [(u8, u8, u8, Option<f64>, Field); 24] = {
                const GROUND: u8 = 1;
                const ATMOSPHERE: u8 = 10;
                const HEIGHT: u8 = 103;
                [
                    (0, 0, HEIGHT, Some(2.0), Field::DryBulb),
                    (0, 6, HEIGHT, Some(2.0), Field::DewPoint),
                    (1, 1, HEIGHT, Some(2.0), Field::RelativeHumidity),
                    (3, 0, GROUND, None, Field::Pressure),
                    (3, 4, GROUND, None, Field::Orography),
                    (3, 5, GROUND, None, Field::Orography),
                    (5, 3, GROUND, None, Field::Infrared),
                    (4, 7, GROUND, None, Field::GlobalIrradiance),
                    (4, 13, GROUND, None, Field::DirectHorizontal),
                    (2, 0, HEIGHT, Some(10.0), Field::WindDirection),
                    (2, 1, HEIGHT, Some(10.0), Field::WindSpeed),
                    (2, 2, HEIGHT, Some(10.0), Field::Eastward),
                    (2, 3, HEIGHT, Some(10.0), Field::Northward),
                    (6, 1, GROUND, None, Field::TotalSkyCover),
                    (6, 1, ATMOSPHERE, None, Field::TotalSkyCover),
                    (19, 0, GROUND, None, Field::Visibility),
                    (1, 3, ATMOSPHERE, None, Field::PrecipitableWater),
                    (1, 64, GROUND, None, Field::PrecipitableWater),
                    (1, 64, ATMOSPHERE, None, Field::PrecipitableWater),
                    (1, 11, GROUND, None, Field::SnowDepth),
                    (19, 1, GROUND, None, Field::Albedo),
                    (1, 7, GROUND, None, Field::Precipitation),
                    (1, 8, GROUND, None, Field::Precipitation),
                    (1, 52, GROUND, None, Field::Precipitation),
                ]
            };
            fn stamped(time: &DateTime, after: Span) -> Result<Timestamp, Malformed> {
                let (Ok(year), Ok(month), Ok(day), Ok(hour), Ok(minute), Ok(seconds)) =
                    (i16::try_from(time.year), i8::try_from(time.month), i8::try_from(time.day), i8::try_from(time.hour), i8::try_from(time.minute), i8::try_from(time.second))
                else {
                    return Err(Malformed::Undated);
                };
                Ok(Offset::UTC.to_timestamp(civil::DateTime::new(year, month, day, hour, minute, seconds, 0)?.checked_add(after)?)?)
            }
            let span = |time: &TimeRange<i32>| -> Result<Span, Malformed> {
                let length = i64::from(time.len);
                Ok(match Table4_4::try_from(time.unit).ok() {
                    Some(Table4_4::Second) => Span::new().try_seconds(length),
                    Some(Table4_4::Minute) => Span::new().try_minutes(length),
                    Some(Table4_4::Hour) => Span::new().try_hours(length),
                    Some(Table4_4::ThreeHours) => Span::new().try_hours(length.strict_mul(3)),
                    Some(Table4_4::SixHours) => Span::new().try_hours(length.strict_mul(6)),
                    Some(Table4_4::TwelveHours) => Span::new().try_hours(length.strict_mul(12)),
                    Some(Table4_4::Day) => Span::new().try_days(length),
                    Some(Table4_4::Month) => Span::new().try_months(length),
                    Some(Table4_4::Year) => Span::new().try_years(length),
                    Some(Table4_4::Decade) => Span::new().try_years(length.strict_mul(10)),
                    Some(Table4_4::Normal) => Span::new().try_years(length.strict_mul(30)),
                    Some(Table4_4::Century) => Span::new().try_years(length.strict_mul(100)),
                    Some(_) | None => return Err(Malformed::Undated),
                }?)
            };
            let definition = message.prod_def();
            let parameter = (message.indicator().discipline, definition.parameter_category(), definition.parameter_number());
            let Some(field) = definition.fixed_surfaces().filter(|_| matches!(definition.prod_tmpl_num(), 0 | 1 | 8 | 11)).and_then(|(surface, _)| {
                PARAMETERS.into_iter().find_map(|(category, number, surface_type, height, field)| {
                    (parameter == (METEOROLOGY, Some(category), Some(number)) && surface.surface_type == surface_type && height.is_none_or(|metres| (surface.value() - metres).abs() < RESOLUTION))
                        .then_some(field)
                })
            }) else {
                return Ok(None);
            };
            let member = |EnsembleForecast { perturbation_num, .. }: EnsembleForecast| Some(perturbation_num);
            let (forecast_time, statistical, member) = match message.section4()?.payload.template {
                ProductDefinitionTemplate::_4_0(Template4_0 { forecast_time, .. }) => (forecast_time, None, None),
                ProductDefinitionTemplate::_4_1(Template4_1 { forecast_time, emsemble_forecast, .. }) => (forecast_time, None, member(emsemble_forecast)),
                ProductDefinitionTemplate::_4_8(Template4_8 { forecast_time, time_interval, .. }) => (forecast_time, Some(time_interval), None),
                ProductDefinitionTemplate::_4_11(Template4_11 { forecast_time, emsemble_forecast, time_interval, .. }) => (forecast_time, Some(time_interval), member(emsemble_forecast)),
                _ => return Ok(None),
            };
            let valid = span(&forecast_time.time).and_then(|offset| stamped(&message.section1()?.payload.ref_time, offset))?;
            let statistic = statistical
                .map(|Statistical { end_of_overall_time_interval, time_ranges, .. }| {
                    stamped(&end_of_overall_time_interval, Span::new()).map(|end| (end, <[StatisticalProcess; 1]>::try_from(time_ranges).ok().map(|[process]| process.statistical_process_type)))
                })
                .transpose()?;
            Ok(match statistic {
                None => Some(Product {
                    field,
                    member,
                    interval: Range { start: valid, end: valid },
                    weight: if parameter == GEOPOTENTIAL { Acceleration::new::<standard_gravity>(1.0).get::<meter_per_second_squared>().recip() } else { 1.0 },
                }),
                Some((end, Some(AVERAGE))) if end > valid => Some(Product { field, member, interval: Range { start: valid, end }, weight: end.duration_since(valid).as_secs_f64() }),
                Some((end, Some(ACCUMULATION))) if end > valid => Some(Product { field, member, interval: Range { start: valid, end }, weight: 1.0 }),
                Some((_, Some(_) | None)) => None,
            })
        };

        let Some(product) = product()? else {
            return Ok(Vec::new());
        };
        let grid = grid.get_or_try_init(|| -> Result<_, Malformed> {
            const GRID_RELATIVE: u8 = 0b0000_1000;
            let (flags, positive) = match &message.section3()?.payload.template {
                GridDefinitionTemplate::_3_0(Template3_0 { lat_lon: LatLonGrid { scanning_mode, .. }, .. })
                | GridDefinitionTemplate::_3_40(Template3_40 { gaussian: GaussianGrid { scanning_mode, .. }, .. }) => (None, scanning_mode.scans_positively_for_i()),
                GridDefinitionTemplate::_3_1(Template3_1 { lat_lon: LatLonGrid { grid, scanning_mode, .. }, .. })
                | GridDefinitionTemplate::_3_41(Template3_41 { gaussian: GaussianGrid { grid, scanning_mode, .. }, .. }) =>
                    (Some(grid.resolution_and_component_flags.0), scanning_mode.scans_positively_for_i()),
                GridDefinitionTemplate::_3_10(Template3_10 { resolution_and_component_flags, scanning_mode, .. })
                | GridDefinitionTemplate::_3_20(Template3_20 { resolution_and_component_flags, scanning_mode, .. })
                | GridDefinitionTemplate::_3_30(Template3_30 { resolution_and_component_flags, scanning_mode, .. }) => (Some(resolution_and_component_flags.0), scanning_mode.scans_positively_for_i()),
                _ => return Err(Malformed::Grid),
            };
            let sign = if positive { 1.0 } else { -1.0 };
            let sites = flags.map_or_else(|| geographic(&message, sign, locations), |flags| projected(&message, flags & GRID_RELATIVE != 0, sign, locations))?;
            let wanted = sites.iter().flatten().flat_map(|cell| cell.corners).collect::<HashSet<_>>();
            let indices = message.ij()?.enumerate().filter_map(|(index, (column, row))| wanted.contains(&(row, column)).then_some((index, (row, column)))).collect_vec();
            Ok(Grid { sites, indices })
        })?;
        let values = {
            let decoder = Grib2SubmessageDecoder::from(message)?;
            grid.indices
                .iter()
                .scan((decoder.dispatch()?, 0), |(samples, next), &(index, corner)| {
                    let value = samples.nth(index.strict_sub(*next));
                    *next = index.strict_add(1);
                    Some(value.map(|value| (corner, f64::from(value))))
                })
                .flatten()
                .collect::<HashMap<_, _>>()
        };
        grid.sites
            .iter()
            .enumerate()
            .map(|(site, cell)| {
                let Cell { corners, axes: [row_axis, column_axis], at: (row_at, column_at), rotation } = cell.ok_or(Malformed::Outside)?;
                let [south_west, south_east, north_west, north_east] = corners.try_map(|corner| values.get(&corner).copied().ok_or(Malformed::Grid))?;
                let lattice = [[south_west, south_east], [north_west, north_east]];
                let interpolated = |samples: &[[f64; 2]; 2]| -> Result<f64, Malformed> {
                    Interp2D::builder(aview2(samples))
                        .x(aview1(&row_axis))
                        .y(aview1(&column_axis))
                        .build()
                        .map_err(Malformed::Lattice)?
                        .interp_scalar(row_at, column_at)
                        .map_err(|InterpolateError::OutOfBounds(_)| Malformed::Outside)
                };
                let value = match product.field {
                    Field::WindDirection => {
                        let lattice = lattice.map(|row| row.map(|degrees| heading(Angle::new::<degree>(degrees))));
                        let [east, north] = [|toward: Vector2<f64>| toward.x, |toward: Vector2<f64>| toward.y].map(|component| interpolated(&lattice.map(|row| row.map(component))));
                        bearing(Vector2::new(east?, north?)).get::<degree>()
                    }
                    Field::DryBulb
                    | Field::DewPoint
                    | Field::RelativeHumidity
                    | Field::Pressure
                    | Field::Infrared
                    | Field::GlobalIrradiance
                    | Field::DirectHorizontal
                    | Field::WindSpeed
                    | Field::Eastward
                    | Field::Northward
                    | Field::TotalSkyCover
                    | Field::Visibility
                    | Field::PrecipitableWater
                    | Field::SnowDepth
                    | Field::Albedo
                    | Field::Precipitation
                    | Field::Orography => interpolated(&lattice)?,
                };
                Ok(Reading { site, member: product.member, field: product.field, interval: product.interval, value: value.is_finite().then_some(value * product.weight), rotation })
            })
            .collect()
    }

    fn accumulated(totals: Vec<Reading>) -> HashMap<(usize, Option<u8>, Field), Vec<Reading>> {
        let totals = totals.into_iter().sorted_by_key(|reading| (reading.site, reading.member, reading.field, reading.interval.start, reading.interval.end)).collect_vec();
        totals
            .chunk_by(|earlier, later| (earlier.site, earlier.member, earlier.field, earlier.interval.start) == (later.site, later.member, later.field, later.interval.start))
            .flat_map(|accumulation| {
                let first = accumulation[0];
                let origin = Reading { interval: Range { start: first.interval.start, end: first.interval.start }, value: Some(0.0), ..first };
                iter::once(origin).chain(accumulation.iter().copied()).dedup_by(|earlier, later| earlier.interval.end == later.interval.end).tuple_windows().map(|(previous, reading)| {
                    let interval = Range { start: previous.interval.end, ..reading.interval };
                    Reading { interval, value: previous.value.zip(reading.value).map(|(before, after)| (after - before) / length(interval).as_secs_f64()), ..reading }
                })
            })
            .into_group_map_by(|step| (step.site, step.member, step.field))
            .into_iter()
            .map(|(key, run)| (key, run.into_iter().sorted_by_key(|step| (step.interval.start, step.interval.end)).dedup_by(|kept, next| next.interval.start < kept.interval.end).collect_vec()))
            .collect()
    }

    fn series(sun: &Sun, orography: Option<f64>, instants: &[Reading], steps: &HashMap<Field, Vec<Reading>>) -> Result<Hours, WeatherError> {
        let ends = iter::chain(instants, steps.values().flatten()).map(|reading| reading.interval.end).sorted_unstable().dedup().collect_vec();
        let rotation = iter::chain(instants, steps.values().flatten()).find(|reading| reading.field == Field::Eastward).and_then(|reading| reading.rotation);
        let at = instants.iter().into_group_map_by(|reading| reading.interval.end);
        let lead = match *ends {
            [] => None,
            [first, ..] => {
                let back = ends.get(1).map(|&next| first.checked_sub(next.duration_since(first))).transpose()?;
                Some(steps.values().flatten().map(|step| step.interval.start).filter(|&start| start < first).min().or(back).ok_or_else(|| nev![Refusal::Instant(first)])?)
            }
        };
        let rise = orography.map(|height| *AsRef::<Place>::as_ref(sun).elevation - Length::new::<meter>(height));
        let hour = |interval: Range<Timestamp>| -> Result<Hour, WeatherError> {
            let elapsed = length(interval);
            let mean = |field: Field| -> Result<Option<f64>, ConversionError> {
                steps
                    .get(&field)
                    .map(|run| averaged(run, interval, |step| step.interval, SignedDuration::ZERO, |step| step.value.map(|value| (value, 1.0))))
                    .transpose()?
                    .flatten()
                    .map(|totals| reduced(totals, 1.0))
                    .transpose()
            };
            let value = |field: Field| -> Result<Option<f64>, ConversionError> {
                Ok(mean(field)?.or_else(|| at.get(&interval.end).and_then(|readings| readings.iter().find(|reading| reading.field == field)).and_then(|reading| reading.value)))
            };
            let [grid_dry_bulb, grid_dew_point] = [Field::DryBulb, Field::DewPoint].try_map(|field| value(field).map(|value| value.map(ThermodynamicTemperature::new::<kelvin>)))?;
            let relative_humidity = value(Field::RelativeHumidity)?
                .map(|humidity| Ratio::new::<percent>(humidity).min(Ratio::new::<percent>(100.0)))
                .or_else(|| grid_dry_bulb.zip(grid_dew_point).map(|(air, dew)| comfort::relative_humidity(air, dew)));
            let lifted = grid_dry_bulb.zip(rise).map(|(air, rise)| lapsed(air, rise));
            let dry_bulb = lifted.map_or(grid_dry_bulb, |(air, _)| Some(air));
            let dew_point = match (rise, dry_bulb, relative_humidity) {
                (Some(_), Some(air), Some(humidity)) => comfort::dew_point(air, humidity)?,
                (None | Some(_), _, _) => grid_dew_point,
            };
            let pressure = value(Field::Pressure)?.map(Pressure::new::<pascal>).and_then(|stated| match (rise, lifted) {
                (None, _) => Some(stated),
                (Some(_), Some((_, scale))) => Some(stated * scale),
                (Some(_), None) => None,
            });
            let [infrared, global_irradiance, beam] = [Field::Infrared, Field::GlobalIrradiance, Field::DirectHorizontal]
                .try_map(|field| value(field).map(|value| value.map(|flux| HeatFluxDensity::new::<watt_per_square_meter>(flux.max(0.0)))))?;
            let direct_irradiance = beam.map(|beam| position(sun, interval).map(|(prepared, solar)| normal(beam, &prepared, &solar))).transpose()?;
            let wind = value(Field::Eastward)?.zip(value(Field::Northward)?).map(|(eastward, northward)| -(rotation.unwrap_or_else(Rotation2::identity) * Vector2::new(eastward, northward)));
            let precipitation = mean(Field::Precipitation)?.map(|rate| Length::new::<millimeter>((rate * elapsed.as_secs_f64()).max(0.0)));
            Ok(Hour {
                interval,
                dry_bulb,
                dew_point,
                relative_humidity,
                pressure,
                infrared,
                global_irradiance,
                direct_irradiance,
                wind_direction: value(Field::WindDirection)?.map(Angle::new::<degree>).or_else(|| wind.map(bearing)),
                wind_speed: value(Field::WindSpeed)?.or_else(|| wind.as_ref().map(Vector2::norm)).map(Velocity::new::<meter_per_second>),
                total_sky_cover: value(Field::TotalSkyCover)?.map(Ratio::new::<percent>),
                visibility: value(Field::Visibility)?.map(Length::new::<meter>),
                precipitable_water: value(Field::PrecipitableWater)?.map(|depth| Length::new::<millimeter>(depth.max(0.0))),
                snow_depth: value(Field::SnowDepth)?.map(|depth| Length::new::<meter>(depth.max(0.0))),
                albedo: value(Field::Albedo)?.map(Ratio::new::<percent>),
                precipitation,
                precipitation_period: precipitation.map(|_| Time::new::<second>(elapsed.as_secs_f64())),
                ..
            })
        };
        let records = iter::chain(lead, ends.iter().copied()).tuple_windows().map(|(start, end)| hour(Range { start, end })).collect::<Result<Vec<_>, _>>()?;
        completed(&Hours::try_new(sun.clone(), records)?)
    }

    let locations = suns.iter().map(|sun| *AsRef::<Location>::as_ref(sun)).collect_vec();
    let parsed = files.into_iter().map(grib::from_reader).collect::<Result<Vec<_>, _>>()?;
    let grids = parsed
        .iter()
        .enumerate()
        .flat_map(|(file, grib)| grib.submessages().enumerate().map(move |(message, (_, submessage))| (Record::Message { file, message }, submessage)))
        .chunk_by(|(_, message)| message.grid_def().clone());
    let readings = (&grids)
        .into_iter()
        .flat_map(|(_, grid)| {
            let locations = locations.as_slice();
            grid.scan(OnceCell::new(), move |cache, (record, message)| Some(sampled(cache, locations, message).map_err(|malformed| (record, malformed))))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?
        .into_iter()
        .flatten()
        .collect_vec();
    let orography = readings.iter().filter(|reading| reading.field == Field::Orography).map(|reading| (reading.site, reading.value)).into_grouping_map().reduce(|kept, _, next| kept.or(next));
    let by_site = {
        let (instants, totals): (Vec<_>, Vec<_>) = readings.into_iter().filter(|reading| reading.field != Field::Orography).partition(|reading| reading.interval.is_empty());
        let by_site = instants.into_iter().fold(suns.iter().map(|_| IndexMap::<_, (Vec<_>, HashMap<_, _>)>::new()).collect_vec(), |mut by_site, reading| {
            by_site[reading.site].entry(reading.member).or_default().0.push(reading);
            by_site
        });
        accumulated(totals).into_iter().fold(by_site, |mut by_site, ((site, member, field), run)| {
            by_site[site].entry(member).or_default().1.insert(field, run);
            by_site
        })
    };
    iter::zip(suns, by_site)
        .enumerate()
        .map(|(site, (sun, members))| {
            let orography = orography.get(&site).copied().flatten();
            members.into_iter().map(|(member, (instants, steps))| series(sun, orography, &instants, &steps).map(|hours| (member, hours))).collect::<Result<BTreeMap<_, _>, _>>()
        })
        .collect()
}

fn geographic<R>(message: &SubMessage<'_, R>, sign: f64, locations: &[Location]) -> Result<Vec<Option<Cell>>, Malformed> {
    let full = Angle::FULL_TURN.get::<degree>();
    let (ni, nj) = message.grid_shape()?;
    (ni >= 2 && nj >= 2).ok_or(Malformed::Grid)?;
    let (rows, columns): (Vec<_>, Vec<_>) = Validated::from(message.ij())
        .map2(Validated::from(message.latlons()), Iterator::zip)
        .ok()
        .map_err(Malformed::Decode)?
        .flat_map(|((column, row), (latitude, longitude))| iter::chain((column == 0).then_some(Either::Left((row, latitude))), (row == 0).then_some(Either::Right((column, longitude)))))
        .partition_map(convert::identity);
    let &(_, origin) = columns.iter().find(|&&(column, _)| column == 0).ok_or(Malformed::Grid)?;
    let latitudes = rows.into_iter().sorted_by(|south, north| south.1.total_cmp(&north.1)).collect_vec();
    let meridians = columns
        .into_iter()
        .map(|(column, longitude)| (column, sign.mul_add((sign * (longitude - origin)).rem_euclid(full), origin)))
        .sorted_by(|west, east| west.1.total_cmp(&east.1))
        .dedup_by(|west, east| east.1 - west.1 < RESOLUTION)
        .collect_vec();
    let closing = meridians.first().zip(meridians.get(1)).zip(meridians.last()).and_then(|((&(column, first), &(_, next)), &(_, last))| {
        let step = next - first;
        ((first + full - last - step).abs() < step / 2.0).then_some((column, first + full))
    });
    let longitudes = iter::chain(meridians, closing).collect_vec();
    let bracket =
        |axis: &[(usize, f64)], at: f64| axis.get(axis.partition_point(|&(_, node)| node < at).saturating_sub(1).min(axis.len().saturating_sub(2))..).and_then(<[_]>::first_chunk::<2>).copied();
    locations
        .iter()
        .map(|location| {
            let longitude = longitudes.first().map_or(location.longitude, |&(_, first)| first + (location.longitude - first).rem_euclid(full));
            let (Some([south, north]), Some([west, east])) = (bracket(&latitudes, location.latitude), bracket(&longitudes, longitude)) else {
                return Err(Malformed::Grid);
            };
            Ok(Some(Cell {
                corners: [(south.0, west.0), (south.0, east.0), (north.0, west.0), (north.0, east.0)],
                axes: [[south.1, north.1], [west.1, east.1]],
                at: (location.latitude, longitude),
                rotation: None,
            }))
        })
        .collect()
}

fn projected<R>(message: &SubMessage<'_, R>, relative: bool, sign: f64, locations: &[Location]) -> Result<Vec<Option<Cell>>, Malformed> {
    let (ni, nj) = message.grid_shape()?;
    let width = NonZero::new(ni).filter(|_| ni >= 2 && nj >= 2).ok_or(Malformed::Grid)?;
    let unit = |latitude: f64, longitude: f64| {
        let ((rise, reach), (sideways, forward)) = (latitude.to_radians().sin_cos(), longitude.to_radians().sin_cos());
        (Vector3::new(reach * forward, reach * sideways, rise), Vector3::new(-sideways, forward, 0.0))
    };
    let points = Validated::from(message.ij())
        .map2(Validated::from(message.latlons()), Iterator::zip)
        .ok()
        .map_err(Malformed::Decode)?
        .map(|((column, row), (latitude, longitude))| (row.strict_mul(ni).strict_add(column), Point3::from(unit(latitude, longitude).0)))
        .sorted_unstable_by_key(|&(index, _)| index)
        .map(|(_, point)| point)
        .collect_vec();
    let frames = locations.iter().map(|location| unit(location.latitude, location.longitude)).collect_vec();
    let cloud = NESlice::try_from_slice(&points).ok_or(Malformed::Grid).and_then(|points| Cloud::new(&points).map_err(Malformed::Proximity))?;
    let closest = nearest(&cloud, &frames.iter().map(|&(up, _)| Point3::from(up)).collect_vec()).map_err(Malformed::Proximity)?;
    iter::zip(frames, closest)
        .map(|((up, east), Neighbor { point: _, item, distance: _ })| {
            let north = up.cross(&east);
            let plane = |point: &Point3<f64>| (point.coords.dot(&up) > 0.0).then(|| Vector2::new(point.coords.dot(&east), point.coords.dot(&north)));
            let (nearest_row, nearest_column) = (item / width, item % width);
            let corner = |row: usize, column: usize| points.get(row.strict_mul(ni).strict_add(column)).and_then(plane);
            let shares = |south: usize, west: usize| {
                let (upper, right) = (south.strict_add(1), west.strict_add(1));
                let (Some(origin), Some(onward), Some(upward), Some(far)) = (corner(south, west), corner(south, right), corner(upper, west), corner(upper, right)) else {
                    return Ok(None);
                };
                let (step, rise, twist, site) = (onward - origin, upward - origin, origin - onward - upward + far, -origin);
                roots::real(&[site.perp(&step), step.perp(&rise) + site.perp(&twist), twist.perp(&rise)], RangeInclusive { start: 0.0, last: 1.0 }).map_err(Malformed::Analysis).map(|found| {
                    found.into_iter().find_map(|upward_share| {
                        let axis = step + twist * upward_share;
                        let onward_share = (site - rise * upward_share).dot(&axis) / axis.norm_squared();
                        (0.0..=1.0).contains(&onward_share).then_some(([(south, west), (south, right), (upper, west), (upper, right)], (upward_share, onward_share), axis))
                    })
                })
            };
            [nearest_row.checked_sub(1), Some(nearest_row)]
                .into_iter()
                .flatten()
                .filter(|&south| south.strict_add(1) < nj)
                .cartesian_product([nearest_column.checked_sub(1), Some(nearest_column)].into_iter().flatten().filter(|&west| west.strict_add(1) < ni))
                .map(|(south, west)| shares(south, west))
                .find_map(Result::transpose)
                .transpose()
                .map(|found| {
                    found.map(|(corners, at, axis)| Cell { corners, axes: [[0.0, 1.0], [0.0, 1.0]], at, rotation: relative.then(|| Rotation2::rotation_between(&Vector2::x(), &(axis * sign))) })
                })
        })
        .collect()
}
