use core::cmp::Reverse;

use enum_map::EnumMap;
use itertools::Itertools as _;
use kurbo::{Line, PathEl, PathSeg, Point};
use nalgebra::{Point3, Vector3, Vector4};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use rasm_linework::{Class, Deflection, Edge, EdgeKind, Element, Linetype, Path, Pen, Tolerance, cleanup};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{
    Feature, Frame, Operation, Prepared, View, ViewsError,
    visibility::{Lines, Piece},
};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn edges(view: &View) -> Result<Vec<Edge>, NEVec<ViewsError>> {
    let Lines { vertices, pieces } = &view.lines;
    let key = |point: Point| [point.x.to_bits(), point.y.to_bits()];
    pieces
        .iter()
        .filter_map(|&Piece { ends: [start, end], depths: [near, far], kind, visibility }| {
            Some((Class { kind: kind?, visibility }, (Line::new(*vertices.get(start)?, *vertices.get(end)?), f64::min(near, far))))
        })
        .into_group_map()
        .into_iter()
        .sorted_unstable_by_key(|&(class, _)| class)
        .collect::<Vec<_>>()
        .into_par_iter()
        .flat_map_iter(move |(class, group)| {
            let ends = group.iter().flat_map(|&(Line { p0, p1 }, least)| [(key(p1), (p0, least)), (key(p0), (p1, least))]).into_group_map();
            let depth =
                move |from: Point, to: Point| ends.get(&key(to))?.iter().min_by(|(near, _), (far, _)| near.distance_squared(from).total_cmp(&far.distance_squared(from))).map(|&(_, least)| least);
            cleanup::chain(group.into_iter().map(|(line, _)| PathSeg::Line(line)), view.tolerance).into_iter().filter_map(move |run| {
                let nearest = run.elements().iter().filter_map(PathEl::end_point).tuple_windows().filter_map(|(from, to)| depth(from, to)).min_by(f64::total_cmp)?;
                Some(Path::try_new(run).map(|path| Edge { path, class, depth: nearest }).map_err(|source| ViewsError::Path { class, source }))
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

pub fn strokes(edges: Vec<Edge>, styles: &EnumMap<Class, (Pen, Option<Linetype>)>, tolerance: Tolerance) -> Result<Vec<Element>, NEVec<ViewsError>> {
    let rank = |Class { kind, visibility }: Class| (Reverse(visibility), kind);
    let priority = |element: &Element| match element {
        Element::Stroke { path: _, pen, linetype } => styles.iter().filter(|(_, (style_pen, style_linetype))| style_pen == pen && style_linetype == linetype).map(|(class, _)| rank(class)).max(),
        Element::Fill { region: _, color: _, opacity: _ }
        | Element::Hatch { region: _, pattern: _, placement: _, pen: _ }
        | Element::Text { text: _, color: _, opacity: _ }
        | Element::Glyphs { run: _, color: _, opacity: _ }
        | Element::Placed { content: _, placement: _, opacity: _ }
        | Element::Clip { region: _, keep: _, elements: _ } => None,
    };
    let painted = edges
        .into_iter()
        .sorted_by(|left, right| rank(left.class).cmp(&rank(right.class)).then(right.depth.total_cmp(&left.depth)))
        .map(|Edge { path, class, depth: _ }| {
            let (pen, linetype) = &styles[class];
            Element::Stroke { path, pen: pen.clone(), linetype: linetype.clone() }
        })
        .collect();
    cleanup::coincident(painted, priority, tolerance).map_err(|errors| errors.into_nonempty_iter().map(|source| ViewsError::Linework { operation: Operation::Cleanup, source }).collect())
}

pub(super) fn features(frame: &Frame, prepared: &Prepared, crease: Deflection) -> Vec<Feature> {
    let Prepared { body: _, mesh, sources } = prepared;
    let viewpoint = frame.inverse * Vector4::new(0.0, 0.0, -1.0, 0.0);
    let point = |vertex: u32| mesh.vertices().get(vertex as usize).map(|&position| Point3::from(position));
    let side = |face: u32| Some((*sources.get(face as usize)?, mesh.triangle(face).normal().map(Vector3::from)));
    mesh.topology().map_or_default(|topology| {
        let half_edges = &topology.half_edges;
        half_edges
            .par_iter()
            .enumerate()
            .filter_map(|(index, half)| {
                let ends @ [start, _] = [point(half.vertex)?, point(half_edges.get(half.next as usize)?.vertex)?];
                let twin = match Some(half.twin).filter(|&twin| twin != u32::MAX).map(|twin| twin as usize) {
                    None => return Some(Feature { ends, kind: Some(EdgeKind::Boundary) }),
                    Some(twin) => half_edges.get(twin).filter(|_| index < twin)?,
                };
                let toward = viewpoint.xyz() - start.coords.scale(viewpoint.w);
                let facing = |normal: &Vector3<f64>| normal.dot(&toward) > 0.0;
                let [first, second] = [half.face, twin.face].map(side);
                let kind = match [first?, second?] {
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
