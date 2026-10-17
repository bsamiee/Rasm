use core::{future, hint::black_box};
use std::io;

use futures::StreamExt as _;
use tokio::{
    io::{AsyncBufReadExt as _, BufReader},
    sync::mpsc,
    task,
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

async fn segments(text: &[u8]) -> Vec<io::Result<Vec<u8>>> {
    let (mut lines, mut read) = (BufReader::new(text).split(b'\n'), Vec::new());
    while let Some(line) = lines.next_segment().await.transpose() {
        read.push(line);
    }
    read
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
        let mut doubled = value;
        task::yield_now().await;
        doubled = doubled.saturating_mul(2);
        black_box(doubled);
    }
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

async fn borrowed(received: &mut mpsc::Receiver<u32>) -> u32 {
    let mut total = 0_u32;
    while let Some(value) = received.recv().await {
        total = total.saturating_add(value);
    }
    total
}

async fn reused(mut received: mpsc::Receiver<u32>) -> mpsc::Receiver<u32> {
    while let Some(value) = received.recv().await {
        black_box(value);
    }
    received
}

async fn drained(mut received: mpsc::Receiver<u32>) -> usize {
    let mut seen = 0_usize;
    while let Some(value) = received.recv().await {
        seen = seen.saturating_add(received.len());
        black_box(value);
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

async fn collected(mut received: mpsc::Receiver<u32>) -> Vec<u32> {
    let mut values = Vec::new();
    while let Some(value) = received.recv().await {
        task::yield_now().await;
        values.push(value);
    }
    values
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
    let channel = || mpsc::channel(4).1;
    let mut inbox = Inbox { received: channel() };
    relayed(mpsc::unbounded_channel().1).await;
    black_box(&(
        summed(channel()).await,
        segments(b"one\ntwo").await,
        measured(b"one\ntwo").await,
        nested(channel()).await,
        borrowed(&mut channel()).await,
        reused(channel()).await,
        drained(channel()).await,
        stopped(channel()).await,
        found(channel()).await,
        collected(channel()).await,
        inbox.emptied().await,
        streamed(channel()).await,
    ));
}
