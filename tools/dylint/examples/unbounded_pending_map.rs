#![feature(sync_nonpoison, nonpoison_mutex)]

use core::hint::black_box;
use std::{
    collections::BTreeMap,
    sync::{Arc, nonpoison::Mutex},
};

use futures::future::{BoxFuture, FutureExt as _};
use tokio::sync::oneshot;

// --- [MODELS] --------------------------------------------------------------------------

struct Call {
    future: BoxFuture<'static, u32>,
}

#[derive(Default)]
struct Parked {
    waiting: Mutex<BTreeMap<u64, oneshot::Sender<u32>>>,
}

#[derive(Default)]
struct Paused {
    calls: Mutex<BTreeMap<u64, (Call, u32)>>,
}

#[derive(Default)]
struct Batched {
    waiting: Arc<Mutex<BTreeMap<u64, oneshot::Sender<u32>>>>,
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
struct Expiring {
    waiting: Arc<Mutex<BTreeMap<u64, oneshot::Sender<u32>>>>,
}

#[derive(Default)]
struct Handed {
    waiting: Mutex<BTreeMap<u64, oneshot::Sender<u32>>>,
}

#[derive(Default)]
struct Tried {
    waiting: Mutex<BTreeMap<u64, oneshot::Sender<u32>>>,
}

#[derive(Default)]
struct Counts {
    seen: Mutex<BTreeMap<u64, u32>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Parked {
    fn park(&self, key: u64) -> oneshot::Receiver<u32> {
        let (sender, receiver) = oneshot::channel();
        self.waiting.lock().insert(key, sender);
        receiver
    }

    fn resume(&self, key: u64) -> Option<oneshot::Sender<u32>> { self.waiting.lock().remove(&key) }
}

impl Paused {
    fn pause(&self, key: u64, step: u32) -> Option<(Call, u32)> { self.calls.lock().insert(key, (Call { future: async { 0 }.boxed() }, step)) }

    fn resume(&self, key: u64) -> Option<BoxFuture<'static, u32>> { self.calls.lock().remove(&key).map(|(call, _)| call.future) }
}

impl Batched {
    fn park(&self, key: u64, sender: oneshot::Sender<u32>) -> usize {
        let mut waiting = self.waiting.lock();
        waiting.insert(key, sender);
        waiting.len()
    }
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
    fn drop(&mut self) { black_box(&self.leases.waiting.lock().remove(&self.key)); }
}

impl Expiring {
    fn park(&self, key: u64, sender: oneshot::Sender<u32>) -> impl Future<Output = Option<oneshot::Sender<u32>>> + use<> {
        self.waiting.lock().insert(key, sender);
        let waiting = Arc::clone(&self.waiting);
        async move { waiting.lock().remove(&key) }
    }
}

impl Handed {
    fn park(&self, key: u64, sender: oneshot::Sender<u32>) -> usize {
        let mut waiting = self.waiting.lock();
        waiting.insert(key, sender);
        black_box(&mut *waiting).len()
    }
}

impl Tried {
    fn park(&self, key: u64, sender: oneshot::Sender<u32>) -> Option<oneshot::Sender<u32>> { self.waiting.lock().insert(key, sender) }

    fn peek(&self) -> bool { self.waiting.try_lock().is_ok() }
}

impl Counts {
    fn count(&self, key: u64) -> Option<u32> { self.seen.lock().insert(key, 1) }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (parked, paused, batched, swept, leases, expiring, counts) =
        (Parked::default(), Paused::default(), Batched::default(), Swept::default(), Leases::default(), Expiring::default(), Counts::default());
    black_box(&(parked.park(1), parked.resume(1), paused.pause(2, 3).is_some(), paused.resume(2).is_some(), batched.park(4, oneshot::channel().0)));
    black_box(&(swept.park(5, oneshot::channel().0), counts.count(6)));
    let (handed, tried) = (Handed::default(), Tried::default());
    black_box(&(handed.park(9, oneshot::channel().0), tried.park(10, oneshot::channel().0), tried.peek()));
    swept.sweep();
    drop(leases.lease(7, oneshot::channel().0));
    drop(expiring.park(8, oneshot::channel().0));
}
