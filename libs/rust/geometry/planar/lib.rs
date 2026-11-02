#![feature(adt_const_params)]

use core::{borrow::Borrow, convert, iter, marker::ConstParamTy};

use cavalier_contours::{
    polyline::{self, BooleanOp, BooleanResult, PlineBooleanOptions, PlineCreation as _, PlineOrientation, PlineSelfIntersectOptions, PlineSource as _, PlineSourceMut as _, PlineVertex},
    shape_algorithms::{self, IndexedPolyline},
};
use geo_types::{LineString, MultiPolygon, Polygon};
use itertools::{Either, Itertools as _};
use nalgebra::Point2;
use nonempty_collections::NEVec;
use spade::InsertionError;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, ConstParamTy)]
pub enum Closure {
    Open,
    Closed,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Display, Clone, Copy, PartialEq, PartialOrd))]
pub struct Tolerance(f64);

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Polyline<const CLOSURE: Closure>(#[as_ref] IndexedPolyline<f64>);

impl<const CLOSURE: Closure> Polyline<CLOSURE> {
    pub fn new<I>(vertices: I, tolerance: Tolerance) -> Result<Self, PlanarError>
    where I: IntoIterator<Item = (Point2<f64>, f64)> {
        indexed::<CLOSURE>(vertices.into_iter().map(|(position, bulge)| PlineVertex::new(position.x, position.y, bulge)), tolerance).map(Self).map_err(|defect| PlanarError::Invalid { defect })
    }

    pub fn from_line_string(line: &LineString<f64>, tolerance: Tolerance) -> Result<Self, PlanarError> { Self::new(line.coords().map(|coord| (Point2::new(coord.x, coord.y), 0.0)), tolerance) }

    pub fn points(&self, tolerance: Tolerance) -> Result<Vec<Point2<f64>>, PlanarError> { flattened(&self.0.polyline, tolerance) }

    pub fn to_line_string(&self, tolerance: Tolerance) -> Result<LineString<f64>, PlanarError> {
        self.points(tolerance).map(|points| {
            let closing = match CLOSURE {
                Closure::Open => None,
                Closure::Closed => points.first().copied(),
            };
            iter::chain(points, closing).map(<[f64; 2]>::from).collect()
        })
    }
}

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Region(#[as_ref] shape_algorithms::Shape<f64>);

impl Region {
    pub fn new<I, H>(polygons: I, tolerance: Tolerance) -> Result<Self, NEVec<PlanarError>>
    where
        I: IntoIterator<Item = (Polyline<{ Closure::Closed }>, H)>,
        H: IntoIterator<Item = Polyline<{ Closure::Closed }>>,
    {
        nested(polygons.into_iter().enumerate().map(|(polygon, (Polyline(exterior), holes))| (polygon, exterior, holes.into_iter().map(|Polyline(hole)| hole))), tolerance)
    }

    pub fn from_polygons<I>(polygons: I, tolerance: Tolerance) -> Result<Self, NEVec<PlanarError>>
    where I: IntoIterator<Item: Borrow<Polygon<f64>>> {
        polygons
            .into_iter()
            .enumerate()
            .filter(|(_, polygon)| !(polygon.borrow().exterior().0.is_empty() && polygon.borrow().interiors().is_empty()))
            .map(|(polygon, rings)| {
                let rings = rings.borrow();
                let ring = |ring: usize, line: &LineString<f64>| {
                    Validated::from(indexed::<{ Closure::Closed }>(line.coords().map(|coord| PlineVertex::new(coord.x, coord.y, 0.0)), tolerance).map_err(|defect| PlanarError::Ring {
                        polygon,
                        ring,
                        defect,
                    }))
                };
                ring(0, rings.exterior()).map2(iter::zip(rings.interiors(), 1..).map(|(line, index)| ring(index, line)).collect::<Validated<Vec<_>, _>>(), |exterior, holes| (polygon, exterior, holes))
            })
            .collect::<Validated<Vec<_>, _>>()
            .ok()
            .and_then(|polygons| nested(polygons, tolerance))
    }

    pub fn to_multi_polygon(&self, tolerance: Tolerance) -> Result<MultiPolygon<f64>, PlanarError> {
        let shape_algorithms::Shape { ccw_plines: bodies, cw_plines: holes, plines_index: _ } = &self.0;
        let ring = |outline: &IndexedPolyline<f64>| flattened(&outline.polyline, tolerance).map(|points| points.into_iter().map(<[f64; 2]>::from).collect::<LineString<f64>>());
        let owned = holes
            .iter()
            .filter_map(|hole| {
                let inside = hole.polyline.iter_segments().next().map(|(start, end)| polyline::seg_midpoint(start, end))?;
                bodies
                    .iter()
                    .enumerate()
                    .filter(|(_, outline)| outline.polyline.winding_number(inside) != 0)
                    .map(|(body, outline)| (body, outline.polyline.area()))
                    .min_by(|(_, left), (_, right)| left.total_cmp(right))
                    .map(|(body, _)| (body, hole))
            })
            .into_group_map();
        bodies
            .iter()
            .enumerate()
            .map(|(body, outline)| {
                ring(outline).and_then(|exterior| owned.get(&body).into_iter().flatten().copied().map(ring).collect::<Result<Vec<_>, _>>().map(|interiors| Polygon::new(exterior, interiors)))
            })
            .collect::<Result<Vec<_>, _>>()
            .map(MultiPolygon::new)
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, thiserror::Error)]
pub enum Defect {
    #[error("non-finite")]
    NonFinite,
    #[error("collapsed")]
    Collapsed,
    #[error("self-intersecting")]
    SelfIntersecting,
    #[error("outside its exterior")]
    Unenclosed,
}

#[derive(Debug, thiserror::Error)]
pub enum PlanarError {
    #[error("polyline is {defect}")]
    Invalid { defect: Defect },
    #[error("ring {ring} of polygon {polygon} is {defect}")]
    Ring { polygon: usize, ring: usize, defect: Defect },
    #[error("arcs cannot be flattened within {tolerance}")]
    Flattening { tolerance: Tolerance },
    #[error("{} constraint segments cross another, first from {} to {}", .segments.len(), .segments.first()[0], .segments.first()[1])]
    Crossing { segments: NEVec<[Point2<f64>; 2]> },
    #[error(transparent)]
    Insertion(#[from] InsertionError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn indexed<const CLOSURE: Closure>(vertices: impl Iterator<Item = PlineVertex<f64>>, tolerance: Tolerance) -> Result<IndexedPolyline<f64>, Defect> {
    let polyline = polyline::Polyline::from_iter(vertices, CLOSURE == Closure::Closed);
    polyline.iter_vertexes().flat_map(|vertex| [vertex.x, vertex.y, vertex.bulge]).all(f64::is_finite).ok_or(Defect::NonFinite)?;
    let distinct = polyline.remove_repeat_pos(tolerance.into_inner()).unwrap_or(polyline);
    (distinct.vertex_count() > 1).ok_or(Defect::Collapsed)?;
    let outline = IndexedPolyline::new(distinct);
    match CLOSURE {
        Closure::Closed if !spans(&outline.polyline, tolerance.into_inner()) => Err(Defect::Collapsed),
        Closure::Closed
            if outline.polyline.scan_for_self_intersect_opt(&PlineSelfIntersectOptions {
                aabb_index: Some(&outline.spatial_index),
                pos_equal_eps: tolerance.into_inner(),
                ..PlineSelfIntersectOptions::default()
            }) =>
            Err(Defect::SelfIntersecting),
        Closure::Open | Closure::Closed => Ok(outline),
    }
}

fn flattened(polyline: &polyline::Polyline<f64>, tolerance: Tolerance) -> Result<Vec<Point2<f64>>, PlanarError> {
    polyline.arcs_to_approx_lines(tolerance.into_inner()).map(|lines| lines.iter_vertexes().map(|vertex| Point2::new(vertex.x, vertex.y)).collect()).ok_or(PlanarError::Flattening { tolerance })
}

fn spans(outline: &polyline::Polyline<f64>, pos_equal_eps: f64) -> bool { 2.0 * outline.area().abs() > pos_equal_eps * outline.path_length() }

fn nested<P, H>(polygons: P, tolerance: Tolerance) -> Result<Region, NEVec<PlanarError>>
where
    P: IntoIterator<Item = (usize, IndexedPolyline<f64>, H)>,
    H: IntoIterator<Item = IndexedPolyline<f64>>,
{
    polygons
        .into_iter()
        .flat_map(|(polygon, exterior, holes)| {
            let interiors = iter::zip(holes, 1..)
                .map(|(hole, ring)| {
                    let BooleanResult { pos_plines: outside, neg_plines: _, result_info: _ } = hole.polyline.boolean_opt(
                        &exterior.polyline,
                        BooleanOp::Not,
                        &PlineBooleanOptions { pline1_aabb_index: Some(&hole.spatial_index), pos_equal_eps: tolerance.into_inner(), ..PlineBooleanOptions::default() },
                    );
                    outside.iter().all(|piece| !spans(&piece.pline, tolerance.into_inner())).then_some(Either::Right(hole.polyline)).ok_or(PlanarError::Ring {
                        polygon,
                        ring,
                        defect: Defect::Unenclosed,
                    })
                })
                .collect::<Vec<_>>();
            iter::once(Ok(Either::Left(exterior.polyline))).chain(interiors)
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map(|rings| {
            let (bodies, holes): (Vec<_>, Vec<_>) = rings.into_iter().partition_map(convert::identity);
            oriented(bodies, holes)
        })
}

fn oriented<B, H>(bodies: B, holes: H) -> Region
where
    B: IntoIterator<Item = polyline::Polyline<f64>>,
    H: IntoIterator<Item = polyline::Polyline<f64>>,
{
    let turned = |orientation: PlineOrientation| {
        move |mut outline: polyline::Polyline<f64>| {
            if outline.orientation() != orientation {
                outline.invert_direction_mut();
            }
            outline
        }
    };
    Region(shape_algorithms::Shape::from_plines(bodies.into_iter().map(turned(PlineOrientation::CounterClockwise)).chain(holes.into_iter().map(turned(PlineOrientation::Clockwise)))))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod booleans;
pub mod cells;
pub mod offsets;
pub mod paths;
pub mod triangulation;
