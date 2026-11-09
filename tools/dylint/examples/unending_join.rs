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

struct Holder {
    sender: watch::Sender<u32>,
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

async fn dropped_after() -> usize {
    let (sender, received) = broadcast::channel::<u32>(4);
    let (seen, ()) = future::join(BroadcastStream::new(received).count(), task::yield_now()).await;
    drop(sender);
    seen
}

async fn dropped_before() -> usize {
    let (sender, received) = broadcast::channel::<u32>(4);
    drop(sender);
    let (seen, ()) = future::join(BroadcastStream::new(received).count(), task::yield_now()).await;
    seen
}

async fn callback_after_drop() -> usize {
    let (sender, received) = broadcast::channel::<u32>(4);
    drop(sender);
    black_box(&(|| 1_u8));
    let (seen, ()) = future::join(BroadcastStream::new(received).count(), task::yield_now()).await;
    seen
}

async fn borrowed_sender() -> usize {
    let (sender, received) = broadcast::channel::<u32>(4);
    let (seen, sent) = future::join(BroadcastStream::new(received).count(), async { sender.send(1) }).await;
    black_box(&sent);
    seen
}

async fn macro_handed(total: u32) -> usize {
    let (sender, received) = broadcast::channel(4);
    let (seen, sent) = tokio::join!(BroadcastStream::new(received).count(), async move { sender.send(total) });
    black_box(&sent);
    seen
}

async fn nested_sender() -> usize {
    let (sender, received) = broadcast::channel::<u32>(4);
    async move {
        let (seen, ()) = future::join(BroadcastStream::new(received).count(), task::yield_now()).await;
        black_box(sender);
        seen
    }
    .await
}

async fn nested_handed(total: u32) -> usize {
    let (sender, received) = broadcast::channel(4);
    async move {
        let (seen, sent) = future::join(BroadcastStream::new(received).count(), async move { sender.send(total) }).await;
        black_box(&sent);
        seen
    }
    .await
}

async fn holder_handed(holder: Holder) -> usize {
    let received = holder.sender.subscribe();
    let finish = async move {
        drop(holder);
    };
    let (seen, ()) = future::join(WatchStream::new(received).count(), finish).await;
    seen
}

async fn copied_holder(sender: &watch::Sender<u32>) -> usize {
    #[derive(Clone, Copy)]
    struct Borrowed<'a> {
        sender: &'a watch::Sender<u32>,
    }

    let holder = Borrowed { sender };
    let received = holder.sender.subscribe();
    let (seen, ()) = future::join(WatchStream::new(received).count(), async move {
        black_box(holder);
    })
    .await;
    seen
}

async fn dropped_both(flag: bool) -> usize {
    let (sender, received) = broadcast::channel::<u32>(4);
    if flag {
        drop(sender);
    } else {
        drop(sender);
    }
    future::join(BroadcastStream::new(received).count(), task::yield_now()).await.0
}

async fn replaced_sender() -> usize {
    let (mut sender, received) = broadcast::channel::<u32>(4);
    black_box(&sender);
    sender = broadcast::channel(4).0;
    black_box(&sender);
    future::join(BroadcastStream::new(received).count(), task::yield_now()).await.0
}

async fn local_reference() -> usize {
    let sender = watch::Sender::new(0_u32);
    let reference = &sender;
    let received = reference.subscribe();
    drop(sender);
    future::join(WatchStream::new(received).count(), task::yield_now()).await.0
}

async fn owned_arc_sender() -> usize {
    let sender = std::sync::Arc::new(watch::Sender::new(0_u32));
    let received = sender.subscribe();
    future::join(WatchStream::new(received).count(), task::yield_now()).await.0
}

async fn mutable_sender(sender: &mut watch::Sender<u32>) -> usize {
    let received = sender.subscribe();
    future::join(WatchStream::new(received).count(), async {
        *sender = watch::Sender::new(1);
    })
    .await
    .0
}

fn deferred() -> impl Future<Output = usize> {
    let (sender, received) = broadcast::channel::<u32>(4);
    let joined = future::join(BroadcastStream::new(received).count(), task::yield_now());
    drop(sender);
    async move { joined.await.0 }
}

async fn stopped_on_error() {
    let sender = watch::Sender::new(0_u32);
    let drained = WatchStream::new(sender.subscribe()).map(Ok::<_, u8>).try_for_each(|_| ready(Err::<(), _>(1)));
    black_box(&future::join(drained, ready(())).await);
}

fn closed_stream() -> WatchStream<u32> {
    let (sender, received) = watch::channel(0);
    drop(sender);
    WatchStream::new(received)
}

fn discarded_stream(stream: WatchStream<u32>) -> WatchStream<u32> {
    drop(stream);
    closed_stream()
}

async fn disconnected_stream() -> usize {
    let sender = watch::Sender::new(0_u32);
    let updates = discarded_stream(WatchStream::new(sender.subscribe()));
    future::join(updates.count(), task::yield_now()).await.0
}

async fn dormant_drain() {
    let sender = watch::Sender::new(0_u32);
    let updates = WatchStream::new(sender.subscribe());
    future::join(
        async {
            drop(updates.count());
        },
        task::yield_now(),
    )
    .await;
}

async fn replaced_stream() -> usize {
    let sender = watch::Sender::new(0_u32);
    let mut updates = WatchStream::new(sender.subscribe());
    black_box(&updates);
    updates = closed_stream();
    future::join(updates.count(), task::yield_now()).await.0
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let progress = Progress::default();
    black_box(&(progress.joined(ready(1_u8)).await, progress.relayed(Some(2), ready(3_u8)).await, progress.raced(ready(4_u8)).await, progress.sampled().await));
    black_box(&(tallied().await, handed(5).await));
    black_box(&(dropped_after().await, dropped_before().await, callback_after_drop().await, borrowed_sender().await, macro_handed(6).await, nested_sender().await, nested_handed(7).await));
    black_box(&(
        holder_handed(Holder { sender: watch::Sender::new(0) }).await,
        copied_holder(&watch::Sender::new(0)).await,
        dropped_both(true).await,
        replaced_sender().await,
        local_reference().await,
    ));
    black_box(&(
        owned_arc_sender().await,
        mutable_sender(&mut watch::Sender::new(0)).await,
        deferred().await,
        stopped_on_error().await,
        disconnected_stream().await,
        dormant_drain().await,
        replaced_stream().await,
    ));
}
