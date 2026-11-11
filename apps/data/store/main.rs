#![feature(const_trait_impl, default_field_values, error_reporter, integer_casts, try_blocks_heterogeneous)]
#![expect(clippy::disallowed_macros, reason = "tonic generates the server trait under `#[async_trait]`, and its implementations take the same macro")]

use core::net::{Ipv4Addr, SocketAddr};
use core::num::NonZero;
use core::time::Duration;
use std::io;
use std::process::ExitCode;
use std::sync::Arc;

use backon::{ConstantBuilder, ExponentialBuilder};
use clap::Parser;
use futures::future::join4;
use http::Uri;
use http::uri::{Authority, Scheme};
use jiff::Timestamp;
use jiff::fmt::friendly::SpanParser;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use rasm_cli::{CommandError, Invocation};
use rasm_grpc::{GrpcError, Telemetry};
use rasm_http::HttpError;
use rasm_telemetry::process::Program;
use rasm_provisioning::{Kind, Policy, ProvisioningError, Registry};
use rasm_store_proto::FILE_DESCRIPTOR_SET;
use rasm_store_proto::store_service_server::StoreServiceServer;
use redis::RedisError;
use secrecy::SecretString;
use sqlx::postgres::PgConnectOptions;
use tokio::runtime::Builder;
use tokio_util::sync::CancellationToken;
use tonic::codec::CompressionEncoding;
use tonic::transport::server::TcpIncoming;
use tonic::transport::{Endpoint, Server};
use tonic_health::server::health_reporter;
use tracing_subscriber::layer::Identity;
use validated::Validated;

use self::service::Store;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Parser)]
#[command(version, about = "Serves sessions of isolated PostgreSQL, bucket, ClickHouse, and cache stores on shared servers over gRPC", long_about = None)]
#[command(after_help = "A kind whose server address is unset answers UNIMPLEMENTED. Messages take gzip or identity encoding, and responses use gzip when the caller accepts it.\n\
A create past a live limit the service holds waits --wait for a store of that kind to end, then answers RESOURCE_EXHAUSTED; a caller's shorter deadline ends the wait with its own status.")]
struct Cli {
    #[arg(long, env = "STORE_LISTEN", value_name = "HOST[:PORT]", value_delimiter = ',', default_value = "0.0.0.0,[::]", help = "Addresses the service listens on")]
    listen: Vec<Authority>,
    #[arg(long, env = "PORT", default_value_t = 8080, help = "Port of each listen address that states none")]
    port: u16,
    #[arg(long, env = "STORE_GRACE", value_name = "SPAN", default_value = "10s", value_parser = span, help = "Time a session outlives its last held stream")]
    grace: Duration,
    #[arg(long, env = "STORE_LEASE", value_name = "SPAN", default_value = "12h", value_parser = span, help = "Time a session no stream ever held outlives its last call")]
    lease: Duration,
    #[arg(long, env = "STORE_WAIT", value_name = "SPAN", default_value = "10s", value_parser = span, help = "Time a create at a live limit waits for a store of its kind to end")]
    wait: Duration,
    #[arg(long, env = "STORE_SHUTDOWN", value_name = "SPAN", default_value = "20s", value_parser = span, help = "Time shutdown waits for begun creates, drops, and open connections")]
    shutdown: Duration,
    #[arg(long, env = "STORE_TIMEOUT", value_name = "SPAN", value_parser = span, help = "Longest time a call waits for its response to start, unbounded when unset")]
    timeout: Option<Duration>,
    #[arg(long, env = "STORE_KEEPALIVE_INTERVAL", value_name = "SPAN", default_value = "5s", value_parser = span, help = "Time between HTTP/2 and TCP keepalive probes on a connection")]
    keepalive_interval: Duration,
    #[arg(long, env = "STORE_KEEPALIVE_TIMEOUT", value_name = "SPAN", default_value = "2s", value_parser = span, help = "Time an unanswered HTTP/2 keepalive probe ends its connection after")]
    keepalive_timeout: Duration,
    #[arg(long, env = "STORE_STREAMS", value_name = "COUNT", help = "Concurrent calls one connection holds, the HTTP/2 default when unset")]
    streams: Option<u32>,
    #[arg(long, env = "STORE_READY", value_name = "COUNT", default_value_t = 2, help = "Clones kept ready per sealed template")]
    ready: usize,
    #[arg(long, env = "STORE_TEMPLATES", value_name = "COUNT", default_value = "8", help = "Sealed templates kept per kind, least recently cloned removed first")]
    templates: NonZero<usize>,
    #[arg(long, hide = true)]
    probe: bool,
    #[arg(long, env = "STORE_PUBLISHED_HOST", value_name = "HOST", help_heading = "Published", help = "Host store records name, default each server's own")]
    published_host: Option<String>,
    #[arg(long, env = "STORE_PUBLISHED_POSTGRES", value_name = "PORT", help_heading = "Published", help = "PostgreSQL port store records name, default the server's own")]
    published_postgres: Option<u16>,
    #[arg(long, env = "STORE_PUBLISHED_STORAGE", value_name = "PORT", help_heading = "Published", help = "Object storage port store records name, default the server's own")]
    published_storage: Option<u16>,
    #[arg(long, env = "STORE_PUBLISHED_CLICKHOUSE", value_name = "PORT", help_heading = "Published", help = "ClickHouse HTTP port store records name, default the server's own")]
    published_clickhouse: Option<u16>,
    #[arg(long, env = "STORE_PUBLISHED_CACHE", value_name = "PORT", help_heading = "Published", help = "Cache port store records name, default the server's own")]
    published_cache: Option<u16>,
    #[arg(long, env = "POSTGRES_URL", value_name = "URL", help_heading = "Servers", help = "PostgreSQL administrator connection")]
    postgres_url: Option<PgConnectOptions>,
    #[arg(long, env = "STORE_POSTGRES_POOL", value_name = "COUNT", default_value = "4", help_heading = "Servers", help = "PostgreSQL administrator connections held at most")]
    postgres_pool: NonZero<u32>,
    #[arg(long, env = "STORE_POSTGRES_CONNECTIONS", value_name = "COUNT", default_value = "3", help_heading = "Servers", help = "Connections one PostgreSQL store's login holds at most")]
    postgres_connections: NonZero<u32>,
    #[arg(long, env = "AWS_ENDPOINT_URL", value_name = "URL", help_heading = "Servers", help = "S3 and STS endpoint, credentials from the AWS provider chain")]
    storage_endpoint: Option<Uri>,
    #[arg(long, env = "AWS_REGION", value_name = "REGION", default_value = "us-east-1", help_heading = "Servers", help = "Region of buckets and bucket store records")]
    storage_region: String,
    #[arg(long, env = "CLICKHOUSE_URL", value_name = "URL", help_heading = "Servers", help = "ClickHouse HTTP interface")]
    clickhouse_url: Option<Uri>,
    #[arg(long, env = "CLICKHOUSE_USER", value_name = "NAME", default_value = "default", help_heading = "Servers", help = "ClickHouse administrator")]
    clickhouse_user: String,
    #[arg(long, env = "CLICKHOUSE_PASSWORD", value_name = "PASSWORD", default_value = "", hide_env_values = true, help_heading = "Servers", help = "ClickHouse administrator password")]
    clickhouse_password: SecretString,
    #[arg(long, env = "CACHE_URL", value_name = "URL", help_heading = "Servers", help = "Valkey administrator connection")]
    cache_url: Option<String>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum StoreError {
    #[error(transparent)]
    Command(#[from] CommandError),
    #[error(transparent)]
    Grpc(#[from] GrpcError),
    #[error(transparent)]
    Serve(Box<HttpError<Self>>),
    #[error(transparent)]
    Provisioning(#[from] ProvisioningError),
    #[error("cannot reach the postgres administrator connection")]
    Postgres { source: sqlx::Error },
    #[error("cannot reach the valkey administrator connection")]
    Cache { source: RedisError },
    #[error("{kind} server states no published address, set --published-host and the published {kind} port")]
    Published { kind: Kind },
    #[error("cannot address the probed port")]
    Address { source: http::Error },
}

impl From<HttpError<Self>> for StoreError {
    fn from(value: HttpError<Self>) -> Self { Self::Serve(Box::new(value)) }
}

impl From<StoreError> for sysexits::ExitCode {
    fn from(value: StoreError) -> Self {
        match value {
            StoreError::Command(error) => error.into(),
            StoreError::Grpc(error) => error.into(),
            StoreError::Serve(error) => (*error).into(),
            StoreError::Provisioning(_) | StoreError::Postgres { source: _ } | StoreError::Cache { source: _ } => Self::Unavailable,
            StoreError::Published { kind: _ } => Self::Config,
            StoreError::Address { source: _ } => Self::Software,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn span(text: &str) -> Result<Duration, jiff::Error> { SpanParser::new().parse_unsigned_duration(text) }

// --- [COMPOSITION] ---------------------------------------------------------------------

async fn run(invocation: Invocation<Cli>) -> Result<(), NEVec<StoreError>> {
    const BACKLOG: u32 = 1_024;
    fn lifted<E>(errors: NEVec<E>) -> NEVec<StoreError>
    where StoreError: From<E> {
        errors.into_nonempty_iter().map(StoreError::from).collect()
    }
    let (cli, stopping) = (invocation.args, CancellationToken::new());
    if cli.probe {
        let probed = try bikeshed Result<(), StoreError> {
            let authority = SocketAddr::from((Ipv4Addr::LOCALHOST, cli.port)).to_string();
            let target = Uri::builder().scheme(Scheme::HTTP).authority(authority).path_and_query("/").build().map_err(|source| StoreError::Address { source })?;
            rasm_grpc::probe(rasm_grpc::connect(&Endpoint::from(target), ConstantBuilder::default().with_max_times(0)).await?, "").await?
        };
        return probed.map_err(NEVec::new);
    }
    let reclaim = ExponentialBuilder::default().with_factor(1.0).with_min_delay(Duration::from_millis(200)).without_max_times().with_total_delay(Some(cli.lease));
    let disconnect = ExponentialBuilder::default().with_factor(1.0).with_min_delay(Duration::from_millis(100)).with_max_times(50);
    let registry = Registry::new(Policy { grace: cli.grace, lease: cli.lease, reclaim, disconnect, ready: cli.ready, templates: cli.templates, clock: Timestamp::now }, stopping.clone());
    let (postgres, bucket, clickhouse, cache) =
        Box::pin(join4(servers::postgres(&registry, &cli), servers::bucket(&registry, &cli), servers::clickhouse(&registry, &cli), servers::cache(&registry, &cli))).await;
    let (postgres, bucket, clickhouse, cache) =
        Validated::from(postgres).map4(bucket.into(), clickhouse.into(), cache.into(), |postgres, bucket, clickhouse, cache| (postgres, bucket, clickhouse, cache)).ok()?;
    let (descriptors, (health, reported)) = ([FILE_DESCRIPTOR_SET, tonic_health::pb::FILE_DESCRIPTOR_SET], health_reporter());
    health.set_serving::<StoreServiceServer<Store>>().await;
    let standard = rasm_grpc::reflection(descriptors).map_err(|error| NEVec::new(error.into()))?.add_service(reported.accept_compressed(CompressionEncoding::Gzip));
    let liveness = Server::builder().http2_keepalive_interval(Some(cli.keepalive_interval)).http2_keepalive_timeout(Some(cli.keepalive_timeout)).max_concurrent_streams(cli.streams);
    let server = cli.timeout.into_iter().fold(liveness, Server::timeout).layer(Telemetry::server(descriptors).map_err(lifted)?);
    let bound = rasm_http::bind::<StoreError>(cli.listen, cli.port, BACKLOG).await.map_err(lifted)?;
    let incoming = bound.into_iter().flat_map(|(_, listeners)| listeners).map(|(_, listener)| TcpIncoming::from(listener).with_nodelay(Some(true)).with_keepalive(Some(cli.keepalive_interval)));
    let signals = rasm_cli::signals().map_err(|error| NEVec::new(error.into()))?;
    let app = Arc::new(Store { registry, stopping, health, standard, wait: cli.wait, postgres, bucket, clickhouse, cache });
    rasm_http::serve(app, |router, stopping| rasm_grpc::accept(server, incoming, router, stopping), signals, cli.shutdown).await.map_err(lifted)
}

fn main() -> ExitCode {
    rasm_cli::run(
        Program { scopes: Vec::from_iter([env!("CARGO_CRATE_NAME"), "rasm_grpc", "rasm_http", "rasm_provisioning"].map(|scope| (scope, None))), .. },
        io::stderr,
        Identity::new(),
        |_: &Cli| Builder::new_multi_thread(),
        async |invocation| run(invocation).await,
    )
}

mod servers;
mod service;
