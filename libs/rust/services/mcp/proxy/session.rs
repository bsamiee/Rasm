use core::mem;
use std::collections::BTreeSet;
use std::error::Report;
use std::io;

use bytes::Bytes;
use futures::StreamExt as _;
use futures::future::Either;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::ErrorData;
use tokio::io::{AsyncRead, AsyncWrite, AsyncWriteExt as _, BufWriter};
use tokio_util::codec::FramedRead;

use super::announced;
use super::jsonrpc::{Envelope, Line, Lines, Method, decode, identity, refusal};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Ended {
    Client,
    Link { delivered: bool },
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(derive_more::Debug)]
pub struct Resumable<I, O> {
    #[debug(skip)]
    client: FramedRead<I, Lines>,
    #[debug(skip)]
    answers: BufWriter<O>,
    limit: usize,
    initialize: Option<(Bytes, Bytes)>,
    initialized: Option<Bytes>,
    changes: Vec<Bytes>,
    pending: BTreeSet<Bytes>,
    swallow: bool,
}

impl<I, O> Resumable<I, O>
where
    I: AsyncRead + Unpin,
    O: AsyncWrite + Unpin,
{
    pub fn new(input: I, output: O, limit: usize) -> Self {
        Self {
            client: FramedRead::new(input, Lines::new(limit)),
            answers: BufWriter::new(output),
            limit,
            initialize: None,
            initialized: None,
            changes: Vec::new(),
            pending: BTreeSet::new(),
            swallow: false,
        }
    }

    pub async fn over<R, W>(&mut self, reader: R, writer: W) -> io::Result<Ended>
    where
        R: AsyncRead + Unpin,
        W: AsyncWrite + Unpin,
    {
        let (mut replies, mut requests, mut delivered) = (FramedRead::new(reader, Lines::new(usize::MAX)), BufWriter::new(writer), false);
        self.swallow = self.initialize.as_ref().is_some_and(|(id, _)| !self.pending.contains(id));
        let replayed = [self.initialize.as_ref().map(|(_, line)| &**line), self.initialized.as_deref()].into_iter().flatten().collect::<Vec<_>>().concat();
        let mut sent = match requests.write_all(&replayed).await {
            Ok(()) => requests.flush().await,
            Err(error) => Err(error),
        };
        while sent.is_ok() {
            let woke = tokio::select! {
                line = replies.next() => Either::Left(line),
                line = self.client.next() => Either::Right(line),
            };
            sent = match woke {
                Either::Left(Some(Ok(Line::Whole(line)))) if self.received(&line) => {
                    delivered = true;
                    self.answers.write_all(&line).await?;
                    if replies.read_buffer().is_empty() {
                        self.answers.flush().await?;
                    }
                    Ok(())
                }
                Either::Left(Some(Ok(Line::Whole(_) | Line::Oversized { head: _, tail: _ }))) => Ok(()),
                Either::Left(Some(Err(error))) => Err(error),
                Either::Left(None) => Err(io::ErrorKind::UnexpectedEof.into()),
                Either::Right(Some(Ok(Line::Whole(line)))) => {
                    self.sent(&line);
                    match requests.write_all(&line).await {
                        Ok(()) if self.client.read_buffer().is_empty() => requests.flush().await,
                        written => written,
                    }
                }
                Either::Right(Some(Ok(Line::Oversized { head, tail }))) => {
                    let error = ErrorData::invalid_request(format!("request exceeds {} bytes, send less", self.limit), None);
                    self.refused(identity(&head, &tail).0.into_iter(), &error).await?;
                    Ok(())
                }
                Either::Right(Some(Err(error))) => return Err(error),
                Either::Right(None) => return Ok(Ended::Client),
            };
        }
        if let Err(error) = sent {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server link lost");
        }
        let greeting = self.initialize.as_ref().map(|(id, _)| id.clone());
        let lost = Vec::from_iter(self.pending.extract_if(.., |id| Some(id) != greeting.as_ref()));
        self.refused(lost.into_iter(), &ErrorData::internal_error("connection to the server was lost before it answered; call again", None)).await?;
        for line in self.changes.iter().filter(|_| self.initialized.is_some()) {
            self.answers.write_all(line).await?;
        }
        self.answers.flush().await?;
        Ok(Ended::Link { delivered })
    }

    async fn refused(&mut self, ids: impl Iterator<Item = Bytes>, error: &ErrorData) -> io::Result<()> {
        for id in ids {
            match refusal(&id, error) {
                Ok(line) => self.answers.write_all(&line).await?,
                Err(error) => tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "request id unreadable"),
            }
        }
        self.answers.flush().await
    }

    fn sent(&mut self, line: &Bytes) {
        let envelope = match decode::<Envelope<'_>>(line) {
            Ok(envelope) => envelope,
            Err(error) => return tracing::trace!({ EXCEPTION_MESSAGE } = %Report::new(&error), "line unreadable, passed through"),
        };
        let id = envelope.id.map(|id| line.slice_ref(id.get().as_bytes()));
        if let (Some(id), Some(_)) = (&id, envelope.method) {
            self.pending.insert(id.clone());
        }
        match envelope.method {
            Some(Method::Initialize) => self.initialize = id.map(|id| (id, line.clone())),
            Some(Method::Initialized) => self.initialized = Some(line.clone()),
            Some(Method::Cancelled) =>
                if let Some(cancelled) = envelope.params.and_then(|params| params.request_id) {
                    self.pending.remove(cancelled.get().as_bytes());
                },
            Some(
                Method::Discover
                | Method::Ping
                | Method::Progress
                | Method::CallTool
                | Method::GetPrompt
                | Method::Subscribe
                | Method::Unsubscribe
                | Method::ResourceUpdated
                | Method::Listen
                | Method::SetLevel
                | Method::Logged
                | Method::Other,
            )
            | None => {}
        }
    }

    fn received(&mut self, line: &[u8]) -> bool {
        let (id, result) = match decode::<Envelope<'_>>(line) {
            Ok(Envelope { id: Some(id), method: None, params: _, result, error: _ }) => (id, result),
            Ok(Envelope { id: _, method: _, params: _, result: _, error: _ }) => return true,
            Err(error) => {
                tracing::trace!({ EXCEPTION_MESSAGE } = %Report::new(&error), "line unreadable, passed through");
                return true;
            }
        };
        let answered = self.pending.remove(id.get().as_bytes());
        let greeting = self.initialize.as_ref().is_some_and(|(held, _)| held == id.get().as_bytes());
        match (greeting, mem::take(&mut self.swallow)) {
            (true, true) => false,
            (true, false) => {
                self.changes = result.map_or_default(|result| announced(result.get().as_bytes()));
                true
            }
            (false, swallowing) => {
                self.swallow = swallowing;
                answered
            }
        }
    }
}
