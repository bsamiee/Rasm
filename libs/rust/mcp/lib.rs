use std::{error::Error, io, net::Ipv4Addr, num::NonZero, sync::Arc};

use axum::{Router, body::Body, http::Request};
use nonempty_collections::NEVec;
use rmcp::{
    ClientCacheConfig, ClientHandler, ClientLifecycleMode, ErrorData, Peer, RoleClient, ServerHandler, Service, ServiceError,
    model::{ClientConfig, ClientResult, GetExtensions as _, ProtocolVersion, ServerNotification, ServerPeerInfo, ServerRequest, SubscriptionFilter},
    service::{ClientInitializeError, DEFAULT_SUBSCRIPTION_CHANNEL_CAPACITY, NotificationContext, RequestContext, SubscriptionEnd, serve_client_with_lifecycle_and_ct},
    transport::{
        IntoTransport, StreamableHttpServerConfig, StreamableHttpService,
        common::http_header::{HEADER_MCP_METHOD, HEADER_MCP_PROTOCOL_VERSION},
        streamable_http_server::session::local::LocalSessionManager,
    },
};
#[cfg(unix)]
use tokio::signal::unix::{SignalKind, signal};
#[cfg(windows)]
use tokio::signal::windows::{ctrl_c, ctrl_close};
use tokio::{net::TcpListener, sync::broadcast};
use tokio_util::{
    sync::CancellationToken,
    task::{AbortOnDropHandle, TaskTracker},
};
use tower_http::trace::TraceLayer;
use tracing_subscriber::{
    EnvFilter,
    filter::{FromEnvError, LevelFilter},
    fmt,
    layer::SubscriberExt as _,
    util::{SubscriberInitExt as _, TryInitError},
};
use validated::Validated;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum ClientError {
    #[error("MCP lifecycle failed")]
    Lifecycle(#[source] Box<ClientInitializeError>),
    #[error("cannot subscribe to server notifications")]
    Subscribe(#[source] ServiceError),
}

#[derive(Debug, thiserror::Error)]
pub enum HostError {
    #[error("cannot listen for shutdown signals")]
    Signal(NEVec<io::Error>),
    #[error("cannot bind {}:{port}", Ipv4Addr::LOCALHOST)]
    Bind { port: NonZero<u16>, source: io::Error },
    #[error("HTTP server failed")]
    Serve(#[source] io::Error),
}

#[derive(Debug, thiserror::Error)]
pub enum TelemetryError {
    #[error("{} holds an invalid filter", EnvFilter::DEFAULT_ENV)]
    Filter(#[from] FromEnvError),
    #[error("cannot install the global tracing subscriber")]
    Install(#[from] TryInitError),
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Handler<H> {
    inner: H,
    notifications: broadcast::Sender<ServerNotification>,
}

impl<H: ClientHandler> Service<RoleClient> for Handler<H> {
    fn handle_request(&self, request: ServerRequest, context: RequestContext<RoleClient>) -> impl Future<Output = Result<ClientResult, ErrorData>> + Send + '_ {
        self.inner.handle_request(request, context)
    }

    fn handle_notification(&self, notification: ServerNotification, context: NotificationContext<RoleClient>) -> impl Future<Output = Result<(), ErrorData>> + Send + '_ {
        let mut published = notification.clone();
        *published.extensions_mut() = context.extensions.clone();
        publish(&self.notifications, published);
        self.inner.handle_notification(notification, context)
    }

    fn get_info(&self) -> ClientConfig { ClientHandler::get_info(&self.inner) }
}

#[derive(Clone, Debug)]
pub struct Client {
    peer: Peer<RoleClient>,
    info: Arc<ServerPeerInfo>,
    notifications: broadcast::Sender<ServerNotification>,
    _waiting: Arc<AbortOnDropHandle<()>>,
    tracker: TaskTracker,
    stop: CancellationToken,
}

impl Client {
    // --- [LIFECYCLE]
    pub async fn connect<H, T, E, A>(handler: H, transport: T, cache: ClientCacheConfig) -> Result<Self, ClientError>
    where
        H: ClientHandler,
        T: IntoTransport<RoleClient, E, A>,
        E: Error + Send + Sync + 'static,
    {
        let notifications = broadcast::Sender::new(DEFAULT_SUBSCRIPTION_CHANNEL_CAPACITY);
        let stop = CancellationToken::new();
        let lifecycle = ClientLifecycleMode::Auto { preferred_versions: ProtocolVersion::KNOWN_VERSIONS.iter().rev().cloned().collect(), legacy_version: None };
        let service = serve_client_with_lifecycle_and_ct(Handler { inner: handler, notifications: notifications.clone() }, transport, lifecycle, stop.clone())
            .await
            .map_err(|error| ClientError::Lifecycle(Box::new(error)))?;
        let peer = service.peer().clone();
        let info = peer.peer_info().ok_or_else(|| ClientError::Lifecycle(Box::new(ClientInitializeError::ExpectedInitResult(None))))?;
        peer.set_response_cache_config(cache).await;
        let tracker = TaskTracker::new();
        let waiting = AbortOnDropHandle::new(tracker.spawn(async move {
            if let Err(error) = service.waiting().await {
                tracing::error!(%error, "session task failed");
            }
        }));
        tracker.close();
        Ok(Self { peer, info, notifications, _waiting: Arc::new(waiting), tracker, stop })
    }

    pub async fn ended(&self) { self.tracker.wait().await; }

    pub async fn stop(&self) {
        self.stop.cancel();
        self.ended().await;
    }

    // --- [READS]
    #[must_use]
    pub const fn peer(&self) -> &Peer<RoleClient> { &self.peer }

    #[must_use]
    pub fn info(&self) -> &ServerPeerInfo { &self.info }

    #[must_use]
    pub fn notifications(&self) -> broadcast::Receiver<ServerNotification> { self.notifications.subscribe() }

    // --- [SUBSCRIPTIONS]
    pub async fn subscribe(&self, filter: SubscriptionFilter) -> Result<AbortOnDropHandle<()>, ClientError> {
        let mut subscription = self.peer.listen(filter).await.map_err(ClientError::Subscribe)?;
        let (peer, notifications) = (self.peer.clone(), self.notifications.clone());
        Ok(AbortOnDropHandle::new(tokio::spawn(async move {
            loop {
                match (subscription.next().await, subscription.end()) {
                    (Ok(Some(notification)), _) => publish(&notifications, notification),
                    (Ok(None), Some(SubscriptionEnd::Lagged { capacity: _ })) => match peer.listen(subscription.acknowledged().clone()).await {
                        Ok(reopened) => subscription = reopened,
                        Err(error) => break tracing::warn!(%error, "subscription reopen failed"),
                    },
                    (Ok(None), end) => break tracing::debug!(?end, "subscription ended"),
                    (Err(error), _) => break tracing::warn!(%error, "subscription failed"),
                }
            }
        })))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn publish(notifications: &broadcast::Sender<ServerNotification>, notification: ServerNotification) {
    let receivers = notifications.send(notification).unwrap_or_default();
    tracing::trace!(receivers, "notification published");
}

// --- [COMPOSITION] ---------------------------------------------------------------------

// --- [HOST]
pub async fn serve_loopback<S, F>(factory: F, port: NonZero<u16>, config: StreamableHttpServerConfig) -> Result<(), HostError>
where
    S: ServerHandler,
    F: Fn() -> Result<S, io::Error> + Send + Sync + 'static,
{
    #[cfg(unix)]
    let (interrupt, terminate) = (signal(SignalKind::interrupt()), signal(SignalKind::terminate()));
    #[cfg(windows)]
    let (interrupt, terminate) = (ctrl_c(), ctrl_close());
    let (mut interrupt, mut terminate) = Validated::from(interrupt).map2(terminate.into(), |interrupt, terminate| (interrupt, terminate)).ok().map_err(HostError::Signal)?;
    let listener = TcpListener::bind((Ipv4Addr::LOCALHOST, port.get())).await.map_err(|source| HostError::Bind { port, source })?;
    let shutdown = config.cancellation_token.clone();
    let service = StreamableHttpService::new(factory, LocalSessionManager::default().into(), config);
    let router = Router::new().route_service("/", service).layer(TraceLayer::new_for_http().make_span_with(|request: &Request<Body>| {
        let header = |name| request.headers().get(name).and_then(|value| value.to_str().ok());
        tracing::info_span!(
            "request",
            http.request.method = %request.method(),
            mcp.method.name = header(HEADER_MCP_METHOD),
            mcp.protocol.version = header(HEADER_MCP_PROTOCOL_VERSION)
        )
    }));
    tracing::info!(%port, "listening");
    axum::serve(listener, router)
        .with_graceful_shutdown(async move {
            tokio::select! {
                _ = interrupt.recv() => {}
                _ = terminate.recv() => {}
                () = shutdown.cancelled() => {}
            }
            tracing::info!("shutting down");
            shutdown.cancel();
        })
        .await
        .map_err(HostError::Serve)
}

// --- [TELEMETRY]
pub fn install_tracing() -> Result<(), TelemetryError> {
    let filter = EnvFilter::builder().with_default_directive(LevelFilter::INFO.into()).from_env()?;
    Ok(tracing_subscriber::registry().with(fmt::layer().with_writer(io::stderr)).with(filter).try_init()?)
}
