#![feature(deref_patterns, gen_blocks)]

use core::{fmt, iter, ops::ControlFlow};
use std::collections::{BTreeMap, BTreeSet};

use nonempty_collections::NEVec;
use petgraph::{
    Direction,
    acyclic::{Acyclic, AcyclicEdgeError},
    algo::{Cycle, astar, kosaraju_scc},
    stable_graph::{EdgeIndex, NodeIndex, StableDiGraph},
    visit::{EdgeRef as _, IntoEdgeReferences as _, NodeFiltered, NodeIndexable as _},
};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Evaluation<N, E> = Acyclic<StableDiGraph<N, E>>;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, thiserror::Error)]
pub enum DependencyError {
    #[error("dependency cycles {}", fmt::from_fn(|formatter| paths.iter().enumerate().try_for_each(|(rank, path)| match path {
        [] => Ok(()),
        [first, rest @ ..] => iter::chain(rest, [first]).try_fold(write!(formatter, "{}{}", if rank == 0 { "" } else { "; " }, first.index())?, |(), node| write!(formatter, " -> {}", node.index())),
    })))]
    Cycle { paths: Vec<Vec<NodeIndex>> },
    #[error("nodes outside the graph: {}", fmt::from_fn(|formatter| {
        let (first, rest) = nodes.split_first();
        rest.iter().try_fold(write!(formatter, "{}", first.index())?, |(), node| write!(formatter, ", {}", node.index()))
    }))]
    Missing { nodes: NEVec<NodeIndex> },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn evaluation<N, E>(graph: StableDiGraph<N, E>) -> Result<Evaluation<N, E>, DependencyError> {
    let skeleton = graph.map(|_, _| (), |_, _| ());
    Evaluation::try_from(graph).map_err(|refused: Cycle<NodeIndex>| DependencyError::Cycle { paths: cycles(&skeleton, Some(refused.node_id())) })
}

pub fn connect<N, E>(graph: &mut Evaluation<N, E>, target: NodeIndex, sources: impl IntoIterator<Item = (NodeIndex, E)>) -> Result<Vec<EdgeIndex>, DependencyError>
where
    N: 'static,
    E: 'static,
{
    let sources = sources.into_iter().collect::<Vec<_>>();
    present(graph, iter::once(target).chain(sources.iter().map(|&(source, _)| source)))?;
    let replaced = graph.edges_directed(target, Direction::Incoming).map(|edge| edge.id()).collect::<Vec<_>>();
    let added = sources.into_iter().try_fold(Vec::new(), |mut added, (source, weight)| match graph.try_add_edge(source, target, weight) {
        Ok(edge) => {
            added.push(edge);
            ControlFlow::Continue(added)
        }
        Err(AcyclicEdgeError::Cycle(_) | AcyclicEdgeError::SelfLoop | AcyclicEdgeError::InvalidEdge) => ControlFlow::Break((added, source)),
    });
    let (removed, outcome) = match added {
        ControlFlow::Continue(added) => (replaced, Ok(added)),
        ControlFlow::Break((added, source)) => {
            let window = graph.get_position(target)..=graph.get_position(source);
            (added, Err(DependencyError::Cycle { paths: vec![closing(graph.inner(), |node| window.contains(&graph.get_position(node)), target, source)] }))
        }
    };
    removed.into_iter().filter_map(|edge| graph.remove_edge(edge)).for_each(drop);
    outcome
}

fn present<N, E>(graph: &StableDiGraph<N, E>, nodes: impl IntoIterator<Item = NodeIndex>) -> Result<(), DependencyError> {
    NEVec::try_from_vec(nodes.into_iter().filter(|&node| !graph.contains_node(node)).collect::<BTreeSet<_>>().into_iter().collect()).map(|nodes| DependencyError::Missing { nodes }).map_or(Ok(()), Err)
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

fn cycles<N, E>(graph: &StableDiGraph<N, E>, lead: Option<NodeIndex>) -> Vec<Vec<NodeIndex>> {
    let block = blocks(graph);
    let within = |node: NodeIndex| block.get(node.index()).copied().flatten();
    let first = lead.and_then(within);
    graph
        .edge_references()
        .filter_map(|edge| within(edge.source()).filter(|&rank| within(edge.target()) == Some(rank)).map(|rank| ((Some(rank) != first, rank), (edge.source(), edge.target()))))
        .collect::<BTreeMap<_, _>>()
        .into_iter()
        .map(|((_, rank), (source, target))| closing(graph, |node| within(node) == Some(rank), target, source))
        .collect()
}

fn closing<N, E>(graph: &StableDiGraph<N, E>, members: impl Fn(NodeIndex) -> bool, start: NodeIndex, goal: NodeIndex) -> Vec<NodeIndex> {
    astar(&NodeFiltered::from_fn(graph, members), start, |node| node == goal, |_| 1_usize, |_| 0_usize).map_or_default(|(_, path)| path)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod critical;
pub mod cycles;
pub mod invalidation;
pub mod order;
