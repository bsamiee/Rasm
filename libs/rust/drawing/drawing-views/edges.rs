use core::cmp::Reverse;
use core::iter;

use enum_map::EnumMap;
use itertools::Itertools as _;
use kurbo::{Line, PathEl, PathSeg, Point};
use nalgebra::{Point3, Vector3, Vector4, distance_squared};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use parry3d_f64::math::Vector;
use parry3d_f64::shape::TopoHalfEdge;
use rasm_drawing::{Class, Deflection, Edge, EdgeKind, Element, Linetype, Path, Pen, Tolerance, cleanup};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::visibility::{Lines, Piece};
use crate::{Feature, Frame, Operation, Prepared, View, ViewsError};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn edges(view: &View) -> Result<Vec<Edge>, NEVec<ViewsError>> {
    let Lines { vertices, pieces } = &view.lines;
    let key = |point: Point| [point.x.to_bits(), point.y.to_bits()];
    pieces
        .iter()
        .filter_map(|&Piece { ends: [start, end], depths: [head, tail], kind, visibility }| Some((Class { kind: kind?, visibility }, (Line::new(vertices[start], vertices[end]), head.min(tail)))))
        .fold(EnumMap::<Class, Vec<(Line, f64)>>::default(), |mut groups, (class, piece)| {
            groups[class].push(piece);
            groups
        })
        .into_iter()
        .collect::<Vec<_>>()
        .into_par_iter()
        .flat_map_iter(move |(class, group)| {
            let ends = group.iter().flat_map(|&(Line { p0, p1 }, least)| [(key(p1), (p0, least)), (key(p0), (p1, least))]).into_group_map();
            let depth = move |from: Point, to: Point| {
                ends.get(&key(to))?
                    .iter()
                    .min_by(|(left, left_least), (right, right_least)| left.distance_squared(from).total_cmp(&right.distance_squared(from)).then(left_least.total_cmp(right_least)))
                    .map(|&(_, least)| least)
            };
            cleanup::chain(group.into_iter().map(|(line, _)| PathSeg::Line(line)), view.tolerance).into_iter().filter_map(move |run| {
                let nearest = run.elements().iter().filter_map(PathEl::end_point).tuple_windows().filter_map(|(from, to)| depth(from, to)).min_by(f64::total_cmp)?;
                Some(Path::try_new(run).map(|path| Edge { path, class, depth: nearest }).map_err(|source| ViewsError::Path { class, source }))
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

pub fn strokes(edges: impl IntoIterator<Item = Edge>, styles: &EnumMap<Class, (Pen, Option<Linetype>)>, tolerance: Tolerance) -> Result<Vec<Element>, NEVec<ViewsError>> {
    let ranked = edges
        .into_iter()
        .sorted_by_key(|edge| (edge.class.visibility, Reverse(edge.class.kind)))
        .map(|Edge { path, class, depth: _ }| {
            let (pen, linetype) = &styles[class];
            Element::Stroke { path, pen: pen.clone(), linetype: linetype.clone() }
        })
        .collect();
    cleanup::coincident(ranked, |_| (), tolerance)
        .map(|kept| kept.into_iter().rev().collect())
        .map_err(|errors| errors.into_nonempty_iter().map(|source| ViewsError::Linework { operation: Operation::Cleanup, source }).collect())
}

pub(super) fn features(frame: &Frame, prepared: &Prepared, crease: Deflection) -> Vec<Feature> {
    let Prepared { body: _, mesh, sources, surface: _ } = prepared;
    let viewpoint = frame.inverse * Vector4::new(0.0, 0.0, -1.0, 0.0);
    let noise = f64::EPSILON.sqrt();
    let point = |vertex: u32| Point3::from(mesh.vertices()[vertex as usize]);
    let facet = |face: u32| {
        let triangle = mesh.triangle(face);
        let scaled = triangle.robust_scaled_normal();
        let [first, second, third] = triangle.edges_scaled_directions().map(Vector::length_squared);
        (sources[face as usize], scaled.try_normalize().filter(|_| scaled.length() > noise * first.max(second).max(third)).map(Vector3::from))
    };
    mesh.topology().map_or_default(|topology| {
        let half_edges = &topology.half_edges;
        let span = |half: &TopoHalfEdge| [point(half.vertex), point(half_edges[half.next as usize].vertex)];
        let side = |half: &TopoHalfEdge| {
            let own = facet(half.face);
            match own {
                (_, Some(_)) => own,
                (_, None) => iter::successors(Some(half), |turn| Some(&half_edges[turn.next as usize]))
                    .take(3)
                    .map(|turn| {
                        let [start, end] = span(turn);
                        (turn, distance_squared(&start, &end))
                    })
                    .max_by(|(_, left), (_, right)| left.total_cmp(right))
                    .and_then(|(longest, _)| half_edges.get(longest.twin as usize).map(|partner| facet(partner.face)))
                    .unwrap_or(own),
            }
        };
        half_edges
            .par_iter()
            .enumerate()
            .filter_map(|(index, half)| {
                let ends @ [start, _] = (index < half.twin as usize).then(|| span(half))?;
                let Some(partner) = half_edges.get(half.twin as usize) else {
                    return Some(Feature { ends, kind: Some(EdgeKind::Boundary) });
                };
                let toward = viewpoint.xyz() - start.coords.scale(viewpoint.w);
                let facing = |normal: &Vector3<f64>| normal.dot(&toward) > noise * toward.norm();
                let kind = match [side(half), side(partner)] {
                    [(None, _), (None, _)] => None,
                    [(None, _), (Some(_), _)] | [(Some(_), _), (None, _)] => Some(Some(EdgeKind::Cut)),
                    [(Some(_), Some(left)), (Some(_), Some(right))] if facing(&left) != facing(&right) => Some(Some(EdgeKind::Silhouette)),
                    [(Some(_), Some(left)), (Some(_), Some(right))] if left.angle(&right) > crease.into_inner() => Some(Some(EdgeKind::Crease)),
                    [(Some(left), _), (Some(right), _)] => (left != right).then_some(None),
                }?;
                Some(Feature { ends, kind })
            })
            .collect()
    })
}
