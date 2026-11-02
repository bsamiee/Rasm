use std::collections::BTreeMap;

use petgraph::{
    algo::{astar, condensation, greedy_feedback_arc_set, kosaraju_scc},
    graph::DiGraph,
    stable_graph::{EdgeIndex, EdgeReference, NodeIndex, StableDiGraph},
    visit::{EdgeFiltered, EdgeRef as _, IntoEdgeReferences as _, NodeFiltered, NodeIndexable as _},
};
use rustworkx_core::connectivity::johnson_simple_cycles;

use crate::DependencyError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Torn<N, E> {
    pub graph: StableDiGraph<N, E>,
    pub edges: Vec<(NodeIndex, NodeIndex, E)>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn condensed<N, E>(graph: &StableDiGraph<N, E>) -> StableDiGraph<Vec<NodeIndex>, EdgeIndex> {
    let mut blocks = StableDiGraph::from(condensation(DiGraph::from(graph.map(|node, _| node, |edge, _| edge)), false));
    blocks.retain_edges(|blocks, edge| blocks.edge_endpoints(edge).is_some_and(|(source, target)| source != target));
    blocks
}

pub fn simple<N, E>(graph: &StableDiGraph<N, E>) -> impl Iterator<Item = Vec<NodeIndex>> + use<N, E> {
    let loops = graph.node_indices().filter(|&node| graph.contains_edge(node, node)).collect::<Vec<_>>();
    let skeleton = graph.filter_map(|_, _| Some(()), |edge, _| graph.edge_endpoints(edge).filter(|(source, target)| source != target).map(|_| ()));
    gen move {
        let mut cycles = johnson_simple_cycles(&skeleton, Some(loops).filter(|found| !found.is_empty()));
        while let Some(found) = cycles.next(&skeleton) {
            yield found;
        }
    }
}

#[must_use]
pub fn tear<N, E>(mut graph: StableDiGraph<N, E>) -> Torn<N, E> {
    let skeleton = graph.map(|_, _| (), |_, _| ());
    let block = blocks(&skeleton);
    let edges = greedy_feedback_arc_set(&EdgeFiltered::from_fn(&skeleton, |edge: EdgeReference<'_, ()>| block.get(edge.source().index()) == block.get(edge.target().index())))
        .filter_map(|edge| graph.remove_edge(edge.id()).map(|weight| (edge.source(), edge.target(), weight)))
        .collect();
    Torn { graph, edges }
}

pub(crate) fn refusal<N, E>(graph: &StableDiGraph<N, E>) -> DependencyError {
    let block = blocks(graph);
    let within = |node: NodeIndex| block.get(node.index()).copied().flatten();
    DependencyError::Cycle {
        paths: graph
            .edge_references()
            .filter_map(|edge| within(edge.source()).filter(|&rank| within(edge.target()) == Some(rank)).map(|rank| (rank, (edge.source(), edge.target()))))
            .collect::<BTreeMap<_, _>>()
            .into_iter()
            .map(|(rank, (source, target))| closing(graph, |node| within(node) == Some(rank), target, source))
            .collect(),
    }
}

fn blocks<N, E>(graph: &StableDiGraph<N, E>) -> Vec<Option<usize>> {
    let mut block = vec![None; graph.node_bound()];
    for (node, rank) in kosaraju_scc(graph).into_iter().enumerate().flat_map(|(rank, members)| members.into_iter().map(move |node| (node, rank))) {
        if let Some(slot) = block.get_mut(node.index()) {
            *slot = Some(rank);
        }
    }
    block
}

pub(crate) fn closing<N, E>(graph: &StableDiGraph<N, E>, members: impl Fn(NodeIndex) -> bool, start: NodeIndex, goal: NodeIndex) -> Vec<NodeIndex> {
    astar(&NodeFiltered::from_fn(graph, members), start, |node| node == goal, |_| 1_usize, |_| 0_usize).map_or_default(|(_, path)| path)
}
