#![feature(cmp_minmax, const_result_trait_fn, const_trait_impl, default_field_values, deref_patterns)]

use core::{cmp, iter};
use std::{
    collections::{BTreeMap, HashMap, HashSet},
    io,
};

use itertools::{Either, Itertools as _};
use kurbo::{BezPath, DEFAULT_ACCURACY, ParamCurve as _, PathEl, Point, QuadBez, Rect, Shape as _, Size, Vec2};
use manatee::Anchor;
use nonempty_collections::{IntoNonEmptyIterator as _, NEBTreeMap, NonEmptyIterator as _};
use petgraph::{
    Directed, EdgeType,
    algo::kosaraju_scc,
    graphmap::DiGraphMap,
    stable_graph::{EdgeIndex, NodeIndex, StableGraph},
    visit::IntoNodeReferences as _,
};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Drawing<N, E, Ty = Directed> = StableGraph<Placed<N>, Routed<E>, Ty>;

type Hierarchy = (Vec<(NodeIndex, Node)>, HashSet<NodeIndex>);

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Format {
    #[display("GraphML")]
    GraphMl,
    #[display("DOT")]
    Dot,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Refusal {
    #[display("outside the graph")]
    Missing,
    #[display("nesting under itself")]
    Nesting,
    #[display("constrained as a cluster")]
    Constrained,
}

#[derive(Debug, Clone, Copy, PartialEq)]
enum Course {
    Straight,
    Arc(f64),
    Bent,
    Loop(f64),
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(const_fn, validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd, Into))]
pub struct Distance(f64);

#[nutype::nutype(validate(predicate = |size| size.is_finite() && size.width >= 0.0 && size.height >= 0.0), derive(Debug, Clone, Copy, PartialEq, Into, AsRef))]
pub struct Extent(Size);

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Node {
    pub extent: Extent,
    pub parent: Option<NodeIndex>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Placed<N> {
    pub weight: N,
    pub frame: Rect,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Routed<E> {
    pub weight: E,
    pub path: BezPath,
    pub label: Option<Rect>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum LayoutError {
    #[error("refused nodes: {}", nodes.iter().format_with(", ", |(node, refusal), sink| sink(&format_args!("{} {refusal}", node.index()))))]
    Refused { nodes: NEBTreeMap<NodeIndex, Refusal> },
    #[error("node {} received no position", node.index())]
    Unplaced { node: NodeIndex },
    #[error("edge {} received no route", edge.index())]
    Unrouted { edge: EdgeIndex },
    #[error(transparent)]
    Layered(#[from] dugong::LayoutError),
    #[error(transparent)]
    Force(#[from] manatee::Error),
    #[error("export {format}")]
    Export { format: Format, source: io::Error },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn nesting<N, E, Ty>(
    graph: &StableGraph<N, E, Ty>,
    node: impl Fn(NodeIndex, &N) -> Node,
    named: impl IntoIterator<Item = NodeIndex>,
    constrained: impl IntoIterator<Item = NodeIndex>,
) -> Result<Hierarchy, LayoutError>
where
    Ty: EdgeType,
{
    let specs = graph.node_references().map(|(index, weight)| (index, node(index, weight))).collect::<Vec<_>>();
    let clusters = specs.iter().filter_map(|&(_, Node { extent: _, parent })| parent).collect::<HashSet<_>>();
    let parents = DiGraphMap::<NodeIndex, ()>::from_edges(specs.iter().filter_map(|&(child, Node { extent: _, parent })| parent.map(|parent| (child, parent))));
    let refused = constrained
        .into_iter()
        .filter(|member| clusters.contains(member))
        .map(|member| (member, Refusal::Constrained))
        .chain(parents.nodes().chain(named).filter(|&member| !graph.contains_node(member)).map(|member| (member, Refusal::Missing)))
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
        .collect::<BTreeMap<_, _>>();
    match NEBTreeMap::try_from_map(refused) {
        Some(nodes) => Err(LayoutError::Refused { nodes }),
        None => Ok((specs, clusters)),
    }
}

fn fanned<I>(step: Distance, edges: I) -> HashMap<EdgeIndex, Course>
where I: IntoIterator<Item = (EdgeIndex, NodeIndex, NodeIndex)> {
    let step = step.into_inner();
    let reaches = move |base: f64| iter::successors(Some(base), move |reach| Some(reach + step));
    edges
        .into_iter()
        .into_group_map_by(|&(_, source, target)| cmp::minmax(source, target))
        .into_iter()
        .flat_map(|([low, high], group)| {
            let odd = !group.len().is_multiple_of(2);
            let courses = if low == high {
                Either::Left(reaches(0.0).map(Course::Loop))
            } else {
                Either::Right(iter::once(Course::Straight).filter(move |_| odd).chain(reaches(if odd { step } else { step / 2.0 }).flat_map(|reach| [Course::Arc(reach), Course::Arc(-reach)])))
            };
            iter::zip(group, courses).map(|((index, source, target), course)| {
                (
                    index,
                    match course {
                        Course::Arc(reach) if source > target => Course::Arc(-reach),
                        Course::Straight | Course::Arc(_) | Course::Bent | Course::Loop(_) => course,
                    },
                )
            })
        })
        .collect()
}

fn direct(source: (Rect, Option<Anchor>), target: (Rect, Option<Anchor>), course: Course, label: Option<Extent>) -> (BezPath, Option<Rect>) {
    let normal = |side: Anchor| match side {
        Anchor::Left => Vec2::new(-1.0, 0.0),
        Anchor::Right => Vec2::new(1.0, 0.0),
        Anchor::Top => Vec2::new(0.0, -1.0),
        Anchor::Bottom => Vec2::new(0.0, 1.0),
    };
    let half = |size: Size, axis: Vec2| Vec2::new(axis.x.abs(), axis.y.abs()).dot(size.to_vec2()) / 2.0;
    let port = |frame: Rect, side: Anchor| frame.center() + normal(side) * half(frame.size(), normal(side));
    let border = |frame: Rect, toward: Point| {
        let offset = toward - frame.center();
        [(offset.x, frame.width()), (offset.y, frame.height())]
            .into_iter()
            .filter(|&(along, _)| along != 0.0)
            .map(|(along, span)| span / (2.0 * along.abs()))
            .reduce(f64::min)
            .map_or_else(|| frame.center(), |scale| frame.center() + offset * scale)
    };
    let aim = |(frame, anchor): (Rect, Option<Anchor>)| anchor.map_or_else(|| frame.center(), |side| port(frame, side));
    let end = |(frame, anchor): (Rect, Option<Anchor>), toward: Point| anchor.map_or_else(|| border(frame, toward), |side| port(frame, side));
    let boxed = |center: Point| label.map(|extent| Rect::from_center_size(center, extent.into_inner()));
    let polyline = |start: Point, rest: &[Point]| iter::once(PathEl::MoveTo(start)).chain(rest.iter().copied().map(PathEl::LineTo)).collect::<BezPath>();
    let contained = |(outer, outer_anchor): (Rect, Option<Anchor>), (inner, inner_anchor): (Rect, Option<Anchor>)| {
        let reach = |side: Anchor, from: Point| normal(side).dot(port(outer, side) - from);
        let side = outer_anchor.or(inner_anchor).unwrap_or_else(|| {
            [Anchor::Top, Anchor::Right, Anchor::Bottom, Anchor::Left].into_nonempty_iter().min_by(|&first, &second| reach(first, port(inner, first)).total_cmp(&reach(second, port(inner, second))))
        });
        let inner_end = port(inner, inner_anchor.unwrap_or(side));
        (outer_anchor.map_or_else(|| inner_end + normal(side) * reach(side, inner_end), |anchor| port(outer, anchor)), inner_end)
    };
    let ((first, tail), (second, head)) = (source, target);
    let (from, to) = (aim(source), aim(target));
    let delta = to - from;
    match course {
        Course::Loop(extra) => {
            let outward = normal(tail.or(head).unwrap_or(Anchor::Right));
            let (depth, reach) = (half(first.size(), outward), half(first.size(), outward.turn_90()));
            let [rise, crest, apex, trough, fall] = [(2.0 / 3.0, -1.0), (5.0 / 6.0, -1.0), (1.0, 0.0), (5.0 / 6.0, 1.0), (2.0 / 3.0, 1.0)]
                .map(|(along, across): (f64, f64)| first.center() + outward * (reach + extra).mul_add(along, depth) + outward.turn_90() * (reach * across));
            (
                polyline(border(first, rise), &[rise, crest, apex, trough, fall, border(first, fall)]),
                label.map(Extent::into_inner).map(|size| Rect::from_center_size(apex + outward * half(size, outward), size)),
            )
        }
        Course::Straight | Course::Arc(_) | Course::Bent if first.contains_rect(second) => {
            let (start, finish) = contained(source, target);
            (polyline(start, &[finish]), boxed(start.midpoint(finish)))
        }
        Course::Straight | Course::Arc(_) | Course::Bent if second.contains_rect(first) => {
            let (finish, start) = contained(target, source);
            (polyline(start, &[finish]), boxed(start.midpoint(finish)))
        }
        Course::Arc(offset) if delta.hypot2() > 0.0 => {
            let control = from.midpoint(to) + delta.normalize().turn_90() * (2.0 * offset);
            let curve = QuadBez::new(end(source, control), control, end(target, control));
            (curve.to_path(DEFAULT_ACCURACY), boxed(curve.eval(0.5)))
        }
        Course::Bent if delta.x != 0.0 && delta.y != 0.0 => {
            let bend = match tail {
                Some(Anchor::Top | Anchor::Bottom) => Point::new(from.x, to.y),
                Some(Anchor::Left | Anchor::Right) | None => Point::new(to.x, from.y),
            };
            (polyline(end(source, bend), &[bend, end(target, bend)]), boxed(bend))
        }
        Course::Straight | Course::Arc(_) | Course::Bent => {
            let (start, finish) = (end(source, to), end(target, from));
            (polyline(start, &[finish]), boxed(start.midpoint(finish)))
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod export;
pub mod force;
pub mod layered;
