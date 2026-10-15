use core::hint::black_box;
use std::{io, thread};

use tokio::{
    runtime::Handle,
    task::{AbortHandle, JoinError, JoinHandle, JoinSet},
};
use tokio_util::{
    sync::CancellationToken,
    task::{AbortOnDropHandle, TaskTracker},
};

// --- [TYPES] ---------------------------------------------------------------------------

trait Release {
    fn release(&mut self, task: JoinHandle<u64>);
}

impl Release for JoinSet<u64> {
    fn release(&mut self, task: JoinHandle<u64>) { drop(task); }
}

impl Release for Vec<JoinHandle<u64>> {
    fn release(&mut self, task: JoinHandle<u64>) { self.push(task); }
}

// --- [MODELS] --------------------------------------------------------------------------

struct Worker {
    task: JoinHandle<u64>,
    guard: AbortOnDropHandle<u64>,
}

struct Releaser;

impl Drop for Releaser {
    fn drop(&mut self) { tokio::spawn(async {}); }
}

struct Released {
    tracker: TaskTracker,
    handle: Handle,
}

impl Drop for Released {
    fn drop(&mut self) { self.tracker.spawn_on(async {}, &self.handle); }
}

struct Service {
    task: JoinHandle<u64>,
    abort: AbortHandle,
    token: CancellationToken,
}

impl Service {
    fn stop(&self) { self.task.abort(); }
}

impl Drop for Service {
    fn drop(&mut self) {
        self.task.abort();
        self.abort.abort();
        self.token.cancel();
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn start(work: u64) -> JoinHandle<u64> { tokio::spawn(async move { work }) }

fn guarded(work: u64) -> AbortOnDropHandle<u64> { AbortOnDropHandle::new(tokio::spawn(async move { work })) }

fn worker(work: u64) -> Worker { Worker { task: tokio::spawn(async move { work }), guard: guarded(work) } }

fn restart(worker: &mut Worker) { worker.task = tokio::spawn(async { 2 }); }

fn rearm(worker: &mut Worker) { worker.guard = guarded(4); }

async fn deferred(fast: bool) -> Result<u64, JoinError> {
    let task;
    if fast {
        task = tokio::spawn(async { 3 });
    } else {
        task = tokio::spawn(async { 4 });
    }
    task.await
}

fn shutdown(token: &CancellationToken) { token.cancel(); }

fn statement() { tokio::spawn(async {}); }

fn on_handle(handle: &Handle) { handle.spawn(async {}); }

fn unread() { let _worker = thread::spawn(|| 1_i32); }

fn through_wrapper() { start(1); }

fn dropped() { drop(tokio::spawn(async {})); }

fn bound_then_dropped() {
    let task = tokio::spawn(async {});
    drop(task);
}

fn abort_handle() { let _abort = tokio::spawn(async {}).abort_handle(); }

fn finished() -> bool { tokio::spawn(async {}).is_finished() }

fn returned_abort() -> AbortHandle { tokio::spawn(async {}).abort_handle() }

async fn kept_abort() -> Result<(), JoinError> {
    let task = tokio::spawn(async {});
    let _abort = task.abort_handle();
    task.await
}

fn tracked(tracker: &TaskTracker) { tracker.spawn(async {}); }

fn tracked_path(tracker: &TaskTracker) { TaskTracker::spawn(tracker, async {}); }

async fn threaded() { let _worker = thread::spawn(|| 1_i32); }

fn named() -> io::Result<()> {
    thread::Builder::new().spawn(|| 1_i32)?;
    Ok(())
}

fn into_set(set: &mut JoinSet<u64>) { set.spawn(async { 1 }); }

fn tracked_future(tracker: &TaskTracker) { tokio::spawn(tracker.track_future(async {})); }

fn bound_tracked(tracker: &TaskTracker) {
    let task = tracker.spawn(async {});
    drop(task);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let tracker = TaskTracker::new();
    let mut set = JoinSet::new();
    statement();
    on_handle(&Handle::current());
    unread();
    threaded().await;
    through_wrapper();
    dropped();
    bound_then_dropped();
    abort_handle();
    finished();
    returned_abort().abort();
    tracked(&tracker);
    tracked_path(&tracker);
    into_set(&mut set);
    tracked_future(&tracker);
    bound_tracked(&tracker);
    set.release(start(2));
    let mut held = Vec::new();
    held.release(start(7));
    let mut built = worker(3);
    restart(&mut built);
    rearm(&mut built);
    drop(Releaser);
    drop(Released { tracker: tracker.clone(), handle: Handle::current() });
    let watched = tokio::spawn(async { 6_i32 });
    let service = Service { task: start(5), abort: watched.abort_handle(), token: CancellationToken::new() };
    service.stop();
    shutdown(&service.token);
    drop(service);
    tracker.close();
    tracker.wait().await;
    black_box(&(kept_abort().await, named(), deferred(true).await, watched.await, start(4).await, built.task.await, built.guard.await, set.join_all().await, held));
}
