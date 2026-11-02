#![feature(const_result_trait_fn, const_trait_impl, default_field_values)]

use core::{iter, num::NonZero};

use csaps::CsapsError;
use faer::{
    MatRef,
    linalg::{solvers::Svd, svd::SvdError},
};
use itertools::Itertools as _;
use nalgebra::{DMatrix, DVector, convert};
use nonempty_collections::NEVec;
use rasm_optimize::OptimizeError;
use rasm_stats::Bounds;
use spade::InsertionError;
use validated::Validated;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Observations {
    sites: DMatrix<f64>,
    values: DVector<f64>,
    weights: DVector<f64>,
}

impl Observations {
    pub fn new(sites: DMatrix<f64>, values: DVector<f64>, weights: Option<DVector<f64>>) -> Result<Self, NEVec<AnalysisError>> {
        let points = sites.ncols();
        let weights = weights.unwrap_or_else(|| DVector::repeat(points, 1.0));
        iter::chain(
            [
                (values.len() == points).ok_or(AnalysisError::Shape { expected: points, found: values.len() }),
                (weights.len() == points).ok_or(AnalysisError::Shape { expected: points, found: weights.len() }),
            ],
            sites.iter().positions(|coordinate| !coordinate.is_finite()).map(|entry| sites.vector_to_matrix_index(entry)).map(|(dimension, point)| Err(AnalysisError::Site { dimension, point })),
        )
        .chain(values.iter().positions(|value| !value.is_finite()).map(|index| Err(AnalysisError::Value { index })))
        .chain(weights.iter().enumerate().filter(|&(_, weight)| !weight.is_finite() || *weight <= 0.0).map(|(index, &weight)| Err(AnalysisError::Weight { axis: 0, index, weight })))
        .collect::<Validated<(), _>>()
        .ok()
        .map(|()| Self { sites, values, weights })
    }

    #[must_use]
    pub const fn sites(&self) -> &DMatrix<f64> { &self.sites }

    #[must_use]
    pub const fn values(&self) -> &DVector<f64> { &self.values }

    #[must_use]
    pub const fn weights(&self) -> &DVector<f64> { &self.weights }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Estimate {
    pub parameters: DVector<f64>,
    pub covariance: DMatrix<f64>,
    pub residual: f64,
    pub freedom: NonZero<usize>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Prediction<T> {
    pub mean: DVector<T>,
    pub variance: DVector<T>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum AnalysisError {
    #[error("{found} entries along an axis of {expected}")]
    Shape { expected: usize, found: usize },
    #[error("site {point} is not finite in dimension {dimension}")]
    Site { dimension: usize, point: usize },
    #[error("value {index} is not finite")]
    Value { index: usize },
    #[error("weight {index} along axis {axis} is {weight}, not a positive finite number")]
    Weight { axis: usize, index: usize, weight: f64 },
    #[error("{found} observations where the fit needs {needed}")]
    Observations { found: usize, needed: usize },
    #[error("site {index} along axis {axis} does not exceed its predecessor")]
    Order { axis: usize, index: usize },
    #[error("design entry of point {point} for parameter {parameter} is not finite")]
    Design { point: usize, parameter: usize },
    #[error("design of rank {rank} determines no {parameters} parameters")]
    Rank { rank: usize, parameters: usize },
    #[error("singular value decomposition reached its iteration limit")]
    Svd,
    #[error("eigenvalue decomposition reached its iteration limit")]
    Evd,
    #[error("polynomial is identically zero")]
    Zero,
    #[error("function is not finite at {at}")]
    Function { at: f64 },
    #[error("root search over [{}, {}] failed", .bracket.start, .bracket.last)]
    Root { bracket: Bounds },
    #[error("integrand is not finite over [{}, {}]", .interval.start, .interval.last)]
    Integrand { interval: Bounds },
    #[error("integral {estimate} holds an error estimate of {error} past its tolerance")]
    Integral { estimate: f64, error: f64 },
    #[error(transparent)]
    Optimize(#[from] OptimizeError),
    #[error(transparent)]
    Spline(#[from] CsapsError),
    #[error(transparent)]
    Triangulation(#[from] InsertionError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn decompose(design: &DMatrix<f64>) -> Result<Svd<f64>, AnalysisError> {
    let (points, parameters) = design.shape();
    design.iter().position(|entry| !entry.is_finite()).map(|entry| design.vector_to_matrix_index(entry)).map_or(Ok(()), |(point, parameter)| Err(AnalysisError::Design { point, parameter }))?;
    let svd = MatRef::from_column_major_slice(design.as_slice(), points, parameters).thin_svd().map_err(|SvdError::NoConvergence| AnalysisError::Svd)?;
    let singular = svd.S().column_vector();
    let threshold = f64::EPSILON * convert::<usize, f64>(points.max(parameters)) * singular.norm_max();
    let rank = singular.iter().filter(|&&value| value > threshold).count();
    (rank == parameters).ok_or(AnalysisError::Rank { rank, parameters }).map(|()| svd)
}

fn estimate(svd: &Svd<f64>, parameters: DVector<f64>, residual: f64, freedom: NonZero<usize>) -> Estimate {
    let (directions, singular) = (svd.V(), svd.S());
    let whitened = DMatrix::from_fn(directions.ncols(), directions.nrows(), |row, column| directions[(column, row)] / singular[row]);
    Estimate { parameters, covariance: whitened.tr_mul(&whitened) * (residual / convert::<usize, f64>(freedom.get())), residual, freedom }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod least_squares;
pub mod quadrature;
pub mod regression;
pub mod roots;
pub mod scattered;
pub mod splines;
