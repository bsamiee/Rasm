use core::{convert::Infallible, future::ready, hint::black_box, pin::pin};

use futures::{StreamExt as _, TryStreamExt as _, future, stream};
use tokio::{
    sync::{broadcast, watch},
    task,
};
use tokio_stream::wrappers::{BroadcastStream, WatchStream};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Default)]
struct Progress(watch::Sender<u32>);

impl Progress {
    async fn joined<F>(&self, call: F) -> F::Output
    where F: Future {
        let updates = WatchStream::new(self.0.subscribe());
        let (output, ()) = future::join(
            call,
            updates.for_each(|update| {
                black_box(update);
                ready(())
            }),
        )
        .await;
        output
    }

    async fn relayed<F>(&self, token: Option<u8>, call: F) -> F::Output
    where F: Future {
        let updates = WatchStream::from_changes(self.0.subscribe());
        let relayed = stream::iter(token.map(|token| updates.map(move |update| Ok::<_, Infallible>((token, update))))).flatten().try_for_each(|sent| {
            black_box(sent);
            ready(Ok(()))
        });
        let (output, Ok(())) = future::join(call, relayed).await;
        output
    }

    async fn raced<F>(&self, call: F) -> F::Output
    where F: Future {
        let updates = WatchStream::new(self.0.subscribe()).for_each(|update| {
            black_box(update);
            ready(())
        });
        match future::select(pin!(call), pin!(updates)).await {
            future::Either::Left((output, _)) => output,
            future::Either::Right(((), call)) => call.await,
        }
    }

    async fn sampled(&self) -> Vec<u32> {
        let (sampled, ()) = future::join(WatchStream::new(self.0.subscribe()).take(2).collect::<Vec<_>>(), task::yield_now()).await;
        sampled
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn tallied() -> usize {
    let (sender, received) = broadcast::channel::<u32>(4);
    let (seen, ()) = tokio::join!(BroadcastStream::new(received).count(), task::yield_now());
    black_box(&sender);
    seen
}

async fn handed(total: u32) -> usize {
    let (sender, received) = broadcast::channel(4);
    let (seen, sent) = future::join(BroadcastStream::new(received).count(), async move { sender.send(total) }).await;
    black_box(&sent);
    seen
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let progress = Progress::default();
    black_box(&(progress.joined(ready(1_u8)).await, progress.relayed(Some(2), ready(3_u8)).await, progress.raced(ready(4_u8)).await, progress.sampled().await));
    black_box(&(tallied().await, handed(5).await));
}
