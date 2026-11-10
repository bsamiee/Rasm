use core::borrow::Borrow;
use core::iter;
use std::collections::{BTreeMap, BTreeSet};

use petgraph::algo::dinics;
use petgraph::graph::NodeIndex;
use petgraph::graphmap::DiGraphMap;
use petgraph::visit::{Bfs, EdgeCount, EdgeRef as _, GraphProp, IntoEdges, NodeCount, Walker as _};
use petgraph::{Directed, Undirected};
use rustworkx_core::max_weight_matching::max_weight_matching;
use validated::Validated;

use crate::connectivity::Cut;
use crate::{Failure, Network, NetworkError, Refusal, Weight, compact, locate};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Cardinality {
    Any,
    Maximum,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Flow<N, E> {
    pub cut: Cut<N, E>,
    pub flows: BTreeMap<E, f64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn maximum<G>(
    graph: G,
    sources: impl IntoIterator<Item: Borrow<G::NodeId>>,
    sinks: impl IntoIterator<Item: Borrow<G::NodeId>>,
    capacity: impl Fn(G::EdgeRef) -> Weight,
) -> Result<Flow<G::NodeId, G::EdgeId>, Failure<G>>
where
    G: Network,
{
    let compacted = compact(graph, |edge| capacity(edge).into_inner());
    let supply = sources.into_iter().map(|node| *node.borrow()).collect::<BTreeSet<_>>();
    let demand = sinks.into_iter().map(|node| *node.borrow()).collect::<BTreeSet<_>>();
    let located = |nodes: &BTreeSet<G::NodeId>| nodes.iter().map(|&node| locate(&compacted, node)).collect::<Validated<Vec<_>, _>>();
    let coincident = supply.intersection(&demand).map(|&node| Validated::<(), _>::fail(Refusal::Coincident(node))).collect::<Validated<(), _>>();
    let (from, to) = located(&supply).map3(located(&demand), coincident, |from, to, ()| (from, to)).ok().map_err(NetworkError::Refused)?;
    let mut network = compacted.map(|_, &node| Some(node), |_, &arc| arc).into_edge_type::<Directed>();
    if !graph.is_directed() {
        let reverse = network.edge_references().map(|arc| (arc.target(), arc.source(), *arc.weight())).collect::<Vec<_>>();
        network.extend_with_edges(reverse);
    }
    let [origin, terminus] = [(); 2].map(|()| network.add_node(None));
    network.extend_with_edges(from.into_iter().map(|source| (origin, source, f64::INFINITY)).chain(to.into_iter().map(|sink| (sink, terminus, f64::INFINITY))));
    let (value, flows) = dinics(&network, origin, terminus);
    let residual = DiGraphMap::<NodeIndex, ()>::from_edges(
        network
            .edge_references()
            .zip(&flows)
            .flat_map(|(arc, &flow)| [(flow < *arc.weight()).then_some((arc.source(), arc.target())), (flow > 0.0).then_some((arc.target(), arc.source()))].into_iter().flatten()),
    );
    let side = Bfs::new(&residual, origin).iter(&residual).filter_map(|node| network[node]).collect::<BTreeSet<_>>();
    let backward = (!graph.is_directed()).then(|| flows.iter().skip(compacted.edge_count())).into_iter().flatten().chain(iter::repeat(&0.0));
    let edges = graph
        .edge_references()
        .filter(|edge| match (side.contains(&edge.source()), side.contains(&edge.target())) {
            (true, false) => true,
            (false, true) => !graph.is_directed(),
            (true, true) | (false, false) => false,
        })
        .map(|edge| edge.id())
        .collect();
    Ok(Flow {
        cut: Cut { value, side, edges },
        flows: graph.edge_references().zip(iter::zip(&flows, backward)).map(|(edge, (forward, backward))| (edge.id(), forward - backward)).filter(|&(_, net)| net != 0.0).collect(),
    })
}

pub fn matching<G>(graph: G, weight: impl Fn(G::EdgeRef) -> i128, cardinality: Cardinality) -> Result<BTreeSet<G::EdgeId>, Failure<G>>
where G: Network + IntoEdges + NodeCount + EdgeCount + GraphProp<EdgeType = Undirected> {
    graph.edge_references().try_fold(0_i128, |total, edge| weight(edge).checked_abs().and_then(|held| total.checked_add(held)?.checked_add(held))).ok_or(NetworkError::Overflow)?;
    let pairs = max_weight_matching(graph, cardinality == Cardinality::Maximum, |edge| Ok::<_, !>(weight(edge)), false).into_ok();
    Ok(pairs
        .into_iter()
        .filter_map(|(left, right)| {
            let (left, right) = (graph.from_index(left), graph.from_index(right));
            graph.edges(left).filter(|edge| edge.target() == right).max_by_key(|&edge| weight(edge)).map(|edge| edge.id())
        })
        .collect())
}
