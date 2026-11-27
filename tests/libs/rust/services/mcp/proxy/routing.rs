use core::time::Duration;

use claims::{assert_ok, assert_some};
use rasm_mcp_tests::Id;
use serde_json::{Value, json};
use tokio::time::{self, Instant};

use super::{GRACE, INITIALIZE, LINE, SERVED, declared, exited, joined, proxy};

// --- [SUBSCRIPTIONS]

#[tokio::test(start_paused = true)]
async fn subscriptions_are_restored_after_a_restart_and_released_with_the_last_subscriber() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (mut first, mut second) = (joined(&proxy).await, joined(&proxy).await);
    first.send(r#"{"jsonrpc":"2.0","id":1,"method":"resources/subscribe","params":{"uri":"file:///a"}}"#).await;
    let subscribed = server.wire.next().await;
    assert_eq!(subscribed.params.uri.as_deref(), Some("file:///a"));
    server.wire.send(&subscribed.answered("{}")).await;
    assert_eq!(first.next().await.id, Some(Id::Number(1)));
    second.send(r#"{"jsonrpc":"2.0","id":1,"method":"resources/subscribe","params":{"uri":"file:///a"}}"#).await;
    assert_eq!(second.next().await.id, Some(Id::Number(1)), "a second subscriber of one URI is answered without a second server request");
    server.wire.send(r#"{"jsonrpc":"2.0","method":"notifications/resources/updated","params":{"uri":"file:///a"}}"#).await;
    assert_eq!((first.next().await.method.as_deref(), second.next().await.method.as_deref()), (Some("notifications/resources/updated"), Some("notifications/resources/updated")));
    assert_ok!(server.exit.send(exited("exit status: 9")));
    first.send(r#"{"jsonrpc":"2.0","id":2,"method":"tools/list"}"#).await;
    let stated = json!({ "_tag": "Down", "cause": [{ "_tag": "Exited", "status": "exit status: 9", "stderr": [] }], "sent": false, "next": "retry" });
    let refused = first.next().await.error;
    assert_eq!((refused.code, refused.data), (Some(-32603), Some(stated)), "inside the backoff window a call names the cause");
    time::sleep(Duration::from_secs(1)).await;
    first.send(r#"{"jsonrpc":"2.0","id":3,"method":"tools/list"}"#).await;
    let mut restarted = assert_some!(starts.recv().await);
    let initialize = restarted.wire.next().await;
    assert_eq!(initialize.method.as_deref(), Some("initialize"), "a server known to be legacy is asked initialize first");
    restarted.wire.send(&initialize.answered(SERVED)).await;
    assert_eq!(restarted.wire.next().await.method.as_deref(), Some("notifications/initialized"));
    let restored = restarted.wire.next().await;
    assert_eq!((restored.method.as_deref(), restored.params.uri.as_deref()), (Some("resources/subscribe"), Some("file:///a")), "the subscription is restored on the new server");
    assert_eq!(first.next().await.method.as_deref(), Some("notifications/tools/list_changed"), "sessions are told the lists changed after a restart");
    let woken = restarted.wire.next().await;
    assert_eq!(woken.method.as_deref(), Some("tools/list"), "the call that woke the server is forwarded after the handshake");
    drop(first);
    assert_eq!(restarted.wire.next().await.method.as_deref(), Some("notifications/cancelled"), "a leaving session's open call is cancelled at the server");
    restarted.wire.silent(Duration::from_secs(1)).await;
    drop(second);
    let released = restarted.wire.next().await;
    assert_eq!((released.method.as_deref(), released.params.uri.as_deref()), (Some("resources/unsubscribe"), Some("file:///a")), "the subscription is released with its last subscriber");
}

#[tokio::test(start_paused = true)]
async fn unsubscribe_reaches_the_server_from_the_last_subscriber_alone() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (mut first, mut second, mut outsider) = (joined(&proxy).await, joined(&proxy).await, joined(&proxy).await);
    let line = |id: u64, method: &str| format!(r#"{{"jsonrpc":"2.0","id":{id},"method":"{method}","params":{{"uri":"file:///a"}}}}"#);
    first.send(&line(1, "resources/subscribe")).await;
    let subscribed = server.wire.next().await;
    server.wire.send(&subscribed.answered("{}")).await;
    assert_eq!(first.next().await.id, Some(Id::Number(1)));
    second.send(&line(1, "resources/subscribe")).await;
    assert_eq!(second.next().await.id, Some(Id::Number(1)));
    for (session, holder) in [(&mut outsider, "a session that never subscribed"), (&mut first, "a subscriber with another left")] {
        session.send(&line(2, "resources/unsubscribe")).await;
        assert_eq!(session.next().await.id, Some(Id::Number(2)), "{holder} is answered by the proxy");
        server.wire.silent(Duration::from_millis(100)).await;
    }
    second.send(&line(2, "resources/unsubscribe")).await;
    let released = server.wire.next().await;
    assert_eq!((released.method.as_deref(), released.params.uri.as_deref()), (Some("resources/unsubscribe"), Some("file:///a")), "the last subscriber's unsubscribe reaches the server");
    server.wire.send(&released.answered("{}")).await;
    assert_eq!(second.next().await.id, Some(Id::Number(2)));
    first.send(&line(3, "resources/subscribe")).await;
    assert_eq!(server.wire.next().await.method.as_deref(), Some("resources/subscribe"), "a released URI is subscribed at the server again");
}

#[tokio::test(start_paused = true)]
async fn leaving_session_takes_its_parked_calls_with_it() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut leaving = joined(&proxy).await;
    leaving.send(r#"{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"ask"}}"#).await;
    assert_eq!(server.wire.next().await.method.as_deref(), Some("tools/call"));
    leaving.send(r#"{"jsonrpc":"2.0","id":2,"method":"resources/subscribe","params":{"uri":"file:///a"}}"#).await;
    server.wire.silent(Duration::from_millis(100)).await;
    drop(leaving);
    assert_eq!(server.wire.next().await.method.as_deref(), Some("notifications/cancelled"), "the leaving session's open call is cancelled at the server");
    server.wire.silent(Duration::from_secs(1)).await;
}

// --- [SERVER_REQUESTS]

#[tokio::test(start_paused = true)]
async fn server_requests_reach_the_session_whose_call_caused_them() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (mut first, mut second) = (joined(&proxy).await, joined(&proxy).await);
    let ask = |id: u64, tool: &str| format!(r#"{{"jsonrpc":"2.0","id":{id},"method":"tools/call","params":{{"name":"{tool}"}}}}"#);
    let asked = |id: &str| format!(r#"{{"jsonrpc":"2.0","id":"{id}","method":"elicitation/create","params":{{"message":"who"}}}}"#);
    first.send(&ask(1, "ask")).await;
    let one = server.wire.next().await;
    second.send(&ask(1, "ask")).await;
    server.wire.silent(Duration::from_millis(100)).await;
    server.wire.send(&asked("e1")).await;
    assert_eq!(first.next().await.id, Some(Id::Text("e1".to_owned())), "a tool not yet known runs alone, so its request has one possible caller");
    first.send(r#"{"jsonrpc":"2.0","id":"e1","result":{"action":"accept","content":{"answer":"first"}}}"#).await;
    assert_eq!(server.wire.next().await.result.content.and_then(|content| content.answer).as_deref(), Some("first"), "the answer returns to the server");
    server.wire.silent(Duration::from_millis(100)).await;
    server.wire.send(&one.answered("{}")).await;
    assert_eq!(first.next().await.id, Some(Id::Number(1)));
    let two = server.wire.next().await;
    server.wire.send(&asked("e2")).await;
    assert_eq!(second.next().await.id, Some(Id::Text("e2".to_owned())), "a tool known to ask runs alone again");
    second.send(r#"{"jsonrpc":"2.0","id":"e2","result":{"action":"decline"}}"#).await;
    assert_eq!(server.wire.next().await.id, Some(Id::Text("e2".to_owned())));
    server.wire.send(&two.answered("{}")).await;
    assert_eq!(second.next().await.id, Some(Id::Number(1)));
    for (session, id) in [(&mut first, 2), (&mut second, 2)] {
        session.send(&ask(id, "quiet")).await;
        let call = server.wire.next().await;
        server.wire.send(&call.answered("{}")).await;
        assert_eq!(session.next().await.id, Some(Id::Number(id)));
    }
    first.send(&ask(3, "quiet")).await;
    let earlier = server.wire.next().await;
    second.send(&ask(3, "quiet")).await;
    let later = server.wire.next().await;
    server.wire.send(&asked("e3")).await;
    first.silent(Duration::from_millis(100)).await;
    second.silent(Duration::from_millis(100)).await;
    server.wire.send(&earlier.answered("{}")).await;
    assert_eq!(first.next().await.id, Some(Id::Number(3)));
    assert_eq!(second.next().await.id, Some(Id::Text("e3".to_owned())), "with two calls in flight the request waits, then goes to the one call still open");
    second.send(r#"{"jsonrpc":"2.0","id":"e3","result":{"action":"decline"}}"#).await;
    assert_eq!(server.wire.next().await.id, Some(Id::Text("e3".to_owned())));
    server.wire.send(&later.answered("{}")).await;
    assert_eq!(second.next().await.id, Some(Id::Number(3)));
    first.send(&ask(4, "quiet")).await;
    let alone = server.wire.next().await;
    second.send(&ask(4, "quiet")).await;
    server.wire.silent(Duration::from_millis(100)).await;
    server.wire.send(&alone.answered("{}")).await;
    assert_eq!(first.next().await.id, Some(Id::Number(4)));
    let waited = server.wire.next().await;
    assert_eq!(waited.method.as_deref(), Some("tools/call"), "a tool seen to ask with two calls open runs alone from then on, as the tool first seen asking does");
    server.wire.send(&waited.answered("{}")).await;
    assert_eq!(second.next().await.id, Some(Id::Number(4)));
}

#[tokio::test(start_paused = true)]
async fn server_request_the_calling_session_declared_no_capability_for_is_refused_to_the_server() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (mut bare, _) = declared(&proxy, &INITIALIZE.replace(r#""capabilities":{"elicitation":{}}"#, r#""capabilities":{}"#)).await;
    let _capable = joined(&proxy).await;
    bare.send(r#"{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"sample"}}"#).await;
    let call = server.wire.next().await;
    assert_eq!(call.params.meta.and_then(|meta| meta.client_capabilities), Some(json!({})), "the call holds the capabilities its own session declared");
    server.wire.send(r#"{"jsonrpc":"2.0","id":"s1","method":"sampling/createMessage","params":{"messages":[],"maxTokens":1}}"#).await;
    let refused = server.wire.next().await;
    assert_eq!(refused.id, Some(Id::Text("s1".to_owned())), "the server learns at once that the calling session cannot sample");
    assert!(refused.error.message.as_deref().is_some_and(|message| message.contains("sampling/createMessage")), "the refusal names the request: {refused:?}");
    bare.silent(Duration::from_millis(100)).await;
}

#[tokio::test(start_paused = true)]
async fn roots_request_outside_any_call_is_answered_once_with_the_roots_each_declaring_session_lists() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let rooted = INITIALIZE.replace(r#""capabilities":{"elicitation":{}}"#, r#""capabilities":{"roots":{}}"#);
    let (mut first, mut second, mut refusing, mut leaving) =
        (declared(&proxy, &rooted).await.0, declared(&proxy, &rooted).await.0, declared(&proxy, &rooted).await.0, declared(&proxy, &rooted).await.0);
    let mut bare = joined(&proxy).await;
    server.wire.send(r#"{"jsonrpc":"2.0","id":"r1","method":"roots/list"}"#).await;
    for session in [&mut first, &mut second, &mut refusing, &mut leaving] {
        assert_eq!(session.next().await.method.as_deref(), Some("roots/list"), "each session that declares roots is asked");
    }
    bare.silent(Duration::from_millis(100)).await;
    first.send(r#"{"jsonrpc":"2.0","id":"r1","result":{"roots":[{"uri":"file:///work/a","name":"a"}]}}"#).await;
    second.send(r#"{"jsonrpc":"2.0","id":"r1","result":{"roots":[{"uri":"file:///work/b","name":"b"},{"uri":"file:///work/a","name":"a"}]}}"#).await;
    refusing.send(r#"{"jsonrpc":"2.0","id":"r1","error":{"code":-32603,"message":"no roots here"}}"#).await;
    server.wire.silent(Duration::from_millis(100)).await;
    drop(leaving);
    let merged = server.wire.next().await;
    let mut listed = assert_some!(merged.result.roots, "the server is answered with a root list: {merged:?}");
    listed.sort_unstable_by_key(Value::to_string);
    let expected = vec![json!({"uri": "file:///work/a", "name": "a"}), json!({"uri": "file:///work/b", "name": "b"})];
    assert_eq!((merged.id, listed), (Some(Id::Text("r1".to_owned())), expected), "one answer holds the roots of each session that listed them, a root two sessions share once");
    server.wire.silent(Duration::from_millis(100)).await;
}

#[tokio::test(start_paused = true)]
async fn roots_request_outside_any_call_is_answered_with_no_root_once_every_asked_session_left() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (mut leaving, _) = declared(&proxy, &INITIALIZE.replace(r#""capabilities":{"elicitation":{}}"#, r#""capabilities":{"roots":{}}"#)).await;
    let _bare = joined(&proxy).await;
    let asked = Instant::now();
    server.wire.send(r#"{"jsonrpc":"2.0","id":"r1","method":"roots/list"}"#).await;
    assert_eq!(leaving.next().await.method.as_deref(), Some("roots/list"));
    drop(leaving);
    let merged = server.wire.next().await;
    assert_eq!((merged.id, merged.result.roots, merged.error.message), (Some(Id::Text("r1".to_owned())), Some(Vec::new()), None), "the server is answered with an empty root list");
    assert!(asked.elapsed() < GRACE, "the last asked session leaving answers before the deadline: {:?}", asked.elapsed());
}

#[tokio::test(start_paused = true)]
async fn roots_request_outside_any_call_is_answered_at_its_deadline_with_the_roots_listed_by_then() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let rooted = INITIALIZE.replace(r#""capabilities":{"elicitation":{}}"#, r#""capabilities":{"roots":{}}"#);
    let (mut first, mut second, mut silent) = (declared(&proxy, &rooted).await.0, declared(&proxy, &rooted).await.0, declared(&proxy, &rooted).await.0);
    let asked = Instant::now();
    server.wire.send(r#"{"jsonrpc":"2.0","id":"r1","method":"roots/list"}"#).await;
    for session in [&mut first, &mut second, &mut silent] {
        assert_eq!(session.next().await.method.as_deref(), Some("roots/list"), "each session that declares roots is asked");
    }
    first.send(r#"{"jsonrpc":"2.0","id":"r1","result":{"roots":[{"uri":"file:///work/a","name":"a"}]}}"#).await;
    second.send(r#"{"jsonrpc":"2.0","id":"r1","result":{"roots":[{"uri":"file:///work/b","name":"b"}]}}"#).await;
    let merged = server.wire.next().await;
    let mut listed = assert_some!(merged.result.roots, "the server is answered with a root list: {merged:?}");
    listed.sort_unstable_by_key(Value::to_string);
    let expected = vec![json!({"uri": "file:///work/a", "name": "a"}), json!({"uri": "file:///work/b", "name": "b"})];
    assert_eq!((merged.id, listed, asked.elapsed()), (Some(Id::Text("r1".to_owned())), expected, GRACE), "a session that stays silent holds the answer no longer than the handshake time");
}

#[tokio::test(start_paused = true)]
async fn roots_listed_after_the_deadline_of_their_request_never_reach_the_server() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (mut late, _) = declared(&proxy, &INITIALIZE.replace(r#""capabilities":{"elicitation":{}}"#, r#""capabilities":{"roots":{}}"#)).await;
    let asked = Instant::now();
    server.wire.send(r#"{"jsonrpc":"2.0","id":"r1","method":"roots/list"}"#).await;
    assert_eq!(late.next().await.method.as_deref(), Some("roots/list"));
    let merged = server.wire.next().await;
    let answered = (merged.id, merged.result.roots, merged.error.message, asked.elapsed());
    assert_eq!(answered, (Some(Id::Text("r1".to_owned())), Some(Vec::new()), None, GRACE), "the server is answered with an empty root list once the handshake time passed");
    late.send(r#"{"jsonrpc":"2.0","id":"r1","result":{"roots":[{"uri":"file:///work/late"}]}}"#).await;
    server.wire.silent(Duration::from_secs(1)).await;
}

#[tokio::test(start_paused = true)]
async fn roots_request_during_one_call_reaches_the_calling_session_alone() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let rooted = INITIALIZE.replace(r#""capabilities":{"elicitation":{}}"#, r#""capabilities":{"roots":{}}"#);
    let ((mut calling, _), (mut other, _)) = (declared(&proxy, &rooted).await, declared(&proxy, &rooted).await);
    calling.send(r#"{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"scan"}}"#).await;
    let call = server.wire.next().await;
    server.wire.send(r#"{"jsonrpc":"2.0","id":"r1","method":"roots/list"}"#).await;
    assert_eq!(calling.next().await.method.as_deref(), Some("roots/list"), "the session whose call is in flight is asked");
    other.silent(Duration::from_millis(100)).await;
    calling.send(r#"{"jsonrpc":"2.0","id":"r1","result":{"roots":[{"uri":"file:///work/a"}]}}"#).await;
    let listed = server.wire.next().await;
    assert_eq!((listed.id, listed.result.roots), (Some(Id::Text("r1".to_owned())), Some(vec![json!({"uri": "file:///work/a"})])), "the server receives the calling session's roots alone");
    server.wire.send(&call.answered("{}")).await;
    assert_eq!(calling.next().await.id, Some(Id::Number(1)));
}

#[tokio::test(start_paused = true)]
async fn server_ping_is_answered_by_the_proxy_under_its_own_id() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut client = joined(&proxy).await;
    server.wire.send(r#"{"jsonrpc":"2.0","id":7,"method":"ping"}"#).await;
    let pong = server.wire.next().await;
    assert_eq!((pong.id, pong.method, pong.error.message), (Some(Id::Number(7)), None, None), "the proxy answers the ping itself under its own id");
    client.silent(Duration::from_millis(100)).await;
}

// --- [CLIENT_REQUESTS]

#[tokio::test(start_paused = true)]
async fn requests_carry_their_session_client_context_whatever_their_params() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut client = joined(&proxy).await;
    for (id, line) in [
        (1, r#"{"jsonrpc":"2.0","id":1,"method":"tools/list"}"#),
        (2, r#"{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}"#),
        (3, r#"{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"echo","arguments":{"text":"x"}}}"#),
        (4, r#"{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"_meta":{"progressToken":"p"},"name":"echo"}}"#),
        (5, r#"{"jsonrpc":"2.0","id":5,"method":"tools/call","params":{"_meta":{},"name":"echo"}}"#),
    ] {
        client.send(line).await;
        let forwarded = server.wire.next().await;
        let meta = assert_some!(forwarded.params.meta.clone(), "request {id} reaches the server with _meta: {forwarded:?}");
        assert_eq!((meta.client_capabilities, meta.client_info.map(|info| info.name)), (Some(json!({"elicitation": {}})), Some("t".to_owned())), "request {id}");
        server.wire.send(&forwarded.answered("{}")).await;
        assert_eq!(client.next().await.id, Some(Id::Number(id)));
    }
}

#[tokio::test(start_paused = true)]
async fn request_id_returns_unchanged_whatever_its_json_type() {
    let (proxy, mut starts, _actor) = proxy(64, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut client = joined(&proxy).await;
    for id in ["0", "-1", "42", r#""42""#, r#""0""#, r#""""#, r#""-1""#, r#""a:b""#, r#""urn:uuid:1234""#, "9007199254740991", r#""xxxxxxxxxxxxxxxxxxxxxxxx""#] {
        client.send(&format!(r#"{{"jsonrpc":"2.0","id":{id},"method":"tools/list"}}"#)).await;
        let asked = server.wire.next().await;
        server.wire.send(&asked.answered(r#"{"tools":[]}"#)).await;
        let returned = assert_ok!(serde_json::to_string(&assert_some!(client.next().await.id)));
        assert_eq!(returned, id, "the reply carries the request id with its JSON type");
    }
}

#[tokio::test(start_paused = true)]
async fn calls_of_other_methods_run_concurrently_once_a_resource_read_asked() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (mut first, mut second) = (joined(&proxy).await, joined(&proxy).await);
    first.send(r#"{"jsonrpc":"2.0","id":1,"method":"resources/read","params":{"uri":"memo://asks"}}"#).await;
    let read = server.wire.next().await;
    server.wire.send(r#"{"jsonrpc":"2.0","id":"e1","method":"elicitation/create","params":{"message":"which"}}"#).await;
    assert_eq!(first.next().await.id, Some(Id::Text("e1".to_owned())));
    first.send(r#"{"jsonrpc":"2.0","id":"e1","result":{"action":"decline"}}"#).await;
    assert_eq!(server.wire.next().await.id, Some(Id::Text("e1".to_owned())));
    server.wire.send(&read.answered(r#"{"contents":[]}"#)).await;
    assert_eq!(first.next().await.id, Some(Id::Number(1)));
    first.send(r#"{"jsonrpc":"2.0","id":2,"method":"tools/list"}"#).await;
    let listed = server.wire.next().await;
    server.wire.send(&listed.answered(r#"{"tools":[]}"#)).await;
    assert_eq!(first.next().await.id, Some(Id::Number(2)));
    let sent = Instant::now();
    first.send(r#"{"jsonrpc":"2.0","id":3,"method":"tools/list"}"#).await;
    second.send(r#"{"jsonrpc":"2.0","id":1,"method":"tools/list"}"#).await;
    let (earlier, later) = (server.wire.next().await, server.wire.next().await);
    assert_eq!((earlier.method.as_deref(), later.method.as_deref()), (Some("tools/list"), Some("tools/list")));
    assert_eq!(sent.elapsed(), Duration::ZERO, "a quiet method runs concurrently though a resource read asked, never waiting out the probation");
    server.wire.send(&earlier.answered(r#"{"tools":[]}"#)).await;
    server.wire.send(&later.answered(r#"{"tools":[]}"#)).await;
    assert_eq!((first.next().await.id, second.next().await.id), (Some(Id::Number(3)), Some(Id::Number(1))));
}

// --- [LOGGING]

#[tokio::test(start_paused = true)]
async fn server_log_level_follows_the_most_verbose_session_present() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (mut verbose, verbose_serving) = declared(&proxy, INITIALIZE).await;
    let mut terse = joined(&proxy).await;
    verbose.send(r#"{"jsonrpc":"2.0","id":1,"method":"logging/setLevel","params":{"level":"debug"}}"#).await;
    assert_eq!(verbose.next().await.id, Some(Id::Number(1)));
    assert_eq!(server.wire.next().await.params.level.as_deref(), Some("debug"));
    terse.send(r#"{"jsonrpc":"2.0","id":1,"method":"logging/setLevel","params":{"level":"error"}}"#).await;
    assert_eq!(terse.next().await.id, Some(Id::Number(1)));
    server.wire.silent(Duration::from_millis(100)).await;
    drop(verbose);
    assert_ok!(assert_ok!(verbose_serving.await));
    assert_eq!(server.wire.next().await.params.level.as_deref(), Some("error"), "the level rises to the least verbose session left");
    drop(terse);
    assert_eq!(server.wire.next().await.params.level.as_deref(), Some("emergency"), "with no session asking for messages the server sends the fewest");
}

#[tokio::test(start_paused = true)]
async fn log_message_reaches_the_sessions_whose_level_it_meets() {
    let (proxy, mut starts, _actor) = proxy(16, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (mut warned, mut unset) = (joined(&proxy).await, joined(&proxy).await);
    warned.send(r#"{"jsonrpc":"2.0","id":1,"method":"logging/setLevel","params":{"level":"warning"}}"#).await;
    let level = server.wire.next().await;
    assert_eq!(level.params.level.as_deref(), Some("warning"));
    server.wire.send(&level.answered("{}")).await;
    assert_eq!(warned.next().await.id, Some(Id::Number(1)));
    for (severity, data) in [("info", "below"), ("warning", "at"), ("error", "above")] {
        server.wire.send(&format!(r#"{{"jsonrpc":"2.0","method":"notifications/message","params":{{"level":"{severity}","data":"{data}"}}}}"#)).await;
    }
    assert_eq!((warned.next().await.params.data, warned.next().await.params.data), (Some("at".to_owned()), Some("above".to_owned())), "a session receives messages at or above the level it set");
    warned.silent(Duration::from_millis(100)).await;
    unset.silent(Duration::from_millis(100)).await;
}

// --- [BACKPRESSURE]

#[tokio::test(start_paused = true)]
async fn session_that_stops_reading_is_disconnected_alone() {
    let (proxy, mut starts, _actor) = proxy(4, LINE);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (_stalled, serving) = declared(&proxy, INITIALIZE).await;
    let mut reader = joined(&proxy).await;
    let padding = "x".repeat(32 << 10);
    for _ in 0..16 {
        server.wire.send(&format!(r#"{{"jsonrpc":"2.0","method":"notifications/tools/list_changed","params":{{"pad":"{padding}"}}}}"#)).await;
        assert_eq!(reader.next().await.method.as_deref(), Some("notifications/tools/list_changed"), "a session that reads keeps receiving");
    }
    assert_ok!(assert_ok!(assert_ok!(time::timeout(Duration::from_secs(1), serving).await, "a session that stops reading is disconnected")));
    server.wire.send(r#"{"jsonrpc":"2.0","method":"notifications/tools/list_changed"}"#).await;
    assert_eq!(reader.next().await.method.as_deref(), Some("notifications/tools/list_changed"), "the reading session stays");
    assert!(!server.stop.is_cancelled(), "the server stays");
}
