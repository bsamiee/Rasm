use core::borrow::Borrow;
use core::iter;
use core::num::NonZero;
use std::sync::OnceLock;

use itertools::{Either, Itertools as _};
use nalgebra::{Isometry3, Matrix3, Matrix6, Point3, Rotation3, SVD, Translation3, Unit, UnitQuaternion, Vector3, Vector6};
use nonempty_collections::{NESlice, NEVec};
use rayon::iter::{IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};

use crate::distances::{self, Projection};
use crate::points::{self, Cloud, Neighbor, Neighborhood};
use crate::{Assembly, Distance, ProximityError, Shortfall};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(greater = 0.0, less = 1.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
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
    pub iterations: NonZero<usize>,
    pub tolerance: Fraction,
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

#[derive(Debug, Clone, Copy)]
struct Correspondence<'point> {
    point: &'point Point3<f64>,
    moved: &'point Point3<f64>,
    corresponding: Point3<f64>,
    normal: Option<Unit<Vector3<f64>>>,
    weight: Weight,
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
    let SVD { u: left, v_t: right, singular_values } = covariance.svd(true, true);
    let (left, right) = left.zip(right.map(|transposed| transposed.transpose()))?;
    (singular_values.y > singular_values.x * f64::EPSILON * nalgebra::convert::<usize, f64>(covariance.nrows())).then(|| {
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
    let kept = overlap.map(|overlap| (overlap.into_inner() * nalgebra::convert::<usize, f64>(source.len().get())).ceil().to_int_saturating::<usize>());
    let normals = iter::repeat_with(OnceLock::<Option<Unit<Vector3<f64>>>>::new)
        .take(match (target, metric) {
            (Target::Cloud(cloud), Metric::Plane(_)) => cloud.as_ref().size(),
            (Target::Cloud(_), Metric::Point) | (Target::Assembly(_), Metric::Point | Metric::Plane(_)) => 0,
        })
        .collect::<Vec<_>>();
    let directions =
        |normal: Option<Unit<Vector3<f64>>>| normal.map_or_else(|| Either::Right([Vector3::x_axis(), Vector3::y_axis(), Vector3::z_axis()].into_iter()), |normal| Either::Left(iter::once(normal)));
    let robust = move |residual: f64| match kernel {
        None => 1.0,
        Some(Kernel::Huber(scale)) => (scale.into_inner() / residual).min(1.0),
        Some(Kernel::Tukey(scale)) => (residual / scale.into_inner()).mul_add(-(residual / scale.into_inner()), 1.0).max(0.0).powi(2),
    };
    let planar = move |pose: &Isometry3<f64>, pairs: &[Correspondence<'_>]| {
        let (hessian, gradient) = pairs
            .iter()
            .flat_map(|&Correspondence { point: _, moved, corresponding, normal, weight }| {
                directions(normal).map(move |direction| {
                    (Vector6::from_iterator(iter::chain(moved.coords.cross(&direction).iter(), direction.iter()).copied()), (moved - corresponding).dot(&direction), weight.into_inner())
                })
            })
            .fold((Matrix6::<f64>::zeros(), Vector6::<f64>::zeros()), |(hessian, gradient), (row, residual, weight)| (hessian + row * row.transpose() * weight, gradient - row * (residual * weight)));
        hessian.cholesky().map(|llt| {
            let increment = llt.solve(&gradient);
            let rotation = UnitQuaternion::from_scaled_axis(increment.fixed_rows::<3>(0).into_owned());
            Isometry3::from_parts(Translation3::from(rotation * pose.translation.vector + increment.fixed_rows::<3>(3)), rotation * pose.rotation)
        })
    };
    let step = move |pose: Isometry3<f64>, iteration: NonZero<usize>| -> Result<Registration, NEVec<ProximityError>> {
        let transformed = source.as_ref().par_iter().map(|point| pose.transform_point(point)).collect::<Vec<_>>();
        let candidates = match target {
            Target::Cloud(cloud) => {
                let found = match max_correspondence_distance {
                    None => Either::Left(points::nearest(cloud, &transformed).map_err(NEVec::new)?.into_par_iter().map(Some)),
                    Some(radius) => Either::Right(
                        points::neighbors(cloud, &transformed, Neighborhood::NearestWithin { max_qty: NonZero::<usize>::MIN, radius })
                            .map_err(NEVec::new)?
                            .into_par_iter()
                            .map(|group| group.into_iter().next()),
                    ),
                };
                (source.as_ref(), &transformed, found)
                    .into_par_iter()
                    .map(|(point, moved, neighbor)| {
                        let Neighbor { point: corresponding, item, distance } = neighbor?;
                        let normal = match metric {
                            Metric::Point => None,
                            Metric::Plane(neighborhood) => *normals[item].get_or_init(|| points::normal(&points::search(cloud, &[corresponding], neighborhood)[0], None)),
                        };
                        Some((point, moved, corresponding, normal, normal.map_or(distance, |normal| (moved - corresponding).dot(&normal).abs())))
                    })
                    .flatten()
                    .collect::<Vec<_>>()
            }
            Target::Assembly(assembly) => itertools::izip!(source.as_ref(), &transformed, distances::project(assembly, &transformed, max_correspondence_distance)?)
                .filter_map(|(point, moved, projection)| {
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
            Ok(weight) => Some(Correspondence { point, moved, corresponding, normal, weight }),
            Err(WeightError::GreaterViolated | WeightError::FiniteViolated) => None,
        })
        .collect::<Vec<Correspondence<'_>>>();
        let shortfall = |shortfall| NEVec::new(ProximityError::Unfitted { iteration, shortfall });
        let (matched, fitted) = NonZero::new(pairs.len()).ok_or_else(|| shortfall(Shortfall::Unmatched)).and_then(|matched| {
            match metric {
                Metric::Point =>
                    fit(pairs.iter().map(|&Correspondence { point, moved: _, corresponding, normal: _, weight }| (*point, corresponding, weight))).ok_or_else(|| shortfall(Shortfall::Degenerate)),
                Metric::Plane(_) => planar(&pose, &pairs).ok_or_else(|| shortfall(Shortfall::Degenerate)),
            }
            .map(|fitted| (matched, fitted))
        })?;
        let [squared, total] = pairs.iter().fold([0.0_f64; 2], |[squared, total], &Correspondence { point, moved: _, corresponding, normal, weight }| {
            let offset = fitted.transform_point(point) - corresponding;
            [directions(normal).map(|direction| offset.dot(&direction).powi(2)).sum::<f64>().mul_add(weight.into_inner(), squared), total + weight.into_inner()]
        });
        Ok(Registration { pose: fitted, rms: (squared / total).sqrt(), matched, iterations: iteration, converged: false })
    };
    let first = try bikeshed Result<_, NEVec<ProximityError>> {
        iter::chain(initial.translation.vector.iter(), initial.rotation.coords.iter()).all(|coordinate| coordinate.is_finite()).ok_or(NEVec::new(ProximityError::Pose { pose: initial }))?;
        step(initial, NonZero::<usize>::MIN)?
    };
    iter::successors(Some(first), move |previous| match previous {
        Ok(Registration { pose, rms, matched, iterations: done, converged: false }) => done
            .checked_add(1)
            .filter(|&iteration| iteration <= iterations)
            .map(|iteration| step(*pose, iteration).map(|next| Registration { converged: next.matched == *matched && (rms - next.rms).abs() <= tolerance.into_inner() * rms, ..next })),
        Ok(Registration { pose: _, rms: _, matched: _, iterations: _, converged: true }) | Err(_) => None,
    })
}
