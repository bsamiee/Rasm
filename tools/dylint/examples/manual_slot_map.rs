use core::hint::black_box;

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
    black_box(&(store.remove(index), ledger, Stack::default().mark()));
}
