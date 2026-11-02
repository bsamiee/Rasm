use core::{iter, range::Range};
use std::io;

use grib::{
    Grib2SubmessageDecoder, LatLons as _, SeekableGrib2Reader, SubMessage,
    codetables::grib2::Table4_4,
    def::grib2::{
        GridDefinitionTemplate, ProductDefinitionTemplate,
        template::{
            Template3_0, Template3_40, Template4_0, Template4_1, Template4_8, Template4_11,
            param_set::{DateTime, GaussianGrid, LatLonGrid, ScaledValue, Statistical, TimeRange},
        },
    },
};
use itertools::Itertools as _;
use jiff::{Span, Timestamp, civil, tz::Offset};
use nalgebra::Vector2;
use ndarray::{Array1, Array2, Axis, ShapeError};
use ndarray_interp::{BuilderError, InterpolateError, interp2d::Interp2D};
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

use crate::{Hour, Hours, Malformed, Record, WeatherError, position};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, PartialOrd, Ord)]
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
    Albedo,
    Precipitation,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy)]
struct Reading {
    field: Field,
    start: Option<Timestamp>,
    end: Timestamp,
    value: Option<f64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn read<R>(reader: R, sun: Sun) -> Result<Hours, WeatherError>
where R: io::Read + io::Seek {
    const METEOROLOGY: u8 = 0;
    const GROUND: u8 = 1;
    const ATMOSPHERE: u8 = 10;
    const HEIGHT: u8 = 103;
    const PARAMETERS: [(u8, u8, u8, Option<i32>, Field); 16] = [
        (0, 0, HEIGHT, Some(2), Field::DryBulb),
        (0, 6, HEIGHT, Some(2), Field::DewPoint),
        (1, 1, HEIGHT, Some(2), Field::RelativeHumidity),
        (3, 0, GROUND, None, Field::Pressure),
        (5, 3, GROUND, None, Field::Infrared),
        (4, 7, GROUND, None, Field::GlobalIrradiance),
        (4, 13, GROUND, None, Field::DirectHorizontal),
        (2, 0, HEIGHT, Some(10), Field::WindDirection),
        (2, 1, HEIGHT, Some(10), Field::WindSpeed),
        (2, 2, HEIGHT, Some(10), Field::Eastward),
        (2, 3, HEIGHT, Some(10), Field::Northward),
        (6, 1, GROUND, None, Field::TotalSkyCover),
        (6, 1, ATMOSPHERE, None, Field::TotalSkyCover),
        (19, 0, GROUND, None, Field::Visibility),
        (19, 1, GROUND, None, Field::Albedo),
        (1, 8, GROUND, None, Field::Precipitation),
    ];
    const AVERAGE: u8 = 0;
    const ACCUMULATION: u8 = 1;
    let file = grib::from_reader(reader)?;
    let &location: &Location = sun.as_ref();
    let sample = |message: SubMessage<'_, SeekableGrib2Reader<R>>| -> Result<Option<Reading>, Malformed> {
        let span = |time: &TimeRange<i32>| -> Result<Span, Malformed> {
            let (unit, factor): (fn(Span, i64) -> Result<Span, jiff::Error>, i64) = match Table4_4::try_from(time.unit).ok() {
                Some(Table4_4::Second) => (Span::try_seconds::<i64>, 1),
                Some(Table4_4::Minute) => (Span::try_minutes::<i64>, 1),
                Some(Table4_4::Hour) => (Span::try_hours::<i64>, 1),
                Some(Table4_4::ThreeHours) => (Span::try_hours::<i64>, 3),
                Some(Table4_4::SixHours) => (Span::try_hours::<i64>, 6),
                Some(Table4_4::TwelveHours) => (Span::try_hours::<i64>, 12),
                Some(Table4_4::Day) => (Span::try_days::<i64>, 1),
                Some(Table4_4::Month) => (Span::try_months::<i64>, 1),
                Some(Table4_4::Year) => (Span::try_years::<i64>, 1),
                Some(Table4_4::Decade) => (Span::try_years::<i64>, 10),
                Some(Table4_4::Normal) => (Span::try_years::<i64>, 30),
                Some(Table4_4::Century) => (Span::try_years::<i64>, 100),
                Some(_) | None => return Err(Malformed::Undated),
            };
            unit(Span::new(), i64::from(time.len)).and_then(|span| span.checked_mul(factor)).map_err(Malformed::Time)
        };
        let instant = |time: &DateTime, after: Span| -> Result<Timestamp, Malformed> {
            let (Ok(year), Ok(month), Ok(day), Ok(hour), Ok(minute), Ok(second)) =
                (i16::try_from(time.year), i8::try_from(time.month), i8::try_from(time.day), i8::try_from(time.hour), i8::try_from(time.minute), i8::try_from(time.second))
            else {
                return Err(Malformed::Undated);
            };
            civil::DateTime::new(year, month, day, hour, minute, second, 0)
                .and_then(|reference| reference.checked_add(after))
                .and_then(|valid| Offset::UTC.to_timestamp(valid))
                .map_err(Malformed::Time)
        };
        let (param, forecast_time, horizontal, statistical) = match message.section4()?.payload.template {
            ProductDefinitionTemplate::_4_0(Template4_0 { param, generating_process: _, forecast_time, horizontal })
            | ProductDefinitionTemplate::_4_1(Template4_1 { param, generating_process: _, forecast_time, horizontal, emsemble_forecast: _ }) => (param, forecast_time, horizontal, None),
            ProductDefinitionTemplate::_4_8(Template4_8 { param, generating_process: _, forecast_time, horizontal, time_interval })
            | ProductDefinitionTemplate::_4_11(Template4_11 { param, generating_process: _, forecast_time, horizontal, emsemble_forecast: _, time_interval }) =>
                (param, forecast_time, horizontal, Some(time_interval)),
            _ => return Ok(None),
        };
        let surface = horizontal.first_surface;
        let level = |metres: i32| {
            let ScaledValue { scale_factor, scaled_value } = surface.value;
            let [up, down] = [scale_factor, scale_factor.saturating_neg()].map(|exponent| 10_i64.checked_pow(u32::from(exponent.max(0).unsigned_abs())));
            up.zip(down).and_then(|(up, down)| i64::from(metres).checked_mul(up).zip(i64::from(scaled_value).checked_mul(down))).is_some_and(|(stated, decoded)| stated == decoded)
        };
        let discipline = message.indicator().discipline;
        let Some(field) = PARAMETERS.into_iter().find_map(|(category, number, surface_type, height, field)| {
            ((METEOROLOGY, category, number, surface_type) == (discipline, param.category, param.num, surface.surface_type) && height.is_none_or(level)).then_some(field)
        }) else {
            return Ok(None);
        };
        let valid = span(&forecast_time.time).and_then(|offset| instant(&message.section1()?.payload.ref_time, offset))?;
        let interval = statistical
            .map(|Statistical { end_of_overall_time_interval, num_time_ranges: _, num_missing: _, time_ranges }| {
                instant(&end_of_overall_time_interval, Span::new()).map(|end| (end, time_ranges.first().map(|process| process.statistical_process_type)))
            })
            .transpose()?;
        let (start, end, weight) = match interval {
            None => (None, valid, 1.0),
            Some((end, Some(AVERAGE))) => (Some(valid), end, end.duration_since(valid).as_secs_f64()),
            Some((end, Some(ACCUMULATION))) => (Some(valid), end, 1.0),
            Some((_, Some(_))) => return Ok(None),
            Some((_, None)) => return Err(Malformed::Undated),
        };
        let scanning = match message.section3()?.payload.template {
            GridDefinitionTemplate::_3_0(Template3_0 { earth: _, lat_lon: LatLonGrid { grid: _, i_direction_inc: _, j_direction_inc: _, scanning_mode } })
            | GridDefinitionTemplate::_3_40(Template3_40 { earth: _, gaussian: GaussianGrid { grid: _, i_direction_inc: _, n: _, scanning_mode } }) => scanning_mode,
            _ => return Err(Malformed::Grid),
        };
        let (ni, nj) = message.grid_shape()?;
        let coordinates = message.ij().and_then(|ij| message.latlons().map(|latlons| ij.zip(latlons)))?;
        let Some((_, (_, origin))) = coordinates.clone().next() else {
            return Err(Malformed::Grid);
        };
        let turn = Angle::FULL_TURN.get::<degree>();
        let sign = if scanning.scans_positively_for_i() { 1.0 } else { -1.0 };
        let (row_order, latitudes): (Vec<_>, Vec<_>) =
            coordinates.clone().filter_map(|((column, row), (latitude, _))| (column == 0).then_some((row, latitude))).sorted_by(|south, north| south.1.total_cmp(&north.1)).unzip();
        let columns = coordinates
            .clone()
            .filter_map(|((column, row), (_, longitude))| (row == 0).then(|| (column, sign.mul_add((sign * (longitude - origin)).rem_euclid(turn), origin))))
            .sorted_by(|west, east| west.1.total_cmp(&east.1))
            .dedup_by(|west, east| west.1.total_cmp(&east.1).is_eq())
            .collect_vec();
        let closing = columns.first().zip(columns.get(1)).zip(columns.last()).and_then(|((&(index, first), &(_, second)), &(_, last))| {
            let step = second - first;
            ((first + turn - last - step).abs() < step / 2.0).then_some((index, first + turn))
        });
        let longitude = columns.first().map_or(location.longitude, |&(_, first)| first + (location.longitude - first).rem_euclid(turn));
        let (column_order, longitudes): (Vec<_>, Vec<_>) = iter::chain(columns, closing).unzip();
        let decoder = Grib2SubmessageDecoder::from(message)?;
        let values = coordinates.zip(decoder.dispatch()?).sorted_unstable_by_key(|&(((column, row), _), _)| (row, column)).map(|(_, value)| f64::from(value)).collect_vec();
        let lattice = Array2::from_shape_vec((nj, ni), values).map_err(|ShapeError { .. }| Malformed::Grid)?.select(Axis(0), &row_order).select(Axis(1), &column_order);
        let value = Interp2D::builder(lattice)
            .x(Array1::from_vec(latitudes))
            .y(Array1::from_vec(longitudes))
            .build()
            .map_err(|error| match error {
                BuilderError::NotEnoughData(_) | BuilderError::Monotonic(_) | BuilderError::ShapeError(_) | BuilderError::ValueError(_) => Malformed::Grid,
            })?
            .interp_scalar(location.latitude, longitude)
            .map_err(|InterpolateError::OutOfBounds(_)| Malformed::Outside)?;
        Ok(Some(Reading { field, start, end, value: value.is_finite().then_some(value * weight) }))
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
        .partition(|reading| reading.start.is_none());
    let accumulations = totals.into_iter().sorted_by_key(|reading| (reading.field, reading.start, reading.end)).collect_vec();
    let steps = accumulations.chunk_by(|earlier, later| (earlier.field, earlier.start) == (later.field, later.start)).flat_map(|accumulation| {
        let origin = accumulation.first().and_then(|&first| first.start.map(|start| Reading { start: None, end: start, value: Some(0.0), ..first }));
        iter::chain(origin, accumulation.iter().copied().dedup_by(|earlier, later| earlier.end == later.end)).tuple_windows().map(|(previous, reading)| Reading {
            start: Some(previous.end),
            value: previous.value.zip(reading.value).map(|(before, after)| after - before),
            ..reading
        })
    });
    let ordered = iter::chain(instants, steps).sorted_by_key(|reading| reading.end).collect_vec();
    let records = ordered.chunk_by(|earlier, later| earlier.end == later.end).filter_map(|readings| readings.first().map(|reading| (reading.end, readings))).collect_vec();
    let back = records.first().zip(records.get(1)).map(|(&(end, _), &(next, _))| end.checked_sub(next.duration_since(end))).transpose()?;
    let lead = records.first().map(|&(end, readings)| readings.iter().filter_map(|reading| reading.start).min().or(back).unwrap_or(end));
    let site = Hours { sun, hours: Vec::new() };
    let hour = |interval: Range<Timestamp>, readings: &[Reading]| -> Result<Hour, WeatherError> {
        let read = |field: Field| readings.iter().find(|reading| reading.field == field);
        let value = |field: Field| read(field).and_then(|reading| reading.value.map(|value| reading.start.map_or(value, |start| value / reading.end.duration_since(start).as_secs_f64())));
        let temperature = |quantity: ThermodynamicTemperature| Temperature::from_kelvin(quantity.get::<kelvin>());
        let [dry_bulb, dew_point] = [Field::DryBulb, Field::DewPoint].map(|field| value(field).map(ThermodynamicTemperature::new::<kelvin>));
        let relative_humidity = value(Field::RelativeHumidity).map(Ratio::new::<percent>);
        let [infrared, global_irradiance, direct] = [Field::Infrared, Field::GlobalIrradiance, Field::DirectHorizontal].map(|field| value(field).map(HeatFluxDensity::new::<watt_per_square_meter>));
        let wind = value(Field::Eastward).zip(value(Field::Northward)).map(|(eastward, northward)| -Vector2::new(eastward, northward));
        let precipitation = read(Field::Precipitation);
        let stated = Hour {
            interval,
            dry_bulb,
            dew_point: dew_point.or_else(|| {
                dry_bulb
                    .zip(relative_humidity)
                    .map(|(air, humidity)| dew_point_temperature(temperature(air), Humidity::from_percent(humidity.get::<percent>())).as_kelvin())
                    .filter(|kelvins| kelvins.is_finite())
                    .map(ThermodynamicTemperature::new::<kelvin>)
            }),
            relative_humidity: relative_humidity.or_else(|| dry_bulb.zip(dew_point).map(|(air, dew)| Ratio::new::<ratio>(p_sat(temperature(dew)).as_pascals() / p_sat(temperature(air)).as_pascals()))),
            pressure: value(Field::Pressure).map(Pressure::new::<pascal>),
            infrared,
            global_irradiance,
            wind_direction: value(Field::WindDirection).map(Angle::new::<degree>).or_else(|| wind.map(|from| Angle::new::<radian>(from.x.atan2(from.y).rem_euclid(Angle::FULL_TURN.get::<radian>())))),
            wind_speed: value(Field::WindSpeed).or_else(|| wind.as_ref().map(Vector2::norm)).map(Velocity::new::<meter_per_second>),
            total_sky_cover: value(Field::TotalSkyCover).map(Ratio::new::<percent>),
            visibility: value(Field::Visibility).map(Length::new::<meter>),
            albedo: value(Field::Albedo).map(Ratio::new::<percent>),
            precipitation: precipitation.and_then(|reading| reading.value).map(Length::new::<millimeter>),
            precipitation_period: precipitation.and_then(|reading| reading.start.map(|start| Time::new::<second>(reading.end.duration_since(start).as_secs_f64()))),
            ..
        };
        let direct_irradiance = direct
            .map(|beam| {
                position(&site, &stated).map(|(_, solar)| {
                    let altitude = Angle::new::<degree>(solar.elevation_angle());
                    if altitude > Angle::ZERO { beam / altitude.sin().get::<ratio>() } else { HeatFluxDensity::ZERO }
                })
            })
            .transpose()?;
        Ok(Hour { direct_irradiance, diffuse_irradiance: global_irradiance.zip(direct).map(|(global, beam)| (global - beam).max(HeatFluxDensity::ZERO)), ..stated })
    };
    let hours = iter::zip(iter::chain(lead, records.iter().map(|&(end, _)| end)), &records).map(|(start, &(end, readings))| hour(Range { start, end }, readings)).collect::<Result<Vec<_>, _>>()?;
    Hours::try_new(site.sun, hours)
}
