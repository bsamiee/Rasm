use core::{error::Error, fmt::Display, future::ready, time::Duration};
use std::{
    borrow::Cow,
    collections::BTreeMap,
    sync::{Arc, nonpoison::Mutex},
};

use derive_where::derive_where;
use futures::{FutureExt as _, Stream, StreamExt as _, TryFuture, TryFutureExt as _, future, stream};
use jsonschema::{paths::Location, validator_for};
use nonempty_collections::NEVec;
use rmcp::{
    ErrorData, Json, RoleServer, ServiceError,
    handler::server::{
        prompt::PromptContext,
        router::prompt::PromptRouter,
        tool::{IntoCallToolResult, ToolCallContext, ToolRoute, ToolRouter},
    },
    model::{
        CallToolResponse, CallToolResult, ClientRequest, ContentBlock, ElicitRequest, ElicitRequestParams, ElicitationAction, ElicitationSchema, ErrorCode, Implementation, InputRequest,
        InputRequiredResult, InputResponses, JsonObject, ListPromptsResult, ListResourceTemplatesResult, ListResourcesResult, ListToolsResult, MetaObject, PromptListChangedNotification,
        ReadResourceResult, Resource, ResourceContents, ResourceListChangedNotification, ResourceTemplate, ResourceUpdatedNotification, ResourceUpdatedNotificationParam, ServerCapabilities,
        ServerConfig, ServerResult, ToolListChangedNotification,
    },
    service::RequestContext,
    transport::common::http_header::JSON_MIME_TYPE,
};
use serde::{Serialize, de::DeserializeOwned};
use serde_json::Value;
use serde_urlencoded::Deserializer;
use tokio::{
    sync::{mpsc, oneshot, watch},
    time::Instant,
};
use tokio_stream::wrappers::WatchStream;
use tokio_util::sync::CancellationToken;

use crate::{Endpoint, Invalid, Notifier, Progress, Unserved, internal};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait App: Clone + Send + Sync + 'static {
    type Error: Error + Send + Sync + 'static + From<Invalid> = Invalid;

    fn serve(self, _: Changes<Self>, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send { stopping.cancelled_owned().map(Ok) }
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
        serde_json::to_string(&self.0).map_err(internal)?.into_resource_contents(uri, mime.or_else(|| Some(JSON_MIME_TYPE.to_owned())))
    }
}

impl<T, E> IntoResourceContents for Result<T, E>
where
    T: IntoResourceContents,
    ErrorData: From<E>,
{
    fn into_resource_contents(self, uri: String, mime: Option<String>) -> Result<Vec<ResourceContents>, ErrorData> { self?.into_resource_contents(uri, mime) }
}

trait Routed<A, T> = Fn(A, String, String) -> Result<T, Unserved> + Send + Sync;

trait List<A> = Fn(A) -> future::BoxFuture<'static, Result<Vec<Resource>, ErrorData>> + Send + Sync;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, Debug)]
pub enum Claude {
    AlwaysLoad(bool),
    MaxResultSizeChars(bounded_integer::BoundedU32<1, 500_000>),
    RequiresUserInteraction,
}

impl FromIterator<Claude> for MetaObject {
    fn from_iter<T>(iter: T) -> Self
    where T: IntoIterator<Item = Claude> {
        let entry = |row| match row {
            Claude::AlwaysLoad(load) => ("anthropic/alwaysLoad", Value::Bool(load)),
            Claude::MaxResultSizeChars(chars) => ("anthropic/maxResultSizeChars", chars.get().into()),
            Claude::RequiresUserInteraction => ("anthropic/requiresUserInteraction", Value::Bool(true)),
        };
        Self(iter.into_iter().map(entry).map(|(key, value)| (key.to_owned(), value)).collect())
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

#[derive(Debug, Serialize)]
struct Execution {
    tool: Cow<'static, str>,
    arguments: Option<JsonObject>,
    outcome: Option<Result<ServerResult, ErrorData>>,
    #[serde(skip)]
    since: Instant,
}

struct Running {
    call: future::BoxFuture<'static, Result<CallToolResponse, ErrorData>>,
    questions: mpsc::UnboundedReceiver<(InputRequiredResult, oneshot::Sender<InputResponses>)>,
    progress: Progress,
    execution: Execution,
    destructive: bool,
}

struct Route<A> {
    read: Box<dyn Routed<A, future::BoxFuture<'static, Result<Vec<ResourceContents>, ErrorData>>>>,
    watch: Box<dyn Routed<A, stream::BoxStream<'static, ()>>>,
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
    pub fn listed<Q>(mut self, list: impl Fn(A) -> Q + Send + Sync + 'static) -> Self
    where Q: TryFuture<Ok = Vec<Resource>, Error: Into<ErrorData>> + Send + 'static {
        self.lists.push(Box::new(move |app| list(app).map_err(Into::into).boxed()));
        self
    }

    pub fn resource<R, S>(self, resource: Resource, read: impl Fn(A, ()) -> R + Send + Sync + 'static, watch: impl Fn(A, ()) -> S + Send + Sync + 'static) -> Result<Self, Invalid>
    where
        R: Future<Output: IntoResourceContents> + Send + 'static,
        S: Stream<Item = ()> + Send + 'static,
    {
        let (uri, mime) = (resource.uri.clone(), resource.mime_type.clone());
        self.route(uri, mime, read, watch).map(|router| router.listed(move |_| ready(Ok::<_, ErrorData>(vec![resource.clone()]))))
    }

    pub fn template<P, R, S>(mut self, template: ResourceTemplate, read: impl Fn(A, P) -> R + Send + Sync + 'static, watch: impl Fn(A, P) -> S + Send + Sync + 'static) -> Result<Self, Invalid>
    where
        P: DeserializeOwned + 'static,
        R: Future<Output: IntoResourceContents> + Send + 'static,
        S: Stream<Item = ()> + Send + 'static,
    {
        let (pattern, mime) = (template.uri_template.clone(), template.mime_type.clone());
        self.templates.push(template);
        self.route(pattern, mime, read, watch)
    }

    pub fn executions(self, resource: Resource, changes: &Changes<A>) -> Result<Self, Invalid> {
        let (rows, executed) = (Arc::clone(&changes.executions), changes.executed.clone());
        let read = move |_, ()| ready(serde_json::to_string(&rows.lock().values().collect::<Vec<_>>()).map_err(internal));
        self.resource(resource.with_mime_type(JSON_MIME_TYPE), read, move |_, ()| WatchStream::from_changes(executed.subscribe()))
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

    fn route<P, R, S>(mut self, pattern: String, mime: Option<String>, read: impl Fn(A, P) -> R + Send + Sync + 'static, watch: impl Fn(A, P) -> S + Send + Sync + 'static) -> Result<Self, Invalid>
    where
        P: DeserializeOwned + 'static,
        R: Future<Output: IntoResourceContents> + Send + 'static,
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
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Serialize, thiserror::Error)]
#[serde(tag = "_tag")]
pub enum Unanswered {
    #[error("user declined to answer, continue without answer")]
    Declined,
    #[error("user dismissed question, call again to ask again")]
    Dismissed,
    #[error("user answer is empty, call again to ask again")]
    Empty,
    #[error("client cannot ask user, ask in conversation and pass answer as arguments")]
    Unsupported,
    #[error("answer breaks requested form, call again to ask again")]
    Malformed {
        data: Value,
        #[serde(skip)]
        source: serde_path_to_error::Error<serde_json::Error>,
    },
    #[error("requested form has no object schema, tool cannot ask question")]
    Unaskable {
        #[serde(skip)]
        source: serde_json::Error,
    },
    #[error("call ended before user answered, call again")]
    Abandoned,
    #[error("question did not reach user, call again")]
    Unreached {
        #[serde(skip)]
        source: ServiceError,
    },
}

// --- [SERVICES] ------------------------------------------------------------------------

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
        #[error("arguments break tool input schema, correct each violation at its path and call again")]
        struct InvalidArguments {
            violations: NEVec<(Location, String)>,
        }
        let validated = router
            .into_iter()
            .map(|ToolRoute { call, attr, .. }| {
                validator_for(&attr.schema_as_json_value()).map_err(|source| Invalid::Schema { tool: attr.name.clone(), source }).map(|validator| {
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

    pub fn updated(&self, uri: impl Into<String>) { self.notifier.notify(ResourceUpdatedNotification::new(ResourceUpdatedNotificationParam::new(uri.into()))); }
}

#[derive(Clone, Debug)]
pub struct Elicitation {
    context: RequestContext<RoleServer>,
    asked: mpsc::UnboundedSender<(InputRequiredResult, oneshot::Sender<InputResponses>)>,
}

impl Elicitation {
    pub async fn elicit<T>(&self, message: String) -> Result<T, Unanswered>
    where T: schemars::JsonSchema + DeserializeOwned {
        self.context.client_capabilities().and_then(|capabilities| capabilities.elicitation).is_some_and(|modes| modes.form.is_some() || modes.url.is_none()).ok_or(Unanswered::Unsupported)?;
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
    pub fn new<P>(instructions: &str, build: impl AsyncFnOnce(P, Changes<A>) -> Result<A, A::Error>) -> impl AsyncFnOnce(P, Implementation, Notifier) -> Result<Self, A::Error> {
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
                    Running { call: call.inspect(move |_| drop(released)).boxed(), questions, progress, execution, destructive }
                };
                tokio::select! {
                    biased;
                    Some((mut required, answer)) = running.questions.recv() => {
                        running.execution.since = Instant::now();
                        self.parked.lock().insert(required.request_state.insert(uuid::Uuid::new_v4().to_string()).clone(), (running, answer));
                        Ok(CallToolResponse::from(required).into())
                    }
                    completed = running.progress.relay(&context, &mut running.call) => completed.map(ServerResult::from),
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
