#![feature(const_result_trait_fn, const_trait_impl, default_field_values, trait_alias)]

use core::{
    iter,
    num::{NonZero, TryFromIntError},
};

use itertools::Itertools as _;
use kiddo::kd_tree::ConstructionError;
use nalgebra::{Isometry3, Point3};
use nonempty_collections::NEVec;
use parry3d_f64::{
    bounding_volume::Aabb,
    math::Pose,
    partitioning::{Bvh, BvhBuildStrategy, BvhNode, BvhWorkspace},
    shape::{ShapeType, SharedShape},
};
use rayon::iter::{IndexedParallelIterator, IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

type Placed<'assembly> = (usize, &'assembly Pose, &'assembly SharedShape, Aabb);

// --- [CONSTANTS] -----------------------------------------------------------------------

const MAX_LEAF_COUNT: usize = 0x3FFF_FFFF;

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(const_fn, validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Distance(f64);

#[derive(derive_more::Debug, Clone, derive_more::AsRef)]
pub struct Assembly {
    elements: Vec<Option<(Pose, SharedShape)>>,
    #[as_ref]
    bvh: Bvh,
    #[debug(skip)]
    workspace: BvhWorkspace,
}

impl Assembly {
    pub fn new<I>(elements: I) -> Result<Self, NEVec<ProximityError>>
    where I: IntoIterator<Item = (Isometry3<f64>, SharedShape)> {
        let elements = elements.into_iter().map(|(pose, shape)| Some((Pose::from(pose), shape))).collect::<Vec<_>>();
        Validated::from((elements.len() <= MAX_LEAF_COUNT).ok_or(ProximityError::Capacity { count: elements.len() }))
            .map2(Validated::from(bounds(elements.par_iter().map(Option::as_ref))), |(), bounds| Self {
                elements,
                bvh: Bvh::from_iter(BvhBuildStrategy::default(), bounds.into_iter().flatten().enumerate()),
                workspace: BvhWorkspace::default(),
            })
            .ok()
    }

    pub fn placed<I>(self, changes: I) -> Result<Self, NEVec<ProximityError>>
    where I: IntoIterator<Item = (usize, Option<(Isometry3<f64>, SharedShape)>)> {
        let changes = changes.into_iter().map(|(index, element)| (index, element.map(|(pose, shape)| (Pose::from(pose), shape)))).collect::<Vec<_>>();
        let slots = changes.iter().map(|&(index, _)| index.saturating_add(1)).fold(self.elements.len(), usize::max);
        let leaves = match changes.iter().map(|&(index, _)| u32::try_from(index)).collect::<Result<Vec<_>, _>>() {
            Ok(leaves) if slots <= MAX_LEAF_COUNT => Ok(leaves),
            Ok(_) | Err(TryFromIntError { .. }) => Err(ProximityError::Capacity { count: slots }),
        };
        Validated::from(leaves)
            .map2(Validated::from(bounds(changes.par_iter().map(|(_, element)| element.as_ref()))), |leaves, bounds| {
                let Self { elements, bvh, mut workspace } = self;
                let (elements, mut bvh) = itertools::izip!(changes, leaves, bounds).fold(
                    (iter::chain(elements, iter::repeat_with(|| None)).take(slots).collect::<Vec<_>>(), bvh),
                    |(mut elements, mut bvh), ((index, element), leaf, aabb)| {
                        match aabb {
                            Some(aabb) => {
                                bvh.insert_or_update_partially(aabb, leaf, 0.0);
                            }
                            None => bvh.remove(leaf),
                        }
                        if let Some(slot) = elements.get_mut(index) {
                            *slot = element;
                        }
                        (elements, bvh)
                    },
                );
                bvh.refit_parallel(&mut workspace);
                Self { elements, bvh, workspace }
            })
            .ok()
    }

    fn element(&self, leaf: u32) -> Option<Placed<'_>> {
        let index = leaf as usize;
        self.elements.get(index)?.as_ref().zip(self.bvh.leaf_node(leaf).map(BvhNode::aabb)).map(|((pose, shape), aabb)| (index, pose, shape, aabb))
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Shortfall {
    #[display("no correspondence inside the bound")]
    Unmatched,
    #[display("no rigid fit from collinear or coincident correspondences")]
    Degenerate,
}

#[derive(Debug, thiserror::Error)]
pub enum ProximityError {
    #[error("{count} elements exceed the {MAX_LEAF_COUNT} leaf limit of the bounding volume hierarchy")]
    Capacity { count: usize },
    #[error("{} inputs hold non-finite or overflowing coordinates, first at index {}", .indices.len(), .indices.first())]
    NonFinite { indices: NEVec<usize> },
    #[error("non-finite initial pose {pose}")]
    Pose { pose: Isometry3<f64> },
    #[error("query point {point} lies inside compound element {element}, which has no signed depth")]
    Compound { point: usize, element: usize },
    #[error("no query between the shapes of inputs {left} and {right}")]
    Unsupported { left: usize, right: usize, shapes: [ShapeType; 2] },
    #[error("{shortfall} at iteration {iteration}")]
    Unfitted { iteration: NonZero<usize>, shortfall: Shortfall },
    #[error(transparent)]
    Tree(#[from] ConstructionError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn bounds<'element>(elements: impl IndexedParallelIterator<Item = Option<&'element (Pose, SharedShape)>>) -> Result<Vec<Option<Aabb>>, ProximityError> {
    let bounds = elements.map(|element| element.map(|(pose, shape)| shape.compute_aabb(pose))).collect::<Vec<_>>();
    finite(bounds.iter().map(|aabb| aabb.is_none_or(|aabb| aabb.mins.is_finite() && aabb.maxs.is_finite()))).map(|()| bounds)
}

fn within(point: &Point3<f64>) -> bool { nalgebra::distance_squared(point, &-point).is_finite() }

fn bounded(points: &[Point3<f64>]) -> Result<(), ProximityError> { finite(points.iter().map(within)) }

fn finite(verdicts: impl IntoIterator<Item = bool>) -> Result<(), ProximityError> {
    NEVec::try_from_vec(verdicts.into_iter().positions(|finite| !finite).collect()).map_or(Ok(()), |indices| Err(ProximityError::NonFinite { indices }))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod containment;
pub mod distances;
pub mod pairs;
pub mod points;
pub mod rays;
pub mod registration;
