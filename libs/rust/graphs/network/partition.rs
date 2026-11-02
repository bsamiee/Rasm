use std::collections::BTreeMap;

use indexmap::IndexSet;
use leiden_rs::{GraphDataBuilder, HierarchicalOutput, Leiden, LeidenConfig, LeidenOutput, Partition};
use petgraph::{
    EdgeType,
    stable_graph::{EdgeReference, NodeIndex, StableGraph},
};
use validated::Validated;

use crate::{NetworkError, Weight, compact, present};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Communities {
    pub membership: BTreeMap<NodeIndex, usize>,
    pub quality: f64,
    pub levels: Vec<(BTreeMap<NodeIndex, usize>, f64)>,
    pub history: Vec<f64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn communities<N, E, Ty>(
    graph: &StableGraph<N, E, Ty>,
    initial: &BTreeMap<NodeIndex, usize>,
    weight: impl Fn(EdgeReference<'_, E>) -> Weight,
    mass: impl Fn(&N) -> Weight,
    config: LeidenConfig,
) -> Result<Communities, NetworkError>
where
    Ty: EdgeType,
{
    initial.keys().map(|&node| present(graph, node).map(drop)).collect::<Validated<(), _>>().ok().map_err(NetworkError::Refused)?;
    let network = compact(graph, |edge| weight(edge).into_inner());
    let builder = GraphDataBuilder::new(network.node_count());
    let mut builder = if Ty::is_directed() { builder.directed() } else { builder };
    network.raw_edges().iter().try_for_each(|arc| builder.add_edge(arc.source().index(), arc.target().index(), arc.weight).map(drop))?;
    network.node_weights().enumerate().try_for_each(|(position, &node)| builder.set_node_weight(position, mass(&graph[node]).into_inner()).map(drop))?;
    let data = builder.build()?;
    let mut labels = IndexSet::new();
    let seed = network.node_weights().map(|&node| labels.insert_full(initial.get(&node).copied().ok_or(node)).0).collect::<Vec<_>>();
    let leiden = Leiden::new(config);
    let membership = |communities: &[usize]| network.node_weights().copied().zip(communities.iter().copied()).collect::<BTreeMap<_, _>>();
    Ok(if labels.len() == seed.len() {
        let HierarchicalOutput { levels, partition, quality, .. } = leiden.run_hierarchical(&data)?;
        Communities { membership: membership(partition.as_slice()), quality, levels: levels.iter().map(|level| (membership(&level.membership), level.quality)).collect(), history: Vec::new() }
    } else {
        let LeidenOutput { partition, quality, quality_history, .. } = leiden.run_with_initial_partition(&data, Partition::from_membership(seed))?;
        Communities { membership: membership(partition.as_slice()), quality, levels: Vec::new(), history: quality_history }
    })
}
