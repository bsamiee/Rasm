use core::{error::Error, future::ready};
use std::{borrow::Cow, error::Report, io, sync::Arc};

use arc_swap::ArcSwap;
use futures::{
    FutureExt as _, StreamExt as _, TryFutureExt as _,
    future::{BoxFuture, Shared},
};
use opentelemetry::{Context, global};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
#[expect(deprecated, reason = "legacy upstreams log through notifications/message and subscribe through resources/subscribe")]
use rmcp::model::{LoggingLevel, LoggingMessageNotificationParam, SubscribeRequestParams};
use rmcp::{
    ClientLifecycleMode, ErrorData, Peer, RoleClient, RoleServer, Service, ServiceError,
    handler::client::progress::ProgressDispatcher,
    model::{
        ClientCapabilities, ClientConfig, ClientNotification, ClientRequest, ClientResult, GetMeta as _, Implementation, NotificationMetaObject, ProtocolVersion, RequestMetaObject, ServerConfig,
        ServerNotification, ServerPeerInfo, ServerRequest, ServerResult, SubscriptionFilter,
    },
    service::{ClientInitializeError, NotificationContext, PeerRequestOptions, RequestContext, SubscriptionEnd, serve_client_with_lifecycle_and_ct},
    transport::IntoTransport,
};
use tokio::sync::{Mutex, broadcast, oneshot};
use tokio_util::{
    sync::{CancellationToken, DropGuard},
    task::AbortOnDropHandle,
};

use crate::{Endpoint, McpError, Meta, Progress, Update, internal, list_changes, resynced, versions};

// --- [TYPES] ---------------------------------------------------------------------------

type Connect = Box<dyn Fn() -> BoxFuture<'static, Result<Link, McpError>> + Send + Sync>;

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(derive_more::Debug)]
pub struct Proxy {
    #[debug(skip)]
    connect: Connect,
    notifier: broadcast::Sender<ServerNotification>,
    link: ArcSwap<Link>,
    relinking: Mutex<()>,
}

impl Proxy {
    pub async fn connect<C, F, T, E, A>(identity: Implementation, notifier: broadcast::Sender<ServerNotification>, transport: C) -> Result<Self, McpError>
    where
        C: Fn() -> F + Send + Sync + 'static,
        F: Future<Output = io::Result<T>> + Send + 'static,
        T: IntoTransport<RoleClient, E, A> + Send + 'static,
        E: Error + Send + Sync + 'static,
        A: Send + 'static,
    {
        let upstream = notifier.clone();
        let connect: Connect = Box::new(move || {
            let (identity, notifier, opened) = (identity.clone(), upstream.clone(), transport());
            async move {
                let progress = ProgressDispatcher::new();
                let client = Client { config: ClientConfig::new(ClientCapabilities::default(), identity), notifier: notifier.clone(), progress: progress.clone() };
                let lifecycle = ClientLifecycleMode::Auto { preferred_versions: vec![ProtocolVersion::LATEST], legacy_version: Some(ProtocolVersion::LATEST_WITH_INITIALIZE) };
                let stop = CancellationToken::new();
                let service = serve_client_with_lifecycle_and_ct(client, opened.await.map_err(|source| McpError::Connect { source })?, lifecycle, stop.clone())
                    .await
                    .map_err(|source| McpError::Handshake { source: Box::new(source) })?;
                let peer = service.peer().clone();
                let ServerPeerInfo { protocol_version: version, mut capabilities, server_info, instructions, meta, .. } =
                    peer.peer_info().map(Arc::unwrap_or_clone).ok_or_else(|| McpError::Handshake { source: Box::new(ClientInitializeError::ExpectedInitResult(None)) })?;
                capabilities.logging = None;
                let changes = list_changes(&capabilities);
                let following = (!version.has_initialize() && changes != SubscriptionFilter::default()).then(|| AbortOnDropHandle::new(tokio::spawn(followed(peer.clone(), notifier, changes))));
                let mut info = server_info.into_iter().fold(ServerConfig::new(capabilities), ServerConfig::with_server_info);
                (info.instructions, info.meta) = (instructions, meta);
                tracing::info!(%version, "upstream linked");
                let ended = service.waiting().map(|quit| tracing::info!(?quit, "upstream ended")).boxed().shared();
                Ok(Link { peer, ended, info, version, progress, _following: following, _stop: stop.drop_guard() })
            }
            .boxed()
        });
        let link = connect().await?;
        Ok(Self { connect, notifier, link: ArcSwap::from_pointee(link), relinking: Mutex::new(()) })
    }

    async fn live(&self) -> Result<Arc<Link>, McpError> {
        let open = |link: Arc<Link>| Some(link).filter(|link| !link.peer.is_transport_closed());
        if let Some(link) = open(self.link.load_full()) {
            return Ok(link);
        }
        let _relinking = self.relinking.lock().await;
        if let Some(link) = open(self.link.load_full()) {
            return Ok(link);
        }
        let link = Arc::new((self.connect)().await?);
        self.link.store(Arc::clone(&link));
        resynced(&list_changes(&link.info.capabilities), []).for_each(|notification| drop(self.notifier.send(notification)));
        Ok(link)
    }
}

impl Service<RoleServer> for Proxy {
    async fn handle_request(&self, mut request: ClientRequest, context: RequestContext<RoleServer>) -> Result<ServerResult, ErrorData> {
        let fault = |error: ServiceError| match error {
            ServiceError::McpError(data) => data,
            ServiceError::TransportClosed => McpError::Ended.into(),
            other => internal(other),
        };
        let link = self.live().await?;
        let meta = request.get_meta_mut();
        *meta = context.meta.clone();
        global::get_text_map_propagator(|propagator| propagator.inject_context(&Context::current(), &mut Meta(&mut meta.0)));
        let options = context
            .client_info()
            .zip(context.client_capabilities())
            .filter(|_| !link.version.has_initialize() && context.meta.protocol_version().is_none())
            .into_iter()
            .fold(PeerRequestOptions::no_options(), |options, (client, capabilities)| options.with_meta(RequestMetaObject::with_client_context(link.version.clone(), client, capabilities)));
        let progress = Progress::default();
        let forwarded = async {
            let mut handle = link.peer.send_cancellable_request(request, options).await.map_err(fault)?;
            let mut updates = link.progress.subscribe(handle.progress_token.clone()).await;
            loop {
                tokio::select! {
                    biased;
                    Some(update) = updates.next() => progress.report(Update { progress: update.progress, total: update.total, message: update.message }),
                    received = &mut handle.rx => break received.map_err(|oneshot::error::RecvError { .. }| ServiceError::TransportClosed).flatten().map_err(fault),
                    () = context.ct.cancelled() => break handle.cancel(None).await.map_err(fault).and(Err(fault(ServiceError::Cancelled { reason: None }))),
                }
            }
        };
        progress.relay(&context, forwarded).await
    }

    fn handle_notification(&self, _: ClientNotification, _: NotificationContext<RoleServer>) -> impl Future<Output = Result<(), ErrorData>> + Send + '_ { ready(Ok(())) }

    fn get_info(&self) -> ServerConfig { self.link.load().info.clone() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { Cow::Borrowed(versions()) }
}

impl Endpoint for Proxy {
    fn watch(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, McpError> {
        let (link, notifier, uri) = (self.link.load_full(), self.notifier.clone(), uri.to_owned());
        Ok(async move {
            if !link.version.has_initialize() {
                return followed(link.peer.clone(), notifier, SubscriptionFilter::builder().resource_subscription(uri).build()).await;
            }
            #[expect(deprecated, reason = "resources/subscribe stays the legacy era's only subscription request")]
            match link.peer.subscribe(SubscribeRequestParams::new(uri.clone())).await {
                Ok(()) => link.ended.clone().await,
                Err(error) => tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), uri, "upstream refused subscription"),
            }
        })
    }
}

#[derive(derive_more::Debug)]
struct Link {
    peer: Peer<RoleClient>,
    #[debug(skip)]
    ended: Shared<BoxFuture<'static, ()>>,
    info: ServerConfig,
    version: ProtocolVersion,
    progress: ProgressDispatcher,
    _following: Option<AbortOnDropHandle<()>>,
    _stop: DropGuard,
}

#[derive(Debug)]
struct Client {
    config: ClientConfig,
    notifier: broadcast::Sender<ServerNotification>,
    progress: ProgressDispatcher,
}

impl Service<RoleClient> for Client {
    async fn handle_request(&self, request: ServerRequest, context: RequestContext<RoleClient>) -> Result<ClientResult, ErrorData> { self.config.handle_request(request, context).await }

    #[expect(deprecated, reason = "legacy upstreams log through notifications/message, which the host log records")]
    async fn handle_notification(&self, notification: ServerNotification, _: NotificationContext<RoleClient>) -> Result<(), ErrorData> {
        match notification {
            ServerNotification::ProgressNotification(progress) => self.progress.handle_notification(progress.params).await,
            ServerNotification::LoggingMessageNotification(message) => {
                let LoggingMessageNotificationParam { level, logger, data, .. } = message.params;
                let logger = logger.as_deref();
                match level {
                    LoggingLevel::Debug => tracing::debug!(logger, %data, "upstream log"),
                    LoggingLevel::Info | LoggingLevel::Notice => tracing::info!(logger, %data, "upstream log"),
                    LoggingLevel::Warning => tracing::warn!(logger, %data, "upstream log"),
                    LoggingLevel::Error | LoggingLevel::Critical | LoggingLevel::Alert | LoggingLevel::Emergency => tracing::error!(logger, %data, "upstream log"),
                }
            }
            other => fanned(&self.notifier, other),
        }
        Ok(())
    }

    fn get_info(&self) -> ClientConfig { self.config.clone() }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn fanned(notifier: &broadcast::Sender<ServerNotification>, mut notification: ServerNotification) {
    *notification.get_meta_mut() = NotificationMetaObject::default();
    drop(notifier.send(notification));
}

async fn followed(peer: Peer<RoleClient>, notifier: broadcast::Sender<ServerNotification>, filter: SubscriptionFilter) {
    let listened = async {
        let mut subscription = peer.listen(filter).await?;
        loop {
            match (subscription.next().await?, subscription.end()) {
                (Some(notification), _) => fanned(&notifier, notification),
                (None, Some(SubscriptionEnd::Lagged { capacity: _ })) => {
                    subscription = peer.listen(subscription.acknowledged().clone()).await?;
                    let acknowledged = subscription.acknowledged();
                    resynced(acknowledged, acknowledged.resource_subscriptions.iter().flatten().cloned()).for_each(|notification| fanned(&notifier, notification));
                }
                (None, end) => break tracing::debug!(?end, "upstream subscription ended"),
            }
        }
        Ok::<(), ServiceError>(())
    };
    listened.unwrap_or_else(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "upstream subscription failed")).await;
}
