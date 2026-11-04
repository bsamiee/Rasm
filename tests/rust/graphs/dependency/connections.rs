use core::assert_matches;
use std::collections::BTreeSet;

use itertools::Either;
use petgraph::{
    algo::Cycle,
    data::Build as _,
    stable_graph::NodeIndex,
    visit::{EdgeRef as _, IntoEdgeReferences as _},
};
use rasm_dependency::{DependencyError, Evaluation, Inputs, connect};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn refused_batch_preserves_topological_order() {
    let mut evaluation = Evaluation::new();
    let [first, _, last] = [(); 3].map(|weight| evaluation.add_node(weight));
    let before = evaluation.nodes_iter().collect::<Vec<_>>();
    let connected = connect(&mut evaluation, [(last, first, ()), (first, last, ())], Inputs::Extend);
    assert_matches!(connected, Err(Either::Left(DependencyError::Cycle { paths: _ })));
    assert_eq!(evaluation.nodes_iter().collect::<Vec<_>>(), before);
    assert_eq!(evaluation.edge_count(), 0);
}

#[test]
fn replacement_preserves_borrowed_nodes_and_edge_weights() -> Result<(), Either<DependencyError, Cycle<NodeIndex>>> {
    let label = String::from("activity");
    let mut evaluation = Evaluation::new();
    let [first, middle, last] = [(); 3].map(|()| evaluation.add_node(label.as_str()));
    connect(&mut evaluation, [(first, middle, 1)], Inputs::Extend)?;
    let connected = connect(&mut evaluation, [(middle, first, 2), (last, middle, 3)], Inputs::Replace)?;
    assert_eq!(connected.replaced, [(first, middle, 1)]);
    assert_eq!(evaluation.nodes_iter().collect::<Vec<_>>(), [last, middle, first]);
    assert_eq!(evaluation.edge_references().map(|edge| (edge.source(), edge.target(), *edge.weight())).collect::<BTreeSet<_>>(), BTreeSet::from([(middle, first, 2), (last, middle, 3)]));
    assert_eq!(evaluation.node_weights().copied().collect::<Vec<_>>(), [label.as_str(); 3]);
    Ok(())
}
