use core::iter;
use std::collections::{BTreeMap, HashMap};

use dugong::{
    EdgeLabel, GraphLabel, LabelPos, NodeLabel, RankDir, WorkControl,
    graphlib::{Graph, GraphOptions},
};
use itertools::Itertools as _;
use kurbo::{Point, Rect, Size, Vec2};
use petgraph::{
    EdgeType,
    graph::GraphIndex,
    stable_graph::{EdgeIndex, NodeIndex, StableGraph},
    visit::NodeIndexable as _,
};

use crate::{
    CurveStyle, Distance, Drawing, Extent, Hierarchy, Interpolation, LayoutError, Node, clipped, drawn, hierarchy, is_cluster, linked, nesting, non_cluster, offsets, outline, path, route, titled,
};

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
    pub rankdir: Option<RankDir> = None,
    pub nodesep: Option<Distance> = None,
    pub ranksep: Option<Distance> = None,
    pub edgesep: Option<Distance> = None,
    pub marginx: Option<Distance> = None,
    pub marginy: Option<Distance> = None,
    pub ranker: Option<Ranker> = None,
    pub acyclicer: Option<Acyclicer> = None,
    pub align: Option<Align> = None,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Label {
    pub extent: Extent,
    pub labelpos: Option<LabelPos> = None,
    pub labeloffset: Option<Distance> = None,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Edge {
    pub label: Option<Label> = None,
    pub minlen: usize = 1,
    pub weight: u32 = 1,
    pub interpolation: Interpolation = Interpolation::Basis,
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
    fn id(index: impl GraphIndex) -> String { index.index().to_string() }
    let Options { rankdir, nodesep, ranksep, edgesep, marginx, marginy, ranker, acyclicer, align } = *options;
    let Hierarchy { nodes, edges, parents, titles } = hierarchy(&graph, node, edge, iter::empty(), iter::empty())?;
    let (ranked, direct): (Vec<_>, Vec<_>) = edges.into_iter().partition(|&(_, source, target, _)| linked(&parents, source, target));
    let non_cluster = non_cluster(&graph, &parents, ranked.iter().flat_map(|&(_, source, target, _)| [source, target]));
    let title_ids = iter::zip(titles.keys(), graph.node_bound()..).map(|(&cluster, title)| (cluster, title.to_string())).collect::<BTreeMap<_, _>>();
    let mut layered = Graph::<NodeLabel, EdgeLabel, GraphLabel>::new(GraphOptions { multigraph: true, compound: true, directed: true });
    let defaults = GraphLabel::default();
    layered.set_graph(GraphLabel {
        rankdir: rankdir.unwrap_or(defaults.rankdir),
        nodesep: nodesep.map_or(defaults.nodesep, Distance::into_inner),
        ranksep: ranksep.map_or(defaults.ranksep, Distance::into_inner),
        edgesep: edgesep.map_or(defaults.edgesep, Distance::into_inner),
        marginx: marginx.map_or(defaults.marginx, Distance::into_inner),
        marginy: marginy.map_or(defaults.marginy, Distance::into_inner),
        align: align.as_ref().map(ToString::to_string),
        ranker: ranker.as_ref().map(ToString::to_string),
        acyclicer: acyclicer.as_ref().map(ToString::to_string),
        ..defaults
    });
    for (&index, &Node { extent, parent: _, shape: _ }) in &nodes {
        let Size { width, height } = if is_cluster(&parents, index) { Size::ZERO } else { extent.into_inner() };
        layered.set_node(id(index), NodeLabel { width, height, ..NodeLabel::default() });
    }
    for (Size { width, height }, title) in iter::zip(titles.values(), title_ids.values()).map(|(extent, title)| (extent.into_inner(), title)) {
        layered.set_node(title.as_str(), NodeLabel { width, height, ..NodeLabel::default() });
    }
    let assignments = parents
        .all_edges()
        .map(|(child, parent, ())| (id(child), id(parent)))
        .chain(title_ids.iter().map(|(&cluster, title)| (title.clone(), id(cluster))))
        .filter_map(|(child, parent)| layered.node_ix(&child).zip(layered.node_ix(&parent)))
        .collect::<Vec<_>>();
    layered.try_set_unparented_parents_ix(&assignments)?;
    for (title, member) in
        titled(&nodes, &parents, &titles).filter_map(|((cluster, _), (member, _))| title_ids.get(&cluster).map(|title| (title, title_ids.get(&member).cloned().unwrap_or_else(|| id(member)))))
    {
        layered.set_edge_with_label(title.as_str(), member, EdgeLabel { weight: 1.0, ..EdgeLabel::default() });
    }
    let edge_label = |Label { extent, labelpos, labeloffset }: Label| {
        let (Size { width, height }, edge_defaults) = (extent.into_inner(), EdgeLabel::default());
        EdgeLabel { width, height, labelpos: labelpos.unwrap_or(edge_defaults.labelpos), labeloffset: labeloffset.map_or(edge_defaults.labeloffset, Distance::into_inner), ..edge_defaults }
    };
    for &(index, source, target, Edge { label, minlen, weight, interpolation: _ }) in &ranked {
        layered.set_edge_named(id(non_cluster(source)), id(non_cluster(target)), Some(id(index)), Some(EdgeLabel { minlen, weight: f64::from(weight), ..label.map_or_default(edge_label) }));
    }
    dugong::layout_controlled(&mut layered, work)?;
    let canvas = Size::new(layered.graph().width, layered.graph().height).to_rect();
    let placed = |name: &str| layered.node(name).and_then(|label| label.x.zip(label.y).map(|center| Rect::from_center_size(center, (label.width, label.height))));
    let frames = nodes.keys().filter_map(|&index| placed(&id(index)).map(|frame| (index, (frame, title_ids.get(&index).and_then(|title| placed(title)))))).collect::<HashMap<_, _>>();
    let shaped = |member: NodeIndex| frames.get(&member).zip(nodes.get(&member)).map(|(&(frame, _), &Node { extent: _, parent: _, shape })| (frame, shape));
    let beside = |rect: Rect, label: &EdgeLabel| {
        let shift = match layered.graph().rankdir {
            RankDir::TB | RankDir::BT => Vec2::new(label.labeloffset + rect.width() / 2.0, 0.0),
            RankDir::LR | RankDir::RL => Vec2::new(0.0, label.labeloffset + rect.height() / 2.0),
        };
        rect + match label.labelpos {
            LabelPos::C => Vec2::ZERO,
            LabelPos::L => -shift,
            LabelPos::R => shift,
        }
    };
    let spread = offsets(layered.graph().edgesep, &direct);
    let routes = ranked
        .iter()
        .filter_map(|&(index, source, target, Edge { label, minlen: _, weight: _, interpolation })| {
            let (tail, head) = (non_cluster(source), non_cluster(target));
            let laid = layered.edge(&id(tail), &id(head), Some(id(index).as_str()))?;
            let (((source_frame, source_shape), (target_frame, target_shape)), ((tail_frame, _), (head_frame, _))) = shaped(source).zip(shaped(target)).zip(shaped(tail).zip(shaped(head)))?;
            let bends = laid.points.iter().map(|&dugong::Point { x, y }| Point::new(x, y)).dropping(1).dropping_back(1);
            let points = iter::once(tail_frame.center()).chain(bends).chain(iter::once(head_frame.center())).collect::<Vec<_>>();
            Some((
                index,
                (
                    path(interpolation, &clipped(&points, &outline(source_shape, source_frame), &outline(target_shape, target_frame))),
                    label.zip(laid.x.zip(laid.y)).map(|(Label { extent, labelpos: _, labeloffset: _ }, center)| Rect::from_center_size(center, extent.into_inner())),
                ),
            ))
        })
        .chain(direct.iter().filter_map(|&(index, source, target, Edge { label, minlen: _, weight: _, interpolation })| {
            let ((source_frame, source_shape), (target_frame, target_shape)) = shaped(source).zip(shaped(target))?;
            let extent = label.map(|Label { extent, labelpos: _, labeloffset: _ }| extent);
            let (path, rect) =
                route((source_frame, source_shape, None), (target_frame, target_shape, None), (CurveStyle::Straight, *spread.get(&index)?), nesting(&parents, source, target), interpolation, extent);
            Some((
                index,
                (
                    path,
                    match (rect, label) {
                        (Some(rect), Some(label)) if source != target => Some(beside(rect, &edge_label(label))),
                        (rect, _) => rect,
                    },
                ),
            ))
        }))
        .collect::<HashMap<_, _>>();
    drawn(graph, &nodes, frames, routes, Some(canvas))
}
