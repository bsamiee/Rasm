#![feature(cmp_minmax, const_convert, const_result_trait_fn, const_trait_impl, control_flow_into_value, default_field_values)]

use core::{cell::LazyCell, cmp, iter, num::NonZero, ops::ControlFlow, range::RangeInclusive};

use basin::{
    ArctanLoss, BoxBounds, CauchyLoss, ConstraintDescription, ConstraintSet, ConstraintSolveError, Constraints, Executor, HuberLoss, Jacobian, LossFunction, OptimizationResult, Residual,
    RobustLeastSquares, SoftL1Loss, SquaredLoss, State as _, TerminationReport, TrustRegionReflective,
};
use faer::{MatRef, linalg::svd::SvdError};
use itertools::{Itertools as _, izip};
use nalgebra::{DMatrix, DMatrixView, DVector, DVectorView, convert};
use nonempty_collections::NEVec;
use num_dual::{Dual2DVec64, DualDVec64, DualNum, gradient, hessian, jacobian};
use rasm_numerical_analysis::{AnalysisError, Input, Observations, Placement, Polynomial, Prediction, RootSource, SearchHooks, design, roots::minimum};
use rasm_stats::{Bounds, Confidence, Space, Tolerance};
use rayon::iter::{Either, IndexedParallelIterator as _, ParallelIterator as _};
use serde::{Deserialize, Serialize};
use statrs::{
    distribution::{ContinuousCDF as _, Normal, StudentsT, StudentsTError},
    function::factorial::binomial,
};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------


pub trait Model: Sync {
    fn evaluate<D>(&self, site: DVectorView<'_, D>, parameters: DVectorView<'_, D>) -> D
    where D: DualNum<Primitive = f64>;
}

pub trait Separable: Sync {
    fn terms(&self) -> usize;

    fn basis<D>(&self, site: DVectorView<'_, D>, shape: DVectorView<'_, D>, term: usize) -> D
    where D: DualNum<Primitive = f64>;
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Regularization {
    Truncated,
    Tikhonov(Tolerance),
    Gcv,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Estimator {
    ReducedChiSquare,
    Known(Tolerance),
    Hc3,
    NeweyWest { lags: NonZero<usize> },
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Loss {
    Squared,
    Huber(Tolerance),
    SoftL1(Tolerance),
    Cauchy(Tolerance),
    Arctan(Tolerance),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Band {
    Confidence,
    Prediction,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Options {
    pub regularization: Regularization = Regularization::Truncated,
    pub loss: Loss = Loss::Squared,
    pub iterations: NonZero<usize> = NonZero::new(25).unwrap(),
    pub steps: NonZero<u64> = NonZero::new(100).unwrap(),
    pub tolerance: Tolerance = Tolerance::try_new(almost::F64_TOLERANCE).ok().unwrap(),
}

#[derive(Debug, Clone, PartialEq)]
pub struct Estimate {
    parameters: DMatrix<f64>,
    residuals: DMatrix<f64>,
    leverage: DVector<f64>,
    null_space: DMatrix<f64>,
    pseudoinverse: DMatrix<f64>,
    lambda: f64,
}

impl Estimate {
    #[must_use]
    pub const fn parameters(&self) -> &DMatrix<f64> { &self.parameters }

    #[must_use]
    pub const fn residuals(&self) -> &DMatrix<f64> { &self.residuals }

    #[must_use]
    pub const fn leverage(&self) -> &DVector<f64> { &self.leverage }

    #[must_use]
    pub const fn null_space(&self) -> &DMatrix<f64> { &self.null_space }

    #[must_use]
    pub fn rank(&self) -> usize { self.null_space.nrows().strict_sub(self.null_space.ncols()) }

    #[must_use]
    pub fn freedom(&self) -> f64 { convert::<usize, f64>(self.leverage.len()) - self.leverage.sum() }

    #[must_use]
    pub const fn lambda(&self) -> f64 { self.lambda }
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub struct Uncertainty {
    pub parameters: DVector<f64>,
    pub covariance: DMatrix<f64>,
    pub noise: f64,
    pub freedom: f64,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Influence {
    pub studentized: DVector<f64>,
    pub cook: DVector<f64>,
    pub press: f64,
    pub aicc: Option<f64>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Minimum {
    pub point: DVector<f64>,
    pub cost: f64,
    pub report: TerminationReport<f64>,
}

#[derive(Debug)]
pub struct Fit {
    pub estimate: Estimate,
    pub active: Vec<usize>,
    pub minima: NEVec<Minimum>,
    pub failures: Vec<RegressionError>,
}

struct Spectrum {
    left: DMatrix<f64>,
    right: DMatrix<f64>,
    rank: usize,
    singular: DVector<f64>,
}

struct Profile<'call, S> {
    separable: &'call S,
    coefficients: &'call DVector<f64>,
}

impl<S> Model for Profile<'_, S>
where S: Separable
{
    fn evaluate<D>(&self, site: DVectorView<'_, D>, parameters: DVectorView<'_, D>) -> D
    where D: DualNum<Primitive = f64> {
        self.coefficients.iter().enumerate().fold(D::from(0.0), |total, (term, &coefficient)| self.separable.basis(site.as_view(), parameters.as_view(), term).mul_add(D::from(coefficient), total))
    }
}

struct Joint<'call, S> {
    separable: &'call S,
    shapes: usize,
}

impl<S> Model for Joint<'_, S>
where S: Separable
{
    fn evaluate<D>(&self, site: DVectorView<'_, D>, parameters: DVectorView<'_, D>) -> D
    where D: DualNum<Primitive = f64> {
        let shape = parameters.rows(0, self.shapes);
        parameters.iter().skip(self.shapes).enumerate().fold(D::from(0.0), |total, (term, coefficient)| self.separable.basis(site.as_view(), shape.as_view(), term).mul_add(coefficient.clone(), total))
    }
}

struct Search<R, J> {
    residual: R,
    jacobian: J,
    lower: DVector<f64>,
    upper: DVector<f64>,
}

impl<R, J> Residual for Search<R, J>
where R: Fn(&DVector<f64>) -> Result<DVector<f64>, RegressionError>
{
    type Param = DVector<f64>;
    type Output = DVector<f64>;
    type Error = RegressionError;

    fn residual(&self, param: &DVector<f64>) -> Result<DVector<f64>, RegressionError> { (self.residual)(param) }
}

impl<R, J> Jacobian for Search<R, J>
where
    R: Fn(&DVector<f64>) -> Result<DVector<f64>, RegressionError>,
    J: Fn(&DVector<f64>) -> Result<DMatrix<f64>, RegressionError>,
{
    type Jacobian = DMatrix<f64>;

    fn jacobian(&self, param: &DVector<f64>) -> Result<DMatrix<f64>, RegressionError> { (self.jacobian)(param) }
}

impl<R, J> Constraints for Search<R, J> {
    type Set = ConstraintSet<BoxBounds<DVector<f64>>>;

    fn constraints(&self) -> <Self::Set as ConstraintDescription>::Borrowed<'_> { ConstraintSet::new().with_bounds(BoxBounds::new(Some(&self.lower), Some(&self.upper))) }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum RegressionError {
    #[error("model slope at point {point} for parameter {parameter} is not finite")]
    Design { point: usize, parameter: usize },
    #[error("model output is not finite at point {point}")]
    Model { point: usize },
    #[error("fit overflows at residual sum {residual} over parameters {parameters}")]
    Overflow { parameters: DMatrix<f64>, residual: f64 },
    #[error("influence of point {point} on response {response} is not finite")]
    Influence { response: usize, point: usize },
    #[error("least-squares search refused its problem")]
    Search(#[source] Box<ConstraintSolveError<Self>>),
    #[error(transparent)]
    Analysis(#[from] AnalysisError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DECOMPOSITION]

fn single_response(observations: &Observations) -> Result<(), RegressionError> {
    let found = observations.values().nrows();
    (found == 1).ok_or(AnalysisError::Shape { input: Input::Responses, expected: 1, found }.into())
}

fn refused_slopes<T>(slopes: &DMatrix<T>) -> impl Iterator<Item = Result<(), RegressionError>>
where T: DualNum<Primitive = f64> {
    slopes.iter().positions(|slope| !slope.re().is_finite()).map(|entry| {
        let (point, parameter) = slopes.vector_to_matrix_index(entry);
        Err(RegressionError::Design { point, parameter })
    })
}

fn scales(observations: &Observations) -> DVector<f64> { observations.weights().map(f64::sqrt) }

fn scaled(matrix: &DMatrix<f64>, scales: &DVector<f64>) -> DMatrix<f64> {
    DMatrix::from_iterator(matrix.nrows(), matrix.ncols(), iter::zip(matrix.iter(), scales.iter().cycle()).map(|(entry, scale)| entry * scale))
}

fn decompose(design: &DMatrix<f64>) -> Result<Spectrum, RegressionError> {
    let (points, parameters) = design.shape();
    let svd = MatRef::from_column_major_slice(design.as_slice(), points, parameters).thin_svd().map_err(|SvdError::NoConvergence| AnalysisError::NoConvergence)?;
    let (left, right) = (svd.U(), svd.V());
    let [shorter, longer] = cmp::minmax(points, parameters);
    let singular = DVector::from_iterator(shorter, svd.S().column_vector().iter().copied());
    let threshold = f64::EPSILON * convert::<usize, f64>(longer) * singular.amax();
    Ok(Spectrum {
        left: DMatrix::from_fn(points, singular.len(), |row, column| left[(row, column)]),
        right: DMatrix::from_fn(parameters, singular.len(), |row, column| right[(row, column)]),
        rank: singular.iter().filter(|&&value| value > threshold).count(),
        singular,
    })
}

fn estimate(design: &DMatrix<f64>, target: &DMatrix<f64>, regularization: Regularization, solved: Option<&DVector<f64>>, hooks: SearchHooks<'_>) -> Result<Estimate, RegressionError> {
    let spectrum = decompose(design)?;
    let filters = |lambda: f64| {
        let scale = lambda.sqrt();
        spectrum.singular.iter().enumerate().map(move |(index, &value)| {
            if index >= spectrum.rank {
                (0.0, 0.0)
            } else if scale <= value {
                let ratio = scale / value;
                let factor = ratio.mul_add(ratio, 1.0).recip();
                (factor, factor / value)
            } else {
                let ratio = value / scale;
                let square = ratio.powi(2);
                let divisor = 1.0 + square;
                (square / divisor, (ratio / divisor) / scale)
            }
        })
    };
    let projected = spectrum.left.tr_mul(target);
    let remainder = (target.norm_squared() - projected.norm_squared()).max(0.0);
    let lambda = match (regularization, spectrum.rank.checked_sub(1).and_then(|last| spectrum.singular.get(last).copied())) {
        (Regularization::Truncated, _) | (Regularization::Gcv, None) => Ok(0.0),
        (Regularization::Tikhonov(lambda), _) => Ok(lambda.into_inner()),
        (Regularization::Gcv, Some(lowest)) => {
            let points = convert::<usize, f64>(spectrum.left.nrows());
            let energies = projected.row_iter().map(|row| row.norm_squared()).collect::<Vec<_>>();
            let gcv = |exponent: f64| {
                let (trace, lack) =
                    iter::zip(filters(exponent.exp()), &energies).fold((0.0, 0.0), |(trace, lack), ((factor, _), energy)| (trace + factor, (1.0 - factor).powi(2).mul_add(*energy, lack)));
                Ok(points * (lack + remainder) / (points - trace).max(f64::EPSILON * points).powi(2))
            };
            let bracket = RangeInclusive { start: 2.0_f64.mul_add(lowest.ln(), f64::EPSILON.ln()), last: 2.0 * spectrum.singular.amax().ln() };
            let tolerance = const { Tolerance::try_new(f64::from(almost::F32_TOLERANCE)).ok().unwrap() };
            Bounds::try_new(bracket)
                .map_err(|source| AnalysisError::Root { bracket, source: RootSource::Bounds(source) })
                .and_then(|interval| minimum(gcv, interval, tolerance, hooks))
                .map(|(exponent, _)| exponent.exp())
        }
    }?;
    let (factors, gains): (Vec<_>, Vec<_>) = filters(lambda).unzip();
    let (factors, gains) = (DVector::from_vec(factors), DVector::from_vec(gains));
    let pseudoinverse = spectrum.right.map_with_location(|_, column, value| value * gains[column]) * spectrum.left.transpose();
    let (parameters, residuals) = solved.map_or_else(
        || {
            let parameters = &pseudoinverse * target;
            let residuals = target - design * &parameters;
            (parameters, residuals)
        },
        |point| (DMatrix::from_column_slice(point.len(), 1, point.as_slice()), target.clone()),
    );
    let residual = residuals.norm_squared();
    (residual.is_finite() && parameters.iter().all(|entry| entry.is_finite())).ok_or_else(|| RegressionError::Overflow { parameters: parameters.clone(), residual })?;
    Ok(Estimate {
        parameters,
        residuals: residuals.transpose(),
        leverage: spectrum.left.map(|entry| entry * entry) * &factors,
        null_space: spectrum.right.columns(spectrum.rank, spectrum.singular.len().strict_sub(spectrum.rank)).into_owned(),
        pseudoinverse,
        lambda,
    })
}

// --- [LOSS]

fn rho(loss: Loss) -> (&'static (dyn LossFunction<f64> + Sync), f64) {
    match loss {
        Loss::Squared => (&SquaredLoss, 1.0),
        Loss::Huber(scale) => (&HuberLoss, scale.into_inner()),
        Loss::SoftL1(scale) => (&SoftL1Loss, scale.into_inner()),
        Loss::Cauchy(scale) => (&CauchyLoss, scale.into_inner()),
        Loss::Arctan(scale) => (&ArctanLoss, scale.into_inner()),
    }
}

fn reweight(loss: Loss, residual: f64) -> f64 {
    let (function, scale) = rho(loss);
    function.evaluate((residual / scale).powi(2)).first_derivative.sqrt()
}

fn robust(design: &DMatrix<f64>, target: &DMatrix<f64>, options: Options, hooks: SearchHooks<'_>) -> Result<Estimate, RegressionError> {
    let first = estimate(design, target, options.regularization, None, hooks)?;
    match options.loss {
        Loss::Squared => Ok(first),
        Loss::Huber(_) | Loss::SoftL1(_) | Loss::Cauchy(_) | Loss::Arctan(_) => (1..options.iterations.get())
            .try_fold(first, |current, _| {
                let lack = target - design * &current.parameters;
                let reweights = DVector::from_iterator(lack.nrows(), lack.row_iter().map(|row| reweight(options.loss, row.norm())));
                match estimate(&scaled(design, &reweights), &scaled(target, &reweights), options.regularization, None, hooks) {
                    Err(error) => ControlFlow::Break(Err(error)),
                    Ok(next) if (&next.parameters - &current.parameters).norm() <= options.tolerance.into_inner() * next.parameters.norm() => ControlFlow::Break(Ok(next)),
                    Ok(next) => ControlFlow::Continue(next),
                }
            })
            .map_continue(Ok)
            .into_value(),
    }
}

// --- [POLYNOMIALS]

#[must_use]
pub fn total_degree(dimensions: usize, degree: u8) -> DMatrix<u8> {
    let monomials = (0..=dimensions).combinations_with_replacement(usize::from(degree));
    let terms = monomials.size_hint().0;
    DMatrix::from_iterator(
        dimensions,
        terms,
        monomials.flat_map(|monomial| (1..=dimensions).map(move |symbol| monomial.iter().filter(|&&index| index == symbol).fold(0_u8, |count, _| count.strict_add(1)))),
    )
}

pub fn fit(observations: &Observations, exponents: DMatrix<u8>, options: Options, hooks: SearchHooks<'_>) -> Result<(Polynomial, Estimate), NEVec<RegressionError>> {
    let (sites, values) = (observations.sites(), observations.values());
    let (dimensions, points) = sites.shape();
    let terms = exponents.ncols();
    [(exponents.nrows() == dimensions).ok_or(AnalysisError::Shape { input: Input::Exponents, expected: dimensions, found: exponents.nrows() }), observations.determines(terms)]
        .into_iter()
        .map(|check| check.map_err(RegressionError::from))
        .collect::<Validated<(), _>>()
        .ok()?;
    let centred = exponents.column_iter().all(|term| {
        term.iter().positions(|&exponent| exponent > 0).all(|lowered| {
            exponents
                .column_iter()
                .any(|other| iter::zip(other.iter(), term.iter()).enumerate().all(|(dimension, (&held, &exponent))| u16::from(held).strict_add(u16::from(dimension == lowered)) == u16::from(exponent)))
        })
    });
    let offsets = DVector::from_iterator(dimensions, sites.row_iter().map(|coordinates| if centred { f64::midpoint(coordinates.min(), coordinates.max()) } else { 0.0 }));
    let spans = DVector::from_iterator(
        dimensions,
        sites.row_iter().map(|coordinates| {
            let span = if centred { coordinates.max() / 2.0 - coordinates.min() / 2.0 } else { coordinates.amax() };
            if span > 0.0 { span } else { 1.0 }
        }),
    );
    let weights = scales(observations);
    let conditioned = design(
        &exponents,
        &DMatrix::from_iterator(dimensions, points, iter::zip(sites.iter(), iter::zip(offsets.iter(), spans.iter()).cycle()).map(|(coordinate, (offset, span))| (coordinate - offset) / span))
            .as_view(),
    );
    let solved = robust(&scaled(&conditioned, &weights), &scaled(&values.transpose(), &weights), options, hooks).map_err(NEVec::new)?;
    let raw = DMatrix::from_fn(terms, terms, |lower, upper| {
        izip!(exponents.column(lower).iter(), exponents.column(upper).iter(), offsets.iter(), spans.iter())
            .map(|(&held, &exponent, &offset, &span)| {
                exponent.checked_sub(held).map_or_default(|rest| binomial(u64::from(exponent), u64::from(held)) * (-offset).powi(i32::from(rest)) / span.powi(i32::from(exponent)))
            })
            .product::<f64>()
    });
    let parameters = &raw * &solved.parameters;
    Ok((Polynomial { exponents, coefficients: parameters.clone() }, Estimate { parameters, null_space: &raw * &solved.null_space, pseudoinverse: &raw * &solved.pseudoinverse, ..solved }))
}

fn terms<T>(exponents: &DMatrix<u8>, sites: &DMatrix<T>, refusals: impl IntoIterator<Item = Result<(), RegressionError>>) -> Result<DMatrix<T>, NEVec<RegressionError>>
where T: DualNum<Primitive = f64> {
    iter::once((sites.nrows() == exponents.nrows()).ok_or(AnalysisError::Shape { input: Input::Sites, expected: exponents.nrows(), found: sites.nrows() }.into()))
        .chain(refusals)
        .collect::<Validated<(), _>>()
        .ok()?;
    let terms = design(exponents, &sites.as_view());
    refused_slopes(&terms).collect::<Validated<(), _>>().ok()?;
    Ok(terms)
}

pub fn evaluate<T>(polynomial: &Polynomial, sites: &DMatrix<T>) -> Result<DMatrix<T>, NEVec<RegressionError>>
where T: DualNum<Primitive = f64> {
    let Polynomial { exponents, coefficients } = polynomial;
    let refusals = iter::once((coefficients.nrows() == exponents.ncols()).ok_or(AnalysisError::Shape { input: Input::Coefficients, expected: exponents.ncols(), found: coefficients.nrows() }.into()))
        .chain(coefficients.iter().positions(|coefficient| !coefficient.is_finite()).map(|entry| {
            let (term, response) = coefficients.vector_to_matrix_index(entry);
            Err(AnalysisError::Value { index: vec![term, response] }.into())
        }));
    terms(exponents, sites, refusals).map(|terms| (&terms * coefficients.map(T::from)).transpose())
}

fn refused_reading(uncertainty: &Uncertainty, points: usize, weights: Option<&[Tolerance]>) -> [Result<(), RegressionError>; 2] {
    let parameters = uncertainty.parameters.len();
    [
        (uncertainty.covariance.shape() == (parameters, parameters)).ok_or(AnalysisError::Shape { input: Input::Covariance, expected: parameters, found: uncertainty.covariance.nrows() }.into()),
        weights.map_or(Ok(()), |weights| (weights.len() == points).ok_or(AnalysisError::Shape { input: Input::Weights, expected: points, found: weights.len() }.into())),
    ]
}

fn noises(noise: f64, weights: Option<&[Tolerance]>, points: usize) -> DVector<f64> { DVector::from_fn(points, |point, _| weights.map_or(noise, |weights| noise / weights[point].into_inner())) }

pub fn predict<T>(polynomial: &Polynomial, uncertainty: &Uncertainty, sites: &DMatrix<T>, weights: Option<&[Tolerance]>) -> Result<Prediction<T>, NEVec<RegressionError>>
where T: DualNum<Primitive = f64> {
    let parameters = polynomial.exponents.ncols();
    let refusals =
        iter::once((uncertainty.parameters.len() == parameters).ok_or(AnalysisError::Shape { input: Input::Coefficients, expected: parameters, found: uncertainty.parameters.len() }.into()))
            .chain(refused_reading(uncertainty, sites.ncols(), weights));
    let terms = terms(&polynomial.exponents, sites, refusals)?;
    Ok(Prediction {
        mean: &terms * uncertainty.parameters.map(T::from),
        variance: (&terms * uncertainty.covariance.map(T::from)).component_mul(&terms).column_sum(),
        noise: noises(uncertainty.noise, weights, sites.ncols()).map(T::from),
        freedom: uncertainty.freedom,
    })
}

// --- [NONLINEAR]

fn residuals<'call, M, D>(model: &'call M, observations: &'call Observations, scales: &'call DVector<f64>, parameters: DVectorView<'call, D>) -> impl ExactSizeIterator<Item = D>
where
    M: Model,
    D: DualNum<Primitive = f64>,
{
    izip!(observations.sites().column_iter(), observations.values().column_iter(), scales.iter())
        .map(move |(site, value, &scale)| model.evaluate(site.map(D::from).as_view(), parameters.as_view()).mul_add(D::from(scale), D::from(-value[0] * scale)))
}

fn finite_residuals<M>(model: &M, observations: &Observations, scales: &DVector<f64>, point: &DVector<f64>) -> Result<DVector<f64>, RegressionError>
where M: Model {
    let values = DVector::from_iterator(scales.len(), residuals(model, observations, scales, point.as_view()));
    values.iter().position(|residual| !residual.is_finite()).map_or(Ok(values), |point| Err(RegressionError::Model { point }))
}

fn slopes<M>(model: &M, observations: &Observations, scales: &DVector<f64>, point: &DVector<f64>) -> Result<DMatrix<f64>, RegressionError>
where M: Model {
    let (_, slopes) = jacobian(|parameters: DVector<DualDVec64>| DVector::from_iterator(scales.len(), residuals(model, observations, scales, parameters.as_view())), point);
    refused_slopes(&slopes).next().transpose()?;
    Ok(slopes)
}

fn minima<K, R, J>(space: &Space<K>, starts: DMatrixView<'_, f64>, options: Options, residual: &R, jacobian: &J) -> Result<(NEVec<Minimum>, Vec<RegressionError>), NEVec<RegressionError>>
where
    K: Sync,
    R: Fn(&DVector<f64>) -> Result<DVector<f64>, RegressionError> + Sync,
    J: Fn(&DVector<f64>) -> Result<DMatrix<f64>, RegressionError> + Sync,
{
    let dimensions = space.len().get();
    iter::once((starts.nrows() == dimensions).ok_or(AnalysisError::Shape { input: Input::Starts, expected: dimensions, found: starts.nrows() }))
        .chain(
            starts.column_iter().flat_map(|start| {
                iter::zip(start, space.values()).enumerate().filter(|(_, (at, bounds))| !bounds.contains(at)).map(|(dimension, (&at, _))| Err(AnalysisError::Outside { dimension, at }))
            }),
        )
        .map(|check| check.map_err(RegressionError::from))
        .collect::<Validated<(), _>>()
        .ok()?;
    let ends = |end: fn(&Bounds) -> f64| DVector::from_iterator(dimensions, space.values().into_iter().map(end));
    let (lower, upper) = (ends(|bounds| bounds.start), ends(|bounds| bounds.last));
    let (function, scale) = rho(options.loss);
    let (failed, solved): (Vec<_>, Vec<_>) = starts
        .par_column_iter()
        .map(|start| {
            let search = Search { residual, jacobian, lower: lower.clone(), upper: upper.clone() };
            Executor::from_start(
                RobustLeastSquares::new(search, function).with_scale(scale),
                TrustRegionReflective::new().with_relative_cost_change_tolerance(options.tolerance.into_inner()),
                start.into_owned(),
            )
            .max_iter(options.steps.get())
            .run()
            .map(|OptimizationResult { state, report, .. }| Minimum { point: state.param().clone(), cost: state.cost(), report })
            .map_err(|failure| match failure {
                ConstraintSolveError::Evaluation(error) => error,
                source => RegressionError::Search(Box::new(source)),
            })
        })
        .partition_map(Either::from);
    let resolution = options.tolerance.into_inner();
    let near = |left: &Minimum, right: &Minimum| {
        izip!(left.point.iter(), right.point.iter(), space.values()).all(|(first, second, bounds)| (first - second).abs() <= resolution * (bounds.last - bounds.start))
    };
    let distinct = solved.into_iter().sorted_by(|left, right| left.cost.total_cmp(&right.cost)).fold(Vec::<Minimum>::new(), |mut kept, solution| {
        if !kept.iter().any(|held| near(held, &solution)) {
            kept.push(solution);
        }
        kept
    });
    match NEVec::try_from_vec(distinct) {
        Some(distinct) => Ok((distinct, failed)),
        None => Err(NEVec::try_from_vec(failed).unwrap_or_else(|| NEVec::new(AnalysisError::Shape { input: Input::Starts, expected: 1, found: 0 }.into()))),
    }
}

fn linearized<M>(model: &M, observations: &Observations, scales: &DVector<f64>, options: Options, point: &DVector<f64>, hooks: SearchHooks<'_>) -> Result<Estimate, NEVec<RegressionError>>
where M: Model {
    let (weighted, slopes) = jacobian(|parameters: DVector<DualDVec64>| DVector::from_iterator(scales.len(), residuals(model, observations, scales, parameters.as_view())), point);
    weighted.iter().positions(|residual| !residual.is_finite()).map(|point| Err(RegressionError::Model { point })).chain(refused_slopes(&slopes)).collect::<Validated<(), _>>().ok()?;
    let reweights = weighted.map(|residual| reweight(options.loss, residual));
    let target = DMatrix::from_iterator(weighted.len(), 1, weighted.iter().map(|residual| -residual));
    estimate(&scaled(&slopes, &reweights), &scaled(&target, &reweights), options.regularization, Some(point), hooks).map_err(NEVec::new)
}

fn active<K>(space: &Space<K>, point: &DVector<f64>) -> Vec<usize> {
    iter::zip(point.iter(), space.values()).positions(|(&value, bounds)| almost::equal(value, bounds.start) || almost::equal(value, bounds.last)).collect()
}

pub fn least_squares<K, M>(model: &M, observations: &Observations, space: &Space<K>, starts: DMatrixView<'_, f64>, options: Options, hooks: SearchHooks<'_>) -> Result<Fit, NEVec<RegressionError>>
where
    K: Sync,
    M: Model,
{
    [single_response(observations), observations.determines(space.len().get()).map_err(RegressionError::from)].into_iter().collect::<Validated<(), _>>().ok()?;
    let scales = scales(observations);
    let (distinct, failures) =
        minima(space, starts, options, &|point: &DVector<f64>| finite_residuals(model, observations, &scales, point), &|point: &DVector<f64>| slopes(model, observations, &scales, point))?;
    let point = &distinct.first().point;
    Ok(Fit { estimate: linearized(model, observations, &scales, options, point, hooks)?, active: active(space, point), minima: distinct, failures })
}

pub fn separable<K, S>(model: &S, observations: &Observations, space: &Space<K>, starts: DMatrixView<'_, f64>, options: Options, hooks: SearchHooks<'_>) -> Result<Fit, NEVec<RegressionError>>
where
    K: Sync,
    S: Separable,
{
    let (shapes, points) = (space.len().get(), observations.sites().ncols());
    [single_response(observations), observations.determines(shapes.saturating_add(model.terms())).map_err(RegressionError::from)].into_iter().collect::<Validated<(), _>>().ok()?;
    let scales = scales(observations);
    let target = scaled(&observations.values().transpose(), &scales);
    let projected = |shape: DVectorView<'_, f64>| -> Result<(DVector<f64>, f64), RegressionError> {
        let basis = DMatrix::from_fn(points, model.terms(), |point, term| model.basis(observations.sites().column(point), shape.as_view(), term) * scales[point]);
        refused_slopes(&basis).next().transpose()?;
        robust(&basis, &target, options, hooks).map(|solved| (solved.parameters.column(0).into_owned(), solved.lambda))
    };
    let (function, scale) = rho(options.loss);
    let rooted = |residual: f64| {
        let held = function.evaluate_scaled(residual, scale);
        let root = residual.signum() * (2.0 * held.value).sqrt();
        (root, if root == 0.0 { 1.0 } else { held.first_derivative / root })
    };
    let residual = |shape: &DVector<f64>| {
        projected(shape.as_view()).and_then(|(coefficients, lambda)| {
            finite_residuals(&Profile { separable: model, coefficients: &coefficients }, observations, &scales, shape).map(|fitted| {
                DVector::from_iterator(points.strict_add(coefficients.len()), fitted.iter().map(|&value| rooted(value).0).chain(coefficients.iter().map(|coefficient| lambda.sqrt() * coefficient)))
            })
        })
    };
    let jacobian = |shape: &DVector<f64>| {
        projected(shape.as_view()).and_then(|(coefficients, _)| {
            let profile = Profile { separable: model, coefficients: &coefficients };
            let (fitted, slopes) = (finite_residuals(&profile, observations, &scales, shape)?, slopes(&profile, observations, &scales, shape)?);
            Ok(DMatrix::from_fn(points.strict_add(coefficients.len()), slopes.ncols(), |row, column| fitted.get(row).map_or(0.0, |&value| rooted(value).1 * slopes[(row, column)])))
        })
    };
    let (distinct, failures) = minima(space, starts, Options { loss: Loss::Squared, ..options }, &residual, &jacobian)?;
    let shape = &distinct.first().point;
    let (coefficients, _) = projected(shape.as_view()).map_err(NEVec::new)?;
    let joint = DVector::from_iterator(shapes.strict_add(coefficients.len()), iter::chain(shape.iter(), coefficients.iter()).copied());
    Ok(Fit { estimate: linearized(&Joint { separable: model, shapes }, observations, &scales, options, &joint, hooks)?, active: active(space, shape), minima: distinct, failures })
}

pub fn propagate<M>(
    model: &M,
    uncertainty: &Uncertainty,
    sites: DMatrixView<'_, f64>,
    site_covariance: Option<&DMatrix<f64>>,
    weights: Option<&[Tolerance]>,
) -> Result<Prediction<f64>, NEVec<RegressionError>>
where
    M: Model,
{
    let dimensions = sites.nrows();
    iter::chain(
        refused_reading(uncertainty, sites.ncols(), weights),
        sites.iter().positions(|coordinate| !coordinate.is_finite()).map(|entry| {
            let (dimension, point) = sites.vector_to_matrix_index(entry);
            Err(AnalysisError::Site { dimension, point, placement: Placement::Unrepresentable }.into())
        }),
    )
    .chain(iter::once(site_covariance.map_or(Ok(()), |covariance| {
        (covariance.shape() == (dimensions, dimensions)).ok_or(AnalysisError::Shape { input: Input::Covariance, expected: dimensions, found: covariance.nrows() }.into())
    })))
    .collect::<Validated<(), _>>()
    .ok()?;
    let (mean, variance): (Vec<_>, Vec<_>) = sites
        .par_column_iter()
        .enumerate()
        .map(|(point, site)| {
            let (value, slope) = gradient(|parameters: DVector<DualDVec64>| model.evaluate(site.map(DualDVec64::from_re).as_view(), parameters.as_view()), &uncertainty.parameters);
            let propagated = site_covariance.map_or_default(|covariance| {
                let lifted = uncertainty.parameters.map(DualDVec64::from_re);
                let (_, tilt) = gradient(|coordinates: DVector<DualDVec64>| model.evaluate(coordinates.as_view(), lifted.as_view()), &site.into_owned());
                tilt.dot(&(covariance * &tilt))
            });
            iter::once(value.is_finite().ok_or(RegressionError::Model { point }))
                .chain(slope.iter().positions(|slope| !slope.is_finite()).map(|parameter| Err(RegressionError::Design { point, parameter })))
                .collect::<Validated<(), _>>()
                .map(|()| (value, slope.dot(&(&uncertainty.covariance * &slope)) + propagated))
        })
        .collect::<Validated<(Vec<_>, Vec<_>), _>>()
        .ok()?;
    Ok(Prediction { mean: DVector::from_vec(mean), variance: DVector::from_vec(variance), noise: noises(uncertainty.noise, weights, sites.ncols()), freedom: uncertainty.freedom })
}

pub fn log_density<'call, M>(model: &'call M, observations: &'call Observations, sigma: Tolerance) -> Result<impl Fn(&[f64], &mut [f64]) -> f64 + Sync + 'call, RegressionError>
where M: Model {
    single_response(observations)?;
    let (scales, precision) = (scales(observations), sigma.into_inner().powi(-2));
    Ok(move |point: &[f64], slot: &mut [f64]| {
        let (value, slope) = gradient(
            |parameters: DVector<DualDVec64>| residuals(model, observations, &scales, parameters.as_view()).map(|residual| residual.powi(2)).sum::<DualDVec64>(),
            &DVector::from_column_slice(point),
        );
        slot.copy_from_slice((slope * (-0.5 * precision)).as_slice());
        value * precision * -0.5
    })
}

// --- [READINGS]

fn refused_influence(response: usize, values: &DVector<f64>) -> Validated<(), RegressionError> {
    values.iter().positions(|value| !value.is_finite()).map(|point| Err(RegressionError::Influence { response, point })).collect()
}

pub fn uncertainty(estimate: &Estimate, estimator: Estimator) -> Result<Vec<Uncertainty>, NEVec<RegressionError>> {
    let (pseudoinverse, points, freedom) = (&estimate.pseudoinverse, estimate.leverage.len(), estimate.freedom());
    let gram = LazyCell::new(|| pseudoinverse * pseudoinverse.transpose());
    let scored = |factors: &DVector<f64>| pseudoinverse.map_with_location(|_, column, value| value * factors[column]);
    let readings = estimate.residuals.row_iter().zip(estimate.parameters.column_iter()).enumerate().map(|(response, (lack, parameters))| {
        let reduced = lack.norm_squared() / freedom;
        match estimator {
            Estimator::ReducedChiSquare => Validated::Good((&*gram * reduced, reduced, freedom)),
            Estimator::Known(sigma) => Validated::Good((&*gram * sigma.into_inner().powi(2), sigma.into_inner().powi(2), f64::INFINITY)),
            Estimator::Hc3 => {
                let factors = lack.transpose().zip_map(&estimate.leverage, |entry, leverage| entry / (1.0 - leverage));
                let influence = scored(&factors);
                refused_influence(response, &factors).map(|()| (&influence * influence.transpose(), reduced, freedom))
            }
            Estimator::NeweyWest { lags } => {
                let influence = scored(&lack.transpose());
                let reach = lags.get().min(points.saturating_sub(1));
                let serial = (1..=reach).fold(&influence * influence.transpose(), |total, lag| {
                    let cross = influence.columns(lag, points.strict_sub(lag)) * influence.columns(0, points.strict_sub(lag)).transpose();
                    total + (&cross + cross.transpose()) * (1.0 - convert::<usize, f64>(lag) / convert::<usize, f64>(reach.strict_add(1)))
                });
                Validated::Good((serial * (convert::<usize, f64>(points) / freedom), reduced, freedom))
            }
        }
        .map(|(covariance, noise, freedom)| Uncertainty { parameters: parameters.into_owned(), covariance, noise, freedom })
    });
    readings.collect::<Validated<Vec<_>, _>>().ok()
}

pub fn observed_information<M>(model: &M, observations: &Observations, loss: Loss, estimate: &Estimate) -> Result<Uncertainty, NEVec<RegressionError>>
where M: Model {
    single_response(observations).map_err(NEVec::new)?;
    let parameters = estimate.parameters.column(0).into_owned();
    let weights = scales(observations);
    let freedom = estimate.freedom();
    let (function, scale) = rho(loss);
    let count = parameters.len();
    let (value, curvature) = izip!(observations.sites().column_iter(), observations.values().column_iter(), weights.iter()).fold(
        (0.0, DMatrix::<f64>::zeros(count, count)),
        |(value, curvature), (site, observed, &weight)| {
            let (residual, slope, bend) = hessian(
                |point: DVector<Dual2DVec64>| {
                    model.evaluate(site.map(Dual2DVec64::from_re).as_view(), point.as_view()).mul_add(Dual2DVec64::from_re(weight), Dual2DVec64::from_re(-observed[0] * weight))
                },
                &parameters,
            );
            let term = function.evaluate_scaled(residual, scale);
            (2.0_f64.mul_add(term.value, value), curvature + (&slope * slope.transpose() * term.second_derivative + bend * term.first_derivative) * 2.0)
        },
    );
    (value.is_finite() && curvature.iter().all(|entry| entry.is_finite())).ok_or_else(|| NEVec::new(RegressionError::Overflow { parameters: estimate.parameters.clone(), residual: value }))?;
    let noise = value / freedom;
    let spectrum = decompose(&curvature).map_err(NEVec::new)?;
    let gains = spectrum.singular.map_with_location(|index, _, singular| if index < spectrum.rank { 2.0 * noise / singular } else { 0.0 });
    Ok(Uncertainty { parameters, covariance: spectrum.right.map_with_location(|_, column, value| value * gains[column]) * spectrum.left.transpose(), noise, freedom })
}

pub fn influence(estimate: &Estimate) -> Result<Vec<Influence>, NEVec<RegressionError>> {
    let (leverage, freedom) = (&estimate.leverage, estimate.freedom());
    let count = convert::<usize, f64>(leverage.len());
    let effective = count - freedom;
    let readings = estimate.residuals.row_iter().enumerate().map(|(response, lack)| {
        let sum = lack.norm_squared();
        let reduced = sum / freedom;
        let studentized = lack.transpose().zip_map(leverage, |entry, hat| entry / (reduced * (1.0 - hat)).sqrt());
        let cook = studentized.zip_map(leverage, |value, hat| value.powi(2) * hat / (effective * (1.0 - hat)));
        let press = iter::zip(lack.iter(), leverage.iter()).map(|(entry, leverage)| (entry / (1.0 - leverage)).powi(2)).sum::<f64>();
        let aicc = Some(count.mul_add((sum / count).ln(), 2.0_f64.mul_add(effective, 2.0 * effective * (effective + 1.0) / (freedom - 1.0)))).filter(|value| value.is_finite() && freedom > 1.0);
        refused_influence(response, &cook).map(|()| Influence { studentized, cook, press, aicc })
    });
    readings.collect::<Validated<Vec<_>, _>>().ok()
}

pub fn band(prediction: &Prediction<f64>, level: Confidence, band: Band) -> Result<Vec<RangeInclusive<f64>>, StudentsTError> {
    let probability = f64::midpoint(1.0, level.into_inner());
    let critical = if prediction.freedom.is_infinite() { Normal::standard().inverse_cdf(probability) } else { StudentsT::new(0.0, 1.0, prediction.freedom)?.inverse_cdf(probability) };
    Ok(izip!(prediction.mean.iter(), prediction.variance.iter(), prediction.noise.iter())
        .map(|(&mean, &variance, &noise)| {
            let half = critical
                * match band {
                    Band::Confidence => variance,
                    Band::Prediction => variance + noise,
                }
                .sqrt();
            RangeInclusive { start: mean - half, last: mean + half }
        })
        .collect())
}

// --- [EXPORTS] -------------------------------------------------------------------------

pub use basin;
pub use nalgebra;
pub use nonempty_collections;
pub use num_dual;
pub use rasm_numerical_analysis;
pub use rasm_stats;
pub use statrs;
