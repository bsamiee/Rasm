use core::{
    cell::Cell,
    hint::black_box,
    ops::{Deref, DerefMut},
};

// --- [TYPES] ---------------------------------------------------------------------------

trait Toggle {
    fn toggle(&mut self);
}

impl Toggle for &mut bool {
    fn toggle(&mut self) { **self = false; }
}

// --- [MODELS] --------------------------------------------------------------------------

struct Observed {
    value: bool,
    reads: Cell<u8>,
}

impl Deref for Observed {
    type Target = bool;

    fn deref(&self) -> &bool {
        self.reads.update(|reads| reads.saturating_add(1));
        &self.value
    }
}

impl DerefMut for Observed {
    fn deref_mut(&mut self) -> &mut bool {
        self.reads.update(|reads| reads.saturating_add(1));
        &mut self.value
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn flipped(flag: &mut bool) { *flag = !*flag; }

fn xored(flag: &mut bool) { *flag ^= true; }

fn inverted(bits: &mut u32) { *bits = !*bits; }

fn copied(flag: &mut bool, other: bool) { *flag = !other; }

fn kept() -> bool {
    let mut flag = true;
    flag ^= false;
    flag
}

fn set(flag: &mut bool) { *flag |= true; }

fn boxed(flag: &mut Box<bool>) { **flag = !**flag; }

fn observed(flag: &mut Observed) { **flag = !**flag; }

fn shadowed(mut flag: &mut bool) {
    *flag = !*flag;
    flag.toggle();
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (mut flag, mut bits) = (true, 1_u32);
    flipped(&mut flag);
    xored(&mut flag);
    inverted(&mut bits);
    copied(&mut flag, false);
    set(&mut flag);
    let mut held = Box::new(flag);
    boxed(&mut held);
    black_box(held);
    let mut counted = Observed { value: true, reads: Cell::new(0) };
    observed(&mut counted);
    shadowed(&mut flag);
    black_box((counted.value, counted.reads.get()));
    black_box(&(flag, bits, kept()));
}
