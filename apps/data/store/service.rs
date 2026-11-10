use core::future::ready;
use core::num::NonZero;
use core::time::Duration;
use std::error::Report;
use std::sync::Arc;

use axum::Router;
use futures::stream::{self, BoxStream};
use futures::{FutureExt as _, StreamExt as _};
use rasm_http::Service;
use rasm_provisioning::{Bucket, Cache, Claim, Clickhouse, Create, Digest, Granted, Kind, Login, Postgres, ProvisioningError, Refusal, Registry, SessionName, StoreId, StoreIdError, Stores};
use rasm_store_proto::store_service_server::{StoreService, StoreServiceServer};
use rasm_store_proto::{BucketStore, CacheStore, ClickhouseStore, PostgresStore, SessionLease, SessionRequest, StoreRequest, TemplateRequest};
use secrecy::ExposeSecret as _;
use tokio::time::Instant;
use tokio_util::sync::CancellationToken;
use tonic::codec::CompressionEncoding;
use tonic::service::Routes;
use tonic::{Code, Request, Response, Status};
use tonic_health::ServingStatus;
use tonic_health::server::HealthReporter;

use crate::StoreError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub(crate) struct Published {
    pub(crate) host: String,
    pub(crate) port: u16,
}

#[derive(Debug, Clone)]
pub(crate) struct Storage {
    pub(crate) endpoint: String,
    pub(crate) region: String,
    pub(crate) credentials: i32,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub(crate) struct Store {
    pub(crate) registry: Arc<Registry>,
    pub(crate) stopping: CancellationToken,
    pub(crate) health: HealthReporter,
    pub(crate) standard: Routes,
    pub(crate) postgres: Option<(Stores<Postgres>, (Published, NonZero<u32>))>,
    pub(crate) bucket: Option<(Stores<Bucket>, Storage)>,
    pub(crate) clickhouse: Option<(Stores<Clickhouse>, Published)>,
    pub(crate) cache: Option<(Stores<Cache>, Published)>,
}

impl Service for Store {
    type Error = StoreError;

    fn router(self: Arc<Self>, stopping: CancellationToken) -> Router {
        drop(stopping);
        let served = StoreServiceServer::from_arc(Arc::clone(&self)).accept_compressed(CompressionEncoding::Gzip).send_compressed(CompressionEncoding::Gzip);
        self.standard.clone().add_service(served).into_axum_router()
    }

    async fn lifetime(&self, stopping: CancellationToken) -> Result<(), StoreError> {
        drop(stopping);
        self.registry.run().await;
        Ok(())
    }

    async fn drain(&self) -> Result<(), StoreError> {
        self.health.set_service_status("", ServingStatus::NotServing).await;
        self.health.set_not_serving::<StoreServiceServer<Self>>().await;
        self.stopping.cancel();
        Ok(())
    }
}

#[tonic::async_trait]
impl StoreService for Store {
    type HoldSessionStream = BoxStream<'static, Result<SessionLease, Status>>;

    // --- [SESSIONS]

    async fn hold_session(&self, request: Request<SessionRequest>) -> Result<Response<Self::HoldSessionStream>, Status> {
        let (grace, held) = self.registry.hold(session(request.into_inner())?);
        let ended = held.map(|()| Err(Status::unavailable("store service is stopping")));
        Ok(Response::new(stream::once(ready(Ok(lease(grace)))).chain(stream::once(ended)).boxed()))
    }

    async fn renew_session(&self, request: Request<SessionRequest>) -> Result<Response<SessionLease>, Status> { Ok(Response::new(lease(self.registry.renew(&session(request.into_inner())?)))) }

    async fn end_session(&self, request: Request<SessionRequest>) -> Result<Response<()>, Status> {
        self.registry.end(&session(request.into_inner())?).await.map(Response::new).map_err(|error| status(&error))
    }

    // --- [STORES]

    async fn create_postgres(&self, request: Request<SessionRequest>) -> Result<Response<PostgresStore>, Status> {
        let (stores, address, name) = requested(self.postgres.as_ref(), Kind::Postgres, request, session)?;
        stores.create(name).await.map(|login| Response::new(postgres(address, &login))).map_err(|error| status(&error))
    }

    async fn create_bucket(&self, request: Request<SessionRequest>) -> Result<Response<BucketStore>, Status> {
        let (stores, storage, name) = requested(self.bucket.as_ref(), Kind::Bucket, request, session)?;
        stores.create(name).await.map(|granted| Response::new(bucket(storage, &granted))).map_err(|error| status(&error))
    }

    async fn create_clickhouse(&self, request: Request<SessionRequest>) -> Result<Response<ClickhouseStore>, Status> {
        let (stores, address, name) = requested(self.clickhouse.as_ref(), Kind::Clickhouse, request, session)?;
        stores.create(name).await.map(|login| Response::new(clickhouse(address, &login))).map_err(|error| status(&error))
    }

    async fn create_cache(&self, request: Request<SessionRequest>) -> Result<Response<CacheStore>, Status> {
        let (stores, address, name) = requested(self.cache.as_ref(), Kind::Cache, request, session)?;
        let numbered = stores.create(name).await.map_err(|error| status(&error))?;
        let id = numbered.login.id.to_string();
        Ok(Response::new(CacheStore {
            host: address.host.clone(),
            port: address.port.into(),
            database: numbered.database,
            username: id.clone(),
            password: numbered.login.password.expose_secret().to_owned(),
            channel_prefix: id.clone(),
            id,
        }))
    }

    async fn renew_bucket(&self, request: Request<StoreRequest>) -> Result<Response<BucketStore>, Status> {
        let (stores, storage) = offered(self.bucket.as_ref(), Kind::Bucket)?;
        stores.renew(stored(request.get_ref())?).await.map(|granted| Response::new(bucket(storage, &granted))).map_err(|error| status(&error))
    }

    async fn keep_store(&self, request: Request<StoreRequest>) -> Result<Response<()>, Status> {
        self.registry.keep(stored(request.get_ref())?).await.map(Response::new).map_err(|error| status(&error))
    }

    async fn drop_store(&self, request: Request<StoreRequest>) -> Result<Response<()>, Status> {
        match request.get_ref().id.parse::<StoreId>() {
            Ok(id) => self.registry.remove(id).await.map(Response::new).map_err(|error| status(&error)),
            Err(StoreIdError { .. }) => Ok(Response::new(())),
        }
    }

    // --- [TEMPLATES]

    async fn open_postgres_template(&self, request: Request<TemplateRequest>) -> Result<Response<PostgresStore>, Status> {
        let (stores, address, (name, digest)) = requested(self.postgres.as_ref(), Kind::Postgres, request, template)?;
        stores.open(name, digest).await.map(|login| Response::new(postgres(address, &login))).map_err(|error| status(&error))
    }

    async fn open_clickhouse_template(&self, request: Request<TemplateRequest>) -> Result<Response<ClickhouseStore>, Status> {
        let (stores, address, (name, digest)) = requested(self.clickhouse.as_ref(), Kind::Clickhouse, request, template)?;
        stores.open(name, digest).await.map(|login| Response::new(clickhouse(address, &login))).map_err(|error| status(&error))
    }

    async fn seal_template(&self, request: Request<StoreRequest>) -> Result<Response<()>, Status> {
        self.registry.seal(stored(request.get_ref())?).await.map(Response::new).map_err(|error| status(&error))
    }

    async fn clone_postgres_template(&self, request: Request<TemplateRequest>) -> Result<Response<PostgresStore>, Status> {
        let (stores, address, (name, digest)) = requested(self.postgres.as_ref(), Kind::Postgres, request, template)?;
        stores.clone_template(name, digest).await.map(|login| Response::new(postgres(address, &login))).map_err(|error| status(&error))
    }

    async fn clone_clickhouse_template(&self, request: Request<TemplateRequest>) -> Result<Response<ClickhouseStore>, Status> {
        let (stores, address, (name, digest)) = requested(self.clickhouse.as_ref(), Kind::Clickhouse, request, template)?;
        stores.clone_template(name, digest).await.map(|login| Response::new(clickhouse(address, &login))).map_err(|error| status(&error))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [REQUESTS]

fn offered<T>(served: Option<&T>, kind: Kind) -> Result<&T, Status> { served.ok_or_else(|| Status::unimplemented(format!("service runs with no {kind} server"))) }

fn requested<S, P, Q, I>(served: Option<&(Stores<S>, P)>, kind: Kind, request: Request<Q>, parsed: impl FnOnce(Q) -> Result<I, Status>) -> Result<(Stores<S>, &P, I), Status>
where S: Create + 'static {
    let (stores, published) = offered(served, kind)?;
    let until = rasm_grpc::timeout(request.metadata()).and_then(|limit| Instant::now().checked_add(limit));
    Ok((stores.waiting(until), published, parsed(request.into_inner())?))
}

fn session(SessionRequest { session }: SessionRequest) -> Result<SessionName, Status> { SessionName::try_new(session).map_err(|error| Status::invalid_argument(error.to_string())) }

fn template(TemplateRequest { session, digest }: TemplateRequest) -> Result<(SessionName, Digest), Status> {
    Ok((self::session(SessionRequest { session })?, Digest::try_new(digest).map_err(|error| Status::invalid_argument(error.to_string()))?))
}

fn stored(request: &StoreRequest) -> Result<StoreId, Status> { request.id.parse().map_err(|error| Status::not_found(format!("no store holds {}: {}", request.id, Report::new(error)))) }

// --- [RESPONSES]

const fn lease(ttl: Duration) -> SessionLease { SessionLease { ttl_seconds: ttl.as_secs().saturating_cast() } }

fn postgres((published, connections): &(Published, NonZero<u32>), login: &Login) -> PostgresStore {
    let id = login.id.to_string();
    PostgresStore {
        host: published.host.clone(),
        port: published.port.into(),
        database: id.clone(),
        username: id.clone(),
        password: login.password.expose_secret().to_owned(),
        connection_limit: connections.get(),
        id,
    }
}

fn bucket(storage: &Storage, granted: &Granted) -> BucketStore {
    let id = granted.id.to_string();
    BucketStore {
        endpoint: storage.endpoint.clone(),
        region: storage.region.clone(),
        bucket: id.clone(),
        access_key_id: granted.credentials.access_key_id().to_owned(),
        secret_access_key: granted.credentials.secret_access_key().to_owned(),
        session_token: granted.credentials.session_token().to_owned(),
        credentials_ttl_seconds: storage.credentials.unsigned_abs(),
        id,
    }
}

fn clickhouse(published: &Published, login: &Login) -> ClickhouseStore {
    let id = login.id.to_string();
    ClickhouseStore { host: published.host.clone(), http_port: published.port.into(), database: id.clone(), username: id.clone(), password: login.password.expose_secret().to_owned(), id }
}

fn status(error: &ProvisioningError) -> Status {
    let code = match error {
        ProvisioningError::Exhausted { kind: _ } => Code::ResourceExhausted,
        ProvisioningError::Store { id: _, refusal: Refusal::Missing } | ProvisioningError::Template { digest: _, claim: Claim::Unsealed } => Code::NotFound,
        ProvisioningError::Store { id: _, refusal: Refusal::InUse } => Code::FailedPrecondition,
        ProvisioningError::Template { digest: _, claim: Claim::Claimed } => Code::AlreadyExists,
        ProvisioningError::Store { id: _, refusal: Refusal::Unissued }
        | ProvisioningError::Clock { at: _ }
        | ProvisioningError::Stopping
        | ProvisioningError::Interrupted { source: _ }
        | ProvisioningError::Encode { id: _, source: _ }
        | ProvisioningError::Postgres { operation: _, source: _ }
        | ProvisioningError::Bucket { operation: _, source: _ }
        | ProvisioningError::Credentials { operation: _, source: _ }
        | ProvisioningError::Clickhouse { operation: _, source: _ }
        | ProvisioningError::Cache { operation: _, source: _ } => Code::Unavailable,
    };
    Status::new(code, Report::new(error).to_string())
}
