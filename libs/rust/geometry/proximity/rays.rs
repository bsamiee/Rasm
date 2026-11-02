use core::ops::{Bound, RangeBounds};

use nalgebra::{Point3, Unit, Vector3};
use parry3d_f64::{
    math::Vector,
    query::{Ray, RayCast as _, RayIntersection},
    shape::{FeatureId, SubShapeId},
};
use rayon::iter::{IndexedParallelIterator, IntoParallelIterator, ParallelIterator as _};

use crate::{Assembly, Distance, ProximityError};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Rays<R> = IntoParallelIterator<Item = (Point3<f64>, Unit<Vector3<f64>>, R), Iter: IndexedParallelIterator>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Hit {
    pub element: usize,
    pub subshape: SubShapeId,
    pub feature: FeatureId,
    pub point: Point3<f64>,
    pub normal: Vector3<f64>,
    pub time_of_impact: f64,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn hits<R>(assembly: &Assembly, rays: impl Rays<R>) -> Result<Vec<Option<Hit>>, ProximityError>
where R: RangeBounds<Distance> + Send {
    cast(rays, |ray, start, reach| {
        assembly
            .bvh
            .find_best(reach, |node, best| node.cast_ray(ray, best), |leaf, best| assembly.elements.get(leaf as usize).and_then(|(pose, shape)| shape.cast_ray_and_get_normal(pose, ray, best, false)))
            .map(|(leaf, RayIntersection { time_of_impact, normal, feature, subshape })| Hit {
                element: leaf as usize,
                subshape,
                feature,
                point: Point3::from(ray.point_at(time_of_impact)),
                normal: Vector3::from(normal),
                time_of_impact: start + time_of_impact,
            })
    })
}

pub fn occluded<R>(assembly: &Assembly, rays: impl Rays<R>) -> Result<Vec<bool>, ProximityError>
where R: RangeBounds<Distance> + Send {
    cast(rays, |ray, _, reach| {
        assembly
            .bvh
            .leaves(|node| node.aabb().intersects_local_ray(ray, reach))
            .any(|leaf| assembly.elements.get(leaf as usize).and_then(|(pose, shape)| shape.cast_ray(pose, ray, reach, true)).is_some_and(|time_of_impact| time_of_impact < reach))
    })
}

fn cast<R, T>(rays: impl Rays<R>, answer: impl Fn(&Ray, f64, f64) -> T + Sync) -> Result<Vec<T>, ProximityError>
where
    R: RangeBounds<Distance> + Send,
    T: Send,
{
    let answers = rays
        .into_par_iter()
        .map(|(origin, direction, interval)| {
            let start = match interval.start_bound().map(|start| start.into_inner()) {
                Bound::Included(start) => start,
                Bound::Excluded(start) => start.next_up(),
                Bound::Unbounded => 0.0,
            };
            let end = match interval.end_bound().map(|end| end.into_inner()) {
                Bound::Included(end) => end.next_up(),
                Bound::Excluded(end) => end,
                Bound::Unbounded => f64::INFINITY,
            };
            let ray = Ray::new(Vector::from(origin + direction.into_inner() * start), Vector::from(direction.into_inner()));
            (ray.origin.is_finite() && ray.dir.is_finite()).then(|| answer(&ray, start, end - start))
        })
        .collect::<Vec<_>>();
    crate::finite(answers.iter().map(Option::is_some))?;
    Ok(answers.into_iter().flatten().collect())
}
