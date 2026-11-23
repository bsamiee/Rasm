use core::time::Duration;
use core::{iter, mem};
use std::collections::{BTreeMap, BTreeSet};
use std::error::Report;
use std::io;
use std::sync::Arc;

use backon::BackoffBuilder as _;
use bytes::Bytes;
use futures::StreamExt as _;
use itertools::Itertools as _;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::ErrorData;
use rmcp::model::{
    CancelledNotificationMethod, CancelledNotificationParam, ClientCapabilities, ConstString as _, DiscoverRequestMethod, EmptyResult, ErrorCode, InitializeRequestParams, InitializeResultMethod,
    InitializedNotificationMethod, JsonRpcVersion2_0, LoggingLevel, PingRequestMethod, ProtocolVersion, RequestId, RequestMetaObject, ServerCapabilities, SetLevelRequestMethod,
    SubscribeRequestMethod, SubscribeRequestParams, UnsubscribeRequestMethod, UnsubscribeRequestParams,
};
use serde::{Deserialize, Serialize};
use serde_json::value::RawValue;
use serde_json::{Map, Value};
use tokio::io::{AsyncRead, AsyncWrite};
use tokio::sync::{mpsc, oneshot, watch};
use tokio::time::Instant;
use tokio_util::sync::CancellationToken;
use tokio_util::time::{DelayQueue, delay_queue};

use super::jsonrpc::{Envelope, Line, Message, Method, Params, decode, encoded, identity, refusal};
use super::routing::{Call, Calls, Client, Effect, Held, Origin, Session, Sessions, Subscribers, issued, read, splice};
use super::upstream::{Next, Upstream, Wake};
use super::{IdleTimeout, Key, Lifecycle, Link, Liveness, Phase, Policy, Profile, Status, announced};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Timer {
    Grace,
    Quiet,
    Probation,
    Settle,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Activity {
    Quiet,
    Touched,
    Probing,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENDED: &str = "the call that asked ended before the request could be routed";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub(super) enum Event {
    Joined { session: u64, out: mpsc::Sender<Bytes>, cut: CancellationToken },
    Line { session: u64, line: Line },
    Lifecycle(oneshot::Sender<Option<Lifecycle>>),
}

#[derive(Debug, derive_more::IsVariant)]
enum Drain {
    Serving,
    Draining { drained: oneshot::Sender<()>, settle: Duration, timer: delay_queue::Key },
    Drained,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
struct Clock {
    timers: DelayQueue<Timer>,
    quiet: Option<delay_queue::Key>,
    activity: Activity,
    drain: Drain,
}

impl Clock {
    fn disarm(&mut self) {
        self.timers.clear();
        (self.quiet, self.activity) = (None, Activity::Quiet);
        match &mut self.drain {
            Drain::Draining { drained: _, settle, timer } => *timer = self.timers.insert(Timer::Settle, *settle),
            Drain::Serving | Drain::Drained => {}
        }
    }

    fn rearm(&mut self, period: Option<Duration>) {
        if let Some(armed) = self.quiet.take() {
            self.timers.try_remove(&armed);
        }
        self.quiet = period.map(|period| self.timers.insert(Timer::Quiet, period));
        self.activity = Activity::Quiet;
    }

    fn drained(&mut self, settled: bool) {
        match mem::replace(&mut self.drain, Drain::Drained) {
            Drain::Draining { drained, settle, timer } if !settled => {
                self.timers.reset(&timer, settle);
                self.drain = Drain::Draining { drained, settle, timer };
            }
            Drain::Draining { drained, settle: _, timer } => {
                self.timers.try_remove(&timer);
                let (Ok(()) | Err(())) = drained.send(());
            }
            kept @ (Drain::Serving | Drain::Drained) => self.drain = kept,
        }
    }
}

#[derive(derive_more::Debug)]
pub(super) struct Actor<F, A, R, W, C> {
    policy: Policy,
    #[debug(skip)]
    connect: F,
    events: mpsc::Receiver<Event>,
    drains: mpsc::Receiver<(Duration, oneshot::Sender<()>)>,
    stopping: CancellationToken,
    status: watch::Sender<Status>,
    upstream: Upstream<A, R, W, C>,
    failures: usize,
    hint: Option<Lifecycle>,
    reprobe: bool,
    profile: Option<Arc<Profile>>,
    logging: bool,
    asking: Vec<oneshot::Sender<Option<Lifecycle>>>,
    sessions: Sessions,
    calls: Calls,
    subscribers: Subscribers,
    clock: Clock,
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

    pub(super) fn new(
        policy: Policy,
        connect: F,
        events: mpsc::Receiver<Event>,
        drains: mpsc::Receiver<(Duration, oneshot::Sender<()>)>,
        stopping: CancellationToken,
        status: watch::Sender<Status>,
        calls: Calls,
    ) -> Self {
        let (failures, delay, profile) = {
            let held = status.borrow();
            (held.failures, held.delay, held.profile.clone())
        };
        Self {
            policy,
            connect,
            events,
            drains,
            stopping,
            status,
            upstream: Upstream::down(delay),
            failures,
            hint: None,
            reprobe: false,
            profile,
            logging: false,
            asking: Vec::new(),
            sessions: Sessions::default(),
            calls,
            subscribers: Subscribers::default(),
            clock: Clock { timers: DelayQueue::new(), quiet: None, activity: Activity::Quiet, drain: Drain::Serving },
        }
    }

    pub(super) async fn run(mut self) {
        if self.failures == 0 {
            self.start();
        }
        loop {
            let room = self.upstream.room();
            tokio::select! {
                () = self.stopping.cancelled() => break self.stop().await,
                wake = self.upstream.next() => self.on_wake(wake),
                event = self.events.recv(), if room => if self.on_event(event) {
                    break self.stop().await;
                },
                Some((settle, drained)) = self.drains.recv() => {
                    let timer = self.clock.timers.insert(Timer::Settle, settle);
                    self.clock.drain = Drain::Draining { drained, settle, timer };
                    self.drained(false);
                }
                Some(expired) = self.clock.timers.next(), if !self.clock.timers.is_empty() => self.on_timer(expired.into_inner()).await,
                Some(session) = self.sessions.left.next(), if !self.sessions.left.is_empty() => self.leave(session),
            }
        }
    }

    fn on_event(&mut self, event: Option<Event>) -> bool {
        match event {
            None => return true,
            Some(Event::Joined { session, out, cut }) => {
                self.sessions.join(session, out, cut);
                self.status.send_modify(|status| status.sessions = self.sessions.joined.len());
            }
            Some(Event::Line { session, line: _ }) if !self.sessions.joined.contains_key(&session) => {}
            Some(Event::Line { session, line: Line::Whole(line) }) => self.on_line(session, &line),
            Some(Event::Line { session, line: Line::Oversized { head, tail } }) => {
                let error = ErrorData::invalid_request(format!("request exceeds {} bytes, send less", self.policy.line), None);
                if let (Some(id), _) = identity(&head, &tail) {
                    self.sessions.reply(session, refusal(&id, &error));
                }
            }
            Some(Event::Lifecycle(asked)) => self.on_lifecycle(asked),
        }
        false
    }

    fn on_wake(&mut self, wake: Wake<R, W, C>) {
        match wake {
            Wake::Line(Some(Ok(Line::Whole(line)))) => self.on_upstream(&line),
            Wake::Line(Some(Ok(Line::Oversized { head, tail }))) => self.on_oversized(&head, &tail),
            Wake::Line(Some(Err(error))) | Wake::Failed(error) => self.end(Some(&error)),
            Wake::Line(None) => self.end(None),
            Wake::Closed(cause) => self.fail(&cause),
            Wake::Connected(Ok(link), stop, started) => self.connected(link, stop, started),
            Wake::Connected(Err(error), _, _) => self.fail(&format!("cannot start: {}", Report::new(&error))),
        }
    }

    fn start(&mut self) {
        let stop = CancellationToken::new();
        self.upstream = Upstream::Connecting { attempt: Box::pin((self.connect)(stop.clone())), stop, started: Instant::now() };
        self.disarm();
        self.status.send_modify(|status| (status.phase, status.starts, status.cause) = (Phase::Starting, status.starts.saturating_add(1), None));
    }

    fn wake(&mut self) {
        match &self.upstream {
            Upstream::Down { retry } if Instant::now() >= *retry && self.clock.drain.is_serving() => self.start(),
            Upstream::Down { retry: _ } | Upstream::Connecting { attempt: _, stop: _, started: _ } | Upstream::Up(_) => {}
        }
    }

    fn connected(&mut self, link: Link<R, W, C>, stop: CancellationToken, started: Instant) {
        self.upstream = Upstream::link(link, self.policy.line, stop, started);
        match self.hint.or_else(|| self.profile.as_ref().map(|profile| profile.lifecycle)) {
            Some(Lifecycle::Initialize) => self.probe(Lifecycle::Initialize),
            Some(Lifecycle::Discover) | None => {
                self.probe(Lifecycle::Discover);
                let _grace: delay_queue::Key = self.clock.timers.insert(Timer::Grace, self.policy.handshake);
            }
        }
        self.rearm();
    }

    fn probe(&mut self, lifecycle: Lifecycle) {
        let line = match lifecycle {
            Lifecycle::Discover => discover(&self.policy.client, self.calls.issue(Origin::Discover)),
            Lifecycle::Initialize => message(Some(self.calls.issue(Origin::Initialize)), InitializeResultMethod::VALUE, Some(&self.policy.client)),
        };
        if let Some(negotiation) = self.upstream.negotiation() {
            negotiation.sent(lifecycle);
        }
        self.upstream.send(line);
    }

    fn negotiated(&mut self, lifecycle: Lifecycle, result: Option<&RawValue>, refused: Option<&RawValue>) {
        match self.upstream.negotiation().and_then(|negotiation| negotiation.answered(lifecycle, result, refused)) {
            Some(Next::Link(lifecycle, handshake)) => self.linked(lifecycle, handshake),
            Some(Next::Probe(lifecycle)) => self.probe(lifecycle),
            Some(Next::Halt(cause)) => self.halt(&cause),
            None => {}
        }
    }

    fn linked(&mut self, lifecycle: Lifecycle, handshake: Box<RawValue>) {
        #[derive(Deserialize)]
        struct Declared {
            capabilities: ServerCapabilities,
        }
        let Some(cost) = self.upstream.linked() else { return };
        self.logging = match decode::<Declared>(handshake.get().as_bytes()) {
            Ok(Declared { capabilities }) => capabilities.logging.is_some(),
            Err(error) => {
                tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server capabilities unreadable, log levels pass through");
                false
            }
        };
        let held = self.profile.take().map(Arc::unwrap_or_clone);
        let (changed, flipped) = held.as_ref().map_or((false, false), |held| (held.handshake.get() != handshake.get(), held.lifecycle != lifecycle));
        let (askers, kept) = held.map_or_default(|Profile { lifecycle: _, handshake: _, cost: _, askers, quiet, responsive }| (askers, (!changed).then_some((quiet, responsive))));
        let (quiet, responsive) = kept.unwrap_or_default();
        let profile = Arc::new(Profile { lifecycle, handshake, cost, askers, quiet, responsive });
        self.profile = Some(Arc::clone(&profile));
        let relinked = self.status.borrow().starts > 1;
        tracing::debug!(%lifecycle, ?cost, changed, "server linked");
        if flipped {
            self.sessions.joined.values().for_each(|session| session.cut.cancel());
        }
        if lifecycle.is_initialize() {
            self.upstream.send(message(None, InitializedNotificationMethod::VALUE, None::<()>));
            for uri in self.subscribers.resources.keys() {
                self.upstream.send(message(Some(self.calls.issue(Origin::Internal)), SubscribeRequestMethod::VALUE, Some(SubscribeRequestParams::new(uri))));
            }
            if let Some(level) = self.subscribers.level {
                self.upstream.send(message(Some(self.calls.issue(Origin::Internal)), SetLevelRequestMethod::VALUE, Some(BTreeMap::from([("level", level)]))));
            }
            for line in announced(profile.handshake.get().as_bytes()).into_iter().filter(|_| relinked || changed) {
                self.sessions.broadcast(&line);
            }
        }
        self.status.send_modify(|status| (status.phase, status.cause, status.profile) = (Phase::Up, None, Some(profile)));
        self.told(Some(lifecycle));
        self.rearm();
        self.replay();
    }

    fn told(&mut self, lifecycle: Option<Lifecycle>) {
        for asked in self.asking.drain(..) {
            let (Ok(()) | Err(Some(Lifecycle::Initialize | Lifecycle::Discover) | None)) = asked.send(lifecycle);
        }
    }

    fn suspect(&mut self) -> bool {
        let silent = self.hint.is_none() && self.profile.is_none() && self.upstream.negotiation().is_some_and(|negotiation| negotiation.silent());
        if silent {
            self.hint = Some(Lifecycle::Initialize);
        }
        silent
    }

    fn end(&mut self, error: Option<&io::Error>) {
        self.reprobe |= self.suspect();
        self.upstream.end(error);
    }

    fn halt(&mut self, cause: &str) {
        self.end(None);
        self.status.send_modify(|status| status.cause = Some(cause.into()));
    }

    fn fail(&mut self, exit: &str) {
        if mem::take(&mut self.reprobe) | self.suspect() {
            tracing::debug!(exit, "server ended on server/discover, asking initialize first");
            drop(self.calls.reset());
            return self.start();
        }
        let stated = self.status.borrow().cause.clone();
        let cause: Arc<str> = stated.map_or_else(|| exit.into(), |stated| format!("{stated}; {exit}").into());
        self.failures = self.failures.saturating_add(1);
        let delay = self.policy.restart.build().take(self.failures).last().unwrap_or_default();
        tracing::debug!({ EXCEPTION_MESSAGE } = %cause, failures = self.failures, ?delay, "server down");
        self.upstream = Upstream::down(delay);
        self.disarm();
        self.status.send_modify(|status| (status.phase, status.failures, status.delay, status.cause) = (Phase::Down, self.failures, delay, Some(Arc::clone(&cause))));
        self.reject(&format!("server exited before answering: {cause}; call again"));
        self.told(None);
        self.drained(false);
    }

    fn reject(&mut self, message: &str) {
        let error = ErrorData::internal_error(message.to_owned(), None);
        let waiting = self.calls.reset().filter_map(|origin| match origin {
            Origin::Session(call) => Some((call.session, call.id)),
            Origin::Discover | Origin::Initialize | Origin::Internal | Origin::Probe => None,
        });
        let parked = mem::take(&mut self.calls.backlog).into_iter().filter_map(|(session, line)| Some((session, identity(&line, &[]).0?)));
        for (session, id) in waiting.chain(parked) {
            self.sessions.reply(session, refusal(&id, &error));
        }
    }

    async fn idle(&mut self) {
        self.asking.retain(|asked| !asked.is_closed());
        if self.upstream.ready() || (self.upstream.negotiation().is_some() && self.sessions.joined.is_empty() && self.asking.is_empty()) {
            let exit = mem::replace(&mut self.upstream, Upstream::down(Duration::ZERO)).close().await;
            tracing::debug!(exit, "server idle, stopped");
            self.disarm();
            self.status.send_modify(|status| status.phase = Phase::Idle);
        } else {
            self.rearm();
        }
    }

    async fn stop(mut self) {
        self.reject("server stopped; call again");
        self.told(None);
        let exit = self.upstream.close().await;
        tracing::debug!(exit, "server stopped");
        self.status.send_modify(|status| status.phase = Phase::Stopped);
    }

    // --- [TIMERS]

    fn disarm(&mut self) {
        self.clock.disarm();
        self.calls.writer = None;
    }

    fn rearm(&mut self) {
        let cost = self.profile.as_ref().map_or(Duration::ZERO, |profile| profile.cost);
        let IdleTimeout { ratio, floor, ceiling } = self.policy.idle;
        let starting = self.upstream.negotiation().is_some();
        self.clock.rearm(match (self.upstream.ready(), self.calls.waiting(), self.policy.liveness) {
            (true, 1.., Some(Liveness { quiet, grace: _ })) => Some(quiet),
            (true, 0, _) if self.sessions.joined.is_empty() => Some(cost.saturating_mul(ratio).max(floor).min(ceiling)),
            (false, _, _) if starting => Some(ceiling),
            (_, _, _) => None,
        });
    }

    fn drained(&mut self, expired: bool) {
        let settled = expired || self.calls.callers().all(|(_, call)| call.effect.is_listen());
        self.clock.drained(settled);
    }

    async fn on_timer(&mut self, timer: Timer) {
        match timer {
            Timer::Grace =>
                if self.upstream.negotiation().is_some_and(|negotiation| negotiation.unasked()) {
                    self.probe(Lifecycle::Initialize);
                },
            Timer::Probation => {
                self.calls.writer = None;
                self.replay();
            }
            Timer::Settle => self.drained(true),
            Timer::Quiet => {
                self.clock.quiet = None;
                match (self.clock.activity, self.calls.waiting(), self.policy.liveness) {
                    (Activity::Quiet | Activity::Probing, 0, _) if self.sessions.joined.is_empty() => self.idle().await,
                    (Activity::Probing, 1.., Some(_)) if self.profile.as_ref().is_some_and(|profile| self.calls.callers().all(|(_, call)| profile.responsive.contains(&call.key))) =>
                        self.halt("server stopped answering"),
                    (Activity::Touched, _, _) | (Activity::Quiet | Activity::Probing, 0, _) | (Activity::Probing, 1.., Some(_)) => self.rearm(),
                    (Activity::Quiet, 1.., Some(Liveness { quiet: _, grace })) => {
                        let id = self.calls.issue(Origin::Probe);
                        self.upstream.send(match self.profile.as_ref().map(|profile| profile.lifecycle) {
                            Some(Lifecycle::Discover) => discover(&self.policy.client, id),
                            Some(Lifecycle::Initialize) | None => message(Some(id), PingRequestMethod::VALUE, None::<()>),
                        });
                        self.clock.activity = Activity::Probing;
                        self.clock.quiet = Some(self.clock.timers.insert(Timer::Quiet, grace));
                    }
                    (Activity::Quiet | Activity::Probing, 1.., None) => {}
                }
            }
        }
    }

    // --- [SESSIONS]

    fn on_lifecycle(&mut self, asked: oneshot::Sender<Option<Lifecycle>>) {
        self.wake();
        self.asking.push(asked);
        let lifecycle = self.profile.as_ref().map(|profile| profile.lifecycle);
        if lifecycle.is_some() || self.upstream.is_down() {
            self.told(lifecycle);
        }
    }

    fn leave(&mut self, session: u64) {
        let Some(Session { out: _, cut, initialized: _, level: _, client: _ }) = self.sessions.joined.remove(&session) else { return };
        cut.cancel();
        self.calls.backlog.retain(|(parked, _)| *parked != session);
        for request in self.calls.of(session) {
            if self.calls.take(request).is_some() {
                self.upstream.send(message(None, CancelledNotificationMethod::VALUE, Some(CancelledNotificationParam::new(Some(RequestId::Number(request.cast_signed())), None))));
                self.settled(request, None);
            }
        }
        for id in Vec::from_iter(self.calls.asked.iter().filter(|(_, (waiting, _))| waiting.contains(&session)).map(|(id, _)| id.clone())) {
            self.answered(session, &id, None);
        }
        for uri in self.subscribers.leave(session) {
            self.upstream.send(message(Some(self.calls.issue(Origin::Internal)), UnsubscribeRequestMethod::VALUE, Some(UnsubscribeRequestParams::new(uri))));
        }
        self.relevel();
        self.status.send_modify(|status| status.sessions = self.sessions.joined.len());
        if self.sessions.joined.is_empty() && self.upstream.ready() {
            self.rearm();
        }
    }

    fn on_line(&mut self, session: u64, line: &Bytes) {
        let envelope = match decode::<Envelope<'_>>(line) {
            Ok(envelope) => envelope,
            Err(error) => return tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), session, "client line unreadable"),
        };
        let params = envelope.params.as_ref();
        match (envelope.id, envelope.method) {
            (Some(id), Some(method)) if self.dispatch(session, line, id, &method, params) => {}
            (Some(_), Some(_)) if !self.upstream.is_down() && self.calls.backlog.len() < self.policy.queue => self.calls.backlog.push_back((session, line.clone())),
            (Some(id), Some(_)) => {
                let cause = self.status.borrow().cause.clone();
                let message = cause.map_or_else(|| "server is starting; call again".to_owned(), |cause| format!("server is down: {cause}; call again later"));
                self.sessions.reply(session, refusal(id.get().as_bytes(), &ErrorData::internal_error(message, None)));
            }
            (None, Some(method)) if method.is_initialized() =>
                if let Some(held) = self.sessions.joined.get_mut(&session) {
                    held.initialized = true;
                },
            (None, Some(method)) if method.is_cancelled() => self.cancelled(session, line, params.and_then(|params| params.request_id)),
            (None, Some(_)) => self.upstream.forward([line.clone()]),
            (Some(id), None) => self.answered(session, id.get().as_bytes(), Some((line, envelope.result))),
            (None, None) => {}
        }
    }

    fn cancelled(&mut self, session: u64, line: &Bytes, cancelled: Option<&RawValue>) {
        let request = cancelled.and_then(|id| self.calls.named(session, id.get().as_bytes()).map(|request| (id, request)));
        match (request, cancelled) {
            (Some((id, request)), _) => {
                self.upstream.forward(splice(line, [(id.get().as_bytes(), Bytes::from(request.to_string()))]));
                if self.calls.take(request).is_some() {
                    self.settled(request, None);
                }
            }
            (None, Some(id)) => self.calls.backlog.retain(|(owner, held)| *owner != session || identity(held, &[]).0.is_none_or(|parked| *parked != *id.get().as_bytes())),
            (None, None) => {}
        }
    }

    fn replay(&mut self) {
        for (session, line) in mem::take(&mut self.calls.backlog) {
            let admitted = match decode::<Envelope<'_>>(&line) {
                Ok(Envelope { id: Some(id), method: Some(method), params, result: _, error: _ }) => self.dispatch(session, &line, id, &method, params.as_ref()),
                Ok(Envelope { id: _, method: _, params: _, result: _, error: _ }) => true,
                Err(error) => {
                    tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), session, "client line unreadable");
                    true
                }
            };
            if !admitted {
                self.calls.backlog.push_back((session, line));
            }
        }
    }

    fn dispatch(&mut self, session: u64, line: &Bytes, id: &RawValue, method: &Method, params: Option<&Params<'_>>) -> bool {
        let uri = || read::<String>(params.and_then(|params| params.uri));
        let effect = match method {
            Method::Ping => {
                self.sessions.answer(session, id, &EmptyResult {});
                return true;
            }
            Method::Initialize => return self.greet(session, id, Lifecycle::Initialize, params),
            Method::Discover => return self.greet(session, id, Lifecycle::Discover, params),
            Method::Subscribe => uri().map_or(Effect::Plain, Effect::Subscribe),
            Method::Unsubscribe => uri().map_or(Effect::Plain, Effect::Unsubscribe),
            Method::SetLevel => read::<LoggingLevel>(params.and_then(|params| params.level)).map_or(Effect::Plain, Effect::Level),
            Method::Listen => Effect::Listen,
            Method::Initialized
            | Method::Cancelled
            | Method::Progress
            | Method::CallTool
            | Method::GetPrompt
            | Method::ResourceUpdated
            | Method::Logged
            | Method::CreateMessage
            | Method::Elicit
            | Method::ListRoots
            | Method::Other(_) => Effect::Plain,
        };
        self.call(session, line, id, method, params, effect)
    }

    fn greet(&mut self, session: u64, id: &RawValue, asked: Lifecycle, params: Option<&Params<'_>>) -> bool {
        #[derive(Serialize, Deserialize)]
        struct Greeting {
            #[serde(rename = "protocolVersion")]
            protocol_version: ProtocolVersion,
            #[serde(flatten)]
            rest: Map<String, Value>,
        }
        self.wake();
        let Some(profile) = &self.profile else { return false };
        let requested = read::<ProtocolVersion>(params.and_then(|params| params.protocol_version));
        let declared = params.and_then(|params| params.capabilities.zip(read::<ClientCapabilities>(params.capabilities)).map(|(raw, capabilities)| (raw, capabilities, params.client_info)));
        if let (Lifecycle::Initialize, Some((raw, capabilities, named)), Some(joined)) = (asked, declared, self.sessions.joined.get_mut(&session)) {
            let named = named.map_or_default(|named| [&br#","io.modelcontextprotocol/clientInfo":"#[..], named.get().as_bytes()].concat());
            let context = Bytes::from([&br#""io.modelcontextprotocol/clientCapabilities":"#[..], raw.get().as_bytes(), &named].concat());
            joined.client = Some(Client { capabilities: Arc::new(capabilities), context });
        }
        let refused = |error: &ErrorData| self.sessions.reply(session, refusal(id.get().as_bytes(), error));
        match (asked, profile.lifecycle) {
            (Lifecycle::Discover, Lifecycle::Discover) => self.sessions.answer(session, id, &profile.handshake),
            (Lifecycle::Discover, Lifecycle::Initialize) => {
                let refused_discover = format!("this server opens with initialize at protocol {} or earlier and serves no server/discover", ProtocolVersion::LATEST_WITH_INITIALIZE.as_str());
                refused(&ErrorData::new(ErrorCode::METHOD_NOT_FOUND, refused_discover, None));
            }
            (Lifecycle::Initialize, Lifecycle::Discover) => {
                let modern = Vec::from_iter(ProtocolVersion::KNOWN_VERSIONS.iter().filter(|version| !version.has_initialize()).cloned());
                refused(&ErrorData::unsupported_protocol_version(requested.unwrap_or(ProtocolVersion::LATEST_WITH_INITIALIZE), &modern));
            }
            (Lifecycle::Initialize, Lifecycle::Initialize) =>
                match (decode::<Greeting>(profile.handshake.get().as_bytes()), requested.filter(|requested| ProtocolVersion::KNOWN_VERSIONS.contains(requested))) {
                    (Ok(served), Some(requested)) if requested.as_str() < served.protocol_version.as_str() =>
                        self.sessions.answer(session, id, &Greeting { protocol_version: requested, rest: served.rest }),
                    (Ok(_), _) => self.sessions.answer(session, id, &profile.handshake),
                    (Err(error), _) => {
                        tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "stored initialize result unreadable");
                        self.sessions.answer(session, id, &profile.handshake);
                    }
                },
        }
        true
    }

    fn call(&mut self, session: u64, line: &Bytes, id: &RawValue, method: &Method, params: Option<&Params<'_>>, effect: Effect) -> bool {
        if !self.clock.drain.is_serving() {
            self.sessions.reply(session, refusal(id.get().as_bytes(), &ErrorData::internal_error("server is stopping; call again", None)));
            return true;
        }
        self.wake();
        if self.profile.is_none() || !self.upstream.ready() {
            return false;
        }
        match (&effect, self.logging, self.sessions.joined.get_mut(&session)) {
            (Effect::Level(level), true, Some(joined)) => {
                joined.level = Some(*level);
                self.relevel();
                self.sessions.answer(session, id, &EmptyResult {});
                return true;
            }
            (Effect::Plain | Effect::Subscribe(_) | Effect::Unsubscribe(_) | Effect::Level(_) | Effect::Listen, _, _) => {}
        }
        if self.subscribers.local(&effect, session) {
            self.sessions.answer(session, id, &EmptyResult {});
            return true;
        }
        let Some(profile) = &self.profile else { return false };
        let key = Key { method: method.clone(), target: params.and_then(|params| params.name.or(params.uri).or(params.reference)).map(|target| target.get().into()) };
        let exclusive = profile.lifecycle.is_initialize() && !profile.quiet.contains(&key);
        if exclusive && self.calls.writer.is_some() {
            return false;
        }
        let meta = params.and_then(|params| params.meta.as_ref());
        let (token, stated) = (meta.and_then(|meta| meta.progress_token), meta.and_then(|meta| meta.client_capabilities));
        let client = self.sessions.joined.get(&session).and_then(|joined| joined.client.as_ref());
        let capabilities = read::<ClientCapabilities>(stated).map(Arc::new).or_else(|| client.map(|client| Arc::clone(&client.capabilities)));
        let context = client.filter(|_| stated.is_none()).and_then(|client| contextualized(line, &client.context));
        let (held, named) = (Bytes::copy_from_slice(id.get().as_bytes()), token.map(|token| Bytes::copy_from_slice(token.get().as_bytes())));
        let request = self.calls.issue(Origin::Session(Call { session, id: held, token: named, key, effect, capabilities, asked: false }));
        if exclusive {
            self.calls.writer = Some((request, self.clock.timers.insert(Timer::Probation, self.policy.probation)));
        }
        let number = Bytes::from(request.to_string());
        self.clock.activity = Activity::Touched;
        if self.calls.waiting() == 1 {
            self.rearm();
        }
        let edits = iter::chain([(id.get().as_bytes(), number.clone())], token.map(|token| (token.get().as_bytes(), number))).chain(context);
        self.upstream.forward(splice(line, edits));
        true
    }

    fn relevel(&mut self) {
        if let Some(level) = self.subscribers.leveled(&self.sessions.joined) {
            self.upstream.send(message(Some(self.calls.issue(Origin::Internal)), SetLevelRequestMethod::VALUE, Some(BTreeMap::from([("level", level)]))));
        }
    }

    // --- [ATTRIBUTION]

    fn decline(&mut self, id: &[u8], reason: &str) { self.upstream.send(refusal(id, &ErrorData::new(ErrorCode::METHOD_NOT_FOUND, reason.to_owned(), None))); }

    fn settled(&mut self, request: u64, completed: Option<Key>) {
        if let Some((_, probation)) = self.calls.writer.take_if(|(writer, _)| *writer == request) {
            self.clock.timers.try_remove(&probation);
        }
        for Held { id, line, method, candidates } in Vec::from_iter(self.calls.held.extract_if(.., |held| {
            held.candidates.remove(&request);
            held.candidates.len() < 2
        })) {
            match candidates.first() {
                Some(caller) => self.ask(*caller, id, line, &method),
                None => self.decline(&id, ENDED),
            }
        }
        let blocked = BTreeSet::from_iter(self.calls.held.iter().flat_map(|held| held.candidates.iter().copied()));
        if !self.calls.held.is_empty() && blocked.len() <= self.calls.held.len() {
            for Held { id, line: _, method: _, candidates: _ } in mem::take(&mut self.calls.held) {
                self.decline(&id, "more than one call was waiting on a request and none states its caller; ask again");
            }
            for key in Vec::from_iter(self.calls.callers().filter(|(caller, _)| blocked.contains(caller)).map(|(_, call)| call.key.clone())) {
                self.learn(key, true);
            }
        }
        if let Some(key) = completed {
            self.learn(key, false);
        }
        if self.calls.waiting() == 0 {
            self.rearm();
        }
        self.drained(false);
        if self.calls.writer.is_none() {
            self.replay();
        }
    }

    fn ask(&mut self, caller: u64, id: Bytes, line: Bytes, method: &Method) {
        let Some(Origin::Session(call)) = self.calls.pending.get_mut(&caller) else { return self.decline(&id, ENDED) };
        call.asked = true;
        let (session, key, declared) = (call.session, call.key.clone(), call.capabilities.as_deref().is_none_or(|capabilities| declares(capabilities, method)));
        if declared {
            self.calls.asked.insert(id, (BTreeSet::from([session]), None));
            self.sessions.deliver(session, [line]);
        } else {
            self.decline(&id, &format!("the client session whose call caused {method} declares no capability for it"));
        }
        self.learn(key, true);
    }

    #[expect(deprecated, reason = "rmcp deprecates its roots types under SEP-2577, and servers opened with initialize still list roots")]
    fn answered(&mut self, session: u64, id: &[u8], answer: Option<(&Bytes, Option<&RawValue>)>) {
        let Some((waiting, listed)) = self.calls.asked.get_mut(id).filter(|(waiting, _)| waiting.contains(&session)) else { return };
        waiting.remove(&session);
        if let Some(listed) = listed {
            listed.extend(answer.and_then(|(_, result)| result).map(RawValue::to_owned));
        }
        if !waiting.is_empty() {
            return;
        }
        match (self.calls.asked.remove(id).and_then(|(_, results)| results), answer) {
            (Some(results), _) => {
                let roots = results.iter().filter_map(|result| read::<rmcp::model::ListRootsResult>(Some(result))).flat_map(|result| result.roots).unique_by(|root| root.uri.clone());
                let merged = rmcp::model::ListRootsResult::new(roots.collect());
                let reply = |request| encoded(&Message::<&RawValue, (), _> { jsonrpc: JsonRpcVersion2_0, id: Some(request), method: None, params: None, result: Some(merged), error: None });
                self.upstream.send(decode::<&RawValue>(id).map_err(serde_path_to_error::Error::into_inner).and_then(reply));
            }
            (None, Some((line, _))) => self.upstream.forward([line.clone()]),
            (None, None) => self.decline(id, "client session closed before answering"),
        }
    }

    fn learn(&mut self, key: Key, asks: bool) {
        let Some(profile) = &mut self.profile else { return };
        if profile.askers.contains(&key) || (!asks && profile.quiet.contains(&key)) {
            return;
        }
        let held = Arc::make_mut(profile);
        if asks {
            held.quiet.remove(&key);
        }
        (if asks { &mut held.askers } else { &mut held.quiet }).insert(key);
        let learned = Arc::clone(profile);
        self.status.send_modify(|status| status.profile = Some(learned));
    }

    // --- [UPSTREAM_READS]

    fn on_upstream(&mut self, line: &Bytes) {
        self.clock.activity = Activity::Touched;
        let envelope = match decode::<Envelope<'_>>(line) {
            Ok(envelope) => envelope,
            Err(error) => return tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server line unreadable"),
        };
        match (envelope.id, envelope.method) {
            (Some(id), None) => self.on_response(line, id, envelope.result, envelope.error),
            (Some(id), Some(method)) => self.on_ask(line, id, method),
            (None, Some(method)) => self.on_notice(line, &method, envelope.params.as_ref()),
            (None, None) => {}
        }
    }

    fn on_oversized(&mut self, head: &[u8], tail: &[u8]) {
        let limit = self.policy.line;
        tracing::debug!(limit, "server message over the line limit, discarded");
        let request = match identity(head, tail) {
            (Some(id), None) => issued(&id),
            (Some(id), Some(_)) => {
                self.upstream.send(refusal(&id, &ErrorData::invalid_request(format!("request exceeds {limit} bytes, send less"), None)));
                None
            }
            (None, Some(_) | None) => None,
        };
        let error = ErrorData::internal_error(format!("server response exceeds {limit} bytes, ask for less output"), None);
        match request.and_then(|request| self.calls.take(request).map(|origin| (request, origin))) {
            Some((request, Origin::Session(call))) => {
                self.sessions.reply(call.session, refusal(&call.id, &error));
                self.settled(request, None);
            }
            Some((_, Origin::Discover)) => self.negotiated(Lifecycle::Discover, None, None),
            Some((_, Origin::Initialize)) => self.negotiated(Lifecycle::Initialize, None, None),
            Some((_, Origin::Internal | Origin::Probe)) | None => {}
        }
    }

    fn on_response(&mut self, line: &Bytes, id: &RawValue, result: Option<&RawValue>, error: Option<&RawValue>) {
        let Some((request, origin)) = issued(id.get().as_bytes()).and_then(|request| self.calls.take(request).map(|origin| (request, origin))) else { return };
        match origin {
            Origin::Session(Call { session, id: held, token: _, key, effect, capabilities: _, asked }) => {
                self.failures = 0;
                self.subscribers.applied(effect, result.is_some(), session);
                self.sessions.deliver(session, splice(line, [(id.get().as_bytes(), held)]));
                self.settled(request, (!asked).then_some(key));
            }
            Origin::Discover => self.negotiated(Lifecycle::Discover, result, error),
            Origin::Initialize => self.negotiated(Lifecycle::Initialize, result, error),
            Origin::Internal => {}
            Origin::Probe => {
                let during = BTreeSet::from_iter(self.calls.callers().take_while(|(caller, _)| *caller < request).map(|(_, call)| call.key.clone()));
                if let Some(profile) = self.profile.as_mut().filter(|profile| !during.is_subset(&profile.responsive)) {
                    Arc::make_mut(profile).responsive.extend(during);
                    let learned = Arc::clone(profile);
                    self.status.send_modify(|status| status.profile = Some(learned));
                }
            }
        }
    }

    fn on_ask(&mut self, line: &Bytes, id: &RawValue, method: Method) {
        if method.is_ping() {
            let pong = Message::<&RawValue, (), _> { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: None, params: None, result: Some(EmptyResult {}), error: None };
            return self.upstream.send(encoded(&pong));
        }
        let Some(quiet) = self.profile.as_ref().filter(|profile| profile.lifecycle.is_initialize()).map(|profile| &profile.quiet) else {
            return self.decline(id.get().as_bytes(), "a server of this protocol version returns an input-required result in place of a request");
        };
        let unknown = BTreeSet::from_iter(self.calls.callers().filter(|(_, call)| !quiet.contains(&call.key)).map(|(request, _)| request));
        let candidates = if unknown.is_empty() { BTreeSet::from_iter(self.calls.callers().map(|(request, _)| request)) } else { unknown };
        let asked = Bytes::copy_from_slice(id.get().as_bytes());
        let rooted = || {
            let declaring = self.sessions.joined.iter().filter(|(_, held)| held.initialized && held.client.as_ref().is_some_and(|client| declares(&client.capabilities, &method)));
            BTreeSet::from_iter(declaring.map(|(session, _)| *session))
        };
        match (candidates.first().copied(), candidates.len(), method.is_list_roots().then(rooted).filter(|rooted| !rooted.is_empty())) {
            (None, _, Some(rooted)) => {
                for session in &rooted {
                    self.sessions.deliver(*session, [line.clone()]);
                }
                self.calls.asked.insert(asked, (rooted, Some(Vec::new())));
            }
            (None, _, None) => self.decline(&asked, "no client session has a call in flight"),
            (Some(caller), 1, _) => self.ask(caller, asked, line.clone(), &method),
            (Some(_), _, _) => self.calls.held.push(Held { id: asked, line: line.clone(), method, candidates }),
        }
    }

    fn on_notice(&mut self, line: &Bytes, method: &Method, params: Option<&Params<'_>>) {
        let routed = |raw: Option<&RawValue>| raw.and_then(|raw| issued(raw.get().as_bytes())).and_then(|request| self.calls.pending.get(&request));
        let subscription = params.and_then(|params| params.meta.as_ref()).and_then(|meta| meta.subscription_id);
        if let (Some(subscription), Some(Origin::Session(call))) = (subscription, routed(subscription)) {
            return self.sessions.deliver(call.session, splice(line, [(subscription.get().as_bytes(), call.id.clone())]));
        }
        match method {
            Method::Progress => {
                let token = params.and_then(|params| params.progress_token);
                if let (Some(token), Some(Origin::Session(Call { session, id: _, token: Some(stated), key: _, effect: _, capabilities: _, asked: _ }))) = (token, routed(token)) {
                    self.sessions.deliver(*session, splice(line, [(token.get().as_bytes(), stated.clone())]));
                }
            }
            Method::Cancelled =>
                if let Some(id) = params.and_then(|params| params.request_id) {
                    self.calls.held.retain(|held| *held.id != *id.get().as_bytes());
                    for session in self.calls.asked.remove(id.get().as_bytes()).into_iter().flat_map(|(waiting, _)| waiting) {
                        self.sessions.deliver(session, [line.clone()]);
                    }
                },
            Method::ResourceUpdated =>
                for session in read::<String>(params.and_then(|params| params.uri)).and_then(|uri| self.subscribers.resources.get(&uri)).into_iter().flatten() {
                    self.sessions.deliver(*session, [line.clone()]);
                },
            Method::Logged =>
                if let Some(level) = read::<LoggingLevel>(params.and_then(|params| params.level)) {
                    let reading = |held: &&Session| held.level.is_some_and(|floor| floor as u8 <= level as u8) && held.out.capacity().saturating_mul(2) > held.out.max_capacity();
                    let dropped = self.sessions.joined.values().filter(reading).filter_map(|held| held.out.try_send(line.clone()).err());
                    dropped.for_each(|dropped| tracing::trace!({ EXCEPTION_MESSAGE } = %Report::new(&dropped), "server log message dropped"));
                },
            Method::Initialize
            | Method::Initialized
            | Method::Discover
            | Method::Ping
            | Method::CallTool
            | Method::GetPrompt
            | Method::Subscribe
            | Method::Unsubscribe
            | Method::Listen
            | Method::SetLevel
            | Method::CreateMessage
            | Method::Elicit
            | Method::ListRoots
            | Method::Other(_) =>
                if self.profile.as_ref().is_some_and(|profile| profile.lifecycle.is_initialize()) {
                    self.sessions.broadcast(line);
                },
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn declares(capabilities: &ClientCapabilities, method: &Method) -> bool {
    match method {
        Method::CreateMessage => capabilities.sampling.is_some(),
        Method::Elicit => capabilities.elicitation.is_some(),
        Method::ListRoots => capabilities.roots.is_some(),
        Method::Initialize
        | Method::Initialized
        | Method::Discover
        | Method::Ping
        | Method::Cancelled
        | Method::Progress
        | Method::CallTool
        | Method::GetPrompt
        | Method::Subscribe
        | Method::Unsubscribe
        | Method::ResourceUpdated
        | Method::Listen
        | Method::SetLevel
        | Method::Logged
        | Method::Other(_) => true,
    }
}

fn contextualized<'line>(line: &'line [u8], context: &[u8]) -> Option<(&'line [u8], Bytes)> {
    #[derive(Deserialize)]
    struct Located<'line> {
        #[serde(borrow)]
        params: Option<&'line RawValue>,
        #[serde(borrow, rename = "_meta")]
        meta: Option<&'line RawValue>,
    }
    let located = |text: &'line [u8]| decode::<Located<'line>>(text).inspect_err(|error| tracing::trace!({ EXCEPTION_MESSAGE } = %Report::new(error), "message params unlocated")).ok();
    let opened = |raw: &'line RawValue| {
        let text = raw.get().as_bytes();
        let filled = text.get(1..).is_some_and(|rest| rest.trim_ascii_start().first() != Some(&b'}'));
        text.starts_with(b"{").then(|| text.get(1..1).map(|at| (at, if filled { &b","[..] } else { &b""[..] }))).flatten()
    };
    let params = located(line)?.params;
    match (params, params.and_then(|params| located(params.get().as_bytes())?.meta)) {
        (_, Some(meta)) => opened(meta).map(|(at, separator)| (at, Bytes::from([context, separator].concat()))),
        (Some(params), None) => opened(params).map(|(at, separator)| (at, Bytes::from([&br#""_meta":{"#[..], context, b"}", separator].concat()))),
        (None, None) => line.iter().rposition(|byte| *byte == b'}').and_then(|end| line.get(end..end)).map(|at| (at, Bytes::from([&br#","params":{"_meta":{"#[..], context, b"}}"].concat()))),
    }
}

fn message(id: Option<u64>, method: &str, params: Option<impl Serialize>) -> Result<Bytes, serde_json::Error> {
    encoded(&Message::<u64, _, ()> { jsonrpc: JsonRpcVersion2_0, id, method: Some(method), params, result: None, error: None })
}

fn discover(client: &InitializeRequestParams, id: u64) -> Result<Bytes, serde_json::Error> {
    let InitializeRequestParams { client_info, capabilities, .. } = client;
    message(Some(id), DiscoverRequestMethod::VALUE, Some(BTreeMap::from([("_meta", RequestMetaObject::with_client_context(ProtocolVersion::LATEST, client_info.clone(), capabilities.clone()))])))
}
