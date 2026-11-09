use core::{borrow::Borrow, fmt, iter, num::NonZero};
use std::collections::{BTreeMap, BTreeSet};

use petgraph::{
    Direction::{self, Outgoing},
    EdgeType,
    graph::Graph,
    visit::{EdgeRef as _, IntoEdgesDirected},
};
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};
use rustworkx_core::centrality::{eigenvector_centrality, katz_centrality};

use crate::{
    Failure, Network, NetworkError, Tolerance, Weight, compact,
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

pub fn geodesic<G>(
    graph: G,
    origins: impl IntoIterator<Item: Borrow<G::NodeId>>,
    cost: impl Fn(G::EdgeRef) -> Weight + Sync,
    extent: impl Fn(G::EdgeRef, Weight) -> Weight + Sync,
    mass: impl Fn(G::NodeId) -> Weight + Sync,
    decay: impl Fn(f64) -> f64 + Sync,
    catchment: Catchment,
) -> Result<BTreeMap<G::NodeId, Centrality>, Failure<G>>
where
    G: Network + IntoEdgesDirected + Sync,
    G::NodeId: Send + Sync,
{
    let origins = present(graph, origins.into_iter().map(|node| *node.borrow())).ok().map_err(NetworkError::Refused)?.into_iter().collect::<BTreeSet<_>>();
    let held = |node: G::NodeId| mass(node).into_inner();
    let counted = |node: G::NodeId| match (graph.is_directed(), origins.contains(&node)) {
        (false, true) => 0.5,
        (true, _) | (false, false) => 1.0,
    };
    let blank = || vec![Centrality::default(); graph.node_bound()];
    let pass = |(mut total, mut dependency): (Vec<Centrality>, Vec<f64>), &origin: &G::NodeId| {
        let tree = search(graph, origin, &cost, &extent, catchment);
        dependency.clear();
        dependency.resize(tree.len(), 0.0);
        let own = tree.iter().enumerate().skip(1).rev().fold(Centrality::default(), |own, (position, (&node, reached))| {
            let weight = held(node);
            let carried = weight.mul_add(counted(node), dependency[position]);
            reached.predecessors.iter().filter_map(|edge| tree.get_full(&if edge.source() == node { edge.target() } else { edge.source() })).for_each(|(index, _, before)| {
                let share = &mut dependency[index];
                *share = (before.paths / reached.paths).mul_add(carried, *share);
            });
            own + Centrality { reach: weight, farness: weight * reached.distance, gravity: weight * decay(reached.distance), ..Centrality::default() }
        });
        let carrier = held(origin);
        iter::once((origin, own))
            .chain(tree.keys().zip(dependency.iter().copied()).skip(1).map(|(&node, share)| (node, Centrality { betweenness: carrier * share, ..Centrality::default() })))
            .for_each(|(node, part)| {
                total[graph.to_index(node)] += part;
            });
        (total, dependency)
    };
    let total = origins.par_iter().fold(|| (blank(), Vec::new()), pass).map(|(total, _)| total).reduce(blank, |mut total, other| {
        iter::zip(&mut total, other).for_each(|(slot, part)| *slot += part);
        total
    });
    Ok(graph.node_identifiers().map(|node| (node, total[graph.to_index(node)])).collect())
}

pub fn eigenvector<G>(graph: G, weight: impl Fn(G::EdgeRef) -> Weight, direction: Direction, convergence: Convergence) -> Result<BTreeMap<G::NodeId, f64>, Failure<G>>
where G: Network {
    let mut network = compact(graph, |edge| weight(edge).into_inner());
    if direction == Outgoing {
        network.reverse();
    }
    let scores = eigenvector_centrality(&network, |edge| Ok::<f64, !>(*edge.weight()), convergence.max_iter.map(NonZero::get), convergence.tol.map(Tolerance::into_inner)).into_ok();
    scored(&network, scores, convergence)
}

pub fn katz<G>(
    graph: G,
    weight: impl Fn(G::EdgeRef) -> Weight,
    mass: impl Fn(G::NodeId) -> Weight,
    alpha: Weight,
    direction: Direction,
    convergence: Convergence,
) -> Result<BTreeMap<G::NodeId, f64>, Failure<G>>
where
    G: Network,
{
    let mut network = compact(graph, |edge| weight(edge).into_inner());
    if direction == Outgoing {
        network.reverse();
    }
    let masses = network.node_weights().map(|&node| mass(node).into_inner()).collect::<Vec<_>>();
    let scores = if masses.iter().all(|&held| held == 0.0) {
        Some(vec![0.0; network.node_count()])
    } else {
        katz_centrality(
            &network,
            |edge| Ok::<f64, !>(*edge.weight()),
            Some(alpha.into_inner()),
            Some(masses.into_iter().enumerate().collect()),
            None,
            convergence.max_iter.map(NonZero::get),
            convergence.tol.map(Tolerance::into_inner),
        )
        .into_ok()
    };
    scored(&network, scores, convergence)
}

fn scored<N, E, Ty>(network: &Graph<N, f64, Ty>, scores: Option<Vec<f64>>, convergence: Convergence) -> Result<BTreeMap<N, f64>, NetworkError<N, E>>
where
    N: Ord + Copy + fmt::Debug,
    E: fmt::Debug,
    Ty: EdgeType,
{
    match (network.node_count(), scores) {
        (0, _) => Ok(BTreeMap::new()),
        (_, None) => Err(NetworkError::Unconverged { convergence }),
        (_, Some(scores)) => Ok(network.node_weights().copied().zip(scores).collect()),
    }
}
