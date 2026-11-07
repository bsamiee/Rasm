use core::cmp;
use std::collections::{BTreeMap, BTreeSet};

use itertools::Itertools as _;
use ordered_float::OrderedFloat;
use petgraph::{
    EdgeType, Undirected,
    algo::kosaraju_scc,
    stable_graph::{EdgeIndex, EdgeReference, NodeIndex, StableGraph, StableUnGraph},
    visit::{EdgeRef as _, IntoEdgeReferences as _},
};
use rustworkx_core::connectivity::{articulation_points, connected_components, stoer_wagner_min_cut};

use crate::{Weight, compact};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Connectedness {
    Weak,
    Strong,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Biconnected {
    pub articulation_points: BTreeSet<NodeIndex>,
    pub bridges: BTreeSet<EdgeIndex>,
    pub blocks: Vec<BTreeSet<EdgeIndex>>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Cut {
    pub value: f64,
    pub side: BTreeSet<NodeIndex>,
    pub edges: BTreeSet<EdgeIndex>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn components<N, E, Ty>(graph: &StableGraph<N, E, Ty>, connectedness: Connectedness) -> Vec<BTreeSet<NodeIndex>>
where Ty: EdgeType {
    match connectedness {
        Connectedness::Weak => connected_components(graph).into_iter().map(BTreeSet::from_iter).collect(),
        Connectedness::Strong => kosaraju_scc(graph).into_iter().map(BTreeSet::from_iter).collect(),
    }
}

#[must_use]
pub fn biconnected<N, E, Ty>(graph: &StableGraph<N, E, Ty>) -> Biconnected
where Ty: EdgeType {
    let parallels = graph
        .edge_references()
        .filter(|edge| edge.source() != edge.target())
        .map(|edge| (cmp::minmax(edge.source(), edge.target()), edge.id()))
        .into_group_map()
        .into_iter()
        .collect::<BTreeMap<_, _>>();
    let mut labels = <_>::default();
    let points = articulation_points(&StableUnGraph::<(), ()>::from_edges(parallels.keys().copied().map(<(NodeIndex, NodeIndex)>::from)), Some(&mut labels));
    let blocks = labels
        .into_iter()
        .flat_map(|((source, target), label)| parallels.get(&cmp::minmax(source, target)).into_iter().flatten().map(move |&edge| (label, edge)))
        .into_grouping_map()
        .collect::<BTreeSet<EdgeIndex>>()
        .into_iter()
        .sorted_unstable_by_key(|&(label, _)| label)
        .map(|(_, block)| block)
        .collect::<Vec<_>>();
    Biconnected { articulation_points: points.into_iter().collect(), bridges: blocks.iter().filter(|block| block.len() == 1).flatten().copied().collect(), blocks }
}

#[must_use]
pub fn min_cut<N, E, Ty>(graph: &StableGraph<N, E, Ty>, weight: impl Fn(EdgeReference<'_, E>) -> Weight) -> Option<Cut>
where Ty: EdgeType {
    let network = compact(graph, |edge| (OrderedFloat(weight(edge).into_inner()), edge.id())).into_edge_type::<Undirected>();
    let cut = stoer_wagner_min_cut(&network, |edge| Ok::<_, !>(edge.weight().0)).into_ok();
    cut.map(|(OrderedFloat(value), side)| {
        let side = side.into_iter().collect::<BTreeSet<_>>();
        let edges = network.edge_references().filter(|edge| side.contains(&edge.source()) != side.contains(&edge.target())).map(|edge| edge.weight().1).collect();
        Cut { value, side: side.into_iter().map(|node| network[node]).collect(), edges }
    })
}
