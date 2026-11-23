use core::future::ready;
use core::ops::RangeInclusive;
use core::time::Duration;
use std::io;

use backon::ExponentialBuilder;
use claims::{assert_err, assert_ok, assert_some};
use futures::FutureExt as _;
use futures::future::BoxFuture;
use rasm_mcp::{History, IdleTimeout, Link, Liveness, Policy, Proxy, decode};
use rasm_mcp_tests::Id;
use rmcp::model::{ClientCapabilities, DiscoverResult, Implementation, InitializeRequestParams, JsonRpcVersion2_0, ProtocolVersion, ServerCapabilities};
use serde::{Deserialize, Serialize};
use serde_json::value::RawValue;
use serde_json::{Number, Value};
use tokio::io::{AsyncBufReadExt as _, AsyncWriteExt as _, BufReader, DuplexStream, ReadHalf, WriteHalf};
use tokio::sync::{mpsc, oneshot};
use tokio::task::JoinHandle;
use tokio::time;
use tokio_util::sync::CancellationToken;

// --- [CONSTANTS] -----------------------------------------------------------------------

const INITIALIZE: &str = r#"{"jsonrpc":"2.0","id":0,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{"elicitation":{}},"clientInfo":{"name":"t","version":"0"}}}"#;
const INITIALIZED: &str = r#"{"jsonrpc":"2.0","method":"notifications/initialized"}"#;
const SERVED: &str = r#"{"protocolVersion":"2025-06-18","capabilities":{"tools":{"listChanged":true},"resources":{"subscribe":true},"logging":{}},"serverInfo":{"name":"s","version":"0"}}"#;
const GRACE: Duration = Duration::from_secs(30);
const QUIET: Duration = Duration::from_mins(2);
const LINE: usize = 1 << 20;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Default, PartialEq, Eq, Deserialize)]
struct Tagged {
    #[serde(rename = "progressToken")]
    progress_token: Option<Id>,
    #[serde(rename = "io.modelcontextprotocol/clientCapabilities")]
    client_capabilities: Option<Value>,
    #[serde(rename = "io.modelcontextprotocol/clientInfo")]
    client_info: Option<Named>,
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
    #[serde(rename = "_meta")]
    meta: Option<Tagged>,
    progress_token: Option<Id>,
    progress: Option<Number>,
    uri: Option<String>,
    level: Option<String>,
    data: Option<String>,
    message: Option<String>,
    protocol_version: Option<String>,
    server_info: Option<Named>,
    content: Option<Answered>,
    roots: Option<Vec<Value>>,
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

    fn progress(&self) -> Option<f64> { self.params.progress.as_ref().and_then(Number::as_f64) }
}

#[derive(Debug, Serialize)]
struct Frame {
    jsonrpc: JsonRpcVersion2_0,
    #[serde(skip_serializing_if = "Option::is_none")]
    id: Option<u64>,
    method: &'static str,
    params: Params,
}

impl Frame {
    fn line(&self) -> String { assert_ok!(serde_json::to_string(self)) }
}

#[derive(Debug, Default, Serialize)]
#[serde(rename_all = "camelCase")]
struct Params {
    #[serde(skip_serializing_if = "Option::is_none")]
    name: Option<&'static str>,
    #[serde(rename = "_meta", skip_serializing_if = "Option::is_none")]
    meta: Option<Meta>,
    #[serde(skip_serializing_if = "Option::is_none")]
    progress_token: Option<Id>,
    #[serde(skip_serializing_if = "Option::is_none")]
    progress: Option<u32>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pad: Option<String>,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "camelCase")]
struct Meta {
    progress_token: Id,
}

#[derive(Debug)]
struct Peer {
    lines: tokio::io::Lines<BufReader<ReadHalf<DuplexStream>>>,
    writer: WriteHalf<DuplexStream>,
}

impl Peer {
    fn new(stream: DuplexStream) -> Self {
        let (reader, writer) = tokio::io::split(stream);
        Self { lines: BufReader::new(reader).lines(), writer }
    }

    async fn send(&mut self, line: &str) {
        assert_ok!(self.writer.write_all(format!("{line}\n").as_bytes()).await);
    }

    async fn chunked(&mut self, line: &str, size: usize) {
        for chunk in format!("{line}\n").as_bytes().chunks(size) {
            assert_ok!(self.writer.write_all(chunk).await);
            tokio::task::yield_now().await;
        }
    }

    async fn next(&mut self) -> Seen {
        let line = assert_ok!(time::timeout(Duration::from_mins(15), self.lines.next_line()).await, "a line arrives inside 15 minutes");
        assert_ok!(decode(assert_some!(assert_ok!(line), "the peer is still connected").as_bytes()))
    }

    async fn silent(&mut self, span: Duration) {
        assert_err!(time::timeout(span, self.lines.next_line()).await, "no line arrives inside {span:?}");
    }
}

#[derive(Debug)]
struct Served {
    wire: Peer,
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
        assert_eq!(self.wire.next().await.method.as_deref(), Some("notifications/initialized"), "an answered initialize is followed by notifications/initialized");
        self
    }

    async fn discovered(mut self) -> Self {
        let asked = self.wire.next().await;
        assert_eq!(asked.method.as_deref(), Some("server/discover"), "an unknown server is asked server/discover first");
        let served = DiscoverResult::new(vec![ProtocolVersion::LATEST], ServerCapabilities::builder().enable_tools().build()).with_server_info(Implementation::new("s", "0"));
        self.wire.send(&asked.answered(&assert_ok!(serde_json::to_string(&served)))).await;
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
        let attached = match connections.send(Served { wire: Peer::new(far), exit, stop }) {
            Ok(()) => Ok(()),
            Err(mpsc::error::SendError(Served { wire: _, exit: _, stop: _ })) => Err(io::Error::from(io::ErrorKind::BrokenPipe)),
        };
        ready(attached.map(|()| Link { reader, writer, closed: exited.map(|status| status.unwrap_or_else(|closed| closed.to_string())).boxed() })).boxed()
    };
    let policy = Policy {
        client: InitializeRequestParams::new(ClientCapabilities::default(), Implementation::new("proxy", "0")),
        restart: ExponentialBuilder::new().with_min_delay(Duration::from_secs(1)).with_max_delay(Duration::from_mins(5)).without_max_times(),
        handshake: GRACE,
        probation: Duration::from_secs(2),
        liveness: Some(Liveness { quiet: QUIET, grace: Duration::from_mins(1) }),
        idle: IdleTimeout { ratio: 300, floor: Duration::from_mins(1), ceiling: Duration::from_hours(2) },
        line,
        queue,
    };
    let (proxy, actor) = Proxy::new(policy, History::default(), connect);
    (proxy, starts, tokio::spawn(actor))
}

fn session(proxy: &Proxy) -> (Peer, JoinHandle<io::Result<()>>) {
    let (near, far) = tokio::io::duplex(64 << 10);
    let (reader, writer) = tokio::io::split(far);
    let proxy = proxy.clone();
    (Peer::new(near), tokio::spawn(async move { proxy.serve(reader, writer).await }))
}

async fn joined(proxy: &Proxy) -> Peer { declared(proxy, INITIALIZE).await.0 }

async fn declared(proxy: &Proxy, initialize: &str) -> (Peer, JoinHandle<io::Result<()>>) {
    let (mut wire, serving) = session(proxy);
    wire.send(initialize).await;
    assert_eq!(wire.next().await.result.server_info.map(|info| info.name).as_deref(), Some("s"), "initialize is answered from the server handshake");
    wire.send(INITIALIZED).await;
    wire.send(r#"{"jsonrpc":"2.0","id":"joined","method":"ping"}"#).await;
    assert_eq!(wire.next().await.id, Some(Id::Text("joined".to_owned())), "a ping answered after notifications/initialized means the proxy counts the session as initialized");
    (wire, serving)
}

fn asking(id: u64) -> String {
    Frame { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: "tools/call", params: Params { name: Some("slow"), meta: Some(Meta { progress_token: Id::Text("p".to_owned()) }), ..Params::default() } }
        .line()
}

fn reporting(call: &Seen, steps: RangeInclusive<u32>, result: &str) -> String {
    let token = assert_some!(call.params.meta.clone().and_then(|meta| meta.progress_token));
    let progress = steps.map(|done| {
        Frame { jsonrpc: JsonRpcVersion2_0, id: None, method: "notifications/progress", params: Params { progress_token: Some(token.clone()), progress: Some(done), ..Params::default() } }.line()
    });
    Vec::from_iter(progress.chain([call.answered(result)])).join("\n")
}

fn padded(id: u64, total: usize) -> String {
    let framed = |pad: String| Frame { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: "tools/list", params: Params { pad: Some(pad), ..Params::default() } }.line();
    framed("x".repeat(total.saturating_sub(framed(String::new()).len())))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod framing;
mod handshake;
mod lifetime;
mod progress;
mod routing;
