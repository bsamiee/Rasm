use core::{
    f64::consts::{FRAC_PI_2, PI},
    hash::BuildHasher,
    num::NonZero,
};
use std::collections::HashMap;

use eulumdat_daylight::{CieSky, PerezSky, SkyParams, SkyRadiance, SolarPosition, constants::SOLAR_CONSTANT_W_M2, solar::air_mass};
use itertools::Either;
use nalgebra::{UnitQuaternion, UnitVector3, Vector3};
use ndarray::{Array1, Array2, Axis, ShapeBuilder as _, Zip};
use rasm_proximity::{Assembly, Distance};
use rasm_solar::{
    dome::{Patch, bin, patches},
    exposure::{Sensor, Transmittance, incidence},
};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use uom::si::{f64::Ratio, heat_flux_density::watt_per_square_meter, illuminance::lux, ratio::ratio, solid_angle::steradian};
use validated::Validated;

use crate::{Hours, Refusal, WeatherError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Spectrum {
    Solar,
    Visible,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Direct {
    Patch,
    Omitted,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Model {
    Perez,
    Cie(CieSky),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Sky {
    pub model: Model = Model::Perez,
    pub spectrum: Spectrum = Spectrum::Solar,
    pub direct: Direct = Direct::Patch,
    pub subdivision: NonZero<u8> = NonZero::<u8>::MIN,
    pub reflectance: Ratio,
}

#[derive(Debug, Clone, PartialEq, derive_more::AsRef)]
pub struct Matrix {
    #[as_ref]
    subdivision: NonZero<u8>,
    #[as_ref]
    radiance: Array2<f64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn matrix(hours: &Hours, sky: Sky) -> Result<Matrix, WeatherError> {
    const DARK: f64 = 1e-4;
    let Sky { model, spectrum, direct, subdivision, reflectance } = sky;
    let dome = patches(subdivision, UnitQuaternion::identity())
        .into_iter()
        .map(|Patch { direction, solid_angle }| (direction.z.acos(), direction.x.atan2(direction.y), solid_angle.get::<steradian>()))
        .collect::<Array1<_>>();
    let records = hours
        .hours
        .par_iter()
        .enumerate()
        .map(|(index, hour)| {
            let (diffuse, beam) = match spectrum {
                Spectrum::Solar => (hour.diffuse_irradiance.map(|value| value.get::<watt_per_square_meter>()), hour.direct_irradiance.map(|value| value.get::<watt_per_square_meter>())),
                Spectrum::Visible => (hour.diffuse_illuminance.map(|value| value.get::<lux>()), hour.direct_illuminance.map(|value| value.get::<lux>())),
            };
            match model {
                Model::Perez => diffuse.zip(beam).zip(hour.diffuse_irradiance.zip(hour.direct_irradiance)).map(|((diffuse, beam), (horizontal, normal))| {
                    (hour, diffuse, Some(beam), Either::Left((horizontal.get::<watt_per_square_meter>(), normal.get::<watt_per_square_meter>())))
                }),
                Model::Cie(cie) => diffuse.map(|diffuse| (hour, diffuse, beam, Either::Right(cie))),
            }
            .ok_or(Refusal::Incomplete(index))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(|refusals| WeatherError::Refused { refusals })?;
    let mut radiance = Array2::zeros((dome.len(), records.len()).f());
    radiance.axis_iter_mut(Axis(1)).into_par_iter().zip(records).try_for_each(|(column, (hour, diffuse, beam, distribution))| {
        let (prepared, position) = crate::position(hours, hour)?;
        let light = match distribution {
            Either::Left((horizontal, normal)) => horizontal + normal,
            Either::Right(_) => beam.map_or(diffuse, |normal| diffuse + normal),
        };
        if light > DARK {
            let zenith = position.zenith_angle().clamp(3.0, 90.0).to_radians();
            let sun = SolarPosition { altitude_rad: FRAC_PI_2 - zenith, azimuth_rad: position.azimuth().to_radians(), zenith_rad: zenith };
            let distribution = match distribution {
                Either::Left((horizontal, normal)) => {
                    let zenith_term = 1.041 * zenith.powi(3);
                    let extraterrestrial = prepared.earth_radius_vector().map_or(SOLAR_CONSTANT_W_M2, |distance| SOLAR_CONSTANT_W_M2 / distance.powi(2));
                    SkyRadiance::perez_from_dhi(
                        PerezSky::new(sun, SkyParams {
                            clearness_eps: (((horizontal + normal) / horizontal + zenith_term) / (1.0 + zenith_term)).clamp(1.0, 11.9),
                            brightness_delta: (horizontal * air_mass(zenith) / extraterrestrial).clamp(0.01, 0.6),
                        }),
                        diffuse,
                    )
                }
                Either::Right(cie) => SkyRadiance::cie_from_dhi(cie, sun, diffuse),
            };
            let ground = beam.map_or(diffuse, |normal| normal.mul_add(position.elevation_angle().to_radians().sin().max(0.0), diffuse)) * reflectance.get::<ratio>() / PI;
            let sunlit = match direct {
                Direct::Patch => {
                    let toward = rasm_solar::direction(position, UnitQuaternion::identity());
                    beam.zip(UnitVector3::try_new(Vector3::new(toward.x, toward.y, toward.z.max(0.0)), 0.0)).map(|(normal, lifted)| (bin(subdivision, UnitQuaternion::identity(), lifted), normal))
                }
                Direct::Omitted => None,
            };
            Zip::indexed(column).and(&dome).for_each(|patch, cell, &(theta, phi, solid_angle)| {
                *cell = match (patch, sunlit) {
                    (0, _) => ground,
                    (_, Some((target, normal))) if patch == target => distribution.luminance(theta, phi) + normal / solid_angle,
                    (_, _) => distribution.luminance(theta, phi),
                };
            });
        }
        Ok::<_, WeatherError>(())
    })?;
    Ok(Matrix { subdivision, radiance })
}

pub fn irradiance<S>(matrix: &Matrix, north: UnitQuaternion<f64>, assembly: &Assembly, translucent: &HashMap<usize, Transmittance, S>, offset: Distance, sensors: &[Sensor]) -> Result<Array2<f64>, WeatherError>
where S: BuildHasher + Sync {
    let sky = |turn| patches(matrix.subdivision, turn).into_iter().skip(1);
    let (directions, solid_angles): (Vec<_>, Vec<_>) = sky(north)
        .chain(sky(north * UnitQuaternion::from_axis_angle(&Vector3::x_axis(), PI)))
        .map(|Patch { direction, solid_angle }| (direction, solid_angle.get::<steradian>()))
        .unzip();
    let (ground, radiance) = matrix.radiance.view().split_at(Axis(0), 1);
    let weighted = incidence(assembly, translucent, offset, sensors, &directions)? * &Array1::from(solid_angles);
    let (above, below) = weighted.view().split_at(Axis(1), radiance.nrows());
    Ok(above.dot(&radiance) + &(below.sum_axis(Axis(1)).insert_axis(Axis(1)) * &ground))
}
