use core::time::Duration;
use std::net::TcpListener;
use std::path::Path;

use aws_sdk_s3::config::{Credentials, Region};
use aws_sdk_s3::primitives::ByteStream;
use backon::{ConstantBuilder, Retryable as _};
use claims::{assert_err, assert_matches, assert_ok, assert_some};
use http::{HeaderName, HeaderValue};
use rasm_provisioning::{Bucket, Cache, Granted, Kind, Numbered, ProvisioningError, Refusal, SessionName};
use rasm_test_support::{Hosted as _, defined};
use redis::aio::{ConnectionManagerConfig, MultiplexedConnection};
use redis::{ConnectionAddr, IntoConnectionInfo as _, RedisConnectionInfo, RedisResult};
use secrecy::ExposeSecret as _;
use sqlx::Connection as _;
use testcontainers::core::WaitFor;
use testcontainers::core::wait::HttpWaitStrategy;
use testcontainers::{ContainerAsync, ImageExt as _};
use testcontainers_modules::rustfs::RustFS;
use testcontainers_modules::valkey::{VALKEY_PORT, Valkey};
use tokio::time;

use crate::{COMPOSE, LOCALHOST, clickhouse, entered, postgres, queried, running, stopped};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn session(name: &str) -> SessionName { assert_ok!(SessionName::try_new(name)) }

async fn valkey(published: Option<u16>) -> (ContainerAsync<Valkey>, ConnectionAddr, Cache) {
    let request = assert_ok!(defined(Path::new(COMPOSE), "cache", Valkey::default().with_valkey_extra_flags("--databases 4")).await);
    let request = published.into_iter().fold(request, |request, port| request.with_mapped_port(port, VALKEY_PORT));
    let (container, client) = assert_ok!(redis::Client::started(request, RedisConnectionInfo::default()).await);
    let server = assert_ok!(Cache::connect(&client, ConnectionManagerConfig::new()).await);
    (container, client.get_connection_info().addr().clone(), server)
}

async fn connected(address: &ConnectionAddr, store: &Numbered) -> RedisResult<MultiplexedConnection> {
    let user = RedisConnectionInfo::default().set_username(store.login.id.to_string()).set_password(store.login.password.expose_secret()).set_db(i64::from(store.database));
    redis::Client::open(address.clone().into_connection_info()?.set_redis_settings(user))?.get_multiplexed_async_connection().await
}

// --- [ISOLATION]

#[tokio::test(flavor = "multi_thread")]
async fn postgres_login_opens_its_own_store_alone() {
    let (_container, port, server) = assert_ok!(postgres(Path::new(COMPOSE), &[]).await);
    let first = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = assert_ok!(first.registry.serve(server.clone()).await);
    let (own, other) = (assert_ok!(stores.create(session("isolation")).await), assert_ok!(stores.create(session("isolation")).await));
    let mut inside = assert_ok!(entered(port, &own, &own.id.to_string()).await);
    assert_ok!(sqlx::query("CREATE TABLE item (id int PRIMARY KEY)").execute(&mut inside).await);
    assert_ok!(sqlx::query("CREATE ROLE reader").execute(&mut inside).await);
    assert_ok!(inside.close().await);
    assert_err!(entered(port, &own, &other.id.to_string()).await, "one store's login is refused on a second store");
    assert_ok!(stopped(first).await);

    let second = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let _stores = assert_ok!(second.registry.serve(server.clone()).await);
    assert!(second.registry.holds(Kind::Postgres, own.id) && second.registry.holds(Kind::Postgres, other.id), "a database comment holds each store's session across a restart");
    assert_ok!(second.registry.end(&session("isolation")).await);
    let left = assert_ok!(
        sqlx::query_scalar::<_, i64>("SELECT (SELECT count(*) FROM pg_database WHERE datname LIKE 'store%') + (SELECT count(*) FROM pg_roles WHERE rolname LIKE 'store%' OR rolname = 'reader')")
            .fetch_one(&server.admin)
            .await
    );
    assert_eq!(left, 0, "an ended session leaves no database, login, or role its login created");
    assert_ok!(stopped(second).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn bucket_keys_reach_their_own_bucket_alone() {
    const ROOT: &str = "rustfsadmin";
    let ready = WaitFor::http(HttpWaitStrategy::new("/health/ready").with_port(aws_sdk_s3::Client::PORT).with_expected_status_code(200_u16));
    let request = assert_ok!(defined(Path::new(COMPOSE), "storage", RustFS::default()).await).with_ready_conditions(vec![ready]);
    let (request, region) = (request.with_env_var("RUSTFS_ACCESS_KEY", ROOT).with_env_var("RUSTFS_SECRET_KEY", ROOT), Region::new("us-east-1"));
    let (container, s3) = assert_ok!(aws_sdk_s3::Client::started(request, aws_sdk_s3::Config::builder().behavior_version_latest().region(region.clone())).await);
    let endpoint = format!("http://{LOCALHOST}:{}", assert_ok!(container.get_host_port_ipv4(aws_sdk_s3::Client::PORT).await));
    let granted = |store: &Granted| {
        let keys = Credentials::new(store.credentials.access_key_id(), store.credentials.secret_access_key(), Some(store.credentials.session_token().to_owned()), None, "store");
        aws_sdk_s3::Client::from_conf(aws_sdk_s3::Config::builder().behavior_version_latest().region(region.clone()).endpoint_url(&endpoint).credentials_provider(keys).force_path_style(true).build())
    };
    let sts = aws_sdk_sts::Config::builder().behavior_version_latest().region(region.clone()).endpoint_url(&endpoint).credentials_provider(Credentials::new(ROOT, ROOT, None, None, "root")).build();
    let server = Bucket { s3, sts: aws_sdk_sts::Client::from_conf(sts), duration_seconds: 900, force: (HeaderName::from_static("x-rustfs-force-delete"), HeaderValue::from_static("true")) };

    let first = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = assert_ok!(first.registry.serve(server.clone()).await);
    let (own, other) = (assert_ok!(stores.create(session("isolation")).await), assert_ok!(stores.create(session("isolation")).await));
    let user = granted(&own);
    assert_ok!(user.put_object().bucket(own.id.to_string()).key("object").body(ByteStream::from_static(b"body")).send().await);
    assert_err!(user.put_object().bucket(other.id.to_string()).key("object").body(ByteStream::from_static(b"body")).send().await, "one store's keys are refused on a second bucket");
    assert_ok!(stopped(first).await);

    let second = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = assert_ok!(second.registry.serve(server.clone()).await);
    assert!(second.registry.holds(Kind::Bucket, own.id), "a bucket tag holds the store's session across a restart");
    let renewed = assert_ok!(stores.renew(own.id).await);
    assert_ok!(granted(&renewed).list_objects_v2().bucket(own.id.to_string()).send().await);
    assert_ok!(second.registry.end(&session("isolation")).await);
    assert_err!(server.s3.head_bucket().bucket(own.id.to_string()).send().await, "the force header removes a bucket holding an object");
    assert_err!(server.s3.head_bucket().bucket(other.id.to_string()).send().await, "an ended session leaves no bucket");
    assert_matches!(stores.renew(own.id).await, Err(ProvisioningError::Store { id, refusal: Refusal::Missing }) if id == own.id, "renewing a removed bucket names it missing");
    assert_ok!(stopped(second).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn clickhouse_user_reads_its_own_database_alone() {
    let (_container, port, server) = assert_ok!(clickhouse(Path::new(COMPOSE)).await);
    let first = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = assert_ok!(first.registry.serve(server.clone()).await);
    let (own, other) = (assert_ok!(stores.create(session("isolation")).await), assert_ok!(stores.create(session("isolation")).await));
    let user = queried(port, &own);
    assert_ok!(user.query("CREATE TABLE item (id UInt64) ENGINE = MergeTree ORDER BY id").execute().await);
    assert_err!(user.query(&format!("CREATE TABLE {}.item (id UInt64) ENGINE = MergeTree ORDER BY id", other.id)).execute().await, "one store's user is refused on a second database");
    let running_query = tokio::spawn(async move { user.query("SELECT sleepEachRow(1) FROM numbers(8) SETTINGS max_block_size = 1").fetch_all::<u8>().await });
    time::sleep(Duration::from_millis(500)).await;
    assert_matches!(first.registry.remove(own.id).await, Err(ProvisioningError::Store { id, refusal: Refusal::InUse }) if id == own.id, "a running query refuses removal");
    assert_ok!(assert_ok!(running_query.await));
    let peer = queried(port, &other);
    assert_ok!(peer.query("SELECT 1").fetch_one::<u8>().await);
    assert_ok!(first.registry.remove(other.id).await, "a finished query never refuses removal");
    assert_ok!(stopped(first).await);

    let second = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let _stores = assert_ok!(second.registry.serve(server.clone()).await);
    assert!(second.registry.holds(Kind::Clickhouse, own.id), "a database comment holds the store's session across a restart");
    assert_ok!(second.registry.end(&session("isolation")).await);
    let left = assert_ok!(server.admin.query("SELECT count() FROM merge('system', '^(databases|users)$') WHERE name LIKE 'store%'").fetch_one::<u64>().await);
    assert_eq!(left, 0, "an ended session leaves no database or user");
    assert_ok!(stopped(second).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn cache_user_is_bound_to_its_own_database() {
    let (_container, address, server) = valkey(None).await;

    let first = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = assert_ok!(first.registry.serve(server.clone()).await);
    let [own, other, last] = [assert_ok!(stores.create(session("isolation")).await), assert_ok!(stores.create(session("isolation")).await), assert_ok!(stores.create(session("isolation")).await)];
    assert_matches!(stores.create(session("isolation")).await, Err(ProvisioningError::Exhausted { kind: Kind::Cache }), "a full range of numbered databases refuses a create");
    let mut user = assert_ok!(connected(&address, &own).await);
    assert_ok!(redis::cmd("SET").arg("key").arg("value").query_async::<()>(&mut user).await);
    assert_err!(redis::cmd("SELECT").arg(other.database).query_async::<()>(&mut user).await, "a store's user is refused on a second numbered database");
    assert_err!(redis::cmd("FLUSHALL").query_async::<()>(&mut user).await, "a store's user cannot flush every database");
    assert_err!(redis::cmd("PUBLISH").arg("elsewhere").arg("message").query_async::<i64>(&mut user).await, "a store's user publishes under its own prefix alone");
    assert_ok!(redis::cmd("PUBLISH").arg(format!("{}:events", own.login.id)).arg("message").query_async::<i64>(&mut user).await);
    assert_matches!(first.registry.remove(own.login.id).await, Err(ProvisioningError::Store { id, refusal: Refusal::InUse }) if id == own.login.id, "an open connection refuses removal");
    drop(user);
    assert_ok!(first.registry.remove(own.login.id).await, "a dropped connection never refuses removal");
    assert_ok!(first.registry.keep(last.login.id).await);
    assert_err!(connected(&address, &own).await, "a removed store's user is gone");
    assert_ok!(stopped(first).await);

    let second = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = assert_ok!(second.registry.serve(server).await);
    assert!(second.registry.holds(Kind::Cache, other.login.id) && second.registry.holds(Kind::Cache, last.login.id), "a hash holds each store's session across a restart");
    let again = assert_ok!(stores.create(session("isolation")).await);
    assert_eq!(again.database, own.database, "a removed store's number is free after a restart");
    assert_ok!(second.registry.end(&session("isolation")).await);
    assert_err!(connected(&address, &other).await, "an ended session leaves no user");
    assert_ok!(connected(&address, &last).await, "a kept store outlives its recovered session");
    assert_ok!(stopped(second).await);
}

// --- [RESTART]

#[tokio::test(flavor = "multi_thread")]
async fn cache_restart_ends_its_stores_and_serves_the_next_create() {
    let published = assert_ok!(assert_ok!(TcpListener::bind((LOCALHOST, 0))).local_addr()).port();
    let (container, address, server) = valkey(Some(published)).await;
    let service = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = assert_ok!(service.registry.serve(server).await);
    let before = [assert_ok!(stores.create(session("before")).await), assert_ok!(stores.create(session("before")).await), assert_ok!(stores.create(session("before")).await)];
    assert_ok!(container.stop().await);
    assert_ok!(container.start().await);
    let returning = ConstantBuilder::default().with_delay(Duration::from_millis(100)).with_max_times(100);
    let created = (|| stores.create(session("after"))).retry(returning).when(|refused| matches!(refused, ProvisioningError::Cache { operation: _, source: _ }));
    let after = assert_ok!(created.await, "a restarted server with no label serves a create on every number");
    let mut user = assert_ok!(connected(&address, &after).await);
    assert_ok!(redis::cmd("SET").arg("key").arg("value").query_async::<()>(&mut user).await);
    assert_err!(connected(&address, assert_some!(before.first())).await, "a restart ends every store created before it");
    assert_ok!(service.registry.end(&session("before")).await);
    assert!(before.iter().all(|store| !service.registry.holds(Kind::Cache, store.login.id)), "a session ends over stores its server lost");
    assert_eq!(assert_ok!(redis::cmd("GET").arg("key").query_async::<String>(&mut user).await), "value", "ending lost stores flushes no database a later store holds");
    assert_ok!(stopped(service).await);
}
