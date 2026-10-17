use core::hint::black_box;
use std::collections::VecDeque;

// --- [MODELS] --------------------------------------------------------------------------

struct History {
    entries: VecDeque<u32>,
    limit: usize,
}

impl History {
    fn trim(&mut self) {
        while self.entries.len() > self.limit {
            self.entries.pop_front();
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn bounded(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    while recent.len() > kept {
        let _ = recent.pop_front();
    }
    recent
}

fn reversed(mut recent: VecDeque<u32>) -> VecDeque<u32> {
    while 3 < recent.len() {
        recent.pop_front();
    }
    recent
}

fn drained(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    recent.drain(..recent.len().saturating_sub(kept));
    recent
}

fn collected(mut recent: VecDeque<u32>, kept: usize) -> Vec<u32> { recent.drain(..recent.len().saturating_sub(kept)).collect() }

fn dropped(mut recent: VecDeque<String>, kept: usize) -> VecDeque<String> {
    while recent.len() > kept {
        drop(recent.pop_front());
    }
    recent
}

fn assigned(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    while recent.len() > kept {
        _ = recent.pop_front();
    }
    recent
}

fn guarded(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    loop {
        if recent.len() <= kept {
            break;
        }
        recent.pop_front();
    }
    recent
}

fn counted(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    for _ in 0..recent.len().saturating_sub(kept) {
        recent.pop_front();
    }
    recent
}

fn ranged(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    for _ in kept..recent.len() {
        recent.pop_front();
    }
    recent
}

fn tested(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    while recent.len() > kept && recent.pop_front().is_some() {}
    recent
}

fn subtracted(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    recent.drain(..recent.len() - kept);
    recent
}

macro_rules! trimmed {
    ($recent:expr, $kept:expr) => {
        while $recent.len() > $kept {
            $recent.pop_front();
        }
    };
}

fn expanded(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    trimmed!(recent, kept);
    recent
}

fn counted_other(mut recent: VecDeque<u32>, kept: usize) -> u32 {
    let mut total = 0;
    while recent.len() > kept {
        total += recent.pop_front().unwrap_or_default();
    }
    total
}

fn back(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    while recent.len() > kept {
        recent.pop_back();
    }
    recent
}

fn matched(mut recent: VecDeque<u32>, kept: Option<usize>) -> VecDeque<u32> {
    match kept {
        Some(kept) =>
            while recent.len() > kept {
                recent.pop_front();
            },
        None => {}
    }
    recent
}

fn listed(mut recent: Vec<u32>, kept: usize) -> Vec<u32> {
    while recent.len() > kept {
        recent.remove(0);
    }
    recent
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let mut history = History { entries: VecDeque::from([1, 2, 3]), limit: 2 };
    history.trim();
    let recent = VecDeque::from([1, 2, 3, 4]);
    black_box(&(history.entries, bounded(recent.clone(), 2), reversed(recent.clone()), drained(recent.clone(), 2), collected(recent.clone(), 2), listed(vec![1, 2, 3], 1)));
    black_box(&(
        dropped(VecDeque::from([String::from("a")]), 2),
        assigned(recent.clone(), 2),
        guarded(recent.clone(), 2),
        counted(recent.clone(), 2),
        ranged(recent.clone(), 2),
        tested(recent.clone(), 2),
    ));
    black_box(&(subtracted(recent.clone(), 2), expanded(recent.clone(), 2), counted_other(recent.clone(), 2), matched(recent.clone(), Some(2)), back(recent, 2)));
}
