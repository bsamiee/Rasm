use nalgebra::{Point3, Vector3};
use nonempty_collections::NEVec;
use parry3d_f64::{
    bounding_volume::{Aabb, BoundingVolume as _},
    math::Pose,
    partitioning::BvhNode,
    query::{self, Contact, PointQuery as _, ShapeDistance, Unsupported},
    shape::{SharedShape, SubShapeId},
};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Assembly, Distance, ProximityError};

// --- [TYPES] ---------------------------------------------------------------------------

type Leaf<'assembly> = (usize, &'assembly Pose, &'assembly SharedShape, Aabb);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Interference {
    Contact { points: [Point3<f64>; 2], normal: Vector3<f64>, dist: f64, subshapes: [SubShapeId; 2] },
    Nested { point: Point3<f64>, inner: usize },
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Clash {
    pub left: usize,
    pub right: usize,
    pub interference: Interference,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn clashes(assembly: &Assembly, other: Option<&Assembly>, clearance: Distance, rule: impl Fn(usize, usize) -> bool + Sync) -> Result<Vec<Clash>, NEVec<ProximityError>> {
    let prediction = clearance.into_inner();
    let candidates = other.unwrap_or(assembly);
    assembly
        .bvh
        .leaf_pairs(&candidates.bvh, |node, candidate| node.aabb().loosened(prediction).intersects(&candidate.aabb()))
        .filter(|&(left, right)| (other.is_some() || left < right) && rule(left as usize, right as usize))
        .collect::<Vec<_>>()
        .into_par_iter()
        .filter_map(|(left, right)| {
            let (element @ (_, pose, shape, _), candidate @ (_, candidate_pose, candidate_shape, _)) = leaf(assembly, left).zip(leaf(candidates, right))?;
            query::contact(pose, &**shape, candidate_pose, &**candidate_shape, prediction)
                .map_err(|Unsupported| unsupported(element, candidate))
                .map(|contact| clash(element, candidate, contact))
                .transpose()
        })
        .collect::<Validated<Vec<Clash>, ProximityError>>()
        .ok()
}

pub fn separations(assembly: &Assembly, other: Option<&Assembly>, max_distance: Option<Distance>, rule: impl Fn(usize, usize) -> bool + Sync) -> Result<Vec<Clash>, NEVec<ProximityError>> {
    let candidates = other.unwrap_or(assembly);
    let max_cost = max_distance.map_or(f64::INFINITY, |bound| bound.into_inner().next_up());
    (0..assembly.elements.len())
        .into_par_iter()
        .filter_map(|index| leaf(assembly, u32::try_from(index).ok()?))
        .filter_map(|element @ (index, pose, shape, aabb)| {
            candidates
                .bvh
                .find_best(
                    max_cost,
                    |node, _| node.aabb().add_half_extents(aabb.half_extents()).distance_to_local_point(aabb.center(), true),
                    |partner, _| {
                        let candidate @ (_, candidate_pose, candidate_shape, _) = leaf(candidates, partner).filter(|&(partner, ..)| (other.is_some() || partner != index) && rule(index, partner))?;
                        Some(match query::distance(pose, &**shape, candidate_pose, &**candidate_shape) {
                            Ok(ShapeDistance { distance, subshape1: _, subshape2: _ }) => (distance, Ok(candidate)),
                            Err(Unsupported) => (f64::NEG_INFINITY, Err(unsupported(element, candidate))),
                        })
                    },
                )
                .and_then(|(_, (distance, nearest))| {
                    nearest
                        .and_then(|candidate @ (_, candidate_pose, candidate_shape, _)| {
                            query::contact(pose, &**shape, candidate_pose, &**candidate_shape, distance).map_err(|Unsupported| unsupported(element, candidate)).map(|contact| clash(element, candidate, contact))
                        })
                        .transpose()
                })
        })
        .collect::<Validated<Vec<Clash>, ProximityError>>()
        .ok()
}

fn leaf(owner: &Assembly, index: u32) -> Option<Leaf<'_>> { owner.element(index).zip(owner.bvh.leaf_node(index).map(BvhNode::aabb)).map(|((element, pose, shape), aabb)| (element, pose, shape, aabb)) }

fn clash(element: Leaf<'_>, candidate: Leaf<'_>, contact: Option<Contact>) -> Option<Clash> {
    let witness = |Contact { point1, point2, normal1, normal2: _, dist, subshape1, subshape2 }: Contact| Interference::Contact {
        points: [point1, point2].map(Point3::from),
        normal: Vector3::from(normal1),
        dist,
        subshapes: [subshape1, subshape2],
    };
    let nested = |(inner, inner_pose, inner_shape, inner_aabb): Leaf<'_>, (_, outer_pose, outer_shape, outer_aabb): Leaf<'_>| {
        outer_aabb
            .contains(&inner_aabb)
            .then(|| inner_shape.project_point(inner_pose, inner_aabb.maxs, false).point)
            .filter(|&point| outer_shape.contains_point(outer_pose, point))
            .map(|point| Interference::Nested { point: Point3::from(point), inner })
    };
    contact
        .filter(|contact| contact.dist <= 0.0)
        .map(witness)
        .or_else(|| nested(element, candidate))
        .or_else(|| nested(candidate, element))
        .or_else(|| contact.map(witness))
        .map(|interference| Clash { left: element.0, right: candidate.0, interference })
}

fn unsupported((left, _, shape, _): Leaf<'_>, (right, _, candidate, _): Leaf<'_>) -> ProximityError {
    ProximityError::Unsupported { left, right, shapes: [shape.shape_type(), candidate.shape_type()] }
}
