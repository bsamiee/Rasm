use core::{cell::Cell, f64::consts::FRAC_1_SQRT_2, iter};

use itertools::Itertools as _;
use kurbo::{BezPath, Line, ParamCurve as _, ParamCurveNearest as _, PathSeg, Point, Shape as _};
use linesweeper::FillRule;
use nalgebra::{Point3, Unit, UnitVector3};
use nonempty_collections::NEVec;
use rasm_linework::{Region, Tolerance, Visibility};
use rasm_mesh::Source;
use rasm_proximity::{
    Distance, DistanceError,
    rays::{self, Hit},
};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use spade::{CdtEdge, ConstrainedDelaunayTriangulation, HierarchyHintGenerator, Intersection, LineIntersectionIterator, Point2, Triangulation as _, handles::DirectedEdgeHandle, mitigate_underflow};
use validated::Validated;

use crate::{
    Frame, Operation, Prepared, Scene, View, ViewsError, ray,
    visibility::{Lines, Piece, window},
};

// --- [TYPES] ---------------------------------------------------------------------------

type Boundary<'cdt> = DirectedEdgeHandle<'cdt, Point2<f64>, Cell<bool>, CdtEdge<()>, Option<Sample>>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Face {
    pub body: usize,
    pub source: Source,
    pub normal: UnitVector3<f64>,
    pub region: Region,
}

#[derive(Debug, Clone, Copy)]
struct Sample {
    key: (usize, Source),
    point: Point3<f64>,
    normal: UnitVector3<f64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn faces(view: &View) -> Result<Vec<Face>, NEVec<ViewsError>> { visible(&view.frame, &view.scene, &view.lines, view.tolerance).map(|faces| faces.into_iter().map(|(face, _)| face).collect()) }

pub(crate) fn visible(frame: &Frame, scene: &Scene, lines: &Lines, tolerance: Tolerance) -> Result<Vec<(Face, Point3<f64>)>, NEVec<ViewsError>> {
    fn bounding(edge: Boundary<'_>) -> bool {
        let owner = |side: Boundary<'_>| side.face().data().map(|Sample { key, point: _, normal: _ }| key);
        owner(edge) != owner(edge.rev())
    }
    fn after(edge: Boundary<'_>) -> Option<Boundary<'_>> { iter::successors(Some(edge.next()), |turn| Some(turn.rev().next())).find(|&turn| bounding(turn)).filter(|next| !next.data().replace(true)) }
    let reach = tolerance.into_inner();
    let corners = window(frame);
    let pitch = reach * FRAC_1_SQRT_2;
    let snapped = |point: Point| mitigate_underflow(Point2::new((point.x / pitch).round() * pitch, (point.y / pitch).round() * pitch));
    let mut triangulation = ConstrainedDelaunayTriangulation::<Point2<f64>, Cell<bool>, (), Option<Sample>, HierarchyHintGenerator<f64>>::new();
    let handles = iter::chain(&lines.vertices, &corners).map(|&point| triangulation.insert(snapped(point))).collect::<Result<Vec<_>, _>>().map_err(|source| NEVec::new(ViewsError::from(source)))?;
    let first = handles.len().strict_sub(corners.len());
    let mut pending = lines
        .pieces
        .iter()
        .filter(|piece| piece.visibility == Visibility::Visible)
        .map(|&Piece { ends, depths: _, kind: _, visibility: _ }| ends)
        .chain((first..first.strict_add(corners.len())).circular_tuple_windows::<(usize, usize)>().map(<[usize; 2]>::from))
        .map(|[from, to]| [handles[from], handles[to]])
        .filter(|[from, to]| from != to)
        .collect::<Vec<_>>();
    while let Some([from, to]) = pending.pop() {
        if triangulation.try_add_constraint(from, to).is_empty()
            && let Some((crossing, hint)) = LineIntersectionIterator::new_from_handles(&triangulation, from, to).find_map(|intersection| match intersection {
                Intersection::EdgeIntersection(edge) if edge.is_constraint_edge() => {
                    let [sight, wall] = [[from, to].map(|vertex| triangulation.vertex(vertex).position()), edge.positions()].map(|[head, tail]| Line::new((head.x, head.y), (tail.x, tail.y)));
                    let length = sight.length();
                    iter::chain(sight.crossing_point(wall).map(|crossing| wall.eval(wall.nearest(crossing, 0.0).t)), [wall.p0, wall.p1])
                        .map(snapped)
                        .find(|&Point2 { x, y }| {
                            let node = Point::new(x, y);
                            sight.nearest(node, 0.0).distance_sq <= reach.powi(2) && node.distance(sight.p0) < length && node.distance(sight.p1) < length
                        })
                        .map(|node| (node, edge.from().fix()))
                }
                Intersection::EdgeIntersection(_) | Intersection::VertexIntersection(_) | Intersection::EdgeOverlap(_) => None,
            })
        {
            let node = triangulation.insert_with_hint(crossing, hint).map_err(|source| NEVec::new(ViewsError::from(source)))?;
            pending.extend([[from, node], [node, to]].into_iter().filter(|_| from != node && node != to));
        }
    }
    let normalized = frame.picture.inverse();
    let (cells, casts): (Vec<_>, Vec<_>) = triangulation
        .inner_faces()
        .filter_map(|face| {
            let Point2 { x, y } = face.center();
            let (near, segment) = ray(frame, normalized * Point::new(x, y));
            match Unit::try_new_and_get(segment, 0.0).map(|(direction, length)| (direction, Distance::try_new(length + reach))) {
                Some((direction, Ok(distance))) => Some((face.fix(), (near - direction.scale(reach), direction, ..distance))),
                None | Some((_, Err(DistanceError::FiniteViolated | DistanceError::GreaterOrEqualViolated))) => None,
            }
        })
        .unzip();
    let crossed = rays::crossings(&scene.assembly, casts).map_err(|source| NEVec::new(ViewsError::from(source)))?;
    let owner = |hit: &Hit| {
        let Prepared { body, mesh: _, sources, surface: _ } = &scene.bodies[hit.element];
        sources[hit.subshape as usize].map(|source| (*body, source))
    };
    let front = |hits: &[Hit]| {
        let nearest = hits.first()?.time_of_impact;
        let hit = hits.iter().take_while(|hit| hit.time_of_impact <= nearest + reach).min_by_key(|hit| owner(hit))?;
        owner(hit).zip(Unit::try_new(hit.normal, 0.0)).map(|(key, normal)| Sample { key, point: hit.point, normal })
    };
    for (cell, hits) in iter::zip(cells, crossed) {
        *triangulation.face_data_mut(cell) = front(&hits);
    }
    triangulation
        .directed_edges()
        .filter(|&seed| bounding(seed))
        .filter_map(|seed| seed.face().data().filter(|_| !seed.data().replace(true)).map(|sample| (seed, sample)))
        .map(|(seed, Sample { key, point, normal })| {
            let ring = iter::successors(Some(seed), |&edge| after(edge))
                .map(|edge| {
                    let Point2 { x, y } = edge.from().position();
                    Point::new(x, y)
                })
                .collect::<Vec<_>>();
            (key, ((point, normal), BezPath::from_path_segments(ring.into_iter().circular_tuple_windows().map(|(from, to)| PathSeg::Line(Line::new(from, to))))))
        })
        .filter(|(_, (_, outline))| 2.0 * outline.area().abs() > reach * outline.perimeter(reach))
        .into_grouping_map()
        .reduce(|(sample, mut outline), _, (_, more)| {
            outline.extend(more);
            (sample, outline)
        })
        .into_iter()
        .sorted_unstable_by_key(|&(key, _)| key)
        .collect::<Vec<_>>()
        .into_par_iter()
        .filter_map(|((body, source), ((point, normal), outline))| {
            Region::new(outline, FillRule::NonZero, tolerance)
                .map(|region| (!region.is_empty()).then_some((Face { body, source, normal, region }, point)))
                .map_err(|refusal| ViewsError::Linework { operation: Operation::Region(body), source: refusal })
                .transpose()
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}
