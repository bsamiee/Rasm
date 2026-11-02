use std::collections::BTreeMap;

use indexmap::IndexSet;
use leiden_rs::{GraphDataBuilder, HierarchicalOutput, Leiden, LeidenConfig, LeidenOutput, Partition};
use petgraph::{
    EdgeType,
    stable_graph::{EdgeReference, NodeIndex, StableGraph},
    visit::{EdgeRef as _, IntoEdgeReferences as _},
};

use crate::{NetworkError, Weight, present, rank};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Communities {
    pub membership: BTreeMap<NodeIndex, usize>,
    pub quality: f64,
    pub levels: Vec<BTreeMap<NodeIndex, usize>>,
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
    initial.keys().try_for_each(|&node| present(graph, node).map(drop))?;
    let nodes = graph.node_indices().collect::<Vec<_>>();
    let builder = GraphDataBuilder::new(nodes.len());
    let mut builder = if Ty::is_directed() { builder.directed() } else { builder };
    graph.edge_references().try_for_each(|edge| builder.add_edge(rank(&nodes, edge.source()), rank(&nodes, edge.target()), weight(edge).into_inner()).map(drop))?;
    nodes.iter().enumerate().try_for_each(|(position, &node)| builder.set_node_weight(position, mass(&graph[node]).into_inner()).map(drop))?;
    let data = builder.build()?;
    let mut labels = IndexSet::new();
    let seed = nodes.iter().map(|&node| labels.insert_full(initial.get(&node).copied().ok_or(node)).0).collect::<Vec<_>>();
    let leiden = Leiden::new(config);
    let membership = |communities: &[usize]| nodes.iter().copied().zip(communities.iter().copied()).collect::<BTreeMap<_, _>>();
    Ok(if labels.len() == seed.len() {
        let HierarchicalOutput { levels, partition, quality, .. } = leiden.run_hierarchical(&data)?;
        Communities {
            membership: membership(partition.as_slice()),
            quality,
            levels: levels.iter().map(|level| membership(&level.membership)).collect(),
            history: levels.iter().map(|level| level.quality).collect(),
        }
    } else {
        let LeidenOutput { partition, quality, quality_history, .. } = leiden.run_with_initial_partition(&data, Partition::from_membership(seed))?;
        Communities { membership: membership(partition.as_slice()), quality, levels: Vec::new(), history: quality_history }
    })
}
