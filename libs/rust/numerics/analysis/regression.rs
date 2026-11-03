use core::{cell::LazyCell, cmp, iter, num::NonZero, ops::ControlFlow, range::RangeInclusive};

use faer::{MatRef, linalg::svd::SvdError};
use itertools::{Itertools as _, iproduct, izip};
use nalgebra::{DMatrix, DMatrixView, DVector, convert};
use nlopt::Target;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use num_dual::{Dual2DVec64, DualDVec64, DualNum, first_derivative, gradient, hessian, jacobian};
use rasm_optimize::{
    Solution,
    nonlinear::{Objective, Settings, optimize},
};
use rasm_stats::{Space, Tolerance, inference::Confidence};
use rayon::iter::{Either, IndexedParallelIterator as _, ParallelIterator as _};
use serde::{Deserialize, Serialize};
use statrs::{
    distribution::{ContinuousCDF as _, Normal, StudentsT},
    function::factorial::binomial,
};
use validated::Validated;

use crate::{AnalysisError, Breakdown, Input, Observations, TOLERANCE, determined, finite, joined, refused_sites, refused_weights, roots::minimum};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Model: Sync {
    fn evaluate<D>(&self, site: &DVector<D>, parameters: &DVector<D>) -> D
    where D: DualNum<Primitive = f64>;
}

pub trait Separable: Sync {
    fn terms(&self) -> usize;

    fn basis<D>(&self, site: &DVector<D>, shape: &DVector<D>, term: usize) -> D
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
    pub tolerance: Tolerance = TOLERANCE,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub struct Polynomial {
    pub exponents: DMatrix<u8>,
    pub coefficients: DMatrix<f64>,
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
pub struct Prediction<T> {
    pub mean: DVector<T>,
    pub variance: DVector<T>,
    pub noise: DVector<T>,
    pub freedom: f64,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Influence {
    pub studentized: DVector<f64>,
    pub cook: DVector<f64>,
    pub press: f64,
    pub aicc: Option<f64>,
}

#[derive(Debug)]
pub struct Fit {
    pub estimate: Estimate,
    pub active: Vec<usize>,
    pub minima: NEVec<Solution>,
    pub failures: Vec<AnalysisError>,
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
    fn evaluate<D>(&self, site: &DVector<D>, parameters: &DVector<D>) -> D
    where D: DualNum<Primitive = f64> {
        self.coefficients.iter().enumerate().fold(D::from(0.0), |total, (term, &coefficient)| self.separable.basis(site, parameters, term).mul_add(D::from(coefficient), total))
    }
}

struct Joint<'call, S> {
    separable: &'call S,
    shapes: usize,
}

impl<S> Model for Joint<'_, S>
where S: Separable
{
    fn evaluate<D>(&self, site: &DVector<D>, parameters: &DVector<D>) -> D
    where D: DualNum<Primitive = f64> {
        let shape = parameters.rows(0, self.shapes).into_owned();
        parameters.iter().skip(self.shapes).enumerate().fold(D::from(0.0), |total, (term, coefficient)| self.separable.basis(site, &shape, term).mul_add(coefficient.clone(), total))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DECOMPOSITION]

fn single_response(observations: &Observations) -> Result<(), AnalysisError> {
    let found = observations.values.nrows();
    (found == 1).ok_or(AnalysisError::Shape { input: Input::Responses, expected: 1, found })
}

fn scales(observations: &Observations) -> DVector<f64> { observations.weights.map(f64::sqrt) }

fn scaled(matrix: &DMatrix<f64>, scales: &DVector<f64>) -> DMatrix<f64> {
    DMatrix::from_iterator(matrix.nrows(), matrix.ncols(), iter::zip(matrix.iter(), scales.iter().cycle()).map(|(entry, scale)| entry * scale))
}

fn decompose(design: &DMatrix<f64>) -> Result<Spectrum, AnalysisError> {
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

fn penalty(spectrum: &Spectrum, projected: &DMatrix<f64>, remainder: f64, regularization: Regularization) -> Result<f64, AnalysisError> {
    let lowest = spectrum.rank.checked_sub(1).and_then(|last| spectrum.singular.get(last).copied());
    match (regularization, lowest) {
        (Regularization::Truncated, _) | (Regularization::Gcv, None) => Ok(0.0),
        (Regularization::Tikhonov(lambda), _) => Ok(lambda.into_inner()),
        (Regularization::Gcv, Some(lowest)) => {
            let points = convert::<usize, f64>(spectrum.left.nrows());
            let gcv = |exponent: f64| {
                let filter = filters(spectrum, exponent.exp());
                let lack = izip!(filter.iter(), projected.row_iter()).map(|(factor, row)| (1.0 - factor).powi(2) * row.norm_squared()).sum::<f64>() + remainder;
                Ok(points * lack / (points - filter.sum()).max(f64::EPSILON * points).powi(2))
            };
            let start = 2.0_f64.mul_add(lowest.ln(), f64::EPSILON.ln());
            minimum(gcv, start, 2.0 * spectrum.singular.amax().ln(), f64::from(almost::F32_TOLERANCE)).map(|(exponent, _)| exponent.exp())
        }
    }
}

fn filters(spectrum: &Spectrum, lambda: f64) -> DVector<f64> {
    spectrum.singular.map_with_location(|index, _, value| {
        let square = value.powi(2);
        if index < spectrum.rank { square / (square + lambda) } else { 0.0 }
    })
}

fn estimate(design: &DMatrix<f64>, target: &DMatrix<f64>, regularization: Regularization, solved: Option<&DVector<f64>>) -> Result<Estimate, AnalysisError> {
    let spectrum = decompose(design)?;
    let projected = spectrum.left.tr_mul(target);
    let remainder = (target.norm_squared() - projected.norm_squared()).max(0.0);
    let lambda = penalty(&spectrum, &projected, remainder, regularization)?;
    let factors = filters(&spectrum, lambda);
    let gains = factors.zip_map(&spectrum.singular, |factor, value| if factor > 0.0 { factor / value } else { 0.0 });
    let pseudoinverse = &spectrum.right * DMatrix::from_diagonal(&gains) * spectrum.left.transpose();
    let (parameters, residuals) = solved.map_or_else(
        || {
            let parameters = &pseudoinverse * target;
            let residuals = target - design * &parameters;
            (parameters, residuals)
        },
        |point| (DMatrix::from_column_slice(point.len(), 1, point.as_slice()), target.clone()),
    );
    let residual = residuals.norm_squared();
    (residual.is_finite() && parameters.iter().all(|entry| entry.is_finite())).ok_or_else(|| AnalysisError::Overflow { parameters: parameters.clone(), residual })?;
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
fn rho<D>(loss: Loss, residual: &D) -> D
where D: DualNum<Primitive = f64> {
    let square = residual.powi(2);
    match loss {
        Loss::Squared => square,
        Loss::Huber(scale) => {
            let scale = scale.into_inner();
            if residual.re().abs() <= scale { square } else { residual.abs().mul_add(D::from(2.0 * scale), D::from(-scale * scale)) }
        }
        Loss::SoftL1(scale) => {
            let scale = scale.into_inner();
            square.mul_add(D::from(scale.powi(-2)), D::from(1.0)).sqrt().mul_add(D::from(2.0 * scale * scale), D::from(-2.0 * scale * scale))
        }
        Loss::Cauchy(scale) => {
            let scale = scale.into_inner();
            square.mul_add(D::from(scale.powi(-2)), D::from(1.0)).ln().mul_add(D::from(scale * scale), D::from(0.0))
        }
    }
}

fn reweight(loss: Loss, residual: f64) -> f64 {
    let (_, slope) = first_derivative(|value| rho(loss, &value), residual);
    if residual == 0.0 { 1.0 } else { (slope / (2.0 * residual)).sqrt() }
}

fn robust(design: &DMatrix<f64>, target: &DMatrix<f64>, options: Options) -> Result<Estimate, AnalysisError> {
    let first = estimate(design, target, options.regularization, None)?;
    if options.loss == Loss::Squared {
        return Ok(first);
    }
    (1..options.iterations.get())
        .try_fold(first, |current, _| {
            let lack = target - design * &current.parameters;
            let reweights = DVector::from_iterator(lack.nrows(), lack.row_iter().map(|row| reweight(options.loss, row.norm())));
            match estimate(&scaled(design, &reweights), &scaled(target, &reweights), options.regularization, None) {
                Err(error) => ControlFlow::Break(Err(error)),
                Ok(next) if (&next.parameters - &current.parameters).norm() <= options.tolerance.into_inner() * next.parameters.norm() => ControlFlow::Break(Ok(next)),
                Ok(next) => ControlFlow::Continue(next),
            }
        })
        .map_continue(Ok)
        .into_value()
}

// --- [POLYNOMIAL]
pub(crate) fn design<T>(exponents: &DMatrix<u8>, sites: &DMatrix<T>) -> DMatrix<T>
where T: DualNum<Primitive = f64> {
    DMatrix::from_row_iterator(
        sites.ncols(),
        exponents.ncols(),
        iproduct!(sites.column_iter(), exponents.column_iter()).map(|(site, term)| iter::zip(site.iter(), term.iter()).map(|(value, &exponent)| value.powi(i32::from(exponent))).product::<T>()),
    )
}

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

pub fn fit(observations: &Observations, exponents: DMatrix<u8>, options: Options) -> Result<(Polynomial, Estimate), NEVec<AnalysisError>> {
    let Observations { sites, values, weights: _ } = observations;
    let (dimensions, points) = sites.shape();
    let terms = exponents.ncols();
    [(exponents.nrows() == dimensions).ok_or(AnalysisError::Shape { input: Input::Exponents, expected: dimensions, found: exponents.nrows() }), determined(points, terms)]
        .into_iter()
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
        &DMatrix::from_iterator(dimensions, points, iter::zip(sites.iter(), iter::zip(offsets.iter(), spans.iter()).cycle()).map(|(coordinate, (offset, span))| (coordinate - offset) / span)),
    );
    let solved = robust(&scaled(&conditioned, &weights), &scaled(&values.transpose(), &weights), options).map_err(NEVec::new)?;
    let raw = DMatrix::from_fn(terms, terms, |lower, upper| {
        izip!(exponents.column(lower).iter(), exponents.column(upper).iter(), offsets.iter(), spans.iter())
            .map(|(&held, &exponent, &offset, &span)| {
                exponent.checked_sub(held).map_or_default(|rest| binomial(u64::from(exponent), u64::from(held)) * (-offset).powi(i32::from(rest)) / span.powi(i32::from(exponent)))
            })
            .product::<f64>()
    });
    let parameters = &raw * &solved.parameters;
    Ok((Polynomial { exponents, coefficients: parameters.clone() }, Estimate { parameters, pseudoinverse: &raw * &solved.pseudoinverse, null_space: &raw * &solved.null_space, ..solved }))
}

fn terms<T>(polynomial: &Polynomial, sites: &DMatrix<T>) -> Result<DMatrix<T>, NEVec<AnalysisError>>
where T: DualNum<Primitive = f64> {
    let Polynomial { exponents, coefficients } = polynomial;
    iter::chain(
        [
            (sites.nrows() == exponents.nrows()).ok_or(AnalysisError::Shape { input: Input::Sites, expected: exponents.nrows(), found: sites.nrows() }),
            (coefficients.nrows() == exponents.ncols()).ok_or(AnalysisError::Shape { input: Input::Coefficients, expected: exponents.ncols(), found: coefficients.nrows() }),
        ],
        refused_sites(sites, |coordinate: &T| coordinate.re().is_finite()),
    )
    .chain(finite(coefficients.iter(), |entry| {
        let (term, response) = coefficients.vector_to_matrix_index(entry);
        AnalysisError::Value { index: vec![term, response] }
    }))
    .collect::<Validated<(), _>>()
    .ok()?;
    Ok(design(exponents, sites))
}

pub fn evaluate<T>(polynomial: &Polynomial, sites: &DMatrix<T>) -> Result<DMatrix<T>, NEVec<AnalysisError>>
where T: DualNum<Primitive = f64> {
    terms(polynomial, sites).map(|terms| (&terms * polynomial.coefficients.map(T::from)).transpose())
}

fn refused_reading(uncertainty: &Uncertainty, parameters: usize, points: usize, weights: Option<&DVector<f64>>) -> impl Iterator<Item = Result<(), AnalysisError>> {
    iter::chain(
        [
            (uncertainty.parameters.len() == parameters).ok_or(AnalysisError::Shape { input: Input::Coefficients, expected: parameters, found: uncertainty.parameters.len() }),
            (uncertainty.covariance.shape() == (parameters, parameters)).ok_or(AnalysisError::Shape { input: Input::Covariance, expected: parameters, found: uncertainty.covariance.nrows() }),
            weights.map_or(Ok(()), |weights| (weights.len() == points).ok_or(AnalysisError::Shape { input: Input::Weights, expected: points, found: weights.len() })),
        ],
        refused_weights(None, weights.into_iter().flatten()),
    )
}

fn noises(noise: f64, weights: Option<&DVector<f64>>, points: usize) -> DVector<f64> { weights.map_or_else(|| DVector::repeat(points, noise), |weights| weights.map(|weight| noise / weight)) }

pub fn predict<T>(polynomial: &Polynomial, uncertainty: &Uncertainty, sites: &DMatrix<T>, weights: Option<&DVector<f64>>) -> Result<Prediction<T>, NEVec<AnalysisError>>
where T: DualNum<Primitive = f64> {
    refused_reading(uncertainty, polynomial.exponents.ncols(), sites.ncols(), weights).collect::<Validated<(), _>>().ok()?;
    let terms = terms(polynomial, sites)?;
    Ok(Prediction {
        mean: &terms * uncertainty.parameters.map(T::from),
        variance: (&terms * uncertainty.covariance.map(T::from)).component_mul(&terms).column_sum(),
        noise: noises(uncertainty.noise, weights, sites.ncols()).map(T::from),
        freedom: uncertainty.freedom,
    })
}

// --- [NONLINEAR]
fn residuals<M, D>(model: &M, observations: &Observations, scales: &DVector<f64>, parameters: &DVector<D>) -> DVector<D>
where
    M: Model,
    D: DualNum<Primitive = f64>,
{
    DVector::from_iterator(
        scales.len(),
        izip!(observations.sites.column_iter(), observations.values.row(0).iter(), scales.iter())
            .map(|(site, &value, &scale)| model.evaluate(&site.map(D::from), parameters).mul_add(D::from(scale), D::from(-value * scale))),
    )
}

fn objective<M, D>(model: &M, observations: &Observations, scales: &DVector<f64>, loss: Loss, parameters: &DVector<D>) -> D
where
    M: Model,
    D: DualNum<Primitive = f64>,
{
    residuals(model, observations, scales, parameters).iter().map(|residual| rho(loss, residual)).sum()
}

fn searched<M>(model: &M, observations: &Observations, scales: &DVector<f64>, loss: Loss, point: &[f64], slot: Option<&mut [f64]>) -> Result<f64, AnalysisError>
where M: Model {
    let point = DVector::from_column_slice(point);
    let value = match slot {
        Some(slot) => {
            let (weighted, design) = jacobian(|parameters: DVector<DualDVec64>| residuals(model, observations, scales, &parameters), &point);
            let (values, slopes): (Vec<_>, Vec<_>) = weighted.iter().map(|&residual| first_derivative(|value| rho(loss, &value), residual)).unzip();
            slot.copy_from_slice(design.tr_mul(&DVector::from_vec(slopes)).as_slice());
            values.into_iter().sum()
        }
        None => objective(model, observations, scales, loss, &point),
    };
    if !value.is_finite() {
        finite(residuals(model, observations, scales, &point).iter(), |observation| AnalysisError::Point { breakdown: Breakdown::Model, point: observation }).next().transpose()?;
    }
    Ok(value)
}

fn minima<K, F>(space: &Space<K>, starts: DMatrixView<'_, f64>, settings: &Settings<'_>, tolerance: Tolerance, objective: &F) -> Result<(NEVec<Solution>, Vec<AnalysisError>), NEVec<AnalysisError>>
where
    K: Sync,
    F: Objective<AnalysisError> + Sync,
{
    let (failed, solved): (Vec<_>, Vec<_>) = starts
        .par_column_iter()
        .map(|start| {
            optimize(space, start.into_owned(), Target::Minimize, objective, settings)
                .map_err(|failures| failures.into_nonempty_iter().map(AnalysisError::from).collect::<NEVec<_>>())
                .and_then(|solved| solved.map_err(NEVec::new))
        })
        .partition_map(Either::from);
    let failures = failed.into_iter().flatten().collect::<Vec<_>>();
    let resolution = tolerance.into_inner();
    let near = |left: &Solution, right: &Solution| {
        izip!(left.point.iter(), right.point.iter(), space.values()).all(|(first, second, bounds)| (first - second).abs() <= resolution * (bounds.last - bounds.start))
    };
    let distinct = solved
        .into_iter()
        .sorted_by(|left, right| left.objective.total_cmp(&right.objective))
        .fold(Vec::<Solution>::new(), |kept, solution| if kept.iter().any(|held| near(held, &solution)) { kept } else { iter::chain(kept, iter::once(solution)).collect() });
    match NEVec::try_from_vec(distinct) {
        Some(distinct) => Ok((distinct, failures)),
        None => Err(NEVec::try_from_vec(failures).unwrap_or_else(|| NEVec::new(AnalysisError::Shape { input: Input::Starts, expected: 1, found: 0 }))),
    }
}

fn linearized<M>(model: &M, observations: &Observations, scales: &DVector<f64>, options: Options, point: &DVector<f64>) -> Result<Estimate, NEVec<AnalysisError>>
where M: Model {
    let (weighted, slopes) = jacobian(|parameters: DVector<DualDVec64>| residuals(model, observations, scales, &parameters), point);
    finite(weighted.iter(), |observation| AnalysisError::Point { breakdown: Breakdown::Model, point: observation })
        .chain(finite(slopes.iter(), |entry| {
            let (observation, parameter) = slopes.vector_to_matrix_index(entry);
            AnalysisError::Design { point: observation, parameter }
        }))
        .collect::<Validated<(), _>>()
        .ok()?;
    let reweights = weighted.map(|residual| reweight(options.loss, residual));
    let target = DMatrix::from_iterator(weighted.len(), 1, weighted.iter().map(|residual| -residual));
    estimate(&scaled(&slopes, &reweights), &scaled(&target, &reweights), options.regularization, Some(point)).map_err(NEVec::new)
}

fn active<K>(space: &Space<K>, point: &DVector<f64>) -> Vec<usize> {
    iter::zip(point.iter(), space.values()).positions(|(&value, bounds)| almost::equal(value, bounds.start) || almost::equal(value, bounds.last)).collect()
}

pub fn least_squares<K, M>(model: &M, observations: &Observations, space: &Space<K>, starts: DMatrixView<'_, f64>, settings: &Settings<'_>, options: Options) -> Result<Fit, NEVec<AnalysisError>>
where
    K: Sync,
    M: Model,
{
    [single_response(observations), determined(observations.sites.ncols(), space.len().get())].into_iter().collect::<Validated<(), _>>().ok()?;
    let scales = scales(observations);
    let search = |point: &[f64], slot: Option<&mut [f64]>| searched(model, observations, &scales, options.loss, point, slot);
    let (distinct, failures) = minima(space, starts, settings, options.tolerance, &search)?;
    let point = &distinct.first().point;
    Ok(Fit { estimate: linearized(model, observations, &scales, options, point)?, active: active(space, point), minima: distinct, failures })
}

pub fn separable<K, S>(model: &S, observations: &Observations, space: &Space<K>, starts: DMatrixView<'_, f64>, settings: &Settings<'_>, options: Options) -> Result<Fit, NEVec<AnalysisError>>
where
    K: Sync,
    S: Separable,
{
    let (shapes, points) = (space.len().get(), observations.sites.ncols());
    [single_response(observations), determined(points, shapes.saturating_add(model.terms()))].into_iter().collect::<Validated<(), _>>().ok()?;
    let scales = scales(observations);
    let target = scaled(&observations.values.transpose(), &scales);
    let projected = |shape: &DVector<f64>| -> Result<(DVector<f64>, f64), AnalysisError> {
        let basis = scaled(&DMatrix::from_fn(points, model.terms(), |point, term| model.basis(&observations.sites.column(point).into_owned(), shape, term)), &scales);
        finite(basis.iter(), |entry| {
            let (point, parameter) = basis.vector_to_matrix_index(entry);
            AnalysisError::Design { point, parameter }
        })
        .next()
        .transpose()?;
        robust(&basis, &target, options).map(|solved| (solved.parameters.column(0).into_owned(), solved.lambda))
    };
    let search = |point: &[f64], slot: Option<&mut [f64]>| {
        projected(&DVector::from_column_slice(point)).and_then(|(coefficients, lambda)| {
            searched(&Profile { separable: model, coefficients: &coefficients }, observations, &scales, options.loss, point, slot).map(|value| lambda.mul_add(coefficients.norm_squared(), value))
        })
    };
    let (distinct, failures) = minima(space, starts, settings, options.tolerance, &search)?;
    let shape = &distinct.first().point;
    let (coefficients, _) = projected(shape).map_err(NEVec::new)?;
    let joint = DVector::from_iterator(shapes.strict_add(coefficients.len()), iter::chain(shape.iter(), coefficients.iter()).copied());
    Ok(Fit { estimate: linearized(&Joint { separable: model, shapes }, observations, &scales, options, &joint)?, active: active(space, shape), minima: distinct, failures })
}

pub fn propagate<M>(
    model: &M,
    uncertainty: &Uncertainty,
    sites: DMatrixView<'_, f64>,
    site_covariance: Option<&DMatrix<f64>>,
    weights: Option<&DVector<f64>>,
) -> Result<Prediction<f64>, NEVec<AnalysisError>>
where
    M: Model,
{
    let dimensions = sites.nrows();
    refused_reading(uncertainty, uncertainty.parameters.len(), sites.ncols(), weights)
        .chain(refused_sites(&sites, |coordinate: &f64| coordinate.is_finite()))
        .chain(iter::once(
            site_covariance.map_or(Ok(()), |covariance| {
                (covariance.shape() == (dimensions, dimensions)).ok_or(AnalysisError::Shape { input: Input::Covariance, expected: dimensions, found: covariance.nrows() })
            }),
        ))
        .collect::<Validated<(), _>>()
        .ok()?;
    let (mean, variance): (Vec<_>, Vec<_>) = sites
        .par_column_iter()
        .enumerate()
        .map(|(point, site)| {
            let (value, slope) = gradient(|parameters: DVector<DualDVec64>| model.evaluate(&site.map(DualDVec64::from_re), &parameters), &uncertainty.parameters);
            let propagated = site_covariance.map_or_default(|covariance| {
                let lifted = uncertainty.parameters.map(DualDVec64::from_re);
                let (_, tilt) = gradient(|coordinates: DVector<DualDVec64>| model.evaluate(&coordinates, &lifted), &site.into_owned());
                tilt.dot(&(covariance * &tilt))
            });
            iter::once(value.is_finite().ok_or(AnalysisError::Point { breakdown: Breakdown::Model, point }))
                .chain(finite(slope.iter(), |parameter| AnalysisError::Design { point, parameter }))
                .collect::<Validated<(), _>>()
                .map(|()| (value, slope.dot(&(&uncertainty.covariance * &slope)) + propagated))
        })
        .collect::<Validated<(Vec<_>, Vec<_>), _>>()
        .ok()?;
    Ok(Prediction { mean: DVector::from_vec(mean), variance: DVector::from_vec(variance), noise: noises(uncertainty.noise, weights, sites.ncols()), freedom: uncertainty.freedom })
}

pub fn log_density<'call, M>(model: &'call M, observations: &'call Observations, sigma: Tolerance) -> Result<impl Fn(&[f64], &mut [f64]) -> f64 + Sync + 'call, AnalysisError>
where M: Model {
    single_response(observations)?;
    let (scales, precision) = (scales(observations), sigma.into_inner().powi(-2));
    Ok(move |point: &[f64], slot: &mut [f64]| {
        let (weighted, design) = jacobian(|parameters: DVector<DualDVec64>| residuals(model, observations, &scales, &parameters), &DVector::from_column_slice(point));
        slot.copy_from_slice((design.tr_mul(&weighted) * -precision).as_slice());
        weighted.norm_squared() * precision * -0.5
    })
}

// --- [READINGS]
pub fn uncertainty(estimate: &Estimate, estimator: Estimator) -> Result<Vec<Uncertainty>, NEVec<AnalysisError>> {
    let (pseudoinverse, points, freedom) = (&estimate.pseudoinverse, estimate.leverage.len(), estimate.freedom());
    let gram = LazyCell::new(|| pseudoinverse * pseudoinverse.transpose());
    let scored = |factors: &DVector<f64>| scaled(&pseudoinverse.transpose(), factors).transpose();
    joined(estimate.residuals.row_iter().zip(estimate.parameters.column_iter()).enumerate().map(|(response, (lack, parameters))| {
        let reduced = lack.norm_squared() / freedom;
        let (covariance, noise, freedom) = match estimator {
            Estimator::ReducedChiSquare => (&*gram * reduced, reduced, freedom),
            Estimator::Known(sigma) => (&*gram * sigma.into_inner().powi(2), sigma.into_inner().powi(2), f64::INFINITY),
            Estimator::Hc3 => {
                let factors = lack.transpose().zip_map(&estimate.leverage, |entry, leverage| entry / (1.0 - leverage));
                finite(factors.iter(), |point| AnalysisError::Influence { response, point }).collect::<Validated<(), _>>().ok()?;
                let influence = scored(&factors);
                (&influence * influence.transpose(), reduced, freedom)
            }
            Estimator::NeweyWest { lags } => {
                let influence = scored(&lack.transpose());
                let reach = lags.get().min(points.saturating_sub(1));
                let serial = (1..=reach).fold(&influence * influence.transpose(), |total, lag| {
                    let cross = influence.columns(lag, points.strict_sub(lag)) * influence.columns(0, points.strict_sub(lag)).transpose();
                    total + (&cross + cross.transpose()) * (1.0 - convert::<usize, f64>(lag) / convert::<usize, f64>(reach.strict_add(1)))
                });
                (serial * (convert::<usize, f64>(points) / freedom), reduced, freedom)
            }
        };
        Ok(Uncertainty { parameters: parameters.into_owned(), covariance, noise, freedom })
    }))
}

pub fn observed_information<M>(model: &M, observations: &Observations, loss: Loss, estimate: &Estimate) -> Result<Uncertainty, NEVec<AnalysisError>>
where M: Model {
    single_response(observations).map_err(NEVec::new)?;
    let parameters = estimate.parameters.column(0).into_owned();
    let weights = scales(observations);
    let (value, _, curvature) = hessian(|point: DVector<Dual2DVec64>| objective(model, observations, &weights, loss, &point), &parameters);
    (value.is_finite() && curvature.iter().all(|entry| entry.is_finite())).ok_or_else(|| NEVec::new(AnalysisError::Overflow { parameters: estimate.parameters.clone(), residual: value }))?;
    let spectrum = decompose(&curvature).map_err(NEVec::new)?;
    let freedom = estimate.freedom();
    let noise = value / freedom;
    let gains = spectrum.singular.map_with_location(|index, _, value| if index < spectrum.rank { 2.0 * noise / value } else { 0.0 });
    Ok(Uncertainty { parameters, covariance: &spectrum.right * DMatrix::from_diagonal(&gains) * spectrum.left.transpose(), noise, freedom })
}

pub fn influence(estimate: &Estimate) -> Result<Vec<Influence>, NEVec<AnalysisError>> {
    let (leverage, freedom) = (&estimate.leverage, estimate.freedom());
    let count = convert::<usize, f64>(leverage.len());
    let effective = count - freedom;
    joined(estimate.residuals.row_iter().enumerate().map(|(response, lack)| {
        let sum = lack.norm_squared();
        let reduced = sum / freedom;
        let studentized = lack.transpose().zip_map(leverage, |entry, hat| entry / (reduced * (1.0 - hat)).sqrt());
        let cook = studentized.zip_map(leverage, |value, hat| value.powi(2) * hat / (effective * (1.0 - hat)));
        let refusal = move |point| AnalysisError::Influence { response, point };
        finite(studentized.iter(), refusal).chain(finite(cook.iter(), refusal)).collect::<Validated<(), _>>().ok()?;
        let press = iter::zip(lack.iter(), leverage.iter()).map(|(entry, leverage)| (entry / (1.0 - leverage)).powi(2)).sum::<f64>();
        let aicc = count.mul_add((sum / count).ln(), 2.0_f64.mul_add(effective, 2.0 * effective * (effective + 1.0) / (freedom - 1.0)));
        Ok(Influence { studentized, cook, press, aicc: Some(aicc).filter(|value| value.is_finite() && freedom > 1.0) })
    }))
}

pub fn band(prediction: &Prediction<f64>, level: Confidence, band: Band) -> Result<Vec<RangeInclusive<f64>>, AnalysisError> {
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
