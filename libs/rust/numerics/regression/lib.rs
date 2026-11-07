#![feature(cmp_minmax, const_result_trait_fn, const_trait_impl, control_flow_into_value, default_field_values, integer_casts)]

use core::{cell::LazyCell, cmp, iter, num::NonZero, ops::ControlFlow, range::RangeInclusive};

use faer::{MatRef, linalg::svd::SvdError};
use itertools::{Itertools as _, izip};
use libm::frexp;
use nalgebra::{DMatrix, DMatrixView, DVector, DVectorView, Matrix2, Vector2, convert};
use nlopt::Target;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use num_dual::{Dual2DVec64, DualDVec64, DualNum, gradient, hessian, jacobian};
use rasm_numerical_analysis::{AnalysisError, Input, Observations, Placement, Polynomial, Prediction, SearchHooks, design, roots::minimum};
use rasm_optimization::{OptimizeError, Settings, Solution, optimize};
use rasm_stats::{Confidence, Space, Tolerance};
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
    pub iterations: NonZero<usize> = NonZero::<usize>::saturating_add(NonZero::<usize>::MIN, 24),
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

#[derive(Debug)]
pub struct Fit {
    pub estimate: Estimate,
    pub active: Vec<usize>,
    pub minima: NEVec<Solution>,
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
    #[error(transparent)]
    Analysis(#[from] AnalysisError),
    #[error(transparent)]
    Optimize(#[from] OptimizeError),
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
            let start = 2.0_f64.mul_add(lowest.ln(), f64::EPSILON.ln());
            minimum(gcv, start, 2.0 * spectrum.singular.amax().ln(), f64::from(almost::F32_TOLERANCE), hooks).map(|(exponent, _)| exponent.exp())
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

fn rho<D>(loss: Loss, residual: &D) -> D
where D: DualNum<Primitive = f64> {
    #[expect(clippy::arithmetic_side_effects, reason = "Clippy cannot match arithmetic allowances through DualNum bounds")]
    fn lifted<D, I>(residual: &D, value: f64, coefficients: impl FnOnce() -> I) -> D
    where
        D: DualNum<Primitive = f64>,
        I: Iterator<Item = (f64, i128)>,
    {
        fn scaled<D>(value: &D, exponent: u128, direction: f64) -> D
        where D: DualNum<Primitive = f64> {
            let chunk = if direction > 0.0 { u128::from(f64::MAX_EXP.unsigned_abs().strict_sub(1)) } else { u128::from(f64::MIN_EXP.unsigned_abs()) };
            (0..exponent.div_ceil(chunk)).fold(value.clone(), |value, index| value * (direction * convert::<u128, f64>(exponent.strict_sub(index.strict_mul(chunk)).min(chunk))).exp2())
        }

        if D::NDERIV == 0 {
            return D::from(value);
        }
        let delta = residual.clone() - residual.re();
        coefficients().zip(0..D::NDERIV).filter(|((coefficient, _), _)| *coefficient != 0.0).fold(D::from(value), |sum, ((coefficient, exponent), index)| {
            let order = NonZero::<u128>::MIN.saturating_add(index.strict_cast::<u128>());
            let magnitude = exponent.unsigned_abs();
            let (quotient, remainder) = (magnitude / order, magnitude % order);
            let direction = if exponent < 0 { -1.0 } else { 1.0 };
            let lower = scaled(&delta, quotient, direction);
            let upper = LazyCell::new(|| scaled(&lower, 1, direction));
            sum + (0..order.get()).fold(D::from(coefficient), |term, power| term * if power < remainder { &*upper } else { &lower })
        })
    }

    fn cauchy<D>(scale: f64, residual: &D) -> D
    where D: DualNum<Primitive = f64> {
        let real = residual.re();
        let inside = real.abs() <= scale;
        let ratio = if inside { real / scale } else { scale / real };
        let square = ratio.powi(2);
        let value = if inside {
            let factor = if square == 0.0 { 1.0 } else { square.ln_1p() / square };
            real * factor * real
        } else {
            let logarithm = if ratio == 0.0 { real.abs().ln() - scale.ln() } else { -ratio.abs().ln() };
            (2.0_f64.mul_add(logarithm, square.ln_1p()) * scale) * scale
        };
        lifted(residual, value, move || {
            let (scale_mantissa, scale_exponent) = frexp(scale);
            let (real_mantissa, real_exponent) = frexp(real);
            let (scale_exponent, real_exponent) = (i128::from(scale_exponent), i128::from(real_exponent));
            let divisor = if inside { (scale_mantissa, scale_exponent) } else { (real_mantissa, real_exponent) };
            (1..=D::NDERIV).scan((Vector2::new(1.0, 0.0), scale_mantissa.powi(2), scale_exponent.strict_mul(2)), move |state, order| {
                let (inverse, mantissa, exponent) = *state;
                let even = order.is_multiple_of(2);
                let (real_factor, cross_factor, imaginary_factor) = match (inside, even) {
                    (true, false) => (1.0, 1.0, square),
                    (true, true) => (square, 1.0, 1.0),
                    (false, _) => (1.0, square, 1.0),
                };
                let inverse = Matrix2::new(real_factor, cross_factor, -1.0, imaginary_factor) * inverse / (1.0 + square);
                let (mantissa, shift) = frexp(mantissa / divisor.0);
                let exponent = exponent.strict_add(i128::from(shift)).strict_sub(divisor.1);
                let factor = inverse[0] * (if even { -2.0 } else { 2.0 }) / convert::<usize, f64>(order);
                let coefficient =
                    if inside && !even { (mantissa * factor * real_mantissa / scale_mantissa, exponent.strict_add(real_exponent).strict_sub(scale_exponent)) } else { (mantissa * factor, exponent) };
                *state = (inverse, mantissa, exponent);
                Some(coefficient)
            })
        })
    }

    fn soft_l1<D>(scale: f64, residual: &D) -> D
    where D: DualNum<Primitive = f64> {
        let real = residual.re();
        let inside = real.abs() <= scale;
        let ratio = if inside { real / scale } else { scale / real };
        let unit = 1.0_f64.hypot(ratio);
        let value = if inside { real / (unit + 1.0) * 2.0 * real } else { real.abs() / (unit + ratio.abs()) * scale * 2.0 };
        lifted(residual, value, move || {
            let (scale_mantissa, scale_exponent) = frexp(scale);
            let (real_mantissa, real_exponent) = frexp(real);
            let (scale_exponent, real_exponent) = (i128::from(scale_exponent), i128::from(real_exponent));
            let (radius_mantissa, radius_exponent) = {
                let (mantissa, exponent) = if inside { (scale_mantissa, scale_exponent) } else { (real_mantissa.abs(), real_exponent) };
                let (mantissa, shift) = frexp(mantissa * unit);
                (mantissa, exponent.strict_add(i128::from(shift)))
            };
            let coordinate = if inside { ratio / unit } else { real.signum() / unit };
            (1..=D::NDERIV).scan((Vector2::new(0.5, 0.0), scale_mantissa.powi(3) / radius_mantissa, scale_exponent.strict_mul(3).strict_sub(radius_exponent)), move |state, order| {
                let (previous, mantissa, exponent) = *state;
                let degree = convert::<usize, f64>(order);
                let previous = if order > 2 { Matrix2::new(-(2.0_f64.mul_add(degree, -3.0)) * coordinate / degree, -(degree - 3.0) / degree, 1.0, 0.0) * previous } else { previous };
                let (mantissa, shift) = frexp(mantissa / radius_mantissa);
                let exponent = exponent.strict_add(i128::from(shift)).strict_sub(radius_exponent);
                let coefficient = match (order == 1, inside) {
                    (true, true) => (real_mantissa * (2.0 / unit), real_exponent),
                    (true, false) => (scale_mantissa.copysign(real) * (2.0 / unit), scale_exponent),
                    (false, _) => (mantissa * 2.0 * previous[0], exponent),
                };
                *state = (previous, mantissa, exponent);
                Some(coefficient)
            })
        })
    }

    match loss {
        Loss::Squared => residual.powi(2),
        Loss::Huber(scale) => {
            let scale = scale.into_inner();
            if residual.re().abs() <= scale { residual.powi(2) } else { residual.abs().mul_add(D::from(2.0 * scale), D::from(-scale * scale)) }
        }
        Loss::SoftL1(scale) => soft_l1(scale.into_inner(), residual),
        Loss::Cauchy(scale) => cauchy(scale.into_inner(), residual),
    }
}

fn reweight(loss: Loss, residual: f64) -> f64 {
    match loss {
        Loss::Squared => 1.0,
        Loss::Huber(scale) => {
            let scale = scale.into_inner();
            if residual.abs() <= scale { 1.0 } else { scale.sqrt() / residual.abs().sqrt() }
        }
        Loss::SoftL1(scale) => {
            let scale = scale.into_inner();
            let magnitude = residual.abs().max(scale);
            (scale.sqrt() / magnitude.sqrt()) / (residual / magnitude).hypot(scale / magnitude).sqrt()
        }
        Loss::Cauchy(scale) => {
            let scale = scale.into_inner();
            let magnitude = residual.abs().max(scale);
            (scale / magnitude) / (residual / magnitude).hypot(scale / magnitude)
        }
    }
}

fn robust(design: &DMatrix<f64>, target: &DMatrix<f64>, options: Options, hooks: SearchHooks<'_>) -> Result<Estimate, RegressionError> {
    let first = estimate(design, target, options.regularization, None, hooks)?;
    match options.loss {
        Loss::Squared => Ok(first),
        Loss::Huber(_) | Loss::SoftL1(_) | Loss::Cauchy(_) => (1..options.iterations.get())
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

fn objective<M, D>(model: &M, observations: &Observations, scales: &DVector<f64>, loss: Loss, parameters: DVectorView<'_, D>) -> D
where
    M: Model,
    D: DualNum<Primitive = f64>,
{
    residuals(model, observations, scales, parameters).map(|residual| rho(loss, &residual)).sum()
}

fn searched<M>(model: &M, observations: &Observations, scales: &DVector<f64>, loss: Loss, point: &[f64], slot: Option<&mut [f64]>) -> Result<f64, RegressionError>
where M: Model {
    let point = DVectorView::from_slice(point, point.len());
    let value = match slot {
        Some(slot) => {
            let (value, slope) = gradient(|parameters: DVector<DualDVec64>| objective(model, observations, scales, loss, parameters.as_view()), &point.into_owned());
            slot.copy_from_slice(slope.as_slice());
            value
        }
        None => objective(model, observations, scales, loss, point),
    };
    if !value.is_finite() {
        residuals(model, observations, scales, point).position(|residual| !residual.is_finite()).map_or(Ok(()), |point| Err(RegressionError::Model { point }))?;
    }
    Ok(value)
}

fn minima<K, F>(space: &Space<K>, starts: DMatrixView<'_, f64>, settings: &Settings<'_>, tolerance: Tolerance, objective: &F) -> Result<(NEVec<Solution>, Vec<RegressionError>), NEVec<RegressionError>>
where
    K: Sync,
    F: Fn(&[f64], Option<&mut [f64]>) -> Result<f64, RegressionError> + Sync,
{
    let (failed, solved): (Vec<_>, Vec<_>) = starts
        .par_column_iter()
        .map(|start| {
            optimize(space, start.into_owned(), Target::Minimize, objective, settings)
                .map_err(|failure| failure.either(NEVec::new, |failures| failures.into_nonempty_iter().map(RegressionError::from).collect()))
        })
        .partition_map(Either::from);
    let failures = failed.into_iter().flatten().collect::<Vec<_>>();
    let resolution = tolerance.into_inner();
    let near = |left: &Solution, right: &Solution| {
        izip!(left.point.iter(), right.point.iter(), space.values()).all(|(first, second, bounds)| (first - second).abs() <= resolution * (bounds.last - bounds.start))
    };
    let distinct = solved.into_iter().sorted_by(|left, right| left.objective.total_cmp(&right.objective)).fold(Vec::<Solution>::new(), |mut kept, solution| {
        if !kept.iter().any(|held| near(held, &solution)) {
            kept.push(solution);
        }
        kept
    });
    match NEVec::try_from_vec(distinct) {
        Some(distinct) => Ok((distinct, failures)),
        None => Err(NEVec::try_from_vec(failures).unwrap_or_else(|| NEVec::new(AnalysisError::Shape { input: Input::Starts, expected: 1, found: 0 }.into()))),
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

pub fn least_squares<K, M>(
    model: &M,
    observations: &Observations,
    space: &Space<K>,
    starts: DMatrixView<'_, f64>,
    settings: &Settings<'_>,
    options: Options,
    hooks: SearchHooks<'_>,
) -> Result<Fit, NEVec<RegressionError>>
where
    K: Sync,
    M: Model,
{
    [single_response(observations), observations.determines(space.len().get()).map_err(RegressionError::from)].into_iter().collect::<Validated<(), _>>().ok()?;
    let scales = scales(observations);
    let search = |point: &[f64], slot: Option<&mut [f64]>| searched(model, observations, &scales, options.loss, point, slot);
    let (distinct, failures) = minima(space, starts, settings, options.tolerance, &search)?;
    let point = &distinct.first().point;
    Ok(Fit { estimate: linearized(model, observations, &scales, options, point, hooks)?, active: active(space, point), minima: distinct, failures })
}

pub fn separable<K, S>(
    model: &S,
    observations: &Observations,
    space: &Space<K>,
    starts: DMatrixView<'_, f64>,
    settings: &Settings<'_>,
    options: Options,
    hooks: SearchHooks<'_>,
) -> Result<Fit, NEVec<RegressionError>>
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
    let search = |point: &[f64], slot: Option<&mut [f64]>| {
        projected(DVectorView::from_slice(point, point.len())).and_then(|(coefficients, lambda)| {
            searched(&Profile { separable: model, coefficients: &coefficients }, observations, &scales, options.loss, point, slot).map(|value| lambda.mul_add(coefficients.norm_squared(), value))
        })
    };
    let (distinct, failures) = minima(space, starts, settings, options.tolerance, &search)?;
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
    refused_reading(uncertainty, sites.ncols(), weights)
        .into_iter()
        .chain(sites.iter().positions(|coordinate| !coordinate.is_finite()).map(|entry| {
            let (dimension, point) = sites.vector_to_matrix_index(entry);
            Err(AnalysisError::Site { dimension, point, placement: Placement::Unrepresentable }.into())
        }))
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
        let (value, slope) = gradient(|parameters: DVector<DualDVec64>| objective(model, observations, &scales, Loss::Squared, parameters.as_view()), &DVector::from_column_slice(point));
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
    let (noise, spectrum) = {
        let (value, _, curvature) = hessian(|point: DVector<Dual2DVec64>| objective(model, observations, &weights, loss, point.as_view()), &parameters);
        (value.is_finite() && curvature.iter().all(|entry| entry.is_finite())).ok_or_else(|| NEVec::new(RegressionError::Overflow { parameters: estimate.parameters.clone(), residual: value }))?;
        (value / freedom, decompose(&curvature).map_err(NEVec::new)?)
    };
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
