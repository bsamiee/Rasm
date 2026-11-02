use std::collections::HashMap;

use itertools::Either;
use petgraph::{
    Direction,
    algo::tred::{dag_to_toposorted_adjacency_list, dag_transitive_reduction_closure},
    graph::{DefaultIx, DiGraph},
    stable_graph::{EdgeIndex, NodeIndex, StableDiGraph},
    visit::{EdgeRef as _, NodeIndexable as _},
};
use rustworkx_core::dag_algo::{TopologicalSortError, lexicographical_topological_sort};

use crate::{DependencyError, Evaluation, cycles};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn topological<N, E, K>(graph: &StableDiGraph<N, E>, mut key: impl FnMut(NodeIndex) -> K, direction: Direction) -> Result<Vec<NodeIndex>, DependencyError>
where K: Ord {
    match lexicographical_topological_sort(graph, |node| Ok::<K, !>(key(node)), direction == Direction::Incoming, None) {
        Ok(sequence) if sequence.len() == graph.node_count() => Ok(sequence),
        Ok(_) | Err(TopologicalSortError::CycleOrBadInitialState) => Err(DependencyError::Cycle { paths: cycles::closings(graph) }),
    }
}

#[must_use]
pub fn layers<N, E>(graph: &Evaluation<N, E>, direction: Direction) -> Vec<Vec<NodeIndex>> {
    let sequence = graph.nodes_iter().collect::<Vec<_>>();
    let ordered = match direction {
        Direction::Outgoing => Either::Left(sequence.into_iter()),
        Direction::Incoming => Either::Right(sequence.into_iter().rev()),
    };
    let (_, layered) = ordered.fold((HashMap::new(), Vec::<Vec<NodeIndex>>::new()), |(mut depth, mut layered), node| {
        let level = graph.neighbors_directed(node, direction.opposite()).filter_map(|neighbor| depth.get(&neighbor).copied()).max().map_or_default(|level: usize| level.strict_add(1));
        depth.insert(node, level);
        match layered.get_mut(level) {
            Some(layer) => layer.push(node),
            None => layered.push(vec![node]),
        }
        (depth, layered)
    });
    layered
}

#[must_use]
pub fn redundant<N, E>(graph: &Evaluation<N, E>) -> Vec<EdgeIndex> {
    let compact = DiGraph::from(graph.map(|node, _| node, |edge, _| edge));
    let mut compacted = vec![None; graph.node_bound()];
    compact.node_indices().zip(compact.node_weights()).for_each(|(node, stable)| {
        if let Some(slot) = compacted.get_mut(stable.index()) {
            *slot = Some(node);
        }
    });
    let sequence = graph.nodes_iter().filter_map(|stable| compacted.get(stable.index()).copied().flatten()).collect::<Vec<_>>();
    let (ordered, ranks) = dag_to_toposorted_adjacency_list::<_, DefaultIx>(&compact, &sequence);
    let (reduction, _) = dag_transitive_reduction_closure(&ordered);
    compact
        .edge_references()
        .filter(|edge| {
            compact.find_edge(edge.source(), edge.target()) != Some(edge.id())
                || ranks.get(edge.source().index()).zip(ranks.get(edge.target().index())).is_some_and(|(&source, &target)| !reduction.contains_edge(source, target))
        })
        .map(|edge| *edge.weight())
        .collect()
}
