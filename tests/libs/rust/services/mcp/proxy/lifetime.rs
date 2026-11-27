use core::time::Duration;

use claims::{assert_none, assert_ok, assert_some};
use futures::future;
use rasm_mcp_tests::Id;
use serde_json::json;
use tokio::time::{self, Instant};

use super::{INITIALIZE, LINE, QUIET, exited, joined, proxy, session};

// --- [IDLE]

#[tokio::test(start_paused = true)]
async fn unused_server_stops_three_hundred_times_its_start_cost_after_it_linked() {
    let (_proxy, mut starts, _actor) = proxy(16, LINE);
    let server = assert_some!(starts.recv().await);
    time::sleep(Duration::from_secs(2)).await;
    let server = server.linked().await;
    let linked = Instant::now();
    assert_ok!(time::timeout(Duration::from_secs(601), server.stop.cancelled()).await, "an unused server stops");
    assert_eq!(linked.elapsed().as_secs(), 600, "a 2 s start keeps an unused server 600 s");
}

#[tokio::test(start_paused = true)]
async fn attached_session_holds_its_server_and_the_idle_time_runs_once_it_left() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let server = assert_some!(starts.recv().await);
    time::sleep(Duration::from_secs(2)).await;
    let server = server.linked().await;
    let caller = joined(&proxy).await;
    time::sleep(Duration::from_hours(3) + Duration::from_secs(17)).await;
    assert!(!server.stop.is_cancelled(), "a server with an attached session outlives its 600 s idle time and the 2 h ceiling");
    drop(caller);
    let left = Instant::now();
    assert_ok!(time::timeout(Duration::from_secs(601), server.stop.cancelled()).await, "the server stops once its last session left");
    assert_eq!(left.elapsed().as_secs(), 600, "the 600 s idle time runs from the moment the last session left");
}

#[tokio::test(start_paused = true)]
async fn server_that_never_answers_stops_at_the_first_ceiling_with_no_session_waiting() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let silent = assert_some!(starts.recv().await);
    let (mut waiting, serving) = session(&proxy);
    waiting.send(INITIALIZE).await;
    let started = Instant::now();
    time::sleep(Duration::from_hours(3)).await;
    assert!(!silent.stop.is_cancelled(), "a silent server stays while a session waits for its first answer");
    drop(waiting);
    assert_ok!(assert_ok!(serving.await), "a session waiting for the first answer ends with its client");
    assert_ok!(time::timeout(Duration::from_hours(2), silent.stop.cancelled()).await, "the silent server stops once no session waits");
    assert_eq!(started.elapsed().as_secs() / 3600, 4, "the silent server stops at the next 2 h ceiling: {:?}", started.elapsed());
    drop(silent);
    time::sleep(Duration::from_hours(3)).await;
    assert_none!(starts.try_recv().ok(), "the server started once, and no session asks for it again");
}

// --- [LIVENESS]

#[tokio::test(start_paused = true)]
async fn server_that_answers_no_ping_during_a_call_outlives_a_ten_minute_call() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut caller = joined(&proxy).await;
    caller.send(r#"{"jsonrpc":"2.0","id":7,"method":"tools/call","params":{"name":"hang"}}"#).await;
    let call = server.wire.next().await;
    assert_eq!(call.method.as_deref(), Some("tools/call"));
    let sent = Instant::now();
    assert_eq!(server.wire.next().await.method.as_deref(), Some("ping"), "a server silent for the quiet period with a call in flight is pinged");
    assert_eq!(sent.elapsed(), QUIET, "the ping follows the quiet period");
    time::sleep_until(sent + Duration::from_mins(10)).await;
    assert!(!server.stop.is_cancelled(), "a server never seen to answer a ping during this call is not stopped for its silence");
    server.wire.send(&call.answered("{}")).await;
    let answer = caller.next().await;
    assert_eq!((answer.id, answer.error.message), (Some(Id::Number(7)), None), "the ten minute call is answered under its own id");
}

#[tokio::test(start_paused = true)]
async fn server_that_answered_a_ping_during_a_call_is_replaced_once_it_answers_none() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut caller = joined(&proxy).await;
    caller.send(r#"{"jsonrpc":"2.0","id":7,"method":"tools/call","params":{"name":"hang"}}"#).await;
    assert_eq!(server.wire.next().await.method.as_deref(), Some("tools/call"));
    let answered = server.wire.next().await;
    assert_eq!(answered.method.as_deref(), Some("ping"));
    server.wire.send(&answered.answered("{}")).await;
    assert_eq!(server.wire.next().await.method.as_deref(), Some("ping"), "the server is pinged again after the next quiet period");
    let learned = assert_some!(proxy.status().borrow().profile.clone());
    let responsive = Vec::from_iter(learned.responsive.iter().map(|key| (key.method.to_string(), key.target.as_deref().map(str::to_owned))));
    assert_eq!(responsive, [("tools/call".to_owned(), Some(r#""hang""#.to_owned()))], "the stored profile names the call the server answered a ping during");
    assert_ok!(time::timeout(Duration::from_secs(61), server.stop.cancelled()).await, "a server that answered a ping during this call and answers none inside the one minute grace is stopped");
    assert_ok!(server.exit.send(exited("killed")));
    let answer = caller.next().await;
    assert_eq!(answer.id, Some(Id::Number(7)), "the call in flight is answered under its own id");
    let stated = json!({ "_tag": "Down", "cause": [{ "_tag": "Unresponsive" }, { "_tag": "Exited", "status": "killed", "stderr": [] }], "sent": true, "next": "retry" });
    assert_eq!((answer.error.code, answer.error.data.as_ref()), (Some(-32603), Some(&stated)), "the error states the cause and that the call reached the server: {answer:?}");
}

#[tokio::test(start_paused = true)]
async fn ping_answered_during_one_call_proves_nothing_about_another() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut caller = joined(&proxy).await;
    caller.send(r#"{"jsonrpc":"2.0","id":7,"method":"tools/call","params":{"name":"hang"}}"#).await;
    let attended = server.wire.next().await;
    let answered = server.wire.next().await;
    assert_eq!(answered.method.as_deref(), Some("ping"));
    server.wire.send(&answered.answered("{}")).await;
    server.wire.send(&attended.answered("{}")).await;
    assert_eq!(caller.next().await.id, Some(Id::Number(7)));
    caller.send(r#"{"jsonrpc":"2.0","id":8,"method":"tools/call","params":{"name":"block"}}"#).await;
    let blocking = server.wire.next().await;
    assert_eq!(blocking.method.as_deref(), Some("tools/call"));
    let sent = Instant::now();
    assert_eq!(server.wire.next().await.method.as_deref(), Some("ping"));
    time::sleep_until(sent + Duration::from_mins(10)).await;
    assert!(!server.stop.is_cancelled(), "a ping answered during one tool's call does not stop the server for its silence in another tool's call");
    server.wire.send(&blocking.answered("{}")).await;
    assert_eq!(caller.next().await.id, Some(Id::Number(8)));
}

// --- [SHUTDOWN]

#[tokio::test(start_paused = true)]
async fn drain_waits_for_calls_that_finish_and_no_longer_than_the_settle_window() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut caller = joined(&proxy).await;
    caller.send(r#"{"jsonrpc":"2.0","id":1,"method":"tools/list"}"#).await;
    let listed = server.wire.next().await;
    server.wire.send(&listed.answered(r#"{"tools":[]}"#)).await;
    assert_eq!(caller.next().await.id, Some(Id::Number(1)), "a first answered call marks the method as one that never asks, so later calls run together");
    caller.send(r#"{"jsonrpc":"2.0","id":2,"method":"tools/list"}"#).await;
    caller.send(r#"{"jsonrpc":"2.0","id":3,"method":"tools/list"}"#).await;
    let (quick, _stuck) = (server.wire.next().await, server.wire.next().await);
    let started = Instant::now();
    let answered = async {
        time::sleep(Duration::from_millis(100)).await;
        server.wire.send(&quick.answered(r#"{"tools":[]}"#)).await;
    };
    tokio::join!(proxy.drain(Duration::from_millis(250)), answered);
    assert_eq!(caller.next().await.id, Some(Id::Number(2)), "a call that finishes inside the window is answered");
    assert_eq!(started.elapsed(), Duration::from_millis(350), "the drain ends one settle window after the last answer");
    caller.send(r#"{"jsonrpc":"2.0","id":4,"method":"tools/list"}"#).await;
    assert!(caller.next().await.error.message.as_deref().is_some_and(|message| message.contains("stopping")), "a call sent during the drain is refused at once");
}

#[tokio::test(start_paused = true)]
async fn stop_and_drain_pass_a_server_that_stopped_reading() {
    let (proxy, mut starts, actor) = proxy(4, LINE);
    let server = assert_some!(starts.recv().await).linked().await;
    let mut caller = joined(&proxy).await;
    let flooding = tokio::spawn(async move {
        let padding = "x".repeat(32 << 10);
        for _ in 0..32 {
            caller.send(&format!(r#"{{"jsonrpc":"2.0","method":"notifications/roots/list_changed","params":{{"pad":"{padding}"}}}}"#)).await;
        }
    });
    time::sleep(Duration::from_secs(1)).await;
    assert!(!flooding.is_finished(), "the server reads nothing, so the session's messages back up");
    assert_ok!(time::timeout(Duration::from_secs(1), proxy.drain(Duration::from_millis(250))).await, "a drain passes messages queued behind a server that stopped reading");
    let exited = async {
        server.stop.cancelled().await;
        assert_ok!(server.exit.send(exited("killed")));
    };
    assert_ok!(time::timeout(Duration::from_secs(1), future::join(proxy.stop(), exited)).await, "a stop passes messages queued behind a server that stopped reading");
    assert_ok!(actor.await, "the proxy task ends with the stop");
}
