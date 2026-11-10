#![feature(cmp_minmax, default_field_values, iterator_try_collect, trait_alias)]

use core::fmt;
use core::hash::Hash;

use itertools::Itertools as _;
use leiden_rs::LeidenError;
use nonempty_collections::NEVec;
use petgraph::data::{Element, FromElements as _};
use petgraph::graph::{Graph, NodeIndex};
use petgraph::visit::{EdgeRef as _, GraphBase, GraphProp, IntoEdgeReferences, IntoNodeIdentifiers, NodeIndexable, VisitMap as _, Visitable};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Network = IntoNodeIdentifiers + IntoEdgeReferences + NodeIndexable + GraphProp + Visitable + GraphBase<NodeId: Ord + Hash + fmt::Debug, EdgeId: Ord + Hash + fmt::Debug>;

pub type Failure<G> = NetworkError<<G as GraphBase>::NodeId, <G as GraphBase>::EdgeId>;

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Into, Serialize, Deserialize))]
pub struct Weight(f64);

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Display, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Into, Serialize, Deserialize))]
pub struct Tolerance(f64);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Refusal<N>
where N: fmt::Debug
{
    #[display("node {_0:?} is outside the network")]
    Missing(N),
    #[display("node {_0:?} is both source and sink")]
    Coincident(N),
}

#[derive(Debug, thiserror::Error)]
pub enum NetworkError<N, E>
where
    N: fmt::Debug,
    E: fmt::Debug,
{
    #[error("{}", .0.iter().format(", "))]
    Refused(NEVec<Refusal<N>>),
    #[error("turn from edge {:?} into edge {:?} at node {node:?} has an invalid cost", edges[0], edges[1])]
    Turn { edges: [E; 2], node: N, source: WeightError },
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

fn present<G>(graph: G, nodes: impl IntoIterator<Item = G::NodeId>) -> Validated<Vec<G::NodeId>, Refusal<G::NodeId>>
where G: Network {
    let members = graph.node_identifiers().fold(graph.visit_map(), |mut members, node| {
        members.visit(node);
        members
    });
    nodes.into_iter().map(|node| members.is_visited(&node).then_some(node).ok_or(Refusal::Missing(node))).collect()
}

fn compact<G, W>(graph: G, weight: impl Fn(G::EdgeRef) -> W) -> Graph<G::NodeId, W, G::EdgeType>
where G: Network {
    let nodes = graph.node_identifiers().sorted_unstable().collect::<Vec<_>>();
    let positions = nodes.iter().enumerate().fold(vec![None; graph.node_bound()], |mut positions, (position, &node)| {
        positions[graph.to_index(node)] = Some(position);
        positions
    });
    let position = |node: G::NodeId| positions.get(graph.to_index(node)).copied().flatten();
    Graph::from_elements(
        nodes
            .iter()
            .map(|&weight| Element::Node { weight })
            .chain(graph.edge_references().filter_map(|edge| Some(Element::Edge { source: position(edge.source())?, target: position(edge.target())?, weight: weight(edge) }))),
    )
}

fn locate<N, W, Ty>(compact: &Graph<N, W, Ty>, node: N) -> Validated<NodeIndex, Refusal<N>>
where
    N: Ord + Copy + fmt::Debug,
    Ty: petgraph::EdgeType,
{
    compact.raw_nodes().binary_search_by_key(&node, |slot| slot.weight).ok().map(NodeIndex::new).ok_or(Refusal::Missing(node)).into()
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
pub use partition::{Communities, Trace, communities};
pub use paths::{Catchment, Reached, Route, Tree, catchment, route, segments};
pub use spanning::{steiner, tree};
