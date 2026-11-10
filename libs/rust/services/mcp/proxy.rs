use core::fmt::Write as _;
use core::mem;
use core::pin::Pin;
use core::sync::atomic::{AtomicU64, AtomicUsize, Ordering};
use core::time::Duration;
use std::collections::{BTreeMap, BTreeSet, VecDeque};
use std::io;
use std::sync::Arc;

use backon::{BackoffBuilder as _, ExponentialBuilder};
use bytes::{Buf as _, Bytes, BytesMut};
use futures::future::{Either, OptionFuture};
use futures::{FutureExt as _, StreamExt};
use rmcp::ErrorData;
use rmcp::model::{
    CancelledNotificationMethod, ConstString as _, DiscoverRequestMethod, ErrorCode, InitializeRequestParams, InitializeResultMethod, InitializedNotificationMethod, JsonRpcVersion2_0,
    PingRequestMethod, ProgressNotificationMethod, PromptListChangedNotificationMethod, ProtocolVersion, ResourceListChangedNotificationMethod, ResourceUpdatedNotificationMethod,
    SubscribeRequestMethod, ToolListChangedNotificationMethod, UnsubscribeRequestMethod,
};
use serde::{Deserialize, Serialize};
use serde_json::value::RawValue;
use serde_json::{Map, Value};
use tokio::io::{AsyncRead, AsyncWrite, AsyncWriteExt as _, BufWriter};
use tokio::sync::{mpsc, oneshot, watch};
use tokio::time::{Instant, Sleep, sleep_until};
use tokio_util::codec::FramedRead;
use tokio_util::sync::CancellationToken;

use crate::stdio::{Envelope, Line, Lines, Message, Params, identity};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Default, Serialize)]
pub enum Phase {
    #[default]
    Starting,
    Up,
    Idle,
    Down,
    Stopped,
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
    pub liveness: Option<Liveness>,
    pub retention: Retention,
    pub line: usize,
    pub queue: usize,
    pub buffer: usize,
}

#[derive(Debug)]
pub struct Link<R, W, C> {
    pub reader: R,
    pub writer: W,
    pub closed: C,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub struct Catalog {
    pub initialize: Map<String, Value>,
    pub cost: Duration,
}

#[derive(Debug, Clone, Default, Serialize)]
pub struct Status {
    pub phase: Phase,
    pub sessions: usize,
    pub starts: u32,
    pub failures: u32,
    pub cause: Option<Arc<str>>,
    pub catalog: Option<Arc<Catalog>>,
}

#[derive(Debug)]
enum Event {
    Joined { session: u64, out: mpsc::Sender<Bytes>, cut: CancellationToken },
    Line { session: u64, line: Line },
    Left { session: u64 },
    Drain(oneshot::Sender<()>),
    Stop,
}

#[derive(Debug)]
enum Origin {
    Session { session: u64, id: Bytes, token: Option<Bytes>, subscription: Option<(bool, String)> },
    Initialize,
    Internal,
}

#[derive(Debug)]
struct Session {
    out: mpsc::Sender<Bytes>,
    cut: CancellationToken,
    initialized: bool,
    requests: BTreeMap<Bytes, u64>,
}

#[derive(derive_more::Debug)]
struct Live<R, W, C> {
    #[debug(skip)]
    lines: FramedRead<R, Lines>,
    #[debug(skip)]
    writer: BufWriter<W>,
    outgoing: VecDeque<Bytes>,
    #[debug(skip)]
    closed: Pin<Box<C>>,
    stop: CancellationToken,
    started: Instant,
    ready: bool,
    answered: bool,
    ended: bool,
    unflushed: bool,
    probing: bool,
}

#[derive(derive_more::Debug)]
enum State<A, R, W, C> {
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

#[derive(Debug)]
enum Wake<R, W, C> {
    Event(Option<Event>),
    Line(Option<io::Result<Line>>),
    Wrote(io::Result<usize>),
    Flushed(io::Result<()>),
    Timer,
    Closed(String),
    Connected(io::Result<Link<R, W, C>>),
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Proxy {
    events: mpsc::Sender<Event>,
    status: watch::Receiver<Status>,
    in_flight: Arc<AtomicUsize>,
    joined: Arc<AtomicU64>,
    line: usize,
    queue: usize,
    buffer: usize,
}

impl Proxy {
    pub fn new<F, A, R, W, C>(policy: Policy, catalog: Option<Catalog>, connect: F) -> (Self, impl Future<Output = ()> + Send)
    where
        F: FnMut(CancellationToken) -> A + Send,
        A: Future<Output = io::Result<Link<R, W, C>>> + Send,
        R: AsyncRead + Send + Unpin,
        W: AsyncWrite + Send + Unpin,
        C: Future<Output = String> + Send, {
        let (events, inbox) = mpsc::channel(policy.queue);
        let catalog = catalog.map(Arc::new);
        let (status, watched) = watch::channel(Status { catalog: catalog.clone(), ..Status::default() });
        let in_flight = Arc::new(AtomicUsize::new(0));
        let proxy = Self { events, status: watched, in_flight: Arc::clone(&in_flight), joined: Arc::new(AtomicU64::new(0)), line: policy.line, queue: policy.queue, buffer: policy.buffer };
        let actor = Actor {
            policy,
            connect,
            events: inbox,
            status,
            in_flight,
            sessions: BTreeMap::new(),
            pending: BTreeMap::new(),
            waiting: 0,
            asked: BTreeMap::new(),
            subscribers: BTreeMap::new(),
            backlog: VecDeque::new(),
            next: 1,
            scratch: BytesMut::new(),
            state: State::Down { retry: Instant::now() },
            failures: 0,
            catalog,
            draining: None,
            timer: Box::pin(sleep_until(Instant::now())),
            armed: false,
            active: false,
        };
        (proxy, actor.run())
    }

    #[must_use]
    pub fn status(&self) -> watch::Receiver<Status> { self.status.clone() }

    #[must_use]
    pub fn in_flight(&self) -> usize { self.in_flight.load(Ordering::Relaxed) }

    pub async fn serve<R, W>(&self, reader: R, writer: W) -> io::Result<()>
    where
        R: AsyncRead + Unpin,
        W: AsyncWrite + Unpin, {
        let (session, cut) = (self.joined.fetch_add(1, Ordering::Relaxed), CancellationToken::new());
        let (out, mut outbound) = mpsc::channel(self.queue);
        let stopped = |mpsc::error::SendError(_)| io::Error::from(io::ErrorKind::ConnectionAborted);
        self.events.send(Event::Joined { session, out, cut: cut.clone() }).await.map_err(stopped)?;
        let mut lines = FramedRead::with_capacity(reader, Lines::new(self.line, self.buffer), self.buffer);
        let mut writer = BufWriter::with_capacity(self.buffer, writer);
        let mut batch = Vec::new();
        let served = loop {
            tokio::select! {
                biased;
                () = cut.cancelled() => break Ok(()),
                count = outbound.recv_many(&mut batch, self.queue) => {
                    if count == 0 {
                        break Ok(());
                    }
                    let written = async {
                        for chunk in batch.drain(..) {
                            writer.write_all(&chunk).await?;
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
                }
                line = lines.next() => match line {
                    Some(Ok(line)) => if self.events.send(Event::Line { session, line }).await.is_err() {
                        break Ok(());
                    },
                    Some(Err(error)) => break Err(error),
                    None => break Ok(()),
                },
            }
        };
        match self.events.send(Event::Left { session }).await {
            Ok(()) | Err(mpsc::error::SendError(_)) => served,
        }
    }

    pub async fn drain(&self) {
        let (drained, done) = oneshot::channel();
        if self.events.send(Event::Drain(drained)).await.is_ok() {
            match done.await {
                Ok(()) | Err(oneshot::error::RecvError { .. }) => {}
            }
        }
    }

    pub async fn stop(&self) {
        match self.events.send(Event::Stop).await {
            Ok(()) | Err(mpsc::error::SendError(_)) => self.events.closed().await,
        }
    }
}

#[derive(derive_more::Debug)]
struct Actor<F, A, R, W, C> {
    policy: Policy,
    #[debug(skip)]
    connect: F,
    events: mpsc::Receiver<Event>,
    status: watch::Sender<Status>,
    in_flight: Arc<AtomicUsize>,
    sessions: BTreeMap<u64, Session>,
    pending: BTreeMap<u64, Origin>,
    waiting: usize,
    asked: BTreeMap<Bytes, u64>,
    subscribers: BTreeMap<String, BTreeSet<u64>>,
    backlog: VecDeque<(u64, Bytes)>,
    next: u64,
    scratch: BytesMut,
    state: State<A, R, W, C>,
    failures: u32,
    catalog: Option<Arc<Catalog>>,
    draining: Option<oneshot::Sender<()>>,
    timer: Pin<Box<Sleep>>,
    armed: bool,
    active: bool,
}

impl<F, A, R, W, C> Actor<F, A, R, W, C>
where
    F: FnMut(CancellationToken) -> A + Send,
    A: Future<Output = io::Result<Link<R, W, C>>> + Send,
    R: AsyncRead + Send + Unpin,
    W: AsyncWrite + Send + Unpin,
    C: Future<Output = String> + Send,
{
    // --- [LIFECYCLE]

    async fn run(mut self) {
        self.start();
        loop {
            let wake = {
                let (attempt, live) = match &mut self.state {
                    State::Down { retry: _ } => (None, None),
                    State::Connecting { attempt, stop: _, started: _ } => (Some(attempt), None),
                    State::Up(live) => (None, Some(&mut **live)),
                };
                let room = live.as_ref().is_none_or(|live| live.outgoing.len() < self.policy.queue);
                let (lines, written, closed) = match live {
                    Some(Live { lines, writer, outgoing, closed, ended, unflushed, .. }) => {
                        let written = match (outgoing.front(), *unflushed) {
                            (Some(chunk), _) => Some(Either::Left(writer.write(chunk).map(Wake::Wrote))),
                            (None, true) => Some(Either::Right(writer.flush().map(Wake::Flushed))),
                            (None, false) => None,
                        };
                        ((!*ended).then_some(lines), written, Some(closed))
                    }
                    None => (None, None, None),
                };
                tokio::select! {
                    biased;
                    Some(written) = OptionFuture::from(written) => written,
                    Some(line) = OptionFuture::from(lines.map(StreamExt::next)) => Wake::Line(line),
                    event = self.events.recv(), if room => Wake::Event(event),
                    Some(cause) = OptionFuture::from(closed) => Wake::Closed(cause),
                    Some(link) = OptionFuture::from(attempt) => Wake::Connected(link),
                    () = &mut self.timer, if self.armed => Wake::Timer,
                }
            };
            match wake {
                Wake::Event(Some(Event::Stop) | None) => break self.stop().await,
                Wake::Event(Some(event)) => self.on_event(event),
                Wake::Line(Some(Ok(Line::Whole(line)))) => self.on_upstream(&line),
                Wake::Line(Some(Ok(Line::Oversized(head)))) => self.on_oversized(&head),
                Wake::Line(Some(Err(error))) => self.end(Some(error)),
                Wake::Line(None) => self.end(None),
                Wake::Wrote(Ok(count)) => self.wrote(count),
                Wake::Flushed(Ok(())) => self.flushed(),
                Wake::Wrote(Err(error)) | Wake::Flushed(Err(error)) => self.end(Some(error)),
                Wake::Timer => self.on_timer().await,
                Wake::Closed(cause) => self.fail(&cause),
                Wake::Connected(Ok(link)) => self.connected(link),
                Wake::Connected(Err(error)) => self.fail(&format!("cannot start: {error}")),
            }
            self.pump();
        }
    }

    fn start(&mut self) {
        let stop = CancellationToken::new();
        self.state = State::Connecting { attempt: Box::pin((self.connect)(stop.clone())), stop, started: Instant::now() };
        self.armed = false;
        self.status.send_modify(|status| (status.phase, status.starts, status.cause) = (Phase::Starting, status.starts.saturating_add(1), None));
    }

    fn connected(&mut self, link: Link<R, W, C>) {
        let State::Connecting { attempt: _, stop, started } = mem::replace(&mut self.state, State::Down { retry: Instant::now() }) else { return };
        let Link { reader, writer, closed } = link;
        let (limit, buffer) = (self.policy.line, self.policy.buffer);
        self.state = State::Up(Box::new(Live {
            lines: FramedRead::with_capacity(reader, Lines::new(limit, buffer), buffer),
            writer: BufWriter::with_capacity(buffer, writer),
            outgoing: VecDeque::new(),
            closed: Box::pin(closed),
            stop,
            started,
            ready: false,
            answered: false,
            ended: false,
            unflushed: false,
            probing: false,
        }));
        let id = self.issue(Origin::Initialize);
        let request = Message::<u64, &InitializeRequestParams, ()> {
            jsonrpc: JsonRpcVersion2_0,
            id: Some(id),
            method: Some(InitializeResultMethod::VALUE),
            params: Some(&self.policy.client),
            result: None,
            error: None,
        };
        self.send(request.line());
    }

    fn linked(&mut self, result: Option<&RawValue>, error: Option<&RawValue>) {
        let decoded = result.map(|result| serde_path_to_error::deserialize::<_, Map<String, Value>>(&mut serde_json::Deserializer::from_str(result.get())));
        let initialize = match (decoded, error) {
            (Some(Ok(initialize)), _) => initialize,
            (Some(Err(refused)), _) => return self.halt(&format!("server answered initialize with an unreadable result: {refused}")),
            (None, refused) => return self.halt(&format!("server refused initialize: {}", refused.map_or("no result", RawValue::get))),
        };
        let State::Up(live) = &mut self.state else { return };
        (live.ready, live.probing) = (true, false);
        let relinked = self.status.borrow().starts > 1;
        let catalog = Arc::new(Catalog { initialize, cost: live.started.elapsed() });
        let changed = self.catalog.as_ref().is_some_and(|held| held.initialize != catalog.initialize);
        self.catalog = Some(Arc::clone(&catalog));
        self.send(Message::<u64, (), ()> { jsonrpc: JsonRpcVersion2_0, id: None, method: Some(InitializedNotificationMethod::VALUE), params: None, result: None, error: None }.line());
        tracing::info!(cost = ?catalog.cost, changed, "server linked");
        let uris = Vec::from_iter(self.subscribers.keys().cloned());
        for uri in uris {
            self.subscription(SubscribeRequestMethod::VALUE, &uri);
        }
        if relinked || changed {
            let lists = [("tools", ToolListChangedNotificationMethod::VALUE), ("prompts", PromptListChangedNotificationMethod::VALUE), ("resources", ResourceListChangedNotificationMethod::VALUE)];
            for (_, method) in lists.into_iter().filter(|(list, _)| catalog.initialize.get("capabilities").and_then(|held| held.get(list)).and_then(|held| held.get("listChanged")) == Some(&Value::Bool(true))) {
                let notice = Message::<u64, (), ()> { jsonrpc: JsonRpcVersion2_0, id: None, method: Some(method), params: None, result: None, error: None }.line();
                self.broadcast(notice);
            }
        }
        self.status.send_modify(|status| (status.phase, status.cause, status.catalog) = (Phase::Up, None, Some(catalog)));
        self.rearm();
        for (session, line) in mem::take(&mut self.backlog) {
            self.on_line(session, &line);
        }
    }

    fn end(&mut self, error: Option<io::Error>) {
        if let Some(error) = error {
            tracing::warn!(%error, "server link failed");
        }
        if let State::Up(live) = &mut self.state {
            live.ended = true;
            live.outgoing.clear();
            live.stop.cancel();
        }
    }

    fn halt(&mut self, cause: &str) {
        self.end(None);
        if let State::Up(live) = &mut self.state {
            live.ready = false;
        }
        self.status.send_modify(|status| status.cause = Some(cause.into()));
    }

    fn fail(&mut self, exit: &str) {
        let (answered, stated) = (matches!(&self.state, State::Up(live) if live.answered), self.status.borrow().cause.clone());
        let cause: Arc<str> = stated.map_or_else(|| exit.into(), |stated| format!("{stated}; {exit}").into());
        self.failures = if answered { 1 } else { self.failures.saturating_add(1) };
        let delay = self.policy.restart.build().take(usize::try_from(self.failures).unwrap_or(usize::MAX)).last().unwrap_or_default();
        tracing::warn!(%cause, failures = self.failures, ?delay, "server down");
        self.state = State::Down { retry: Instant::now().checked_add(delay).unwrap_or_else(Instant::now) };
        self.armed = false;
        self.status.send_modify(|status| (status.phase, status.failures, status.cause) = (Phase::Down, self.failures, Some(Arc::clone(&cause))));
        self.reject(&format!("server exited before answering: {cause}; call again"));
        self.drained();
    }

    fn reject(&mut self, message: &str) {
        let error = ErrorData::new(ErrorCode::INTERNAL_ERROR, message.to_owned(), None);
        for origin in mem::take(&mut self.pending).into_values() {
            if let Origin::Session { session, id, token: _, subscription: _ } = origin {
                if let Some(held) = self.sessions.get_mut(&session) {
                    held.requests.remove(&id);
                }
                self.refuse(session, &id, &error);
            }
        }
        for (session, line) in mem::take(&mut self.backlog) {
            if let Ok(Envelope { id: Some(id), .. }) = Envelope::read(&line) {
                self.refuse(session, id.get().as_bytes(), &error);
            }
        }
        self.asked.clear();
        self.waiting = 0;
        self.in_flight.store(0, Ordering::Relaxed);
    }

    async fn idle(&mut self) {
        let State::Up(live) = mem::replace(&mut self.state, State::Down { retry: Instant::now() }) else { return };
        let Live { lines, writer, closed, stop, .. } = *live;
        stop.cancel();
        drop((lines, writer));
        let exit = closed.await;
        tracing::info!(exit, "server idle, stopped");
        self.armed = false;
        self.status.send_modify(|status| status.phase = Phase::Idle);
    }

    async fn stop(mut self) {
        self.reject("server stopped; call again");
        match mem::replace(&mut self.state, State::Down { retry: Instant::now() }) {
            State::Up(live) => {
                let Live { lines, writer, closed, stop, .. } = *live;
                stop.cancel();
                drop((lines, writer));
                let exit = closed.await;
                tracing::info!(exit, "server stopped");
            }
            State::Connecting { attempt, stop, started: _ } => {
                stop.cancel();
                drop(attempt);
            }
            State::Down { retry: _ } => {}
        }
        self.status.send_modify(|status| status.phase = Phase::Stopped);
    }

    // --- [TIMER]

    fn rearm(&mut self) {
        let ready = matches!(&self.state, State::Up(live) if live.ready);
        let cost = self.catalog.as_ref().map_or(Duration::ZERO, |catalog| catalog.cost);
        let Retention { ratio, floor, ceiling } = self.policy.retention;
        let period = match (ready, self.waiting, &self.policy.liveness) {
            (true, 1.., Some(liveness)) => Some(liveness.quiet),
            (true, 0, _) if self.subscribers.is_empty() => Some(cost.saturating_mul(ratio).clamp(floor, ceiling)),
            (_, _, _) => None,
        };
        self.arm(period);
    }

    fn arm(&mut self, period: Option<Duration>) {
        (self.armed, self.active) = (period.is_some(), false);
        if let Some(deadline) = period.and_then(|period| Instant::now().checked_add(period)) {
            self.timer.as_mut().reset(deadline);
        }
    }

    async fn on_timer(&mut self) {
        let probing = matches!(&self.state, State::Up(live) if live.probing);
        match (self.active, self.waiting, self.policy.liveness) {
            (true, _, _) => self.rearm(),
            (false, 0, _) if self.subscribers.is_empty() => self.idle().await,
            (false, 0, _) => self.rearm(),
            (false, 1.., Some(Liveness { quiet: _, grace })) if !probing => {
                let id = self.issue(Origin::Internal);
                self.send(Message::<u64, (), ()> { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: Some(PingRequestMethod::VALUE), params: None, result: None, error: None }.line());
                if let State::Up(live) = &mut self.state {
                    live.probing = true;
                }
                self.arm(Some(grace));
            }
            (false, 1.., _) => self.halt("server stopped answering"),
        }
    }

    // --- [UPSTREAM_WRITES]

    fn issue(&mut self, origin: Origin) -> u64 {
        let id = self.next;
        self.next = self.next.saturating_add(1);
        self.pending.insert(id, origin);
        id
    }

    fn send(&mut self, line: Result<Bytes, serde_json::Error>) {
        match (line, &mut self.state) {
            (Ok(line), State::Up(live)) if !live.ended => live.outgoing.push_back(line),
            (Ok(_), State::Up(_) | State::Down { .. } | State::Connecting { .. }) => {}
            (Err(error), _) => tracing::warn!(%error, "message not encoded"),
        }
    }

    fn pump(&mut self) {
        let State::Up(live) = &mut self.state else { return };
        while let Some(chunk) = live.outgoing.front_mut() {
            match live.writer.write(chunk).now_or_never() {
                Some(Ok(count)) => {
                    live.unflushed = true;
                    chunk.advance(count);
                    if chunk.is_empty() {
                        live.outgoing.pop_front();
                    }
                }
                Some(Err(_)) | None => break,
            }
        }
    }

    fn wrote(&mut self, count: usize) {
        let State::Up(live) = &mut self.state else { return };
        live.unflushed = true;
        if let Some(chunk) = live.outgoing.front_mut() {
            chunk.advance(count);
            if chunk.is_empty() {
                live.outgoing.pop_front();
            }
        }
    }

    fn flushed(&mut self) {
        if let State::Up(live) = &mut self.state {
            live.unflushed = false;
        }
    }

    fn subscription(&mut self, method: &str, uri: &str) {
        #[derive(Serialize)]
        struct Resource<'uri> {
            uri: &'uri str,
        }
        let id = self.issue(Origin::Internal);
        self.send(Message::<u64, Resource<'_>, ()> { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: Some(method), params: Some(Resource { uri }), result: None, error: None }.line());
    }

    fn copy(&mut self, bytes: &[u8]) -> Bytes {
        self.scratch.extend_from_slice(bytes);
        self.scratch.split().freeze()
    }

    // --- [SESSION_WRITES]

    fn deliver(&mut self, session: u64, chunks: impl IntoIterator<Item = Bytes>) {
        let Some(held) = self.sessions.get(&session) else { return };
        let kept = chunks.into_iter().try_for_each(|chunk| held.out.try_send(chunk));
        match kept {
            Ok(()) => {}
            Err(mpsc::error::TrySendError::Full(_)) => {
                tracing::warn!(session, queue = self.policy.queue, "session stopped reading, disconnected");
                self.leave(session);
            }
            Err(mpsc::error::TrySendError::Closed(_)) => self.leave(session),
        }
    }

    fn broadcast(&mut self, line: Result<Bytes, serde_json::Error>) {
        match line {
            Ok(line) => {
                let listening = Vec::from_iter(self.sessions.iter().filter(|(_, held)| held.initialized).map(|(session, _)| *session));
                for session in listening {
                    self.deliver(session, [line.clone()]);
                }
            }
            Err(error) => tracing::warn!(%error, "message not encoded"),
        }
    }

    fn refuse(&mut self, session: u64, id: &[u8], error: &ErrorData) {
        match serde_path_to_error::deserialize::<_, &RawValue>(&mut serde_json::Deserializer::from_slice(id)) {
            Ok(id) => match (Message::<&RawValue, (), ()> { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: None, params: None, result: None, error: Some(error) }).line() {
                Ok(line) => self.deliver(session, [line]),
                Err(error) => tracing::warn!(%error, "message not encoded"),
            },
            Err(error) => tracing::debug!(%error, "request id unreadable"),
        }
    }

    fn answer(&mut self, session: u64, id: &RawValue, result: &impl Serialize) {
        match (Message::<&RawValue, (), _> { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: None, params: None, result: Some(result), error: None }).line() {
            Ok(line) => self.deliver(session, [line]),
            Err(error) => tracing::warn!(%error, "message not encoded"),
        }
    }

    // --- [SESSIONS]

    fn on_event(&mut self, event: Event) {
        match event {
            Event::Joined { session, out, cut } => {
                self.sessions.insert(session, Session { out, cut, initialized: false, requests: BTreeMap::new() });
                self.status.send_modify(|status| status.sessions = self.sessions.len());
            }
            Event::Line { session, line: Line::Whole(line) } => self.on_line(session, &line),
            Event::Line { session, line: Line::Oversized(head) } => {
                let error = ErrorData::new(ErrorCode::INVALID_REQUEST, format!("request exceeds {} bytes, send less", self.policy.line), None);
                if let Some(id) = identity(&head) {
                    self.refuse(session, &id, &error);
                }
            }
            Event::Left { session } => self.leave(session),
            Event::Drain(drained) => {
                self.draining = Some(drained);
                self.drained();
            }
            Event::Stop => {}
        }
    }

    fn drained(&mut self) {
        if self.waiting == 0
            && let Some(drained) = self.draining.take()
        {
            match drained.send(()) {
                Ok(()) | Err(()) => {}
            }
            self.draining = None;
        }
    }

    fn leave(&mut self, session: u64) {
        let Some(Session { out: _, cut, initialized: _, requests }) = self.sessions.remove(&session) else { return };
        cut.cancel();
        #[derive(Serialize)]
        struct Cancelled {
            #[serde(rename = "requestId")]
            request_id: u64,
        }
        for request_id in requests.into_values() {
            if self.pending.remove(&request_id).is_some() {
                self.settled();
                self.send(
                    Message::<u64, Cancelled, ()> { jsonrpc: JsonRpcVersion2_0, id: None, method: Some(CancelledNotificationMethod::VALUE), params: Some(Cancelled { request_id }), result: None, error: None }
                        .line(),
                );
            }
        }
        let error = ErrorData::new(ErrorCode::INTERNAL_ERROR, "client session closed before answering", None);
        for id in Vec::from_iter(self.asked.extract_if(.., |_, asked| *asked == session).map(|(id, _)| id)) {
            match serde_path_to_error::deserialize::<_, &RawValue>(&mut serde_json::Deserializer::from_slice(&id)) {
                Ok(id) => self.send(Message::<&RawValue, (), ()> { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: None, params: None, result: None, error: Some(&error) }.line()),
                Err(error) => tracing::debug!(%error, "request id unreadable"),
            }
        }
        let unwatched = Vec::from_iter(self.subscribers.extract_if(.., |_, held| held.remove(&session) && held.is_empty()).map(|(uri, _)| uri));
        for uri in unwatched {
            self.subscription(UnsubscribeRequestMethod::VALUE, &uri);
        }
        self.status.send_modify(|status| status.sessions = self.sessions.len());
        self.drained();
    }

    fn settled(&mut self) {
        self.waiting = self.waiting.saturating_sub(1);
        self.in_flight.store(self.waiting, Ordering::Relaxed);
        if self.waiting == 0 && !self.armed {
            self.rearm();
        }
    }

    fn on_line(&mut self, session: u64, line: &Bytes) {
        let envelope = match Envelope::read(line) {
            Ok(envelope) => envelope,
            Err(error) => return tracing::debug!(%error, session, "client line unreadable"),
        };
        match (envelope.id, envelope.method.as_deref()) {
            (Some(id), Some(method)) => self.on_request(session, line, id, method, envelope.params.as_ref()),
            (None, Some(method)) if method == InitializedNotificationMethod::VALUE => {
                if let Some(held) = self.sessions.get_mut(&session) {
                    held.initialized = true;
                }
            }
            (None, Some(method)) if method == CancelledNotificationMethod::VALUE => {
                let cancelled = envelope.params.as_ref().and_then(|params| params.request_id);
                let request = cancelled.zip(self.sessions.get(&session)).and_then(|(id, held)| held.requests.get(id.get().as_bytes()).map(|request| (id, *request)));
                if let Some((id, request)) = request {
                    let number = self.digits(request);
                    self.forward(splice(line, (id.get().as_bytes(), number), None));
                }
            }
            (None, Some(_)) => self.forward([Some(line.clone()), None, None, None, None]),
            (Some(id), None) =>
                if self.asked.get(id.get().as_bytes()) == Some(&session) {
                    self.asked.remove(id.get().as_bytes());
                    self.forward([Some(line.clone()), None, None, None, None]);
                },
            (None, None) => {}
        }
    }

    fn on_request(&mut self, session: u64, line: &Bytes, id: &RawValue, method: &str, params: Option<&Params<'_>>) {
        let (ready, due) = match &self.state {
            State::Up(live) => (live.ready && !live.ended, false),
            State::Down { retry } => (false, Instant::now() >= *retry),
            State::Connecting { .. } => (false, false),
        };
        let waiting = !ready && (due || !matches!(self.state, State::Down { .. }));
        if due {
            self.start();
        }
        if method == InitializeResultMethod::VALUE {
            match (self.catalog.clone(), waiting) {
                (Some(catalog), _) => self.greet(session, id, params.and_then(|params| params.protocol_version.as_deref()), &catalog),
                (None, true) => self.backlog.push_back((session, line.clone())),
                (None, false) => self.unavailable(session, id),
            }
        } else if method == DiscoverRequestMethod::VALUE {
            self.refuse(session, id.get().as_bytes(), &ErrorData::method_not_found::<DiscoverRequestMethod>());
        } else if method == PingRequestMethod::VALUE {
            self.answer(session, id, &Map::new());
        } else if self.draining.is_some() {
            self.refuse(session, id.get().as_bytes(), &ErrorData::new(ErrorCode::INTERNAL_ERROR, "server is stopping; call again", None));
        } else if ready {
            self.call(session, line, id, method, params);
        } else if waiting && self.backlog.len() < self.policy.queue {
            self.backlog.push_back((session, line.clone()));
        } else {
            self.unavailable(session, id);
        }
    }

    fn unavailable(&mut self, session: u64, id: &RawValue) {
        let cause = self.status.borrow().cause.clone();
        let message = cause.map_or_else(|| "server is starting; call again".to_owned(), |cause| format!("server is down: {cause}; call again later"));
        self.refuse(session, id.get().as_bytes(), &ErrorData::new(ErrorCode::INTERNAL_ERROR, message, None));
    }

    fn greet(&mut self, session: u64, id: &RawValue, requested: Option<&str>, catalog: &Catalog) {
        const VERSION: &str = "protocolVersion";
        let served = catalog.initialize.get(VERSION).and_then(Value::as_str);
        let known = requested.filter(|requested| ProtocolVersion::KNOWN_VERSIONS.iter().any(|known| known.as_str() == *requested));
        let version = match (known, served) {
            (Some(requested), Some(served)) if requested <= served => Some(requested),
            (_, served) => served,
        };
        let mut result = catalog.initialize.clone();
        if let Some(version) = version {
            result.insert(VERSION.to_owned(), version.into());
        }
        self.answer(session, id, &result);
    }

    fn call(&mut self, session: u64, line: &Bytes, id: &RawValue, method: &str, params: Option<&Params<'_>>) {
        let uri = || params.and_then(|params| params.uri.as_deref()).map(str::to_owned);
        let subscription = match method {
            _ if method == SubscribeRequestMethod::VALUE => uri().map(|uri| (true, uri)),
            _ if method == UnsubscribeRequestMethod::VALUE => uri().map(|uri| (false, uri)),
            _ => None,
        };
        if let Some((false, uri)) = &subscription
            && let Some(held) = self.subscribers.get_mut(uri)
            && held.len() > 1
        {
            held.remove(&session);
            return self.answer(session, id, &Map::new());
        }
        let token = params.and_then(|params| params.meta.as_ref()).and_then(|meta| meta.progress_token);
        let (held, stated) = (self.copy(id.get().as_bytes()), token.map(|token| self.copy(token.get().as_bytes())));
        let request = self.issue(Origin::Session { session, id: held.clone(), token: stated, subscription });
        if let Some(joined) = self.sessions.get_mut(&session) {
            joined.requests.insert(held, request);
        }
        let number = self.digits(request);
        self.waiting = self.waiting.saturating_add(1);
        self.in_flight.store(self.waiting, Ordering::Relaxed);
        self.active = true;
        if self.waiting == 1 {
            self.rearm();
        }
        self.forward(splice(line, (id.get().as_bytes(), number.clone()), token.map(|token| (token.get().as_bytes(), number))));
    }

    fn digits(&mut self, number: u64) -> Bytes {
        match write!(self.scratch, "{number}") {
            Ok(()) | Err(core::fmt::Error) => self.scratch.split().freeze(),
        }
    }

    fn forward(&mut self, chunks: [Option<Bytes>; 5]) {
        if let State::Up(live) = &mut self.state
            && live.ready
            && !live.ended
        {
            live.outgoing.extend(chunks.into_iter().flatten());
        }
    }

    // --- [UPSTREAM_READS]

    fn on_upstream(&mut self, line: &Bytes) {
        self.active = true;
        if let State::Up(live) = &mut self.state {
            live.probing = false;
        }
        let envelope = match Envelope::read(line) {
            Ok(envelope) => envelope,
            Err(error) => return tracing::debug!(%error, "server line unreadable"),
        };
        match (envelope.id, envelope.method.as_deref()) {
            (Some(id), None) => self.on_response(line, id, envelope.result, envelope.error),
            (Some(id), Some(method)) => self.on_ask(line, id, method),
            (None, Some(method)) => self.on_notice(line, method, envelope.params.as_ref()),
            (None, None) => {}
        }
    }

    fn on_oversized(&mut self, head: &Bytes) {
        let limit = self.policy.line;
        tracing::warn!(limit, "server message over the line limit, discarded");
        let request = identity(head).and_then(|id| core::str::from_utf8(&id).ok().and_then(|id| id.parse::<u64>().ok()));
        let error = ErrorData::new(ErrorCode::INTERNAL_ERROR, format!("server response exceeds {limit} bytes, ask for less output"), None);
        if let Some(Origin::Session { session, id, token: _, subscription: _ }) = request.and_then(|request| self.pending.remove(&request)) {
            if let Some(held) = self.sessions.get_mut(&session) {
                held.requests.remove(&id);
            }
            self.settled();
            self.refuse(session, &id, &error);
            self.drained();
        }
    }

    fn on_response(&mut self, line: &Bytes, id: &RawValue, result: Option<&RawValue>, error: Option<&RawValue>) {
        let Some(origin) = id.get().parse::<u64>().ok().and_then(|request| self.pending.remove(&request)) else { return };
        match origin {
            Origin::Session { session, id: held, token: _, subscription } => {
                if let State::Up(live) = &mut self.state {
                    live.answered = true;
                }
                if let Some(joined) = self.sessions.get_mut(&session) {
                    joined.requests.remove(&held);
                }
                match (subscription, result.is_some()) {
                    (Some((true, uri)), true) => drop(self.subscribers.entry(uri).or_default().insert(session)),
                    (Some((false, uri)), _) => drop(self.subscribers.extract_if(.., |held, sessions| *held == uri && sessions.remove(&session) && sessions.is_empty()).count()),
                    (Some((true, _)), false) | (None, _) => {}
                }
                self.settled();
                self.deliver(session, splice(line, (id.get().as_bytes(), held), None).into_iter().flatten());
                self.drained();
            }
            Origin::Initialize => self.linked(result, error),
            Origin::Internal => {}
        }
    }

    fn on_ask(&mut self, line: &Bytes, id: &RawValue, method: &str) {
        let caller = self.pending.values().rev().find_map(|origin| match origin {
            Origin::Session { session, .. } => Some(*session),
            Origin::Initialize | Origin::Internal => None,
        });
        let only = || {
            let mut initialized = self.sessions.iter().filter(|(_, held)| held.initialized).map(|(session, _)| *session);
            initialized.next().filter(|_| initialized.next().is_none())
        };
        if method == PingRequestMethod::VALUE {
            self.send(Message::<&RawValue, (), _> { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: None, params: None, result: Some(Map::new()), error: None }.line());
        } else if let Some(session) = caller.or_else(only) {
            let asked = self.copy(id.get().as_bytes());
            self.asked.insert(asked, session);
            self.deliver(session, [line.clone()]);
        } else {
            let error = ErrorData::new(ErrorCode::METHOD_NOT_FOUND, format!("{method}: no client session has a call in flight"), None);
            self.send(Message::<&RawValue, (), ()> { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: None, params: None, result: None, error: Some(&error) }.line());
        }
    }

    fn on_notice(&mut self, line: &Bytes, method: &str, params: Option<&Params<'_>>) {
        if method == ProgressNotificationMethod::VALUE {
            let token = params.and_then(|params| params.progress_token);
            let routed = token.and_then(|token| token.get().parse::<u64>().ok().and_then(|request| self.pending.get(&request)).map(|origin| (token, origin)));
            if let Some((token, Origin::Session { session, id: _, token: Some(stated), subscription: _ })) = routed {
                let (session, chunks) = (*session, splice(line, (token.get().as_bytes(), stated.clone()), None));
                self.deliver(session, chunks.into_iter().flatten());
            }
        } else if method == CancelledNotificationMethod::VALUE {
            let asked = params.and_then(|params| params.request_id).and_then(|id| self.asked.remove(id.get().as_bytes()));
            if let Some(session) = asked {
                self.deliver(session, [line.clone()]);
            }
        } else if method == ResourceUpdatedNotificationMethod::VALUE {
            let watching = params.and_then(|params| params.uri.as_deref()).and_then(|uri| self.subscribers.get(uri)).map(|held| Vec::from_iter(held.iter().copied()));
            for session in watching.into_iter().flatten() {
                self.deliver(session, [line.clone()]);
            }
        } else {
            self.broadcast(Ok(line.clone()));
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn splice(line: &Bytes, first: (&[u8], Bytes), second: Option<(&[u8], Bytes)>) -> [Option<Bytes>; 5] {
    let span = |part: &[u8]| part.as_ptr().addr().checked_sub(line.as_ptr().addr()).and_then(|start| Some((start, start.checked_add(part.len()).filter(|end| *end <= line.len())?)));
    let whole = || [Some(line.clone()), None, None, None, None];
    match (span(first.0), second.as_ref().map(|(part, _)| span(part))) {
        (Some((start, end)), None) => [Some(line.slice(..start)), Some(first.1), Some(line.slice(end..)), None, None],
        (Some(one), Some(Some(two))) => {
            let (replacing, replaced) = second.map_or_else(|| (first.1.clone(), first.1.clone()), |(_, replacement)| (first.1.clone(), replacement));
            let [(start, end, earlier), (next, last, later)] = if one.0 <= two.0 { [(one.0, one.1, replacing), (two.0, two.1, replaced)] } else { [(two.0, two.1, replaced), (one.0, one.1, replacing)] };
            if end <= next { [Some(line.slice(..start)), Some(earlier), Some(line.slice(end..next)), Some(later), Some(line.slice(last..))] } else { whole() }
        }
        (None, _) | (_, Some(None)) => whole(),
    }
}
