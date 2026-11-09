use core::f64::consts::PI;

use csv::StringRecord;
use itertools::Itertools as _;
use jiff::civil;
use nalgebra::Vector2;
use rasm_site::Place;
use serde::{Deserialize, Serialize};
use serde_with::serde_as;
use statrs::{
    consts::EULER_MASCHERONI,
    statistics::{Data, OrderStatistics as _, Statistics as _},
};
use uom::si::{
    available_energy::joule_per_kilogram,
    f64::{Angle, AvailableEnergy, Frequency, Ratio, TemperatureInterval, ThermodynamicTemperature, Time, Velocity},
    ratio::ratio,
    temperature_interval,
    thermodynamic_temperature::kelvin,
    velocity::meter_per_second,
};

use crate::{
    Hour, Hours, WeatherError, bearing,
    comfort::{Psychrometrics, psychrometrics},
    epw::{Direction, Elapsed, Enthalpy, Interval, MoistureRatio, Shielding, Speed, Stated, Temperature},
    heading, local, weighted,
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, Deserialize, Serialize)]
enum Heading {
    Heating,
    Cooling,
    Extremes,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Design {
    pub source: String,
    pub edition: Edition,
    pub heating: Heating,
    pub cooling: Cooling,
    pub extremes: Extremes,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Edition {
    Ashrae2009 { hours: Option<Time> },
    Ashrae2017 { minimum_wet_bulb: Option<ThermodynamicTemperature> },
    Ashrae2021 { shielding: Option<Frequency> },
    Ashrae2025 { shielding: Option<Frequency>, wet_bulb: Annual },
}

#[serde_as]
#[derive(Debug, Clone, Copy, PartialEq, Deserialize, Serialize)]
pub struct Humidification {
    #[serde_as(as = "Option<Stated<Temperature, 0>>")]
    pub dew_point: Option<ThermodynamicTemperature>,
    #[serde_as(as = "Option<Stated<MoistureRatio, 0>>")]
    pub humidity_ratio: Option<Ratio>,
    #[serde_as(as = "Option<Stated<Temperature, 0>>")]
    pub dry_bulb: Option<ThermodynamicTemperature>,
}

#[serde_as]
#[derive(Debug, Clone, Copy, PartialEq, Deserialize, Serialize)]
pub struct Heating {
    pub month: Option<i8>,
    #[serde_as(as = "[Option<Stated<Temperature, 0>>; 2]")]
    pub dry_bulb: [Option<ThermodynamicTemperature>; 2],
    pub humidification: [Humidification; 2],
    #[serde_as(as = "[(Option<Stated<Speed, 0>>, Option<Stated<Temperature, 0>>); 2]")]
    pub wind: [(Option<Velocity>, Option<ThermodynamicTemperature>); 2],
    #[serde_as(as = "(Option<Stated<Speed, 0>>, Option<Stated<Direction, 0>>)")]
    pub coincident: (Option<Velocity>, Option<Angle>),
}

#[serde_as]
#[derive(Debug, Clone, Copy, PartialEq, Deserialize, Serialize)]
pub struct Cooling {
    pub month: Option<i8>,
    #[serde_as(as = "Option<Stated<Interval, 0>>")]
    pub range: Option<TemperatureInterval>,
    #[serde_as(as = "[(Option<Stated<Temperature, 0>>, Option<Stated<Temperature, 0>>); 3]")]
    pub dry_bulb: [(Option<ThermodynamicTemperature>, Option<ThermodynamicTemperature>); 3],
    #[serde_as(as = "[(Option<Stated<Temperature, 0>>, Option<Stated<Temperature, 0>>); 3]")]
    pub wet_bulb: [(Option<ThermodynamicTemperature>, Option<ThermodynamicTemperature>); 3],
    #[serde_as(as = "(Option<Stated<Speed, 0>>, Option<Stated<Direction, 0>>)")]
    pub coincident: (Option<Velocity>, Option<Angle>),
    pub dehumidification: [Humidification; 3],
    #[serde_as(as = "[(Option<Stated<Enthalpy, 0>>, Option<Stated<Temperature, 0>>); 3]")]
    pub enthalpy: [(Option<AvailableEnergy>, Option<ThermodynamicTemperature>); 3],
}

#[serde_as]
#[derive(Debug, Clone, Copy, PartialEq, Deserialize, Serialize)]
pub struct Annual {
    #[serde_as(as = "[Option<Stated<Temperature, 0>>; 2]")]
    pub mean: [Option<ThermodynamicTemperature>; 2],
    #[serde_as(as = "[Option<Stated<Interval, 0>>; 2]")]
    pub deviation: [Option<TemperatureInterval>; 2],
    #[serde_as(as = "[[Option<Stated<Temperature, 0>>; 2]; 4]")]
    pub returns: [[Option<ThermodynamicTemperature>; 2]; 4],
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Extremes {
    pub wind: [Option<Velocity>; 3],
    pub wet_bulb: Option<ThermodynamicTemperature>,
    pub dry_bulb: Annual,
}

#[serde_as]
#[derive(Deserialize, Serialize)]
struct Ashrae2009<'source> {
    count: u8,
    source: &'source str,
    blank: &'source str,
    heating_label: Heading,
    heating: Heating,
    cooling_label: Heading,
    cooling: Cooling,
    #[serde_as(as = "Option<Stated<Elapsed, 0>>")]
    hours: Option<Time>,
    extremes_label: Heading,
    #[serde_as(as = "[Option<Stated<Speed, 0>>; 3]")]
    wind: [Option<Velocity>; 3],
    #[serde_as(as = "Option<Stated<Temperature, 0>>")]
    wet_bulb: Option<ThermodynamicTemperature>,
    dry_bulb: Annual,
}

#[serde_as]
#[derive(Deserialize, Serialize)]
struct Ashrae2017<'source> {
    count: u8,
    source: &'source str,
    blank: &'source str,
    heating_label: Heading,
    heating: Heating,
    cooling_label: Heading,
    cooling: Cooling,
    #[serde_as(as = "Option<Stated<Temperature, 0>>")]
    wet_bulb: Option<ThermodynamicTemperature>,
    extremes_label: Heading,
    #[serde_as(as = "[Option<Stated<Speed, 0>>; 3]")]
    wind: [Option<Velocity>; 3],
    dry_bulb: Annual,
    #[serde_as(as = "Option<Stated<Temperature, 0>>")]
    #[serde(skip_serializing_if = "Option::is_none")]
    minimum_wet_bulb: Option<ThermodynamicTemperature>,
}

#[serde_as]
#[derive(Deserialize, Serialize)]
struct Ashrae2021<'source> {
    count: u8,
    source: &'source str,
    blank: &'source str,
    heating_label: Heading,
    heating: Heating,
    #[serde_as(as = "Option<Stated<Shielding, 0>>")]
    shielding: Option<Frequency>,
    cooling_label: Heading,
    cooling: Cooling,
    #[serde_as(as = "Option<Stated<Temperature, 0>>")]
    wet_bulb: Option<ThermodynamicTemperature>,
    extremes_label: Heading,
    #[serde_as(as = "[Option<Stated<Speed, 0>>; 3]")]
    wind: [Option<Velocity>; 3],
    dry_bulb: Annual,
}

#[serde_as]
#[derive(Deserialize, Serialize)]
struct Ashrae2025<'source> {
    count: u8,
    source: &'source str,
    heating_label: Heading,
    heating: Heating,
    #[serde_as(as = "Option<Stated<Shielding, 0>>")]
    shielding: Option<Frequency>,
    cooling_label: Heading,
    cooling: Cooling,
    #[serde_as(as = "Option<Stated<Temperature, 0>>")]
    wet_bulb: Option<ThermodynamicTemperature>,
    extremes_label: Heading,
    #[serde_as(as = "[Option<Stated<Speed, 0>>; 3]")]
    wind: [Option<Velocity>; 3],
    dry_bulb: Annual,
    annual_wet_bulb: Annual,
}

#[derive(Serialize)]
enum Layout<'source> {
    #[serde(rename = "0")]
    Absent,
    Ashrae2009(Ashrae2009<'source>),
    Ashrae2017(Ashrae2017<'source>),
    Ashrae2021(Ashrae2021<'source>),
    Ashrae2025(Ashrae2025<'source>),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn profile(peak: ThermodynamicTemperature, range: TemperatureInterval) -> [ThermodynamicTemperature; 24] {
    const RANGE_FRACTIONS: [f64; 24] = [0.88, 0.92, 0.95, 0.98, 1.0, 0.98, 0.91, 0.74, 0.55, 0.38, 0.23, 0.13, 0.05, 0.0, 0.0, 0.06, 0.14, 0.24, 0.39, 0.5, 0.59, 0.68, 0.75, 0.82];
    RANGE_FRACTIONS.map(|fraction| peak - range * fraction)
}

pub fn computed<S>(hours: &Hours<S>, source: String, coincidence: Ratio) -> Result<Design, WeatherError>
where S: AsRef<Place> {
    type Sample<'hour> = (civil::DateTime, &'hour Hour, Option<Psychrometrics>);
    const HEATING: [f64; 2] = [0.004, 0.01];
    const COOLING: [f64; 3] = [0.996, 0.99, 0.98];
    const WIND: [f64; 3] = [0.99, 0.975, 0.95];
    const RETURNS: [f64; 4] = [5.0, 10.0, 20.0, 50.0];
    fn ranked<'sample, T>(within: impl IntoIterator<Item = &'sample T>, field: impl Fn(&T) -> Option<f64>) -> Vec<(f64, &'sample T)>
    where T: 'sample {
        within.into_iter().filter_map(|sample| field(sample).map(|value| (value, sample))).sorted_by(|(left, _), (right, _)| left.total_cmp(right)).collect()
    }
    fn level<T>(within: &[T], field: impl Fn(&T) -> Option<f64>, share: f64) -> Option<f64> {
        let values = within.iter().filter_map(field).collect_vec();
        (!values.is_empty()).then(|| Data::new(values).quantile(share))
    }
    fn window<'sample, 'view, T>(ranked: &'view [(f64, &'sample T)], share: f64, band: f64) -> &'view [(f64, &'sample T)] {
        let count = nalgebra::convert::<usize, f64>(ranked.len());
        let [low, high] = [share - band, share + band].map(|bound| (bound.clamp(0.0, 1.0) * count).to_int_saturating::<usize>());
        let low = low.min(ranked.len().saturating_sub(1));
        &ranked[low..high.max(low.strict_add(1)).min(ranked.len())]
    }
    fn average<'sample, T>(within: impl IntoIterator<Item = &'sample T>, field: impl Fn(&T) -> Option<f64>) -> Option<f64>
    where T: 'sample {
        weighted(within.into_iter().filter_map(|sample| field(sample).map(|value| (value, 1.0))))
    }
    fn coincident<T>(within: &[(f64, &T)], share: f64, band: f64, other: impl Fn(&T) -> Option<f64>) -> Option<f64> { average(window(within, share, band).iter().map(|&(_, sample)| sample), other) }
    let band = coincidence.get::<ratio>();
    let samples = hours
        .records
        .iter()
        .map(|hour| {
            let state = match (hour.dry_bulb, hour.relative_humidity, hour.pressure) {
                (Some(dry_bulb), Some(humidity), Some(pressure)) => psychrometrics(dry_bulb, humidity, pressure)?,
                (None | Some(_), None | Some(_), None | Some(_)) => None,
            };
            Ok((local(hours, hour), hour, state))
        })
        .collect::<Result<Vec<Sample<'_>>, WeatherError>>()?;
    let dry_bulb = |sample: &Sample<'_>| sample.1.dry_bulb.map(|value| value.value);
    let wet_bulb = |sample: &Sample<'_>| sample.2.map(|state| state.wet_bulb.value);
    let dew_point = |sample: &Sample<'_>| sample.2.and_then(|state| state.dew_point).map(|value| value.value);
    let humidity_ratio = |sample: &Sample<'_>| sample.2.map(|state| state.humidity_ratio.value);
    let enthalpy = |sample: &Sample<'_>| sample.2.map(|state| state.enthalpy.value);
    let speed = |sample: &Sample<'_>| sample.1.wind_speed.map(|value| value.value);
    let [dry, wet, dew, energy] = [dry_bulb, wet_bulb, dew_point, enthalpy].map(|field| ranked(&samples, field));
    let temperature = |value: Option<f64>| value.map(ThermodynamicTemperature::new::<kelvin>);
    let velocity = |value: Option<f64>| value.map(Velocity::new::<meter_per_second>);
    let prevailing = |share: f64| {
        let toward = window(&dry, share, band).iter().filter_map(|&(_, sample)| sample.1.wind_direction.map(heading)).sum::<Vector2<f64>>();
        (toward.norm() > 0.0).then(|| bearing(toward))
    };
    let monthly = samples.iter().into_group_map_by(|sample| sample.0.month());
    let (coldest, hottest) = monthly
        .iter()
        .filter_map(|(&month, within)| average(within.iter().copied(), dry_bulb).map(|mean| (month, mean)))
        .minmax_by(|(left_month, colder), (right_month, warmer)| colder.total_cmp(warmer).then(left_month.cmp(right_month)))
        .into_option()
        .map(|((coldest, _), (hottest, _))| (coldest, hottest))
        .unzip();
    let within = |month: Option<i8>| month.and_then(|month| monthly.get(&month)).map_or(&[][..], Vec::as_slice);
    let cold_wind = ranked(within(coldest).iter().copied(), speed);
    let range = within(hottest)
        .iter()
        .copied()
        .into_group_map_by(|sample| sample.0.date())
        .into_iter()
        .sorted_by_key(|&(date, _)| date)
        .filter_map(|(_, day)| day.into_iter().filter_map(dry_bulb).minmax_by(f64::total_cmp).into_option())
        .map(|(low, high)| (high - low, 1.0));
    let humidification = |share: f64| Humidification {
        dew_point: temperature(level(&samples, dew_point, share)),
        humidity_ratio: coincident(&dew, share, band, humidity_ratio).map(Ratio::new::<ratio>),
        dry_bulb: temperature(coincident(&dew, share, band, dry_bulb)),
    };
    let yearly = samples
        .iter()
        .into_group_map_by(|sample| sample.0.year())
        .into_iter()
        .sorted_by_key(|&(year, _)| year)
        .filter_map(|(_, year)| year.into_iter().filter_map(dry_bulb).minmax_by(f64::total_cmp).into_option())
        .collect_vec();
    let [highs, lows] = [yearly.iter().map(|&(_, high)| high).collect_vec(), yearly.iter().map(|&(low, _)| low).collect_vec()];
    let [mean_high, mean_low] = [&highs, &lows].map(|values| (!values.is_empty()).then(|| values.iter().mean()));
    let [spread_high, spread_low] = [&highs, &lows].map(|values| (values.len() > 1).then(|| values.iter().std_dev()));
    let returned = |period: f64| {
        let factor = -(6.0_f64.sqrt() / PI) * (EULER_MASCHERONI + (period / (period - 1.0)).ln().ln());
        [(mean_high, spread_high, 1.0), (mean_low, spread_low, -1.0)].map(|(mean, spread, direction): (Option<f64>, Option<f64>, f64)| {
            mean.zip(spread).map(|(mean, spread)| ThermodynamicTemperature::new::<kelvin>((direction * factor).mul_add(spread, mean)))
        })
    };
    Ok(Design {
        source,
        edition: Edition::Ashrae2021 { shielding: None },
        heating: Heating {
            month: coldest,
            dry_bulb: HEATING.map(|share| temperature(level(&samples, dry_bulb, share))),
            humidification: HEATING.map(humidification),
            wind: HEATING.map(|share| 1.0 - share).map(|share| (velocity(level(within(coldest), |sample| speed(sample), share)), temperature(coincident(&cold_wind, share, band, dry_bulb)))),
            coincident: (velocity(coincident(&dry, HEATING[0], band, speed)), prevailing(HEATING[0])),
        },
        cooling: Cooling {
            month: hottest,
            range: weighted(range).map(TemperatureInterval::new::<temperature_interval::kelvin>),
            dry_bulb: COOLING.map(|share| (temperature(level(&samples, dry_bulb, share)), temperature(coincident(&dry, share, band, wet_bulb)))),
            wet_bulb: COOLING.map(|share| (temperature(level(&samples, wet_bulb, share)), temperature(coincident(&wet, share, band, dry_bulb)))),
            coincident: (velocity(coincident(&dry, COOLING[0], band, speed)), prevailing(COOLING[0])),
            dehumidification: COOLING.map(humidification),
            enthalpy: COOLING.map(|share| (level(&samples, enthalpy, share).map(AvailableEnergy::new::<joule_per_kilogram>), temperature(coincident(&energy, share, band, dry_bulb)))),
        },
        extremes: Extremes {
            wind: WIND.map(|share| velocity(level(&samples, speed, share))),
            wet_bulb: temperature(samples.iter().filter_map(wet_bulb).reduce(f64::max)),
            dry_bulb: Annual {
                mean: [temperature(mean_high), temperature(mean_low)],
                deviation: [spread_high, spread_low].map(|spread| spread.map(TemperatureInterval::new::<temperature_interval::kelvin>)),
                returns: RETURNS.map(returned),
            },
        },
    })
}

pub(crate) fn decode(record: &StringRecord) -> Result<Option<Design>, csv::Error> {
    let record: StringRecord = record.iter().map(|field| if matches!(field, "N/A" | "N_A") { "" } else { field }).collect();
    match (record.deserialize::<Option<u8>>(None)?, record.len(), record.get(19) == Some("Cooling"), record.get(1).is_some_and(|source| source.contains("2017"))) {
        (None | Some(0), ..) => Ok(None),
        (Some(_), 69, true, false) => record.deserialize::<Ashrae2009<'_>>(None).map(
            |Ashrae2009 { count: _, source, blank: _, heating_label: _, heating, cooling_label: _, cooling, hours, extremes_label: _, wind, wet_bulb, dry_bulb }| {
                Some(Design { source: source.to_owned(), edition: Edition::Ashrae2009 { hours }, heating, cooling, extremes: Extremes { wind, wet_bulb, dry_bulb } })
            },
        ),
        (Some(_), 68, ..) | (Some(_), 69, true, true) => record.deserialize::<Ashrae2017<'_>>(None).map(
            |Ashrae2017 { count: _, source, blank: _, heating_label: _, heating, cooling_label: _, cooling, wet_bulb, extremes_label: _, wind, dry_bulb, minimum_wet_bulb }| {
                Some(Design { source: source.to_owned(), edition: Edition::Ashrae2017 { minimum_wet_bulb }, heating, cooling, extremes: Extremes { wind, wet_bulb, dry_bulb } })
            },
        ),
        (Some(_), 69, ..) => record.deserialize::<Ashrae2021<'_>>(None).map(
            |Ashrae2021 { count: _, source, blank: _, heating_label: _, heating, shielding, cooling_label: _, cooling, wet_bulb, extremes_label: _, wind, dry_bulb }| {
                Some(Design { source: source.to_owned(), edition: Edition::Ashrae2021 { shielding }, heating, cooling, extremes: Extremes { wind, wet_bulb, dry_bulb } })
            },
        ),
        (Some(_), ..) => record.deserialize::<Ashrae2025<'_>>(None).map(
            |Ashrae2025 { count: _, source, heating_label: _, heating, shielding, cooling_label: _, cooling, wet_bulb, extremes_label: _, wind, dry_bulb, annual_wet_bulb }| {
                Some(Design { source: source.to_owned(), edition: Edition::Ashrae2025 { shielding, wet_bulb: annual_wet_bulb }, heating, cooling, extremes: Extremes { wind, wet_bulb, dry_bulb } })
            },
        ),
    }
}

pub(crate) fn encode(design: Option<&Design>) -> impl Serialize {
    let Some(Design { source, edition, heating, cooling, extremes: Extremes { wind, wet_bulb, dry_bulb } }) = design else {
        return Layout::Absent;
    };
    let (source, heating, cooling, wind, wet_bulb, dry_bulb) = (source.as_str(), *heating, *cooling, *wind, *wet_bulb, *dry_bulb);
    let (count, blank, heating_label, cooling_label, extremes_label) = (1, "", Heading::Heating, Heading::Cooling, Heading::Extremes);
    match *edition {
        Edition::Ashrae2009 { hours } => Layout::Ashrae2009(Ashrae2009 { count, source, blank, heating_label, heating, cooling_label, cooling, hours, extremes_label, wind, wet_bulb, dry_bulb }),
        Edition::Ashrae2017 { minimum_wet_bulb } =>
            Layout::Ashrae2017(Ashrae2017 { count, source, blank, heating_label, heating, cooling_label, cooling, wet_bulb, extremes_label, wind, dry_bulb, minimum_wet_bulb }),
        Edition::Ashrae2021 { shielding } =>
            Layout::Ashrae2021(Ashrae2021 { count, source, blank, heating_label, heating, shielding, cooling_label, cooling, wet_bulb, extremes_label, wind, dry_bulb }),
        Edition::Ashrae2025 { shielding, wet_bulb: annual_wet_bulb } =>
            Layout::Ashrae2025(Ashrae2025 { count, source, heating_label, heating, shielding, cooling_label, cooling, wet_bulb, extremes_label, wind, dry_bulb, annual_wet_bulb }),
    }
}
