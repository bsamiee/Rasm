use core::fmt::Debug;
use core::future::ready;
use core::iter;
use core::marker::PhantomData;
use core::net::{Ipv4Addr, SocketAddr};
use core::num::TryFromIntError;
use core::time::Duration;
use std::ffi::OsString;
use std::sync::Arc;

use arc_swap::ArcSwap;
use clap::Parser;
use derive_more::Deref;
use futures::future::BoxFuture;
use http::Uri;
use http::uri::Scheme;
use jiff::fmt::friendly::SpanParser;
use object_store::CredentialProvider;
use object_store::aws::{AmazonS3, AmazonS3Builder, AwsCredential};
use rasm_store_proto::store_service_client::StoreServiceClient;
use rasm_store_proto::{BucketStore, CacheStore, ClickhouseStore, PostgresStore, SessionLease, SessionRequest, StoreRequest, TemplateRequest};
use rasm_testing::Fixture;
use redis::{ConnectionAddr, IntoConnectionInfo as _, RedisConnectionInfo};
use sqlx::postgres::{PgConnectOptions, PgPoolOptions};
use sqlx::{Database, PgPool, Pool};
use tokio::time::{self, Instant};
use tonic::codec::Streaming;
use tonic::transport::Channel;
use tonic::{Code, Response, Status};
use uuid::{NoContext, Uuid};

// --- [TYPES] ---------------------------------------------------------------------------

// --- [KIND]

pub trait Kind: Sized {
    type Record: Debug;

    fn create(client: &mut StoreServiceClient<Channel>, request: SessionRequest) -> impl Future<Output = Result<Response<Self::Record>, Status>>;

    fn opened(within: &Session, record: &Self::Record) -> Result<(StoreRequest, Self), StoreError>;

    fn released(self, deadline: Option<Duration>) -> impl Future<Output = usize> {
        drop((self, deadline));
        ready(0)
    }
}

impl Kind for PgPool {
    type Record = PostgresStore;

    async fn create(client: &mut StoreServiceClient<Channel>, request: SessionRequest) -> Result<Response<PostgresStore>, Status> { client.create_postgres(request).await }

    fn opened(_: &Session, PostgresStore { id, host, port, database, username, password, connection_limit }: &PostgresStore) -> Result<(StoreRequest, Self), StoreError> {
        let options = PgConnectOptions::new_without_pgpass().host(host).port(port16(id, *port)?).database(database).username(username).password(password);
        Ok((StoreRequest { id: id.clone() }, PgPoolOptions::new().max_connections(*connection_limit).connect_lazy_with(options)))
    }

    async fn released(self, deadline: Option<Duration>) -> usize { released(self, deadline).await }
}

impl Kind for AmazonS3 {
    type Record = BucketStore;

    async fn create(client: &mut StoreServiceClient<Channel>, request: SessionRequest) -> Result<Response<BucketStore>, Status> { client.create_bucket(request).await }

    fn opened(within: &Session, record: &BucketStore) -> Result<(StoreRequest, Self), StoreError> {
        let request = StoreRequest { id: record.id.clone() };
        let plain = record.endpoint.parse::<Uri>().map_err(http::Error::from)?.scheme() == Some(&Scheme::HTTP);
        let renewed = Renewed { client: within.client(), request: request.clone(), held: ArcSwap::from_pointee(Renewed::granted(record)) };
        let builder = AmazonS3Builder::new().with_endpoint(&record.endpoint).with_allow_http(plain).with_region(&record.region).with_bucket_name(&record.bucket);
        Ok((request, builder.with_credentials(Arc::new(renewed)).build()?))
    }
}

impl Kind for clickhouse::Client {
    type Record = ClickhouseStore;

    async fn create(client: &mut StoreServiceClient<Channel>, request: SessionRequest) -> Result<Response<ClickhouseStore>, Status> { client.create_clickhouse(request).await }

    fn opened(_: &Session, ClickhouseStore { id, host, http_port, database, username, password }: &ClickhouseStore) -> Result<(StoreRequest, Self), StoreError> {
        let url = Uri::builder().scheme(Scheme::HTTP).authority(format!("{host}:{}", port16(id, *http_port)?)).path_and_query("/").build()?;
        Ok((StoreRequest { id: id.clone() }, Self::default().with_url(url.to_string()).with_database(database).with_user(username).with_password(password)))
    }
}

impl Kind for Cache {
    type Record = CacheStore;

    async fn create(client: &mut StoreServiceClient<Channel>, request: SessionRequest) -> Result<Response<CacheStore>, Status> { client.create_cache(request).await }

    fn opened(_: &Session, CacheStore { id, host, port, database, username, password, channel_prefix }: &CacheStore) -> Result<(StoreRequest, Self), StoreError> {
        let settings = RedisConnectionInfo::default().set_db(i64::from(*database)).set_username(username).set_password(password);
        let client = redis::Client::open(ConnectionAddr::Tcp(host.clone(), port16(id, *port)?).into_connection_info()?.set_redis_settings(settings))?;
        Ok((StoreRequest { id: id.clone() }, Self { client, channel_prefix: channel_prefix.clone() }))
    }
}

// --- [TEMPLATE]

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Step {
    Open,
    Clone,
}

pub trait Templated: Kind {
    fn template(client: &mut StoreServiceClient<Channel>, step: Step, request: TemplateRequest) -> impl Future<Output = Result<Response<Self::Record>, Status>>;
}

impl Templated for PgPool {
    async fn template(client: &mut StoreServiceClient<Channel>, step: Step, request: TemplateRequest) -> Result<Response<PostgresStore>, Status> {
        match step {
            Step::Open => client.open_postgres_template(request).await,
            Step::Clone => client.clone_postgres_template(request).await,
        }
    }
}

impl Templated for clickhouse::Client {
    async fn template(client: &mut StoreServiceClient<Channel>, step: Step, request: TemplateRequest) -> Result<Response<ClickhouseStore>, Status> {
        match step {
            Step::Open => client.open_clickhouse_template(request).await,
            Step::Clone => client.clone_clickhouse_template(request).await,
        }
    }
}

pub trait Template {
    type Driver: Templated;

    fn digest() -> String;

    fn build(driver: &Self::Driver) -> impl Future<Output = Result<(), StoreError>>;
}

// --- [SOURCE]

pub trait Source {
    type Driver: Kind;

    fn record(session: &Session) -> impl Future<Output = Result<<Self::Driver as Kind>::Record, StoreError>>;
}

impl<K> Source for K
where K: Kind
{
    type Driver = Self;

    async fn record(session: &Session) -> Result<K::Record, StoreError> {
        K::create(&mut session.client(), SessionRequest { session: session.name.to_string() }).await.map(Response::into_inner).map_err(refused(Operation::Create(session.name)))
    }
}

impl<T> Source for Cloned<T>
where T: Template
{
    type Driver = T::Driver;

    async fn record(session: &Session) -> Result<<T::Driver as Kind>::Record, StoreError> {
        let (mut client, request) = (session.client(), TemplateRequest { session: session.name.to_string(), digest: T::digest() });
        loop {
            match T::Driver::template(&mut client, Step::Clone, request.clone()).await {
                Ok(cloned) => break Ok(cloned.into_inner()),
                Err(unsealed) if unsealed.code() == Code::NotFound => {}
                Err(source) => break Err(refused(Operation::Clone(request.digest))(source)),
            }
            match T::Driver::template(&mut client, Step::Open, request.clone()).await {
                Ok(opened) => {
                    let (build, driver) = T::Driver::opened(session, opened.get_ref())?;
                    let outcome = T::build(&driver).await;
                    driver.released(session.deadline).await;
                    match outcome {
                        Ok(()) => client.seal_template(build).await.map(drop).map_err(refused(Operation::Seal(request.digest.clone())))?,
                        Err(cause) => {
                            client.drop_store(build.clone()).await.map_err(refused(Operation::Drop(build.id)))?;
                            break Err(cause);
                        }
                    }
                }
                Err(taken) if taken.code() == Code::AlreadyExists => {}
                Err(source) => break Err(refused(Operation::Open(request.digest))(source)),
            }
        }
    }
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Parser)]
#[command(no_binary_name = true)]
pub struct Endpoint {
    #[arg(long, env)]
    pub store_port: u16,
    #[arg(long, env = "STORE_DEADLINE", value_name = "SPAN", default_value = "30s", value_parser = |span: &str| SpanParser::new().parse_unsigned_duration(span))]
    #[arg(help = "Longest wait for one store call or one pool close, above the 10s the service waits at a live limit")]
    pub deadline: Option<Duration>,
}

impl Endpoint {
    pub fn published() -> Result<Self, StoreError> { Ok(Self::try_parse_from(iter::empty::<OsString>())?) }
}

#[derive(Debug, Clone)]
pub struct Cache {
    pub client: redis::Client,
    pub channel_prefix: String,
}

#[derive(Debug)]
pub struct Cloned<T>(PhantomData<fn() -> T>);

#[derive(Debug, Clone, PartialEq, Eq, derive_more::Display)]
pub enum Operation {
    #[display("end session {_0}")]
    End(Uuid),
    #[display("create a store in session {_0}")]
    Create(Uuid),
    #[display("open template {_0}")]
    Open(String),
    #[display("seal template {_0}")]
    Seal(String),
    #[display("clone template {_0}")]
    Clone(String),
    #[display("keep store {_0}")]
    Keep(String),
    #[display("drop store {_0}")]
    Drop(String),
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum StoreError {
    #[error("store service at {endpoint} granted no session")]
    Silent { endpoint: Uri, source: Option<Box<Status>> },
    #[error("store service refused to {operation}")]
    Refused { operation: Operation, source: Box<Status> },
    #[error("store {store} is held by a client, {held} connections of its pool still checked out")]
    Leaked { store: String, held: usize, source: Box<Status> },
    #[error(transparent)]
    Endpoint(#[from] clap::Error),
    #[error(transparent)]
    Address(#[from] http::Error),
    #[error("store {store} states a port above 65535")]
    Port { store: String, source: TryFromIntError },
    #[error(transparent)]
    Postgres(#[from] sqlx::Error),
    #[error(transparent)]
    Clickhouse(#[from] clickhouse::error::Error),
    #[error(transparent)]
    Bucket(#[from] object_store::Error),
    #[error(transparent)]
    Cache(#[from] redis::RedisError),
}

// --- [SERVICES] ------------------------------------------------------------------------

// --- [SESSION]

#[derive(Debug)]
pub struct Session {
    name: Uuid,
    deadline: Option<Duration>,
    client: StoreServiceClient<Channel>,
    hold: Streaming<SessionLease>,
}

impl Session {
    #[must_use]
    pub const fn name(&self) -> Uuid { self.name }

    #[must_use]
    pub fn client(&self) -> StoreServiceClient<Channel> { self.client.clone() }
}

impl Fixture for Session {
    type Within = Endpoint;
    type Kept = Uuid;
    type Error = StoreError;

    async fn created(Endpoint { store_port, deadline }: &Endpoint) -> Result<Self, StoreError> {
        let endpoint = Uri::builder().scheme(Scheme::HTTP).authority(SocketAddr::from((Ipv4Addr::LOCALHOST, *store_port)).to_string()).path_and_query("/").build()?;
        let configured = deadline.iter().copied().fold(tonic::transport::Endpoint::from(endpoint.clone()), tonic::transport::Endpoint::timeout);
        let mut client = StoreServiceClient::new(configured.connect_lazy());
        let at = jiff::Timestamp::now();
        let name = Uuid::new_v7(uuid::Timestamp::from_unix(NoContext, at.as_second().unsigned_abs(), at.subsec_nanosecond().unsigned_abs()));
        let silent = |source: Option<Status>| StoreError::Silent { endpoint: endpoint.clone(), source: source.map(Box::new) };
        let mut hold = client.hold_session(SessionRequest { session: name.to_string() }).await.map_err(|refusal| silent(Some(refusal)))?.into_inner();
        let SessionLease { ttl_seconds: _ } = hold.message().await.map_err(|refusal| silent(Some(refusal)))?.ok_or_else(|| silent(None))?;
        Ok(Self { name, deadline: *deadline, client, hold })
    }

    async fn dropped(self) -> Result<(), StoreError> {
        let Self { name, deadline: _, mut client, hold } = self;
        let ended = client.end_session(SessionRequest { session: name.to_string() }).await;
        drop(hold);
        ended.map(drop).map_err(refused(Operation::End(name)))
    }

    fn kept(self) -> impl Future<Output = Result<Uuid, StoreError>> {
        let Self { name, deadline: _, client: _, hold } = self;
        drop(hold);
        ready(Ok(name))
    }
}

// --- [STORE]

#[derive(Debug, Deref)]
pub struct Store<S>
where S: Source
{
    #[deref]
    driver: S::Driver,
    record: <S::Driver as Kind>::Record,
    request: StoreRequest,
    deadline: Option<Duration>,
    client: StoreServiceClient<Channel>,
}

impl<S> Store<S>
where S: Source
{
    #[must_use]
    pub const fn record(&self) -> &<S::Driver as Kind>::Record { &self.record }
}

impl<S> Fixture for Store<S>
where S: Source
{
    type Within = Session;
    type Kept = <S::Driver as Kind>::Record;
    type Error = StoreError;

    async fn created(within: &Session) -> Result<Self, StoreError> {
        let record = S::record(within).await?;
        let (request, driver) = S::Driver::opened(within, &record)?;
        Ok(Self { driver, record, request, deadline: within.deadline, client: within.client() })
    }

    async fn dropped(self) -> Result<(), StoreError> {
        let Self { driver, record: _, request, deadline, mut client } = self;
        let held = driver.released(deadline).await;
        client.drop_store(request.clone()).await.map(drop).map_err(|source| match source.code() {
            Code::FailedPrecondition => StoreError::Leaked { store: request.id, held, source: Box::new(source) },
            _ => refused(Operation::Drop(request.id))(source),
        })
    }

    async fn kept(self) -> Result<Self::Kept, StoreError> {
        let Self { driver: _, record, request, deadline: _, mut client } = self;
        client.keep_store(request.clone()).await.map(|_| record).map_err(refused(Operation::Keep(request.id)))
    }
}

// --- [CREDENTIALS]

#[derive(Debug)]
struct Renewed {
    client: StoreServiceClient<Channel>,
    request: StoreRequest,
    held: ArcSwap<(Arc<AwsCredential>, Instant, Duration)>,
}

impl Renewed {
    fn granted(record: &BucketStore) -> (Arc<AwsCredential>, Instant, Duration) {
        let BucketStore { id: _, endpoint: _, region: _, bucket: _, access_key_id, secret_access_key, session_token, credentials_ttl_seconds } = record;
        let credential = AwsCredential { key_id: access_key_id.clone(), secret_key: secret_access_key.clone(), token: Some(session_token.clone()) };
        (Arc::new(credential), Instant::now(), Duration::from_secs(u64::from(*credentials_ttl_seconds)))
    }
}

impl CredentialProvider for Renewed {
    type Credential = AwsCredential;

    fn get_credential<'this, 'call>(&'this self) -> BoxFuture<'call, object_store::Result<Arc<AwsCredential>>>
    where
        'this: 'call,
        Self: 'call,
    {
        Box::pin(async move {
            let held = self.held.load_full();
            if held.1.elapsed() < held.2 {
                return Ok(Arc::clone(&held.0));
            }
            let renewed = self.client.clone().renew_bucket(self.request.clone()).await.map_err(|source| object_store::Error::Generic { store: "S3", source: Box::new(source) })?;
            let granted = Self::granted(renewed.get_ref());
            let credential = Arc::clone(&granted.0);
            self.held.store(Arc::new(granted));
            Ok(credential)
        })
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn refused(operation: Operation) -> impl FnOnce(Status) -> StoreError { move |source| StoreError::Refused { operation, source: Box::new(source) } }

async fn released<D>(pool: Pool<D>, deadline: Option<Duration>) -> usize
where D: Database {
    match time::timeout(deadline.unwrap_or(Duration::MAX), pool.close()).await {
        Ok(()) => 0,
        Err(time::error::Elapsed { .. }) => (pool.size() as usize).saturating_sub(pool.num_idle()),
    }
}

fn port16(store: &str, port: u32) -> Result<u16, StoreError> { u16::try_from(port).map_err(|source| StoreError::Port { store: store.to_owned(), source }) }
