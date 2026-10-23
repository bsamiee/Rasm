#![feature(error_reporter)]

use core::future::ready;
use std::{error::Report, ffi::OsString, io, process, sync::Arc};

use futures::{FutureExt as _, StreamExt as _, TryFutureExt as _, TryStreamExt as _, future, stream};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_mcp::{Endpoint, Notifier, PROTOCOL_VERSIONS, Progress, Unserved, fault};
use rmcp::{
    ClientLifecycleMode, ErrorData, Peer, RoleClient, RoleServer, Service, ServiceError,
    model::{
        CallToolResult, ClientCapabilities, ClientConfig, ClientRequest, ClientResult, CompleteResult, GetMeta as _, GetPromptResult, Implementation, ListPromptsResult, ListResourceTemplatesResult,
        ListResourcesResult, ListToolsResult, MetaObject, ProtocolVersion, ReadResourceResult, RequestMetaObject, ResultType, ServerConfig, ServerNotification, ServerPeerInfo, ServerRequest,
        ServerResult, SubscriptionFilter,
    },
    service::{ClientInitializeError, NotificationContext, PeerRequestOptions, RequestContext, SubscriptionEnd, serve_client_with_lifecycle_and_ct},
    transport::TokioChildProcess,
};
use tokio::{
    io::{AsyncBufReadExt as _, BufReader},
    process::Command,
};
use tokio_stream::wrappers::{SplitStream, errors::BroadcastStreamRecvError};
use tokio_util::{sync::CancellationToken, task::AbortOnDropHandle};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(clap::Parser)]
#[command(about)]
struct Args {
    #[arg(help = "Stdio MCP server executable")]
    command: OsString,
    #[arg(trailing_var_arg = true, allow_hyphen_values = true, help = "Stdio MCP server arguments")]
    arguments: Vec<OsString>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum BridgeError {
    #[error("cannot spawn upstream server {}", command.display())]
    Spawn { command: OsString, source: io::Error },
    #[error("upstream handshake failed")]
    Handshake(#[source] Box<ClientInitializeError>),
    #[error("upstream handshake sent no server info")]
    Unidentified,
    #[error("upstream server ended before shutdown")]
    Ended,
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Upstream {
    config: ClientConfig,
    notifier: Notifier,
}

impl Service<RoleClient> for Upstream {
    async fn handle_request(&self, request: ServerRequest, context: RequestContext<RoleClient>) -> Result<ClientResult, ErrorData> { self.config.handle_request(request, context).await }

    fn handle_notification(&self, notification: ServerNotification, _: NotificationContext<RoleClient>) -> impl Future<Output = Result<(), ErrorData>> {
        self.notifier.notify(notification);
        ready(Ok(()))
    }

    fn get_info(&self) -> ClientConfig { self.config.clone() }
}

struct Bridge {
    peer: Peer<RoleClient>,
    info: ServerConfig,
    version: ProtocolVersion,
    notifier: Notifier,
    stop: CancellationToken,
    ended: future::Shared<future::BoxFuture<'static, ()>>,
    _stderr: Option<AbortOnDropHandle<()>>,
}

impl Bridge {
    async fn spawn(args: Args, identity: Implementation, notifier: Notifier) -> Result<Self, BridgeError> {
        let mut command = Command::new(&args.command);
        command.args(&args.arguments).kill_on_drop(true);
        let (transport, stderr) = TokioChildProcess::builder(command).stderr(process::Stdio::piped()).spawn().map_err(|source| BridgeError::Spawn { command: args.command, source })?;
        let stderr = stderr.map(|stderr| {
            let logged = SplitStream::new(BufReader::new(stderr).split(b'\n')).map_ok(|line| tracing::info!(target: "upstream", message = %String::from_utf8_lossy(&line))).try_collect::<()>();
            AbortOnDropHandle::new(tokio::spawn(logged.unwrap_or_else(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "cannot read upstream stderr"))))
        });
        let (legacy, preferred_versions) = PROTOCOL_VERSIONS.iter().rev().cloned().partition::<Vec<_>, _>(ProtocolVersion::has_initialize);
        let lifecycle = ClientLifecycleMode::Auto { preferred_versions, legacy_version: legacy.into_iter().next() };
        let (upstream, stop) = (Upstream { config: ClientConfig::new(ClientCapabilities::default(), identity.clone()), notifier: notifier.clone() }, CancellationToken::new());
        let service = serve_client_with_lifecycle_and_ct(upstream, transport, lifecycle, stop.clone()).await.map_err(|error| BridgeError::Handshake(Box::new(error)))?;
        let (peer, ended) = (service.peer().clone(), service.waiting().map(|reason| tracing::info!(reason = ?reason, "upstream server ended")).boxed().shared());
        let ServerPeerInfo { protocol_version: version, mut capabilities, server_info, instructions, meta, .. } = Arc::unwrap_or_clone(peer.peer_info().ok_or(BridgeError::Unidentified)?);
        capabilities.logging = None;
        capabilities.resources.iter_mut().filter(|_| version.has_initialize()).for_each(|resources| resources.subscribe = None);
        let mut info = ServerConfig::new(capabilities).with_server_info(server_info.unwrap_or(identity));
        (info.instructions, info.meta) = (instructions, meta);
        Ok(Self { peer, info, version, notifier, stop, ended, _stderr: stderr })
    }
}

impl Endpoint for Bridge {
    type Error = BridgeError;

    fn info(&self) -> &ServerConfig { &self.info }

    async fn handle(&self, mut request: ClientRequest, context: RequestContext<RoleServer>) -> Result<ServerResult, ErrorData> {
        let options = match (self.version.has_initialize(), context.meta.client_capabilities(), context.meta.client_info()) {
            (false, Some(capabilities), Some(client)) => PeerRequestOptions::no_options().with_meta(RequestMetaObject::with_client_context(self.version.clone(), client, capabilities)),
            (true | false, _, _) => PeerRequestOptions::no_options(),
        };
        let (progress, cancelled) = (Progress::default(), context.ct.clone());
        let mut updates = stream::iter(context.meta.get_progress_token().map(|_| self.notifier.subscribe())).flatten();
        *request.get_meta_mut() = context.meta.clone();
        scrub(request.get_meta_mut());
        let forwarded = async {
            let mut handle = self.peer.send_cancellable_request(request, options).await.map_err(fault)?;
            loop {
                tokio::select! {
                    biased;
                    Some(update) = updates.next() => match update {
                        Ok(ServerNotification::ProgressNotification(update)) if update.params.progress_token == handle.progress_token =>
                            progress.report(update.params.progress, update.params.total, update.params.message),
                        Ok(_) | Err(BroadcastStreamRecvError::Lagged(_)) => {}
                    },
                    received = &mut handle.rx => break received.map_err(|_closed| ServiceError::TransportClosed).flatten().map_err(fault),
                    () = cancelled.cancelled() => break handle.cancel(None).await.map_err(fault).and(Err(fault(ServiceError::Cancelled { reason: None }))),
                }
            }
        };
        let mut response = progress.relay(&context, forwarded).await?;
        if let ServerResult::CompleteResult(CompleteResult { result_type, .. })
        | ServerResult::GetPromptResult(GetPromptResult { result_type, .. })
        | ServerResult::ListPromptsResult(ListPromptsResult { result_type, .. })
        | ServerResult::ListResourcesResult(ListResourcesResult { result_type, .. })
        | ServerResult::ListResourceTemplatesResult(ListResourceTemplatesResult { result_type, .. })
        | ServerResult::ReadResourceResult(ReadResourceResult { result_type, .. })
        | ServerResult::ListToolsResult(ListToolsResult { result_type, .. })
        | ServerResult::CallToolResult(CallToolResult { result_type, .. }) = &mut response
        {
            result_type.get_or_insert(ResultType::COMPLETE);
        }
        Ok(response)
    }

    fn subscribe(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, Unserved> {
        Ok(follow(self.peer.clone(), self.notifier.clone(), SubscriptionFilter::builder().resource_subscription(uri).build()))
    }

    async fn serve(&self, stopping: CancellationToken) -> Result<(), BridgeError> {
        let listed = self.listed();
        let _following =
            (!self.version.has_initialize() && listed != SubscriptionFilter::default()).then(|| AbortOnDropHandle::new(tokio::spawn(follow(self.peer.clone(), self.notifier.clone(), listed))));
        let closed = stopping.run_until_cancelled(self.ended.clone()).await.map_or(Ok(()), |()| Err(BridgeError::Ended));
        self.stop.cancel();
        self.ended.clone().await;
        closed
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn follow(peer: Peer<RoleClient>, notifier: Notifier, filter: SubscriptionFilter) {
    let followed = async {
        let mut subscription = peer.listen(filter).await?;
        loop {
            match (subscription.next().await?, subscription.end()) {
                (Some(mut notification), _) => {
                    scrub(notification.get_meta_mut());
                    notifier.notify(notification);
                }
                (None, Some(SubscriptionEnd::Lagged { .. })) => {
                    subscription = peer.listen(subscription.acknowledged().clone()).await?;
                    notifier.resync(subscription.acknowledged());
                }
                (None, end) => break tracing::debug!(end = ?end, "upstream subscription ended"),
            }
        }
        Ok::<(), ServiceError>(())
    };
    followed.unwrap_or_else(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "upstream subscription failed")).await;
}

fn scrub(meta: &mut MetaObject) { meta.retain(|key, _| !key.starts_with("io.modelcontextprotocol/")); }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> process::ExitCode { rasm_mcp::run(env!("CARGO_PKG_VERSION"), Bridge::spawn) }
