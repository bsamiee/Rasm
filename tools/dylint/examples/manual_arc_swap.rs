#![feature(sync_nonpoison, nonpoison_mutex, nonpoison_rwlock, lock_value_accessors)]

use core::hint::black_box;
use std::sync::{
    Arc,
    nonpoison::{Mutex, RwLock},
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Default)]
struct Config {
    port: u16,
}

#[derive(Debug, Default)]
struct Registry {
    snapshot: RwLock<Arc<Config>>,
}

#[derive(Debug, Default)]
struct Cache {
    latest: Mutex<Arc<Config>>,
}

#[derive(Debug, Default)]
struct Tuned {
    snapshot: RwLock<Arc<Config>>,
}

#[derive(Debug, Default)]
struct Lent {
    snapshot: Mutex<Arc<Config>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Registry {
    fn update(&self, next: Config) { *self.snapshot.write() = Arc::new(next); }

    fn current(&self) -> Arc<Config> { Arc::clone(&self.snapshot.read()) }
}

impl Cache {
    fn update(&self, next: Config) { self.latest.set(Arc::new(next)); }

    fn port(&self) -> u16 { self.latest.lock().port }
}

impl Tuned {
    fn retune(&self, port: u16) { Arc::make_mut(&mut *self.snapshot.write()).port = port; }

    fn replace(&self, next: Config) { *self.snapshot.write() = Arc::new(next); }

    fn port(&self) -> u16 { self.snapshot.read().port }
}

impl Lent {
    fn update(&self, next: Config) { *self.snapshot.lock() = Arc::new(next); }

    const fn lock(&self) -> &Mutex<Arc<Config>> { &self.snapshot }
}

fn shared() -> u16 {
    let lock = Arc::new(Mutex::new(Arc::new(Config::default())));
    let writer = Arc::clone(&lock);
    writer.set(Arc::new(Config { port: 9 }));
    lock.lock().port
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (registry, cache, tuned, lent) = (Registry::default(), Cache::default(), Tuned::default(), Lent::default());
    registry.update(Config { port: 80 });
    cache.update(Config { port: 81 });
    tuned.retune(82);
    tuned.replace(Config { port: 83 });
    lent.update(Config { port: 84 });
    black_box([registry.current().port, cache.port(), tuned.port(), lent.lock().lock().port, shared()]);
}
