use core::marker::PhantomData;
use std::collections::{BTreeMap, BTreeSet};

use petgraph::{
    Direction,
    stable_graph::{NodeIndex, StableDiGraph},
    visit::{Dfs, Reversed, VisitMap as _, Visitable, Walker as _},
};

use crate::{DependencyError, Evaluation, present};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Due<'id> {
    node: NodeIndex,
    brand: PhantomData<fn(&'id ()) -> &'id ()>,
}

impl Due<'_> {
    #[must_use]
    pub const fn node(&self) -> NodeIndex { self.node }
}

#[derive(Debug, Clone, Copy)]
struct Pending {
    inputs: usize,
    stale: bool,
}

#[derive(Debug)]
pub struct Propagation<'graph, 'id, N, E> {
    graph: &'graph StableDiGraph<N, E>,
    pending: BTreeMap<NodeIndex, Pending>,
    changed: BTreeSet<NodeIndex>,
    brand: PhantomData<fn(&'id ()) -> &'id ()>,
}

impl<'id, N, E> Propagation<'_, 'id, N, E> {
    pub fn finish(&mut self, Due { node, brand: _ }: Due<'id>, changed: bool) -> Vec<Due<'id>> {
        self.pending.remove(&node);
        if changed {
            self.changed.insert(node);
        }
        let released = self.complete(node, changed);
        self.advance(released)
    }

    pub fn outstanding(&self) -> impl Iterator<Item = NodeIndex> { self.pending.iter().filter(|&(_, &Pending { inputs: _, stale })| stale).map(|(&node, _)| node) }

    #[must_use]
    pub const fn changed(&self) -> &BTreeSet<NodeIndex> { &self.changed }

    fn complete(&mut self, node: NodeIndex, changed: bool) -> Vec<NodeIndex> {
        self.graph
            .neighbors_directed(node, Direction::Outgoing)
            .filter(|target| {
                self.pending.get_mut(target).is_some_and(|Pending { inputs, stale }| {
                    *stale |= changed;
                    *inputs = inputs.strict_sub(1);
                    *inputs == 0
                })
            })
            .collect()
    }

    fn advance(&mut self, mut released: Vec<NodeIndex>) -> Vec<Due<'id>> {
        let mut ready = Vec::new();
        while let Some(node) = released.pop() {
            if self.pending.get(&node).is_some_and(|&Pending { inputs: _, stale }| stale) {
                ready.push(Due { node, brand: PhantomData });
            } else {
                self.pending.remove(&node);
                released.extend(self.complete(node, false));
            }
        }
        ready
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn reach<N, E>(graph: &StableDiGraph<N, E>, nodes: impl IntoIterator<Item = NodeIndex>, direction: Direction) -> Result<StableDiGraph<&N, &E>, DependencyError> {
    walk(graph, nodes, direction).map(|discovered| graph.filter_map(|node, weight| discovered.is_visited(&node).then_some(weight), |_, weight| Some(weight)))
}

pub fn propagate<'graph, N, E, R>(
    evaluation: &'graph Evaluation<N, E>,
    dirty: impl IntoIterator<Item = NodeIndex>,
    run: impl for<'id> FnOnce(Propagation<'graph, 'id, N, E>, Vec<Due<'id>>) -> R,
) -> Result<R, DependencyError> {
    let graph = evaluation.graph();
    let dirty = dirty.into_iter().collect::<BTreeSet<_>>();
    let discovered = walk(graph, dirty.iter().copied(), Direction::Outgoing)?;
    let pending = discovered
        .ones()
        .map(NodeIndex::new)
        .map(|node| (node, Pending { inputs: graph.neighbors_directed(node, Direction::Incoming).filter(|source| discovered.is_visited(source)).count(), stale: dirty.contains(&node) }))
        .collect::<BTreeMap<_, _>>();
    let released = pending.iter().filter(|&(_, &Pending { inputs, stale: _ })| inputs == 0).map(|(&node, _)| node).collect();
    let mut propagation = Propagation { graph, pending, changed: BTreeSet::new(), brand: PhantomData };
    let ready = propagation.advance(released);
    Ok(run(propagation, ready))
}

fn walk<N, E>(graph: &StableDiGraph<N, E>, nodes: impl IntoIterator<Item = NodeIndex>, direction: Direction) -> Result<<StableDiGraph<N, E> as Visitable>::Map, DependencyError> {
    let nodes = nodes.into_iter().collect::<Vec<_>>();
    present(graph, nodes.iter().copied())?;
    let mut search = Dfs::from_parts(nodes, graph.visit_map());
    match direction {
        Direction::Outgoing => (&mut search).iter(graph).for_each(drop),
        Direction::Incoming => (&mut search).iter(Reversed(graph)).for_each(drop),
    }
    Ok(search.discovered)
}
