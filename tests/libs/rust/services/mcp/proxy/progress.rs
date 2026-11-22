use core::ops::RangeInclusive;
use core::time::Duration;

use claims::{assert_ok, assert_some};
use rasm_mcp_tests::Id;
use tokio::time;

use super::{INITIALIZE, INITIALIZED, LINE, Peer, asking, joined, proxy, reporting, session};

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn reported(caller: &mut Peer, id: u64) -> Vec<f64> {
    let mut steps = Vec::new();
    loop {
        let seen = caller.next().await;
        match seen.progress() {
            Some(done) if seen.params.progress_token == Some(Id::Text("p".to_owned())) => steps.push(done),
            Some(_) | None => {
                assert_eq!((seen.id.clone(), seen.error.message.clone()), (Some(Id::Number(id)), None), "the call is answered after its progress: {seen:?}");
                break steps;
            }
        }
    }
}

async fn answered_at_once(calls: RangeInclusive<u64>, steps: u32) {
    let (proxy, mut starts, _actor) = proxy(256, LINE);
    let mut server = assert_some!(starts.recv().await).discovered().await;
    let mut caller = joined(&proxy).await;
    for id in calls {
        caller.send(&asking(id)).await;
        let call = server.wire.next().await;
        server.wire.send(&reporting(&call, 1..=steps, "{}")).await;
        assert_eq!(reported(&mut caller, id).await, Vec::from_iter((1..=steps).map(f64::from)), "call {id}: each progress value written with a result is read before it, in the order written");
    }
}

// --- [ROUTING]

#[tokio::test(start_paused = true)]
async fn progress_reaches_its_request_under_the_client_token_and_stops_with_it() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (mut plain, mut tokened) = (joined(&proxy).await, joined(&proxy).await);
    let call = |id: u64, meta: &str| format!(r#"{{"jsonrpc":"2.0","id":{id},"method":"tools/call","params":{{"name":"quiet"{meta}}}}}"#);
    for session in [&mut plain, &mut tokened] {
        session.send(&call(1, "")).await;
        let known = server.wire.next().await;
        server.wire.send(&known.answered("{}")).await;
        assert_eq!(session.next().await.id, Some(Id::Number(1)));
    }
    plain.send(&call(7, "")).await;
    let _held = server.wire.next().await;
    tokened.send(&call(8, r#","_meta":{"progressToken":"p"}"#)).await;
    let owner = server.wire.next().await;
    let token = assert_ok!(serde_json::to_string(&assert_some!(owner.params.meta.clone().and_then(|meta| meta.progress_token))));
    let progress = |done: u64| format!(r#"{{"jsonrpc":"2.0","method":"notifications/progress","params":{{"progressToken":{token},"progress":{done}}}}}"#);
    server.wire.send(&progress(1)).await;
    let reported = tokened.next().await;
    assert_eq!(
        (reported.method.as_deref(), reported.params.progress_token),
        (Some("notifications/progress"), Some(Id::Text("p".to_owned()))),
        "progress reaches the request that set the token, under the client's token"
    );
    plain.silent(Duration::from_millis(100)).await;
    server.wire.send(&owner.answered("{}")).await;
    assert_eq!(tokened.next().await.id, Some(Id::Number(8)));
    server.wire.send(&progress(2)).await;
    tokened.silent(Duration::from_millis(100)).await;
    plain.silent(Duration::from_millis(100)).await;
}

// --- [TRANSLATION]

#[tokio::test(start_paused = true)]
async fn initialize_session_reads_progress_a_discover_server_wrote_with_its_result_first_and_in_order() { answered_at_once(7..=7, 40).await; }

#[tokio::test(flavor = "multi_thread")]
async fn initialize_session_reads_progress_of_calls_a_discover_server_answers_at_once() { answered_at_once(1..=256, 4).await; }

#[tokio::test(start_paused = true)]
async fn progress_of_an_initialize_session_increases_across_the_rounds_of_one_call() {
    let (proxy, mut starts, _actor) = proxy(256, LINE);
    let mut server = assert_some!(starts.recv().await).discovered().await;
    let mut caller = joined(&proxy).await;
    caller.send(&asking(7)).await;
    let first = server.wire.next().await;
    server.wire.send(&reporting(&first, 1..=2, r#"{"resultType":"input_required","requestState":"held"}"#)).await;
    let retried = server.wire.next().await;
    assert_ne!(retried.id, first.id, "the retry is a request of its own, and its progress starts again");
    server.wire.send(&reporting(&retried, 1..=3, "{}")).await;
    assert_eq!(reported(&mut caller, 7).await, [1.0, 2.0, 3.0], "progress under the session's one token only increases");
}

#[tokio::test(start_paused = true)]
async fn initialize_session_that_stops_reading_progress_is_disconnected_alone() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).discovered().await;
    let (mut reader, (mut stalled, serving)) = (joined(&proxy).await, session(&proxy));
    stalled.send(INITIALIZE).await;
    assert_eq!(stalled.next().await.id, Some(Id::Number(0)));
    stalled.send(INITIALIZED).await;
    stalled.send(&asking(7)).await;
    let held = server.wire.next().await;
    let token = assert_ok!(serde_json::to_string(&assert_some!(held.params.meta.and_then(|meta| meta.progress_token))));
    let padding = "x".repeat(32 << 10);
    for done in 1..=32 {
        server.wire.send(&format!(r#"{{"jsonrpc":"2.0","method":"notifications/progress","params":{{"progressToken":{token},"progress":{done},"message":"{padding}"}}}}"#)).await;
    }
    assert_ok!(assert_ok!(assert_ok!(time::timeout(Duration::from_secs(1), serving).await, "a session that stops reading its progress is disconnected")));
    assert_eq!(server.wire.next().await.method.as_deref(), Some("notifications/cancelled"), "the disconnected session's call is cancelled at the server");
    reader.send(&asking(8)).await;
    let call = server.wire.next().await;
    server.wire.send(&reporting(&call, 1..=1, "{}")).await;
    assert_eq!(reported(&mut reader, 8).await, [1.0], "a session that reads keeps its calls and their progress");
    assert!(!server.stop.is_cancelled(), "the server stays");
}
