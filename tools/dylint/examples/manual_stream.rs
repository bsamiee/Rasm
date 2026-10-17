use core::future;
use std::io;

use futures::StreamExt as _;
use tokio::{
    io::{AsyncBufReadExt as _, BufReader},
    sync::mpsc,
    task::{self, JoinSet},
};
use tokio_stream::wrappers::ReceiverStream;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Inbox {
    received: mpsc::Receiver<u32>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn summed(mut received: mpsc::Receiver<u32>) -> u32 {
    let mut total = 0_u32;
    while let Some(value) = received.recv().await {
        total = total.saturating_add(value);
    }
    total
}

async fn logged(text: &[u8]) {
    let mut lines = BufReader::new(text).split(b'\n');
    while let Some(line) = lines.next_segment().await.inspect_err(|error| tracing::warn!(%error, "read failed")).ok().flatten() {
        tracing::info!(message = %String::from_utf8_lossy(&line));
    }
}

async fn measured(text: &[u8]) -> io::Result<usize> {
    let (mut lines, mut length) = (BufReader::new(text).lines(), 0_usize);
    while let Some(line) = lines.next_line().await? {
        length = length.saturating_add(line.len());
    }
    Ok(length)
}

async fn relayed(mut received: mpsc::UnboundedReceiver<u32>) {
    while let Some(value) = received.recv().await {
        task::yield_now().await;
        tracing::info!(value);
    }
}

async fn joined(mut set: JoinSet<u32>) -> usize {
    let mut finished = 0_usize;
    while let Some(outcome) = set.join_next().await {
        finished = finished.saturating_add(usize::from(outcome.is_ok()));
    }
    finished
}

async fn borrowed(received: &mut mpsc::Receiver<u32>) -> u32 {
    let mut total = 0_u32;
    while let Some(value) = received.recv().await {
        total = total.saturating_add(value);
    }
    total
}

async fn reused(mut received: mpsc::Receiver<u32>) -> mpsc::Receiver<u32> {
    while let Some(value) = received.recv().await {
        tracing::info!(value);
    }
    received
}

async fn drained(mut received: mpsc::Receiver<u32>) -> usize {
    let mut seen = 0_usize;
    while let Some(value) = received.recv().await {
        seen = seen.saturating_add(received.len()).saturating_add(usize::try_from(value).unwrap_or_default());
    }
    seen
}

async fn stopped(mut received: mpsc::Receiver<u32>) -> u32 {
    let mut total = 0_u32;
    while let Some(value) = received.recv().await {
        if value == 0 {
            break;
        }
        total = total.saturating_add(value);
    }
    total
}

async fn found(mut received: mpsc::Receiver<u32>) -> Option<u32> {
    while let Some(value) = received.recv().await {
        if value > 9 {
            return Some(value);
        }
    }
    None
}

async fn checked(mut received: mpsc::Receiver<u32>) -> Result<u32, core::num::TryFromIntError> {
    let mut total = 0_u8;
    while let Some(value) = received.recv().await {
        total = total.saturating_add(u8::try_from(value)?);
    }
    Ok(u32::from(total))
}

async fn collected(mut received: mpsc::Receiver<u32>) -> Vec<u32> {
    let mut values = Vec::new();
    while let Some(value) = received.recv().await {
        task::yield_now().await;
        values.push(value);
    }
    values
}

async fn nested(mut received: mpsc::Receiver<u32>) -> u32 {
    let mut total = 0_u32;
    while let Some(value) = received.recv().await {
        for step in 0..value {
            if step > 3 {
                break;
            }
            total = total.saturating_add(step);
        }
    }
    total
}

impl Inbox {
    async fn emptied(&mut self) -> u32 {
        let mut total = 0_u32;
        while let Some(value) = self.received.recv().await {
            total = total.saturating_add(value);
        }
        total
    }
}

async fn streamed(received: mpsc::Receiver<u32>) -> u32 { ReceiverStream::new(received).fold(0_u32, |total, value| future::ready(total.saturating_add(value))).await }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let channel = || {
        let (sender, received) = mpsc::channel(4);
        sender.try_send(1).unwrap_or_default();
        received
    };
    let (unbounded, relay) = mpsc::unbounded_channel();
    unbounded.send(2).unwrap_or_default();
    drop(unbounded);
    let mut set = JoinSet::new();
    set.spawn(async { 3 });
    let mut inbox = Inbox { received: channel() };
    logged(b"one\ntwo").await;
    relayed(relay).await;
    tracing::info!(
        summed = summed(channel()).await,
        measured = measured(b"one\ntwo").await.unwrap_or_default(),
        joined = joined(set).await,
        borrowed = borrowed(&mut channel()).await,
        reused = reused(channel()).await.len(),
        drained = drained(channel()).await,
        stopped = stopped(channel()).await,
        found = found(channel()).await,
        checked = checked(channel()).await.unwrap_or_default(),
        collected = collected(channel()).await.len(),
        nested = nested(channel()).await,
        emptied = inbox.emptied().await,
        streamed = streamed(channel()).await,
    );
}
