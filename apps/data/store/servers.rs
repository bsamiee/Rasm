use core::num::NonZero;
use std::sync::Arc;

use aws_config::{BehaviorVersion, Region};
use http::{HeaderName, HeaderValue};
use rasm_provisioning::{Bucket, Cache, Clickhouse, Kind, Postgres, Registry, Stores};
use redis::ConnectionAddr;
use redis::aio::ConnectionManagerConfig;
use secrecy::ExposeSecret as _;
use sqlx::postgres::PgPoolOptions;

use crate::service::{Published, Storage};
use crate::{Cli, StoreError};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn published(cli: &Cli, kind: Kind, host: Option<&str>, port: Option<u16>, stated: Option<u16>) -> Result<Published, StoreError> {
    let host = cli.published_host.as_deref().or(host).ok_or(StoreError::Published { kind })?;
    Ok(Published { host: host.to_owned(), port: stated.or(port).ok_or(StoreError::Published { kind })? })
}

pub(crate) async fn postgres(registry: &Arc<Registry>, cli: &Cli) -> Result<Option<(Stores<Postgres>, (Published, NonZero<u32>))>, StoreError> {
    let Some(options) = cli.postgres_url.clone() else { return Ok(None) };
    let address = published(cli, Kind::Postgres, Some(options.get_host()), Some(options.get_port()), cli.published_postgres)?;
    let admin = PgPoolOptions::new().max_connections(cli.postgres_pool.get()).connect_with(options).await.map_err(|source| StoreError::Postgres { source })?;
    Ok(Some((registry.serve(Postgres { admin, connections: cli.postgres_connections }).await?, (address, cli.postgres_connections))))
}

pub(crate) async fn bucket(registry: &Arc<Registry>, cli: &Cli) -> Result<Option<(Stores<Bucket>, Storage)>, StoreError> {
    const CREDENTIALS: i32 = 43_200;
    let Some(endpoint) = &cli.storage_endpoint else { return Ok(None) };
    let address = published(cli, Kind::Bucket, endpoint.host(), endpoint.port_u16(), cli.published_storage)?;
    let shared = aws_config::defaults(BehaviorVersion::latest()).endpoint_url(endpoint.to_string()).region(Region::new(cli.storage_region.clone())).load().await;
    let s3 = aws_sdk_s3::Client::from_conf(aws_sdk_s3::config::Builder::from(&shared).force_path_style(true).build());
    let server = Bucket { s3, sts: aws_sdk_sts::Client::new(&shared), duration_seconds: CREDENTIALS, force: (HeaderName::from_static("x-rustfs-force-delete"), HeaderValue::from_static("true")) };
    let storage = Storage { endpoint: format!("{}://{}:{}", endpoint.scheme_str().unwrap_or("http"), address.host, address.port), region: cli.storage_region.clone(), credentials: CREDENTIALS };
    Ok(Some((registry.serve(server).await?, storage)))
}

pub(crate) async fn clickhouse(registry: &Arc<Registry>, cli: &Cli) -> Result<Option<(Stores<Clickhouse>, Published)>, StoreError> {
    let Some(url) = &cli.clickhouse_url else { return Ok(None) };
    let address = published(cli, Kind::Clickhouse, url.host(), url.port_u16(), cli.published_clickhouse)?;
    let admin = clickhouse::Client::default().with_url(url.to_string()).with_user(&cli.clickhouse_user).with_password(cli.clickhouse_password.expose_secret());
    Ok(Some((registry.serve(Clickhouse { admin }).await?, address)))
}

pub(crate) async fn cache(registry: &Arc<Registry>, cli: &Cli) -> Result<Option<(Stores<Cache>, Published)>, StoreError> {
    let Some(url) = &cli.cache_url else { return Ok(None) };
    let client = redis::Client::open(url.as_str()).map_err(|source| StoreError::Cache { source })?;
    let (host, port) = match client.get_connection_info().addr() {
        ConnectionAddr::Tcp(host, port) | ConnectionAddr::TcpTls { host, port, .. } => (Some(host.as_str()), Some(*port)),
        _ => (None, None),
    };
    let address = published(cli, Kind::Cache, host, port, cli.published_cache)?;
    let server = Cache::connect(&client, ConnectionManagerConfig::new()).await.map_err(|source| StoreError::Cache { source })?;
    Ok(Some((registry.serve(server).await?, address)))
}
