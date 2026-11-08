#![feature(adt_const_params, cmp_minmax)]

use core::{borrow::Borrow, cell::LazyCell, convert, fmt, iter, marker::ConstParamTy};

use cavalier_contours::{
    core::math::Vector2,
    polyline::{
        self, BooleanOp, BooleanResult, PlineBasicIntersect, PlineBooleanOptions, PlineContainsOptions, PlineContainsResult, PlineCreation as _, PlineIntersect, PlineOrientation,
        PlineOverlappingIntersect, PlineSelfIntersectOptions, PlineSource as _, PlineSourceMut as _, PlineVertex, SelfIntersectsInclude,
    },
    shape_algorithms::{IndexedPolyline, Shape},
    static_aabb2d_index::Control,
};
use geo_types::{LineString, MultiPolygon, Polygon};
use itertools::{Either, Itertools as _};
use nalgebra::{Point2, Reflection2, Similarity2};
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use spade::InsertionError;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, ConstParamTy)]
pub enum Closure {
    Open,
    Closed,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Display, Clone, Copy, PartialEq, PartialOrd))]
pub struct Tolerance(f64);

#[nutype::nutype(validate(finite), derive(Debug, Display, Clone, Copy, PartialEq, PartialOrd))]
pub struct Distance(f64);

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Polyline<const CLOSURE: Closure> {
    #[as_ref]
    outline: IndexedPolyline<f64>,
    #[as_ref]
    lengths: NEVec<f64>,
}

impl<const CLOSURE: Closure> Polyline<CLOSURE> {
    pub fn new<I>(vertices: I, tolerance: Tolerance) -> Result<Self, PlanarError>
    where I: IntoIterator<Item = (Point2<f64>, f64)> {
        indexed(vertices.into_iter().map(|(position, bulge)| PlineVertex::new(position.x, position.y, bulge)), tolerance)
    }

    pub fn from_line_string(line: &LineString<f64>, tolerance: Tolerance) -> Result<Self, PlanarError> { indexed(straight(line), tolerance) }

    pub fn points(&self, tolerance: Tolerance) -> Result<Vec<Point2<f64>>, PlanarError> { flattened(&self.outline.polyline, tolerance).map(|points| points.map(Point2::from).collect()) }

    pub fn to_line_string(&self, tolerance: Tolerance) -> Result<LineString<f64>, PlanarError> {
        flattened(&self.outline.polyline, tolerance).map(|points| {
            let mut line = points.collect::<LineString<_>>();
            if CLOSURE == Closure::Closed {
                line.close();
            }
            line
        })
    }

    pub fn transformed(&self, placement: &Similarity2<f64>, mirror: Option<&Reflection2<f64>>, tolerance: Tolerance) -> Result<Self, PlanarError> {
        indexed(placed(&self.outline.polyline, placement, mirror), tolerance)
    }
}

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Region(#[as_ref] Shape<f64>);

impl Region {
    pub fn new<I, H>(polygons: I, tolerance: Tolerance) -> Result<Self, PlanarError>
    where
        I: IntoIterator<Item = (Polyline<{ Closure::Closed }>, H)>,
        H: IntoIterator<Item = Polyline<{ Closure::Closed }>>,
    {
        nested(polygons.into_iter().map(|(exterior, holes)| iter::once(exterior).chain(holes).map(|ring| Ok(ring.outline))).enumerate(), tolerance)
    }

    pub fn from_polygons<I>(polygons: I, tolerance: Tolerance) -> Result<Self, PlanarError>
    where I: IntoIterator<Item: Borrow<Polygon<f64>>> {
        nested(
            polygons.into_iter().enumerate().filter(|(_, polygon)| !(polygon.borrow().exterior().0.is_empty() && polygon.borrow().interiors().is_empty())).map(|(index, polygon)| {
                let rings = polygon.borrow();
                (index, iter::once(rings.exterior()).chain(rings.interiors()).map(|ring| checked::<{ Closure::Closed }>(straight(ring), tolerance)).collect_vec())
            }),
            tolerance,
        )
    }

    #[must_use]
    pub fn polygons(&self) -> Vec<(Polyline<{ Closure::Closed }>, Vec<Polyline<{ Closure::Closed }>>)> {
        rings(&self.0).map(|(body, holes)| (measured(body.clone()), holes.into_iter().map(|hole| measured(hole.clone())).collect())).collect()
    }

    pub fn to_multi_polygon(&self, tolerance: Tolerance) -> Result<MultiPolygon<f64>, PlanarError> {
        let line = |outline: &IndexedPolyline<f64>| flattened(&outline.polyline, tolerance).map(Iterator::collect::<LineString<_>>);
        rings(&self.0)
            .map(|(body, holes)| line(body).and_then(|exterior| holes.into_iter().map(line).collect::<Result<Vec<_>, _>>().map(|interiors| Polygon::new(exterior, interiors))))
            .collect::<Result<Vec<_>, _>>()
            .map(MultiPolygon::new)
    }

    pub fn transformed(&self, placement: &Similarity2<f64>, mirror: Option<&Reflection2<f64>>, tolerance: Tolerance) -> Result<Self, PlanarError> {
        let Shape { ccw_plines: bodies, cw_plines: holes, plines_index: _ } = &self.0;
        resulting(bodies.iter().map(|body| placed(&body.polyline, placement, mirror)), holes.iter().map(|hole| placed(&hole.polyline, placement, mirror)), tolerance)
    }
}

impl From<Polyline<{ Closure::Closed }>> for Region {
    fn from(value: Polyline<{ Closure::Closed }>) -> Self { Self(oriented([value.outline], iter::empty())) }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, thiserror::Error)]
pub enum Defect {
    #[error("non-finite at vertex {vertex}")]
    NonFinite { vertex: usize },
    #[error("collapsed")]
    Collapsed,
    #[error("self-intersecting at {} points, first at {}", points.len(), points.first())]
    SelfIntersecting { points: NEVec<Point2<f64>> },
    #[error("outside its exterior")]
    Unenclosed,
    #[error("overlapping")]
    Overlapping,
}

#[derive(Debug, thiserror::Error)]
pub enum PlanarError {
    #[error("polyline is {defect}")]
    Invalid { defect: Defect },
    #[error("{}", rings.iter().map(|([polygon, ring], defect)| fmt::from_fn(move |formatter| write!(formatter, "ring {ring} of polygon {polygon} is {defect}"))).format(", "))]
    Region { rings: NEVec<([usize; 2], Defect)> },
    #[error("{}", loops.iter().map(|(index, defect)| fmt::from_fn(move |formatter| write!(formatter, "output loop {index} is {defect}"))).format(", "))]
    Output { loops: NEVec<(usize, Defect)> },
    #[error("arcs cannot be flattened within {tolerance}")]
    Flattening { tolerance: Tolerance },
    #[error("{} constraint segments cross another, first from {} to {}", .segments.len(), .segments.first()[0], .segments.first()[1])]
    Crossing { segments: NEVec<[Point2<f64>; 2]> },
    #[error("{}", sites.iter().map(|(site, source)| fmt::from_fn(move |formatter| write!(formatter, "site {site} is refused as {source}"))).format(", "))]
    Insertion { sites: NEVec<(usize, InsertionError)> },
    #[error("triangulation refused positions")]
    Extent { source: InsertionError },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn winding_number(region: &Region, point: Point2<f64>, tolerance: Tolerance) -> Option<i32> {
    let (at, reach) = (Vector2::new(point.x, point.y), tolerance.into_inner());
    let [min_x, min_y, max_x, max_y] = [point.x - reach, point.y - reach, point.x + reach, point.y + reach];
    region
        .0
        .plines_index
        .query_iter(min_x, min_y, max_x, max_y)
        .map(|index| boundary(&region.0, index))
        .map(|IndexedPolyline { polyline: outline, spatial_index }| {
            spatial_index
                .query_iter(min_x, min_y, max_x, max_y)
                .all(|index| {
                    let (start, end) = segment(outline, index);
                    polyline::seg_distance_is_greater_than(start, end, at, reach, reach)
                })
                .then(|| outline.winding_number(at))
        })
        .sum()
}

pub fn boolean(left: &Polyline<{ Closure::Closed }>, right: &Polyline<{ Closure::Closed }>, operation: BooleanOp, tolerance: Tolerance) -> Result<Region, PlanarError> {
    let IndexedPolyline { polyline: first, spatial_index } = &left.outline;
    let BooleanResult { pos_plines: bodies, neg_plines: holes, result_info: _ } =
        first.boolean_opt(&right.outline.polyline, operation, &PlineBooleanOptions { pline1_aabb_index: Some(spatial_index), pos_equal_eps: tolerance.into_inner(), collapsed_area_eps: None });
    resulting(bodies.iter().map(|body| body.pline.iter_vertexes()), holes.iter().map(|hole| hole.pline.iter_vertexes()), tolerance)
}

fn checked<const CLOSURE: Closure>(vertices: impl Iterator<Item = PlineVertex<f64>>, tolerance: Tolerance) -> Result<IndexedPolyline<f64>, Defect> {
    let source = polyline::Polyline::from_iter(vertices, CLOSURE == Closure::Closed);
    source.iter_vertexes().position(|vertex| ![vertex.x, vertex.y, vertex.bulge].into_iter().all(f64::is_finite)).map_or(Ok(()), |vertex| Err(Defect::NonFinite { vertex }))?;
    let distinct = source.remove_repeat_pos(tolerance.into_inner()).unwrap_or(source);
    (distinct.vertex_count() > 1).ok_or(Defect::Collapsed)?;
    let outline = IndexedPolyline::new(distinct);
    let crossings = || {
        let mut points = Vec::new();
        outline.polyline.visit_self_intersects_opt::<Control<()>, _>(
            &mut |intersect: PlineIntersect<f64>| {
                match intersect {
                    PlineIntersect::Basic(PlineBasicIntersect { start_index1: _, start_index2: _, point }) => points.push(point),
                    PlineIntersect::Overlapping(PlineOverlappingIntersect { start_index1: _, start_index2: _, point1, point2 }) => points.extend([point1, point2]),
                }
                Control::Continue
            },
            &PlineSelfIntersectOptions { aabb_index: Some(&outline.spatial_index), pos_equal_eps: tolerance.into_inner(), include: SelfIntersectsInclude::All },
        );
        points.into_iter().map(|point| Point2::new(point.x, point.y)).try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>)
    };
    match CLOSURE {
        Closure::Closed if collapsed(&outline.polyline, tolerance) => Err(Defect::Collapsed),
        Closure::Closed => crossings().map_or(Ok(()), |points| Err(Defect::SelfIntersecting { points })),
        Closure::Open => Ok(()),
    }?;
    Ok(outline)
}

fn measured<const CLOSURE: Closure>(outline: IndexedPolyline<f64>) -> Polyline<CLOSURE> {
    let first = polyline::seg_length(outline.polyline.at(0), outline.polyline.at(1));
    let rest = outline
        .polyline
        .iter_segments()
        .skip(1)
        .scan(first, |length, (start, end)| {
            *length += polyline::seg_length(start, end);
            Some(*length)
        })
        .collect_vec();
    Polyline { outline, lengths: NEVec::from((first, rest)) }
}

fn indexed<const CLOSURE: Closure>(vertices: impl Iterator<Item = PlineVertex<f64>>, tolerance: Tolerance) -> Result<Polyline<CLOSURE>, PlanarError> {
    checked::<CLOSURE>(vertices, tolerance).map(measured).map_err(|defect| PlanarError::Invalid { defect })
}

fn surviving<const CLOSURE: Closure>(vertices: impl Iterator<Item = PlineVertex<f64>>, tolerance: Tolerance) -> Result<Option<IndexedPolyline<f64>>, PlanarError> {
    match checked::<CLOSURE>(vertices, tolerance) {
        Ok(outline) => Ok(Some(outline)),
        Err(Defect::Collapsed) => Ok(None),
        Err(defect @ (Defect::NonFinite { vertex: _ } | Defect::SelfIntersecting { points: _ } | Defect::Unenclosed | Defect::Overlapping)) => Err(PlanarError::Invalid { defect }),
    }
}

fn separated(vertices: &[PlineVertex<f64>], tolerance: Tolerance) -> Result<Vec<IndexedPolyline<f64>>, Defect> {
    let reach = tolerance.into_inner();
    let pieces = |points: NEVec<Point2<f64>>| {
        let revisited = |vertex: &PlineVertex<f64>| points.iter().any(|point| Vector2::new(point.x, point.y).fuzzy_eq_eps(vertex.pos(), reach));
        let (rest, lobes) = vertices.iter().fold((Vec::new(), Vec::new()), |(mut path, mut lobes), vertex| {
            if revisited(vertex)
                && let Some(start) = path.iter().rposition(|earlier: &PlineVertex<f64>| earlier.pos().fuzzy_eq_eps(vertex.pos(), reach))
            {
                lobes.push(path.split_off(start));
            }
            path.push(*vertex);
            (path, lobes)
        });
        (!lobes.is_empty()).ok_or(Defect::SelfIntersecting { points })?;
        iter::chain(lobes, [rest])
            .filter_map(|piece| match checked::<{ Closure::Closed }>(piece.into_iter(), tolerance) {
                Ok(outline) => Some(Ok(outline)),
                Err(Defect::Collapsed) => None,
                Err(defect @ (Defect::NonFinite { vertex: _ } | Defect::SelfIntersecting { points: _ } | Defect::Unenclosed | Defect::Overlapping)) => Some(Err(defect)),
            })
            .collect()
    };
    match checked::<{ Closure::Closed }>(vertices.iter().copied(), tolerance) {
        Ok(outline) => Ok(vec![outline]),
        Err(Defect::Collapsed) => Ok(Vec::new()),
        Err(Defect::SelfIntersecting { points }) => pieces(points),
        Err(defect @ (Defect::NonFinite { vertex: _ } | Defect::Unenclosed | Defect::Overlapping)) => Err(defect),
    }
}

fn straight(line: &LineString<f64>) -> impl Iterator<Item = PlineVertex<f64>> { line.coords().map(|coord| PlineVertex::new(coord.x, coord.y, 0.0)) }

fn placed<'source>(outline: &'source polyline::Polyline<f64>, placement: &'source Similarity2<f64>, mirror: Option<&'source Reflection2<f64>>) -> impl Iterator<Item = PlineVertex<f64>> {
    outline.iter_vertexes().map(move |vertex| {
        let (position, bulge) = match mirror {
            Some(reflection) => {
                let mut coordinates = nalgebra::Vector2::new(vertex.x, vertex.y);
                reflection.reflect(&mut coordinates);
                (Point2::from(coordinates), -vertex.bulge)
            }
            None => (Point2::new(vertex.x, vertex.y), vertex.bulge),
        };
        let moved = placement.transform_point(&position);
        PlineVertex::new(moved.x, moved.y, bulge)
    })
}

fn flattened(polyline: &polyline::Polyline<f64>, tolerance: Tolerance) -> Result<impl ExactSizeIterator<Item = [f64; 2]> + use<>, PlanarError> {
    polyline.arcs_to_approx_lines(tolerance.into_inner()).map(|lines| lines.vertex_data.into_iter().map(|vertex| [vertex.x, vertex.y])).ok_or(PlanarError::Flattening { tolerance })
}

fn collapsed(outline: &polyline::Polyline<f64>, tolerance: Tolerance) -> bool { 2.0 * outline.area().abs() <= tolerance.into_inner() * outline.path_length() }

fn segment(path: &polyline::Polyline<f64>, index: usize) -> (PlineVertex<f64>, PlineVertex<f64>) { (path.at(index), path.at(path.next_wrapping_index(index))) }

fn boundary(shape: &Shape<f64>, index: usize) -> &IndexedPolyline<f64> { index.checked_sub(shape.ccw_plines.len()).map_or_else(|| &shape.ccw_plines[index], |hole| &shape.cw_plines[hole]) }

fn nested<I, R>(polygons: I, tolerance: Tolerance) -> Result<Region, PlanarError>
where
    I: IntoIterator<Item = (usize, R)>,
    R: IntoIterator<Item = Result<IndexedPolyline<f64>, Defect>>,
{
    let (kept, refused): (Vec<_>, Vec<Vec<_>>) = polygons
        .into_iter()
        .map(|(polygon, rings)| {
            let (valid, refused): (Vec<_>, Vec<_>) = rings.into_iter().enumerate().partition_map(|(ring, outline)| match outline {
                Ok(outline) => Either::Left((ring, outline)),
                Err(defect) => Either::Right(([polygon, ring], defect)),
            });
            let mut valid = valid.into_iter();
            (valid.next().and_then(|(ring, exterior)| (ring == 0).then(|| (polygon, exterior, valid.collect_vec()))), refused)
        })
        .unzip();
    enclosed(
        kept.into_iter().flatten().enumerate().flat_map(|(body, (polygon, exterior, holes))| {
            iter::once(Either::Left((([polygon, 0], None), exterior))).chain(holes.into_iter().map(move |(ring, hole)| Either::Right((([polygon, ring], Some(body)), hole))))
        }),
        refused.into_iter().flatten(),
        tolerance,
    )
    .map_err(|rings| PlanarError::Region { rings })
}

fn resulting<B, H, V>(bodies: B, holes: H, tolerance: Tolerance) -> Result<Region, PlanarError>
where
    B: IntoIterator<Item = V>,
    H: IntoIterator<Item = V>,
    V: IntoIterator<Item = PlineVertex<f64>>,
{
    let (kept, refused): (Vec<Vec<_>>, Vec<_>) = bodies
        .into_iter()
        .map(Either::Left)
        .chain(holes.into_iter().map(Either::Right))
        .enumerate()
        .map(|(index, role)| {
            let body = role.is_left();
            separated(&role.into_inner().into_iter().collect_vec(), tolerance)
                .map(|pieces| {
                    let whole = pieces.iter().map(|piece| piece.polyline.area()).sum::<f64>() < 0.0;
                    pieces
                        .into_iter()
                        .map(|piece| {
                            let along = (piece.polyline.area() < 0.0) == whole;
                            let subject = ((index, None), piece);
                            if along == body { Either::Left(subject) } else { Either::Right(subject) }
                        })
                        .collect()
                })
                .map_err(|defect| (index, defect))
        })
        .partition_result();
    enclosed(kept.into_iter().flatten(), refused, tolerance).map_err(|loops| PlanarError::Output { loops })
}

fn enclosed<S, L, R>(loops: L, refused: R, tolerance: Tolerance) -> Result<Region, NEVec<(S, Defect)>>
where
    S: Copy + Ord,
    L: IntoIterator<Item = Either<((S, Option<usize>), IndexedPolyline<f64>), ((S, Option<usize>), IndexedPolyline<f64>)>>,
    R: IntoIterator<Item = (S, Defect)>,
{
    let (bodies, holes): (Vec<_>, Vec<_>) = loops.into_iter().partition_map(convert::identity);
    let ((body_subjects, bodies), (hole_subjects, holes)): ((Vec<_>, Vec<_>), (Vec<_>, Vec<_>)) = (bodies.into_iter().unzip(), holes.into_iter().unzip());
    let shape = oriented(bodies, holes);
    let subjects = iter::chain(body_subjects, hole_subjects).collect_vec();
    let (pos_equal_eps, plines_index) = (tolerance.into_inner(), &shape.plines_index);
    let remains = |left: &IndexedPolyline<f64>, right: &IndexedPolyline<f64>, operation: BooleanOp| {
        let BooleanResult { pos_plines: kept, neg_plines: carved, result_info: _ } =
            left.polyline.boolean_opt(&right.polyline, operation, &PlineBooleanOptions { pline1_aabb_index: Some(&left.spatial_index), pos_equal_eps, collapsed_area_eps: None });
        iter::chain(&kept, &carved).any(|piece| !collapsed(&piece.pline, tolerance))
    };
    let nesting = |first: &IndexedPolyline<f64>, second: &IndexedPolyline<f64>| match first
        .polyline
        .contains_opt(&second.polyline, &PlineContainsOptions { pline1_aabb_index: Some(&first.spatial_index), pos_equal_eps })
    {
        PlineContainsResult::Disjoint => None,
        PlineContainsResult::Pline1InsidePline2 => Some([true, false]),
        PlineContainsResult::Pline2InsidePline1 => Some([false, true]),
        PlineContainsResult::Intersected | PlineContainsResult::InvalidInput =>
            remains(first, second, BooleanOp::And).then(|| [!remains(first, second, BooleanOp::Not), !remains(second, first, BooleanOp::Not)]),
    };
    let (contained, overlapping): (Vec<_>, Vec<_>) = iter::chain(&shape.ccw_plines, &shape.cw_plines)
        .enumerate()
        .flat_map(|(first, outline)| {
            outline.spatial_index.bounds().into_iter().flat_map(move |bounds| {
                plines_index.query_iter(bounds.min_x, bounds.min_y, bounds.max_x, bounds.max_y).filter(move |second| *second > first).map(move |second| (first, outline, second))
            })
        })
        .filter_map(|(first, outline, second)| nesting(outline, boundary(&shape, second)).map(|within| (first, second, within)))
        .partition_map(|(first, second, within)| match within {
            [true, false] => Either::Left((first, second)),
            [false, true] => Either::Left((second, first)),
            [true, true] | [false, false] => Either::Right([first, second]),
        });
    let containers = contained.into_iter().into_group_map();
    let area = |index: &usize| boundary(&shape, *index).polyline.area().abs();
    let solid = shape.ccw_plines.len();
    let enclosure = subjects.iter().enumerate().flat_map(|(index, (_, owner))| {
        let outer = containers.get(&index).map_or(&[][..], Vec::as_slice);
        let body = index < solid;
        let depth = outer.iter().map(|outer| if *outer < solid { 1 } else { -1 }).sum::<i32>();
        let owned = owner.is_none_or(|owner| outer.contains(&owner));
        match (!body && (!owned || outer.is_empty()), depth == i32::from(!body)) {
            (true, _) => [Some((index, Defect::Unenclosed)), None],
            (false, true) => [None, None],
            (false, false) => [Some((index, Defect::Overlapping)), outer.iter().min_by(|left, right| area(left).total_cmp(&area(right))).map(|outer| (*outer, Defect::Overlapping))],
        }
    });
    let defects =
        enclosure.flatten().chain(overlapping.into_iter().flatten().map(|index| (index, Defect::Overlapping))).unique_by(|(index, _)| *index).map(|(index, defect)| (subjects[index].0, defect));
    iter::chain(refused, defects).sorted_by_key(|(subject, _)| *subject).dedup().try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>).map_or(Ok(Region(shape)), Err)
}

fn oriented<B, H>(bodies: B, holes: H) -> Shape<f64>
where
    B: IntoIterator<Item = IndexedPolyline<f64>>,
    H: IntoIterator<Item = IndexedPolyline<f64>>,
{
    let turned = |role: PlineOrientation| {
        move |IndexedPolyline { polyline: mut outline, spatial_index: _ }: IndexedPolyline<f64>| {
            if outline.orientation() != role {
                outline.invert_direction_mut();
            }
            outline
        }
    };
    Shape::from_plines(bodies.into_iter().map(turned(PlineOrientation::CounterClockwise)).chain(holes.into_iter().map(turned(PlineOrientation::Clockwise))))
}

fn rings(shape: &Shape<f64>) -> impl Iterator<Item = (&IndexedPolyline<f64>, Vec<&IndexedPolyline<f64>>)> {
    let Shape { ccw_plines: bodies, cw_plines: holes, plines_index: _ } = shape;
    let mut owned = iter::zip(holes, owners(shape)).filter_map(|(hole, owner)| owner.map(|owner| (owner, hole))).into_group_map();
    bodies.iter().enumerate().map(move |(body, outline)| (outline, owned.remove(&body).unwrap_or_default()))
}

fn owners(shape: &Shape<f64>) -> Vec<Option<usize>> {
    let Shape { ccw_plines: bodies, cw_plines: holes, plines_index } = shape;
    let areas = LazyCell::new(|| bodies.iter().map(|body| body.polyline.area()).collect_vec());
    holes
        .iter()
        .map(|hole| {
            let (start, end) = segment(&hole.polyline, 0);
            let inside = polyline::seg_midpoint(start, end);
            plines_index
                .query_iter(inside.x, inside.y, inside.x, inside.y)
                .filter_map(|body| bodies.get(body).map(|outline| (body, outline, areas[body])))
                .filter(|(_, outline, _)| outline.polyline.winding_number(inside) != 0)
                .min_by(|(_, _, left), (_, _, right)| left.total_cmp(right))
                .map(|(body, _, _)| body)
        })
        .collect()
}

fn position(point: impl Into<[f64; 2]>) -> spade::Point2<f64> { spade::mitigate_underflow(point.into().into()) }

// --- [COMPOSITION] ---------------------------------------------------------------------

mod cells;
mod offsets;
mod paths;
mod triangulation;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use cells::voronoi;
pub use offsets::{parallel, region};
pub use paths::{Clearance, Station, clearance, closest, intersections, pieces, slice, station};
pub use triangulation::Triangulation;
