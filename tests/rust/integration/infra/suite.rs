use core::time::Duration;
use std::path::Path;

use aws_sdk_s3::primitives::ByteStream;
use backon::{ConstantBuilder, Retryable as _};
use claims::{assert_ok, assert_some};
use futures::future;
use oci_spec::distribution::Reference;
use rasm_infra_tests::{Hosted as _, defined};
use rdkafka::consumer::{Consumer as _, StreamConsumer};
use rdkafka::producer::{FutureProducer, FutureRecord};
use rdkafka::{ClientConfig, Message as _};
use redis::{AsyncConnectionConfig, RedisConnectionInfo};
use sqlx::PgPool;
use sqlx::postgres::PgPoolOptions;
use testcontainers::core::WaitFor;
use testcontainers::core::wait::HttpWaitStrategy;
use testcontainers::{ContainerRequest, ImageExt as _};
use testcontainers_modules::clickhouse::ClickHouse;
use testcontainers_modules::kafka::apache::Kafka;
use testcontainers_modules::postgres::Postgres;
use testcontainers_modules::rustfs::RustFS;
use testcontainers_modules::valkey::Valkey;

// --- [CONSTANTS] -----------------------------------------------------------------------

const COMPOSE: &str = "../../../../infra/compose.yaml";

// --- [OPERATIONS] ----------------------------------------------------------------------

#[tokio::test]
async fn postgres_server_keeps_committed_rows_across_a_restart() {
    let (server, before) = assert_ok!(PgPool::started(assert_ok!(defined(Path::new(COMPOSE), "postgres", Postgres::default()).await), PgPoolOptions::new()).await);
    assert_ok!(sqlx::raw_sql("create table own (value integer); insert into own values (42)").execute(&before).await);
    before.close().await;
    assert_ok!(server.stop().await);
    assert!(!assert_ok!(server.is_running().await), "the server stops when its test stops it");
    assert_ok!(server.start().await);
    let port = assert_ok!(server.get_host_port_ipv4(PgPool::PORT).await);
    let options = before.connect_options().as_ref().clone().port(port);
    let reread = PgPoolOptions::new().connect_lazy_with(options);
    let read = (async || sqlx::query_scalar::<_, i32>("select value from own").fetch_one(&reread).await).retry(ConstantBuilder::new().with_delay(Duration::from_millis(100)).without_max_times());
    assert_eq!(assert_ok!(read.await), 42, "a restarted server serves the row committed before its stop");
    reread.close().await;
}

#[tokio::test]
async fn clickhouse_server_runs_the_release_the_compose_file_states() {
    let request: ContainerRequest<ClickHouse> = assert_ok!(defined(Path::new(COMPOSE), "clickhouse", ClickHouse::default()).await).with_env_var("CLICKHOUSE_PASSWORD", "suite");
    let release = assert_some!(assert_ok!(request.descriptor().parse::<Reference>()).tag().map(str::to_owned));
    let (_server, client) = assert_ok!(clickhouse::Client::started(request, clickhouse::Client::default()).await);
    assert_eq!(assert_ok!(client.query("select version()").fetch_one::<String>().await), release, "the hosted server is the release the compose file states");
}

#[tokio::test]
async fn valkey_server_answers_a_paused_command_once_it_resumes() {
    let (server, client) = assert_ok!(redis::Client::started(assert_ok!(defined(Path::new(COMPOSE), "cache", Valkey::default()).await), RedisConnectionInfo::default()).await);
    let mut connection = assert_ok!(client.get_multiplexed_async_connection_with_config(&AsyncConnectionConfig::new().set_response_timeout(None)).await);
    assert_ok!(redis::cmd("SET").arg("key").arg(42).exec_async(&mut connection).await);
    assert_ok!(server.pause().await);
    let asked = redis::cmd("GET").arg("key").to_owned();
    let (read, resumed) = future::join(asked.query_async::<u32>(&mut connection), server.unpause()).await;
    assert_ok!(resumed);
    assert_eq!(assert_ok!(read), 42, "a command sent to a paused server is answered once it resumes");
}

#[tokio::test]
async fn storage_server_returns_the_object_it_stored() {
    let ready = WaitFor::http(HttpWaitStrategy::new("/health/ready").with_port(aws_sdk_s3::Client::PORT).with_expected_status_code(200_u16));
    let request = assert_ok!(defined(Path::new(COMPOSE), "storage", RustFS::default()).await).with_ready_conditions(vec![ready]);
    let request = request.with_env_var("RUSTFS_ACCESS_KEY", "suite").with_env_var("RUSTFS_SECRET_KEY", "suitesecret");
    let config = aws_sdk_s3::config::Builder::new().behavior_version_latest().region(aws_sdk_s3::config::Region::from_static("us-east-1"));
    let (_server, client) = assert_ok!(aws_sdk_s3::Client::started(request, config).await);
    assert_ok!(client.create_bucket().bucket("suite").send().await);
    assert_ok!(client.put_object().bucket("suite").key("object").body(ByteStream::from_static(b"payload")).send().await);
    let read = assert_ok!(assert_ok!(client.get_object().bucket("suite").key("object").send().await).body.collect().await);
    assert_eq!(read.into_bytes().as_ref(), b"payload", "the hosted server returns the bytes it stored");
}

#[tokio::test]
async fn kafka_server_delivers_a_produced_record() {
    let (_server, mut config) = assert_ok!(ClientConfig::started(Kafka::default().into(), ClientConfig::new()).await);
    let producer: FutureProducer = assert_ok!(config.create());
    assert_ok!(producer.send(FutureRecord::to("events").key("key").payload("event"), Duration::from_secs(30)).await);
    let consumer: StreamConsumer = assert_ok!(config.set("group.id", "suite").set("auto.offset.reset", "earliest").create());
    assert_ok!(consumer.subscribe(&["events"]));
    assert_eq!(assert_ok!(consumer.recv().await).payload(), Some(b"event".as_slice()), "a consumer reads the record a producer sent");
}
