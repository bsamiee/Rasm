#![feature(exit_status_error)]

use core::time::Duration;
use std::collections::BTreeMap;
use std::io;
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
use testcontainers::core::wait::ExitWaitStrategy;
use testcontainers::core::{ContainerPort, Host, WaitFor};
use testcontainers::runners::{AsyncBuilder as _, AsyncRunner as _};
use testcontainers::{ContainerAsync, ContainerRequest, GenericBuildableImage, Image, ImageExt};
use testcontainers_modules::clickhouse::{CLICKHOUSE_PORT, ClickHouse};
use testcontainers_modules::kafka::apache::{KAFKA_PORT, Kafka};
use testcontainers_modules::postgres::Postgres;
use testcontainers_modules::rustfs::RustFS;
use testcontainers_modules::valkey::{VALKEY_PORT, Valkey};
use tokio::io::AsyncWriteExt as _;
use tokio::process::Command;

// --- [CONSTANTS] -----------------------------------------------------------------------

pub const GATEWAY: &str = "host.docker.internal";

// --- [TYPES] ---------------------------------------------------------------------------

type Stated = BTreeMap<String, String>;

pub trait Hosted: Sized {
    type Image: Image;
    type Config;

    const PORT: ContainerPort;

    fn connected(stated: &Stated, host: String, port: u16, config: Self::Config) -> Result<Self, ServerError>;

    fn started(request: ContainerRequest<Self::Image>, config: Self::Config) -> impl Future<Output = Result<(ContainerAsync<Self::Image>, Self), ServerError>> {
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

    fn connected(stated: &Stated, host: String, port: u16, config: PgPoolOptions) -> Result<Self, ServerError> {
        let options = PgConnectOptions::new_without_pgpass().host(&host).port(port);
        let options = options.username(read(stated, "POSTGRES_USER")?).password(read(stated, "POSTGRES_PASSWORD")?).database(read(stated, "POSTGRES_DB")?);
        Ok(config.connect_lazy_with(options))
    }
}

impl Hosted for clickhouse::Client {
    type Image = ClickHouse;
    type Config = Self;

    const PORT: ContainerPort = CLICKHOUSE_PORT;

    fn connected(stated: &Stated, host: String, port: u16, config: Self) -> Result<Self, ServerError> {
        let url = Uri::builder().scheme(Scheme::HTTP).authority(format!("{host}:{port}")).path_and_query("/").build()?;
        let settings = [("CLICKHOUSE_USER", Self::with_user as fn(Self, String) -> Self), ("CLICKHOUSE_PASSWORD", Self::with_password), ("CLICKHOUSE_DB", Self::with_database)];
        Ok(settings.into_iter().fold(config.with_url(url.to_string()), |client, (variable, set)| stated.get(variable).cloned().into_iter().fold(client, set)))
    }
}

impl Hosted for redis::Client {
    type Image = Valkey;
    type Config = RedisConnectionInfo;

    const PORT: ContainerPort = VALKEY_PORT;

    fn connected(_: &Stated, host: String, port: u16, config: RedisConnectionInfo) -> Result<Self, ServerError> {
        Ok(Self::open(ConnectionAddr::Tcp(host, port).into_connection_info()?.set_redis_settings(config))?)
    }
}

impl Hosted for aws_sdk_s3::Client {
    type Image = RustFS;
    type Config = aws_sdk_s3::config::Builder;

    const PORT: ContainerPort = ContainerPort::Tcp(9000);

    fn connected(stated: &Stated, host: String, port: u16, config: aws_sdk_s3::config::Builder) -> Result<Self, ServerError> {
        let endpoint = Uri::builder().scheme(Scheme::HTTP).authority(format!("{host}:{port}")).path_and_query("/").build()?;
        let credentials = Credentials::new(read(stated, "RUSTFS_ACCESS_KEY")?, read(stated, "RUSTFS_SECRET_KEY")?, None, None, env!("CARGO_PKG_NAME"));
        Ok(Self::from_conf(config.endpoint_url(endpoint.to_string()).credentials_provider(credentials).force_path_style(true).build()))
    }
}

impl Hosted for ClientConfig {
    type Image = Kafka;
    type Config = Self;

    const PORT: ContainerPort = KAFKA_PORT;

    fn connected(_: &Stated, host: String, port: u16, mut config: Self) -> Result<Self, ServerError> {
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

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Ran {
    pub code: Option<i64>,
    pub output: String,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum ServerError {
    #[error(transparent)]
    Address(#[from] http::Error),
    #[error("server request states no `{variable}`")]
    Unstated { variable: &'static str },
    #[error("docker compose resolved no model of infra/compose.yaml")]
    Compose { source: io::Error },
    #[error("docker compose printed a model of infra/compose.yaml this crate cannot read")]
    Model { source: serde_path_to_error::Error<serde_json::Error> },
    #[error("infra/compose.yaml states no image for service {service}")]
    Unlisted { service: String },
    #[error(transparent)]
    Image(#[from] oci_spec::distribution::ParseError),
    #[error(transparent)]
    Server(#[from] testcontainers::TestcontainersError),
    #[error(transparent)]
    Cache(#[from] redis::RedisError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn defined<I>(service: &str, image: I) -> Result<ContainerRequest<I>, ServerError>
where I: Image {
    let resolved = async {
        let mut compose =
            Command::new("docker").args(["compose", "--file", "-", "--profile", "*", "config", "--format", "json"]).stdin(Stdio::piped()).stdout(Stdio::piped()).stderr(Stdio::inherit()).spawn()?;
        let mut input = compose.stdin.take().ok_or(io::ErrorKind::BrokenPipe)?;
        input.write_all(include_bytes!("../../../../infra/compose.yaml")).await?;
        drop(input);
        let output = compose.wait_with_output().await?;
        output.status.exit_ok().map(|()| output.stdout).map_err(io::Error::other)
    };
    let model = resolved.await.map_err(|source| ServerError::Compose { source })?;
    let mut reader = serde_json::Deserializer::from_slice(&model);
    let composed = serde_path_to_error::deserialize(&mut reader).and_then(|composed| reader.end().map(|()| composed).map_err(|source| serde_path_to_error::Error::new(Track::new().path(), source)));
    let Compose { mut services } = composed.map_err(|source| ServerError::Model { source })?;
    let unlisted = || ServerError::Unlisted { service: service.to_owned() };
    let Service { image: stated, command } = services.remove(service).ok_or_else(unlisted)?;
    let reference = stated.ok_or_else(unlisted)?.parse::<Reference>()?;
    let named = image.with_name(format!("{}/{}", reference.registry(), reference.repository()));
    let tagged = reference.tag().into_iter().fold(named, ImageExt::with_tag);
    Ok(command.into_iter().fold(tagged, ImageExt::with_cmd))
}

pub async fn ran(name: &str, dockerfile: &str, command: impl IntoIterator<Item = impl Into<String>>) -> Result<Ran, ServerError> {
    let image = GenericBuildableImage::new(name, "latest").with_dockerfile_string(dockerfile).build_image().await?;
    let exited = image.with_wait_for(WaitFor::exit(ExitWaitStrategy::new())).with_cmd(command).with_host(GATEWAY, Host::HostGateway).with_startup_timeout(Duration::from_mins(10));
    let container = exited.start().await?;
    let (code, output, errors) = (container.exit_code().await?, container.stdout_to_vec().await?, container.stderr_to_vec().await?);
    Ok(Ran { code, output: format!("{}{}", String::from_utf8_lossy(&output), String::from_utf8_lossy(&errors)) })
}

fn read<'stated>(stated: &'stated Stated, variable: &'static str) -> Result<&'stated str, ServerError> { stated.get(variable).map(String::as_str).ok_or(ServerError::Unstated { variable }) }
