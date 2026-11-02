use std::{
    collections::{BTreeMap, HashMap},
    iter,
};

use petgraph::{
    EdgeType,
    stable_graph::{NodeIndex, StableGraph},
};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use rustworkx_core::centrality::eigenvector_centrality;

use crate::{
    NetworkError, Weight, compact,
    paths::{Catchment, search},
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, Default, PartialEq, derive_more::Add, derive_more::AddAssign)]
pub struct Centrality {
    pub reach: f64,
    pub farness: f64,
    pub gravity: f64,
    pub betweenness: f64,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Convergence {
    pub max_iter: Option<usize> = None,
    pub tol: Option<f64> = None,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn geodesic<N, E, Ty>(
    graph: &StableGraph<N, E, Ty>,
    cost: impl Fn(&E) -> Weight + Sync,
    mass: impl Fn(&N) -> Weight + Sync,
    decay: impl Fn(f64) -> f64 + Sync,
    catchment: Catchment,
) -> BTreeMap<NodeIndex, Centrality>
where
    N: Sync,
    E: Sync,
    Ty: EdgeType + Sync,
{
    let scale = if graph.is_directed() { 1.0 } else { 0.5 };
    let held = |node: NodeIndex| mass(&graph[node]).into_inner();
    let pass = |source: NodeIndex| {
        let tree = search(graph, source, &cost, catchment);
        let (dependency, own) = tree.iter().skip(1).rev().fold((HashMap::<NodeIndex, f64>::new(), Centrality::default()), |(dependency, own), (&node, reached)| {
            let weight = held(node);
            let carried = weight + dependency.get(&node).copied().unwrap_or_default();
            (
                reached.predecessors.iter().filter_map(|predecessor| tree.get_key_value(predecessor)).fold(dependency, |mut dependency, (&predecessor, before)| {
                    let share = dependency.entry(predecessor).or_default();
                    *share = (before.paths / reached.paths).mul_add(carried, *share);
                    dependency
                }),
                own + Centrality { reach: weight, farness: weight * reached.distance, gravity: weight * decay(reached.distance), betweenness: 0.0 },
            )
        });
        let origin = held(source) * scale;
        iter::once((source, own))
            .chain(dependency.into_iter().filter(move |&(node, _)| node != source).map(move |(node, share)| (node, Centrality { reach: 0.0, farness: 0.0, gravity: 0.0, betweenness: origin * share })))
    };
    let merge = |mut total: HashMap<NodeIndex, Centrality>, (node, part): (NodeIndex, Centrality)| {
        *total.entry(node).or_default() += part;
        total
    };
    graph
        .node_indices()
        .collect::<Vec<_>>()
        .into_par_iter()
        .map(pass)
        .fold(HashMap::new, |total, parts| parts.fold(total, merge))
        .reduce(HashMap::new, |total, other| other.into_iter().fold(total, merge))
        .into_iter()
        .collect()
}

pub fn eigenvector<N, E, Ty>(graph: &StableGraph<N, E, Ty>, weight: impl Fn(&E) -> Weight, convergence: Convergence) -> Result<BTreeMap<NodeIndex, f64>, NetworkError>
where Ty: EdgeType {
    let network = compact(graph, |edge| weight(edge).into_inner());
    let Ok(scores) = eigenvector_centrality(&network, |edge| Ok::<f64, !>(*edge.weight()), convergence.max_iter, convergence.tol);
    match (network.node_count(), scores) {
        (0, _) => Ok(BTreeMap::new()),
        (_, None) => Err(NetworkError::Unconverged),
        (_, Some(scores)) => Ok(network.node_weights().copied().zip(scores).collect()),
    }
}
