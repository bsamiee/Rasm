use std::collections::BTreeMap;
use std::io;
use std::path::Path;
use std::process::Stdio;

use aws_sdk_s3::config::Credentials;
use http::Uri;
use http::uri::Scheme;
use oci_spec::distribution::Reference;
use rdkafka::ClientConfig;
use redis::{ConnectionAddr, IntoConnectionInfo as _, RedisConnectionInfo};
use serde::Deserialize;
use serde_path_to_error::Track;
use sqlx::PgPool;
use sqlx::postgres::{PgConnectOptions, PgPoolOptions};
use testcontainers::core::ContainerPort;
use testcontainers::runners::AsyncRunner as _;
use testcontainers::{ContainerAsync, ContainerRequest, Image, ImageExt};
use testcontainers_modules::clickhouse::{CLICKHOUSE_PORT, ClickHouse};
use testcontainers_modules::kafka::apache::{KAFKA_PORT, Kafka};
use testcontainers_modules::postgres::Postgres;
use testcontainers_modules::rustfs::RustFS;
use testcontainers_modules::valkey::{VALKEY_PORT, Valkey};
use tokio::process::Command;

use crate::FixtureError;

// --- [TYPES] ---------------------------------------------------------------------------

type Stated = BTreeMap<String, String>;

pub trait Hosted: Sized {
    type Image: Image;
    type Config;

    const PORT: ContainerPort;

    fn connected(stated: &Stated, host: String, port: u16, config: Self::Config) -> Result<Self, FixtureError>;

    fn started(request: ContainerRequest<Self::Image>, config: Self::Config) -> impl Future<Output = Result<(ContainerAsync<Self::Image>, Self), FixtureError>> {
        async move {
            let stated = request.env_vars().map(|(name, value)| (name.into_owned(), value.into_owned())).collect::<Stated>();
            let server = request.start().await?;
            let (host, port) = (server.get_host().await?, server.get_host_port_ipv4(Self::PORT).await?);
            let driver = Self::connected(&stated, host.to_string(), port, config)?;
            Ok((server, driver))
        }
    }
}

impl Hosted for PgPool {
    type Image = Postgres;
    type Config = PgPoolOptions;

    const PORT: ContainerPort = ContainerPort::Tcp(5432);

    fn connected(stated: &Stated, host: String, port: u16, config: PgPoolOptions) -> Result<Self, FixtureError> {
        let options = PgConnectOptions::new_without_pgpass().host(&host).port(port);
        let options = options.username(read(stated, "POSTGRES_USER")?).password(read(stated, "POSTGRES_PASSWORD")?).database(read(stated, "POSTGRES_DB")?);
        Ok(config.connect_lazy_with(options))
    }
}

impl Hosted for clickhouse::Client {
    type Image = ClickHouse;
    type Config = Self;

    const PORT: ContainerPort = CLICKHOUSE_PORT;

    fn connected(stated: &Stated, host: String, port: u16, config: Self) -> Result<Self, FixtureError> {
        let url = Uri::builder().scheme(Scheme::HTTP).authority(format!("{host}:{port}")).path_and_query("/").build()?;
        let settings = [("CLICKHOUSE_USER", Self::with_user as fn(Self, String) -> Self), ("CLICKHOUSE_PASSWORD", Self::with_password), ("CLICKHOUSE_DB", Self::with_database)];
        Ok(settings.into_iter().fold(config.with_url(url.to_string()), |client, (variable, set)| stated.get(variable).cloned().into_iter().fold(client, set)))
    }
}

impl Hosted for redis::Client {
    type Image = Valkey;
    type Config = RedisConnectionInfo;

    const PORT: ContainerPort = VALKEY_PORT;

    fn connected(_: &Stated, host: String, port: u16, config: RedisConnectionInfo) -> Result<Self, FixtureError> {
        Ok(Self::open(ConnectionAddr::Tcp(host, port).into_connection_info()?.set_redis_settings(config))?)
    }
}

impl Hosted for aws_sdk_s3::Client {
    type Image = RustFS;
    type Config = aws_sdk_s3::config::Builder;

    const PORT: ContainerPort = ContainerPort::Tcp(9000);

    fn connected(stated: &Stated, host: String, port: u16, config: aws_sdk_s3::config::Builder) -> Result<Self, FixtureError> {
        let endpoint = Uri::builder().scheme(Scheme::HTTP).authority(format!("{host}:{port}")).path_and_query("/").build()?;
        let credentials = Credentials::new(read(stated, "RUSTFS_ACCESS_KEY")?, read(stated, "RUSTFS_SECRET_KEY")?, None, None, env!("CARGO_PKG_NAME"));
        Ok(Self::from_conf(config.endpoint_url(endpoint.to_string()).credentials_provider(credentials).force_path_style(true).build()))
    }
}

impl Hosted for ClientConfig {
    type Image = Kafka;
    type Config = Self;

    const PORT: ContainerPort = KAFKA_PORT;

    fn connected(_: &Stated, host: String, port: u16, mut config: Self) -> Result<Self, FixtureError> {
        config.set("bootstrap.servers", format!("{host}:{port}"));
        Ok(config)
    }
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Deserialize)]
struct Compose {
    services: BTreeMap<String, Service>,
}

#[derive(Debug, Deserialize)]
struct Service {
    image: Option<String>,
    command: Option<Vec<String>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn defined<I>(file: &Path, service: &str, image: I) -> Result<ContainerRequest<I>, FixtureError>
where I: Image {
    let resolved = Command::new("docker").args(["compose", "--file"]).arg(file).args(["--profile", "*", "config", "--format", "json"]).stderr(Stdio::inherit()).output().await;
    let model = resolved.and_then(|output| output.status.exit_ok().map(|()| output.stdout).map_err(io::Error::other)).map_err(|source| FixtureError::Compose { file: file.to_owned(), source })?;
    let mut reader = serde_json::Deserializer::from_slice(&model);
    let composed = serde_path_to_error::deserialize(&mut reader).and_then(|composed| reader.end().map(|()| composed).map_err(|source| serde_path_to_error::Error::new(Track::new().path(), source)));
    let Compose { mut services } = composed.map_err(|source| FixtureError::Model { file: file.to_owned(), source })?;
    let unlisted = || FixtureError::Unlisted { file: file.to_owned(), service: service.to_owned() };
    let Service { image: stated, command } = services.remove(service).ok_or_else(unlisted)?;
    let reference = stated.ok_or_else(unlisted)?.parse::<Reference>()?;
    let named = image.with_name(format!("{}/{}", reference.registry(), reference.repository()));
    let tagged = reference.tag().into_iter().fold(named, ImageExt::with_tag);
    Ok(command.into_iter().fold(tagged, ImageExt::with_cmd))
}

fn read<'stated>(stated: &'stated Stated, variable: &'static str) -> Result<&'stated str, FixtureError> { stated.get(variable).map(String::as_str).ok_or(FixtureError::Unstated { variable }) }
