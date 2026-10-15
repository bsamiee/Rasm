use core::{
    future::{self, Ready},
    hint::black_box,
    pin::Pin,
};

use futures::future::join_all;

// --- [TYPES] ---------------------------------------------------------------------------

type Boxed<'work> = Pin<Box<dyn Future<Output = u64> + Send + 'work>>;

trait Source {
    fn fetch(&self) -> Pin<Box<dyn Future<Output = u64> + '_>>;
}

trait Sink {
    fn drain(&self) -> Pin<Box<dyn Future<Output = u64> + '_>>;
}

trait Relay {
    fn relay(&self) -> Pin<Box<dyn Future<Output = u64> + Send + '_>>;
}

// --- [MODELS] --------------------------------------------------------------------------

struct Counter {
    start: u64,
}

struct Immediate;

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

impl IntoFuture for Immediate {
    type IntoFuture = Ready<u64>;
    type Output = u64;

    fn into_future(self) -> Self::IntoFuture { future::ready(9) }
}

impl Sink for Counter {
    fn drain(&self) -> Pin<Box<dyn Future<Output = u64> + '_>> { Box::pin(async move { self.start }) }
}

impl Sink for u64 {
    fn drain(&self) -> Pin<Box<dyn Future<Output = u64> + '_>> { Box::pin(async move { *self }) }
}

impl Relay for Counter {
    fn relay(&self) -> Pin<Box<dyn Future<Output = u64> + Send + '_>> { Box::pin(async move { self.start }) }
}

impl Relay for u64 {
    fn relay(&self) -> Pin<Box<dyn Future<Output = u64> + Send + '_>> { Box::pin(async move { *self }) }
}

impl Source for u64 {
    fn fetch(&self) -> Pin<Box<dyn Future<Output = u64> + '_>> { Box::pin(async move { *self }) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn boxed(work: u64) -> Pin<Box<dyn Future<Output = u64> + Send>> { Box::pin(async move { work }) }

fn aliased(work: &u64) -> Boxed<'_> { Box::pin(async move { *work }) }

fn depth(level: u64) -> Pin<Box<dyn Future<Output = u64>>> {
    Box::pin(async move {
        match level.checked_sub(1) {
            None => 0,
            Some(rest) => depth(rest).await.strict_add(1),
        }
    })
}

fn chosen(fast: bool) -> Pin<Box<dyn Future<Output = u64>>> { if fast { Box::pin(async { 1 }) } else { Box::pin(future::ready(2)) } }

fn stored(work: u64) -> Boxed<'static> { Box::pin(async move { work.strict_mul(2) }) }

async fn handled(handler: fn(u64) -> Boxed<'static>) -> u64 { handler(5).await }

async fn direct(work: u64) -> u64 { boxed(work).await }

async fn descend(level: u64) -> u64 {
    match level.checked_sub(1) {
        None => 0,
        Some(rest) => Box::pin(descend(rest)).await.strict_add(1),
    }
}

pub fn exported(work: u64) -> Pin<Box<dyn Future<Output = u64> + Send>> { Box::pin(async move { work.strict_add(1) }) }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let counter = Counter { start: 1 };
    let sinks: [&dyn Sink; 2] = [&counter, &10];
    black_box((
        [boxed(1).await, aliased(&2).await, counter.next().await, counter.countdown(3).await, counter.fetch().await, 4.fetch().await],
        [depth(2).await, handled(stored).await, direct(6).await, join_all(sinks.map(Sink::drain)).await.into_iter().sum(), descend(2).await, exported(7).await],
        [Immediate.await, chosen(true).await, counter.relay().await, 5.relay().await],
    ));
}
