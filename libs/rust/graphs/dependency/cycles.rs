use std::collections::HashMap;

use petgraph::{
    algo::{condensation, greedy_feedback_arc_set, kosaraju_scc},
    graph::DiGraph,
    stable_graph::{EdgeIndex, EdgeReference, NodeIndex, StableDiGraph},
    visit::{EdgeFiltered, EdgeRef as _},
};
use rustworkx_core::connectivity::johnson_simple_cycles;

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
    let block = kosaraju_scc(&skeleton).into_iter().enumerate().flat_map(|(rank, members)| members.into_iter().map(move |node| (node, rank))).collect::<HashMap<_, _>>();
    let edges = greedy_feedback_arc_set(&EdgeFiltered::from_fn(&skeleton, |edge: EdgeReference<'_, ()>| block.get(&edge.source()) == block.get(&edge.target())))
        .filter_map(|edge| graph.remove_edge(edge.id()).map(|weight| (edge.source(), edge.target(), weight)))
        .collect();
    Torn { graph, edges }
}
