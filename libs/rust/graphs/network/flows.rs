use core::{borrow::Borrow, iter};
use std::collections::{BTreeMap, BTreeSet};

use nalgebra::convert;
use petgraph::{
    Directed, EdgeType,
    algo::dinics,
    graphmap::DiGraphMap,
    stable_graph::{EdgeIndex, EdgeReference, NodeIndex, StableGraph, StableUnGraph},
    visit::{Bfs, EdgeRef as _, IntoEdgeReferences as _, NodeIndexable as _, Walker as _},
};
use rustworkx_core::max_weight_matching::max_weight_matching;

use crate::{
    NetworkError, Refusal, Tolerance, Weight, compact,
    connectivity::{Cut, severed},
    locate,
};

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

pub fn maximum<N, E, Ty>(
    graph: &StableGraph<N, E, Ty>,
    sources: impl IntoIterator<Item: Borrow<NodeIndex>>,
    sinks: impl IntoIterator<Item: Borrow<NodeIndex>>,
    capacity: impl Fn(EdgeReference<'_, E>) -> Weight,
    resolution: Tolerance,
) -> Result<Flow, NetworkError>
where
    Ty: EdgeType,
{
    let compacted = compact(graph, |edge| (capacity(edge).into_inner() / resolution.into_inner()).round().to_int_saturating::<u64>());
    let terminals = [sources.into_iter().map(|node| *node.borrow()).collect::<BTreeSet<_>>(), sinks.into_iter().map(|node| *node.borrow()).collect()];
    let [from, to] = terminals.each_ref().try_map(|nodes| nodes.iter().map(|&node| locate(&compacted, node)).try_collect::<Vec<_>>())?;
    let [supply, demand] = &terminals;
    supply.intersection(demand).next().map(|&node| NetworkError::Refused { node, reason: Refusal::Coincident }).map_or(Ok(()), Err)?;
    let bound = compacted.edge_weights().try_fold(1_u64, |total, &arc| total.checked_add(arc)).ok_or(NetworkError::Overflow { resolution })?;
    let live = compacted.node_count();
    let mut network = compacted.into_edge_type::<Directed>();
    let reverse = (!Ty::is_directed()).then(|| network.edge_references().map(|arc| (arc.target(), arc.source(), *arc.weight())).collect::<Vec<_>>());
    network.extend_with_edges(reverse.into_iter().flatten());
    let [origin, terminus] = [(); 2].map(|()| network.add_node(NodeIndex::end()));
    network.extend_with_edges(from.into_iter().map(|source| (origin, source, bound)).chain(to.into_iter().map(|sink| (sink, terminus, bound))));
    let (value, flows) = dinics(&network, origin, terminus);
    let residual = DiGraphMap::<NodeIndex, ()>::from_edges(
        network
            .edge_references()
            .zip(&flows)
            .flat_map(|(arc, &flow)| [(flow < *arc.weight()).then_some((arc.source(), arc.target())), (flow > 0).then_some((arc.target(), arc.source()))].into_iter().flatten()),
    );
    let side = Bfs::new(&residual, origin).iter(&residual).filter(|node| node.index() < live).filter_map(|node| network.node_weight(node).copied()).collect();
    let scaled = |amount: u64| convert::<u64, f64>(amount) * resolution.into_inner();
    let backward = (!Ty::is_directed()).then(|| flows.iter().skip(graph.edge_count())).into_iter().flatten().chain(iter::repeat(&0));
    Ok(Flow {
        cut: severed(graph, scaled(value), side),
        flows: graph
            .edge_indices()
            .zip(iter::zip(&flows, backward))
            .filter(|&(_, (forward, backward))| forward != backward)
            .map(|(edge, (&forward, &backward))| (edge, scaled(forward) - scaled(backward)))
            .collect(),
    })
}

pub fn matching<N, E>(graph: &StableUnGraph<N, E>, weight: impl Fn(EdgeReference<'_, E>) -> Weight, resolution: Tolerance, cardinality: Cardinality) -> Result<BTreeSet<EdgeIndex>, NetworkError> {
    let quantized = |edge: EdgeReference<'_, E>| (weight(edge).into_inner() / resolution.into_inner()).round().to_int_saturating::<i128>();
    graph.edge_references().map(quantized).try_fold(0_i128, |total, held| total.checked_add(held)?.checked_add(held)).ok_or(NetworkError::Overflow { resolution })?;
    let pairs = max_weight_matching(graph, cardinality == Cardinality::Maximum, |edge| Ok::<_, !>(quantized(edge)), false).into_ok();
    Ok(pairs.into_iter().filter_map(|(left, right)| graph.edges_connecting(graph.from_index(left), graph.from_index(right)).max_by_key(|&edge| quantized(edge)).map(|edge| edge.id())).collect())
}
