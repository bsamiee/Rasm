use itertools::Itertools as _;
use nalgebra::{Point3, Vector3};
use nonempty_collections::NEVec;
use parry3d_f64::{
    bounding_volume::BoundingVolume as _,
    query::{self, Contact, PointQuery as _, Unsupported},
    shape::SubShapeId,
};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Assembly, Distance, Placed, ProximityError};

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
        .filter_map(|(left, right)| assembly.element(left).zip(candidates.element(right)))
        .filter(|&((left, ..), (right, ..))| (other.is_some() || left < right) && rule(left, right))
        .collect::<Vec<_>>()
        .into_par_iter()
        .filter_map(|(element @ (_, pose, shape, _), candidate @ (_, candidate_pose, candidate_shape, _))| {
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
    assembly
        .bvh
        .leaves(|_| true)
        .filter_map(|leaf| assembly.element(leaf))
        .collect::<Vec<_>>()
        .into_par_iter()
        .filter_map(|element @ (index, pose, shape, aabb)| {
            let nearest = |max_cost: f64| {
                candidates.bvh.find_best(
                    max_cost,
                    |node, _| node.aabb().add_half_extents(aabb.half_extents()).distance_to_local_point(aabb.center(), true),
                    |leaf, best| {
                        let candidate @ (_, candidate_pose, candidate_shape, _) = candidates.element(leaf).filter(|&(partner, ..)| (other.is_some() || partner != index) && rule(index, partner))?;
                        match query::contact(pose, &**shape, candidate_pose, &**candidate_shape, best) {
                            Ok(contact) => contact.map(|contact| (contact.dist.max(0.0), Ok((candidate, contact)))),
                            Err(Unsupported) => Some((f64::NEG_INFINITY, Err(unsupported(element, candidate)))),
                        }
                    },
                )
            };
            max_distance
                .map_or_else(|| nearest(f64::INFINITY), |bound| nearest(bound.into_inner().next_up()))
                .and_then(|(_, (_, nearest))| nearest.map(|(candidate, contact)| clash(element, candidate, Some(contact))).transpose())
        })
        .collect::<Validated<Vec<Clash>, ProximityError>>()
        .ok()
        .map(|separations| separations.into_iter().sorted_unstable_by_key(|separation| separation.left).collect())
}

fn clash(element: Placed<'_>, candidate: Placed<'_>, contact: Option<Contact>) -> Option<Clash> {
    let witness = |Contact { point1, point2, normal1, normal2: _, dist, subshape1, subshape2 }: Contact| Interference::Contact {
        points: [point1, point2].map(Point3::from),
        normal: Vector3::from(normal1),
        dist,
        subshapes: [subshape1, subshape2],
    };
    let nested = |(inner, inner_pose, inner_shape, inner_aabb): Placed<'_>, (_, outer_pose, outer_shape, outer_aabb): Placed<'_>| {
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

fn unsupported((left, _, shape, _): Placed<'_>, (right, _, candidate, _): Placed<'_>) -> ProximityError {
    ProximityError::Unsupported { left, right, shapes: [shape.shape_type(), candidate.shape_type()] }
}
