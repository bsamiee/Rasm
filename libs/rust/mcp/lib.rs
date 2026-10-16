#![feature(derive_from, error_reporter, iterator_try_collect, nonpoison_mutex, sync_nonpoison)]

use core::{
    borrow::Borrow,
    error::Error,
    fmt::{self, Debug, Display, Formatter},
    from::From,
    future::{Ready, ready},
    iter, mem,
    net::SocketAddr,
    pin::pin,
};
use std::{
    backtrace::Backtrace,
    borrow::Cow,
    collections::{BTreeMap, btree_map::Entry},
    error::Report,
    io, panic,
    process::ExitCode,
    sync::{Arc, LazyLock, OnceLock, nonpoison::Mutex},
};

use axum::{
    Router,
    body::Body,
    extract::Request,
    http::{
        HeaderValue, Method, Response, StatusCode,
        header::{InvalidHeaderValue, WWW_AUTHENTICATE},
        request::Parts,
    },
    middleware::{self, Next},
    response::{IntoResponse as _, Json},
    routing::get,
};
use bounded_integer::BoundedU32;
use clap::{Args, FromArgMatches as _, Parser, error::ErrorKind};
use futures::{
    FutureExt as _, Stream, StreamExt as _, TryFutureExt as _, TryStreamExt as _,
    future::{self, OptionFuture, join_all},
    stream,
};
use jsonschema::{Validator, validator_for};
use matchit::InsertError;
use nonempty_collections::NEVec;
use opentelemetry::{
    KeyValue, global,
    metrics::Histogram,
    propagation::{Extractor, TextMapPropagator as _},
    trace::TracerProvider as _,
};
use opentelemetry_appender_tracing::layer::OpenTelemetryTracingBridge;
use opentelemetry_otlp::{ExporterBuildError, LogExporter, MetricExporter, OTEL_EXPORTER_OTLP_ENDPOINT, SpanExporter, WithExportConfig as _};
use opentelemetry_sdk::{Resource as Telemetry, error::OTelSdkError, logs::SdkLoggerProvider, metrics::SdkMeterProvider, propagation::TraceContextPropagator, trace::SdkTracerProvider};
use opentelemetry_semantic_conventions::attribute::{ERROR_TYPE, EXCEPTION_MESSAGE, EXCEPTION_STACKTRACE, JSONRPC_REQUEST_ID, RPC_RESPONSE_STATUS_CODE, SERVICE_NAME, SERVICE_VERSION};
use percent_encoding::percent_decode_str;
use rmcp::{
    ErrorData, Peer, RoleServer, ServerHandler,
    handler::server::{
        prompt::PromptContext,
        router::{prompt::PromptRouter, tool::ToolRouter},
        tool::{IntoCallToolResult, ToolCallContext},
    },
    model::{
        CallToolRequestParams, CallToolResponse, CallToolResult, ConstString as _, ContentBlock, ElicitRequest, ElicitRequestParams, ElicitResult, ElicitationAction, ElicitationSchema,
        GetPromptRequestParams, GetPromptResponse, Implementation, InputRequest, InputRequiredResult, InputResponses, JsonObject, ListPromptsResult, ListResourceTemplatesResult, ListResourcesResult,
        ListToolsResult, MetaObject, Notification, PaginatedRequestParams, ProgressNotificationParam, PromptListChangedNotification, ProtocolVersion, ReadResourceRequestParams, ReadResourceResponse,
        ReadResourceResult, Resource, ResourceContents, ResourceListChangedNotification, ResourceTemplate, ResourceUpdatedNotification, ResourceUpdatedNotificationParam, ServerCapabilities,
        ServerConfig, ServerNotification, SubscribeRequestParams, SubscriptionFilter, SubscriptionsListenRequestMethod, Tool, ToolListChangedNotification, UnsubscribeRequestParams,
    },
    service::{DEFAULT_SUBSCRIPTION_CHANNEL_CAPACITY, ElicitationError, ElicitationSafe, MaybeSendFuture, NotificationContext, RequestContext, SubscriptionContext, SubscriptionSendError},
    transport::{
        common::http_header::{HEADER_MCP_METHOD, HEADER_SESSION_ID},
        streamable_http_server::{StreamableHttpServerConfig, StreamableHttpService, session::local::LocalSessionManager},
    },
};
use serde::{Serialize, de::DeserializeOwned};
use serde_json::Value;
use serde_path_to_error::deserialize;
use tokio::{
    net::TcpListener,
    runtime::Runtime,
    signal::unix::{SignalKind, signal},
    sync::{
        broadcast::{self, error::SendError},
        watch,
    },
    time::Instant,
};
use tokio_stream::wrappers::{BroadcastStream, WatchStream, errors::BroadcastStreamRecvError};
use tokio_util::{
    sync::CancellationToken,
    task::{AbortOnDropHandle, TaskTracker},
};
use tower::{
    ServiceExt as _,
    util::{BoxCloneSyncService, service_fn},
};
use tower_oauth2_resource_server::{
    claims::DefaultClaims,
    error::{AuthError, StartupError},
    error_handler::ErrorHandler,
    server::OAuth2ResourceServer,
    tenant::TenantConfiguration,
};
use tracing::{Instrument as _, field::Empty};
use tracing_opentelemetry::{OpenTelemetrySpanExt as _, SetParentError};
use tracing_subscriber::{EnvFilter, filter::Directive, fmt::time::SystemTime, layer::SubscriberExt as _, util::SubscriberInitExt as _};
use url::{Position, Url};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Endpoint: Send + Sync + Sized + 'static {
    type Error: Error + Send + Sync + 'static;

    fn session(self: Arc<Self>, stopping: &CancellationToken, requests: &TaskTracker) -> impl ServerHandler + use<Self>;

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
    #[command(flatten)]
    authorization: Option<Authorization>,
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

#[derive(Args, Clone, Debug)]
struct Authorization {
    #[arg(long, env = "MCP_RESOURCE", required = false, requires = "servers", help = "Public URL of the MCP endpoint that access tokens name as their audience")]
    resource: Url,
    #[arg(
        long = "authorization-server",
        env = "MCP_AUTHORIZATION_SERVERS",
        value_name = "ISSUER",
        value_delimiter = ',',
        requires = "resource",
        help = "Issuer identifier of an authorization server, spelled as its metadata states it"
    )]
    servers: Vec<String>,
}

#[derive(Debug, From)]
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
        serde_json::to_string(&Rendered { error: &self.0, message: self.0.to_string() })
            .map(|text| CallToolResult::error(vec![ContentBlock::text(text)]).into())
            .map_err(|error| ErrorData::internal_error(Report::new(error).to_string(), None))
    }
}

#[derive(Debug)]
pub enum Elicited<R> {
    Complete(R),
    InputRequired(InputRequiredResult),
}

impl<R> IntoCallToolResult for Elicited<R>
where R: IntoCallToolResult
{
    fn into_call_tool_result(self) -> Result<CallToolResponse, ErrorData> {
        match self {
            Self::Complete(result) => result.into_call_tool_result(),
            Self::InputRequired(required) => Ok(required.into()),
        }
    }
}

#[derive(Debug, Serialize, thiserror::Error)]
#[serde(tag = "_tag")]
#[error("arguments break the tool's input schema at {}, correct each listed path, then call again", paths.join(", "))]
struct InvalidArguments {
    paths: Vec<String>,
    errors: Vec<String>,
}

#[derive(Clone, Copy, Debug)]
pub enum Anthropic {
    MaxResultSizeChars(BoundedU32<1, 500_000>),
    AlwaysLoad,
    RequiresUserInteraction,
}

impl FromIterator<Anthropic> for MetaObject {
    fn from_iter<I>(iter: I) -> Self
    where I: IntoIterator<Item = Anthropic> {
        Self(
            iter.into_iter()
                .map(|entry| match entry {
                    Anthropic::MaxResultSizeChars(chars) => ("anthropic/maxResultSizeChars".to_owned(), Value::from(chars.get())),
                    Anthropic::AlwaysLoad => ("anthropic/alwaysLoad".to_owned(), Value::Bool(true)),
                    Anthropic::RequiresUserInteraction => ("anthropic/requiresUserInteraction".to_owned(), Value::Bool(true)),
                })
                .collect(),
        )
    }
}

pub struct ResourceRouter<A> {
    routes: matchit::Router<BoxCloneSyncService<(A, String, JsonObject), ReadResourceResult, ErrorData>>,
    resources: Vec<Resource>,
    templates: Vec<ResourceTemplate>,
}

impl<A> Clone for ResourceRouter<A> {
    fn clone(&self) -> Self { Self { routes: self.routes.clone(), resources: self.resources.clone(), templates: self.templates.clone() } }
}

impl<A> Debug for ResourceRouter<A> {
    fn fmt(&self, f: &mut Formatter<'_>) -> fmt::Result { f.debug_struct("ResourceRouter").field("resources", &self.resources).field("templates", &self.templates).finish_non_exhaustive() }
}

impl<A> Default for ResourceRouter<A> {
    fn default() -> Self { Self { routes: matchit::Router::new(), resources: Vec::new(), templates: Vec::new() } }
}

impl<A> ResourceRouter<A>
where A: Clone + Send + Sync + 'static
{
    pub fn resource<F, R>(mut self, resource: Resource, read: F) -> Result<Self, Unroutable>
    where
        F: Fn(A, String) -> R + Clone + Send + Sync + 'static,
        R: Future<Output = Result<Vec<ResourceContents>, ErrorData>> + Send + 'static,
    {
        self.routes
            .insert(resource.uri.clone(), BoxCloneSyncService::new(service_fn(move |(app, uri, _): (A, String, JsonObject)| read(app, uri).map_ok(ReadResourceResult::new))))
            .map_err(|source| Unroutable { uri: resource.uri.clone(), source })?;
        self.resources.push(resource);
        Ok(self)
    }

    pub fn template<P, F, R>(mut self, template: ResourceTemplate, read: F) -> Result<Self, Unroutable>
    where
        P: DeserializeOwned + Send + 'static,
        F: Fn(A, String, P) -> R + Clone + Send + Sync + 'static,
        R: Future<Output = Result<Vec<ResourceContents>, ErrorData>> + Send + 'static,
    {
        self.routes
            .insert(
                template.uri_template.clone(),
                BoxCloneSyncService::new(service_fn(move |(app, uri, variables): (A, String, JsonObject)| {
                    let read = read.clone();
                    ready(
                        deserialize::<_, P>(Value::Object(variables)).map_err(|error| {
                            ErrorData::invalid_params(format!("{uri} matches the template but not its variables: {error}, read resources/templates/list for the variable forms"), None)
                        }),
                    )
                    .and_then(move |variables| read(app, uri, variables))
                    .map_ok(ReadResourceResult::new)
                })),
            )
            .map_err(|source| Unroutable { uri: template.uri_template.clone(), source })?;
        self.templates.push(template);
        Ok(self)
    }
}

struct Compiled {
    schema: Arc<JsonObject>,
    validator: Validator,
}

#[derive(Debug, Serialize)]
struct Execution {
    tool: Cow<'static, str>,
    arguments: Option<JsonObject>,
    result: Option<CallToolResult>,
}

#[derive(Clone, Debug)]
pub enum Target {
    Tool(Cow<'static, str>),
    Prompt(String),
    Resource(String),
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
#[error("{uri} cannot join the resource router")]
pub struct Unroutable {
    pub uri: String,
    pub source: InsertError,
}

#[derive(Debug, thiserror::Error)]
pub enum Unanswered {
    #[error("cannot elicit the answer")]
    Elicitation(#[from] ElicitationError),
    #[error("cannot derive the requested schema")]
    Schema(#[source] serde_json::Error),
}

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
    #[error("cannot load the authorization servers")]
    Authorization(#[source] StartupError),
    #[error("cannot state the resource metadata URL in a WWW-Authenticate challenge")]
    Challenge(#[source] InvalidHeaderValue),
}

impl<S, X> From<Failed<S, X>> for NEVec<Failed<S, X>> {
    fn from(value: Failed<S, X>) -> Self { Self::new(value) }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Subscriber<H> {
    stopping: CancellationToken,
    subscribed: watch::Sender<BTreeMap<String, H>>,
    relay: OnceLock<AbortOnDropHandle<()>>,
}

impl<H> Subscriber<H> {
    #[must_use]
    pub fn new(stopping: CancellationToken) -> Self { Self { stopping, subscribed: watch::Sender::default(), relay: OnceLock::new() } }

    pub fn subscribe<F>(&self, uri: String, hold: F) -> Ready<Result<(), ErrorData>>
    where F: FnOnce(&String) -> H {
        self.subscribed.send_modify(|uris| {
            uris.entry(uri).or_insert_with_key(hold);
        });
        ready(Ok(()))
    }

    pub fn unsubscribe(&self, uri: &str) -> Ready<Result<(), ErrorData>> {
        self.subscribed.send_modify(|uris| {
            uris.remove(uri);
        });
        ready(Ok(()))
    }
}

#[derive(Clone, Debug)]
pub struct Changes<T, P, R> {
    pub tools: watch::Sender<Arc<T>>,
    pub prompts: watch::Sender<Arc<P>>,
    pub resources: watch::Sender<Arc<R>>,
    pub updated: broadcast::Sender<String>,
}

impl<T, P, R> Changes<T, P, R>
where
    T: Send + Sync + 'static,
    P: Send + Sync + 'static,
    R: Send + Sync + 'static,
{
    pub fn new(tools: T, prompts: P, resources: R) -> Self {
        Self {
            tools: watch::Sender::new(Arc::new(tools)),
            prompts: watch::Sender::new(Arc::new(prompts)),
            resources: watch::Sender::new(Arc::new(resources)),
            updated: broadcast::Sender::new(DEFAULT_SUBSCRIPTION_CHANNEL_CAPACITY),
        }
    }

    pub fn update(&self, uri: String) {
        match self.updated.send(uri) {
            Ok(_) | Err(SendError(_)) => {}
        }
    }

    fn notifications<H>(&self, subscribed: watch::Receiver<BTreeMap<String, H>>) -> impl Stream<Item = ServerNotification> + Send + 'static
    where H: Send + Sync + 'static {
        let updates = BroadcastStream::new(self.updated.subscribe())
            .flat_map(move |received| {
                stream::iter(match received {
                    Ok(uri) => subscribed.borrow().contains_key(&uri).then_some(uri).into_iter().collect::<Vec<_>>(),
                    Err(BroadcastStreamRecvError::Lagged(_)) => subscribed.borrow().keys().cloned().collect(),
                })
            })
            .map(|uri| ServerNotification::ResourceUpdatedNotification(ResourceUpdatedNotification::new(ResourceUpdatedNotificationParam::new(uri))));
        stream::select(
            stream::select(
                WatchStream::from_changes(self.tools.subscribe()).map(|_| ServerNotification::ToolListChangedNotification(ToolListChangedNotification::default())),
                WatchStream::from_changes(self.prompts.subscribe()).map(|_| ServerNotification::PromptListChangedNotification(PromptListChangedNotification::default())),
            ),
            stream::select(WatchStream::from_changes(self.resources.subscribe()).map(|_| ServerNotification::ResourceListChangedNotification(ResourceListChangedNotification::default())), updates),
        )
    }

    pub fn initialized<H>(&self, subscriber: &Subscriber<H>, peer: Peer<RoleServer>)
    where H: Send + Sync + 'static {
        subscriber.relay.get_or_init(|| AbortOnDropHandle::new(tokio::spawn(relay(peer, self.notifications(subscriber.subscribed.subscribe())))));
    }

    pub async fn listen<H>(&self, subscriber: &Subscriber<H>, context: SubscriptionContext)
    where H: Send + Sync + 'static {
        let (_, accepted) = watch::channel(context.accepted().resource_subscriptions.iter().flatten().map(|uri| (uri.clone(), ())).collect());
        let (stopped, closed) = (pin!(subscriber.stopping.cancelled()), pin!(context.cancelled()));
        let sent = self
            .notifications(accepted)
            .take_until(future::select(stopped, closed))
            .map(Ok)
            .try_for_each(async |notification| match context.sink().send(notification).await {
                Err(SubscriptionSendError::NotificationNotAccepted(_)) => Ok(()),
                sent => sent,
            })
            .await;
        match sent {
            Ok(()) | Err(SubscriptionSendError::SubscriptionClosed) => {}
            Err(error) => tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "subscription closed"),
        }
    }
}

struct Inner<A> {
    app: A,
    info: ServerConfig,
    changes: Changes<ToolRouter<A>, PromptRouter<A>, ResourceRouter<A>>,
    executions: Mutex<BTreeMap<usize, Execution>>,
    validators: Mutex<BTreeMap<Cow<'static, str>, Compiled>>,
}

pub struct Server<A>(Arc<Inner<A>>);

impl<A> Clone for Server<A> {
    fn clone(&self) -> Self { Self(Arc::clone(&self.0)) }
}

impl<A> Debug for Server<A> {
    fn fmt(&self, f: &mut Formatter<'_>) -> fmt::Result { f.debug_struct("Server").field("info", &self.0.info).finish_non_exhaustive() }
}

impl<A> Server<A>
where A: Clone + Send + Sync + 'static
{
    pub fn new(identity: Implementation, instructions: &'static str, app: A, tools: ToolRouter<A>, prompts: PromptRouter<A>, resources: ResourceRouter<A>) -> Self {
        let capabilities = ServerCapabilities::builder()
            .enable_prompts()
            .enable_prompts_list_changed()
            .enable_resources()
            .enable_resources_list_changed()
            .enable_resources_subscribe()
            .enable_tools()
            .enable_tool_list_changed()
            .build();
        Self(Arc::new(Inner {
            app,
            info: ServerConfig::new(capabilities).with_server_info(identity).with_instructions(instructions),
            changes: Changes::new(tools, prompts, resources),
            executions: Mutex::default(),
            validators: Mutex::default(),
        }))
    }

    #[must_use]
    pub fn changes(&self) -> &Changes<ToolRouter<A>, PromptRouter<A>, ResourceRouter<A>> { &self.0.changes }

    fn executions(&self) -> String { format!("mcp://{}/executions", self.0.info.server_info.name) }
}

impl<A> Endpoint for Server<A>
where A: Clone + Send + Sync + 'static
{
    type Error = !;

    fn session(self: Arc<Self>, stopping: &CancellationToken, requests: &TaskTracker) -> impl ServerHandler + use<A> {
        Session { server: Self::clone(&self), requests: requests.clone(), subscriber: Subscriber::new(stopping.clone()) }
    }
}

struct Session<A> {
    server: Server<A>,
    requests: TaskTracker,
    subscriber: Subscriber<()>,
}

macro_rules! observed {
    ($($method:ident(&$this:ident, $request:tt: $params:ty) -> $response:ty = $name:literal, $target:expr, $body:block)*) => {$(
        async fn $method(&$this, $request: $params, context: RequestContext<RoleServer>) -> Result<$response, ErrorData> {
            observe($name, $target, &context, |_| false, async $body).await
        }
    )*};
}

impl<A> ServerHandler for Session<A>
where A: Clone + Send + Sync + 'static
{
    fn get_info(&self) -> ServerConfig {
        let changes = &self.server.0.changes;
        let mut info = self.server.0.info.clone();
        info.capabilities.prompts.take_if(|_| changes.prompts.borrow().map.is_empty());
        info.capabilities.resources.take_if(|_| {
            let resources = changes.resources.borrow();
            resources.resources.is_empty() && resources.templates.is_empty() && !changes.tools.borrow().list_all().iter().any(destructive)
        });
        info
    }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { Cow::Borrowed(PROTOCOL_VERSIONS) }

    async fn call_tool(&self, request: CallToolRequestParams, mut context: RequestContext<RoleServer>) -> Result<CallToolResponse, ErrorData> {
        let observed = context.clone();
        let failed = |response: &CallToolResponse| matches!(response, CallToolResponse::Complete(result) if result.is_error == Some(true));
        observe("tools/call", Some(Target::Tool(request.name.clone())), &observed, failed, async move {
            let tools = Arc::clone(&self.server.0.changes.tools.borrow());
            let tool = tools.get(&request.name).ok_or_else(|| ErrorData::invalid_params("tool not found", None))?;
            let arguments = Value::Object(request.arguments.clone().unwrap_or_default());
            let (paths, errors): (Vec<_>, Vec<_>) = {
                let mut validators = self.server.0.validators.lock();
                let compiled = || validator_for(&Value::Object(JsonObject::clone(&tool.input_schema))).map_err(|error| ErrorData::internal_error(Report::new(error).to_string(), None));
                let cached = match validators.entry(tool.name.clone()) {
                    Entry::Occupied(slot) if Arc::ptr_eq(&slot.get().schema, &tool.input_schema) => slot.into_mut(),
                    entry @ (Entry::Occupied(_) | Entry::Vacant(_)) => entry.insert_entry(Compiled { schema: Arc::clone(&tool.input_schema), validator: compiled()? }).into_mut(),
                };
                cached.validator.iter_errors(&arguments).map(|error| (error.instance_path().to_string(), error.to_string())).unzip()
            };
            if !paths.is_empty() {
                return Failure(InvalidArguments { paths, errors }).into_call_tool_result();
            }
            let stage = watch::Sender::new(request.name.clone());
            let _progress = context.meta.get_progress_token().map(|token| {
                let stages = WatchStream::new(stage.subscribe()).zip(stream::iter(0_u32..));
                let progress = stages
                    .map(move |(message, step)| ServerNotification::ProgressNotification(Notification::new(ProgressNotificationParam::new(token.clone(), f64::from(step)).with_message(message))));
                AbortOnDropHandle::new(tokio::spawn(relay(context.peer.clone(), progress)))
            });
            context.extensions.insert(stage);
            let execution = destructive(tool).then(|| Execution { tool: request.name.clone(), arguments: request.arguments.clone(), result: None });
            let cancelled = if execution.is_some() { mem::replace(&mut context.ct, CancellationToken::new()) } else { context.ct.clone() };
            let mut call = pin!(tools.call(ToolCallContext::new(&self.server.0.app, request, context)));
            tokio::select! {
                biased;
                response = &mut call => response,
                () = cancelled.cancelled(), if execution.is_some() => {
                    let row = {
                        let mut rows = self.server.0.executions.lock();
                        let row = rows.last_key_value().map_or(0, |(row, _)| row.saturating_add(1));
                        rows.extend(execution.map(|pending| (row, pending)));
                        row
                    };
                    let (_held, response) = (self.requests.token(), call.await);
                    if let (Ok(CallToolResponse::Complete(result)), Some(recorded)) = (&response, self.server.0.executions.lock().get_mut(&row)) {
                        recorded.result = Some(result.clone());
                    }
                    self.server.0.changes.update(self.server.executions());
                    response
                }
            }
        })
        .await
    }

    async fn get_prompt(&self, request: GetPromptRequestParams, context: RequestContext<RoleServer>) -> Result<GetPromptResponse, ErrorData> {
        let observed = context.clone();
        observe("prompts/get", Some(Target::Prompt(request.name.clone())), &observed, |_| false, async move {
            let prompts = Arc::clone(&self.server.0.changes.prompts.borrow());
            prompts.get_prompt(PromptContext::new(&self.server.0.app, request.name, request.arguments, context)).await
        })
        .await
    }

    observed! {
        list_tools(&self, _: Option<PaginatedRequestParams>) -> ListToolsResult = "tools/list", None, {
            Ok(ListToolsResult::with_all_items(self.server.0.changes.tools.borrow().list_all()))
        }
        list_prompts(&self, _: Option<PaginatedRequestParams>) -> ListPromptsResult = "prompts/list", None, {
            Ok(ListPromptsResult::with_all_items(self.server.0.changes.prompts.borrow().list_all()))
        }
        list_resources(&self, request: Option<PaginatedRequestParams>) -> ListResourcesResult = "resources/list", None, {
            let changes = &self.server.0.changes;
            let executions = changes.tools.borrow().list_all().iter().any(destructive).then(|| Resource::new(self.server.executions(), "executions").with_mime_type("application/json"));
            let rows = changes.resources.borrow().resources.iter().cloned().chain(executions).collect();
            page(rows, request).map(|(rows, next_cursor)| ListResourcesResult { next_cursor, ..ListResourcesResult::with_all_items(rows) })
        }
        list_resource_templates(&self, request: Option<PaginatedRequestParams>) -> ListResourceTemplatesResult = "resources/templates/list", None, {
            page(self.server.0.changes.resources.borrow().templates.clone(), request)
                .map(|(rows, next_cursor)| ListResourceTemplatesResult { next_cursor, ..ListResourceTemplatesResult::with_all_items(rows) })
        }
        read_resource(&self, request: ReadResourceRequestParams) -> ReadResourceResponse = "resources/read", Some(Target::Resource(request.uri.clone())), {
            if request.uri == self.server.executions() {
                let mut rows = self.server.0.executions.lock();
                let listed = serde_json::to_string(&rows.values().collect::<Vec<_>>()).map_err(|error| ErrorData::internal_error(Report::new(error).to_string(), None))?;
                rows.retain(|_, row| row.result.is_none());
                return Ok(ReadResourceResult::new(vec![ResourceContents::text(listed, request.uri).with_mime_type("application/json")]).into());
            }
            let resources = Arc::clone(&self.server.0.changes.resources.borrow());
            let matched = resources
                .routes
                .at(&request.uri)
                .map_err(|_unmatched| ErrorData::resource_not_found(format!("{} names no resource, list resources and resource templates for the served uris", request.uri), None))?;
            let variables = matched
                .params
                .iter()
                .map(|(name, value)| percent_decode_str(value).decode_utf8().map(|value| (name.to_owned(), Value::String(value.into_owned()))))
                .try_collect::<JsonObject>()
                .map_err(|error| ErrorData::invalid_params(format!("{} holds a variable that is not percent-encoded UTF-8: {error}, encode each variable as UTF-8", request.uri), None))?;
            matched.value.clone().oneshot((self.server.0.app.clone(), request.uri.clone(), variables)).await.map(ReadResourceResponse::from)
        }
    }

    fn subscribe(&self, request: SubscribeRequestParams, _: RequestContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + MaybeSendFuture + '_ {
        self.subscriber.subscribe(request.uri, |_| ())
    }

    fn unsubscribe(&self, request: UnsubscribeRequestParams, _: RequestContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + MaybeSendFuture + '_ {
        self.subscriber.unsubscribe(&request.uri)
    }

    async fn on_initialized(&self, context: NotificationContext<RoleServer>) { self.server.0.changes.initialized(&self.subscriber, context.peer); }

    fn accepted_subscription_filter(&self, requested: &SubscriptionFilter) -> Option<SubscriptionFilter> { Some(requested.clone()) }

    fn listen(&self, context: SubscriptionContext) -> impl Future<Output = Result<(), ErrorData>> + MaybeSendFuture + '_ { self.server.0.changes.listen(&self.subscriber, context).map(Ok) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn observe<T, E, F, C>(method: &str, target: Option<Target>, context: &RequestContext<RoleServer>, failed: C, call: F) -> Result<T, E>
where
    E: Borrow<ErrorData>,
    F: Future<Output = Result<T, E>>,
    C: FnOnce(&T) -> bool,
{
    struct Carrier<'meta>(&'meta JsonObject);
    impl Extractor for Carrier<'_> {
        fn get(&self, key: &str) -> Option<&str> { self.0.get(key).and_then(|value| deserialize::<_, &str>(value).ok()) }

        fn keys(&self) -> Vec<&str> { self.0.keys().map(String::as_str).collect() }
    }
    static DURATION: LazyLock<Histogram<f64>> = LazyLock::new(|| global::meter(env!("CARGO_PKG_NAME")).f64_histogram("mcp.server.operation.duration").with_unit("s").build());
    let (tool, prompt, resource) = match &target {
        Some(Target::Tool(name)) => (Some(name.as_ref()), None, None),
        Some(Target::Prompt(name)) => (None, Some(name.as_str()), None),
        Some(Target::Resource(uri)) => (None, None, Some(uri.as_str())),
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
    match span.set_parent(TraceContextPropagator::new().extract(&Carrier(&context.meta))) {
        Ok(()) | Err(SetParentError::LayerNotFound | SetParentError::SpanDisabled | SetParentError::AlreadyStarted) => {}
    }
    let started = Instant::now();
    let result = call.instrument(span.clone()).await;
    let elapsed = started.elapsed().as_secs_f64();
    let error_type = match &result {
        Ok(value) => failed(value).then_some(Cow::Borrowed("tool_error")),
        Err(error) => Some(Cow::Owned(error.borrow().code.0.to_string())),
    };
    span.in_scope(|| {
        span.record(ERROR_TYPE, error_type.as_deref());
        match &result {
            Ok(_) => tracing::info!(elapsed, "request completed"),
            Err(error) => {
                span.record(RPC_RESPONSE_STATUS_CODE, error_type.as_deref());
                tracing::warn!(elapsed, { EXCEPTION_MESSAGE } = %error.borrow().message, "request failed");
            }
        }
    });
    DURATION.record(elapsed, &iter::once(KeyValue::new("mcp.method.name", method.to_owned())).chain(error_type.map(|kind| KeyValue::new(ERROR_TYPE, kind))).collect::<Vec<_>>());
    result
}

pub async fn elicit<T>(context: &RequestContext<RoleServer>, responses: Option<&InputResponses>, key: &str, message: &str) -> Result<Elicited<T>, Unanswered>
where T: ElicitationSafe + DeserializeOwned {
    match (context.protocol_version().is_some_and(|version| !version.has_initialize()), responses.and_then(|answers| answers.get(key))) {
        (false, _) => Ok(context.peer.elicit::<T>(message).await?.map(Elicited::Complete).ok_or(ElicitationError::NoContent)?),
        (true, Some(answer)) => {
            let result = deserialize::<_, ElicitResult>(answer).map_err(|error| ElicitationError::ParseError { error: error.into_inner(), data: answer.clone() })?;
            if result.action == ElicitationAction::Accept {
                let accepted = result.content.ok_or(ElicitationError::NoContent)?;
                Ok(deserialize::<_, T>(&accepted).map(Elicited::Complete).map_err(|error| ElicitationError::ParseError { error: error.into_inner(), data: accepted.clone() })?)
            } else if result.action == ElicitationAction::Decline {
                Err(ElicitationError::UserDeclined.into())
            } else {
                Err(ElicitationError::UserCancelled.into())
            }
        }
        (true, None) if context.client_capabilities().and_then(|capabilities| capabilities.elicitation).is_none() => Err(ElicitationError::CapabilityNotSupported.into()),
        (true, None) => ElicitationSchema::from_type::<T>()
            .map(|requested_schema| {
                let request = ElicitRequest::new(ElicitRequestParams::FormElicitationParams { meta: None, message: message.to_owned(), requested_schema });
                Elicited::InputRequired(InputRequiredResult::from_input_requests(BTreeMap::from([(key.to_owned(), InputRequest::Elicitation(request))])))
            })
            .map_err(Unanswered::Schema),
    }
}

async fn relay<S>(peer: Peer<RoleServer>, notifications: S)
where S: Stream<Item = ServerNotification> + Send {
    if let Err(error) = notifications.map(Ok).try_for_each(|notification| peer.send_notification(notification)).await {
        tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "session closed");
    }
}

fn destructive(tool: &Tool) -> bool { tool.annotations.as_ref().is_none_or(|hints| hints.read_only_hint != Some(true) && hints.is_destructive()) }

fn page<T>(rows: Vec<T>, request: Option<PaginatedRequestParams>) -> Result<(Vec<T>, Option<String>), ErrorData> {
    const PAGE: usize = 1024;
    let start = request
        .and_then(|params| params.cursor)
        .map(|cursor| cursor.parse::<usize>())
        .transpose()
        .map_err(|error| ErrorData::invalid_params(format!("cursor is no page offset: {error}, pass the nextCursor a list returned"), None))?
        .unwrap_or_default();
    let end = start.saturating_add(PAGE);
    let next = (end < rows.len()).then(|| end.to_string());
    Ok((rows.into_iter().skip(start).take(PAGE).collect(), next))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<A, F, E, B>(build: F) -> ExitCode
where
    A: Parser,
    F: AsyncFnOnce(A, Implementation) -> Result<E, B>,
    E: Endpoint,
    B: Error + Send + Sync + 'static,
{
    struct Exporters {
        traces: SdkTracerProvider,
        logs: SdkLoggerProvider,
        metrics: SdkMeterProvider,
    }
    #[derive(Clone, Serialize)]
    struct Metadata {
        resource: String,
        authorization_servers: Vec<String>,
    }
    struct Challenge {
        missing: HeaderValue,
        invalid: HeaderValue,
    }
    impl ErrorHandler<Body> for Challenge {
        fn map_error(&self, error: AuthError) -> Response<Body> {
            let challenge = if error == AuthError::MissingAuthorizationHeader { &self.missing } else { &self.invalid };
            (StatusCode::UNAUTHORIZED, [(WWW_AUTHENTICATE, challenge.clone())]).into_response()
        }
    }
    fn install<S, X>(options: &Options, identity: &Implementation) -> Result<Option<Exporters>, Failed<S, X>> {
        let mut json = json_subscriber::JsonLayer::stderr();
        json.with_timer("timestamp", SystemTime).with_level("level").with_target("target").with_top_level_flattened_span_list().with_flattened_event();
        json.add_static_field(SERVICE_NAME, identity.name.clone().into());
        json.add_static_field(SERVICE_VERSION, identity.version.clone().into());
        let exporters = options
            .export
            .as_ref()
            .map(|endpoint| {
                let signal = |kind: &str| format!("{}/v1/{kind}", endpoint.as_str().trim_end_matches('/'));
                let resource = Telemetry::builder().with_service_name(identity.name.clone()).with_attribute(KeyValue::new(SERVICE_VERSION, identity.version.clone())).build();
                Ok::<_, ExporterBuildError>(Exporters {
                    traces: SdkTracerProvider::builder().with_resource(resource.clone()).with_batch_exporter(SpanExporter::builder().with_http().with_endpoint(signal("traces")).build()?).build(),
                    logs: SdkLoggerProvider::builder().with_resource(resource.clone()).with_batch_exporter(LogExporter::builder().with_http().with_endpoint(signal("logs")).build()?).build(),
                    metrics: SdkMeterProvider::builder().with_resource(resource).with_periodic_exporter(MetricExporter::builder().with_http().with_endpoint(signal("metrics")).build()?).build(),
                })
            })
            .transpose();
        let installed = match &exporters {
            Ok(Some(installed)) => Some(installed),
            Ok(None) | Err(_) => None,
        };
        if let Some(exported) = installed {
            global::set_meter_provider(exported.metrics.clone());
        }
        tracing_subscriber::registry()
            .with(options.filter.iter().cloned().fold(EnvFilter::default(), EnvFilter::add_directive))
            .with(json)
            .with(installed.map(|exported| tracing_opentelemetry::layer().with_tracer(exported.traces.tracer(identity.name.clone()))))
            .with(installed.map(|exported| OpenTelemetryTracingBridge::new(&exported.logs)))
            .init();
        panic::set_hook(Box::new(|info| tracing::error!({ EXCEPTION_MESSAGE } = %info, { EXCEPTION_STACKTRACE } = %Backtrace::force_capture(), "panicked")));
        exporters.map_err(Failed::Export)
    }
    async fn host<E, B, G>(options: Options, build: G) -> Result<Validated<(), Failed<B, E::Error>>, NEVec<Failed<B, E::Error>>>
    where
        E: Endpoint,
        G: Future<Output = Result<E, B>>,
    {
        let socket = TcpListener::bind(options.listen).await.map_err(|error| Failed::Bind(options.listen, error))?;
        tracing::info!(listen = %options.listen, "listening");
        let endpoint = Arc::new(build.await.map_err(Failed::Start)?);
        let authorized = OptionFuture::from(options.authorization.map(async |authorization| {
            let resource = match (authorization.resource.path(), authorization.resource.query()) {
                ("/", None) => &authorization.resource[..Position::AfterPort],
                (_, _) => authorization.resource.as_str(),
            };
            let mut metadata = authorization.resource.clone();
            metadata.set_path(&format!("/.well-known/oauth-protected-resource{}", Some(authorization.resource.path()).filter(|path| *path != "/").unwrap_or_default()));
            let challenge = |error: &str| HeaderValue::try_from(format!("Bearer resource_metadata=\"{metadata}\"{error}")).map_err(|error| NEVec::new(Failed::Challenge(error)));
            let challenge = Challenge { missing: challenge("")?, invalid: challenge(", error=\"invalid_token\"")? };
            let tenants = join_all(authorization.servers.iter().map(|issuer| TenantConfiguration::builder(issuer).audiences(&[resource]).build())).await;
            let tenants = match tenants.into_iter().map(|tenant| tenant.map_err(Failed::Authorization)).collect::<Validated<Vec<_>, _>>() {
                Validated::Good(tenants) => tenants,
                Validated::Fail(errors) => return Err(errors),
            };
            let guard = OAuth2ResourceServer::<DefaultClaims>::builder().add_tenants(tenants).build().await.map_err(|error| NEVec::new(Failed::Authorization(error)))?;
            let document = Metadata { resource: resource.to_owned(), authorization_servers: authorization.servers };
            Ok((authorization.resource.clone(), metadata.path().to_owned(), document, guard.into_layer_with_error_handler(Arc::new(challenge))))
        }))
        .await
        .transpose()?;
        let (draining, stopping, sessions) = (CancellationToken::new(), CancellationToken::new(), CancellationToken::new());
        let (requests, streams) = (TaskTracker::new(), TaskTracker::new());
        let config = StreamableHttpServerConfig::default().with_json_response(true).with_cancellation_token(sessions.clone()).enforce_origin_validation();
        let config = match &authorized {
            Some((resource, ..)) => config.with_allowed_hosts([resource.authority()]),
            None => config,
        };
        let (factory, scope, calls) = (Arc::clone(&endpoint), stopping.clone(), requests.clone());
        let service = StreamableHttpService::new(move || Ok(Arc::clone(&factory).session(&scope, &calls)), Arc::new(LocalSessionManager::default()), config);
        let (tracked, listened) = (requests.clone(), streams.clone());
        let router = match authorized {
            Some((resource, metadata, document, guard)) => Router::new().route_service(resource.path(), service).route_layer(guard).route(&metadata, get(Json(document))),
            None => Router::new().route_service("/", service),
        };
        let router = router.layer(middleware::from_fn(move |request: Request, next: Next| {
            let held = (request.method() == Method::POST)
                .then(|| if request.headers().get(HEADER_MCP_METHOD).is_some_and(|method| method == SubscriptionsListenRequestMethod::VALUE) { listened.token() } else { tracked.token() });
            next.run(request).map(|response| {
                response.map(|body| match held {
                    Some(token) => Body::from_stream(stream::unfold((body.into_data_stream(), token), async move |(mut data, guard)| data.next().await.map(|chunk| (chunk, (data, guard))))),
                    None => body,
                })
            })
        }));
        let mut http = pin!(axum::serve(socket, router).with_graceful_shutdown(draining.clone().cancelled_owned()).into_future());
        let mut lifetime = pin!(Arc::clone(&endpoint).serve(stopping.clone()));
        let (mut interrupt, mut terminate, mut hangup) =
            (signal(SignalKind::interrupt()).map_err(Failed::Signal)?, signal(SignalKind::terminate()).map_err(Failed::Signal)?, signal(SignalKind::hangup()).map_err(Failed::Signal)?);
        let ended = tokio::select! {
            biased;
            _ = interrupt.recv() => None,
            _ = terminate.recv() => None,
            _ = hangup.recv() => None,
            ended = &mut lifetime => Some(ended),
            served = &mut http => return Ok(served.map_err(Failed::Serve).into()),
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
        let served = http.await;
        Ok(Validated::from(ended.map_err(Failed::Ended)).map2(served.map_err(Failed::Serve).into(), |(), ()| ()))
    }
    let command = Options::augment_args(A::command());
    let matches = command.clone().get_matches();
    let (options, app, identity) = match (Options::from_arg_matches(&matches), A::from_arg_matches(&matches), command.get_version()) {
        (Ok(options), Ok(_), Some(_)) if !options.listen.ip().is_loopback() && options.authorization.is_none() =>
            command.clone().error(ErrorKind::MissingRequiredArgument, "a non-loopback --listen address takes --resource and --authorization-server").exit(),
        (Ok(options), Ok(app), Some(version)) => {
            let mut identity = Implementation::new(command.get_name(), version);
            identity.description = command.get_about().map(ToString::to_string);
            (options, app, identity)
        }
        (Err(error), _, _) | (_, Err(error), _) => error.exit(),
        (Ok(_), Ok(_), None) => command.clone().error(ErrorKind::MissingRequiredArgument, "binary states no version, derive its Parser with #[command(version)]").exit(),
    };
    let outcome = install::<B, E::Error>(&options, &identity).map_or_else(Validated::fail, |exporters| {
        let hosted = Runtime::new().map_err(|error| NEVec::new(Failed::Runtime(error))).and_then(|runtime| runtime.block_on(host(options, build(app, identity)))).unwrap_or_else(Validated::Fail);
        let flushed = exporters
            .into_iter()
            .flat_map(|exported| [exported.traces.shutdown(), exported.logs.shutdown(), exported.metrics.shutdown()])
            .map(|flush| flush.map_err(Failed::Flush))
            .collect::<Validated<(), _>>();
        hosted.map2(flushed, |(), ()| ())
    });
    match outcome.ok() {
        Ok(()) => ExitCode::SUCCESS,
        Err(errors) => {
            errors.iter().for_each(|error| tracing::error!({ EXCEPTION_MESSAGE } = %Report::new(error), "server failed"));
            ExitCode::FAILURE
        }
    }
}
