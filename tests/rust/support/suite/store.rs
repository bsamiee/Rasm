use core::marker::PhantomData;
use core::net::Ipv4Addr;
use core::time::Duration;
use core::{fmt, iter};
use std::io::ErrorKind;
use std::sync::LazyLock;

use backon::{ConstantBuilder, Retryable as _};
use claims::{assert_err, assert_matches, assert_ok};
use futures::{TryStreamExt as _, future};
use object_store::aws::AmazonS3;
use object_store::path::Path;
use object_store::{ObjectStore as _, ObjectStoreExt as _, PutPayload};
use rasm_store_proto::{BucketStore, ClickhouseStore, PostgresStore};
use rasm_test_support::{Cache, Cloned, Endpoint, Failure, Fixture as _, FixtureError, Kind as _, Session, Store, Template};
use sqlx::PgPool;
use tokio::net::TcpListener;
use tokio::sync::oneshot;
use uuid::Uuid;

// --- [TYPES] ---------------------------------------------------------------------------

trait Scripted {
    const KEY: &'static str;
    const SCRIPT: &'static str;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVENTS: [&str; 2] = ["create table entry (id UInt32, amount UInt32) engine = MergeTree order by id", "insert into entry values (1, 10), (2, 32)"];

static RUN: LazyLock<Uuid> = LazyLock::new(Uuid::new_v4);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Script<S>(PhantomData<S>);

impl<S> Template for Script<S>
where S: Scripted
{
    type Driver = PgPool;

    fn digest() -> String { blake3::Hasher::new().update(S::KEY.as_bytes()).update(RUN.as_bytes()).finalize().to_hex().to_string() }

    async fn build(driver: &PgPool) -> Result<(), FixtureError> { sqlx::raw_sql(S::SCRIPT).execute(driver).await.map(drop).map_err(FixtureError::from) }
}

#[derive(Debug)]
struct Ledger;

impl Scripted for Ledger {
    const KEY: &'static str = Self::SCRIPT;
    const SCRIPT: &'static str =
        "create table entry (id integer primary key, amount integer not null); insert into entry values (1, 10), (2, 32); create table build as select gen_random_uuid()::text as mark";
}

#[derive(Debug)]
struct Broken;

impl Scripted for Broken {
    const KEY: &'static str = "repaired";
    const SCRIPT: &'static str = "select from absent";
}

#[derive(Debug)]
struct Mended;

impl Scripted for Mended {
    const KEY: &'static str = Broken::KEY;
    const SCRIPT: &'static str = "create table mended (id integer)";
}

#[derive(Debug)]
struct Events;

impl Template for Events {
    type Driver = clickhouse::Client;

    fn digest() -> String { blake3::Hasher::new().update(EVENTS.concat().as_bytes()).update(RUN.as_bytes()).finalize().to_hex().to_string() }

    async fn build(driver: &clickhouse::Client) -> Result<(), FixtureError> {
        for statement in EVENTS {
            driver.query(statement).execute().await?;
        }
        Ok(())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn endpoint() -> Endpoint { assert_ok!(Endpoint::published()) }

async fn serves(witness: &Session, record: &PostgresStore) -> bool {
    let (_, pool) = assert_ok!(PgPool::opened(witness, record));
    let served = sqlx::query("select 1").execute(&pool).await.is_ok();
    pool.close().await;
    served
}

// --- [KINDS]

#[tokio::test]
async fn postgres_store_is_one_private_database() -> Result<(), Failure<Uuid>> {
    Session::scoped(&endpoint(), async |session| {
        Store::<PgPool>::scoped(session, async |store| {
            let PostgresStore { id: _, host: _, port: _, database, username, password: _, connection_limit } = store.record();
            let served: (String, String) = assert_ok!(sqlx::query_as("select current_database()::text, current_user::text").fetch_one(&**store).await);
            assert_eq!(served, (database.clone(), username.clone()), "the pool reaches the database and login its record names");
            assert_eq!(store.options().get_max_connections(), *connection_limit, "the pool holds at most the connections its login may open");
            let held = future::join_all(iter::repeat_with(|| store.acquire()).take(assert_ok!(usize::try_from(*connection_limit)))).await;
            assert_ok!(held.into_iter().collect::<Result<Vec<_>, _>>(), "the login opens every connection its record allows");
            Store::<PgPool>::scoped(session, async |other| {
                assert_ok!(sqlx::query("create table own (value integer)").execute(&**store).await);
                assert_err!(sqlx::query("select value from own").execute(&**other).await, "a table of one store is absent from another");
            })
            .await
        })
        .await
    })
    .await
}

#[tokio::test]
async fn bucket_store_is_one_private_bucket() -> Result<(), Failure<Uuid>> {
    Session::scoped(&endpoint(), async |session| {
        Store::<AmazonS3>::scoped(session, async |store| {
            let path = Path::from("folder/object");
            assert_ok!(store.put(&path, PutPayload::from_static(b"payload")).await);
            assert_eq!(assert_ok!(assert_ok!(store.get(&path).await).bytes().await).as_ref(), b"payload", "the bucket returns the bytes it stored");
            let listed = assert_ok!(store.list(None).map_ok(|object| object.location).try_collect::<Vec<_>>().await);
            assert_eq!(listed, [path], "the bucket holds this test's object alone");
        })
        .await
    })
    .await
}

#[tokio::test]
async fn bucket_credentials_past_their_life_are_renewed() -> Result<(), Failure<Uuid>> {
    Session::scoped(&endpoint(), async |session| {
        Store::<AmazonS3>::scoped(session, async |store| {
            let lapsed = BucketStore { secret_access_key: "lapsed".to_owned(), credentials_ttl_seconds: 0, ..store.record().clone() };
            let (_, renewing) = assert_ok!(AmazonS3::opened(session, &lapsed));
            assert_ok!(renewing.put(&Path::from("object"), PutPayload::from_static(b"payload")).await, "a driver whose credentials lapsed signs with the ones the service renews");
        })
        .await
    })
    .await
}

#[tokio::test]
async fn clickhouse_store_is_one_private_database() -> Result<(), Failure<Uuid>> {
    Session::scoped(&endpoint(), async |session| {
        Store::<clickhouse::Client>::scoped(session, async |store| {
            let ClickhouseStore { id: _, host: _, http_port: _, database, username, password: _ } = store.record();
            let served = assert_ok!(store.query("select currentDatabase(), currentUser()").fetch_one::<(String, String)>().await);
            assert_eq!(served, (database.clone(), username.clone()), "the client reaches the database and login its record names");
            assert_ok!(store.query("create table own (value UInt32) engine = Memory").execute().await);
            assert_ok!(store.query("insert into own values (10), (32)").execute().await);
            assert_eq!(assert_ok!(store.query("select sum(value) from own").fetch_one::<u64>().await), 42, "the login reads the rows it wrote");
            assert_err!(store.query("create database beside").execute().await, "the login creates nothing outside its database");
        })
        .await
    })
    .await
}

#[tokio::test]
async fn cache_store_is_one_login_with_its_channels() -> Result<(), Failure<Uuid>> {
    Session::scoped(&endpoint(), async |session| {
        Store::<Cache>::scoped(session, async |store| {
            let mut connection = assert_ok!(store.client.get_multiplexed_async_connection().await);
            let login: String = assert_ok!(redis::cmd("ACL").arg("WHOAMI").query_async(&mut connection).await);
            assert_eq!(login, store.record().username, "the client signs in as the login its record names");
            assert_ok!(redis::cmd("SET").arg("key").arg(42).exec_async(&mut connection).await);
            assert_eq!(assert_ok!(redis::cmd("GET").arg("key").query_async::<u32>(&mut connection).await), 42, "the login reads the key it wrote");
            let mut published = async |channel: String| redis::cmd("PUBLISH").arg(channel).arg("event").query_async::<u32>(&mut connection).await;
            assert_ok!(published(format!("{}events", store.channel_prefix)).await, "the login publishes under its channel prefix");
            assert_err!(published("events".to_owned()).await, "the login publishes on no channel outside its prefix");
        })
        .await
    })
    .await
}

// --- [LIFETIME]

#[tokio::test]
async fn silent_endpoint_is_named_by_its_refusal() {
    let listener = assert_ok!(TcpListener::bind((Ipv4Addr::LOCALHOST, 0)).await);
    let silent = Endpoint { store_port: assert_ok!(listener.local_addr()).port(), deadline: None };
    drop(listener);
    assert_matches!(Session::created(&silent).await, Err(FixtureError::Silent { endpoint, source: _ }) if endpoint.port_u16() == Some(silent.store_port));
}

#[tokio::test]
async fn passed_body_has_its_store_dropped() {
    let (seen, record) = oneshot::channel();
    let passed = Session::scoped(&endpoint(), async |session| Store::<PgPool>::scoped(session, async |store| assert_ok!(seen.send(store.record().clone()))).await).await;
    assert_ok!(passed, "a passed body drops its store and ends its session");
    let witness = assert_ok!(Session::created(&endpoint()).await);
    assert!(!serves(&witness, &assert_ok!(record.await)).await, "the store of a passed body is dropped");
    assert_ok!(witness.dropped().await);
}

#[tokio::test]
async fn failed_bodies_have_their_stores_kept_past_their_session() {
    let (panicked, refused) = (oneshot::channel(), oneshot::channel());
    let endpoint = endpoint();
    let session = Session::scoped(&endpoint, async |session| {
        let first = Store::<PgPool>::scoped(session, async |store| {
            assert_ok!(panicked.0.send(store.record().clone()));
            assert!(store.record().id.is_empty(), "this body fails once its store exists");
        });
        assert_matches!(first.await, Err(Failure::Kept(_)), "a panicked body keeps its store");
        let second = Store::<PgPool>::scoped(session, async |store| {
            assert_ok!(refused.0.send(store.record().clone()));
            Err::<(), _>(fmt::Error)
        });
        second.await
    });
    assert_matches!(session.await, Err(Failure::Kept(_)), "a session whose body failed is kept");
    let control = assert_ok!(Session::created(&endpoint).await);
    let abandoned = assert_ok!(Store::<PgPool>::created(&control).await).record().clone();
    drop(control);
    let witness = assert_ok!(Session::created(&endpoint).await);
    let ended = async || (!serves(&witness, &abandoned).await).ok_or(ErrorKind::WouldBlock);
    assert_ok!(ended.retry(ConstantBuilder::new().with_delay(Duration::from_millis(100)).without_max_times()).await, "a store abandoned with its hold ends with its session");
    for kept in [assert_ok!(panicked.1.await), assert_ok!(refused.1.await)] {
        assert!(serves(&witness, &kept).await, "a kept store serves after a store abandoned later ended with its session");
        let (request, pool) = assert_ok!(PgPool::opened(&witness, &kept));
        pool.close().await;
        assert_ok!(witness.client().drop_store(request).await);
    }
    assert_ok!(witness.dropped().await);
}

#[tokio::test]
async fn leaked_connection_fails_its_passed_body() {
    let (seen, leaked) = oneshot::channel();
    let session = assert_ok!(Session::created(&Endpoint { deadline: Some(Duration::from_secs(10)), ..endpoint() }).await);
    let refusal = Store::<PgPool>::scoped(&session, async |store| assert_ok!(seen.send(assert_ok!(store.acquire().await)))).await;
    assert_matches!(refusal, Err(Failure::Undropped(FixtureError::Leaked { store: _, held: 1, source })) if source.code() == tonic::Code::FailedPrecondition);
    drop(assert_ok!(leaked.await));
    assert_ok!(session.dropped().await);
}

// --- [TEMPLATES]

#[tokio::test]
async fn postgres_clones_start_from_one_sealed_template() -> Result<(), Failure<Uuid>> {
    Session::scoped(&endpoint(), async |session| {
        Store::<Cloned<Script<Ledger>>>::scoped(session, async |store| {
            Store::<Cloned<Script<Ledger>>>::scoped(session, async |other| {
                assert_ok!(sqlx::query("delete from entry where id = 1").execute(&**store).await);
                let total = async |pool: &PgPool| assert_ok!(sqlx::query_scalar::<_, i64>("select sum(amount) from entry").fetch_one(pool).await);
                assert_eq!((total(store).await, total(other).await), (32, 42), "each clone starts with the template rows and keeps its own writes");
                let mark = async |pool: &PgPool| assert_ok!(sqlx::query_scalar::<_, String>("select mark from build").fetch_one(pool).await);
                assert_eq!(mark(store).await, mark(other).await, "two clones of one digest come from one build of its template");
            })
            .await
        })
        .await
    })
    .await
}

#[tokio::test]
async fn clickhouse_clones_start_from_one_sealed_template() -> Result<(), Failure<Uuid>> {
    Session::scoped(&endpoint(), async |session| {
        let (first, second) = future::join(
            Store::<Cloned<Events>>::scoped(session, async |store| {
                assert_eq!(assert_ok!(store.query("select sum(amount) from entry").fetch_one::<u64>().await), 42, "a clone holds the template rows");
            }),
            Store::<Cloned<Events>>::scoped(session, async |store| {
                assert_eq!(assert_ok!(store.query("select sum(amount) from entry").fetch_one::<u64>().await), 42, "a clone holds the template rows");
            }),
        )
        .await;
        assert_ok!(first.and(second), "two clones built at once wait for one template and read its rows");
    })
    .await
}

#[tokio::test]
async fn failed_build_abandons_its_template() -> Result<(), Failure<Uuid>> {
    Session::scoped(&endpoint(), async |session| {
        assert_matches!(Store::<Cloned<Script<Broken>>>::scoped(session, async |_| ()).await, Err(Failure::Uncreated(FixtureError::Postgres(_))), "a failed build fails with its cause");
        Store::<Cloned<Script<Mended>>>::scoped(session, async |store| {
            assert_eq!(assert_ok!(sqlx::query("select id from mended").execute(&**store).await).rows_affected(), 0, "the clone holds the table the repaired build made");
        })
        .await
    })
    .await
}
