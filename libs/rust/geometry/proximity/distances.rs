use nalgebra::Point3;
use nonempty_collections::NEVec;
use parry3d_f64::{
    math::Vector,
    query::{PointProjection, PointQuery as _},
    shape::SubShapeId,
};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Assembly, Distance, Placed, ProximityError};

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
    let nearest = |target: Vector, max_cost: f64| {
        assembly.bvh.find_best(
            max_cost,
            |node, _| node.aabb().distance_to_local_point(target, true),
            |leaf, best| {
                let element @ (_, pose, shape, _) = assembly.element(leaf)?;
                shape.project_point_with_max_dist(pose, target, false, best).map(|projection| (projection.point.distance(target), (element, projection)))
            },
        )
    };
    points
        .par_iter()
        .enumerate()
        .map(|(index, &point)| {
            let target = Vector::from(point);
            max_distance
                .map_or_else(|| nearest(target, f64::INFINITY), |bound| nearest(target, bound.into_inner().next_up()))
                .map(|(_, (distance, (element, projection)))| signed(index, element, target, distance, projection))
                .transpose()
        })
        .collect::<Validated<Vec<_>, ProximityError>>()
        .ok()
}

pub(crate) fn signed(point: usize, (element, pose, shape, _): Placed<'_>, target: Vector, distance: f64, projection: PointProjection) -> Result<Projection, ProximityError> {
    match shape.as_compound() {
        Some(compound) if compound.contains_point(pose, target) => Err(ProximityError::Compound { point, element }),
        Some(_) | None => Ok(Projection { element, subshape: projection.subshape, point: Point3::from(projection.point), distance: if projection.is_inside { -distance } else { distance } }),
    }
}
