use core::{f64::consts::FRAC_PI_3, num::NonZero};
use std::borrow::Cow;

use crate::{MeshError, Operation, Solid};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Refinement {
    Divisions(NonZero<u16>, Option<Smoothing>),
    Length(f64, Option<Smoothing>),
    Tolerance(f64, Smoothing),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Smoothing {
    pub sharp_angle: f64 = FRAC_PI_3,
    pub smoothness: f64 = 0.0,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn refine(solid: &Solid, refinement: Refinement) -> Result<Option<Solid>, MeshError> {
    let base = |smoothing: Option<Smoothing>| smoothing.map_or(Cow::Borrowed(&solid.0), |Smoothing { sharp_angle, smoothness }| Cow::Owned(solid.0.smooth_out(sharp_angle.to_degrees(), smoothness)));
    Solid::checked(
        Operation::Refine,
        match refinement {
            Refinement::Divisions(count, smoothing) => base(smoothing).refine(NonZero::<i32>::from(count).get()),
            Refinement::Length(length, smoothing) => base(smoothing).refine_to_length(length),
            Refinement::Tolerance(tolerance, smoothing) => base(Some(smoothing)).refine_to_tolerance(tolerance),
        },
    )
}
