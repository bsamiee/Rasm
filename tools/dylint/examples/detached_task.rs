use core::hint::black_box;
use std::thread;

use tokio::task::{JoinError, JoinHandle, JoinSet};
use tokio_util::task::{AbortOnDropHandle, TaskTracker};

// --- [MODELS] --------------------------------------------------------------------------

struct Worker {
    task: JoinHandle<u64>,
    guard: AbortOnDropHandle<u64>,
}

struct Releaser;

impl Drop for Releaser {
    fn drop(&mut self) { tokio::spawn(async {}); }
}

// --- [MACROS] --------------------------------------------------------------------------

macro_rules! spawned {
    ($work:expr) => {
        tokio::spawn($work);
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn statement() { tokio::spawn(async {}); }

fn unread() { let _worker = thread::spawn(|| 1_i32); }

async fn threaded() { let _worker = thread::spawn(|| 1_i32); }

fn dropped() { drop(tokio::spawn(async {})); }

fn finished() -> bool { tokio::spawn(async {}).is_finished() }

fn restart(worker: &mut Worker) { worker.task = tokio::spawn(async { 2 }); }

fn detached(set: &mut JoinSet<u64>) { set.detach_all(); }

fn expanded() {
    spawned!(async {});
}

fn tracked(tracker: &TaskTracker) { tracker.spawn(async {}); }

fn tracked_path(tracker: &TaskTracker) { TaskTracker::spawn(tracker, async {}); }

fn tracked_future(tracker: &TaskTracker) { tokio::spawn(tracker.track_future(async {})); }

fn bound_tracked(tracker: &TaskTracker) {
    let task = tracker.spawn(async {});
    drop(task);
}

async fn kept_abort() -> Result<(), JoinError> {
    let task = tokio::spawn(async {});
    let _abort = task.abort_handle();
    task.await
}

fn rearm(worker: &mut Worker) { worker.guard = AbortOnDropHandle::new(tokio::spawn(async { 4 })); }

async fn deferred(fast: bool) -> Result<u64, JoinError> {
    let task;
    if fast {
        task = tokio::spawn(async { 3 });
    } else {
        task = tokio::spawn(async { 4 });
    }
    task.await
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let tracker = TaskTracker::new();
    let mut worker = Worker { task: tokio::spawn(async { 1 }), guard: AbortOnDropHandle::new(tokio::spawn(async { 1 })) };
    statement();
    unread();
    threaded().await;
    dropped();
    expanded();
    detached(&mut JoinSet::new());
    tracked(&tracker);
    tracked_path(&tracker);
    tracked_future(&tracker);
    bound_tracked(&tracker);
    restart(&mut worker);
    rearm(&mut worker);
    drop(Releaser);
    tracker.close();
    tracker.wait().await;
    black_box(&(finished(), kept_abort().await, deferred(true).await, worker.task.await, worker.guard.await));
}
