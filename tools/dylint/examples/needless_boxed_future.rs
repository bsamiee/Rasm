use core::{
    future::{self, Ready},
    hint::black_box,
    pin::Pin,
};

// --- [TYPES] ---------------------------------------------------------------------------

type Boxed = Pin<Box<dyn Future<Output = u64> + Send>>;

trait Source {
    fn fetch(&self) -> Pin<Box<dyn Future<Output = u64> + '_>>;
}

trait Relay {
    fn relay(&self) -> Pin<Box<dyn Future<Output = u64> + Send + '_>>;
}

trait Sink {
    fn drain(&self) -> Pin<Box<dyn Future<Output = u64> + '_>>;
}

// --- [MODELS] --------------------------------------------------------------------------

struct Counter {
    start: u64,
}

struct Immediate;

struct Echo;

impl Counter {
    fn next(&self) -> Pin<Box<dyn Future<Output = u64> + '_>> { Box::pin(async move { self.start.strict_add(1) }) }

    fn countdown(&self, steps: u64) -> Pin<Box<dyn Future<Output = u64> + '_>> {
        Box::pin(async move {
            match steps.checked_sub(1) {
                None => self.start,
                Some(rest) => self.countdown(rest).await,
            }
        })
    }
}

impl Source for Counter {
    fn fetch(&self) -> Pin<Box<dyn Future<Output = u64> + '_>> { Box::pin(async move { self.start }) }
}

impl Relay for Counter {
    fn relay(&self) -> Pin<Box<dyn Future<Output = u64> + Send + '_>> { Box::pin(async move { self.start }) }
}

impl Sink for Counter {
    fn drain(&self) -> Pin<Box<dyn Future<Output = u64> + '_>> { Box::pin(async move { self.start }) }
}

impl Source for u64 {
    fn fetch(&self) -> Pin<Box<dyn Future<Output = u64> + '_>> { Box::pin(async move { *self }) }
}

impl Relay for u64 {
    fn relay(&self) -> Pin<Box<dyn Future<Output = u64> + Send + '_>> { Box::pin(async move { *self }) }
}

impl Sink for u64 {
    fn drain(&self) -> Pin<Box<dyn Future<Output = u64> + '_>> { Box::pin(async move { *self }) }
}

impl IntoFuture for Immediate {
    type IntoFuture = Ready<u64>;
    type Output = u64;

    fn into_future(self) -> Self::IntoFuture { future::ready(9) }
}

impl IntoFuture for Echo {
    type IntoFuture = Boxed;
    type Output = u64;

    fn into_future(self) -> Self::IntoFuture { Box::pin(async { 8 }) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn boxed(work: u64) -> Boxed { Box::pin(async move { work }) }

fn depth(level: u64) -> Pin<Box<dyn Future<Output = u64>>> {
    Box::pin(async move {
        match level.checked_sub(1) {
            None => 0,
            Some(rest) => depth(rest).await.strict_add(1),
        }
    })
}

fn chosen(fast: bool) -> Pin<Box<dyn Future<Output = u64>>> { if fast { Box::pin(async { 1 }) } else { Box::pin(future::ready(2)) } }

fn stored(work: u64) -> Boxed { Box::pin(async move { work.strict_mul(2) }) }

async fn handled(handler: fn(u64) -> Boxed) -> u64 { handler(5).await }

async fn descend(level: u64) -> u64 {
    match level.checked_sub(1) {
        None => 0,
        Some(rest) => {
            let below: u64 = Box::pin(descend(rest)).await;
            below.strict_add(1)
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let counter = Counter { start: 1 };
    let sinks: [&dyn Sink; 2] = [&counter, &2];
    black_box([
        counter.next().await,
        counter.countdown(3).await,
        counter.fetch().await,
        3.fetch().await,
        counter.relay().await,
        4.relay().await,
        sinks[0].drain().await,
        sinks[1].drain().await,
        Immediate.await,
        Echo.await,
        boxed(1).await,
        depth(2).await,
        chosen(true).await,
        handled(stored).await,
        descend(2).await,
    ]);
}
