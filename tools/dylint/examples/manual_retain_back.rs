use core::{cell::Cell, hint::black_box};
use std::collections::VecDeque;

// --- [TYPES] ---------------------------------------------------------------------------

trait Measured {
    fn len(&self) -> usize;
}

impl Measured for VecDeque<u32> {
    fn len(&self) -> usize { 0 }
}

trait Retained {
    fn retain_back(&mut self, kept: usize);
}

impl Retained for &mut VecDeque<u32> {
    fn retain_back(&mut self, _: usize) { self.clear(); }
}

// --- [MODELS] --------------------------------------------------------------------------

struct History {
    entries: VecDeque<u32>,
    limit: usize,
}

struct Observed(u32);

impl Drop for Observed {
    fn drop(&mut self) {
        if self.0 == 0 {
            std::panic::panic_any(self.0);
        }
    }
}

struct Limits {
    values: Vec<usize>,
    queries: Cell<usize>,
}

impl AsRef<Limits> for Limits {
    fn as_ref(&self) -> &Self {
        self.queries.update(|queries| queries.saturating_add(1));
        self
    }
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

fn nested(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    while recent.len() > kept {
        {
            {
                let _ = recent.pop_front();
            };
        };
    }
    recent
}

fn indexed(mut recent: VecDeque<u32>, kept: &[usize], index: fn() -> usize) -> VecDeque<u32> {
    while VecDeque::len(&recent) > kept[index()] {
        VecDeque::pop_front(&mut recent);
    }
    recent
}

fn qualified(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    while VecDeque::len(&recent) > kept {
        VecDeque::pop_front(&mut recent);
    }
    recent
}

fn observed(mut recent: VecDeque<Observed>, kept: usize) -> VecDeque<Observed> {
    while recent.len() > kept {
        recent.pop_front();
    }
    recent
}

fn shrinking(mut recent: VecDeque<u32>) -> VecDeque<u32> {
    while recent.len() > recent.len() / 2 {
        recent.pop_front();
    }
    recent
}

fn delegated(mut recent: VecDeque<u32>, limits: &Limits) -> VecDeque<u32> {
    while recent.len() > <&Limits as AsRef<Limits>>::as_ref(&limits).values.len() {
        recent.pop_front();
    }
    recent
}

fn overridden(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
    while <VecDeque<u32> as Measured>::len(&recent) > kept {
        recent.pop_front();
    }
    recent
}

fn shadowed(mut recent: &mut VecDeque<u32>, kept: usize) {
    while recent.len() > kept {
        recent.pop_front();
    }
    Retained::retain_back(&mut recent, kept);
}

fn ranged_self(mut recent: VecDeque<u32>) -> VecDeque<u32> {
    for _ in recent.len()..recent.len() {
        recent.pop_front();
    }
    recent
}

fn drained_self(mut recent: VecDeque<u32>) -> VecDeque<u32> {
    recent.drain(..recent.len().saturating_sub(recent.len()));
    recent
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let mut history = History { entries: VecDeque::from([1, 2, 3]), limit: 2 };
    history.trim();
    let recent = VecDeque::from([1, 2, 3, 4]);
    black_box(nested(recent.clone(), 2));
    black_box(indexed(recent.clone(), &[2], || 0));
    black_box(qualified(recent.clone(), 2));
    black_box(observed(VecDeque::from([Observed(1)]), 0));
    black_box(shrinking(recent.clone()));
    let limits = Limits { values: vec![1, 2], queries: Cell::new(0) };
    black_box((delegated(recent.clone(), &limits), limits.queries.get()));
    black_box(overridden(recent.clone(), 2));
    let mut borrowed = recent.clone();
    shadowed(&mut borrowed, 2);
    black_box(borrowed);
    black_box((ranged_self(recent.clone()), drained_self(recent.clone())));
    black_box(&(history.entries, bounded(recent.clone(), 2), reversed(recent.clone()), drained(recent.clone(), 2), collected(recent.clone(), 2), listed(vec![1, 2, 3], 1)));
    black_box(&(
        dropped(VecDeque::from([String::from("a")]), 2),
        assigned(recent.clone(), 2),
        guarded(recent.clone(), 2),
        counted(recent.clone(), 2),
        ranged(recent.clone(), 2),
        tested(recent.clone(), 2),
    ));
    black_box(shadowed_option::trim(recent.clone(), 2));
    black_box(&(subtracted(recent.clone(), 2), expanded(recent.clone(), 2), counted_other(recent.clone(), 2), matched(recent.clone(), Some(2)), back(recent, 2)));
}

mod shadowed_option {
    use super::VecDeque;

    // --- [TYPES] -----------------------------------------------------------------------

    trait Presence {
        fn is_some(self) -> bool;
    }

    impl<T> Presence for Option<T> {
        fn is_some(self) -> bool { false }
    }

    // --- [OPERATIONS] ------------------------------------------------------------------

    pub(super) fn trim(mut recent: VecDeque<u32>, kept: usize) -> VecDeque<u32> {
        while recent.len() > kept && recent.pop_front().is_some() {}
        recent
    }
}
