#![expect(deprecated, reason = "the initialize-era revisions keep resources/subscribe and logging, which SEP-2577 deprecates")]

use core::future::ready;
use std::borrow::Cow;
use std::collections::{BTreeMap, HashSet};
use std::error::Report;
use std::sync::nonpoison::Mutex;
use std::sync::{Arc, OnceLock, Weak};

use futures::{FutureExt as _, Stream, StreamExt as _, TryFutureExt as _, TryStreamExt as _, stream};
use itertools::Either;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::model::{
    CallToolRequestParams, CallToolResponse, CancelTaskParams, CancelledNotificationParam, CompleteRequestParams, CompleteResult, CreateTaskResult, CustomNotification, CustomRequest, CustomResult,
    DiscoverResult, GetPromptRequestParams, GetPromptResponse, GetSkillRequestParams, GetSkillResult, GetTaskParams, GetTaskResult, InitializeRequestParams, InitializeResult, ListPromptsResult,
    ListResourceTemplatesResult, ListResourcesResult, ListSkillsResult, ListToolsResult, LoggingLevel, LoggingMessageNotificationParam, PaginatedRequestParams, ProgressNotificationParam,
    PromptListChangedNotification, ProtocolVersion, ReadResourceDirectoryRequestParams, ReadResourceDirectoryResult, ReadResourceRequestParams, ReadResourceResponse, ResourceListChangedNotification,
    ResourceUpdatedNotification, ResourceUpdatedNotificationParam, ResourcesCapability, ServerCapabilities, ServerConfig, ServerNotification, SetLevelRequestMethod, SetLevelRequestParams,
    SubscribeRequestMethod, SubscribeRequestParams, SubscriptionFilter, Task, Tool, ToolListChangedNotification, UnsubscribeRequestParams, UpdateTaskParams,
};
use rmcp::service::{NotificationContext, RequestContext, SubscriptionContext};
use rmcp::{ErrorData, RoleServer, ServerHandler, ServiceError};
use tokio::sync::{broadcast, watch};
use tokio_stream::wrappers::BroadcastStream;
use tokio_stream::wrappers::errors::BroadcastStreamRecvError;
use tokio_util::sync::CancellationToken;
use tokio_util::task::task_tracker::TaskTrackerToken;
use tokio_util::task::{AbortOnDropHandle, TaskTracker};
use tracing::Instrument as _;

use crate::{McpError, Subscribe};

// --- [TYPES] ---------------------------------------------------------------------------

type Held = BTreeMap<String, Arc<AbortOnDropHandle<()>>>;

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug, Default)]
pub struct Log(Arc<Mutex<Option<LoggingLevel>>>);

impl Log {
    pub async fn send(&self, context: &RequestContext<RoleServer>, message: LoggingMessageNotificationParam) -> Result<(), ServiceError> {
        let initialized = context.protocol_version().is_none_or(|version| version.has_initialize());
        let floor = if initialized { self.0.get_cloned() } else { context.meta.log_level() };
        if floor.map_or(initialized, |floor| message.level as u8 >= floor as u8) { context.peer.notify_logging_message(message).await } else { Ok(()) }
    }
}

#[derive(Debug)]
pub struct Shared<S> {
    service: S,
    notifier: broadcast::Sender<ServerNotification>,
    stopping: CancellationToken,
    requests: TaskTracker,
    subscriptions: TaskTracker,
    subscribers: Mutex<BTreeMap<String, Weak<AbortOnDropHandle<()>>>>,
}

impl<S> Shared<S>
where S: Subscribe
{
    pub async fn new<X>(capacity: usize, build: impl AsyncFnOnce(broadcast::Sender<ServerNotification>) -> Result<S, X>) -> Result<Arc<Self>, X> {
        let notifier = broadcast::Sender::new(capacity);
        let service = build(notifier.clone()).await?;
        Ok(Arc::new(Self { service, notifier, stopping: CancellationToken::new(), requests: TaskTracker::new(), subscriptions: TaskTracker::new(), subscribers: Mutex::default() }))
    }

    pub const fn service(&self) -> &S { &self.service }

    #[must_use]
    pub fn connection(self: &Arc<Self>) -> Connection<S> { Connection { subscriptions: self.subscriptions(), tasks: Mutex::default(), log: Log::default() } }

    pub(crate) fn subscriptions(self: &Arc<Self>) -> Subscriptions<S> {
        Subscriptions { shared: Arc::clone(self), subscribed: watch::Sender::default(), accepted: Mutex::default(), broadcast: OnceLock::new() }
    }

    pub async fn drain(&self) {
        self.requests.close();
        self.requests.wait().await;
        self.stopping.cancel();
        self.subscriptions.close();
        self.subscriptions.wait().await;
    }

    fn subscribe(&self, uri: &str) -> Result<Arc<AbortOnDropHandle<()>>, McpError> {
        self.subscribers.with_mut(|subscribers| {
            subscribers.get(uri).and_then(Weak::upgrade).filter(|held| !held.is_finished()).map_or_else(
                || {
                    let subscribed = self.stopping.clone().run_until_cancelled_owned(self.service.subscribe(uri)?).map(drop).instrument(tracing::info_span!(parent: None, "subscription", uri));
                    let held = Arc::new(AbortOnDropHandle::new(self.subscriptions.spawn(subscribed)));
                    subscribers.retain(|_, other| other.strong_count() > 0);
                    subscribers.insert(uri.to_owned(), Arc::downgrade(&held));
                    Ok(held)
                },
                Ok,
            )
        })
    }

    fn subscribed<'uri>(&self, uris: impl IntoIterator<Item = &'uri String>) -> Held {
        uris.into_iter()
            .filter_map(|uri| self.subscribe(uri).map(|held| (uri.clone(), held)).inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "subscription refused")).ok())
            .collect()
    }

    fn listen(&self, filter: SubscriptionFilter, held: watch::Receiver<Held>) -> impl Stream<Item = ServerNotification> + Send + 'static {
        BroadcastStream::new(self.notifier.subscribe()).flat_map(move |received| {
            let selected = match received {
                Ok(ServerNotification::ResourceUpdatedNotification(updated)) if held.borrow().contains_key(&updated.params.uri) => Either::Left(Some(updated.into())),
                Ok(notification @ ServerNotification::ToolListChangedNotification(_)) if filter.tools_list_changed == Some(true) => Either::Left(Some(notification)),
                Ok(notification @ ServerNotification::PromptListChangedNotification(_)) if filter.prompts_list_changed == Some(true) => Either::Left(Some(notification)),
                Ok(notification @ ServerNotification::ResourceListChangedNotification(_)) if filter.resources_list_changed == Some(true) => Either::Left(Some(notification)),
                Ok(_) => Either::Left(None),
                Err(BroadcastStreamRecvError::Lagged(_)) => Either::Right(resynced(&filter, Vec::from_iter(held.borrow().keys().cloned()))),
            };
            stream::iter(selected.map_left(Option::into_iter))
        })
    }
}

#[derive(Debug)]
pub(crate) struct Subscriptions<S> {
    shared: Arc<Shared<S>>,
    subscribed: watch::Sender<Held>,
    accepted: Mutex<Held>,
    broadcast: OnceLock<AbortOnDropHandle<()>>,
}

impl<S> Subscriptions<S> {
    pub(crate) fn service(&self) -> &S { &self.shared.service }
}

impl<S> ServerHandler for Subscriptions<S>
where S: Subscribe
{
    fn get_info(&self) -> ServerConfig { self.shared.service.get_info() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { self.shared.service.supported_protocol_versions() }

    fn subscribe(&self, request: SubscribeRequestParams, _: RequestContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + Send + '_ {
        ready(match ServerHandler::get_info(self).capabilities.resources {
            Some(ResourcesCapability { subscribe: Some(true), .. }) =>
                self.shared.subscribe(&request.uri).map(|held| self.subscribed.send_modify(|uris| drop(uris.insert(request.uri, held)))).map_err(ErrorData::from),
            Some(_) | None => Err(ErrorData::method_not_found::<SubscribeRequestMethod>()),
        })
    }

    fn unsubscribe(&self, request: UnsubscribeRequestParams, _: RequestContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + Send + '_ {
        self.subscribed.send_if_modified(|uris| uris.remove(&request.uri).is_some());
        ready(Ok(()))
    }

    fn accepted_subscription_filter(&self, requested: &SubscriptionFilter) -> Option<SubscriptionFilter> {
        let mut accepted = requested.supported_by(&ServerHandler::get_info(self).capabilities);
        accepted.resource_subscriptions = accepted.resource_subscriptions.take().map(|uris| {
            let held = self.shared.subscribed(&uris);
            let kept = Vec::from_iter(held.keys().cloned());
            self.accepted.with_mut(|accepted| accepted.extend(held));
            kept
        });
        Some(accepted)
    }

    async fn listen(&self, context: SubscriptionContext) -> Result<(), ErrorData> {
        let held = self.shared.subscribed(context.accepted().resource_subscriptions.iter().flatten());
        self.accepted.with_mut(|accepted| accepted.retain(|uri, _| !held.contains_key(uri)));
        let notifications = self.shared.listen(context.accepted().clone(), watch::channel(held).1).take_until(self.shared.stopping.cancelled()).take_until(context.cancelled());
        let sent = notifications.map(Ok).try_for_each(|notification| context.sink().send(notification));
        sent.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "subscription closed")).await;
        Ok(())
    }

    fn on_initialized(&self, context: NotificationContext<RoleServer>) -> impl Future<Output = ()> + Send + '_ {
        self.broadcast.get_or_init(|| {
            let notifications = self.shared.listen(list_changes(&ServerHandler::get_info(self).capabilities), self.subscribed.subscribe());
            let sent = async move { notifications.map(Ok).try_for_each(|notification| context.peer.send_notification(notification)).await };
            AbortOnDropHandle::new(tokio::spawn(sent.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "connection closed"))))
        });
        ready(())
    }
}

#[derive(Debug)]
pub struct Connection<S> {
    subscriptions: Subscriptions<S>,
    tasks: Mutex<HashSet<String>>,
    log: Log,
}

impl<S> Connection<S>
where S: ServerHandler + Subscribe
{
    fn admitted(&self, context: &mut RequestContext<RoleServer>) -> Result<TaskTrackerToken, ErrorData> {
        let requests = &self.subscriptions.shared.requests;
        let held = requests.token();
        context.extensions.insert(self.log.clone());
        (!requests.is_closed()).then_some(held).ok_or_else(|| McpError::Stopping.into())
    }

    fn owned(&self, task: &str) -> Result<(), ErrorData> { self.tasks.lock().contains(task).ok_or_else(|| McpError::Task { task: task.to_owned() }.into()) }
}

macro_rules! served {
    ($($method:ident($request:ty $(, $task:ident)?) -> $result:ty;)*) => {$(
        async fn $method(&self, request: $request, mut context: RequestContext<RoleServer>) -> Result<$result, ErrorData> {
            let _held = self.admitted(&mut context)?;
            $(self.owned(&request.$task)?;)?
            self.subscriptions.shared.service.$method(request, context).await
        }
    )*};
}

impl<S> ServerHandler for Connection<S>
where S: ServerHandler + Subscribe
{
    served! {
        initialize(InitializeRequestParams) -> InitializeResult;
        complete(CompleteRequestParams) -> CompleteResult;
        get_prompt(GetPromptRequestParams) -> GetPromptResponse;
        list_prompts(Option<PaginatedRequestParams>) -> ListPromptsResult;
        list_resources(Option<PaginatedRequestParams>) -> ListResourcesResult;
        list_resource_templates(Option<PaginatedRequestParams>) -> ListResourceTemplatesResult;
        read_resource(ReadResourceRequestParams) -> ReadResourceResponse;
        list_tools(Option<PaginatedRequestParams>) -> ListToolsResult;
        on_custom_request(CustomRequest) -> CustomResult;
        get_task(GetTaskParams, task_id) -> GetTaskResult;
        update_task(UpdateTaskParams, task_id) -> ();
        cancel_task(CancelTaskParams, task_id) -> ();
        list_skills(Option<PaginatedRequestParams>) -> ListSkillsResult;
        get_skill(GetSkillRequestParams) -> GetSkillResult;
        read_resource_directory(ReadResourceDirectoryRequestParams) -> ReadResourceDirectoryResult;
    }

    fn get_info(&self) -> ServerConfig { ServerHandler::get_info(&self.subscriptions.shared.service) }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { ServerHandler::supported_protocol_versions(&self.subscriptions.shared.service) }

    fn negotiate_initialize(&self, request: &InitializeRequestParams) -> Result<InitializeResult, ErrorData> { self.subscriptions.shared.service.negotiate_initialize(request) }

    fn get_tool(&self, name: &str) -> Option<Tool> { self.subscriptions.shared.service.get_tool(name) }

    async fn ping(&self, mut context: RequestContext<RoleServer>) -> Result<(), ErrorData> {
        let _held = self.admitted(&mut context)?;
        self.subscriptions.shared.service.ping(context).await
    }

    async fn discover(&self, mut context: RequestContext<RoleServer>) -> Result<DiscoverResult, ErrorData> {
        let _held = self.admitted(&mut context)?;
        self.subscriptions.shared.service.discover(context).await
    }

    async fn call_tool(&self, request: CallToolRequestParams, mut context: RequestContext<RoleServer>) -> Result<CallToolResponse, ErrorData> {
        let _held = self.admitted(&mut context)?;
        let response = self.subscriptions.shared.service.call_tool(request, context).await?;
        if let CallToolResponse::Task(CreateTaskResult { task: Task { task_id, .. }, .. }) = &response {
            self.tasks.lock().insert(task_id.clone());
        }
        Ok(response)
    }

    fn set_level(&self, request: SetLevelRequestParams, _: RequestContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + Send + '_ {
        ready(ServerHandler::get_info(self).capabilities.logging.map(|_| self.log.0.set(Some(request.level))).ok_or_else(ErrorData::method_not_found::<SetLevelRequestMethod>))
    }

    async fn subscribe(&self, request: SubscribeRequestParams, mut context: RequestContext<RoleServer>) -> Result<(), ErrorData> {
        let _held = self.admitted(&mut context)?;
        self.subscriptions.subscribe(request, context).await
    }

    async fn unsubscribe(&self, request: UnsubscribeRequestParams, mut context: RequestContext<RoleServer>) -> Result<(), ErrorData> {
        let _held = self.admitted(&mut context)?;
        self.subscriptions.unsubscribe(request, context).await
    }

    fn accepted_subscription_filter(&self, requested: &SubscriptionFilter) -> Option<SubscriptionFilter> { self.subscriptions.accepted_subscription_filter(requested) }

    fn listen(&self, context: SubscriptionContext) -> impl Future<Output = Result<(), ErrorData>> + Send + '_ { self.subscriptions.listen(context) }

    async fn on_initialized(&self, context: NotificationContext<RoleServer>) {
        self.subscriptions.on_initialized(context.clone()).await;
        self.subscriptions.shared.service.on_initialized(context).await;
    }

    fn on_cancelled(&self, notification: CancelledNotificationParam, context: NotificationContext<RoleServer>) -> impl Future<Output = ()> + Send + '_ {
        self.subscriptions.shared.service.on_cancelled(notification, context)
    }

    fn on_progress(&self, notification: ProgressNotificationParam, context: NotificationContext<RoleServer>) -> impl Future<Output = ()> + Send + '_ {
        self.subscriptions.shared.service.on_progress(notification, context)
    }

    fn on_roots_list_changed(&self, context: NotificationContext<RoleServer>) -> impl Future<Output = ()> + Send + '_ { self.subscriptions.shared.service.on_roots_list_changed(context) }

    fn on_custom_notification(&self, notification: CustomNotification, context: NotificationContext<RoleServer>) -> impl Future<Output = ()> + Send + '_ {
        self.subscriptions.shared.service.on_custom_notification(notification, context)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn list_changes(capabilities: &ServerCapabilities) -> SubscriptionFilter {
    SubscriptionFilter::builder().tools_list_changed().prompts_list_changed().resources_list_changed().build().supported_by(capabilities)
}

pub(crate) fn resynced<U>(filter: &SubscriptionFilter, uris: U) -> impl Iterator<Item = ServerNotification> + use<U>
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
