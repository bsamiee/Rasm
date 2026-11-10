#![feature(cmp_minmax, const_result_trait_fn, const_trait_impl, default_field_values, deref_patterns)]

use core::cmp::{self, Ordering};
use core::num::ParseFloatError;
use core::{fmt, iter};
use std::collections::{BTreeMap, BTreeSet, HashMap, HashSet};
use std::io;

use dugong::graphlib::GraphError;
use itertools::{Either, Itertools as _};
use kurbo::{BezPath, DEFAULT_ACCURACY, Ellipse, Line, ParamCurve as _, PathEl, Point, QuadBez, Rect, Shape as _, Size, Vec2};
use manatee::Anchor;
use nonempty_collections::{IntoNonEmptyIterator as _, NEBTreeSet, NonEmptyIterator as _};
use petgraph::algo::kosaraju_scc;
use petgraph::graphmap::DiGraphMap;
use petgraph::stable_graph::{EdgeIndex, NodeIndex, StableGraph};
use petgraph::visit::{Dfs, EdgeRef as _, IntoEdgeReferences as _, IntoNodeReferences as _, Reversed, Walker as _};
use petgraph::{Directed, Direction, EdgeType};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, derive_more::Display)]
pub enum Refusal {
    #[display("outside the graph")]
    Missing,
    #[display("nesting under itself")]
    Nesting,
    #[display("constrained as a cluster")]
    Constrained,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Shape {
    #[display("box")]
    Rectangle,
    #[display("ellipse")]
    Ellipse,
    #[display("diamond")]
    Diamond,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::IsVariant)]
pub enum CurveStyle {
    Straight,
    Segments,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(const_fn, validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd, Into))]
pub struct Distance(f64);

#[nutype::nutype(validate(predicate = |size| size.is_finite() && size.width >= 0.0 && size.height >= 0.0), derive(Debug, Clone, Copy, PartialEq, Into, AsRef))]
pub struct Extent(Size);

#[nutype::nutype(validate(finite, greater_or_equal = 0.0, less_or_equal = 1.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd, Into))]
pub struct Tension(f64);

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Interpolation {
    Linear,
    Basis,
    Cardinal(Tension),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Node {
    pub extent: Extent,
    pub parent: Option<NodeIndex> = None,
    pub shape: Shape = Shape::Rectangle,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Placed<N> {
    pub weight: N,
    pub frame: Rect,
    pub shape: Shape,
    pub parent: Option<NodeIndex>,
    pub label: Option<Rect>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Routed<E> {
    pub weight: E,
    pub path: BezPath,
    pub label: Option<Rect>,
}

#[derive(Debug, Clone)]
pub struct Drawing<N, E, Ty = Directed>
where Ty: EdgeType
{
    pub graph: StableGraph<Placed<N>, Routed<E>, Ty>,
    pub canvas: Rect,
}

struct Hierarchy<S> {
    titles: BTreeMap<NodeIndex, Extent>,
    nodes: BTreeMap<NodeIndex, Node>,
    edges: Vec<(EdgeIndex, NodeIndex, NodeIndex, S)>,
    parents: DiGraphMap<NodeIndex, ()>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum LayoutError {
    #[error("refused nodes: {}", refusals.iter().format_with(", ", |(node, refusal), sink| sink(&format_args!("{} {refusal}", node.index()))))]
    Refused { refusals: NEBTreeSet<(NodeIndex, Refusal)> },
    #[error("{} absent from the layout", fmt::from_fn(|formatter| match element {
        Either::Left(node) => write!(formatter, "node {}", node.index()),
        Either::Right(edge) => write!(formatter, "edge {}", edge.index()),
    }))]
    Absent { element: Either<NodeIndex, EdgeIndex> },
    #[error(transparent)]
    Graph(#[from] GraphError),
    #[error(transparent)]
    Layered(#[from] dugong::LayoutError),
    #[error(transparent)]
    Force(#[from] manatee::Error),
    #[error("arrowsize {value} of edge {}", edge.index())]
    Arrowsize { edge: EdgeIndex, value: String, source: ParseFloatError },
    #[error("export {format}")]
    Export { format: Format, source: io::Error },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn hierarchy<N, E, Ty, S>(
    graph: &StableGraph<N, E, Ty>,
    node: impl Fn(NodeIndex, &N) -> Node,
    edge: impl Fn(EdgeIndex, &E) -> S,
    named: impl IntoIterator<Item = NodeIndex>,
    constrained: impl IntoIterator<Item = NodeIndex>,
) -> Result<Hierarchy<S>, LayoutError>
where
    Ty: EdgeType,
{
    let nodes = graph.node_references().map(|(index, weight)| (index, node(index, weight))).collect::<BTreeMap<_, _>>();
    let parents = DiGraphMap::<NodeIndex, ()>::from_edges(nodes.iter().filter_map(|(&child, &Node { extent: _, parent, shape: _ })| parent.map(|parent| (child, parent))));
    let missing = |member: NodeIndex| (!graph.contains_node(member)).then_some((member, Refusal::Missing));
    let refusals = parents
        .nodes()
        .chain(named)
        .filter_map(missing)
        .chain(constrained.into_iter().flat_map(|member| [missing(member), is_cluster(&parents, member).then_some((member, Refusal::Constrained))]).flatten())
        .chain(
            kosaraju_scc(&parents)
                .into_iter()
                .filter(|component| match component {
                    [member] => parents.contains_edge(*member, *member),
                    _ => true,
                })
                .flatten()
                .map(|member| (member, Refusal::Nesting)),
        )
        .collect::<BTreeSet<_>>();
    match NEBTreeSet::try_from_set(refusals) {
        Some(refusals) => Err(LayoutError::Refused { refusals }),
        None => Ok(Hierarchy {
            titles: nodes
                .iter()
                .filter_map(|(&index, &Node { extent, parent: _, shape: _ })| (is_cluster(&parents, index) && !extent.into_inner().is_zero_area()).then_some((index, extent)))
                .collect(),
            nodes,
            edges: graph.edge_references().map(|reference| (reference.id(), reference.source(), reference.target(), edge(reference.id(), reference.weight()))).collect(),
            parents,
        }),
    }
}

fn is_cluster(parents: &DiGraphMap<NodeIndex, ()>, member: NodeIndex) -> bool { parents.neighbors_directed(member, Direction::Incoming).next().is_some() }

fn ancestors(parents: &DiGraphMap<NodeIndex, ()>, member: NodeIndex) -> impl Iterator<Item = NodeIndex> {
    iter::successors(parents.neighbors(member).next(), |&cluster| parents.neighbors(cluster).next())
}

fn nesting(parents: &DiGraphMap<NodeIndex, ()>, source: NodeIndex, target: NodeIndex) -> Option<Ordering> {
    match source == target {
        true => Some(Ordering::Equal),
        false if ancestors(parents, target).contains(&source) => Some(Ordering::Greater),
        false if ancestors(parents, source).contains(&target) => Some(Ordering::Less),
        false => None,
    }
}

fn linked(parents: &DiGraphMap<NodeIndex, ()>, source: NodeIndex, target: NodeIndex) -> bool {
    match nesting(parents, source, target) {
        None => true,
        Some(Ordering::Equal) => !is_cluster(parents, source),
        Some(Ordering::Greater | Ordering::Less) => false,
    }
}

fn titled(nodes: &BTreeMap<NodeIndex, Node>, parents: &DiGraphMap<NodeIndex, ()>, titles: &BTreeMap<NodeIndex, Extent>) -> impl Iterator<Item = ((NodeIndex, Extent), (NodeIndex, Extent))> {
    nodes
        .iter()
        .filter_map(move |(&member, &Node { extent, parent: _, shape: _ })| if is_cluster(parents, member) { titles.get(&member).map(|&title| (member, title)) } else { Some((member, extent)) })
        .flat_map(move |(member, extent)| ancestors(parents, member).filter_map(move |cluster| titles.get(&cluster).map(|&title| ((cluster, title), (member, extent)))))
}

fn non_cluster<N, E, Ty, I>(graph: &StableGraph<N, E, Ty>, parents: &DiGraphMap<NodeIndex, ()>, ends: I) -> impl Fn(NodeIndex) -> NodeIndex + use<N, E, Ty, I>
where
    Ty: EdgeType,
    I: IntoIterator<Item = NodeIndex>,
{
    let children = ends
        .into_iter()
        .filter(|&end| is_cluster(parents, end))
        .unique()
        .filter_map(|cluster| {
            let outside = graph.neighbors_undirected(cluster).filter(|&neighbor| neighbor != cluster && !ancestors(parents, neighbor).contains(&cluster)).collect::<HashSet<_>>();
            Dfs::new(Reversed(parents), cluster)
                .iter(Reversed(parents))
                .filter(|&member| !is_cluster(parents, member))
                .find_or_last(|&leaf| graph.neighbors_undirected(leaf).all(|neighbor| !outside.contains(&neighbor)))
                .map(|leaf| (cluster, leaf))
        })
        .collect::<HashMap<_, _>>();
    move |member| children.get(&member).copied().unwrap_or(member)
}

fn offsets<S>(step: f64, edges: &[(EdgeIndex, NodeIndex, NodeIndex, S)]) -> HashMap<EdgeIndex, f64> {
    let series = move |base: f64| iter::successors(Some(base), move |offset| Some(offset + step));
    edges
        .iter()
        .into_group_map_by(|&&(_, source, target, _)| cmp::minmax(source, target))
        .into_iter()
        .flat_map(|([low, high], group)| {
            let (base, skipped) = if group.len().is_multiple_of(2) { (step / 2.0, 0) } else { (0.0, 1) };
            let fanned = series(base).flat_map(|offset| [offset, -offset]).skip(skipped);
            iter::zip(group, iter::zip(fanned, series(step))).map(move |(&(index, source, _, _), (offset, extra))| match (low == high, source == low) {
                (true, _) => (index, extra),
                (false, true) => (index, offset),
                (false, false) => (index, -offset),
            })
        })
        .collect()
}

fn route(
    source: (Rect, Shape, Option<Anchor>),
    target: (Rect, Shape, Option<Anchor>),
    (curve_style, offset): (CurveStyle, f64),
    nesting: Option<Ordering>,
    interpolation: Interpolation,
    label: Option<Extent>,
) -> (BezPath, Option<Rect>) {
    let border = |shape: Shape, frame: Rect, toward: Point| -> Point {
        let (center, offset) = (frame.center(), toward - frame.center());
        [(offset.x, frame.width()), (offset.y, frame.height())]
            .into_iter()
            .filter(|&(along, _)| along != 0.0)
            .map(|(along, span)| span / along.abs())
            .reduce(f64::min)
            .and_then(|scale| crossing(&outline(shape, frame), Line::new(center + offset * scale, center)))
            .unwrap_or(center)
    };
    let normal = |side: Anchor| match side {
        Anchor::Left => Vec2::new(-1.0, 0.0),
        Anchor::Right => Vec2::new(1.0, 0.0),
        Anchor::Top => Vec2::new(0.0, -1.0),
        Anchor::Bottom => Vec2::new(0.0, 1.0),
    };
    let half = |size: Size, axis: Vec2| Vec2::new(axis.x.abs(), axis.y.abs()).dot(size.to_vec2()) / 2.0;
    let port = |frame: Rect, side: Anchor| frame.center() + normal(side) * half(frame.size(), normal(side));
    let aim = |(frame, _, anchor): (Rect, Shape, Option<Anchor>)| anchor.map_or_else(|| frame.center(), |side| port(frame, side));
    let end = |(frame, shape, anchor): (Rect, Shape, Option<Anchor>), toward: Point| anchor.map_or_else(|| border(shape, frame, toward), |side| port(frame, side));
    let centered = |center: Point| label.map(|extent| Rect::from_center_size(center, extent.into_inner()));
    let straight = |(start, finish): (Point, Point)| (path(interpolation, &[start, finish]), centered(start.midpoint(finish)));
    let contained = |(outer, _, outer_anchor): (Rect, Shape, Option<Anchor>), (inner, _, inner_anchor): (Rect, Shape, Option<Anchor>), spread: f64| {
        let depth = |side: Anchor, from: Point| normal(side).dot(port(outer, side) - from);
        let side = match (outer_anchor, inner_anchor) {
            (Some(side), _) | (None, Some(side)) => side,
            (None, None) => [Anchor::Top, Anchor::Right, Anchor::Bottom, Anchor::Left]
                .into_nonempty_iter()
                .min_by(|&first, &second| depth(first, port(inner, first)).total_cmp(&depth(second, port(inner, second)))),
        };
        let finish = port(inner, inner_anchor.unwrap_or(side));
        let shift = normal(side).turn_90() * spread;
        (outer_anchor.map_or_else(|| finish + normal(side) * depth(side, finish), |anchor| port(outer, anchor)) + shift, finish + shift)
    };
    let ((first, shape, tail), (_, _, head)) = (source, target);
    let (from, to) = (aim(source), aim(target));
    let delta = to - from;
    let diagonal = delta.x != 0.0 && delta.y != 0.0;
    match (curve_style, nesting) {
        (CurveStyle::Straight | CurveStyle::Segments, Some(Ordering::Equal)) => {
            let outward = normal(tail.or(head).unwrap_or(Anchor::Right));
            let (depth, width) = (half(first.size(), outward), half(first.size(), outward.turn_90()));
            let [rise, crest, apex, trough, fall] = [(2.0 / 3.0, -1.0), (5.0 / 6.0, -1.0), (1.0, 0.0), (5.0 / 6.0, 1.0), (2.0 / 3.0, 1.0)]
                .map(|(along, across): (f64, f64)| first.center() + outward * (width + offset).mul_add(along, depth) + outward.turn_90() * (offset.mul_add(0.5, width) * across));
            (
                path(interpolation, &[border(shape, first, rise), rise, crest, apex, trough, fall, border(shape, first, fall)]),
                label.map(Extent::into_inner).map(|size| Rect::from_center_size(apex + outward * half(size, outward), size)),
            )
        }
        (CurveStyle::Straight, None) if offset != 0.0 && delta.hypot2() > 0.0 => {
            let control = from.midpoint(to) + delta.normalize().turn_90() * (2.0 * offset);
            let quadratic = QuadBez::new(end(source, control), control, end(target, control));
            (quadratic.to_path(DEFAULT_ACCURACY), centered(quadratic.eval(0.5)))
        }
        (CurveStyle::Segments, None) if delta.hypot2() > 0.0 && (offset != 0.0 || diagonal) => {
            let corner = match (diagonal, tail) {
                (false, _) => from.midpoint(to),
                (true, Some(Anchor::Top | Anchor::Bottom)) => Point::new(from.x, to.y),
                (true, Some(Anchor::Left | Anchor::Right) | None) => Point::new(to.x, from.y),
            };
            let bend = corner + delta.normalize().turn_90() * offset;
            (path(interpolation, &[end(source, bend), bend, end(target, bend)]), centered(bend))
        }
        (CurveStyle::Straight | CurveStyle::Segments, Some(Ordering::Greater)) => straight(contained(source, target, offset)),
        (CurveStyle::Straight | CurveStyle::Segments, Some(Ordering::Less)) => {
            let (finish, start) = contained(target, source, -offset);
            straight((start, finish))
        }
        (CurveStyle::Straight | CurveStyle::Segments, None) => straight((end(source, to), end(target, from))),
    }
}

fn outline(shape: Shape, frame: Rect) -> BezPath {
    let center = frame.center();
    match shape {
        Shape::Rectangle => frame.to_path(DEFAULT_ACCURACY),
        Shape::Ellipse => Ellipse::from_rect(frame).to_path(DEFAULT_ACCURACY),
        Shape::Diamond => BezPath::from_vec(vec![
            PathEl::MoveTo(Point::new(center.x, frame.y0)),
            PathEl::LineTo(Point::new(frame.x1, center.y)),
            PathEl::LineTo(Point::new(center.x, frame.y1)),
            PathEl::LineTo(Point::new(frame.x0, center.y)),
            PathEl::ClosePath,
        ]),
    }
}

fn crossing(outline: &BezPath, span: Line) -> Option<Point> {
    outline.segments().flat_map(|side| side.intersect_line(span)).map(|hit| hit.line_t).min_by(f64::total_cmp).map(|line_t| span.eval(line_t))
}

fn path(interpolation: Interpolation, points: &[Point]) -> BezPath {
    match (interpolation, points) {
        (Interpolation::Basis, &[first, second, _, ..]) => iter::once(PathEl::MoveTo(first))
            .chain(iter::once(PathEl::LineTo(first.lerp(second, 1.0 / 6.0))))
            .chain(
                points
                    .iter()
                    .copied()
                    .chain(points.last().copied())
                    .tuple_windows()
                    .map(|(before, control, after)| PathEl::CurveTo(before.lerp(control, 1.0 / 3.0), before.lerp(control, 2.0 / 3.0), before.midpoint(after).lerp(control, 2.0 / 3.0))),
            )
            .chain(points.last().copied().map(PathEl::LineTo))
            .collect(),
        (Interpolation::Cardinal(tension), &[first, _, _, ..]) => {
            let scale = (1.0 - tension.into_inner()) / 6.0;
            let tangents = iter::once(Vec2::ZERO).chain(points.iter().tuple_windows().map(|(&before, _, &after)| (after - before) * scale)).chain(iter::once(Vec2::ZERO));
            iter::once(PathEl::MoveTo(first))
                .chain(iter::zip(points.iter().copied(), tangents).tuple_windows().map(|((start, outgoing), (finish, incoming))| PathEl::CurveTo(start + outgoing, finish - incoming, finish)))
                .collect()
        }
        (Interpolation::Linear | Interpolation::Basis | Interpolation::Cardinal(_), [first, rest @ ..]) => iter::once(PathEl::MoveTo(*first)).chain(rest.iter().copied().map(PathEl::LineTo)).collect(),
        (Interpolation::Linear | Interpolation::Basis | Interpolation::Cardinal(_), []) => BezPath::new(),
    }
}

fn drawn<N, E, Ty>(
    graph: StableGraph<N, E, Ty>,
    nodes: &BTreeMap<NodeIndex, Node>,
    mut frames: HashMap<NodeIndex, (Rect, Option<Rect>)>,
    mut routes: HashMap<EdgeIndex, (BezPath, Option<Rect>)>,
    margins: Option<Rect>,
) -> Result<Drawing<N, E, Ty>, LayoutError>
where
    Ty: EdgeType,
{
    let canvas = frames
        .values()
        .flat_map(|&(frame, title)| iter::once(frame).chain(title))
        .chain(routes.values().flat_map(|(path, label)| iter::once(path.bounding_box()).chain(*label)))
        .chain(margins)
        .reduce(|whole, rect| whole.union(rect))
        .unwrap_or_default();
    match (graph.node_indices().find(|index| !frames.contains_key(index)), graph.edge_indices().find(|index| !routes.contains_key(index))) {
        (Some(node), _) => Err(LayoutError::Absent { element: Either::Left(node) }),
        (None, Some(edge)) => Err(LayoutError::Absent { element: Either::Right(edge) }),
        (None, None) => Ok(Drawing {
            graph: graph.filter_map_owned(
                |index, weight| {
                    let ((frame, label), &Node { extent: _, parent, shape }) = frames.remove(&index).zip(nodes.get(&index))?;
                    Some(Placed { weight, frame, shape, parent, label })
                },
                |index, weight| routes.remove(&index).map(|(path, label)| Routed { weight, path, label }),
            ),
            canvas,
        }),
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod export;
pub mod force;
pub mod layered;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use export::{Attributes, Format, write};
