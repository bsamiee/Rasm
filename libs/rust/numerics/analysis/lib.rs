#![feature(cmp_minmax, const_result_trait_fn, const_trait_impl, control_flow_into_value, default_field_values, deref_patterns, vec_from_fn)]

use core::{borrow::Borrow, iter, range::RangeInclusive};

use csaps::CsapsError;
use itertools::Itertools as _;
use nalgebra::{DMatrix, DVector, Dim, Matrix, RawStorage, Scalar};
use ndarray::ShapeError;
use nonempty_collections::NEVec;
use rasm_optimize::OptimizeError;
use rasm_stats::Tolerance;
use spade::InsertionError;
use statrs::distribution::StudentsTError;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Input {
    Sites,
    Values,
    Weights,
    Responses,
    Exponents,
    Coefficients,
    Covariance,
    Starts,
    Axes,
    Readings,
    #[display("length scales")]
    Lengths,
    #[display("integrand output")]
    Integrand,
    Equations,
    #[display("sites along dimension {dimension}")]
    AxisSites {
        dimension: usize,
    },
    #[display("weights along dimension {dimension}")]
    AxisWeights {
        dimension: usize,
    },
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOLERANCE: Tolerance = Tolerance::try_new(almost::F64_TOLERANCE).ok().unwrap();

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Observations {
    sites: DMatrix<f64>,
    values: DMatrix<f64>,
    weights: DVector<f64>,
}

impl Observations {
    pub fn new(sites: DMatrix<f64>, values: DMatrix<f64>, weights: Option<DVector<f64>>) -> Result<Self, NEVec<AnalysisError>> {
        let points = sites.ncols();
        let weights = weights.unwrap_or_else(|| DVector::repeat(points, 1.0));
        iter::chain(
            [(Input::Values, values.ncols()), (Input::Weights, weights.len())].map(|(input, found)| (found == points).ok_or(AnalysisError::Shape { input, expected: points, found })),
            refused_sites(&sites, |coordinate: &f64| coordinate.is_finite()),
        )
        .chain(finite(values.iter(), |entry| {
            let (response, point) = values.vector_to_matrix_index(entry);
            AnalysisError::Value { index: vec![response, point] }
        }))
        .chain(refused_weights(None, weights.iter()))
        .collect::<Validated<(), _>>()
        .ok()
        .map(|()| Self { sites, values, weights })
    }

    #[must_use]
    pub const fn sites(&self) -> &DMatrix<f64> { &self.sites }

    #[must_use]
    pub const fn values(&self) -> &DMatrix<f64> { &self.values }

    #[must_use]
    pub const fn weights(&self) -> &DVector<f64> { &self.weights }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum AnalysisError {
    #[error("{input} holds {found} entries where {expected} are needed")]
    Shape { input: Input, expected: usize, found: usize },
    #[error("site {point} falls outside the representable range in dimension {dimension}")]
    Site { dimension: usize, point: usize },
    #[error("value [{}] is not finite", .index.iter().format(", "))]
    Value { index: Vec<usize> },
    #[error("weight {point}{} is {weight}, not a positive finite number", .dimension.map_or_else(String::new, |dimension| format!(" along dimension {dimension}")))]
    Weight { dimension: Option<usize>, point: usize, weight: f64 },
    #[error("{found} points where {needed} are needed")]
    Underdetermined { found: usize, needed: usize },
    #[error("site {point} along dimension {dimension} does not exceed its predecessor")]
    Unordered { dimension: usize, point: usize },
    #[error("{at} falls outside the span of dimension {dimension}")]
    Outside { dimension: usize, at: f64 },
    #[error("sites along dimension {dimension} span {span}, reaching their period {period}")]
    Period { dimension: usize, span: f64, period: f64 },
    #[error("model slope at point {point} for parameter {parameter} is not finite")]
    Design { point: usize, parameter: usize },
    #[error("{breakdown} at point {point}")]
    Point { breakdown: Breakdown, point: usize },
    #[error("fit overflows at residual sum {residual} over parameters {parameters}")]
    Overflow { parameters: DMatrix<f64>, residual: f64 },
    #[error("influence of point {point} on response {response} is not finite")]
    Influence { response: usize, point: usize },
    #[error("decomposition reached its iteration limit")]
    NoConvergence,
    #[error("polynomial is identically zero")]
    Zero,
    #[error("coefficient {index} of {coefficient} over leading coefficient {leading} is not finite")]
    Coefficient { index: usize, coefficient: f64, leading: f64 },
    #[error("function is not finite over {}", .interval.iter().format_with(", ", |range, write| write(&format_args!("[{}, {}]", range.start, range.last))))]
    Function { interval: Vec<RangeInclusive<f64>> },
    #[error("root search over [{}, {}] failed", .bracket.start, .bracket.last)]
    Root { bracket: RangeInclusive<f64> },
    #[error("root {root} holds no parameter derivative where the equation's slope vanishes")]
    Tangent { root: f64 },
    #[error("newton iteration stopped at residual {residual} at {point}")]
    System { point: DVector<f64>, residual: f64 },
    #[error("integral {estimate} holds error estimate {error} past tolerance")]
    Integral { estimate: f64, error: f64 },
    #[error("breakline from site {from} to site {to} joins no two distinct sites or crosses another")]
    Breakline { from: usize, to: usize },
    #[error("drift term {term} is collinear with earlier terms at the sites")]
    Drift { term: usize },
    #[error(transparent)]
    Optimize(#[from] OptimizeError),
    #[error(transparent)]
    Spline(#[from] CsapsError),
    #[error(transparent)]
    Triangulation(#[from] InsertionError),
    #[error(transparent)]
    Distribution(#[from] StudentsTError),
    #[error(transparent)]
    Reshape(#[from] ShapeError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn determined(points: usize, parameters: usize) -> Result<(), AnalysisError> { (points > parameters).ok_or(AnalysisError::Underdetermined { found: points, needed: parameters.saturating_add(1) }) }

fn finite<I>(entries: I, refusal: impl Fn(usize) -> AnalysisError) -> impl Iterator<Item = Result<(), AnalysisError>>
where I: IntoIterator<Item: Borrow<f64>> {
    entries.into_iter().positions(|entry| !entry.borrow().is_finite()).map(move |index| Err(refusal(index)))
}

fn refused_sites<T, R, C, S>(sites: &Matrix<T, R, C, S>, representable: impl Fn(&T) -> bool) -> impl Iterator<Item = Result<(), AnalysisError>>
where
    T: Scalar,
    R: Dim,
    C: Dim,
    S: RawStorage<T, R, C>,
{
    sites.iter().positions(move |coordinate| !representable(coordinate)).map(move |entry| {
        let (dimension, point) = sites.vector_to_matrix_index(entry);
        Err(AnalysisError::Site { dimension, point })
    })
}

fn refused_weights<'weights>(dimension: Option<usize>, weights: impl IntoIterator<Item = &'weights f64>) -> impl Iterator<Item = Result<(), AnalysisError>> {
    weights.into_iter().enumerate().filter(|&(_, &weight)| !weight.is_finite() || weight <= 0.0).map(move |(point, &weight)| Err(AnalysisError::Weight { dimension, point, weight }))
}

fn joined<T>(outcomes: impl IntoIterator<Item = Result<T, NEVec<AnalysisError>>>) -> Result<Vec<T>, NEVec<AnalysisError>> {
    outcomes.into_iter().map(|outcome| outcome.map_or_else(Validated::Fail, Validated::Good)).collect::<Validated<_, _>>().ok()
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod quadrature;
pub mod regression;
pub mod roots;
pub mod scattered;
pub mod splines;
