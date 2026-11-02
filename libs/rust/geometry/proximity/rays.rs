use core::ops::{Bound, RangeBounds};

use itertools::Itertools as _;
use nalgebra::{Isometry3, Point3, Unit, Vector3};
use nonempty_collections::NEVec;
use parry3d_f64::{
    bounding_volume::BoundingVolume as _,
    math::{Pose, Vector},
    query::{self, Ray, RayCast as _, RayIntersection, ShapeCastHit, ShapeCastOptions, ShapeCastStatus, Unsupported},
    shape::{FeatureId, SharedShape, SubShapeId},
};
use rayon::iter::{IndexedParallelIterator, IntoParallelIterator, ParallelIterator as _};
use validated::Validated;

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

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Obstruction {
    pub element: usize,
    pub subshapes: [SubShapeId; 2],
    pub points: [Point3<f64>; 2],
    pub normal: Vector3<f64>,
    pub status: ShapeCastStatus,
    pub time_of_impact: f64,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn hits<R>(assembly: &Assembly, rays: impl Rays<R>) -> Result<Vec<Option<Hit>>, ProximityError>
where R: RangeBounds<Distance> + Send {
    cast(
        rays.into_par_iter(),
        |&origin| Some(origin),
        |ray, start, reach, _| {
            assembly
                .bvh
                .find_best(
                    reach,
                    |node, best| node.cast_ray(ray, best),
                    |leaf, best| {
                        let (element, pose, shape) = assembly.element(leaf)?;
                        shape.cast_ray_and_get_normal(pose, ray, best, false).map(|intersection| (intersection.time_of_impact, (element, intersection)))
                    },
                )
                .map(|(_, (_, (element, intersection)))| hit(ray, start, element, intersection))
        },
    )
}

pub fn occluded<R>(assembly: &Assembly, rays: impl Rays<R>) -> Result<Vec<bool>, ProximityError>
where R: RangeBounds<Distance> + Send {
    cast(
        rays.into_par_iter(),
        |&origin| Some(origin),
        |ray, _, reach, _| {
            assembly
                .bvh
                .leaves(|node| node.aabb().intersects_local_ray(ray, reach))
                .filter_map(|leaf| assembly.element(leaf))
                .any(|(_, pose, shape)| shape.cast_ray(pose, ray, reach, true).is_some_and(|time_of_impact| time_of_impact < reach))
        },
    )
}

pub fn crossings<R>(assembly: &Assembly, rays: impl Rays<R>) -> Result<Vec<Vec<Hit>>, ProximityError>
where R: RangeBounds<Distance> + Send {
    cast(
        rays.into_par_iter(),
        |&origin| Some(origin),
        |ray, start, reach, _| {
            assembly
                .bvh
                .leaves(|node| node.aabb().intersects_local_ray(ray, reach))
                .filter_map(|leaf| assembly.element(leaf))
                .filter_map(|(element, pose, shape)| {
                    shape.cast_ray_and_get_normal(pose, ray, reach, false).filter(|intersection| intersection.time_of_impact < reach).map(|intersection| hit(ray, start, element, intersection))
                })
                .sorted_by(|left, right| left.time_of_impact.total_cmp(&right.time_of_impact).then(left.element.cmp(&right.element)))
                .collect()
        },
    )
}

pub fn sweeps<R, B>(assembly: &Assembly, clearance: Distance, bodies: B) -> Result<Vec<Option<Obstruction>>, NEVec<ProximityError>>
where
    R: RangeBounds<Distance> + Send,
    B: IntoParallelIterator<Item = (Isometry3<f64>, SharedShape, Unit<Vector3<f64>>, R), Iter: IndexedParallelIterator>,
{
    let clearance = clearance.into_inner();
    cast(
        bodies.into_par_iter().enumerate().map(|(body, (pose, shape, direction, interval))| ((body, pose, shape), direction, interval)),
        |(_, pose, _)| pose.rotation.coords.iter().all(|coordinate| coordinate.is_finite()).then(|| Point3::from(pose.translation.vector)),
        |ray, start, reach, (body, pose, shape)| {
            let pose = Pose { translation: ray.origin, ..Pose::from(pose) };
            let bounds = shape.compute_aabb(&pose);
            let path = Ray::new(bounds.center(), ray.dir);
            assembly
                .bvh
                .find_best(
                    reach,
                    |node, best| node.aabb().add_half_extents(bounds.loosened(clearance).half_extents()).cast_local_ray(&path, best, true).unwrap_or(f64::MAX),
                    |leaf, best| {
                        let (element, element_pose, element_shape) = assembly.element(leaf)?;
                        match query::cast_shapes(
                            &pose,
                            ray.dir,
                            &*shape,
                            element_pose,
                            Vector::ZERO,
                            &**element_shape,
                            ShapeCastOptions { max_time_of_impact: best, target_distance: clearance, ..ShapeCastOptions::default() },
                        ) {
                            Ok(swept) => swept.map(|swept| (swept.time_of_impact, Ok((element, *element_pose, swept)))),
                            Err(Unsupported) => Some((f64::NEG_INFINITY, Err(ProximityError::Unsupported { left: body, right: element, shapes: [shape.shape_type(), element_shape.shape_type()] }))),
                        }
                    },
                )
                .map(|(_, (_, swept))| {
                    swept.map(|(element, element_pose, ShapeCastHit { time_of_impact, witness1, witness2, normal1, normal2: _, status, subshape1, subshape2 }): (usize, Pose, ShapeCastHit)| {
                        Obstruction {
                            element,
                            subshapes: [subshape1, subshape2],
                            points: [Pose { translation: ray.point_at(time_of_impact), ..pose }.transform_point(witness1), element_pose.transform_point(witness2)].map(Point3::from),
                            normal: Vector3::from(pose.transform_vector(normal1)),
                            status,
                            time_of_impact: start + time_of_impact,
                        }
                    })
                })
        },
    )
    .map_err(NEVec::new)?
    .into_iter()
    .map(Option::transpose)
    .collect::<Validated<Vec<_>, ProximityError>>()
    .ok()
}

fn cast<X, R, T>(
    rays: impl IndexedParallelIterator<Item = (X, Unit<Vector3<f64>>, R)>,
    origin: impl Fn(&X) -> Option<Point3<f64>> + Sync,
    answer: impl Fn(&Ray, f64, f64, X) -> T + Sync,
) -> Result<Vec<T>, ProximityError>
where
    X: Send,
    R: RangeBounds<Distance> + Send,
    T: Default + Send,
{
    let answers = rays
        .map(|(input, direction, interval)| {
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
            origin(&input)
                .map(|origin| origin + direction.into_inner() * start)
                .filter(|origin| crate::representable(origin) && direction.iter().all(|coordinate| coordinate.is_finite()))
                .map(|origin| if start < end { answer(&Ray::new(Vector::from(origin), Vector::from(direction.into_inner())), start, end - start, input) } else { T::default() })
        })
        .collect::<Vec<_>>();
    crate::finite(answers.iter().map(Option::is_some))?;
    Ok(answers.into_iter().flatten().collect())
}

fn hit(ray: &Ray, start: f64, element: usize, RayIntersection { time_of_impact, normal, feature, subshape }: RayIntersection) -> Hit {
    Hit { element, subshape, feature, point: Point3::from(ray.point_at(time_of_impact)), normal: Vector3::from(normal), time_of_impact: start + time_of_impact }
}
