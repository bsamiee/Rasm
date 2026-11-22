use core::time::Duration;

use claims::{assert_none, assert_ok, assert_some};
use rasm_mcp_tests::Id;
use tokio::time::{self, Instant};

use super::{GRACE, INITIALIZE, INITIALIZED, LINE, SERVED, joined, proxy, session};

// --- [NEGOTIATION]

#[tokio::test(start_paused = true)]
async fn silent_server_is_asked_initialize_after_the_grace_on_one_connection() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await);
    let (mut waiting, _) = session(&proxy);
    waiting.send(INITIALIZE).await;
    assert_eq!(server.wire.next().await.method.as_deref(), Some("server/discover"));
    let asked = Instant::now();
    let initialize = server.wire.next().await;
    assert_eq!(initialize.method.as_deref(), Some("initialize"), "a server that stays silent is asked initialize on the same connection");
    assert_eq!(asked.elapsed(), GRACE, "initialize follows the grace period");
    server.wire.send(&initialize.answered(SERVED)).await;
    assert_eq!(waiting.next().await.result.protocol_version.as_deref(), Some("2025-06-18"), "the waiting session is answered once the server links");
    assert_eq!(server.wire.next().await.method.as_deref(), Some("notifications/initialized"));
}

#[tokio::test(start_paused = true)]
async fn server_that_ends_on_discover_is_asked_initialize_first_with_no_restart_delay() {
    let (_proxy, mut starts, _actor) = proxy(16, LINE);
    let mut first = assert_some!(starts.recv().await);
    assert_eq!(first.wire.next().await.method.as_deref(), Some("server/discover"));
    assert_ok!(first.exit.send("exit status: 1".to_owned()));
    let ended = Instant::now();
    let mut second = assert_some!(starts.recv().await);
    assert_eq!(second.wire.next().await.method.as_deref(), Some("initialize"), "the next start asks initialize first");
    assert_eq!(ended.elapsed(), Duration::ZERO, "the first end counts as no failure, so the next start waits no restart delay");
}

// --- [SHARED]

#[tokio::test(start_paused = true)]
async fn sessions_opening_before_and_after_the_link_share_one_handshake_under_their_own_ids() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let starting = assert_some!(starts.recv().await);
    let opening = |client: &str| INITIALIZE.replace(r#""id":0"#, &format!(r#""id":"{client}""#));
    let mut waiting = Vec::new();
    for client in ["early-0", "early-1", "early-2"] {
        let (mut wire, _) = session(&proxy);
        wire.send(&opening(client)).await;
        waiting.push((client, wire));
    }
    let mut server = starting.linked().await;
    for (client, wire) in &mut waiting {
        let answer = wire.next().await;
        assert_eq!((answer.id, answer.result.server_info.map(|info| info.name)), (Some(Id::Text((*client).to_owned())), Some("s".to_owned())), "a waiter gets its own id and the one handshake");
    }
    let (mut late, _) = session(&proxy);
    late.send(&opening("late")).await;
    let answer = late.next().await;
    assert_eq!((answer.id, answer.result.protocol_version.as_deref()), (Some(Id::Text("late".to_owned())), Some("2025-06-18")), "a later session opens from the stored handshake");
    late.send(INITIALIZED).await;
    server.wire.silent(Duration::from_secs(1)).await;
    assert_none!(starts.try_recv().ok(), "the server started once and was asked initialize once");
}

#[tokio::test(start_paused = true)]
async fn starting_server_survives_its_waiting_session_leaving() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let server = assert_some!(starts.recv().await);
    let (mut waiting, serving) = session(&proxy);
    waiting.send(INITIALIZE).await;
    drop(waiting);
    time::sleep(Duration::from_secs(5)).await;
    assert!(!server.stop.is_cancelled(), "a starting server is not stopped because its waiting session left");
    let _server = server.linked().await;
    assert_ok!(assert_ok!(serving.await));
    assert_none!(starts.try_recv().ok(), "the server started once");
}

#[tokio::test(start_paused = true)]
async fn client_ping_is_answered_by_the_proxy_alone() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut later = joined(&proxy).await;
    later.send(r#"{"jsonrpc":"2.0","id":1,"method":"ping"}"#).await;
    assert_eq!(later.next().await.id, Some(Id::Number(1)));
    server.wire.silent(Duration::from_millis(100)).await;
}
