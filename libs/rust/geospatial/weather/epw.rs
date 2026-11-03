use core::{
    fmt, iter,
    num::NonZero,
    range::{Range, RangeInclusive},
};
use std::io;

use csv::{ReaderBuilder, StringRecord, WriterBuilder};
use itertools::{Either, Itertools as _};
use jiff::{
    SignedDuration, Zoned, civil,
    fmt::strtime::{self, BrokenDownTime},
    tz::{Offset, TimeZone},
};
use rasm_site::{Elevation, Place, Position};
use rasm_solar::{Refraction, Sun, events};
use serde::{
    Deserialize, Serialize,
    de::{DeserializeOwned, IgnoredAny},
};
use uom::si::{
    angle::degree,
    f64::{Angle, HeatFluxDensity, Illuminance, Length, Luminance, MassDensity, Pressure, Ratio, SpecificHeatCapacity, ThermalConductivity, ThermodynamicTemperature, Time, Velocity},
    heat_flux_density::watt_per_square_meter,
    illuminance::lux,
    length::{centimeter, kilometer, meter, millimeter},
    luminance::candela_per_square_meter,
    mass_density::kilogram_per_cubic_meter,
    pressure::pascal,
    ratio::{percent, ratio},
    specific_heat_capacity::joule_per_kilogram_kelvin,
    thermal_conductivity::watt_per_meter_kelvin,
    thermodynamic_temperature::degree_celsius,
    time,
    velocity::meter_per_second,
};
use validated::Validated;

use crate::{
    Ceiling, Hour, Hours, Malformed, Record, Refusal, WeatherError,
    design::{self, Design},
    extraterrestrial, refused, written,
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize, derive_more::Display)]
#[display("{}", fmt::from_fn(|formatter| self.serialize(formatter)))]
pub enum Keyword {
    #[serde(rename = "LOCATION")]
    Location,
    #[serde(rename = "DESIGN CONDITIONS")]
    DesignConditions,
    #[serde(rename = "TYPICAL/EXTREME PERIODS")]
    Periods,
    #[serde(rename = "GROUND TEMPERATURES")]
    GroundTemperatures,
    #[serde(rename = "HOLIDAYS/DAYLIGHT SAVINGS", alias = "HOLIDAYS/DAYLIGHT SAVING")]
    Holidays,
    #[serde(rename = "COMMENTS 1")]
    FirstComment,
    #[serde(rename = "COMMENTS 2")]
    SecondComment,
    #[serde(rename = "DATA PERIODS")]
    DataPeriods,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum Kind {
    Typical,
    Extreme,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Deserialize)]
#[serde(try_from = "String")]
pub enum Day {
    Date { month: i8, day: i8 },
    Ordinal(i16),
    Weekday { nth: i8, weekday: civil::Weekday, month: i8 },
}

impl Day {
    pub fn date(self, year: i16) -> Result<civil::Date, WeatherError> { self.resolved(year).map_err(WeatherError::Time) }

    fn resolved(self, year: i16) -> Result<civil::Date, jiff::Error> {
        match self {
            Self::Date { month, day } => civil::Date::new(year, month, day),
            Self::Ordinal(day) => civil::Date::new(year, 1, 1)?.with().day_of_year_no_leap(day).build(),
            Self::Weekday { nth, weekday, month } => civil::Date::new(year, month, 1)?.nth_weekday_of_month(nth, weekday),
        }
    }

    fn checked(self) -> Result<Self, jiff::Error> {
        const LEAP_YEAR: i16 = 2000;
        match self {
            Self::Date { month: _, day: _ } | Self::Ordinal(_) => self.resolved(LEAP_YEAR).map(|_| self),
            Self::Weekday { nth: -1 | 1..=5, weekday: _, month } => civil::Date::new(LEAP_YEAR, month, 1).map(|_| self),
            Self::Weekday { nth, weekday: _, month: _ } => Err(jiff::Error::from_args(format_args!("{nth} ranks no weekday of a month, which takes 1 to 5 or last"))),
        }
    }
}

impl TryFrom<String> for Day {
    type Error = jiff::Error;

    fn try_from(value: String) -> Result<Self, jiff::Error> {
        let spaced = value.replace(['/', ':', '-'], " ");
        let tokens = spaced.split_whitespace().filter(|token| !(token.eq_ignore_ascii_case("in") || token.eq_ignore_ascii_case("of"))).collect_vec();
        let month_named = |token: &str| named("%b", token, BrokenDownTime::month);
        let weekday_named = |token: &str| named("%a", token, BrokenDownTime::weekday);
        let unnamed = || jiff::Error::from_args(format_args!("{value} is no EPW day"));
        match *tokens {
            [ordinal] => ordinal.parse().map(Self::Ordinal).map_err(|source| jiff::Error::from_args(format_args!("{ordinal} in {value}: {source}"))),
            [first, second] => match (first.parse().ok(), second.parse().ok()) {
                (Some(month), Some(day)) => Ok(Self::Date { month, day }),
                (Some(day), None) => month_named(second).map(|month| Self::Date { month, day }),
                (None, Some(day)) => month_named(first).map(|month| Self::Date { month, day }),
                (None, None) => Err(unnamed()),
            },
            [rank, first, second] => {
                let nth = if rank.eq_ignore_ascii_case("last") { Some(-1) } else { rank.trim_end_matches(char::is_alphabetic).parse().ok() }.ok_or_else(unnamed)?;
                let (weekday, month) = weekday_named(first).map(|weekday| (weekday, second)).or_else(|_| weekday_named(second).map(|weekday| (weekday, first)))?;
                month_named(month).map(|named_month| Self::Weekday { nth, weekday, month: named_month })
            }
            _ => Err(unnamed()),
        }
        .and_then(Self::checked)
    }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Epw {
    pub station: Station,
    pub design: Option<Design>,
    pub periods: Vec<Period>,
    pub ground: Vec<Ground>,
    pub daylight_saving: Option<RangeInclusive<Day>>,
    pub holidays: Vec<(String, Day)>,
    pub comments: [String; 2],
    pub weekday: civil::Weekday,
    pub hours: Hours,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct Station {
    pub city: String,
    pub region: String,
    pub country: String,
    pub source: String,
    pub wmo: String,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Period {
    pub name: String,
    pub kind: Kind,
    pub days: RangeInclusive<Day>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Ground {
    pub depth: Length,
    pub conductivity: Option<ThermalConductivity>,
    pub density: Option<MassDensity>,
    pub specific_heat: Option<SpecificHeatCapacity>,
    pub monthly: [ThermodynamicTemperature; 12],
}

#[derive(Default, Serialize, Deserialize)]
#[serde(default)]
struct Row {
    year: i16,
    month: i8,
    day: i8,
    hour: i8,
    minute: i8,
    flags: String,
    dry_bulb: Option<f64>,
    dew_point: Option<f64>,
    relative_humidity: Option<f64>,
    pressure: Option<f64>,
    extraterrestrial_horizontal: Option<f64>,
    extraterrestrial_normal: Option<f64>,
    infrared: Option<f64>,
    global_irradiance: Option<f64>,
    direct_irradiance: Option<f64>,
    diffuse_irradiance: Option<f64>,
    global_illuminance: Option<f64>,
    direct_illuminance: Option<f64>,
    diffuse_illuminance: Option<f64>,
    zenith_luminance: Option<f64>,
    wind_direction: Option<f64>,
    wind_speed: Option<f64>,
    total_sky_cover: Option<f64>,
    opaque_sky_cover: Option<f64>,
    visibility: Option<f64>,
    ceiling: Option<f64>,
    observation: Option<i8>,
    present_weather: String,
    precipitable_water: Option<f64>,
    aerosol_optical_depth: Option<f64>,
    snow_depth: Option<f64>,
    since_snowfall: Option<f64>,
    albedo: Option<f64>,
    precipitation: Option<f64>,
    precipitation_period: Option<f64>,
}

struct Column<Q> {
    missing: f64,
    quantity: fn(f64) -> Q,
    value: fn(Q) -> f64,
}

// --- [TABLES] --------------------------------------------------------------------------

const CEILINGS: [(f64, Option<Ceiling>); 3] = [(99_999.0, None), (88_888.0, Some(Ceiling::Cirroform)), (77_777.0, Some(Ceiling::Unlimited))];
const TEMPERATURE: Column<ThermodynamicTemperature> = Column { missing: 99.9, quantity: ThermodynamicTemperature::new::<degree_celsius>, value: |temperature| temperature.get::<degree_celsius>() };
const HUMIDITY: Column<Ratio> = Column { missing: 999.0, quantity: Ratio::new::<percent>, value: |humidity| humidity.get::<percent>() };
const PRESSURE: Column<Pressure> = Column { missing: 999_999.0, quantity: Pressure::new::<pascal>, value: |pressure| pressure.get::<pascal>() };
const IRRADIANCE: Column<HeatFluxDensity> = Column { missing: 9999.0, quantity: HeatFluxDensity::new::<watt_per_square_meter>, value: |irradiance| irradiance.get::<watt_per_square_meter>() };
const ILLUMINANCE: Column<Illuminance> = Column { missing: 999_900.0, quantity: Illuminance::new::<lux>, value: |illuminance| illuminance.get::<lux>() };
const LUMINANCE: Column<Luminance> = Column { missing: 9999.0, quantity: Luminance::new::<candela_per_square_meter>, value: |luminance| luminance.get::<candela_per_square_meter>() };
const DIRECTION: Column<Angle> = Column { missing: 999.0, quantity: Angle::new::<degree>, value: |direction| direction.get::<degree>() };
const SPEED: Column<Velocity> = Column { missing: 999.0, quantity: Velocity::new::<meter_per_second>, value: |speed| speed.get::<meter_per_second>() };
const COVER: Column<Ratio> = Column { missing: 99.0, quantity: |tenths| Ratio::new::<ratio>(tenths / 10.0), value: |cover| cover.get::<ratio>() * 10.0 };
const VISIBILITY: Column<Length> = Column { missing: 9999.0, quantity: Length::new::<kilometer>, value: |visibility| visibility.get::<kilometer>() };
const WATER: Column<Length> = Column { missing: 999.0, quantity: Length::new::<millimeter>, value: |water| water.get::<millimeter>() };
const AEROSOL: Column<Ratio> = Column { missing: 0.999, quantity: Ratio::new::<ratio>, value: |depth| depth.get::<ratio>() };
const SNOW: Column<Length> = Column { missing: 999.0, quantity: Length::new::<centimeter>, value: |snow| snow.get::<centimeter>() };
const DAYS: Column<Time> = Column { missing: 99.0, quantity: Time::new::<time::day>, value: |elapsed| elapsed.get::<time::day>() };
const ALBEDO: Column<Ratio> = Column { missing: 999.0, quantity: Ratio::new::<ratio>, value: |reflected| reflected.get::<ratio>() };
const ACCUMULATION: Column<Time> = Column { missing: 99.0, quantity: Time::new::<time::hour>, value: |period| period.get::<time::hour>() };

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn read<R>(mut reader: R) -> Result<Epw, WeatherError>
where R: io::Read {
    fn header<T, F>(headers: &[(Keyword, &StringRecord, StringRecord)], keyword: Keyword, decode: F) -> Validated<T, (Record, Malformed)>
    where F: FnOnce(&StringRecord, &StringRecord) -> Result<T, Malformed> {
        Validated::from(
            headers
                .iter()
                .find(|(stated, _, _)| *stated == keyword)
                .ok_or(Malformed::Absent)
                .and_then(|(_, line, record)| decode(line, record))
                .map_err(|malformed| (Record::Header(keyword), malformed)),
        )
    }

    fn repeated<T>(record: &StringRecord, preceding: usize) -> Result<impl Iterator<Item = T>, csv::Error>
    where T: DeserializeOwned {
        record.iter().skip(preceding).collect::<StringRecord>().deserialize::<Vec<Option<T>>>(None).map(|stated| stated.into_iter().flatten())
    }

    fn present<Q>(column: &Column<Q>, value: Option<f64>) -> Option<Q> { value.filter(|stated| *stated < column.missing).map(column.quantity) }

    let hour = |row: Row, offset: Offset, per_hour: NonZero<u8>| -> Result<Hour, Malformed> {
        let step = SignedDuration::from_hours(1).div_f64(f64::from(per_hour.get()));
        let elapsed = if per_hour > NonZero::<u8>::MIN { SignedDuration::from_mins(i64::from(row.minute)) } else { SignedDuration::from_hours(1) };
        let end = offset.to_timestamp(civil::DateTime::new(row.year, row.month, row.day, row.hour.saturating_sub(1), 0, 0, 0)?.checked_add(elapsed)?)?;
        Ok(Hour {
            interval: Range { start: end.checked_sub(step)?, end },
            dry_bulb: present(&TEMPERATURE, row.dry_bulb),
            dew_point: present(&TEMPERATURE, row.dew_point),
            relative_humidity: present(&HUMIDITY, row.relative_humidity),
            pressure: present(&PRESSURE, row.pressure),
            infrared: present(&IRRADIANCE, row.infrared),
            global_irradiance: present(&IRRADIANCE, row.global_irradiance),
            direct_irradiance: present(&IRRADIANCE, row.direct_irradiance),
            diffuse_irradiance: present(&IRRADIANCE, row.diffuse_irradiance),
            global_illuminance: present(&ILLUMINANCE, row.global_illuminance),
            direct_illuminance: present(&ILLUMINANCE, row.direct_illuminance),
            diffuse_illuminance: present(&ILLUMINANCE, row.diffuse_illuminance),
            zenith_luminance: present(&LUMINANCE, row.zenith_luminance),
            wind_direction: present(&DIRECTION, row.wind_direction),
            wind_speed: present(&SPEED, row.wind_speed),
            total_sky_cover: present(&COVER, row.total_sky_cover),
            opaque_sky_cover: present(&COVER, row.opaque_sky_cover),
            visibility: present(&VISIBILITY, row.visibility),
            ceiling: row.ceiling.and_then(|code| CEILINGS.iter().find(|&&(threshold, _)| code >= threshold).map_or_else(|| Some(Ceiling::Height(Length::new::<meter>(code))), |&(_, ceiling)| ceiling)),
            present_weather: row
                .present_weather
                .trim_matches(|stated: char| stated.is_whitespace() || matches!(stated, '\'' | '"'))
                .as_bytes()
                .as_array::<9>()
                .filter(|_| row.observation == Some(0))
                .map(|codes| codes.map(|code| if code.is_ascii_digit() { code.strict_sub(b'0') } else { 9 })),
            precipitable_water: present(&WATER, row.precipitable_water),
            aerosol_optical_depth: present(&AEROSOL, row.aerosol_optical_depth),
            snow_depth: present(&SNOW, row.snow_depth),
            since_snowfall: present(&DAYS, row.since_snowfall),
            albedo: present(&ALBEDO, row.albedo),
            precipitation: present(&WATER, row.precipitation),
            precipitation_period: present(&ACCUMULATION, row.precipitation_period),
        })
    };
    let mut bytes = Vec::new();
    reader.read_to_end(&mut bytes).map_err(csv::Error::from)?;
    let lines = ReaderBuilder::new()
        .has_headers(false)
        .flexible(true)
        .from_reader(bytes.trim_prefix(b"\xEF\xBB\xBF"))
        .byte_records()
        .map_ok(|line| {
            let position = line.position().cloned();
            let mut text = StringRecord::from_byte_record_lossy(line);
            text.set_position(position);
            text
        })
        .collect::<Result<Vec<_>, _>>()?;
    let (headers, rows): (Vec<_>, Vec<_>) = lines.iter().partition_map(|line| {
        let mut record = line.clone();
        record.trim();
        match record.deserialize::<Keyword>(None) {
            Ok(stated) => Either::Left((stated, line, record)),
            Err(csv::Error { .. }) => Either::Right(record),
        }
    });
    let location = header(&headers, Keyword::Location, |_, record| {
        let (_, station, position, zone, elevation) = record.deserialize::<(IgnoredAny, Station, Position, f64, Elevation)>(None)?;
        Ok((station, position, Offset::try_from(SignedDuration::try_from_secs_f64(Time::new::<time::hour>(zone).get::<time::second>())?)?, elevation))
    });
    let data = header(&headers, Keyword::DataPeriods, |_, record| {
        let (_, _, per_hour, _, weekday) = record.deserialize::<(IgnoredAny, IgnoredAny, NonZero<u8>, IgnoredAny, &str)>(None)?;
        Ok((per_hour, named("%a", weekday, BrokenDownTime::weekday)?))
    });
    let stamp = location.iter().next().zip(data.iter().next()).map(|(&(_, _, offset, _), &(per_hour, _))| (offset, per_hour));
    let design = header(&headers, Keyword::DesignConditions, |_, record| record.deserialize::<(IgnoredAny, Vec<String>)>(None).and_then(|(_, stated)| design::decode(&stated)).map_err(Malformed::Csv));
    let periods = header(&headers, Keyword::Periods, |_, record| {
        Ok(repeated::<(String, Kind, Day, Day)>(record, 2)?.map(|(name, kind, start, last)| Period { name, kind, days: RangeInclusive { start, last } }).collect_vec())
    });
    let ground = header(&headers, Keyword::GroundTemperatures, |_, record| {
        Ok(repeated::<(f64, Option<f64>, Option<f64>, Option<f64>, [f64; 12])>(record, 2)?
            .map(|(depth, conductivity, density, specific_heat, monthly)| Ground {
                depth: Length::new::<meter>(depth),
                conductivity: conductivity.map(ThermalConductivity::new::<watt_per_meter_kelvin>),
                density: density.map(MassDensity::new::<kilogram_per_cubic_meter>),
                specific_heat: specific_heat.map(SpecificHeatCapacity::new::<joule_per_kilogram_kelvin>),
                monthly: monthly.map(TEMPERATURE.quantity),
            })
            .collect_vec())
    });
    let calendar = header(&headers, Keyword::Holidays, |_, record| {
        let (_, _, start, last) = record.deserialize::<(IgnoredAny, IgnoredAny, String, String)>(None)?;
        let saving =
            (!start.trim_start_matches('0').is_empty()).then(|| Day::try_from(start).and_then(|first| Day::try_from(last).map(|end| RangeInclusive { start: first, last: end }))).transpose()?;
        Ok((saving, repeated::<(String, Day)>(record, 5)?.collect_vec()))
    });
    let comment = |stated| header(&headers, stated, |line, _| Ok(line.iter().skip(1).join(",").trim_matches([',', ' ']).to_owned()));
    let comments = comment(Keyword::FirstComment).map2(comment(Keyword::SecondComment), |first, second| [first, second]);
    let hours = rows
        .into_iter()
        .filter_map(|record| {
            let decoded = match record.deserialize::<Row>(None) {
                Ok(row) => stamp.map(|(offset, per_hour)| hour(row, offset, per_hour))?,
                Err(source) => Err(Malformed::Csv(source)),
            };
            Some(decoded.map_err(|malformed| (Record::Row(record.position().map_or_default(csv::Position::line)), malformed)))
        })
        .collect::<Validated<Vec<Hour>, _>>();
    let (((station, position, offset, elevation), design, periods, ground), (daylight_saving, holidays), comments, ((_, weekday), hours)) = location
        .map4(design, periods, ground, |location, design, periods, ground| (location, design, periods, ground))
        .map4(calendar, comments, data.map2(hours, |data, hours| (data, hours)), |stated, calendar, comments, series| (stated, calendar, comments, series))
        .ok()
        .map_err(|records| WeatherError::Records { records })?;
    Ok(Epw {
        station,
        design,
        periods,
        ground,
        daylight_saving,
        holidays,
        comments,
        weekday,
        hours: Hours::try_new(Sun::new(Place { position, elevation, zone: TimeZone::fixed(offset) }, Some(Refraction::Standard), None)?, hours)?,
    })
}

pub fn write<W>(epw: &Epw, writer: W) -> Result<(), WeatherError>
where W: io::Write {
    fn coded<Q>(column: &Column<Q>, quantity: Option<Q>) -> f64 { quantity.map_or(column.missing, |stated| written((column.value)(stated))) }

    const STEPS: u8 = 60;
    let Epw { station, design, periods, ground, daylight_saving, holidays, comments: [first_comment, second_comment], weekday, hours } = epw;
    let sun: &Sun = hours.as_ref();
    let place: &Place = sun.as_ref();
    let offset = place.zone.to_fixed_offset()?;
    let series: &[Hour] = hours.as_ref();
    let length = |record: &Hour| record.interval.end.duration_since(record.interval.start);
    let step = match series {
        [first, ..] => length(first),
        [] => SignedDuration::from_hours(1),
    };
    let Some(per_hour) = (1..=STEPS).filter_map(NonZero::new).find(|records| STEPS.is_multiple_of(records.get()) && SignedDuration::from_mins(i64::from(STEPS.strict_div(records.get()))) == step)
    else {
        return refused([Refusal::Step(0)]);
    };
    refused(
        series
            .iter()
            .positions(|record| {
                length(record) != step || offset.to_datetime(record.interval.end).time().duration_since(civil::Time::midnight()).as_nanos().checked_rem_euclid(step.as_nanos()) != Some(0)
            })
            .map(Refusal::Step),
    )?;
    let spread = 0.15_f64.sqrt();
    let received = |record: &Hour, lit: &[Range<Zoned>]| {
        lit.iter()
            .skip(lit.partition_point(|window| window.end.timestamp() <= record.interval.start))
            .take_while(|window| window.start.timestamp() < record.interval.end)
            .cartesian_product([(0.5 - spread, 5.0 / 18.0), (0.5, 8.0 / 18.0), (0.5 + spread, 5.0 / 18.0)])
            .try_fold((HeatFluxDensity::default(), HeatFluxDensity::default()), |(horizontal, normal), (window, (node, weight))| {
                let (from, to) = (window.start.timestamp().max(record.interval.start), window.end.timestamp().min(record.interval.end));
                let (prepared, position) = rasm_solar::position(sun, from.checked_add(to.duration_since(from).mul_f64(node))?)?;
                let arriving = extraterrestrial(&prepared) * (weight * to.duration_since(from).div_duration_f64(length(record)));
                Ok::<_, WeatherError>((horizontal + arriving * position.elevation_angle().to_radians().sin().max(0.0), normal + arriving))
            })
    };
    let extraterrestrial = series
        .chunk_by(|earlier, later| earlier.interval.end == later.interval.start)
        .filter_map(|run| run.first().zip(run.last()).map(|(first, last)| (run, Range { start: first.interval.start, end: last.interval.end })))
        .map(|(run, interval)| {
            events::windows(sun, interval, Angle::default()).map_err(WeatherError::Solar).and_then(|lit| run.iter().map(|record| received(record, &lit)).collect::<Result<Vec<_>, _>>())
        })
        .flatten_ok()
        .collect::<Result<Vec<_>, _>>()?;
    let row = |(record, (extraterrestrial_horizontal, extraterrestrial_normal)): (&Hour, (HeatFluxDensity, HeatFluxDensity))| -> Result<Row, jiff::Error> {
        let before = offset.to_datetime(record.interval.end).checked_sub(SignedDuration::from_mins(1))?;
        Ok(Row {
            year: before.year(),
            month: before.month(),
            day: before.day(),
            hour: before.hour().strict_add(1),
            minute: before.minute().strict_add(1),
            flags: String::new(),
            dry_bulb: Some(coded(&TEMPERATURE, record.dry_bulb)),
            dew_point: Some(coded(&TEMPERATURE, record.dew_point)),
            relative_humidity: Some(coded(&HUMIDITY, record.relative_humidity)),
            pressure: Some(coded(&PRESSURE, record.pressure)),
            extraterrestrial_horizontal: Some(coded(&IRRADIANCE, Some(extraterrestrial_horizontal))),
            extraterrestrial_normal: Some(coded(&IRRADIANCE, Some(extraterrestrial_normal))),
            infrared: Some(coded(&IRRADIANCE, record.infrared)),
            global_irradiance: Some(coded(&IRRADIANCE, record.global_irradiance)),
            direct_irradiance: Some(coded(&IRRADIANCE, record.direct_irradiance)),
            diffuse_irradiance: Some(coded(&IRRADIANCE, record.diffuse_irradiance)),
            global_illuminance: Some(coded(&ILLUMINANCE, record.global_illuminance)),
            direct_illuminance: Some(coded(&ILLUMINANCE, record.direct_illuminance)),
            diffuse_illuminance: Some(coded(&ILLUMINANCE, record.diffuse_illuminance)),
            zenith_luminance: Some(coded(&LUMINANCE, record.zenith_luminance)),
            wind_direction: Some(coded(&DIRECTION, record.wind_direction)),
            wind_speed: Some(coded(&SPEED, record.wind_speed)),
            total_sky_cover: Some(coded(&COVER, record.total_sky_cover)),
            opaque_sky_cover: Some(coded(&COVER, record.opaque_sky_cover)),
            visibility: Some(coded(&VISIBILITY, record.visibility)),
            ceiling: match record.ceiling {
                Some(Ceiling::Height(height)) => Some(height.get::<meter>()),
                stated @ (None | Some(Ceiling::Cirroform | Ceiling::Unlimited)) => CEILINGS.iter().find(|&&(_, ceiling)| ceiling == stated).map(|&(code, _)| code),
            },
            observation: Some(record.present_weather.map_or(9, |_| 0)),
            present_weather: record.present_weather.unwrap_or([9; 9]).iter().join(""),
            precipitable_water: Some(coded(&WATER, record.precipitable_water)),
            aerosol_optical_depth: Some(coded(&AEROSOL, record.aerosol_optical_depth)),
            snow_depth: Some(coded(&SNOW, record.snow_depth)),
            since_snowfall: Some(coded(&DAYS, record.since_snowfall)),
            albedo: Some(coded(&ALBEDO, record.albedo)),
            precipitation: Some(coded(&WATER, record.precipitation)),
            precipitation_period: Some(coded(&ACCUMULATION, record.precipitation_period)),
        })
    };
    let rows = iter::zip(series, extraterrestrial).map(row).collect::<Result<Vec<_>, _>>()?;
    let dated = series.array_windows().all(|[earlier, later]| earlier.interval.end == later.interval.start) && rows.first().zip(rows.last()).is_some_and(|(first, last)| first.year != last.year);
    let date = |written: &Row| if dated { format!("{}/{}/{}", written.month, written.day, written.year) } else { format!("{}/{}", written.month, written.day) };
    let rendered = |stated: Day| -> Result<String, jiff::Error> {
        match stated.checked()? {
            Day::Date { month, day } => Ok(format!("{month}/{day}")),
            Day::Ordinal(ordinal) => Ok(ordinal.to_string()),
            Day::Weekday { nth, weekday: rule_weekday, month } => {
                let mut names = BrokenDownTime::default();
                names.set_weekday(Some(rule_weekday));
                names.set_month(Some(month))?;
                names.to_string("%A in %B").map(|rule| if nth == -1 { format!("Last {rule}") } else { format!("{nth} {rule}") })
            }
        }
    };
    let text = |keyword| move |stated: Day| rendered(stated).map_err(|source| (Record::Header(keyword), Malformed::Time(source)));
    let (typical, saving, special) = periods
        .iter()
        .map(|Period { name, kind, days }| Validated::from(text(Keyword::Periods)(days.start)).map2(Validated::from(text(Keyword::Periods)(days.last)), |first, end| (name, *kind, first, end)))
        .collect::<Validated<Vec<_>, _>>()
        .map3(
            daylight_saving
                .map_or_else(|| Validated::Good(vec![String::from("0"); 2]), |stated| [stated.start, stated.last].into_iter().map(text(Keyword::Holidays)).collect::<Validated<Vec<_>, _>>()),
            holidays.iter().map(|(name, day)| text(Keyword::Holidays)(*day).map(|written| (name, written))).collect::<Validated<Vec<_>, _>>(),
            |typical, saving, special| (typical, saving, special),
        )
        .ok()
        .map_err(|records| WeatherError::Records { records })?;
    let mut names = BrokenDownTime::default();
    names.set_weekday(Some(*weekday));
    let mut output = WriterBuilder::new().has_headers(false).flexible(true).from_writer(writer);
    output.serialize((Keyword::Location, station, place.position, Time::new::<time::second>(f64::from(offset.seconds())).get::<time::hour>(), place.elevation))?;
    output.serialize((Keyword::DesignConditions, design::encode(design.as_ref())))?;
    output.serialize((Keyword::Periods, periods.len(), typical))?;
    output.serialize((
        Keyword::GroundTemperatures,
        ground.len(),
        ground
            .iter()
            .map(|Ground { depth, conductivity, density, specific_heat, monthly }| {
                (
                    depth.get::<meter>(),
                    conductivity.map(|stated| stated.get::<watt_per_meter_kelvin>()),
                    density.map(|stated| stated.get::<kilogram_per_cubic_meter>()),
                    specific_heat.map(|stated| stated.get::<joule_per_kilogram_kelvin>()),
                    monthly.map(|mean| coded(&TEMPERATURE, Some(mean))),
                )
            })
            .collect_vec(),
    ))?;
    output.serialize((Keyword::Holidays, if rows.iter().any(|written| (written.month, written.day) == (2, 29)) { "Yes" } else { "No" }, saving, holidays.len(), special))?;
    output.serialize((Keyword::FirstComment, first_comment))?;
    output.serialize((Keyword::SecondComment, second_comment))?;
    output.serialize((Keyword::DataPeriods, 1, per_hour, "Data", names.to_string("%A")?, rows.first().map(date), rows.last().map(date)))?;
    rows.iter().try_for_each(|written| output.serialize(written))?;
    output.flush().map_err(|source| WeatherError::Csv(source.into()))
}

fn named<T>(format: &str, token: &str, field: fn(&BrokenDownTime) -> Option<T>) -> Result<T, jiff::Error> {
    strtime::parse(format, token.get(..3).unwrap_or(token)).and_then(|names| field(&names).ok_or_else(|| jiff::Error::from_args(format_args!("{token} matches {format} without naming it"))))
}
