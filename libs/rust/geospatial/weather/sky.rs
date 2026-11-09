use core::{
    f64::consts::{FRAC_PI_2, PI},
    hash::Hash,
    iter,
    num::NonZero,
};

use bounded_integer::BoundedU8;
use itertools::{EitherOrBoth, Itertools as _, izip};
use jiff::civil;
use nalgebra::{UnitQuaternion, UnitVector3};
use ndarray::{Array2, Axis, ShapeBuilder as _};
use rasm_solar_geometry::{Sun, direction, patches};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use uom::si::{
    angle::{degree, radian},
    f64::{Angle, Ratio, Time},
    heat_flux_density::watt_per_square_meter,
    illuminance::lux,
    length::centimeter,
    ratio::ratio,
    solid_angle::steradian,
    thermodynamic_temperature::degree_celsius,
    time::second,
};
use validated::Validated;
#[cfg(feature = "proximity")]
use {
    core::hash::BuildHasher,
    itertools::Either,
    nalgebra::Vector3,
    ndarray::ArrayView1,
    rasm_proximity::{Assembly, Distance},
    rasm_solar_geometry::{Sensor, Transmittance, incidence},
    std::collections::HashMap,
    uom::si::f64::{HeatFluxDensity, Illuminance},
};

use crate::{Defect, Hour, Hours, Refusal, WeatherError, cos_zenith, extraterrestrial, length, local};

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

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Model {
    Perez,
    Cie(BoundedU8<1, 15>),
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
    sky: Sky,
    #[as_ref]
    radiance: Array2<f64>,
    #[as_ref([(usize, UnitVector3<f64>, f64)])]
    sun: Vec<(usize, UnitVector3<f64>, f64)>,
}

struct ClearnessBin {
    lower: f64,
    distribution: [[f64; 4]; 5],
    efficacy: [[f64; 4]; 2],
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn matrix<K>(hours: &Hours<Sun>, sky: Sky, key: impl Fn(civil::DateTime, &Hour) -> Option<K>) -> Result<(Vec<(K, Time)>, Matrix), WeatherError>
where K: Ord + Hash {
    const NIGHT: f64 = 1.0e-4;
    const GENERAL: [[f64; 5]; 15] = [
        [4.0, -0.70, 0.0, -1.0, 0.00],
        [4.0, -0.70, 2.0, -1.5, 0.15],
        [1.1, -0.80, 0.0, -1.0, 0.00],
        [1.1, -0.80, 2.0, -1.5, 0.15],
        [0.0, -1.00, 0.0, -1.0, 0.00],
        [0.0, -1.00, 2.0, -1.5, 0.15],
        [0.0, -1.00, 5.0, -2.5, 0.30],
        [0.0, -1.00, 10.0, -3.0, 0.45],
        [-1.0, -0.55, 2.0, -1.5, 0.15],
        [-1.0, -0.55, 5.0, -2.5, 0.30],
        [-1.0, -0.55, 10.0, -3.0, 0.45],
        [-1.0, -0.32, 10.0, -3.0, 0.45],
        [-1.0, -0.32, 16.0, -3.0, 0.30],
        [-1.0, -0.15, 16.0, -3.0, 0.30],
        [-1.0, -0.15, 24.0, -2.8, 0.15],
    ];
    fn perez(horizontal: f64, normal: f64, zenith: f64, extraterrestrial: f64) -> (f64, usize, &'static ClearnessBin) {
        static PEREZ: [ClearnessBin; 8] = [
            ClearnessBin {
                lower: 1.0,
                distribution: [
                    [1.3525, -0.2576, -0.2690, -1.4366],
                    [-0.7670, 0.0007, 1.2734, -0.1233],
                    [2.8000, 0.6004, 1.2375, 1.0000],
                    [1.8734, 0.6297, 0.9738, 0.2809],
                    [0.0356, -0.1246, -0.5718, 0.9938],
                ],
                efficacy: [[57.20, -4.55, -2.98, 117.12], [97.24, -0.46, 12.00, -8.91]],
            },
            ClearnessBin {
                lower: 1.065,
                distribution: [
                    [-1.2219, -0.7730, 1.4148, 1.1016],
                    [-0.2054, 0.0367, -3.9128, 0.9156],
                    [6.9750, 0.1774, 6.4477, -0.1239],
                    [-1.5798, -0.5081, -1.7812, 0.1080],
                    [0.2624, 0.0672, -0.2190, -0.4285],
                ],
                efficacy: [[98.99, -3.46, -1.21, 12.38], [107.22, 1.15, 0.59, -3.95]],
            },
            ClearnessBin {
                lower: 1.230,
                distribution: [
                    [-1.1000, -0.2515, 0.8952, 0.0156],
                    [0.2782, -0.1812, -4.5000, 1.1766],
                    [24.7219, -13.0812, -37.7000, 34.8438],
                    [-5.0000, 1.5218, 3.9229, -2.6204],
                    [-0.0156, 0.1597, 0.4199, -0.5562],
                ],
                efficacy: [[109.83, -4.90, -1.71, -8.81], [104.97, 2.96, -5.53, -8.77]],
            },
            ClearnessBin {
                lower: 1.500,
                distribution: [
                    [-0.5484, -0.6654, -0.2672, 0.7117],
                    [0.7234, -0.6219, -5.6812, 2.6297],
                    [33.3389, -18.3000, -62.2500, 52.0781],
                    [-3.5000, 0.0016, 1.1477, 0.1062],
                    [0.4659, -0.3296, -0.0876, -0.0329],
                ],
                efficacy: [[110.34, -5.84, -1.99, -4.56], [102.39, 5.59, -13.95, -13.90]],
            },
            ClearnessBin {
                lower: 1.950,
                distribution: [
                    [-0.6000, -0.3566, -2.5000, 2.3250],
                    [0.2937, 0.0496, -5.6812, 1.8415],
                    [21.0000, -4.7656, -21.5906, 7.2492],
                    [-3.5000, -0.1554, 1.4062, 0.3988],
                    [0.0032, 0.0766, -0.0656, -0.1294],
                ],
                efficacy: [[106.36, -3.97, -1.75, -6.16], [100.71, 5.94, -22.75, -23.74]],
            },
            ClearnessBin {
                lower: 2.800,
                distribution: [
                    [-1.0156, -0.3670, 1.0078, 1.4051],
                    [0.2875, -0.5328, -3.8500, 3.3750],
                    [14.0000, -0.9999, -7.1406, 7.5469],
                    [-3.4000, -0.1078, -1.0750, 1.5702],
                    [-0.0672, 0.4016, 0.3017, -0.4844],
                ],
                efficacy: [[107.19, -1.25, -1.51, -26.73], [106.42, 3.83, -36.15, -28.83]],
            },
            ClearnessBin {
                lower: 4.500,
                distribution: [
                    [-1.0000, 0.0211, 0.5025, -0.5119],
                    [-0.3000, 0.1922, 0.7023, -1.6317],
                    [19.0000, -5.0000, 1.2438, -1.9094],
                    [-4.0000, 0.0250, 0.3844, 0.2656],
                    [1.0468, -0.3788, -2.4517, 1.4656],
                ],
                efficacy: [[105.75, 0.77, -1.26, -34.44], [141.88, 1.90, -53.24, -14.03]],
            },
            ClearnessBin {
                lower: 6.200,
                distribution: [
                    [-1.0500, 0.0289, 0.4260, 0.3590],
                    [-0.3250, 0.1156, 0.7781, 0.0025],
                    [31.0625, -14.5000, -46.1148, 55.3750],
                    [-7.2312, 0.4050, 13.3500, 0.6234],
                    [1.5000, -0.6426, 1.8564, 0.5636],
                ],
                efficacy: [[101.18, 1.58, -1.10, -8.29], [152.23, 0.35, -45.27, -7.98]],
            },
        ];
        let cubed = 1.041 * zenith.powi(3);
        let [_, held, _, _, _, released, _, _] = &PEREZ;
        let clearness = ((((horizontal + normal) / horizontal).max(1.0) + cubed) / (1.0 + cubed)).clamp(1.0, 11.9);
        let measured = (horizontal / 0.15_f64.mul_add((93.885 - zenith.to_degrees()).powf(-1.253), zenith.cos()) / extraterrestrial).max(0.01);
        let brightness = if clearness > held.lower && clearness < released.lower { measured.max(0.2) } else { measured };
        let bin = PEREZ.partition_point(|row| clearness >= row.lower).strict_sub(1);
        (brightness, bin, &PEREZ[bin])
    }

    fn coefficients(brightness: f64, zenith: f64, bin: usize, rows: [[f64; 4]; 5]) -> [f64; 5] {
        let fitted = |[constant, zenithal, bright, both]: [f64; 4]| brightness.mul_add(both.mul_add(zenith, bright), zenithal.mul_add(zenith, constant));
        match (bin, rows) {
            (0, [horizon, gradient, [c1, c2, c3, c4], [d1, d2, d3, d4], backscatter]) => [
                fitted(horizon),
                fitted(gradient),
                (brightness * c2.mul_add(zenith, c1)).powf(c3).exp() - c4,
                brightness.mul_add(d4, d3) - (brightness * d2.mul_add(zenith, d1)).exp(),
                fitted(backscatter),
            ],
            (_, rows) => rows.map(fitted),
        }
    }
    let Sky { model, spectrum, light, subdivision, reflectance } = sky;
    let dome = patches(subdivision, UnitQuaternion::identity()).collect_vec();
    let groups = hours
        .records
        .iter()
        .enumerate()
        .filter_map(|(index, hour)| key(local(hours, hour), hour).map(|group| (group, (index, hour))))
        .into_group_map()
        .into_iter()
        .sorted_by(|(left, _), (right, _)| left.cmp(right))
        .map(|(group, run)| (group, run.iter().map(|&(_, hour)| length(hour.interval).as_secs_f64()).sum::<f64>(), run))
        .collect_vec();
    let members =
        groups.iter().enumerate().flat_map(|(column, (_, total, run))| run.iter().map(move |&(index, hour)| (column, index, hour, length(hour.interval).as_secs_f64() / total))).collect_vec();
    let positions = members.par_iter().map(|&(_, _, hour, _)| crate::position(&hours.site, hour.interval)).collect::<Result<Vec<_>, _>>()?;
    let readings = iter::zip(&members, &positions)
        .map(|(&(_, index, hour, _), (prepared, position))| {
            let zenith = position.zenith_angle().clamp(3.0, 90.0).to_radians();
            let measured = hour
                .diffuse_irradiance
                .zip(hour.direct_irradiance)
                .map(|(horizontal, normal)| perez(horizontal.get::<watt_per_square_meter>(), normal.get::<watt_per_square_meter>(), zenith, extraterrestrial(prepared).get::<watt_per_square_meter>()));
            let water = hour.precipitable_water.map(|depth| depth.get::<centimeter>()).or_else(|| hour.dew_point.map(|dew| 0.07_f64.mul_add(dew.get::<degree_celsius>(), -0.075).exp()));
            let efficacies = measured.zip(water).map(|((brightness, _, row), water)| {
                let [[direct, moisture, angular, bright], [scattered, wet, overhead, luminous]] = row.efficacy;
                [
                    bright.mul_add(brightness, angular.mul_add(5.73_f64.mul_add(zenith, -5.0).exp(), moisture.mul_add(water, direct))).max(0.0),
                    luminous.mul_add(brightness.ln(), overhead.mul_add(zenith.cos(), wet.mul_add(water, scattered))),
                ]
            });
            let (diffuse, beam) = match spectrum {
                Spectrum::Solar => (hour.diffuse_irradiance.map(|value| value.get::<watt_per_square_meter>()), hour.direct_irradiance.map(|value| value.get::<watt_per_square_meter>())),
                Spectrum::Visible => (
                    hour.diffuse_illuminance
                        .map(|value| value.get::<lux>())
                        .or_else(|| hour.diffuse_irradiance.zip(efficacies).map(|(flux, [_, diffuse])| flux.get::<watt_per_square_meter>() * diffuse)),
                    hour.direct_illuminance.map(|value| value.get::<lux>()).or_else(|| hour.direct_irradiance.zip(efficacies).map(|(flux, [direct, _])| flux.get::<watt_per_square_meter>() * direct)),
                ),
            };
            let distribution = match model {
                Model::Perez => measured.map(|(brightness, bin, row)| (coefficients(brightness, zenith, bin, row.distribution), 1.0, zenith)),
                Model::Cie(kind) => {
                    let coefficients @ [_, _, circumsolar, width, _] = GENERAL[usize::from(kind.get()).strict_sub(1)];
                    Some((coefficients, circumsolar.mul_add(-(width * FRAC_PI_2).exp(), 1.0), position.zenith_angle().clamp(0.0, 90.0).to_radians()))
                }
            };
            let skylight = |reflected| diffuse.zip(distribution).map(|(diffuse, distribution)| (diffuse, reflected, distribution));
            match (light, model, beam) {
                (Light::Diffuse, Model::Cie(_), None) => skylight(0.0).map(EitherOrBoth::Left),
                (Light::Diffuse, Model::Perez, None) | (Light::Direct | Light::Global, Model::Perez | Model::Cie(_), None) => None,
                (Light::Diffuse, Model::Perez | Model::Cie(_), Some(beam)) => skylight(beam).map(EitherOrBoth::Left),
                (Light::Direct, Model::Perez | Model::Cie(_), Some(beam)) => Some(EitherOrBoth::Right(beam)),
                (Light::Global, Model::Perez | Model::Cie(_), Some(beam)) => skylight(beam).map(|scattered| EitherOrBoth::Both(scattered, beam)),
            }
            .ok_or(Refusal::Hour { index, defect: Defect::Incomplete })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    let sun = izip!(&members, &readings, &positions)
        .filter_map(|(&(column, _, _, share), reading, (_, position))| {
            reading
                .as_ref()
                .right()
                .copied()
                .filter(|&beam| beam > NIGHT && position.elevation_angle() > 0.0)
                .map(|beam| (column, direction(Angle::new::<degree>(position.azimuth()), Angle::new::<degree>(position.elevation_angle()), UnitQuaternion::identity()), beam * share))
        })
        .collect();
    let hourly = izip!(members, readings, positions).collect_vec();
    let mut radiance = Array2::<f64>::zeros((dome.len(), groups.len()).f());
    radiance.axis_iter_mut(Axis(1)).into_par_iter().zip(hourly.chunk_by(|((earlier, ..), ..), ((later, ..), ..)| earlier == later).collect_vec()).for_each_init(
        Vec::<f64>::new,
        |luminance, (mut column, run)| {
            let lit = run.iter().filter_map(|(member, reading, (_, position))| {
                reading.as_ref().left().copied().filter(|&(diffuse, beam, _)| diffuse + beam > NIGHT).map(|distribution| (member, distribution, position))
            });
            for ((_, _, hour, share), (diffuse, beam, ([horizon, gradient, circumsolar, width, backscatter], baseline, zenith)), position) in lit {
                let toward = direction(Angle::new::<degree>(position.azimuth()), Angle::new::<radian>(FRAC_PI_2 - zenith), UnitQuaternion::identity());
                luminance.resize(dome.len().strict_sub(1), 0.0);
                let horizontal = iter::zip(luminance.iter_mut(), dome.iter().skip(1)).fold(0.0, |total, (relative, patch)| {
                    let scattering = toward.dot(&patch.direction).clamp(-1.0, 1.0);
                    *relative = (horizon.mul_add((gradient / patch.direction.z).exp(), 1.0)
                        * circumsolar.mul_add((width * scattering.acos()).exp(), backscatter.mul_add(scattering.powi(2), baseline)))
                    .max(0.0);
                    (*relative * patch.direction.z).mul_add(patch.solid_angle.get::<steradian>(), total)
                });
                let ground = beam.mul_add(cos_zenith(position), diffuse) * reflectance.or(hour.albedo).map_or(0.2, |albedo| albedo.get::<ratio>()) / PI;
                iter::zip(column.iter_mut(), iter::once(ground).chain(luminance.iter().map(|&relative| if horizontal > 0.0 { relative * diffuse / horizontal } else { diffuse / PI })))
                    .for_each(|(cell, value)| *cell = value.mul_add(*share, *cell));
            }
        },
    );
    Ok((groups.into_iter().map(|(group, total, _)| (group, Time::new::<second>(total))).collect(), Matrix { sky, radiance, sun }))
}

#[cfg(feature = "proximity")]
pub fn irradiance<S>(
    matrix: &Matrix,
    north: UnitQuaternion<f64>,
    assembly: &Assembly,
    translucent: &HashMap<usize, Transmittance, S>,
    offset: Distance,
    sensors: &[Sensor],
) -> Result<Either<Array2<HeatFluxDensity>, Array2<Illuminance>>, WeatherError>
where
    S: BuildHasher + Sync,
{
    let sky = |turn| patches(matrix.sky.subdivision, turn).skip(1);
    let (directions, solid_angles): (Vec<_>, Vec<_>) =
        sky(north).chain(sky(north * UnitQuaternion::from_axis_angle(&Vector3::x_axis(), PI))).map(|patch| (patch.direction, patch.solid_angle.get::<steradian>())).unzip();
    let (ground, radiance) = matrix.radiance.view().split_at(Axis(0), 1);
    let mut weighted = incidence(assembly, translucent, offset, sensors, &directions)?;
    weighted.zip_mut_with(&ArrayView1::from(&solid_angles), |value, solid_angle| *value *= solid_angle);
    let (above, below) = weighted.view().split_at(Axis(1), radiance.nrows());
    let mut received = above.dot(&radiance);
    received.scaled_add(1.0, &below.sum_axis(Axis(1)).insert_axis(Axis(1)).dot(&ground));
    matrix
        .sun
        .iter()
        .try_for_each(|&(column, toward, beam)| incidence(assembly, translucent, offset, sensors, &[north * toward]).map(|direct| received.column_mut(column).scaled_add(beam, &direct.column(0))))?;
    Ok(match matrix.sky.spectrum {
        Spectrum::Solar => Either::Left(received.mapv(HeatFluxDensity::new::<watt_per_square_meter>)),
        Spectrum::Visible => Either::Right(received.mapv(Illuminance::new::<lux>)),
    })
}
