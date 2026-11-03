use core::{
    f64::consts::{FRAC_PI_2, PI},
    hash::BuildHasher,
    iter,
    num::NonZero,
};
use std::collections::HashMap;

use itertools::{Either, EitherOrBoth};
use nalgebra::{UnitQuaternion, UnitVector3, Vector3};
use ndarray::{Array2, Axis, ShapeBuilder as _, Zip};
use rasm_proximity::{Assembly, Distance};
use rasm_solar::{
    dome::{Patch, bin, patches},
    exposure::{Sensor, Transmittance, incidence},
};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, ParallelIterator as _};
use uom::si::{f64::Ratio, heat_flux_density::watt_per_square_meter, illuminance::lux, ratio::ratio, solid_angle::steradian};
use validated::Validated;

use crate::{Hours, Refusal, WeatherError, extraterrestrial};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Spectrum {
    Solar,
    Visible,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Light {
    Diffuse,
    Direct,
    Global,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Standard {
    Type1,
    Type2,
    Type3,
    Type4,
    Type5,
    Type6,
    Type7,
    Type8,
    Type9,
    Type10,
    Type11,
    Type12,
    Type13,
    Type14,
    Type15,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Model {
    Perez,
    Cie(Standard),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Sky {
    pub model: Model = Model::Perez,
    pub spectrum: Spectrum = Spectrum::Solar,
    pub light: Light = Light::Global,
    pub subdivision: NonZero<u8> = NonZero::<u8>::MIN,
    pub reflectance: Option<Ratio> = None,
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
    const NIGHT: f64 = 1.0e-4;
    const PEREZ: [(f64, [[f64; 4]; 5]); 8] = [
        (1.0, [[1.3525, -0.2576, -0.2690, -1.4366], [-0.7670, 0.0007, 1.2734, -0.1233], [2.8000, 0.6004, 1.2375, 1.0000], [1.8734, 0.6297, 0.9738, 0.2809], [0.0356, -0.1246, -0.5718, 0.9938]]),
        (1.065, [[-1.2219, -0.7730, 1.4148, 1.1016], [-0.2054, 0.0367, -3.9128, 0.9156], [6.9750, 0.1774, 6.4477, -0.1239], [-1.5798, -0.5081, -1.7812, 0.1080], [0.2624, 0.0672, -0.2190, -0.4285]]),
        (
            1.230,
            [[-1.1000, -0.2515, 0.8952, 0.0156], [0.2782, -0.1812, -4.5000, 1.1766], [24.7219, -13.0812, -37.7000, 34.8438], [-5.0000, 1.5218, 3.9229, -2.6204], [-0.0156, 0.1597, 0.4199, -0.5562]],
        ),
        (
            1.500,
            [[-0.5484, -0.6654, -0.2672, 0.7117], [0.7234, -0.6219, -5.6812, 2.6297], [33.3389, -18.3000, -62.2500, 52.0781], [-3.5000, 0.0016, 1.1477, 0.1062], [0.4659, -0.3296, -0.0876, -0.0329]],
        ),
        (1.950, [[-0.6000, -0.3566, -2.5000, 2.3250], [0.2937, 0.0496, -5.6812, 1.8415], [21.0000, -4.7656, -21.5906, 7.2492], [-3.5000, -0.1554, 1.4062, 0.3988], [0.0032, 0.0766, -0.0656, -0.1294]]),
        (2.800, [[-1.0156, -0.3670, 1.0078, 1.4051], [0.2875, -0.5328, -3.8500, 3.3750], [14.0000, -0.9999, -7.1406, 7.5469], [-3.4000, -0.1078, -1.0750, 1.5702], [-0.0672, 0.4016, 0.3017, -0.4844]]),
        (4.500, [[-1.0000, 0.0211, 0.5025, -0.5119], [-0.3000, 0.1922, 0.7023, -1.6317], [19.0000, -5.0000, 1.2438, -1.9094], [-4.0000, 0.0250, 0.3844, 0.2656], [1.0468, -0.3788, -2.4517, 1.4656]]),
        (6.200, [[-1.0500, 0.0289, 0.4260, 0.3590], [-0.3250, 0.1156, 0.7781, 0.0025], [31.0625, -14.5000, -46.1148, 55.3750], [-7.2312, 0.4050, 13.3500, 0.6234], [1.5000, -0.6426, 1.8564, 0.5636]]),
    ];
    const fn general(standard: Standard) -> [f64; 5] {
        match standard {
            Standard::Type1 => [4.0, -0.70, 0.0, -1.0, 0.00],
            Standard::Type2 => [4.0, -0.70, 2.0, -1.5, 0.15],
            Standard::Type3 => [1.1, -0.80, 0.0, -1.0, 0.00],
            Standard::Type4 => [1.1, -0.80, 2.0, -1.5, 0.15],
            Standard::Type5 => [0.0, -1.00, 0.0, -1.0, 0.00],
            Standard::Type6 => [0.0, -1.00, 2.0, -1.5, 0.15],
            Standard::Type7 => [0.0, -1.00, 5.0, -2.5, 0.30],
            Standard::Type8 => [0.0, -1.00, 10.0, -3.0, 0.45],
            Standard::Type9 => [-1.0, -0.55, 2.0, -1.5, 0.15],
            Standard::Type10 => [-1.0, -0.55, 5.0, -2.5, 0.30],
            Standard::Type11 => [-1.0, -0.55, 10.0, -3.0, 0.45],
            Standard::Type12 => [-1.0, -0.32, 10.0, -3.0, 0.45],
            Standard::Type13 => [-1.0, -0.32, 16.0, -3.0, 0.30],
            Standard::Type14 => [-1.0, -0.15, 16.0, -3.0, 0.30],
            Standard::Type15 => [-1.0, -0.15, 24.0, -2.8, 0.15],
        }
    }
    let Sky { model, spectrum, light, subdivision, reflectance } = sky;
    let dome = patches(subdivision, UnitQuaternion::identity()).into_iter().map(|Patch { direction, solid_angle }| (direction, solid_angle.get::<steradian>())).collect::<Vec<_>>();
    let readings = hours
        .records
        .iter()
        .enumerate()
        .map(|(index, hour)| {
            let (diffuse, beam) = match spectrum {
                Spectrum::Solar => (hour.diffuse_irradiance.map(|value| value.get::<watt_per_square_meter>()), hour.direct_irradiance.map(|value| value.get::<watt_per_square_meter>())),
                Spectrum::Visible => (hour.diffuse_illuminance.map(|value| value.get::<lux>()), hour.direct_illuminance.map(|value| value.get::<lux>())),
            };
            let distribution = match model {
                Model::Perez =>
                    hour.diffuse_irradiance.zip(hour.direct_irradiance).map(|(horizontal, normal)| Either::Left((horizontal.get::<watt_per_square_meter>(), normal.get::<watt_per_square_meter>()))),
                Model::Cie(standard) => Some(Either::Right(standard)),
            };
            let skylight = |reflected| diffuse.zip(distribution).map(|(diffuse, distribution)| (diffuse, reflected, distribution));
            match (light, model, beam) {
                (Light::Diffuse, Model::Cie(_), None) => skylight(0.0).map(EitherOrBoth::Left),
                (Light::Diffuse, Model::Perez, None) | (Light::Direct | Light::Global, Model::Perez | Model::Cie(_), None) => None,
                (Light::Diffuse, Model::Perez | Model::Cie(_), Some(beam)) => skylight(beam).map(EitherOrBoth::Left),
                (Light::Direct, Model::Perez | Model::Cie(_), Some(beam)) => Some(EitherOrBoth::Right(beam)),
                (Light::Global, Model::Perez | Model::Cie(_), Some(beam)) => skylight(beam).map(|scattered| EitherOrBoth::Both(scattered, beam)),
            }
            .map(|reading| (hour, reading))
            .ok_or(Refusal::Incomplete(index))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(|refusals| WeatherError::Refused { refusals })?;
    let perez = |(horizontal, normal): (f64, f64), zenith: f64, extraterrestrial: f64| {
        let cubed = 1.041 * zenith.powi(3);
        let [(_, [horizon, gradient, [c1, c2, c3, c4], [d1, d2, d3, d4], backscatter]), clearer @ ..] = PEREZ;
        let [(held, _), _, _, _, (released, _), _, _] = clearer;
        let clearness = ((((horizontal + normal) / horizontal).max(1.0) + cubed) / (1.0 + cubed)).clamp(1.0, 11.9);
        let measured = (horizontal / 0.15_f64.mul_add((93.885 - zenith.to_degrees()).powf(-1.253), zenith.cos()) / extraterrestrial).max(0.01);
        let brightness = if clearness > held && clearness < released { measured.max(0.2) } else { measured };
        let fitted = |[constant, zenithal, bright, both]: [f64; 4]| brightness.mul_add(both.mul_add(zenith, bright), zenithal.mul_add(zenith, constant));
        clearer.iter().rfind(|(lower, _)| clearness >= *lower).map_or_else(
            || {
                [
                    fitted(horizon),
                    fitted(gradient),
                    (brightness * c2.mul_add(zenith, c1)).powf(c3).exp() - c4,
                    brightness.mul_add(d4, d3) - (brightness * d2.mul_add(zenith, d1)).exp(),
                    fitted(backscatter),
                ]
            },
            |&(_, rows)| rows.map(fitted),
        )
    };
    let mut radiance = Array2::<f64>::zeros((dome.len(), readings.len()).f());
    radiance
        .axis_iter_mut(Axis(1))
        .into_par_iter()
        .zip(readings)
        .filter(|(_, (_, reading))| reading.as_ref().map_any(|&(diffuse, beam, _)| diffuse + beam, |&beam| beam).reduce(f64::max) > NIGHT)
        .try_for_each(|(mut column, (hour, reading))| {
            let (prepared, position) = crate::position(hours, hour)?;
            if let Some(&(diffuse, beam, distribution)) = reading.as_ref().left() {
                let ([horizon, gradient, circumsolar, width, backscatter], baseline, zenith) = match distribution {
                    Either::Left(measured) => {
                        let zenith = position.zenith_angle().clamp(3.0, 90.0).to_radians();
                        (perez(measured, zenith, extraterrestrial(&prepared).get::<watt_per_square_meter>()), 1.0, zenith)
                    }
                    Either::Right(standard) => {
                        let coefficients @ [_, _, circumsolar, width, _] = general(standard);
                        (coefficients, circumsolar.mul_add(-(width * FRAC_PI_2).exp(), 1.0), position.zenith_angle().clamp(0.0, 90.0).to_radians())
                    }
                };
                let ((sine, cosine), (east, north)) = (zenith.sin_cos(), position.azimuth().to_radians().sin_cos());
                let sun = Vector3::new(sine * east, sine * north, cosine);
                iter::zip(column.iter_mut(), &dome).skip(1).for_each(|(cell, (direction, _))| {
                    let scattering = sun.dot(direction).clamp(-1.0, 1.0);
                    *cell = (horizon.mul_add((gradient / direction.z).exp(), 1.0) * circumsolar.mul_add((width * scattering.acos()).exp(), backscatter.mul_add(scattering.powi(2), baseline))).max(0.0);
                });
                let horizontal = iter::zip(&column, &dome).skip(1).fold(0.0, |total, (relative, (direction, solid_angle))| (relative * direction.z).mul_add(*solid_angle, total));
                column.mapv_inplace(|relative| if horizontal > 0.0 { relative * diffuse / horizontal } else { diffuse / PI });
                if let Some(ground) = column.first_mut() {
                    *ground = beam.mul_add(position.elevation_angle().to_radians().sin().max(0.0), diffuse) * reflectance.or(hour.albedo).map_or(0.2, |albedo| albedo.get::<ratio>()) / PI;
                }
            }
            if let Some(beam) = reading.right()
                && let toward = rasm_solar::direction(position, UnitQuaternion::identity())
                && let Some(lifted) = UnitVector3::try_new(Vector3::new(toward.x, toward.y, toward.z.max(0.0)), 0.0)
                && let target = bin(subdivision, UnitQuaternion::identity(), lifted)
                && let Some((cell, (_, solid_angle))) = column.get_mut(target).zip(dome.get(target))
            {
                *cell += beam / solid_angle;
            }
            Ok::<_, WeatherError>(())
        })?;
    Ok(Matrix { subdivision, radiance })
}

pub fn irradiance<S>(
    matrix: &Matrix,
    north: UnitQuaternion<f64>,
    assembly: &Assembly,
    translucent: &HashMap<usize, Transmittance, S>,
    offset: Distance,
    sensors: &[Sensor],
) -> Result<Array2<f64>, WeatherError>
where
    S: BuildHasher + Sync,
{
    let sky = |turn| patches(matrix.subdivision, turn).into_iter().skip(1);
    let (directions, solid_angles): (Vec<_>, Vec<_>) =
        sky(north).chain(sky(north * UnitQuaternion::from_axis_angle(&Vector3::x_axis(), PI))).map(|Patch { direction, solid_angle }| (direction, solid_angle.get::<steradian>())).unzip();
    let (ground, radiance) = matrix.radiance.view().split_at(Axis(0), 1);
    let mut weighted = incidence(assembly, translucent, offset, sensors, &directions)?;
    Zip::from(weighted.columns_mut()).and(&solid_angles).for_each(|mut column, &solid_angle| column.mapv_inplace(|exposure| exposure * solid_angle));
    let (above, below) = weighted.view().split_at(Axis(1), radiance.nrows());
    let mut received = above.dot(&radiance);
    Zip::from(received.rows_mut()).and(below.rows()).for_each(|mut row, exposure| row.scaled_add(exposure.sum(), &ground.row(0)));
    Ok(received)
}
