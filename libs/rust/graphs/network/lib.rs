#![feature(array_try_map, cmp_minmax, default_field_values, float_conversions, iterator_try_collect)]

use core::fmt;

use leiden_rs::LeidenError;
use petgraph::{
    EdgeType,
    data::{Element, FromElements as _},
    graph::Graph,
    stable_graph::{EdgeIndex, EdgeReference, NodeIndex, StableGraph},
    visit::{EdgeRef as _, IntoEdgeReferences as _},
};

use crate::centrality::Convergence;

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

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Display, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Into, Serialize, Deserialize))]
pub struct Tolerance(f64);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum NetworkError {
    #[error("node {} is {reason}", node.index())]
    Refused { node: NodeIndex, reason: Refusal },
    #[error("turn from edge {} into edge {} at node {} has an invalid cost", edges[0].index(), edges[1].index(), node.index())]
    Turn { edges: [EdgeIndex; 2], node: NodeIndex, source: WeightError },
    #[error(
        "iteration ran out unconverged under bound {} and tolerance {}",
        fmt::from_fn(|formatter| match convergence.max_iter {
            Some(bound) => write!(formatter, "{bound}"),
            None => formatter.write_str("default"),
        }),
        fmt::from_fn(|formatter| match convergence.tol {
            Some(tol) => write!(formatter, "{tol}"),
            None => formatter.write_str("default"),
        })
    )]
    Unconverged { convergence: Convergence },
    #[error("weights at resolution {resolution} overflow the integer range")]
    Overflow { resolution: Tolerance },
    #[error(transparent)]
    Partition(#[from] LeidenError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn present<N, E, Ty>(graph: &StableGraph<N, E, Ty>, node: NodeIndex) -> Result<NodeIndex, NetworkError>
where Ty: EdgeType {
    graph.contains_node(node).then_some(node).ok_or(NetworkError::Refused { node, reason: Refusal::Missing })
}

fn rank(nodes: &[NodeIndex], node: NodeIndex) -> usize { nodes.partition_point(|&held| held < node) }

fn compact<N, E, Ty, W>(graph: &StableGraph<N, E, Ty>, weight: impl Fn(EdgeReference<'_, E>) -> W) -> Graph<NodeIndex, W, Ty>
where Ty: EdgeType {
    let nodes = graph.node_indices().collect::<Vec<_>>();
    Graph::from_elements(nodes.iter().map(|&node| Element::Node { weight: node }).chain(graph.edge_references().map(|edge| Element::Edge {
        source: rank(&nodes, edge.source()),
        target: rank(&nodes, edge.target()),
        weight: weight(edge),
    })))
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
