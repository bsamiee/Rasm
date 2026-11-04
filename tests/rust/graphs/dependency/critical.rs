use std::collections::BTreeMap;

use enum_map::{EnumMap, enum_map};
use itertools::Either;
use nonempty_collections::nebts;
use petgraph::{acyclic::AcyclicEdgeError, data::Build as _, stable_graph::NodeIndex};
use rasm_dependency::{
    DependencyError, Evaluation, Refusal,
    critical::{Activity, Constraint, Event, Precedence, Schedule, schedule},
};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn date_overflow_retains_independent_causes() -> Result<(), AcyclicEdgeError<NodeIndex>> {
    let mut evaluation = Evaluation::new();
    let (failed, dependent, independent) = (evaluation.add_node(i64::MAX), evaluation.add_node(0), evaluation.add_node(i64::MAX));
    evaluation.try_add_edge(failed, dependent, ())?;
    let scheduled = schedule(
        &evaluation,
        |&duration| duration,
        |_| EnumMap::default(),
        |&()| Precedence { predecessor: Event::Finish, successor: Event::Start, lag: 0 },
        (i64::checked_add, i64::checked_sub),
        1,
        None,
    );
    assert_eq!(
        scheduled,
        Err(DependencyError::Refused { reason: Refusal::Date, nodes: nebts![failed, independent] }),
        "independent overflows must retain both causes, and dependent failures must retain their originating activity"
    );
    Ok(())
}

#[test]
fn unsigned_dates_preserve_representable_limits() -> Result<(), Either<AcyclicEdgeError<NodeIndex>, DependencyError>> {
    let mut evaluation = Evaluation::new();
    let (precedence, tail_duration) = (Precedence { predecessor: Event::Finish, successor: Event::Start, lag: 1 }, 2);
    let head_duration = u8::MAX.strict_sub(precedence.lag).strict_sub(tail_duration);
    let (first, last) = (evaluation.add_node(head_duration), evaluation.add_node(tail_duration));
    let relationship = evaluation.try_add_edge(first, last, precedence).map_err(Either::Left)?;
    let scheduled = schedule(&evaluation, |&duration| duration, |_| EnumMap::default(), Precedence::clone, (u8::checked_add, u8::checked_sub), 0, None).map_err(Either::Right)?;
    let dates = [(first, enum_map! { Event::Start => 0, Event::Finish => head_duration }), (last, enum_map! { Event::Start => u8::MAX.strict_sub(tail_duration), Event::Finish => u8::MAX })];
    assert_eq!(
        scheduled,
        Schedule {
            activities: BTreeMap::from(dates.map(|(node, dates)| (node, Activity { early: dates, late: dates, float: enum_map! { _ => 0 } }))),
            relationships: BTreeMap::from([(relationship, enum_map! { _ => 0 })]),
            makespan: u8::MAX,
        },
        "unsigned precedence must retain forward and backward dates, activity and relationship floats, and completion at the representable limit"
    );
    Ok(())
}

#[test]
fn loose_event_limits_preserve_representable_dates() -> Result<(), DependencyError> {
    let mut evaluation = Evaluation::<_, ()>::new();
    let node = evaluation.add_node(1_u8);
    let scheduled = schedule(
        &evaluation,
        |&duration| duration,
        |_| {
            enum_map! {
                Event::Start => Constraint { earliest: None, latest: Some(u8::MAX) },
                Event::Finish => Constraint { earliest: Some(0), latest: None },
            }
        },
        |&()| Precedence { predecessor: Event::Finish, successor: Event::Start, lag: 0 },
        (u8::checked_add, u8::checked_sub),
        0,
        None,
    )?;
    let dates = enum_map! { Event::Start => 0, Event::Finish => 1 };
    assert_eq!(scheduled, Schedule { activities: BTreeMap::from([(node, Activity { early: dates, late: dates, float: enum_map! { _ => 0 } })]), relationships: BTreeMap::new(), makespan: 1 });
    Ok(())
}
