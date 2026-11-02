use core::{
    cell::Cell,
    f64::consts::{FRAC_PI_2, PI},
};
use std::collections::HashMap;

use indexmap::IndexMap;
use nalgebra::{SVector, Unit};
use petgraph::{
    Direction::{self, Incoming, Outgoing},
    EdgeType,
    algo::astar,
    data::{Element, FromElements as _},
    stable_graph::{EdgeIndex, EdgeReference, NodeIndex, StableGraph},
    visit::{Control, EdgeRef as _, IntoEdgeReferences as _, IntoEdges, Reversed, ReversedEdgeReference, Visitable},
};
use rustworkx_core::traversal::{DijkstraEvent, dijkstra_search};

use crate::{NetworkError, Tolerance, Weight, present};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Reached {
    pub distance: f64,
    pub extent: f64,
    pub paths: f64,
    pub predecessors: Vec<EdgeIndex>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Route {
    pub distance: f64,
    pub steps: Vec<(EdgeIndex, NodeIndex)>,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Catchment {
    pub radius: Option<Weight> = None,
    pub tolerance: Option<Tolerance> = None,
    pub direction: Direction = Outgoing,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn catchment<N, E, Ty>(
    graph: &StableGraph<N, E, Ty>,
    source: NodeIndex,
    cost: impl Fn(EdgeReference<'_, E>) -> Weight,
    extent: impl Fn(EdgeReference<'_, E>, Weight) -> Weight,
    catchment: Catchment,
) -> Result<IndexMap<NodeIndex, Reached>, NetworkError>
where
    Ty: EdgeType,
{
    present(graph, source).ok().map_err(NetworkError::Refused).map(|source| search(graph, source, cost, extent, catchment))
}

pub(crate) fn search<N, E, Ty>(
    graph: &StableGraph<N, E, Ty>,
    source: NodeIndex,
    cost: impl Fn(EdgeReference<'_, E>) -> Weight,
    extent: impl Fn(EdgeReference<'_, E>, Weight) -> Weight,
    catchment: Catchment,
) -> IndexMap<NodeIndex, Reached>
where
    Ty: EdgeType,
{
    fn settle<'graph, G, E>(
        graph: G,
        source: NodeIndex,
        travel: impl Fn(G::EdgeRef) -> EdgeReference<'graph, E>,
        cost: impl Fn(EdgeReference<'graph, E>) -> Weight,
        extent: impl Fn(EdgeReference<'graph, E>, Weight) -> Weight,
        Catchment { radius, tolerance, direction: _ }: Catchment,
    ) -> IndexMap<NodeIndex, Reached>
    where
        G: IntoEdges<NodeId = NodeIndex> + Visitable,
        E: 'graph,
    {
        let tolerance = tolerance.map_or_default(Tolerance::into_inner);
        let leg = Cell::new((EdgeIndex::end(), 0.0, 0.0));
        let origin = Reached { distance: 0.0, extent: 0.0, paths: 1.0, predecessors: Vec::new() };
        let mut current = (origin.distance, origin.extent, origin.paths);
        let mut settled = IndexMap::from([(source, origin)]);
        let mut frontier = HashMap::<NodeIndex, Vec<(EdgeIndex, f64, f64, f64)>>::new();
        dijkstra_search(
            graph,
            [source],
            |edge| {
                let edge = travel(edge);
                let spent = cost(edge);
                let step = (edge.id(), spent.into_inner(), extent(edge, spent).into_inner());
                leg.set(step);
                Ok::<f64, !>(step.1)
            },
            |event| match event {
                DijkstraEvent::Discover(node, distance) => {
                    let tied = frontier.remove(&node).into_iter().flatten().filter(|&(_, arrival, _, _)| arrival - distance <= tolerance * distance);
                    let reached = tied.fold(None, |reached, (edge, _, spanned, paths)| {
                        Some(match reached {
                            None => Reached { distance, extent: spanned, paths, predecessors: vec![edge] },
                            Some(Reached { distance: _, extent: least, paths: counted, mut predecessors }) => {
                                predecessors.push(edge);
                                Reached { distance, extent: least.min(spanned), paths: counted + paths, predecessors }
                            }
                        })
                    });
                    if let Some(reached) = reached {
                        current = (reached.distance, reached.extent, reached.paths);
                        settled.insert(node, reached);
                    }
                    Control::<()>::Continue
                }
                DijkstraEvent::EdgeRelaxed(_, to, _) | DijkstraEvent::EdgeNotRelaxed(_, to, _) => {
                    let ((edge, step, span), (base, spent, paths)) = (leg.get(), current);
                    let (arrival, spanned) = (base + step, spent + span);
                    if radius.is_some_and(|bound| spanned > bound.into_inner()) {
                        Control::Prune
                    } else {
                        frontier.entry(to).or_default().push((edge, arrival, spanned, paths));
                        Control::Continue
                    }
                }
                DijkstraEvent::ExamineEdge(..) | DijkstraEvent::Finish(_) => Control::Continue,
            },
        )
        .into_ok();
        settled
    }
    match catchment.direction {
        Outgoing => settle(graph, source, |edge| edge, cost, extent, catchment),
        Incoming => settle(Reversed(graph), source, ReversedEdgeReference::into_unreversed, cost, extent, catchment),
    }
}

pub fn route<N, E, Ty>(
    graph: &StableGraph<N, E, Ty>,
    source: NodeIndex,
    goal: impl Fn(NodeIndex) -> bool,
    cost: impl Fn(EdgeReference<'_, E>) -> Weight,
    estimate: impl Fn(NodeIndex) -> Weight,
) -> Result<Option<Route>, NetworkError>
where
    Ty: EdgeType,
{
    present(graph, source).ok().map_err(NetworkError::Refused).map(|source| {
        astar(graph, source, goal, |edge| cost(edge).into_inner(), |node| estimate(node).into_inner()).and_then(|(distance, nodes)| {
            nodes
                .array_windows()
                .map(|&[from, to]| graph.edges_connecting(from, to).min_by_key(|&edge| cost(edge)).map(|edge| (edge.id(), to)))
                .try_collect::<Vec<_>>()
                .map(|steps| Route { distance, steps })
        })
    })
}

pub fn segments<N, E, Ty, const D: usize>(
    graph: &StableGraph<N, E, Ty>,
    tangents: impl Fn(EdgeReference<'_, E>) -> [Unit<SVector<f64, D>>; 2],
) -> Result<StableGraph<EdgeIndex, Weight, Ty>, NetworkError>
where
    Ty: EdgeType,
{
    let mut ends = graph
        .edge_references()
        .enumerate()
        .flat_map(|(line, edge)| {
            let [start, end] = tangents(edge);
            [(edge.source(), Outgoing, line, edge.id(), start), (edge.target(), Incoming, line, edge.id(), -end)]
        })
        .collect::<Vec<_>>();
    ends.sort_by_key(|&(node, ..)| node);
    let turns = ends
        .chunk_by(|left, right| left.0 == right.0)
        .flat_map(|meeting| {
            meeting.iter().enumerate().flat_map(move |(order, &(node, arrival, from, behind, leaving))| {
                meeting
                    .iter()
                    .enumerate()
                    .filter(move |&(other, &(_, departure, _, ahead, _))| behind != ahead && if Ty::is_directed() { (arrival, departure) == (Incoming, Outgoing) } else { order < other })
                    .map(move |(_, &(_, _, to, ahead, entering))| {
                        Weight::try_new((PI - leaving.angle(&*entering)) / FRAC_PI_2).map(|weight| Element::Edge { source: from, target: to, weight }).map_err(|source| NetworkError::Turn {
                            edges: [behind, ahead],
                            node,
                            source,
                        })
                    })
            })
        })
        .try_collect::<Vec<_>>()?;
    Ok(StableGraph::from_elements(graph.edge_indices().map(|weight| Element::Node { weight }).chain(turns)))
}
