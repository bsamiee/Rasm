use itertools::Itertools as _;
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use petgraph::Direction;
use petgraph::algo::tred::{dag_to_toposorted_adjacency_list, dag_transitive_reduction_closure};
use petgraph::graph::DefaultIx;
use petgraph::stable_graph::{EdgeIndex, NodeIndex};
use petgraph::visit::{EdgeRef as _, NodeIndexable as _};
use rustworkx_core::dag_algo::{TopologicalSortError, lexicographical_topological_sort};

use crate::{Evaluation, at, compact, dense, entries, pass};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn topological<N, E, K>(evaluation: &Evaluation<N, E>, mut key: impl FnMut(NodeIndex) -> K, direction: Direction) -> Result<Vec<NodeIndex>, TopologicalSortError<!>>
where K: Ord {
    lexicographical_topological_sort(evaluation.inner(), |node| Ok::<K, !>(key(node)), direction == Direction::Incoming, None)
}

#[must_use]
pub fn layers<N, E>(evaluation: &Evaluation<N, E>, direction: Direction) -> Vec<NEVec<NodeIndex>> {
    let levels = pass(evaluation, direction, |node, levels| {
        evaluation.neighbors_directed(node, direction.opposite()).filter_map(|neighbor| at(levels, neighbor)).max().map_or_default(|level: usize| level.strict_add(1))
    });
    entries(&levels)
        .sorted_by_key(|&(_, &level)| level)
        .chunk_by(|&(_, &level)| level)
        .into_iter()
        .filter_map(|(_, layer)| layer.map(|(node, _)| node).try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>))
        .collect()
}

#[must_use]
pub fn redundant<N, E>(evaluation: &Evaluation<N, E>) -> Vec<EdgeIndex> {
    let graph = compact(evaluation);
    let compacted = dense(evaluation.node_bound(), graph.node_indices().zip(graph.node_weights()).map(|(node, &stable)| (stable, node)));
    let sequence = evaluation.nodes_iter().filter_map(|stable| at(&compacted, stable)).collect::<Vec<_>>();
    let (ordered, ranks) = dag_to_toposorted_adjacency_list::<_, DefaultIx>(&graph, &sequence);
    let (reduction, _) = dag_transitive_reduction_closure(&ordered);
    graph
        .edge_references()
        .filter(|edge| {
            graph.find_edge(edge.source(), edge.target()) != Some(edge.id())
                || ranks.get(edge.source().index()).zip(ranks.get(edge.target().index())).is_some_and(|(&source, &target)| !reduction.contains_edge(source, target))
        })
        .map(|edge| *edge.weight())
        .collect()
}
