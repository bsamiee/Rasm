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

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Deserialize, Serialize)]
enum Heading {
    Heating,
    Cooling,
    Extremes,
}

type HeatingColumns = (i8, [f64; 2], [[f64; 3]; 2], [[f64; 2]; 2], [f64; 2]);
type CoolingColumns = (i8, f64, [[f64; 2]; 3], [[f64; 2]; 3], [f64; 2], [[f64; 3]; 3], [[f64; 2]; 3]);
type ExtremeColumns = ([f64; 2], [f64; 2], [[f64; 2]; 4]);
type Ashrae2009<S> = (u8, S, S, Heading, HeatingColumns, Heading, CoolingColumns, f64, Heading, [f64; 3], f64, ExtremeColumns);
type Ashrae2021<S> = (u8, S, S, Heading, HeatingColumns, f64, Heading, CoolingColumns, f64, Heading, [f64; 3], ExtremeColumns);

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
    Ashrae2009 { hours: Time },
    Ashrae2021 { shielding: Frequency },
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Humidification {
    pub dew_point: ThermodynamicTemperature,
    pub humidity_ratio: Ratio,
    pub dry_bulb: ThermodynamicTemperature,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Heating {
    pub month: i8,
    pub dry_bulb: [ThermodynamicTemperature; 2],
    pub humidification: [Humidification; 2],
    pub wind: [(Velocity, ThermodynamicTemperature); 2],
    pub coincident: (Velocity, Angle),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Cooling {
    pub month: i8,
    pub range: TemperatureInterval,
    pub dry_bulb: [(ThermodynamicTemperature, ThermodynamicTemperature); 3],
    pub wet_bulb: [(ThermodynamicTemperature, ThermodynamicTemperature); 3],
    pub coincident: (Velocity, Angle),
    pub dehumidification: [Humidification; 3],
    pub enthalpy: [(AvailableEnergy, ThermodynamicTemperature); 3],
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Extremes {
    pub wind: [Velocity; 3],
    pub wet_bulb: ThermodynamicTemperature,
    pub mean: [ThermodynamicTemperature; 2],
    pub deviation: [TemperatureInterval; 2],
    pub returns: [[ThermodynamicTemperature; 2]; 4],
}

#[derive(Serialize)]
enum Conditions<'design> {
    #[serde(rename = "0")]
    Absent,
    Ashrae2009(Ashrae2009<&'design str>),
    Ashrae2021(Ashrae2021<&'design str>),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn decode(fields: &[String]) -> Result<Option<Design>, csv::Error> {
    let record = StringRecord::from(fields);
    let celsius = ThermodynamicTemperature::new::<degree_celsius>;
    let interval = TemperatureInterval::new::<temperature_interval::degree_celsius>;
    let velocity = Velocity::new::<meter_per_second>;
    let humidification = |[dew_point, humidity_ratio, dry_bulb]: [f64; 3]| Humidification {
        dew_point: celsius(dew_point),
        humidity_ratio: Ratio::new::<part_per_thousand>(humidity_ratio),
        dry_bulb: celsius(dry_bulb),
    };
    let coincident = |[speed, direction]: [f64; 2]| (velocity(speed), Angle::new::<degree>(direction));
    let paired = |[first, second]: [f64; 2]| (celsius(first), celsius(second));
    match (record.deserialize::<u8>(None)?, fields) {
        (0, _) => Ok(None),
        (_, [_, edition, ..]) if edition.contains("2009") => record.deserialize::<Ashrae2009<String>>(None).map(|(_, source, _, _, heating, _, cooling, hours, _, wind, wet_bulb, extremes)| {
            Some((source, Edition::Ashrae2009 { hours: Time::new::<hour>(hours) }, heating, cooling, wind, wet_bulb, extremes))
        }),
        _ => record.deserialize::<Ashrae2021<String>>(None).map(|(_, source, _, _, heating, shielding, _, cooling, wet_bulb, _, wind, extremes)| {
            Some((source, Edition::Ashrae2021 { shielding: Frequency::new::<cycle_per_hour>(shielding) }, heating, cooling, wind, wet_bulb, extremes))
        }),
    }
    .map(|parts| {
        parts.map(
            |(
                source,
                edition,
                (heating_month, heating_dry_bulb, heating_humidification, heating_wind, heating_coincident),
                (cooling_month, range, cooling_dry_bulb, cooling_wet_bulb, cooling_coincident, dehumidification, enthalpy),
                extreme_wind,
                extreme_wet_bulb,
                (mean, deviation, returns),
            )| Design {
                source,
                edition,
                heating: Heating {
                    month: heating_month,
                    dry_bulb: heating_dry_bulb.map(celsius),
                    humidification: heating_humidification.map(humidification),
                    wind: heating_wind.map(|[speed, dry_bulb]| (velocity(speed), celsius(dry_bulb))),
                    coincident: coincident(heating_coincident),
                },
                cooling: Cooling {
                    month: cooling_month,
                    range: interval(range),
                    dry_bulb: cooling_dry_bulb.map(paired),
                    wet_bulb: cooling_wet_bulb.map(paired),
                    coincident: coincident(cooling_coincident),
                    dehumidification: dehumidification.map(humidification),
                    enthalpy: enthalpy.map(|[energy, dry_bulb]| (AvailableEnergy::new::<kilojoule_per_kilogram>(energy), celsius(dry_bulb))),
                },
                extremes: Extremes {
                    wind: extreme_wind.map(velocity),
                    wet_bulb: celsius(extreme_wet_bulb),
                    mean: mean.map(celsius),
                    deviation: deviation.map(interval),
                    returns: returns.map(|pair| pair.map(celsius)),
                },
            },
        )
    })
}

pub(crate) fn encode(design: Option<&Design>) -> impl Serialize {
    let celsius = |temperature: ThermodynamicTemperature| temperature.get::<degree_celsius>();
    let interval = |difference: TemperatureInterval| difference.get::<temperature_interval::degree_celsius>();
    let speed = |velocity: Velocity| velocity.get::<meter_per_second>();
    let humidification = |Humidification { dew_point, humidity_ratio, dry_bulb }: Humidification| {
        [celsius(dew_point), humidity_ratio.get::<part_per_thousand>(), celsius(dry_bulb)]
    };
    let coincident = |(velocity, direction): (Velocity, Angle)| [speed(velocity), direction.get::<degree>()];
    let paired = |(first, second): (ThermodynamicTemperature, ThermodynamicTemperature)| [celsius(first), celsius(second)];
    design.map_or(Conditions::Absent, |Design { source, edition, heating, cooling, extremes }| {
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
            cooling.enthalpy.map(|(energy, dry_bulb)| [energy.get::<kilojoule_per_kilogram>(), celsius(dry_bulb)]),
        );
        let wind = extremes.wind.map(speed);
        let wet_bulb = celsius(extremes.wet_bulb);
        let extreme_columns = (extremes.mean.map(celsius), extremes.deviation.map(interval), extremes.returns.map(|pair| pair.map(celsius)));
        match *edition {
            Edition::Ashrae2009 { hours } => Conditions::Ashrae2009((
                1,
                source.as_str(),
                "",
                Heading::Heating,
                heating_columns,
                Heading::Cooling,
                cooling_columns,
                hours.get::<hour>(),
                Heading::Extremes,
                wind,
                wet_bulb,
                extreme_columns,
            )),
            Edition::Ashrae2021 { shielding } => Conditions::Ashrae2021((
                1,
                source.as_str(),
                "",
                Heading::Heating,
                heating_columns,
                shielding.get::<cycle_per_hour>(),
                Heading::Cooling,
                cooling_columns,
                wet_bulb,
                Heading::Extremes,
                wind,
                extreme_columns,
            )),
        }
    })
}
