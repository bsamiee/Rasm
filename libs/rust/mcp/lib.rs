#![feature(error_reporter)]

use core::{error::Error, future::Future, net::SocketAddr, pin::pin};
use std::{backtrace::Backtrace, borrow::Cow, collections::HashMap, error::Report, io, process::ExitCode, sync::{Arc, LazyLock}};

use axum::{
    Router,
    body::Body,
    extract::Request,
    http::{Method, request::Parts},
    middleware::{self, Next},
};
use clap::{Args, CommandFactory as _, FromArgMatches as _, Parser, error::ErrorKind};
use futures::{FutureExt as _, Stream, StreamExt as _, TryStreamExt as _, future, stream};
use opentelemetry::{KeyValue, global, metrics::Histogram, propagation::TextMapPropagator as _, trace::TracerProvider as _};
use opentelemetry_appender_tracing::layer::OpenTelemetryTracingBridge;
use opentelemetry_otlp::{ExporterBuildError, LogExporter, MetricExporter, OTEL_EXPORTER_OTLP_ENDPOINT, SpanExporter, WithExportConfig as _};
use opentelemetry_sdk::{error::OTelSdkError, logs::SdkLoggerProvider, metrics::SdkMeterProvider, propagation::TraceContextPropagator, trace::SdkTracerProvider};
use opentelemetry_semantic_conventions::attribute::{ERROR_TYPE, EXCEPTION_MESSAGE, EXCEPTION_STACKTRACE, JSONRPC_REQUEST_ID, RPC_RESPONSE_STATUS_CODE, SERVICE_NAME, SERVICE_VERSION};
use rmcp::{
    ErrorData, Peer, RoleServer, ServerHandler,
    model::{
        ConstString as _, Implementation, PromptListChangedNotification, ProtocolVersion, ResourceListChangedNotification, ServerNotification, SubscriptionsListenRequestMethod,
        ToolListChangedNotification,
    },
    service::{RequestContext, SubscriptionContext, SubscriptionSendError},
    transport::{
        common::http_header::{HEADER_MCP_METHOD, HEADER_SESSION_ID},
        streamable_http_server::{StreamableHttpServerConfig, StreamableHttpService, session::local::LocalSessionManager},
    },
};
use tokio::{
    net::TcpListener,
    signal::unix::{SignalKind, signal},
    sync::watch,
    time::Instant,
};
use tokio_stream::wrappers::WatchStream;
use tokio_util::{sync::CancellationToken, task::TaskTracker};
use tracing::{Instrument as _, field::Empty};
use tracing_opentelemetry::{OpenTelemetrySpanExt as _, SetParentError};
use tracing_subscriber::{EnvFilter, filter::Directive, fmt::time::SystemTime, layer::SubscriberExt as _, util::SubscriberInitExt as _};
use url::Url;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Endpoint: Send + Sync + Sized + 'static {
    type Error: Error + Send + Sync + 'static;

    fn session(&self, stopping: &CancellationToken) -> impl ServerHandler + use<Self>;

    fn serve(self: Arc<Self>, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send {
        async move {
            stopping.cancelled().await;
            Ok(())
        }
    }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

pub const PROTOCOL_VERSIONS: &[ProtocolVersion] = &[ProtocolVersion::V_2025_06_18, ProtocolVersion::V_2025_11_25, ProtocolVersion::V_2026_07_28];

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args, Clone, Debug)]
struct Options {
    #[arg(long, env = "MCP_LISTEN", help = "Socket address the Streamable HTTP endpoint binds")]
    listen: SocketAddr,
    #[arg(
        long = "log",
        env = EnvFilter::DEFAULT_ENV,
        value_delimiter = ',',
        default_values = ["info", "rmcp=warn", "rmcp::service=error", "opentelemetry=warn"],
        help = "Tracing filter directives"
    )]
    filter: Vec<Directive>,
    #[arg(long = "otlp-endpoint", env = OTEL_EXPORTER_OTLP_ENDPOINT, help = "OTLP/HTTP base endpoint receiving traces, metrics, and logs, absent for no export")]
    export: Option<Url>,
}

#[derive(Clone, Copy, Debug)]
pub enum Target<'name> {
    Tool(&'name str),
    Prompt(&'name str),
    Resource(&'name str),
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Failed<S, X> {
    #[error("cannot build the OTLP exporters")]
    Export(#[source] ExporterBuildError),
    #[error("cannot flush telemetry")]
    Flush(#[source] OTelSdkError),
    #[error("cannot start the async runtime")]
    Runtime(#[source] io::Error),
    #[error("cannot listen for shutdown signals")]
    Signal(#[source] io::Error),
    #[error("cannot bind {0}")]
    Bind(SocketAddr, #[source] io::Error),
    #[error("cannot start the endpoint")]
    Start(#[source] S),
    #[error("endpoint ended")]
    Ended(#[source] X),
    #[error("HTTP server failed")]
    Serve(#[source] io::Error),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn observe<T, F, C>(method: &str, target: Option<Target<'_>>, context: &RequestContext<RoleServer>, failed: C, call: F) -> Result<T, ErrorData>
where
    F: Future<Output = Result<T, ErrorData>>,
    C: FnOnce(&T) -> bool,
{
    static DURATION: LazyLock<Histogram<f64>> = LazyLock::new(|| global::meter(env!("CARGO_PKG_NAME")).f64_histogram("mcp.server.operation.duration").with_unit("s").build());
    let (tool, prompt, resource) = match target {
        Some(Target::Tool(name)) => (Some(name), None, None),
        Some(Target::Prompt(name)) => (None, Some(name), None),
        Some(Target::Resource(uri)) => (None, None, Some(uri)),
        None => (None, None, None),
    };
    let span = tracing::info_span!(
        "request",
        otel.name = tool.or(prompt).or(resource).map_or_else(|| method.to_owned(), |name| format!("{method} {name}")),
        otel.kind = "server",
        mcp.method.name = method,
        gen_ai.tool.name = tool,
        gen_ai.prompt.name = prompt,
        mcp.resource.uri = resource,
        mcp.protocol.version = context.protocol_version().as_ref().map(ProtocolVersion::as_str),
        mcp.session.id = context.extensions.get::<Parts>().and_then(|parts| parts.headers.get(HEADER_SESSION_ID)).and_then(|value| value.to_str().ok()),
        { JSONRPC_REQUEST_ID } = %context.id,
        { ERROR_TYPE } = Empty,
        { RPC_RESPONSE_STATUS_CODE } = Empty,
    );
    if let Some(parent) = context.meta.get_traceparent() {
        let carrier = [("traceparent", Some(parent)), ("tracestate", context.meta.get_tracestate())]
            .into_iter()
            .filter_map(|(key, value)| value.map(|value| (key.to_owned(), value.to_owned())))
            .collect::<HashMap<_, _>>();
        match span.set_parent(TraceContextPropagator::new().extract(&carrier)) {
            Ok(()) | Err(SetParentError::LayerNotFound | SetParentError::SpanDisabled | SetParentError::AlreadyStarted) => {}
        }
    }
    let started = Instant::now();
    let result = call.instrument(span.clone()).await;
    let elapsed = started.elapsed().as_secs_f64();
    let error_type = match &result {
        Ok(value) => failed(value).then_some(Cow::Borrowed("tool_error")),
        Err(error) => Some(Cow::Owned(error.code.0.to_string())),
    };
    span.in_scope(|| {
        error_type.iter().for_each(|kind| {
            span.record(ERROR_TYPE, kind.as_ref());
        });
        match &result {
            Ok(_) => tracing::info!(elapsed, "request completed"),
            Err(error) => {
                span.record(RPC_RESPONSE_STATUS_CODE, error.code.0);
                tracing::warn!(elapsed, { EXCEPTION_MESSAGE } = %error.message, "request failed");
            }
        }
    });
    DURATION.record(elapsed, &[KeyValue::new("mcp.method.name", method.to_owned())].into_iter().chain(error_type.map(|kind| KeyValue::new(ERROR_TYPE, kind))).collect::<Vec<_>>());
    result
}

pub fn changes<T, P, R>(tools: watch::Receiver<T>, prompts: watch::Receiver<P>, resources: watch::Receiver<R>) -> impl Stream<Item = ServerNotification> + Send + 'static
where
    T: Clone + Send + Sync + 'static,
    P: Clone + Send + Sync + 'static,
    R: Clone + Send + Sync + 'static,
{
    stream::select(
        WatchStream::from_changes(tools).map(|_| ServerNotification::ToolListChangedNotification(ToolListChangedNotification::default())),
        stream::select(
            WatchStream::from_changes(prompts).map(|_| ServerNotification::PromptListChangedNotification(PromptListChangedNotification::default())),
            WatchStream::from_changes(resources).map(|_| ServerNotification::ResourceListChangedNotification(ResourceListChangedNotification::default())),
        ),
    )
}

pub async fn relay<S>(peer: Peer<RoleServer>, notifications: S)
where S: Stream<Item = ServerNotification> + Send {
    if let Err(error) = notifications.map(Ok).try_for_each(|notification| peer.send_notification(notification)).await {
        tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "session closed");
    }
}

pub async fn listen<S>(context: &SubscriptionContext, notifications: S, stopping: &CancellationToken) -> Result<(), ErrorData>
where S: Stream<Item = ServerNotification> + Send {
    let ended = future::select(pin!(stopping.cancelled()), pin!(context.cancelled()));
    let sent = notifications
        .take_until(ended)
        .map(Ok)
        .try_for_each(|notification| async move {
            match context.sink().send(notification).await {
                Err(SubscriptionSendError::NotificationNotAccepted(_)) => Ok(()),
                sent => sent,
            }
        })
        .await;
    match sent {
        Ok(()) | Err(SubscriptionSendError::SubscriptionClosed) => Ok(()),
        Err(error) => Err(ErrorData::internal_error(Report::new(error).to_string(), None)),
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<A, F, E, B>(build: F) -> ExitCode
where
    A: Parser,
    F: AsyncFnOnce(A, Implementation) -> Result<E, B>,
    E: Endpoint,
    B: Error + Send + Sync + 'static,
{
    type Exporters = (SdkTracerProvider, SdkLoggerProvider, SdkMeterProvider);
    fn install<S, X>(options: &Options, identity: &Implementation) -> Result<Option<Exporters>, Failed<S, X>> {
        let mut json = json_subscriber::JsonLayer::stderr();
        json.with_timer("timestamp", SystemTime).with_level("level").with_target("target").with_top_level_flattened_span_list().with_flattened_event();
        json.add_static_field(SERVICE_NAME, identity.name.clone().into());
        json.add_static_field(SERVICE_VERSION, identity.version.clone().into());
        let exporters = options
            .export
            .as_ref()
            .map(|endpoint| {
                let signal = |path: &str| format!("{}/v1/{path}", endpoint.as_str().trim_end_matches('/'));
                let resource = opentelemetry_sdk::Resource::builder().with_service_name(identity.name.clone()).with_attribute(KeyValue::new(SERVICE_VERSION, identity.version.clone())).build();
                Ok::<Exporters, ExporterBuildError>((
                    SdkTracerProvider::builder().with_resource(resource.clone()).with_batch_exporter(SpanExporter::builder().with_http().with_endpoint(signal("traces")).build()?).build(),
                    SdkLoggerProvider::builder().with_resource(resource.clone()).with_batch_exporter(LogExporter::builder().with_http().with_endpoint(signal("logs")).build()?).build(),
                    SdkMeterProvider::builder().with_resource(resource).with_periodic_exporter(MetricExporter::builder().with_http().with_endpoint(signal("metrics")).build()?).build(),
                ))
            })
            .transpose();
        let installed = exporters.as_ref().ok().and_then(Option::as_ref);
        installed.iter().for_each(|(_, _, metrics)| global::set_meter_provider(metrics.clone()));
        tracing_subscriber::registry()
            .with(options.filter.iter().cloned().fold(EnvFilter::default(), EnvFilter::add_directive))
            .with(json)
            .with(installed.map(|(traces, _, _)| tracing_opentelemetry::layer().with_tracer(traces.tracer(identity.name.clone()))))
            .with(installed.map(|(_, logs, _)| OpenTelemetryTracingBridge::new(logs)))
            .init();
        std::panic::set_hook(Box::new(|panic| tracing::error!({ EXCEPTION_MESSAGE } = %panic, { EXCEPTION_STACKTRACE } = %Backtrace::force_capture(), "panicked")));
        exporters.map_err(Failed::Export)
    }
    async fn host<E, B, G>(options: Options, build: G) -> Result<Validated<(), Failed<B, E::Error>>, Failed<B, E::Error>>
    where
        E: Endpoint,
        G: Future<Output = Result<E, B>>,
    {
        let listener = TcpListener::bind(options.listen).await.map_err(|error| Failed::Bind(options.listen, error))?;
        tracing::info!(listen = %options.listen, "listening");
        let endpoint = Arc::new(build.await.map_err(Failed::Start)?);
        let (draining, stopping, sessions) = (CancellationToken::new(), CancellationToken::new(), CancellationToken::new());
        let (requests, streams) = (TaskTracker::new(), TaskTracker::new());
        let config = StreamableHttpServerConfig::default().with_json_response(true).with_cancellation_token(sessions.clone()).enforce_origin_validation();
        let (factory, scope) = (Arc::clone(&endpoint), stopping.clone());
        let service = StreamableHttpService::new(move || Ok(factory.session(&scope)), Arc::new(LocalSessionManager::default()), config);
        let (tracked, listened) = (requests.clone(), streams.clone());
        let router = Router::new().route_service("/", service).layer(middleware::from_fn(move |request: Request, next: Next| {
            let token = (request.method() == Method::POST)
                .then(|| if request.headers().get(HEADER_MCP_METHOD).is_some_and(|method| method == SubscriptionsListenRequestMethod::VALUE) { listened.token() } else { tracked.token() });
            next.run(request).map(|response| {
                response.map(|body| match token {
                    Some(token) => Body::from_stream(stream::unfold((body.into_data_stream(), token), |(mut data, token)| async move { data.next().await.map(|chunk| (chunk, (data, token))) })),
                    None => body,
                })
            })
        }));
        let mut server = pin!(axum::serve(listener, router).with_graceful_shutdown(draining.clone().cancelled_owned()).into_future());
        let mut lifetime = pin!(Arc::clone(&endpoint).serve(stopping.clone()));
        let (mut interrupt, mut terminate, mut hangup) = (
            signal(SignalKind::interrupt()).map_err(Failed::Signal)?,
            signal(SignalKind::terminate()).map_err(Failed::Signal)?,
            signal(SignalKind::hangup()).map_err(Failed::Signal)?,
        );
        let ended = tokio::select! {
            biased;
            _ = interrupt.recv() => None,
            _ = terminate.recv() => None,
            _ = hangup.recv() => None,
            ended = &mut lifetime => Some(ended),
            served = &mut server => return Ok(served.map_err(Failed::Serve).into()),
        };
        tracing::info!("shutting down");
        draining.cancel();
        requests.close();
        requests.wait().await;
        stopping.cancel();
        let ended = match ended {
            Some(ended) => ended,
            None => lifetime.await,
        };
        streams.close();
        streams.wait().await;
        sessions.cancel();
        let served = server.await;
        Ok(Validated::from(ended.map_err(Failed::Ended)).map2(served.map_err(Failed::Serve).into(), |(), ()| ()))
    }
    let command = Options::augment_args(A::command());
    let matches = command.clone().get_matches();
    let (options, app, identity) = match (Options::from_arg_matches(&matches), A::from_arg_matches(&matches), command.get_version()) {
        (Ok(options), Ok(app), Some(version)) => {
            let identity = Implementation::new(command.get_name(), version);
            (options, app, command.get_about().map_or(identity.clone(), |about| identity.with_description(about.to_string())))
        }
        (Err(error), _, _) | (_, Err(error), _) => error.exit(),
        (Ok(_), Ok(_), None) => command.clone().error(ErrorKind::MissingRequiredArgument, "binary states no version, derive its Parser with #[command(version)]").exit(),
    };
    let outcome = install::<B, E::Error>(&options, &identity)
        .map(|exporters| {
            let hosted = tokio::runtime::Runtime::new().map_err(Failed::Runtime).and_then(|runtime| runtime.block_on(host(options, build(app, identity)))).unwrap_or_else(Validated::fail);
            let flushed = exporters.into_iter().flat_map(|(traces, logs, metrics)| [traces.shutdown(), logs.shutdown(), metrics.shutdown()]).map(|flush| flush.map_err(Failed::Flush)).collect::<Validated<(), _>>();
            hosted.map2(flushed, |(), ()| ())
        })
        .unwrap_or_else(Validated::fail);
    match outcome.ok() {
        Ok(()) => ExitCode::SUCCESS,
        Err(errors) => {
            errors.iter().for_each(|error| tracing::error!({ EXCEPTION_MESSAGE } = %Report::new(error), "server failed"));
            ExitCode::FAILURE
        }
    }
}
