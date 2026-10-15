use tokio::{
    sync::oneshot,
    task::{self, JoinError, JoinHandle, JoinSet},
};
use tokio_util::task::AbortOnDropHandle;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Task {
    name: &'static str,
    handle: JoinHandle<u64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn spawned(count: u64) -> Vec<JoinHandle<u64>> { (0..count).map(|work| tokio::spawn(async move { work })).collect() }

async fn owned(handles: Vec<JoinHandle<u64>>) -> Result<u64, JoinError> {
    let mut total = 0;
    for handle in handles {
        total = handle.await?.strict_add(total);
    }
    Ok(total)
}

async fn borrowed(mut handles: Vec<JoinHandle<u64>>) -> Result<u64, JoinError> {
    let mut total = 0;
    for handle in &mut handles {
        total = handle.await?.strict_add(total);
    }
    Ok(total)
}

async fn named(handles: Vec<(&str, JoinHandle<u64>)>) -> Result<Vec<(&str, u64)>, JoinError> {
    let mut results = Vec::new();
    for (name, handle) in handles {
        results.push((name, handle.await?));
    }
    Ok(results)
}

async fn reversed(handles: Vec<JoinHandle<u64>>) -> Result<u64, JoinError> {
    let mut total = 0;
    for handle in handles.into_iter().rev() {
        total = handle.await?.strict_add(total);
    }
    Ok(total)
}

async fn fielded(tasks: Vec<Task>) -> Result<Vec<(&'static str, u64)>, JoinError> {
    let mut results = Vec::new();
    for task in tasks {
        results.push((task.name, task.handle.await?));
    }
    Ok(results)
}

async fn guarded(handles: Vec<JoinHandle<u64>>) -> Result<u64, JoinError> {
    let mut total = 0;
    for handle in handles {
        total = AbortOnDropHandle::new(handle).await?.strict_add(total);
    }
    Ok(total)
}

async fn received(receivers: Vec<oneshot::Receiver<u64>>) -> Result<u64, oneshot::error::RecvError> {
    let mut total = 0;
    for receiver in receivers {
        total = receiver.await?.strict_add(total);
    }
    Ok(total)
}

async fn fetched(id: u64) -> Result<u64, JoinError> {
    task::yield_now().await;
    Ok(id)
}

async fn pending(ids: Vec<u64>) -> Result<Vec<u64>, JoinError> {
    let replies = ids.into_iter().map(fetched).collect::<Vec<_>>();
    let mut out = Vec::new();
    for reply in replies {
        out.push(reply.await?);
    }
    Ok(out)
}

async fn per_id(ids: Vec<u64>) -> Result<Vec<u64>, JoinError> {
    let mut out = Vec::new();
    for id in ids {
        out.push(fetched(id).await?);
    }
    Ok(out)
}

async fn yielded(handles: &[JoinHandle<u64>]) -> usize {
    let mut finished = 0;
    for handle in handles {
        task::yield_now().await;
        finished = usize::from(handle.is_finished()).strict_add(finished);
    }
    finished
}

async fn aborted(handles: Vec<JoinHandle<u64>>) -> usize {
    for handle in &handles {
        handle.abort();
    }
    let mut set = handles.into_iter().collect::<JoinSet<_>>();
    let mut cancelled = 0;
    while let Some(joined) = set.join_next().await {
        cancelled = usize::from(joined.is_ok_and(|result| result.is_err())).strict_add(cancelled);
    }
    cancelled
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() -> Result<(), JoinError> {
    let (sender, receiver) = oneshot::channel();
    sender.send(1).unwrap_or_default();
    owned(spawned(2)).await?;
    borrowed(spawned(2)).await?;
    named(vec![("first", tokio::spawn(async { 1 }))]).await?;
    reversed(spawned(2)).await?;
    fielded(vec![Task { name: "first", handle: tokio::spawn(async { 1 }) }]).await?;
    guarded(spawned(2)).await?;
    received(vec![receiver]).await.unwrap_or_default();
    pending(vec![1, 2]).await?;
    per_id(vec![1, 2]).await?;
    yielded(&spawned(2)).await;
    aborted(spawned(2)).await;
    Ok(())
}
