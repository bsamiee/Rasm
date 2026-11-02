use core::borrow::Borrow;
use std::collections::BTreeSet;

use petgraph::{
    algo::min_spanning_tree,
    data::Element,
    stable_graph::{EdgeIndex, NodeIndex, StableUnGraph},
    visit::EdgeRef as _,
};
use rustworkx_core::steiner_tree::{SteinerTreeResult, steiner_tree};

use crate::{NetworkError, Weight, compact, locate};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn tree<N, E, W>(graph: &StableUnGraph<N, E>, weight: impl Fn(&E) -> W) -> BTreeSet<EdgeIndex>
where W: Ord + Clone {
    min_spanning_tree(&graph.map(|_, _| (), |edge, value| (weight(value), edge)))
        .filter_map(|element| match element {
            Element::Edge { source: _, target: _, weight: (_, edge) } => Some(edge),
            Element::Node { weight: () } => None,
        })
        .collect()
}

pub fn steiner<N, E>(graph: &StableUnGraph<N, E>, terminals: impl IntoIterator<Item: Borrow<NodeIndex>>, cost: impl Fn(&E) -> Weight) -> Result<Option<BTreeSet<EdgeIndex>>, NetworkError> {
    let network = compact(graph, &cost);
    let located = terminals.into_iter().map(|node| locate(&network, *node.borrow())).try_collect::<BTreeSet<_>>()?;
    let Ok(tree) = steiner_tree(&network, &located.into_iter().collect::<Vec<_>>(), |edge| Ok::<f64, !>(edge.weight().into_inner()));
    Ok(tree.map(|SteinerTreeResult { used_node_indices: _, used_edge_endpoints }| {
        used_edge_endpoints
            .into_iter()
            .filter_map(|ends| {
                let [source, target] = <[usize; 2]>::from(ends).try_map(|end| network.node_weight(NodeIndex::new(end)).copied())?;
                graph.edges_connecting(source, target).min_by_key(|edge| cost(edge.weight())).map(|edge| edge.id())
            })
            .collect()
    }))
}
