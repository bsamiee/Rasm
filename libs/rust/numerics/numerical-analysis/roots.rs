use core::{iter, num::NonZero, ops::ControlFlow, range::RangeInclusive};

use argmin::{
    core::{CostFunction, Error, Executor},
    solver::brent::BrentRoot,
};
use basin::{BoxBounds, Constrained, ConstraintSet, ConstraintSolveError, GoldenSection, PointState};
use faer::{MatRef, linalg::evd::EvdError};
use itertools::Itertools as _;
use nalgebra::{DMatrix, DVector, DVectorView, Point1, convert, linalg::balancing::balance_parlett_reinsch};
use nonempty_collections::NEVec;
use num_complex::Complex;
use num_dual::{Dual, DualDVec64, DualNum, DualStruct, implicit_derivative, jacobian};
use rasm_stats::{Bounds, Tolerance};
use rayon::{
    iter::{IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _, once},
    slice::ParallelSlice as _,
};
use validated::Validated;

use crate::{AnalysisError, Input, RootSource, SearchHooks, TOLERANCE, refused_sites};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Root {
    pub value: Complex<f64>,
    pub error: f64,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Newton {
    pub tolerance: Tolerance = TOLERANCE,
    pub limit: NonZero<usize> = NonZero::new(50).unwrap(),
}

struct Equation<F>(F);

impl<F> basin::CostFunction for Equation<F>
where F: Fn(f64) -> Result<f64, AnalysisError>
{
    type Param = f64;
    type Output = f64;
    type Error = AnalysisError;

    fn cost(&self, param: &f64) -> Result<f64, AnalysisError> { (self.0)(*param).and_then(|value| value.is_finite().ok_or_else(|| pole(*param)).map(|()| value)) }
}

impl<F> CostFunction for Equation<F>
where F: Fn(f64) -> Result<f64, AnalysisError>
{
    type Param = f64;
    type Output = f64;

    fn cost(&self, param: &f64) -> Result<f64, Error> { basin::CostFunction::cost(self, param).map_err(Error::new) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [BRACKETS]

fn pole(at: f64) -> AnalysisError { AnalysisError::Function { interval: vec![RangeInclusive { start: at, last: at }] } }

pub fn minimum<F>(cost: F, start: f64, last: f64, tolerance: f64, hooks: SearchHooks<'_>) -> Result<(f64, f64), AnalysisError>
where F: Fn(f64) -> Result<f64, AnalysisError> {
    let bracket = RangeInclusive { start, last };
    let interval = Bounds::try_new(bracket).map_err(|source| AnalysisError::Root { bracket, source: RootSource::Bounds(source) })?;
    let absolute = (f64::EPSILON * interval.start.abs().max(interval.last.abs())).max(f64::MIN_POSITIVE * f64::EPSILON).next_up();
    let problem = Constrained::new(Equation(cost), ConstraintSet::new().with_bounds(BoxBounds::new(Some(interval.start), Some(interval.last))));
    let search = GoldenSection::new().with_relative_position_tolerance(tolerance).with_absolute_position_tolerance(absolute);
    let executor = hooks.0.cloned().into_iter().fold(basin::Executor::new(problem, search, PointState::new(f64::midpoint(interval.start, interval.last))), basin::Executor::with_cancellation_token);
    hooks
        .1
        .into_iter()
        .map(|factory| factory())
        .fold(executor, |executor, (observer, mode)| executor.observe_solver(observer, mode))
        .run()
        .map_err(|error| match error {
            ConstraintSolveError::Evaluation(error) => error,
            source => AnalysisError::Root { bracket, source: RootSource::Basin(Box::new(source)) },
        })
        .and_then(|result| if result.report.termination.is_converged() { Ok((*result.best_param(), result.best_cost())) } else { Err(AnalysisError::SearchStopped { bracket, report: result.report }) })
}

pub fn bracketed<F>(function: F, interval: Bounds, steps: NonZero<usize>, tolerance: Option<Tolerance>, hooks: SearchHooks<'_>) -> Result<Vec<f64>, NEVec<AnalysisError>>
where F: Fn(f64) -> f64 + Sync {
    let count = convert::<usize, f64>(steps.get());
    let nodes = (0..steps.get())
        .into_par_iter()
        .map(|step| Point1::new(interval.start).lerp(&Point1::new(interval.last), convert::<usize, f64>(step) / count).x)
        .chain(once(interval.last))
        .map(|at| {
            let value = function(at);
            value.is_finite().ok_or_else(|| pole(at)).map(|()| (at, value))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    let equation = |at: f64| Ok::<_, AnalysisError>(function(at));
    let pole_free = |outcome: Result<(f64, f64), AnalysisError>| match outcome {
        Ok(found) => Ok(Some(found)),
        Err(AnalysisError::Function { interval: _ }) => Ok(None),
        Err(
            error @ (AnalysisError::Shape { input: _, expected: _, found: _ }
            | AnalysisError::Site { dimension: _, point: _, placement: _ }
            | AnalysisError::Value { index: _ }
            | AnalysisError::Weight { dimension: _, point: _, weight: _ }
            | AnalysisError::Underdetermined { found: _, needed: _ }
            | AnalysisError::Outside { dimension: _, at: _ }
            | AnalysisError::Period { dimension: _, span: _, period: _ }
            | AnalysisError::Point { breakdown: _, point: _ }
            | AnalysisError::NoConvergence
            | AnalysisError::Zero
            | AnalysisError::Coefficient { index: _, coefficient: _, leading: _ }
            | AnalysisError::Root { bracket: _, source: _ }
            | AnalysisError::SearchStopped { bracket: _, report: _ }
            | AnalysisError::Tangent { root: _ }
            | AnalysisError::System { point: _, residual: _ }
            | AnalysisError::Integral { estimate: _, error: _ }
            | AnalysisError::Breakline { from: _, to: _ }
            | AnalysisError::Drift { term: _ }
            | AnalysisError::Spline(_)
            | AnalysisError::Quadrature(_)
            | AnalysisError::Triangulation(_)
            | AnalysisError::Reshape(_)),
        ) => Err(error),
    };
    let solve = |start: f64, last: f64, bound: f64| {
        let found = Executor::new(Equation(equation), BrentRoot::new(start, last, tolerance.map_or_default(Tolerance::into_inner)))
            .run()
            .map_err(|report| report.downcast::<AnalysisError>().unwrap_or_else(|source| AnalysisError::Root { bracket: RangeInclusive { start, last }, source: RootSource::Argmin(source) }))
            .and_then(|result| result.state.param.map(|at| (at, result.state.cost)).ok_or(AnalysisError::NoConvergence));
        pole_free(found).map(|found| found.and_then(|(at, cost)| (cost.abs() <= bound).then_some(at)))
    };
    let tangent = |start: f64, middle: f64, last: f64, reach: f64| match pole_free(minimum(|at| Ok(function(at).abs()), start, last, almost::F64_TOLERANCE, hooks)) {
        Ok(Some((at, _))) if function(at).is_sign_negative() != function(middle).is_sign_negative() =>
            [(start, at), (at, last)].into_iter().filter_map(|(below, above)| solve(below, above, reach).transpose()).collect(),
        Ok(Some((at, least))) if least <= almost::F64_TOLERANCE * reach => vec![Ok(at)],
        Ok(Some(_) | None) => Vec::new(),
        Err(error) => vec![Err(error)],
    };
    nodes
        .par_iter()
        .filter(|&&(_, value)| value == 0.0)
        .map(|&(at, _)| Ok(at))
        .chain(nodes.par_array_windows().filter_map(|&[(start, below), (last, above)]| {
            (below != 0.0 && above != 0.0 && below.is_sign_negative() != above.is_sign_negative()).then(|| solve(start, last, below.abs().max(above.abs())).transpose()).flatten()
        }))
        .chain(
            nodes
                .par_array_windows()
                .filter(|&&[(_, before), (_, value), (_, after)]| {
                    value != 0.0 && value.is_sign_negative() == before.is_sign_negative() && value.is_sign_negative() == after.is_sign_negative() && value.abs() <= before.abs().min(after.abs())
                })
                .flat_map_iter(|&[(start, before), (middle, _), (last, after)]| tangent(start, middle, last, before.abs().max(after.abs()))),
        )
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map(|roots| roots.into_iter().sorted_by(f64::total_cmp).dedup_by(|left, right| almost::equal(*left, *right)).collect())
}

// --- [POLYNOMIALS]

pub fn polynomial(coefficients: &[f64]) -> Result<Vec<Root>, NEVec<AnalysisError>> {
    let (origin, significant, leading) = coefficients
        .iter()
        .positions(|&coefficient| coefficient != 0.0)
        .minmax()
        .into_option()
        .and_then(|(origin, highest)| coefficients.get(origin..=highest).and_then(|significant| significant.last().map(|&leading| (origin, significant, leading))))
        .ok_or_else(|| NEVec::new(AnalysisError::Zero))?;
    significant
        .iter()
        .enumerate()
        .filter(|&(_, coefficient)| !(-coefficient / leading).is_finite())
        .map(|(index, &coefficient)| Err(AnalysisError::Coefficient { index: origin.strict_add(index), coefficient, leading }))
        .collect::<Validated<(), _>>()
        .ok()?;
    let degree = significant.len().strict_sub(1);
    let scale = (DVectorView::from_slice(significant, significant.len()).amax() / leading.abs()).log2().ceil().min(f64::from(f64::MAX_EXP) - 1.0).exp2();
    let mut companion =
        DMatrix::from_fn(degree, degree, |row, column| if row == 0 { -significant[degree.strict_sub(column).strict_sub(1)] / leading } else { f64::from(row.strict_sub(1) == column) }) / scale;
    balance_parlett_reinsch(&mut companion);
    let order = convert::<usize, f64>(degree);
    let horner = |at: Complex<f64>| significant.iter().rev().fold((Complex::<f64>::ZERO, Complex::<f64>::ZERO), |(value, slope), &coefficient| (value * at + coefficient, slope * at + value));
    MatRef::from_column_major_slice(companion.as_slice(), degree, degree)
        .eigenvalues()
        .map(|roots| {
            roots
                .into_iter()
                .map(|root| {
                    let polished = (0..2).fold(root * scale, |at, _| {
                        let (value, slope) = horner(at);
                        Some(at - value / slope).filter(|next| next.is_finite()).unwrap_or(at)
                    });
                    let (value, slope) = horner(polished);
                    let error = (order * value.norm() / slope.norm()).max(f64::EPSILON * polished.norm());
                    Root { value: polished, error: if error.is_finite() { error } else { almost::F64_TOLERANCE * polished.norm() } }
                })
                .chain(iter::repeat_n(Root { value: Complex::ZERO, error: 0.0 }, origin))
                .collect()
        })
        .map_err(|EvdError::NoConvergence| NEVec::new(AnalysisError::NoConvergence))
}

pub fn real(coefficients: &[f64], interval: RangeInclusive<f64>) -> Result<Vec<f64>, NEVec<AnalysisError>> {
    polynomial(coefficients).map(|roots| {
        roots
            .into_iter()
            .filter(|&Root { value, error }| value.im.abs() <= error && RangeInclusive { start: interval.start - error, last: interval.last + error }.contains(&value.re))
            .map(|Root { value, error: _ }| value.re.clamp(interval.start, interval.last))
            .sorted_by(f64::total_cmp)
            .dedup_by(|left, right| almost::equal(*left, *right))
            .collect()
    })
}

// --- [SYSTEMS]

pub fn system<F>(equations: F, start: DVector<f64>, newton: Newton) -> Result<DVector<f64>, AnalysisError>
where F: Fn(DVector<DualDVec64>) -> DVector<DualDVec64> {
    fn magnitude(values: impl Iterator<Item = f64>) -> f64 { values.map(f64::abs).fold(0.0_f64, f64::maximum) }
    let finite = |point: &DVector<f64>| refused_sites(point, |coordinate: &f64| coordinate.is_finite()).collect::<Result<(), _>>();
    finite(&start)?;
    let dimensions = start.len();
    let tolerance = newton.tolerance.into_inner();
    let evaluate = |point: DVector<DualDVec64>| {
        let values = equations(point);
        (values.len() == dimensions).ok_or(AnalysisError::Shape { input: Input::Equations, expected: dimensions, found: values.len() }).map(|()| values)
    };
    let residual = |point: &DVector<f64>| evaluate(point.map(DualDVec64::from_re)).map(|values| magnitude(values.iter().map(|value| value.re)));
    let outcome = (0..newton.limit.get()).try_fold(start, |point, _| {
        let (mut direction, slopes) = match jacobian(evaluate, &point) {
            Ok(linearization) => linearization,
            Err(error) => return ControlFlow::Break(Err(error)),
        };
        let current = magnitude(direction.iter().copied());
        match (current <= tolerance, current.is_finite() && slopes.iter().all(|slope| slope.is_finite())) {
            (true, _) => ControlFlow::Break(Ok(point)),
            (false, false) => ControlFlow::Break(Err(AnalysisError::System { residual: current, point })),
            (false, true) if !slopes.lu().solve_mut(&mut direction) => ControlFlow::Break(Err(AnalysisError::System { residual: current, point })),
            (false, true) => iter::successors(Some(1.0_f64), |&damping| Some(damping / 2.0).filter(|&next| next >= f64::EPSILON))
                .map(|damping| &point - &direction * damping)
                .filter(|trial| finite(trial).is_ok())
                .find_map(|trial| match residual(&trial) {
                    Ok(next) if next < current => Some(Ok(trial)),
                    Ok(_) => None,
                    Err(error) => Some(Err(error)),
                })
                .unwrap_or_else(|| Err(AnalysisError::System { residual: current, point }))
                .map_or_else(|error| ControlFlow::Break(Err(error)), ControlFlow::Continue),
        }
    });
    match outcome {
        ControlFlow::Break(solved) => solved,
        ControlFlow::Continue(point) => residual(&point).and_then(|current| if current <= tolerance { Ok(point) } else { Err(AnalysisError::System { residual: current, point }) }),
    }
}

pub fn sensitivity<G, D, A>(equation: G, finite: impl Fn(&D) -> bool, roots: &[f64], parameters: &A::Inner) -> Result<Vec<D>, NEVec<AnalysisError>>
where
    G: Fn(Dual<D>, &A) -> Dual<D>,
    D: DualNum<Primitive = f64>,
    A: DualStruct,
{
    roots
        .iter()
        .map(|&root| {
            let derived = implicit_derivative::<_, D, A>(&equation, root, parameters);
            finite(&derived).ok_or(AnalysisError::Tangent { root }).map(|()| derived)
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}
