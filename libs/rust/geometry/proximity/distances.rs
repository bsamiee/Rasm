use nalgebra::Point3;
use nonempty_collections::NEVec;
use parry3d_f64::math::Vector;
use parry3d_f64::query::{PointProjection, PointQuery as _};
use parry3d_f64::shape::SubShapeId;
use rayon::iter::{IndexedParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Assembly, Distance, ProximityError};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Projection {
    pub element: usize,
    pub subshape: SubShapeId,
    pub point: Point3<f64>,
    pub distance: f64,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn project(assembly: &Assembly, points: &[Point3<f64>], max_distance: Option<Distance>) -> Result<Vec<Option<Projection>>, NEVec<ProximityError>> {
    crate::bounded(points).map_err(NEVec::new)?;
    points
        .par_iter()
        .enumerate()
        .map(|(index, &point)| {
            let target = Vector::from(point);
            assembly
                .bvh
                .find_best(
                    max_distance.map_or(f64::INFINITY, |bound| bound.into_inner().next_up()),
                    |node, _| node.aabb().distance_to_local_point(target, true),
                    |leaf, best| {
                        let element @ (_, pose, shape) = assembly.element(leaf)?;
                        shape.project_point_with_max_dist(pose, target, false, best).map(|projection| (projection.point.distance(target), (element, projection)))
                    },
                )
                .map(|(_, (distance, ((element, pose, shape), projection)))| match shape.as_compound() {
                    Some(compound) if compound.contains_point(pose, target) => Err(ProximityError::Compound { point: index, element }),
                    Some(_) | None => Ok(signed(element, projection, distance, projection.is_inside)),
                })
                .transpose()
        })
        .collect::<Validated<Vec<_>, ProximityError>>()
        .ok()
}

pub(crate) fn signed(element: usize, projection: PointProjection, distance: f64, inside: bool) -> Projection {
    Projection { element, subshape: projection.subshape, point: Point3::from(projection.point), distance: if inside { -distance } else { distance } }
}
