use core::{
    f64::consts::{FRAC_PI_2, PI, TAU},
    iter,
    num::NonZero,
    ops::ControlFlow,
    range::RangeInclusive,
};
use std::collections::BTreeMap;

use bilby::{GaussHermite, GaussJacobi, GaussLaguerre, GaussLegendre, QuadratureRule};
use dashu::{
    base::BitTest as _,
    float::{Context, FBig, Repr, round::mode::HalfEven},
    integer::IBig,
};
use itertools::{Either, Itertools as _};
use libm::frexp;
use nalgebra::{DVector, DVectorView, MatrixXx3, Rotation3, UnitVector3, Vector2, Vector3, convert};
use nonempty_collections::{IntoNonEmptyIterator, NEVec, NonEmptyIterator as _, nev};
use num_dual::DualNum;
use num_traits::Float as _;
use ordered_float::{FloatIsNan, NotNan};
use rasm_stats::{Bounds, Tolerance};
use validated::Validated;

use crate::{AnalysisError, Input, TOLERANCE, joined, refused_sites};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub enum Domain {
    Box(NEVec<Bounds>),
    Triangle(MatrixXx3<f64>),
    Hemisphere(UnitVector3<f64>),
    Sphere,
}

#[nutype::nutype(validate(finite, greater = -1.0), derive(Debug, Clone, Copy, PartialEq))]
pub struct Exponent(f64);

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Weight {
    Jacobi { interval: Bounds, alpha: Exponent, beta: Exponent },
    Laguerre { alpha: Exponent },
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

enum Chart {
    Cartesian,
    Triangle { affine: Either<MatrixXx3<f64>, MatrixXx3<FBig<HalfEven>>>, area: (f64, isize) },
    Spherical(Option<Rotation3<f64>>),
}

impl Chart {
    fn buffer(&self, dimensions: usize) -> DVector<f64> {
        DVector::zeros(match self {
            Self::Cartesian => dimensions,
            Self::Triangle { affine, area: _ } => affine.as_ref().either(MatrixXx3::nrows, MatrixXx3::nrows),
            Self::Spherical(_) => 3,
        })
    }
}

struct Piece<'chart, T> {
    chart: &'chart Chart,
    lower: NEVec<f64>,
    upper: NEVec<f64>,
    depth: usize,
    estimate: DVector<T>,
}

struct Subdivision<'chart, T> {
    halves: [Piece<'chart, T>; 2],
    error: f64,
    evaluations: usize,
}

struct Refinement<'chart, T> {
    pieces: BTreeMap<(NotNan<f64>, usize), Piece<'chart, T>>,
    next: usize,
    estimate: DVector<T>,
    error: f64,
    evaluations: usize,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn mass(factors: impl IntoIterator<Item = (f64, isize)>) -> (f64, isize) {
    factors.into_iter().fold((1.0, 0_isize), |(mantissa, exponent), (factor, shift)| {
        let (factor, carry) = frexp(factor);
        let (mantissa, offset) = frexp(mantissa * factor);
        (mantissa, exponent.strict_add(shift).strict_add(carry.strict_cast::<isize>()).strict_add(offset.strict_cast::<isize>()))
    })
}

fn half_width(start: f64, last: f64) -> (f64, (f64, isize)) {
    let half = last / 2.0 - start / 2.0;
    let (mantissa, exponent) = frexp(last - start);
    (half, (mantissa, exponent.strict_sub(1).strict_cast::<isize>()))
}

fn magnitude<T>(values: &DVector<T>, norm: &impl Fn(&T) -> f64) -> f64
where T: DualNum<Primitive = f64> {
    values.iter().map(norm).fold(0.0, f64::maximum)
}

fn converged<T>(estimate: &DVector<T>, error: f64, settings: Settings, norm: &impl Fn(&T) -> f64) -> bool
where T: DualNum<Primitive = f64> {
    let size = magnitude(estimate, norm);
    size.is_finite() && error <= settings.absolute.map_or_default(Tolerance::into_inner).max(settings.relative.into_inner() * size)
}

fn accepted<T>(integral: Integral<T>, settings: Settings, norm: &impl Fn(&T) -> f64) -> Result<Integral<T>, AnalysisError>
where T: DualNum<Primitive = f64> {
    if converged(&integral.estimate, integral.error, settings, norm) { Ok(integral) } else { Err(AnalysisError::Integral { estimate: magnitude(&integral.estimate, norm), error: integral.error }) }
}

fn summed<T>(
    outputs: usize,
    nodes: impl IntoIterator<Item = (DVector<T>, (f64, isize))>,
    interval: impl Fn() -> Vec<RangeInclusive<f64>>,
    norm: &impl Fn(&T) -> f64,
) -> Result<DVector<T>, AnalysisError>
where
    T: DualNum<Primitive = f64>,
{
    #[expect(clippy::arithmetic_side_effects, reason = "Clippy cannot match arithmetic allowances through DualNum bounds")]
    fn scaled<T>(values: DVector<T>, (mantissa, exponent): (f64, isize)) -> DVector<T>
    where T: DualNum<Primitive = f64> {
        let extent = if exponent > 0 { f64::MAX_EXP.strict_sub(1) } else { f64::MIN_EXP }.strict_cast::<isize>();
        let first = if exponent > 0 { exponent.min(extent) } else { exponent.max(extent) };
        let values = {
            let factor = mantissa * convert::<isize, f64>(first).exp2();
            values.apply_into(|value| *value *= factor)
        };
        let remaining = exponent.strict_sub(first).unsigned_abs();
        let chunk = extent.unsigned_abs();
        (0..remaining.div_ceil(chunk)).fold(values, |values, index| {
            let factor = (convert::<usize, f64>(remaining.strict_sub(index.strict_mul(chunk)).min(chunk)) * convert::<isize, f64>(extent.signum())).exp2();
            values.apply_into(|value| *value *= factor)
        })
    }
    let sum = nodes.into_iter().try_fold(DVector::<T>::zeros(outputs), |sum, (output, weight)| {
        (output.len() == outputs)
            .ok_or(AnalysisError::Shape { input: Input::Integrand, expected: outputs, found: output.len() })
            .and_then(|()| magnitude(&output, norm).is_finite().ok_or_else(|| AnalysisError::Function { interval: interval() }))
            .map(|()| sum + scaled(output, weight))
    })?;
    magnitude(&sum, norm).is_finite().ok_or_else(|| AnalysisError::Function { interval: interval() }).map(|()| sum)
}

pub fn integrate<F, T, I>(integrand: F, parts: I, settings: Settings, norm: impl Fn(&T) -> f64) -> Result<Integral<T>, NEVec<AnalysisError>>
where
    F: Fn(DVectorView<'_, f64>) -> DVector<T>,
    T: DualNum<Primitive = f64>,
    I: IntoNonEmptyIterator<Item = Domain>,
{
    fn split<'chart, T>(
        piece: &Piece<'chart, T>,
        measured: &impl Fn(&Chart, &NEVec<f64>, &NEVec<f64>) -> Result<(DVector<T>, usize), AnalysisError>,
        norm: &impl Fn(&T) -> f64,
    ) -> Result<Subdivision<'chart, T>, NEVec<AnalysisError>>
    where
        T: DualNum<Primitive = f64>,
    {
        let axis = piece.depth % piece.lower.len();
        let (left_upper, right_lower) = piece
            .lower
            .nonempty_iter()
            .zip(&piece.upper)
            .enumerate()
            .map(|(index, (&start, &last))| {
                let middle = f64::midpoint(start, last);
                if index == axis { (middle, middle) } else { (last, start) }
            })
            .unzip();
        Validated::from(measured(piece.chart, &piece.lower, &left_upper))
            .map2(Validated::from(measured(piece.chart, &right_lower, &piece.upper)), |(left_integral, left_count), (right_integral, right_count)| {
                let error = magnitude(&(&piece.estimate - (&left_integral + &right_integral)), norm);
                let child = |lower: NEVec<f64>, upper: NEVec<f64>, estimate: DVector<T>| Piece { chart: piece.chart, lower, upper, depth: piece.depth.strict_add(1), estimate };
                Subdivision {
                    halves: [child(piece.lower.clone(), left_upper, left_integral), child(right_lower, piece.upper.clone(), right_integral)],
                    error,
                    evaluations: left_count.saturating_add(right_count),
                }
            })
            .ok()
    }
    fn insert<'chart, T>(
        state: &mut Refinement<'chart, T>,
        Subdivision { halves, error, evaluations }: Subdivision<'chart, T>,
        parent: Option<(NotNan<f64>, &Piece<'chart, T>)>,
        norm: &impl Fn(&T) -> f64,
    ) -> Result<(), AnalysisError>
    where
        T: DualNum<Primitive = f64>,
    {
        let share = NotNan::new(error / 2.0).map_err(|FloatIsNan| AnalysisError::Integral { estimate: magnitude(&state.estimate, norm), error })?;
        let released = match parent {
            Some((held, piece)) => {
                state.estimate -= &piece.estimate;
                held.into_inner()
            }
            None => 0.0,
        };
        for half in &halves {
            state.estimate += &half.estimate;
        }
        state.error = state.error - released + error;
        state.evaluations = state.evaluations.saturating_add(evaluations);
        let next = state.next.strict_add(halves.len());
        state.pieces.extend(halves.into_iter().enumerate().map(|(offset, piece)| ((share, state.next.strict_add(offset)), piece)));
        state.next = next;
        Ok(())
    }
    #[expect(clippy::arithmetic_side_effects, reason = "Clippy cannot express finite f64 decoding into Dashu integers")]
    let exact = |value: f64| {
        let (mantissa, exponent, sign) = value.integer_decode();
        FBig::from_repr_const(Repr::new(IBig::from(mantissa) * sign, isize::from(exponent)))
    };
    let chart = |chart: &Chart, nodes: &[(f64, (f64, isize))], point: &mut DVector<f64>| match chart {
        Chart::Cartesian => {
            iter::zip(point.iter_mut(), nodes).for_each(|(coordinate, &(node, _))| *coordinate = node);
            (1.0, 0)
        }
        Chart::Triangle { affine, area } => {
            let (along, across) = (nodes[0].0, nodes[1].0);
            match affine {
                Either::Left(affine) => {
                    point.copy_from(&affine.column(0));
                    point.gemv(1.0, &affine.fixed_columns::<2>(1), &Vector2::new(along, (1.0 - along) * across), 1.0);
                }
                Either::Right(affine) => {
                    let along = exact(along);
                    #[expect(clippy::arithmetic_side_effects, reason = "Clippy cannot express finite, bounded-exponent Dashu arithmetic")]
                    let coordinates = affine * Vector3::new(FBig::ONE, along.clone(), (FBig::ONE - along) * exact(across));
                    point.zip_apply(&coordinates, |coordinate, exact| *coordinate = exact.to_f64().value());
                }
            }
            mass([*area, (1.0 - along, 0)])
        }
        Chart::Spherical(rotation) => {
            let ((radius, height), (sideways, forward)) = (nodes[0].0.sin_cos(), nodes[1].0.sin_cos());
            let direction = Vector3::new(radius * forward, radius * sideways, height);
            point.copy_from(&rotation.as_ref().map_or(direction, |rotation| rotation.transform_vector(&direction)));
            (radius, 0)
        }
    };
    let parts = parts.into_nonempty_iter().collect::<NEVec<_>>();
    parts
        .iter()
        .flat_map(|part| match part {
            Domain::Triangle(vertices) => Either::Left(refused_sites(vertices, |coordinate: &f64| coordinate.is_finite())),
            Domain::Box(_) | Domain::Hemisphere(_) | Domain::Sphere => Either::Right(iter::empty()),
        })
        .collect::<Validated<(), _>>()
        .ok()?;
    let parts = parts
        .into_nonempty_iter()
        .map(|domain| match domain {
            Domain::Box(bounds) => (Chart::Cartesian, bounds.into_nonempty_iter().map(|bounds| (bounds.start, bounds.last)).collect::<NEVec<_>>()),
            Domain::Triangle(vertices) => {
                let mut affine = vertices.map(exact);
                let (origin, mut edges) = affine.columns_range_pair_mut(0, 1..);
                edges.column_iter_mut().for_each(|mut edge| edge -= &origin);
                let (first, second) = (affine.column(1), affine.column(2));
                #[expect(clippy::arithmetic_side_effects, reason = "Clippy cannot express finite, bounded-exponent Dashu arithmetic")]
                let determinant = first.dot(&first) * second.dot(&second) - first.dot(&second).sqr();
                let context = Context::<HalfEven>::new(convert(f64::MANTISSA_DIGITS));
                let area = context.unwrap_fp(context.sqrt(determinant.repr()));
                let exponent = area.repr().exponent().strict_add(area.repr().significand().bit_len().strict_cast::<isize>());
                #[expect(clippy::arithmetic_side_effects, reason = "Clippy cannot express finite, bounded-exponent Dashu arithmetic")]
                let area = ((area >> exponent).to_f64().value(), exponent);
                let coordinates = affine.map(|coordinate| coordinate.to_f64().value());
                let affine = if coordinates.iter().all(|coordinate| coordinate.is_finite()) { Either::Left(coordinates) } else { Either::Right(affine) };
                (Chart::Triangle { affine, area }, nev![(0.0, 1.0), (0.0, 1.0)])
            }
            Domain::Hemisphere(axis) => (
                Chart::Spherical(Some(Rotation3::rotation_between(&Vector3::z(), &axis.into_inner()).unwrap_or_else(|| Rotation3::from_axis_angle(&Vector3::x_axis(), PI)))),
                nev![(0.0, FRAC_PI_2), (0.0, TAU)],
            ),
            Domain::Sphere => (Chart::Spherical(None), nev![(0.0, PI), (0.0, TAU)]),
        })
        .collect::<NEVec<_>>();
    let rule = GaussLegendre::new(settings.degree.get()).map_err(|source| NEVec::new(source.into()))?;
    let outputs = {
        let (mapping, bounds) = parts.first();
        let nodes = bounds.iter().map(|&(start, last)| (f64::midpoint(start, last), (1.0, 0))).collect::<Vec<_>>();
        let mut origin = mapping.buffer(nodes.len());
        chart(mapping, &nodes, &mut origin);
        let probe = integrand(origin.column(0));
        magnitude(&probe, &norm).is_finite().ok_or_else(|| NEVec::new(AnalysisError::Function { interval: bounds.iter().map(|&(start, last)| RangeInclusive { start, last }).collect() }))?;
        probe.len()
    };
    let measured = |mapping: &Chart, lower: &NEVec<f64>, upper: &NEVec<f64>| -> Result<(DVector<T>, usize), AnalysisError> {
        let mut point = mapping.buffer(lower.len().get());
        let nodes = iter::zip(lower.iter(), upper.iter())
            .map(|(&start, &last)| {
                let (middle, (half, width)) = (f64::midpoint(start, last), half_width(start, last));
                iter::zip(rule.nodes(), rule.weights()).map(move |(&node, &weight)| (half.mul_add(node, middle), mass([(weight, 0), width])))
            })
            .multi_cartesian_product()
            .map(|nodes| {
                let jacobian = chart(mapping, &nodes, &mut point);
                (integrand(point.column(0)), mass(nodes.iter().map(|&(_, weight)| weight).chain(iter::once(jacobian))))
            });
        let interval = || iter::zip(lower.iter(), upper.iter()).map(|(&start, &last)| RangeInclusive { start, last }).collect();
        summed(outputs, nodes, interval, &norm).map(|sum| (sum, iter::repeat_n(rule.order(), lower.len().get()).fold(1, usize::saturating_mul)))
    };
    let seeded = joined(parts.iter().map(|(mapping, bounds)| {
        let (lower, upper) = bounds.nonempty_iter().copied().unzip();
        let (estimate, whole) = measured(mapping, &lower, &upper).map_err(NEVec::new)?;
        split(&Piece { chart: mapping, lower, upper, depth: 0, estimate }, &measured, &norm)
            .map(|subdivision| Subdivision { evaluations: subdivision.evaluations.saturating_add(whole), ..subdivision })
    }))?;
    let mut state = Refinement { pieces: BTreeMap::new(), next: 0, estimate: DVector::<T>::zeros(outputs), error: 0.0, evaluations: 1 };
    seeded.into_iter().try_for_each(|halves| insert(&mut state, halves, None, &norm)).map_err(NEVec::new)?;
    (state.pieces.len()..settings.limit.get())
        .try_for_each(|_| {
            let widest = (!converged(&state.estimate, state.error, settings, &norm)).then(|| state.pieces.pop_last()).flatten().map(|((share, _), piece)| (share, piece));
            match widest.map(|(share, piece)| split(&piece, &measured, &norm).and_then(|halves| insert(&mut state, halves, Some((share, &piece)), &norm).map_err(NEVec::new))) {
                None => ControlFlow::Break(Ok(())),
                Some(Ok(())) => ControlFlow::Continue(()),
                Some(Err(errors)) => ControlFlow::Break(Err(errors)),
            }
        })
        .break_value()
        .transpose()?;
    let Refinement { pieces: _, next: _, estimate, error, evaluations } = state;
    accepted(Integral { estimate, error, evaluations }, settings, &norm).map_err(NEVec::new)
}

pub fn weighted<F, T>(integrand: F, weighting: Weight, settings: Settings, norm: impl Fn(&T) -> f64) -> Result<Integral<T>, AnalysisError>
where
    F: Fn(f64) -> DVector<T>,
    T: DualNum<Primitive = f64>,
{
    let span = match weighting {
        Weight::Jacobi { interval, alpha: _, beta: _ } => RangeInclusive { start: interval.start, last: interval.last },
        Weight::Laguerre { alpha: _ } => RangeInclusive { start: 0.0, last: f64::INFINITY },
        Weight::Hermite => RangeInclusive { start: f64::NEG_INFINITY, last: f64::INFINITY },
    };
    let measured = |degree: NonZero<usize>, width: Option<usize>| -> Result<(DVector<T>, usize), AnalysisError> {
        let (QuadratureRule { nodes, weights }, scale, shift) = match weighting {
            Weight::Jacobi { interval, alpha, beta } =>
                (GaussJacobi::new(degree.get(), alpha.into_inner(), beta.into_inner())?.rule().clone(), half_width(interval.start, interval.last), f64::midpoint(interval.start, interval.last)),
            Weight::Laguerre { alpha } => (GaussLaguerre::new(degree.get(), alpha.into_inner())?.rule().clone(), (1.0, (1.0, 0)), 0.0),
            Weight::Hermite => (GaussHermite::new(degree.get())?.rule().clone(), (1.0, (1.0, 0)), 0.0),
        };
        let mut evaluated = iter::zip(nodes, weights).map(|(node, weight)| (integrand(node.mul_add(scale.0, shift)), mass([(weight, 0), scale.1]))).peekable();
        let width = width.or_else(|| evaluated.peek().map(|(first, _)| first.len())).unwrap_or_default();
        summed(width, evaluated, || vec![span], &norm).map(|sum| (sum, degree.get()))
    };
    let doubled = |degree: &NonZero<usize>| degree.checked_mul(const { NonZero::new(2).unwrap() }).filter(|next| next.get() <= settings.degree.get().saturating_mul(settings.limit.get()));
    let (coarse, cost) = measured(settings.degree, None)?;
    let width = Some(coarse.len());
    iter::successors(doubled(&settings.degree), doubled)
        .try_fold(Integral { estimate: coarse, error: f64::INFINITY, evaluations: cost }, |current, degree| {
            match measured(degree, width).map(|(estimate, count)| {
                let error = magnitude(&(&current.estimate - &estimate), &norm);
                Integral { estimate, error, evaluations: current.evaluations.saturating_add(count) }
            }) {
                Err(error) => ControlFlow::Break(Err(error)),
                Ok(next) if converged(&next.estimate, next.error, settings, &norm) => ControlFlow::Break(Ok(next)),
                Ok(next) => ControlFlow::Continue(next),
            }
        })
        .map_continue(|last| accepted(last, settings, &norm))
        .into_value()
}
