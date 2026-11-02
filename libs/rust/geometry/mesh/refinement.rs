use core::f64::consts::FRAC_PI_3;
use std::borrow::Cow;

use bounded_integer::BoundedU16;
use manifold_rust::cancel::CancelToken;

use crate::{Defect, MeshError, Operation, Solid, Tolerance};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Refinement {
    Divisions(BoundedU16<2, { u16::MAX }>, Option<Smoothing>),
    Length(Tolerance, Option<Smoothing>),
    Tolerance(Tolerance, Smoothing),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Smoothing {
    pub sharp_angle: f64 = FRAC_PI_3,
    pub min_smoothness: f64 = 0.0,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn refine(solid: &Solid, refinement: Refinement, token: Option<&CancelToken>) -> Result<Option<Solid>, MeshError> {
    let base =
        |smoothing: Option<Smoothing>| smoothing.map_or(Cow::Borrowed(&solid.0), |Smoothing { sharp_angle, min_smoothness }| Cow::Owned(solid.0.smooth_out(sharp_angle.to_degrees(), min_smoothness)));
    Solid::checked(
        Operation::Refine,
        match refinement {
            Refinement::Divisions(count, smoothing) => base(smoothing).refine(i32::from(count.get())),
            Refinement::Length(length, smoothing) => base(smoothing).refine_to_length(length.into_inner()),
            Refinement::Tolerance(tolerance, smoothing) => base(Some(smoothing)).refine_to_tolerance(tolerance.into_inner()),
        },
        token,
        Some(Defect::SelfIntersecting),
    )
}
