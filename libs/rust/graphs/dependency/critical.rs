use core::ops::{Neg, Sub};
use std::collections::BTreeMap;

use enum_map::EnumMap;
use nonempty_collections::NEBTreeSet;
use petgraph::{
    Direction,
    algo::Measure,
    stable_graph::{EdgeIndex, NodeIndex},
    visit::{EdgeRef as _, IntoEdgeReferences as _},
};

use crate::{DependencyError, Evaluation, Refusal, at, pass};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, enum_map::Enum)]
pub enum Event {
    Start,
    Finish,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, enum_map::Enum)]
pub enum Float {
    Total,
    Free,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Precedence<T> {
    pub predecessor: Event,
    pub successor: Event,
    pub lag: T,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub struct Constraint<T> {
    pub earliest: Option<T>,
    pub latest: Option<T>,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Activity<T> {
    pub early: EnumMap<Event, T>,
    pub late: EnumMap<Event, T>,
    pub float: EnumMap<Float, T>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Schedule<T> {
    pub activities: BTreeMap<NodeIndex, Activity<T>>,
    pub relationships: BTreeMap<EdgeIndex, EnumMap<Float, T>>,
    pub makespan: T,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn schedule<N, E, T>(
    evaluation: &Evaluation<N, E>,
    duration: impl Fn(&N) -> T,
    constraint: impl Fn(&N) -> EnumMap<Event, Constraint<T>>,
    precedence: impl Fn(&E) -> Precedence<T>,
    start: T,
    deadline: Option<T>,
) -> Result<Schedule<T>, DependencyError>
where
    T: Measure + Copy + Ord + Sub<Output = T> + Neg<Output = T>,
{
    if let Some(nodes) = NEBTreeSet::try_from_set(evaluation.node_indices().filter(|&node| duration(&evaluation[node]) < T::default()).collect()) {
        return Err(DependencyError::Refused { reason: Refusal::Duration, nodes });
    }
    let bound = |(near, date): (Event, T), length: T| match near {
        Event::Start => date.add(length),
        Event::Finish => date,
    };
    let dated = |direction: Direction, origin: (Event, T)| {
        let extreme: fn(T, T) -> T = match direction {
            Direction::Outgoing => Ord::max,
            Direction::Incoming => Ord::min,
        };
        let limit = |Constraint { earliest, latest }: Constraint<T>| match direction {
            Direction::Outgoing => earliest,
            Direction::Incoming => latest,
        };
        pass(evaluation, direction, |node, dates| {
            let (length, bounds) = (duration(&evaluation[node]), constraint(&evaluation[node]));
            let finish = evaluation
                .edges_directed(node, direction.opposite())
                .filter_map(|edge| {
                    let Precedence { predecessor, successor, lag } = precedence(edge.weight());
                    let (neighbor, near, far, lag) = match direction {
                        Direction::Outgoing => (edge.source(), successor, predecessor, lag),
                        Direction::Incoming => (edge.target(), predecessor, successor, lag.neg()),
                    };
                    at(dates, neighbor).map(|known: EnumMap<Event, T>| (near, known[far].add(lag)))
                })
                .chain(bounds.into_iter().filter_map(|(event, bounds)| limit(bounds).map(|date| (event, date))))
                .fold(bound(origin, length), |finish, candidate| extreme(finish, bound(candidate, length)));
            EnumMap::from_fn(|event| match event {
                Event::Start => finish.sub(length),
                Event::Finish => finish,
            })
        })
    };
    let early = dated(Direction::Outgoing, (Event::Start, start));
    let completion = early.iter().flatten().map(|dates| dates[Event::Finish]).max().unwrap_or(start);
    let close = deadline.unwrap_or(completion);
    let late = dated(Direction::Incoming, (Event::Finish, close));
    let relationships = evaluation
        .edge_references()
        .filter_map(|edge| {
            let Precedence { predecessor, successor, lag } = precedence(edge.weight());
            let opened = at(&early, edge.source())?[predecessor].add(lag);
            let (soonest, latest) = (at(&early, edge.target())?[successor], at(&late, edge.target())?[successor]);
            Some((edge.id(), EnumMap::from_fn(|float| match float {
                Float::Total => latest.sub(opened),
                Float::Free => soonest.sub(opened),
            })))
        })
        .collect::<BTreeMap<_, _>>();
    let activities = evaluation
        .node_indices()
        .filter_map(|node| {
            let (early, late) = (at(&early, node)?, at(&late, node)?);
            let free = evaluation.edges_directed(node, Direction::Outgoing).filter_map(|edge| relationships.get(&edge.id()).map(|float| float[Float::Free])).min().unwrap_or_else(|| close.sub(early[Event::Finish]));
            Some((node, Activity { early, late, float: EnumMap::from_fn(|float| match float {
                Float::Total => late[Event::Finish].sub(early[Event::Finish]),
                Float::Free => free,
            }) }))
        })
        .collect();
    Ok(Schedule { activities, relationships, makespan: completion.sub(start) })
}
