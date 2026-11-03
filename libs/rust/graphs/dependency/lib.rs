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

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Connected<E> {
    pub added: Vec<EdgeIndex>,
    pub replaced: Edges<E>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, thiserror::Error)]
pub enum DependencyError {
    #[error("dependency cycles {}", paths.iter().map(|path| indices(iter::chain(path, path.first()), " -> ")).format("; "))]
    Cycle { paths: NEVec<Vec<NodeIndex>> },
    #[error("nodes outside the graph: {}", indices(nodes, ", "))]
    Missing { nodes: NEBTreeSet<NodeIndex> },
    #[error("activities with negative duration: {}", indices(nodes, ", "))]
    Duration { nodes: NEBTreeSet<NodeIndex> },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn evaluation<N, E>(graph: StableDiGraph<N, E>) -> Result<Evaluation<N, E>, DependencyError> { cycles::cycle(&graph).map_or_else(|| Ok(acyclic(graph)), Err) }

pub fn connect<N, E>(evaluation: &mut Evaluation<N, E>, edges: impl IntoIterator<Item = (NodeIndex, NodeIndex, E)>, inputs: Inputs) -> Result<Connected<E>, DependencyError>
where
    N: 'static,
    E: 'static,
{
    let edges = edges.into_iter().collect::<Vec<_>>();
    present(evaluation, edges.iter().flat_map(|&(source, target, _)| [source, target]))?;
    let graph: &StableDiGraph<N, E> = evaluation;
    let targets = edges.iter().map(|&(_, target, _)| target).collect::<BTreeSet<_>>();
    let incoming = match inputs {
        Inputs::Extend => BTreeSet::new(),
        Inputs::Replace => targets.iter().flat_map(|&target| graph.edges_directed(target, Direction::Incoming).map(|edge| edge.id())).collect(),
    };
    if targets.len() > 1 && !incoming.is_empty() {
        attach(&mut acyclic(unweighted(graph, |edge| !incoming.contains(&edge))), edges.iter().map(|&(source, target, _)| (source, target, ())))?;
        let replaced = disconnect(evaluation, incoming);
        attach(evaluation, edges).map(|added| Connected { added, replaced })
    } else {
        attach(evaluation, edges).map(|added| Connected { added, replaced: disconnect(evaluation, incoming) })
    }
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
    let incident = graph.contains_node(node).then(|| [Direction::Incoming, Direction::Outgoing].into_iter().flat_map(|direction| graph.edges_directed(node, direction)).map(|edge| edge.id()).collect::<Vec<_>>())?;
    let edges = disconnect(evaluation, incident);
    evaluation.remove_node(node).map(|weight| (weight, edges))
}

fn acyclic<N, E>(graph: StableDiGraph<N, E>) -> Evaluation<N, E> { Acyclic::try_from(graph).unwrap_or_default() }

fn attach<N, E>(evaluation: &mut Evaluation<N, E>, edges: impl IntoIterator<Item = (NodeIndex, NodeIndex, E)>) -> Result<Vec<EdgeIndex>, DependencyError>
where
    N: 'static,
    E: 'static,
{
    let (added, refused): (Vec<_>, Vec<_>) = edges.into_iter().map(|edge| link(evaluation, edge)).partition_result();
    match NEVec::try_from_vec(refused) {
        None => Ok(added),
        Some(refused) => {
            let cycle = cycles::refused(evaluation, refused);
            disconnect(evaluation, added);
            Err(cycle)
        }
    }
}

fn link<N, E>(evaluation: &mut Evaluation<N, E>, (source, target, weight): (NodeIndex, NodeIndex, E)) -> Result<EdgeIndex, (NodeIndex, NodeIndex)> {
    evaluation.try_add_edge(source, target, weight).map_err(|(AcyclicEdgeError::Cycle(_) | AcyclicEdgeError::SelfLoop | AcyclicEdgeError::InvalidEdge)| (source, target))
}

fn present<N, E>(graph: &StableDiGraph<N, E>, nodes: impl IntoIterator<Item = NodeIndex>) -> Result<(), DependencyError> {
    NEBTreeSet::try_from_set(nodes.into_iter().filter(|&node| !graph.contains_node(node)).collect()).map_or(Ok(()), |nodes| Err(DependencyError::Missing { nodes }))
}

fn pass<N, E, V>(evaluation: &Evaluation<N, E>, direction: Direction, mut step: impl FnMut(NodeIndex, &[Option<V>]) -> V) -> Vec<Option<V>> {
    let sequence = evaluation.nodes_iter().collect::<Vec<_>>();
    let ordered = match direction {
        Direction::Outgoing => Either::Left(sequence.into_iter()),
        Direction::Incoming => Either::Right(sequence.into_iter().rev()),
    };
    ordered.fold(dense(evaluation.node_bound(), []), |values, node| {
        let value = step(node, &values);
        placed(values, (node, value))
    })
}

fn dense<T>(bound: usize, pairs: impl IntoIterator<Item = (NodeIndex, T)>) -> Vec<Option<T>> { pairs.into_iter().fold(iter::repeat_with(|| None).take(bound).collect(), placed) }

fn placed<T>(mut slots: Vec<Option<T>>, (node, value): (NodeIndex, T)) -> Vec<Option<T>> {
    if let Some(slot) = slots.get_mut(node.index()) {
        *slot = Some(value);
    }
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

pub mod critical;
pub mod cycles;
pub mod invalidation;
pub mod order;
