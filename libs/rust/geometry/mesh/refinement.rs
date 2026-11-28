use core::f64::consts::FRAC_PI_3;
use std::borrow::Cow;

use bounded_integer::BoundedU16;
use manifold_rust::cancel::CancelToken;
use typed_floats::tf64::StrictlyPositiveFinite;

use crate::{MeshError, Operation, Solid};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Refinement {
    Divisions(BoundedU16<2, { u16::MAX }>, Option<Smoothing>),
    Length(StrictlyPositiveFinite, Option<Smoothing>),
    Tolerance(StrictlyPositiveFinite, Smoothing),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Smoothing {
    pub sharp_angle: f64 = FRAC_PI_3,
    pub min_smoothness: f64 = 0.0,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn refine(solid: &Solid, refinement: Refinement, token: Option<&CancelToken>) -> Result<Option<Solid>, MeshError> {
    let smoothed =
        |smoothing: Option<Smoothing>| smoothing.map_or(Cow::Borrowed(&solid.0), |Smoothing { sharp_angle, min_smoothness }| Cow::Owned(solid.0.smooth_out(sharp_angle.to_degrees(), min_smoothness)));
    Solid::checked(
        Operation::Refine,
        match refinement {
            Refinement::Divisions(count, smoothing) => smoothed(smoothing).refine(i32::from(count.get())),
            Refinement::Length(length, smoothing) => smoothed(smoothing).refine_to_length(length.get()),
            Refinement::Tolerance(tolerance, smoothing) => smoothed(Some(smoothing)).refine_to_tolerance(tolerance.get()),
        },
        token,
    )
}
