#![feature(default_field_values, trait_alias)]

use core::num::NonZero;

use itertools::Itertools as _;
use kiddo::kd_tree::ConstructionError;
use nalgebra::Isometry3;
use nonempty_collections::NEVec;
use parry3d_f64::{
    math::Pose,
    partitioning::{Bvh, BvhBuildStrategy},
    shape::SharedShape,
};
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Distance(f64);

#[derive(Debug, Clone)]
pub struct Assembly {
    elements: Vec<(Pose, SharedShape)>,
    bvh: Bvh,
}

impl Assembly {
    pub fn new<I>(strategy: BvhBuildStrategy, elements: I) -> Result<Self, NEVec<ProximityError>>
    where I: IntoIterator<Item = (Isometry3<f64>, SharedShape)> {
        const MAX_LEAF_COUNT: usize = 0x3FFF_FFFF;
        let elements = elements.into_iter().map(|(pose, shape)| (Pose::from(pose), shape)).collect::<Vec<_>>();
        let aabbs = elements.par_iter().map(|(pose, shape)| shape.compute_aabb(pose)).collect::<Vec<_>>();
        Validated::from((aabbs.len() <= MAX_LEAF_COUNT).ok_or(ProximityError::Capacity { count: aabbs.len() }))
            .map2(Validated::from(finite(aabbs.iter().map(|aabb| aabb.mins.is_finite() && aabb.maxs.is_finite()))), |(), ()| Self { elements, bvh: Bvh::from_leaves(strategy, &aabbs) })
            .ok()
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum ProximityError {
    #[error("{count} elements exceed the 30-bit bounding volume hierarchy leaf count")]
    Capacity { count: usize },
    #[error("non-finite coordinates at {} inputs, first at index {}", .indices.len(), .indices.first())]
    NonFinite { indices: NEVec<usize> },
    #[error("no contact query between elements {left} and {right}")]
    Unsupported { left: usize, right: usize },
    #[error("no rigid fit from the correspondences at iteration {iteration}")]
    Unmatched { iteration: NonZero<usize> },
    #[error(transparent)]
    Tree(#[from] ConstructionError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn finite(verdicts: impl Iterator<Item = bool>) -> Result<(), ProximityError> {
    match NEVec::try_from_vec(verdicts.positions(|finite| !finite).collect()) {
        None => Ok(()),
        Some(indices) => Err(ProximityError::NonFinite { indices }),
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod containment;
pub mod distances;
pub mod pairs;
pub mod points;
pub mod rays;
pub mod registration;
