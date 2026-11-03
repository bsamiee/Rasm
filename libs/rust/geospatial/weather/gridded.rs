use core::{
    convert, iter,
    num::NonZero,
    range::{Range, RangeInclusive},
};
use std::{collections::HashMap, io};

use grib::{
    Grib2SubmessageDecoder, LatLons as _, SeekableGrib2Reader, SubMessage,
    codetables::grib2::Table4_4,
    def::grib2::{
        GridDefinitionTemplate, ProductDefinitionTemplate,
        template::{
            Template3_0, Template3_1, Template3_10, Template3_20, Template3_30, Template3_40, Template3_41, Template4_0, Template4_1, Template4_8, Template4_11,
            param_set::{DateTime, GaussianGrid, LatLonGrid, Statistical, StatisticalProcess, TimeRange},
        },
    },
};
use itertools::{Either, Itertools as _};
use jiff::{SignedDuration, Span, Timestamp, civil, tz::Offset};
use nalgebra::{Rotation2, Vector2, Vector3};
use ndarray::{Array1, Array2, arr2};
use ndarray_interp::{BuilderError, InterpolateError, interp2d::Interp2D};
use rasm_analysis::roots;
use rasm_solar::Sun;
use solar_positioning::Location;
use thermalcomfort::{
    Humidity, Temperature,
    psychrometrics::{dew_point_temperature, p_sat},
};
use uom::{
    ConstZero as _,
    si::{
        angle::{degree, radian},
        f64::{Angle, HeatFluxDensity, Length, Pressure, Ratio, ThermodynamicTemperature, Time, Velocity},
        heat_flux_density::watt_per_square_meter,
        length::{meter, millimeter},
        pressure::pascal,
        ratio::{percent, ratio},
        thermodynamic_temperature::kelvin,
        time::second,
        velocity::meter_per_second,
    },
};
use validated::Validated;

use crate::{Hour, Hours, Malformed, Record, WeatherError, extraterrestrial, position};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
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
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy)]
struct Reading {
    field: Field,
    interval: Range<Timestamp>,
    value: Option<f64>,
    rotation: Option<Rotation2<f64>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn read<R>(sun: Sun, reader: R) -> Result<Hours, WeatherError>
where R: io::Read + io::Seek {
    const METEOROLOGY: u8 = 0;
    const GROUND: u8 = 1;
    const ATMOSPHERE: u8 = 10;
    const HEIGHT: u8 = 103;
    const PARAMETERS: [(u8, u8, u8, Option<f64>, Field); 22] = [
        (0, 0, HEIGHT, Some(2.0), Field::DryBulb),
        (0, 6, HEIGHT, Some(2.0), Field::DewPoint),
        (1, 1, HEIGHT, Some(2.0), Field::RelativeHumidity),
        (3, 0, GROUND, None, Field::Pressure),
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
    ];
    const AVERAGE: u8 = 0;
    const ACCUMULATION: u8 = 1;
    const RESOLUTION: f64 = 1.0e-6;
    const GRID_RELATIVE: u8 = 0b0000_1000;
    let file = grib::from_reader(reader)?;
    let &location: &Location = sun.as_ref();
    let sample = |message: SubMessage<'_, SeekableGrib2Reader<R>>| -> Result<Option<Reading>, Malformed> {
        let span = |time: &TimeRange<i32>| -> Result<Span, Malformed> {
            let length = i64::from(time.len);
            match Table4_4::try_from(time.unit).ok() {
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
            }
            .map_err(Malformed::Time)
        };
        let instant = |time: &DateTime, after: Span| -> Result<Timestamp, Malformed> {
            let (Ok(year), Ok(month), Ok(day), Ok(hour), Ok(minute), Ok(seconds)) =
                (i16::try_from(time.year), i8::try_from(time.month), i8::try_from(time.day), i8::try_from(time.hour), i8::try_from(time.minute), i8::try_from(time.second))
            else {
                return Err(Malformed::Undated);
            };
            civil::DateTime::new(year, month, day, hour, minute, seconds, 0)
                .and_then(|reference| reference.checked_add(after))
                .and_then(|valid| Offset::UTC.to_timestamp(valid))
                .map_err(Malformed::Time)
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
        let (forecast_time, statistical) = match message.section4()?.payload.template {
            ProductDefinitionTemplate::_4_0(Template4_0 { param: _, generating_process: _, forecast_time, horizontal: _ })
            | ProductDefinitionTemplate::_4_1(Template4_1 { param: _, generating_process: _, forecast_time, horizontal: _, emsemble_forecast: _ }) => (forecast_time, None),
            ProductDefinitionTemplate::_4_8(Template4_8 { param: _, generating_process: _, forecast_time, horizontal: _, time_interval })
            | ProductDefinitionTemplate::_4_11(Template4_11 { param: _, generating_process: _, forecast_time, horizontal: _, emsemble_forecast: _, time_interval }) =>
                (forecast_time, Some(time_interval)),
            _ => return Ok(None),
        };
        let valid = span(&forecast_time.time).and_then(|offset| instant(&message.section1()?.payload.ref_time, offset))?;
        let statistic = statistical
            .map(|Statistical { end_of_overall_time_interval, num_time_ranges: _, num_missing: _, time_ranges }| {
                instant(&end_of_overall_time_interval, Span::new()).map(|end| (end, <[StatisticalProcess; 1]>::try_from(time_ranges).ok().map(|[process]| process.statistical_process_type)))
            })
            .transpose()?;
        let (interval, weight) = match statistic {
            None => (Range { start: valid, end: valid }, 1.0),
            Some((end, _)) if end <= valid => return Ok(None),
            Some((end, Some(AVERAGE))) => (Range { start: valid, end }, end.duration_since(valid).as_secs_f64()),
            Some((end, Some(ACCUMULATION))) => (Range { start: valid, end }, 1.0),
            Some((_, Some(_) | None)) => return Ok(None),
        };
        let (geographic, scanning, flags) = match message.section3()?.payload.template {
            GridDefinitionTemplate::_3_0(Template3_0 { earth: _, lat_lon: LatLonGrid { grid, i_direction_inc: _, j_direction_inc: _, scanning_mode } })
            | GridDefinitionTemplate::_3_40(Template3_40 { earth: _, gaussian: GaussianGrid { grid, i_direction_inc: _, n: _, scanning_mode } }) =>
                (true, scanning_mode, grid.resolution_and_component_flags),
            GridDefinitionTemplate::_3_1(Template3_1 { earth: _, lat_lon: LatLonGrid { grid, i_direction_inc: _, j_direction_inc: _, scanning_mode }, rotation: _ })
            | GridDefinitionTemplate::_3_41(Template3_41 { earth: _, gaussian: GaussianGrid { grid, i_direction_inc: _, n: _, scanning_mode }, rotation: _ }) =>
                (false, scanning_mode, grid.resolution_and_component_flags),
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
            }) => (false, scanning_mode, resolution_and_component_flags),
            _ => return Err(Malformed::Grid),
        };
        let (ni, nj) = message.grid_shape()?;
        let width = NonZero::new(ni).filter(|_| ni >= 2 && nj >= 2).ok_or(Malformed::Grid)?;
        let indices = message.ij()?;
        let full = Angle::FULL_TURN.get::<degree>();
        let sign = if scanning.scans_positively_for_i() { 1.0_f64 } else { -1.0 };
        let (corners, [row_axis, column_axis], (row_at, column_at), rotation) = if geographic {
            let (rows, columns): (Vec<_>, Vec<_>) = indices
                .clone()
                .zip(message.latlons()?)
                .flat_map(|((column, row), (latitude, longitude))| iter::chain((column == 0).then_some(Either::Left((row, latitude))), (row == 0).then_some(Either::Right((column, longitude)))))
                .partition_map(convert::identity);
            let Some(&(_, origin)) = columns.iter().find(|&&(column, _)| column == 0) else {
                return Err(Malformed::Grid);
            };
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
            let longitude = longitudes.first().map_or(location.longitude, |&(_, first)| first + (location.longitude - first).rem_euclid(full));
            let bracket = |axis: &[(usize, f64)], at: f64| {
                axis.get(axis.partition_point(|&(_, node)| node < at).saturating_sub(1).min(axis.len().saturating_sub(2))..).and_then(<[_]>::first_chunk::<2>).copied()
            };
            let (Some([south, north]), Some([west, east])) = (bracket(&latitudes, location.latitude), bracket(&longitudes, longitude)) else {
                return Err(Malformed::Grid);
            };
            ([(south.0, west.0), (south.0, east.0), (north.0, west.0), (north.0, east.0)], [[south.1, north.1], [west.1, east.1]], (location.latitude, longitude), None)
        } else {
            let unit = |latitude: f64, longitude: f64| {
                let ((rise, reach), (sideways, forward)) = (latitude.to_radians().sin_cos(), longitude.to_radians().sin_cos());
                (Vector3::new(reach * forward, reach * sideways, rise), Vector3::new(-sideways, forward, 0.0))
            };
            let (up, east) = unit(location.latitude, location.longitude);
            let north = up.cross(&east);
            let plane = |point: &Vector3<f64>| (point.dot(&up) > 0.0).then(|| Vector2::new(point.dot(&east), point.dot(&north)));
            let points = indices
                .clone()
                .zip(message.latlons()?)
                .map(|((column, row), (latitude, longitude))| (row.strict_mul(ni).strict_add(column), unit(latitude, longitude).0))
                .sorted_unstable_by_key(|&(index, _)| index)
                .map(|(_, point)| point)
                .collect_vec();
            let (row, column) = points
                .iter()
                .enumerate()
                .filter_map(|(index, point)| plane(point).map(|projected| (index, projected.norm_squared())))
                .min_by(|(_, nearer), (_, farther)| nearer.total_cmp(farther))
                .map(|(index, _)| (index / width, index % width))
                .ok_or(Malformed::Outside)?;
            let corner = |row: usize, column: usize| points.get(row.strict_mul(ni).strict_add(column)).and_then(plane);
            let cell = |(south, west): (usize, usize)| -> Result<Option<_>, Malformed> {
                let (north, east) = (south.strict_add(1), west.strict_add(1));
                let (Some(origin), Some(onward), Some(upward), Some(far)) = (corner(south, west), corner(south, east), corner(north, west), corner(north, east)) else {
                    return Ok(None);
                };
                let (step, rise, twist, site) = (onward - origin, upward - origin, origin - onward - upward + far, -origin);
                Ok(roots::real(&[site.perp(&step), step.perp(&rise) + site.perp(&twist), twist.perp(&rise)], RangeInclusive { start: 0.0, last: 1.0 })
                    .map_err(Malformed::Analysis)?
                    .into_iter()
                    .find_map(|upward_share| {
                        let axis = step + twist * upward_share;
                        let onward_share = (site - rise * upward_share).dot(&axis) / axis.norm_squared();
                        (0.0..=1.0).contains(&onward_share).then(|| {
                            let rotation = (flags.0 & GRID_RELATIVE != 0).then(|| Rotation2::rotation_between(&Vector2::x(), &(axis * sign)));
                            ([(south, west), (south, east), (north, west), (north, east)], [[0.0, 1.0], [0.0, 1.0]], (upward_share, onward_share), rotation)
                        })
                    }))
            };
            [row.checked_sub(1), Some(row)]
                .into_iter()
                .flatten()
                .filter(|&south| south.strict_add(1) < nj)
                .cartesian_product([column.checked_sub(1), Some(column)].into_iter().flatten().filter(|&west| west.strict_add(1) < ni))
                .map(cell)
                .find_map(Result::transpose)
                .transpose()?
                .ok_or(Malformed::Outside)?
        };
        let decoder = Grib2SubmessageDecoder::from(message)?;
        let [south_west, south_east, north_west, north_east] = <[f64; 4]>::try_from(
            indices
                .zip(decoder.dispatch()?)
                .filter_map(|((column, row), value)| corners.iter().position(|&corner| corner == (row, column)).map(|corner| (corner, f64::from(value))))
                .sorted_unstable_by_key(|&(corner, _)| corner)
                .map(|(_, value)| value)
                .collect_vec(),
        )
        .map_err(|Vec { .. }| Malformed::Grid)?;
        let lattice = arr2(&[[south_west, south_east], [north_west, north_east]]);
        let interpolated = |samples: Array2<f64>| -> Result<f64, Malformed> {
            Interp2D::builder(samples)
                .x(Array1::from_vec(row_axis.to_vec()))
                .y(Array1::from_vec(column_axis.to_vec()))
                .build()
                .map_err(|error| match error {
                    BuilderError::NotEnoughData(_) | BuilderError::Monotonic(_) | BuilderError::ShapeError(_) | BuilderError::ValueError(_) => Malformed::Grid,
                })?
                .interp_scalar(row_at, column_at)
                .map_err(|InterpolateError::OutOfBounds(_)| Malformed::Outside)
        };
        let value = match field {
            Field::WindDirection => {
                let [eastward, northward] = [f64::sin, f64::cos].map(|component| interpolated(lattice.mapv(|degrees| component(degrees.to_radians()))));
                eastward.and_then(|eastward| northward.map(|northward| eastward.atan2(northward).to_degrees().rem_euclid(full)))?
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
            | Field::Precipitation => interpolated(lattice)?,
        };
        Ok(Some(Reading { field, interval, value: value.is_finite().then_some(value * weight), rotation }))
    };
    let (instants, totals): (Vec<_>, Vec<_>) = file
        .submessages()
        .enumerate()
        .map(|(index, (_, message))| sample(message).map_err(|malformed| (Record::Message(index), malformed)))
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(|records| WeatherError::Records { records })?
        .into_iter()
        .flatten()
        .partition(|reading| reading.interval.is_empty());
    let accumulations = totals.into_iter().sorted_by_key(|reading| (reading.field, reading.interval.start, reading.interval.end)).collect_vec();
    let steps = accumulations
        .chunk_by(|earlier, later| (earlier.field, earlier.interval.start) == (later.field, later.interval.start))
        .flat_map(|accumulation| {
            let origin = accumulation.first().map(|&first| Reading { interval: Range { start: first.interval.start, end: first.interval.start }, value: Some(0.0), ..first });
            iter::chain(origin, accumulation.iter().copied()).dedup_by(|earlier, later| earlier.interval.end == later.interval.end).tuple_windows().map(|(previous, reading)| Reading {
                interval: Range { start: previous.interval.end, ..reading.interval },
                value: previous.value.zip(reading.value).map(|(before, after)| after - before),
                ..reading
            })
        })
        .into_group_map_by(|step| step.field)
        .into_iter()
        .map(|(field, run)| {
            let disjoint = run
                .into_iter()
                .sorted_by_key(|step| (step.interval.start, step.interval.end))
                .coalesce(|kept, next| (next.interval.start < kept.interval.end).ok_or((kept, next)).map(|()| kept))
                .collect_vec();
            (field, disjoint)
        })
        .collect::<HashMap<_, _>>();
    let ends = iter::chain(instants.iter(), steps.values().flatten()).map(|reading| reading.interval.end).sorted_unstable().dedup().collect_vec();
    let rotation = iter::chain(instants.iter(), steps.values().flatten()).find(|reading| reading.field == Field::Eastward).and_then(|reading| reading.rotation);
    let instants = instants.into_iter().into_group_map_by(|reading| reading.interval.end);
    let back = ends.first().zip(ends.get(1)).map(|(&first, &next)| first.checked_sub(next.duration_since(first))).transpose()?;
    let lead = ends.first().map(|&first| steps.values().flatten().map(|step| step.interval.start).filter(|&start| start < first).min().or(back).unwrap_or(first));
    let hour = |interval: Range<Timestamp>| {
        let length = interval.end.duration_since(interval.start);
        let integral = |field: Field| {
            steps.get(&field).and_then(|run| {
                run.iter()
                    .skip(run.partition_point(|step| step.interval.end <= interval.start))
                    .take_while(|step| step.interval.start < interval.end)
                    .try_fold((SignedDuration::ZERO, 0.0), |(covered, total), step| {
                        let overlap = step.interval.end.min(interval.end).duration_since(step.interval.start.max(interval.start));
                        let share = overlap.as_secs_f64() / step.interval.end.duration_since(step.interval.start).as_secs_f64();
                        step.value.zip(covered.checked_add(overlap)).map(|(value, covered)| (covered, value.mul_add(share, total)))
                    })
                    .and_then(|(covered, total)| (covered.is_positive() && covered == length).then_some(total))
            })
        };
        let value = |field: Field| {
            integral(field)
                .map(|total| total / length.as_secs_f64())
                .or_else(|| instants.get(&interval.end).and_then(|readings| readings.iter().find(|reading| reading.field == field)).and_then(|reading| reading.value))
        };
        let temperature = |quantity: ThermodynamicTemperature| Temperature::from_kelvin(quantity.get::<kelvin>());
        let [dry_bulb, dew_point] = [Field::DryBulb, Field::DewPoint].map(|field| value(field).map(ThermodynamicTemperature::new::<kelvin>));
        let relative_humidity = value(Field::RelativeHumidity)
            .map(Ratio::new::<percent>)
            .or_else(|| dry_bulb.zip(dew_point).map(|(air, dew)| Ratio::new::<ratio>(p_sat(temperature(dew)).as_pascals() / p_sat(temperature(air)).as_pascals())))
            .map(|humidity| humidity.min(Ratio::new::<ratio>(1.0)));
        let [infrared, global_irradiance, direct_irradiance] =
            [Field::Infrared, Field::GlobalIrradiance, Field::DirectHorizontal].map(|field| value(field).map(|flux| HeatFluxDensity::new::<watt_per_square_meter>(flux.max(0.0))));
        let wind = value(Field::Eastward).zip(value(Field::Northward)).map(|(eastward, northward)| -(rotation.unwrap_or_else(Rotation2::identity) * Vector2::new(eastward, northward)));
        let precipitation = integral(Field::Precipitation).map(|depth| Length::new::<millimeter>(depth.max(0.0)));
        Hour {
            interval,
            dry_bulb,
            dew_point: dew_point.or_else(|| {
                dry_bulb
                    .zip(relative_humidity)
                    .map(|(air, humidity)| dew_point_temperature(temperature(air), Humidity::from_percent(humidity.get::<percent>())).as_kelvin())
                    .filter(|kelvins| kelvins.is_finite())
                    .map(ThermodynamicTemperature::new::<kelvin>)
            }),
            relative_humidity,
            pressure: value(Field::Pressure).map(Pressure::new::<pascal>),
            infrared,
            global_irradiance,
            direct_irradiance,
            diffuse_irradiance: global_irradiance.zip(direct_irradiance).map(|(global, beam)| (global - beam).max(HeatFluxDensity::ZERO)),
            wind_direction: value(Field::WindDirection).map(Angle::new::<degree>).or_else(|| wind.map(|from| Angle::new::<radian>(from.x.atan2(from.y).rem_euclid(Angle::FULL_TURN.get::<radian>())))),
            wind_speed: value(Field::WindSpeed).or_else(|| wind.as_ref().map(Vector2::norm)).map(Velocity::new::<meter_per_second>),
            total_sky_cover: value(Field::TotalSkyCover).map(Ratio::new::<percent>),
            visibility: value(Field::Visibility).map(Length::new::<meter>),
            precipitable_water: value(Field::PrecipitableWater).map(|depth| Length::new::<millimeter>(depth.max(0.0))),
            snow_depth: value(Field::SnowDepth).map(|depth| Length::new::<meter>(depth.max(0.0))),
            albedo: value(Field::Albedo).map(Ratio::new::<percent>),
            precipitation,
            precipitation_period: precipitation.map(|_| Time::new::<second>(length.as_secs_f64())),
            ..
        }
    };
    let horizontal = Hours::try_new(sun, iter::chain(lead, ends.iter().copied()).tuple_windows().map(|(start, end)| hour(Range { start, end })).collect())?;
    let hours = horizontal
        .records
        .iter()
        .map(|record| {
            record
                .direct_irradiance
                .map(|beam| {
                    position(&horizontal, record).map(|(prepared, solar)| {
                        let altitude = solar.elevation_angle().to_radians();
                        if altitude > 0.0 { (beam / altitude.sin()).min(extraterrestrial(&prepared)) } else { HeatFluxDensity::ZERO }
                    })
                })
                .transpose()
                .map(|direct_irradiance| Hour { direct_irradiance, ..record.clone() })
        })
        .collect::<Result<Vec<_>, _>>()?;
    Ok(Hours { records: hours, ..horizontal })
}
