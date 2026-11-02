use core::{future::ready, num::NonZero, time::Duration};
use std::{
    collections::BTreeMap,
    sync::{Arc, nonpoison::Mutex},
};

use arc_swap::ArcSwapOption;
use derive_where::derive_where;
use futures::{FutureExt as _, StreamExt as _, future};
use rmcp::{
    ErrorData, RoleServer, ServerHandler,
    handler::server::{prompt::PromptContext, router::prompt::PromptRouter, tool::ToolRouter},
    model::{
        CallToolRequestParams, CallToolResponse, CancelTaskParams, Cursor, GetPromptRequestMethod, GetPromptRequestParams, GetPromptResponse, GetTaskParams, GetTaskResult, Implementation,
        ListPromptsResult, ListResourceTemplatesResult, ListResourcesResult, ListToolsResult, PaginatedRequestParams, PromptListChangedNotification, ReadResourceRequestMethod,
        ReadResourceRequestParams, ReadResourceResponse, ReadResourceResult, ResourceListChangedNotification, ResourceUpdatedNotification, ResourceUpdatedNotificationParam, ServerCapabilities,
        ServerConfig, Tool, ToolListChangedNotification, UpdateTaskParams,
    },
    service::RequestContext,
    task_manager::TaskManager,
};
use tokio::sync::watch;
use tokio_util::sync::CancellationToken;
use validated::Validated;

use self::{
    resources::{ResourceRouter, Resources},
    tools::{Execution, Running},
};
use crate::{Endpoint, Failure, McpError, Notifier, internal};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait App: Clone + Send + Sync + 'static {
    type Error: Failure + From<McpError> = McpError;

    fn serve(self, _: Changes<Self>, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send { stopping.cancelled_owned().map(Ok) }
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Defaults {
    pub instructions: Option<String> = None,
    pub page: NonZero<usize> = Option::unwrap(NonZero::new(1024)),
    pub retention: Duration = Duration::from_hours(1),
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive_where(Clone, Debug)]
pub struct Changes<A> {
    notifier: Notifier,
    ledger: watch::Sender<BTreeMap<usize, Execution>>,
    #[derive_where(skip(Debug))]
    tools: Arc<ArcSwapOption<ToolRouter<A>>>,
    #[derive_where(skip(Debug))]
    prompts: Arc<ArcSwapOption<PromptRouter<A>>>,
    #[derive_where(skip(Debug))]
    resources: Arc<ArcSwapOption<Resources<A>>>,
}

impl<A> Changes<A>
where A: App
{
    pub fn tools(&self, router: ToolRouter<A>) -> Result<(), McpError> {
        let validated = router.into_iter().map(tools::validated).collect::<Validated<Vec<_>, _>>().ok().map_err(|errors| McpError::Declarations { errors })?;
        self.tools.store(Some(Arc::new(validated.into_iter().fold(ToolRouter::new(), ToolRouter::with_route))));
        self.notifier.notify(ToolListChangedNotification::default());
        Ok(())
    }

    pub fn prompts(&self, router: PromptRouter<A>) {
        self.prompts.store(Some(Arc::new(router)));
        self.notifier.notify(PromptListChangedNotification::default());
    }

    pub fn resources(&self, router: ResourceRouter<A>) -> Result<(), McpError> {
        self.resources.store(Some(Arc::new(Resources::try_from(router)?)));
        self.listed();
        Ok(())
    }

    pub fn listed(&self) { self.notifier.notify(ResourceListChangedNotification::default()); }

    pub fn updated(&self, uri: impl Into<String>) { self.notifier.notify(ResourceUpdatedNotification::new(ResourceUpdatedNotificationParam::new(uri.into()))); }
}

#[derive_where(Debug)]
pub struct Server<A> {
    #[derive_where(skip)]
    app: A,
    info: ServerConfig,
    changes: Changes<A>,
    page: NonZero<usize>,
    retention: Duration,
    #[derive_where(skip)]
    parked: Mutex<BTreeMap<String, Running>>,
    #[derive_where(skip)]
    tasks: TaskManager,
}

impl<A> Server<A>
where A: App
{
    pub fn new<P>(defaults: Defaults, build: impl AsyncFnOnce(P, Changes<A>) -> Result<A, A::Error>) -> impl AsyncFnOnce(P, Implementation, Notifier) -> Result<Self, A::Error> {
        let Defaults { instructions, page, retention } = defaults;
        async move |args: P, identity: Implementation, notifier: Notifier| {
            let changes = Changes { notifier, ledger: watch::Sender::default(), tools: Arc::default(), prompts: Arc::default(), resources: Arc::default() };
            let app = build(args, changes.clone()).await?;
            let mut capabilities = ServerCapabilities::builder()
                .enable_prompts()
                .enable_prompts_list_changed()
                .enable_resources()
                .enable_resources_list_changed()
                .enable_resources_subscribe()
                .enable_tools()
                .enable_tool_list_changed()
                .enable_tasks()
                .build();
            capabilities.tools = capabilities.tools.filter(|_| changes.tools.load().is_some());
            capabilities.extensions = capabilities.extensions.filter(|_| changes.tools.load().is_some());
            capabilities.prompts = capabilities.prompts.filter(|_| changes.prompts.load().is_some());
            capabilities.resources = capabilities.resources.filter(|_| changes.resources.load().is_some());
            let info = instructions.into_iter().fold(ServerConfig::new(capabilities).with_server_info(identity), ServerConfig::with_instructions);
            Ok(Self { app, info, changes, page, retention, parked: Mutex::default(), tasks: TaskManager::default() })
        }
    }
}

impl<A> ServerHandler for Server<A>
where A: App
{
    fn get_info(&self) -> ServerConfig { self.info.clone() }

    fn get_tool(&self, name: &str) -> Option<Tool> { self.changes.tools.load().as_deref().and_then(|router| router.get(name)).cloned() }

    async fn call_tool(&self, request: CallToolRequestParams, context: RequestContext<RoleServer>) -> Result<CallToolResponse, ErrorData> { tools::call(self, request, context).await }

    fn list_tools(&self, request: Option<PaginatedRequestParams>, _: RequestContext<RoleServer>) -> impl Future<Output = Result<ListToolsResult, ErrorData>> + Send {
        let (tools, next_cursor) = paged(self.changes.tools.load().as_deref().map_or_default(ToolRouter::list_all), request, self.page, |tool| &tool.name);
        ready(Ok(ListToolsResult { next_cursor, ..ListToolsResult::with_all_items(tools) }))
    }

    async fn get_prompt(&self, request: GetPromptRequestParams, context: RequestContext<RoleServer>) -> Result<GetPromptResponse, ErrorData> {
        let prompts = self.changes.prompts.load_full().ok_or_else(ErrorData::method_not_found::<GetPromptRequestMethod>)?;
        prompts.get_prompt(PromptContext::new(&self.app, request.name, request.arguments, context)).await
    }

    fn list_prompts(&self, request: Option<PaginatedRequestParams>, _: RequestContext<RoleServer>) -> impl Future<Output = Result<ListPromptsResult, ErrorData>> + Send {
        let (prompts, next_cursor) = paged(self.changes.prompts.load().as_deref().map_or_default(PromptRouter::list_all), request, self.page, |prompt| &prompt.name);
        ready(Ok(ListPromptsResult { next_cursor, ..ListPromptsResult::with_all_items(prompts) }))
    }

    async fn list_resources(&self, request: Option<PaginatedRequestParams>, _: RequestContext<RoleServer>) -> Result<ListResourcesResult, ErrorData> {
        let registered = self.changes.resources.load_full();
        let listed = future::join_all(registered.iter().flat_map(|resources| &resources.lists).map(|list| list(self.app.clone()))).await.into_iter().collect::<Validated<Vec<_>, _>>();
        let rows = listed.ok().map_err(|errors| serde_json::to_value(&errors).map_or_else(internal, |data| ErrorData { data: Some(data), ..errors.first().clone() }))?.concat();
        let (resources, next_cursor) = paged(rows, request, self.page, |resource| &resource.uri);
        Ok(ListResourcesResult { next_cursor, ..ListResourcesResult::with_all_items(resources) })
    }

    fn list_resource_templates(&self, request: Option<PaginatedRequestParams>, _: RequestContext<RoleServer>) -> impl Future<Output = Result<ListResourceTemplatesResult, ErrorData>> + Send {
        let templates = self.changes.resources.load().as_deref().map_or_default(|resources| resources.templates.clone());
        let (templates, next_cursor) = paged(templates, request, self.page, |template| &template.uri_template);
        ready(Ok(ListResourceTemplatesResult { next_cursor, ..ListResourceTemplatesResult::with_all_items(templates) }))
    }

    async fn read_resource(&self, request: ReadResourceRequestParams, _: RequestContext<RoleServer>) -> Result<ReadResourceResponse, ErrorData> {
        let resources = self.changes.resources.load_full().ok_or_else(ErrorData::method_not_found::<ReadResourceRequestMethod>)?;
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
    type Error = A::Error;

    fn watch(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, McpError> {
        let resources = self.changes.resources.load_full().ok_or_else(|| McpError::Unrouted { uri: uri.to_owned() })?;
        let watching = resources.routed(uri).and_then(|(route, variables)| (route.watch)(self.app.clone(), uri.to_owned(), variables))?;
        let (changes, uri) = (self.changes.clone(), uri.to_owned());
        Ok(watching.map(move |()| changes.updated(&uri)).collect())
    }

    fn serve(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), A::Error>> + Send { self.app.clone().serve(self.changes.clone(), stopping) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn paged<T>(mut rows: Vec<T>, params: Option<PaginatedRequestParams>, page: NonZero<usize>, key: impl Fn(&T) -> &str) -> (Vec<T>, Option<Cursor>) {
    rows.sort_by(|left, right| key(left).cmp(key(right)));
    rows.dedup_by(|later, kept| key(later) == key(kept));
    let start = params.and_then(|params| params.cursor).map_or_default(|cursor| rows.partition_point(|row| key(row) <= cursor.as_str()));
    let mut kept = rows.split_off(start);
    let further = kept.len() > page.get();
    kept.truncate(page.get());
    let next_cursor = kept.last().filter(|_| further).map(|row| key(row).to_owned());
    (kept, next_cursor)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod resources;
pub mod tools;
