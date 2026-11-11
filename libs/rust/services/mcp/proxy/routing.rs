use core::sync::atomic::{AtomicUsize, Ordering};
use std::collections::{BTreeMap, BTreeSet, VecDeque};
use std::error::Report;
use std::sync::Arc;

use bytes::Bytes;
use futures::FutureExt as _;
use futures::future::BoxFuture;
use futures::stream::FuturesUnordered;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::model::JsonRpcVersion2_0;
use serde::{Deserialize, Serialize};
use serde_json::value::RawValue;
use tokio::sync::mpsc;
use tokio_util::sync::CancellationToken;
use tokio_util::time::delay_queue;

use super::Key;
use super::jsonrpc::{Message, Severity, decode, encoded};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, derive_more::IsVariant)]
pub(super) enum Effect {
    Plain,
    Subscribe(String),
    Unsubscribe(String),
    Level(Severity),
    Listen,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub(super) struct Call {
    pub(super) session: u64,
    pub(super) id: Bytes,
    pub(super) token: Option<Bytes>,
    pub(super) key: Key,
    pub(super) effect: Effect,
    pub(super) asked: bool,
}

#[derive(Debug)]
pub(super) enum Origin {
    Session(Call),
    Discover,
    Initialize,
    Internal,
}

#[derive(Debug)]
pub(super) struct Session {
    pub(super) out: mpsc::Sender<Bytes>,
    pub(super) cut: CancellationToken,
    pub(super) initialized: bool,
    pub(super) level: Option<Severity>,
}

#[derive(Debug)]
pub(super) struct Held {
    pub(super) id: Bytes,
    pub(super) line: Bytes,
    pub(super) candidates: BTreeSet<u64>,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(derive_more::Debug, Default)]
pub(super) struct Sessions {
    pub(super) joined: BTreeMap<u64, Session>,
    #[debug(skip)]
    pub(super) left: FuturesUnordered<BoxFuture<'static, u64>>,
}

impl Sessions {
    pub(super) fn join(&mut self, session: u64, out: mpsc::Sender<Bytes>, cut: CancellationToken) {
        let gone = out.clone();
        self.left.push(async move { gone.closed().map(|()| session).await }.boxed());
        self.joined.insert(session, Session { out, cut, initialized: false, level: None });
    }

    pub(super) fn deliver(&self, session: u64, chunks: impl IntoIterator<Item = Bytes>) {
        let Some(held) = self.joined.get(&session) else { return };
        match chunks.into_iter().try_for_each(|chunk| held.out.try_send(chunk)) {
            Ok(()) | Err(mpsc::error::TrySendError::Closed(_)) => {}
            Err(mpsc::error::TrySendError::Full(_)) => {
                tracing::debug!(session, queue = held.out.max_capacity(), "session stopped reading, disconnected");
                held.cut.cancel();
            }
        }
    }

    pub(super) fn broadcast(&self, line: &Bytes) {
        for (session, _) in self.joined.iter().filter(|(_, held)| held.initialized) {
            self.deliver(*session, [line.clone()]);
        }
    }

    pub(super) fn reply(&self, session: u64, line: Result<Bytes, serde_json::Error>) {
        match line {
            Ok(line) => self.deliver(session, [line]),
            Err(error) => tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "message not encoded"),
        }
    }

    pub(super) fn answer(&self, session: u64, id: &RawValue, result: &impl Serialize) {
        self.reply(session, encoded(&Message::<&RawValue, (), _> { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: None, params: None, result: Some(result), error: None }));
    }
}

#[derive(Debug)]
pub(super) struct Calls {
    pub(super) pending: BTreeMap<u64, Origin>,
    named: BTreeMap<(u64, Bytes), u64>,
    next: u64,
    in_flight: Arc<AtomicUsize>,
    pub(super) asked: BTreeMap<Bytes, u64>,
    pub(super) held: Vec<Held>,
    pub(super) writer: Option<(u64, delay_queue::Key)>,
    pub(super) backlog: VecDeque<(u64, Bytes)>,
}

impl Calls {
    pub(super) const fn new(in_flight: Arc<AtomicUsize>) -> Self {
        Self { pending: BTreeMap::new(), named: BTreeMap::new(), next: 1, in_flight, asked: BTreeMap::new(), held: Vec::new(), writer: None, backlog: VecDeque::new() }
    }

    pub(super) fn waiting(&self) -> usize { self.in_flight.load(Ordering::Relaxed) }

    pub(super) fn issue(&mut self, origin: Origin) -> u64 {
        let id = self.next;
        self.next = self.next.saturating_add(1);
        if let Origin::Session(call) = &origin {
            self.in_flight.fetch_add(1, Ordering::Relaxed);
            self.named.insert((call.session, call.id.clone()), id);
        }
        self.pending.insert(id, origin);
        id
    }

    pub(super) fn take(&mut self, request: u64) -> Option<Origin> {
        let origin = self.pending.remove(&request)?;
        if let Origin::Session(call) = &origin {
            self.in_flight.fetch_sub(1, Ordering::Relaxed);
            self.named.remove(&(call.session, call.id.clone()));
        }
        Some(origin)
    }

    pub(super) fn named(&self, session: u64, id: &[u8]) -> Option<u64> { self.named.get(&(session, Bytes::copy_from_slice(id))).copied() }

    pub(super) fn of(&self, session: u64) -> Vec<u64> { self.named.range((session, Bytes::new())..).take_while(|((owner, _), _)| *owner == session).map(|(_, request)| *request).collect() }

    pub(super) fn reset(&mut self) -> impl Iterator<Item = Origin> + use<> {
        self.named.clear();
        self.asked.clear();
        self.held.clear();
        self.in_flight.store(0, Ordering::Relaxed);
        core::mem::take(&mut self.pending).into_values()
    }

    pub(super) fn callers(&self) -> impl Iterator<Item = (u64, &Call)> {
        self.pending.iter().filter_map(|(request, origin)| match origin {
            Origin::Session(call) => Some((*request, call)),
            Origin::Discover | Origin::Initialize | Origin::Internal => None,
        })
    }
}

#[derive(Debug, Default)]
pub(super) struct Subscriptions {
    pub(super) resources: BTreeMap<String, BTreeSet<u64>>,
    pub(super) level: Option<Severity>,
}

impl Subscriptions {
    pub(super) fn local(&mut self, effect: &Effect, session: u64) -> bool {
        match effect {
            Effect::Subscribe(uri) => self.resources.get_mut(uri).map(|held| held.insert(session)).is_some(),
            Effect::Unsubscribe(uri) => self.resources.get_mut(uri).filter(|held| held.len() > 1 || !held.contains(&session)).map(|held| held.remove(&session)).is_some(),
            Effect::Level(level) => self.level.is_some_and(|held| held <= *level),
            Effect::Plain | Effect::Listen => false,
        }
    }

    pub(super) fn applied(&mut self, effect: Effect, answered: bool, session: u64) {
        match (effect, answered) {
            (Effect::Subscribe(uri), true) => drop(self.resources.entry(uri).or_default().insert(session)),
            (Effect::Unsubscribe(uri), _) => drop(self.resources.extract_if(.., |held, sessions| *held == uri && sessions.remove(&session) && sessions.is_empty()).count()),
            (Effect::Level(level), true) => self.level = Some(self.level.map_or(level, |held| held.min(level))),
            (Effect::Subscribe(_) | Effect::Level(_), false) | (Effect::Plain | Effect::Listen, _) => {}
        }
    }

    pub(super) fn leave(&mut self, session: u64) -> Vec<String> { self.resources.extract_if(.., |_, held| held.remove(&session) && held.is_empty()).map(|(uri, _)| uri).collect() }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(super) fn issued(raw: &[u8]) -> Option<u64> {
    match decode::<u64>(raw) {
        Ok(request) => Some(request),
        Err(error) => {
            tracing::trace!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server named an id this proxy never issued");
            None
        }
    }
}

pub(super) fn read<'de, T>(raw: Option<&'de RawValue>) -> Option<T>
where T: Deserialize<'de> {
    match decode::<T>(raw?.get().as_bytes()) {
        Ok(value) => Some(value),
        Err(error) => {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "message parameter unreadable");
            None
        }
    }
}

pub(super) fn splice(line: &Bytes, first: (&[u8], Bytes), second: Option<(&[u8], Bytes)>) -> impl Iterator<Item = Bytes> + use<> {
    let span = |part: &[u8]| line.subslice_range(part).map(|range| (range.start, range.end));
    let whole = || [Some(line.clone()), None, None, None, None];
    let chunks = match (span(first.0), second.as_ref().map(|(part, _)| span(part))) {
        (Some((start, end)), None) => [Some(line.slice(..start)), Some(first.1), Some(line.slice(end..)), None, None],
        (Some(one), Some(Some(two))) => {
            let (replacing, replaced) = second.map_or_else(|| (first.1.clone(), first.1.clone()), |(_, replacement)| (first.1.clone(), replacement));
            let [(start, end, earlier), (next, last, later)] =
                if one.0 <= two.0 { [(one.0, one.1, replacing), (two.0, two.1, replaced)] } else { [(two.0, two.1, replaced), (one.0, one.1, replacing)] };
            if end <= next { [Some(line.slice(..start)), Some(earlier), Some(line.slice(end..next)), Some(later), Some(line.slice(last..))] } else { whole() }
        }
        (None, _) | (_, Some(None)) => whole(),
    };
    chunks.into_iter().flatten()
}
