use core::{borrow::Borrow, iter, num::NonZero};
use std::sync::OnceLock;

use faer::{
    MatRef, Side,
    linalg::{cholesky::llt::factor::LltError, solvers::Solve as _},
};
use itertools::{Either, Itertools as _};
use nalgebra::{Isometry3, Matrix3, Matrix6, Point3, Rotation3, Translation3, Unit, UnitQuaternion, Vector3, Vector6};
use nonempty_collections::{NESlice, NEVec};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};

use crate::{
    Assembly, Distance, ProximityError, Shortfall,
    distances::{self, Projection},
    points::{self, Cloud, Neighbor, Neighborhood},
};

// --- [TYPES] ---------------------------------------------------------------------------

type Correspondence = (Point3<f64>, Point3<f64>, Point3<f64>, Option<Unit<Vector3<f64>>>, Weight);

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(const_fn, validate(greater = 0.0, less = 1.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Fraction(f64);

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Weight(f64);

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
    pub tolerance: Fraction = Fraction::try_new(1e-6_f64).ok().unwrap(),
    pub max_correspondence_distance: Option<Distance> = None,
    pub overlap: Option<Fraction> = None,
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
where I: IntoIterator<Item: Borrow<(Point3<f64>, Point3<f64>, Weight)>, IntoIter: Clone> {
    let pairs = pairs.into_iter();
    let (total, source, target) = pairs.clone().fold((0.0_f64, Vector3::<f64>::zeros(), Vector3::<f64>::zeros()), |(total, source, target), pair| {
        let &(point, corresponding, weight) = pair.borrow();
        (total + weight.into_inner(), source + point.coords * weight.into_inner(), target + corresponding.coords * weight.into_inner())
    });
    let (source, target) = (total.is_finite() && total > 0.0).then(|| (source / total, target / total))?;
    let covariance = pairs
        .map(|pair| {
            let &(point, corresponding, weight) = pair.borrow();
            (point.coords - source) * (corresponding.coords - target).transpose() * weight.into_inner()
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
    let normals = iter::repeat_with(OnceLock::<Option<Unit<Vector3<f64>>>>::new)
        .take(match (target, metric) {
            (Target::Cloud(cloud), Metric::Plane(_)) => cloud.as_ref().size(),
            (Target::Cloud(_), Metric::Point) | (Target::Assembly(_), Metric::Point | Metric::Plane(_)) => 0,
        })
        .collect::<Vec<_>>();
    let directions =
        |normal: Option<Unit<Vector3<f64>>>| normal.map_or_else(|| Either::Right([Vector3::x_axis(), Vector3::y_axis(), Vector3::z_axis()].into_iter()), |normal| Either::Left(iter::once(normal)));
    let robust = move |residual: f64| {
        kernel.map_or(1.0, |kernel| match kernel {
            Kernel::Huber(scale) => (scale.into_inner() / residual).min(1.0),
            Kernel::Tukey(scale) => (residual / scale.into_inner()).mul_add(-(residual / scale.into_inner()), 1.0).max(0.0).powi(2),
        })
    };
    let planar = move |pose: &Isometry3<f64>, pairs: &[Correspondence]| {
        let (hessian, gradient) = pairs
            .iter()
            .flat_map(|&(_, moved, corresponding, normal, weight)| {
                directions(normal).map(move |direction| {
                    (Vector6::from_iterator(iter::chain(moved.coords.cross(&direction).iter(), direction.iter()).copied()), (moved - corresponding).dot(&direction), weight.into_inner())
                })
            })
            .fold((Matrix6::<f64>::zeros(), Vector6::<f64>::zeros()), |(hessian, gradient), (row, residual, weight)| (hessian + row * row.transpose() * weight, gradient - row * (residual * weight)));
        MatRef::from_column_major_slice(hessian.as_slice(), 6, 6).llt(Side::Lower).map(|llt| {
            let solution = llt.solve(MatRef::from_column_major_slice(gradient.as_slice(), 6, 1));
            let increment = Vector6::from_fn(|row, _| *solution.get(row, 0));
            let rotation = UnitQuaternion::from_scaled_axis(increment.fixed_rows::<3>(0).into_owned());
            Isometry3::from_parts(Translation3::from(rotation * pose.translation.vector + increment.fixed_rows::<3>(3)), rotation * pose.rotation)
        })
    };
    let step = move |pose: Isometry3<f64>, iteration: NonZero<usize>| -> Result<Registration, NEVec<ProximityError>> {
        let transformed = source.as_ref().par_iter().map(|point| pose.transform_point(point)).collect::<Vec<_>>();
        let candidates = match target {
            Target::Cloud(cloud) => {
                let found = match max_correspondence_distance {
                    None => Either::Left(points::nearest(cloud, &transformed).map_err(NEVec::new)?.into_iter().map(Some)),
                    Some(radius) => Either::Right(
                        points::neighbors(cloud, &transformed, Neighborhood::NearestWithin { max_qty: NonZero::<usize>::MIN, radius }).map_err(NEVec::new)?.into_iter().map(|group| group.into_iter().next()),
                    ),
                }
                .collect::<Vec<_>>();
                source
                    .as_ref()
                    .par_iter()
                    .zip(&transformed)
                    .zip(found)
                    .filter_map(|((&point, &moved), neighbor)| {
                        let Neighbor { point: corresponding, item, distance } = neighbor?;
                        let normal = match metric {
                            Metric::Point => None,
                            Metric::Plane(neighborhood) => normals.get(item).and_then(|normal| {
                                *normal.get_or_init(|| points::search(cloud, &[corresponding], neighborhood).first().and_then(|group| points::normal(group, None)))
                            }),
                        };
                        Some((point, moved, corresponding, normal, distance))
                    })
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
        .filter_map(|(point, moved, corresponding, normal, distance)| match Weight::try_new(robust(distance)) {
            Ok(weight) => Some((point, moved, corresponding, normal, weight)),
            Err(WeightError::GreaterViolated | WeightError::FiniteViolated) => None,
        })
        .collect::<Vec<Correspondence>>();
        let shortfall = |shortfall| NEVec::new(ProximityError::Unfitted { iteration, shortfall });
        let (matched, fitted) = NonZero::new(pairs.len()).ok_or_else(|| shortfall(Shortfall::Unmatched)).and_then(|matched| {
            match metric {
                Metric::Point => fit(pairs.iter().map(|&(point, _, corresponding, _, weight)| (point, corresponding, weight))).ok_or_else(|| shortfall(Shortfall::Degenerate)),
                Metric::Plane(_) => planar(&pose, &pairs).map_err(|LltError::NonPositivePivot { index: _ }| shortfall(Shortfall::Degenerate)),
            }
            .map(|fitted| (matched, fitted))
        })?;
        let [squared, total] = pairs.iter().fold([0.0_f64; 2], |[squared, total], &(point, _, corresponding, normal, weight)| {
            let offset = fitted.transform_point(&point) - corresponding;
            [directions(normal).map(|direction| offset.dot(&direction).powi(2)).sum::<f64>().mul_add(weight.into_inner(), squared), total + weight.into_inner()]
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
            .map(|iteration| step(*pose, iteration).map(|next| Registration { converged: next.matched == *matched && (rms - next.rms).abs() <= tolerance.into_inner() * rms, ..next })),
        Ok(Registration { pose: _, rms: _, matched: _, iterations: _, converged: true }) | Err(_) => None,
    })
}
