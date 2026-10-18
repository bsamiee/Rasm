#![feature(sync_nonpoison, nonpoison_mutex, nonpoison_rwlock, lock_value_accessors)]

use core::hint::black_box;
use std::sync::{
    Arc,
    nonpoison::{Mutex, RwLock},
};

use tokio::sync::watch;

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

#[derive(Debug, Default)]
struct Published {
    current: watch::Sender<Config>,
}

#[derive(Debug)]
struct Overwritten {
    current: watch::Sender<Config>,
}

#[derive(Debug, Default)]
struct Watched {
    current: watch::Sender<Config>,
}

#[derive(Debug, Default)]
struct Tweaked {
    current: watch::Sender<Config>,
}

#[derive(Debug)]
struct Paired {
    current: watch::Sender<Config>,
}

#[derive(Debug, Default)]
struct Plain {
    config: RwLock<Config>,
}

#[derive(Debug, Default)]
struct Unpacked {
    snapshot: RwLock<Arc<Config>>,
}

#[derive(Debug, Default)]
struct Accessed {
    snapshot: RwLock<Arc<Config>>,
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

impl Published {
    fn update(&self, next: Config) -> Config { self.current.send_replace(next) }

    fn port(&self) -> u16 { self.current.borrow().port }
}

impl Overwritten {
    fn new() -> Self { Self { current: watch::Sender::default() } }

    fn update(&self, next: Config) { self.current.send_modify(|current| *current = next); }

    fn port(&self) -> u16 { self.current.borrow().port }
}

impl Watched {
    fn update(&self, next: Config) -> Config { self.current.send_replace(next) }

    fn watch(&self) -> watch::Receiver<Config> { self.current.subscribe() }
}

impl Tweaked {
    fn retune(&self, port: u16) { self.current.send_modify(|current| current.port = port); }

    fn port(&self) -> u16 { self.current.borrow().port }
}

impl Paired {
    fn new() -> (Self, watch::Receiver<Config>) {
        let (current, receiver) = watch::channel(Config::default());
        (Self { current }, receiver)
    }

    fn update(&self, next: Config) -> Config { self.current.send_replace(next) }
}

impl Plain {
    fn update(&self, next: Config) { *self.config.write() = next; }

    fn port(&self) -> u16 { self.config.read().port }
}

impl Unpacked {
    fn update(&self, next: Config) { *self.snapshot.write() = Arc::new(next); }

    fn into_snapshot(self) -> Arc<Config> {
        let Self { snapshot } = self;
        snapshot.into_inner()
    }
}

impl Accessed {
    fn update(&self, next: Config) { self.snapshot.set(Arc::new(next)); }

    fn current(&self) -> Arc<Config> { self.snapshot.get_cloned() }
}

fn shared() -> u16 {
    let lock = Arc::new(Mutex::new(Arc::new(Config::default())));
    let writer = Arc::clone(&lock);
    writer.set(Arc::new(Config { port: 9 }));
    lock.lock().port
}

fn local() -> u16 {
    let sender = watch::Sender::new(Config::default());
    sender.send_replace(Config { port: 10 });
    sender.borrow().port
}

fn channel() -> u16 {
    let (sender, receiver) = watch::channel(Config::default());
    sender.send_replace(Config { port: 11 });
    receiver.borrow().port
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (registry, cache, tuned, lent) = (Registry::default(), Cache::default(), Tuned::default(), Lent::default());
    let (published, overwritten, watched, tweaked, (paired, receiver)) = (Published::default(), Overwritten::new(), Watched::default(), Tweaked::default(), Paired::new());
    registry.update(Config { port: 80 });
    cache.update(Config { port: 81 });
    tuned.retune(82);
    tuned.replace(Config { port: 83 });
    lent.update(Config { port: 84 });
    overwritten.update(Config { port: 85 });
    tweaked.retune(86);
    let (plain, unpacked, accessed) = (Plain::default(), Unpacked::default(), Accessed::default());
    plain.update(Config { port: 90 });
    unpacked.update(Config { port: 91 });
    accessed.update(Config { port: 92 });
    black_box(&(plain.port(), unpacked.into_snapshot(), accessed.current()));
    black_box([published.update(Config { port: 87 }), watched.update(Config { port: 88 }), paired.update(Config { port: 89 })]);
    black_box(&(watched.watch(), receiver));
    black_box([registry.current().port, cache.port(), tuned.port(), lent.lock().lock().port, shared(), published.port(), overwritten.port(), tweaked.port(), local(), channel()]);
}
