use core::{borrow::Borrow, iter, num::NonZero};

use faer::{
    MatRef, Side,
    linalg::{cholesky::llt::factor::LltError, solvers::Solve as _},
};
use itertools::{Either, Itertools as _};
use nalgebra::{Isometry3, Matrix3, Matrix6, Point3, Rotation3, Translation3, Unit, UnitQuaternion, Vector3, Vector6};
use nonempty_collections::{NESlice, NEVec};
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};

use crate::{
    Assembly, Distance, ProximityError, Shortfall,
    distances::{self, Projection},
    points::{self, Cloud, Neighbor, Neighborhood},
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
pub enum Metric {
    Point,
    Plane(Neighborhood),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Kernel {
    Huber(Distance),
    Tukey(Distance),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Icp {
    pub iterations: NonZero<usize> = NonZero::new(50).unwrap(),
    pub tolerance: Distance = Distance::try_new(1e-6_f64).ok().unwrap(),
    pub max_correspondence_distance: Option<Distance> = None,
    pub overlap: Option<Overlap> = None,
    pub kernel: Option<Kernel> = None,
    pub metric: Metric = Metric::Point,
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
pub fn fit<I>(pairs: I) -> Option<Isometry3<f64>>
where I: IntoIterator<Item: Borrow<(Point3<f64>, Point3<f64>, f64)>, IntoIter: Clone> {
    let pairs = pairs.into_iter();
    let total = pairs.clone().map(|pair| pair.borrow().2).sum::<f64>();
    let source = pairs.clone().map(|pair| pair.borrow().0.coords * pair.borrow().2).sum::<Vector3<f64>>() / total;
    let target = pairs.clone().map(|pair| pair.borrow().1.coords * pair.borrow().2).sum::<Vector3<f64>>() / total;
    let covariance = pairs
        .map(|pair| {
            let &(point, corresponding, weight) = pair.borrow();
            (point.coords - source) * (corresponding.coords - target).transpose() * weight
        })
        .sum::<Matrix3<f64>>();
    let svd = MatRef::from_column_major_slice(covariance.as_slice(), 3, 3).svd().ok()?;
    let singular = svd.S().column_vector();
    (*singular.get(1) > *singular.get(0) * f64::EPSILON * nalgebra::convert::<usize, f64>(covariance.nrows())).then(|| {
        let [left, right] = [svd.U(), svd.V()].map(|factor| Matrix3::from_fn(|row, column| *factor.get(row, column)));
        let rotation = UnitQuaternion::from_rotation_matrix(&Rotation3::from_matrix_unchecked(
            right * Matrix3::from_diagonal(&Vector3::new(1.0_f64, 1.0_f64, (right * left.transpose()).determinant().signum())) * left.transpose(),
        ));
        Isometry3::from_parts(Translation3::from(target - rotation * source), rotation)
    })
}

pub fn register(
    target: Target<'_>,
    source: NESlice<'_, Point3<f64>>,
    initial: Isometry3<f64>,
    Icp { iterations, tolerance, max_correspondence_distance, overlap, kernel, metric }: Icp,
) -> impl Iterator<Item = Result<Registration, NEVec<ProximityError>>> {
    let kept = overlap.map(|overlap| nalgebra::convert_unchecked::<f64, usize>((overlap.into_inner() * nalgebra::convert::<usize, f64>(source.len().get())).ceil()));
    let directions =
        |normal: Option<Unit<Vector3<f64>>>| normal.map_or_else(|| Either::Right([Vector3::x_axis(), Vector3::y_axis(), Vector3::z_axis()].into_iter()), |normal| Either::Left(iter::once(normal)));
    let robust = move |residual: f64| match kernel {
        None => 1.0,
        Some(Kernel::Huber(scale)) if residual <= scale.into_inner() => 1.0,
        Some(Kernel::Huber(scale)) => scale.into_inner() / residual,
        Some(Kernel::Tukey(scale)) if residual < scale.into_inner() => (residual / scale.into_inner()).mul_add(-(residual / scale.into_inner()), 1.0).powi(2),
        Some(Kernel::Tukey(_)) => 0.0,
    };
    let step = move |pose: Isometry3<f64>, iteration: NonZero<usize>| -> Result<Registration, NEVec<ProximityError>> {
        let transformed = source.as_ref().par_iter().map(|point| pose.transform_point(point)).collect::<Vec<_>>();
        let candidates = match target {
            Target::Cloud(cloud) => {
                let found = itertools::izip!(source.as_ref(), &transformed, points::nearest(cloud, &transformed, max_correspondence_distance).map_err(NEVec::new)?)
                    .filter_map(|(&point, &moved, neighbor)| neighbor.map(|Neighbor { point: corresponding, item: _, distance }| (point, moved, corresponding, distance)))
                    .collect::<Vec<_>>();
                let normals = match metric {
                    Metric::Point => None,
                    Metric::Plane(neighborhood) => Some(points::normals(cloud, &found.iter().map(|&(_, _, corresponding, _)| corresponding).collect::<Vec<_>>(), neighborhood).map_err(NEVec::new)?),
                };
                iter::zip(found, normals.into_iter().flatten().chain(iter::repeat(None)))
                    .map(|((point, moved, corresponding, distance), normal)| (point, moved, corresponding, normal, distance))
                    .collect::<Vec<_>>()
            }
            Target::Assembly(assembly) => itertools::izip!(source.as_ref(), &transformed, distances::project(assembly, &transformed, max_correspondence_distance)?)
                .filter_map(|(&point, &moved, projection)| {
                    projection.map(|Projection { element: _, subshape: _, point: corresponding, distance }| {
                        (point, moved, corresponding, matches!(metric, Metric::Plane(_)).then(|| Unit::try_new(moved - corresponding, 0.0)).flatten(), distance.abs())
                    })
                })
                .collect(),
        };
        let pairs = match kept {
            None => candidates.into_iter(),
            Some(count) => candidates.into_iter().k_smallest_by(count, |left, right| left.4.total_cmp(&right.4)),
        }
        .map(|(point, moved, corresponding, normal, distance)| (point, moved, corresponding, normal, robust(distance)))
        .collect::<Vec<_>>();
        let shortfall = |shortfall| NEVec::new(ProximityError::Unfitted { iteration, shortfall });
        let (matched, fitted) = NonZero::new(pairs.len()).ok_or_else(|| shortfall(Shortfall::Unmatched)).and_then(|matched| {
            match metric {
                Metric::Point => fit(pairs.iter().map(|&(point, _, corresponding, _, weight)| (point, corresponding, weight))).ok_or_else(|| shortfall(Shortfall::Degenerate)),
                Metric::Plane(_) => {
                    let (hessian, gradient) = pairs
                        .iter()
                        .flat_map(|&(_, moved, corresponding, normal, weight)| {
                            directions(normal).map(move |direction| {
                                (Vector6::from_iterator(iter::chain(moved.coords.cross(&direction).iter(), direction.iter()).copied()), (moved - corresponding).dot(&direction), weight)
                            })
                        })
                        .fold((Matrix6::<f64>::zeros(), Vector6::<f64>::zeros()), |(hessian, gradient), (row, residual, weight)| {
                            (hessian + row * row.transpose() * weight, gradient - row * (residual * weight))
                        });
                    MatRef::from_column_major_slice(hessian.as_slice(), 6, 6).llt(Side::Lower).map_err(|LltError::NonPositivePivot { index: _ }| shortfall(Shortfall::Degenerate)).map(|llt| {
                        let solution = llt.solve(MatRef::from_column_major_slice(gradient.as_slice(), 6, 1));
                        let increment = Vector6::from_fn(|row, _| *solution.get(row, 0));
                        let rotation = UnitQuaternion::from_scaled_axis(increment.fixed_rows::<3>(0).into_owned());
                        Isometry3::from_parts(Translation3::from(rotation * pose.translation.vector + increment.fixed_rows::<3>(3)), rotation * pose.rotation)
                    })
                }
            }
            .map(|fitted| (matched, fitted))
        })?;
        let [squared, total] = pairs.iter().fold([0.0_f64; 2], |[squared, total], &(point, _, corresponding, normal, weight)| {
            let offset = fitted.transform_point(&point) - corresponding;
            [directions(normal).map(|direction| offset.dot(&direction).powi(2)).sum::<f64>().mul_add(weight, squared), total + weight]
        });
        Ok(Registration { pose: fitted, rms: (squared / total).sqrt(), matched, iterations: iteration, converged: false })
    };
    let first = iter::chain(initial.translation.vector.iter(), initial.rotation.coords.iter())
        .all(|coordinate| coordinate.is_finite())
        .ok_or(NEVec::new(ProximityError::Pose { pose: initial }))
        .and_then(|()| step(initial, NonZero::<usize>::MIN));
    iter::successors(Some(first), move |previous| match previous {
        Ok(Registration { pose, rms, matched, iterations: done, converged: false }) => done
            .checked_add(1)
            .filter(|&iteration| iteration <= iterations)
            .map(|iteration| step(*pose, iteration).map(|next| Registration { converged: next.matched == *matched && (rms - next.rms).abs() <= tolerance.into_inner(), ..next })),
        Ok(Registration { pose: _, rms: _, matched: _, iterations: _, converged: true }) | Err(_) => None,
    })
}
