use std::collections::BTreeMap;

use claims::{assert_err, assert_ok, assert_some};
use enum_map::EnumMap;
use itertools::Either;
use nonempty_collections::{NEBTreeSet, nev};
use petgraph::Direction;
use petgraph::stable_graph::StableDiGraph;
use proptest::prelude::ProptestConfig;
use proptest::strategy::Strategy as _;
use proptest::test_runner::FileFailurePersistence;
use rasm_dependency_graph::{Activity, DependencyError, Event, Precedence, Refusal, evaluation, layers, schedule};

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn finish_to_start<T>(lag: &T) -> Precedence<T>
where T: Copy {
    Precedence { predecessor: Event::Finish, successor: Event::Start, lag: *lag }
}

#[test]
fn schedule_matches_hand_computed_critical_path() {
    let mut graph = StableDiGraph::<u32, u32>::new();
    let [first, short, long, last] = [3, 2, 4, 1].map(|duration| graph.add_node(duration));
    graph.extend_with_edges([(first, short), (first, long), (short, last), (long, last)]);
    let network = assert_ok!(evaluation(graph));
    let planned = assert_ok!(schedule(&network, |duration: &u32| *duration, |_: &u32| EnumMap::default(), finish_to_start, 0, None));
    let activity = |early, late, float| Activity { early: EnumMap::from_array(early), late: EnumMap::from_array(late), float: EnumMap::from_array(float) };
    let edge = |source, target| assert_some!(network.find_edge(source, target));
    assert_eq!(planned.makespan, 8, "makespan is the longest path 3 + 4 + 1");
    assert_eq!(
        planned.activities,
        BTreeMap::from([(first, activity([0, 3], [0, 3], [0, 0])), (short, activity([3, 5], [5, 7], [2, 2])), (long, activity([3, 7], [3, 7], [0, 0])), (last, activity([7, 8], [7, 8], [0, 0])),]),
        "early and late dates with total and free float per activity",
    );
    assert_eq!(
        planned.relationships,
        BTreeMap::from([
            (edge(first, short), EnumMap::from_array([2, 0])),
            (edge(first, long), EnumMap::from_array([0, 0])),
            (edge(short, last), EnumMap::from_array([2, 2])),
            (edge(long, last), EnumMap::from_array([0, 0])),
        ]),
        "total and free float per precedence",
    );
}

#[test]
fn layers_group_activities_by_longest_predecessor_chain() {
    let mut graph = StableDiGraph::<(), ()>::new();
    let [first, short, long, last] = [(); 4].map(|()| graph.add_node(()));
    graph.extend_with_edges([(first, short), (first, long), (short, last), (long, last)]);
    let network = assert_ok!(evaluation(graph));
    assert_eq!(layers(&network, Direction::Outgoing), vec![nev![first], nev![short, long], nev![last]], "one layer per precedence depth");
    assert_eq!(layers(&network, Direction::Incoming), vec![nev![last], nev![short, long], nev![first]], "reversed direction layers from the sinks");
}

#[test]
fn evaluation_refuses_a_cycle() {
    let mut graph = StableDiGraph::<(), ()>::new();
    let [head, tail] = [(); 2].map(|()| graph.add_node(()));
    graph.extend_with_edges([(head, tail), (tail, head)]);
    assert_eq!(assert_err!(evaluation(graph)), DependencyError::Cycle { paths: nev![Either::Left(vec![tail, head])] }, "the loop is reported as the path closing it");
}

#[test]
fn schedule_refuses_negative_durations() {
    let mut graph = StableDiGraph::<i32, i32>::new();
    let [valid, negative] = [1, -2].map(|duration| graph.add_node(duration));
    graph.extend_with_edges([(valid, negative)]);
    let network = assert_ok!(evaluation(graph));
    assert_eq!(
        schedule(&network, |duration: &i32| *duration, |_: &i32| EnumMap::default(), finish_to_start, 0, None),
        Err(DependencyError::Refused { reason: Refusal::Duration, nodes: NEBTreeSet::new(negative) }),
        "the negative activity alone is named",
    );
}
// --- [REFERENCE]

proptest::proptest! {
    #![proptest_config(ProptestConfig { failure_persistence: Some(Box::new(FileFailurePersistence::WithSource("regressions"))), ..ProptestConfig::default() })]

    #[test]
    fn schedule_matches_a_forward_and_backward_pass_over_any_acyclic_network(
        (durations, edges) in (0_usize..10).prop_flat_map(|count| (
            proptest::collection::vec(0_u32..20, count),
            proptest::collection::vec(proptest::bool::weighted(0.35), count * count.saturating_sub(1) / 2),
        )),
    ) {
        let pairs = (0..durations.len()).flat_map(|source| (source + 1..durations.len()).map(move |target| (source, target)));
        let linked = Vec::from_iter(pairs.zip(&edges).filter(|(_, linked)| **linked).map(|(pair, _)| pair));
        let mut graph = StableDiGraph::<u32, u32>::new();
        let nodes = Vec::from_iter(durations.iter().map(|duration| graph.add_node(*duration)));
        graph.extend_with_edges(linked.iter().map(|(source, target)| (nodes[*source], nodes[*target])));
        let network = assert_ok!(evaluation(graph));
        let planned = assert_ok!(schedule(&network, |duration: &u32| *duration, |_: &u32| EnumMap::default(), finish_to_start, 0, None));
        let predecessors = |node: usize| linked.iter().filter(move |(_, target)| *target == node).map(|(source, _)| *source);
        let successors = |node: usize| linked.iter().filter(move |(source, _)| *source == node).map(|(_, target)| *target);
        let mut early_start = vec![0_u32; durations.len()];
        for node in 0..durations.len() {
            early_start[node] = predecessors(node).map(|source| early_start[source] + durations[source]).max().unwrap_or(0);
        }
        let makespan = (0..durations.len()).map(|node| early_start[node] + durations[node]).max().unwrap_or(0);
        let mut late_finish = vec![makespan; durations.len()];
        for node in (0..durations.len()).rev() {
            late_finish[node] = successors(node).map(|target| late_finish[target] - durations[target]).min().unwrap_or(makespan);
        }
        let activity = |node: usize| {
            let (early, late) = ([early_start[node], early_start[node] + durations[node]], [late_finish[node] - durations[node], late_finish[node]]);
            let free = successors(node).map(|target| early_start[target]).min().unwrap_or(makespan) - early[1];
            Activity { early: EnumMap::from_array(early), late: EnumMap::from_array(late), float: EnumMap::from_array([late[0] - early[0], free]) }
        };
        proptest::prop_assert_eq!(planned.makespan, makespan, "durations {:?}, edges {:?}", durations, linked);
        proptest::prop_assert_eq!(planned.activities, BTreeMap::from_iter((0..durations.len()).map(|node| (nodes[node], activity(node)))), "durations {:?}, edges {:?}", durations, linked);
    }
}
