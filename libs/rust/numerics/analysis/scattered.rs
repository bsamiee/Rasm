use core::{f64::consts::TAU, iter};
use std::sync::LazyLock;

use faer::{
    MatRef, Side,
    linalg::{
        cholesky::llt::factor::LltError,
        solvers::{Llt, Solve as _},
    },
};
use itertools::{Itertools as _, izip};
use nalgebra::{DMatrix, DMatrixView, DVector, DVectorView, Dyn, MatrixView2xX, Point2, U1, convert};
use nonempty_collections::NEVec;
use rasm_stats::Tolerance;
use rayon::iter::ParallelIterator as _;
use spade::{
    ConstrainedDelaunayTriangulation, DelaunayTriangulation, FloatTriangulation as _, HasPosition, HierarchyHintGenerator, InsertionError, Triangulation as _,
    handles::{FixedVertexHandle, VertexHandle},
};
use validated::Validated;

use crate::{
    AnalysisError, Breakdown, Input, Observations, determined, finite, refused_sites,
    regression::{Polynomial, Prediction, design},
};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Delaunay = DelaunayTriangulation<Observation, (), (), (), HierarchyHintGenerator<f64>>;

pub type Constrained = ConstrainedDelaunayTriangulation<Observation, (), (), (), HierarchyHintGenerator<f64>>;

pub type Sites<'sites> = MatrixView2xX<'sites, f64, U1, Dyn>;

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Interpolant {
    NearestNeighbor,
    NaturalNeighbor,
    Gradient(Flatness),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Kernel {
    Exponential,
    Matern32,
    Matern52,
    Gaussian,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Observation {
    pub position: Point2<f64>,
    pub index: usize,
    pub value: f64,
}

impl HasPosition for Observation {
    type Scalar = f64;

    fn position(&self) -> spade::Point2<f64> { spade::mitigate_underflow(<[f64; 2]>::from(self.position).into()) }
}

#[nutype::nutype(validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq))]
pub struct Flatness(f64);

#[derive(Debug, Clone)]
pub struct Kriging {
    sites: DMatrix<f64>,
    lengths: DVector<f64>,
    variance: f64,
    kernel: Kernel,
    nugget: f64,
    basis: DMatrix<f64>,
    drift: Polynomial,
    weights: DMatrix<f64>,
    factor: Llt<f64>,
    trend: Llt<f64>,
    likelihood: f64,
}

impl Kriging {
    #[must_use]
    pub const fn drift(&self) -> &Polynomial { &self.drift }

    #[must_use]
    pub const fn likelihood(&self) -> f64 { self.likelihood }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TRIANGULATIONS]
fn observations(sites: Sites<'_>, values: DVectorView<'_, f64>) -> Result<Vec<Observation>, NEVec<AnalysisError>> {
    iter::once((values.len() == sites.ncols()).ok_or(AnalysisError::Shape { input: Input::Values, expected: sites.ncols(), found: values.len() }))
        .chain(refused_sites(&sites, |coordinate: &f64| matches!(spade::validate_coordinate(*coordinate), Ok(()) | Err(InsertionError::TooSmall))))
        .chain(finite(values.iter(), |point| AnalysisError::Value { index: vec![point] }))
        .collect::<Validated<(), _>>()
        .ok()?;
    Ok(iter::zip(sites.column_iter(), values.iter()).enumerate().map(|(index, (site, &value))| Observation { position: Point2::new(site.x, site.y), index, value }).collect())
}

pub fn triangulate(sites: Sites<'_>, values: DVectorView<'_, f64>) -> Result<Delaunay, NEVec<AnalysisError>> {
    DelaunayTriangulation::bulk_load_stable(observations(sites, values)?).map_err(|source| NEVec::new(AnalysisError::from(source)))
}

pub fn constrain(sites: Sites<'_>, values: DVectorView<'_, f64>, breaklines: &[[usize; 2]]) -> Result<Constrained, NEVec<AnalysisError>> {
    let points = sites.ncols();
    breaklines.iter().filter(|&&[from, to]| from == to || from >= points || to >= points).map(|&[from, to]| Err(AnalysisError::Breakline { from, to })).collect::<Validated<(), _>>().ok()?;
    let mut conflicts = Vec::new();
    let constrained = ConstrainedDelaunayTriangulation::try_bulk_load_cdt(observations(sites, values)?, breaklines.to_vec(), |edge| conflicts.push(edge))
        .map_err(|source| NEVec::new(AnalysisError::from(source)))?;
    let named = |vertex: usize| constrained.vertex(FixedVertexHandle::from_index(vertex)).data().index;
    conflicts.into_iter().map(|[from, to]| Err(AnalysisError::Breakline { from: named(from), to: named(to) })).collect::<Validated<(), _>>().ok()?;
    Ok(constrained)
}

fn query<S, M, I, A, F>(sites: Sites<'_>, init: I, answer: A, finite: F) -> Result<Vec<Option<M>>, NEVec<AnalysisError>>
where
    I: Fn() -> S + Send + Sync,
    A: Fn(&mut S, spade::Point2<f64>) -> Option<M> + Send + Sync,
    F: Fn(&M) -> bool,
    M: Send,
{
    refused_sites(&sites, |coordinate: &f64| coordinate.is_finite()).collect::<Validated<(), _>>().ok()?;
    let answers = sites
        .par_column_iter()
        .map_init(init, |state, site| {
            let position = spade::mitigate_underflow(spade::Point2::new(site.x, site.y));
            match spade::validate_vertex(&position) {
                Ok(()) => answer(state, position),
                Err(InsertionError::TooSmall | InsertionError::TooLarge | InsertionError::NAN) => None,
            }
        })
        .collect::<Vec<_>>();
    answers
        .iter()
        .positions(|outcome| outcome.as_ref().is_some_and(|value| !finite(value)))
        .map(|point| Err(AnalysisError::Point { breakdown: Breakdown::Interpolant, point }))
        .collect::<Validated<(), _>>()
        .ok()?;
    Ok(answers)
}

pub fn interpolate(triangulation: &Delaunay, sites: Sites<'_>, interpolant: Interpolant) -> Result<Vec<Option<f64>>, NEVec<AnalysisError>> {
    let value = |vertex: VertexHandle<'_, Observation>| vertex.data().value;
    let coincident = |position: spade::Point2<f64>| {
        triangulation.nearest_neighbor(position).filter(|vertex| {
            vertex.out_edges().map(|edge| edge.length_2()).max_by(f64::total_cmp).is_some_and(|longest| vertex.position().distance_2(position) <= f64::EPSILON * f64::EPSILON * longest)
        })
    };
    let slopes = LazyLock::new(|| triangulation.natural_neighbor().estimate_gradients(value));
    query(
        sites,
        || triangulation.natural_neighbor(),
        |neighbor, position| match interpolant {
            Interpolant::NearestNeighbor => triangulation.nearest_neighbor(position).map(value),
            Interpolant::NaturalNeighbor => coincident(position).map(value).or_else(|| neighbor.interpolate(value, position)),
            Interpolant::Gradient(flatness) => coincident(position).map(value).or_else(|| neighbor.interpolate_gradient(value, &*slopes, flatness.into_inner(), position)),
        },
        |value: &f64| value.is_finite(),
    )
}

pub fn barycentric(triangulation: &Constrained, sites: Sites<'_>) -> Result<Vec<Option<f64>>, NEVec<AnalysisError>> {
    query(sites, || triangulation.barycentric(), |barycentric, position| barycentric.interpolate(|vertex| vertex.data().value, position), |value: &f64| value.is_finite())
}

pub fn gradients(triangulation: &Delaunay, sites: Sites<'_>) -> Result<Vec<Option<[f64; 2]>>, NEVec<AnalysisError>> {
    let slope = triangulation.natural_neighbor().estimate_gradients(|vertex: VertexHandle<'_, Observation>| vertex.data().value);
    query(
        sites,
        || (triangulation.natural_neighbor(), Vec::new()),
        |(neighbor, weights), position| {
            neighbor.get_weights(position, weights);
            (!weights.is_empty()).then(|| {
                weights.iter().fold([0.0_f64; 2], |[east, north], &(vertex, weight)| {
                    let [along_east, along_north] = slope(triangulation.vertex(vertex));
                    [along_east.mul_add(weight, east), along_north.mul_add(weight, north)]
                })
            })
        },
        |slope: &[f64; 2]| slope.iter().all(|along| along.is_finite()),
    )
}

// --- [KRIGING]
fn correlation(kernel: Kernel, lengths: &DVector<f64>, first: DVectorView<'_, f64>, second: DVectorView<'_, f64>) -> f64 {
    let distance = izip!(first.iter(), second.iter(), lengths.iter()).map(|(near, far, length)| ((near - far) / length).powi(2)).sum::<f64>().sqrt();
    match kernel {
        Kernel::Exponential => (-distance).exp(),
        Kernel::Matern32 => {
            let scaled = 3.0_f64.sqrt() * distance;
            (1.0 + scaled) * (-scaled).exp()
        }
        Kernel::Matern52 => {
            let scaled = 5.0_f64.sqrt() * distance;
            (1.0 + scaled + scaled * scaled / 3.0) * (-scaled).exp()
        }
        Kernel::Gaussian => (-0.5 * distance * distance).exp(),
    }
}

fn solved(factor: &Llt<f64>, right: &DMatrix<f64>) -> DMatrix<f64> {
    let solution = factor.solve(MatRef::from_column_major_slice(right.as_slice(), right.nrows(), right.ncols()));
    DMatrix::from_fn(right.nrows(), right.ncols(), |row, column| solution[(row, column)])
}

pub fn krige(observations: &Observations, kernel: Kernel, lengths: &[Tolerance], variance: Tolerance, noise: Option<Tolerance>, drift: DMatrix<u8>) -> Result<Kriging, NEVec<AnalysisError>> {
    let Observations { sites, values, weights: observation_weights } = observations;
    let (dimensions, points) = sites.shape();
    let terms = drift.ncols();
    let log_determinant = |factor: &Llt<f64>| factor.L().diagonal().column_vector().iter().map(|entry| 2.0 * entry.ln()).sum::<f64>();
    [
        (lengths.len() == dimensions).ok_or(AnalysisError::Shape { input: Input::Lengths, expected: dimensions, found: lengths.len() }),
        (drift.nrows() == dimensions).ok_or(AnalysisError::Shape { input: Input::Exponents, expected: dimensions, found: drift.nrows() }),
        determined(points, terms),
    ]
    .into_iter()
    .collect::<Validated<(), _>>()
    .ok()?;
    let lengths = DVector::from_iterator(dimensions, lengths.iter().map(|length| length.into_inner()));
    let variance = variance.into_inner();
    let nugget = noise.map_or_default(Tolerance::into_inner);
    let covariance = DMatrix::from_fn(points, points, |row, column| variance * correlation(kernel, &lengths, sites.column(row), sites.column(column)))
        + DMatrix::from_diagonal(&observation_weights.map(|weight| nugget / weight));
    let factor = MatRef::from_column_major_slice(covariance.as_slice(), points, points)
        .llt(Side::Lower)
        .map_err(|LltError::NonPositivePivot { index }| NEVec::new(AnalysisError::Point { breakdown: Breakdown::Kernel, point: index }))?;
    let basis = design(&drift, sites);
    let whitened = solved(&factor, &basis);
    let gram = basis.tr_mul(&whitened);
    let trend = MatRef::from_column_major_slice(gram.as_slice(), terms, terms).llt(Side::Lower).map_err(|LltError::NonPositivePivot { index }| NEVec::new(AnalysisError::Drift { term: index }))?;
    let targets = values.transpose();
    let coefficients = solved(&trend, &whitened.tr_mul(&targets));
    let lack = &targets - &basis * &coefficients;
    let weights = solved(&factor, &lack);
    let determinants = log_determinant(&factor) + log_determinant(&trend);
    let likelihood = -0.5 * convert::<usize, f64>(targets.ncols()).mul_add(convert::<usize, f64>(points.strict_sub(terms)).mul_add(TAU.ln(), determinants), lack.dot(&weights));
    Ok(Kriging { sites: sites.clone(), lengths, variance, kernel, nugget, basis, drift: Polynomial { exponents: drift, coefficients }, weights, factor, trend, likelihood })
}

pub fn predict(kriging: &Kriging, sites: DMatrixView<'_, f64>) -> Result<Vec<Prediction<f64>>, NEVec<AnalysisError>> {
    let dimensions = kriging.sites.nrows();
    iter::once((sites.nrows() == dimensions).ok_or(AnalysisError::Shape { input: Input::Sites, expected: dimensions, found: sites.nrows() }))
        .chain(refused_sites(&sites, |coordinate: &f64| coordinate.is_finite()))
        .collect::<Validated<(), _>>()
        .ok()?;
    let cross =
        DMatrix::from_fn(kriging.sites.ncols(), sites.ncols(), |known, query| kriging.variance * correlation(kriging.kernel, &kriging.lengths, kriging.sites.column(known), sites.column(query)));
    let reduced = solved(&kriging.factor, &cross);
    let terms = design(&kriging.drift.exponents, &sites.clone_owned()).transpose();
    let gap = &terms - kriging.basis.tr_mul(&reduced);
    let correction = solved(&kriging.trend, &gap);
    let means = kriging.drift.coefficients.tr_mul(&terms) + kriging.weights.tr_mul(&cross);
    let variance = DVector::from_iterator(
        sites.ncols(),
        izip!(cross.column_iter(), reduced.column_iter(), gap.column_iter(), correction.column_iter())
            .map(|(cross, reduced, gap, correction)| (kriging.variance - cross.dot(&reduced) + gap.dot(&correction)).max(0.0)),
    );
    let noise = DVector::repeat(sites.ncols(), kriging.nugget);
    Ok(means.row_iter().map(|mean| Prediction { mean: mean.transpose(), variance: variance.clone(), noise: noise.clone(), freedom: f64::INFINITY }).collect())
}
