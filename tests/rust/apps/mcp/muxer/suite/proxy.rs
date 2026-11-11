use core::future::ready;
use core::time::Duration;
use std::io;

use backon::ExponentialBuilder;
use claims::{assert_err, assert_none, assert_ok, assert_some};
use futures::FutureExt as _;
use futures::future::{self, BoxFuture};
use mcp_muxer_tests::decoded;
use rasm_mcp::{Era, History, Line, Lines, Link, Liveness, Phase, Policy, Proxy, Retention, identity};
use rmcp::model::{ClientCapabilities, Implementation, InitializeRequestParams, JsonRpcVersion2_0};
use serde::{Deserialize, Serialize};
use serde_json::value::RawValue;
use tokio::io::{AsyncBufReadExt as _, AsyncWriteExt as _, BufReader, DuplexStream, ReadHalf, WriteHalf};
use tokio::sync::{mpsc, oneshot};
use tokio::task::JoinHandle;
use tokio::time::{self, Instant};
use tokio_util::codec::Decoder as _;
use tokio_util::sync::CancellationToken;

// --- [CONSTANTS] -----------------------------------------------------------------------

const INITIALIZE: &str = r#"{"jsonrpc":"2.0","id":0,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"t","version":"0"}}}"#;
const INITIALIZED: &str = r#"{"jsonrpc":"2.0","method":"notifications/initialized"}"#;
const SERVED: &str = r#"{"protocolVersion":"2025-06-18","capabilities":{"tools":{"listChanged":true},"resources":{"subscribe":true},"logging":{}},"serverInfo":{"name":"s","version":"0"}}"#;
const GRACE: Duration = Duration::from_secs(30);
const QUIET: Duration = Duration::from_mins(2);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(untagged)]
enum Id {
    Number(u64),
    Text(String),
}

#[derive(Debug, Clone, Default, PartialEq, Eq, Deserialize)]
struct Named {
    name: String,
}

#[derive(Debug, Clone, Default, PartialEq, Eq, Deserialize)]
struct Answered {
    answer: Option<String>,
}

#[derive(Debug, Clone, Default, PartialEq, Eq, Deserialize)]
#[serde(default, rename_all = "camelCase")]
struct Fields {
    uri: Option<String>,
    level: Option<String>,
    data: Option<String>,
    message: Option<String>,
    protocol_version: Option<String>,
    server_info: Option<Named>,
    content: Option<Answered>,
}

#[derive(Debug, Clone, Default, PartialEq, Eq, Deserialize)]
#[serde(default)]
struct Seen {
    id: Option<Id>,
    method: Option<String>,
    params: Fields,
    result: Fields,
    error: Fields,
}

impl Seen {
    fn answered(&self, result: &str) -> String {
        #[derive(Serialize)]
        struct Reply<'seen> {
            jsonrpc: JsonRpcVersion2_0,
            id: &'seen Id,
            result: Box<RawValue>,
        }
        assert_ok!(serde_json::to_string(&Reply { jsonrpc: JsonRpcVersion2_0, id: assert_some!(self.id.as_ref()), result: assert_ok!(RawValue::from_string(result.to_owned())) }))
    }
}

#[derive(Debug)]
struct Wire {
    lines: tokio::io::Lines<BufReader<ReadHalf<DuplexStream>>>,
    writer: WriteHalf<DuplexStream>,
}

impl Wire {
    fn new(stream: DuplexStream) -> Self {
        let (reader, writer) = tokio::io::split(stream);
        Self { lines: BufReader::new(reader).lines(), writer }
    }

    async fn send(&mut self, line: &str) {
        assert_ok!(self.writer.write_all(format!("{line}\n").as_bytes()).await);
    }

    async fn next(&mut self) -> Seen { assert_ok!(decoded(assert_some!(assert_ok!(self.lines.next_line().await)).as_bytes())) }

    async fn silent(&mut self, span: Duration) {
        assert_err!(time::timeout(span, self.lines.next_line()).await, "no line arrives inside {span:?}");
    }
}

#[derive(Debug)]
struct Served {
    wire: Wire,
    exit: oneshot::Sender<String>,
    stop: CancellationToken,
}

impl Served {
    async fn linked(mut self) -> Self {
        assert_eq!(self.wire.next().await.method.as_deref(), Some("server/discover"), "an unknown server is asked server/discover first");
        self.wire.send(r#"{"jsonrpc":"2.0","id":1,"error":{"code":-32601,"message":"Method not found"}}"#).await;
        let asked = self.wire.next().await;
        assert_eq!(asked.method.as_deref(), Some("initialize"), "a refusal of server/discover is followed by initialize on the same connection");
        self.wire.send(&asked.answered(SERVED)).await;
        assert_eq!(self.wire.next().await.method.as_deref(), Some("notifications/initialized"));
        self
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn proxy(queue: usize, line: usize) -> (Proxy, mpsc::UnboundedReceiver<Served>, JoinHandle<()>) {
    let (connections, starts) = mpsc::unbounded_channel();
    let connect = move |stop: CancellationToken| -> BoxFuture<'static, io::Result<Link<ReadHalf<DuplexStream>, WriteHalf<DuplexStream>, BoxFuture<'static, String>>>> {
        let (near, far) = tokio::io::duplex(64 << 10);
        let (reader, writer) = tokio::io::split(near);
        let (exit, exited) = oneshot::channel();
        let attached = match connections.send(Served { wire: Wire::new(far), exit, stop }) {
            Ok(()) => Ok(()),
            Err(mpsc::error::SendError(Served { wire: _, exit: _, stop: _ })) => Err(io::Error::from(io::ErrorKind::BrokenPipe)),
        };
        ready(attached.map(|()| Link { reader, writer, closed: exited.map(|exit| exit.unwrap_or_else(|closed| closed.to_string())).boxed() })).boxed()
    };
    let policy = Policy {
        client: InitializeRequestParams::new(ClientCapabilities::default(), Implementation::new("proxy", "0")),
        restart: ExponentialBuilder::new().with_min_delay(Duration::from_secs(1)).with_max_delay(Duration::from_mins(5)).without_max_times(),
        handshake: GRACE,
        probation: Duration::from_secs(2),
        liveness: Some(Liveness { quiet: QUIET, grace: Duration::from_mins(1) }),
        retention: Retention { ratio: 300, floor: Duration::from_mins(1), ceiling: Duration::from_hours(2) },
        line,
        queue,
    };
    let (proxy, actor) = Proxy::new(policy, History::default(), connect);
    (proxy, starts, tokio::spawn(actor))
}

fn session(proxy: &Proxy) -> (Wire, JoinHandle<io::Result<()>>) {
    let (near, far) = tokio::io::duplex(64 << 10);
    let (reader, writer) = tokio::io::split(far);
    let proxy = proxy.clone();
    (Wire::new(near), tokio::spawn(async move { proxy.serve(reader, writer).await }))
}

async fn joined(proxy: &Proxy) -> Wire {
    let (mut wire, _) = session(proxy);
    wire.send(INITIALIZE).await;
    assert_eq!(wire.next().await.result.server_info.map(|info| info.name).as_deref(), Some("s"), "initialize is answered from the server handshake");
    wire.send(INITIALIZED).await;
    wire
}

// --- [HANDSHAKE]

#[tokio::test(start_paused = true)]
async fn silent_server_is_asked_initialize_after_the_grace_on_one_connection() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 20);
    let mut server = assert_some!(starts.recv().await);
    let (mut waiting, _) = session(&proxy);
    waiting.send(INITIALIZE).await;
    assert_eq!(server.wire.next().await.method.as_deref(), Some("server/discover"));
    let asked = Instant::now();
    let initialize = server.wire.next().await;
    assert_eq!(initialize.method.as_deref(), Some("initialize"), "a server that stays silent is asked initialize on the same connection");
    assert!(asked.elapsed() >= GRACE, "initialize follows the grace period");
    server.wire.send(&initialize.answered(SERVED)).await;
    assert_eq!(waiting.next().await.result.protocol_version.as_deref(), Some("2025-06-18"), "the waiting session is answered once the server links");
    assert_eq!((proxy.status().borrow().starts, proxy.status().borrow().catalog.as_ref().map(|catalog| catalog.era)), (1, Some(Era::Legacy)), "the server started once and its era is stored");
}

#[tokio::test(start_paused = true)]
async fn server_that_ends_on_discover_is_asked_initialize_first() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 20);
    let mut first = assert_some!(starts.recv().await);
    assert_eq!(first.wire.next().await.method.as_deref(), Some("server/discover"));
    assert_ok!(first.exit.send("exit status: 1".to_owned()));
    let mut second = assert_some!(starts.recv().await);
    assert_eq!(second.wire.next().await.method.as_deref(), Some("initialize"), "the next start asks initialize first");
    assert_eq!(proxy.status().borrow().failures, 0, "the first end counts as no failure");
}

#[tokio::test(start_paused = true)]
async fn starting_server_survives_its_sessions_leaving() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 20);
    let server = assert_some!(starts.recv().await);
    let (mut waiting, serving) = session(&proxy);
    waiting.send(INITIALIZE).await;
    drop(waiting);
    time::sleep(Duration::from_secs(5)).await;
    assert!(!server.stop.is_cancelled(), "a starting server is not stopped because its waiting session left");
    let _server = server.linked().await;
    assert_ok!(proxy.status().wait_for(|status| status.phase == Phase::Up).await);
    assert_ok!(assert_ok!(serving.await));
    assert_none!(starts.try_recv().ok(), "the server started once");
}

#[tokio::test(start_paused = true)]
async fn server_that_never_answers_stops_at_the_first_retention_ceiling_with_no_session_waiting() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 20);
    let silent = assert_some!(starts.recv().await);
    let (mut waiting, serving) = session(&proxy);
    waiting.send(INITIALIZE).await;
    let started = Instant::now();
    time::sleep(Duration::from_hours(3)).await;
    assert!(!silent.stop.is_cancelled(), "a silent server stays while a session waits for its first answer");
    drop(waiting);
    assert_ok!(assert_ok!(serving.await), "a session waiting for the first answer ends with its client");
    silent.stop.cancelled().await;
    assert!(started.elapsed() >= Duration::from_hours(4) && started.elapsed() < Duration::from_secs(14_405), "the silent server stops at the next ceiling: {:?}", started.elapsed());
    drop(silent);
    assert_ok!(proxy.status().wait_for(|status| status.phase == Phase::Idle).await);
    assert_none!(starts.try_recv().ok(), "the server started once");
}

// --- [LIFETIME]

#[tokio::test(start_paused = true)]
async fn unused_server_stops_after_three_hundred_times_its_start_cost() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 20);
    let server = assert_some!(starts.recv().await);
    time::sleep(Duration::from_secs(2)).await;
    let server = server.linked().await;
    let linked = Instant::now();
    assert_ok!(proxy.status().wait_for(|status| status.phase == Phase::Up).await);
    server.stop.cancelled().await;
    assert!(linked.elapsed() >= Duration::from_secs(600) && linked.elapsed() < Duration::from_secs(605), "a 2 s start keeps the server 600 s: {:?}", linked.elapsed());
    drop(server);
    assert_ok!(proxy.status().wait_for(|status| status.phase == Phase::Idle).await);
}

#[tokio::test(start_paused = true)]
async fn silent_server_is_pinged_then_replaced() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 20);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut caller = joined(&proxy).await;
    caller.send(r#"{"jsonrpc":"2.0","id":7,"method":"tools/call","params":{"name":"hang"}}"#).await;
    assert_eq!(server.wire.next().await.method.as_deref(), Some("tools/call"));
    let sent = Instant::now();
    assert_eq!(server.wire.next().await.method.as_deref(), Some("ping"), "a server silent for the quiet period with a call in flight is pinged");
    assert!(sent.elapsed() >= QUIET);
    server.stop.cancelled().await;
    assert_ok!(server.exit.send("killed".to_owned()));
    let answer = caller.next().await;
    assert_eq!(answer.id, Some(Id::Number(7)), "the call in flight is answered under its own id");
    assert!(answer.error.message.as_deref().is_some_and(|message| message.contains("stopped answering")), "the error states the cause: {answer:?}");
}

#[tokio::test(start_paused = true)]
async fn drain_waits_for_calls_that_finish_and_no_longer_than_the_settle_window() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 20);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut caller = joined(&proxy).await;
    caller.send(r#"{"jsonrpc":"2.0","id":1,"method":"tools/list"}"#).await;
    let listed = server.wire.next().await;
    server.wire.send(&listed.answered(r#"{"tools":[]}"#)).await;
    assert_eq!(caller.next().await.id, Some(Id::Number(1)));
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
    assert!(started.elapsed() >= Duration::from_millis(350) && started.elapsed() < Duration::from_millis(400), "the drain ends one settle window after the last answer: {:?}", started.elapsed());
    caller.send(r#"{"jsonrpc":"2.0","id":4,"method":"tools/list"}"#).await;
    assert!(caller.next().await.error.message.as_deref().is_some_and(|message| message.contains("stopping")), "a call sent during the drain is refused at once");
}

// --- [ROUTING]

#[tokio::test(start_paused = true)]
async fn subscriptions_are_restored_after_a_restart_and_released_with_the_last_subscriber() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 20);
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
    assert_ok!(server.exit.send("exit status: 9".to_owned()));
    first.send(r#"{"jsonrpc":"2.0","id":2,"method":"tools/list"}"#).await;
    assert!(first.next().await.error.message.as_deref().is_some_and(|message| message.contains("server is down")), "inside the backoff window a call names the cause");
    time::sleep(Duration::from_secs(1)).await;
    first.send(r#"{"jsonrpc":"2.0","id":3,"method":"tools/list"}"#).await;
    let mut server = assert_some!(starts.recv().await);
    let initialize = server.wire.next().await;
    assert_eq!(initialize.method.as_deref(), Some("initialize"), "a server known to be legacy is asked initialize first");
    server.wire.send(&initialize.answered(SERVED)).await;
    assert_eq!(server.wire.next().await.method.as_deref(), Some("notifications/initialized"));
    let restored = server.wire.next().await;
    assert_eq!((restored.method.as_deref(), restored.params.uri.as_deref()), (Some("resources/subscribe"), Some("file:///a")), "the subscription is restored on the new server");
    assert_eq!(first.next().await.method.as_deref(), Some("notifications/tools/list_changed"), "sessions are told the lists changed after a restart");
    let woken = server.wire.next().await;
    assert_eq!(woken.method.as_deref(), Some("tools/list"), "the call that woke the server is forwarded after the handshake");
    drop(first);
    assert_eq!(server.wire.next().await.method.as_deref(), Some("notifications/cancelled"), "a leaving session's open call is cancelled at the server");
    server.wire.silent(Duration::from_secs(1)).await;
    drop(second);
    let released = server.wire.next().await;
    assert_eq!((released.method.as_deref(), released.params.uri.as_deref()), (Some("resources/unsubscribe"), Some("file:///a")), "the subscription is released with its last subscriber");
}

#[tokio::test(start_paused = true)]
async fn leaving_session_takes_its_parked_calls_with_it() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 20);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut leaving = joined(&proxy).await;
    leaving.send(r#"{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"ask"}}"#).await;
    assert_eq!(server.wire.next().await.method.as_deref(), Some("tools/call"));
    leaving.send(r#"{"jsonrpc":"2.0","id":2,"method":"resources/subscribe","params":{"uri":"file:///a"}}"#).await;
    server.wire.silent(Duration::from_millis(100)).await;
    drop(leaving);
    assert_eq!(server.wire.next().await.method.as_deref(), Some("notifications/cancelled"), "the leaving session's open call is cancelled at the server");
    server.wire.silent(Duration::from_secs(1)).await;
    server.stop.cancelled().await;
    drop(server);
    assert_ok!(proxy.status().wait_for(|status| status.phase == Phase::Idle).await, "a server no session subscribes to stops once unused");
}

#[tokio::test(start_paused = true)]
async fn server_requests_reach_the_session_whose_call_caused_them() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 20);
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
    let learned = assert_some!(proxy.status().borrow().catalog.clone());
    assert_eq!((learned.askers.len(), learned.quiet.len()), (2, 0), "both tools are stored as tools that ask");
}

#[tokio::test(start_paused = true)]
async fn session_that_stops_reading_is_disconnected_and_log_messages_follow_levels() {
    let (proxy, mut starts, _actor) = proxy(4, 1 << 20);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let (stalled, serving) = session(&proxy);
    let mut stalled = stalled;
    stalled.send(INITIALIZE).await;
    stalled.send(INITIALIZED).await;
    let mut reader = joined(&proxy).await;
    reader.send(r#"{"jsonrpc":"2.0","id":1,"method":"logging/setLevel","params":{"level":"warning"}}"#).await;
    let level = server.wire.next().await;
    assert_eq!(level.params.level.as_deref(), Some("warning"));
    server.wire.send(&level.answered("{}")).await;
    assert_eq!(reader.next().await.id, Some(Id::Number(1)));
    server.wire.send(r#"{"jsonrpc":"2.0","method":"notifications/message","params":{"level":"info","data":"quiet"}}"#).await;
    server.wire.send(r#"{"jsonrpc":"2.0","method":"notifications/message","params":{"level":"error","data":"loud"}}"#).await;
    assert_eq!(reader.next().await.params.data.as_deref(), Some("loud"), "a session receives log messages at or above the level it set and no other");
    let padding = "x".repeat(32 << 10);
    for _ in 0..16 {
        server.wire.send(&format!(r#"{{"jsonrpc":"2.0","method":"notifications/tools/list_changed","params":{{"pad":"{padding}"}}}}"#)).await;
        assert_eq!(reader.next().await.method.as_deref(), Some("notifications/tools/list_changed"), "a session that reads keeps receiving");
    }
    assert_ok!(assert_ok!(assert_ok!(time::timeout(Duration::from_secs(1), serving).await, "a session that stops reading is disconnected")));
    assert_eq!(proxy.status().borrow().sessions, 1, "the server and the reading session stay");
}

#[tokio::test(start_paused = true)]
async fn oversized_server_notification_and_request_fail_no_call() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 10);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut caller = joined(&proxy).await;
    let padding = "x".repeat(2 << 10);
    caller.send(r#"{"jsonrpc":"2.0","id":7,"method":"tools/call","params":{"name":"wide"}}"#).await;
    let call = server.wire.next().await;
    let issued = assert_ok!(serde_json::to_string(assert_some!(call.id.as_ref())));
    server.wire.send(&format!(r#"{{"jsonrpc":"2.0","method":"notifications/message","params":{{"level":"info","data":"{padding}"}}}}"#)).await;
    server.wire.send(&format!(r#"{{"jsonrpc":"2.0","id":{issued},"method":"elicitation/create","params":{{"message":"{padding}"}}}}"#)).await;
    server.wire.send(&call.answered("{}")).await;
    let answer = caller.next().await;
    assert_eq!((answer.id.clone(), answer.error.message.as_deref()), (Some(Id::Number(7)), None), "a notification or request over the line limit fails no call: {answer:?}");
    let refused = server.wire.next().await;
    assert_eq!(refused.id, call.id, "the request over the line limit is refused under its own id");
    assert!(refused.error.message.as_deref().is_some_and(|message| message.contains("exceeds")), "the refusal states the limit: {refused:?}");
}

#[tokio::test(start_paused = true)]
async fn stop_and_drain_pass_a_server_that_stopped_reading() {
    let (proxy, mut starts, actor) = proxy(4, 1 << 20);
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
        assert_ok!(server.exit.send("killed".to_owned()));
    };
    assert_ok!(time::timeout(Duration::from_secs(1), future::join(proxy.stop(), exited)).await, "a stop passes messages queued behind a server that stopped reading");
    assert_ok!(actor.await, "the proxy task ends with the stop");
}

#[tokio::test]
async fn oversized_line_names_its_request_from_either_end() {
    let mut lines = Lines::new(64);
    let padding = "x".repeat(4096);
    let mut leading = format!(r#"{{"jsonrpc":"2.0","id":41,"result":{{"text":"{padding}"}}}}"#).into_bytes();
    leading.push(b'\n');
    let mut trailing = format!(r#"{{"result":{{"text":"{padding}","id":7}},"jsonrpc":"2.0","id":42}}"#).into_bytes();
    trailing.push(b'\n');
    for (line, id) in [(leading, "41"), (trailing, "42")] {
        let mut buffer = bytes::BytesMut::new();
        let decoded = line.chunks(100).find_map(|chunk| {
            buffer.extend_from_slice(chunk);
            assert_ok!(lines.decode(&mut buffer))
        });
        let named = match &decoded {
            Some(Line::Oversized { head, tail }) => identity(head, tail).0,
            Some(Line::Whole(_)) | None => None,
        };
        assert_eq!(named.as_deref(), Some(id.as_bytes()), "a line over the limit is yielded once and its request id is read from the head or the tail: {decoded:?}");
    }
}
