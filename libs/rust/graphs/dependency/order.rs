use petgraph::{
    Directed, Direction,
    stable_graph::NodeIndex,
    visit::{EdgeCount, GraphBase, GraphProp, IntoEdgesDirected, IntoNodeIdentifiers, NodeCount, NodeIndexable, Visitable},
};
use rustworkx_core::{
    connectivity::find_cycle,
    dag_algo::{self, TopologicalSortError, lexicographical_topological_sort},
    err::LayersError,
};

use crate::DependencyError;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn topological<G, K>(graph: G, mut key: impl FnMut(NodeIndex) -> K) -> Result<Vec<NodeIndex>, DependencyError>
where
    G: GraphBase<NodeId = NodeIndex> + GraphProp<EdgeType = Directed> + IntoNodeIdentifiers + IntoEdgesDirected + NodeCount + EdgeCount + Visitable,
    K: Ord,
{
    let sequence = lexicographical_topological_sort(graph, |node| Ok::<K, !>(key(node)), false, None).map_err(|TopologicalSortError::CycleOrBadInitialState| cycle(graph))?;
    (sequence.len() == graph.node_count()).then_some(sequence).ok_or_else(|| cycle(graph))
}

pub fn layers<G>(graph: G) -> Result<Vec<Vec<NodeIndex>>, DependencyError>
where G: GraphBase<NodeId = NodeIndex> + NodeIndexable + IntoNodeIdentifiers + IntoEdgesDirected + NodeCount + EdgeCount + Visitable {
    let sources = graph.node_identifiers().filter(|&node| graph.neighbors_directed(node, Direction::Incoming).next().is_none()).collect();
    let layered = dag_algo::layers(graph, sources).filter(|layer| !layer.as_ref().is_ok_and(Vec::is_empty)).try_collect::<Vec<_>>().map_err(|LayersError(_)| cycle(graph))?;
    (layered.iter().map(Vec::len).sum::<usize>() == graph.node_count()).then_some(layered).ok_or_else(|| cycle(graph))
}

fn cycle<G>(graph: G) -> DependencyError
where G: GraphBase<NodeId = NodeIndex> + IntoNodeIdentifiers + IntoEdgesDirected + NodeCount + EdgeCount + Visitable {
    DependencyError::Cycle { path: find_cycle(graph, None).into_iter().map(|(source, _)| source).collect() }
}
