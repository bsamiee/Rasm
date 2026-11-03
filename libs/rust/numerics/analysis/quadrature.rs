use core::{
    f64::consts::{FRAC_PI_2, PI, TAU},
    iter,
    num::NonZero,
    ops::ControlFlow,
    range::RangeInclusive,
};
use std::collections::BinaryHeap;

use gauss_quad::{FiniteAboveNegOneF64, GaussHermite, GaussJacobi, GaussLaguerre, GaussLegendre};
use itertools::Itertools as _;
use nalgebra::{DVector, DVectorView, MatrixXx3, Rotation3, UnitVector3, Vector3};
use nonempty_collections::{IntoNonEmptyIterator, NEVec, NonEmptyIterator as _};
use num_dual::DualNum;
use ordered_float::{FloatIsNan, NotNan};
use rasm_stats::{Bounds, Tolerance};
use validated::Validated;

use crate::{AnalysisError, Input, TOLERANCE, joined};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub enum Domain {
    Box(NEVec<Bounds>),
    Triangle(MatrixXx3<f64>),
    Hemisphere(UnitVector3<f64>),
    Sphere,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Weight {
    Jacobi { interval: Bounds, alpha: FiniteAboveNegOneF64, beta: FiniteAboveNegOneF64 },
    Laguerre { alpha: FiniteAboveNegOneF64 },
    Hermite,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Settings {
    pub degree: NonZero<usize> = NonZero::new(10).unwrap(),
    pub absolute: Option<Tolerance> = None,
    pub relative: Tolerance = TOLERANCE,
    pub limit: NonZero<usize> = NonZero::new(50).unwrap(),
}

#[derive(Debug, Clone, PartialEq)]
pub struct Integral<T> {
    pub estimate: DVector<T>,
    pub error: f64,
    pub evaluations: usize,
}

struct Piece<T> {
    domain: Domain,
    lower: DVector<f64>,
    upper: DVector<f64>,
    depth: usize,
    estimate: DVector<T>,
}

struct Refinement<T> {
    pieces: Vec<Option<Piece<T>>>,
    heap: BinaryHeap<(NotNan<f64>, usize)>,
    estimate: DVector<T>,
    error: f64,
    evaluations: usize,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn extent(domain: &Domain) -> Vec<(f64, f64)> {
    match domain {
        Domain::Box(bounds) => bounds.iter().map(|bounds| (bounds.start, bounds.last)).collect(),
        Domain::Triangle(_) => vec![(0.0, 1.0), (0.0, 1.0)],
        Domain::Hemisphere(_) => vec![(0.0, FRAC_PI_2), (0.0, TAU)],
        Domain::Sphere => vec![(0.0, PI), (0.0, TAU)],
    }
}

fn chart(domain: &Domain, coordinates: &[f64]) -> (DVector<f64>, f64) {
    let direction = |polar: f64, azimuth: f64| Vector3::new(polar.sin() * azimuth.cos(), polar.sin() * azimuth.sin(), polar.cos());
    match (domain, coordinates) {
        (Domain::Triangle(vertices), &[along, across]) => {
            let (first, second) = (vertices.column(1) - vertices.column(0), vertices.column(2) - vertices.column(0));
            let area = first.norm_squared().mul_add(second.norm_squared(), -first.dot(&second).powi(2)).max(0.0).sqrt();
            (vertices.column(0) + &first * along + &second * ((1.0 - along) * across), area * (1.0 - along))
        }
        (Domain::Hemisphere(axis), &[polar, azimuth]) => {
            let rotation = Rotation3::rotation_between(&Vector3::z(), &axis.into_inner()).unwrap_or_else(|| Rotation3::from_axis_angle(&Vector3::x_axis(), PI));
            (DVector::from_column_slice(rotation.transform_vector(&direction(polar, azimuth)).as_slice()), polar.sin())
        }
        (Domain::Sphere, &[polar, azimuth]) => (DVector::from_column_slice(direction(polar, azimuth).as_slice()), polar.sin()),
        (Domain::Box(_), _) | (Domain::Triangle(_) | Domain::Hemisphere(_) | Domain::Sphere, [] | [_] | [_, _, _, ..]) => (DVector::from_column_slice(coordinates), 1.0),
    }
}

fn magnitude<T>(values: &DVector<T>) -> f64
where T: DualNum<Primitive = f64> {
    values.iter().map(|value| value.re().abs()).fold(0.0, f64::max)
}

fn converged<T>(estimate: &DVector<T>, error: f64, settings: Settings) -> bool
where T: DualNum<Primitive = f64> {
    let size = magnitude(estimate);
    size.is_finite() && error <= settings.absolute.map_or_default(Tolerance::into_inner).max(settings.relative.into_inner() * size)
}

fn accepted<T>(integral: Integral<T>, settings: Settings) -> Result<Integral<T>, AnalysisError>
where T: DualNum<Primitive = f64> {
    if converged(&integral.estimate, integral.error, settings) { Ok(integral) } else { Err(AnalysisError::Integral { estimate: magnitude(&integral.estimate), error: integral.error }) }
}

fn summed<T>(outputs: usize, nodes: impl IntoIterator<Item = (DVector<T>, f64)>, interval: impl FnOnce() -> Vec<RangeInclusive<f64>>) -> Result<DVector<T>, AnalysisError>
where T: DualNum<Primitive = f64> {
    let sum = nodes.into_iter().try_fold(DVector::<T>::zeros(outputs), |sum, (output, mass)| {
        (output.len() == outputs).ok_or(AnalysisError::Shape { input: Input::Integrand, expected: outputs, found: output.len() }).map(|()| sum + output * T::from(mass))
    })?;
    sum.iter().all(|entry| entry.re().is_finite()).ok_or_else(|| AnalysisError::Function { interval: interval() }).map(|()| sum)
}

pub fn integrate<F, T, I>(integrand: F, parts: I, settings: Settings) -> Result<Integral<T>, NEVec<AnalysisError>>
where
    F: Fn(DVectorView<'_, f64>) -> DVector<T>,
    T: DualNum<Primitive = f64>,
    I: IntoNonEmptyIterator<Item = Domain>,
{
    let parts = parts.into_nonempty_iter().collect::<NEVec<_>>();
    let rule = GaussLegendre::new(settings.degree);
    let probe = extent(parts.first());
    let (origin, _) = chart(parts.first(), &probe.iter().map(|&(start, last)| f64::midpoint(start, last)).collect::<Vec<_>>());
    let outputs = integrand(origin.column(0)).len();
    let measured = |domain: &Domain, lower: &DVector<f64>, upper: &DVector<f64>| -> Result<(DVector<T>, usize), AnalysisError> {
        let nodes = iter::zip(lower.iter(), upper.iter())
            .map(|(&start, &last)| {
                let (middle, half) = (f64::midpoint(start, last), last / 2.0 - start / 2.0);
                rule.iter().map(move |&(node, weight)| (half.mul_add(node, middle), weight * half))
            })
            .multi_cartesian_product()
            .map(|nodes| {
                let (point, jacobian) = chart(domain, &nodes.iter().map(|&(coordinate, _)| coordinate).collect::<Vec<_>>());
                (integrand(point.column(0)), nodes.iter().map(|&(_, weight)| weight).product::<f64>() * jacobian)
            });
        let interval = || iter::zip(lower.iter(), upper.iter()).map(|(&start, &last)| RangeInclusive { start, last }).collect();
        summed(outputs, nodes, interval).map(|sum| (sum, iter::repeat_n(rule.degree(), lower.len()).fold(1, usize::saturating_mul)))
    };
    let split = |piece: &Piece<T>| {
        let axis = piece.depth.checked_rem(piece.lower.len()).unwrap_or_default();
        let (left_upper, right_lower): (Vec<_>, Vec<_>) = iter::zip(piece.lower.iter(), piece.upper.iter())
            .enumerate()
            .map(|(index, (&start, &last))| {
                let middle = f64::midpoint(start, last);
                if index == axis { (middle, middle) } else { (last, start) }
            })
            .unzip();
        let (left_upper, right_lower) = (DVector::from_vec(left_upper), DVector::from_vec(right_lower));
        Validated::from(measured(&piece.domain, &piece.lower, &left_upper))
            .map2(Validated::from(measured(&piece.domain, &right_lower, &piece.upper)), |(left_integral, left_count), (right_integral, right_count)| {
                let deviation = magnitude(&(&piece.estimate - (&left_integral + &right_integral)));
                let child = |lower: DVector<f64>, upper: DVector<f64>, estimate: DVector<T>| Piece { domain: piece.domain.clone(), lower, upper, depth: piece.depth.strict_add(1), estimate };
                ([child(piece.lower.clone(), left_upper, left_integral), child(right_lower, piece.upper.clone(), right_integral)], deviation, left_count.saturating_add(right_count))
            })
            .ok()
    };
    let insert = |state: &mut Refinement<T>, (halves, deviation, count): ([Piece<T>; 2], f64, usize), parent: Option<(NotNan<f64>, &Piece<T>)>| -> Result<(), AnalysisError> {
        let share = NotNan::new(deviation / 2.0).map_err(|FloatIsNan| AnalysisError::Integral { estimate: magnitude(&state.estimate), error: deviation })?;
        let (removed, released) = parent.map_or_else(|| (DVector::<T>::zeros(state.estimate.len()), 0.0), |(held, piece)| (piece.estimate.clone(), held.into_inner()));
        state.estimate = halves.iter().fold(&state.estimate - removed, |total, half| total + &half.estimate);
        state.error = state.error - released + deviation;
        state.evaluations = state.evaluations.saturating_add(count);
        state.heap.extend([0, 1].map(|offset| (share, state.pieces.len().strict_add(offset))));
        state.pieces.extend(halves.map(Some));
        Ok(())
    };
    let seeded = joined(parts.into_iter().map(|domain| {
        let (lower, upper): (Vec<_>, Vec<_>) = extent(&domain).into_iter().unzip();
        let (lower, upper) = (DVector::from_vec(lower), DVector::from_vec(upper));
        let (estimate, whole) = measured(&domain, &lower, &upper).map_err(NEVec::new)?;
        split(&Piece { domain, lower, upper, depth: 0, estimate }).map(|(halves, deviation, count)| (halves, deviation, count.saturating_add(whole)))
    }))?;
    let mut state = Refinement { pieces: Vec::new(), heap: BinaryHeap::new(), estimate: DVector::<T>::zeros(outputs), error: 0.0, evaluations: 1 };
    seeded.into_iter().try_for_each(|halves| insert(&mut state, halves, None)).map_err(NEVec::new)?;
    (state.pieces.len()..settings.limit.get())
        .try_for_each(|_| {
            let widest = (!converged(&state.estimate, state.error, settings))
                .then(|| state.heap.pop())
                .flatten()
                .and_then(|(share, index)| state.pieces.get_mut(index).and_then(Option::take).map(|piece| (share, piece)));
            match widest.map(|(share, piece)| split(&piece).and_then(|halves| insert(&mut state, halves, Some((share, &piece))).map_err(NEVec::new))) {
                None => ControlFlow::Break(Ok(())),
                Some(Ok(())) => ControlFlow::Continue(()),
                Some(Err(errors)) => ControlFlow::Break(Err(errors)),
            }
        })
        .break_value()
        .transpose()?;
    let Refinement { pieces: _, heap: _, estimate, error, evaluations } = state;
    accepted(Integral { estimate, error, evaluations }, settings).map_err(NEVec::new)
}

pub fn weighted<F, T>(integrand: F, weight: Weight, settings: Settings) -> Result<Integral<T>, AnalysisError>
where
    F: Fn(f64) -> DVector<T>,
    T: DualNum<Primitive = f64>,
{
    let span = match weight {
        Weight::Jacobi { interval, alpha: _, beta: _ } => RangeInclusive { start: interval.start, last: interval.last },
        Weight::Laguerre { alpha: _ } => RangeInclusive { start: 0.0, last: f64::INFINITY },
        Weight::Hermite => RangeInclusive { start: f64::NEG_INFINITY, last: f64::INFINITY },
    };
    let measured = |degree: NonZero<usize>, width: Option<usize>| -> Result<(DVector<T>, usize), AnalysisError> {
        let nodes = match weight {
            Weight::Jacobi { interval, alpha, beta } => {
                let half = interval.last / 2.0 - interval.start / 2.0;
                GaussJacobi::new(degree, alpha, beta).iter().map(|&(node, mass)| (half.mul_add(node, f64::midpoint(interval.start, interval.last)), mass * half)).collect::<Vec<_>>()
            }
            Weight::Laguerre { alpha } => GaussLaguerre::new(degree, alpha).iter().copied().collect(),
            Weight::Hermite => GaussHermite::new(degree).iter().copied().collect(),
        };
        let outputs = nodes.iter().map(|&(node, mass)| (integrand(node), mass)).collect::<Vec<_>>();
        let width = width.or_else(|| outputs.first().map(|(output, _)| output.len())).unwrap_or_default();
        summed(width, outputs, || vec![span]).map(|sum| (sum, degree.get()))
    };
    let doubled = |degree: &NonZero<usize>| degree.checked_mul(NonZero::<usize>::MIN.saturating_add(1)).filter(|next| next.get() <= settings.degree.get().saturating_mul(settings.limit.get()));
    let (coarse, cost) = measured(settings.degree, None)?;
    let width = Some(coarse.len());
    iter::successors(doubled(&settings.degree), doubled)
        .try_fold(Integral { estimate: coarse, error: f64::INFINITY, evaluations: cost }, |current, degree| {
            match measured(degree, width).map(|(estimate, count)| {
                let error = magnitude(&(&current.estimate - &estimate));
                Integral { estimate, error, evaluations: current.evaluations.saturating_add(count) }
            }) {
                Err(error) => ControlFlow::Break(Err(error)),
                Ok(next) if converged(&next.estimate, next.error, settings) => ControlFlow::Break(Ok(next)),
                Ok(next) => ControlFlow::Continue(next),
            }
        })
        .map_continue(|last| accepted(last, settings))
        .into_value()
}
