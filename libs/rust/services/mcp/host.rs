use core::future::ready;
use std::{
    borrow::Cow,
    collections::{BTreeMap, HashSet},
    error::Report,
    sync::{Arc, OnceLock, Weak, nonpoison::Mutex},
};

use futures::{FutureExt as _, Stream, StreamExt as _, TryFutureExt as _, TryStreamExt as _, stream};
use itertools::Either;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::{
    ErrorData, RoleServer, ServerHandler, Service,
    model::{
        CancelTaskRequest, ClientNotification, ClientRequest, CreateTaskResult, ErrorCode, GetTaskRequest, ProtocolVersion, ResourcesCapability, ServerConfig, ServerNotification, ServerResult,
        SubscribeRequestMethod, SubscribeRequestParams, SubscriptionFilter, Task, UnsubscribeRequestParams, UpdateTaskRequest,
    },
    service::{NotificationContext, RequestContext, SubscriptionContext},
};
use tokio::sync::{broadcast, watch};
use tokio_stream::wrappers::{BroadcastStream, errors::BroadcastStreamRecvError};
use tokio_util::{
    sync::CancellationToken,
    task::{AbortOnDropHandle, TaskTracker},
};
use tracing::Instrument as _;

use crate::{Endpoint, McpError, list_changes, resynced};

// --- [TYPES] ---------------------------------------------------------------------------

type Leases = BTreeMap<String, Arc<AbortOnDropHandle<()>>>;

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Host<E> {
    endpoint: E,
    notifier: broadcast::Sender<ServerNotification>,
    stopping: CancellationToken,
    requests: TaskTracker,
    watches: TaskTracker,
    watchers: Mutex<BTreeMap<String, Weak<AbortOnDropHandle<()>>>>,
}

impl<E> Host<E>
where E: Endpoint
{
    pub async fn new<X>(build: impl AsyncFnOnce(broadcast::Sender<ServerNotification>) -> Result<E, X>) -> Result<Arc<Self>, X> {
        const CAPACITY: usize = 64;
        let notifier = broadcast::Sender::new(CAPACITY);
        let endpoint = build(notifier.clone()).await?;
        Ok(Arc::new(Self { endpoint, notifier, stopping: CancellationToken::new(), requests: TaskTracker::new(), watches: TaskTracker::new(), watchers: Mutex::default() }))
    }

    pub const fn endpoint(&self) -> &E { &self.endpoint }

    #[must_use]
    pub fn session(self: &Arc<Self>) -> Session<E> {
        Session { handler: Handler { host: Arc::clone(self), subscribed: watch::Sender::default(), accepted: Mutex::default() }, tasks: Mutex::default(), relay: OnceLock::new() }
    }

    pub async fn drain(&self) {
        self.requests.close();
        self.requests.wait().await;
        self.stopping.cancel();
        self.watches.close();
        self.watches.wait().await;
    }

    #[expect(deprecated, reason = "the MCP attribute names moved to the GenAI semantic-conventions repository, which publishes no Rust crate, with values unchanged")]
    fn lease(&self, uri: &str) -> Result<Arc<AbortOnDropHandle<()>>, McpError> {
        self.watchers.with_mut(|watchers| {
            watchers.get(uri).and_then(Weak::upgrade).filter(|lease| !lease.is_finished()).map_or_else(
                || {
                    let watched = self
                        .stopping
                        .clone()
                        .run_until_cancelled_owned(self.endpoint.watch(uri)?)
                        .map(drop)
                        .instrument(tracing::info_span!(parent: None, "watch", { opentelemetry_semantic_conventions::attribute::MCP_RESOURCE_URI } = uri));
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

    fn notifications(&self, filter: SubscriptionFilter, held: watch::Receiver<Leases>) -> impl Stream<Item = ServerNotification> + Send + 'static {
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
pub struct Session<E> {
    handler: Handler<E>,
    tasks: Mutex<HashSet<String>>,
    relay: OnceLock<AbortOnDropHandle<()>>,
}

impl<E> Service<RoleServer> for Session<E>
where E: Endpoint
{
    async fn handle_request(&self, request: ClientRequest, context: RequestContext<RoleServer>) -> Result<ServerResult, ErrorData> {
        let host = &self.handler.host;
        let _held = host.requests.token();
        if host.requests.is_closed() {
            return Err(McpError::Stopping.into());
        }
        let owned = |task: &str| self.tasks.with_mut(|tasks| tasks.contains(task)).ok_or_else(|| ErrorData::from(McpError::Task { task: task.to_owned() }));
        let modern = context.protocol_version().is_some_and(|version| !version.has_initialize());
        let result = match request {
            request @ (ClientRequest::InitializeRequest(_)
            | ClientRequest::DiscoverRequest(_)
            | ClientRequest::PingRequest(_)
            | ClientRequest::SetLevelRequest(_)
            | ClientRequest::SubscribeRequest(_)
            | ClientRequest::UnsubscribeRequest(_)
            | ClientRequest::SubscriptionsListenRequest(_)) => return self.handler.handle_request(request, context).await,
            ClientRequest::GetTaskRequest(GetTaskRequest { params, .. }) if let Err(refused) = owned(&params.task_id) => Err(refused),
            ClientRequest::UpdateTaskRequest(UpdateTaskRequest { params, .. }) if let Err(refused) = owned(&params.task_id) => Err(refused),
            ClientRequest::CancelTaskRequest(CancelTaskRequest { params, .. }) if let Err(refused) = owned(&params.task_id) => Err(refused),
            request => host.endpoint.handle_request(request, context).await,
        };
        match result {
            Ok(ServerResult::InputRequiredResult(_)) if !modern => Err(McpError::InputRequired.into()),
            Ok(mut result) => {
                if let ServerResult::CreateTaskResult(CreateTaskResult { task: Task { task_id, .. }, .. }) = &result {
                    self.tasks.with_mut(|tasks| tasks.insert(task_id.clone()));
                }
                if modern {
                    result.fill_missing_cache_hints();
                } else {
                    result.strip_result_type_for_legacy_peer();
                }
                Ok(result)
            }
            Err(error) if modern && error.code == ErrorCode::RESOURCE_NOT_FOUND => Err(ErrorData { code: ErrorCode::INVALID_PARAMS, ..error }),
            Err(error) => Err(error),
        }
    }

    async fn handle_notification(&self, notification: ClientNotification, context: NotificationContext<RoleServer>) -> Result<(), ErrorData> {
        if let ClientNotification::InitializedNotification(_) = &notification {
            let host = &self.handler.host;
            let (notifications, peer) = (host.notifications(list_changes(&host.endpoint.get_info().capabilities), self.handler.subscribed.subscribe()), context.peer.clone());
            let relayed = async move { notifications.map(Ok).try_for_each(|notification| peer.send_notification(notification)).await };
            self.relay.get_or_init(|| AbortOnDropHandle::new(tokio::spawn(relayed.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "session closed")))));
        }
        self.handler.host.endpoint.handle_notification(notification, context).await
    }

    fn get_info(&self) -> ServerConfig { self.handler.host.endpoint.get_info() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { self.handler.host.endpoint.supported_protocol_versions() }
}

#[derive(Debug)]
struct Handler<E> {
    host: Arc<Host<E>>,
    subscribed: watch::Sender<Leases>,
    accepted: Mutex<Leases>,
}

impl<E> ServerHandler for Handler<E>
where E: Endpoint
{
    fn get_info(&self) -> ServerConfig { self.host.endpoint.get_info() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { self.host.endpoint.supported_protocol_versions() }

    fn subscribe(&self, request: SubscribeRequestParams, _: RequestContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + Send + '_ {
        ready(match ServerHandler::get_info(self).capabilities.resources {
            Some(ResourcesCapability { subscribe: Some(true), .. }) =>
                self.host.lease(&request.uri).map(|lease| self.subscribed.send_modify(|uris| drop(uris.insert(request.uri, lease)))).map_err(ErrorData::from),
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
            let leased = self.host.leased(&uris);
            let kept = Vec::from_iter(leased.keys().cloned());
            self.accepted.with_mut(|held| held.extend(leased));
            kept
        });
        Some(accepted)
    }

    async fn listen(&self, context: SubscriptionContext) -> Result<(), ErrorData> {
        let held = self.host.leased(context.accepted().resource_subscriptions.iter().flatten());
        self.accepted.with_mut(|accepted| accepted.retain(|uri, _| !held.contains_key(uri)));
        let notifications = self.host.notifications(context.accepted().clone(), watch::channel(held).1).take_until(self.host.stopping.cancelled()).take_until(context.cancelled());
        let sent = notifications.map(Ok).try_for_each(|notification| context.sink().send(notification));
        sent.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "subscription closed")).await;
        Ok(())
    }
}
