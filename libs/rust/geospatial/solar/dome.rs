use core::{
    f64::consts::{FRAC_PI_2, TAU},
    iter,
    num::NonZero,
};

use nalgebra::{UnitQuaternion, UnitVector3, Vector3};
use uom::si::{
    angle::{radian, revolution},
    f64::{Angle, SolidAngle},
    solid_angle::steradian,
};

use crate::direction;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Patch {
    pub direction: UnitVector3<f64>,
    pub solid_angle: SolidAngle,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn patches(subdivision: NonZero<u8>, north: UnitQuaternion<f64>) -> Vec<Patch> {
    let (band, zenith_row, row) = rows(subdivision);
    iter::once(Patch { direction: north * -Vector3::z_axis(), solid_angle: SolidAngle::new::<steradian>(TAU) })
        .chain((0..=zenith_row).flat_map(|index| {
            let (_, count) = row(index);
            let lower = band * nalgebra::convert::<usize, f64>(index);
            let width = TAU / nalgebra::convert::<usize, f64>(count.get());
            let solid_angle = SolidAngle::new::<steradian>(width * ((lower + band).min(FRAC_PI_2).sin() - lower.sin()));
            (0..count.get()).map(move |column| Patch {
                direction: direction(Angle::new::<radian>(width * nalgebra::convert::<usize, f64>(column)), Angle::new::<radian>(band.mul_add(0.5, lower)), north),
                solid_angle,
            })
        }))
        .collect()
}

#[must_use]
pub fn bin(subdivision: NonZero<u8>, north: UnitQuaternion<f64>, direction: UnitVector3<f64>) -> usize {
    let local = north.inverse_transform_unit_vector(&direction);
    let (band, _, row) = rows(subdivision);
    if local.z >= 0.0 {
        let (first, count) = row((local.z.atan2(local.xy().norm()) / band).floor().to_int_saturating::<usize>());
        first.strict_add(sector(Angle::new::<radian>(local.x.atan2(local.y)), count))
    } else {
        0
    }
}

#[must_use]
pub fn sector(azimuth: Angle, count: NonZero<usize>) -> usize {
    let sectors = nalgebra::convert::<usize, f64>(count.get());
    azimuth.get::<revolution>().mul_add(sectors, 0.5).floor().rem_euclid(sectors).to_int_saturating::<usize>()
}

fn rows(subdivision: NonZero<u8>) -> (f64, usize, impl Fn(usize) -> (usize, NonZero<usize>) + Copy) {
    const TREGENZA: [usize; 7] = [30, 30, 24, 24, 18, 12, 6];
    let split = NonZero::<usize>::from(subdivision);
    let zenith_row = TREGENZA.len().strict_mul(split.get());
    (FRAC_PI_2 / (nalgebra::convert::<usize, f64>(zenith_row) + 0.5), zenith_row, move |row: usize| {
        let (tier, step) = (row / split, row % split);
        let count = TREGENZA.get(tier).copied().and_then(NonZero::new).map_or(NonZero::<usize>::MIN, |count| count.saturating_mul(split));
        (TREGENZA.iter().take(tier).sum::<usize>().strict_mul(split.get().strict_pow(2)).strict_add(step.strict_mul(count.get())).strict_add(1), count)
    })
}
