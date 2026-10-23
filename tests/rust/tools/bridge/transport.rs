#![cfg(unix)]

use core::{future::ready, net::SocketAddr};
use std::io::{self, BufRead};

use futures::{StreamExt as _, TryStreamExt as _, future};
use nonempty_collections::NEVec;
use opentelemetry::{
    KeyValue, Value,
    trace::{SpanId, TraceId},
};
use opentelemetry_otlp::OTEL_EXPORTER_OTLP_ENDPOINT;
use opentelemetry_proto::tonic::{
    Attributes,
    common::v1::AnyValue,
    logs::v1::ResourceLogs,
    metrics::v1::{ResourceMetrics, metric::Data},
    resource::v1::Resource,
    trace::v1::{ResourceSpans, span::SpanKind},
};
use opentelemetry_semantic_conventions::{
    SCHEMA_URL,
    attribute::{PROCESS_PID, SERVICE_INSTANCE_ID, SERVICE_NAME},
};
use process_wrap::tokio::ChildWrapper as _;
use reqwest::StatusCode;
use rmcp::{
    ServiceError, ServiceExt as _,
    model::{PaginatedRequestParams, RequestParamsMeta as _},
    service::ClientInitializeError,
    transport::StreamableHttpClientTransport,
};
use serde::{Deserialize, de::DeserializeOwned};
use serde_json::{Map, json};
use testcontainers::{
    CopyTargetOptions, GenericImage, ImageExt as _, TestcontainersError,
    core::{IntoContainerPort as _, WaitFor, wait::HttpWaitStrategy},
    runners::AsyncRunner as _,
};
use tokio::{
    io::{AsyncBufReadExt as _, BufReader},
    net::unix::pipe,
    process::Command,
    signal::unix::SignalKind,
    task::JoinError,
};
use tokio_stream::wrappers::LinesStream;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, derive_more::Display)]
enum Operation {
    #[display("open bridge log pipe")]
    Pipe,
    #[display("spawn bridge")]
    Spawn,
    #[display("read bridge log")]
    Log,
    #[display("signal bridge")]
    Signal,
    #[display("await bridge exit")]
    Wait,
    #[display("read exported telemetry")]
    Telemetry,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SERVICE: &str = "bridged";
const INSTANCE: &str = "transport";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Deserialize)]
struct Event {
    listen: Option<SocketAddr>,
    #[serde(rename = "service.name")]
    service: String,
    #[serde(rename = "service.instance.id")]
    instance: String,
}

#[derive(Deserialize)]
struct Correlated {
    #[serde(rename = "openTelemetry")]
    context: Option<Context>,
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct Context {
    trace_id: String,
}

#[derive(Default, Deserialize)]
#[serde(default)]
struct Exported {
    #[serde(rename = "resourceSpans")]
    spans: Vec<ResourceSpans>,
    #[serde(rename = "resourceMetrics")]
    metrics: Vec<ResourceMetrics>,
    #[serde(rename = "resourceLogs")]
    logs: Vec<ResourceLogs>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Failure {
    #[error("cannot {operation}")]
    Io { operation: Operation, source: io::Error },
    #[error("cannot decode {line}")]
    Decode { line: String, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("bridge ended before listening\n{log}")]
    Unready { log: String },
    #[error("MCP client handshake failed")]
    Initialize(#[from] Box<ClientInitializeError>),
    #[error("MCP request failed")]
    Request(#[from] ServiceError),
    #[error("MCP client task failed")]
    Client(#[from] JoinError),
    #[error("collector container failed")]
    Container(#[from] TestcontainersError),
}

impl From<Failure> for NEVec<Failure> {
    fn from(value: Failure) -> Self { Self::new(value) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn decode<T>(line: &str) -> Result<T, Failure>
where T: DeserializeOwned {
    serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_str(line)).map_err(|source| Failure::Decode { line: line.to_owned(), source })
}

async fn exchange(endpoint: Option<&str>, params: PaginatedRequestParams) -> Result<(Event, String, Option<u32>), NEVec<Failure>> {
    let io = |operation| move |source| Failure::Io { operation, source };
    let (writer, reader) = pipe::pipe().map_err(io(Operation::Pipe))?;
    let mut child = Command::new(env!("CARGO_BIN_EXE_rasm-mcp-bridge"))
        .args(["--listen", "127.0.0.1:0", "--log", "info"])
        .args(endpoint.into_iter().flat_map(|_| ["--traces-exporter=otlp", "--metrics-exporter=otlp", "--logs-exporter=otlp"]))
        .args(["--", "ast-grep-server"])
        .envs(endpoint.into_iter().flat_map(|endpoint| [(OTEL_EXPORTER_OTLP_ENDPOINT, endpoint), ("OTEL_SERVICE_NAME", SERVICE)]))
        .envs(endpoint.map(|_| ("OTEL_RESOURCE_ATTRIBUTES", format!("{SERVICE_NAME}=attributed,{SERVICE_INSTANCE_ID}={INSTANCE}"))))
        .stderr(writer.into_blocking_fd().map_err(io(Operation::Pipe))?)
        .kill_on_drop(true)
        .spawn()
        .map_err(io(Operation::Spawn))?;
    let (mut lines, mut log, pid) = (LinesStream::new(BufReader::new(reader).lines()).map_err(io(Operation::Log)), Vec::new(), child.id());
    let listening = lines
        .by_ref()
        .try_filter_map(|line| {
            ready(decode::<Event>(&line).map(|event| {
                log.push(line);
                event.listen.map(|address| (address, event))
            }))
        })
        .try_next()
        .await?;
    let Some((address, event)) = listening else { return Err(Failure::Unready { log: log.join("\n") }.into()) };
    let session = async {
        let client = ().serve(StreamableHttpClientTransport::from_uri(format!("http://{address}"))).await.map_err(Box::new)?;
        let listed = client.list_tools(Some(params)).await;
        client.cancel().await?;
        Ok::<_, Failure>(listed?)
    };
    let stopped = async {
        let listed = session.await;
        (listed, child.signal(SignalKind::terminate().as_raw_value()).map_err(io(Operation::Signal)), child.wait().await.map_err(io(Operation::Wait)))
    };
    let (rest, (listed, signalled, status)) = future::join(lines.try_collect::<Vec<_>>(), stopped).await;
    let (tools, status, log) = Validated::from(rest).map4(signalled.into(), listed.into(), status.into(), |rest, (), tools, status| (tools, status, [log, rest].concat().join("\n"))).ok()?;
    assert!(status.success(), "bridge exited {status}\n{log}");
    assert!(!tools.tools.is_empty(), "bridge listed no tools\n{log}");
    Ok((event, log, pid))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::test]
async fn tools_list_and_graceful_shutdown() -> Result<(), NEVec<Failure>> {
    let (event, log, _) = exchange(None, PaginatedRequestParams::default()).await?;
    assert_eq!(event.service, env!("CARGO_PKG_NAME"), "bridge resource named another service\n{log}");
    Ok(())
}

#[tokio::test]
#[ignore = "requires OpenTelemetry Collector container"]
async fn otlp_export_flushes_on_shutdown() -> Result<(), NEVec<Failure>> {
    let (otlp, health, file) = (4318_u16, 13133_u16, "/telemetry.jsonl");
    let config = json!({
        "receivers": { "otlp": { "protocols": { "http": { "endpoint": format!("0.0.0.0:{otlp}") } } } },
        "exporters": { "file": { "path": file } },
        "extensions": { "health_check": { "endpoint": format!("0.0.0.0:{health}") } },
        "service": {
            "extensions": ["health_check"],
            "pipelines": Map::from_iter(["traces", "metrics", "logs"].map(|signal| (signal.to_owned(), json!({ "receivers": ["otlp"], "exporters": ["file"] })))),
        },
    });
    let request = GenericImage::new("otel/opentelemetry-collector", "latest")
        .with_exposed_port(otlp.tcp())
        .with_exposed_port(health.tcp())
        .with_wait_for(WaitFor::http(HttpWaitStrategy::new("/").with_port(health.tcp()).with_expected_status_code(StatusCode::OK)))
        .with_copy_to(CopyTargetOptions::new(file).with_mode(0o666), Vec::new())
        .with_cmd([format!("--config=yaml:{config}")]);
    let (collector, endpoint) = async {
        let collector = request.pull_image().await?.start().await?;
        let endpoint = format!("http://{}:{}", collector.get_host().await?, collector.get_host_port_ipv4(otlp).await?);
        Ok::<_, TestcontainersError>((collector, endpoint))
    }
    .await
    .map_err(Failure::from)?;
    let (trace, parent, mut params) = (TraceId::from_bytes([0x11; 16]), SpanId::from_bytes([0x22; 8]), PaginatedRequestParams::default());
    params.set_traceparent(&format!("00-{trace}-{parent}-01"));
    let (event, log, pid) = exchange(Some(&endpoint), params).await?;
    assert_eq!([event.service.as_str(), event.instance.as_str()], [SERVICE, INSTANCE], "bridge resource ignored OTEL_SERVICE_NAME or OTEL_RESOURCE_ATTRIBUTES\n{log}");
    let correlated = log.lines().map(decode::<Correlated>).collect::<Validated<Vec<_>, _>>().ok()?;
    assert!(correlated.iter().any(|line| line.context.as_ref().is_some_and(|context| TraceId::from_hex(&context.trace_id) == Ok(trace))), "no log line holds trace {trace}\n{log}");
    collector.stop().await.map_err(Failure::from)?;
    let (code, output, copied) = future::join3(collector.exit_code(), collector.stderr_to_vec(), collector.copy_file_from(file, Vec::new())).await;
    collector.rm().await.map_err(Failure::from)?;
    let read = |line: io::Result<String>| line.map_err(|source| Failure::Io { operation: Operation::Telemetry, source }).and_then(|line| decode(&line));
    let exported = copied.map_or_else(|source| Validated::fail(source.into()), |bytes| BufRead::lines(bytes.as_slice()).map(read).collect());
    let (code, output, exported) = Validated::from(code.map_err(Failure::from))
        .map3(output.map_err(Failure::from).into(), exported, |code, output, exported: Vec<Exported>| (code, String::from_utf8_lossy_owned(output), exported))
        .ok()?;
    let (Attributes(method), service) = (
        Attributes::from([KeyValue::new("mcp.method.name", "tools/list")]),
        pid.map(|pid| Attributes::from([KeyValue::new(SERVICE_NAME, SERVICE), KeyValue::new(SERVICE_INSTANCE_ID, INSTANCE), KeyValue::new(PROCESS_PID, i64::from(pid))])),
    );
    let served = |resource: Option<&Resource>, schema: &str| {
        schema == SCHEMA_URL && resource.zip(service.as_ref()).is_some_and(|(described, Attributes(service))| service.iter().all(|attribute| described.attributes.contains(attribute)))
    };
    assert_eq!(code, Some(0), "collector exited {code:?}\n{output}");
    assert!(
        exported
            .iter()
            .flat_map(|line| &line.spans)
            .filter(|resource| served(resource.resource.as_ref(), &resource.schema_url))
            .flat_map(|resource| &resource.scope_spans)
            .flat_map(|scope| &scope.spans)
            .any(|span| span.trace_id == trace.to_bytes()
                && span.parent_span_id == parent.to_bytes()
                && span.kind() == SpanKind::Server
                && method.iter().all(|attribute| span.attributes.contains(attribute))),
        "no tools/list server span joined trace {trace} under span {parent} on bridge resource\n{output}"
    );
    assert!(
        exported
            .iter()
            .flat_map(|line| &line.metrics)
            .filter(|resource| served(resource.resource.as_ref(), &resource.schema_url))
            .flat_map(|resource| &resource.scope_metrics)
            .filter(|scope| scope.scope.as_ref().is_some_and(|instrumented| instrumented.version == env!("CARGO_PKG_VERSION")))
            .flat_map(|scope| &scope.metrics)
            .filter(|recorded| recorded.name == "mcp.server.operation.duration")
            .flat_map(|recorded| match &recorded.data {
                Some(Data::Histogram(histogram)) => histogram.data_points.as_slice(),
                _ => &[],
            })
            .any(|point| point.count > 0 && method.iter().all(|attribute| point.attributes.contains(attribute))),
        "no mcp.server.operation.duration point recorded for tools/list\n{output}"
    );
    let records = exported
        .iter()
        .flat_map(|line| &line.logs)
        .filter(|resource| served(resource.resource.as_ref(), &resource.schema_url))
        .flat_map(|resource| &resource.scope_logs)
        .flat_map(|scope| &scope.log_records)
        .collect::<Vec<_>>();
    assert!(records.iter().any(|record| record.body == Some(AnyValue::from(Value::from("shutting down")))), "no \"shutting down\" log record exported\n{output}");
    assert!(
        records.iter().any(|record| record.body == Some(AnyValue::from(Value::from("request completed"))) && method.iter().all(|attribute| record.attributes.contains(attribute))),
        "no \"request completed\" log record holds mcp.method.name=tools/list\n{output}"
    );
    Ok(())
}
