use itertools::Itertools as _;
use nalgebra::Point3;
use nonempty_collections::NEVec;
use parry3d_f64::{math::Vector, query::PointQuery as _};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{
    Assembly, Distance, ProximityError,
    distances::{self, Projection},
};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn containing(assembly: &Assembly, points: &[Point3<f64>], tolerance: Distance) -> Result<Vec<Vec<Projection>>, NEVec<ProximityError>> {
    let tolerance = tolerance.into_inner();
    crate::bounded(points).map_err(NEVec::new)?;
    points
        .par_iter()
        .enumerate()
        .map(|(index, &point)| {
            let target = Vector::from(point);
            assembly
                .bvh
                .leaves(|node| node.aabb().distance_to_local_point(target, true) <= tolerance)
                .filter_map(|leaf| assembly.element(leaf))
                .map(|element @ (_, pose, shape, _)| {
                    let projection = shape.project_point(pose, target, false);
                    distances::signed(index, element, target, projection.point.distance(target), projection)
                })
                .filter_ok(|projection| projection.distance <= tolerance)
                .collect::<Result<Vec<_>, ProximityError>>()
                .map(|projections| projections.into_iter().sorted_by(|left, right| left.distance.total_cmp(&right.distance).then(left.element.cmp(&right.element))).collect())
        })
        .collect::<Validated<Vec<_>, ProximityError>>()
        .ok()
}
