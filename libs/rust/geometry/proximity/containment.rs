use itertools::Itertools as _;
use nalgebra::Point3;
use parry3d_f64::math::Vector;
use parry3d_f64::query::PointQuery as _;
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};

use crate::distances::{self, Projection};
use crate::{Assembly, Distance, ProximityError};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn containing(assembly: &Assembly, points: &[Point3<f64>], tolerance: Distance) -> Result<Vec<Vec<Projection>>, ProximityError> {
    let tolerance = tolerance.into_inner();
    crate::bounded(points)?;
    Ok(points
        .par_iter()
        .copied()
        .map(Vector::from)
        .map(|target| {
            assembly
                .bvh
                .leaves(|node| node.aabb().distance_to_local_point(target, true) <= tolerance)
                .filter_map(|leaf| {
                    let (element, pose, shape) = assembly.element(leaf)?;
                    let projection = shape.project_point(pose, target, false);
                    Some(distances::signed(element, projection, projection.point.distance(target), shape.contains_point(pose, target))).filter(|projection| projection.distance <= tolerance)
                })
                .sorted_by(|left, right| left.distance.total_cmp(&right.distance).then(left.element.cmp(&right.element)))
                .collect()
        })
        .collect())
}
