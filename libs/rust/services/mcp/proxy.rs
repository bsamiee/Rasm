use core::{error::Error, future::ready};
use std::{borrow::Cow, collections::HashMap, error::Report, process::Stdio, sync::Arc};

use arc_swap::ArcSwap;
use futures::{
    FutureExt as _, StreamExt as _, TryFutureExt as _, TryStreamExt as _,
    future::{BoxFuture, OptionFuture, Shared},
    stream,
};
use opentelemetry::{Context, global};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
#[cfg(windows)]
use process_wrap::tokio::JobObject;
#[cfg(unix)]
use process_wrap::tokio::ProcessGroup;
use process_wrap::tokio::{CommandWrap, KillOnDrop};
use rasm_daemon::Lifetime;
use rmcp::{
    ClientLifecycleMode, ErrorData, Peer, RoleClient, RoleServer, ServerHandler, Service, ServiceError,
    handler::client::progress::ProgressDispatcher,
    model::{
        CallToolRequest, CallToolRequestParams, CallToolResponse, CallToolResult, CancelTaskParams, CancelTaskRequest, ClientCapabilities, ClientConfig, ClientRequest, ClientResult, CompleteRequest,
        CompleteRequestParams, CompleteResult, CustomRequest, CustomResult, GetMeta as _, GetPromptRequest, GetPromptRequestParams, GetPromptResponse, GetPromptResult, GetTaskParams, GetTaskRequest,
        GetTaskResult, Implementation, ListPromptsRequest, ListPromptsResult, ListResourceTemplatesRequest, ListResourceTemplatesResult, ListResourcesRequest, ListResourcesResult, ListToolsRequest,
        ListToolsResult, MetaObject, PaginatedRequestParams, ProtocolVersion, ReadResourceRequest, ReadResourceRequestParams, ReadResourceResponse, ReadResourceResult, RequestMetaObject, ResultType,
        ServerCapabilities, ServerConfig, ServerNotification, ServerPeerInfo, ServerRequest, ServerResult, SubscriptionFilter, Tool, UpdateTaskParams, UpdateTaskRequest,
    },
    service::{ClientInitializeError, NotificationContext, PeerRequestOptions, RequestContext, SubscriptionEnd, serve_client_with_lifecycle_and_ct},
    transport::{IntoTransport, TokioChildProcess},
};
use tokio::{
    io::{AsyncBufReadExt as _, BufReader},
    process::Command,
    sync::oneshot,
};
use tokio_stream::wrappers::{BroadcastStream, SplitStream, errors::BroadcastStreamRecvError};
use tokio_util::{sync::CancellationToken, task::AbortOnDropHandle};

use crate::{Endpoint, McpError, Meta, Notifier, Progress, Response, Update, host::Host, internal, list_changes, resynced};

// --- [TYPES] ---------------------------------------------------------------------------

type Tools = ArcSwap<HashMap<Cow<'static, str>, Tool>>;
type Linking = Shared<BoxFuture<'static, Result<Arc<Link>, Arc<McpError>>>>;

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
struct Upstream {
    config: ClientConfig,
    notifier: Notifier,
    progress: ProgressDispatcher,
}

impl Service<RoleClient> for Upstream {
    async fn handle_request(&self, request: ServerRequest, context: RequestContext<RoleClient>) -> Result<ClientResult, ErrorData> { self.config.handle_request(request, context).await }

    async fn handle_notification(&self, notification: ServerNotification, _: NotificationContext<RoleClient>) -> Result<(), ErrorData> {
        match notification {
            ServerNotification::ProgressNotification(progress) => self.progress.handle_notification(progress.params).await,
            other => upstreamed(&self.notifier, other),
        }
        Ok(())
    }

    fn get_info(&self) -> ClientConfig { self.config.clone() }
}

#[derive(Debug)]
struct Link {
    peer: Peer<RoleClient>,
    info: ServerConfig,
    version: ProtocolVersion,
    stop: CancellationToken,
    ended: Shared<BoxFuture<'static, ()>>,
    _stderr: Option<AbortOnDropHandle<()>>,
}

#[derive(Debug)]
pub struct Proxy {
    identity: Implementation,
    notifier: Notifier,
    progress: ProgressDispatcher,
    tools: Arc<Tools>,
    link: Linking,
}

impl Proxy {
    pub fn spawn(mut command: Command, host: Host) -> Result<Self, McpError> {
        let program = command.as_std().get_program().to_owned();
        rasm_telemetry::inject(&Context::current(), &mut command);
        let Host { identity, filter: _, notifier } = host;
        let mut wrapped = CommandWrap::from(command);
        wrapped.wrap(KillOnDrop).wrap(cfg_select! {
            unix => ProcessGroup::leader(),
            windows => JobObject,
        });
        let (transport, stderr) = TokioChildProcess::builder(wrapped).stderr(Stdio::piped()).spawn().map_err(|source| McpError::Spawn { command: program, source })?;
        let stderr = stderr.map(|stderr| {
            let logged =
                SplitStream::new(BufReader::new(stderr).split(b'\n')).map_ok(|line| tracing::info!(target: "upstream", message = %String::from_utf8_lossy(line.trim_ascii_end()))).try_collect::<()>();
            AbortOnDropHandle::new(tokio::spawn(logged.unwrap_or_else(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "cannot read upstream stderr"))))
        });
        Ok(Self::linked(transport, identity, notifier, stderr))
    }

    pub fn connect<T, E, A>(transport: T, identity: Implementation, notifier: Notifier) -> Self
    where
        T: IntoTransport<RoleClient, E, A> + Send + 'static,
        E: Error + Send + Sync + 'static,
        A: Send + 'static,
    {
        Self::linked(transport, identity, notifier, None)
    }

    fn linked<T, E, A>(transport: T, identity: Implementation, notifier: Notifier, stderr: Option<AbortOnDropHandle<()>>) -> Self
    where
        T: IntoTransport<RoleClient, E, A> + Send + 'static,
        E: Error + Send + Sync + 'static,
        A: Send + 'static,
    {
        let (progress, tools, stop, fallback) = (ProgressDispatcher::new(), Arc::<Tools>::default(), CancellationToken::new(), identity.clone());
        let upstream = Upstream {
            config: ClientConfig::new(ClientCapabilities::default(), identity.clone()).with_protocol_version(ProtocolVersion::LATEST_WITH_INITIALIZE),
            notifier: notifier.clone(),
            progress: progress.clone(),
        };
        let link = async move {
            let handshake = |source| McpError::Handshake { source: Box::new(source) };
            let linked = async {
                let service = serve_client_with_lifecycle_and_ct(upstream, transport, ClientLifecycleMode::Initialize, stop.clone()).await.map_err(handshake)?;
                let peer = service.peer().clone();
                let ServerPeerInfo { protocol_version: version, mut capabilities, server_info, instructions, meta, .. } =
                    Arc::unwrap_or_clone(peer.peer_info().ok_or_else(|| handshake(ClientInitializeError::ExpectedInitResult(None)))?);
                capabilities.logging = None;
                capabilities.resources.iter_mut().filter(|_| version.has_initialize()).for_each(|resources| resources.subscribe = None);
                let mut info = ServerConfig::new(capabilities).with_server_info(server_info.unwrap_or(fallback));
                (info.instructions, info.meta) = (instructions, meta);
                let ended = service.waiting().map(|reason| tracing::info!(reason = ?reason, "upstream server ended")).boxed().shared();
                Ok::<_, McpError>(Link { peer, info, version, stop: stop.clone(), ended, _stderr: None })
            };
            match linked.await {
                Ok(link) => Ok(Arc::new(Link { _stderr: stderr, ..link })),
                Err(error) => {
                    if let Some(Err(failure)) = OptionFuture::from(stderr).await {
                        tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(failure), "upstream stderr relay ended before upstream exit");
                    }
                    Err(Arc::new(error))
                }
            }
        };
        Self { identity, notifier, progress, tools, link: link.boxed().shared() }
    }

    fn link(&self) -> Result<&Arc<Link>, McpError> {
        match self.link.peek() {
            Some(Ok(link)) => Ok(link),
            Some(Err(failure)) => Err(McpError::Unavailable { source: Arc::clone(failure) }),
            None => Err(McpError::Starting),
        }
    }

    async fn forward<R, T>(&self, request: R, context: RequestContext<RoleServer>) -> Result<T, ErrorData>
    where
        R: Into<ClientRequest>,
        T: Response,
    {
        let fault = |error: ServiceError| match error {
            ServiceError::McpError(data) => data,
            other => internal(other),
        };
        let link = self.link()?;
        let mut request = request.into();
        let meta = request.get_meta_mut();
        *meta = context.meta.clone();
        scrub(meta);
        global::get_text_map_propagator(|propagator| {
            propagator.fields().for_each(|field| drop(meta.remove(field)));
            propagator.inject_context(&Context::current(), &mut Meta(&mut meta.0));
        });
        let downstream = context.client_info().zip(context.client_capabilities()).filter(|_| !link.version.has_initialize());
        let options = downstream
            .into_iter()
            .fold(PeerRequestOptions::no_options(), |options, (client, capabilities)| options.with_meta(RequestMetaObject::with_client_context(link.version.clone(), client, capabilities)));
        let (progress, cancelled) = (Progress::default(), context.ct.clone());
        let forwarded = async {
            let mut handle = link.peer.send_cancellable_request(request, options).await.map_err(fault)?;
            let mut updates = self.progress.subscribe(handle.progress_token.clone()).await;
            loop {
                tokio::select! {
                    biased;
                    Some(update) = updates.next() => progress.report(Update { progress: update.progress, total: update.total, message: update.message }),
                    received = &mut handle.rx => break received.map_err(|oneshot::error::RecvError { .. }| ServiceError::TransportClosed).flatten().map_err(fault),
                    () = cancelled.cancelled() => break handle.cancel(None).await.map_err(fault).and(Err(fault(ServiceError::Cancelled { reason: None }))),
                }
            }
        };
        let mut result = progress.relay(&context, forwarded).await?;
        if let ServerResult::CompleteResult(CompleteResult { result_type, .. })
        | ServerResult::GetPromptResult(GetPromptResult { result_type, .. })
        | ServerResult::ListPromptsResult(ListPromptsResult { result_type, .. })
        | ServerResult::ListResourcesResult(ListResourcesResult { result_type, .. })
        | ServerResult::ListResourceTemplatesResult(ListResourceTemplatesResult { result_type, .. })
        | ServerResult::ReadResourceResult(ReadResourceResult { result_type, .. })
        | ServerResult::ListToolsResult(ListToolsResult { result_type, .. })
        | ServerResult::CallToolResult(CallToolResult { result_type, .. }) = &mut result
        {
            result_type.get_or_insert(ResultType::COMPLETE);
        }
        T::answered(result).ok_or_else(|| internal(ServiceError::UnexpectedResponse))
    }
}

impl ServerHandler for Proxy {
    async fn complete(&self, request: CompleteRequestParams, context: RequestContext<RoleServer>) -> Result<CompleteResult, ErrorData> { self.forward(CompleteRequest::new(request), context).await }

    async fn get_prompt(&self, request: GetPromptRequestParams, context: RequestContext<RoleServer>) -> Result<GetPromptResponse, ErrorData> {
        self.forward(GetPromptRequest::new(request), context).await
    }

    async fn list_prompts(&self, request: Option<PaginatedRequestParams>, context: RequestContext<RoleServer>) -> Result<ListPromptsResult, ErrorData> {
        self.forward(ListPromptsRequest { params: request, ..ListPromptsRequest::default() }, context).await
    }

    async fn list_resources(&self, request: Option<PaginatedRequestParams>, context: RequestContext<RoleServer>) -> Result<ListResourcesResult, ErrorData> {
        self.forward(ListResourcesRequest { params: request, ..ListResourcesRequest::default() }, context).await
    }

    async fn list_resource_templates(&self, request: Option<PaginatedRequestParams>, context: RequestContext<RoleServer>) -> Result<ListResourceTemplatesResult, ErrorData> {
        self.forward(ListResourceTemplatesRequest { params: request, ..ListResourceTemplatesRequest::default() }, context).await
    }

    async fn read_resource(&self, request: ReadResourceRequestParams, context: RequestContext<RoleServer>) -> Result<ReadResourceResponse, ErrorData> {
        self.forward(ReadResourceRequest::new(request), context).await
    }

    async fn call_tool(&self, request: CallToolRequestParams, context: RequestContext<RoleServer>) -> Result<CallToolResponse, ErrorData> { self.forward(CallToolRequest::new(request), context).await }

    async fn list_tools(&self, request: Option<PaginatedRequestParams>, context: RequestContext<RoleServer>) -> Result<ListToolsResult, ErrorData> {
        self.forward(ListToolsRequest { params: request, ..ListToolsRequest::default() }, context).await
    }

    async fn get_task(&self, request: GetTaskParams, context: RequestContext<RoleServer>) -> Result<GetTaskResult, ErrorData> { self.forward(GetTaskRequest::new(request), context).await }

    async fn update_task(&self, request: UpdateTaskParams, context: RequestContext<RoleServer>) -> Result<(), ErrorData> { self.forward(UpdateTaskRequest::new(request), context).await }

    async fn cancel_task(&self, request: CancelTaskParams, context: RequestContext<RoleServer>) -> Result<(), ErrorData> { self.forward(CancelTaskRequest::new(request), context).await }

    async fn on_custom_request(&self, request: CustomRequest, context: RequestContext<RoleServer>) -> Result<CustomResult, ErrorData> { self.forward(request, context).await }

    fn get_tool(&self, name: &str) -> Option<Tool> { self.tools.load().get(name).cloned() }

    fn get_info(&self) -> ServerConfig { self.link().map_or_else(|_| ServerConfig::new(ServerCapabilities::default()).with_server_info(self.identity.clone()), |link| link.info.clone()) }
}

impl Endpoint for Proxy {
    fn ready(&self) -> Result<(), McpError> { self.link().and(Ok(())) }

    fn watch(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, McpError> {
        self.link().map(|link| follow(link.peer.clone(), self.notifier.clone(), SubscriptionFilter::builder().resource_subscription(uri).build()))
    }
}

impl Lifetime for Proxy {
    type Error = McpError;

    async fn lifetime(&self, stopping: CancellationToken) -> Result<(), McpError> {
        async fn refresh(peer: Peer<RoleClient>, tools: Arc<Tools>, notifications: BroadcastStream<ServerNotification>) {
            let changed = notifications.filter(|received| ready(matches!(received, Ok(ServerNotification::ToolListChangedNotification(_)) | Err(BroadcastStreamRecvError::Lagged(_)))));
            stream::once(ready(()))
                .chain(changed.map(drop))
                .for_each(async |()| {
                    tools.store(Arc::default());
                    match peer.list_all_tools().await {
                        Ok(listed) => tools.store(Arc::new(listed.into_iter().map(|tool| (tool.name.clone(), tool)).collect())),
                        Err(error) => tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "cannot list upstream tools"),
                    }
                })
                .await;
        }
        let Some(linked) = stopping.run_until_cancelled(self.link.clone()).await else { return Ok(()) };
        let link = linked.map_err(|source| McpError::Unavailable { source })?;
        tracing::info!(version = %link.version, "upstream linked");
        let changes = list_changes(&link.info.capabilities);
        let _following =
            (!link.version.has_initialize() && changes != SubscriptionFilter::default()).then(|| AbortOnDropHandle::new(tokio::spawn(follow(link.peer.clone(), self.notifier.clone(), changes))));
        let _refreshing = link
            .info
            .capabilities
            .tools
            .is_some()
            .then(|| AbortOnDropHandle::new(tokio::spawn(refresh(link.peer.clone(), Arc::clone(&self.tools), BroadcastStream::new(self.notifier.0.subscribe())))));
        let closed = stopping.run_until_cancelled(link.ended.clone()).await.is_none().ok_or(McpError::Ended);
        link.stop.cancel();
        link.ended.clone().await;
        closed
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn follow(peer: Peer<RoleClient>, notifier: Notifier, filter: SubscriptionFilter) {
    let followed = async {
        let mut subscription = peer.listen(filter).await?;
        loop {
            match (subscription.next().await?, subscription.end()) {
                (Some(notification), _) => upstreamed(&notifier, notification),
                (None, Some(SubscriptionEnd::Lagged { capacity: _ })) => {
                    subscription = peer.listen(subscription.acknowledged().clone()).await?;
                    let acknowledged = subscription.acknowledged();
                    resynced(acknowledged, acknowledged.resource_subscriptions.iter().flatten().cloned()).for_each(|notification| upstreamed(&notifier, notification));
                }
                (None, end) => break tracing::debug!(end = ?end, "upstream subscription ended"),
            }
        }
        Ok::<(), ServiceError>(())
    };
    followed.unwrap_or_else(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "upstream subscription failed")).await;
}

fn upstreamed(notifier: &Notifier, mut notification: ServerNotification) {
    scrub(notification.get_meta_mut());
    notifier.notify(notification);
}

fn scrub(meta: &mut MetaObject) {
    const PREFIX: &str = "io.modelcontextprotocol/";
    meta.retain(|key, _| !key.starts_with(PREFIX));
}
