#![feature(associated_type_defaults, error_reporter, impl_restriction, iterator_try_collect, lock_value_accessors, nonpoison_mutex, sync_nonpoison, trait_alias)]

use core::{error::Error, fmt::Display, future::ready, mem::discriminant, net::SocketAddr, pin::pin, str::Utf8Error, time::Duration};
use std::{
    backtrace::Backtrace,
    borrow::Cow,
    collections::BTreeMap,
    error::Report,
    io, panic, process,
    sync::{Arc, LazyLock, OnceLock, Weak, nonpoison::Mutex},
};

use axum::http::{request::Parts, uri::Authority};
use clap::{Args, FromArgMatches as _, Parser};
use derive_where::derive_where;
use futures::{FutureExt as _, Stream, StreamExt as _, TryFutureExt as _, TryStreamExt as _, future, stream};
use jsonschema::{ValidationError, paths::Location, validator_for};
use nonempty_collections::NEVec;
use opentelemetry::{KeyValue, global, metrics::Histogram, propagation::Extractor, trace::TracerProvider as _};
use opentelemetry_appender_tracing::layer::OpenTelemetryTracingBridge;
use opentelemetry_otlp::{ExporterBuildError, LogExporter, MetricExporter, SpanExporter};
use opentelemetry_sdk::{error::OTelSdkError, logs::SdkLoggerProvider, metrics::SdkMeterProvider, propagation::TraceContextPropagator, trace::SdkTracerProvider};
use opentelemetry_semantic_conventions::attribute::{ERROR_TYPE, EXCEPTION_MESSAGE, EXCEPTION_STACKTRACE, JSONRPC_REQUEST_ID, RPC_RESPONSE_STATUS_CODE, SERVICE_NAME, SERVICE_VERSION};
use rmcp::{
    ErrorData, Json, RoleServer, ServerHandler, ServiceError,
    handler::server::{
        common::{AsRequestContext, FromContextPart},
        prompt::PromptContext,
        router::prompt::PromptRouter,
        tool::{IntoCallToolResult, ToolCallContext, ToolRoute, ToolRouter},
    },
    model::{
        CallToolRequest, CallToolRequestParams, CallToolResponse, CallToolResult, CancelTaskParams, CancelTaskRequest, ClientRequest, CompleteRequest, CompleteRequestParams, CompleteResult,
        ContentBlock, CustomRequest, CustomResult, ElicitRequest, ElicitRequestParams, ElicitationAction, ElicitationSchema, ErrorCode, GetPromptRequest, GetPromptRequestParams, GetPromptResponse,
        GetTaskParams, GetTaskRequest, GetTaskResult, Implementation, InputRequest, InputRequiredResult, InputResponses, JsonObject, ListPromptsRequest, ListPromptsResult,
        ListResourceTemplatesRequest, ListResourceTemplatesResult, ListResourcesRequest, ListResourcesResult, ListToolsRequest, ListToolsResult, MetaObject, PaginatedRequestParams,
        ProgressNotificationParam, PromptListChangedNotification, ProtocolVersion, ReadResourceRequest, ReadResourceRequestParams, ReadResourceResponse, ReadResourceResult, Request, Resource,
        ResourceContents, ResourceListChangedNotification, ResourceTemplate, ResourceUpdatedNotification, ResourceUpdatedNotificationParam, ResourcesCapability, ServerCapabilities, ServerConfig,
        ServerNotification, ServerResult, SubscribeRequestMethod, SubscribeRequestParams, SubscriptionFilter, ToolListChangedNotification, UnsubscribeRequestMethod, UnsubscribeRequestParams,
        UpdateTaskParams, UpdateTaskRequest,
    },
    service::{DEFAULT_SUBSCRIPTION_CHANNEL_CAPACITY, NotificationContext, RequestContext, SubscriptionContext},
    transport::{StreamableHttpServerConfig, StreamableHttpService, common::http_header::HEADER_SESSION_ID, streamable_http_server::session::local::LocalSessionManager},
};
use serde::{Serialize, de::DeserializeOwned};
use serde_json::Value;
use serde_urlencoded::{Deserializer, de};
#[cfg(unix)]
use tokio::signal::unix::{SignalKind, signal};
#[cfg(windows)]
use tokio::signal::windows;
use tokio::{
    net, runtime,
    sync::{broadcast, mpsc, oneshot, watch},
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
use tracing_subscriber::{EnvFilter, Layer as _, fmt::time::SystemTime, layer::SubscriberExt as _, util::SubscriberInitExt as _};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait App: Clone + Send + Sync + 'static {
    type Error: Error + Send + Sync + 'static + From<Invalid> = Invalid;

    fn serve(self, _: Changes<Self>, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send { stopping.cancelled_owned().map(Ok) }
}

pub trait Endpoint: Send + Sync + 'static {
    type Error: Error + Send + Sync + 'static;

    fn info(&self) -> &ServerConfig;

    fn handle(&self, request: ClientRequest, context: RequestContext<RoleServer>) -> impl Future<Output = Result<ServerResult, ErrorData>> + Send;

    fn subscribe(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, Unserved>;

    fn serve(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send;

    fn listed(&self) -> SubscriptionFilter { SubscriptionFilter::builder().tools_list_changed().prompts_list_changed().resources_list_changed().build().supported_by(&self.info().capabilities) }
}

pub trait IntoResourceContents {
    fn into_resource_contents(self, uri: String, mime: Option<String>) -> Result<Vec<ResourceContents>, ErrorData>;
}

impl IntoResourceContents for Vec<ResourceContents> {
    fn into_resource_contents(self, _: String, _: Option<String>) -> Result<Vec<ResourceContents>, ErrorData> { Ok(self) }
}

impl IntoResourceContents for String {
    fn into_resource_contents(self, uri: String, mime: Option<String>) -> Result<Vec<ResourceContents>, ErrorData> {
        Ok(vec![mime.into_iter().fold(ResourceContents::text(self, uri), ResourceContents::with_mime_type)])
    }
}

impl<T> IntoResourceContents for Json<T>
where T: Serialize
{
    fn into_resource_contents(self, uri: String, mime: Option<String>) -> Result<Vec<ResourceContents>, ErrorData> {
        serde_json::to_string(&self.0).map_err(internal)?.into_resource_contents(uri, mime.or_else(|| Some(JSON.to_owned())))
    }
}

impl<T, E> IntoResourceContents for Result<T, E>
where
    T: IntoResourceContents,
    ErrorData: From<E>,
{
    fn into_resource_contents(self, uri: String, mime: Option<String>) -> Result<Vec<ResourceContents>, ErrorData> { self?.into_resource_contents(uri, mime) }
}

pub impl(crate) trait Host {
    fn entry(self) -> (&'static str, Value);
}

trait Routed<A, T> = Fn(A, String, String) -> Result<T, Unserved> + Send + Sync;

trait List<A> = Fn(A) -> future::BoxFuture<'static, Result<Vec<Resource>, ErrorData>> + Send + Sync;

#[derive(Debug, derive_more::Display)]
enum Stage {
    #[display("cannot start the async runtime")]
    Runtime,
    #[display("cannot bind {_0}")]
    Bind(SocketAddr),
    #[display("cannot listen for shutdown signals")]
    Signal,
    #[display("HTTP server failed")]
    Serve,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const JSON: &str = "application/json";

pub const PROTOCOL_VERSIONS: &[ProtocolVersion] = &[ProtocolVersion::V_2025_06_18, ProtocolVersion::V_2025_11_25, ProtocolVersion::V_2026_07_28];

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args)]
struct Options {
    #[arg(long, env = "MCP_LISTEN", help = "Socket address the Streamable HTTP endpoint binds")]
    listen: SocketAddr,
    #[arg(long = "allowed-host", env = "MCP_ALLOWED_HOSTS", value_name = "AUTHORITY", value_delimiter = ',', help = "Host authority the endpoint answers beside loopback names and the bound address")]
    hosts: Vec<Authority>,
    #[arg(long, env = EnvFilter::DEFAULT_ENV, value_parser = str::parse::<EnvFilter>, default_value = "info,rmcp=warn,rmcp::service=error,opentelemetry=warn", help = "Tracing filter directives")]
    log: EnvFilter,
    #[arg(long, env = "MCP_OTLP", help = "Export traces, metrics, and logs over OTLP/HTTP to the endpoints the OTEL_EXPORTER_OTLP_* variables name")]
    otlp: bool,
}

#[derive(Clone, Copy, Debug)]
pub enum Claude {
    AlwaysLoad(bool),
    MaxResultSizeChars(bounded_integer::BoundedU32<1, 500_000>),
    RequiresUserInteraction,
}

impl Host for Claude {
    fn entry(self) -> (&'static str, Value) {
        match self {
            Self::AlwaysLoad(load) => ("anthropic/alwaysLoad", Value::Bool(load)),
            Self::MaxResultSizeChars(chars) => ("anthropic/maxResultSizeChars", chars.get().into()),
            Self::RequiresUserInteraction => ("anthropic/requiresUserInteraction", Value::Bool(true)),
        }
    }
}

#[derive(Debug, derive_more::From)]
pub struct Failure<E>(pub E);

impl<E> IntoCallToolResult for Failure<E>
where E: Serialize + Display
{
    fn into_call_tool_result(self) -> Result<CallToolResponse, ErrorData> {
        #[derive(Serialize)]
        struct Rendered<'error, E> {
            #[serde(flatten)]
            error: &'error E,
            message: String,
        }
        ContentBlock::json(Rendered { error: &self.0, message: self.0.to_string() }).map(|block| CallToolResult::error(vec![block]).into())
    }
}

impl<E> From<Failure<E>> for ErrorData
where E: Serialize + Display
{
    fn from(value: Failure<E>) -> Self { serde_json::to_value(&value.0).map_or_else(internal, |data| Self::internal_error(value.0.to_string(), Some(data))) }
}

#[derive_where(Debug, Default)]
pub struct ResourceRouter<A> {
    #[derive_where(skip(Debug))]
    routes: matchit::Router<Route<A>>,
    #[derive_where(skip(Debug))]
    lists: Vec<Box<dyn List<A>>>,
    templates: Vec<ResourceTemplate>,
}

impl<A> ResourceRouter<A>
where A: 'static
{
    #[must_use]
    pub fn listed<L, Q, E>(mut self, list: L) -> Self
    where
        L: Fn(A) -> Q + Send + Sync + 'static,
        Q: Future<Output = Result<Vec<Resource>, E>> + Send + 'static,
        E: 'static,
        ErrorData: From<E>,
    {
        self.lists.push(Box::new(move |app| list(app).map_err(ErrorData::from).boxed()));
        self
    }

    pub fn resource<F, R, W, S>(self, resource: Resource, read: F, watch: W) -> Result<Self, Invalid>
    where
        F: Fn(A) -> R + Send + Sync + 'static,
        R: Future<Output: IntoResourceContents> + Send + 'static,
        W: Fn(A, ()) -> S + Send + Sync + 'static,
        S: Stream<Item = ()> + Send + 'static,
    {
        let (uri, mime) = (resource.uri.clone(), resource.mime_type.clone());
        self.route(uri, mime, move |app, ()| read(app), watch).map(|router| router.listed(move |_| ready(Ok::<_, ErrorData>(vec![resource.clone()]))))
    }

    pub fn template<P, F, R, W, S>(mut self, template: ResourceTemplate, read: F, watch: W) -> Result<Self, Invalid>
    where
        P: DeserializeOwned + 'static,
        F: Fn(A, P) -> R + Send + Sync + 'static,
        R: Future<Output: IntoResourceContents> + Send + 'static,
        W: Fn(A, P) -> S + Send + Sync + 'static,
        S: Stream<Item = ()> + Send + 'static,
    {
        let (pattern, mime) = (template.uri_template.clone(), template.mime_type.clone());
        self.templates.push(template);
        self.route(pattern, mime, read, watch)
    }

    pub fn executions(self, resource: Resource, changes: &Changes<A>) -> Result<Self, Invalid> {
        let (rows, executed) = (Arc::clone(&changes.executions), changes.executed.clone());
        let read = move |_| ready(serde_json::to_string(&rows.lock().values().collect::<Vec<_>>()).map_err(internal));
        self.resource(resource.with_mime_type(JSON), read, move |_, ()| WatchStream::from_changes(executed.subscribe()))
    }

    fn route<P, F, R, W, S>(mut self, pattern: String, mime: Option<String>, read: F, watch: W) -> Result<Self, Invalid>
    where
        P: DeserializeOwned + 'static,
        F: Fn(A, P) -> R + Send + Sync + 'static,
        R: Future<Output: IntoResourceContents> + Send + 'static,
        W: Fn(A, P) -> S + Send + Sync + 'static,
        S: Stream<Item = ()> + Send + 'static,
    {
        let decoded = |uri: &str, variables: &str| {
            serde_path_to_error::deserialize(Deserializer::new(form_urlencoded::parse(variables.as_bytes()))).map_err(|source| Unserved::Malformed { uri: uri.to_owned(), source })
        };
        let read = Box::new(move |app, uri: String, variables: String| {
            let mime = mime.clone();
            decoded(&uri, &variables).map(|variables| read(app, variables).map(move |contents| contents.into_resource_contents(uri, mime)).boxed())
        });
        let watch = Box::new(move |app, uri: String, variables: String| decoded(&uri, &variables).map(|variables| watch(app, variables).boxed()));
        self.routes.insert(&pattern, Route { read, watch }).map_err(|source| Invalid::Route { pattern, source })?;
        Ok(self)
    }

    fn routed(&self, uri: &str) -> Result<(&Route<A>, String), Unserved> {
        let matchit::Match { value, params } = self.routes.at(uri).map_err(|matchit::MatchError::NotFound| Unserved::Unrouted { uri: uri.to_owned() })?;
        params
            .iter()
            .map(|(name, variable)| percent_encoding::percent_decode_str(variable).decode_utf8().map(|decoded| (name, decoded)))
            .try_collect::<Vec<_>>()
            .map(|variables| (value, form_urlencoded::Serializer::new(String::new()).extend_pairs(variables).finish()))
            .map_err(|source| Unserved::Unencoded { uri: uri.to_owned(), source })
    }
}

struct Route<A> {
    read: Box<dyn Routed<A, future::BoxFuture<'static, Result<Vec<ResourceContents>, ErrorData>>>>,
    watch: Box<dyn Routed<A, stream::BoxStream<'static, ()>>>,
}

struct Running {
    call: future::BoxFuture<'static, Result<CallToolResponse, ErrorData>>,
    questions: mpsc::UnboundedReceiver<(InputRequiredResult, oneshot::Sender<InputResponses>)>,
    progress: watch::Receiver<(f64, Option<f64>, Option<String>)>,
    execution: Execution,
    destructive: bool,
}

#[derive(Debug, Serialize)]
struct Execution {
    tool: Cow<'static, str>,
    arguments: Option<JsonObject>,
    outcome: Option<Result<ServerResult, ErrorData>>,
    #[serde(skip)]
    since: Instant,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum Invalid {
    #[error("resource route {pattern} cannot be served")]
    Route { pattern: String, source: matchit::InsertError },
    #[error("tool {tool} input schema does not compile")]
    Schema { tool: Cow<'static, str>, source: ValidationError<'static> },
}

#[derive(Debug, thiserror::Error)]
pub enum Unserved {
    #[error("{uri} names no resource, list resources and resource templates for the served uris")]
    Unrouted { uri: String },
    #[error("{uri} holds a variable that is not percent-encoded UTF-8, encode each variable as UTF-8")]
    Unencoded { uri: String, source: Utf8Error },
    #[error("{uri} matches the template but not its variables, read resources/templates/list for the variable forms")]
    Malformed { uri: String, source: serde_path_to_error::Error<de::Error> },
    #[error("{name} names no tool, list tools for the served names")]
    Unlisted { name: Cow<'static, str> },
    #[error("requestState names no paused call of this tool with these arguments, call again without it")]
    Unparked,
    #[error("the server is shutting down, call again once it restarts")]
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

#[derive(Debug, Serialize, thiserror::Error)]
#[serde(tag = "_tag")]
pub enum Unanswered {
    #[error("the user declined to answer, continue without the answer")]
    Declined,
    #[error("the user dismissed the question, call again to ask again")]
    Dismissed,
    #[error("the user answered without content, call again to ask again")]
    Empty,
    #[error("the client cannot ask the user, ask in the conversation and pass the answer as arguments")]
    Unsupported,
    #[error("the answer breaks the requested form, call again to ask again")]
    Malformed {
        data: Value,
        #[serde(skip)]
        source: serde_path_to_error::Error<serde_json::Error>,
    },
    #[error("the requested form has no object schema, the tool cannot ask this question")]
    Unaskable {
        #[serde(skip)]
        source: serde_json::Error,
    },
    #[error("the call ended before the user answered, call again")]
    Abandoned,
    #[error("the question did not reach the user, call again")]
    Unreached {
        #[serde(skip)]
        source: ServiceError,
    },
}

#[derive(Debug, thiserror::Error)]
enum Failed<X> {
    #[error("cannot build the OTLP exporters")]
    Export(#[source] ExporterBuildError),
    #[error("cannot flush telemetry")]
    Flush(#[source] OTelSdkError),
    #[error("{stage}")]
    Io { stage: Stage, source: io::Error },
    #[error("endpoint failed")]
    Endpoint(#[source] X),
    #[error("a second shutdown signal exited before the endpoint finished")]
    Abandoned,
}

impl<X> From<Failed<X>> for NEVec<Failed<X>> {
    fn from(value: Failed<X>) -> Self { Self::new(value) }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug)]
pub struct Notifier<N = ServerNotification>(broadcast::Sender<N>);

impl<N> Default for Notifier<N> {
    fn default() -> Self { Self(broadcast::Sender::new(DEFAULT_SUBSCRIPTION_CHANNEL_CAPACITY)) }
}

impl<N> Notifier<N>
where N: Clone + Send + 'static
{
    pub fn notify<M>(&self, notification: M)
    where M: Into<N> {
        let (Ok(_) | Err(broadcast::error::SendError(_))) = self.0.send(notification.into());
    }

    #[must_use]
    pub fn subscribe(&self) -> BroadcastStream<N> { BroadcastStream::new(self.0.subscribe()) }
}

impl Notifier {
    pub fn resync(&self, filter: &SubscriptionFilter) { resynced(filter, filter.resource_subscriptions.iter().flatten().cloned()).for_each(|notification| self.notify(notification)); }
}

#[derive_where(Clone, Debug)]
pub struct Changes<A> {
    notifier: Notifier,
    executions: Arc<Mutex<BTreeMap<usize, Execution>>>,
    executed: watch::Sender<()>,
    #[derive_where(skip(Debug))]
    tools: Arc<arc_swap::ArcSwap<ToolRouter<A>>>,
    #[derive_where(skip(Debug))]
    prompts: Arc<arc_swap::ArcSwap<PromptRouter<A>>>,
    #[derive_where(skip(Debug))]
    resources: Arc<arc_swap::ArcSwap<ResourceRouter<A>>>,
}

impl<A> Changes<A>
where A: App
{
    pub fn tools(&self, router: ToolRouter<A>) -> Result<(), A::Error> {
        #[derive(Debug, Serialize, thiserror::Error)]
        #[serde(tag = "_tag")]
        #[error("arguments break the tool's input schema, correct each listed violation at its path, then call again")]
        struct InvalidArguments {
            violations: NEVec<(Location, String)>,
        }
        let validated = router
            .into_iter()
            .map(|ToolRoute { call, attr, .. }| {
                validator_for(&Value::Object(JsonObject::clone(&attr.input_schema))).map_err(|source| Invalid::Schema { tool: attr.name.clone(), source }).map(|validator| {
                    ToolRoute::new_dyn(attr, move |context: ToolCallContext<'_, A>| {
                        let violations =
                            validator.iter_errors(&Value::Object(context.arguments.clone().unwrap_or_default())).map(|violation| (violation.instance_path().clone(), violation.to_string())).collect();
                        NEVec::try_from_vec(violations).map_or_else(|| call(context), |violations| ready(Failure(InvalidArguments { violations }).into_call_tool_result()).boxed())
                    })
                })
            })
            .try_fold(ToolRouter::new(), |router, route| route.map(|route| router.with_route(route)))?;
        self.tools.store(Arc::new(validated));
        self.notifier.notify(ToolListChangedNotification::default());
        Ok(())
    }

    pub fn prompts(&self, router: PromptRouter<A>) {
        self.prompts.store(Arc::new(router));
        self.notifier.notify(PromptListChangedNotification::default());
    }

    pub fn resources(&self, router: ResourceRouter<A>) {
        self.resources.store(Arc::new(router));
        self.listed();
    }

    pub fn listed(&self) { self.notifier.notify(ResourceListChangedNotification::default()); }

    pub fn updated<U>(&self, uri: U)
    where U: Into<String> {
        self.notifier.notify(ResourceUpdatedNotification::new(ResourceUpdatedNotificationParam::new(uri.into())));
    }
}

#[derive(Clone, Debug, Default)]
pub struct Progress(watch::Sender<(f64, Option<f64>, Option<String>)>);

impl Progress {
    pub fn relay(&self, context: &RequestContext<RoleServer>) -> impl Future<Output = ()> + Send + 'static { relayed(self.0.subscribe(), context) }

    pub fn stage<M>(&self, message: M)
    where M: Into<String> {
        self.0.send_modify(|(progress, _, last)| (*progress, *last) = (*progress + 1.0_f64, Some(message.into())));
    }

    pub fn report(&self, progress: f64, total: Option<f64>, message: Option<String>) { self.0.send_if_modified(|last| (progress > last.0).then(|| *last = (progress, total, message)).is_some()); }
}

impl<C> FromContextPart<C> for Progress
where C: AsRequestContext
{
    fn from_context_part(context: &mut C) -> Result<Self, ErrorData> {
        context.as_request_context().extensions.get::<Self>().cloned().ok_or_else(|| ErrorData::internal_error("progress reaches a tool of a rasm_mcp::Server alone", None))
    }
}

#[derive(Clone, Debug)]
pub struct Elicitation {
    context: RequestContext<RoleServer>,
    asked: mpsc::UnboundedSender<(InputRequiredResult, oneshot::Sender<InputResponses>)>,
}

impl Elicitation {
    pub async fn elicit<T>(&self, message: String) -> Result<T, Unanswered>
    where T: schemars::JsonSchema + DeserializeOwned {
        if !self.context.client_capabilities().and_then(|capabilities| capabilities.elicitation).is_some_and(|modes| modes.form.is_some() || modes.url.is_none()) {
            return Err(Unanswered::Unsupported);
        }
        let requested_schema = ElicitationSchema::from_type::<T>().map_err(|source| Unanswered::Unaskable { source })?;
        let params = ElicitRequestParams::FormElicitationParams { meta: None, message, requested_schema };
        let result = if self.context.protocol_version().is_none_or(|version| version.has_initialize()) {
            self.context.peer.create_elicitation(params).await.map_err(|source| Unanswered::Unreached { source })?
        } else {
            let key = uuid::Uuid::new_v4().to_string();
            let required = InputRequiredResult::from_input_requests(BTreeMap::from([(key.clone(), InputRequest::Elicitation(ElicitRequest::new(params)))]));
            let answer = loop {
                let (responder, answered) = oneshot::channel();
                self.asked.send((required.clone(), responder)).map_err(|mpsc::error::SendError(_)| Unanswered::Abandoned)?;
                let Some(answer) = answered.await.map_err(|oneshot::error::RecvError { .. }| Unanswered::Abandoned)?.remove(&key) else { continue };
                break answer;
            };
            serde_path_to_error::deserialize(&answer).map_err(|source| Unanswered::Malformed { data: answer, source })?
        };
        match (result.action, result.content) {
            (ElicitationAction::Accept, Some(content)) => serde_path_to_error::deserialize(&content).map_err(|source| Unanswered::Malformed { data: content, source }),
            (ElicitationAction::Accept, None) => Err(Unanswered::Empty),
            (ElicitationAction::Decline, _) => Err(Unanswered::Declined),
            (_, _) => Err(Unanswered::Dismissed),
        }
    }
}

impl<C> FromContextPart<C> for Elicitation
where C: AsRequestContext
{
    fn from_context_part(context: &mut C) -> Result<Self, ErrorData> {
        context.as_request_context().extensions.get::<Self>().cloned().ok_or_else(|| ErrorData::internal_error("elicitation reaches a tool of a rasm_mcp::Server alone", None))
    }
}

#[derive_where(Debug)]
pub struct Server<A> {
    #[derive_where(skip)]
    app: A,
    info: ServerConfig,
    changes: Changes<A>,
    #[derive_where(skip)]
    parked: Mutex<BTreeMap<String, (Running, oneshot::Sender<InputResponses>)>>,
}

impl<A> Server<A>
where A: App
{
    pub fn new<P, F>(instructions: &str, build: F) -> impl AsyncFnOnce(P, Implementation, Notifier) -> Result<Self, A::Error>
    where F: AsyncFnOnce(P, Changes<A>) -> Result<A, A::Error> {
        async move |args: P, identity: Implementation, notifier| {
            let changes = Changes { notifier, executions: Arc::default(), executed: watch::Sender::default(), tools: Arc::default(), prompts: Arc::default(), resources: Arc::default() };
            let app = build(args, changes.clone()).await?;
            let capabilities = ServerCapabilities::builder()
                .enable_prompts()
                .enable_prompts_list_changed()
                .enable_resources()
                .enable_resources_list_changed()
                .enable_resources_subscribe()
                .enable_tools()
                .enable_tool_list_changed()
                .build();
            Ok(Self { app, info: ServerConfig::new(capabilities).with_server_info(identity).with_instructions(instructions), changes, parked: Mutex::default() })
        }
    }
}

impl<A> Endpoint for Server<A>
where A: App
{
    type Error = A::Error;

    fn info(&self) -> &ServerConfig { &self.info }

    async fn handle(&self, request: ClientRequest, context: RequestContext<RoleServer>) -> Result<ServerResult, ErrorData> {
        const ABANDONED: Duration = Duration::from_hours(1);
        macro_rules! paged {
            ($listed:ident, $rows:expr, $params:expr, $key:ident) => {{
                const PAGE: usize = 1024;
                let mut rows: Vec<_> = $rows;
                rows.sort_by(|left, right| left.$key.cmp(&right.$key));
                rows.drain(..$params.and_then(|params| params.cursor).map_or(0, |cursor| rows.partition_point(|row| AsRef::<str>::as_ref(&row.$key) <= cursor.as_str())));
                let next_cursor = rows.get(PAGE - 1..=PAGE).and_then(<[_]>::first).map(|row| AsRef::<str>::as_ref(&row.$key).to_owned());
                rows.truncate(PAGE);
                Ok($listed { next_cursor, ..$listed::with_all_items(rows) }.into())
            }};
        }
        match request {
            ClientRequest::CallToolRequest(call) => {
                let parked = self.parked.with_mut(|waiting| {
                    waiting.retain(|_, (held, _)| held.execution.since.elapsed() < ABANDONED);
                    let retried = |_: &String, (held, _): &mut (Running, _)| (&held.execution.tool, &held.execution.arguments) == (&call.params.name, &call.params.arguments);
                    call.params.request_state.as_ref().map(|state| waiting.extract_if(state..=state, retried).next().ok_or(Unserved::Unparked)).transpose()
                })?;
                let mut running = if let Some((_, (running, answer))) = parked {
                    let (Ok(()) | Err(_)) = answer.send(call.params.input_responses.unwrap_or_default());
                    running
                } else {
                    let tools = self.changes.tools.load_full();
                    let tool = tools.get(&call.params.name).ok_or_else(|| Unserved::Unlisted { name: call.params.name.clone() })?;
                    let destructive = tool.annotations.as_ref().is_none_or(|hints| hints.read_only_hint != Some(true) && hints.is_destructive());
                    let execution = Execution { tool: call.params.name.clone(), arguments: call.params.arguments.clone(), outcome: None, since: Instant::now() };
                    let (progress, (asked, questions), mut called) = (Progress::default(), mpsc::unbounded_channel(), context.clone());
                    called.extensions.insert(progress.clone());
                    called.ct = CancellationToken::new();
                    called.extensions.insert(Elicitation { context: called.clone(), asked });
                    let (app, released) = (self.app.clone(), called.ct.clone().drop_guard());
                    let call = async move { tools.call(ToolCallContext::new(&app, call.params, called)).await };
                    Running { call: call.inspect(move |_| drop(released)).boxed(), questions, progress: progress.0.subscribe(), execution, destructive }
                };
                tokio::select! {
                    biased;
                    Some((mut required, answer)) = running.questions.recv() => {
                        running.execution.since = Instant::now();
                        self.parked.lock().insert(required.request_state.insert(uuid::Uuid::new_v4().to_string()).clone(), (running, answer));
                        Ok(CallToolResponse::from(required).into())
                    }
                    (completed, ()) = future::join(&mut running.call, relayed(running.progress.clone(), &context)) => completed.map(ServerResult::from),
                    () = context.ct.cancelled() => {
                        if running.destructive {
                            drop(running.questions);
                            let row = self.changes.executions.with_mut(|rows| {
                                rows.retain(|_, row| row.outcome.is_none() || row.since.elapsed() < ABANDONED);
                                *rows.entry(rows.last_key_value().map_or(0, |(row, _)| row.saturating_add(1))).insert_entry(running.execution).key()
                            });
                            let outcome = running.call.await.map(ServerResult::from);
                            self.changes.executions.lock().entry(row).and_modify(|recorded| (recorded.outcome, recorded.since) = (Some(outcome), Instant::now()));
                            self.changes.executed.send_replace(());
                        }
                        Err(internal(ServiceError::Cancelled { reason: None }))
                    }
                }
            }
            ClientRequest::ListToolsRequest(list) => paged!(ListToolsResult, self.changes.tools.load().list_all(), list.params, name),
            ClientRequest::GetPromptRequest(get) =>
                self.changes.prompts.load_full().get_prompt(PromptContext::new(&self.app, get.params.name, get.params.arguments, context)).await.map(ServerResult::from),
            ClientRequest::ListPromptsRequest(list) => paged!(ListPromptsResult, self.changes.prompts.load().list_all(), list.params, name),
            ClientRequest::ListResourcesRequest(list) => {
                let listed = future::join_all(self.changes.resources.load_full().lists.iter().map(|lister| lister(self.app.clone()))).await.into_iter().collect::<validated::Validated<Vec<_>, _>>();
                let rows = listed.ok().map_err(|errors| serde_json::to_value(&errors).map_or_else(internal, |data| ErrorData { data: Some(data), ..errors.first().clone() }))?.concat();
                paged!(ListResourcesResult, rows, list.params, uri)
            }
            ClientRequest::ListResourceTemplatesRequest(list) => paged!(ListResourceTemplatesResult, self.changes.resources.load().templates.clone(), list.params, uri_template),
            ClientRequest::ReadResourceRequest(read) => {
                let reading = self.changes.resources.load().routed(&read.params.uri).and_then(|(route, variables)| (route.read)(self.app.clone(), read.params.uri, variables))?;
                reading.await.map(|contents| ReadResourceResult::new(contents).into())
            }
            other => Err(ErrorData::new(ErrorCode::METHOD_NOT_FOUND, other.method().to_owned(), None)),
        }
    }

    fn subscribe(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, Unserved> {
        let watching = self.changes.resources.load().routed(uri).and_then(|(route, variables)| (route.watch)(self.app.clone(), uri.to_owned(), variables))?;
        let (changes, uri) = (self.changes.clone(), uri.to_owned());
        Ok(watching.map(move |()| changes.updated(uri.clone())).collect())
    }

    fn serve(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), A::Error>> + Send { self.app.clone().serve(self.changes.clone(), stopping) }
}

struct Hosting<E> {
    endpoint: E,
    notifier: Notifier,
    stopping: CancellationToken,
    requests: TaskTracker,
    watches: TaskTracker,
    leases: Mutex<BTreeMap<String, Weak<AbortOnDropHandle<()>>>>,
}

struct Session<E> {
    host: Arc<Hosting<E>>,
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
            let held = Arc::new(AbortOnDropHandle::new(self.host.watches.spawn(watched)));
            leases.insert(uri.clone(), Arc::downgrade(&held));
            held
        };
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
            global::meter(env!("CARGO_PKG_NAME")).f64_histogram("mcp.server.operation.duration").with_unit("s").with_boundaries(BOUNDARIES.to_vec()).build()
        });
        let _held = self.host.requests.token();
        let (method, version) = (request.method().to_owned(), context.protocol_version());
        let target = match &request {
            ClientRequest::CallToolRequest(call) => Some(("gen_ai.tool.name", call.params.name.to_string(), true)),
            ClientRequest::GetPromptRequest(get) => Some(("gen_ai.prompt.name", get.params.name.clone(), true)),
            ClientRequest::ReadResourceRequest(read) => Some(("mcp.resource.uri", read.params.uri.clone(), false)),
            _ => None,
        };
        let span = tracing::info_span!(
            "request",
            otel.name = target.as_ref().map_or_else(|| method.clone(), |(_, name, _)| format!("{method} {name}")),
            otel.kind = "server",
            mcp.method.name = method,
            gen_ai.tool.name = Empty,
            gen_ai.prompt.name = Empty,
            mcp.resource.uri = Empty,
            mcp.protocol.version = version.as_ref().map(ProtocolVersion::as_str),
            mcp.session.id = context.extensions.get::<Parts>().and_then(|parts| parts.headers.get(HEADER_SESSION_ID)).and_then(|value| value.to_str().ok()),
            { JSONRPC_REQUEST_ID } = %context.id,
            { ERROR_TYPE } = Empty,
            { RPC_RESPONSE_STATUS_CODE } = Empty,
        );
        if let Some((key, name, _)) = &target {
            span.record(*key, name.as_str());
        }
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
            target.filter(|&(_, _, metered)| metered).map(|(key, name, _)| KeyValue::new(key, name)),
            error_type.map(|kind| KeyValue::new(ERROR_TYPE, kind)),
        ];
        DURATION.record(elapsed.as_secs_f64(), &attributes.into_iter().flatten().collect::<Vec<_>>());
        result
    }

    fn notifications(&self, filter: SubscriptionFilter) -> impl Stream<Item = ServerNotification> + Send + 'static {
        let (admitted, held, missed) = (resynced(&filter, []).map(|notification| discriminant(&notification)).collect::<Vec<_>>(), self.subscribed.subscribe(), self.subscribed.subscribe());
        self.host
            .notifier
            .subscribe()
            .flat_map(move |received| match received {
                Ok(notification) => future::Either::Left(stream::once(ready(notification))),
                Err(BroadcastStreamRecvError::Lagged(_)) => future::Either::Right(stream::iter(resynced(&filter, Vec::from_iter(missed.borrow().keys().cloned())))),
            })
            .filter(move |notification| {
                ready(match notification {
                    ServerNotification::ResourceUpdatedNotification(updated) => held.borrow().contains_key(&updated.params.uri),
                    other => admitted.contains(&discriminant(other)),
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
        accepted
            .resource_subscriptions
            .iter_mut()
            .for_each(|uris| uris.retain(|uri| self.hold(uri.clone()).inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "subscription refused")).is_ok()));
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

fn relayed(updates: watch::Receiver<(f64, Option<f64>, Option<String>)>, context: &RequestContext<RoleServer>) -> impl Future<Output = ()> + Send + 'static {
    let (peer, cancelled) = (context.peer.clone(), context.ct.clone());
    let updates = stream::iter(context.meta.get_progress_token().map(|token| {
        WatchStream::from_changes(updates).map(move |(progress, total, message)| {
            let mut update = ProgressNotificationParam::new(token.clone(), progress);
            (update.total, update.message) = (total, message);
            Ok(update)
        })
    }))
    .flatten();
    async move { updates.take_until(cancelled.cancelled()).try_for_each(|update| peer.notify_progress(update)).await }
        .unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "progress relay failed"))
}

fn internal<E>(error: E) -> ErrorData
where E: Error {
    ErrorData::internal_error(Report::new(error).to_string(), None)
}

#[must_use]
pub fn fault(error: ServiceError) -> ErrorData { if let ServiceError::McpError(data) = error { data } else { internal(error) } }

pub fn unwatched<A, P>(_: A, _: P) -> stream::Empty<()> { stream::empty() }

#[must_use]
pub fn meta<R>(rows: R) -> MetaObject
where R: IntoIterator<Item: Host> {
    MetaObject(rows.into_iter().map(Host::entry).map(|(key, value)| (key.to_owned(), value)).collect())
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<P, F, E>(version: &'static str, build: F) -> process::ExitCode
where
    P: Parser,
    F: AsyncFnOnce(P, Implementation, Notifier) -> Result<E, E::Error>,
    E: Endpoint,
{
    let mut command = Options::augment_args(P::command().version(version));
    let matches = command.get_matches_mut();
    let (options, args) = Options::from_arg_matches(&matches).and_then(|options| P::from_arg_matches(&matches).map(|args| (options, args))).unwrap_or_else(|error| error.exit());
    let identity = command.get_about().into_iter().fold(Implementation::new(command.get_name(), version), |identity, about| identity.with_description(about.to_string()));
    let mut json = json_subscriber::JsonLayer::stderr();
    json.with_timer("timestamp", SystemTime).with_level("level").with_target("target").with_top_level_flattened_span_list().with_flattened_event().with_opentelemetry_ids(true);
    json.add_static_field(SERVICE_NAME, identity.name.clone().into());
    json.add_static_field(SERVICE_VERSION, identity.version.clone().into());
    let exporters = options
        .otlp
        .then(|| {
            let resource = opentelemetry_sdk::Resource::builder().with_service_name(identity.name.clone()).with_attribute(KeyValue::new(SERVICE_VERSION, identity.version.clone())).build();
            Ok::<_, ExporterBuildError>((
                SdkTracerProvider::builder().with_resource(resource.clone()).with_batch_exporter(SpanExporter::builder().with_http().build()?).build(),
                SdkLoggerProvider::builder().with_resource(resource.clone()).with_batch_exporter(LogExporter::builder().with_http().build()?).build(),
                SdkMeterProvider::builder().with_resource(resource).with_periodic_exporter(MetricExporter::builder().with_http().build()?).build(),
            ))
        })
        .transpose();
    tracing_subscriber::registry()
        .with(options.log.clone())
        .with(json)
        .with(exporters.as_ref().ok().and_then(Option::as_ref).map(|(traces, logs, metrics)| {
            global::set_meter_provider(metrics.clone());
            global::set_text_map_propagator(TraceContextPropagator::new());
            tracing_opentelemetry::layer().with_tracer(traces.tracer(identity.name.clone())).and_then(OpenTelemetryTracingBridge::new(logs))
        }))
        .init();
    panic::set_hook(Box::new(|info| tracing::error!({ EXCEPTION_MESSAGE } = %info, { EXCEPTION_STACKTRACE } = %Backtrace::force_capture(), "panicked")));
    let reported = |errors: &NEVec<Failed<E::Error>>| errors.iter().for_each(|error| tracing::error!({ EXCEPTION_MESSAGE } = %Report::new(error), "server failed"));
    let Some(exporters) = exporters.map_err(|error| Failed::Export(error).into()).inspect_err(reported).ok() else { return process::ExitCode::FAILURE };
    let listening = async move {
        let signals = cfg_select! {
            unix => [SignalKind::interrupt(), SignalKind::terminate(), SignalKind::hangup()].map(|kind| signal(kind).map(SignalStream::new)),
            windows => [windows::ctrl_c().map(CtrlCStream::new)],
        };
        let mut signals = signals.into_iter().try_collect::<stream::SelectAll<_>>().map_err(|source| Failed::Io { stage: Stage::Signal, source })?;
        let socket = net::TcpListener::bind(options.listen).await.map_err(|source| Failed::Io { stage: Stage::Bind(options.listen), source })?;
        tracing::info!(listen = %options.listen, "listening");
        let notifier = Notifier::default();
        let future::Either::Left((started, _)) = future::select(pin!(build(args, identity, notifier.clone())), signals.next()).await else { return Ok(()) };
        let endpoint = started.map_err(Failed::Endpoint)?;
        let host = Arc::new(Hosting { endpoint, notifier, stopping: CancellationToken::new(), requests: TaskTracker::new(), watches: TaskTracker::new(), leases: Mutex::default() });
        let manager = Arc::new(LocalSessionManager::default());
        let mut config = StreamableHttpServerConfig::default().with_json_response(true).enforce_origin_validation();
        config.allowed_hosts.extend(options.hosts.iter().map(Authority::to_string).chain((!options.listen.ip().is_unspecified()).then(|| options.listen.ip().to_string())));
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
        let served = http.as_mut().take_output().unwrap_or(Ok(())).map_err(|source| Failed::Io { stage: Stage::Serve, source });
        validated::Validated::from(ended).map2(served.into(), |(), ()| ()).ok()
    };
    let hosted = runtime::Runtime::new()
        .map_err(|source| Failed::Io { stage: Stage::Runtime, source }.into())
        .and_then(|runtime| {
            let hosted = runtime.block_on(listening);
            runtime.shutdown_background();
            hosted
        })
        .inspect_err(reported);
    let flushed = exporters
        .into_iter()
        .flat_map(|(traces, logs, metrics)| [traces.shutdown(), logs.shutdown(), metrics.shutdown()])
        .map(|flush| flush.map_err(Failed::Flush))
        .collect::<validated::Validated<(), _>>()
        .ok()
        .inspect_err(reported);
    hosted.and(flushed).map_or(process::ExitCode::FAILURE, |()| process::ExitCode::SUCCESS)
}
