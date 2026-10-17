use core::hint::black_box;

use tokio::task::{AbortHandle, JoinHandle};
use tokio_util::sync::CancellationToken;

// --- [MODELS] --------------------------------------------------------------------------

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
        AbortHandle::abort(&self.abort);
        self.token.cancel();
    }
}

struct Shared {
    token: CancellationToken,
}

impl Drop for Shared {
    fn drop(&mut self) {
        let local = (CancellationToken::new(),);
        local.0.cancel();
        black_box(&self.token);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let watched = tokio::spawn(async { 6_i32 });
    let service = Service { task: tokio::spawn(async { 5 }), abort: watched.abort_handle(), token: CancellationToken::new() };
    service.stop();
    drop(service);
    drop(Shared { token: CancellationToken::new() });
    black_box(&watched.await);
}
