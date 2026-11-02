#![feature(gen_blocks)]

use core::iter;

use itertools::Itertools as _;
use nonempty_collections::NEBTreeSet;
use petgraph::{
    Direction,
    acyclic::{Acyclic, AcyclicEdgeError},
    stable_graph::{EdgeIndex, NodeIndex, StableDiGraph},
    visit::EdgeRef as _,
};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Evaluation<N, E> = Acyclic<StableDiGraph<N, E>>;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Inputs {
    Extend,
    Replace,
}

// --- [MODELS] --------------------------------------------------------------------------

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
    let paths = cycles::closings(&graph);
    paths.is_empty().ok_or(DependencyError::Cycle { paths })?;
    Evaluation::try_from(graph).map_err(|refused| DependencyError::Cycle { paths: vec![vec![refused.node_id()]] })
}

pub fn connect<N, E>(graph: &mut Evaluation<N, E>, target: NodeIndex, sources: impl IntoIterator<Item = (NodeIndex, E)>, inputs: Inputs) -> Result<Rewiring<E>, DependencyError>
where
    N: 'static,
    E: 'static,
{
    let sources = sources.into_iter().collect::<Vec<_>>();
    present(graph, iter::once(target).chain(sources.iter().map(|&(source, _)| source)))?;
    let closing = |evaluation: &Evaluation<N, E>, source: NodeIndex| {
        let window = evaluation.get_position(target)..=evaluation.get_position(source);
        cycles::closing(evaluation.inner(), |node| window.contains(&evaluation.get_position(node)), target, source)
    };
    let paths = sources.iter().filter(|&&(source, _)| !graph.is_valid_edge(source, target)).map(|&(source, _)| closing(graph, source)).collect::<Vec<_>>();
    paths.is_empty().ok_or(DependencyError::Cycle { paths })?;
    let replaced = match inputs {
        Inputs::Extend => Vec::new(),
        Inputs::Replace => graph
            .edges_directed(target, Direction::Incoming)
            .map(|edge| (edge.id(), edge.source()))
            .collect::<Vec<_>>()
            .into_iter()
            .filter_map(|(edge, source)| graph.remove_edge(edge).map(|weight| (source, weight)))
            .collect(),
    };
    sources
        .into_iter()
        .map(|(source, weight)| {
            graph
                .try_add_edge(source, target, weight)
                .map_err(|(AcyclicEdgeError::Cycle(_) | AcyclicEdgeError::SelfLoop | AcyclicEdgeError::InvalidEdge)| DependencyError::Cycle { paths: vec![closing(graph, source)] })
        })
        .collect::<Result<Vec<_>, _>>()
        .map(|added| Rewiring { added, replaced })
}

fn present<N, E>(graph: &StableDiGraph<N, E>, nodes: impl IntoIterator<Item = NodeIndex>) -> Result<(), DependencyError> {
    NEBTreeSet::try_from_set(nodes.into_iter().filter(|&node| !graph.contains_node(node)).collect()).map_or(Ok(()), |nodes| Err(DependencyError::Missing { nodes }))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod critical;
pub mod cycles;
pub mod invalidation;
pub mod order;
