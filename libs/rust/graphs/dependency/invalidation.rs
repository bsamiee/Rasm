use core::{iter, ops::ControlFlow};
use std::collections::BTreeSet;

use petgraph::{
    Direction,
    algo::{
        toposort,
        tred::{dag_to_toposorted_adjacency_list, dag_transitive_reduction_closure},
    },
    graph::{DefaultIx, DiGraph},
    stable_graph::{EdgeIndex, NodeIndex, StableDiGraph},
    visit::{Dfs, EdgeRef as _, Reversed, VisitMap as _, Visitable as _, Walker as _},
};

use crate::{DependencyError, cycle, order::layers};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn reach<N, E>(graph: &StableDiGraph<N, E>, nodes: impl IntoIterator<Item = NodeIndex>, direction: Direction) -> Result<StableDiGraph<&N, &E>, DependencyError> {
    let mut walk = Dfs::from_parts(nodes.into_iter().map(|node| graph.contains_node(node).then_some(node).ok_or(DependencyError::Missing { node })).try_collect()?, graph.visit_map());
    match direction {
        Direction::Outgoing => (&mut walk).iter(graph).for_each(drop),
        Direction::Incoming => (&mut walk).iter(Reversed(graph)).for_each(drop),
    }
    Ok(graph.filter_map(|node, weight| walk.discovered.is_visited(&node).then_some(weight), |_, weight| Some(weight)))
}

pub fn propagate<N, E, B, C>(
    graph: &StableDiGraph<N, E>,
    dirty: impl IntoIterator<Item = NodeIndex>,
    mut recompute: impl FnMut(&[NodeIndex]) -> ControlFlow<B, C>,
) -> Result<ControlFlow<(B, BTreeSet<NodeIndex>)>, DependencyError>
where
    C: IntoIterator<Item = NodeIndex>,
{
    let dirty = dirty.into_iter().collect::<BTreeSet<_>>();
    let affected = reach(graph, dirty.iter().copied(), Direction::Outgoing)?;
    let stale = |node: &NodeIndex, changed: &BTreeSet<NodeIndex>| dirty.contains(node) || affected.neighbors_directed(*node, Direction::Incoming).any(|source| changed.contains(&source));
    let mut waves = layers(&affected)?.into_iter();
    let settled = waves.by_ref().try_fold(BTreeSet::new(), |changed, wave| {
        let due = wave.into_iter().filter(|node| stale(node, &changed)).collect::<Vec<_>>();
        match (!due.is_empty()).then(|| recompute(&due)) {
            None => ControlFlow::Continue(changed),
            Some(ControlFlow::Continue(found)) => ControlFlow::Continue(iter::chain(changed, found).collect()),
            Some(ControlFlow::Break(reason)) => ControlFlow::Break((reason, due, changed)),
        }
    });
    Ok(settled.map_continue(drop).map_break(|(reason, due, changed)| (reason, iter::chain(due, waves.flatten().filter(|node| stale(node, &changed))).collect())))
}

pub fn redundant<N, E>(graph: &StableDiGraph<N, E>) -> Result<Vec<EdgeIndex>, DependencyError> {
    let compact = DiGraph::from(graph.map(|node, _| node, |edge, _| edge));
    let sequence = toposort(&compact, None).map_err(|refused| cycle(graph, compact.node_weight(refused.node_id()).copied()))?;
    let (ordered, ranks) = dag_to_toposorted_adjacency_list::<_, DefaultIx>(&compact, &sequence);
    let (reduction, _) = dag_transitive_reduction_closure(&ordered);
    Ok(compact
        .edge_references()
        .filter(|edge| {
            compact.find_edge(edge.source(), edge.target()) != Some(edge.id())
                || ranks.get(edge.source().index()).zip(ranks.get(edge.target().index())).is_some_and(|(&source, &target)| !reduction.contains_edge(source, target))
        })
        .map(|edge| *edge.weight())
        .collect())
}
