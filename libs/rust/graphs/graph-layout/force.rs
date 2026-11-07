use core::{cell::OnceCell, iter, num::NonZero};
use std::collections::HashMap;

use itertools::{Either, Itertools as _};
use kurbo::{Axis, Point, Rect, Size};
use manatee::{
    Anchor, BoundsExtras, FcoseRandomPolicy, FcoseRandomSource, LayoutRect, WorkControl,
    algo::fcose::{
        IndexedAlignmentConstraint, IndexedCompound, IndexedEdge, IndexedFcoseOptions, IndexedGraph, IndexedLayoutResult, IndexedNode, IndexedRelativePlacementConstraint,
        layout_indexed_with_random_policy_and_work_control,
    },
};
use nonempty_collections::{NEBTreeMap, NonEmptyIterator as _};
use petgraph::{
    EdgeType,
    stable_graph::{EdgeIndex, NodeIndex, StableGraph},
};

use crate::{CurveStyle, Distance, Drawing, Extent, Hierarchy, Interpolation, LayoutError, Node, ancestors, drawn, hierarchy, is_cluster, linked, nesting, non_cluster, offsets, route, titled};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(const_fn, validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd, Into))]
pub struct Length(f64);

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd, Into))]
pub struct Elasticity(f64);

#[derive(Debug, Clone, PartialEq)]
pub enum Start {
    Spectral { node_separation: Option<Distance> },
    Centers(NEBTreeMap<NodeIndex, Point>),
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Alignment {
    pub axis: Axis,
    pub members: Vec<NodeIndex>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Placement {
    pub axis: Axis,
    pub first: NodeIndex,
    pub second: NodeIndex,
    pub gap: Distance,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Options {
    pub random_policy: FcoseRandomPolicy = FcoseRandomPolicy::seeded(FcoseRandomSource::XorShift64Star, 0),
    pub start: Start = Start::Spectral { node_separation: None },
    pub num_iter: Option<NonZero<usize>> = None,
    pub default_edge_length: Length = Self::EDGE_LENGTH,
    pub compound_padding: Option<Distance> = None,
    pub control_point_step_size: Distance = Self::STEP_SIZE,
    pub relocate_center: Option<Point> = None,
    pub alignment_constraint: Vec<Alignment> = Vec::new(),
    pub relative_placement_constraint: Vec<Placement> = Vec::new(),
}

impl Options {
    const EDGE_LENGTH: Length = Length::try_new(50.0).ok().unwrap();
    const STEP_SIZE: Distance = Distance::try_new(40.0).ok().unwrap();
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Edge {
    pub label: Option<Extent> = None,
    pub ideal_length: Option<Length> = None,
    pub elasticity: Option<Elasticity> = None,
    pub source_anchor: Option<Anchor> = None,
    pub target_anchor: Option<Anchor> = None,
    pub curve_style: CurveStyle = CurveStyle::Straight,
    pub interpolation: Interpolation = Interpolation::Linear,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn layout<N, E, Ty, W>(
    options: &Options,
    work: &mut W,
    node: impl Fn(NodeIndex, &N) -> Node,
    edge: impl Fn(EdgeIndex, &E) -> Edge,
    graph: StableGraph<N, E, Ty>,
) -> Result<Drawing<N, E, Ty>, LayoutError>
where
    Ty: EdgeType,
    W: WorkControl + ?Sized,
{
    let constrained = options
        .alignment_constraint
        .iter()
        .flat_map(|Alignment { axis: _, members }| members.iter().copied())
        .chain(options.relative_placement_constraint.iter().flat_map(|&Placement { axis: _, first, second, gap: _ }| [first, second]));
    let named = match &options.start {
        Start::Spectral { node_separation: _ } => Either::Left(iter::empty()),
        Start::Centers(starts) => Either::Right(starts.keys().into_iter().copied()),
    };
    let Hierarchy { nodes, edges, parents, titles } = hierarchy(&graph, node, edge, named, constrained)?;
    let bounds = |center: Point| Rect::from_points(center, center);
    let start_bounds = match &options.start {
        Start::Spectral { node_separation: _ } => HashMap::new(),
        Start::Centers(starts) => starts
            .into_iter()
            .flat_map(|(&index, &center)| iter::once(index).chain(ancestors(&parents, index)).map(move |owner| (owner, bounds(center))))
            .into_grouping_map()
            .reduce(|whole, _, rect| whole.union(rect)),
    };
    let global_start_bounds = OnceCell::new();
    let start_center = |index: NodeIndex| match &options.start {
        Start::Spectral { node_separation: _ } => Point::ORIGIN,
        Start::Centers(starts) => start_bounds
            .get(&index)
            .copied()
            .or_else(|| {
                graph
                    .neighbors_undirected(index)
                    .filter(|&neighbor| parents.neighbors(neighbor).next() == parents.neighbors(index).next())
                    .filter_map(|neighbor| starts.get(&neighbor).copied().map(bounds))
                    .reduce(|whole, rect| whole.union(rect))
            })
            .or_else(|| ancestors(&parents, index).find_map(|cluster| start_bounds.get(&cluster).copied()))
            .unwrap_or_else(|| *global_start_bounds.get_or_init(|| starts.values().copied().map(bounds).reduce(|whole, rect| whole.union(rect))))
            .center(),
    };
    let (leaves, compounds): (Vec<_>, Vec<_>) = nodes.iter().map(|(&index, &record)| (index, record)).partition(|&(index, _)| !is_cluster(&parents, index));
    let leaf_positions = leaves.iter().enumerate().map(|(position, &(index, _))| (index, position)).collect::<HashMap<_, _>>();
    let title_positions = iter::zip(titles.keys().copied(), leaves.len()..).collect::<HashMap<_, _>>();
    let compound_positions = compounds.iter().enumerate().map(|(position, &(index, _))| (index, position)).collect::<HashMap<_, _>>();
    let leaf = |index: NodeIndex| leaf_positions.get(&index).copied();
    let compound = |index: NodeIndex| compound_positions.get(&index).copied();
    let springs = edges.iter().filter(|&&(_, source, target, _)| linked(&parents, source, target)).collect::<Vec<_>>();
    let non_cluster = non_cluster(&graph, &parents, springs.iter().flat_map(|&&(_, source, target, _)| [source, target]));
    let indexed =
        |Size { width, height }: Size, parent: Option<NodeIndex>, Point { x, y }: Point| IndexedNode { parent: parent.and_then(compound), width, height, x, y, bounds_extras: BoundsExtras::default() };
    let indexed_graph = IndexedGraph {
        nodes: leaves
            .iter()
            .map(|&(index, Node { extent, parent, shape: _ })| indexed(extent.into_inner(), parent, start_center(index)))
            .chain(titles.iter().map(|(&cluster, &extent)| indexed(extent.into_inner(), Some(cluster), start_center(cluster))))
            .collect(),
        edges: springs
            .iter()
            .filter_map(|&&(_, source, target, Edge { label, ideal_length, elasticity, source_anchor, target_anchor, curve_style, interpolation: _ })| {
                let (label_width, label_height) = label.map(Extent::into_inner).map(|Size { width, height }| (width, height)).unzip();
                Some(IndexedEdge {
                    source: leaf(non_cluster(source))?,
                    target: leaf(non_cluster(target))?,
                    label_width,
                    label_height,
                    source_anchor,
                    target_anchor,
                    curve_style_segments: curve_style.is_segments(),
                    ideal_length: ideal_length.unwrap_or(options.default_edge_length).into_inner(),
                    elasticity: elasticity.map_or_default(Elasticity::into_inner),
                })
            })
            .collect(),
        compounds: compounds.iter().map(|&(_, Node { extent: _, parent, shape: _ })| IndexedCompound { parent: parent.and_then(compound) }).collect(),
    };
    let (horizontal, vertical): (Vec<_>, Vec<_>) = options.alignment_constraint.iter().partition_map(|Alignment { axis, members }| {
        let aligned = members.iter().copied().filter_map(leaf).collect::<Vec<_>>();
        match axis {
            Axis::Horizontal => Either::Left(aligned),
            Axis::Vertical => Either::Right(aligned),
        }
    });
    let relative_placement_constraint = options
        .relative_placement_constraint
        .iter()
        .filter_map(|&Placement { axis, first, second, gap }| {
            let (preceding, following) = leaf(first).zip(leaf(second))?;
            let ((left, right), (top, bottom)) = match axis {
                Axis::Horizontal => ((Some(preceding), Some(following)), (None, None)),
                Axis::Vertical => ((None, None), (Some(preceding), Some(following))),
            };
            Some(IndexedRelativePlacementConstraint { left, right, top, bottom, gap: gap.into_inner() })
        })
        .chain(titled(&nodes, &parents, &titles).filter_map(|((cluster, title), (member, extent))| {
            Some(IndexedRelativePlacementConstraint {
                left: None,
                right: None,
                top: Some(*title_positions.get(&cluster)?),
                bottom: Some(title_positions.get(&member).copied().or_else(|| leaf(member))?),
                gap: f64::midpoint(title.into_inner().height, extent.into_inner().height),
            })
        }))
        .collect();
    let indexed_opts = IndexedFcoseOptions {
        randomize: matches!(options.start, Start::Spectral { node_separation: _ }),
        node_separation: match &options.start {
            Start::Spectral { node_separation } => node_separation.map(Distance::into_inner),
            Start::Centers(_) => None,
        },
        num_iter: options.num_iter.map(NonZero::get),
        default_edge_length: Some(options.default_edge_length.into_inner()),
        alignment_constraint: Some(IndexedAlignmentConstraint { horizontal, vertical }),
        relative_placement_constraint,
        compound_padding: options.compound_padding.map(Distance::into_inner),
        relocate_center: options.relocate_center.map(Into::into),
        ..IndexedFcoseOptions::default()
    };
    let IndexedLayoutResult { node_positions, compound_positions: _, compound_bounds } =
        layout_indexed_with_random_policy_and_work_control(&indexed_graph, &indexed_opts, options.random_policy, work)?;
    let title_frames = iter::zip(&titles, node_positions.iter().skip(leaves.len()))
        .map(|((&cluster, &extent), &manatee::Point { x, y })| (cluster, Rect::from_center_size((x, y), extent.into_inner())))
        .collect::<HashMap<_, _>>();
    let frames = iter::chain(
        iter::zip(&leaves, &node_positions).map(|(&(index, Node { extent, parent: _, shape: _ }), &manatee::Point { x, y })| (index, (Rect::from_center_size((x, y), extent.into_inner()), None))),
        iter::zip(&compounds, compound_bounds)
            .map(|(&(index, _), LayoutRect { left, top, width, height })| (index, (Rect::from_origin_size((left, top), (width, height)), title_frames.get(&index).copied()))),
    )
    .collect::<HashMap<_, _>>();
    let spread = offsets(options.control_point_step_size.into_inner(), &edges);
    let end = |member: NodeIndex, anchor: Option<Anchor>| frames.get(&member).zip(nodes.get(&member)).map(|(&(frame, _), &Node { extent: _, parent: _, shape })| (frame, shape, anchor));
    let routes = edges
        .iter()
        .filter_map(|&(index, source, target, Edge { label, ideal_length: _, elasticity: _, source_anchor, target_anchor, curve_style, interpolation })| {
            Some((index, route(end(source, source_anchor)?, end(target, target_anchor)?, (curve_style, *spread.get(&index)?), nesting(&parents, source, target), interpolation, label)))
        })
        .collect::<HashMap<_, _>>();
    drawn(graph, &nodes, frames, routes, None)
}
