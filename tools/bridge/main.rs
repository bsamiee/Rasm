#![feature(error_reporter, nonpoison_mutex, sync_nonpoison)]

use core::{future::ready, mem};
use std::{
    borrow::Cow,
    collections::BTreeMap,
    error::Report,
    ffi::OsString,
    io,
    process::{ExitCode, Stdio},
    sync::{Arc, Weak, nonpoison::Mutex},
};

use clap::Parser;
use futures::{
    FutureExt as _, StreamExt as _, TryFutureExt as _,
    future::{BoxFuture, OptionFuture, Shared},
    stream,
};
use globset::{Glob, GlobSet};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_mcp::{Changes, Endpoint, PROTOCOL_VERSIONS, Subscriber, Target};
use rmcp::{
    ClientHandler, ClientLifecycleMode, ErrorData, Peer, RoleClient, RoleServer, ServerHandler, ServiceError,
    handler::client::progress::ProgressDispatcher,
    model::{
        CallToolRequest, CallToolRequestParams, CallToolResponse, CancelTaskParams, CancelTaskRequest, ClientCapabilities, ClientConfig, ClientRequest, CompleteRequest, CompleteRequestParams,
        CompleteResult, CustomRequest, CustomResult, GetMeta as _, GetPromptRequest, GetPromptRequestParams, GetPromptResponse, GetTaskParams, GetTaskRequest, GetTaskResult, Implementation,
        ListPromptsRequest, ListPromptsResult, ListResourceTemplatesRequest, ListResourceTemplatesResult, ListResourcesRequest, ListResourcesResult, ListToolsRequest, ListToolsResult,
        PaginatedRequestParams, ProgressNotificationParam, ProtocolVersion, ReadResourceRequest, ReadResourceRequestParams, ReadResourceResponse, RequestMetaObject, ServerConfig, ServerNotification,
        ServerResult, SubscribeRequestMethod, SubscribeRequestParams, SubscriptionFilter, Tool, UnsubscribeRequestParams, UpdateTaskParams, UpdateTaskRequest,
    },
    service::{ClientInitializeError, MaybeSendFuture, NotificationContext, PeerRequestOptions, RequestContext, SubscriptionContext, SubscriptionEnd, serve_client_with_lifecycle_and_ct},
    transport::TokioChildProcess,
};
use tokio::{
    io::{AsyncBufReadExt as _, BufReader},
    process::Command,
    sync::watch,
};
use tokio_stream::wrappers::WatchStream;
use tokio_util::{
    sync::{CancellationToken, DropGuard},
    task::{AbortOnDropHandle, TaskTracker},
};

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
    marks: Changes<(), (), ()>,
}

impl ClientHandler for Upstream {
    async fn on_progress(&self, params: ProgressNotificationParam, _: NotificationContext<RoleClient>) { self.progress.handle_notification(params).await; }

    async fn on_tool_list_changed(&self, _: NotificationContext<RoleClient>) { self.marks.tools.send_modify(|_| ()); }

    async fn on_prompt_list_changed(&self, _: NotificationContext<RoleClient>) { self.marks.prompts.send_modify(|_| ()); }

    async fn on_resource_list_changed(&self, _: NotificationContext<RoleClient>) { self.marks.resources.send_modify(|_| ()); }

    fn get_info(&self) -> ClientConfig { self.config.clone() }
}

struct Bridge {
    peer: Peer<RoleClient>,
    info: ServerConfig,
    version: ProtocolVersion,
    hidden: GlobSet,
    progress: ProgressDispatcher,
    marks: Changes<(), (), ()>,
    changes: Changes<Option<Vec<Tool>>, (), ()>,
    leases: Mutex<BTreeMap<String, Weak<DropGuard>>>,
    tasks: TaskTracker,
    stop: CancellationToken,
    ended: Shared<BoxFuture<'static, ()>>,
    _stderr: Option<AbortOnDropHandle<()>>,
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
                while let Some(line) = lines.next_segment().await.inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "upstream stderr read failed")).ok().flatten() {
                    tracing::info!(target: "upstream", message = %String::from_utf8_lossy(&line));
                }
            }))
        });
        let (progress, tasks, stop) = (ProgressDispatcher::new(), TaskTracker::new(), CancellationToken::new());
        let marks = Changes::new((), (), ());
        let stale = marks.tools.subscribe();
        let lifecycle = ClientLifecycleMode::Auto {
            preferred_versions: PROTOCOL_VERSIONS.iter().rev().filter(|version| !version.has_initialize()).cloned().collect(),
            legacy_version: PROTOCOL_VERSIONS.iter().rev().find(|version| version.has_initialize()).cloned(),
        };
        let handler = Upstream { config: ClientConfig::new(ClientCapabilities::default(), identity.clone()), progress: progress.clone(), marks: marks.clone() };
        let service = serve_client_with_lifecycle_and_ct(handler, transport, lifecycle, stop.clone()).await.map_err(|error| BridgeError::Lifecycle(Box::new(error)))?;
        let (peer, ended) = (service.peer().clone(), service.waiting().map(|quit| tracing::info!(quit = ?quit, "upstream ended")).boxed().shared());
        let upstream = peer.peer_info().ok_or(BridgeError::Unidentified)?;
        let visible = {
            let hidden = hidden.clone();
            move |listed: Vec<Tool>| Arc::new(Some(listed.into_iter().filter(|tool| !hidden.is_match(tool.name.as_ref())).collect()))
        };
        let listing = upstream.capabilities.tools.as_ref().and_then(|tools| tools.list_changed) == Some(true);
        let tools = watch::Sender::new(OptionFuture::from(listing.then(|| peer.list_all_tools())).await.transpose().map_err(BridgeError::Tools)?.map_or_default(&visible));
        if listing {
            let tools = tools.clone();
            let listings = WatchStream::from_changes(stale).zip(stream::repeat(peer.clone())).then(async |(_, client)| client.list_all_tools().await);
            tasks.spawn(stop.clone().run_until_cancelled_owned(listings.for_each(move |listed| {
                match listed {
                    Ok(listed) => drop(tools.send_if_modified(|current| mem::replace(current, visible(listed)) != *current)),
                    Err(error) => tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "tool list refresh failed"),
                }
                ready(())
            })));
        }
        let filter = SubscriptionFilter::builder().tools_list_changed().prompts_list_changed().resources_list_changed().build().supported_by(&upstream.capabilities);
        if !upstream.protocol_version.has_initialize() && filter != SubscriptionFilter::default() {
            track(&tasks, &stop, follow(peer.clone(), marks.clone(), filter));
        }
        let mut info = ServerConfig::new(upstream.capabilities.clone()).with_server_info(upstream.server_info.clone().unwrap_or(identity));
        info.capabilities.logging = None;
        info.capabilities.resources.iter_mut().filter(|_| upstream.protocol_version.has_initialize()).for_each(|capability| capability.subscribe = None);
        info.instructions.clone_from(&upstream.instructions);
        info.meta.clone_from(&upstream.meta);
        let changes = Changes { tools, prompts: marks.prompts.clone(), resources: marks.resources.clone(), updated: marks.updated.clone() };
        Ok(Self { peer, info, version: upstream.protocol_version.clone(), hidden, progress, marks, changes, leases: Mutex::default(), tasks, stop, ended, _stderr: stderr })
    }

    fn lease(&self, uri: &String) -> Arc<DropGuard> {
        let mut leases = self.leases.lock();
        leases.retain(|_, held| held.strong_count() > 0);
        leases.get(uri).and_then(Weak::upgrade).unwrap_or_else(|| {
            let released = CancellationToken::new();
            let held = Arc::new(released.clone().drop_guard());
            leases.insert(uri.clone(), Arc::downgrade(&held));
            let filter = SubscriptionFilter::builder().resource_subscriptions(vec![uri.clone()]).build();
            track(&self.tasks, &self.stop, released.run_until_cancelled_owned(follow(self.peer.clone(), self.marks.clone(), filter)).map(|followed| followed.unwrap_or(Ok(()))));
            held
        })
    }
}

impl Endpoint for Bridge {
    type Error = BridgeError;

    fn session(self: Arc<Self>, stopping: &CancellationToken, _: &TaskTracker) -> impl ServerHandler + use<> { Proxy { bridge: self, subscriber: Subscriber::new(stopping.clone()) } }

    async fn serve(self: Arc<Self>, stopping: CancellationToken) -> Result<(), BridgeError> {
        let closed = match stopping.run_until_cancelled(self.ended.clone()).await {
            Some(()) => Err(BridgeError::Ended),
            None => Ok(()),
        };
        self.stop.cancel();
        self.ended.clone().await;
        self.tasks.close();
        self.tasks.wait().await;
        closed
    }
}

struct Proxy {
    bridge: Arc<Bridge>,
    subscriber: Subscriber<Arc<DropGuard>>,
}

impl Proxy {
    async fn forward(&self, mut request: ClientRequest, context: &RequestContext<RoleServer>) -> Result<ServerResult, ErrorData> {
        let fault = |error: ServiceError| if let ServiceError::McpError(data) = error { data } else { ErrorData::internal_error(Report::new(error).to_string(), None) };
        let target = if let ClientRequest::CallToolRequest(call) = &request {
            Some(Target::Tool(call.params.name.clone()))
        } else if let ClientRequest::GetPromptRequest(prompt) = &request {
            Some(Target::Prompt(prompt.params.name.clone()))
        } else if let ClientRequest::ReadResourceRequest(read) = &request {
            Some(Target::Resource(read.params.uri.clone()))
        } else {
            None
        };
        let hidden = matches!(&target, Some(Target::Tool(name)) if self.bridge.hidden.is_match(name.as_ref()));
        let method = request.method().to_owned();
        let failed = |result: &ServerResult| matches!(result, ServerResult::CallToolResult(result) if result.is_error == Some(true));
        rasm_mcp::observe(&method, target, context, failed, async {
            if hidden {
                return Err(ErrorData::invalid_params("tool not found", None));
            }
            if let (ClientRequest::ListToolsRequest(_), Some(tools)) = (&request, &**self.bridge.changes.tools.borrow()) {
                return Ok(ServerResult::ListToolsResult(ListToolsResult::with_all_items(tools.clone())));
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
                (true | false, _, _) => PeerRequestOptions::no_options(),
            };
            let downstream = context.meta.get_progress_token();
            if let Some(token) = &downstream
                && let Err(error) = context.peer.notify_progress(ProgressNotificationParam::new(token.clone(), 0.0)).await
            {
                tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "progress relay failed");
            }
            let mut handle = self.bridge.peer.send_cancellable_request(request, options).await.map_err(fault)?;
            let mut progress = stream::iter(OptionFuture::from(downstream.as_ref().map(|_| self.bridge.progress.subscribe(handle.progress_token.clone()))).await).flatten();
            let mut relayed = 0.0_f64;
            let mut response = loop {
                tokio::select! {
                    biased;
                    received = &mut handle.rx => break received.map_err(|_closed| ServiceError::TransportClosed).flatten().map_err(fault),
                    () = context.ct.cancelled() => break handle.cancel(None).await.map_err(fault).and(Err(ErrorData::internal_error("request cancelled", None))),
                    Some(mut update) = progress.next() => if update.progress > relayed && let Some(token) = &downstream {
                        relayed = update.progress;
                        update.progress_token = token.clone();
                        if let Err(error) = context.peer.notify_progress(update).await {
                            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "progress relay failed");
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
        list_tools(request: Option<PaginatedRequestParams>) -> ListToolsResult = ListToolsRequest { params: request, ..ListToolsRequest::default() } =>
            ServerResult::ListToolsResult(result) => result;
    }

    fn get_info(&self) -> ServerConfig { self.bridge.info.clone() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { Cow::Borrowed(PROTOCOL_VERSIONS) }

    async fn on_initialized(&self, context: NotificationContext<RoleServer>) { self.bridge.changes.initialized(&self.subscriber, context.peer); }

    fn subscribe(&self, request: SubscribeRequestParams, _: RequestContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + MaybeSendFuture + '_ {
        match self.bridge.info.capabilities.resources.as_ref().and_then(|resources| resources.subscribe) {
            Some(true) => self.subscriber.subscribe(request.uri, |uri| self.bridge.lease(uri)),
            Some(false) | None => ready(Err(ErrorData::method_not_found::<SubscribeRequestMethod>())),
        }
    }

    fn unsubscribe(&self, request: UnsubscribeRequestParams, _: RequestContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + MaybeSendFuture + '_ {
        self.subscriber.unsubscribe(&request.uri)
    }

    fn accepted_subscription_filter(&self, requested: &SubscriptionFilter) -> Option<SubscriptionFilter> { Some(requested.clone()) }

    async fn listen(&self, context: SubscriptionContext) -> Result<(), ErrorData> {
        let _leases = context.accepted().resource_subscriptions.iter().flatten().map(|uri| self.bridge.lease(uri)).collect::<Vec<_>>();
        self.bridge.changes.listen(&self.subscriber, context).await;
        Ok(())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn follow(peer: Peer<RoleClient>, marks: Changes<(), (), ()>, filter: SubscriptionFilter) -> Result<(), ServiceError> {
    let mut subscription = peer.listen(filter).await?;
    loop {
        match (subscription.next().await?, subscription.end()) {
            (Some(ServerNotification::ToolListChangedNotification(_)), _) => marks.tools.send_modify(|_| ()),
            (Some(ServerNotification::PromptListChangedNotification(_)), _) => marks.prompts.send_modify(|_| ()),
            (Some(ServerNotification::ResourceListChangedNotification(_)), _) => marks.resources.send_modify(|_| ()),
            (Some(ServerNotification::ResourceUpdatedNotification(updated)), _) => marks.update(updated.params.uri),
            (Some(_), _) => {}
            (None, Some(SubscriptionEnd::Lagged { capacity: _ })) => {
                subscription = peer.listen(subscription.acknowledged().clone()).await?;
                let acknowledged = subscription.acknowledged();
                [(acknowledged.tools_list_changed, &marks.tools), (acknowledged.prompts_list_changed, &marks.prompts), (acknowledged.resources_list_changed, &marks.resources)]
                    .into_iter()
                    .filter(|(kind, _)| *kind == Some(true))
                    .for_each(|(_, mark)| mark.send_modify(|_| ()));
                acknowledged.resource_subscriptions.iter().flatten().for_each(|uri| marks.update(uri.clone()));
            }
            (None, end) => break tracing::debug!(end = ?end, "upstream subscription ended"),
        }
    }
    Ok(())
}

fn track<F>(tasks: &TaskTracker, stop: &CancellationToken, task: F)
where F: Future<Output = Result<(), ServiceError>> + Send + 'static {
    tasks.spawn(stop.clone().run_until_cancelled_owned(task.unwrap_or_else(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "upstream subscription failed"))));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode { rasm_mcp::run(async |args: Args, identity: Implementation| Bridge::spawn(args, identity).await) }
