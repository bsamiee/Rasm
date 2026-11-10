use core::future::ready;
use std::borrow::Cow;
use std::collections::{BTreeMap, HashSet};
use std::error::Report;
use std::sync::{Arc, OnceLock, Weak};

use futures::{FutureExt as _, Stream, StreamExt as _, TryFutureExt as _, TryStreamExt as _, stream};
use itertools::Either;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::model::{
    CancelTaskRequest, ClientNotification, ClientRequest, CreateTaskResult, GetTaskRequest, PromptListChangedNotification, ProtocolVersion, ResourceListChangedNotification,
    ResourceUpdatedNotification, ResourceUpdatedNotificationParam, ResourcesCapability, ServerCapabilities, ServerConfig, ServerNotification, ServerResult, SubscribeRequestMethod,
    SubscribeRequestParams, SubscriptionFilter, Task, ToolListChangedNotification, UnsubscribeRequestParams, UpdateTaskRequest,
};
use rmcp::service::{NotificationContext, RequestContext, SubscriptionContext};
use rmcp::{ErrorData, RoleServer, ServerHandler, Service};
use tokio::sync::{Mutex, broadcast, watch};
use tokio_stream::wrappers::BroadcastStream;
use tokio_stream::wrappers::errors::BroadcastStreamRecvError;
use tokio_util::sync::CancellationToken;
use tokio_util::task::{AbortOnDropHandle, TaskTracker};
use tracing::Instrument as _;

use crate::{McpError, Subscribe};

// --- [TYPES] ---------------------------------------------------------------------------

type Held = BTreeMap<String, Arc<AbortOnDropHandle<()>>>;

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Shared<S> {
    service: S,
    notifier: broadcast::Sender<ServerNotification>,
    stopping: CancellationToken,
    requests: TaskTracker,
    subscriptions: TaskTracker,
    subscribers: watch::Sender<BTreeMap<String, Weak<AbortOnDropHandle<()>>>>,
}

impl<S> Shared<S>
where S: Subscribe
{
    pub async fn new<X>(capacity: usize, build: impl AsyncFnOnce(broadcast::Sender<ServerNotification>) -> Result<S, X>) -> Result<Arc<Self>, X> {
        let notifier = broadcast::Sender::new(capacity);
        let service = build(notifier.clone()).await?;
        Ok(Arc::new(Self { service, notifier, stopping: CancellationToken::new(), requests: TaskTracker::new(), subscriptions: TaskTracker::new(), subscribers: watch::Sender::default() }))
    }

    pub const fn service(&self) -> &S { &self.service }

    #[must_use]
    pub fn connection(self: &Arc<Self>) -> Connection<S> {
        Connection {
            subscriptions: Subscriptions { shared: Arc::clone(self), subscribed: watch::Sender::default(), accepted: watch::Sender::default() },
            tasks: Mutex::default(),
            broadcast: OnceLock::new(),
        }
    }

    pub async fn drain(&self) {
        self.requests.close();
        self.requests.wait().await;
        self.stopping.cancel();
        self.subscriptions.close();
        self.subscriptions.wait().await;
    }

    fn subscribe(&self, uri: &str) -> Result<Arc<AbortOnDropHandle<()>>, McpError> {
        let mut outcome = Err(McpError::Stopping);
        self.subscribers.send_if_modified(|subscribers| {
            outcome = subscribers.get(uri).and_then(Weak::upgrade).filter(|held| !held.is_finished()).map_or_else(
                || {
                    let subscribed = self.stopping.clone().run_until_cancelled_owned(self.service.subscribe(uri)?).map(drop).instrument(tracing::info_span!(parent: None, "subscription", uri));
                    let held = Arc::new(AbortOnDropHandle::new(self.subscriptions.spawn(subscribed)));
                    subscribers.retain(|_, other| other.strong_count() > 0);
                    subscribers.insert(uri.to_owned(), Arc::downgrade(&held));
                    Ok(held)
                },
                Ok,
            );
            false
        });
        outcome
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
pub struct Connection<S> {
    subscriptions: Subscriptions<S>,
    tasks: Mutex<HashSet<String>>,
    broadcast: OnceLock<AbortOnDropHandle<()>>,
}

impl<S> Service<RoleServer> for Connection<S>
where S: Subscribe
{
    async fn handle_request(&self, request: ClientRequest, context: RequestContext<RoleServer>) -> Result<ServerResult, ErrorData> {
        let shared = &self.subscriptions.shared;
        let _held = shared.requests.token();
        if shared.requests.is_closed() {
            return Err(McpError::Stopping.into());
        }
        let task = match &request {
            ClientRequest::GetTaskRequest(GetTaskRequest { params, .. }) => Some(&params.task_id),
            ClientRequest::UpdateTaskRequest(UpdateTaskRequest { params, .. }) => Some(&params.task_id),
            ClientRequest::CancelTaskRequest(CancelTaskRequest { params, .. }) => Some(&params.task_id),
            _ => None,
        };
        if let Some(task) = task
            && !self.tasks.lock().await.contains(task)
        {
            return Err(McpError::Task { task: task.clone() }.into());
        }
        let result = match request {
            request @ (ClientRequest::InitializeRequest(_)
            | ClientRequest::DiscoverRequest(_)
            | ClientRequest::PingRequest(_)
            | ClientRequest::SubscribeRequest(_)
            | ClientRequest::UnsubscribeRequest(_)
            | ClientRequest::SubscriptionsListenRequest(_)) => self.subscriptions.handle_request(request, context).await,
            request => shared.service.handle_request(request, context).await,
        }?;
        if let ServerResult::CreateTaskResult(CreateTaskResult { task: Task { task_id, .. }, .. }) = &result {
            self.tasks.lock().await.insert(task_id.clone());
        }
        Ok(result)
    }

    async fn handle_notification(&self, notification: ClientNotification, context: NotificationContext<RoleServer>) -> Result<(), ErrorData> {
        if let ClientNotification::InitializedNotification(_) = &notification {
            let shared = &self.subscriptions.shared;
            let (notifications, peer) = (shared.listen(list_changes(&shared.service.get_info().capabilities), self.subscriptions.subscribed.subscribe()), context.peer.clone());
            let sent = async move { notifications.map(Ok).try_for_each(|notification| peer.send_notification(notification)).await };
            self.broadcast.get_or_init(|| AbortOnDropHandle::new(tokio::spawn(sent.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "connection closed")))));
        }
        self.subscriptions.shared.service.handle_notification(notification, context).await
    }

    fn get_info(&self) -> ServerConfig { self.subscriptions.shared.service.get_info() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { self.subscriptions.shared.service.supported_protocol_versions() }
}

#[derive(Debug)]
struct Subscriptions<S> {
    shared: Arc<Shared<S>>,
    subscribed: watch::Sender<Held>,
    accepted: watch::Sender<Held>,
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
            self.accepted.send_modify(|accepted| accepted.extend(held));
            kept
        });
        Some(accepted)
    }

    async fn listen(&self, context: SubscriptionContext) -> Result<(), ErrorData> {
        let held = self.shared.subscribed(context.accepted().resource_subscriptions.iter().flatten());
        self.accepted.send_modify(|accepted| accepted.retain(|uri, _| !held.contains_key(uri)));
        let notifications = self.shared.listen(context.accepted().clone(), watch::channel(held).1).take_until(self.shared.stopping.cancelled()).take_until(context.cancelled());
        let sent = notifications.map(Ok).try_for_each(|notification| context.sink().send(notification));
        sent.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "subscription closed")).await;
        Ok(())
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
