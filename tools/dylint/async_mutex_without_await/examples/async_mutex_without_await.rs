use std::{error::Error, sync::Arc};

use tokio::{
    sync::{Mutex, MutexGuard, RwLock, TryLockError},
    task,
};

// --- [SERVICES] ------------------------------------------------------------------------

static TOTAL: Mutex<u64> = Mutex::const_new(0);

#[derive(Debug, Default)]
struct Counter {
    hits: Mutex<u64>,
    samples: RwLock<Vec<u64>>,
}

impl Counter {
    async fn bump(&self) -> u64 {
        let mut hits = self.hits.lock().await;
        *hits += 1;
        *hits
    }

    async fn add(&self, sample: u64) { self.samples.write().await.push(sample); }

    async fn sum(&self) -> u64 { self.samples.read().await.iter().sum() }
}

#[derive(Debug, Default)]
struct Shared {
    count: Arc<Mutex<u64>>,
}

impl Clone for Shared {
    fn clone(&self) -> Self { Self { count: Arc::clone(&self.count) } }
}

impl Shared {
    async fn increment(&self) -> u64 {
        let mut count = self.count.lock().await;
        *count += 1;
        *count
    }
}

#[derive(Debug, Default)]
struct Ledger {
    entries: Mutex<Vec<u64>>,
}

impl Ledger {
    async fn record(&self, value: u64) {
        let mut entries = self.entries.lock().await;
        task::yield_now().await;
        entries.push(value);
    }
}

#[derive(Debug, Default)]
struct Destructured {
    count: Mutex<u64>,
}

impl Destructured {
    async fn bump(&self) { *self.count.lock().await += 1; }

    async fn hold(&self) -> u64 {
        let Self { count } = self;
        let guard = count.lock().await;
        task::yield_now().await;
        *guard
    }
}

#[derive(Debug, Default)]
struct Unpacked {
    count: Mutex<u64>,
}

impl Unpacked {
    async fn bump(&self) -> u64 {
        let Self { count } = self;
        let mut guard = count.lock().await;
        *guard += 1;
        *guard
    }
}

#[derive(Debug, Default)]
struct Escapes {
    slots: Mutex<Vec<u64>>,
    queue: Mutex<u64>,
    pending: Mutex<u64>,
}

impl Escapes {
    async fn total(&self) -> u64 { total(&self.slots).await }

    async fn hold(&self) -> MutexGuard<'_, u64> { self.queue.lock().await }

    fn peek(&self) -> Result<u64, TryLockError> { self.pending.try_lock().map(|pending| *pending) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn total(slots: &Mutex<Vec<u64>>) -> u64 { slots.lock().await.iter().sum() }

async fn local() -> u64 {
    let total = Mutex::new(0);
    *total.lock().await += 1;
    total.into_inner()
}

fn initial() -> u64 { *TOTAL.blocking_lock() }

async fn current() -> u64 { *TOTAL.lock().await }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() -> Result<(), Box<dyn Error + Send + Sync>> {
    let counter = Counter::default();
    let shared = Shared::default();
    let ledger = Ledger::default();
    let escapes = Escapes::default();
    let destructured = Destructured::default();
    let unpacked = Unpacked::default();
    destructured.bump().await;
    counter.add(counter.bump().await).await;
    let observed = [
        counter.sum().await,
        shared.clone().increment().await,
        shared.increment().await,
        *escapes.hold().await,
        escapes.total().await,
        destructured.hold().await,
        unpacked.bump().await,
        escapes.peek()?,
        local().await,
        current().await,
        task::spawn_blocking(initial).await?,
    ];
    ledger.record(observed.iter().sum()).await;
    Ok(())
}
