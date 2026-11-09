use core::{
    fmt, iter,
    marker::PhantomData,
    num::NonZero,
    range::{Range, RangeInclusive},
    str::FromStr,
};
use std::io;

use csv::{QuoteStyle, ReaderBuilder, StringRecord, WriterBuilder};
use itertools::{Either, Itertools as _};
use jiff::{
    SignedDuration, civil,
    fmt::strtime::{self, BrokenDownTime},
    tz::{Offset, TimeZone},
};
use rasm_site::{Elevation, Place, Position};
use serde::{
    Deserialize, Deserializer, Serialize, Serializer,
    de::{DeserializeOwned, IgnoredAny, value},
    ser::Error as _,
};
use serde_with::{DeserializeAs, DeserializeFromStr, SerializeAs, serde_as};
use uom::si::{
    angle::degree,
    available_energy::kilojoule_per_kilogram,
    f64::{
        Angle, AvailableEnergy, Frequency, HeatFluxDensity, Illuminance, Length, Luminance, MassDensity, Pressure, Ratio, SpecificHeatCapacity, TemperatureInterval, ThermalConductivity,
        ThermodynamicTemperature, Time, Velocity,
    },
    frequency::cycle_per_hour,
    heat_flux_density::watt_per_square_meter,
    illuminance::lux,
    length::{centimeter, kilometer, meter, millimeter},
    luminance::candela_per_square_meter,
    mass_density::kilogram_per_cubic_meter,
    pressure::pascal,
    ratio::{part_per_thousand, percent, ratio},
    specific_heat_capacity::joule_per_kilogram_kelvin,
    temperature_interval,
    thermal_conductivity::watt_per_meter_kelvin,
    thermodynamic_temperature::degree_celsius,
    time,
    velocity::meter_per_second,
};
use validated::Validated;

use crate::{
    Ceiling, Defect, Hour, Hours, Malformed, Record, Refusal, WeatherError, aligned,
    design::{self, Design},
    length, refused, runs,
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

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Term {
    Rank,
    Weekday,
    Month,
    Day,
}

pub(crate) trait Column {
    type Quantity: Copy + PartialEq;
    const MISSING: f64;
    fn quantity(value: f64) -> Self::Quantity;
    fn value(quantity: Self::Quantity) -> f64;
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, DeserializeFromStr)]
pub enum Day {
    Date { month: i8, day: i8 },
    Ordinal(i16),
    Weekday { nth: i8, weekday: civil::Weekday, month: i8 },
}

impl Day {
    pub fn date(self, year: i16) -> Result<civil::Date, jiff::Error> {
        match self {
            Self::Date { month, day } => civil::Date::new(year, month, day),
            Self::Ordinal(day) => civil::Date::new(year, 1, 1)?.with().day_of_year_no_leap(day).build(),
            Self::Weekday { nth, weekday, month } => civil::Date::new(year, month, 1)?.nth_weekday_of_month(nth, weekday),
        }
    }

    fn checked(self) -> Result<Self, Malformed> {
        const LEAP_YEAR: i16 = 2000;
        match self {
            Self::Date { month: _, day: _ } | Self::Ordinal(_) => Ok(self.date(LEAP_YEAR).map(|_| self)?),
            Self::Weekday { nth: -1 | 1..=5, weekday: _, month } => Ok(civil::Date::new(LEAP_YEAR, month, 1).map(|_| self)?),
            Self::Weekday { nth, weekday: _, month: _ } => Err(Malformed::Day { text: nth.to_string(), term: Term::Rank }),
        }
    }
}

impl FromStr for Day {
    type Err = Malformed;

    fn from_str(s: &str) -> Result<Self, Malformed> {
        let tokens = s.split(['/', ':', '-']).flat_map(str::split_whitespace).filter(|token| !(token.eq_ignore_ascii_case("in") || token.eq_ignore_ascii_case("of"))).collect_vec();
        let month_named = |token: &str| named("%b", token, BrokenDownTime::month, Term::Month);
        let weekday_named = |token: &str| named("%a", token, BrokenDownTime::weekday, Term::Weekday);
        let unnamed = || Malformed::Day { text: s.to_owned(), term: Term::Day };
        match *tokens {
            [ordinal] => ordinal.parse().map(Self::Ordinal).map_err(|source| Malformed::Ordinal { text: ordinal.to_owned(), source }),
            [first, second] => match (first.parse().ok(), second.parse().ok()) {
                (Some(month), Some(day)) => Ok(Self::Date { month, day }),
                (Some(day), None) => month_named(second).map(|month| Self::Date { month, day }),
                (None, Some(day)) => month_named(first).map(|month| Self::Date { month, day }),
                (None, None) => Err(unnamed()),
            },
            [rank, first, second] => {
                let nth = if rank.eq_ignore_ascii_case("last") { Some(-1) } else { rank.trim_end_matches(char::is_alphabetic).parse().ok() }
                    .ok_or_else(|| Malformed::Day { text: rank.to_owned(), term: Term::Rank })?;
                let (weekday, month) = weekday_named(first).map(|weekday| (weekday, second)).or_else(|_| weekday_named(second).map(|weekday| (weekday, first)))?;
                month_named(month).map(|named_month| Self::Weekday { nth, weekday, month: named_month })
            }
            _ => Err(unnamed()),
        }
        .and_then(Self::checked)
    }
}

impl Serialize for Day {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where S: Serializer {
        serializer.serialize_str(&text(*self).map_err(S::Error::custom)?)
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
    pub years: Option<Vec<i16>>,
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

#[serde_as]
#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
pub struct Ground {
    #[serde_as(as = "Stated<Depth, 0>")]
    pub depth: Length,
    #[serde_as(as = "Option<Stated<Conductivity, 0>>")]
    pub conductivity: Option<ThermalConductivity>,
    #[serde_as(as = "Option<Stated<Density, 0>>")]
    pub density: Option<MassDensity>,
    #[serde_as(as = "Option<Stated<SpecificHeat, 0>>")]
    pub specific_heat: Option<SpecificHeatCapacity>,
    #[serde_as(as = "[Stated<Temperature, 2>; 12]")]
    pub monthly: [ThermodynamicTemperature; 12],
}

#[serde_as]
#[derive(Default, Serialize, Deserialize)]
#[serde(default)]
struct Row {
    year: i16,
    month: i8,
    day: i8,
    hour: i8,
    minute: i8,
    flags: Option<String>,
    #[serde_as(as = "Coded<Temperature, 1>")]
    dry_bulb: Option<ThermodynamicTemperature>,
    #[serde_as(as = "Coded<Temperature, 1>")]
    dew_point: Option<ThermodynamicTemperature>,
    #[serde_as(as = "Coded<Humidity, 0>")]
    relative_humidity: Option<Ratio>,
    #[serde_as(as = "Coded<Barometric, 0>")]
    pressure: Option<Pressure>,
    #[serde_as(as = "Coded<Irradiance, 0>")]
    extraterrestrial_horizontal: Option<HeatFluxDensity>,
    #[serde_as(as = "Coded<Irradiance, 0>")]
    extraterrestrial_normal: Option<HeatFluxDensity>,
    #[serde_as(as = "Coded<Irradiance, 0>")]
    infrared: Option<HeatFluxDensity>,
    #[serde_as(as = "Coded<Irradiance, 0>")]
    global_irradiance: Option<HeatFluxDensity>,
    #[serde_as(as = "Coded<Irradiance, 0>")]
    direct_irradiance: Option<HeatFluxDensity>,
    #[serde_as(as = "Coded<Irradiance, 0>")]
    diffuse_irradiance: Option<HeatFluxDensity>,
    #[serde_as(as = "Coded<Daylight, 0>")]
    global_illuminance: Option<Illuminance>,
    #[serde_as(as = "Coded<Daylight, 0>")]
    direct_illuminance: Option<Illuminance>,
    #[serde_as(as = "Coded<Daylight, 0>")]
    diffuse_illuminance: Option<Illuminance>,
    #[serde_as(as = "Coded<Zenith, 0>")]
    zenith_luminance: Option<Luminance>,
    #[serde_as(as = "Coded<Direction, 0>")]
    wind_direction: Option<Angle>,
    #[serde_as(as = "Coded<Speed, 1>")]
    wind_speed: Option<Velocity>,
    #[serde_as(as = "Coded<Cover, 0>")]
    total_sky_cover: Option<Ratio>,
    #[serde_as(as = "Coded<Cover, 0>")]
    opaque_sky_cover: Option<Ratio>,
    #[serde_as(as = "Coded<Visibility, 1>")]
    visibility: Option<Length>,
    #[serde_as(as = "Ceilings")]
    ceiling: Option<Ceiling>,
    observation: Option<i8>,
    present_weather: String,
    #[serde_as(as = "Coded<Water, 0>")]
    precipitable_water: Option<Length>,
    #[serde_as(as = "Coded<Aerosol, 4>")]
    aerosol_optical_depth: Option<Ratio>,
    #[serde_as(as = "Coded<Snow, 0>")]
    snow_depth: Option<Length>,
    #[serde_as(as = "Coded<Days, 0>")]
    since_snowfall: Option<Time>,
    #[serde_as(as = "Coded<Albedo, 3>")]
    albedo: Option<Ratio>,
    #[serde_as(as = "Coded<Water, 1>")]
    precipitation: Option<Length>,
    #[serde_as(as = "Coded<Accumulation, 1>")]
    precipitation_period: Option<Time>,
}

pub(crate) struct Coded<C, const DECIMALS: usize>(PhantomData<C>);

pub(crate) struct Stated<C, const DECIMALS: usize>(PhantomData<C>);

struct Ceilings;

macro_rules! columns {
    ($($column:ident($quantity:ty, $unit:ty, $missing:expr);)*) => {$(
        pub(crate) struct $column;

        impl Column for $column {
            type Quantity = $quantity;
            const MISSING: f64 = $missing;
            fn quantity(value: f64) -> $quantity { <$quantity>::new::<$unit>(value) }
            fn value(quantity: $quantity) -> f64 { quantity.get::<$unit>() }
        }
    )*};
}

columns! {
    Temperature(ThermodynamicTemperature, degree_celsius, 99.9);
    Humidity(Ratio, percent, 999.0);
    Barometric(Pressure, pascal, 999_999.0);
    Irradiance(HeatFluxDensity, watt_per_square_meter, 9999.0);
    Daylight(Illuminance, lux, 999_900.0);
    Zenith(Luminance, candela_per_square_meter, 9999.0);
    Direction(Angle, degree, 999.0);
    Speed(Velocity, meter_per_second, 999.0);
    Visibility(Length, kilometer, 9999.0);
    Water(Length, millimeter, 999.0);
    Aerosol(Ratio, ratio, 0.999);
    Snow(Length, centimeter, 999.0);
    Days(Time, time::day, 99.0);
    Albedo(Ratio, ratio, 999.0);
    Accumulation(Time, time::hour, 99.0);
    Depth(Length, meter, f64::INFINITY);
    Conductivity(ThermalConductivity, watt_per_meter_kelvin, f64::INFINITY);
    Density(MassDensity, kilogram_per_cubic_meter, f64::INFINITY);
    SpecificHeat(SpecificHeatCapacity, joule_per_kilogram_kelvin, f64::INFINITY);
    Interval(TemperatureInterval, temperature_interval::degree_celsius, f64::INFINITY);
    MoistureRatio(Ratio, part_per_thousand, f64::INFINITY);
    Enthalpy(AvailableEnergy, kilojoule_per_kilogram, f64::INFINITY);
    Shielding(Frequency, cycle_per_hour, f64::INFINITY);
    Elapsed(Time, time::hour, f64::INFINITY);
}

struct Cover;

impl Column for Cover {
    type Quantity = Ratio;

    const MISSING: f64 = 99.0;

    fn quantity(value: f64) -> Ratio { Ratio::new::<ratio>(value / 10.0) }

    fn value(quantity: Ratio) -> f64 { quantity.get::<ratio>() * 10.0 }
}

impl<'de, C, const DECIMALS: usize> DeserializeAs<'de, Option<C::Quantity>> for Coded<C, DECIMALS>
where C: Column
{
    fn deserialize_as<D>(deserializer: D) -> Result<Option<C::Quantity>, D::Error>
    where D: Deserializer<'de> {
        Option::<f64>::deserialize(deserializer).map(|value| value.filter(|stated| *stated < C::MISSING).map(C::quantity))
    }
}

impl<C, const DECIMALS: usize> SerializeAs<Option<C::Quantity>> for Coded<C, DECIMALS>
where C: Column
{
    fn serialize_as<S>(source: &Option<C::Quantity>, serializer: S) -> Result<S::Ok, S::Error>
    where S: Serializer {
        serializer.serialize_str(&source.map_or_else(|| format!("{:.DECIMALS$}", C::MISSING), |quantity| decimal(C::value(quantity), DECIMALS, |stated| C::quantity(stated) == quantity)))
    }
}

impl<'de, C, const DECIMALS: usize> DeserializeAs<'de, C::Quantity> for Stated<C, DECIMALS>
where C: Column
{
    fn deserialize_as<D>(deserializer: D) -> Result<C::Quantity, D::Error>
    where D: Deserializer<'de> {
        f64::deserialize(deserializer).map(C::quantity)
    }
}

impl<C, const DECIMALS: usize> SerializeAs<C::Quantity> for Stated<C, DECIMALS>
where C: Column
{
    fn serialize_as<S>(source: &C::Quantity, serializer: S) -> Result<S::Ok, S::Error>
    where S: Serializer {
        serializer.serialize_str(&decimal(C::value(*source), DECIMALS, |stated| C::quantity(stated) == *source))
    }
}

impl<'de> DeserializeAs<'de, Option<Ceiling>> for Ceilings {
    fn deserialize_as<D>(deserializer: D) -> Result<Option<Ceiling>, D::Error>
    where D: Deserializer<'de> {
        Option::<f64>::deserialize(deserializer)
            .map(|code| code.and_then(|code| CEILINGS.iter().find(|&&(threshold, _)| code >= threshold).map_or_else(|| Some(Ceiling::Height(Length::new::<meter>(code))), |&(_, ceiling)| ceiling)))
    }
}

impl SerializeAs<Option<Ceiling>> for Ceilings {
    fn serialize_as<S>(source: &Option<Ceiling>, serializer: S) -> Result<S::Ok, S::Error>
    where S: Serializer {
        match *source {
            Some(Ceiling::Height(height)) => serializer.serialize_str(&decimal(height.get::<meter>(), 0, |stated| Length::new::<meter>(stated) == height)),
            stated @ (None | Some(Ceiling::Cirroform | Ceiling::Unlimited)) => CEILINGS.iter().find(|&&(_, ceiling)| ceiling == stated).map(|&(code, _)| format!("{code:.0}")).serialize(serializer),
        }
    }
}

// --- [TABLES] --------------------------------------------------------------------------

const CEILINGS: [(f64, Option<Ceiling>); 3] = [(99_999.0, None), (88_888.0, Some(Ceiling::Cirroform)), (77_777.0, Some(Ceiling::Unlimited))];

// --- [OPERATIONS] ----------------------------------------------------------------------

macro_rules! transferred {
    ($record:ident { $($field:ident: $value:expr),* $(,)? } from $source:ident) => {
        $record {
            $($field: $value,)*
            dry_bulb: $source.dry_bulb,
            dew_point: $source.dew_point,
            relative_humidity: $source.relative_humidity,
            pressure: $source.pressure,
            extraterrestrial_horizontal: $source.extraterrestrial_horizontal,
            extraterrestrial_normal: $source.extraterrestrial_normal,
            infrared: $source.infrared,
            global_irradiance: $source.global_irradiance,
            direct_irradiance: $source.direct_irradiance,
            diffuse_irradiance: $source.diffuse_irradiance,
            global_illuminance: $source.global_illuminance,
            direct_illuminance: $source.direct_illuminance,
            diffuse_illuminance: $source.diffuse_illuminance,
            zenith_luminance: $source.zenith_luminance,
            wind_direction: $source.wind_direction,
            wind_speed: $source.wind_speed,
            total_sky_cover: $source.total_sky_cover,
            opaque_sky_cover: $source.opaque_sky_cover,
            visibility: $source.visibility,
            ceiling: $source.ceiling,
            precipitable_water: $source.precipitable_water,
            aerosol_optical_depth: $source.aerosol_optical_depth,
            snow_depth: $source.snow_depth,
            since_snowfall: $source.since_snowfall,
            albedo: $source.albedo,
            precipitation: $source.precipitation,
            precipitation_period: $source.precipitation_period,
        }
    };
}

pub fn read<R>(reader: R) -> Result<Epw, WeatherError>
where R: io::Read {
    fn header<T, F>(headers: &[(Keyword, StringRecord, StringRecord)], keyword: Keyword, decode: F) -> Validated<T, (Record, Malformed)>
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

    fn redated(records: Vec<Hour>, zone: &TimeZone) -> Result<Vec<Hour>, jiff::Error> {
        let first = records.first().map(|record| zone.to_datetime(record.interval.start));
        records
            .into_iter()
            .scan((first.map(civil::DateTime::year), first), |(year, previous), record| {
                let local = zone.to_datetime(record.interval.start);
                if previous.replace(local).is_some_and(|earlier| (local.month(), local.day(), local.time()) < (earlier.month(), earlier.day(), earlier.time())) {
                    *year = year.map(|stated| stated.strict_add(1));
                }
                Some(year.map(|stated| (stated, record)))
            })
            .flatten()
            .map(|(stated, record)| crate::redated(record, zone, stated))
            .collect()
    }

    let hour = |row: Row, offset: Offset, per_hour: NonZero<u8>| -> Result<Hour, Malformed> {
        let elapsed = if per_hour > NonZero::<u8>::MIN { SignedDuration::from_mins(i64::from(row.minute)) } else { SignedDuration::from_hours(1) };
        let end = offset.to_timestamp(civil::DateTime::new(row.year, row.month, row.day, row.hour.saturating_sub(1), 0, 0, 0)?.checked_add(elapsed)?)?;
        Ok(transferred!(Hour {
            interval: Range { start: end.checked_sub(SignedDuration::from_hours(1).div_f64(f64::from(per_hour.get())))?, end },
            present_weather: row
                .present_weather
                .trim_matches(|stated: char| stated.is_whitespace() || matches!(stated, '\'' | '"'))
                .as_bytes()
                .as_array::<9>()
                .filter(|_| row.observation == Some(0))
                .map(|codes| codes.map(|code| if code.is_ascii_digit() { code.strict_sub(b'0') } else { 9 })),
            flags: row.flags,
        } from row))
    };
    let (headers, rows): (Vec<_>, Vec<_>) = ReaderBuilder::new().has_headers(false).flexible(true).from_reader(reader).byte_records().process_results(|lines| {
        lines.partition_map(|record| {
            let position = record.position().cloned();
            let mut record = StringRecord::from_byte_record_lossy(record);
            record.set_position(position);
            if let Some(first) = record.get(0)
                && let Ok(stated) = Keyword::deserialize(value::StrDeserializer::<value::Error>::new(first.trim()))
            {
                let line = record.clone();
                record.trim();
                Either::Left((stated, line, record))
            } else {
                record.trim();
                Either::Right(record)
            }
        })
    })?;
    let location = header(&headers, Keyword::Location, |_, record| {
        let (_, station, position, zone, elevation) = record.deserialize::<(IgnoredAny, Station, Position, f64, Elevation)>(None)?;
        Ok((station, position, Offset::try_from(SignedDuration::try_from_secs_f64(Time::new::<time::hour>(zone).get::<time::second>())?)?, elevation))
    });
    let data = header(&headers, Keyword::DataPeriods, |_, record| {
        let (_, _, per_hour, _, weekday) = record.deserialize::<(IgnoredAny, IgnoredAny, NonZero<u8>, IgnoredAny, &str)>(None)?;
        Ok((per_hour, named("%a", weekday, BrokenDownTime::weekday, Term::Weekday)?))
    });
    let stamp = location.iter().next().zip(data.iter().next()).map(|(&(_, _, offset, _), &(per_hour, _))| (offset, per_hour));
    let design = header(&headers, Keyword::DesignConditions, |_, record| Ok(design::decode(&record.iter().skip(1).collect())?));
    let periods = header(&headers, Keyword::Periods, |_, record| {
        Ok(repeated::<(String, Kind, Day, Day)>(record, 2)?.map(|(name, kind, start, last)| Period { name, kind, days: RangeInclusive { start, last } }).collect_vec())
    });
    let ground = header(&headers, Keyword::GroundTemperatures, |_, record| Ok(repeated::<Ground>(record, 2)?.collect_vec()));
    let calendar = header(&headers, Keyword::Holidays, |_, record| {
        let (_, _, start, last) = record.deserialize::<(IgnoredAny, IgnoredAny, String, String)>(None)?;
        let saving =
            (!start.trim_start_matches('0').is_empty()).then(|| start.parse::<Day>().and_then(|first| last.parse::<Day>().map(|end| RangeInclusive { start: first, last: end }))).transpose()?;
        Ok((saving, repeated::<(String, Day)>(record, 5)?.collect_vec()))
    });
    let comment = |stated| header(&headers, stated, |line, _| Ok(line.iter().skip(1).join(",").trim_end_matches(',').to_owned()));
    let comments = comment(Keyword::FirstComment).map2(comment(Keyword::SecondComment), |first, second| [first, second]);
    let decoded = rows
        .into_iter()
        .filter_map(|record| {
            let decoded = match record.deserialize::<Row>(None) {
                Ok(row) => stamp.map(|(offset, per_hour)| hour(row, offset, per_hour))?,
                Err(source) => Err(Malformed::Csv(source)),
            };
            Some(decoded.map_err(|malformed| (Record::Row(record.position().map_or_default(csv::Position::line)), malformed)))
        })
        .collect::<Validated<Vec<Hour>, _>>();
    let (((station, position, offset, elevation), design, periods, ground), (daylight_saving, holidays), comments, ((_, weekday), records)) = location
        .map4(design, periods, ground, |location, design, periods, ground| (location, design, periods, ground))
        .map4(calendar, comments, data.map2(decoded, |data, records| (data, records)), |stated, calendar, comments, series| (stated, calendar, comments, series))
        .ok()?;
    let zone = TimeZone::fixed(offset);
    let years = (runs(&records).count() > 1)
        .then(|| records.iter().map(|record| zone.to_datetime(record.interval.end).checked_sub(SignedDuration::from_mins(1)).map(civil::DateTime::year)).collect::<Result<Vec<_>, _>>())
        .transpose()?;
    let records = if years.is_some() { redated(records, &zone)? } else { records };
    Ok(Epw { station, design, periods, ground, daylight_saving, holidays, comments, weekday, years, hours: Hours::try_new(Place { position, elevation, zone }, records)? })
}

pub fn write<W>(epw: &Epw, writer: W) -> Result<(), WeatherError>
where W: io::Write {
    let Epw { station, design, periods, ground, daylight_saving, holidays, comments: [first_comment, second_comment], weekday, years, hours } = epw;
    let place: &Place = hours.as_ref();
    let offset = place.zone.to_fixed_offset()?;
    let series: &[Hour] = hours.as_ref();
    let hour = SignedDuration::from_hours(1);
    let step = series.first().map_or(hour, |record| length(record.interval));
    let per_hour =
        (aligned(hour, step) && aligned(step, SignedDuration::from_mins(1))).then(|| (1..=u8::MAX).filter_map(NonZero::new).find(|records| hour.checked_div(i32::from(records.get())) == Some(step)));
    let Some(per_hour) = per_hour.flatten() else {
        return refused([Refusal::Hour { index: 0, defect: Defect::Step }]);
    };
    refused(
        series
            .iter()
            .positions(|record| length(record.interval) != step || !aligned(offset.to_datetime(record.interval.end).time().duration_since(civil::Time::midnight()), step))
            .map(|index| Refusal::Hour { index, defect: Defect::Step }),
    )?;
    let checked = |keyword: Keyword, day: Day| Validated::from(day.checked().map_err(|malformed| (Record::Header(keyword), malformed)));
    periods
        .iter()
        .flat_map(|period| [period.days.start, period.days.last])
        .map(|day| checked(Keyword::Periods, day))
        .chain(daylight_saving.iter().flat_map(|saving| [saving.start, saving.last]).chain(holidays.iter().map(|&(_, day)| day)).map(|day| checked(Keyword::Holidays, day)))
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    refused(years.as_ref().filter(|years| years.len() != series.len()).map(|years| Refusal::Years { stated: years.len(), hours: series.len() }))?;
    let row = |(record, year): (&Hour, Option<i16>)| -> Result<Row, jiff::Error> {
        let before = offset.to_datetime(record.interval.end).checked_sub(SignedDuration::from_mins(1))?;
        Ok(transferred!(Row {
            year: year.unwrap_or_else(|| before.year()),
            month: before.month(),
            day: before.day(),
            hour: before.hour().strict_add(1),
            minute: before.minute().strict_add(1),
            flags: record.flags.clone(),
            observation: Some(record.present_weather.map_or(9, |_| 0)),
            present_weather: record.present_weather.unwrap_or([9; 9]).iter().join(""),
        } from record))
    };
    let rows = iter::zip(series, years.iter().flatten().copied().map(Some).chain(iter::repeat(None))).map(row).collect::<Result<Vec<_>, _>>()?;
    let dated =
        runs(series).count() == 1 && series.first().zip(series.last()).is_some_and(|(first, last)| offset.to_datetime(first.interval.start).year() != offset.to_datetime(last.interval.start).year());
    let date = |stated: &Row| if dated { format!("{}/{}/{}", stated.month, stated.day, stated.year) } else { format!("{:>2}/{:>2}", stated.month, stated.day) };
    let saving = daylight_saving.map_or_else(|| Ok(vec![String::from("0"); 2]), |saving| [saving.start, saving.last].into_iter().map(text).collect::<Result<Vec<_>, _>>())?;
    let mut output = WriterBuilder::new().has_headers(false).flexible(true).quote_style(QuoteStyle::Never).from_writer(writer);
    let [latitude, longitude] = [place.position.latitude(), place.position.longitude()].map(|angle| {
        let degrees = angle.get::<degree>();
        decimal(degrees, 2, |stated| stated.total_cmp(&degrees).is_eq())
    });
    let zone = Time::new::<time::second>(f64::from(offset.seconds())).get::<time::hour>();
    let elevation = place.elevation.get::<meter>();
    let [zone, elevation] = [zone, elevation].map(|value| decimal(value, 1, |stated| stated.total_cmp(&value).is_eq()));
    output.serialize((Keyword::Location, station, latitude, longitude, zone, elevation))?;
    output.serialize((Keyword::DesignConditions, design::encode(design.as_ref())))?;
    output.serialize((Keyword::Periods, periods.len(), periods.iter().map(|Period { name, kind, days }| (name, kind, days.start, days.last)).collect_vec()))?;
    output.serialize((Keyword::GroundTemperatures, ground.len(), ground))?;
    output.serialize((Keyword::Holidays, if rows.iter().any(|stated| (stated.month, stated.day) == (2, 29)) { "Yes" } else { "No" }, saving, holidays.len(), holidays))?;
    output.serialize((Keyword::FirstComment, first_comment))?;
    output.serialize((Keyword::SecondComment, second_comment))?;
    output.serialize((Keyword::DataPeriods, 1, per_hour, "Data", names(Some(*weekday), None, "%A")?, rows.first().map(date), rows.last().map(date)))?;
    rows.iter().try_for_each(|stated| output.serialize(stated))?;
    output.flush().map_err(|source| WeatherError::Csv(source.into()))
}

fn named<T>(format: &str, token: &str, field: fn(&BrokenDownTime) -> Option<T>, term: Term) -> Result<T, Malformed> {
    field(&strtime::parse(format, token.get(..3).unwrap_or(token))?).ok_or_else(|| Malformed::Day { text: token.to_owned(), term })
}

fn text(day: Day) -> Result<String, jiff::Error> {
    Ok(match day {
        Day::Date { month, day } => format!("{month}/{day:>2}"),
        Day::Ordinal(ordinal) => ordinal.to_string(),
        Day::Weekday { nth, weekday, month } => {
            let rule = names(Some(weekday), Some(month), "%A in %B")?;
            if nth == -1 { format!("Last {rule}") } else { format!("{nth} {rule}") }
        }
    })
}

fn names(weekday: Option<civil::Weekday>, month: Option<i8>, format: &str) -> Result<String, jiff::Error> {
    let mut names = BrokenDownTime::default();
    names.set_weekday(weekday);
    names.set_month(month)?;
    names.to_string(format)
}

fn decimal(value: f64, minimum: usize, exact: impl Fn(f64) -> bool) -> String {
    (minimum..=f64::DIGITS as usize).map(|places| format!("{value:.places$}")).find(|text| text.parse().is_ok_and(&exact)).unwrap_or_else(|| value.to_string())
}
