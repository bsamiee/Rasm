use core::future;
use core::pin::Pin;
use core::time::Duration;
use std::error::Report;
use std::io;

use bytes::Bytes;
use futures::future::OptionFuture;
use futures::{SinkExt, StreamExt as _};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use serde_json::value::RawValue;
use tokio::io::{AsyncRead, AsyncWrite};
use tokio::time::Instant;
use tokio_util::codec::{BytesCodec, FramedRead, FramedWrite};
use tokio_util::sync::CancellationToken;

use super::jsonrpc::{Line, Lines};
use super::{Era, Link};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Default, derive_more::IsVariant)]
enum Probe {
    #[default]
    Unsent,
    Pending,
    Answered(Box<RawValue>),
    Refused(String),
}

#[derive(Debug)]
pub(super) enum Next {
    Link(Era, Box<RawValue>),
    Probe(Era),
    Halt(String),
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Default)]
pub(super) struct Negotiation {
    discover: Probe,
    initialize: Probe,
}

impl Negotiation {
    pub(super) fn sent(&mut self, era: Era) {
        match era {
            Era::Modern => self.discover = Probe::Pending,
            Era::Legacy => self.initialize = Probe::Pending,
        }
    }

    pub(super) fn answered(&mut self, era: Era, result: Option<&RawValue>, refused: Option<&RawValue>) -> Option<Next> {
        let answer = result.map_or_else(|| Probe::Refused(refused.map_or("no result", RawValue::get).to_owned()), |result| Probe::Answered(result.to_owned()));
        match era {
            Era::Modern => self.discover = answer,
            Era::Legacy => self.initialize = answer,
        }
        match (&self.discover, &self.initialize) {
            (Probe::Unsent | Probe::Pending | Probe::Answered(_) | Probe::Refused(_), Probe::Answered(handshake)) => Some(Next::Link(Era::Legacy, handshake.clone())),
            (Probe::Answered(handshake), Probe::Unsent | Probe::Refused(_)) => Some(Next::Link(Era::Modern, handshake.clone())),
            (Probe::Refused(_), Probe::Unsent) => Some(Next::Probe(Era::Legacy)),
            (Probe::Unsent, Probe::Refused(_)) => Some(Next::Probe(Era::Modern)),
            (Probe::Refused(discover), Probe::Refused(initialize)) => Some(Next::Halt(format!("server refused server/discover ({discover}) and initialize ({initialize})"))),
            (Probe::Unsent | Probe::Pending | Probe::Answered(_) | Probe::Refused(_), Probe::Pending) | (Probe::Pending, Probe::Unsent | Probe::Refused(_)) | (Probe::Unsent, Probe::Unsent) => None,
        }
    }

    pub(super) const fn unasked(&self) -> bool { self.initialize.is_unsent() }

    pub(super) const fn silent(&self) -> bool { self.discover.is_pending() && self.initialize.is_unsent() }
}

#[derive(Debug, derive_more::IsVariant)]
enum Stage {
    Handshake(Negotiation),
    Ready,
    Ending,
}

#[derive(derive_more::Debug)]
pub(super) struct Live<R, W, C> {
    #[debug(skip)]
    lines: FramedRead<R, Lines>,
    #[debug(skip)]
    writer: FramedWrite<W, BytesCodec>,
    #[debug(skip)]
    closed: Pin<Box<C>>,
    stop: CancellationToken,
    started: Instant,
    stage: Stage,
}

#[derive(derive_more::Debug)]
pub(super) enum Wake<R, W, C> {
    Line(Option<io::Result<Line>>),
    Failed(io::Error),
    Closed(String),
    Connected(#[debug(skip)] io::Result<Link<R, W, C>>, CancellationToken, Instant),
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(derive_more::Debug, derive_more::IsVariant)]
pub(super) enum Upstream<A, R, W, C> {
    Down {
        retry: Instant,
    },
    Connecting {
        #[debug(skip)]
        attempt: Pin<Box<A>>,
        stop: CancellationToken,
        started: Instant,
    },
    Up(Box<Live<R, W, C>>),
}

impl<A, R, W, C> Upstream<A, R, W, C>
where
    A: Future<Output = io::Result<Link<R, W, C>>>,
    R: AsyncRead + Unpin,
    W: AsyncWrite + Unpin,
    C: Future<Output = String>,
{
    // --- [LIFECYCLE]

    pub(super) fn down(delay: Duration) -> Self { Self::Down { retry: Instant::now().checked_add(delay).unwrap_or_else(Instant::now) } }

    pub(super) fn link(link: Link<R, W, C>, limit: usize, stop: CancellationToken, started: Instant) -> Self {
        let Link { reader, writer, closed } = link;
        let (lines, writer) = (FramedRead::new(reader, Lines::new(limit)), FramedWrite::new(writer, BytesCodec::new()));
        Self::Up(Box::new(Live { lines, writer, closed: Box::pin(closed), stop, started, stage: Stage::Handshake(Negotiation::default()) }))
    }

    pub(super) async fn next(&mut self) -> Wake<R, W, C> {
        match self {
            Self::Down { retry: _ } => future::pending().await,
            Self::Connecting { attempt, stop, started } => {
                let wake = Wake::Connected(attempt.as_mut().await, stop.clone(), *started);
                *self = Self::down(Duration::ZERO);
                wake
            }
            Self::Up(open) => loop {
                let Live { lines, writer, closed, stop: _, started: _, stage } = &mut **open;
                let reading = !stage.is_ending();
                let unsent = reading && !writer.write_buffer().is_empty();
                tokio::select! {
                    biased;
                    Some(flushed) = OptionFuture::from(unsent.then(|| SinkExt::<Bytes>::flush(writer))) => if let Err(error) = flushed {
                        break Wake::Failed(error);
                    },
                    Some(line) = OptionFuture::from(reading.then(|| lines.next())) => break Wake::Line(line),
                    cause = closed => break Wake::Closed(cause),
                }
            },
        }
    }

    pub(super) fn end(&mut self, error: Option<&io::Error>) {
        if let Some(error) = error {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "server link failed");
        }
        match self {
            Self::Up(open) => {
                open.writer.write_buffer_mut().clear();
                open.stop.cancel();
                open.stage = Stage::Ending;
            }
            Self::Down { retry: _ } | Self::Connecting { attempt: _, stop: _, started: _ } => {}
        }
    }

    pub(super) async fn close(self) -> Option<String> {
        match self {
            Self::Up(open) => {
                let Live { lines, writer, closed, stop, started: _, stage: _ } = *open;
                stop.cancel();
                drop((lines, writer));
                Some(closed.await)
            }
            Self::Connecting { attempt: _, stop, started: _ } => {
                stop.cancel();
                None
            }
            Self::Down { retry: _ } => None,
        }
    }

    // --- [STAGES]

    fn writable(&mut self) -> Option<&mut Live<R, W, C>> {
        match self {
            Self::Up(open) if !open.stage.is_ending() => Some(&mut **open),
            Self::Up(_) | Self::Down { retry: _ } | Self::Connecting { attempt: _, stop: _, started: _ } => None,
        }
    }

    pub(super) fn ready(&self) -> bool { matches!(self, Self::Up(open) if open.stage.is_ready()) }

    pub(super) fn room(&self) -> bool { !matches!(self, Self::Up(open) if open.writer.write_buffer().len() >= open.writer.backpressure_boundary()) }

    pub(super) fn negotiation(&mut self) -> Option<&mut Negotiation> {
        match self.writable().map(|open| &mut open.stage) {
            Some(Stage::Handshake(negotiation)) => Some(negotiation),
            Some(Stage::Ready | Stage::Ending) | None => None,
        }
    }

    pub(super) fn linked(&mut self) -> Option<Duration> {
        let open = self.writable().filter(|open| open.stage.is_handshake())?;
        open.stage = Stage::Ready;
        Some(open.started.elapsed())
    }

    // --- [WRITES]

    pub(super) fn send(&mut self, line: Result<Bytes, serde_json::Error>) {
        match (line, self.writable()) {
            (Ok(line), Some(open)) => open.writer.write_buffer_mut().extend_from_slice(&line),
            (Ok(_), None) => {}
            (Err(error), _) => tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "message not encoded"),
        }
    }

    pub(super) fn forward(&mut self, chunks: impl IntoIterator<Item = Bytes>) {
        if let Some(open) = self.writable().filter(|open| open.stage.is_ready()) {
            chunks.into_iter().for_each(|chunk| open.writer.write_buffer_mut().extend_from_slice(&chunk));
        }
    }
}
