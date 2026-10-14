use std::thread;

use tokio::{
    runtime::Handle,
    task::{AbortHandle, JoinError, JoinHandle, JoinSet},
};
use tokio_util::task::{AbortOnDropHandle, TaskTracker};

// --- [TYPES] ---------------------------------------------------------------------------

trait Release {
    fn release(&mut self, task: JoinHandle<u64>);
}

impl Release for JoinSet<u64> {
    fn release(&mut self, task: JoinHandle<u64>) { drop(task); }
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

// --- [OPERATIONS] ----------------------------------------------------------------------

fn start(work: u64) -> JoinHandle<u64> { tokio::spawn(async move { work }) }

fn guarded(work: u64) -> AbortOnDropHandle<u64> { AbortOnDropHandle::new(tokio::spawn(async move { work })) }

fn worker(work: u64) -> Worker { Worker { task: tokio::spawn(async move { work }), guard: guarded(work) } }

fn statement() { tokio::spawn(async {}); }

fn on_handle(handle: &Handle) { handle.spawn(async {}); }

fn unread() { let _worker = thread::spawn(|| 1); }

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

fn into_set(set: &mut JoinSet<u64>) { set.spawn(async { 1 }); }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() -> Result<(), JoinError> {
    let tracker = TaskTracker::new();
    let mut set = JoinSet::new();
    statement();
    on_handle(&Handle::current());
    unread();
    through_wrapper();
    dropped();
    bound_then_dropped();
    abort_handle();
    finished();
    returned_abort().abort();
    kept_abort().await?;
    tracked(&tracker);
    into_set(&mut set);
    set.release(start(2));
    let built = worker(3);
    drop(Releaser);
    tracker.close();
    tracker.wait().await;
    start(4).await?;
    built.task.await?;
    built.guard.await?;
    set.join_all().await;
    Ok(())
}
