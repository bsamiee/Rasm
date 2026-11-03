use csv::StringRecord;
use serde::{Deserialize, Serialize};
use uom::si::{
    angle::degree,
    available_energy::kilojoule_per_kilogram,
    f64::{Angle, AvailableEnergy, Frequency, Ratio, TemperatureInterval, ThermodynamicTemperature, Time, Velocity},
    frequency::cycle_per_hour,
    ratio::part_per_thousand,
    temperature_interval,
    thermodynamic_temperature::degree_celsius,
    time::hour,
    velocity::meter_per_second,
};

use crate::written;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Deserialize, Serialize)]
enum Heading {
    Heating,
    Cooling,
    Extremes,
}

type Column = Option<f64>;
type HeatingColumns = (Option<i8>, [Column; 2], [[Column; 3]; 2], [[Column; 2]; 2], [Column; 2]);
type CoolingColumns = (Option<i8>, Column, [[Column; 2]; 3], [[Column; 2]; 3], [Column; 2], [[Column; 3]; 3], [[Column; 2]; 3]);
type AnnualColumns = ([Column; 2], [Column; 2], [[Column; 2]; 4]);
type Opening<'source> = (u8, &'source str, &'source str, Heading, HeatingColumns);
type Central = (Heading, CoolingColumns, Column, Heading, [Column; 3]);
type Ashrae2009<'source> = (Opening<'source>, Central, Column, AnnualColumns);
type Ashrae2021<'source> = (Opening<'source>, Column, Central, AnnualColumns);
type Ashrae2025<'source> = (u8, &'source str, Heading, HeatingColumns, Column, Central, AnnualColumns, AnnualColumns);

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

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Humidification {
    pub dew_point: Option<ThermodynamicTemperature>,
    pub humidity_ratio: Option<Ratio>,
    pub dry_bulb: Option<ThermodynamicTemperature>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Heating {
    pub month: Option<i8>,
    pub dry_bulb: [Option<ThermodynamicTemperature>; 2],
    pub humidification: [Humidification; 2],
    pub wind: [(Option<Velocity>, Option<ThermodynamicTemperature>); 2],
    pub coincident: (Option<Velocity>, Option<Angle>),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Cooling {
    pub month: Option<i8>,
    pub range: Option<TemperatureInterval>,
    pub dry_bulb: [(Option<ThermodynamicTemperature>, Option<ThermodynamicTemperature>); 3],
    pub wet_bulb: [(Option<ThermodynamicTemperature>, Option<ThermodynamicTemperature>); 3],
    pub coincident: (Option<Velocity>, Option<Angle>),
    pub dehumidification: [Humidification; 3],
    pub enthalpy: [(Option<AvailableEnergy>, Option<ThermodynamicTemperature>); 3],
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Annual {
    pub mean: [Option<ThermodynamicTemperature>; 2],
    pub deviation: [Option<TemperatureInterval>; 2],
    pub returns: [[Option<ThermodynamicTemperature>; 2]; 4],
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Extremes {
    pub wind: [Option<Velocity>; 3],
    pub wet_bulb: Option<ThermodynamicTemperature>,
    pub dry_bulb: Annual,
}

#[derive(Deserialize, Serialize)]
struct Ashrae2017<'source>(#[serde(borrow)] Opening<'source>, Central, AnnualColumns, #[serde(default, skip_serializing_if = "Option::is_none")] Column);

#[derive(Serialize)]
enum Conditions<'source> {
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

pub(crate) fn decode(fields: &[String]) -> Result<Option<Design>, csv::Error> {
    let record: StringRecord = fields.iter().map(|field| if matches!(field.as_str(), "N/A" | "N_A") { "" } else { field.as_str() }).collect();
    let celsius = |value: Column| value.map(ThermodynamicTemperature::new::<degree_celsius>);
    let interval = |value: Column| value.map(TemperatureInterval::new::<temperature_interval::degree_celsius>);
    let velocity = |value: Column| value.map(Velocity::new::<meter_per_second>);
    let factor = |value: Column| value.map(Frequency::new::<cycle_per_hour>);
    let humidification = |[dew_point, humidity_ratio, dry_bulb]: [Column; 3]| Humidification {
        dew_point: celsius(dew_point),
        humidity_ratio: humidity_ratio.map(Ratio::new::<part_per_thousand>),
        dry_bulb: celsius(dry_bulb),
    };
    let coincident = |[speed, direction]: [Column; 2]| (velocity(speed), direction.map(Angle::new::<degree>));
    let paired = |[first, second]: [Column; 2]| (celsius(first), celsius(second));
    let annual = |(mean, deviation, returns): AnnualColumns| Annual { mean: mean.map(celsius), deviation: deviation.map(interval), returns: returns.map(|pair| pair.map(celsius)) };
    let design = |source: &str,
                  edition: Edition,
                  (heating_month, heating_dry_bulb, heating_humidification, heating_wind, heating_coincident): HeatingColumns,
                  (cooling_month, range, cooling_dry_bulb, cooling_wet_bulb, cooling_coincident, dehumidification, enthalpy): CoolingColumns,
                  wind: [Column; 3],
                  wet_bulb: Column,
                  dry_bulb: AnnualColumns| {
        Design {
            source: source.to_owned(),
            edition,
            heating: Heating {
                month: heating_month,
                dry_bulb: heating_dry_bulb.map(celsius),
                humidification: heating_humidification.map(humidification),
                wind: heating_wind.map(|[speed, coincident_dry_bulb]| (velocity(speed), celsius(coincident_dry_bulb))),
                coincident: coincident(heating_coincident),
            },
            cooling: Cooling {
                month: cooling_month,
                range: interval(range),
                dry_bulb: cooling_dry_bulb.map(paired),
                wet_bulb: cooling_wet_bulb.map(paired),
                coincident: coincident(cooling_coincident),
                dehumidification: dehumidification.map(humidification),
                enthalpy: enthalpy.map(|[energy, coincident_dry_bulb]| (energy.map(AvailableEnergy::new::<kilojoule_per_kilogram>), celsius(coincident_dry_bulb))),
            },
            extremes: Extremes { wind: wind.map(velocity), wet_bulb: celsius(wet_bulb), dry_bulb: annual(dry_bulb) },
        }
    };
    match (record.deserialize::<Option<u8>>(None)?, record.len(), record.get(19) == Some("Cooling"), record.get(1).is_some_and(|source| source.contains("2017"))) {
        (None | Some(0), ..) => Ok(None),
        (Some(_), 69, true, false) => record.deserialize::<Ashrae2009<'_>>(None).map(|((_, source, _, _, heating), (_, cooling, hours, _, wind), wet_bulb, dry_bulb)| {
            Some(design(source, Edition::Ashrae2009 { hours: hours.map(Time::new::<hour>) }, heating, cooling, wind, wet_bulb, dry_bulb))
        }),
        (Some(_), 68, ..) | (Some(_), 69, true, true) =>
            record.deserialize::<Ashrae2017<'_>>(None).map(|Ashrae2017((_, source, _, _, heating), (_, cooling, wet_bulb, _, wind), dry_bulb, minimum_wet_bulb)| {
                Some(design(source, Edition::Ashrae2017 { minimum_wet_bulb: celsius(minimum_wet_bulb) }, heating, cooling, wind, wet_bulb, dry_bulb))
            }),
        (Some(_), 69, ..) => record.deserialize::<Ashrae2021<'_>>(None).map(|((_, source, _, _, heating), shielding, (_, cooling, wet_bulb, _, wind), dry_bulb)| {
            Some(design(source, Edition::Ashrae2021 { shielding: factor(shielding) }, heating, cooling, wind, wet_bulb, dry_bulb))
        }),
        (Some(_), ..) => record.deserialize::<Ashrae2025<'_>>(None).map(|(_, source, _, heating, shielding, (_, cooling, wet_bulb, _, wind), dry_bulb, annual_wet_bulb)| {
            Some(design(source, Edition::Ashrae2025 { shielding: factor(shielding), wet_bulb: annual(annual_wet_bulb) }, heating, cooling, wind, wet_bulb, dry_bulb))
        }),
    }
}

pub(crate) fn encode(design: Option<&Design>) -> impl Serialize {
    let celsius = |temperature: Option<ThermodynamicTemperature>| temperature.map(|stated| written(stated.get::<degree_celsius>()));
    let interval = |difference: Option<TemperatureInterval>| difference.map(|stated| written(stated.get::<temperature_interval::degree_celsius>()));
    let speed = |velocity: Option<Velocity>| velocity.map(|stated| written(stated.get::<meter_per_second>()));
    let factor = |shielding: Option<Frequency>| shielding.map(|stated| written(stated.get::<cycle_per_hour>()));
    let humidification =
        |Humidification { dew_point, humidity_ratio, dry_bulb }: Humidification| [celsius(dew_point), humidity_ratio.map(|stated| written(stated.get::<part_per_thousand>())), celsius(dry_bulb)];
    let coincident = |(velocity, direction): (Option<Velocity>, Option<Angle>)| [speed(velocity), direction.map(|stated| written(stated.get::<degree>()))];
    let paired = |(first, second): (Option<ThermodynamicTemperature>, Option<ThermodynamicTemperature>)| [celsius(first), celsius(second)];
    let annual = |Annual { mean, deviation, returns }: Annual| (mean.map(celsius), deviation.map(interval), returns.map(|pair| pair.map(celsius)));
    let Some(Design { source, edition, heating, cooling, extremes }) = design else {
        return Conditions::Absent;
    };
    let heating_columns = (
        heating.month,
        heating.dry_bulb.map(celsius),
        heating.humidification.map(humidification),
        heating.wind.map(|(velocity, dry_bulb)| [speed(velocity), celsius(dry_bulb)]),
        coincident(heating.coincident),
    );
    let cooling_columns = (
        cooling.month,
        interval(cooling.range),
        cooling.dry_bulb.map(paired),
        cooling.wet_bulb.map(paired),
        coincident(cooling.coincident),
        cooling.dehumidification.map(humidification),
        cooling.enthalpy.map(|(energy, dry_bulb)| [energy.map(|stated| written(stated.get::<kilojoule_per_kilogram>())), celsius(dry_bulb)]),
    );
    let wind = extremes.wind.map(speed);
    let wet_bulb = celsius(extremes.wet_bulb);
    let dry_bulb = annual(extremes.dry_bulb);
    let opening = (1, source.as_str(), "", Heading::Heating, heating_columns);
    let central = |last: Column| (Heading::Cooling, cooling_columns, last, Heading::Extremes, wind);
    match *edition {
        Edition::Ashrae2009 { hours } => Conditions::Ashrae2009((opening, central(hours.map(|stated| written(stated.get::<hour>()))), wet_bulb, dry_bulb)),
        Edition::Ashrae2017 { minimum_wet_bulb } => Conditions::Ashrae2017(Ashrae2017(opening, central(wet_bulb), dry_bulb, celsius(minimum_wet_bulb))),
        Edition::Ashrae2021 { shielding } => Conditions::Ashrae2021((opening, factor(shielding), central(wet_bulb), dry_bulb)),
        Edition::Ashrae2025 { shielding, wet_bulb: annual_wet_bulb } =>
            Conditions::Ashrae2025((1, source.as_str(), Heading::Heating, heating_columns, factor(shielding), central(wet_bulb), dry_bulb, annual(annual_wet_bulb))),
    }
}
