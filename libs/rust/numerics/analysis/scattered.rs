use itertools::Itertools as _;
use nalgebra::{DMatrixView, Point2};
use nonempty_collections::NEVec;
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use spade::{DelaunayTriangulation, FloatTriangulation as _, HasPosition, HierarchyHintGenerator, HintGenerator, handles::VertexHandle};
use validated::Validated;

use crate::{AnalysisError, Observations};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Datum {
    pub position: Point2<f64>,
    pub value: f64,
}

impl HasPosition for Datum {
    type Scalar = f64;

    fn position(&self) -> spade::Point2<f64> { spade::mitigate_underflow(<[f64; 2]>::from(self.position).into()) }
}

#[nutype::nutype(validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Flatness(f64);

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Interpolant {
    NearestNeighbor,
    Barycentric,
    NaturalNeighbor,
    Gradient(Flatness),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn triangulate(observations: &Observations) -> Result<DelaunayTriangulation<Datum, (), (), (), HierarchyHintGenerator<f64>>, AnalysisError> {
    let sites = observations.sites();
    (sites.nrows() == 2).ok_or(AnalysisError::Shape { expected: 2, found: sites.nrows() })?;
    DelaunayTriangulation::bulk_load_stable(
        sites.fixed_rows::<2>(0).column_iter().zip(observations.values().iter()).map(|(site, &value)| Datum { position: Point2::new(site.x, site.y), value }).collect(),
    )
    .map_err(AnalysisError::from)
}

pub fn interpolate<L>(triangulation: &DelaunayTriangulation<Datum, (), (), (), L>, sites: DMatrixView<'_, f64>, interpolant: Interpolant) -> Result<Vec<Option<f64>>, NEVec<AnalysisError>>
where L: HintGenerator<f64> + Sync {
    fn value(vertex: VertexHandle<'_, Datum>) -> f64 { vertex.data().value }
    (sites.nrows() == 2).ok_or_else(|| NEVec::new(AnalysisError::Shape { expected: 2, found: sites.nrows() }))?;
    let sites = sites.fixed_rows::<2>(0);
    sites
        .column_iter()
        .enumerate()
        .flat_map(|(point, site)| [site.x, site.y].into_iter().positions(|coordinate| !coordinate.is_finite()).map(move |dimension| Err(AnalysisError::Site { dimension, point })))
        .collect::<Validated<(), _>>()
        .ok()?;
    let query = |point: usize| {
        let site = sites.column(point);
        spade::mitigate_underflow(spade::Point2::new(site.x, site.y))
    };
    let located = |point: usize| Some(query(point)).filter(|position| position.x.abs().max(position.y.abs()) <= spade::MAX_ALLOWED_VALUE);
    let coincident = |position: spade::Point2<f64>| {
        triangulation.nearest_neighbor(position).filter(|vertex| {
            vertex.out_edges().map(|edge| edge.length_2()).max_by(f64::total_cmp).is_some_and(|longest| vertex.position().distance_2(position) <= f64::EPSILON * f64::EPSILON * longest)
        })
    };
    let points = 0..sites.ncols();
    Ok(match interpolant {
        Interpolant::NearestNeighbor => points.into_par_iter().map(|point| located(point).and_then(|position| triangulation.nearest_neighbor(position)).map(value)).collect(),
        Interpolant::Barycentric =>
            points.into_par_iter().map_init(|| triangulation.barycentric(), |barycentric, point| located(point).and_then(|position| barycentric.interpolate(value, position))).collect(),
        Interpolant::NaturalNeighbor => points
            .into_par_iter()
            .map_init(
                || triangulation.natural_neighbor(),
                |neighbor, point| located(point).and_then(|position| coincident(position).map_or_else(|| neighbor.interpolate(value, position), |vertex| Some(value(vertex)))),
            )
            .collect(),
        Interpolant::Gradient(flatness) => {
            let gradient = triangulation.natural_neighbor().estimate_gradients(value);
            points
                .into_par_iter()
                .map_init(
                    || triangulation.natural_neighbor(),
                    |neighbor, point| {
                        located(point)
                            .and_then(|position| coincident(position).map_or_else(|| neighbor.interpolate_gradient(value, &gradient, flatness.into_inner(), position), |vertex| Some(value(vertex))))
                    },
                )
                .collect()
        }
    })
}
