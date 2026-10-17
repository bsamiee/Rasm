use core::{hint::black_box, time::Duration};

use tokio::{
    sync::oneshot,
    task::{self, JoinError, JoinHandle},
    time,
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Pending {
    reply: oneshot::Receiver<u64>,
}

#[derive(Debug)]
struct Connection {
    served: u64,
}

impl Connection {
    async fn query(&mut self, id: u64) -> u64 {
        task::yield_now().await;
        self.served = self.served.strict_add(id);
        self.served
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn received(receivers: Vec<oneshot::Receiver<u64>>) -> Result<u64, oneshot::error::RecvError> {
    let mut total = 0;
    for receiver in receivers {
        total = receiver.await?.strict_add(total);
    }
    Ok(total)
}

async fn replied(requests: Vec<Pending>) -> Result<u64, oneshot::error::RecvError> {
    let mut total = 0;
    for pending in requests {
        total = pending.reply.await?.strict_add(total);
    }
    Ok(total)
}

async fn bounded(receivers: Vec<oneshot::Receiver<u64>>) -> Vec<Result<Result<u64, oneshot::error::RecvError>, time::error::Elapsed>> {
    let mut replies = Vec::new();
    for receiver in receivers {
        replies.push(time::timeout(Duration::ZERO, receiver).await);
    }
    replies
}

async fn fetched(id: u64) -> u64 {
    task::yield_now().await;
    id
}

async fn per_id(ids: Vec<u64>) -> Vec<u64> {
    let mut out = Vec::new();
    for id in ids {
        out.push(fetched(id).await);
    }
    out
}

async fn owned(handles: Vec<JoinHandle<u64>>) -> Result<u64, JoinError> {
    let mut total = 0;
    for handle in handles {
        total = handle.await?.strict_add(total);
    }
    Ok(total)
}

async fn queried(connection: &mut Connection, ids: Vec<u64>) -> u64 {
    let mut total = 0;
    for id in ids {
        total = connection.query(id).await.strict_add(total);
    }
    total
}

async fn first_ready(ids: Vec<u64>) -> Option<u64> {
    for id in ids {
        if fetched(id).await > 1 {
            return Some(id);
        }
    }
    None
}

async fn yielded(handles: &[JoinHandle<u64>]) -> usize {
    let mut finished = 0;
    for handle in handles {
        task::yield_now().await;
        finished = usize::from(handle.is_finished()).strict_add(finished);
    }
    finished
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let spawned = |count: u64| (0..count).map(|work| tokio::spawn(async move { work })).collect::<Vec<_>>();
    let receivers = || vec![oneshot::channel().1];
    let mut connection = Connection { served: 0 };
    let served = connection.query(3).await;
    black_box(&(
        served,
        received(receivers()).await,
        replied(vec![Pending { reply: oneshot::channel().1 }]).await,
        bounded(receivers()).await,
        per_id(vec![1, 2]).await,
        owned(spawned(2)).await,
        queried(&mut connection, vec![1, 2]).await,
        first_ready(vec![1, 2]).await,
        yielded(&spawned(2)).await,
    ));
}
