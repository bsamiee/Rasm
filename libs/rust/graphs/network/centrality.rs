use core::{borrow::Borrow, iter, num::NonZero};
use std::collections::{BTreeMap, BTreeSet};

use hashbrown::HashMap;
use petgraph::{
    Direction::{self, Outgoing},
    EdgeType,
    graph::Graph,
    stable_graph::{EdgeReference, NodeIndex, StableGraph},
    visit::NodeIndexable as _,
};
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};
use rustworkx_core::centrality::{eigenvector_centrality, katz_centrality};
use validated::Validated;

use crate::{
    NetworkError, Tolerance, Weight, compact,
    paths::{Catchment, search},
    present,
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, Default, PartialEq, derive_more::Add, derive_more::AddAssign)]
pub struct Centrality {
    pub reach: f64,
    pub farness: f64,
    pub gravity: f64,
    pub betweenness: f64,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Convergence {
    pub max_iter: Option<NonZero<usize>> = None,
    pub tol: Option<Tolerance> = None,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn geodesic<N, E, Ty>(
    graph: &StableGraph<N, E, Ty>,
    origins: impl IntoIterator<Item: Borrow<NodeIndex>>,
    cost: impl Fn(EdgeReference<'_, E>) -> Weight + Sync,
    extent: impl Fn(EdgeReference<'_, E>, Weight) -> Weight + Sync,
    mass: impl Fn(&N) -> Weight + Sync,
    decay: impl Fn(f64) -> f64 + Sync,
    catchment: Catchment,
) -> Result<BTreeMap<NodeIndex, Centrality>, NetworkError>
where
    N: Sync,
    E: Sync,
    Ty: EdgeType + Sync,
{
    let origins = origins.into_iter().map(|node| present(graph, *node.borrow())).collect::<Validated<BTreeSet<_>, _>>().ok().map_err(NetworkError::Refused)?;
    let held = |node: NodeIndex| mass(&graph[node]).into_inner();
    let counted = |node: NodeIndex| match (Ty::is_directed(), origins.contains(&node)) {
        (false, true) => 0.5,
        (true, _) | (false, false) => 1.0,
    };
    let blank = || vec![Centrality::default(); graph.node_bound()];
    let pass = |mut total: Vec<Centrality>, &origin: &NodeIndex| {
        let tree = search(graph, origin, &cost, &extent, catchment);
        let (own, dependency) = tree.iter().enumerate().skip(1).rev().fold((Centrality::default(), vec![0.0; tree.len()]), |(own, mut dependency), (position, (&node, reached))| {
            let weight = held(node);
            let paired = weight * counted(node);
            let carried = dependency.get(position).map_or(paired, |share| paired + share);
            reached.predecessors.iter().filter_map(|&edge| graph.edge_endpoints(edge)).filter_map(|(tail, head)| tree.get_full(if tail == node { &head } else { &tail })).for_each(
                |(index, _, before)| {
                    if let Some(share) = dependency.get_mut(index) {
                        *share = (before.paths / reached.paths).mul_add(carried, *share);
                    }
                },
            );
            (own + Centrality { reach: weight, farness: weight * reached.distance, gravity: weight * decay(reached.distance), ..Centrality::default() }, dependency)
        });
        let carrier = held(origin);
        iter::once((origin, own)).chain(tree.keys().zip(dependency).skip(1).map(|(&node, share)| (node, Centrality { betweenness: carrier * share, ..Centrality::default() }))).for_each(
            |(node, part)| {
                if let Some(slot) = total.get_mut(node.index()) {
                    *slot += part;
                }
            },
        );
        total
    };
    let total = origins.par_iter().fold(blank, pass).reduce(blank, |mut total, other| {
        iter::zip(&mut total, other).for_each(|(slot, part)| *slot += part);
        total
    });
    Ok(total.into_iter().enumerate().map(|(index, centrality)| (NodeIndex::new(index), centrality)).filter(|&(node, _)| graph.contains_node(node)).collect())
}

pub fn eigenvector<N, E, Ty>(
    graph: &StableGraph<N, E, Ty>,
    weight: impl Fn(EdgeReference<'_, E>) -> Weight,
    direction: Direction,
    convergence: Convergence,
) -> Result<BTreeMap<NodeIndex, f64>, NetworkError>
where
    Ty: EdgeType,
{
    let mut network = compact(graph, |edge| weight(edge).into_inner());
    if direction == Outgoing {
        network.reverse();
    }
    let scores = eigenvector_centrality(&network, |edge| Ok::<f64, !>(*edge.weight()), convergence.max_iter.map(NonZero::get), convergence.tol.map(Tolerance::into_inner)).into_ok();
    scored(&network, scores, convergence)
}

pub fn katz<N, E, Ty>(
    graph: &StableGraph<N, E, Ty>,
    weight: impl Fn(EdgeReference<'_, E>) -> Weight,
    mass: impl Fn(&N) -> Weight,
    alpha: Weight,
    direction: Direction,
    convergence: Convergence,
) -> Result<BTreeMap<NodeIndex, f64>, NetworkError>
where
    Ty: EdgeType,
{
    let mut network = compact(graph, |edge| weight(edge).into_inner());
    if direction == Outgoing {
        network.reverse();
    }
    let beta = network.node_indices().map(|node| (node.index(), mass(&graph[network[node]]).into_inner())).collect::<HashMap<_, _>>();
    let scores = if beta.values().all(|&held| held == 0.0) {
        Some(vec![0.0; network.node_count()])
    } else {
        katz_centrality(&network, |edge| Ok::<f64, !>(*edge.weight()), Some(alpha.into_inner()), Some(beta), None, convergence.max_iter.map(NonZero::get), convergence.tol.map(Tolerance::into_inner))
            .into_ok()
    };
    scored(&network, scores, convergence)
}

fn scored<Ty>(network: &Graph<NodeIndex, f64, Ty>, scores: Option<Vec<f64>>, convergence: Convergence) -> Result<BTreeMap<NodeIndex, f64>, NetworkError>
where Ty: EdgeType {
    match (network.node_count(), scores) {
        (0, _) => Ok(BTreeMap::new()),
        (_, None) => Err(NetworkError::Unconverged { convergence }),
        (_, Some(scores)) => Ok(network.node_weights().copied().zip(scores).collect()),
    }
}
