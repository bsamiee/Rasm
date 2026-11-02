use std::collections::BTreeMap;

use indexmap::IndexSet;
use leiden_rs::{HierarchicalOutput, Leiden, LeidenConfig, LeidenOutput, Partition, from_petgraph};
use petgraph::{
    EdgeType,
    stable_graph::{NodeIndex, StableGraph},
};

use crate::{NetworkError, Weight, compact};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Communities {
    pub membership: BTreeMap<NodeIndex, usize>,
    pub quality: f64,
    pub finer: Option<Box<Self>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn communities<N, E, Ty>(graph: &StableGraph<N, E, Ty>, initial: &BTreeMap<NodeIndex, usize>, weight: impl Fn(&E) -> Weight, config: LeidenConfig) -> Result<Communities, NetworkError>
where Ty: EdgeType {
    let network = compact(graph, weight);
    let data = from_petgraph(&network)?;
    let mut labels = IndexSet::new();
    let seed = network.node_weights().map(|&node| labels.insert_full(initial.get(&node).copied().ok_or(node)).0).collect::<Vec<_>>();
    let leiden = Leiden::new(config);
    let (levels, partition, quality) = if labels.len() == seed.len() {
        leiden.run_hierarchical(&data).map(|HierarchicalOutput { levels, partition, quality, .. }| (levels, partition, quality))?
    } else {
        leiden.run_with_initial_partition(&data, Partition::from_membership(seed)).map(|LeidenOutput { partition, quality, .. }| (Vec::new(), partition, quality))?
    };
    let membership = |communities: &[usize]| network.node_weights().copied().zip(communities.iter().copied()).collect::<BTreeMap<_, _>>();
    let ([levels @ .., _] | levels @ []) = levels.as_slice();
    Ok(Communities {
        membership: membership(partition.as_slice()),
        quality,
        finer: levels.iter().fold(None, |finer, level| Some(Box::new(Communities { membership: membership(&level.membership), quality: level.quality, finer }))),
    })
}
