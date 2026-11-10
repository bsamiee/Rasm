use core::time::Duration;
use std::collections::BTreeMap;
use std::collections::btree_map::Entry;
use std::error::Report;
use std::io;
use std::sync::Arc;

use futures::future;
use jiff::Timestamp;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use tokio::io::{AsyncRead, AsyncWrite};
use tokio::sync::watch;
use tokio_util::sync::CancellationToken;
use tokio_util::task::TaskTracker;
use tracing::Instrument as _;

use super::{History, Link, Phase, Policy, Proxy, Status};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Store<K>: Send + Sync + 'static {
    fn load(&self, key: &K) -> impl Future<Output = io::Result<Option<History>>> + Send;

    fn save(&self, key: &K, history: History) -> impl Future<Output = io::Result<()>> + Send;
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Pool<K, S> {
    members: watch::Sender<BTreeMap<K, Proxy>>,
    store: Arc<S>,
    tasks: TaskTracker,
}

impl<K, S> Pool<K, S>
where
    K: Clone + Ord + Send + Sync + 'static,
    S: Store<K>,
{
    pub fn new(store: S) -> Self { Self { members: watch::Sender::default(), store: Arc::new(store), tasks: TaskTracker::new() } }

    #[must_use]
    pub fn members(&self) -> watch::Ref<'_, BTreeMap<K, Proxy>> { self.members.borrow() }

    pub async fn join<F, A, R, W, C>(&self, key: &K, start: impl FnOnce() -> (Policy, F, tracing::Span)) -> Proxy
    where
        F: FnMut(CancellationToken) -> A + Send + 'static,
        A: Future<Output = io::Result<Link<R, W, C>>> + Send + 'static,
        R: AsyncRead + Send + Unpin + 'static,
        W: AsyncWrite + Send + Unpin + 'static,
        C: Future<Output = String> + Send + 'static,
    {
        if let Some(member) = self.members.borrow().get(key) {
            return member.clone();
        }
        let history = match self.store.load(key).await {
            Ok(history) => history.unwrap_or_default(),
            Err(error) => {
                tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "stored server history unreadable");
                History::default()
            }
        };
        let (policy, connect, span) = start();
        let (mut member, actor) = Proxy::new(policy, history, connect);
        let started = self.members.send_if_modified(|members| match members.entry(key.clone()) {
            Entry::Vacant(vacant) => {
                vacant.insert(member.clone());
                true
            }
            Entry::Occupied(held) => {
                member = held.get().clone();
                false
            }
        });
        if started {
            drop(self.tasks.spawn(actor.instrument(span.clone())));
            drop(self.tasks.spawn(kept(self.members.clone(), Arc::clone(&self.store), key.clone(), member.clone()).instrument(span)));
        }
        member
    }

    pub async fn remove(&self, key: &K) {
        let mut removed = None;
        self.members.send_if_modified(|members| {
            removed = members.remove(key);
            removed.is_some()
        });
        if let Some(member) = removed {
            member.stop().await;
        }
    }

    pub async fn unused(&self) {
        let quiet = |members: &BTreeMap<K, Proxy>| {
            members.values().all(|member| {
                let status = member.status.borrow();
                status.sessions == 0 && status.phase > Phase::Up
            })
        };
        if let Err(error) = self.members.subscribe().wait_for(quiet).await {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "pool closed");
        }
    }

    pub async fn drain(&self, settle: Duration) {
        let members = Vec::from_iter(self.members.borrow().values().cloned());
        future::join_all(members.iter().map(|member| member.drain(settle))).await;
    }

    pub async fn stop(&self) {
        let members = Vec::from_iter(self.members.borrow().values().cloned());
        future::join_all(members.iter().map(Proxy::stop)).await;
        self.tasks.close();
        self.tasks.wait().await;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn kept<K, S>(members: watch::Sender<BTreeMap<K, Proxy>>, store: Arc<S>, key: K, member: Proxy)
where
    K: Ord,
    S: Store<K>,
{
    let mut status = member.status();
    let mut written = (status.borrow().catalog.clone(), status.borrow().failures);
    loop {
        let Status { phase, sessions, starts: _, failures, delay, cause, catalog } = status.borrow_and_update().clone();
        let learned = catalog.as_ref().is_some_and(|catalog| written.0.as_ref().is_none_or(|held| !Arc::ptr_eq(held, catalog)));
        if learned || written.1 != failures {
            let down = failures > 0 && phase > Phase::Up;
            let until = Timestamp::now().checked_add(delay).inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "retry time past the calendar")).ok();
            let history = History { catalog: catalog.as_deref().cloned(), failures: if down { failures } else { 0 }, cause: cause.filter(|_| down), until: until.filter(|_| down) };
            if let Err(error) = store.save(&key, history).await {
                tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server history not stored");
            }
            written = (catalog, failures);
        }
        let left = |members: &mut BTreeMap<K, Proxy>, unused: bool| {
            let own = members.get(&key).is_some_and(|held| held.events.same_channel(&member.events));
            own && (!unused || member.status.borrow().sessions == 0) && members.remove(&key).is_some()
        };
        if sessions == 0 && phase >= Phase::Idle && members.send_if_modified(|members| left(members, true)) {
            break member.stop().await;
        }
        members.send_modify(|_| {});
        if let Err(error) = status.changed().await {
            tracing::trace!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server proxy ended");
            members.send_if_modified(|members| left(members, false));
            break;
        }
    }
}
