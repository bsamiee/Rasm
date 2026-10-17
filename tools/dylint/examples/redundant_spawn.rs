use core::hint::black_box;

use tokio::{
    runtime::Handle,
    sync::oneshot,
    task::{JoinError, JoinHandle},
};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn rewrapped_direct(task: JoinHandle<u8>) -> JoinHandle<Result<u8, JoinError>> { tokio::spawn(task) }

fn rewrapped_tried(task: JoinHandle<Result<u8, JoinError>>) -> JoinHandle<Result<u8, JoinError>> { tokio::spawn(async move { task.await? }) }

fn rewrapped_on(task: JoinHandle<u8>, handle: &Handle) -> JoinHandle<Result<u16, JoinError>> { handle.spawn(async move { task.await.map(u16::from) }) }

fn computed() -> JoinHandle<u8> { tokio::spawn(async { 0_u8 }) }

fn traced(task: JoinHandle<u8>) -> JoinHandle<Result<u8, JoinError>> {
    tokio::spawn(async move {
        let value = task.await?;
        Ok(value.strict_add(1))
    })
}

fn joined(first: JoinHandle<u8>, second: JoinHandle<u8>) -> JoinHandle<Result<(u8, u8), JoinError>> { tokio::spawn(async move { Ok((first.await?, second.await?)) }) }

fn forwarded(received: oneshot::Receiver<u8>) -> JoinHandle<Result<u8, oneshot::error::RecvError>> { tokio::spawn(async move { received.await }) }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() -> Result<(), JoinError> {
    let handle = Handle::current();
    let (sender, received) = oneshot::channel();
    drop(sender);
    traced(computed()).await??;
    rewrapped_direct(computed()).await??;
    rewrapped_on(computed(), &handle).await??;
    rewrapped_tried(tokio::spawn(async { Ok(1_u8) })).await??;
    joined(computed(), computed()).await??;
    black_box(&forwarded(received).await?);
    Ok(())
}
