use core::{iter, num::NonZero};
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
    visit::{EdgeRef as _, IntoEdgeReferences as _},
};

use crate::{Course, Distance, Drawing, Extent, LayoutError, Node, Placed, Routed, direct, fanned, nesting};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum CurveStyle {
    Straight,
    Segments,
}

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
    pub default_edge_length: Length = Length::try_new(50.0).ok().unwrap(),
    pub compound_padding: Distance = Distance::try_new(0.0).ok().unwrap(),
    pub control_point_step_size: Distance = Distance::try_new(40.0).ok().unwrap(),
    pub relocate_center: Option<Point> = None,
    pub alignment_constraint: Vec<Alignment> = Vec::new(),
    pub relative_placement_constraint: Vec<Placement> = Vec::new(),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Edge {
    pub label: Option<Extent> = None,
    pub ideal_length: Option<Length> = None,
    pub elasticity: Option<Elasticity> = None,
    pub source_anchor: Option<Anchor> = None,
    pub target_anchor: Option<Anchor> = None,
    pub curve_style: CurveStyle = CurveStyle::Straight,
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
    let (centers, node_separation) = match &options.start {
        Start::Spectral { node_separation } => (None, *node_separation),
        Start::Centers(centers) => (Some(centers), None),
    };
    let constrained = options
        .alignment_constraint
        .iter()
        .flat_map(|Alignment { axis: _, members }| members.iter().copied())
        .chain(options.relative_placement_constraint.iter().flat_map(|&Placement { axis: _, first, second, gap: _ }| [first, second]));
    let (specs, clusters) = nesting(&graph, node, centers.into_iter().flat_map(NEBTreeMap::keys).copied().chain(constrained.clone()), constrained)?;
    let parents = specs.iter().map(|&(index, Node { extent: _, parent })| (index, parent)).collect::<HashMap<_, _>>();
    let ancestors = |index: NodeIndex| iter::successors(parents.get(&index).copied().flatten(), |cluster| parents.get(cluster).copied().flatten());
    let spot = |center: Point| Rect::from_points(center, center);
    let started = centers
        .into_iter()
        .flatten()
        .flat_map(|(&index, &center)| iter::once(index).chain(ancestors(index)).map(move |owner| (owner, spot(center))))
        .into_grouping_map()
        .reduce(|bounds, _, rect| bounds.union(rect));
    let seed = |starts: &NEBTreeMap<NodeIndex, Point>, index: NodeIndex| {
        starts
            .get(&index)
            .copied()
            .map(spot)
            .or_else(|| {
                graph
                    .neighbors_undirected(index)
                    .filter(|neighbor| parents.get(neighbor) == parents.get(&index))
                    .filter_map(|neighbor| starts.get(&neighbor).copied().map(spot))
                    .reduce(|bounds, rect| bounds.union(rect))
            })
            .or_else(|| ancestors(index).find_map(|cluster| started.get(&cluster).copied()))
            .unwrap_or_else(|| starts.values().copied().map(spot).reduce(|bounds, rect| bounds.union(rect)))
            .center()
    };
    let (leaves, compounds): (Vec<_>, Vec<_>) = specs.into_iter().partition(|(index, _)| !clusters.contains(index));
    let element = iter::chain(&leaves, &compounds).enumerate().map(|(position, &(index, _))| (index, position)).collect::<HashMap<_, _>>();
    let leaf = |index: NodeIndex| element.get(&index).copied().filter(|&position| position < leaves.len());
    let compound = |index: NodeIndex| element.get(&index)?.checked_sub(leaves.len());
    let edges = graph.edge_references().map(|reference| (reference.id(), reference.source(), reference.target(), edge(reference.id(), reference.weight()))).collect::<Vec<_>>();
    let indexed_graph = IndexedGraph {
        nodes: leaves
            .iter()
            .map(|&(index, Node { extent, parent })| {
                let (Size { width, height }, Point { x, y }) = (extent.into_inner(), centers.map_or(Point::ORIGIN, |starts| seed(starts, index)));
                IndexedNode { parent: parent.and_then(compound), width, height, x, y, bounds_extras: BoundsExtras::default() }
            })
            .collect(),
        edges: edges
            .iter()
            .filter_map(|&(_, source, target, Edge { label, ideal_length, elasticity, source_anchor, target_anchor, curve_style })| {
                let (label_width, label_height) = label.map(Extent::into_inner).map(|Size { width, height }| (width, height)).unzip();
                Some(IndexedEdge {
                    source: leaf(source)?,
                    target: leaf(target)?,
                    label_width,
                    label_height,
                    source_anchor,
                    target_anchor,
                    curve_style_segments: match curve_style {
                        CurveStyle::Straight => false,
                        CurveStyle::Segments => true,
                    },
                    ideal_length: ideal_length.unwrap_or(options.default_edge_length).into_inner(),
                    elasticity: elasticity.map_or(0.0, Elasticity::into_inner),
                })
            })
            .collect(),
        compounds: compounds.iter().map(|&(_, Node { extent: _, parent })| IndexedCompound { parent: parent.and_then(compound) }).collect(),
    };
    let (horizontal, vertical): (Vec<_>, Vec<_>) = options.alignment_constraint.iter().partition_map(|Alignment { axis, members }| {
        let indexes = members.iter().copied().filter_map(leaf).collect::<Vec<_>>();
        match axis {
            Axis::Horizontal => Either::Left(indexes),
            Axis::Vertical => Either::Right(indexes),
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
        .collect();
    let indexed_opts = IndexedFcoseOptions {
        randomize: centers.is_none(),
        node_separation: node_separation.map(Distance::into_inner),
        num_iter: options.num_iter.map(NonZero::get),
        default_edge_length: Some(options.default_edge_length.into_inner()),
        alignment_constraint: Some(IndexedAlignmentConstraint { horizontal, vertical }),
        relative_placement_constraint,
        compound_padding: Some(options.compound_padding.into_inner()),
        relocate_center: options.relocate_center.map(Into::into),
        ..IndexedFcoseOptions::default()
    };
    let IndexedLayoutResult { node_positions, compound_positions: _, compound_bounds } =
        layout_indexed_with_random_policy_and_work_control(&indexed_graph, &indexed_opts, options.random_policy, work)?;
    let frames = iter::chain(
        iter::zip(&leaves, node_positions).map(|(&(index, Node { extent, parent: _ }), manatee::Point { x, y })| (index, Rect::from_center_size((x, y), extent.into_inner()))),
        iter::zip(&compounds, compound_bounds).map(|(&(index, _), LayoutRect { left, top, width, height })| (index, Rect::from_origin_size((left, top), (width, height)))),
    )
    .collect::<HashMap<_, _>>();
    let (bent, straight): (Vec<_>, Vec<_>) =
        edges.iter().partition_map(|&(index, source, target, Edge { label: _, ideal_length: _, elasticity: _, source_anchor: _, target_anchor: _, curve_style })| match curve_style {
            CurveStyle::Segments if source != target => Either::Left((index, Course::Bent)),
            CurveStyle::Straight | CurveStyle::Segments => Either::Right((index, source, target)),
        });
    let courses = iter::chain(fanned(options.control_point_step_size, straight), bent).collect::<HashMap<_, _>>();
    let mut routes = edges
        .into_iter()
        .filter_map(|(index, source, target, Edge { label, ideal_length: _, elasticity: _, source_anchor, target_anchor, curve_style: _ })| {
            Some((index, direct((*frames.get(&source)?, source_anchor), (*frames.get(&target)?, target_anchor), *courses.get(&index)?, label)))
        })
        .collect::<HashMap<_, _>>();
    Ok(graph.filter_map_owned(|index, weight| frames.get(&index).map(|&frame| Placed { weight, frame }), |index, weight| routes.remove(&index).map(|(path, label)| Routed { weight, path, label })))
}
