use core::{cmp::Reverse, hint::black_box};
use std::collections::{BinaryHeap, VecDeque};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Default)]
struct Store<T> {
    items: Vec<Option<T>>,
    next: Vec<usize>,
}

impl<T> Store<T> {
    fn insert(&mut self, item: T) -> usize {
        match self.next.pop() {
            Some(index) => {
                if let Some(slot) = self.items.get_mut(index) {
                    *slot = Some(item);
                }
                index
            }
            None => {
                self.items.push(Some(item));
                self.items.len().saturating_sub(1)
            }
        }
    }

    fn remove(&mut self, index: usize) -> Option<T> {
        let removed = self.items.get_mut(index)?.take();
        self.next.push(index);
        removed
    }
}

#[derive(Debug, Default)]
struct Pool {
    entries: Vec<Option<u32>>,
    vacant: BinaryHeap<Reverse<usize>>,
}

impl Pool {
    fn claim(&mut self, entry: u32) -> usize {
        let Some(Reverse(index)) = self.vacant.pop() else {
            self.entries.push(Some(entry));
            return self.entries.len().saturating_sub(1);
        };
        if let Some(slot) = self.entries.get_mut(index) {
            *slot = Some(entry);
        }
        index
    }

    fn release(&mut self, index: usize) -> Option<u32> {
        let released = self.entries.get_mut(index)?.take();
        self.vacant.push(Reverse(index));
        released
    }
}

#[derive(Debug, Default)]
struct Ledger {
    entries: Vec<Option<u32>>,
    order: Vec<usize>,
}

impl Ledger {
    fn record(&mut self, entry: u32) {
        self.order.push(self.entries.len());
        self.entries.push(Some(entry));
    }
}

#[derive(Debug, Default)]
struct Stack {
    values: Vec<u32>,
    marks: Vec<usize>,
}

impl Stack {
    fn mark(&mut self) -> Option<usize> {
        self.marks.push(self.values.len());
        self.marks.pop()
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let mut store = Store::default();
    let index = store.insert(1_u32);
    let mut ledger = Ledger::default();
    ledger.record(2);
    let mut pool = Pool::default();
    let claimed = pool.claim(3);
    black_box(&(store.remove(index), ledger, Stack::default().mark(), pool.release(claimed), Graph::default().reached(0)));
}

#[derive(Debug, Default)]
struct Graph {
    nodes: Vec<Option<Vec<usize>>>,
    frontier: VecDeque<usize>,
}

impl Graph {
    fn reached(&mut self, start: usize) -> usize {
        self.frontier.push_back(start);
        let mut count = 0_usize;
        while let Some(index) = self.frontier.pop_front() {
            count = count.saturating_add(1);
            self.frontier.extend(self.nodes.get(index).and_then(Option::as_ref).into_iter().flatten().copied());
        }
        count
    }
}
