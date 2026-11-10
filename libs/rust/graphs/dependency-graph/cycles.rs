use core::cmp::Reverse;
use core::iter;

use itertools::{Either, Itertools as _};
use nonempty_collections::{IntoIteratorExt as _, IntoNonEmptyIterator as _, NEVec, NonEmptyIterator};
use petgraph::acyclic::AcyclicEdgeError;
use petgraph::algo::{astar, condensation, kosaraju_scc};
use petgraph::stable_graph::{EdgeIndex, NodeIndex, StableDiGraph};
use petgraph::visit::{EdgeRef as _, IntoEdgeReferences as _, NodeFiltered, NodeIndexable as _};
use rustworkx_core::connectivity::johnson_simple_cycles;

use crate::{DependencyError, Edges, Evaluation, acyclic, at, compact, dense, unweighted};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Block<E> {
    pub members: NEVec<NodeIndex>,
    pub torn: Edges<E>,
}

#[derive(Debug, Clone)]
pub struct Torn<N, E> {
    pub evaluation: Evaluation<N, E>,
    pub blocks: Vec<Block<E>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn condensed<N, E>(graph: &StableDiGraph<N, E>) -> Evaluation<NEVec<NodeIndex>, EdgeIndex> {
    let mut components = StableDiGraph::from(condensation(compact(graph), false));
    components.retain_edges(|retained, edge| retained.edge_endpoints(edge).is_some_and(distinct));
    acyclic(components.filter_map_owned(|_, members| NEVec::try_from_vec(members), |_, edge| Some(edge)))
}

pub fn simple<N, E>(graph: &StableDiGraph<N, E>) -> impl Iterator<Item = Vec<NodeIndex>> + use<N, E> {
    let loops = graph.node_indices().filter(|&node| graph.contains_edge(node, node)).collect::<Vec<_>>();
    let unweighted = unweighted(graph, |edge| graph.edge_endpoints(edge).is_some_and(distinct));
    let mut cycles = johnson_simple_cycles(&unweighted, Some(loops).filter(|found| !found.is_empty()));
    iter::from_fn(move || cycles.next(&unweighted))
}

#[must_use]
pub fn tear<N, E, K>(mut graph: StableDiGraph<N, E>, cost: impl Fn(&E) -> K) -> Torn<N, E>
where K: Ord {
    let components = components(&graph);
    let mut kept = acyclic(unweighted(&graph, |_| false));
    let cut = graph
        .edge_references()
        .sorted_by_cached_key(|edge| Reverse(cost(edge.weight())))
        .filter_map(|edge| {
            kept.try_add_edge(edge.source(), edge.target(), ())
                .err()
                .map(|(AcyclicEdgeError::Cycle(_) | AcyclicEdgeError::SelfLoop | AcyclicEdgeError::InvalidEdge)| (edge.id(), (edge.source(), edge.target())))
        })
        .collect::<Vec<_>>();
    let mut torn = cut.into_iter().filter_map(|(edge, (source, target))| Some((at(&components, source)?, (source, target, graph.remove_edge(edge)?)))).into_group_map();
    let evaluation = acyclic(graph);
    let blocks = evaluation
        .nodes_iter()
        .filter_map(|node| Some((at(&components, node)?, node)))
        .sorted_by_key(|&(component, _)| component)
        .chunk_by(|&(component, _)| component)
        .into_iter()
        .filter_map(|(component, members)| {
            Some(Block { members: members.map(|(_, node)| node).try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>)?, torn: torn.remove(&component).unwrap_or_default() })
        })
        .collect();
    Torn { evaluation, blocks }
}

pub(crate) fn cycle<N, E>(graph: &StableDiGraph<N, E>) -> Option<DependencyError> {
    let components = components(graph);
    graph
        .edge_references()
        .filter_map(|edge| at(&components, edge.source()).filter(|&component| at(&components, edge.target()) == Some(component)).map(|component| (component, edge.source(), edge.target())))
        .unique_by(|&(component, _, _)| component)
        .map(|(component, source, target)| path(graph, |node| at(&components, node) == Some(component), target, source))
        .try_into_nonempty_iter()
        .map(NonEmptyIterator::collect::<NEVec<_>>)
        .map(|paths| DependencyError::Cycle { paths })
}

pub(crate) fn refused<N, E>(evaluation: &Evaluation<N, E>, edges: NEVec<(NodeIndex, NodeIndex)>) -> DependencyError {
    DependencyError::Cycle {
        paths: edges
            .into_nonempty_iter()
            .map(|(source, target)| {
                let window = evaluation.get_position(target)..=evaluation.get_position(source);
                path(evaluation, |node| window.contains(&evaluation.get_position(node)), target, source)
            })
            .collect(),
    }
}

fn components<N, E>(graph: &StableDiGraph<N, E>) -> Vec<Option<usize>> {
    dense(graph.node_bound(), kosaraju_scc(graph).into_iter().rev().enumerate().flat_map(|(component, members)| members.into_iter().map(move |node| (node, component))))
}

fn distinct((source, target): (NodeIndex, NodeIndex)) -> bool { source != target }

fn path<N, E>(graph: &StableDiGraph<N, E>, members: impl Fn(NodeIndex) -> bool, start: NodeIndex, goal: NodeIndex) -> Either<Vec<NodeIndex>, (NodeIndex, NodeIndex)> {
    astar(&NodeFiltered::from_fn(graph, members), start, |node| node == goal, |_| 1_usize, |_| 0_usize).map_or(Either::Right((start, goal)), |(_, path)| Either::Left(path))
}
