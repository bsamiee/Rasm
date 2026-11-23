use core::iter;
use std::collections::{BTreeMap, BTreeSet};

use claims::{assert_err, assert_ok, assert_some};
use enum_map::EnumMap;
use itertools::Either;
use nonempty_collections::{NEBTreeSet, nev};
use petgraph::Direction;
use petgraph::stable_graph::StableDiGraph;
use proptest::prelude::ProptestConfig;
use proptest::strategy::Strategy as _;
use proptest::test_runner::FileFailurePersistence;
use rasm_dependency_graph::{Activity, DependencyError, Event, Outcome, Precedence, Refusal, evaluation, layers, propagate, schedule};

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
    fn schedule_matches_a_forward_and_backward_pass_over_any_precedence_kind_and_lag(
        (durations, links) in (0_usize..10).prop_flat_map(|count| (
            proptest::collection::vec(0_i64..20, count),
            proptest::collection::vec(proptest::option::weighted(0.35, (proptest::bool::ANY, proptest::bool::ANY, 0_i64..5)), count * count.saturating_sub(1) / 2),
        )),
    ) {
        let event = |finish: bool| if finish { Event::Finish } else { Event::Start };
        let pairs = (0..durations.len()).flat_map(|source| (source + 1..durations.len()).map(move |target| (source, target)));
        let linked = Vec::from_iter(pairs.zip(&links).filter_map(|(pair, link)| link.map(|(from, to, lag)| (pair, Precedence { predecessor: event(from), successor: event(to), lag }))));
        let mut graph = StableDiGraph::<i64, Precedence<i64>>::new();
        let nodes = Vec::from_iter(durations.iter().map(|duration| graph.add_node(*duration)));
        let edges = Vec::from_iter(linked.iter().map(|&((source, target), precedence)| graph.add_edge(nodes[source], nodes[target], precedence)));
        let network = assert_ok!(evaluation(graph));
        let planned = assert_ok!(schedule(&network, |duration: &i64| *duration, |_: &i64| EnumMap::default(), |precedence: &Precedence<i64>| *precedence, 0, None));
        let at = |start: i64, node: usize, held: Event| match held {
            Event::Start => start,
            Event::Finish => start + durations[node],
        };
        let mut early = vec![0_i64; durations.len()];
        for node in 0..durations.len() {
            let incoming = linked.iter().filter(|((_, target), _)| *target == node);
            let bound = incoming.map(|&((source, _), held)| at(early[source], source, held.predecessor) + held.lag - at(0, node, held.successor));
            early[node] = bound.fold(0, i64::max);
        }
        let close = (0..durations.len()).map(|node| early[node] + durations[node]).max().unwrap_or(0);
        let mut late = vec![close; durations.len()];
        for node in (0..durations.len()).rev() {
            let outgoing = linked.iter().filter(|((source, _), _)| *source == node);
            let bound = outgoing.map(|&((_, target), held)| at(late[target] - durations[target], target, held.successor) - held.lag + durations[node] - at(0, node, held.predecessor));
            late[node] = bound.fold(close, i64::min);
        }
        let floats = Vec::from_iter(linked.iter().map(|&((source, target), precedence)| {
            let opened = at(early[source], source, precedence.predecessor) + precedence.lag;
            [at(late[target] - durations[target], target, precedence.successor) - opened, at(early[target], target, precedence.successor) - opened]
        }));
        let activity = |node: usize| {
            let (start, finish, length) = (early[node], late[node], durations[node]);
            let free = iter::zip(&linked, &floats).filter(|(((source, _), _), _)| *source == node).map(|(_, [_, free])| *free).min().unwrap_or(close - start - length);
            Activity { early: EnumMap::from_array([start, start + length]), late: EnumMap::from_array([finish - length, finish]), float: EnumMap::from_array([finish - start - length, free]) }
        };
        proptest::prop_assert_eq!(planned.makespan, close, "durations {:?}, precedences {:?}", durations, linked);
        let activities = BTreeMap::from_iter((0..durations.len()).map(|node| (nodes[node], activity(node))));
        let relationships = BTreeMap::from_iter(iter::zip(&edges, &floats).map(|(edge, float)| (*edge, EnumMap::from_array(*float))));
        proptest::prop_assert_eq!(planned.activities, activities, "durations {:?}, precedences {:?}", durations, linked);
        proptest::prop_assert_eq!(planned.relationships, relationships, "durations {:?}, precedences {:?}", durations, linked);
    }

    #[test]
    fn propagation_runs_each_node_a_change_reaches_after_its_ancestors_and_no_other(
        (dirty, outcomes, edges) in (1_usize..10).prop_flat_map(|count| (
            proptest::collection::vec(proptest::bool::weighted(0.3), count),
            proptest::collection::vec(proptest::sample::select(&[Outcome::Unchanged, Outcome::Changed, Outcome::Failed][..]), count),
            proptest::collection::vec(proptest::bool::weighted(0.35), count * (count - 1) / 2),
        )),
    ) {
        let pairs = (0..dirty.len()).flat_map(|source| (source + 1..dirty.len()).map(move |target| (source, target)));
        let linked = Vec::from_iter(pairs.zip(&edges).filter(|(_, linked)| **linked).map(|(pair, _)| pair));
        let mut graph = StableDiGraph::<(), ()>::new();
        let nodes = Vec::from_iter(dirty.iter().map(|_| graph.add_node(())));
        graph.extend_with_edges(linked.iter().map(|(source, target)| (nodes[*source], nodes[*target])));
        let network = assert_ok!(evaluation(graph));
        let marked = Vec::from_iter(dirty.iter().enumerate().filter(|(_, dirty)| **dirty).map(|(node, _)| nodes[node]));
        let ran = assert_ok!(propagate(&network, marked, None, |mut propagation, mut ready| {
            let mut ran = Vec::new();
            while let Some(next) = ready.pop() {
                let node = next.node().index();
                ran.push(node);
                ready.extend(propagation.finish(next, outcomes[node]));
            }
            ran
        }));
        let stronger = |held: Outcome, input: Outcome| match (held, input) {
            (Outcome::Failed, Outcome::Unchanged | Outcome::Changed | Outcome::Failed) | (Outcome::Unchanged | Outcome::Changed, Outcome::Failed) => Outcome::Failed,
            (Outcome::Changed, Outcome::Unchanged | Outcome::Changed) | (Outcome::Unchanged, Outcome::Changed) => Outcome::Changed,
            (Outcome::Unchanged, Outcome::Unchanged) => Outcome::Unchanged,
        };
        let mut received = Vec::<Option<Outcome>>::with_capacity(dirty.len());
        let mut runs = Vec::with_capacity(dirty.len());
        for node in 0..dirty.len() {
            let inputs = linked.iter().filter(|(_, target)| *target == node).filter_map(|(source, _)| received[*source]);
            let start = dirty[node].then_some(Outcome::Changed);
            let reached = inputs.fold(start, |held, input| Some(held.map_or(input, |held| stronger(held, input))));
            let run = reached == Some(Outcome::Changed);
            runs.push(run);
            received.push(reached.map(|reached| if run { outcomes[node] } else { reached }));
        }
        let expected = BTreeSet::from_iter((0..dirty.len()).filter(|node| runs[*node]));
        proptest::prop_assert_eq!(BTreeSet::from_iter(ran.iter().copied()), expected, "dirty {:?}, outcomes {:?}, edges {:?}", dirty, outcomes, linked);
        proptest::prop_assert_eq!(ran.len(), BTreeSet::from_iter(ran.iter().copied()).len(), "each node runs once: {:?}", ran);
        let mut below = vec![BTreeSet::<usize>::new(); dirty.len()];
        for node in (0..dirty.len()).rev() {
            below[node] = BTreeSet::from_iter(linked.iter().filter(|(source, _)| *source == node).flat_map(|(_, target)| iter::once(*target).chain(below[*target].iter().copied())));
        }
        let position = |node: usize| ran.iter().position(|ran| *ran == node);
        let descents = (0..dirty.len()).flat_map(|above| below[above].iter().map(move |under| (above, *under)));
        let early = Vec::from_iter(descents.filter(|&(above, under)| position(above).zip(position(under)).is_some_and(|(above, under)| above > under)));
        proptest::prop_assert!(early.is_empty(), "a node runs before an ancestor that also runs: {:?} in {:?}", early, ran);
    }
}
