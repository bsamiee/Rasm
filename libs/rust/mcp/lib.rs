#![feature(associated_type_defaults, error_reporter, iterator_try_collect, lock_value_accessors, nonpoison_mutex, sync_nonpoison, trait_alias)]

use core::{error::Error, future::ready, mem::discriminant, net::SocketAddr, pin::pin, str::Utf8Error};
use std::{
    borrow::Cow,
    collections::BTreeMap,
    error::Report,
    io, process,
    sync::{Arc, LazyLock, OnceLock, Weak, nonpoison::Mutex},
};

use axum::http::{request::Parts, uri::Authority};
use clap::{Args, FromArgMatches as _, Parser};
use futures::{FutureExt as _, Stream, StreamExt as _, TryFutureExt as _, TryStreamExt as _, future, stream};
use jsonschema::ValidationError;
use nonempty_collections::NEVec;
use opentelemetry::{InstrumentationScope, KeyValue, global, metrics::Histogram, propagation::Extractor};
use opentelemetry_semantic_conventions::attribute::{ERROR_TYPE, EXCEPTION_MESSAGE, JSONRPC_REQUEST_ID, RPC_RESPONSE_STATUS_CODE};
use rasm_telemetry::{Telemetry, options};
use rmcp::{
    ErrorData, RoleServer, ServerHandler, ServiceError,
    model::{
        CallToolRequest, CallToolRequestParams, CallToolResponse, CallToolResult, CancelTaskParams, CancelTaskRequest, ClientRequest, CompleteRequest, CompleteRequestParams, CompleteResult,
        CustomRequest, CustomResult, ErrorCode, GetPromptRequest, GetPromptRequestParams, GetPromptResponse, GetTaskParams, GetTaskRequest, GetTaskResult, Implementation, JsonObject,
        ListPromptsRequest, ListPromptsResult, ListResourceTemplatesRequest, ListResourceTemplatesResult, ListResourcesRequest, ListResourcesResult, ListToolsRequest, ListToolsResult,
        PaginatedRequestParams, ProgressNotificationParam, PromptListChangedNotification, ProtocolVersion, ReadResourceRequest, ReadResourceRequestParams, ReadResourceResponse, Request,
        ResourceListChangedNotification, ResourceUpdatedNotification, ResourceUpdatedNotificationParam, ResourcesCapability, ServerConfig, ServerNotification, ServerResult, SubscribeRequestMethod,
        SubscribeRequestParams, SubscriptionFilter, ToolListChangedNotification, UnsubscribeRequestMethod, UnsubscribeRequestParams, UpdateTaskParams, UpdateTaskRequest,
    },
    service::{DEFAULT_SUBSCRIPTION_CHANNEL_CAPACITY, NotificationContext, RequestContext, SubscriptionContext},
    transport::{StreamableHttpServerConfig, StreamableHttpService, common::http_header::HEADER_SESSION_ID, streamable_http_server::session::local::LocalSessionManager},
};
use serde_json::Value;
use serde_urlencoded::de;
#[cfg(unix)]
use tokio::signal::unix::{SignalKind, signal};
#[cfg(windows)]
use tokio::signal::windows;
use tokio::{
    net, runtime,
    sync::{broadcast, watch},
    time::Instant,
};
#[cfg(windows)]
use tokio_stream::wrappers::CtrlCStream;
#[cfg(unix)]
use tokio_stream::wrappers::SignalStream;
use tokio_stream::wrappers::{BroadcastStream, WatchStream, errors::BroadcastStreamRecvError};
use tokio_util::{
    sync::CancellationToken,
    task::{AbortOnDropHandle, TaskTracker},
};
use tracing::{Instrument as _, field::Empty};
use tracing_opentelemetry::{OpenTelemetrySpanExt as _, SetParentError};
use tracing_subscriber::{EnvFilter, layer::Identity};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Endpoint: Send + Sync + 'static {
    type Error: Error + Send + Sync + 'static;

    fn info(&self) -> &ServerConfig;

    fn handle(&self, request: ClientRequest, context: RequestContext<RoleServer>) -> impl Future<Output = Result<ServerResult, ErrorData>> + Send;

    fn subscribe(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, Unserved>;

    fn serve(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send;

    fn listed(&self) -> SubscriptionFilter { SubscriptionFilter::builder().tools_list_changed().prompts_list_changed().resources_list_changed().build().supported_by(&self.info().capabilities) }
}

#[derive(Debug, derive_more::Display)]
enum Operation {
    #[display("start async runtime")]
    Runtime,
    #[display("bind {_0}")]
    Bind(SocketAddr),
    #[display("read bound socket address")]
    Address,
    #[display("listen for shutdown signals")]
    Signal,
    #[display("serve HTTP")]
    Serve,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

pub const PROTOCOL_VERSIONS: &[ProtocolVersion] = &[ProtocolVersion::V_2025_06_18, ProtocolVersion::V_2025_11_25, ProtocolVersion::V_2026_07_28];

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args)]
struct Options {
    #[arg(long, env = "MCP_LISTEN", help = "Socket address of the Streamable HTTP endpoint")]
    listen: SocketAddr,
    #[arg(long = "allowed-host", env = "MCP_ALLOWED_HOSTS", value_name = "AUTHORITY", value_delimiter = ',', help = "Host authority to answer beside loopback names and bound address")]
    hosts: Vec<Authority>,
    #[command(flatten)]
    telemetry: options::Options,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum Invalid {
    #[error("cannot route resource {pattern}")]
    Route { pattern: String, source: matchit::InsertError },
    #[error("cannot compile tool {tool} input schema")]
    Schema { tool: Cow<'static, str>, source: ValidationError<'static> },
}

#[derive(Debug, thiserror::Error)]
pub enum Unserved {
    #[error("{uri} names no resource, list resources and resource templates")]
    Unrouted { uri: String },
    #[error("{uri} holds a variable that is not percent-encoded UTF-8")]
    Unencoded { uri: String, source: Utf8Error },
    #[error("{uri} variables break its template, read resources/templates/list for variable forms")]
    Malformed { uri: String, source: serde_path_to_error::Error<de::Error> },
    #[error("{name} names no tool, list tools")]
    Unlisted { name: Cow<'static, str> },
    #[error("requestState names no paused call of this tool with these arguments, call again without it")]
    Unparked,
    #[error("server is shutting down, call again after restart")]
    Stopping,
}

impl From<Unserved> for ErrorData {
    fn from(value: Unserved) -> Self {
        let code = match &value {
            Unserved::Unrouted { uri: _ } => ErrorCode::RESOURCE_NOT_FOUND,
            Unserved::Unencoded { uri: _, source: _ } | Unserved::Malformed { uri: _, source: _ } | Unserved::Unlisted { name: _ } | Unserved::Unparked => ErrorCode::INVALID_PARAMS,
            Unserved::Stopping => ErrorCode::INTERNAL_ERROR,
        };
        Self::new(code, Report::new(value).to_string(), None)
    }
}

#[derive(Debug, thiserror::Error)]
enum Failed<X> {
    #[error("cannot {operation}")]
    Io { operation: Operation, source: io::Error },
    #[error("endpoint failed")]
    Endpoint(#[source] X),
    #[error("second shutdown signal arrived before endpoint stopped")]
    Abandoned,
}

impl<X> From<Failed<X>> for NEVec<Failed<X>> {
    fn from(value: Failed<X>) -> Self { Self::new(value) }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug)]
pub struct Notifier(broadcast::Sender<ServerNotification>);

impl Notifier {
    pub fn notify(&self, notification: impl Into<ServerNotification>) { let (Ok(_) | Err(broadcast::error::SendError(_))) = self.0.send(notification.into()); }

    #[must_use]
    pub fn subscribe(&self) -> BroadcastStream<ServerNotification> { BroadcastStream::new(self.0.subscribe()) }

    pub fn resync(&self, filter: &SubscriptionFilter) { resynced(filter, filter.resource_subscriptions.iter().flatten().cloned()).for_each(|notification| self.notify(notification)); }
}

#[derive(Clone, Debug, Default)]
pub struct Progress(watch::Sender<(f64, Option<f64>, Option<String>)>);

impl Progress {
    pub fn relay<F>(&self, context: &RequestContext<RoleServer>, call: F) -> impl Future<Output = F::Output> + Send
    where F: Future<Output: Send> + Send {
        let (peer, updates) = (context.peer.clone(), WatchStream::new(self.0.subscribe()));
        let updates = stream::iter(context.meta.get_progress_token().map(|token| {
            updates.filter(|&(progress, _, _)| ready(progress > 0.0_f64)).map(move |(progress, total, message)| {
                let mut update = ProgressNotificationParam::new(token.clone(), progress);
                (update.total, update.message) = (total, message);
                Ok(update)
            })
        }))
        .flatten();
        async move {
            let relayed = updates.try_for_each(|update| peer.notify_progress(update)).unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "progress relay failed"));
            match future::select(pin!(call), pin!(relayed)).await {
                future::Either::Left((output, _)) => output,
                future::Either::Right(((), call)) => call.await,
            }
        }
    }

    pub fn stage(&self, message: impl Into<String>) { self.0.send_modify(|(progress, _, last)| (*progress, *last) = (*progress + 1.0_f64, Some(message.into()))); }

    pub fn report(&self, progress: f64, total: Option<f64>, message: Option<String>) { self.0.send_if_modified(|last| (progress > last.0).then(|| *last = (progress, total, message)).is_some()); }
}

struct Host<E> {
    endpoint: E,
    notifier: Notifier,
    stopping: CancellationToken,
    requests: TaskTracker,
    watches: TaskTracker,
    leases: Mutex<BTreeMap<String, Weak<AbortOnDropHandle<()>>>>,
}

struct Session<E> {
    host: Arc<Host<E>>,
    subscribed: watch::Sender<BTreeMap<String, Arc<AbortOnDropHandle<()>>>>,
    relay: OnceLock<AbortOnDropHandle<()>>,
}

impl<E> Session<E>
where E: Endpoint
{
    fn hold(&self, uri: String) -> Result<(), Unserved> {
        let mut leases = self.host.leases.lock();
        leases.retain(|_, held| held.strong_count() > 0);
        let held = if let Some(held) = leases.get(&uri).and_then(Weak::upgrade) {
            held
        } else {
            let watched = self.host.stopping.clone().run_until_cancelled_owned(self.host.endpoint.subscribe(&uri)?).map(drop).instrument(tracing::info_span!("watch", mcp.resource.uri = uri));
            Arc::new(AbortOnDropHandle::new(self.host.watches.spawn(watched)))
        };
        leases.insert(uri.clone(), Arc::downgrade(&held));
        self.subscribed.send_modify(|uris| drop(uris.insert(uri, held)));
        Ok(())
    }

    async fn dispatch(&self, request: ClientRequest, context: RequestContext<RoleServer>) -> Result<ServerResult, ErrorData> {
        struct Carrier<'meta>(&'meta JsonObject);
        impl Extractor for Carrier<'_> {
            fn get(&self, key: &str) -> Option<&str> { self.0.get(key).and_then(Value::as_str) }

            fn keys(&self) -> Vec<&str> { self.0.keys().map(String::as_str).collect() }
        }
        static DURATION: LazyLock<Histogram<f64>> = LazyLock::new(|| {
            const BOUNDARIES: [f64; 14] = [0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1.0, 2.0, 5.0, 10.0, 30.0, 60.0, 120.0, 300.0];
            global::meter_with_scope(InstrumentationScope::builder(env!("CARGO_PKG_NAME")).with_version(env!("CARGO_PKG_VERSION")).build())
                .f64_histogram("mcp.server.operation.duration")
                .with_unit("s")
                .with_boundaries(BOUNDARIES.to_vec())
                .build()
        });
        let _held = self.host.requests.token();
        let (method, version) = (request.method().to_owned(), context.protocol_version());
        let (tool, prompt, uri) = match &request {
            ClientRequest::CallToolRequest(call) => (Some(call.params.name.to_string()), None, None),
            ClientRequest::GetPromptRequest(get) => (None, Some(get.params.name.clone()), None),
            ClientRequest::ReadResourceRequest(read) => (None, None, Some(read.params.uri.clone())),
            _ => (None, None, None),
        };
        let span = tracing::info_span!(
            "request",
            otel.name = tool.as_ref().or(prompt.as_ref()).or(uri.as_ref()).map_or_else(|| method.clone(), |name| format!("{method} {name}")),
            otel.kind = "server",
            mcp.method.name = method,
            gen_ai.tool.name = tool.as_deref(),
            gen_ai.prompt.name = prompt.as_deref(),
            mcp.resource.uri = uri.as_deref(),
            mcp.protocol.version = version.as_ref().map(ProtocolVersion::as_str),
            mcp.session.id = context.extensions.get::<Parts>().and_then(|parts| parts.headers.get(HEADER_SESSION_ID)).and_then(|value| value.to_str().ok()),
            { JSONRPC_REQUEST_ID } = %context.id,
            { ERROR_TYPE } = Empty,
            { RPC_RESPONSE_STATUS_CODE } = Empty,
        );
        let (Ok(()) | Err(SetParentError::LayerNotFound | SetParentError::SpanDisabled | SetParentError::AlreadyStarted)) =
            span.set_parent(global::get_text_map_propagator(|propagator| propagator.extract(&Carrier(&context.meta))));
        let (started, cancelled) = (Instant::now(), context.ct.clone());
        let result = match request {
            _ if self.host.requests.is_closed() => Err(Unserved::Stopping.into()),
            ClientRequest::SubscribeRequest(subscribe) if matches!(self.host.endpoint.info().capabilities.resources, Some(ResourcesCapability { subscribe: Some(true), .. })) =>
                self.hold(subscribe.params.uri).map(ServerResult::empty).map_err(ErrorData::from),
            ClientRequest::SubscribeRequest(_) => Err(ErrorData::method_not_found::<SubscribeRequestMethod>()),
            ClientRequest::UnsubscribeRequest(unsubscribe) => {
                self.subscribed.send_if_modified(|uris| uris.remove(&unsubscribe.params.uri).is_some());
                Ok(ServerResult::empty(()))
            }
            other => self.host.endpoint.handle(other, context).instrument(span.clone()).await,
        }
        .map_err(|error| match error.code {
            ErrorCode::RESOURCE_NOT_FOUND if version.is_some_and(|version| !version.has_initialize()) => ErrorData { code: ErrorCode::INVALID_PARAMS, ..error },
            _ => error,
        });
        let elapsed = started.elapsed();
        let (error_type, failure) = match (&result, cancelled.is_cancelled()) {
            (Ok(_) | Err(_), true) => (Some(Cow::Borrowed("cancelled")), None),
            (Ok(ServerResult::CallToolResult(CallToolResult { is_error: Some(true), .. })), false) => (Some(Cow::Borrowed("tool_error")), None),
            (Ok(_), false) => (None, None),
            (Err(error), false) => (Some(Cow::Owned(error.code.0.to_string())), Some(error)),
        };
        span.record(ERROR_TYPE, error_type.as_deref());
        if let Some(error) = failure {
            tracing::warn!(parent: span.record(RPC_RESPONSE_STATUS_CODE, error_type.as_deref()), elapsed = elapsed.as_secs_f64(), { EXCEPTION_MESSAGE } = %error.message, "request failed");
        } else {
            tracing::info!(parent: &span, elapsed = elapsed.as_secs_f64(), "request completed");
        }
        let attributes = [
            Some(KeyValue::new("mcp.method.name", method)),
            tool.map(|name| KeyValue::new("gen_ai.tool.name", name)),
            prompt.map(|name| KeyValue::new("gen_ai.prompt.name", name)),
            error_type.map(|kind| KeyValue::new(ERROR_TYPE, kind)),
        ];
        DURATION.record(elapsed.as_secs_f64(), &attributes.into_iter().flatten().collect::<Vec<_>>());
        result
    }

    fn notifications(&self, filter: SubscriptionFilter) -> impl Stream<Item = ServerNotification> + Send + 'static {
        let (admitted, held) = (resynced(&filter, []).map(|notification| discriminant(&notification)).collect::<Vec<_>>(), self.subscribed.subscribe());
        self.host.notifier.subscribe().flat_map(move |received| {
            stream::iter(match received {
                Ok(ServerNotification::ResourceUpdatedNotification(updated)) => Vec::from_iter(held.borrow().contains_key(&updated.params.uri).then(|| updated.into())),
                Ok(other) => Vec::from_iter(admitted.contains(&discriminant(&other)).then_some(other)),
                Err(BroadcastStreamRecvError::Lagged(_)) => Vec::from_iter(resynced(&filter, Vec::from_iter(held.borrow().keys().cloned()))),
            })
        })
    }
}

macro_rules! dispatched {
    ($method:ident($request:ident: $params:ty) = $sent:expr) => {
        async fn $method(&self, $request: $params, context: RequestContext<RoleServer>) -> Result<(), ErrorData> { self.dispatch(ClientRequest::from($sent), context).await.map(drop) }
    };
    ($method:ident($request:ident: $params:ty) -> $response:ty = $sent:expr => $($variant:ident)|+) => {
        async fn $method(&self, $request: $params, context: RequestContext<RoleServer>) -> Result<$response, ErrorData> {
            match self.dispatch(ClientRequest::from($sent), context).await? {
                $(ServerResult::$variant(result) => Ok(result.into()),)+
                _ => Err(internal(ServiceError::UnexpectedResponse)),
            }
        }
    };
}

impl<E> ServerHandler for Session<E>
where E: Endpoint
{
    dispatched!(complete(request: CompleteRequestParams) -> CompleteResult = CompleteRequest::new(request) => CompleteResult);
    dispatched!(get_prompt(request: GetPromptRequestParams) -> GetPromptResponse = GetPromptRequest::new(request) => GetPromptResult | InputRequiredResult);
    dispatched!(list_prompts(request: Option<PaginatedRequestParams>) -> ListPromptsResult = ListPromptsRequest { params: request, ..Default::default() } => ListPromptsResult);
    dispatched!(list_resources(request: Option<PaginatedRequestParams>) -> ListResourcesResult = ListResourcesRequest { params: request, ..Default::default() } => ListResourcesResult);
    dispatched!(list_resource_templates(request: Option<PaginatedRequestParams>) -> ListResourceTemplatesResult =
        ListResourceTemplatesRequest { params: request, ..Default::default() } => ListResourceTemplatesResult);
    dispatched!(read_resource(request: ReadResourceRequestParams) -> ReadResourceResponse = ReadResourceRequest::new(request) => ReadResourceResult | InputRequiredResult);
    dispatched!(subscribe(request: SubscribeRequestParams) = Request::<SubscribeRequestMethod, _>::new(request));
    dispatched!(unsubscribe(request: UnsubscribeRequestParams) = Request::<UnsubscribeRequestMethod, _>::new(request));
    dispatched!(call_tool(request: CallToolRequestParams) -> CallToolResponse = CallToolRequest::new(request) => CallToolResult | InputRequiredResult | CreateTaskResult);
    dispatched!(list_tools(request: Option<PaginatedRequestParams>) -> ListToolsResult = ListToolsRequest { params: request, ..Default::default() } => ListToolsResult);
    dispatched!(get_task(request: GetTaskParams) -> GetTaskResult = GetTaskRequest::new(request) => GetTaskResult);
    dispatched!(update_task(request: UpdateTaskParams) = UpdateTaskRequest::new(request));
    dispatched!(cancel_task(request: CancelTaskParams) = CancelTaskRequest::new(request));
    dispatched!(on_custom_request(request: CustomRequest) -> CustomResult = request => CustomResult);

    fn get_info(&self) -> ServerConfig { self.host.endpoint.info().clone() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { Cow::Borrowed(PROTOCOL_VERSIONS) }

    fn accepted_subscription_filter(&self, requested: &SubscriptionFilter) -> Option<SubscriptionFilter> {
        let mut accepted = requested.supported_by(&self.host.endpoint.info().capabilities);
        if let Some(uris) = accepted.resource_subscriptions.as_mut() {
            uris.retain(|uri| self.hold(uri.clone()).inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "subscription refused")).is_ok());
        }
        Some(accepted)
    }

    async fn on_initialized(&self, context: NotificationContext<RoleServer>) {
        let notifications = self.notifications(self.host.endpoint.listed());
        let relayed = async move { notifications.map(Ok).try_for_each(|notification| context.peer.send_notification(notification)).await };
        self.relay.get_or_init(|| AbortOnDropHandle::new(tokio::spawn(relayed.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "session closed")))));
    }

    async fn listen(&self, context: SubscriptionContext) -> Result<(), ErrorData> {
        let notifications = self.notifications(context.accepted().clone()).take_until(self.host.stopping.cancelled()).take_until(context.cancelled());
        let sent = notifications.map(Ok).try_for_each(|notification| context.sink().send(notification));
        sent.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "subscription closed")).await;
        Ok(())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn resynced<U>(filter: &SubscriptionFilter, uris: U) -> impl Iterator<Item = ServerNotification> + use<U>
where U: IntoIterator<Item = String> {
    [
        (filter.tools_list_changed, ToolListChangedNotification::default().into()),
        (filter.prompts_list_changed, PromptListChangedNotification::default().into()),
        (filter.resources_list_changed, ResourceListChangedNotification::default().into()),
    ]
    .into_iter()
    .filter_map(|(listed, notification)| (listed == Some(true)).then_some(notification))
    .chain(uris.into_iter().map(|uri| ResourceUpdatedNotification::new(ResourceUpdatedNotificationParam::new(uri)).into()))
}

fn internal(error: impl Error) -> ErrorData { ErrorData::internal_error(Report::new(error).to_string(), None) }

#[must_use]
pub fn fault(error: ServiceError) -> ErrorData { if let ServiceError::McpError(data) = error { data } else { internal(error) } }

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<P, E>(version: &'static str, build: impl AsyncFnOnce(P, Implementation, Notifier) -> Result<E, E::Error>) -> process::ExitCode
where
    P: Parser,
    E: Endpoint,
{
    let mut command = Options::augment_args(P::command().version(version));
    let matches = command.get_matches_mut();
    let (options, args) = Options::from_arg_matches(&matches).and_then(|options| P::from_arg_matches(&matches).map(|args| (options, args))).unwrap_or_else(|error| error.exit());
    let identity = command.get_about().into_iter().fold(Implementation::new(command.get_name(), version), |identity, about| identity.with_description(about.to_string()));
    let Options { listen, hosts, telemetry } = options;
    let listening = async move {
        let signals = cfg_select! {
            unix => [SignalKind::interrupt(), SignalKind::terminate(), SignalKind::hangup()].map(|kind| signal(kind).map(SignalStream::new)),
            windows => [windows::ctrl_c().map(CtrlCStream::new)],
        };
        let mut signals = signals.into_iter().try_collect::<stream::SelectAll<_>>().map_err(|source| Failed::Io { operation: Operation::Signal, source })?;
        let socket = net::TcpListener::bind(listen).await.map_err(|source| Failed::Io { operation: Operation::Bind(listen), source })?;
        let address = socket.local_addr().map_err(|source| Failed::Io { operation: Operation::Address, source })?;
        let notifier = Notifier(broadcast::Sender::new(DEFAULT_SUBSCRIPTION_CHANNEL_CAPACITY));
        let future::Either::Left((started, _)) = future::select(pin!(build(args, identity, notifier.clone())), signals.next()).await else { return Ok(()) };
        let endpoint = started.map_err(Failed::Endpoint)?;
        tracing::info!(listen = %address, "listening");
        let host = Arc::new(Host { endpoint, notifier, stopping: CancellationToken::new(), requests: TaskTracker::new(), watches: TaskTracker::new(), leases: Mutex::default() });
        let manager = Arc::new(LocalSessionManager::default());
        let mut config = StreamableHttpServerConfig::default().with_json_response(true).enforce_origin_validation();
        config.allowed_hosts.extend(hosts.iter().map(Authority::to_string).chain(Some(address.ip()).filter(|ip| !ip.is_unspecified()).map(|ip| ip.to_string())));
        let factory = Arc::clone(&host);
        let service = StreamableHttpService::new(move || Ok(Session { host: Arc::clone(&factory), subscribed: watch::Sender::default(), relay: OnceLock::new() }), Arc::clone(&manager), config);
        let mut http = pin!(future::maybe_done(axum::serve(socket, axum::Router::new().route_service("/", service)).with_graceful_shutdown(host.stopping.clone().cancelled_owned()).into_future()));
        let mut lifetime = pin!(future::maybe_done(host.endpoint.serve(host.stopping.clone())));
        future::select(signals.next(), future::select(lifetime.as_mut(), http.as_mut())).await;
        tracing::info!("shutting down");
        let drained = async {
            host.requests.close();
            host.requests.wait().await;
            host.stopping.cancel();
            host.watches.close();
            future::join_all(manager.sessions.read().await.values().map(|session| session.close_standalone_sse_stream(None))).await;
            future::join(lifetime.as_mut(), host.watches.wait()).await;
        };
        future::select(pin!(future::join(drained, http.as_mut())), signals.next()).await;
        let ended = lifetime.as_mut().take_output().map_or(Err(Failed::Abandoned), |ended| ended.map_err(Failed::Endpoint));
        let served = http.as_mut().take_output().unwrap_or(Ok(())).map_err(|source| Failed::Io { operation: Operation::Serve, source });
        validated::Validated::from(ended).map2(served.into(), |(), ()| ()).ok()
    };
    Telemetry::run(telemetry, &command, [], EnvFilter::new("info,rmcp=warn,rmcp::service=error"), io::stderr, Identity::new(), |_| {
        let runtime = runtime::Runtime::new().map_err(|source| Failed::Io { operation: Operation::Runtime, source })?;
        let hosted = runtime.block_on(listening);
        runtime.shutdown_background();
        hosted
    })
}

pub mod server;
