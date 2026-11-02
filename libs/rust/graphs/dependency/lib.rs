#![feature(deref_patterns, gen_blocks, iterator_try_collect)]

use core::{fmt, iter};

use petgraph::{
    acyclic::{Acyclic, AcyclicEdgeError},
    algo::{astar, toposort},
    stable_graph::{EdgeIndex, NodeIndex, StableDiGraph},
    visit::NodeFiltered,
};
use rustworkx_core::connectivity::find_cycle;

// --- [TYPES] ---------------------------------------------------------------------------

pub type Evaluation<N, E> = Acyclic<StableDiGraph<N, E>>;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, thiserror::Error)]
pub enum DependencyError {
    #[error("dependency cycle {}", fmt::from_fn(|formatter| match path {
        [] => Ok(()),
        [first, rest @ ..] => iter::chain(rest, [first]).try_fold(write!(formatter, "{}", first.index())?, |(), node| write!(formatter, " -> {}", node.index())),
    }))]
    Cycle { path: Vec<NodeIndex> },
    #[error("node {} is outside the graph", node.index())]
    Missing { node: NodeIndex },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn evaluation<N, E>(graph: StableDiGraph<N, E>) -> Result<Evaluation<N, E>, DependencyError> {
    toposort(&graph, None).map_err(|refused| cycle(&graph, Some(refused.node_id())))?;
    Evaluation::try_from(graph).map_err(|refused| DependencyError::Cycle { path: vec![refused.node_id()] })
}

pub fn connect<N, E>(graph: &mut Evaluation<N, E>, source: NodeIndex, target: NodeIndex, weight: E) -> Result<EdgeIndex, DependencyError>
where
    N: 'static,
    E: 'static,
{
    [source, target].into_iter().try_for_each(|node| graph.contains_node(node).ok_or(DependencyError::Missing { node }))?;
    graph.try_add_edge(source, target, weight).map_err(|(AcyclicEdgeError::Cycle(_) | AcyclicEdgeError::SelfLoop | AcyclicEdgeError::InvalidEdge)| {
        let window = graph.get_position(target)..=graph.get_position(source);
        DependencyError::Cycle {
            path: astar(&NodeFiltered::from_fn(graph.inner(), |node| window.contains(&graph.get_position(node))), target, |node| node == source, |_| 1_usize, |_| 0_usize)
                .map_or_default(|(_, path)| path),
        }
    })
}

fn cycle<N, E>(graph: &StableDiGraph<N, E>, start: Option<NodeIndex>) -> DependencyError { DependencyError::Cycle { path: find_cycle(graph, start).into_iter().map(|(source, _)| source).collect() } }

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod critical;
pub mod cycles;
pub mod invalidation;
pub mod order;
