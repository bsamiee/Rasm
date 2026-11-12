use core::time::Duration;
use std::sync::Arc;

use claims::{assert_matches, assert_ok};
use futures::future::join_all;
use rasm_provisioning::{Claim, Digest, ProvisioningError, Refusal, SessionName};
use sqlx::Connection as _;
use tokio::time;

use crate::{clickhouse, entered, postgres, queried, running, stopped};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn session(name: &str) -> SessionName { assert_ok!(SessionName::try_new(name)) }

fn digest() -> Digest { assert_ok!(Digest::try_new("0f3a")) }

// --- [BUILDS]

#[tokio::test(flavor = "multi_thread")]
async fn thirty_sessions_on_one_new_digest_run_one_build() {
    let (_container, port, server) = assert_ok!(postgres(&["max_connections=200"]).await);
    let service = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = Arc::new(assert_ok!(service.registry.serve(server).await));
    let callers = join_all((0..30).map(|caller| {
        let (stores, registry, session) = (Arc::clone(&stores), Arc::clone(&service.registry), session(&format!("caller-{caller}")));
        async move {
            let built = match stores.open(session.clone(), digest()).await {
                Ok(build) => {
                    let mut builder = assert_ok!(entered(port, &build, &build.id.to_string()).await);
                    assert_ok!(sqlx::query("CREATE TABLE item (id int PRIMARY KEY)").execute(&mut builder).await);
                    assert_ok!(sqlx::query("INSERT INTO item VALUES (7)").execute(&mut builder).await);
                    assert_ok!(builder.close().await);
                    assert_ok!(registry.seal(build.id).await);
                    assert_matches!(registry.seal(build.id).await, Err(ProvisioningError::Store { id, refusal: Refusal::Missing }) if id == build.id, "a second seal finds no open build");
                    1_usize
                }
                Err(refused) => {
                    assert_matches!(refused, ProvisioningError::Template { digest: _, claim: Claim::Claimed }, "a second opener learns the digest is claimed");
                    0
                }
            };
            let clone = assert_ok!(stores.clone_template(session, digest()).await);
            let mut inside = assert_ok!(entered(port, &clone, &clone.id.to_string()).await);
            assert_eq!(assert_ok!(sqlx::query_scalar::<_, i32>("SELECT id FROM item").fetch_one(&mut inside).await), 7, "a clone holds the built rows");
            assert_ok!(sqlx::query("ALTER TABLE item ADD COLUMN note text").execute(&mut inside).await);
            assert_ok!(inside.close().await);
            built
        }
    }));
    assert_eq!(callers.await.into_iter().sum::<usize>(), 1, "thirty sessions on one new digest run one build");
    assert_ok!(stopped(service).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn ended_builder_frees_the_next_opener() {
    let (_container, _port, server) = assert_ok!(postgres(&[]).await);
    let service = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = Arc::new(assert_ok!(service.registry.serve(server).await));
    assert_ok!(stores.open(session("killed"), digest()).await);
    assert_matches!(stores.open(session("waiter"), digest()).await, Err(ProvisioningError::Template { digest: _, claim: Claim::Claimed }), "an open build claims its digest");
    let waiter = tokio::spawn({
        let stores = Arc::clone(&stores);
        async move { stores.clone_template(session("waiter"), digest()).await }
    });
    time::sleep(Duration::from_millis(300)).await;
    assert!(!waiter.is_finished(), "a clone waits on the open build");
    assert_ok!(service.registry.end(&session("killed")).await);
    assert_matches!(assert_ok!(waiter.await), Err(ProvisioningError::Template { digest: _, claim: Claim::Unsealed }), "a build that ends unsealed leaves its waiter no template");
    assert_ok!(stores.open(session("waiter"), digest()).await);
    assert_ok!(stopped(service).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn ready_clone_serves_the_next_caller_and_survives_a_restart() {
    let (_container, port, server) = assert_ok!(postgres(&[]).await);
    let first = running(Duration::from_secs(1), Duration::from_mins(1), 2);
    let stores = assert_ok!(first.registry.serve(server.clone()).await);
    let build = assert_ok!(stores.open(session("builder"), digest()).await);
    assert_ok!(first.registry.seal(build.id).await);
    assert_ok!(stores.clone_template(session("first"), digest()).await);
    time::sleep(Duration::from_millis(1500)).await;
    let ready = "SELECT datname::text FROM pg_database WHERE shobj_description(oid, 'pg_database') LIKE '%\"ready\"%' ORDER BY datname";
    let stocked = assert_ok!(sqlx::query_scalar::<_, String>(ready).fetch_all(&server.admin).await);
    assert_eq!(stocked.len(), 1, "a clone leaves one ready clone behind");
    assert_ok!(stopped(first).await);

    let second = running(Duration::from_secs(1), Duration::from_mins(1), 2);
    let stores = assert_ok!(second.registry.serve(server.clone()).await);
    let taken = assert_ok!(stores.clone_template(session("second"), digest()).await);
    assert_eq!(vec![taken.id.to_string()], stocked, "a recovered ready clone serves the next caller");
    assert_ok!(assert_ok!(entered(port, &taken, &taken.id.to_string()).await).close().await);
    assert_ok!(stopped(second).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn clickhouse_clones_hold_tables_rows_and_views() {
    let (_container, port, server) = assert_ok!(clickhouse().await);
    let service = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = Arc::new(assert_ok!(service.registry.serve(server).await));
    let build = assert_ok!(stores.open(session("builder"), digest()).await);
    let builder = queried(port, &build);
    for statement in [
        "CREATE TABLE item (id UInt64, note String) ENGINE = MergeTree ORDER BY id",
        "INSERT INTO item SELECT number, toString(number) FROM numbers(1000)",
        "CREATE VIEW small AS SELECT id FROM item WHERE id < 5",
        "CREATE TABLE scratch (id UInt8) ENGINE = Memory",
    ] {
        assert_ok!(builder.query(statement).execute().await);
    }
    assert_ok!(service.registry.seal(build.id).await);
    assert_matches!(builder.query("SELECT 1").fetch_one::<u8>().await, Err(_), "a sealed template's builder cannot log in");
    let clones = join_all((0..30).map(|caller| {
        let stores = Arc::clone(&stores);
        async move {
            let clone = assert_ok!(stores.clone_template(session(&format!("caller-{caller}")), digest()).await);
            let user = queried(port, &clone);
            assert_ok!(user.query("INSERT INTO item VALUES (1000, 'own')").execute().await);
            assert_ok!(user.query("SELECT (SELECT count() FROM item)::UInt64, (SELECT count() FROM small)::UInt64, (SELECT count() FROM scratch)::UInt64").fetch_one::<(u64, u64, u64)>().await)
        }
    }));
    assert!(clones.await.into_iter().all(|counts| counts == (1001, 5, 0)), "thirty parallel clones each hold the table, its rows, the view, and an empty memory table");
    assert_ok!(stopped(service).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn clickhouse_ready_clone_serves_the_next_caller() {
    let (_container, port, server) = assert_ok!(clickhouse().await);
    let (admin, service) = (server.admin.clone(), running(Duration::from_secs(1), Duration::from_mins(1), 1));
    let stores = assert_ok!(service.registry.serve(server).await);
    let build = assert_ok!(stores.open(session("builder"), digest()).await);
    let builder = queried(port, &build);
    assert_ok!(builder.query("CREATE TABLE item (id UInt64) ENGINE = MergeTree ORDER BY id").execute().await);
    assert_ok!(builder.query("INSERT INTO item SELECT number FROM numbers(1000)").execute().await);
    assert_ok!(service.registry.seal(build.id).await);
    let first = assert_ok!(stores.clone_template(session("first"), digest()).await);
    time::sleep(Duration::from_millis(1500)).await;
    let stocked = assert_ok!(admin.query("SELECT name FROM system.databases WHERE comment LIKE '%\"ready\"%'").fetch_all::<String>().await);
    assert_eq!(stocked.len(), 1, "a clone leaves one ready clone behind");
    let taken = assert_ok!(stores.clone_template(session("second"), digest()).await);
    assert_eq!(vec![taken.id.to_string()], stocked, "a ready clone serves the next caller");
    let user = queried(port, &taken);
    assert_ok!(user.query("INSERT INTO item VALUES (1000)").execute().await);
    assert_eq!(assert_ok!(user.query("SELECT count() FROM item").fetch_one::<u64>().await), 1001, "a ready clone holds the built rows and takes its login's writes");
    assert_eq!(assert_ok!(queried(port, &first).query("SELECT count() FROM item").fetch_one::<u64>().await), 1000, "one clone's rows stay out of a second clone");
    assert_ok!(stopped(service).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn sealed_templates_past_the_bound_leave_least_recently_used_first() {
    let (_container, _port, server) = assert_ok!(postgres(&[]).await);
    let (admin, service) = (server.admin.clone(), running(Duration::from_secs(1), Duration::from_mins(1), 0));
    let stores = assert_ok!(service.registry.serve(server).await);
    let sealed = async |name: &str| {
        let build = assert_ok!(stores.open(session("builder"), assert_ok!(Digest::try_new(name))).await);
        assert_ok!(service.registry.seal(build.id).await);
        build.id.to_string()
    };
    let (oldest, kept, newest) = (sealed("first").await, sealed("second").await, sealed("third").await);
    time::sleep(Duration::from_millis(1500)).await;
    let templates = assert_ok!(sqlx::query_scalar::<_, String>("SELECT datname::text FROM pg_database WHERE datistemplate AND datname LIKE 'store%' ORDER BY datname").fetch_all(&admin).await);
    assert_eq!(templates, [kept, newest], "a third sealed template past a bound of two removes the least recently used");
    let roles = assert_ok!(sqlx::query_scalar::<_, i64>("SELECT count(*) FROM pg_roles WHERE rolname = $1").bind(oldest).fetch_one(&admin).await);
    assert_eq!(roles, 0, "a removed template's builder role is gone");
    assert_matches!(
        stores.clone_template(session("late"), assert_ok!(Digest::try_new("first"))).await,
        Err(ProvisioningError::Template { digest: _, claim: Claim::Unsealed }),
        "a removed template leaves its digest unsealed"
    );
    assert_ok!(stopped(service).await);
}
