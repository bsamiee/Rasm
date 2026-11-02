use core::{cell::Cell, cmp, iter};
use std::collections::HashMap;

use itertools::Itertools as _;
use kurbo::{BezPath, Circle, PathEl, Point, Triangle};
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
use spade::{
    ConstrainedDelaunayTriangulation, Point2, Triangulation as _,
    handles::{FixedDirectedEdgeHandle, FixedFaceHandle, FixedVertexHandle, PossiblyOuterTag},
    mitigate_underflow,
};
use validated::Validated;

use crate::{
    Frame, Operation, Prepared, Scene, View, ViewsError, ray,
    visibility::{Lines, Piece, window},
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Face {
    pub body: usize,
    pub source: Source,
    pub normal: UnitVector3<f64>,
    pub region: Region,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn faces(view: &View) -> Result<Vec<Face>, NEVec<ViewsError>> { visible(&view.frame, &view.scene, &view.lines, view.tolerance).map(|faces| faces.into_iter().map(|(face, _)| face).collect()) }

pub(crate) fn visible(frame: &Frame, scene: &Scene, lines: &Lines, tolerance: Tolerance) -> Result<Vec<(Face, Point3<f64>)>, NEVec<ViewsError>> {
    let reach = tolerance.into_inner();
    let corners = window(frame);
    let first = lines.vertices.len();
    let normalized = frame.picture.inverse();
    let mut conflicts = Vec::new();
    let mut triangulation = ConstrainedDelaunayTriangulation::<Point2<f64>>::try_bulk_load_cdt(
        iter::chain(&lines.vertices, &corners).map(|point| mitigate_underflow(Point2::new(point.x, point.y))).collect(),
        lines
            .pieces
            .iter()
            .filter_map(|&Piece { ends, depths: _, kind: _, visibility }| (visibility == Visibility::Visible).then_some(ends))
            .chain((first..first.strict_add(corners.len())).circular_tuple_windows::<(usize, usize)>().map(<[usize; 2]>::from))
            .collect(),
        |conflict| conflicts.push(conflict),
    )
    .map_err(|source| NEVec::new(ViewsError::from(source)))?;
    for [from, to] in conflicts {
        triangulation.add_constraint_and_split(FixedVertexHandle::from_index(from), FixedVertexHandle::from_index(to), mitigate_underflow);
    }
    let (cells, casts): (Vec<_>, Vec<_>) = triangulation
        .inner_faces()
        .filter_map(|face| {
            let [head, middle, tail] = face.positions().map(|Point2 { x, y }| (x, y));
            let Circle { center, radius } = Triangle::new(head, middle, tail).inscribed_circle();
            let (near, segment) = ray(frame, normalized * center);
            match Unit::try_new_and_get(segment, 0.0).map(|(direction, length)| (direction, Distance::try_new(length + reach))) {
                Some((direction, Ok(distance))) => Some(((face.fix(), radius), (near - direction.scale(reach), direction, ..distance))),
                None | Some((_, Err(DistanceError::FiniteViolated | DistanceError::GreaterOrEqualViolated))) => None,
            }
        })
        .unzip();
    let hits = rays::hits(&scene.assembly, casts).map_err(|source| NEVec::new(ViewsError::from(source)))?;
    let labels = iter::zip(cells, hits)
        .filter_map(|((cell, radius), hit)| {
            let Hit { element, subshape, feature: _, point, normal, time_of_impact: _ } = hit?;
            let Prepared { body, mesh: _, sources } = scene.bodies.get(element)?;
            let source = sources.get(subshape as usize).copied().flatten()?;
            Unit::try_new(normal, 0.0).map(|normal| (cell, ((*body, source), (radius, point, normal))))
        })
        .collect::<HashMap<_, _>>();
    let label = |face: FixedFaceHandle<PossiblyOuterTag>| face.as_inner().and_then(|inner| labels.get(&inner)).map(|&(key, _)| key);
    let free = vec![Cell::new(true); triangulation.num_directed_edges()];
    let take = |edge: FixedDirectedEdgeHandle| free.get(edge.index()).is_some_and(|slot| slot.replace(false));
    let corner = |edge: FixedDirectedEdgeHandle| {
        let Point2 { x, y } = triangulation.directed_edge(edge).from().position();
        Point::new(x, y)
    };
    let after = |edge: FixedDirectedEdgeHandle, key: (usize, Source)| {
        iter::successors(Some(triangulation.directed_edge(edge).next()), |turn| Some(turn.rev().next()))
            .find(|turn| label(turn.rev().face().fix()) != Some(key))
            .map(|turn| turn.fix())
            .filter(|&next| take(next))
    };
    triangulation
        .inner_faces()
        .filter_map(|face| {
            labels.get(&face.fix()).map(|&(key, sample)| {
                let outline = face
                    .adjacent_edges()
                    .into_iter()
                    .filter(|edge| label(edge.rev().face().fix()) != Some(key) && take(edge.fix()))
                    .flat_map(|seed| {
                        iter::once(PathEl::MoveTo(corner(seed.fix())))
                            .chain(iter::successors(after(seed.fix(), key), |&edge| after(edge, key)).map(|edge| PathEl::LineTo(corner(edge))))
                            .chain(iter::once(PathEl::ClosePath))
                    })
                    .collect::<BezPath>();
                (key, (sample, outline))
            })
        })
        .into_grouping_map()
        .reduce(|(kept, mut outline), _, (sample, more)| {
            outline.extend(more);
            (cmp::max_by(kept, sample, |(left, ..), (right, ..)| left.total_cmp(right)), outline)
        })
        .into_iter()
        .sorted_unstable_by_key(|&((body, Source { original, face }), _)| (body, original, face))
        .collect::<Vec<_>>()
        .into_par_iter()
        .filter_map(|((body, source), ((_, point, normal), outline))| {
            Region::new(&outline, FillRule::NonZero, tolerance)
                .map(|region| (!region.is_empty()).then_some((Face { body, source, normal, region }, point)))
                .map_err(|refusal| ViewsError::Linework { operation: Operation::Region(body), source: refusal })
                .transpose()
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}
