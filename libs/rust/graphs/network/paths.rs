use core::f64::consts::{FRAC_PI_2, PI};
use std::collections::{HashMap, hash_map::Entry};

use indexmap::IndexMap;
use nalgebra::{SVector, Unit};
use petgraph::{
    Direction::{self, Incoming, Outgoing},
    EdgeType,
    algo::astar,
    data::{Element, FromElements as _},
    stable_graph::{EdgeIndex, EdgeReference, NodeIndex, StableGraph},
    visit::{Control, EdgeRef as _, IntoEdgeReferences as _, IntoEdges, Reversed, Visitable},
};
use rustworkx_core::traversal::{DijkstraEvent, dijkstra_search};

use crate::{NetworkError, Weight, present};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Reached {
    pub distance: f64,
    pub paths: f64,
    pub predecessors: Vec<NodeIndex>,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Catchment {
    pub radius: Option<Weight> = None,
    pub tolerance: Option<Weight> = None,
    pub direction: Direction = Outgoing,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn reach<N, E, Ty>(graph: &StableGraph<N, E, Ty>, source: NodeIndex, cost: impl Fn(&E) -> Weight, catchment: Catchment) -> Result<IndexMap<NodeIndex, Reached>, NetworkError>
where Ty: EdgeType {
    present(graph, source).map(|source| search(graph, source, cost, catchment))
}

pub(crate) fn search<N, E, Ty>(graph: &StableGraph<N, E, Ty>, source: NodeIndex, cost: impl Fn(&E) -> Weight, catchment: Catchment) -> IndexMap<NodeIndex, Reached>
where Ty: EdgeType {
    fn settle<G>(graph: G, source: NodeIndex, cost: impl Fn(&G::EdgeWeight) -> Weight, Catchment { radius, tolerance, direction: _ }: Catchment) -> IndexMap<NodeIndex, Reached>
    where G: IntoEdges<NodeId = NodeIndex> + Visitable {
        let tied = |left: f64, right: f64| (left - right).abs() <= tolerance.map_or_default(Weight::into_inner);
        let mut settled = IndexMap::<NodeIndex, Reached>::new();
        let mut frontier = HashMap::from([(source, Reached { distance: 0.0, paths: 1.0, predecessors: Vec::new() })]);
        dijkstra_search(
            graph,
            [source],
            |edge| Ok::<f64, !>(cost(edge.weight()).into_inner()),
            |event| match event {
                DijkstraEvent::Discover(node, _) => {
                    settled.extend(frontier.remove_entry(&node));
                    Control::<()>::Continue
                }
                DijkstraEvent::EdgeRelaxed(from, to, weight) | DijkstraEvent::EdgeNotRelaxed(from, to, weight) if let Some(&Reached { distance, paths, predecessors: _ }) = settled.get(&from) =>
                    match (distance + cost(weight).into_inner(), event, frontier.entry(to)) {
                        (next, ..) if radius.is_some_and(|bound| next > bound.into_inner()) => Control::Prune,
                        (next, _, Entry::Occupied(mut held)) if tied(next, held.get().distance) => {
                            let Reached { distance: best, paths: count, predecessors } = held.get_mut();
                            *best = best.min(next);
                            *count += paths;
                            predecessors.push(from);
                            Control::Continue
                        }
                        (next, DijkstraEvent::EdgeRelaxed(..), entry) => {
                            entry.insert_entry(Reached { distance: next, paths, predecessors: vec![from] });
                            Control::Continue
                        }
                        (_, DijkstraEvent::EdgeNotRelaxed(..) | DijkstraEvent::Discover(..) | DijkstraEvent::ExamineEdge(..) | DijkstraEvent::Finish(_), _) => Control::Continue,
                    },
                DijkstraEvent::EdgeRelaxed(..) | DijkstraEvent::EdgeNotRelaxed(..) | DijkstraEvent::ExamineEdge(..) | DijkstraEvent::Finish(_) => Control::Continue,
            },
        )
        .into_ok();
        settled
    }
    match catchment.direction {
        Outgoing => settle(graph, source, cost, catchment),
        Incoming => settle(Reversed(graph), source, cost, catchment),
    }
}

pub fn route<N, E, Ty>(
    graph: &StableGraph<N, E, Ty>,
    source: NodeIndex,
    goal: impl Fn(NodeIndex) -> bool,
    cost: impl Fn(&E) -> Weight,
    estimate: impl Fn(NodeIndex) -> Weight,
) -> Result<Option<(f64, Vec<NodeIndex>)>, NetworkError>
where
    Ty: EdgeType,
{
    present(graph, source).map(|source| astar(graph, source, goal, |edge| cost(edge.weight()).into_inner(), |node| estimate(node).into_inner()))
}

pub fn angular<N, E, Ty, const D: usize>(
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
