use core::cmp::Ordering;
use std::collections::BTreeMap;

use enum_map::EnumMap;
use nonempty_collections::{IntoNonEmptyIterator as _, NEBTreeSet, NEVec, NonEmptyIterator as _};
use num_traits::{CheckedAdd, CheckedSub, Zero};
use petgraph::{
    Direction,
    stable_graph::{EdgeIndex, NodeIndex},
    visit::{EdgeRef as _, IntoEdgeReferences as _},
};
use validated::Validated;

use crate::{DependencyError, Evaluation, Refusal, at, entries, pass};

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

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
#[derive_where::derive_where(Default)]
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
    T: Copy + Ord + Zero + CheckedAdd + CheckedSub,
{
    NEBTreeSet::try_from_set(evaluation.node_indices().filter(|&node| duration(&evaluation[node]) < T::zero()).collect())
        .map_or(Ok(()), |nodes| Err(DependencyError::Refused { reason: Refusal::Duration, nodes }))?;
    let refused = |nodes: NEVec<NodeIndex>| DependencyError::Refused { reason: Refusal::Date, nodes: nodes.into_nonempty_iter().collect() };
    let dated = |direction: Direction, origin: (Event, T)| {
        let tighter = match direction {
            Direction::Outgoing => Ordering::Greater,
            Direction::Incoming => Ordering::Less,
        };
        let limit = |Constraint { earliest, latest }: Constraint<T>| match direction {
            Direction::Outgoing => earliest,
            Direction::Incoming => latest,
        };
        pass(evaluation, direction, |node, dates| {
            let (length, bounds) = (duration(&evaluation[node]), constraint(&evaluation[node]));
            let bound = |(near, date): (Event, T)| {
                EnumMap::try_from_fn(|event| {
                    match (near, event) {
                        (Event::Start, Event::Finish) => date.checked_add(&length),
                        (Event::Finish, Event::Start) => date.checked_sub(&length),
                        (Event::Start, Event::Start) | (Event::Finish, Event::Finish) => Some(date),
                    }
                    .ok_or(node)
                })
            };
            evaluation
                .edges_directed(node, direction.opposite())
                .filter_map(|edge| {
                    let Precedence { predecessor, successor, lag } = precedence(edge.weight());
                    let (neighbor, near, far) = match direction {
                        Direction::Outgoing => (edge.source(), successor, predecessor),
                        Direction::Incoming => (edge.target(), predecessor, successor),
                    };
                    at(dates, neighbor).map(|known: Result<EnumMap<Event, T>, NodeIndex>| {
                        let known = known?;
                        match direction {
                            Direction::Outgoing => known[far].checked_add(&lag),
                            Direction::Incoming => known[far].checked_sub(&lag),
                        }
                        .map(|date| (near, date))
                        .ok_or(node)
                    })
                })
                .chain(bounds.into_iter().filter_map(|(event, bounds)| limit(bounds).map(|date| Ok((event, date)))))
                .try_fold(bound(origin)?, |dates, candidate| {
                    let (near, date) = candidate?;
                    if date.cmp(&dates[near]) == tighter { bound((near, date)) } else { Ok(dates) }
                })
        })
        .into_iter()
        .map(Option::transpose)
        .collect::<Validated<Vec<_>, _>>()
    };
    let early = dated(Direction::Outgoing, (Event::Start, start)).ok().map_err(refused)?;
    let completion = entries(&early).map(|(node, dates)| (node, dates[Event::Finish])).max_by_key(|&(_, date)| date);
    let close = deadline.unwrap_or_else(|| completion.map_or(start, |(_, date)| date));
    let makespan = completion.map_or_else(|| Ok(T::zero()), |(node, date)| date.checked_sub(&start).ok_or(node));
    let (late, makespan) = dated(Direction::Incoming, (Event::Finish, close)).map2(Validated::from(makespan), |late, makespan| (late, makespan)).ok().map_err(refused)?;
    let relationships = evaluation
        .edge_references()
        .filter_map(|edge| {
            let Precedence { predecessor, successor, lag } = precedence(edge.weight());
            let (source, target) = (edge.source(), edge.target());
            let before = at(&early, source)?[predecessor];
            let (soonest, latest) = (at(&early, target)?[successor], at(&late, target)?[successor]);
            Some(before.checked_add(&lag).ok_or(source).and_then(|opened| {
                EnumMap::try_from_fn(|float| {
                    match float {
                        Float::Total => latest,
                        Float::Free => soonest,
                    }
                    .checked_sub(&opened)
                    .ok_or(target)
                })
                .map(|float| (edge.id(), float))
            }))
        })
        .collect::<Validated<BTreeMap<_, _>, _>>()
        .ok()
        .map_err(refused)?;
    let activities = evaluation
        .node_indices()
        .filter_map(|node| {
            let (early, late) = (at(&early, node)?, at(&late, node)?);
            let free = evaluation
                .edges_directed(node, Direction::Outgoing)
                .filter_map(|edge| relationships.get(&edge.id()).map(|float| float[Float::Free]))
                .min()
                .map_or_else(|| close.checked_sub(&early[Event::Finish]), Some)
                .ok_or(node);
            Some(free.and_then(|free| {
                EnumMap::try_from_fn(|float| match float {
                    Float::Total => late[Event::Finish].checked_sub(&early[Event::Finish]).ok_or(node),
                    Float::Free => Ok(free),
                })
                .map(|float| (node, Activity { early, late, float }))
            }))
        })
        .collect::<Validated<_, _>>()
        .ok()
        .map_err(refused)?;
    Ok(Schedule { activities, relationships, makespan })
}
