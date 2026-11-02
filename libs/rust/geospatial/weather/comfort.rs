use core::range::Range;

use itertools::Either;
use jiff::Timestamp;
use solar_positioning::SolarPosition;
use thermalcomfort::{
    ClothingInsulation, Humidity, MetabolicRate, PmvPpdResult, Speed, Temperature,
    models::{
        adaptive::{AdaptiveAshraeResult, AdaptiveEnResult, AdaptiveInputs, AdaptiveOptions, adaptive_ashrae, adaptive_en},
        pmv::{PmvPpdInputs, PmvPpdIsoOptions},
        solar_gain::{SolarGainInputs, SolarGainOptions, solar_gain},
        utci::{self, StressCategory, UtciInputs, UtciOptions, UtciResult},
    },
    pmv_ppd_iso,
    psychrometrics::{PsyTaRhOptions, PsychrometricValues, psy_ta_rh},
    v_relative,
};
use uom::si::{
    angle::degree,
    available_energy::joule_per_kilogram,
    f64::{Angle, AvailableEnergy, HeatFluxDensity, Pressure, Ratio, TemperatureInterval, ThermodynamicTemperature, Velocity},
    heat_flux_density::watt_per_square_meter,
    pressure::pascal,
    ratio::ratio,
    temperature_interval,
    thermodynamic_temperature::{degree_celsius, kelvin},
    velocity::meter_per_second,
};

use crate::{Hours, WeatherError, position};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Standard {
    Ashrae,
    En,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Conditions {
    pub air: ThermodynamicTemperature,
    pub radiant: ThermodynamicTemperature,
    pub speed: Velocity,
    pub humidity: Ratio,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Exposure {
    pub facing: Angle,
    pub transmittance: Ratio,
    pub sky_view: Ratio,
    pub gain: SolarGainOptions,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Psychrometrics {
    pub saturation: Pressure,
    pub vapour: Pressure,
    pub humidity_ratio: Ratio,
    pub wet_bulb: ThermodynamicTemperature,
    pub dew_point: ThermodynamicTemperature,
    pub enthalpy: AvailableEnergy,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn psychrometrics(air: ThermodynamicTemperature, humidity: Ratio, pressure: Pressure) -> Option<Psychrometrics> {
    let PsychrometricValues { p_sat, p_vap, hr, t_wb, t_dp, h } =
        psy_ta_rh(temperature(air), relative_humidity(humidity), PsyTaRhOptions { p_atm: thermalcomfort::Pressure::from_pascals(pressure.get::<pascal>()) });
    Some(Psychrometrics {
        saturation: Pressure::new::<pascal>(p_sat.as_pascals()),
        vapour: Pressure::new::<pascal>(p_vap.as_pascals()),
        humidity_ratio: Ratio::new::<ratio>(hr),
        wet_bulb: ThermodynamicTemperature::new::<kelvin>(t_wb.as_kelvin()),
        dew_point: ThermodynamicTemperature::new::<kelvin>(t_dp.as_kelvin()),
        enthalpy: AvailableEnergy::new::<joule_per_kilogram>(h),
    })
    .filter(|&Psychrometrics { saturation, vapour, humidity_ratio, wet_bulb, dew_point, enthalpy }| {
        [saturation.value, vapour.value, humidity_ratio.value, wet_bulb.value, dew_point.value, enthalpy.value].into_iter().all(f64::is_finite)
    })
}

pub fn outdoor(hours: &Hours, exposure: Exposure, sunlit: impl Fn(SolarPosition) -> Ratio) -> Result<Vec<(Range<Timestamp>, Option<Conditions>)>, WeatherError> {
    let delta_mrt = |sun: SolarPosition, direct: HeatFluxDensity| {
        let (sine, cosine) = (Angle::new::<degree>(sun.azimuth()) - exposure.facing).sin_cos();
        let inputs = SolarGainInputs {
            sol_altitude: thermalcomfort::Angle::from_degrees(sun.elevation_angle()),
            sharp: thermalcomfort::Angle::from_degrees(sine.abs().atan2(cosine).get::<degree>()),
            sol_radiation_dir: thermalcomfort::HeatFluxDensity::from_watts_per_square_meter(direct.get::<watt_per_square_meter>()),
            sol_transmittance: exposure.transmittance.get::<ratio>(),
            f_svv: exposure.sky_view.get::<ratio>(),
            f_bes: sunlit(sun).get::<ratio>(),
        };
        Some(solar_gain(inputs, exposure.gain).delta_mrt.as_celsius()).filter(|delta| delta.is_finite()).map(TemperatureInterval::new::<temperature_interval::degree_celsius>)
    };
    hours
        .hours
        .iter()
        .map(|hour| {
            position(hours, hour).map(|(_, sun)| {
                let conditions = match (hour.dry_bulb, hour.relative_humidity, hour.wind_speed, sun.is_sun_up(), hour.direct_irradiance) {
                    (Some(air), Some(humidity), Some(speed), false, _) => Some(Conditions { air, radiant: air, speed, humidity }),
                    (Some(air), Some(humidity), Some(speed), true, Some(direct)) => delta_mrt(sun, direct).map(|delta| Conditions { air, radiant: air + delta, speed, humidity }),
                    (None, ..) | (_, None, ..) | (_, _, None, ..) | (.., true, None) => None,
                };
                (hour.interval, conditions)
            })
        })
        .collect()
}

#[must_use]
pub fn utci(conditions: Conditions) -> Option<(ThermodynamicTemperature, StressCategory)> {
    let (tdb, tr, speed, rh) = environment(conditions);
    let UtciResult { utci, stress_category } = utci::utci(UtciInputs { tdb, tr, v: speed, rh }, UtciOptions { round_output: false, ..UtciOptions::default() });
    stress_category.map(|category| (ThermodynamicTemperature::new::<degree_celsius>(utci), category))
}

#[must_use]
pub fn pmv(conditions: Conditions, met: MetabolicRate, clo: ClothingInsulation, options: PmvPpdIsoOptions) -> Option<PmvPpdResult> {
    let (tdb, tr, speed, rh) = environment(conditions);
    Some(pmv_ppd_iso(PmvPpdInputs { tdb, tr, vr: v_relative(speed, met), rh, met, clo }, options)).filter(|result| result.pmv.is_finite())
}

#[must_use]
pub fn adaptive(conditions: Conditions, running_mean: ThermodynamicTemperature, standard: Standard) -> Option<Either<AdaptiveAshraeResult, AdaptiveEnResult>> {
    let (tdb, tr, speed, _) = environment(conditions);
    let inputs = AdaptiveInputs { tdb, tr, t_running_mean: temperature(running_mean), v: speed };
    let options = AdaptiveOptions { round_output: false, ..AdaptiveOptions::default() };
    Some(match standard {
        Standard::Ashrae => Either::Left(adaptive_ashrae(inputs, options)),
        Standard::En => Either::Right(adaptive_en(inputs, options)),
    })
    .filter(|result| result.as_ref().either(|ashrae| ashrae.tmp_cmf, |en| en.tmp_cmf).as_kelvin().is_finite())
}

fn environment(conditions: Conditions) -> (Temperature, Temperature, Speed, Humidity) {
    let Conditions { air, radiant, speed, humidity } = conditions;
    (temperature(air), temperature(radiant), Speed::from_meters_per_second(speed.get::<meter_per_second>()), relative_humidity(humidity))
}

fn temperature(value: ThermodynamicTemperature) -> Temperature { Temperature::from_kelvin(value.get::<kelvin>()) }

fn relative_humidity(value: Ratio) -> Humidity { Humidity::from_ratio(value.get::<ratio>()) }
