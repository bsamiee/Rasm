use core::iter;
use std::collections::{HashMap, HashSet};

use dugong::{
    EdgeLabel, GraphLabel, LabelPos, NodeLabel, RankDir, WorkControl,
    graphlib::{Graph, GraphOptions},
};
use itertools::{Itertools as _, Position};
use kurbo::{Affine, BezPath, DEFAULT_ACCURACY, Line, ParamCurve as _, PathEl, Point, Rect, Shape as _, Size};
use petgraph::{
    EdgeType,
    stable_graph::{EdgeIndex, NodeIndex, StableGraph},
    visit::{EdgeRef as _, IntoEdgeReferences as _},
};

use crate::{Distance, Drawing, Extent, LayoutError, Node, Placed, Routed, direct, fanned, nesting};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "kebab-case")]
pub enum Ranker {
    NetworkSimplex,
    TightTree,
    LongestPath,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Acyclicer {
    #[display("dfs")]
    DepthFirst,
    #[display("greedy")]
    Greedy,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Align {
    #[display("ul")]
    UpLeft,
    #[display("ur")]
    UpRight,
    #[display("dl")]
    DownLeft,
    #[display("dr")]
    DownRight,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Options {
    pub rankdir: RankDir = RankDir::TB,
    pub nodesep: Distance = Distance::try_new(50.0).ok().unwrap(),
    pub ranksep: Distance = Distance::try_new(50.0).ok().unwrap(),
    pub edgesep: Distance = Distance::try_new(20.0).ok().unwrap(),
    pub marginx: Distance = Distance::try_new(0.0).ok().unwrap(),
    pub marginy: Distance = Distance::try_new(0.0).ok().unwrap(),
    pub ranker: Ranker = Ranker::NetworkSimplex,
    pub acyclicer: Acyclicer = Acyclicer::DepthFirst,
    pub align: Option<Align> = None,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Label {
    pub extent: Extent,
    pub labelpos: LabelPos = LabelPos::R,
    pub labeloffset: Distance = Distance::try_new(10.0).ok().unwrap(),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Edge {
    pub label: Option<Label> = None,
    pub minlen: usize = 1,
    pub weight: u32 = 1,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn layout<N, E, Ty>(
    options: &Options,
    work: &mut dyn WorkControl,
    node: impl Fn(NodeIndex, &N) -> Node,
    edge: impl Fn(EdgeIndex, &E) -> Edge,
    graph: StableGraph<N, E, Ty>,
) -> Result<Drawing<N, E, Ty>, LayoutError>
where
    Ty: EdgeType,
{
    let Options { rankdir, nodesep, ranksep, edgesep, marginx, marginy, ranker, acyclicer, align } = *options;
    let (specs, clusters) = nesting(&graph, node, iter::empty(), iter::empty())?;
    let links = specs.iter().filter_map(|&(child, Node { extent: _, parent })| parent.map(|parent| (child, parent))).collect::<Vec<_>>();
    let parents = links.iter().copied().collect::<HashMap<_, _>>();
    let children = links.iter().map(|&(child, parent)| (parent, child)).into_group_map();
    let within = |member: NodeIndex, cluster: NodeIndex| iter::successors(parents.get(&member), |&parent| parents.get(parent)).contains(&cluster);
    let anchor = |cluster: NodeIndex| {
        let neighbors = graph.neighbors_undirected(cluster).filter(|&neighbor| neighbor != cluster && !within(neighbor, cluster)).collect::<HashSet<_>>();
        let mut stack = vec![cluster];
        iter::from_fn(|| stack.pop().inspect(|member| stack.extend(children.get(member).into_iter().flatten().rev())))
            .filter(|member| !clusters.contains(member))
            .find_or_last(|&leaf| graph.neighbors_undirected(leaf).all(|neighbor| !neighbors.contains(&neighbor)))
    };
    let edges = graph
        .edge_references()
        .map(|reference| {
            let (source, target) = (reference.source(), reference.target());
            let ranked = !((source == target && clusters.contains(&source)) || within(source, target) || within(target, source));
            (reference.id(), source, target, edge(reference.id(), reference.weight()), ranked)
        })
        .collect::<Vec<_>>();
    let anchors = edges
        .iter()
        .filter(|&&(_, _, _, _, ranked)| ranked)
        .flat_map(|&(_, source, target, _, _)| [source, target])
        .filter(|end| clusters.contains(end))
        .unique()
        .filter_map(|cluster| anchor(cluster).map(|leaf| (cluster, leaf)))
        .collect::<HashMap<_, _>>();
    let end = |member: NodeIndex| anchors.get(&member).copied().unwrap_or(member);
    let id = |index: NodeIndex| index.index().to_string();
    let name = |index: EdgeIndex| index.index().to_string();
    let mut layered = Graph::<NodeLabel, EdgeLabel, GraphLabel>::new(GraphOptions { multigraph: true, compound: true, directed: true });
    layered.set_graph(GraphLabel {
        rankdir,
        nodesep: nodesep.into_inner(),
        ranksep: ranksep.into_inner(),
        edgesep: edgesep.into_inner(),
        marginx: marginx.into_inner(),
        marginy: marginy.into_inner(),
        align: align.as_ref().map(ToString::to_string),
        ranker: Some(ranker.to_string()),
        acyclicer: Some(acyclicer.to_string()),
        ..GraphLabel::default()
    });
    for &(index, Node { extent, parent: _ }) in &specs {
        let Size { width, height } = extent.into_inner();
        layered.set_node(id(index), NodeLabel { width, height, ..NodeLabel::default() });
    }
    for &(child, parent) in &links {
        layered.set_parent(id(child), id(parent));
    }
    for &(index, source, target, Edge { label, minlen, weight }, _) in edges.iter().filter(|&&(_, _, _, _, ranked)| ranked) {
        let boxed = label.map_or_else(EdgeLabel::default, |Label { extent, labelpos, labeloffset }| {
            let Size { width, height } = extent.into_inner();
            EdgeLabel { width, height, labelpos, labeloffset: labeloffset.into_inner(), ..EdgeLabel::default() }
        });
        layered.set_edge_named(id(end(source)), id(end(target)), Some(name(index)), Some(EdgeLabel { minlen, weight: f64::from(weight), ..boxed }));
    }
    dugong::layout_controlled(&mut layered, work)?;
    let frames = specs
        .iter()
        .map(|&(index, _)| {
            layered
                .node(&id(index))
                .and_then(|placed| placed.x.zip(placed.y).map(|center| (index, Rect::from_center_size(center, (placed.width, placed.height)))))
                .ok_or(LayoutError::Unplaced { node: index })
        })
        .collect::<Result<HashMap<_, _>, _>>()?;
    let bounds = |member: NodeIndex| frames.get(&member).copied();
    let settle = |terminal: Point, frame: Rect, toward: Point| {
        if terminal.is_finite() { terminal } else { Point::new(frame.center().x, if toward.y < frame.center().y { frame.y0 } else { frame.y1 }) }
    };
    let cut = |points: Vec<Point>, frame: Rect| match points.iter().position(|&point| frame.contains(point)).and_then(|inside| points.split_at_checked(inside)) {
        Some((outside @ [.., before], [entered, ..])) => {
            let span = Line::new(*before, *entered);
            let crossing = frame.path_segments(DEFAULT_ACCURACY).flat_map(|side| side.intersect_line(span)).map(|hit| hit.line_t).min_by(f64::total_cmp).map_or(span.p0, |line_t| span.eval(line_t));
            outside.iter().copied().chain(iter::once(crossing)).dedup().collect()
        }
        Some(([], _) | (_, [])) | None => points,
    };
    let ranked = |index: EdgeIndex, source: NodeIndex, target: NodeIndex, extent: Option<Extent>| {
        let (tail, head) = (end(source), end(target));
        let route = layered.edge(&id(tail), &id(head), Some(name(index).as_str())).filter(|route| !route.points.is_empty())?;
        bounds(tail).zip(bounds(head)).and_then(|(tail_frame, head_frame)| {
            let points = route.points.iter().map(|&dugong::Point { x, y }| Point::new(x, y));
            let mut interior = points.clone().dropping(1).dropping_back(1);
            let (toward_tail, toward_head) = interior.next().zip(interior.next_back()).unwrap_or_else(|| (head_frame.center(), tail_frame.center()));
            let settled = points
                .with_position()
                .map(|(Position { is_first, is_last }, point)| match (is_first, is_last) {
                    (true, _) => settle(point, tail_frame, toward_tail),
                    (false, true) => settle(point, head_frame, toward_head),
                    (false, false) => point,
                })
                .collect::<Vec<_>>();
            let leaving = if source == tail { settled } else { cut(settled.into_iter().rev().collect(), bounds(source)?).into_iter().rev().collect() };
            let arriving = if target == head { leaving } else { cut(leaving, bounds(target)?) };
            Some((
                arriving.into_iter().with_position().map(|(Position { is_first, is_last: _ }, point)| if is_first { PathEl::MoveTo(point) } else { PathEl::LineTo(point) }).collect::<BezPath>(),
                extent.zip(route.x.zip(route.y)).map(|(size, center)| Rect::from_center_size(center, size.into_inner())),
            ))
        })
    };
    let beside = |rect: Rect, Label { extent: _, labelpos, labeloffset }: Label| {
        let reach = labeloffset.into_inner() + rect.width() / 2.0;
        Affine::translate((
            match labelpos {
                LabelPos::C => 0.0,
                LabelPos::L => -reach,
                LabelPos::R => reach,
            },
            0.0,
        ))
        .transform_rect_bbox(rect)
    };
    let courses = fanned(edgesep, edges.iter().filter(|&&(_, _, _, _, ranked_edge)| !ranked_edge).map(|&(index, source, target, _, _)| (index, source, target)));
    let mut routes = edges
        .iter()
        .map(|&(index, source, target, Edge { label, minlen: _, weight: _ }, ranked_edge)| {
            let extent = label.map(|Label { extent, labelpos: _, labeloffset: _ }| extent);
            let route = if ranked_edge {
                ranked(index, source, target, extent)
            } else {
                bounds(source).zip(bounds(target)).zip(courses.get(&index)).map(|((source_frame, target_frame), &course)| {
                    let (path, boxed) = direct((source_frame, None), (target_frame, None), course, extent);
                    (path, boxed.zip(label.filter(|_| source != target)).map(|(rect, spec)| beside(rect, spec)).or(boxed))
                })
            };
            route.map(|route| (index, route)).ok_or(LayoutError::Unrouted { edge: index })
        })
        .collect::<Result<HashMap<_, _>, LayoutError>>()?;
    Ok(graph.filter_map_owned(|index, weight| frames.get(&index).map(|&frame| Placed { weight, frame }), |index, weight| routes.remove(&index).map(|(path, label)| Routed { weight, path, label })))
}
