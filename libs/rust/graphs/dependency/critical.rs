use core::{iter, ops::Sub};
use std::collections::{BTreeMap, HashMap};

use petgraph::{Direction, algo::Measure, stable_graph::NodeIndex, visit::EdgeRef as _};

use crate::Evaluation;

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

#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub struct Horizon<T> {
    pub start: Option<T>,
    pub finish: Option<T>,
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

#[must_use]
pub fn schedule<N, E, T>(graph: &Evaluation<N, E>, duration: impl Fn(&N) -> T, precedence: impl Fn(&E) -> Precedence<T>, Horizon { start: from, finish: until }: Horizon<T>) -> Schedule<T>
where T: Measure + Copy + Ord + Sub<Output = T> {
    let from = from.unwrap_or_default();
    let sequence = graph.nodes_iter().collect::<Vec<_>>();
    let date = |event: Event, start: T, finish: T| match event {
        Event::Start => start,
        Event::Finish => finish,
    };
    let (early, end) = sequence.iter().fold((HashMap::with_capacity(sequence.len()), None), |(mut dates, end), &node| {
        let length = duration(&graph[node]);
        let (finish, driver) = graph
            .edges_directed(node, Direction::Incoming)
            .filter_map(|edge| {
                let Precedence { predecessor, successor, lag } = precedence(edge.weight());
                dates.get(&edge.source()).map(|&(start, finish, _): &(T, T, Option<NodeIndex>)| {
                    let bound = date(predecessor, start, finish).add(lag);
                    (date(successor, bound.add(length), bound), edge.source())
                })
            })
            .max_by_key(|&(finish, _)| finish)
            .filter(|&(finish, _)| finish >= from.add(length))
            .map_or_else(|| (from.add(length), None), |(finish, source)| (finish, Some(source)));
        dates.insert(node, (finish.sub(length), finish, driver));
        (dates, end.filter(|&(last, _)| last >= finish).or(Some((finish, node))))
    });
    let finished = end.map_or(from, |(finish, _)| finish);
    let deadline = until.unwrap_or(finished);
    let activities = sequence.iter().rev().filter_map(|node| early.get_key_value(node)).fold(HashMap::with_capacity(sequence.len()), |mut dates, (&node, &(start, finish, _))| {
        let length = finish.sub(start);
        let (late_finish, free_float) = graph
            .edges_directed(node, Direction::Outgoing)
            .filter_map(|edge| {
                let Precedence { predecessor, successor, lag } = precedence(edge.weight());
                dates.get(&edge.target()).map(|next: &Activity<T>| {
                    let bound = date(successor, next.late_start, next.late_finish).sub(lag);
                    (date(predecessor, bound.add(length), bound), date(successor, next.early_start, next.early_finish).sub(lag).sub(date(predecessor, start, finish)))
                })
            })
            .reduce(|(late_finish, free_float), (edge_finish, edge_float)| (late_finish.min(edge_finish), free_float.min(edge_float)))
            .map_or_else(|| (deadline, deadline.sub(finish)), |(late_finish, free_float)| (late_finish.min(deadline), free_float));
        dates.insert(node, Activity { early_start: start, early_finish: finish, late_start: late_finish.sub(length), late_finish, total_float: late_finish.sub(finish), free_float });
        dates
    });
    let mut path = iter::successors(end.map(|(_, node)| node), |node| early.get(node).and_then(|&(_, _, driver)| driver)).collect::<Vec<_>>();
    path.reverse();
    Schedule { activities: activities.into_iter().collect(), makespan: finished.sub(from), path }
}
