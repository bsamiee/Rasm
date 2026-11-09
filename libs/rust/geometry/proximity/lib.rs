#![feature(default_field_values, float_conversions, trait_alias, try_blocks_heterogeneous)]

use core::{
    borrow::Borrow,
    num::{NonZero, TryFromIntError},
};

use itertools::Itertools as _;
use kiddo::kd_tree::ConstructionError;
use nalgebra::{Isometry3, Point3};
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use parry3d_f64::{
    bounding_volume::Aabb,
    math::Pose,
    partitioning::{Bvh, BvhBuildStrategy, BvhWorkspace},
    shape::{ShapeType, SharedShape},
};
use rayon::iter::{IndexedParallelIterator, IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

// --- [CONSTANTS] -----------------------------------------------------------------------

const MAX_LEAF_COUNT: usize = 0x3FFF_FFFF;

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Distance(f64);

#[derive_where::derive_where(Debug, Clone)]
#[derive(derive_more::AsRef)]
pub struct Assembly {
    elements: Vec<Option<(Pose, SharedShape)>>,
    #[as_ref]
    bvh: Bvh,
    #[derive_where(skip(Debug, Clone))]
    workspace: Option<Box<BvhWorkspace>>,
}

impl Assembly {
    pub fn new<I>(strategy: BvhBuildStrategy, elements: I) -> Result<Self, NEVec<ProximityError>>
    where I: IntoIterator<Item = (Isometry3<f64>, SharedShape)> {
        let elements = elements.into_iter().map(|(pose, shape)| Some((Pose::from(pose), shape))).collect::<Vec<_>>();
        Validated::from((elements.len() <= MAX_LEAF_COUNT).ok_or(ProximityError::Capacity { count: elements.len() }))
            .map2(Validated::from(bounds(elements.par_iter().map(Option::as_ref))), |(), bounds| Self {
                elements,
                bvh: Bvh::from_iter(strategy, bounds.into_iter().flatten().enumerate()),
                workspace: None,
            })
            .ok()
    }

    pub fn place<I>(&mut self, changes: I) -> Result<(), NEVec<ProximityError>>
    where I: IntoIterator<Item = (usize, Option<(Isometry3<f64>, SharedShape)>)> {
        let changes = changes.into_iter().map(|(index, element)| (index, element.map(|(pose, shape)| (Pose::from(pose), shape)))).collect::<Vec<_>>();
        let slots = changes.iter().map(|&(index, _)| index.saturating_add(1)).fold(self.elements.len(), usize::max);
        let leaves = match changes.iter().map(|&(index, _)| u32::try_from(index)).collect::<Result<Vec<_>, _>>() {
            Ok(leaves) if slots <= MAX_LEAF_COUNT => Ok(leaves),
            Ok(_) | Err(TryFromIntError { .. }) => Err(ProximityError::Capacity { count: slots }),
        };
        let (leaves, bounds) = Validated::from(leaves).map2(Validated::from(bounds(changes.par_iter().map(|(_, element)| element.as_ref()))), |leaves, bounds| (leaves, bounds)).ok()?;
        self.elements.resize_with(slots, || None);
        itertools::izip!(changes, leaves, bounds).for_each(|((index, element), leaf, aabb)| {
            match aabb {
                Some(aabb) => {
                    self.bvh.insert_or_update_partially(aabb, leaf, 0.0);
                }
                None => self.bvh.remove(leaf),
            }
            self.elements[index] = element;
        });
        let workspace = self.workspace.get_or_insert_default();
        self.bvh.refit_parallel(workspace);
        self.bvh.optimize_incremental(workspace);
        Ok(())
    }

    fn element(&self, leaf: u32) -> Option<(usize, &Pose, &SharedShape)> {
        let index = leaf as usize;
        self.elements[index].as_ref().map(|(pose, shape)| (index, pose, shape))
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
    Capacity {
        count: usize,
    },
    #[error("{} inputs hold non-finite or overflowing coordinates, first at index {}", .indices.len(), .indices.first())]
    NonFinite {
        indices: NEVec<usize>,
    },
    #[error("non-finite initial pose {pose}")]
    Pose {
        pose: Isometry3<f64>,
    },
    #[error("query point {point} lies inside compound element {element}, which has no signed depth")]
    Compound {
        point: usize,
        element: usize,
    },
    #[error("no query between the shapes of inputs {left} and {right}")]
    Unsupported {
        left: usize,
        right: usize,
        shapes: [ShapeType; 2],
    },
    #[error("{shortfall} at iteration {iteration}")]
    Unfitted {
        iteration: NonZero<usize>,
        shortfall: Shortfall,
    },
    #[error(transparent)]
    Tree(#[from] ConstructionError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn bounds<'element>(elements: impl IndexedParallelIterator<Item = Option<&'element (Pose, SharedShape)>>) -> Result<Vec<Option<Aabb>>, ProximityError> {
    let bounds = elements.map(|element| element.map(|(pose, shape)| shape.compute_aabb(pose))).collect::<Vec<_>>();
    finite(bounds.iter().map(|aabb| aabb.is_none_or(|aabb| [aabb.mins, aabb.maxs].iter().all(|corner| representable(&Point3::from(*corner)))))).map(|()| bounds)
}

fn representable(point: &Point3<f64>) -> bool { nalgebra::distance_squared(point, &-point).is_finite() }

fn bounded<I>(points: I) -> Result<(), ProximityError>
where I: IntoIterator<Item: Borrow<Point3<f64>>> {
    finite(points.into_iter().map(|point| representable(point.borrow())))
}

fn finite(verdicts: impl IntoIterator<Item = bool>) -> Result<(), ProximityError> {
    verdicts.into_iter().positions(|finite| !finite).try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>).map_or(Ok(()), |indices| Err(ProximityError::NonFinite { indices }))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod containment;
mod distances;
mod pairs;
mod points;
pub mod rays;
mod registration;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use containment::containing;
pub use distances::{Projection, project};
pub use kiddo;
pub use nalgebra;
pub use nonempty_collections;
pub use pairs::{Clash, Interference, clashes, separations};
pub use parry3d_f64;
pub use points::{Cloud, Item, Neighbor, Neighborhood, nearest, neighbors, normals};
pub use rayon;
pub use registration::{Fraction, FractionError, Icp, Kernel, Metric, Registration, Target, Weight, WeightError, fit, register};
