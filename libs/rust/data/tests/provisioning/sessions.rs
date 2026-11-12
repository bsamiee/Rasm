use core::iter;
use core::time::Duration;

use claims::{assert_err, assert_matches, assert_ok, assert_some};
use futures::future::join_all;
use hdrhistogram::Histogram;
use jiff::Timestamp;
use rasm_provisioning::{Digest, Kind, Postgres, ProvisioningError, Refusal, SessionName, StoreId};
use serde::Serialize;
use sqlx::postgres::{PgPool, PgPoolOptions};
use sqlx::{AssertSqlSafe, Connection as _};
use tokio::io::AsyncWriteExt as _;
use tokio::time::{self, Instant};

use crate::{DISCONNECT_DELAY, DISCONNECT_TRIES, entered, postgres, running, stopped};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Serialize)]
struct Latency {
    calls: u64,
    p50_us: u64,
    p99_us: u64,
    max_us: u64,
}

#[derive(Debug, Serialize)]
struct Load {
    sessions: usize,
    elapsed_ms: u128,
    create: Latency,
    remove: Latency,
    held_refusal_ms: u128,
    checkpoints_requested: i64,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn exists(admin: &PgPool, id: StoreId) -> bool {
    assert_ok!(sqlx::query_scalar::<_, i64>("SELECT count(*) FROM pg_database WHERE datname = $1").bind(id.to_string()).fetch_one(admin).await) == 1
}

fn session(name: &str) -> SessionName { assert_ok!(SessionName::try_new(name)) }

// --- [LEASES]

#[tokio::test(flavor = "multi_thread")]
async fn killed_holder_ends_one_grace_later_and_renewed_session_stays() {
    let (_container, _port, server) = assert_ok!(postgres(&[]).await);
    let (admin, service) = (server.admin.clone(), running(Duration::from_secs(1), Duration::from_secs(4), 0));
    let stores = assert_ok!(service.registry.serve(server).await);
    let (grace, stream) = service.registry.hold(session("held"));
    let holder = tokio::spawn(stream);
    let (held, called) = (assert_ok!(stores.create(session("held")).await), assert_ok!(stores.create(session("called")).await));
    assert_eq!(grace, Duration::from_secs(1), "a hold states the grace that follows its end");
    holder.abort();
    time::sleep(Duration::from_millis(2500)).await;
    assert!(!exists(&admin, held.id).await, "a killed holder's store is gone one grace later");
    assert!(exists(&admin, called.id).await, "a never-held session outlives another session's grace");
    assert_eq!(service.registry.renew(&session("called")), Duration::from_secs(4), "a renewal returns the lease counted from the call");
    time::sleep(Duration::from_millis(2500)).await;
    assert!(exists(&admin, called.id).await, "a renewed session's store stays past its first lease");
    time::sleep(Duration::from_millis(3000)).await;
    assert!(!exists(&admin, called.id).await, "a session ends one lease after its last call");
    assert_ok!(stopped(service).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn open_connection_refuses_removal() {
    let (_container, port, server) = assert_ok!(postgres(&[]).await);
    let (admin, service) = (server.admin.clone(), running(Duration::from_secs(1), Duration::from_mins(1), 0));
    let stores = assert_ok!(service.registry.serve(server).await);
    let store = assert_ok!(stores.create(session("leak")).await);
    let (leaked, started) = (assert_ok!(entered(port, &store, &store.id.to_string()).await), Instant::now());
    assert_matches!(service.registry.remove(store.id).await, Err(ProvisioningError::Store { id, refusal: Refusal::InUse }) if id == store.id, "removal with an open connection is refused");
    assert!(started.elapsed() >= DISCONNECT_DELAY.saturating_mul(assert_ok!(u32::try_from(DISCONNECT_TRIES))), "a held connection is refused once the whole disconnect schedule ran");
    assert!(exists(&admin, store.id).await, "a refused removal leaves the store whole");
    assert_ok!(leaked.close().await);
    assert_ok!(service.registry.remove(store.id).await);
    assert!(!exists(&admin, store.id).await, "removal succeeds once the connection closed");
    assert_ok!(service.registry.remove(store.id).await);
    assert_ok!(stopped(service).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn kept_store_outlives_its_session() {
    let (_container, port, server) = assert_ok!(postgres(&[]).await);
    let (admin, service) = (server.admin.clone(), running(Duration::from_secs(1), Duration::from_mins(1), 0));
    let stores = assert_ok!(service.registry.serve(server).await);
    let (kept, ended) = (assert_ok!(stores.create(session("failed")).await), assert_ok!(stores.create(session("failed")).await));
    assert_ok!(service.registry.keep(kept.id).await);
    assert_ok!(service.registry.end(&session("failed")).await);
    assert!(!exists(&admin, ended.id).await, "an ended session's store is gone");
    assert_ok!(assert_ok!(entered(port, &kept, &kept.id.to_string()).await).close().await);
    assert_ok!(service.registry.remove(kept.id).await);
    assert!(!exists(&admin, kept.id).await, "a kept store ends on removal");
    assert_matches!(service.registry.keep(kept.id).await, Err(ProvisioningError::Store { id, refusal: Refusal::Missing }) if id == kept.id, "keeping a removed store names it missing");
    assert_ok!(stopped(service).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn closed_connections_never_refuse_removal() {
    let (_container, port, server) = assert_ok!(postgres(&["max_connections=200"]).await);
    let service = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = assert_ok!(service.registry.serve(server).await);
    let closing = async || {
        let store = assert_ok!(stores.create(session("closing")).await);
        for opened in join_all(iter::repeat_n(&store, 3).map(async |store| entered(port, store, &store.id.to_string()).await)).await {
            assert_ok!(assert_ok!(opened).close().await);
        }
        service.registry.remove(store.id).await
    };
    for removed in join_all(iter::repeat_with(closing).take(30)).await {
        assert_ok!(removed, "a store with every connection closed is removed");
    }
    assert_ok!(stopped(service).await);
}

#[tokio::test(flavor = "multi_thread")]
async fn kept_store_ends_one_lease_after_the_call() {
    let (_container, _port, server) = assert_ok!(postgres(&[]).await);
    let (admin, service) = (server.admin.clone(), running(Duration::from_secs(1), Duration::from_secs(2), 0));
    let stores = assert_ok!(service.registry.serve(server).await);
    let holder = tokio::spawn(service.registry.hold(session("held")).1);
    let (kept, held) = (assert_ok!(stores.create(session("held")).await), assert_ok!(stores.create(session("held")).await));
    assert_ok!(service.registry.keep(kept.id).await);
    time::sleep(Duration::from_millis(3500)).await;
    assert!(!exists(&admin, kept.id).await, "a kept store ends one lease after the call");
    assert!(exists(&admin, held.id).await, "a held session's other store stays past the kept store's end");
    holder.abort();
    assert_ok!(stopped(service).await);
}

// --- [RECOVERY]

#[tokio::test(flavor = "multi_thread")]
async fn restart_recovers_stores_and_drops_unsealed_builds() {
    let (_container, port, server) = assert_ok!(postgres(&[]).await);
    let (admin, first) = (server.admin.clone(), running(Duration::from_secs(1), Duration::from_mins(1), 0));
    let stores = assert_ok!(first.registry.serve(server.clone()).await);
    let (held, build) = (assert_ok!(stores.create(session("survivor")).await), assert_ok!(stores.open(session("builder"), assert_ok!(Digest::try_new("digest"))).await));
    let [idle, busy] = [(), ()].map(|()| assert_some!(StoreId::new(Timestamp::now())));
    for stray in [idle, busy] {
        assert_ok!(sqlx::query(AssertSqlSafe(format!("CREATE DATABASE {stray}"))).execute(&admin).await);
    }
    let occupied = assert_ok!(entered(port, &held, &busy.to_string()).await);
    assert_ok!(stopped(first).await);
    assert!(exists(&admin, held.id).await && exists(&admin, build.id).await, "shutdown leaves every store in place");

    let second = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let _stores = assert_ok!(second.registry.serve(server).await);
    assert_ok!(assert_ok!(entered(port, &held, &held.id.to_string()).await).close().await);
    assert!(second.registry.holds(Kind::Postgres, held.id), "a store created before the restart is held after it");
    assert!(!exists(&admin, build.id).await, "the start pass drops an unsealed build");
    assert!(!exists(&admin, idle).await, "the start pass drops a store with no label");
    assert!(exists(&admin, busy).await, "the start pass leaves a store with an open connection");
    assert_ok!(occupied.close().await);
    assert_ok!(second.registry.end(&session("survivor")).await);
    assert!(!exists(&admin, held.id).await, "a recovered store ends with its recovered session");
    assert_ok!(stopped(second).await);
}

// --- [LIMITS]

#[tokio::test(flavor = "multi_thread")]
async fn create_past_the_limit_is_refused() {
    let (container, port, server) = assert_ok!(postgres(&["max_connections=20"]).await);
    let service = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = assert_ok!(service.registry.serve(server).await);
    let mut live = Vec::from_iter(join_all(iter::repeat_with(|| stores.create(session("limit"))).take(5)).await.into_iter().map(|created| assert_ok!(created)));
    assert_matches!(stores.create(session("limit")).await, Err(ProvisioningError::Exhausted { kind: Kind::Postgres }), "the create past the limit is refused");
    let first = assert_some!(live.first());
    let open = Vec::from_iter(
        join_all(live.iter().flat_map(|store| iter::repeat_n(store, 3)).map(async |store| entered(port, store, &store.id.to_string()).await)).await.into_iter().map(|opened| assert_ok!(opened)),
    );
    assert_err!(entered(port, first, &first.id.to_string()).await, "a login holds its connection limit at most");
    let logged = String::from_utf8_lossy(&assert_ok!(container.stderr_to_vec().await)).into_owned();
    assert!(!logged.contains("too many clients"), "every login at its connection limit fits the server");
    for connection in open {
        assert_ok!(connection.close().await);
    }
    assert_ok!(service.registry.remove(assert_some!(live.pop()).id).await);
    assert_ok!(stores.create(session("limit")).await);
    assert_ok!(stopped(service).await);
}

// --- [LOAD]

#[tokio::test(flavor = "multi_thread")]
#[ignore = "load measurement, run by name"]
async fn one_hundred_fifty_sessions_create_and_drop() {
    const SESSIONS: usize = 150;
    let (_container, port, server) = assert_ok!(postgres(&["max_connections=600"]).await);
    let admin = assert_ok!(PgPoolOptions::new().max_connections(8).connect_with(server.admin.connect_options().as_ref().clone()).await);
    let service = running(Duration::from_secs(1), Duration::from_mins(1), 0);
    let stores = assert_ok!(service.registry.serve(Postgres { admin: admin.clone(), connections: server.connections }).await);
    let requested = async || assert_ok!(sqlx::query_scalar::<_, i64>("SELECT num_requested FROM pg_stat_checkpointer").fetch_one(&admin).await);
    let (before, started) = (requested().await, Instant::now());
    let timed = join_all((0..SESSIONS).map(async |index| {
        let called = Instant::now();
        let (store, created) = (assert_ok!(stores.create(session(&format!("load-{index}"))).await), called.elapsed());
        assert_ok!(assert_ok!(entered(port, &store, &store.id.to_string()).await).close().await);
        let closed = Instant::now();
        assert_ok!(service.registry.remove(store.id).await, "a store with its connection closed is removed");
        (created, closed.elapsed())
    }));
    let (timed, elapsed) = (timed.await, started.elapsed());
    let summary = |latencies: &mut dyn Iterator<Item = Duration>| {
        let mut latency = assert_ok!(Histogram::<u64>::new(3));
        for taken in latencies {
            assert_ok!(latency.record(assert_ok!(u64::try_from(taken.as_micros()))));
        }
        Latency { calls: latency.len(), p50_us: latency.value_at_quantile(0.5), p99_us: latency.value_at_quantile(0.99), max_us: latency.max() }
    };
    let (create, remove) = (summary(&mut timed.iter().map(|(created, _)| *created)), summary(&mut timed.iter().map(|(_, removed)| *removed)));
    let checkpoints_requested = requested().await.saturating_sub(before);
    let store = assert_ok!(stores.create(session("leak")).await);
    let (leaked, held) = (assert_ok!(entered(port, &store, &store.id.to_string()).await), Instant::now());
    assert_matches!(service.registry.remove(store.id).await, Err(ProvisioningError::Store { id: _, refusal: Refusal::InUse }), "removal with an open connection is refused");
    let measured = Load { sessions: SESSIONS, elapsed_ms: elapsed.as_millis(), create, remove, held_refusal_ms: held.elapsed().as_millis(), checkpoints_requested };
    assert_ok!(leaked.close().await);
    let mut printed = assert_ok!(serde_json::to_vec_pretty(&measured));
    printed.push(b'\n');
    assert_ok!(tokio::io::stdout().write_all(&printed).await);
    assert_ok!(stopped(service).await);
}
