#![feature(gen_blocks)]

use core::iter;

use itertools::Itertools as _;
use nonempty_collections::NEBTreeSet;
use petgraph::{
    Direction,
    acyclic::{Acyclic, AcyclicEdgeError},
    algo::Cycle,
    data::Build as _,
    stable_graph::{EdgeIndex, NodeIndex, StableDiGraph},
    visit::EdgeRef as _,
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Inputs {
    Extend,
    Replace,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Evaluation<N, E> {
    order: Acyclic<StableDiGraph<(), ()>>,
    graph: StableDiGraph<N, E>,
}

impl<N, E> Evaluation<N, E> {
    #[must_use]
    pub const fn order(&self) -> &Acyclic<StableDiGraph<(), ()>> { &self.order }

    #[must_use]
    pub const fn graph(&self) -> &StableDiGraph<N, E> { &self.graph }

    pub fn add_node(&mut self, weight: N) -> NodeIndex {
        self.order.add_node(());
        self.graph.add_node(weight)
    }

    pub fn remove_node(&mut self, node: NodeIndex) -> Option<N> {
        self.order.remove_node(node)?;
        self.graph.remove_node(node)
    }
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Rewiring<E> {
    pub added: Vec<EdgeIndex>,
    pub replaced: Vec<(NodeIndex, E)>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, thiserror::Error)]
pub enum DependencyError {
    #[error("dependency cycles {}", paths.iter().map(|path| iter::chain(path, path.first()).map(|node| node.index()).format(" -> ")).format("; "))]
    Cycle { paths: Vec<Vec<NodeIndex>> },
    #[error("nodes outside the graph: {}", nodes.iter().map(|node| node.index()).format(", "))]
    Missing { nodes: NEBTreeSet<NodeIndex> },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn evaluation<N, E>(graph: StableDiGraph<N, E>) -> Result<Evaluation<N, E>, DependencyError> {
    Acyclic::try_from(graph.map(|_, _| (), |_, _| ())).map_err(|Cycle { .. }| DependencyError::Cycle { paths: cycles::closings(&graph) }).map(|order| Evaluation { order, graph })
}

pub fn connect<N, E>(evaluation: &mut Evaluation<N, E>, target: NodeIndex, sources: impl IntoIterator<Item = (NodeIndex, E)>, inputs: Inputs) -> Result<Rewiring<E>, DependencyError> {
    let sources = sources.into_iter().collect::<Vec<_>>();
    present(&evaluation.graph, iter::once(target).chain(sources.iter().map(|&(source, _)| source)))?;
    let mut order = evaluation.order.clone();
    let refused = sources
        .iter()
        .filter_map(|&(source, _)| order.try_add_edge(source, target, ()).map_err(|(AcyclicEdgeError::Cycle(_) | AcyclicEdgeError::SelfLoop | AcyclicEdgeError::InvalidEdge)| source).err())
        .collect::<Vec<_>>();
    if refused.is_empty() {
        let incoming = match inputs {
            Inputs::Extend => Vec::new(),
            Inputs::Replace => evaluation.graph.edges_directed(target, Direction::Incoming).map(|edge| (edge.id(), edge.source())).collect(),
        };
        let added = sources.into_iter().map(|(source, weight)| evaluation.graph.add_edge(source, target, weight)).collect();
        let replaced = incoming
            .into_iter()
            .filter_map(|(edge, source)| {
                order.remove_edge(edge)?;
                evaluation.graph.remove_edge(edge).map(|weight| (source, weight))
            })
            .collect();
        evaluation.order = order;
        Ok(Rewiring { added, replaced })
    } else {
        Err(DependencyError::Cycle {
            paths: refused
                .into_iter()
                .map(|source| {
                    let window = evaluation.order.get_position(target)..=evaluation.order.get_position(source);
                    cycles::closing(evaluation.order.inner(), |node| window.contains(&evaluation.order.get_position(node)), target, source)
                })
                .collect(),
        })
    }
}

fn present<N, E>(graph: &StableDiGraph<N, E>, nodes: impl IntoIterator<Item = NodeIndex>) -> Result<(), DependencyError> {
    NEBTreeSet::try_from_set(nodes.into_iter().filter(|&node| !graph.contains_node(node)).collect()).map_or(Ok(()), |nodes| Err(DependencyError::Missing { nodes }))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod critical;
pub mod cycles;
pub mod invalidation;
pub mod order;
