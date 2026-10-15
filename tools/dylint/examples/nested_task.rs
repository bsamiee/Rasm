use tokio::{
    runtime::Handle,
    task::{JoinError, JoinHandle},
};

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [REPORTED]

fn rewrapped_direct(task: JoinHandle<u8>) -> JoinHandle<Result<u8, JoinError>> { tokio::spawn(task) }

fn rewrapped_on(task: JoinHandle<u8>, handle: &Handle) -> JoinHandle<Result<u16, JoinError>> { handle.spawn(async move { task.await.map(u16::from) }) }

// --- [ACCEPTED]

fn computed() -> JoinHandle<u8> { tokio::spawn(async { 0_u8 }) }

fn traced(task: JoinHandle<u8>) -> JoinHandle<u8> {
    tokio::spawn(async move {
        let value = task.await.unwrap_or_default();
        value.strict_add(1)
    })
}

fn joined(first: JoinHandle<u8>, second: JoinHandle<u8>) -> JoinHandle<Result<(u8, u8), JoinError>> { tokio::spawn(async move { Ok((first.await?, second.await?)) }) }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() -> Result<(), JoinError> {
    let handle = Handle::current();
    traced(computed()).await?;
    rewrapped_direct(computed()).await??;
    rewrapped_on(computed(), &handle).await??;
    joined(computed(), computed()).await??;
    Ok(())
}
