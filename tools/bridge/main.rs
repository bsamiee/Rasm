use std::{borrow::Cow, collections::BTreeMap, io, num::NonZero, pin::pin, sync::Arc, time::Duration};

use clap::Parser;
use globset::{Glob, GlobSet};
use mcp::{Client, ClientError, HostError, TelemetryError};
use rmcp::{
    ClientCacheConfig, ErrorData, RoleServer, ServerHandler, ServiceError,
    model::{
        CallToolRequest, CallToolRequestParams, CallToolResponse, CancelTaskParams, CancelTaskRequest, CancelledNotificationParam, ClientCapabilities, ClientConfig, ClientRequest, CompleteRequest,
        CompleteRequestParams, CompleteResult, CustomRequest, CustomResult, GetPromptRequest, GetPromptRequestParams, GetPromptResponse, GetTaskParams, GetTaskRequest, GetTaskResult, Implementation,
        ListPromptsRequest, ListPromptsResult, ListResourceTemplatesRequest, ListResourceTemplatesResult, ListResourcesRequest, ListResourcesResult, ListToolsRequest, ListToolsResult,
        PaginatedRequestParams, ReadResourceRequest, ReadResourceRequestParams, ReadResourceResponse, ServerConfig, ServerNotification, ServerResult, SubscriptionFilter, Tool, UpdateTaskParams,
        UpdateTaskRequest,
    },
    service::{PeerRequestOptions, RequestContext, SubscriptionContext, SubscriptionSendError},
    transport::{ConfigureCommandExt as _, StreamableHttpServerConfig, TokioChildProcess},
};
use thiserror::Error;
use tokio::{
    process::Command,
    sync::{broadcast::error::RecvError, watch},
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Parser)]
#[command(version, about)]
struct Args {
    #[arg(long, help = "Loopback port the Streamable HTTP endpoint listens on")]
    port: NonZero<u16>,
    #[arg(long = "hide", value_name = "GLOB", help = "Glob of server tool names left out of the endpoint, repeated per glob")]
    hidden: Vec<Glob>,
    #[arg(help = "Stdio MCP server to bridge, written after --")]
    command: String,
    #[arg(help = "Arguments of the stdio MCP server")]
    arguments: Vec<String>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
enum BridgeError {
    #[error(transparent)]
    Telemetry(#[from] TelemetryError),
    #[error(transparent)]
    Glob(#[from] globset::Error),
    #[error("cannot spawn upstream `{command}`")]
    Spawn { command: String, source: io::Error },
    #[error(transparent)]
    Upstream(#[from] ClientError),
    #[error("cannot list upstream tools")]
    Tools(#[source] ServiceError),
    #[error("upstream session ended")]
    Closed,
    #[error(transparent)]
    Host(#[from] HostError),
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug)]
struct Proxy {
    upstream: Client,
    hidden: Arc<GlobSet>,
    config: Arc<ServerConfig>,
    tools: watch::Sender<BTreeMap<Cow<'static, str>, Tool>>,
}

impl Proxy {
    // --- [LIFECYCLE]
    async fn new(upstream: Client, hidden: GlobSet, identity: Implementation) -> Result<Self, ServiceError> {
        let info = upstream.info();
        let mut capabilities = info.capabilities.clone();
        capabilities.logging = None;
        let mut config = ServerConfig::new(capabilities).with_server_info(info.server_info.clone().unwrap_or(identity));
        config.instructions.clone_from(&info.instructions);
        config.meta.clone_from(&info.meta);
        let proxy = Self { upstream, hidden: Arc::new(hidden), config: Arc::new(config), tools: watch::Sender::new(BTreeMap::new()) };
        proxy.refresh_tools().await.map(|()| proxy)
    }

    async fn follow_upstream(&self) {
        let mut notifications = self.upstream.notifications();
        let mut ended = pin!(self.upstream.ended());
        loop {
            tokio::select! {
                biased;
                () = &mut ended => break,
                received = notifications.recv() => {
                    if let Ok(ServerNotification::ToolListChangedNotification(_)) | Err(RecvError::Lagged(_)) = received {
                        self.refresh_tools().await.unwrap_or_else(|error| tracing::warn!(%error, "tool list refresh failed"));
                    }
                }
            }
        }
    }

    // --- [TOOLS]
    async fn refresh_tools(&self) -> Result<(), ServiceError> {
        let listed = if self.upstream.info().capabilities.tools.is_some() { self.upstream.peer().list_all_tools().await? } else { Vec::new() };
        self.tools.send_replace(listed.into_iter().filter(|tool| !self.hidden.is_match(tool.name.as_ref())).map(|tool| (tool.name.clone(), tool)).collect());
        Ok(())
    }

    // --- [FORWARDING]
    async fn forward<R>(&self, request: R, context: &RequestContext<RoleServer>) -> Result<ServerResult, ErrorData>
    where R: Into<ClientRequest> {
        const REQUEST_TIMEOUT: Duration = Duration::from_secs(300);
        let request = request.into();
        if let ClientRequest::CallToolRequest(call) = &request
            && self.hidden.is_match(call.params.name.as_ref())
        {
            return Err(ErrorData::invalid_params("tool not found", None));
        }
        let mut meta = context.meta.clone();
        meta.retain(|key, _| !key.starts_with("io.modelcontextprotocol/"));
        let downstream_token = context.meta.get_progress_token();
        let mut notifications = self.upstream.notifications();
        let handle = self.upstream.peer().send_cancellable_request(request, PeerRequestOptions::with_timeout(REQUEST_TIMEOUT).with_meta(meta)).await.map_err(error_data)?;
        let (id, upstream_token) = (handle.id.clone(), handle.progress_token.clone());
        let mut pending = pin!(handle.await_response());
        let mut response = loop {
            tokio::select! {
                received = &mut pending => break received,
                () = context.ct.cancelled() => break self.upstream.peer().notify_cancelled(CancelledNotificationParam::new(Some(id), None)).await.and(Err(ServiceError::Cancelled { reason: None })),
                received = notifications.recv() => {
                    if let (Ok(ServerNotification::ProgressNotification(mut progress)), Some(token)) = (received, &downstream_token)
                        && progress.params.progress_token == upstream_token
                    {
                        progress.params.progress_token = token.clone();
                        context.peer.notify_progress(progress.params).await.unwrap_or_else(|error| tracing::debug!(%error, "progress relay failed"));
                    }
                }
            }
        }
        .map_err(error_data)?;
        if let ServerResult::ListToolsResult(listed) = &mut response {
            listed.tools.retain(|tool| !self.hidden.is_match(tool.name.as_ref()));
        }
        Ok(response)
    }
}

macro_rules! forwarded {
    ($($method:ident($request:ident: $params:ty) -> $response:ty = $sent:expr => $($pattern:pat => $value:expr),+;)*) => {$(
        async fn $method(&self, $request: $params, context: RequestContext<RoleServer>) -> Result<$response, ErrorData> {
            let result = self.forward($sent, &context).await?;
            $(if let $pattern = result { return Ok($value); })+
            Err(error_data(ServiceError::UnexpectedResponse))
        }
    )*};
}

impl ServerHandler for Proxy {
    // --- [FORWARDING]
    forwarded! {
        complete(request: CompleteRequestParams) -> CompleteResult = CompleteRequest::new(request) =>
            ServerResult::CompleteResult(result) => result;
        get_prompt(request: GetPromptRequestParams) -> GetPromptResponse = GetPromptRequest::new(request) =>
            ServerResult::GetPromptResult(result) => result.into(),
            ServerResult::InputRequiredResult(result) => result.into();
        list_prompts(request: Option<PaginatedRequestParams>) -> ListPromptsResult = ListPromptsRequest { params: request, ..Default::default() } =>
            ServerResult::ListPromptsResult(result) => result;
        list_resources(request: Option<PaginatedRequestParams>) -> ListResourcesResult = ListResourcesRequest { params: request, ..Default::default() } =>
            ServerResult::ListResourcesResult(result) => result;
        list_resource_templates(request: Option<PaginatedRequestParams>) -> ListResourceTemplatesResult = ListResourceTemplatesRequest { params: request, ..Default::default() } =>
            ServerResult::ListResourceTemplatesResult(result) => result;
        read_resource(request: ReadResourceRequestParams) -> ReadResourceResponse = ReadResourceRequest::new(request) =>
            ServerResult::ReadResourceResult(result) => result.into(),
            ServerResult::InputRequiredResult(result) => result.into();
        call_tool(request: CallToolRequestParams) -> CallToolResponse = CallToolRequest::new(request) =>
            ServerResult::CallToolResult(result) => result.into(),
            ServerResult::InputRequiredResult(result) => result.into(),
            ServerResult::CreateTaskResult(result) => result.into();
        list_tools(request: Option<PaginatedRequestParams>) -> ListToolsResult = ListToolsRequest { params: request, ..Default::default() } =>
            ServerResult::ListToolsResult(result) => result;
        get_task(request: GetTaskParams) -> GetTaskResult = GetTaskRequest::new(request) =>
            ServerResult::GetTaskResult(result) => result;
        update_task(request: UpdateTaskParams) -> () = UpdateTaskRequest::new(request) =>
            ServerResult::TaskAckResult(_) | ServerResult::EmptyResult(_) => ();
        cancel_task(request: CancelTaskParams) -> () = CancelTaskRequest::new(request) =>
            ServerResult::TaskAckResult(_) | ServerResult::EmptyResult(_) => ();
        on_custom_request(request: CustomRequest) -> CustomResult = request =>
            ServerResult::CustomResult(result) => result;
    }

    // --- [IDENTITY]
    fn get_info(&self) -> ServerConfig { self.config.as_ref().clone() }

    // --- [TOOLS]
    fn get_tool(&self, name: &str) -> Option<Tool> { self.tools.borrow().get(name).cloned() }

    // --- [SUBSCRIPTIONS]
    fn accepted_subscription_filter(&self, requested: &SubscriptionFilter) -> Option<SubscriptionFilter> { Some(requested.clone()) }

    async fn listen(&self, context: SubscriptionContext) -> Result<(), ErrorData> {
        let mut upstream = self.upstream.peer().listen(context.accepted().clone()).await.map_err(error_data)?;
        let relay = async {
            while let Some(notification) = upstream.next().await.map_err(error_data)? {
                match context.sink().send(notification).await {
                    Ok(()) => {}
                    Err(SubscriptionSendError::SubscriptionClosed) => return Ok(()),
                    Err(error) => return Err(ErrorData::internal_error(error.to_string(), None)),
                }
            }
            Ok(())
        };
        context.request_context().ct.run_until_cancelled(relay).await.unwrap_or(Ok(()))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn error_data(error: ServiceError) -> ErrorData { if let ServiceError::McpError(data) = error { data } else { ErrorData::internal_error(error.to_string(), None) } }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() -> Result<(), BridgeError> {
    let args = Args::parse();
    mcp::install_tracing()?;
    let hidden = GlobSet::new(&args.hidden)?;
    let identity = Implementation::new(env!("CARGO_PKG_NAME"), env!("CARGO_PKG_VERSION"));
    let transport = TokioChildProcess::new(Command::new(&args.command).configure(|child| {
        child.args(&args.arguments).kill_on_drop(true);
    }))
    .map_err(|source| BridgeError::Spawn { command: args.command, source })?;
    let upstream = Client::connect(ClientConfig::new(ClientCapabilities::default(), identity.clone()), transport, ClientCacheConfig::disabled()).await?;
    let served = async {
        let proxy = Proxy::new(upstream.clone(), hidden, identity).await.map_err(BridgeError::Tools)?;
        let _tool_changes = upstream
            .subscribe(SubscriptionFilter::builder().tools_list_changed().build())
            .await
            .inspect_err(|error| tracing::info!(%error, "upstream refused tool list subscription, unsolicited list changes refresh tools"));
        let config = StreamableHttpServerConfig::default();
        let shutdown = config.cancellation_token.clone();
        let factory = proxy.clone();
        let mut host = pin!(mcp::serve_loopback(move || Ok(factory.clone()), args.port, config));
        tokio::select! {
            biased;
            served = &mut host => served.map_err(BridgeError::from),
            () = proxy.follow_upstream() => {
                shutdown.cancel();
                host.await.map_err(BridgeError::from).and(Err(BridgeError::Closed))
            }
        }
    }
    .await;
    upstream.stop().await;
    served
}
