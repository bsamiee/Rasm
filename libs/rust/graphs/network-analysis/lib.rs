#![feature(cmp_minmax, default_field_values, float_conversions, iterator_try_collect)]

use core::{fmt, iter};

use itertools::Itertools as _;
use leiden_rs::LeidenError;
use nonempty_collections::NEVec;
use petgraph::{
    EdgeType,
    data::{Element, FromElements as _},
    graph::Graph,
    stable_graph::{EdgeIndex, EdgeReference, NodeIndex, StableGraph},
    visit::IntoEdgeReferences as _,
};
use validated::Validated;

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
    #[error("{}", .0.iter().map(|&(node, reason)| fmt::from_fn(move |formatter| write!(formatter, "node {} is {reason}", node.index()))).format(", "))]
    Refused(NEVec<(NodeIndex, Refusal)>),
    #[error("turn from edge {} into edge {} at node {} has an invalid cost", edges[0].index(), edges[1].index(), node.index())]
    Turn { edges: [EdgeIndex; 2], node: NodeIndex, source: WeightError },
    #[error(
        "power iteration did not converge within {} at {}",
        fmt::from_fn(|formatter| match convergence.max_iter {
            Some(bound) => write!(formatter, "{bound} iterations"),
            None => formatter.write_str("the default iteration count"),
        }),
        fmt::from_fn(|formatter| match convergence.tol {
            Some(tol) => write!(formatter, "tolerance {tol}"),
            None => formatter.write_str("the default tolerance"),
        })
    )]
    Unconverged { convergence: Convergence },
    #[error("summed edge weights overflow the integer range of the solver")]
    Overflow,
    #[error(transparent)]
    Partition(#[from] LeidenError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn present<N, E, Ty>(graph: &StableGraph<N, E, Ty>, node: NodeIndex) -> Validated<NodeIndex, (NodeIndex, Refusal)>
where Ty: EdgeType {
    graph.contains_node(node).then_some(node).ok_or((node, Refusal::Missing)).into()
}

fn compact<N, E, Ty, W>(graph: &StableGraph<N, E, Ty>, weight: impl Fn(EdgeReference<'_, E>) -> W) -> Graph<NodeIndex, W, Ty>
where Ty: EdgeType {
    let skeleton = Graph::from(graph.map(|node, _| node, |_, _| ()));
    Graph::from_elements(skeleton.node_weights().map(|&node| Element::Node { weight: node }).chain(iter::zip(skeleton.raw_edges(), graph.edge_references()).map(|(arc, edge)| Element::Edge {
        source: arc.source().index(),
        target: arc.target().index(),
        weight: weight(edge),
    })))
}

fn locate<W, Ty>(compact: &Graph<NodeIndex, W, Ty>, node: NodeIndex) -> Validated<NodeIndex, (NodeIndex, Refusal)>
where Ty: EdgeType {
    compact.raw_nodes().binary_search_by_key(&node, |slot| slot.weight).ok().map(NodeIndex::new).ok_or((node, Refusal::Missing)).into()
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod centrality;
mod connectivity;
mod flows;
mod partition;
mod paths;
mod spanning;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use centrality::{Centrality, Convergence, eigenvector, geodesic, katz};
pub use connectivity::{Biconnected, Connectedness, Cut, biconnected, components, min_cut};
pub use flows::{Cardinality, Flow, matching, maximum};
pub use partition::{Communities, communities};
pub use paths::{Catchment, Reached, Route, catchment, route, segments};
pub use spanning::{steiner, tree};
