#![feature(sync_nonpoison, nonpoison_mutex)]

use core::hint::black_box;
use std::{
    collections::{BTreeMap, HashMap},
    sync::{Arc, nonpoison::Mutex},
};

use futures::future::{BoxFuture, FutureExt as _};
use tokio::{sync::oneshot, task::JoinHandle};

// --- [MODELS] --------------------------------------------------------------------------

struct Call {
    future: BoxFuture<'static, u32>,
}

#[derive(Default)]
struct Parked {
    waiting: Mutex<BTreeMap<u64, oneshot::Sender<u32>>>,
}

#[derive(Default)]
struct Tasks {
    running: Mutex<HashMap<u64, JoinHandle<()>>>,
}

#[derive(Default)]
struct Paused {
    calls: Mutex<BTreeMap<u64, (Call, u32)>>,
}

#[derive(Default)]
struct Swept {
    waiting: Mutex<BTreeMap<u64, oneshot::Sender<u32>>>,
}

#[derive(Default)]
struct Leases {
    waiting: Mutex<BTreeMap<u64, oneshot::Sender<u32>>>,
}

struct Lease<'leases> {
    leases: &'leases Leases,
    key: u64,
}

#[derive(Default)]
struct Counts {
    seen: Mutex<BTreeMap<u64, u32>>,
}

#[derive(Default)]
struct Batched {
    waiting: Mutex<BTreeMap<u64, oneshot::Sender<u32>>>,
}

#[derive(Default)]
struct Expiring {
    waiting: Arc<Mutex<BTreeMap<u64, oneshot::Sender<u32>>>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Parked {
    fn park(&self, key: u64) -> oneshot::Receiver<u32> {
        let (sender, receiver) = oneshot::channel();
        self.waiting.lock().insert(key, sender);
        receiver
    }

    fn resume(&self, key: u64, value: u32) -> bool { self.waiting.lock().remove(&key).is_some_and(|sender| sender.send(value).is_ok()) }
}

impl Tasks {
    fn start(&self, key: u64, task: JoinHandle<()>) -> Option<JoinHandle<()>> { self.running.lock().insert(key, task) }

    fn stop(&self, key: u64) -> bool { self.running.lock().remove(&key).is_some_and(|task| task.is_finished()) }
}

impl Paused {
    fn pause(&self, key: u64, step: u32) -> Option<(Call, u32)> { self.calls.lock().insert(key, (Call { future: async { 0 }.boxed() }, step)) }

    fn resume(&self, key: u64) -> Option<BoxFuture<'static, u32>> { self.calls.lock().remove(&key).map(|(call, _)| call.future) }
}

impl Swept {
    fn park(&self, key: u64, sender: oneshot::Sender<u32>) -> Option<oneshot::Sender<u32>> { self.waiting.lock().insert(key, sender) }

    fn sweep(&self) { self.waiting.lock().retain(|_, sender| !sender.is_closed()); }
}

impl Leases {
    fn lease(&self, key: u64, sender: oneshot::Sender<u32>) -> Lease<'_> {
        self.waiting.lock().insert(key, sender);
        Lease { leases: self, key }
    }
}

impl Drop for Lease<'_> {
    fn drop(&mut self) { drop(self.leases.waiting.lock().remove(&self.key)); }
}

impl Counts {
    fn count(&self, key: u64) -> Option<u32> { self.seen.lock().insert(key, 1) }
}

impl Batched {
    fn park(&self, key: u64, sender: oneshot::Sender<u32>) -> usize {
        let mut waiting = self.waiting.lock();
        waiting.insert(key, sender);
        waiting.len()
    }
}

impl Expiring {
    fn park(&self, key: u64, sender: oneshot::Sender<u32>) -> impl Future<Output = ()> + use<> {
        self.waiting.lock().insert(key, sender);
        let waiting = Arc::clone(&self.waiting);
        async move { drop(waiting.lock().remove(&key)) }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (parked, tasks, paused, swept, leases, counts, batched, expiring) =
        (Parked::default(), Tasks::default(), Paused::default(), Swept::default(), Leases::default(), Counts::default(), Batched::default(), Expiring::default());
    black_box((parked.park(1), parked.resume(1, 2), tasks.stop(3), paused.pause(4, 5).is_some(), paused.resume(4).is_some(), counts.count(6)));
    black_box((swept.park(7, oneshot::channel().0).is_some(), batched.park(8, oneshot::channel().0)));
    swept.sweep();
    drop(leases.lease(9, oneshot::channel().0));
    drop(expiring.park(10, oneshot::channel().0));
    drop(tokio::runtime::Runtime::new().map(|runtime| tasks.start(11, runtime.spawn(async {}))));
}
