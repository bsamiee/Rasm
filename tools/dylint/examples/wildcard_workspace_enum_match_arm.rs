use core::{cmp::Ordering, hint::black_box};
use std::collections::VecDeque;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Signal {
    Start,
    Pause,
    Resume,
    Stop,
}

#[derive(Debug, Clone, Copy)]
enum Command {
    Send { signal: Signal },
    Wait,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn wildcard(signal: Signal) -> u8 {
    match signal {
        Signal::Start => 0,
        _ => 1,
    }
}

const fn bound(signal: Signal) -> Signal {
    match signal {
        Signal::Stop => Signal::Start,
        other => other,
    }
}

const fn hidden(signal: Signal) -> u8 {
    match signal {
        Signal::Start => 0,
        Signal::Pause | _ => 2,
    }
}

const fn borrowed(signal: &Signal) -> u8 {
    match signal {
        Signal::Resume => 3,
        Signal::Pause => 4,
        &_ => 5,
    }
}

const fn named(signal: Signal) -> Signal {
    match signal {
        Signal::Stop => Signal::Start,
        other @ (Signal::Start | Signal::Pause | Signal::Resume) => other,
    }
}

const fn guarded(signal: Signal, armed: bool) -> u8 {
    match signal {
        Signal::Start => 0,
        _ if armed => 1,
        Signal::Pause | Signal::Resume | Signal::Stop => 2,
    }
}

const fn armed(signal: Signal, ready: bool) -> u8 {
    match signal {
        Signal::Start if ready => 0,
        _ => 1,
    }
}

const fn sent(command: Command) -> u8 {
    match command {
        Command::Send { signal: Signal::Start } => 0,
        Command::Send { signal: _ } => 1,
        Command::Wait => 2,
    }
}

const fn foreign(order: Ordering) -> u8 {
    match order {
        Ordering::Less => 0,
        _ => 1,
    }
}

const fn member(invalid: &rasm_mcp::Invalid) -> u8 {
    match invalid {
        rasm_mcp::Invalid::Route { pattern: _, source: _ } => 0,
        _ => 1,
    }
}

const fn optional(value: Option<Signal>) -> u8 {
    match value {
        Some(Signal::Start) => 0,
        _ => 1,
    }
}

const fn if_let(signal: Signal) -> u8 { if let Signal::Start = signal { 0 } else { 1 } }

const fn if_let_alone(signal: Signal) -> u8 {
    if let Signal::Start = signal {
        return 0;
    }
    1
}

const fn let_else(signal: Signal) -> Option<u8> {
    let Signal::Pause = signal else { return None };
    Some(1)
}

const fn let_else_set(signal: Signal) -> Option<u8> {
    let (Signal::Pause | Signal::Stop) = signal else { return None };
    Some(1)
}

const fn tested(signal: Signal) -> bool { matches!(signal, Signal::Stop) }

const fn tested_set(signal: Signal) -> bool { matches!(signal, Signal::Start | Signal::Resume) }

const fn tested_branch(signal: Signal) -> u8 { if !matches!(signal, Signal::Stop) { 0 } else { 1 } }

fn compared(signal: Signal) -> u8 { if signal == Signal::Resume { 0 } else { 1 } }

fn looped(mut signal: Signal) -> Signal {
    while signal == Signal::Pause {
        signal = Signal::Resume;
    }
    signal
}

const fn nested(value: Option<Signal>) -> u8 {
    match value {
        Some(Signal::Start) => 0,
        Some(_) => 1,
        None => 2,
    }
}

const fn paired(left: Signal, right: Signal) -> u8 {
    match (left, right) {
        (Signal::Start, Signal::Start) => 0,
        (_, _) => 1,
    }
}

const fn paired_count(left: Signal, count: u8) -> u8 {
    match (left, count) {
        (Signal::Start, 0) => 0,
        (Signal::Pause | Signal::Resume | Signal::Stop, count) => count,
        (Signal::Start, _) => 1,
    }
}

const fn outcome(value: Result<Signal, Signal>) -> u8 {
    match value {
        Ok(Signal::Start) => 0,
        Ok(_) | Err(_) => 1,
    }
}

macro_rules! classified {
    ($signal:expr) => {
        match $signal {
            Signal::Start => 0,
            _ => 1,
        }
    };
}

const fn expanded(signal: Signal) -> u8 { classified!(signal) }

fn listed(left: Signal, right: Signal) -> u8 { if [left, right].contains(&Signal::Stop) { 0 } else { 1 } }

fn either(left: Signal, right: Signal) -> u8 { if left == Signal::Stop || right == Signal::Pause { 0 } else { 1 } }

const fn chained(signal: Signal, ready: bool) -> u8 {
    if let Signal::Start = signal
        && ready
    {
        0
    } else {
        1
    }
}

fn queued(signals: &VecDeque<Signal>) -> u8 { if signals.contains(&Signal::Resume) { 0 } else { 1 } }

fn held(codes: &[u8]) -> u8 { if codes.contains(&3) { 0 } else { 1 } }

fn ordered(orders: &[Ordering]) -> u8 { if orders.contains(&Ordering::Less) { 0 } else { 1 } }

fn swept(signals: &[Signal]) -> u8 {
    if signals.iter().any(|signal| match signal {
        Signal::Stop => true,
        Signal::Start | Signal::Pause | Signal::Resume => false,
    }) {
        0
    } else {
        1
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    black_box(&(
        wildcard(Signal::Start),
        bound(Signal::Stop),
        hidden(Signal::Pause),
        borrowed(&Signal::Resume),
        named(Signal::Pause),
        guarded(Signal::Stop, true),
        armed(Signal::Stop, true),
        [sent(Command::Wait), sent(Command::Send { signal: Signal::Pause })],
        foreign(Ordering::Less),
        optional(None),
        member,
        [if_let(Signal::Stop), if_let_alone(Signal::Stop), compared(Signal::Stop), tested_branch(Signal::Stop)],
        (let_else(Signal::Stop), let_else_set(Signal::Start), tested(Signal::Stop), tested_set(Signal::Pause), looped(Signal::Pause), stopped(Signal::Stop), paused(Signal::Pause)),
        [nested(None), paired(Signal::Stop, Signal::Start), paired_count(Signal::Stop, 2), outcome(Ok(Signal::Stop)), expanded(Signal::Stop)],
        (listed, either, chained, queued, held, ordered, swept),
    ));
}

const fn stopped(signal: Signal) -> bool {
    match signal {
        Signal::Stop => true,
        _ => false,
    }
}

const fn paused(signal: Signal) -> bool { if let Signal::Pause = signal { true } else { false } }
