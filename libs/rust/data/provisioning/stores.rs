use std::error::Report;
use std::sync::Arc;

use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use secrecy::SecretString;
use tokio::time::error::Elapsed;
use tokio::time::{self, Instant};

use crate::ledger::Phase;
use crate::registry::Registry;
use crate::{Claim, Digest, Kind, Label, ProvisioningError, Removal, SessionName, StoreId};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Server: Send + Sync {
    fn kind(&self) -> Kind;

    fn limit(&self) -> impl Future<Output = Result<Option<u32>, ProvisioningError>> + Send;

    fn stores(&self) -> impl Future<Output = Result<Vec<(StoreId, Option<Label>)>, ProvisioningError>> + Send;

    fn label(&self, id: StoreId, label: &Label) -> impl Future<Output = Result<(), ProvisioningError>> + Send;

    fn remove(&self, id: StoreId, removal: Removal) -> impl Future<Output = Result<(), ProvisioningError>> + Send;
}

pub trait Create: Server {
    type Access: Send + 'static;

    fn create(&self, id: StoreId, label: &Label) -> impl Future<Output = Result<Self::Access, ProvisioningError>> + Send;
}

pub trait Template: Create {
    fn copy(&self, id: StoreId, template: StoreId, label: &Label) -> impl Future<Output = Result<(), ProvisioningError>> + Send;

    fn grant(&self, id: StoreId, template: StoreId, label: &Label) -> impl Future<Output = Result<Self::Access, ProvisioningError>> + Send;
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub(crate) enum Change {
    Label(Label),
    Remove(Removal),
}

#[derive(Debug, Clone)]
pub struct Login {
    pub id: StoreId,
    pub password: SecretString,
}

impl Login {
    #[cfg(any(feature = "postgres", feature = "clickhouse", feature = "cache"))]
    pub(crate) fn new(id: StoreId) -> Self { Self { id, password: SecretString::from(uuid::Uuid::new_v4().simple().to_string()) } }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Stores<S> {
    pub(crate) registry: Arc<Registry>,
    pub(crate) server: Arc<S>,
    pub(crate) limit: Option<u32>,
    pub(crate) wait: Option<Instant>,
}

impl<S> Stores<S>
where S: Create + 'static
{
    #[must_use]
    pub fn waiting(&self, until: Option<Instant>) -> Self { Self { registry: Arc::clone(&self.registry), server: Arc::clone(&self.server), limit: self.limit, wait: until } }

    pub async fn create(&self, session: SessionName) -> Result<S::Access, ProvisioningError> { self.made(session, None).await }

    pub async fn open(&self, session: SessionName, digest: Digest) -> Result<S::Access, ProvisioningError>
    where S: Template {
        self.made(session, Some(digest)).await
    }

    pub async fn clone_template(&self, session: SessionName, digest: Digest) -> Result<S::Access, ProvisioningError>
    where S: Template {
        loop {
            if let Some(access) = self.vacancy(self.cloned(session.clone(), digest.clone()).await).await? {
                break Ok(access);
            }
        }
    }

    async fn cloned(&self, session: SessionName, digest: Digest) -> Result<S::Access, ProvisioningError>
    where S: Template {
        let (registry, server, kind) = (Arc::clone(&self.registry), Arc::clone(&self.server), self.server.kind());
        registry.reached(|ledger| !ledger.building(kind, &digest)).await?;
        self.registry
            .owned(async move {
                let (id, now) = (registry.issue()?, Instant::now());
                let until = now.checked_add(registry.policy.lease);
                let taken = registry.edit(|ledger| {
                    ledger.take(kind, &digest, &session, until, now).map(|(template, ready)| match ready {
                        Some(ready) => Ok((template, ready, false)),
                        None => ledger.admit(kind, id, Some((&session, until)), None).map(|()| (template, id, true)),
                    })
                });
                let (template, id, copied) = taken.ok_or_else(|| ProvisioningError::Template { digest: digest.clone(), claim: Claim::Unsealed })??;
                let granted = async {
                    if copied {
                        server.copy(id, template, &Label::Ready { digest: digest.clone() }).await?;
                    }
                    server.grant(id, template, &Label::Held { session }).await
                };
                let access = registry.settle(kind, id, Phase::Held, granted.await);
                registry.spawn(restock(Arc::clone(&registry), server, digest));
                access
            })
            .await
    }

    async fn made(&self, session: SessionName, digest: Option<Digest>) -> Result<S::Access, ProvisioningError> {
        let kind = self.server.kind();
        loop {
            let (registry, server, session, digest) = (Arc::clone(&self.registry), Arc::clone(&self.server), session.clone(), digest.clone());
            let made = self.registry.owned(async move {
                let (id, until) = (registry.issue()?, Instant::now().checked_add(registry.policy.lease));
                registry.edit(|ledger| ledger.admit(kind, id, Some((&session, until)), digest.as_ref()).err()).map_or(Ok(()), Err)?;
                let (label, phase) = match digest {
                    Some(digest) => (Label::Build { session, digest: digest.clone() }, Phase::Build { digest }),
                    None => (Label::Held { session }, Phase::Held),
                };
                registry.settle(kind, id, phase, server.create(id, &label).await)
            });
            if let Some(access) = self.vacancy(made.await).await? {
                break Ok(access);
            }
        }
    }

    async fn vacancy<T>(&self, made: Result<T, ProvisioningError>) -> Result<Option<T>, ProvisioningError> {
        let (Err(ProvisioningError::Exhausted { kind: _ }), Some(_), Some(until)) = (&made, self.limit, self.wait) else { return made.map(Some) };
        match time::timeout_at(until, self.registry.reached(|ledger| ledger.vacant(self.server.kind()))).await {
            Ok(vacant) => vacant.map(|()| None),
            Err(Elapsed { .. }) => made.map(Some),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RECORDS]

#[cfg(any(feature = "postgres", feature = "bucket", feature = "clickhouse", feature = "cache"))]
pub(crate) fn named(name: &str) -> Option<StoreId> { name.parse().inspect_err(|error| tracing::trace!({ EXCEPTION_MESSAGE } = %Report::new(error), name, "name holds no store id")).ok() }

#[cfg(any(feature = "postgres", feature = "bucket", feature = "clickhouse", feature = "cache"))]
pub(crate) fn encoded(id: StoreId, record: &impl serde::Serialize) -> Result<String, ProvisioningError> { serde_json::to_string(record).map_err(|source| ProvisioningError::Encode { id, source }) }

#[cfg(any(feature = "postgres", feature = "bucket", feature = "clickhouse", feature = "cache"))]
pub(crate) fn decoded<T>(id: StoreId, record: &[u8]) -> Option<T>
where T: serde::de::DeserializeOwned {
    let mut decoder = serde_json::Deserializer::from_slice(record);
    let read = serde_path_to_error::deserialize::<_, T>(&mut decoder)
        .and_then(|decoded| decoder.end().map(|()| decoded).map_err(|source| serde_path_to_error::Error::new(serde_path_to_error::Track::new().path(), source)));
    read.inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), %id, "store record unreadable")).ok()
}

// --- [POOL]

async fn restock<S>(registry: Arc<Registry>, server: Arc<S>, digest: Digest)
where S: Template + 'static {
    let kind = server.kind();
    let stocked: Result<(), ProvisioningError> = async {
        let id = registry.issue()?;
        let Some((template, displaced)) = registry.edit(|ledger| ledger.stock(kind, &digest, id, registry.policy.ready).transpose()).transpose()? else { return Ok(()) };
        if let Some(gone) = displaced {
            registry.spawn(Arc::clone(&registry).linger(vec![(kind, gone)]));
        }
        let copied = server.copy(id, template, &Label::Ready { digest: digest.clone() }).await;
        registry.settle(kind, id, Phase::Ready { digest }, copied)
    }
    .await;
    if let Err(error) = stocked {
        tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), %kind, "ready clone not stocked");
    }
}
