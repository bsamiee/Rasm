use nalgebra::{Point3, Vector3};
use nonempty_collections::NEVec;
use parry3d_f64::{
    bounding_volume::{Aabb, BoundingVolume as _},
    math::Pose,
    partitioning::BvhNode,
    query::{self, Contact, Unsupported},
    shape::{SharedShape, SubShapeId},
};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Assembly, Distance, ProximityError};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Interference {
    Contact { points: [Point3<f64>; 2], normal: Vector3<f64>, dist: f64, subshapes: [SubShapeId; 2] },
    Within { point: Point3<f64> },
    Contains { point: Point3<f64> },
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Clash {
    pub left: usize,
    pub right: usize,
    pub interference: Interference,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn clashes(assembly: &Assembly, other: Option<&Assembly>, clearance: Distance) -> Result<Vec<Clash>, NEVec<ProximityError>> {
    fn placed(owner: &Assembly, leaf: u32) -> Option<(&(Pose, SharedShape), Aabb)> { owner.elements.get(leaf as usize).zip(owner.bvh.leaf_node(leaf).map(BvhNode::aabb)) }
    let prediction = clearance.into_inner();
    let candidates = other.unwrap_or(assembly);
    let witness = |Contact { point1, point2, normal1, normal2: _, dist, subshape1, subshape2 }: Contact| Interference::Contact {
        points: [point1, point2].map(Point3::from),
        normal: Vector3::from(normal1),
        dist,
        subshapes: [subshape1, subshape2],
    };
    let nested = |((inner_pose, inner), inner_aabb): (&(Pose, SharedShape), Aabb), ((outer_pose, outer), outer_aabb): (&(Pose, SharedShape), Aabb)| {
        outer_aabb.contains(&inner_aabb).then(|| inner.project_point(inner_pose, inner_aabb.maxs, false).point).filter(|&point| outer.contains_point(outer_pose, point)).map(Point3::from)
    };
    assembly
        .bvh
        .leaf_pairs(&candidates.bvh, |node, candidate| node.aabb().loosened(prediction).intersects(&candidate.aabb()))
        .filter(|&(left, right)| other.is_some() || left < right)
        .collect::<Vec<_>>()
        .into_par_iter()
        .filter_map(|(left, right)| {
            let (element @ ((pose, shape), _), candidate @ ((candidate_pose, candidate_shape), _)) = placed(assembly, left).zip(placed(candidates, right))?;
            query::contact(pose, &**shape, candidate_pose, &**candidate_shape, prediction)
                .map_err(|Unsupported| ProximityError::Unsupported { left: left as usize, right: right as usize })
                .map(|contact| {
                    contact
                        .filter(|contact| contact.dist <= 0.0)
                        .map(witness)
                        .or_else(|| nested(element, candidate).map(|point| Interference::Within { point }))
                        .or_else(|| nested(candidate, element).map(|point| Interference::Contains { point }))
                        .or_else(|| contact.map(witness))
                        .map(|interference| Clash { left: left as usize, right: right as usize, interference })
                })
                .transpose()
        })
        .collect::<Validated<Vec<Clash>, ProximityError>>()
        .ok()
}
