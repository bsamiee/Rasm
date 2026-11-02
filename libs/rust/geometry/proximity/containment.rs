use itertools::Itertools as _;
use nalgebra::Point3;
use parry3d_f64::{math::Vector, query::PointQuery as _};
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};

use crate::{
    Assembly, Distance, ProximityError,
    distances::{self, Projection},
};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn containing(assembly: &Assembly, points: &[Point3<f64>], tolerance: Distance) -> Result<Vec<Vec<Projection>>, ProximityError> {
    let tolerance = tolerance.into_inner();
    crate::finite(points.iter().map(|&point| Vector::from(point).is_finite()))?;
    Ok(points
        .par_iter()
        .copied()
        .map(Vector::from)
        .map(|target| {
            assembly
                .bvh
                .leaves(|node| node.aabb().distance_to_local_point(target, true) <= tolerance)
                .map(|leaf| leaf as usize)
                .filter_map(|element| {
                    let (pose, shape) = assembly.elements.get(element)?;
                    Some(distances::signed(element, target, shape.project_point(pose, target, false), shape.contains_point(pose, target))).filter(|projection| projection.distance <= tolerance)
                })
                .sorted_by(|left, right| left.distance.total_cmp(&right.distance).then(left.element.cmp(&right.element)))
                .collect()
        })
        .collect())
}
