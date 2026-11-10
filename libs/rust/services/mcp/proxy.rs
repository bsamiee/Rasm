use core::sync::atomic::{AtomicU64, AtomicUsize, Ordering};
use core::iter;
use core::time::Duration;
use std::collections::BTreeSet;
use std::error::Report;
use std::io;
use std::sync::Arc;

use backon::ExponentialBuilder;
use bytes::Bytes;
use futures::{StreamExt as _, TryFutureExt as _};
use jiff::{SignedDuration, Timestamp};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::model::{Implementation, InitializeRequestParams, ServerCapabilities, ServerJsonRpcMessage};
use serde::{Deserialize, Serialize};
use serde_json::value::RawValue;
use tokio::io::{AsyncRead, AsyncWrite, AsyncWriteExt as _, BufWriter};
use tokio::sync::{mpsc, oneshot, watch};
use tokio_util::codec::FramedRead;
use tokio_util::sync::CancellationToken;

use self::actor::{Actor, Event};
use self::jsonrpc::{Envelope, Line, Lines, Method, decode, encoded};
use self::routing::Calls;
use crate::shared::{list_changes, resynced};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Default, Serialize, Deserialize, derive_more::Display)]
#[display(rename_all = "lowercase")]
#[serde(rename_all = "lowercase")]
pub enum Phase {
    #[default]
    Starting,
    Up,
    Down,
    Idle,
    Stopped,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize, derive_more::Display, derive_more::IsVariant)]
#[display(rename_all = "lowercase")]
#[serde(rename_all = "lowercase")]
pub enum Era {
    Legacy,
    Modern,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy)]
pub struct Liveness {
    pub quiet: Duration,
    pub grace: Duration,
}

#[derive(Debug, Clone, Copy)]
pub struct Retention {
    pub ratio: u32,
    pub floor: Duration,
    pub ceiling: Duration,
}

#[derive(Debug, Clone)]
pub struct Policy {
    pub client: InitializeRequestParams,
    pub restart: ExponentialBuilder,
    pub handshake: Duration,
    pub probation: Duration,
    pub liveness: Option<Liveness>,
    pub retention: Retention,
    pub line: usize,
    pub queue: usize,
}

#[derive(Debug)]
pub struct Link<R, W, C> {
    pub reader: R,
    pub writer: W,
    pub closed: C,
}

#[derive(Debug, Clone, PartialEq, Eq, PartialOrd, Ord, Serialize, Deserialize)]
pub struct Key {
    pub method: Method,
    pub name: Option<Box<str>>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Catalog {
    pub era: Era,
    pub handshake: Box<RawValue>,
    pub cost: Duration,
    pub askers: BTreeSet<Key>,
    pub quiet: BTreeSet<Key>,
}

#[derive(Debug, Clone, Default, Serialize, Deserialize)]
pub struct History {
    pub catalog: Option<Catalog>,
    pub failures: usize,
    pub cause: Option<Arc<str>>,
    pub until: Option<Timestamp>,
}

#[derive(Debug, Clone, Default, Serialize)]
pub struct Status {
    pub phase: Phase,
    pub sessions: usize,
    pub starts: u32,
    pub failures: usize,
    pub delay: Duration,
    pub cause: Option<Arc<str>>,
    pub catalog: Option<Arc<Catalog>>,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Proxy {
    events: mpsc::Sender<Event>,
    status: watch::Receiver<Status>,
    in_flight: Arc<AtomicUsize>,
    joined: Arc<AtomicU64>,
    client: Implementation,
    line: usize,
    queue: usize,
}

impl Proxy {
    pub fn new<F, A, R, W, C>(policy: Policy, history: History, connect: F) -> (Self, impl Future<Output = ()> + Send)
    where
        F: FnMut(CancellationToken) -> A + Send,
        A: Future<Output = io::Result<Link<R, W, C>>> + Send,
        R: AsyncRead + Send + Unpin,
        W: AsyncWrite + Send + Unpin,
        C: Future<Output = String> + Send,
    {
        let (events, inbox) = mpsc::channel(policy.queue);
        let History { catalog, failures, cause, until } = history;
        let delay = until.map(|until| until.duration_since(Timestamp::now())).filter(SignedDuration::is_positive).map_or(Duration::ZERO, SignedDuration::unsigned_abs);
        let phase = if failures == 0 { Phase::Starting } else { Phase::Down };
        let (status, watched) = watch::channel(Status { phase, failures, delay, cause, catalog: catalog.map(Arc::new), ..Status::default() });
        let in_flight = Arc::new(AtomicUsize::new(0));
        let proxy =
            Self { events, status: watched, in_flight: Arc::clone(&in_flight), joined: Arc::new(AtomicU64::new(0)), client: policy.client.client_info.clone(), line: policy.line, queue: policy.queue };
        (proxy, Actor::new(policy, connect, inbox, status, Calls::new(in_flight)).run())
    }

    #[must_use]
    pub fn status(&self) -> watch::Receiver<Status> { self.status.clone() }

    #[must_use]
    pub fn in_flight(&self) -> usize { self.in_flight.load(Ordering::Relaxed) }

    pub async fn serve<R, W>(&self, reader: R, writer: W) -> io::Result<()>
    where
        R: AsyncRead + Send + Unpin + 'static,
        W: AsyncWrite + Send + Unpin + 'static,
    {
        let mut lines = FramedRead::new(reader, Lines::new(self.line));
        let Some(first) = lines.next().await.transpose()? else { return Ok(()) };
        let initializes = match &first {
            Line::Whole(line) => decode::<Envelope<'_>>(line).is_ok_and(|envelope| envelope.method == Some(Method::Initialize)),
            Line::Oversized { head: _, tail: _ } => false,
        };
        let stopped = |mpsc::error::SendError(_)| io::Error::from(io::ErrorKind::ConnectionAborted);
        let (era, early) = if initializes {
            let (asked, mut answered) = oneshot::channel();
            self.events.send(Event::Era(asked)).await.map_err(stopped)?;
            tokio::select! {
                biased;
                era = &mut answered => (era.map_err(io::Error::other)?, None),
                early = lines.next() => match early.transpose()? {
                    Some(early) => (answered.await.map_err(io::Error::other)?, Some(early)),
                    None => return Ok(()),
                },
            }
        } else {
            (None, None)
        };
        let (session, cut) = (self.joined.fetch_add(1, Ordering::Relaxed), CancellationToken::new());
        let (out, mut outbound) = mpsc::channel(self.queue);
        self.events.send(Event::Joined { session, out, cut: cut.clone() }).await.map_err(stopped)?;
        let leading = iter::once(first).chain(early);
        match (initializes, era) {
            (true, Some(Era::Modern)) => return Box::pin(forward::serve(self, session, leading, lines, writer, outbound, cut)).await,
            (true, Some(Era::Legacy) | None) | (false, Some(Era::Legacy | Era::Modern) | None) => {}
        }
        for line in leading {
            self.events.send(Event::Line { session, line }).await.map_err(stopped)?;
        }
        let (mut writer, mut batch) = (BufWriter::new(writer), Vec::new());
        loop {
            tokio::select! {
                biased;
                () = cut.cancelled() => break Ok(()),
                count = outbound.recv_many(&mut batch, self.queue) => {
                    if count == 0 {
                        break Ok(());
                    }
                    let written = async {
                        for chunk in &batch {
                            writer.write_all(chunk).await?;
                        }
                        if outbound.is_empty() { writer.flush().await } else { Ok(()) }
                    };
                    tokio::select! {
                        biased;
                        () = cut.cancelled() => break Ok(()),
                        written = written => if let Err(error) = written {
                            break Err(error);
                        },
                    }
                    batch.clear();
                }
                line = lines.next() => match line {
                    Some(Ok(line)) => if let Err(mpsc::error::SendError(_)) = self.events.send(Event::Line { session, line }).await {
                        break Ok(());
                    },
                    Some(Err(error)) => break Err(error),
                    None => break Ok(()),
                },
            }
        }
    }

    pub async fn drain(&self, settle: Duration) {
        let (drained, done) = oneshot::channel();
        let asked = self.events.send(Event::Drain { settle, drained }).map_err(|mpsc::error::SendError(_)| io::ErrorKind::ConnectionAborted.into());
        if let Err(error) = asked.and_then(|()| done.map_err(io::Error::other)).await {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server stopped before its calls drained");
        }
    }

    pub async fn stop(&self) {
        match self.events.send(Event::Stop).await {
            Ok(()) | Err(mpsc::error::SendError(_)) => self.events.closed().await,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn announced(handshake: &[u8]) -> Vec<Bytes> {
    #[derive(Deserialize)]
    struct Declared {
        capabilities: ServerCapabilities,
    }
    let changes = decode::<Declared>(handshake).map(|Declared { capabilities }| resynced(&list_changes(&capabilities), []).map(|change| encoded(&ServerJsonRpcMessage::notification(change))));
    match changes.map_err(serde_path_to_error::Error::into_inner).and_then(Iterator::collect) {
        Ok(lines) => lines,
        Err(error) => {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server capabilities unreadable");
            Vec::new()
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod actor;
mod forward;
pub(crate) mod jsonrpc;
pub(crate) mod pool;
mod routing;
pub(crate) mod session;
mod upstream;
