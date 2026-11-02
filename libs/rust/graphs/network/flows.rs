use core::iter;
use std::collections::{BTreeMap, BTreeSet};

use petgraph::{
    Directed, EdgeType,
    algo::dinics,
    graphmap::DiGraphMap,
    stable_graph::{EdgeIndex, NodeIndex, StableGraph, StableUnGraph},
    visit::{Bfs, EdgeRef as _, NodeIndexable as _, Walker as _},
};
use rustworkx_core::max_weight_matching::max_weight_matching;

use crate::{NetworkError, Refusal, Weight, compact, connectivity::Cut, locate};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Cardinality {
    Any,
    Maximum,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Flow {
    pub cut: Cut,
    pub flows: BTreeMap<EdgeIndex, f64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn maximum<N, E, Ty>(graph: &StableGraph<N, E, Ty>, source: NodeIndex, sink: NodeIndex, capacity: impl Fn(&E) -> Weight) -> Result<Flow, NetworkError>
where Ty: EdgeType {
    let compacted = compact(graph, |edge| capacity(edge).into_inner());
    let [from, to] = [source, sink].try_map(|node| locate(&compacted, node))?;
    (from != to).ok_or(NetworkError::Refused { node: source, reason: Refusal::Coincident })?;
    let mut network = compacted.into_edge_type::<Directed>();
    let reverse = network.edge_references().filter(|_| !Ty::is_directed()).map(|arc| (arc.target(), arc.source(), *arc.weight())).collect::<Vec<_>>();
    network.extend_with_edges(reverse);
    let (value, flows) = dinics(&network, from, to);
    let residual = DiGraphMap::<NodeIndex, ()>::from_edges(
        network
            .edge_references()
            .zip(&flows)
            .flat_map(|(arc, &flow)| [(arc.weight() - flow > 0.0).then_some((arc.source(), arc.target())), (flow > 0.0).then_some((arc.target(), arc.source()))].into_iter().flatten()),
    );
    Ok(Flow {
        cut: Cut { value, side: Bfs::new(&residual, from).iter(&residual).filter_map(|node| network.node_weight(node).copied()).collect() },
        flows: graph
            .edge_indices()
            .zip(&flows)
            .zip(flows.iter().skip(graph.edge_count()).chain(iter::repeat(&0.0)))
            .map(|((edge, forward), backward)| (edge, forward - backward))
            .filter(|&(_, net)| net != 0.0)
            .collect(),
    })
}

#[must_use]
pub fn matching<N, E>(graph: &StableUnGraph<N, E>, weight: impl Fn(&E) -> i128, cardinality: Cardinality) -> BTreeSet<EdgeIndex> {
    let Ok(pairs) = max_weight_matching(graph, cardinality == Cardinality::Maximum, |edge| Ok::<_, !>(weight(edge.weight())), false);
    pairs.into_iter().filter_map(|(left, right)| graph.edges_connecting(graph.from_index(left), graph.from_index(right)).max_by_key(|edge| weight(edge.weight())).map(|edge| edge.id())).collect()
}
