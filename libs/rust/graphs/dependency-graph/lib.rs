
use core::{fmt, iter};
use std::collections::BTreeSet;

use itertools::{Either, Itertools as _};
use nonempty_collections::{NEBTreeSet, NEVec};
use petgraph::{
    Direction,
    acyclic::{Acyclic, AcyclicEdgeError},
    graph::DiGraph,
    stable_graph::{EdgeIndex, NodeIndex, StableDiGraph},
    visit::{EdgeRef as _, NodeIndexable as _},
};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Evaluation<N, E> = Acyclic<StableDiGraph<N, E>>;

pub type Edges<E> = Vec<(NodeIndex, NodeIndex, E)>;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Inputs {
    Extend,
    Replace,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Refusal {
    #[display("nodes outside the graph")]
    Missing,
    #[display("activities with negative duration")]
    Duration,
    #[display("activities with unrepresentable dates")]
    Date,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Connected<E> {
    pub added: Vec<EdgeIndex>,
    pub replaced: Edges<E>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, thiserror::Error)]
pub enum DependencyError {
    #[error("dependency cycles {}", paths.iter().map(|path| fmt::from_fn(move |formatter| match path {
        Either::Left(path) => write!(formatter, "{}", indices(iter::chain(path, path.first()), " -> ")),
        Either::Right((source, target)) => write!(formatter, "no path {} -> {}", source.index(), target.index()),
    })).format("; "))]
    Cycle {
        paths: NEVec<Either<Vec<NodeIndex>, (NodeIndex, NodeIndex)>>,
    },
    #[error("{reason}: {}", indices(nodes, ", "))]
    Refused {
        reason: Refusal,
        nodes: NEBTreeSet<NodeIndex>,
    },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn evaluation<N, E>(graph: StableDiGraph<N, E>) -> Result<Evaluation<N, E>, DependencyError> { cycles::cycle(&graph).map_or_else(|| Ok(acyclic(graph)), Err) }

pub fn connect<N, E>(evaluation: &mut Evaluation<N, E>, edges: impl IntoIterator<Item = (NodeIndex, NodeIndex, E)>, inputs: Inputs) -> Result<Connected<E>, DependencyError>
where E: 'static {
    fn insert<N, E>(evaluation: &mut Evaluation<N, E>, edges: impl IntoIterator<Item = (NodeIndex, NodeIndex, E)>) -> Result<Vec<EdgeIndex>, DependencyError>
    where E: 'static {
        let (added, refused): (Vec<_>, Vec<_>) = edges.into_iter().partition_map(|(source, target, weight)| match evaluation.try_add_edge(source, target, weight) {
            Ok(edge) => Either::Left(edge),
            Err(AcyclicEdgeError::Cycle(_) | AcyclicEdgeError::SelfLoop | AcyclicEdgeError::InvalidEdge) => Either::Right((source, target)),
        });
        match NEVec::try_from_vec(refused) {
            None => Ok(added),
            Some(refused) => {
                let cycle = cycles::refused(evaluation, refused);
                disconnect(evaluation, added);
                Err(cycle)
            }
        }
    }
    let edges = edges.into_iter().collect::<Vec<_>>();
    present(evaluation, edges.iter().flat_map(|&(source, target, _)| [source, target]))?;
    let graph: &StableDiGraph<N, E> = evaluation;
    let incoming = match inputs {
        Inputs::Extend => BTreeSet::new(),
        Inputs::Replace =>
            edges.iter().map(|&(_, target, _)| target).collect::<BTreeSet<_>>().into_iter().flat_map(|target| graph.edges_directed(target, Direction::Incoming).map(|edge| edge.id())).collect(),
    };
    let replaced = if edges.len() > 1 {
        insert(&mut acyclic(unweighted(graph, |edge| !incoming.contains(&edge))), edges.iter().map(|&(source, target, _)| (source, target, ())))?;
        Some(disconnect(evaluation, incoming.iter().copied()))
    } else {
        None
    };
    insert(evaluation, edges).map(|added| Connected { added, replaced: replaced.unwrap_or_else(|| disconnect(evaluation, incoming)) })
}

pub fn disconnect<N, E>(evaluation: &mut Evaluation<N, E>, edges: impl IntoIterator<Item = EdgeIndex>) -> Edges<E> {
    edges
        .into_iter()
        .filter_map(|edge| {
            let (source, target) = evaluation.edge_endpoints(edge)?;
            evaluation.remove_edge(edge).map(|weight| (source, target, weight))
        })
        .collect()
}

pub fn remove<N, E>(evaluation: &mut Evaluation<N, E>, node: NodeIndex) -> Option<(N, Edges<E>)> {
    let graph: &StableDiGraph<N, E> = evaluation;
    let incident = graph
        .contains_node(node)
        .then(|| [Direction::Incoming, Direction::Outgoing].into_iter().flat_map(|direction| graph.edges_directed(node, direction)).map(|edge| edge.id()).collect::<Vec<_>>())?;
    let edges = disconnect(evaluation, incident);
    evaluation.remove_node(node).map(|weight| (weight, edges))
}

#[expect(clippy::expect_used, reason = "callers pass a subgraph of an acyclic graph or a graph `cycles::cycle` found acyclic, and petgraph 0.8.3 wraps a graph in `Acyclic` through `TryFrom` alone")]
fn acyclic<N, E>(graph: StableDiGraph<N, E>) -> Evaluation<N, E> { Acyclic::try_from(graph).expect("an acyclic graph") }

fn present<N, E>(graph: &StableDiGraph<N, E>, nodes: impl IntoIterator<Item = NodeIndex>) -> Result<(), DependencyError> {
    NEBTreeSet::try_from_set(nodes.into_iter().filter(|&node| !graph.contains_node(node)).collect()).map_or(Ok(()), |nodes| Err(DependencyError::Refused { reason: Refusal::Missing, nodes }))
}

fn pass<N, E, V>(evaluation: &Evaluation<N, E>, direction: Direction, mut step: impl FnMut(NodeIndex, &[Option<V>]) -> V) -> Vec<Option<V>> {
    let ordered = match direction {
        Direction::Outgoing => Either::Left(evaluation.nodes_iter()),
        Direction::Incoming => Either::Right(evaluation.nodes_iter().collect::<Vec<_>>().into_iter().rev()),
    };
    ordered.fold(dense(evaluation.node_bound(), []), |values, node| {
        let value = step(node, &values);
        placed(values, (node, value))
    })
}

fn dense<T>(bound: usize, pairs: impl IntoIterator<Item = (NodeIndex, T)>) -> Vec<Option<T>> { pairs.into_iter().fold(iter::repeat_with(|| None).take(bound).collect(), placed) }

fn placed<T>(mut slots: Vec<Option<T>>, (node, value): (NodeIndex, T)) -> Vec<Option<T>> {
    slots[node.index()] = Some(value);
    slots
}

fn at<T>(slots: &[Option<T>], node: NodeIndex) -> Option<T>
where T: Copy {
    slots.get(node.index()).copied().flatten()
}

fn entries<T>(slots: &[Option<T>]) -> impl Iterator<Item = (NodeIndex, &T)> { slots.iter().enumerate().filter_map(|(index, value)| value.as_ref().map(|value| (NodeIndex::new(index), value))) }

fn unweighted<N, E>(graph: &StableDiGraph<N, E>, kept: impl Fn(EdgeIndex) -> bool) -> StableDiGraph<(), ()> { graph.filter_map(|_, _| Some(()), |edge, _| kept(edge).then_some(())) }

fn compact<N, E>(graph: &StableDiGraph<N, E>) -> DiGraph<NodeIndex, EdgeIndex> { DiGraph::from(graph.map(|node, _| node, |edge, _| edge)) }

fn indices<'nodes>(nodes: impl IntoIterator<Item = &'nodes NodeIndex>, separator: &'static str) -> impl fmt::Display { nodes.into_iter().map(|node| node.index()).format(separator) }

// --- [COMPOSITION] ---------------------------------------------------------------------

mod critical;
pub mod cycles;
mod invalidation;
mod order;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use critical::{Activity, Constraint, Event, Float, Precedence, Schedule, schedule};
pub use enum_map;
pub use invalidation::{Outcome, Propagation, Ready, propagate, reach};
pub use itertools;
pub use nonempty_collections;
pub use num_traits;
pub use order::{layers, redundant, topological};
pub use petgraph;
pub use rustworkx_core;
