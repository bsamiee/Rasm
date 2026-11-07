use core::{num::NonZero, range::RangeInclusive};

use itertools::Either;
use rasm_numerical_analysis::roots;
use rasm_stats::{Bounds, BoundsError};
use solar_positioning::SolarPosition;
use thermalcomfort::{
    ClothingInsulation, MetabolicRate, PmvPpdResult, Speed, Temperature,
    clothing::{CloDynamicAshraeOptions, CloDynamicIsoOptions, clo_dynamic_ashrae, clo_dynamic_iso},
    constants::{R_AIR, STEFAN_BOLTZMANN},
    environment::{OperativeTemperatureInputs, OperativeTemperatureOptions, operative_temperature},
    models::{
        AtOptions, HeatIndexRothfuszOptions, HumidexOptions, PetInputs, PetOptions, SetInputs, SetOptions, WindChillTemperatureOptions,
        adaptive::{AdaptiveAshraeResult, AdaptiveEnResult, AdaptiveInputs, AdaptiveOptions, adaptive_ashrae, adaptive_en},
        at, heat_index_rothfusz, humidex, pet_steady,
        pmv::{PmvPpdAshraeOptions, PmvPpdInputs, PmvPpdIsoOptions, pmv_ppd_ashrae},
        set_tmp,
        solar_gain::{SolarGainInputs, SolarGainOptions, solar_gain},
        utci::{self, StressCategory, UtciInputs, UtciOptions, UtciResult},
        wind_chill_temperature,
    },
    pmv_ppd_iso,
    psychrometrics::{PsyTaRhOptions, PsychrometricValues, p_sat, psy_ta_rh},
    v_relative,
};
use uom::si::{
    angle::{degree, radian},
    available_energy::joule_per_kilogram,
    f64::{Angle, AvailableEnergy, Pressure, Ratio, SpecificVolume, ThermodynamicTemperature, Velocity},
    heat_flux_density::watt_per_square_meter,
    pressure::pascal,
    ratio::ratio,
    specific_volume::cubic_meter_per_kilogram,
    thermodynamic_temperature::{degree_celsius, kelvin},
    velocity::meter_per_second,
};

use crate::{Hour, Hours, Refusal, WeatherError, atmospheric, cos_zenith, humidity, position, positive, quantity, refused, temperature};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Standard {
    Ashrae,
    En,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOWEST: f64 = -100.0;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Conditions {
    pub dry_bulb: ThermodynamicTemperature,
    pub mean_radiant: ThermodynamicTemperature,
    pub air_speed: Velocity,
    pub relative_humidity: Ratio,
    pub pressure: Option<Pressure>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Exposure {
    pub facing: Angle,
    pub transmittance: Ratio,
    pub sky_view: Ratio,
    pub sunlit: Ratio,
    pub surfaces: Option<ThermodynamicTemperature>,
    pub options: SolarGainOptions,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Psychrometrics {
    pub saturation_pressure: Pressure,
    pub vapour_pressure: Pressure,
    pub humidity_ratio: Ratio,
    pub wet_bulb: ThermodynamicTemperature,
    pub dew_point: Option<ThermodynamicTemperature>,
    pub enthalpy: AvailableEnergy,
    pub specific_volume: SpecificVolume,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Limit {
    pub operative: RangeInclusive<ThermodynamicTemperature>,
    pub acceptable: bool,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Limits {
    Ashrae { acceptability_80: Limit, acceptability_90: Limit },
    En { category_i: Limit, category_ii: Limit, category_iii: Limit },
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Adaptive {
    pub comfort: ThermodynamicTemperature,
    pub operative: ThermodynamicTemperature,
    pub limits: Limits,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Index {
    Set { met: MetabolicRate, clo: ClothingInsulation, options: SetOptions },
    Pet { met: MetabolicRate, clo: ClothingInsulation, options: PetOptions },
    Heat(HeatIndexRothfuszOptions),
    Humidex(HumidexOptions),
    Apparent(AtOptions),
    WindChill(WindChillTemperatureOptions),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PSYCHROMETRICS]

pub fn psychrometrics(dry_bulb: ThermodynamicTemperature, relative_humidity: Ratio, pressure: Pressure) -> Result<Option<Psychrometrics>, WeatherError> {
    const DRY_AIR_HEAT: f64 = 1.006;
    const VAPOUR_HEAT: f64 = 1.86;
    const VOLUME_PER_HUMIDITY_RATIO: f64 = 1.607_858;
    let (tdb, options) = (temperature(dry_bulb), PsyTaRhOptions { p_atm: atmospheric(pressure) });
    let PsychrometricValues { p_sat: saturation_pressure, p_vap: vapour_pressure, hr, t_wb: _, t_dp: _, h: enthalpy } = psy_ta_rh(tdb, humidity(relative_humidity), options);
    let air = tdb.as_celsius();
    let inside = air > LOWEST && positive(pressure) && (0.0..=1.0).contains(&relative_humidity.get::<ratio>()) && (0.0..f64::INFINITY).contains(&hr);
    refused((!inside).then_some(Refusal::Psychrometric { dry_bulb, relative_humidity, pressure }))?;
    let balance = |wet_bulb: f64| {
        let (latent, recovered, condensate): (f64, f64, f64) = if wet_bulb >= 0.0 { (2501.0, 2.326, 4.186) } else { (2830.0, 0.24, 2.1) };
        let saturated = psy_ta_rh(Temperature::from_celsius(wet_bulb), thermalcomfort::Humidity::from_ratio(1.0), options).hr;
        DRY_AIR_HEAT.mul_add(wet_bulb - air, recovered.mul_add(-wet_bulb, latent) * saturated) / condensate.mul_add(-wet_bulb, VAPOUR_HEAT.mul_add(air, latent)) - hr
    };
    let dew_point = dew_point(dry_bulb, relative_humidity)?;
    Ok(root(balance, dew_point.map_or(LOWEST, |dew| dew.get::<degree_celsius>()), air)?.map(|wet_bulb| Psychrometrics {
        saturation_pressure: Pressure::new::<pascal>(saturation_pressure.as_pascals()),
        vapour_pressure: Pressure::new::<pascal>(vapour_pressure.as_pascals()),
        humidity_ratio: Ratio::new::<ratio>(hr),
        wet_bulb: ThermodynamicTemperature::new::<degree_celsius>(wet_bulb),
        dew_point,
        enthalpy: AvailableEnergy::new::<joule_per_kilogram>(enthalpy),
        specific_volume: SpecificVolume::new::<cubic_meter_per_kilogram>(R_AIR * dry_bulb.get::<kelvin>() * VOLUME_PER_HUMIDITY_RATIO.mul_add(hr, 1.0) / pressure.get::<pascal>()),
    }))
}

#[must_use]
pub fn relative_humidity(dry_bulb: ThermodynamicTemperature, dew_point: ThermodynamicTemperature) -> Ratio {
    Ratio::new::<ratio>((p_sat(temperature(dew_point)).as_pascals() / p_sat(temperature(dry_bulb)).as_pascals()).min(1.0))
}

pub fn dew_point(dry_bulb: ThermodynamicTemperature, relative_humidity: Ratio) -> Result<Option<ThermodynamicTemperature>, WeatherError> {
    let vapour = relative_humidity.get::<ratio>() * p_sat(temperature(dry_bulb)).as_pascals();
    Ok(root(|dew_point| p_sat(Temperature::from_celsius(dew_point)).as_pascals() - vapour, LOWEST, dry_bulb.get::<degree_celsius>())?.map(ThermodynamicTemperature::new::<degree_celsius>))
}

fn root<F>(equation: F, start: f64, last: f64) -> Result<Option<f64>, WeatherError>
where F: Fn(f64) -> f64 + Sync {
    match Bounds::try_new(RangeInclusive { start, last }) {
        Ok(bounds) => roots::bracketed(equation, bounds, NonZero::<usize>::MIN, None, (None, None)).map(|found| found.first().copied()).map_err(WeatherError::Analysis),
        Err(BoundsError::PredicateViolated) => Ok(start.total_cmp(&last).is_eq().then_some(start)),
    }
}

// --- [OUTDOOR]

pub fn outdoor(hours: &Hours, mut exposure: impl FnMut(&Hour, SolarPosition) -> Exposure) -> impl Iterator<Item = Result<(&Hour, Option<Conditions>), WeatherError>> {
    const BODY_EMISSIVITY: f64 = 0.95;
    const RADIATIVE_COEFFICIENT: f64 = 6.0;
    let conditions = |hour: &Hour, sun: SolarPosition, Exposure { facing, transmittance, sky_view, sunlit, surfaces, options }: Exposure| -> Option<Conditions> {
        let dry_bulb = hour.dry_bulb?;
        let (surfaces, transmittance, half_sky) = (surfaces.unwrap_or(dry_bulb).get::<kelvin>(), transmittance.get::<ratio>(), 0.5 * sky_view.get::<ratio>());
        let sky = (hour.infrared?.get::<watt_per_square_meter>() / STEFAN_BOLTZMANN).powf(0.25);
        let solar = if sun.is_sun_up() {
            let (direct, diffuse) = (hour.direct_irradiance?, hour.diffuse_irradiance?);
            let (sine, cosine) = (Angle::new::<degree>(sun.azimuth()) - facing).sin_cos();
            let beam = solar_gain(
                SolarGainInputs {
                    sol_altitude: thermalcomfort::Angle::from_degrees(sun.elevation_angle()),
                    sharp: thermalcomfort::Angle::from_radians(sine.abs().atan2(cosine).get::<radian>()),
                    sol_radiation_dir: thermalcomfort::HeatFluxDensity::from_watts_per_square_meter(direct.get::<watt_per_square_meter>()),
                    sol_transmittance: transmittance,
                    f_svv: 0.0,
                    f_bes: sunlit.get::<ratio>(),
                },
                SolarGainOptions { round_output: false, ..options },
            );
            let (global, scattered) = ((direct * cos_zenith(&sun) + diffuse).get::<watt_per_square_meter>(), diffuse.get::<watt_per_square_meter>());
            beam.delta_mrt.as_celsius() + half_sky * transmittance * options.floor_reflectance.mul_add(global, scattered) * options.asw / (BODY_EMISSIVITY * RADIATIVE_COEFFICIENT)
        } else {
            0.0
        };
        Some(Conditions {
            dry_bulb,
            mean_radiant: Some(half_sky.mul_add(sky - surfaces, surfaces + solar)).filter(|radiant: &f64| radiant.is_finite()).map(ThermodynamicTemperature::new::<kelvin>)?,
            air_speed: hour.wind_speed?,
            relative_humidity: hour.relative_humidity?,
            pressure: hour.pressure,
        })
    };
    hours.records.iter().map(move |hour| position(&hours.sun, hour.interval).map(|(_, sun)| (hour, conditions(hour, sun, exposure(hour, sun)))))
}

// --- [INDICES]

#[must_use]
pub fn utci(conditions: Conditions) -> Option<(ThermodynamicTemperature, StressCategory)> {
    let (tdb, tr, speed, rh) = environment(conditions);
    let UtciResult { utci, stress_category } =
        utci::utci(UtciInputs { tdb, tr, v: Speed::from_meters_per_second(speed.as_meters_per_second().clamp(0.5, 17.0)), rh }, UtciOptions { round_output: false, ..UtciOptions::default() });
    stress_category.map(|category| (quantity(Temperature::from_celsius(utci)), category))
}

#[must_use]
pub fn pmv(conditions: Conditions, met: MetabolicRate, clo: ClothingInsulation, options: Either<PmvPpdIsoOptions, PmvPpdAshraeOptions>) -> Option<PmvPpdResult> {
    let (tdb, tr, speed, rh) = environment(conditions);
    let inputs = |dynamic: ClothingInsulation| PmvPpdInputs { tdb, tr, vr: v_relative(speed, met), rh, met, clo: dynamic };
    Some(options.either(
        |options| pmv_ppd_iso(inputs(clo_dynamic_iso(clo, met, speed, CloDynamicIsoOptions::default())), options),
        |options| pmv_ppd_ashrae(inputs(clo_dynamic_ashrae(clo, met, CloDynamicAshraeOptions::default())), options),
    ))
    .filter(|result| result.pmv.is_finite())
}

#[must_use]
pub fn equivalent(conditions: Conditions, index: Index) -> Option<ThermodynamicTemperature> {
    let (tdb, tr, speed, rh) = environment(conditions);
    let p_atm = |stated: thermalcomfort::Pressure| conditions.pressure.map_or(stated, atmospheric);
    let equivalent = match index {
        Index::Set { met, clo, options } => set_tmp(SetInputs { tdb, tr, v: speed, rh, met, clo }, SetOptions { p_atm: p_atm(options.p_atm), ..options }),
        Index::Pet { met, clo, options } => pet_steady(PetInputs { tdb, tr, v: speed, rh, met, clo }, PetOptions { p_atm: p_atm(options.p_atm), ..options }).pet,
        Index::Heat(options) => Temperature::from_celsius(heat_index_rothfusz(tdb, rh, options).hi),
        Index::Humidex(options) => Temperature::from_celsius(humidex(tdb, rh, options).humidex),
        Index::Apparent(options) => at(tdb, rh, speed, options),
        Index::WindChill(options) => wind_chill_temperature(tdb, speed, options),
    };
    equivalent.as_kelvin().is_finite().then(|| quantity(equivalent))
}

#[must_use]
pub fn adaptive(conditions: Conditions, running_mean: ThermodynamicTemperature, standard: Standard, options: AdaptiveOptions) -> Option<Adaptive> {
    let (tdb, tr, speed, _) = environment(conditions);
    let inputs = AdaptiveInputs { tdb, tr, t_running_mean: temperature(running_mean), v: speed };
    let limit = |low: Temperature, up: Temperature, acceptable: bool| Limit { operative: RangeInclusive { start: quantity(low), last: quantity(up) }, acceptable };
    let (comfort, limits, use_ashrae) = match standard {
        Standard::Ashrae => {
            let AdaptiveAshraeResult { tmp_cmf, tmp_cmf_80_low, tmp_cmf_80_up, tmp_cmf_90_low, tmp_cmf_90_up, acceptability_80, acceptability_90 } = adaptive_ashrae(inputs, options);
            (tmp_cmf, Limits::Ashrae { acceptability_80: limit(tmp_cmf_80_low, tmp_cmf_80_up, acceptability_80), acceptability_90: limit(tmp_cmf_90_low, tmp_cmf_90_up, acceptability_90) }, true)
        }
        Standard::En => {
            let AdaptiveEnResult {
                tmp_cmf,
                tmp_cmf_cat_i_low,
                tmp_cmf_cat_i_up,
                tmp_cmf_cat_ii_low,
                tmp_cmf_cat_ii_up,
                tmp_cmf_cat_iii_low,
                tmp_cmf_cat_iii_up,
                acceptability_cat_i,
                acceptability_cat_ii,
                acceptability_cat_iii,
            } = adaptive_en(inputs, options);
            (
                tmp_cmf,
                Limits::En {
                    category_i: limit(tmp_cmf_cat_i_low, tmp_cmf_cat_i_up, acceptability_cat_i),
                    category_ii: limit(tmp_cmf_cat_ii_low, tmp_cmf_cat_ii_up, acceptability_cat_ii),
                    category_iii: limit(tmp_cmf_cat_iii_low, tmp_cmf_cat_iii_up, acceptability_cat_iii),
                },
                false,
            )
        }
    };
    comfort.as_kelvin().is_finite().then(|| Adaptive {
        comfort: quantity(comfort),
        operative: quantity(operative_temperature(OperativeTemperatureInputs { tdb, tr, v: speed }, OperativeTemperatureOptions { use_ashrae })),
        limits,
    })
}

fn environment(conditions: Conditions) -> (Temperature, Temperature, Speed, thermalcomfort::Humidity) {
    let Conditions { dry_bulb, mean_radiant, air_speed, relative_humidity, pressure: _ } = conditions;
    (temperature(dry_bulb), temperature(mean_radiant), Speed::from_meters_per_second(air_speed.get::<meter_per_second>()), humidity(relative_humidity))
}
