use core::mem::DropGuard;
use core::num::NonZero;
use core::time::Duration;
use std::error::Report;
use std::panic;
use std::sync::{Arc, OnceLock};

use backon::{ExponentialBuilder, Retryable as _};
use enum_map::EnumMap;
use futures::FutureExt as _;
use futures::future::{self, BoxFuture, join_all};
use jiff::Timestamp;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use tokio::sync::watch;
use tokio::time::{self, Instant};
use tokio_util::sync::CancellationToken;
use tokio_util::task::TaskTracker;

use crate::ledger::{Entry, Ledger, Phase, Tenure};
use crate::stores::{Change, Create, Stores, Wait};
use crate::{Kind, Label, ProvisioningError, Refusal, Removal, SessionName, StoreId};

// --- [TYPES] ---------------------------------------------------------------------------

type Erased = dyn Fn(StoreId, Change) -> BoxFuture<'static, Result<(), ProvisioningError>> + Send + Sync;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, derive_more::Debug)]
pub struct Policy {
    pub grace: Duration,
    pub lease: Duration,
    pub reclaim: ExponentialBuilder,
    pub disconnect: ExponentialBuilder,
    pub ready: usize,
    pub templates: NonZero<usize>,
    #[debug(skip)]
    pub clock: fn() -> Timestamp,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(derive_more::Debug)]
pub struct Registry {
    policy: Policy,
    ledger: watch::Sender<Ledger>,
    #[debug(skip)]
    servers: EnumMap<Kind, OnceLock<Arc<Erased>>>,
    tasks: TaskTracker,
    stopping: CancellationToken,
}

impl Registry {
    // --- [LIFETIME]

    #[must_use]
    pub fn new(policy: Policy, stopping: CancellationToken) -> Arc<Self> {
        Arc::new(Self { policy, ledger: watch::Sender::default(), servers: EnumMap::default(), tasks: TaskTracker::new(), stopping })
    }

    pub async fn serve<S>(self: &Arc<Self>, server: S) -> Result<Stores<S>, ProvisioningError>
    where S: Create + 'static {
        let (kind, server) = (server.kind(), Arc::new(server));
        self.servers[kind].get_or_init(|| {
            let server = Arc::clone(&server);
            Arc::new(move |id, change| {
                let server = Arc::clone(&server);
                let changed = async move {
                    match change {
                        Change::Label(label) => server.label(id, &label).await,
                        Change::Remove(removal) => server.remove(id, removal).await,
                    }
                };
                changed.boxed()
            })
        });
        let (limit, listed) = future::join(server.limit(), server.stores()).await;
        let (limit, mut listed, now) = (limit?, listed?, Instant::now());
        listed.sort_by_key(|(_, label)| !matches!(label, Some(Label::Template { digest: _ })));
        let strays = self.edit(|ledger| {
            ledger.bound(kind, limit);
            let mut recovered = |(id, label): (StoreId, Option<Label>)| label.is_some_and(|label| ledger.recover(kind, id, label, now.checked_add(self.policy.lease), now));
            listed.into_iter().filter_map(|(id, label)| (!recovered((id, label))).then_some(id)).collect::<Vec<_>>()
        });
        join_all(strays.into_iter().map(async |id| {
            if let Err(error) = self.dropped(kind, id, Removal::Idle).await {
                tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), %id, "stray store kept");
            }
        }))
        .await;
        Ok(Stores { registry: Arc::clone(self), server, wait: Wait::Refused })
    }

    pub async fn run(self: &Arc<Self>) {
        let mut ledger = self.ledger.subscribe();
        let expiring = async {
            loop {
                let next = ledger.borrow_and_update().next();
                let changed = match next {
                    Some(deadline) => time::timeout_at(deadline, ledger.changed()).await,
                    None => Ok(ledger.changed().await),
                };
                match changed {
                    Ok(Ok(())) => {}
                    Ok(Err(watch::error::RecvError { .. })) => break,
                    Err(time::error::Elapsed { .. }) => self.expire(),
                }
            }
        };
        self.stopping.run_until_cancelled(expiring).await;
        self.tasks.close();
        self.tasks.wait().await;
    }

    // --- [SESSIONS]

    pub fn hold(self: &Arc<Self>, session: SessionName) -> (Duration, impl Future<Output = ()> + Send + 'static) {
        self.ledger.send_modify(|ledger| ledger.hold(&session));
        let (grace, stopping) = (self.policy.grace, self.stopping.clone());
        let held = DropGuard::new((Arc::clone(self), session), move |(registry, session): (Arc<Self>, SessionName)| {
            registry.ledger.send_modify(|ledger| ledger.release(&session, Instant::now().checked_add(grace)));
        });
        (grace, async move {
            let _held = held;
            stopping.cancelled_owned().await;
        })
    }

    pub fn renew(&self, session: &SessionName) -> Duration {
        let (now, grace, lease) = (Instant::now(), self.policy.grace, self.policy.lease);
        self.edit(|ledger| match ledger.touch(session, now.checked_add(lease)) {
            Tenure::Called { until: _ } => lease,
            Tenure::Held { holds: _ } => grace,
            Tenure::Released { until } => until.map_or(grace, |until| until.saturating_duration_since(now)),
        })
    }

    pub async fn end(self: &Arc<Self>, session: &SessionName) -> Result<(), ProvisioningError> {
        let (registry, stores) = (Arc::clone(self), self.edit(|ledger| ledger.end(session)));
        self.owned(async move {
            registry.reclaim(stores).await;
            Ok(())
        })
        .await
    }

    // --- [STORES]

    pub async fn keep(self: &Arc<Self>, id: StoreId) -> Result<(), ProvisioningError> {
        let held = self.located(id).and_then(|(kind, server, entry)| match (entry.phase, entry.session) {
            (Phase::Held | Phase::Kept { until: _ }, Some(session)) => Some((kind, server, session)),
            (Phase::Pending { claim: _ } | Phase::Build { digest: _ } | Phase::Ready { digest: _ } | Phase::Leaving | Phase::Held | Phase::Kept { until: _ }, _) => None,
        });
        let ((kind, server, session), registry) = (held.ok_or(ProvisioningError::Store { id, refusal: Refusal::Missing })?, Arc::clone(self));
        self.owned(async move {
            server(id, Change::Label(Label::Kept { session })).await?;
            registry.edit(|ledger| ledger.keep(kind, id, Instant::now().checked_add(registry.policy.lease))).ok_or(ProvisioningError::Store { id, refusal: Refusal::Missing })
        })
        .await
    }

    pub async fn remove(self: &Arc<Self>, id: StoreId) -> Result<(), ProvisioningError> {
        let Some(kind) = self.ledger.borrow().locate(id).map(|(kind, _)| kind) else { return Ok(()) };
        let registry = Arc::clone(self);
        self.owned(async move {
            let idle = || registry.dropped(kind, id, Removal::Idle);
            idle.retry(registry.policy.disconnect).when(|error| matches!(error, ProvisioningError::Store { id: _, refusal: Refusal::InUse })).await
        })
        .await
    }

    pub async fn seal(self: &Arc<Self>, id: StoreId) -> Result<(), ProvisioningError> {
        let build = self.located(id).and_then(|(kind, server, entry)| match entry.phase {
            Phase::Build { digest } => Some((kind, server, digest)),
            Phase::Pending { claim: _ } | Phase::Held | Phase::Kept { until: _ } | Phase::Ready { digest: _ } | Phase::Leaving => None,
        });
        let ((kind, server, digest), registry) = (build.ok_or(ProvisioningError::Store { id, refusal: Refusal::Missing })?, Arc::clone(self));
        self.owned(async move {
            server(id, Change::Label(Label::Template { digest })).await?;
            let stale = registry.edit(|ledger| ledger.seal(kind, id, Instant::now(), registry.policy.templates)).ok_or(ProvisioningError::Store { id, refusal: Refusal::Missing })?;
            registry.spawn(Arc::clone(&registry).linger(stale.into_iter().map(|gone| (kind, gone)).collect()));
            Ok(())
        })
        .await
    }

    #[must_use]
    pub fn holds(&self, kind: Kind, id: StoreId) -> bool {
        self.ledger.borrow().locate(id).is_some_and(|(held, entry)| held == kind && (entry.phase.is_held() || entry.phase.is_kept() || entry.phase.is_build()))
    }

    // --- [TASKS]

    pub(crate) const fn policy(&self) -> &Policy { &self.policy }

    pub(crate) fn watch(&self) -> watch::Receiver<Ledger> { self.ledger.subscribe() }

    pub(crate) fn spawn(&self, work: impl Future<Output = ()> + Send + 'static) { drop(self.tasks.spawn(work)); }

    pub(crate) fn issue(&self) -> Result<StoreId, ProvisioningError> {
        let at = (self.policy.clock)();
        StoreId::new(at).ok_or(ProvisioningError::Clock { at })
    }

    pub(crate) fn edit<T>(&self, change: impl FnOnce(&mut Ledger) -> T) -> T
    where T: Default {
        let mut outcome = T::default();
        self.ledger.send_modify(|ledger| outcome = change(ledger));
        outcome
    }

    pub(crate) async fn owned<T>(&self, work: impl Future<Output = Result<T, ProvisioningError>> + Send + 'static) -> Result<T, ProvisioningError>
    where T: Send + 'static {
        if self.tasks.is_closed() {
            return Err(ProvisioningError::Stopping);
        }
        match self.tasks.spawn(work).await {
            Ok(outcome) => outcome,
            Err(error) => match error.try_into_panic() {
                Ok(payload) => panic::resume_unwind(payload),
                Err(source) => Err(ProvisioningError::Interrupted { source }),
            },
        }
    }

    pub(crate) fn settle<T>(self: &Arc<Self>, kind: Kind, id: StoreId, phase: Phase, made: Result<T, ProvisioningError>) -> Result<T, ProvisioningError> {
        let settled = made.and_then(|access| self.edit(|ledger| ledger.commit(kind, id, phase)).then_some(access).ok_or(ProvisioningError::Store { id, refusal: Refusal::Missing }));
        settled.inspect_err(|_| self.spawn(Arc::clone(self).linger(vec![(kind, id)])))
    }

    pub(crate) async fn linger(self: Arc<Self>, stores: Vec<(Kind, StoreId)>) {
        let removed = join_all(stores.into_iter().map(async |(kind, id)| {
            let idle = (|| self.dropped(kind, id, Removal::Idle)).retry(self.policy.reclaim).await;
            let forced = match idle {
                Ok(()) => Ok(()),
                Err(error) => {
                    tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), %id, "store removed by force");
                    self.dropped(kind, id, Removal::Forced).await
                }
            };
            if let Err(error) = forced {
                tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(&error), %id, "store not removed");
            }
        }));
        self.stopping.run_until_cancelled(removed).await;
    }

    fn located(&self, id: StoreId) -> Option<(Kind, Arc<Erased>, Entry)> {
        let (kind, entry) = self.ledger.borrow().locate(id).map(|(kind, entry)| (kind, entry.clone()))?;
        self.servers[kind].get().map(|server| (kind, Arc::clone(server), entry))
    }

    async fn dropped(&self, kind: Kind, id: StoreId, removal: Removal) -> Result<(), ProvisioningError> {
        if let Some(server) = self.servers[kind].get() {
            server(id, Change::Remove(removal)).await?;
        }
        self.ledger.send_modify(|ledger| ledger.forget(kind, id));
        Ok(())
    }

    async fn reclaim(self: &Arc<Self>, stores: Vec<(Kind, StoreId)>) {
        let attempts = join_all(stores.into_iter().map(async |(kind, id)| {
            self.dropped(kind, id, Removal::Idle).await.inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), %id, "store outlives its session")).err().map(|_| (kind, id))
        }));
        let lingering = Vec::from_iter(attempts.await.into_iter().flatten());
        if !lingering.is_empty() {
            self.spawn(Arc::clone(self).linger(lingering));
        }
    }

    fn expire(self: &Arc<Self>) {
        let (registry, ended) = (Arc::clone(self), self.edit(|ledger| ledger.expire(Instant::now())));
        self.spawn(async move { registry.reclaim(ended).await });
    }
}
