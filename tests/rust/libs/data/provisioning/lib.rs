use core::iter;
use core::num::NonZero;
use core::time::Duration;
use std::io;
use std::path::Path;
use std::sync::Arc;

use backon::ExponentialBuilder;
use jiff::Timestamp;
use rasm_provisioning::{Clickhouse, Login, Policy, Postgres, Registry};
use rasm_test_support::{FixtureError, Hosted as _, defined};
use secrecy::ExposeSecret as _;
use sqlx::postgres::{PgConnectOptions, PgConnection, PgPoolOptions};
use sqlx::{ConnectOptions as _, PgPool};
use testcontainers::{ContainerAsync, ImageExt as _};
use testcontainers_modules::{clickhouse, postgres};
use tokio::task::JoinHandle;
use tokio_util::sync::CancellationToken;

// --- [CONSTANTS] -----------------------------------------------------------------------

#[cfg(test)]
pub const COMPOSE: &str = concat!(env!("CARGO_MANIFEST_DIR"), "/../../../../../infra/compose.yaml");
pub const LOCALHOST: &str = "127.0.0.1";
pub const DISCONNECT_DELAY: Duration = Duration::from_millis(100);
pub const DISCONNECT_TRIES: usize = 50;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Running {
    pub registry: Arc<Registry>,
    pub stopping: CancellationToken,
    pub lifetime: JoinHandle<()>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn running(grace: Duration, lease: Duration, ready: usize) -> Running {
    let stopping = CancellationToken::new();
    let reclaim = ExponentialBuilder::default().with_factor(1.0).with_min_delay(Duration::from_millis(200)).without_max_times().with_total_delay(Some(lease));
    let disconnect = ExponentialBuilder::default().with_factor(1.0).with_min_delay(DISCONNECT_DELAY).with_max_times(DISCONNECT_TRIES);
    let registry = Registry::new(Policy { grace, lease, reclaim, disconnect, ready, templates: const { NonZero::new(2).unwrap() }, clock: Timestamp::now }, stopping.clone());
    let lifetime = tokio::spawn({
        let registry = Arc::clone(&registry);
        async move { registry.run().await }
    });
    Running { registry, stopping, lifetime }
}

pub async fn stopped(running: Running) -> io::Result<()> {
    running.stopping.cancel();
    running.lifetime.await.map_err(io::Error::other)
}

pub async fn postgres(compose: &Path, settings: &[&str]) -> Result<(ContainerAsync<postgres::Postgres>, u16, Postgres), FixtureError> {
    const POSTGRES: &str = "postgres";
    let command = iter::chain([POSTGRES], settings.iter().flat_map(|setting| ["-c", setting]));
    let request = defined(compose, POSTGRES, postgres::Postgres::default()).await?.with_cmd(command);
    let (container, admin) = PgPool::started(request, PgPoolOptions::new().max_connections(1)).await?;
    let port = container.get_host_port_ipv4(PgPool::PORT).await?;
    Ok((container, port, Postgres { admin, connections: const { NonZero::new(3).unwrap() } }))
}

pub async fn entered(port: u16, login: &Login, database: &str) -> Result<PgConnection, sqlx::Error> {
    PgConnectOptions::new().host(LOCALHOST).port(port).username(&login.id.to_string()).password(login.password.expose_secret()).database(database).connect().await
}

pub async fn clickhouse(compose: &Path) -> Result<(ContainerAsync<clickhouse::ClickHouse>, u16, Clickhouse), FixtureError> {
    const CLICKHOUSE: &str = "clickhouse";
    let request = defined(compose, CLICKHOUSE, clickhouse::ClickHouse::default()).await?;
    let request = request.with_env_var("CLICKHOUSE_PASSWORD", CLICKHOUSE).with_env_var("CLICKHOUSE_DEFAULT_ACCESS_MANAGEMENT", "1");
    let (container, admin) = ::clickhouse::Client::started(request, ::clickhouse::Client::default()).await?;
    let port = container.get_host_port_ipv4(::clickhouse::Client::PORT).await?;
    Ok((container, port, Clickhouse { admin }))
}

#[must_use]
pub fn queried(port: u16, login: &Login) -> ::clickhouse::Client {
    let name = login.id.to_string();
    ::clickhouse::Client::default().with_url(format!("http://{LOCALHOST}:{port}")).with_user(&name).with_password(login.password.expose_secret()).with_database(name)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[cfg(test)]
mod kinds;
#[cfg(test)]
mod sessions;
#[cfg(test)]
mod templates;
