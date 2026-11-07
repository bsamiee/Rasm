use core::borrow::Borrow;
use std::collections::BTreeSet;

use petgraph::{
    EdgeType,
    algo::min_spanning_tree,
    data::Element,
    stable_graph::{EdgeIndex, EdgeReference, NodeIndex, StableGraph, StableUnGraph},
    visit::EdgeRef as _,
};
use rustworkx_core::steiner_tree::{SteinerTreeResult, steiner_tree};
use validated::Validated;

use crate::{NetworkError, Weight, compact, locate};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn tree<N, E, Ty, W>(graph: &StableGraph<N, E, Ty>, weight: impl Fn(EdgeReference<'_, E>) -> W) -> BTreeSet<EdgeIndex>
where
    Ty: EdgeType,
    W: Ord + Clone,
{
    min_spanning_tree(&compact(graph, |edge| (weight(edge), edge.id())))
        .filter_map(|element| match element {
            Element::Edge { source: _, target: _, weight: (_, edge) } => Some(edge),
            Element::Node { weight: _ } => None,
        })
        .collect()
}

pub fn steiner<N, E>(
    graph: &StableUnGraph<N, E>,
    terminals: impl IntoIterator<Item: Borrow<NodeIndex>>,
    cost: impl Fn(EdgeReference<'_, E>) -> Weight,
) -> Result<Option<BTreeSet<EdgeIndex>>, NetworkError> {
    let network = compact(graph, |edge| (cost(edge), edge.id()));
    let located = terminals.into_iter().map(|node| locate(&network, *node.borrow())).collect::<Validated<BTreeSet<_>, _>>().ok().map_err(NetworkError::Refused)?;
    let tree = steiner_tree(&network, &located.into_iter().collect::<Vec<_>>(), |edge| Ok::<f64, !>(edge.weight().0.into_inner())).into_ok();
    Ok(tree.map(|SteinerTreeResult { used_node_indices: _, used_edge_endpoints }| {
        used_edge_endpoints
            .into_iter()
            .filter_map(|(source, target)| network.edges_connecting(NodeIndex::new(source), NodeIndex::new(target)).map(|edge| *edge.weight()).min().map(|(_, edge)| edge))
            .collect()
    }))
}
