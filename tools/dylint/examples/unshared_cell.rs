use core::cell::{Cell, RefCell};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn summed(items: &[u32]) -> u32 {
    let total = Cell::new(0);
    for item in items {
        total.update(|current| item.strict_add(current));
    }
    total.get()
}

fn listed(items: &[u32]) -> Vec<u32> {
    let listed = RefCell::new(Vec::new());
    for item in items {
        listed.borrow_mut().push(*item);
    }
    listed.into_inner()
}

fn annotated(items: &[u32]) -> u32 {
    let total: Cell<u32> = Cell::default();
    for item in items {
        total.set(*item);
    }
    total.get()
}

fn captured() -> u32 {
    let total = Cell::new(0_u32);
    let bump = || total.update(|current| current.strict_add(1));
    bump();
    bump();
    total.get()
}

fn added(total: &Cell<u32>, items: &[u32]) {
    for item in items {
        total.update(|current| item.strict_add(current));
    }
}

fn shared(items: &[u32]) -> u32 {
    let total = Cell::new(0);
    added(&total, items);
    total.get()
}

fn returned(start: u32) -> Cell<u32> {
    let total = Cell::new(start);
    total.update(|current| current.strict_add(1));
    total
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let items = [1, 2, 3];
    summed(&items);
    listed(&items);
    annotated(&items);
    captured();
    shared(&items);
    added(&Cell::new(0), &items);
    returned(1);
}
