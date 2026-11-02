use core::{iter, ops::Sub};
use std::collections::BTreeMap;

use petgraph::{
    Direction,
    algo::Measure,
    stable_graph::{NodeIndex, StableDiGraph},
    visit::EdgeRef as _,
};

use crate::{DependencyError, order::topological};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Event {
    Start,
    Finish,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Precedence<T> {
    pub predecessor: Event,
    pub successor: Event,
    pub lag: T,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Activity<T> {
    pub early_start: T,
    pub early_finish: T,
    pub late_start: T,
    pub late_finish: T,
    pub total_float: T,
    pub free_float: T,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Schedule<T> {
    pub activities: BTreeMap<NodeIndex, Activity<T>>,
    pub makespan: T,
    pub path: Vec<NodeIndex>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn schedule<N, E, T>(graph: &StableDiGraph<N, E>, duration: impl Fn(&N) -> T, precedence: impl Fn(&E) -> Precedence<T>) -> Result<Schedule<T>, DependencyError>
where T: Measure + Copy + Ord + Sub<Output = T> {
    let sequence = topological(graph, |node| node)?;
    let date = |event: Event, start: T, finish: T| match event {
        Event::Start => start,
        Event::Finish => finish,
    };
    let early = sequence
        .iter()
        .scan(BTreeMap::new(), |dates, &node| {
            let length = duration(&graph[node]);
            let (finish, driver) = graph
                .edges_directed(node, Direction::Incoming)
                .filter_map(|edge| {
                    let Precedence { predecessor, successor, lag } = precedence(edge.weight());
                    dates.get(&edge.source()).map(|&(start, finish): &(T, T)| {
                        let reach = date(predecessor, start, finish).add(lag);
                        (date(successor, reach.add(length), reach), edge.source())
                    })
                })
                .max_by_key(|&(finish, _)| finish)
                .filter(|&(finish, _)| finish >= length)
                .map_or((length, None), |(finish, source)| (finish, Some(source)));
            dates.insert(node, (finish.sub(length), finish));
            Some((node, (finish.sub(length), finish, driver)))
        })
        .collect::<BTreeMap<_, _>>();
    let (makespan, end) = early.iter().map(|(&node, &(_, finish, _))| (finish, node)).max_by_key(|&(finish, _)| finish).unzip();
    let makespan = makespan.unwrap_or_default();
    let activities = sequence
        .iter()
        .rev()
        .filter_map(|node| early.get_key_value(node))
        .scan(BTreeMap::new(), |dates, (&node, &(start, finish, _))| {
            let length = finish.sub(start);
            let (late_finish, free_float) = graph
                .edges_directed(node, Direction::Outgoing)
                .filter_map(|edge| {
                    let Precedence { predecessor, successor, lag } = precedence(edge.weight());
                    dates.get(&edge.target()).zip(early.get(&edge.target())).map(|(&(late_start, late_finish), &(next_start, next_finish, _)): (&(T, T), _)| {
                        let bound = date(successor, late_start, late_finish).sub(lag);
                        (date(predecessor, bound.add(length), bound), date(successor, next_start, next_finish).sub(lag).sub(date(predecessor, start, finish)))
                    })
                })
                .fold((makespan, makespan.sub(finish)), |(cap, slack), (next_cap, next_slack)| (cap.min(next_cap), slack.min(next_slack)));
            dates.insert(node, (late_finish.sub(length), late_finish));
            Some((node, Activity { early_start: start, early_finish: finish, late_start: late_finish.sub(length), late_finish, total_float: late_finish.sub(finish), free_float }))
        })
        .collect();
    let mut path = iter::successors(end, |node| early.get(node).and_then(|&(_, _, driver)| driver)).collect::<Vec<_>>();
    path.reverse();
    Ok(Schedule { activities, makespan, path })
}
