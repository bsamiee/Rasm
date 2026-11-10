use core::cmp;
use std::collections::{BTreeMap, BTreeSet};

use itertools::Itertools as _;
use ordered_float::OrderedFloat;
use petgraph::Undirected;
use petgraph::algo::kosaraju_scc;
use petgraph::graphmap::UnGraphMap;
use petgraph::visit::{EdgeRef as _, IntoNeighborsDirected};
use rustworkx_core::connectivity::{articulation_points, connected_components, stoer_wagner_min_cut};

use crate::{Network, Weight, compact};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Connectedness {
    Weak,
    Strong,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Biconnected<N, E> {
    pub articulation_points: BTreeSet<N>,
    pub bridges: BTreeSet<E>,
    pub blocks: Vec<BTreeSet<E>>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Cut<N, E> {
    pub value: f64,
    pub side: BTreeSet<N>,
    pub edges: BTreeSet<E>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn components<G>(graph: G, connectedness: Connectedness) -> Vec<BTreeSet<G::NodeId>>
where G: Network + IntoNeighborsDirected {
    match connectedness {
        Connectedness::Weak => connected_components(graph).into_iter().map(BTreeSet::from_iter).collect(),
        Connectedness::Strong => kosaraju_scc(graph).into_iter().map(BTreeSet::from_iter).collect(),
    }
}

#[must_use]
pub fn biconnected<G>(graph: G) -> Biconnected<G::NodeId, G::EdgeId>
where G: Network {
    let parallels = graph
        .edge_references()
        .filter(|edge| edge.source() != edge.target())
        .map(|edge| (cmp::minmax(edge.source(), edge.target()), edge.id()))
        .into_group_map()
        .into_iter()
        .collect::<BTreeMap<_, _>>();
    let mut labels = <_>::default();
    let points = articulation_points(&UnGraphMap::<_, ()>::from_edges(parallels.keys().copied().map(<(G::NodeId, G::NodeId)>::from)), Some(&mut labels));
    let blocks = labels
        .into_iter()
        .flat_map(|((source, target), label)| parallels.get(&cmp::minmax(source, target)).into_iter().flatten().map(move |&edge| (label, edge)))
        .into_grouping_map()
        .collect::<BTreeSet<_>>()
        .into_iter()
        .sorted_unstable_by_key(|&(label, _)| label)
        .map(|(_, block)| block)
        .collect::<Vec<_>>();
    Biconnected { articulation_points: points.into_iter().collect(), bridges: blocks.iter().filter(|block| block.len() == 1).flatten().copied().collect(), blocks }
}

#[must_use]
pub fn min_cut<G>(graph: G, weight: impl Fn(G::EdgeRef) -> Weight) -> Option<Cut<G::NodeId, G::EdgeId>>
where G: Network {
    let network = compact(graph, |edge| (OrderedFloat(weight(edge).into_inner()), edge.id())).into_edge_type::<Undirected>();
    let cut = stoer_wagner_min_cut(&network, |edge| Ok::<_, !>(edge.weight().0)).into_ok();
    cut.map(|(OrderedFloat(value), side)| {
        let side = side.into_iter().collect::<BTreeSet<_>>();
        let edges = network.edge_references().filter(|edge| side.contains(&edge.source()) != side.contains(&edge.target())).map(|edge| edge.weight().1).collect();
        Cut { value, side: side.into_iter().map(|node| network[node]).collect(), edges }
    })
}
