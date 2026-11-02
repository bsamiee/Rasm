use core::iter;
use std::collections::{BTreeMap, BTreeSet, btree_map::Entry};

use petgraph::{
    Direction,
    stable_graph::{NodeIndex, StableDiGraph},
    visit::{Dfs, Reversed, VisitMap as _, Visitable as _, Walker as _},
};

use crate::{DependencyError, Evaluation, present};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Due(NodeIndex);

impl Due {
    #[must_use]
    pub const fn node(&self) -> NodeIndex { self.0 }
}

#[derive(Debug)]
pub struct Propagation<'graph, N, E> {
    graph: &'graph StableDiGraph<N, E>,
    pending: BTreeMap<NodeIndex, usize>,
    stale: BTreeSet<NodeIndex>,
    due: BTreeSet<NodeIndex>,
    changed: BTreeSet<NodeIndex>,
}

impl<'graph, N, E> Propagation<'graph, N, E> {
    pub fn new(evaluation: &'graph Evaluation<N, E>, dirty: impl IntoIterator<Item = NodeIndex>) -> Result<(Self, Vec<Due>), DependencyError> {
        let graph = evaluation.inner();
        let stale = dirty.into_iter().collect::<BTreeSet<_>>();
        present(graph, stale.iter().copied())?;
        let mut walk = Dfs::from_parts(stale.iter().copied().collect(), graph.visit_map());
        let affected = (&mut walk).iter(graph).collect::<Vec<_>>();
        let mut pending =
            affected.into_iter().map(|node| (node, graph.neighbors_directed(node, Direction::Incoming).filter(|source| walk.discovered.is_visited(source)).count())).collect::<BTreeMap<_, _>>();
        let released = pending.extract_if(.., |_, &mut inputs| inputs == 0).map(|(node, _)| node).collect();
        let mut propagation = Self { graph, pending, stale, due: BTreeSet::new(), changed: BTreeSet::new() };
        let ready = propagation.advance(released);
        Ok((propagation, ready))
    }

    pub fn finish(&mut self, Due(node): Due, changed: bool) -> Vec<Due> {
        self.due.remove(&node);
        let released = self.complete(node, changed);
        self.advance(released)
    }

    #[must_use]
    pub fn owed(&self) -> BTreeSet<NodeIndex> { iter::chain(&self.due, self.pending.keys()).copied().collect() }

    #[must_use]
    pub const fn changed(&self) -> &BTreeSet<NodeIndex> { &self.changed }

    fn complete(&mut self, node: NodeIndex, changed: bool) -> Vec<NodeIndex> {
        let graph = self.graph;
        if changed {
            self.changed.insert(node);
            self.stale.extend(graph.neighbors_directed(node, Direction::Outgoing));
        }
        graph
            .neighbors_directed(node, Direction::Outgoing)
            .filter(|&target| match self.pending.entry(target) {
                Entry::Occupied(held) if *held.get() == 1 => {
                    held.remove();
                    true
                }
                Entry::Occupied(mut held) => {
                    *held.get_mut() = held.get().strict_sub(1);
                    false
                }
                Entry::Vacant(_) => false,
            })
            .collect()
    }

    fn advance(&mut self, mut released: Vec<NodeIndex>) -> Vec<Due> {
        let mut ready = Vec::new();
        while let Some(node) = released.pop() {
            if self.stale.remove(&node) {
                self.due.insert(node);
                ready.push(Due(node));
            } else {
                released.extend(self.complete(node, false));
            }
        }
        ready
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn reach<N, E>(graph: &StableDiGraph<N, E>, nodes: impl IntoIterator<Item = NodeIndex>, direction: Direction) -> Result<StableDiGraph<&N, &E>, DependencyError> {
    let nodes = nodes.into_iter().collect::<Vec<_>>();
    present(graph, nodes.iter().copied())?;
    let mut walk = Dfs::from_parts(nodes, graph.visit_map());
    match direction {
        Direction::Outgoing => (&mut walk).iter(graph).for_each(drop),
        Direction::Incoming => (&mut walk).iter(Reversed(graph)).for_each(drop),
    }
    Ok(graph.filter_map(|node, weight| walk.discovered.is_visited(&node).then_some(weight), |_, weight| Some(weight)))
}
