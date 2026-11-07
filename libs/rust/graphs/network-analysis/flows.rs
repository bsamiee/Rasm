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
use validated::Validated;

use crate::{NetworkError, Refusal, Tolerance, Weight, compact, connectivity::Cut, locate};

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
    let supply = sources.into_iter().map(|node| *node.borrow()).collect::<BTreeSet<_>>();
    let demand = sinks.into_iter().map(|node| *node.borrow()).collect::<BTreeSet<_>>();
    let located = |nodes: &BTreeSet<NodeIndex>| nodes.iter().map(|&node| locate(&compacted, node)).collect::<Validated<Vec<_>, _>>();
    let coincident = supply.intersection(&demand).map(|&node| Validated::<(), _>::fail((node, Refusal::Coincident))).collect::<Validated<(), _>>();
    let (from, to) = located(&supply).map3(located(&demand), coincident, |from, to, ()| (from, to)).ok().map_err(NetworkError::Refused)?;
    let bound = compacted.edge_weights().try_fold(1_u64, |total, &arc| total.checked_add(arc)).ok_or(NetworkError::Overflow)?;
    let mut network = compacted.map(|_, &node| Some(node), |_, &arc| arc).into_edge_type::<Directed>();
    if !Ty::is_directed() {
        let reverse = network.edge_references().map(|arc| (arc.target(), arc.source(), *arc.weight())).collect::<Vec<_>>();
        network.extend_with_edges(reverse);
    }
    let [origin, terminus] = [(); 2].map(|()| network.add_node(None));
    network.extend_with_edges(from.into_iter().map(|source| (origin, source, bound)).chain(to.into_iter().map(|sink| (sink, terminus, bound))));
    let (value, flows) = dinics(&network, origin, terminus);
    let residual = DiGraphMap::<NodeIndex, ()>::from_edges(
        network
            .edge_references()
            .zip(&flows)
            .flat_map(|(arc, &flow)| [(flow < *arc.weight()).then_some((arc.source(), arc.target())), (flow > 0).then_some((arc.target(), arc.source()))].into_iter().flatten()),
    );
    let side = Bfs::new(&residual, origin).iter(&residual).filter_map(|node| network[node]).collect::<BTreeSet<_>>();
    let scaled = |amount: u64| convert::<u64, f64>(amount) * resolution.into_inner();
    let backward = (!Ty::is_directed()).then(|| flows.iter().skip(graph.edge_count())).into_iter().flatten().chain(iter::repeat(&0));
    let edges = graph
        .edge_references()
        .filter(|edge| match (side.contains(&edge.source()), side.contains(&edge.target())) {
            (true, false) => true,
            (false, true) => !Ty::is_directed(),
            (true, true) | (false, false) => false,
        })
        .map(|edge| edge.id())
        .collect();
    Ok(Flow {
        cut: Cut { value: scaled(value), side, edges },
        flows: graph
            .edge_indices()
            .zip(iter::zip(&flows, backward))
            .filter(|&(_, (forward, backward))| forward != backward)
            .map(|(edge, (&forward, &backward))| (edge, scaled(forward) - scaled(backward)))
            .collect(),
    })
}

pub fn matching<N, E>(graph: &StableUnGraph<N, E>, weight: impl Fn(EdgeReference<'_, E>) -> i128, cardinality: Cardinality) -> Result<BTreeSet<EdgeIndex>, NetworkError> {
    graph.edge_references().try_fold(0_i128, |total, edge| weight(edge).checked_abs().and_then(|held| total.checked_add(held)?.checked_add(held))).ok_or(NetworkError::Overflow)?;
    let pairs = max_weight_matching(graph, cardinality == Cardinality::Maximum, |edge| Ok::<_, !>(weight(edge)), false).into_ok();
    Ok(pairs.into_iter().filter_map(|(left, right)| graph.edges_connecting(graph.from_index(left), graph.from_index(right)).max_by_key(|&edge| weight(edge)).map(|edge| edge.id())).collect())
}
