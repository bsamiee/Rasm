#![feature(array_try_map, cmp_minmax, default_field_values, iterator_try_collect)]

use leiden_rs::LeidenError;
use petgraph::{
    EdgeType,
    graph::Graph,
    stable_graph::{EdgeIndex, NodeIndex, StableGraph},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Refusal {
    #[display("outside the network")]
    Missing,
    #[display("both source and sink")]
    Coincident,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Into, Serialize, Deserialize))]
pub struct Weight(f64);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum NetworkError {
    #[error("node {} is {reason}", node.index())]
    Refused { node: NodeIndex, reason: Refusal },
    #[error("turn from edge {} into edge {} at node {}", edges[0].index(), edges[1].index(), node.index())]
    Turn { edges: [EdgeIndex; 2], node: NodeIndex, source: WeightError },
    #[error("power iteration found no dominant eigenvector")]
    Unconverged,
    #[error(transparent)]
    Partition(#[from] LeidenError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn present<N, E, Ty>(graph: &StableGraph<N, E, Ty>, node: NodeIndex) -> Result<NodeIndex, NetworkError>
where Ty: EdgeType {
    graph.contains_node(node).then_some(node).ok_or(NetworkError::Refused { node, reason: Refusal::Missing })
}

fn compact<N, E, Ty, W>(graph: &StableGraph<N, E, Ty>, weight: impl Fn(&E) -> W) -> Graph<NodeIndex, W, Ty>
where Ty: EdgeType {
    Graph::from(graph.map(|node, _| node, |_, edge| weight(edge)))
}

fn locate<W, Ty>(compact: &Graph<NodeIndex, W, Ty>, node: NodeIndex) -> Result<NodeIndex, NetworkError>
where Ty: EdgeType {
    compact.raw_nodes().binary_search_by_key(&node, |slot| slot.weight).ok().map(NodeIndex::new).ok_or(NetworkError::Refused { node, reason: Refusal::Missing })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod centrality;
pub mod connectivity;
pub mod flows;
pub mod partition;
pub mod paths;
pub mod spanning;
