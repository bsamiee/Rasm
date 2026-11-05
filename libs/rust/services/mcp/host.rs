use core::{net::SocketAddr, num::NonZero};
use std::{
    borrow::Cow,
    collections::BTreeMap,
    error::Report,
    process,
    sync::{Arc, LazyLock, OnceLock, Weak, nonpoison::Mutex},
};

use axum::extract::ConnectInfo;
use clap::{Args, CommandFactory, FromArgMatches};
use futures::{FutureExt as _, Stream, StreamExt as _, TryFutureExt as _, TryStreamExt as _, stream};
use http::{request::Parts, uri::Authority};
use itertools::Either;
use opentelemetry::{
    Context, InstrumentationScope, KeyValue, global,
    metrics::Histogram,
    trace::{Status, TraceContextExt as _},
};
use opentelemetry_http::HeaderExtractor;
use opentelemetry_semantic_conventions::{
    SCHEMA_URL,
    attribute::{ERROR_TYPE, EXCEPTION_MESSAGE, JSONRPC_REQUEST_ID, NETWORK_PEER_ADDRESS, NETWORK_PEER_PORT, NETWORK_PROTOCOL_NAME, NETWORK_TRANSPORT, RPC_RESPONSE_STATUS_CODE},
};
use rasm_command::Augmented;
use rasm_daemon::{Daemon, Lifetime};
use rasm_telemetry::{Defaults, Handle, Layer};
#[expect(deprecated, reason = "rmcp::ServerHandler::set_level requires its deprecated request type")]
use rmcp::model::SetLevelRequestParams;
use rmcp::{
    ErrorData, RoleServer, ServerHandler,
    model::{
        CallToolRequestMethod, CallToolRequestParams, CallToolResponse, CancelTaskMethod, CancelTaskParams, CancelledNotificationParam, CompleteRequestMethod, CompleteRequestParams, CompleteResult,
        ConstString as _, CustomNotification, CustomRequest, CustomResult, DiscoverRequestMethod, DiscoverResult, GetPromptRequestMethod, GetPromptRequestParams, GetPromptResponse, GetTaskMethod,
        GetTaskParams, GetTaskResult, Implementation, InitializeRequestParams, InitializeResult, InitializeResultMethod, ListPromptsRequestMethod, ListPromptsResult,
        ListResourceTemplatesRequestMethod, ListResourceTemplatesResult, ListResourcesRequestMethod, ListResourcesResult, ListToolsRequestMethod, ListToolsResult, PaginatedRequestParams,
        PingRequestMethod, ProgressNotificationParam, ProtocolVersion, ReadResourceRequestMethod, ReadResourceRequestParams, ReadResourceResponse, ResourcesCapability, ServerConfig,
        ServerNotification, SetLevelRequestMethod, SubscribeRequestMethod, SubscribeRequestParams, SubscriptionFilter, Tool, UnsubscribeRequestMethod, UnsubscribeRequestParams, UpdateTaskMethod,
        UpdateTaskParams,
    },
    service::{NotificationContext, RequestContext, SubscriptionContext},
    transport::{StreamableHttpServerConfig, StreamableHttpService, common::http_header::HEADER_SESSION_ID, streamable_http_server::session::local::LocalSessionManager},
};
use tokio::{
    sync::{broadcast, watch},
    time::Instant,
};
use tokio_stream::wrappers::{BroadcastStream, errors::BroadcastStreamRecvError};
use tokio_util::{
    sync::CancellationToken,
    task::{AbortOnDropHandle, TaskTracker},
};
use tracing::{
    Instrument as _,
    field::{self, Empty},
};
use tracing_opentelemetry::{OpenTelemetrySpanExt as _, SetParentError};

use crate::{Endpoint, McpError, Meta, Response, list_changes, resynced, versions};

// --- [TYPES] ---------------------------------------------------------------------------

type Leases = BTreeMap<String, Arc<AbortOnDropHandle<()>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const MCP_RESOURCE_URI: &str = "mcp.resource.uri";
const MCP_PROTOCOL_VERSION: &str = "mcp.protocol.version";
const TRANSPORT: &str = "tcp";
const PROTOCOL: &str = "http";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Host {
    pub identity: Implementation,
    pub filter: Handle,
    pub notifier: broadcast::Sender<ServerNotification>,
}

#[derive(Args)]
#[group(skip)]
#[command(next_help_heading = "MCP")]
struct Options {
    #[arg(long, env = "MCP_ALLOWED_HOSTS", value_name = "AUTHORITY", value_delimiter = ',', help = "Host header accepted beside loopback, listen host names and bound addresses")]
    allowed_host: Vec<Authority>,
    #[arg(long, env = "MCP_ALLOWED_ORIGINS", value_name = "ORIGIN", value_delimiter = ',', help = "Origin header accepted from a browser client, `:*` matching any port")]
    allowed_origin: Vec<String>,
    #[arg(long, env = "MCP_MAX_REQUEST_BODY", value_name = "BYTES", help = "Request body size accepted before a 413 response, rmcp's limit when unset")]
    max_request_body: Option<NonZero<usize>>,
}

enum Target {
    Tool(String),
    Prompt(String),
    Resource(String),
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Hosted<E> {
    endpoint: E,
    notifier: broadcast::Sender<ServerNotification>,
    stopping: CancellationToken,
    requests: TaskTracker,
    watches: TaskTracker,
    watchers: Mutex<BTreeMap<String, Weak<AbortOnDropHandle<()>>>>,
    config: StreamableHttpServerConfig,
}

impl<E> Hosted<E>
where E: Endpoint
{
    fn lease(&self, uri: &str) -> Result<Arc<AbortOnDropHandle<()>>, McpError> {
        self.watchers.with_mut(|watchers| {
            watchers.get(uri).and_then(Weak::upgrade).filter(|lease| !lease.is_finished()).map_or_else(
                || {
                    let watched =
                        self.stopping.clone().run_until_cancelled_owned(self.endpoint.watch(uri)?).map(drop).instrument(tracing::info_span!(parent: None, "watch", { MCP_RESOURCE_URI } = uri));
                    let lease = Arc::new(AbortOnDropHandle::new(self.watches.spawn(watched)));
                    watchers.retain(|_, held| held.strong_count() > 0);
                    watchers.insert(uri.to_owned(), Arc::downgrade(&lease));
                    Ok(lease)
                },
                Ok,
            )
        })
    }

    fn leased<'uri>(&self, uris: impl IntoIterator<Item = &'uri String>) -> Leases {
        uris.into_iter()
            .filter_map(|uri| self.lease(uri).map(|lease| (uri.clone(), lease)).inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "subscription refused")).ok())
            .collect()
    }
}

impl<E> Lifetime for Hosted<E>
where E: Endpoint
{
    type Error = E::Error;

    fn lifetime(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), E::Error>> + Send { self.endpoint.lifetime(stopping) }
}

impl<E> Daemon for Hosted<E>
where E: Endpoint
{
    fn router(self: Arc<Self>) -> axum::Router {
        let config = self.config.clone();
        axum::Router::new().route_service(
            "/",
            StreamableHttpService::new(
                move || Ok(Session { host: Arc::clone(&self), subscribed: watch::Sender::default(), accepted: Mutex::default(), relay: OnceLock::new(), opened: OnceLock::new() }),
                Arc::new(LocalSessionManager::default()),
                config,
            ),
        )
    }

    async fn drain(&self) -> Result<(), E::Error> {
        self.requests.close();
        self.requests.wait().await;
        self.stopping.cancel();
        self.watches.close();
        self.watches.wait().await;
        Ok(())
    }
}

struct Session<E> {
    host: Arc<Hosted<E>>,
    subscribed: watch::Sender<Leases>,
    accepted: Mutex<Leases>,
    relay: OnceLock<AbortOnDropHandle<()>>,
    opened: OnceLock<(Instant, ProtocolVersion)>,
}

impl<E> Session<E>
where E: Endpoint
{
    async fn traced<T>(
        &self,
        method: impl Into<Cow<'static, str>>,
        target: Option<Target>,
        context: &RequestContext<RoleServer>,
        call: impl Future<Output = Result<T, ErrorData>>,
    ) -> Result<T, ErrorData>
    where
        T: Response,
    {
        const MCP_METHOD_NAME: &str = "mcp.method.name";
        const MCP_SESSION_ID: &str = "mcp.session.id";
        const GEN_AI_TOOL_NAME: &str = "gen_ai.tool.name";
        const GEN_AI_PROMPT_NAME: &str = "gen_ai.prompt.name";
        const GEN_AI_OPERATION_NAME: &str = "gen_ai.operation.name";
        const MCP_SERVER_OPERATION_DURATION: &str = "mcp.server.operation.duration";
        const EXECUTE_TOOL: &str = "execute_tool";
        static OPERATION: LazyLock<Histogram<f64>> = LazyLock::new(|| duration(MCP_SERVER_OPERATION_DURATION));
        let _held = self.host.requests.token();
        let (method, version, parts) = (method.into(), context.protocol_version(), context.extensions.get::<Parts>());
        let peer = parts.and_then(|parts| parts.extensions.get::<ConnectInfo<SocketAddr>>()).map(|&ConnectInfo(peer)| peer);
        let (tool, prompt, uri) = match target {
            Some(Target::Tool(name)) => (Some(name), None, None),
            Some(Target::Prompt(name)) => (None, Some(name), None),
            Some(Target::Resource(uri)) => (None, None, Some(uri)),
            None => (None, None, None),
        };
        let span = tracing::info_span!(
            "request",
            otel.name = tool.as_ref().or(prompt.as_ref()).map_or_else(|| method.to_string(), |name| format!("{method} {name}")),
            otel.kind = "server",
            { MCP_METHOD_NAME } = &*method,
            { GEN_AI_OPERATION_NAME } = tool.is_some().then_some(EXECUTE_TOOL),
            { GEN_AI_TOOL_NAME } = tool.as_deref(),
            { GEN_AI_PROMPT_NAME } = prompt.as_deref(),
            { MCP_RESOURCE_URI } = uri.as_deref(),
            { MCP_PROTOCOL_VERSION } = version.as_ref().map(ProtocolVersion::as_str),
            { MCP_SESSION_ID } = parts.and_then(|parts| parts.headers.get(HEADER_SESSION_ID)).and_then(|value| value.to_str().ok()),
            { JSONRPC_REQUEST_ID } = %context.id,
            { NETWORK_TRANSPORT } = TRANSPORT,
            { NETWORK_PROTOCOL_NAME } = PROTOCOL,
            { NETWORK_PEER_ADDRESS } = peer.map(|peer| field::display(peer.ip())),
            { NETWORK_PEER_PORT } = peer.map(|peer| peer.port()),
            { ERROR_TYPE } = Empty,
            { RPC_RESPONSE_STATUS_CODE } = Empty,
        );
        let (carried, transported) = global::get_text_map_propagator(|propagator| {
            (
                propagator.extract_with_context(&Context::new(), &Meta(&context.meta.0)),
                parts.map_or_else(Context::new, |parts| propagator.extract_with_context(&Context::new(), &HeaderExtractor(&parts.headers))),
            )
        });
        let (parent, linked) = match (carried.has_active_span(), transported.has_active_span()) {
            (true, true) if carried.span().span_context() != transported.span().span_context() => (Some(carried), Some(transported)),
            (true, _) => (Some(carried), None),
            (false, true) => (Some(transported), None),
            (false, false) => (None, None),
        };
        if let Some(linked) = linked {
            span.add_link(linked.span().span_context().clone());
        }
        let (Ok(()) | Err(SetParentError::LayerNotFound | SetParentError::SpanDisabled | SetParentError::AlreadyStarted)) = parent.map_or(Ok(()), |parent| span.set_parent(parent));
        let (started, cancelled) = (Instant::now(), context.ct.clone());
        let result = match (self.host.requests.is_closed(), self.host.endpoint.ready()) {
            (true, _) => Err(McpError::Stopping.into()),
            (false, Err(unready)) => Err(unready.into()),
            (false, Ok(())) => call.instrument(span.clone()).await,
        };
        let elapsed = started.elapsed();
        let (error_type, failure) = match (&result, cancelled.is_cancelled()) {
            (Ok(_) | Err(_), true) => (Some(Cow::Borrowed("cancelled")), None),
            (Ok(response), false) if response.failed() => (Some(Cow::Borrowed("tool_error")), None),
            (Ok(_), false) => (None, None),
            (Err(error), false) => (Some(Cow::Owned(error.code.0.to_string())), Some(error)),
        };
        span.record(ERROR_TYPE, error_type.as_deref()).record(RPC_RESPONSE_STATUS_CODE, failure.map(|error| field::display(error.code.0)));
        if let Some(kind) = &error_type {
            span.set_status(Status::error(failure.map_or_else(|| kind.clone(), |error| error.message.clone())));
        }
        if let Some(error) = failure {
            tracing::warn!(parent: &span, elapsed = elapsed.as_secs_f64(), { EXCEPTION_MESSAGE } = %error.message, "request failed");
        } else {
            tracing::info!(parent: &span, elapsed = elapsed.as_secs_f64(), "request completed");
        }
        let attributes = [
            Some(KeyValue::new(MCP_METHOD_NAME, method)),
            tool.is_some().then(|| KeyValue::new(GEN_AI_OPERATION_NAME, EXECUTE_TOOL)),
            tool.map(|name| KeyValue::new(GEN_AI_TOOL_NAME, name)),
            prompt.map(|name| KeyValue::new(GEN_AI_PROMPT_NAME, name)),
            version.map(|version| KeyValue::new(MCP_PROTOCOL_VERSION, version.as_str().to_owned())),
            Some(KeyValue::new(NETWORK_TRANSPORT, TRANSPORT)),
            Some(KeyValue::new(NETWORK_PROTOCOL_NAME, PROTOCOL)),
            failure.map(|error| KeyValue::new(RPC_RESPONSE_STATUS_CODE, error.code.0.to_string())),
            error_type.map(|kind| KeyValue::new(ERROR_TYPE, kind)),
        ];
        OPERATION.record(elapsed.as_secs_f64(), &attributes.into_iter().flatten().collect::<Vec<_>>());
        result
    }

    fn notifications(&self, filter: SubscriptionFilter, held: watch::Receiver<Leases>) -> impl Stream<Item = ServerNotification> + Send + 'static {
        BroadcastStream::new(self.host.notifier.subscribe()).flat_map(move |received| {
            stream::iter(
                match received {
                    Ok(ServerNotification::ResourceUpdatedNotification(updated)) if held.borrow().contains_key(&updated.params.uri) => Either::Left(Some(updated.into())),
                    Ok(notification @ ServerNotification::ToolListChangedNotification(_)) if filter.tools_list_changed == Some(true) => Either::Left(Some(notification)),
                    Ok(notification @ ServerNotification::PromptListChangedNotification(_)) if filter.prompts_list_changed == Some(true) => Either::Left(Some(notification)),
                    Ok(notification @ ServerNotification::ResourceListChangedNotification(_)) if filter.resources_list_changed == Some(true) => Either::Left(Some(notification)),
                    Ok(_) => Either::Left(None),
                    Err(BroadcastStreamRecvError::Lagged(_)) => Either::Right(resynced(&filter, Vec::from_iter(held.borrow().keys().cloned()))),
                }
                .map_left(Option::into_iter),
            )
        })
    }
}

impl<E> Drop for Session<E> {
    fn drop(&mut self) {
        const MCP_SERVER_SESSION_DURATION: &str = "mcp.server.session.duration";
        static SESSION: LazyLock<Histogram<f64>> = LazyLock::new(|| duration(MCP_SERVER_SESSION_DURATION));
        if let Some((opened, version)) = self.opened.take() {
            SESSION.record(
                opened.elapsed().as_secs_f64(),
                &[KeyValue::new(MCP_PROTOCOL_VERSION, version.as_str().to_owned()), KeyValue::new(NETWORK_TRANSPORT, TRANSPORT), KeyValue::new(NETWORK_PROTOCOL_NAME, PROTOCOL)],
            );
        }
    }
}

impl<E> ServerHandler for Session<E>
where E: Endpoint
{
    async fn ping(&self, context: RequestContext<RoleServer>) -> Result<(), ErrorData> { self.traced(PingRequestMethod::VALUE, None, &context, self.host.endpoint.ping(context.clone())).await }

    async fn initialize(&self, request: InitializeRequestParams, context: RequestContext<RoleServer>) -> Result<InitializeResult, ErrorData> {
        let negotiated = async {
            context.peer.set_peer_info(request.clone());
            let negotiated = self.negotiate_initialize(&request)?;
            let (Ok(()) | Err((_, _))) = self.opened.set((Instant::now(), negotiated.protocol_version.clone()));
            Ok(negotiated)
        };
        self.traced(InitializeResultMethod::VALUE, None, &context, negotiated).await
    }

    async fn discover(&self, context: RequestContext<RoleServer>) -> Result<DiscoverResult, ErrorData> {
        self.traced(DiscoverRequestMethod::VALUE, None, &context, async { Ok(DiscoverResult::from_server_info(versions().to_vec(), self.get_info())) }).await
    }

    async fn complete(&self, request: CompleteRequestParams, context: RequestContext<RoleServer>) -> Result<CompleteResult, ErrorData> {
        self.traced(CompleteRequestMethod::VALUE, None, &context, self.host.endpoint.complete(request, context.clone())).await
    }

    #[expect(deprecated, reason = "rmcp::ServerHandler::set_level requires its deprecated request type")]
    async fn set_level(&self, request: SetLevelRequestParams, context: RequestContext<RoleServer>) -> Result<(), ErrorData> {
        self.traced(SetLevelRequestMethod::VALUE, None, &context, self.host.endpoint.set_level(request, context.clone())).await
    }

    async fn get_prompt(&self, request: GetPromptRequestParams, context: RequestContext<RoleServer>) -> Result<GetPromptResponse, ErrorData> {
        self.traced(GetPromptRequestMethod::VALUE, Some(Target::Prompt(request.name.clone())), &context, self.host.endpoint.get_prompt(request, context.clone())).await
    }

    async fn list_prompts(&self, request: Option<PaginatedRequestParams>, context: RequestContext<RoleServer>) -> Result<ListPromptsResult, ErrorData> {
        self.traced(ListPromptsRequestMethod::VALUE, None, &context, self.host.endpoint.list_prompts(request, context.clone())).await
    }

    async fn list_resources(&self, request: Option<PaginatedRequestParams>, context: RequestContext<RoleServer>) -> Result<ListResourcesResult, ErrorData> {
        self.traced(ListResourcesRequestMethod::VALUE, None, &context, self.host.endpoint.list_resources(request, context.clone())).await
    }

    async fn list_resource_templates(&self, request: Option<PaginatedRequestParams>, context: RequestContext<RoleServer>) -> Result<ListResourceTemplatesResult, ErrorData> {
        self.traced(ListResourceTemplatesRequestMethod::VALUE, None, &context, self.host.endpoint.list_resource_templates(request, context.clone())).await
    }

    async fn read_resource(&self, request: ReadResourceRequestParams, context: RequestContext<RoleServer>) -> Result<ReadResourceResponse, ErrorData> {
        self.traced(ReadResourceRequestMethod::VALUE, Some(Target::Resource(request.uri.clone())), &context, self.host.endpoint.read_resource(request, context.clone())).await
    }

    async fn subscribe(&self, request: SubscribeRequestParams, context: RequestContext<RoleServer>) -> Result<(), ErrorData> {
        let uri = request.uri.clone();
        let held = async move {
            match self.get_info().capabilities.resources {
                Some(ResourcesCapability { subscribe: Some(true), .. }) => {
                    let lease = self.host.lease(&request.uri)?;
                    self.subscribed.send_if_modified(|uris| uris.insert(request.uri, lease).is_none());
                    Ok(())
                }
                Some(_) | None => Err(ErrorData::method_not_found::<SubscribeRequestMethod>()),
            }
        };
        self.traced(SubscribeRequestMethod::VALUE, Some(Target::Resource(uri)), &context, held).await
    }

    async fn unsubscribe(&self, request: UnsubscribeRequestParams, context: RequestContext<RoleServer>) -> Result<(), ErrorData> {
        let uri = request.uri.clone();
        let released = async move {
            self.subscribed.send_if_modified(|uris| uris.remove(&request.uri).is_some());
            Ok(())
        };
        self.traced(UnsubscribeRequestMethod::VALUE, Some(Target::Resource(uri)), &context, released).await
    }

    async fn call_tool(&self, request: CallToolRequestParams, context: RequestContext<RoleServer>) -> Result<CallToolResponse, ErrorData> {
        self.traced(CallToolRequestMethod::VALUE, Some(Target::Tool(request.name.to_string())), &context, self.host.endpoint.call_tool(request, context.clone())).await
    }

    async fn list_tools(&self, request: Option<PaginatedRequestParams>, context: RequestContext<RoleServer>) -> Result<ListToolsResult, ErrorData> {
        self.traced(ListToolsRequestMethod::VALUE, None, &context, self.host.endpoint.list_tools(request, context.clone())).await
    }

    async fn get_task(&self, request: GetTaskParams, context: RequestContext<RoleServer>) -> Result<GetTaskResult, ErrorData> {
        self.traced(GetTaskMethod::VALUE, None, &context, self.host.endpoint.get_task(request, context.clone())).await
    }

    async fn update_task(&self, request: UpdateTaskParams, context: RequestContext<RoleServer>) -> Result<(), ErrorData> {
        self.traced(UpdateTaskMethod::VALUE, None, &context, self.host.endpoint.update_task(request, context.clone())).await
    }

    async fn cancel_task(&self, request: CancelTaskParams, context: RequestContext<RoleServer>) -> Result<(), ErrorData> {
        self.traced(CancelTaskMethod::VALUE, None, &context, self.host.endpoint.cancel_task(request, context.clone())).await
    }

    async fn on_custom_request(&self, request: CustomRequest, context: RequestContext<RoleServer>) -> Result<CustomResult, ErrorData> {
        self.traced(request.method.clone(), None, &context, self.host.endpoint.on_custom_request(request, context.clone())).await
    }

    async fn on_cancelled(&self, notification: CancelledNotificationParam, context: NotificationContext<RoleServer>) { self.host.endpoint.on_cancelled(notification, context).await }

    async fn on_progress(&self, notification: ProgressNotificationParam, context: NotificationContext<RoleServer>) { self.host.endpoint.on_progress(notification, context).await }

    async fn on_roots_list_changed(&self, context: NotificationContext<RoleServer>) { self.host.endpoint.on_roots_list_changed(context).await }

    async fn on_custom_notification(&self, notification: CustomNotification, context: NotificationContext<RoleServer>) { self.host.endpoint.on_custom_notification(notification, context).await }

    fn get_tool(&self, name: &str) -> Option<Tool> { self.host.endpoint.get_tool(name) }

    fn get_info(&self) -> ServerConfig { self.host.endpoint.get_info() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { Cow::Borrowed(versions()) }

    fn accepted_subscription_filter(&self, requested: &SubscriptionFilter) -> Option<SubscriptionFilter> {
        let mut accepted = requested.supported_by(&self.get_info().capabilities);
        accepted.resource_subscriptions = accepted.resource_subscriptions.take().map(|uris| {
            let leased = self.host.leased(&uris);
            let kept = Vec::from_iter(leased.keys().cloned());
            self.accepted.with_mut(|held| held.extend(leased));
            kept
        });
        Some(accepted)
    }

    async fn on_initialized(&self, context: NotificationContext<RoleServer>) {
        let (notifications, peer) = (self.notifications(list_changes(&self.get_info().capabilities), self.subscribed.subscribe()), context.peer.clone());
        let relayed = async move { notifications.map(Ok).try_for_each(|notification| peer.send_notification(notification)).await };
        self.relay.get_or_init(|| AbortOnDropHandle::new(tokio::spawn(relayed.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "session closed")))));
        self.host.endpoint.on_initialized(context).await;
    }

    async fn listen(&self, context: SubscriptionContext) -> Result<(), ErrorData> {
        let held = self.host.leased(context.accepted().resource_subscriptions.iter().flatten());
        self.accepted.with_mut(|accepted| accepted.retain(|uri, _| !held.contains_key(uri)));
        let notifications = self.notifications(context.accepted().clone(), watch::channel(held).1).take_until(self.host.stopping.cancelled()).take_until(context.cancelled());
        let sent = notifications.map(Ok).try_for_each(|notification| context.sink().send(notification));
        sent.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "subscription closed")).await;
        Ok(())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn duration(name: &'static str) -> Histogram<f64> {
    const BOUNDARIES: [f64; 14] = [0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1.0, 2.0, 5.0, 10.0, 30.0, 60.0, 120.0, 300.0];
    global::meter_with_scope(InstrumentationScope::builder(env!("CARGO_PKG_NAME")).with_schema_url(SCHEMA_URL).build())
        .f64_histogram(name)
        .with_unit("s")
        .with_boundaries(BOUNDARIES.to_vec())
        .build()
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<P, L, E>(defaults: Defaults, layer: L, build: impl AsyncFnOnce(P, Host) -> Result<E, E::Error>) -> process::ExitCode
where
    P: CommandFactory + FromArgMatches,
    L: Layer,
    E: Endpoint,
{
    const CAPACITY: usize = 64;
    let program = P::command();
    let identity = program.get_about().into_iter().map(ToString::to_string).fold(Implementation::new(program.get_name(), program.get_version().unwrap_or_default()), Implementation::with_description);
    rasm_daemon::run(
        Defaults { directives: format!("rmcp=warn,rmcp::service=error,{}", defaults.directives).into(), ..defaults },
        layer,
        async |Augmented { options: Options { allowed_host, allowed_origin, max_request_body }, args }, addresses, filter, stopping| {
            let notifier = broadcast::Sender::new(CAPACITY);
            let endpoint = build(args, Host { identity, filter, notifier: notifier.clone() }).await?;
            let mut config = max_request_body
                .map(NonZero::get)
                .into_iter()
                .fold(StreamableHttpServerConfig::default(), StreamableHttpServerConfig::with_max_request_body_bytes)
                .with_json_response(true)
                .with_allowed_origins(allowed_origin)
                .enforce_origin_validation()
                .with_cancellation_token(stopping.child_token());
            let listened =
                addresses.iter().flat_map(|(authority, bound)| bound.iter().map(SocketAddr::ip).filter(|ip| !ip.is_unspecified()).flat_map(|ip| [authority.host().to_owned(), ip.to_string()]));
            config.allowed_hosts.extend(allowed_host.iter().map(Authority::to_string).chain(listened));
            Ok(Hosted { endpoint, notifier, stopping, requests: TaskTracker::new(), watches: TaskTracker::new(), watchers: Mutex::default(), config })
        },
    )
}
