use core::future::ready;
use core::mem;
use std::borrow::Cow;
use std::error::Report;
use std::io;

use bytes::{Bytes, BytesMut};
use futures::{Stream, StreamExt as _, TryFutureExt as _, TryStreamExt as _, sink, stream};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::handler::client::progress::ProgressDispatcher;
use rmcp::model::{
    ClientCapabilities, ClientConfig, ClientJsonRpcMessage, ClientNotification, ClientRequest, DEFAULT_MRTR_MAX_ROUNDS, GetMeta as _, InputRequest, InputRequiredResult, InputResponses,
    NotificationMetaObject, ProgressNotificationParam, ProtocolVersion, RequestMetaObject, ServerConfig, ServerJsonRpcMessage, ServerNotification, ServerPeerInfo, ServerRequest, ServerResult,
    SubscriptionFilter,
};
use rmcp::service::{ClientLifecycleMode, NotificationContext, PeerRequestOptions, RequestContext, SubscriptionEnd, serve_client_with_lifecycle_and_ct, serve_server_with_ct};
use rmcp::{ClientHandler, ErrorData, Peer, RoleClient, RoleServer, Service, ServiceError};
use serde::de::DeserializeOwned;
use tokio::io::{AsyncWrite, AsyncWriteExt as _};
use tokio::sync::{broadcast, mpsc, oneshot};
use tokio_util::sync::CancellationToken;
use tokio_util::task::AbortOnDropHandle;

use super::Proxy;
use super::actor::Event;
use super::jsonrpc::{Line, decode, encoded};
use crate::error::internal;
use crate::shared::{Shared, Subscriptions, list_changes, resynced};
use crate::{McpError, Progress, Subscribe};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
struct Client {
    config: ClientConfig,
    progress: ProgressDispatcher,
}

impl ClientHandler for Client {
    fn get_info(&self) -> ClientConfig { self.config.clone() }

    fn on_progress(&self, params: ProgressNotificationParam, _: NotificationContext<RoleClient>) -> impl Future<Output = ()> + Send + '_ { self.progress.handle_notification(params) }
}

#[derive(Debug)]
struct Forward {
    peer: Peer<RoleClient>,
    info: ServerConfig,
    version: ProtocolVersion,
    progress: ProgressDispatcher,
    notifier: broadcast::Sender<ServerNotification>,
    _following: Option<AbortOnDropHandle<()>>,
}

impl Forward {
    async fn sent(&self, request: ClientRequest, context: &RequestContext<RoleServer>, progress: &Progress) -> Result<ServerResult, ErrorData> {
        let options = context
            .client_info()
            .zip(context.client_capabilities())
            .into_iter()
            .fold(PeerRequestOptions::no_options(), |options, (client, capabilities)| options.with_meta(RequestMetaObject::with_client_context(self.version.clone(), client, capabilities)));
        let mut handle = self.peer.send_cancellable_request(request, options).await.map_err(fault)?;
        let mut updates = self.progress.subscribe(handle.progress_token.clone()).await;
        loop {
            tokio::select! {
                biased;
                Some(update) = updates.next() => progress.report(update),
                received = &mut handle.rx => break received.map_err(|oneshot::error::RecvError { .. }| ServiceError::TransportClosed).flatten().map_err(fault),
                () = context.ct.cancelled() => break handle.cancel(None).await.map_err(fault).and(Err(fault(ServiceError::Cancelled { reason: None }))),
            }
        }
    }

    async fn answered(request: &mut ClientRequest, required: InputRequiredResult, context: &RequestContext<RoleServer>) -> Result<(), ErrorData> {
        let InputRequiredResult { input_requests, request_state, .. } = required;
        let asked = stream::iter(input_requests.unwrap_or_default()).then(async |(key, input)| {
            let asked = match input {
                InputRequest::CreateMessage(asked) => ServerRequest::CreateMessageRequest(asked),
                InputRequest::Elicitation(asked) => ServerRequest::ElicitRequest(asked),
                InputRequest::ListRoots(asked) => ServerRequest::ListRootsRequest(asked),
                _ => return Err(fault(ServiceError::UnexpectedResponse)),
            };
            let answer = context.peer.send_request(asked).await.map_err(fault)?;
            serde_json::to_value(answer).map(|answer| (key, answer)).map_err(internal)
        });
        let responses = asked.try_collect::<InputResponses>().await?;
        let (input_responses, state) = match request {
            ClientRequest::CallToolRequest(call) => (&mut call.params.input_responses, &mut call.params.request_state),
            ClientRequest::GetPromptRequest(get) => (&mut get.params.input_responses, &mut get.params.request_state),
            ClientRequest::ReadResourceRequest(read) => (&mut read.params.input_responses, &mut read.params.request_state),
            _ => return Err(fault(ServiceError::UnexpectedResponse)),
        };
        (*input_responses, *state) = (Some(responses).filter(|responses| !responses.is_empty()), request_state);
        Ok(())
    }
}

impl Service<RoleServer> for Forward {
    async fn handle_request(&self, mut request: ClientRequest, context: RequestContext<RoleServer>) -> Result<ServerResult, ErrorData> {
        *request.get_meta_mut() = context.meta.clone();
        let progress = Progress::default();
        let forwarded = async {
            for _round in 0..DEFAULT_MRTR_MAX_ROUNDS {
                match self.sent(request.clone(), &context, &progress).await? {
                    ServerResult::InputRequiredResult(required) => Self::answered(&mut request, required, &context).await?,
                    answer => return Ok(answer),
                }
            }
            Err(fault(ServiceError::InputRequiredRoundsExceeded { max_rounds: DEFAULT_MRTR_MAX_ROUNDS }))
        };
        progress.relay(&context, forwarded).await
    }

    fn handle_notification(&self, _: ClientNotification, _: NotificationContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + Send + '_ { ready(Ok(())) }

    fn get_info(&self) -> ServerConfig { self.info.clone() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { Cow::Borrowed(ProtocolVersion::known_up_to(&ProtocolVersion::LATEST_WITH_INITIALIZE)) }
}

impl Subscribe for Forward {
    fn subscribe(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, McpError> {
        Ok(followed(self.peer.clone(), self.notifier.clone(), SubscriptionFilter::builder().resource_subscription(uri).build()))
    }
}

#[derive(Debug)]
struct Legacy(Subscriptions<Forward>);

impl Service<RoleServer> for Legacy {
    async fn handle_request(&self, request: ClientRequest, context: RequestContext<RoleServer>) -> Result<ServerResult, ErrorData> {
        match request {
            request @ (ClientRequest::InitializeRequest(_)
            | ClientRequest::DiscoverRequest(_)
            | ClientRequest::PingRequest(_)
            | ClientRequest::SubscribeRequest(_)
            | ClientRequest::UnsubscribeRequest(_)
            | ClientRequest::SubscriptionsListenRequest(_)) => self.0.handle_request(request, context).await,
            request => self.0.service().handle_request(request, context).await,
        }
    }

    fn handle_notification(&self, notification: ClientNotification, context: NotificationContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + Send + '_ {
        self.0.handle_notification(notification, context)
    }

    fn get_info(&self) -> ServerConfig { self.0.service().get_info() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { self.0.service().supported_protocol_versions() }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn fault(error: ServiceError) -> ErrorData {
    match error {
        ServiceError::McpError(data) => data,
        other => internal(other),
    }
}

async fn followed(peer: Peer<RoleClient>, notifier: broadcast::Sender<ServerNotification>, filter: SubscriptionFilter) {
    let fanned = |mut notification: ServerNotification| {
        *notification.get_meta_mut() = NotificationMetaObject::default();
        if let Err(broadcast::error::SendError(unheard)) = notifier.send(notification) {
            tracing::trace!(?unheard, "notification had no listener");
        }
    };
    let listened = async {
        let mut subscription = peer.listen(filter).await?;
        loop {
            match (subscription.next().await?, subscription.end()) {
                (Some(notification), _) => fanned(notification),
                (None, Some(SubscriptionEnd::Lagged { capacity: _ })) => {
                    subscription = peer.listen(subscription.acknowledged().clone()).await?;
                    let acknowledged = subscription.acknowledged();
                    resynced(acknowledged, acknowledged.resource_subscriptions.iter().flatten().cloned()).for_each(&fanned);
                }
                (None, end) => break tracing::debug!(?end, "server subscription ended"),
            }
        }
        Ok::<(), ServiceError>(())
    };
    listened.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server subscription failed")).await;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(super) async fn serve<L, W>(proxy: &Proxy, session: u64, lines: L, writer: W, outbound: mpsc::Receiver<Bytes>, cut: CancellationToken) -> io::Result<()>
where
    L: Stream<Item = io::Result<Line>> + Send + Unpin + 'static,
    W: AsyncWrite + Send + Unpin + 'static,
{
    const LISTS: usize = 4;
    fn typed<M>(line: &[u8]) -> Option<M>
    where M: DeserializeOwned {
        decode::<M>(line).inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "message unreadable, skipped")).ok()
    }
    let stop = CancellationToken::new();
    let _stopped = stop.clone().drop_guard();
    let requests = sink::unfold((proxy.events.clone(), session), async |(events, session): (mpsc::Sender<Event>, u64), message: ClientJsonRpcMessage| {
        let sent = events.send(Event::Line { session, line: Line::Whole(encoded(&message)?) }).await;
        sent.map(|()| (events, session)).map_err(|mpsc::error::SendError(_)| io::Error::from(io::ErrorKind::ConnectionAborted))
    });
    let answers = stream::unfold((outbound, BytesMut::new()), async |(mut outbound, mut held)| {
        loop {
            let chunk = outbound.recv().await?;
            held.extend_from_slice(&chunk);
            if chunk.ends_with(b"\n")
                && let Some(message) = typed::<ServerJsonRpcMessage>(&mem::take(&mut held))
            {
                break Some((message, (outbound, held)));
            }
        }
    });
    let asked = lines.filter_map(async |line| match line {
        Ok(Line::Whole(line)) => typed::<ClientJsonRpcMessage>(&line),
        Ok(Line::Oversized { head: _, tail: _ }) => None,
        Err(error) => {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "session link failed");
            None
        }
    });
    let replies = sink::unfold(writer, async |mut writer: W, message: ServerJsonRpcMessage| {
        writer.write_all(&encoded(&message)?).await?;
        writer.flush().await.map(|()| writer)
    });
    let progress = ProgressDispatcher::new();
    let client = Client { config: ClientConfig::new(ClientCapabilities::default(), proxy.client.clone()), progress: progress.clone() };
    let lifecycle = ClientLifecycleMode::Discover { preferred_versions: vec![ProtocolVersion::LATEST] };
    let transport = (Box::pin(requests), Box::pin(answers.take_until(cut.cancelled_owned())));
    let upstream = serve_client_with_lifecycle_and_ct(client, transport, lifecycle, stop.clone()).await.map_err(io::Error::other)?;
    let peer = upstream.peer().clone();
    let discovered = peer.peer_info().ok_or_else(|| io::Error::from(io::ErrorKind::InvalidData))?;
    let ServerPeerInfo { protocol_version: version, mut capabilities, server_info, instructions, meta, .. } = ServerPeerInfo::clone(&discovered);
    capabilities.logging = None;
    let changes = list_changes(&capabilities);
    let mut info = server_info.into_iter().fold(ServerConfig::new(capabilities), ServerConfig::with_server_info);
    (info.instructions, info.meta) = (instructions, meta);
    let shared = Shared::new(LISTS, async |notifier: broadcast::Sender<ServerNotification>| {
        let following = (changes != SubscriptionFilter::default()).then(|| AbortOnDropHandle::new(tokio::spawn(followed(peer.clone(), notifier.clone(), changes))));
        Ok::<_, io::Error>(Forward { peer, info, version, progress, notifier, _following: following })
    })
    .await?;
    let served = Box::pin(serve_server_with_ct(Legacy(shared.subscriptions()), (Box::pin(replies), Box::pin(asked)), stop.clone())).await.map_err(io::Error::other)?;
    tokio::select! {
        quit = served.waiting() => tracing::debug!(?quit, "translated session ended"),
        quit = upstream.waiting() => tracing::debug!(?quit, "server ended the translated session"),
    }
    Ok(())
}
