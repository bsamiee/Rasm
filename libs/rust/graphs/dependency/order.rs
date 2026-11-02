use petgraph::{Direction, stable_graph::NodeIndex, visit::NodeIndexable};
use rustworkx_core::{
    dag_algo::{self, TopologicalSortError, lexicographical_topological_sort},
    err::LayersError,
};

use crate::{Dependencies, DependencyError, cycle};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn topological<G, K>(graph: G, mut key: impl FnMut(NodeIndex) -> K) -> Result<Vec<NodeIndex>, DependencyError>
where
    G: Dependencies,
    K: Ord,
{
    let sequence = lexicographical_topological_sort(graph, |node| Ok::<K, !>(key(node)), false, None).map_err(|TopologicalSortError::CycleOrBadInitialState| cycle(graph, None))?;
    (sequence.len() == graph.node_count()).then_some(sequence).ok_or_else(|| cycle(graph, None))
}

pub fn layers<G>(graph: G) -> Result<Vec<Vec<NodeIndex>>, DependencyError>
where G: Dependencies + NodeIndexable {
    let sources = graph.node_identifiers().filter(|&node| graph.neighbors_directed(node, Direction::Incoming).next().is_none()).collect();
    let layered = dag_algo::layers(graph, sources).filter(|layer| !layer.as_ref().is_ok_and(Vec::is_empty)).try_collect::<Vec<_>>().map_err(|LayersError(_)| cycle(graph, None))?;
    (layered.iter().map(Vec::len).sum::<usize>() == graph.node_count()).then_some(layered).ok_or_else(|| cycle(graph, None))
}
