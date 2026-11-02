use core::{iter, num::NonZero, ops::ControlFlow};

use faer::MatRef;
use itertools::{Either, Itertools as _};
use nalgebra::{Isometry3, Matrix3, Point3, Rotation3, Translation3, UnitQuaternion, Vector3};
use nonempty_collections::NESlice;
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};

use crate::{
    Assembly, ProximityError, distances,
    points::{self, Cloud},
};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(greater = 0.0, less = 1.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Overlap(f64);

#[derive(Debug, Clone, Copy)]
pub enum Target<'target> {
    Cloud(&'target Cloud),
    Assembly(&'target Assembly),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Icp {
    pub iterations: NonZero<usize> = NonZero::<usize>::saturating_add(NonZero::<usize>::MIN, 49),
    pub tolerance: f64 = 1e-6_f64,
    pub max_correspondence_distance: f64 = f64::INFINITY,
    pub overlap: Option<Overlap> = None,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Registration {
    pub pose: Isometry3<f64>,
    pub rms: f64,
    pub matched: NonZero<usize>,
    pub iterations: NonZero<usize>,
    pub converged: bool,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn fit(pairs: &[(Point3<f64>, Point3<f64>)]) -> Option<Isometry3<f64>> {
    let count = nalgebra::convert::<usize, f64>(NonZero::new(pairs.len())?.get());
    let source = pairs.iter().map(|(point, _)| point.coords).sum::<Vector3<f64>>() / count;
    let target = pairs.iter().map(|(_, corresponding)| corresponding.coords).sum::<Vector3<f64>>() / count;
    let covariance = pairs.iter().map(|(point, corresponding)| (point.coords - source) * (corresponding.coords - target).transpose()).sum::<Matrix3<f64>>();
    let svd = MatRef::from_column_major_slice(covariance.as_slice(), 3, 3).svd().ok()?;
    let singular = svd.S().column_vector();
    (*singular.get(1) > *singular.get(0) * f64::EPSILON * nalgebra::convert::<usize, f64>(covariance.nrows()))
        .then(|| {
            let [left, right] = [svd.U(), svd.V()].map(|factor| Matrix3::from_fn(|row, column| *factor.get(row, column)));
            let rotation = UnitQuaternion::from_rotation_matrix(&Rotation3::from_matrix_unchecked(
                right * Matrix3::from_diagonal(&Vector3::new(1.0_f64, 1.0_f64, (right * left.transpose()).determinant().signum())) * left.transpose(),
            ));
            Isometry3::from_parts(Translation3::from(target - rotation * source), rotation)
        })
        .filter(|pose| pose.translation.vector.iter().all(|coordinate| coordinate.is_finite()))
}

pub fn register(
    target: Target<'_>,
    source: &NESlice<'_, Point3<f64>>,
    initial: Isometry3<f64>,
    Icp { iterations, tolerance, max_correspondence_distance, overlap }: Icp,
) -> Result<Registration, ProximityError> {
    let kept = overlap.map(|overlap| nalgebra::convert_unchecked::<f64, usize>((overlap.into_inner() * nalgebra::convert::<usize, f64>(source.len().get())).ceil()));
    let step = |pose: &Isometry3<f64>, iteration: NonZero<usize>| {
        let moved = source.as_ref().par_iter().map(|point| pose.transform_point(point)).collect::<Vec<_>>();
        let candidates = match target {
            Target::Cloud(cloud) => Either::Left(iter::zip(source, points::nearest(cloud, &moved)?).map(|(&point, neighbour)| (point, neighbour.point, neighbour.distance))),
            Target::Assembly(assembly) => Either::Right(
                iter::zip(source, distances::project(assembly, &moved, max_correspondence_distance)?)
                    .filter_map(|(&point, projection)| projection.map(|projection| (point, projection.point, projection.distance.abs()))),
            ),
        }
        .filter(|&(_, _, distance)| distance <= max_correspondence_distance);
        let pairs = match kept {
            None => candidates.map(|(point, corresponding, _)| (point, corresponding)).collect::<Vec<_>>(),
            Some(count) => candidates.k_smallest_by(count, |(_, _, left), (_, _, right)| left.total_cmp(right)).map(|(point, corresponding, _)| (point, corresponding)).collect(),
        };
        let (matched, fitted) = NonZero::new(pairs.len()).zip(fit(&pairs)).ok_or(ProximityError::Unmatched { iteration })?;
        Ok(Registration {
            pose: fitted,
            rms: (pairs.iter().map(|(point, corresponding)| nalgebra::distance_squared(&fitted.transform_point(point), corresponding)).sum::<f64>() / nalgebra::convert::<usize, f64>(matched.get()))
                .sqrt(),
            matched,
            iterations: iteration,
            converged: false,
        })
    };
    (const { NonZero::new(2).unwrap() }..=iterations)
        .try_fold(step(&initial, NonZero::<usize>::MIN)?, |previous, iteration| match step(&previous.pose, iteration) {
            Err(error) => ControlFlow::Break(Err(error)),
            Ok(next) if next.matched == previous.matched && (previous.rms - next.rms).abs() <= tolerance * previous.rms => ControlFlow::Break(Ok(Registration { converged: true, ..next })),
            Ok(next) => ControlFlow::Continue(next),
        })
        .map_continue(Ok)
        .into_value()
}
