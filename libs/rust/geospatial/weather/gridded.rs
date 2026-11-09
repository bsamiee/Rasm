use core::{cell::OnceCell, convert, iter, range::Range};
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
            param_set::{DateTime, EarthShape, EnsembleForecast, GaussianGrid, LatLonGrid, ScanningMode, Statistical, StatisticalProcess, TimeRange},
        },
    },
    projection::{self, Lcc, LccParams, Merc, MercParams, Project, Stere, StereParams},
};
use indexmap::IndexMap;
use itertools::{Either, Itertools as _};
use jiff::{SignedDuration, Span, Timestamp, civil, tz::Offset};
use nalgebra::{Rotation2, Vector2};
use ndarray::{aview1, aview2};
use ndarray_interp::{InterpolateError, interp2d::Interp2D};
use nonempty_collections::nev;
use rasm_site::Place;
use uom::{
    ConstZero as _,
    si::{
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
    },
};
use validated::Validated;

use crate::{Hour, Hours, Malformed, Record, Refusal, WeatherError, averaged, bearing, heading, length};

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

#[derive(Debug, Clone, PartialEq)]
pub struct Sampled {
    pub surface: Option<Length>,
    pub members: BTreeMap<Option<u8>, Hours>,
}

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

pub fn read<R, I>(places: &[Place], files: I) -> Result<Vec<Sampled>, WeatherError>
where
    R: io::Read + io::Seek,
    I: IntoIterator<Item = R>,
{
    fn sampled<R>(grid: &OnceCell<Grid>, locations: &[(f64, f64)], message: SubMessage<'_, R>) -> Result<Vec<Reading>, Malformed>
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
            let section = message.section3()?;
            let (flags, positive) = match &section.payload.template {
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
            let sites = flags.map_or_else(|| geographic(&message, sign, locations), |flags| projected(&message, &section.payload.template, flags & GRID_RELATIVE != 0, locations))?;
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

    fn series(place: &Place, instants: &[Reading], steps: &HashMap<Field, Vec<Reading>>) -> Result<Hours, WeatherError> {
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
        let hour = |interval: Range<Timestamp>| {
            let elapsed = length(interval);
            let mean = |field: Field| steps.get(&field).and_then(|run| averaged(run, interval, |step| step.interval, SignedDuration::ZERO, |step| step.value.map(|value| (value, 1.0))));
            let value = |field: Field| mean(field).or_else(|| at.get(&interval.end).and_then(|readings| readings.iter().find(|reading| reading.field == field)).and_then(|reading| reading.value));
            let [dry_bulb, dew_point] = [Field::DryBulb, Field::DewPoint].map(|field| value(field).map(ThermodynamicTemperature::new::<kelvin>));
            let [infrared, global_irradiance, beam] =
                [Field::Infrared, Field::GlobalIrradiance, Field::DirectHorizontal].map(|field| value(field).map(|flux| HeatFluxDensity::new::<watt_per_square_meter>(flux.max(0.0))));
            let wind = value(Field::Eastward).zip(value(Field::Northward)).map(|(eastward, northward)| -(rotation.unwrap_or_else(Rotation2::identity) * Vector2::new(eastward, northward)));
            let precipitation = mean(Field::Precipitation).map(|rate| Length::new::<millimeter>((rate * elapsed.as_secs_f64()).max(0.0)));
            Hour {
                interval,
                dry_bulb,
                dew_point,
                relative_humidity: value(Field::RelativeHumidity).map(|humidity| Ratio::new::<percent>(humidity).min(Ratio::new::<percent>(100.0))),
                pressure: value(Field::Pressure).map(Pressure::new::<pascal>),
                infrared,
                global_irradiance,
                diffuse_irradiance: global_irradiance.zip(beam).map(|(global, beam)| (global - beam).max(HeatFluxDensity::ZERO)),
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
            }
        };
        let records = iter::chain(lead, ends.iter().copied()).tuple_windows().map(|(start, end)| hour(Range { start, end })).collect_vec();
        Hours::try_new(place.clone(), records)
    }

    let locations = places.iter().map(|place| (place.position.latitude().get::<degree>(), place.position.longitude().get::<degree>())).collect_vec();
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
        let by_site = instants.into_iter().fold(places.iter().map(|_| IndexMap::<_, (Vec<_>, HashMap<_, _>)>::new()).collect_vec(), |mut by_site, reading| {
            by_site[reading.site].entry(reading.member).or_default().0.push(reading);
            by_site
        });
        accumulated(totals).into_iter().fold(by_site, |mut by_site, ((site, member, field), run)| {
            by_site[site].entry(member).or_default().1.insert(field, run);
            by_site
        })
    };
    iter::zip(places, by_site)
        .enumerate()
        .map(|(site, (place, members))| {
            let members = members.into_iter().map(|(member, (instants, steps))| series(place, &instants, &steps).map(|hours| (member, hours))).collect::<Result<BTreeMap<_, _>, _>>()?;
            Ok(Sampled { surface: orography.get(&site).copied().flatten().map(Length::new::<meter>), members })
        })
        .collect()
}

fn geographic<R>(message: &SubMessage<'_, R>, sign: f64, locations: &[(f64, f64)]) -> Result<Vec<Option<Cell>>, Malformed> {
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
        .map(|&(latitude, longitude)| {
            let longitude = longitudes.first().map_or(longitude, |&(_, first)| first + (longitude - first).rem_euclid(full));
            let (Some([south, north]), Some([west, east])) = (bracket(&latitudes, latitude), bracket(&longitudes, longitude)) else {
                return Err(Malformed::Grid);
            };
            Ok(Some(Cell {
                corners: [(south.0, west.0), (south.0, east.0), (north.0, west.0), (north.0, east.0)],
                axes: [[south.1, north.1], [west.1, east.1]],
                at: (latitude, longitude),
                rotation: None,
            }))
        })
        .collect()
}

fn projected<R>(message: &SubMessage<'_, R>, template: &GridDefinitionTemplate, relative: bool, locations: &[(f64, f64)]) -> Result<Vec<Option<Cell>>, Malformed> {
    const ANGLE: f64 = 1.0e-6;
    const SPACING: f64 = 1.0e-3;
    let (ni, nj) = message.grid_shape()?;
    (ni >= 2 && nj >= 2).ok_or(Malformed::Grid)?;
    let ellipsoid = |shape: &EarthShape| shape.radii().map(|(major, minor)| projection::Ellipsoid::from_a_and_b(major, minor)).ok_or(Malformed::Grid);
    let spacing = |scanning: &ScanningMode, dx: u32, dy: u32| {
        [(dx, scanning.scans_positively_for_i()), (dy, scanning.scans_positively_for_j())].map(|(step, positive)| f64::from(step) * SPACING * if positive { 1.0 } else { -1.0 })
    };
    let radians = |longitude: f64, latitude: f64| (longitude.to_radians(), latitude.to_radians());
    let (projection, first, [dx, dy]): (Box<dyn Project>, _, [f64; 2]) = match template {
        GridDefinitionTemplate::_3_30(Template3_30 { earth_shape, first_point_lat, first_point_lon, lad, lov, dx, dy, scanning_mode, latin1, latin2, .. }) => (
            Box::new(
                Lcc::new(&LccParams {
                    ellipsoid: ellipsoid(earth_shape)?,
                    lat_0: f64::from(*lad) * ANGLE,
                    lon_0: f64::from(*lov) * ANGLE,
                    lat_1: f64::from(*latin1) * ANGLE,
                    lat_2: f64::from(*latin2) * ANGLE,
                })
                .map_err(Malformed::Projection)?,
            ),
            radians(f64::from(*first_point_lon) * ANGLE, f64::from(*first_point_lat) * ANGLE),
            spacing(scanning_mode, *dx, *dy),
        ),
        GridDefinitionTemplate::_3_20(Template3_20 { earth_shape, first_point_lat, first_point_lon, lad, lov, dx, dy, projection_centre, scanning_mode, .. }) if !projection_centre.is_bipolar() => (
            Box::new(
                Stere::new(&StereParams {
                    ellipsoid: ellipsoid(earth_shape)?,
                    lat_ts: f64::from(*lad) * ANGLE,
                    lat_0: if projection_centre.contains_north_pole_on_projection_plane() { 90.0 } else { -90.0 },
                    lon_0: f64::from(*lov) * ANGLE,
                    k_0: 1.0,
                })
                .map_err(Malformed::Projection)?,
            ),
            radians(f64::from(*first_point_lon) * ANGLE, f64::from(*first_point_lat) * ANGLE),
            spacing(scanning_mode, *dx, *dy),
        ),
        GridDefinitionTemplate::_3_10(Template3_10 { earth_shape, first_point_lat, first_point_lon, lad, last_point_lat, last_point_lon, scanning_mode, orientation: 0, .. }) => {
            let full = Angle::FULL_TURN.get::<degree>();
            let [west, east] = [first_point_lon, last_point_lon].map(|longitude| f64::from(*longitude) * ANGLE);
            let (west, east) = match (scanning_mode.scans_positively_for_i(), east - west) {
                (true, gap) if gap < 0.0 => (west, east + full),
                (false, gap) if gap > 0.0 => (west + full, east),
                (true | false, _) => (west, east),
            };
            let projection = Merc::new(&MercParams { ellipsoid: ellipsoid(earth_shape)?, lat_ts: f64::from(*lad) * ANGLE, lon_0: west.midpoint(east) }).map_err(Malformed::Projection)?;
            let [start, end] = [(west, *first_point_lat), (east, *last_point_lat)]
                .try_map(|(longitude, latitude)| projection.project(&radians(longitude, f64::from(latitude) * ANGLE), false).map_err(Malformed::Projection))?;
            let spacing = [(end.0 - start.0, ni), (end.1 - start.1, nj)].map(|(span, count)| span / nalgebra::convert::<usize, f64>(count.strict_sub(1)));
            (Box::new(projection), radians(west, f64::from(*first_point_lat) * ANGLE), spacing)
        }
        _ => return Err(Malformed::Grid),
    };
    let forward = |point: (f64, f64)| projection.project(&point, false).map_err(Malformed::Projection);
    let origin = forward(first)?;
    let cell = |at: f64, count: usize| {
        (0.0..=nalgebra::convert::<usize, f64>(count.strict_sub(1))).contains(&at).then(|| {
            let lower = at.floor().to_int_saturating::<usize>().min(count.strict_sub(2));
            (lower, at - nalgebra::convert::<usize, f64>(lower))
        })
    };
    locations
        .iter()
        .map(|&(latitude, longitude)| {
            let site = radians(longitude, latitude);
            let (x, y) = forward(site)?;
            let rotation = relative
                .then(|| {
                    projection
                        .project(&(x + dx.abs(), y), true)
                        .map(|(east, north)| Rotation2::rotation_between(&Vector2::x(), &Vector2::new((east - site.0).sin() * site.1.cos(), north - site.1)))
                        .map_err(Malformed::Projection)
                })
                .transpose()?;
            Ok(cell((y - origin.1) / dy, nj).zip(cell((x - origin.0) / dx, ni)).map(|((south, row_at), (west, column_at))| {
                let (upper, right) = (south.strict_add(1), west.strict_add(1));
                Cell { corners: [(south, west), (south, right), (upper, west), (upper, right)], axes: [[0.0, 1.0], [0.0, 1.0]], at: (row_at, column_at), rotation }
            }))
        })
        .collect()
}
