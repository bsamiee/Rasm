#![feature(error_reporter)]

use core::future::Future;
use std::{
    borrow::Cow,
    error::Report,
    ffi::OsString,
    io,
    process::{ExitCode, Stdio},
    sync::{Arc, OnceLock},
};

use clap::Parser;
use futures::{
    FutureExt as _, StreamExt as _,
    future::{BoxFuture, Shared},
};
use globset::{Glob, GlobSet};
use rasm_mcp::{Endpoint, PROTOCOL_VERSIONS, Target};
use rmcp::{
    ClientHandler, ClientLifecycleMode, ErrorData, Peer, RoleClient, RoleServer, ServerHandler, ServiceError,
    handler::client::progress::ProgressDispatcher,
    model::{
        CallToolRequest, CallToolRequestParams, CallToolResponse, CallToolResult, CancelTaskParams, CancelTaskRequest, ClientCapabilities, ClientConfig, ClientRequest, CompleteRequest,
        CompleteRequestParams, CompleteResult, CustomRequest, CustomResult, GetMeta as _, GetPromptRequest, GetPromptRequestParams, GetPromptResponse, GetTaskParams, GetTaskRequest, GetTaskResult,
        Implementation, ListPromptsRequest, ListPromptsResult, ListResourceTemplatesRequest, ListResourceTemplatesResult, ListResourcesRequest, ListResourcesResult, ListToolsRequest, ListToolsResult,
        PaginatedRequestParams, ProgressNotificationParam, ProtocolVersion, ReadResourceRequest, ReadResourceRequestParams, ReadResourceResponse, RequestMetaObject, ServerConfig, ServerNotification,
        ServerResult, SubscriptionFilter, Tool, UpdateTaskParams, UpdateTaskRequest,
    },
    service::{ClientInitializeError, NotificationContext, PeerRequestOptions, RequestContext, SubscriptionContext, SubscriptionEnd, serve_client_with_lifecycle_and_ct},
    transport::TokioChildProcess,
};
use tokio::{
    io::{AsyncBufReadExt as _, BufReader},
    process::Command,
    sync::watch,
};
use tokio_util::{sync::CancellationToken, task::AbortOnDropHandle};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Parser, Debug)]
#[command(version, about)]
struct Args {
    #[arg(long = "hide", value_name = "GLOB", help = "Glob of server tool names left out of the endpoint, repeated per glob")]
    hidden: Vec<Glob>,
    #[arg(help = "Stdio MCP server to bridge, written after --")]
    command: OsString,
    #[arg(help = "Arguments of the stdio MCP server")]
    arguments: Vec<OsString>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum BridgeError {
    #[error("cannot compile the hidden tool globs")]
    Glob(#[from] globset::Error),
    #[error("cannot spawn the upstream server")]
    Spawn(#[source] io::Error),
    #[error("upstream lifecycle failed")]
    Lifecycle(#[source] Box<ClientInitializeError>),
    #[error("upstream completed its lifecycle without server info")]
    Unidentified,
    #[error("cannot list the upstream tools")]
    Tools(#[source] ServiceError),
    #[error("upstream server ended")]
    Ended,
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Upstream {
    config: ClientConfig,
    progress: ProgressDispatcher,
    tools: watch::Sender<()>,
    prompts: watch::Sender<()>,
    resources: watch::Sender<()>,
}

impl ClientHandler for Upstream {
    async fn on_progress(&self, params: ProgressNotificationParam, _: NotificationContext<RoleClient>) { self.progress.handle_notification(params).await; }

    async fn on_tool_list_changed(&self, _: NotificationContext<RoleClient>) { self.tools.send_replace(()); }

    async fn on_prompt_list_changed(&self, _: NotificationContext<RoleClient>) { self.prompts.send_replace(()); }

    async fn on_resource_list_changed(&self, _: NotificationContext<RoleClient>) { self.resources.send_replace(()); }

    fn get_info(&self) -> ClientConfig { self.config.clone() }
}

struct Bridge {
    peer: Peer<RoleClient>,
    info: ServerConfig,
    version: ProtocolVersion,
    hidden: GlobSet,
    progress: ProgressDispatcher,
    tools: watch::Sender<Arc<[Tool]>>,
    prompts: watch::Receiver<()>,
    resources: watch::Receiver<()>,
    stop: CancellationToken,
    ended: Shared<BoxFuture<'static, ()>>,
    _relays: [Option<AbortOnDropHandle<()>>; 3],
}

impl Bridge {
    async fn spawn(args: Args, identity: Implementation) -> Result<Self, BridgeError> {
        let hidden = GlobSet::new(&args.hidden)?;
        let mut command = Command::new(&args.command);
        command.args(&args.arguments).kill_on_drop(true);
        let (transport, stderr) = TokioChildProcess::builder(command).stderr(Stdio::piped()).spawn().map_err(BridgeError::Spawn)?;
        let stderr = stderr.map(|stderr| {
            AbortOnDropHandle::new(tokio::spawn(async move {
                let mut lines = BufReader::new(stderr).split(b'\n');
                loop {
                    match lines.next_segment().await {
                        Ok(Some(line)) => tracing::info!(target: "upstream", message = %String::from_utf8_lossy(&line)),
                        Ok(None) => break,
                        Err(error) => break tracing::warn!(exception.message = %Report::new(error), "upstream stderr read failed"),
                    }
                }
            }))
        });
        let (progress, stop) = (ProgressDispatcher::new(), CancellationToken::new());
        let ((tools, stale), (prompts, prompted), (resources, resourced)) = (watch::channel(()), watch::channel(()), watch::channel(()));
        let lifecycle = ClientLifecycleMode::Auto {
            preferred_versions: PROTOCOL_VERSIONS.iter().rev().filter(|version| !version.has_initialize()).cloned().collect(),
            legacy_version: PROTOCOL_VERSIONS.iter().rev().find(|version| version.has_initialize()).cloned(),
        };
        let upstream = Upstream { config: ClientConfig::new(ClientCapabilities::default(), identity.clone()), progress: progress.clone(), tools, prompts, resources };
        let (marks, service) = ((upstream.tools.clone(), upstream.prompts.clone(), upstream.resources.clone()), serve_client_with_lifecycle_and_ct(upstream, transport, lifecycle, stop.clone()).await.map_err(|error| BridgeError::Lifecycle(Box::new(error)))?);
        let (peer, ended) = (service.peer().clone(), service.waiting().map(|quit| tracing::info!(quit = ?quit, "upstream ended")).boxed().shared());
        let upstream = peer.peer_info().ok_or(BridgeError::Unidentified)?;
        let visible = move |hidden: &GlobSet, listed: Vec<Tool>| listed.into_iter().filter(|tool| !hidden.is_match(tool.name.as_ref())).collect::<Arc<[Tool]>>();
        let snapshot = upstream.capabilities.tools.as_ref().and_then(|tools| tools.list_changed).is_some_and(|changed| changed);
        let tools = watch::Sender::new(if snapshot { visible(&hidden, peer.list_all_tools().await.map_err(BridgeError::Tools)?) } else { Arc::default() });
        let refresh = snapshot.then(|| {
            let (peer, hidden, tools, mut stale) = (peer.clone(), hidden.clone(), tools.clone(), stale);
            AbortOnDropHandle::new(tokio::spawn(async move {
                while stale.changed().await.is_ok() {
                    match peer.list_all_tools().await {
                        Ok(listed) => tools.send_if_modified(|current| {
                            let fresh = visible(&hidden, listed);
                            (*current != fresh).then(|| *current = fresh).is_some()
                        }),
                        Err(error) => tracing::warn!(exception.message = %Report::new(error), "tool list refresh failed"),
                    };
                }
            }))
        });
        let changes = SubscriptionFilter::builder().tools_list_changed().prompts_list_changed().resources_list_changed().build().supported_by(&upstream.capabilities);
        let follow = (!upstream.protocol_version.has_initialize() && changes != SubscriptionFilter::default()).then(|| {
            let peer = peer.clone();
            AbortOnDropHandle::new(tokio::spawn(async move {
                let mark = |filter: &SubscriptionFilter| {
                    [(filter.tools_list_changed, &marks.0), (filter.prompts_list_changed, &marks.1), (filter.resources_list_changed, &marks.2)]
                        .into_iter()
                        .filter(|(accepted, _)| *accepted == Some(true))
                        .for_each(|(_, changed)| changed.send_replace(()));
                };
                let mut filter = changes;
                loop {
                    let mut subscription = match peer.listen(filter.clone()).await {
                        Ok(subscription) => subscription,
                        Err(error) => break tracing::warn!(exception.message = %Report::new(error), "list change subscription failed"),
                    };
                    loop {
                        match (subscription.next().await, subscription.end()) {
                            (Ok(Some(ServerNotification::ToolListChangedNotification(_))), _) => marks.0.send_replace(()),
                            (Ok(Some(ServerNotification::PromptListChangedNotification(_))), _) => marks.1.send_replace(()),
                            (Ok(Some(ServerNotification::ResourceListChangedNotification(_))), _) => marks.2.send_replace(()),
                            (Ok(Some(_)), _) => (),
                            (Ok(None), Some(SubscriptionEnd::Lagged { capacity: _ })) => break mark(subscription.acknowledged()),
                            (Ok(None), end) => return tracing::debug!(end = ?end, "list change subscription ended"),
                            (Err(error), _) => return tracing::warn!(exception.message = %Report::new(error), "list change subscription failed"),
                        }
                    }
                    filter = subscription.acknowledged().clone();
                }
            }))
        });
        let mut info = ServerConfig::new(upstream.capabilities.clone()).with_server_info(upstream.server_info.clone().unwrap_or(identity));
        info.capabilities.logging = None;
        info.capabilities.resources.iter_mut().for_each(|resources| resources.subscribe = None);
        info.instructions.clone_from(&upstream.instructions);
        info.meta.clone_from(&upstream.meta);
        Ok(Self { peer, info, version: upstream.protocol_version.clone(), hidden, progress, tools, prompts: prompted, resources: resourced, stop, ended, _relays: [stderr, refresh, follow] })
    }
}

impl Endpoint for Bridge {
    type Error = BridgeError;

    fn session(&self, stopping: &CancellationToken) -> impl ServerHandler + use<> { Proxy { bridge: Arc::clone(self), stopping: stopping.clone(), relay: OnceLock::new() } }

    async fn serve(self: Arc<Self>, stopping: CancellationToken) -> Result<(), BridgeError> {
        let closed = tokio::select! {
            () = self.ended.clone() => Err(BridgeError::Ended),
            () = stopping.cancelled() => Ok(()),
        };
        self.stop.cancel();
        self.ended.clone().await;
        closed
    }
}

struct Proxy {
    bridge: Arc<Bridge>,
    stopping: CancellationToken,
    relay: OnceLock<AbortOnDropHandle<()>>,
}

impl Proxy {
    async fn forward(&self, mut request: ClientRequest, context: &RequestContext<RoleServer>) -> Result<ServerResult, ErrorData> {
        let fault = |error: ServiceError| match error {
            ServiceError::McpError(data) => data,
            other => ErrorData::internal_error(Report::new(other).to_string(), None),
        };
        let target = match &request {
            ClientRequest::CallToolRequest(call) => Some(Target::Tool(call.params.name.as_ref())),
            ClientRequest::GetPromptRequest(prompt) => Some(Target::Prompt(prompt.params.name.as_str())),
            ClientRequest::ReadResourceRequest(read) => Some(Target::Resource(read.params.uri.as_str())),
            _ => None,
        };
        let hidden = matches!(target, Some(Target::Tool(name)) if self.bridge.hidden.is_match(name));
        let method = request.method().to_owned();
        let failed = |result: &ServerResult| matches!(result, ServerResult::CallToolResult(CallToolResult { is_error: Some(true), .. }));
        rasm_mcp::observe(&method, target, context, failed, async {
            if hidden {
                return Err(ErrorData::invalid_params("tool not found", None));
            }
            *request.get_meta_mut() = RequestMetaObject(context.meta.0.clone());
            request.get_meta_mut().retain(|key, _| !key.starts_with("io.modelcontextprotocol/"));
            let options = match (self.bridge.version.has_initialize(), context.meta.client_capabilities(), context.meta.client_info()) {
                (false, Some(capabilities), Some(client)) => {
                    let mut meta = RequestMetaObject::new();
                    meta.set_client_capabilities(capabilities);
                    meta.set_client_info(client);
                    PeerRequestOptions::no_options().with_meta(meta)
                }
                _ => PeerRequestOptions::no_options(),
            };
            let downstream = context.meta.get_progress_token();
            if let Some(token) = &downstream
                && let Err(error) = context.peer.notify_progress(ProgressNotificationParam::new(token.clone(), 0.0)).await
            {
                tracing::debug!(exception.message = %Report::new(error), "progress relay failed");
            }
            let mut handle = self.bridge.peer.send_cancellable_request(request, options).await.map_err(fault)?;
            let mut progress = self.bridge.progress.subscribe(handle.progress_token.clone()).await;
            let mut relayed = 0.0;
            let mut response = loop {
                tokio::select! {
                    biased;
                    received = &mut handle.rx => break received.map_err(|_closed| ServiceError::TransportClosed).and_then(core::convert::identity).map_err(fault),
                    () = context.ct.cancelled() => break handle.cancel(None).await.map_err(fault).and(Err(ErrorData::internal_error("request cancelled", None))),
                    Some(mut update) = progress.next(), if downstream.is_some() => if update.progress > relayed && let Some(token) = &downstream {
                        relayed = update.progress;
                        update.progress_token = token.clone();
                        if let Err(error) = context.peer.notify_progress(update).await {
                            tracing::debug!(exception.message = %Report::new(error), "progress relay failed");
                        }
                    },
                }
            }?;
            if let ServerResult::ListToolsResult(listed) = &mut response {
                listed.tools.retain(|tool| !self.bridge.hidden.is_match(tool.name.as_ref()));
            }
            Ok(response)
        })
        .await
    }
}

macro_rules! forwarded {
    ($($method:ident($request:ident: $params:ty) -> $response:ty = $sent:expr => $($pattern:pat => $value:expr),+;)*) => {$(
        async fn $method(&self, $request: $params, context: RequestContext<RoleServer>) -> Result<$response, ErrorData> {
            match self.forward($sent.into(), &context).await? {
                $($pattern => Ok($value),)+
                _ => Err(ErrorData::internal_error(Report::new(ServiceError::UnexpectedResponse).to_string(), None)),
            }
        }
    )*};
}

impl ServerHandler for Proxy {
    forwarded! {
        complete(request: CompleteRequestParams) -> CompleteResult = CompleteRequest::new(request) =>
            ServerResult::CompleteResult(result) => result;
        get_prompt(request: GetPromptRequestParams) -> GetPromptResponse = GetPromptRequest::new(request) =>
            ServerResult::GetPromptResult(result) => result.into(),
            ServerResult::InputRequiredResult(result) => result.into();
        list_prompts(request: Option<PaginatedRequestParams>) -> ListPromptsResult = ListPromptsRequest { params: request, ..ListPromptsRequest::default() } =>
            ServerResult::ListPromptsResult(result) => result;
        list_resources(request: Option<PaginatedRequestParams>) -> ListResourcesResult = ListResourcesRequest { params: request, ..ListResourcesRequest::default() } =>
            ServerResult::ListResourcesResult(result) => result;
        list_resource_templates(request: Option<PaginatedRequestParams>) -> ListResourceTemplatesResult = ListResourceTemplatesRequest { params: request, ..ListResourceTemplatesRequest::default() } =>
            ServerResult::ListResourceTemplatesResult(result) => result;
        read_resource(request: ReadResourceRequestParams) -> ReadResourceResponse = ReadResourceRequest::new(request) =>
            ServerResult::ReadResourceResult(result) => result.into(),
            ServerResult::InputRequiredResult(result) => result.into();
        call_tool(request: CallToolRequestParams) -> CallToolResponse = CallToolRequest::new(request) =>
            ServerResult::CallToolResult(result) => result.into(),
            ServerResult::InputRequiredResult(result) => result.into(),
            ServerResult::CreateTaskResult(result) => result.into();
        get_task(request: GetTaskParams) -> GetTaskResult = GetTaskRequest::new(request) =>
            ServerResult::GetTaskResult(result) => result;
        update_task(request: UpdateTaskParams) -> () = UpdateTaskRequest::new(request) =>
            ServerResult::TaskAckResult(_) | ServerResult::EmptyResult(_) => ();
        cancel_task(request: CancelTaskParams) -> () = CancelTaskRequest::new(request) =>
            ServerResult::TaskAckResult(_) | ServerResult::EmptyResult(_) => ();
        on_custom_request(request: CustomRequest) -> CustomResult = request =>
            ServerResult::CustomResult(result) => result;
    }

    async fn list_tools(&self, request: Option<PaginatedRequestParams>, context: RequestContext<RoleServer>) -> Result<ListToolsResult, ErrorData> {
        match self.bridge.info.capabilities.tools.as_ref().and_then(|tools| tools.list_changed) {
            Some(true) => rasm_mcp::observe("tools/list", None, &context, |_| false, async { Ok(ListToolsResult::with_all_items(self.bridge.tools.borrow().to_vec())) }).await,
            Some(false) | None => match self.forward(ListToolsRequest { params: request, ..ListToolsRequest::default() }.into(), &context).await? {
                ServerResult::ListToolsResult(listed) => Ok(listed),
                _ => Err(ErrorData::internal_error(Report::new(ServiceError::UnexpectedResponse).to_string(), None)),
            },
        }
    }

    fn get_info(&self) -> ServerConfig { self.bridge.info.clone() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { Cow::Borrowed(PROTOCOL_VERSIONS) }

    async fn on_initialized(&self, context: NotificationContext<RoleServer>) {
        let changes = rasm_mcp::changes(self.bridge.tools.subscribe(), self.bridge.prompts.clone(), self.bridge.resources.clone());
        self.relay.get_or_init(|| AbortOnDropHandle::new(tokio::spawn(rasm_mcp::relay(context.peer, changes))));
    }

    fn accepted_subscription_filter(&self, requested: &SubscriptionFilter) -> Option<SubscriptionFilter> { Some(requested.clone()) }

    fn listen(&self, context: SubscriptionContext) -> impl Future<Output = Result<(), ErrorData>> + Send + '_ {
        let changes = rasm_mcp::changes(self.bridge.tools.subscribe(), self.bridge.prompts.clone(), self.bridge.resources.clone());
        async move { rasm_mcp::listen(&context, changes, &self.stopping).await }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode { rasm_mcp::run(async |args: Args, identity: Implementation| Bridge::spawn(args, identity).await) }
