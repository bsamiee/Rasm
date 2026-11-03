use core::marker::PhantomData;
use std::collections::BTreeSet;

use petgraph::{
    Direction,
    stable_graph::{NodeIndex, StableDiGraph},
    visit::{Dfs, NodeIndexable as _, Reversed, VisitMap as _, Visitable, Walker as _},
};

use crate::{DependencyError, Evaluation, acyclic, at, dense, entries, present};

// --- [TYPES] ---------------------------------------------------------------------------

type Brand<'id> = PhantomData<fn(&'id ()) -> &'id ()>;

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub enum Outcome {
    Unchanged,
    Changed,
    Failed,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Ready<'id> {
    node: NodeIndex,
    brand: Brand<'id>,
}

impl Ready<'_> {
    #[must_use]
    pub const fn node(&self) -> NodeIndex { self.node }
}

#[derive(Debug, Clone, Copy)]
struct Pending {
    inputs: usize,
    received: Outcome,
}

#[derive(Debug)]
pub struct Propagation<'graph, 'id, N, E> {
    graph: &'graph StableDiGraph<N, E>,
    pending: Vec<Option<Pending>>,
    dirty: BTreeSet<NodeIndex>,
    brand: Brand<'id>,
}

impl<'id, N, E> Propagation<'_, 'id, N, E> {
    pub fn finish(&mut self, Ready { node, brand: _ }: Ready<'id>, outcome: Outcome) -> Vec<Ready<'id>> {
        let released = self.settle(node, outcome);
        self.advance(released)
    }

    pub fn outstanding(&self) -> impl Iterator<Item = (NodeIndex, Outcome)> {
        entries(&self.pending).map(|(node, &Pending { inputs: _, received })| (node, received))
    }

    pub fn dirty(&self) -> impl Iterator<Item = NodeIndex> { self.dirty.iter().copied() }

    fn settle(&mut self, node: NodeIndex, outcome: Outcome) -> Vec<NodeIndex> {
        if let Some(slot) = self.pending.get_mut(node.index()) {
            *slot = slot.filter(|_| outcome == Outcome::Failed).map(|pending| Pending { received: outcome, ..pending });
        }
        let graph = self.graph;
        graph
            .neighbors_directed(node, Direction::Outgoing)
            .filter_map(|target| {
                let remaining = self.pending.get_mut(target.index()).and_then(Option::as_mut).map(|pending| {
                    *pending = Pending { inputs: pending.inputs.strict_sub(1), received: pending.received.max(outcome) };
                    pending.inputs
                });
                match remaining {
                    Some(0) => Some(target),
                    None if outcome == Outcome::Changed => {
                        self.dirty.insert(target);
                        None
                    }
                    Some(1..) | None => None,
                }
            })
            .collect()
    }

    fn advance(&mut self, mut released: Vec<NodeIndex>) -> Vec<Ready<'id>> {
        let mut ready = Vec::new();
        while let Some(node) = released.pop() {
            match at(&self.pending, node) {
                Some(Pending { inputs: _, received: Outcome::Changed }) => ready.push(Ready { node, brand: PhantomData }),
                Some(Pending { inputs: _, received: received @ (Outcome::Unchanged | Outcome::Failed) }) => released.extend(self.settle(node, received)),
                None => {}
            }
        }
        ready
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn reach<N, E>(evaluation: &Evaluation<N, E>, nodes: impl IntoIterator<Item = NodeIndex>, direction: Direction) -> Result<Evaluation<&N, &E>, DependencyError> {
    let nodes = nodes.into_iter().collect::<Vec<_>>();
    present(evaluation, nodes.iter().copied())?;
    let discovered = walk(evaluation, nodes, direction);
    Ok(acyclic(evaluation.filter_map(|node, weight| discovered.is_visited(&node).then_some(weight), |_, weight| Some(weight))))
}

pub fn propagate<'graph, N, E, R>(
    evaluation: &'graph Evaluation<N, E>,
    dirty: impl IntoIterator<Item = NodeIndex>,
    requested: Option<&[NodeIndex]>,
    run: impl for<'id> FnOnce(Propagation<'graph, 'id, N, E>, Vec<Ready<'id>>) -> R,
) -> Result<R, DependencyError> {
    let dirty = dirty.into_iter().collect::<BTreeSet<_>>();
    present(evaluation, dirty.iter().chain(requested.into_iter().flatten()).copied())?;
    let downstream = walk(evaluation, dirty.iter().copied(), Direction::Outgoing);
    let upstream = requested.map(|outputs| walk(evaluation, outputs.iter().copied(), Direction::Incoming));
    let member = |node: NodeIndex| downstream.is_visited(&node) && upstream.as_ref().is_none_or(|upstream| upstream.is_visited(&node));
    let pending = dense(
        evaluation.node_bound(),
        downstream.ones().map(NodeIndex::new).filter(|&node| member(node)).map(|node| {
            let inputs = evaluation.neighbors_directed(node, Direction::Incoming).filter(|&source| member(source)).count();
            (node, Pending { inputs, received: if dirty.contains(&node) { Outcome::Changed } else { Outcome::Unchanged } })
        }),
    );
    let released = entries(&pending).filter_map(|(node, &Pending { inputs, received: _ })| (inputs == 0).then_some(node)).collect();
    let deferred = dirty.iter().copied().filter(|&node| !member(node)).collect();
    let mut propagation = Propagation { graph: evaluation.inner(), pending, dirty: deferred, brand: PhantomData };
    let ready = propagation.advance(released);
    Ok(run(propagation, ready))
}

fn walk<N, E>(graph: &StableDiGraph<N, E>, nodes: impl IntoIterator<Item = NodeIndex>, direction: Direction) -> <StableDiGraph<N, E> as Visitable>::Map {
    let mut search = Dfs::from_parts(nodes.into_iter().collect(), graph.visit_map());
    match direction {
        Direction::Outgoing => (&mut search).iter(graph).for_each(drop),
        Direction::Incoming => (&mut search).iter(Reversed(graph)).for_each(drop),
    }
    search.discovered
}
