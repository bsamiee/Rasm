use std::{
    cmp,
    collections::{BTreeMap, BTreeSet},
};

use ordered_float::OrderedFloat;
use petgraph::{
    EdgeType, Undirected,
    algo::kosaraju_scc,
    stable_graph::{EdgeIndex, NodeIndex, StableGraph, StableUnGraph},
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
    let parallels = graph.edge_references().filter(|edge| edge.source() != edge.target()).fold(BTreeMap::<_, Vec<EdgeIndex>>::new(), |mut parallels, edge| {
        parallels.entry(cmp::minmax(edge.source(), edge.target())).or_default().push(edge.id());
        parallels
    });
    let mut labels = <_>::default();
    let points = articulation_points(&StableUnGraph::<(), ()>::from_edges(parallels.keys().copied().map(<(NodeIndex, NodeIndex)>::from)), Some(&mut labels));
    let blocks = labels
        .into_iter()
        .fold(BTreeMap::<usize, BTreeSet<EdgeIndex>>::new(), |mut blocks, ((source, target), label)| {
            blocks.entry(label).or_default().extend(parallels.get(&cmp::minmax(source, target)).into_iter().flatten());
            blocks
        })
        .into_values()
        .collect::<Vec<_>>();
    Biconnected { articulation_points: points.into_iter().collect(), bridges: blocks.iter().filter(|block| block.len() == 1).flatten().copied().collect(), blocks }
}

#[must_use]
pub fn min_cut<N, E, Ty>(graph: &StableGraph<N, E, Ty>, weight: impl Fn(&E) -> Weight) -> Option<Cut>
where Ty: EdgeType {
    let network = compact(graph, |edge| OrderedFloat(weight(edge).into_inner())).into_edge_type::<Undirected>();
    let Ok(cut) = stoer_wagner_min_cut(&network, |edge| Ok::<_, !>(*edge.weight()));
    cut.map(|(OrderedFloat(value), side)| Cut { value, side: side.into_iter().filter_map(|node| network.node_weight(node).copied()).collect() })
}
