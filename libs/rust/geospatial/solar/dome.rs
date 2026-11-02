use core::{
    f64::consts::{FRAC_PI_2, TAU},
    num::NonZero,
};

use nalgebra::{Unit, UnitQuaternion, Vector3};
use uom::si::{
    angle::radian,
    f64::{Angle, SolidAngle},
    solid_angle::steradian,
};

use crate::toward;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Patch {
    pub direction: Unit<Vector3<f64>>,
    pub solid_angle: SolidAngle,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn patches(subdivision: NonZero<u8>, north: UnitQuaternion<f64>) -> Vec<Patch> {
    let (band, zenith, row) = rows(subdivision);
    (0..=zenith)
        .flat_map(|index| {
            let (_, count) = row(index);
            let lower = band * nalgebra::convert::<usize, f64>(index);
            let width = TAU / nalgebra::convert::<usize, f64>(count);
            let solid_angle = SolidAngle::new::<steradian>(width * ((lower + band).min(FRAC_PI_2).sin() - lower.sin()));
            (0..count).map(move |column| Patch {
                direction: toward(north, Angle::new::<radian>(width * nalgebra::convert::<usize, f64>(column)), Angle::new::<radian>(band.mul_add(0.5, lower))),
                solid_angle,
            })
        })
        .collect()
}

#[must_use]
pub fn bin(subdivision: NonZero<u8>, north: UnitQuaternion<f64>, direction: Unit<Vector3<f64>>) -> Option<usize> {
    let local = north.inverse_transform_unit_vector(&direction);
    let (band, _, row) = rows(subdivision);
    (local.z >= 0.0).then(|| {
        let (first, count) = row(nalgebra::convert_unchecked::<f64, usize>((local.z.atan2(local.xy().norm()) / band).floor()));
        let columns = nalgebra::convert::<usize, f64>(count);
        first.strict_add(nalgebra::convert_unchecked::<f64, usize>((local.x.atan2(local.y) / TAU).mul_add(columns, 0.5).floor().rem_euclid(columns)))
    })
}

fn rows(subdivision: NonZero<u8>) -> (f64, usize, impl Fn(usize) -> (usize, usize) + Copy) {
    const TREGENZA: [usize; 7] = [30, 30, 24, 24, 18, 12, 6];
    let split = NonZero::<usize>::from(subdivision);
    let zenith = TREGENZA.len().strict_mul(split.get());
    (FRAC_PI_2 / (nalgebra::convert::<usize, f64>(zenith) + 0.5), zenith, move |row: usize| {
        let (tier, step) = (row / split, row % split);
        let count = TREGENZA.get(tier).map_or(1, |&count| count.strict_mul(split.get()));
        (TREGENZA.iter().take(tier).sum::<usize>().strict_mul(split.get().strict_pow(2)).strict_add(step.strict_mul(count)), count)
    })
}
