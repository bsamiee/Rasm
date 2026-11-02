use nalgebra::Point3;
use parry3d_f64::{
    math::Vector,
    query::{PointProjection, PointQuery as _},
    shape::SubShapeId,
};
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};

use crate::{Assembly, ProximityError};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Projection {
    pub element: usize,
    pub subshape: SubShapeId,
    pub point: Point3<f64>,
    pub distance: f64,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn project(assembly: &Assembly, points: &[Point3<f64>], max_distance: f64) -> Result<Vec<Option<Projection>>, ProximityError> {
    crate::finite(points.iter().map(|&point| Vector::from(point).is_finite()))?;
    Ok(points
        .par_iter()
        .copied()
        .map(Vector::from)
        .map(|point| {
            assembly
                .bvh
                .find_best(
                    max_distance.next_up(),
                    |node, _| node.aabb().distance_to_local_point(point, true),
                    |leaf, best| {
                        assembly
                            .elements
                            .get(leaf as usize)
                            .and_then(|(pose, shape)| shape.project_point_with_max_dist(pose, point, false, best))
                            .map(|projection| (projection.point.distance(point), projection))
                    },
                )
                .map(|(leaf, (_, projection))| signed(leaf as usize, point, projection, projection.is_inside))
        })
        .collect())
}

pub(crate) fn signed(element: usize, target: Vector, projection: PointProjection, inside: bool) -> Projection {
    let distance = projection.point.distance(target);
    Projection { element, subshape: projection.subshape, point: Point3::from(projection.point), distance: if inside { -distance } else { distance } }
}
