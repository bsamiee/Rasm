use core::cell::Cell;
use core::f64::consts::{FRAC_PI_2, PI};
use core::hash::Hash;
use std::collections::HashMap;

use indexmap::IndexMap;
use nalgebra::{SVector, Unit};
use petgraph::Direction::{self, Incoming, Outgoing};
use petgraph::algo::astar;
use petgraph::data::{Element, FromElements as _};
use petgraph::stable_graph::StableGraph;
use petgraph::visit::{Control, EdgeRef as _, GraphBase, IntoEdgeReferences, IntoEdges, IntoEdgesDirected, Reversed, ReversedEdgeReference, Visitable};
use rustworkx_core::traversal::{DijkstraEvent, dijkstra_search};

use crate::{Failure, Network, NetworkError, Tolerance, Weight, present};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Tree<G> = IndexMap<<G as GraphBase>::NodeId, Reached<<G as IntoEdgeReferences>::EdgeRef>>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Reached<R> {
    pub distance: f64,
    pub extent: f64,
    pub paths: f64,
    pub predecessors: Vec<R>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Route<R> {
    pub distance: f64,
    pub steps: Vec<R>,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Catchment {
    pub radius: Option<Weight> = None,
    pub tolerance: Option<Tolerance> = None,
    pub direction: Direction = Outgoing,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn catchment<G>(graph: G, source: G::NodeId, cost: impl Fn(G::EdgeRef) -> Weight, extent: impl Fn(G::EdgeRef, Weight) -> Weight, catchment: Catchment) -> Result<Tree<G>, Failure<G>>
where G: Network + IntoEdgesDirected {
    present(graph, [source]).ok().map_err(NetworkError::Refused).map(|_| search(graph, source, cost, extent, catchment))
}

pub(crate) fn search<G>(graph: G, source: G::NodeId, cost: impl Fn(G::EdgeRef) -> Weight, extent: impl Fn(G::EdgeRef, Weight) -> Weight, catchment: Catchment) -> Tree<G>
where G: Network + IntoEdgesDirected {
    fn settle<H, R>(
        graph: H,
        source: H::NodeId,
        travel: impl Fn(H::EdgeRef) -> R,
        cost: impl Fn(R) -> Weight,
        extent: impl Fn(R, Weight) -> Weight,
        Catchment { radius, tolerance, direction: _ }: Catchment,
    ) -> IndexMap<H::NodeId, Reached<R>>
    where
        H: IntoEdges + Visitable<NodeId: Hash + Eq>,
        R: Copy,
    {
        let tolerance = tolerance.map_or_default(Tolerance::into_inner);
        let leg = Cell::new(None::<(R, f64, f64)>);
        let origin = Reached { distance: 0.0, extent: 0.0, paths: 1.0, predecessors: Vec::new() };
        let mut current = (origin.distance, origin.extent, origin.paths);
        let mut settled = IndexMap::from([(source, origin)]);
        let mut frontier = HashMap::<H::NodeId, Vec<(R, f64, f64, f64)>>::new();
        dijkstra_search(
            graph,
            [source],
            |edge| {
                let edge = travel(edge);
                let spent = cost(edge);
                leg.set(Some((edge, spent.into_inner(), extent(edge, spent).into_inner())));
                Ok::<f64, !>(spent.into_inner())
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
                    let (base, spent, paths) = current;
                    match leg.take().map(|(edge, step, span)| (edge, base + step, spent + span)) {
                        Some((_, _, spanned)) if radius.is_some_and(|bound| spanned > bound.into_inner()) => Control::Prune,
                        Some((edge, arrival, spanned)) => {
                            frontier.entry(to).or_default().push((edge, arrival, spanned, paths));
                            Control::Continue
                        }
                        None => Control::Continue,
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

pub fn route<G>(
    graph: G,
    source: G::NodeId,
    goal: impl Fn(G::NodeId) -> bool,
    cost: impl Fn(G::EdgeRef) -> Weight,
    estimate: impl Fn(G::NodeId) -> Weight,
) -> Result<Option<Route<G::EdgeRef>>, Failure<G>>
where
    G: Network + IntoEdges,
{
    present(graph, [source]).ok().map_err(NetworkError::Refused).map(|_| {
        astar(graph, source, goal, |edge| cost(edge).into_inner(), |node| estimate(node).into_inner()).and_then(|(distance, nodes)| {
            nodes.array_windows().map(|&[from, to]| graph.edges(from).filter(|edge| edge.target() == to).min_by_key(|&edge| cost(edge))).try_collect::<Vec<_>>().map(|steps| Route { distance, steps })
        })
    })
}

pub fn segments<G, const D: usize>(graph: G, tangents: impl Fn(G::EdgeRef) -> [Unit<SVector<f64, D>>; 2]) -> Result<StableGraph<G::EdgeId, Weight, G::EdgeType>, Failure<G>>
where G: Network {
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
                    .filter(move |&(other, &(_, departure, _, ahead, _))| behind != ahead && if graph.is_directed() { (arrival, departure) == (Incoming, Outgoing) } else { order < other })
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
    Ok(StableGraph::from_elements(graph.edge_references().map(|edge| Element::Node { weight: edge.id() }).chain(turns)))
}
