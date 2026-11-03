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
use itertools::{Either, Itertools as _};
use jiff::{SignedDuration, Span, Timestamp, civil, tz::Offset};
use nalgebra::{Rotation2, Vector2, Vector3};
use ndarray::{Array1, Array2, arr2};
use ndarray_interp::{InterpolateError, interp2d::Interp2D};
use nonempty_collections::nev;
use rasm_analysis::roots;
use rasm_site::Place;
use rasm_solar::{Sun, lapsed};
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

use crate::{Hour, Hours, Malformed, Record, Refusal, WeatherError, averaged, bearing, comfort, completed, heading, length, normal, position};

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
    fn sampled<R>(cells: &OnceCell<Vec<Option<Cell>>>, locations: &[Location], message: SubMessage<'_, R>) -> Result<Vec<Reading>, Malformed>
    where R: Grib2Read {
        fn product<R>(message: &SubMessage<'_, R>) -> Result<Option<Product>, Malformed> {
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
            let member = |EnsembleForecast { ensemble_forecast_type: _, perturbation_num, num_forecasts: _ }: EnsembleForecast| Some(perturbation_num);
            let (forecast_time, statistical, member) = match message.section4()?.payload.template {
                ProductDefinitionTemplate::_4_0(Template4_0 { param: _, generating_process: _, forecast_time, horizontal: _ }) => (forecast_time, None, None),
                ProductDefinitionTemplate::_4_1(Template4_1 { param: _, generating_process: _, forecast_time, horizontal: _, emsemble_forecast }) => (forecast_time, None, member(emsemble_forecast)),
                ProductDefinitionTemplate::_4_8(Template4_8 { param: _, generating_process: _, forecast_time, horizontal: _, time_interval }) => (forecast_time, Some(time_interval), None),
                ProductDefinitionTemplate::_4_11(Template4_11 { param: _, generating_process: _, forecast_time, horizontal: _, emsemble_forecast, time_interval }) =>
                    (forecast_time, Some(time_interval), member(emsemble_forecast)),
                _ => return Ok(None),
            };
            let valid = span(&forecast_time.time).and_then(|offset| stamped(&message.section1()?.payload.ref_time, offset))?;
            let statistic = statistical
                .map(|Statistical { end_of_overall_time_interval, num_time_ranges: _, num_missing: _, time_ranges }| {
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
        }

        fn lattice<R>(message: &SubMessage<'_, R>, template: &GridDefinitionTemplate, locations: &[Location]) -> Result<Vec<Option<Cell>>, Malformed> {
            const GRID_RELATIVE: u8 = 0b0000_1000;
            let sign = |positive: bool| if positive { 1.0_f64 } else { -1.0 };
            match template {
                GridDefinitionTemplate::_3_0(Template3_0 { earth: _, lat_lon: LatLonGrid { grid: _, i_direction_inc: _, j_direction_inc: _, scanning_mode } })
                | GridDefinitionTemplate::_3_40(Template3_40 { earth: _, gaussian: GaussianGrid { grid: _, i_direction_inc: _, n: _, scanning_mode } }) =>
                    geographic(message, sign(scanning_mode.scans_positively_for_i()), locations),
                GridDefinitionTemplate::_3_1(Template3_1 { earth: _, lat_lon: LatLonGrid { grid, i_direction_inc: _, j_direction_inc: _, scanning_mode }, rotation: _ })
                | GridDefinitionTemplate::_3_41(Template3_41 { earth: _, gaussian: GaussianGrid { grid, i_direction_inc: _, n: _, scanning_mode }, rotation: _ }) =>
                    projected(message, grid.resolution_and_component_flags.0 & GRID_RELATIVE != 0, sign(scanning_mode.scans_positively_for_i()), locations),
                GridDefinitionTemplate::_3_10(Template3_10 {
                    earth_shape: _,
                    ni: _,
                    nj: _,
                    first_point_lat: _,
                    first_point_lon: _,
                    resolution_and_component_flags,
                    lad: _,
                    last_point_lat: _,
                    last_point_lon: _,
                    scanning_mode,
                    orientation: _,
                    di: _,
                    dj: _,
                })
                | GridDefinitionTemplate::_3_20(Template3_20 {
                    earth_shape: _,
                    ni: _,
                    nj: _,
                    first_point_lat: _,
                    first_point_lon: _,
                    resolution_and_component_flags,
                    lad: _,
                    lov: _,
                    dx: _,
                    dy: _,
                    projection_centre: _,
                    scanning_mode,
                })
                | GridDefinitionTemplate::_3_30(Template3_30 {
                    earth_shape: _,
                    ni: _,
                    nj: _,
                    first_point_lat: _,
                    first_point_lon: _,
                    resolution_and_component_flags,
                    lad: _,
                    lov: _,
                    dx: _,
                    dy: _,
                    projection_centre: _,
                    scanning_mode,
                    latin1: _,
                    latin2: _,
                    south_pole_lat: _,
                    south_pole_lon: _,
                }) => projected(message, resolution_and_component_flags.0 & GRID_RELATIVE != 0, sign(scanning_mode.scans_positively_for_i()), locations),
                _ => Err(Malformed::Grid),
            }
        }

        let Some(product) = product(&message)? else {
            return Ok(Vec::new());
        };
        let cells = cells.get_or_try_init(|| lattice(&message, &message.section3()?.payload.template, locations))?;
        let wanted = cells.iter().flatten().flat_map(|cell| cell.corners).collect::<HashSet<_>>();
        let indices = message.ij()?;
        let decoder = Grib2SubmessageDecoder::from(message)?;
        let values =
            indices.zip(decoder.dispatch()?).filter(|&((column, row), _)| wanted.contains(&(row, column))).map(|((column, row), value)| ((row, column), f64::from(value))).collect::<HashMap<_, _>>();
        cells
            .iter()
            .enumerate()
            .map(|(site, cell)| {
                let Cell { corners, axes: [row_axis, column_axis], at: (row_at, column_at), rotation } = cell.ok_or(Malformed::Outside)?;
                let [south_west, south_east, north_west, north_east] = corners.try_map(|corner| values.get(&corner).copied().ok_or(Malformed::Grid))?;
                let lattice = arr2(&[[south_west, south_east], [north_west, north_east]]);
                let interpolated = |samples: Array2<f64>| -> Result<f64, Malformed> {
                    Interp2D::builder(samples)
                        .x(Array1::from_vec(row_axis.to_vec()))
                        .y(Array1::from_vec(column_axis.to_vec()))
                        .build()
                        .map_err(Malformed::Lattice)?
                        .interp_scalar(row_at, column_at)
                        .map_err(|InterpolateError::OutOfBounds(_)| Malformed::Outside)
                };
                let value = match product.field {
                    Field::WindDirection => {
                        let [east, north] =
                            [|toward: Vector2<f64>| toward.x, |toward: Vector2<f64>| toward.y].map(|component| interpolated(lattice.mapv(|degrees| component(heading(Angle::new::<degree>(degrees))))));
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
                    | Field::Orography => interpolated(lattice)?,
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
                let origin = accumulation.first().map(|&first| Reading { interval: Range { start: first.interval.start, end: first.interval.start }, value: Some(0.0), ..first });
                iter::chain(origin, accumulation.iter().copied()).dedup_by(|earlier, later| earlier.interval.end == later.interval.end).tuple_windows().map(|(previous, reading)| {
                    let interval = Range { start: previous.interval.end, ..reading.interval };
                    Reading { interval, value: previous.value.zip(reading.value).map(|(before, after)| (after - before) / length(interval).as_secs_f64()), ..reading }
                })
            })
            .into_group_map_by(|step| (step.site, step.member, step.field))
            .into_iter()
            .map(|(key, run)| (key, run.into_iter().sorted_by_key(|step| (step.interval.start, step.interval.end)).dedup_by(|kept, next| next.interval.start < kept.interval.end).collect_vec()))
            .collect()
    }

    fn series(sun: &Sun, orography: Option<f64>, instants: &[Reading], steps: &HashMap<Field, &[Reading]>) -> Result<Hours, WeatherError> {
        let ends = iter::chain(instants, steps.values().copied().flatten()).map(|reading| reading.interval.end).sorted_unstable().dedup().collect_vec();
        let rotation = iter::chain(instants, steps.values().copied().flatten()).find(|reading| reading.field == Field::Eastward).and_then(|reading| reading.rotation);
        let at = instants.iter().into_group_map_by(|reading| reading.interval.end);
        let lead = match *ends {
            [] => None,
            [first, ..] => {
                let back = ends.get(1).map(|&next| first.checked_sub(next.duration_since(first))).transpose()?;
                Some(steps.values().copied().flatten().map(|step| step.interval.start).filter(|&start| start < first).min().or(back).ok_or_else(|| nev![Refusal::Instant(first)])?)
            }
        };
        let rise = orography.map(|height| *AsRef::<Place>::as_ref(sun).elevation - Length::new::<meter>(height));
        let hour = |interval: Range<Timestamp>| -> Result<Hour, WeatherError> {
            let elapsed = length(interval);
            let mean = |field: Field| steps.get(&field).and_then(|run| averaged(run, interval, |step| step.interval, SignedDuration::ZERO, |step| step.value));
            let value = |field: Field| mean(field).or_else(|| at.get(&interval.end).and_then(|readings| readings.iter().find(|reading| reading.field == field)).and_then(|reading| reading.value));
            let [grid_dry_bulb, grid_dew_point] = [Field::DryBulb, Field::DewPoint].map(|field| value(field).map(ThermodynamicTemperature::new::<kelvin>));
            let relative_humidity = value(Field::RelativeHumidity)
                .map(|humidity| Ratio::new::<percent>(humidity).min(Ratio::new::<percent>(100.0)))
                .or_else(|| grid_dry_bulb.zip(grid_dew_point).map(|(air, dew)| comfort::relative_humidity(air, dew)));
            let lifted = grid_dry_bulb.zip(rise).map(|(air, rise)| lapsed(air, rise));
            let dry_bulb = lifted.map_or(grid_dry_bulb, |(air, _)| Some(air));
            let dew_point = match (rise, dry_bulb, relative_humidity) {
                (Some(_), Some(air), Some(humidity)) => comfort::dew_point(air, humidity)?,
                (None | Some(_), _, _) => grid_dew_point,
            };
            let pressure = value(Field::Pressure).map(Pressure::new::<pascal>).and_then(|stated| match (rise, lifted) {
                (None, _) => Some(stated),
                (Some(_), Some((_, scale))) => Some(stated * scale),
                (Some(_), None) => None,
            });
            let [infrared, global_irradiance, beam] =
                [Field::Infrared, Field::GlobalIrradiance, Field::DirectHorizontal].map(|field| value(field).map(|flux| HeatFluxDensity::new::<watt_per_square_meter>(flux.max(0.0))));
            let direct_irradiance = beam.map(|beam| position(sun, interval).map(|(prepared, solar)| normal(beam, &prepared, &solar))).transpose()?;
            let wind = value(Field::Eastward).zip(value(Field::Northward)).map(|(eastward, northward)| -(rotation.unwrap_or_else(Rotation2::identity) * Vector2::new(eastward, northward)));
            let precipitation = mean(Field::Precipitation).map(|rate| Length::new::<millimeter>((rate * elapsed.as_secs_f64()).max(0.0)));
            Ok(Hour {
                interval,
                dry_bulb,
                dew_point,
                relative_humidity,
                pressure,
                infrared,
                global_irradiance,
                direct_irradiance,
                wind_direction: value(Field::WindDirection).map(Angle::new::<degree>).or_else(|| wind.map(bearing)),
                wind_speed: value(Field::WindSpeed).or_else(|| wind.as_ref().map(Vector2::norm)).map(Velocity::new::<meter_per_second>),
                total_sky_cover: value(Field::TotalSkyCover).map(Ratio::new::<percent>),
                visibility: value(Field::Visibility).map(Length::new::<meter>),
                precipitable_water: value(Field::PrecipitableWater).map(|depth| Length::new::<millimeter>(depth.max(0.0))),
                snow_depth: value(Field::SnowDepth).map(|depth| Length::new::<meter>(depth.max(0.0))),
                albedo: value(Field::Albedo).map(Ratio::new::<percent>),
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
            let (cells, locations) = (OnceCell::new(), locations.as_slice());
            grid.map(move |(record, message)| sampled(&cells, locations, message).map_err(|malformed| (record, malformed)))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?
        .into_iter()
        .flatten()
        .collect_vec();
    let (instants, totals): (Vec<_>, Vec<_>) = readings.iter().copied().filter(|reading| reading.field != Field::Orography).partition(|reading| reading.interval.is_empty());
    let steps = accumulated(totals);
    suns.iter()
        .enumerate()
        .map(|(site, sun)| {
            let orography = readings.iter().find_map(|reading| (reading.site == site && reading.field == Field::Orography).then_some(reading.value).flatten());
            iter::chain(&instants, steps.values().flatten())
                .filter(|reading| reading.site == site)
                .map(|reading| reading.member)
                .unique()
                .map(|member| {
                    let own = |reading: &&Reading| reading.site == site && reading.member == member;
                    let steps = steps.iter().filter(|((stated, held, _), _)| *stated == site && *held == member).map(|(&(_, _, field), run)| (field, run.as_slice())).collect::<HashMap<_, _>>();
                    series(sun, orography, &instants.iter().filter(own).copied().collect_vec(), &steps).map(|hours| (member, hours))
                })
                .collect::<Result<BTreeMap<_, _>, _>>()
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
        .map(|((column, row), (latitude, longitude))| (row.strict_mul(ni).strict_add(column), unit(latitude, longitude).0))
        .sorted_unstable_by_key(|&(index, _)| index)
        .map(|(_, point)| point)
        .collect_vec();
    locations
        .iter()
        .map(|location| {
            let (up, east) = unit(location.latitude, location.longitude);
            let north = up.cross(&east);
            let plane = |point: &Vector3<f64>| (point.dot(&up) > 0.0).then(|| Vector2::new(point.dot(&east), point.dot(&north)));
            let Some((nearest_row, nearest_column)) = points
                .iter()
                .enumerate()
                .filter_map(|(index, point)| plane(point).map(|projected| (index, projected.norm_squared())))
                .min_by(|(_, nearer), (_, farther)| nearer.total_cmp(farther))
                .map(|(index, _)| (index / width, index % width))
            else {
                return Ok(None);
            };
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
