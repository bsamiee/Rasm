use std::collections::BTreeMap;

use indexmap::IndexSet;
use leiden_rs::{GraphDataBuilder, HierarchicalOutput, Leiden, LeidenConfig, LeidenOutput, Partition};
use validated::Validated;

use crate::{Failure, Network, NetworkError, Weight, compact, locate};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Communities<N> {
    pub membership: BTreeMap<N, usize>,
    pub quality: f64,
    pub trace: Trace<N>,
}

#[derive(Debug, Clone, PartialEq)]
pub enum Trace<N> {
    Levels(Vec<(BTreeMap<N, usize>, f64)>),
    History(Vec<f64>),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn communities<G>(
    graph: G,
    initial: &BTreeMap<G::NodeId, usize>,
    weight: impl Fn(G::EdgeRef) -> Weight,
    mass: impl Fn(G::NodeId) -> Weight,
    config: LeidenConfig,
) -> Result<Communities<G::NodeId>, Failure<G>>
where
    G: Network,
{
    let network = compact(graph, |edge| weight(edge).into_inner());
    initial.keys().map(|&node| locate(&network, node).map(drop)).collect::<Validated<(), _>>().ok().map_err(NetworkError::Refused)?;
    let builder = GraphDataBuilder::new(network.node_count());
    let mut builder = if graph.is_directed() { builder.directed() } else { builder };
    network.raw_edges().iter().try_for_each(|arc| builder.add_edge(arc.source().index(), arc.target().index(), arc.weight).map(drop))?;
    network.node_weights().enumerate().try_for_each(|(position, &node)| builder.set_node_weight(position, mass(node).into_inner()).map(drop))?;
    let data = builder.build()?;
    let mut labels = IndexSet::new();
    let seed = network.node_weights().map(|&node| labels.insert_full(initial.get(&node).copied().ok_or(node)).0).collect::<Vec<_>>();
    let leiden = Leiden::new(config);
    let membership = |communities: &[usize]| network.node_weights().copied().zip(communities.iter().copied()).collect::<BTreeMap<_, _>>();
    Ok(if labels.len() == seed.len() {
        let HierarchicalOutput { levels, partition, quality, .. } = leiden.run_hierarchical(&data)?;
        Communities { membership: membership(partition.as_slice()), quality, trace: Trace::Levels(levels.iter().map(|level| (membership(&level.membership), level.quality)).collect()) }
    } else {
        let LeidenOutput { partition, quality, quality_history, .. } = leiden.run_with_initial_partition(&data, Partition::from_membership(seed))?;
        Communities { membership: membership(partition.as_slice()), quality, trace: Trace::History(quality_history) }
    })
}
