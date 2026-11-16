use core::cell::{Cell, Ref, RefCell};
use core::hint::black_box;

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

fn value_operations(start: u32) -> (u32, u32, u32) {
    let value = Cell::new(start);
    value.set(start.strict_add(1));
    let previous = value.replace(start.strict_add(2));
    let taken = value.take();
    (previous, taken, value.into_inner())
}

fn nested_replacement(start: u32) -> (u32, u32) {
    let value = Cell::new(start);
    let replaced = value.replace(value.get().strict_add(1));
    (replaced, value.into_inner())
}

fn consume_guard(value: Ref<'_, String>) -> usize { value.len() }

fn guarded(value: String) -> usize {
    let value = RefCell::new(value);
    consume_guard(value.borrow())
}

fn borrow_result() -> bool {
    let value = RefCell::new(1);
    let held = value.borrow();
    let refused = value.try_borrow_mut().is_err();
    black_box(&*held);
    refused
}

fn consume_cells(values: &[Cell<u32>; 2]) -> u32 { values.iter().map(Cell::get).sum() }

fn array_view(values: [u32; 2]) -> u32 {
    let values = Cell::new(values);
    consume_cells(values.as_array_of_cells())
}

fn noncopy_value(value: String) -> String {
    let value = Cell::new(value);
    value.into_inner()
}

fn shared_alias(external: &Cell<u32>) -> u32 {
    let ref value = *external;
    value.set(1);
    value.get()
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let items = [1, 2, 3];
    summed(&items);
    listed(&items);
    captured();
    shared(&items);
    added(&Cell::new(0), &items);
    returned(1);
    black_box((value_operations(1), nested_replacement(2), guarded(String::from("value")), borrow_result(), array_view([1, 2]), noncopy_value(String::from("owned"))));
    black_box(shared_alias(&Cell::new(0)));
}
