use core::{
    hint::{self, black_box},
    sync::atomic::{AtomicBool, Ordering},
};
use std::{sync::Arc, thread};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Default)]
struct Worker {
    stop: AtomicBool,
    paused: AtomicBool,
    ready: AtomicBool,
}

#[derive(Debug, Default)]
struct Pair {
    left: AtomicBool,
}

#[derive(Debug, Default)]
struct Gate {
    open: AtomicBool,
    passes: u32,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Worker {
    fn halt(&self) { self.stop.store(true, Ordering::Release); }

    fn run(&self) -> u32 {
        let mut steps = 0_u32;
        while !self.stop.load(Ordering::Acquire) && steps < 8 {
            steps = steps.saturating_add(1);
        }
        steps
    }

    fn pause(&self, resume: bool) { self.paused.store(!resume, Ordering::Release); }

    fn hold(&self) { self.paused.store(true, Ordering::Release); }

    fn idle(&self) -> u32 {
        let mut spins = 0_u32;
        while !self.paused.load(Ordering::Acquire) && spins < 8 {
            spins = spins.saturating_add(1);
        }
        spins
    }

    fn arm(&self) { self.ready.store(true, Ordering::Release); }

    fn armed(&self) -> bool { self.ready.load(Ordering::Acquire) }
}

fn released(pair: &Pair) {
    let Pair { left } = pair;
    left.store(false, Ordering::Release);
}

fn spun(pair: &Pair) {
    pair.left.store(true, Ordering::Release);
    while !pair.left.load(Ordering::Acquire) {
        hint::spin_loop();
    }
}

fn counted(gate: &Gate) -> u32 {
    let Gate { open: _, passes } = gate;
    *passes
}

fn gated(gate: &Gate) {
    gate.open.store(true, Ordering::Release);
    while !gate.open.load(Ordering::Acquire) {
        hint::spin_loop();
    }
}

fn shared() -> thread::Result<u32> {
    let stop = Arc::new(AtomicBool::new(false));
    let flag = Arc::clone(&stop);
    let worker = thread::spawn(move || {
        let mut steps = 0_u32;
        while !flag.load(Ordering::Acquire) {
            steps = steps.saturating_add(1);
            hint::spin_loop();
        }
        steps
    });
    stop.store(true, Ordering::Release);
    worker.join()
}

fn watched(flag: &AtomicBool) { flag.store(false, Ordering::Release); }

fn handed() {
    let done = AtomicBool::new(false);
    thread::scope(|scope| {
        scope.spawn(|| {
            while !done.load(Ordering::Acquire) {
                hint::spin_loop();
            }
        });
        done.store(true, Ordering::Release);
    });
    watched(&done);
}

async fn polled() -> u32 {
    let stop = AtomicBool::new(false);
    let mut steps = 0_u32;
    stop.store(true, Ordering::Release);
    while !stop.load(Ordering::Acquire) && steps < 8 {
        steps = steps.saturating_add(1);
    }
    steps
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (worker, pair, gate) = (Worker::default(), Pair::default(), Gate::default());
    worker.halt();
    worker.pause(true);
    worker.hold();
    worker.arm();
    spun(&pair);
    released(&pair);
    gated(&gate);
    handed();
    watched(&AtomicBool::new(true));
    black_box(&([worker.run(), worker.idle(), counted(&gate), futures::executor::block_on(polled())], shared(), worker.armed(), pair.left.load(Ordering::Acquire)));
}
