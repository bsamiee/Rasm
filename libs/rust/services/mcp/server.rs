use core::{future::ready, iter, mem, num::NonZero, pin::pin, time::Duration};
use std::{
    collections::BTreeMap,
    sync::{Arc, nonpoison::Mutex},
};

use arc_swap::ArcSwapOption;
use derive_where::derive_where;
use futures::{
    FutureExt as _, StreamExt as _, TryFuture, TryFutureExt as _,
    future::{self, BoxFuture},
};
use itertools::{Either, Itertools as _};
use rasm_daemon::Lifetime;
use rasm_telemetry::Handle;
use rmcp::{
    ErrorData, RoleServer, ServerHandler,
    handler::server::{prompt::PromptContext, router::prompt::PromptRouter, tool::ToolRouter},
    model::{
        ArgumentInfo, CallToolRequestParams, CallToolResponse, CancelTaskParams, CompleteRequestMethod, CompleteRequestParams, CompleteResult, CompletionContext, CompletionInfo, Cursor,
        GetPromptRequestMethod, GetPromptRequestParams, GetPromptResponse, GetTaskParams, GetTaskResult, ListPromptsResult, ListResourceTemplatesResult, ListResourcesResult, ListToolsResult,
        PaginatedRequestParams, PromptListChangedNotification, PromptReference, ReadResourceRequestMethod, ReadResourceRequestParams, ReadResourceResponse, ReadResourceResult, Reference,
        ResourceListChangedNotification, ResourceTemplateReference, ResourceUpdatedNotification, ResourceUpdatedNotificationParam, ServerCapabilities, ServerConfig, ServerNotification, Tool,
        ToolListChangedNotification, UpdateTaskParams,
    },
    service::RequestContext,
    task_manager::TaskManager,
};
use tokio::sync::{broadcast, watch};
use tokio_util::{sync::CancellationToken, task::TaskTracker};
use validated::Validated;

use self::{
    resources::{ResourceRouter, Resources},
    tools::{Ledger, Parked},
};
use crate::{Endpoint, Failure, McpError, host::Host, internal};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait App: Clone + Send + Sync + 'static {
    type Error: Failure + From<McpError> = McpError;

    fn serve(self, _: Registry<Self>, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send { stopping.cancelled_owned().map(Ok) }
}

trait Completer<A> = Fn(A, ArgumentInfo, Option<CompletionContext>) -> BoxFuture<'static, Result<Vec<String>, ErrorData>> + Send + Sync;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Settings {
    pub instructions: Option<String> = None,
    pub page: NonZero<usize> = NonZero::new(1024).unwrap(),
    pub retention: Duration = Duration::from_hours(1),
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive_where(Debug, Default)]
pub struct CompletionRouter<A> {
    #[derive_where(skip(Debug))]
    prompts: BTreeMap<String, Box<dyn Completer<A>>>,
}

impl<A> CompletionRouter<A>
where A: 'static
{
    #[must_use]
    pub fn prompt<Q>(mut self, name: impl Into<String>, complete: impl Fn(A, ArgumentInfo, Option<CompletionContext>) -> Q + Send + Sync + 'static) -> Self
    where Q: TryFuture<Ok = Vec<String>, Error: Into<ErrorData>> + Send + 'static {
        self.prompts.insert(name.into(), completer(complete));
        self
    }
}

#[derive_where(Clone, Debug)]
pub struct Registry<A> {
    notifier: broadcast::Sender<ServerNotification>,
    ledger: Ledger,
    #[derive_where(skip(Debug))]
    tools: Arc<ArcSwapOption<ToolRouter<A>>>,
    #[derive_where(skip(Debug))]
    prompts: Arc<ArcSwapOption<PromptRouter<A>>>,
    #[derive_where(skip(Debug))]
    resources: Arc<ArcSwapOption<Resources<A>>>,
    #[derive_where(skip(Debug))]
    completions: Arc<ArcSwapOption<CompletionRouter<A>>>,
}

impl<A> Registry<A>
where A: App
{
    pub fn tools(&self, mut router: ToolRouter<A>) -> Result<(), McpError> {
        router.map = mem::take(&mut router.map).into_values().map(tools::validated).collect::<Validated<_, _>>().ok().map_err(|errors| McpError::Declarations { errors })?;
        router.clear_notifier();
        self.tools.store(Some(Arc::new(router)));
        drop(self.notifier.send(ToolListChangedNotification::default().into()));
        Ok(())
    }

    pub fn enable(&self, tool: &str) {
        let previous = self.tools.rcu(|held| {
            held.as_deref().cloned().map(|mut router| {
                router.enable_route(tool);
                Arc::new(router)
            })
        });
        if previous.as_deref().is_some_and(|router| router.is_disabled(tool)) {
            drop(self.notifier.send(ToolListChangedNotification::default().into()));
        }
    }

    pub fn disable(&self, tool: &str) {
        let previous = self.tools.rcu(|held| {
            held.as_deref().cloned().map(|mut router| {
                router.disable_route(tool.to_owned());
                Arc::new(router)
            })
        });
        if previous.as_deref().is_some_and(|router| router.has_route(tool)) {
            drop(self.notifier.send(ToolListChangedNotification::default().into()));
        }
    }

    pub fn prompts(&self, router: PromptRouter<A>) {
        self.prompts.store(Some(Arc::new(router)));
        drop(self.notifier.send(PromptListChangedNotification::default().into()));
    }

    pub fn resources(&self, router: ResourceRouter<A>) -> Result<(), McpError> {
        self.resources.store(Some(Arc::new(Resources::try_from(router)?)));
        self.resources_list_changed();
        Ok(())
    }

    pub fn completions(&self, router: CompletionRouter<A>) -> Result<(), McpError> {
        let prompts = self.prompts.load();
        router
            .prompts
            .keys()
            .map(|prompt| prompts.as_deref().is_some_and(|declared| declared.has_route(prompt)).ok_or_else(|| McpError::Completion { prompt: prompt.clone() }))
            .collect::<Validated<(), _>>()
            .ok()
            .map_err(|errors| McpError::Declarations { errors })?;
        self.completions.store(Some(Arc::new(router)));
        Ok(())
    }

    pub fn resources_list_changed(&self) { drop(self.notifier.send(ResourceListChangedNotification::default().into())); }

    pub fn resource_updated(&self, uri: impl Into<String>) { drop(self.notifier.send(ResourceUpdatedNotification::new(ResourceUpdatedNotificationParam::new(uri.into())).into())); }
}

#[derive_where(Debug)]
pub struct Server<A> {
    #[derive_where(skip)]
    app: A,
    info: ServerConfig,
    registry: Registry<A>,
    page: NonZero<usize>,
    retention: Duration,
    #[derive_where(skip)]
    parked: Arc<Mutex<Parked>>,
    tracker: TaskTracker,
    halted: CancellationToken,
    #[derive_where(skip)]
    tasks: TaskManager,
}

impl<A> Server<A>
where A: App
{
    pub fn new<P>(settings: Settings, build: impl AsyncFnOnce(P, Registry<A>, Handle) -> Result<A, A::Error>) -> impl AsyncFnOnce(P, Host) -> Result<Self, A::Error> {
        let Settings { instructions, page, retention } = settings;
        async move |args: P, Host { identity, filter, notifier }: Host| {
            let registry = Registry { notifier, ledger: watch::Sender::default(), tools: Arc::default(), prompts: Arc::default(), resources: Arc::default(), completions: Arc::default() };
            let app = build(args, registry.clone(), filter).await?;
            let (tools, resources) = (registry.tools.load().is_some(), registry.resources.load().is_some());
            let mut capabilities = ServerCapabilities::builder()
                .enable_completions()
                .enable_prompts()
                .enable_prompts_list_changed()
                .enable_resources()
                .enable_resources_list_changed()
                .enable_resources_subscribe()
                .enable_tools()
                .enable_tool_list_changed();
            capabilities.completions = capabilities.completions.filter(|_| registry.completions.load().is_some() || resources);
            capabilities.tools = capabilities.tools.filter(|_| tools);
            capabilities.prompts = capabilities.prompts.filter(|_| registry.prompts.load().is_some());
            capabilities.resources = capabilities.resources.filter(|_| resources);
            let capabilities = if tools { capabilities.enable_tasks() } else { capabilities }.build();
            let info = instructions.into_iter().fold(ServerConfig::new(capabilities).with_server_info(identity), ServerConfig::with_instructions);
            Ok(Self { app, info, registry, page, retention, parked: Arc::default(), tracker: TaskTracker::new(), halted: CancellationToken::new(), tasks: TaskManager::default() })
        }
    }
}

impl<A> ServerHandler for Server<A>
where A: App
{
    fn get_info(&self) -> ServerConfig { self.info.clone() }

    async fn complete(&self, request: CompleteRequestParams, _: RequestContext<RoleServer>) -> Result<CompleteResult, ErrorData> {
        self.info.capabilities.completions.is_some().ok_or_else(ErrorData::method_not_found::<CompleteRequestMethod>)?;
        let (completions, resources) = (self.registry.completions.load_full(), self.registry.resources.load_full());
        let completer = match &request.r#ref {
            Reference::Prompt(PromptReference { name, .. }) => completions.as_deref().and_then(|router| router.prompts.get(name)),
            Reference::Resource(ResourceTemplateReference { uri, .. }) => resources.as_deref().and_then(|resources| {
                resources.templates.binary_search_by(|(template, _)| template.uri_template.as_str().cmp(uri)).ok().and_then(|index| resources.templates.get(index)).map(|(_, complete)| complete)
            }),
            _ => None,
        };
        let values = match completer {
            Some(complete) => complete(self.app.clone(), request.argument, request.context).await?,
            None => Vec::new(),
        };
        let total = values.len();
        CompletionInfo::with_pagination(values.into_iter().take(CompletionInfo::MAX_VALUES).collect(), u32::try_from(total).ok(), total > CompletionInfo::MAX_VALUES)
            .map(CompleteResult::new)
            .map_err(|message| ErrorData::internal_error(message, None))
    }

    fn get_tool(&self, name: &str) -> Option<Tool> { self.registry.tools.load().as_deref().and_then(|router| router.get(name)).cloned() }

    async fn call_tool(&self, request: CallToolRequestParams, context: RequestContext<RoleServer>) -> Result<CallToolResponse, ErrorData> { tools::call(self, request, context).await }

    fn list_tools(&self, request: Option<PaginatedRequestParams>, _: RequestContext<RoleServer>) -> impl Future<Output = Result<ListToolsResult, ErrorData>> + Send {
        let tools = self.registry.tools.load().as_deref().map_or_default(ToolRouter::list_all);
        let skipped = following(&tools, request.and_then(|params| params.cursor).as_deref(), |tool| &tool.name);
        let (tools, next_cursor) = paged(tools.into_iter().skip(skipped), self.page, |tool| &tool.name);
        ready(Ok(ListToolsResult { next_cursor, ..ListToolsResult::with_all_items(tools) }))
    }

    async fn get_prompt(&self, request: GetPromptRequestParams, context: RequestContext<RoleServer>) -> Result<GetPromptResponse, ErrorData> {
        let prompts = self.registry.prompts.load_full().ok_or_else(ErrorData::method_not_found::<GetPromptRequestMethod>)?;
        prompts.get_prompt(PromptContext::new(&self.app, request.name, request.arguments, context)).await
    }

    fn list_prompts(&self, request: Option<PaginatedRequestParams>, _: RequestContext<RoleServer>) -> impl Future<Output = Result<ListPromptsResult, ErrorData>> + Send {
        let prompts = self.registry.prompts.load().as_deref().map_or_default(PromptRouter::list_all);
        let skipped = following(&prompts, request.and_then(|params| params.cursor).as_deref(), |prompt| &prompt.name);
        let (prompts, next_cursor) = paged(prompts.into_iter().skip(skipped), self.page, |prompt| &prompt.name);
        ready(Ok(ListPromptsResult { next_cursor, ..ListPromptsResult::with_all_items(prompts) }))
    }

    async fn list_resources(&self, request: Option<PaginatedRequestParams>, _: RequestContext<RoleServer>) -> Result<ListResourcesResult, ErrorData> {
        let (registered, after) = (self.registry.resources.load_full(), request.and_then(|params| params.cursor));
        let limit = self.page.saturating_add(1);
        let listed =
            future::join_all(registered.iter().flat_map(|resources| &resources.lists).map(|list| list(self.app.clone(), after.clone(), limit))).await.into_iter().collect::<Validated<Vec<_>, _>>();
        let listed = listed.ok().map_err(|errors| serde_json::to_value(&errors).map_or_else(internal, |data| ErrorData { data: Some(data), ..errors.first().clone() }))?;
        let declared = registered
            .as_deref()
            .into_iter()
            .flat_map(|resources| resources.declared.iter().skip(following(&resources.declared, after.as_deref(), |(resource, _)| &resource.uri)).map(|(resource, _)| resource.clone()));
        let rows = iter::once(Either::Left(declared))
            .chain(listed.into_iter().map(|rows| {
                Either::Right(rows.into_iter().filter(|resource| after.as_deref().is_none_or(|after| resource.uri.as_str() > after)).sorted_unstable_by(|left, right| left.uri.cmp(&right.uri)))
            }))
            .kmerge_by(|left, right| left.uri < right.uri)
            .dedup_by(|left, right| left.uri == right.uri);
        let (resources, next_cursor) = paged(rows, self.page, |resource| &resource.uri);
        Ok(ListResourcesResult { next_cursor, ..ListResourcesResult::with_all_items(resources) })
    }

    fn list_resource_templates(&self, request: Option<PaginatedRequestParams>, _: RequestContext<RoleServer>) -> impl Future<Output = Result<ListResourceTemplatesResult, ErrorData>> + Send {
        let registered = self.registry.resources.load();
        let templates = registered.as_deref().map_or(&[][..], |resources| &resources.templates);
        let skipped = following(templates, request.and_then(|params| params.cursor).as_deref(), |(template, _)| &template.uri_template);
        let (templates, next_cursor) = paged(templates.iter().skip(skipped).map(|(template, _)| template.clone()), self.page, |template| &template.uri_template);
        ready(Ok(ListResourceTemplatesResult { next_cursor, ..ListResourceTemplatesResult::with_all_items(templates) }))
    }

    async fn read_resource(&self, request: ReadResourceRequestParams, _: RequestContext<RoleServer>) -> Result<ReadResourceResponse, ErrorData> {
        let resources = self.registry.resources.load_full().ok_or_else(ErrorData::method_not_found::<ReadResourceRequestMethod>)?;
        let reading = resources.routed(&request.uri).and_then(|(route, variables)| (route.read)(self.app.clone(), request.uri, variables))?;
        Ok(ReadResourceResult::new(reading.await?).into())
    }

    fn get_task(&self, request: GetTaskParams, _: RequestContext<RoleServer>) -> impl Future<Output = Result<GetTaskResult, ErrorData>> + Send {
        ready(self.tasks.get_task(&request.task_id).map(GetTaskResult::new))
    }

    fn update_task(&self, request: UpdateTaskParams, _: RequestContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + Send {
        ready(self.tasks.update_task(&request.task_id, request.input_responses))
    }

    fn cancel_task(&self, request: CancelTaskParams, _: RequestContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + Send { ready(self.tasks.cancel_task(&request.task_id)) }
}

impl<A> Endpoint for Server<A>
where A: App
{
    fn watch(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, McpError> {
        let resources = self.registry.resources.load_full().ok_or_else(|| McpError::ResourceNotFound { uri: uri.to_owned() })?;
        let watching = resources.routed(uri).and_then(|(route, variables)| (route.watch)(self.app.clone(), uri.to_owned(), variables))?;
        let (registry, uri) = (self.registry.clone(), uri.to_owned());
        Ok(watching.map(move |()| registry.resource_updated(&uri)).collect())
    }
}

impl<A> Lifetime for Server<A>
where A: App
{
    type Error = A::Error;

    fn lifetime(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), A::Error>> + Send {
        let (tracker, halted, parked) = (self.tracker.clone(), self.halted.clone(), Arc::clone(&self.parked));
        let served = self.app.clone().serve(self.registry.clone(), stopping.clone());
        let drained = async move {
            stopping.cancelled().await;
            halted.cancel();
            drop(parked.with_mut(mem::take));
            tracker.close();
            tracker.wait().await;
        };
        async move {
            match future::select(pin!(served), pin!(drained)).await {
                future::Either::Left((Err(error), _)) => Err(error),
                future::Either::Left((Ok(()), drained)) => drained.map(Ok).await,
                future::Either::Right(((), served)) => served.await,
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn completer<A, Q>(complete: impl Fn(A, ArgumentInfo, Option<CompletionContext>) -> Q + Send + Sync + 'static) -> Box<dyn Completer<A>>
where Q: TryFuture<Ok = Vec<String>, Error: Into<ErrorData>> + Send + 'static {
    Box::new(move |app, argument, context| complete(app, argument, context).map_err(Into::into).boxed())
}

fn following<T>(rows: &[T], cursor: Option<&str>, key: impl Fn(&T) -> &str) -> usize { cursor.map_or_default(|cursor| rows.partition_point(|row| key(row) <= cursor)) }

fn paged<T>(rows: impl IntoIterator<Item = T>, page: NonZero<usize>, key: impl Fn(&T) -> &str) -> (Vec<T>, Option<Cursor>) {
    let mut rows = rows.into_iter().peekable();
    let kept = rows.by_ref().take(page.get()).collect_vec();
    let next_cursor = rows.peek().and_then(|_| kept.last()).map(|row| key(row).to_owned());
    (kept, next_cursor)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod resources;
pub mod tools;
