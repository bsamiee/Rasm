#![feature(try_blocks_heterogeneous)]

use core::{fmt::Debug, future, iter, net::SocketAddr, pin::pin, time::Duration};
use std::{collections::HashSet, io, sync::Arc};

use axum::{
    Router,
    extract::connect_info::Connected,
    serve::{IncomingStream, Listener},
};
use futures::{
    FutureExt as _, Stream, StreamExt as _,
    future::{join_all, join3, maybe_done},
};
use http::uri::Authority;
use itertools::Itertools as _;
use jiff::fmt::friendly::SpanPrinter;
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use opentelemetry_semantic_conventions::attribute::{SERVER_ADDRESS, SERVER_PORT};
use socket2::SockRef;
use tokio::{
    net::{self, TcpListener, TcpSocket},
    task::JoinSet,
    time,
};
use tokio_util::sync::CancellationToken;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait App {
    type Error: Send;

    fn router(self: Arc<Self>, stopping: CancellationToken) -> Router;

    fn lifetime(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send { stopping.cancelled_owned().map(Ok) }

    fn drain(&self) -> impl Future<Output = Result<(), Self::Error>> + Send { future::ready(Ok(())) }
}

#[derive(Debug, Clone, Copy, derive_more::Display)]
#[display(rename_all = "lowercase")]
enum Trigger {
    Request,
    Lifetime,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Stage {
    Drain,
    Lifetime,
    Server,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Operation {
    Bind,
    #[display("read address")]
    Address,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Abandonment {
    #[display("second shutdown request")]
    Request,
    #[display("grace deadline {}", SpanPrinter::new().unsigned_duration_to_string(_0))]
    Deadline(Duration),
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum ServerError<X> {
    #[error("cannot resolve {authority}")]
    Resolve { authority: Authority, source: Option<io::Error> },
    #[error("cannot {operation} {address}")]
    Io { operation: Operation, address: SocketAddr, source: io::Error },
    #[error("cannot serve")]
    Serve { source: io::Error },
    #[error(transparent)]
    App(X),
    #[error("{reason} abandoned shutdown with {} running", .pending.iter().format(", "))]
    Abandoned { reason: Abandonment, pending: NEVec<Stage> },
}

impl<X> From<ServerError<X>> for sysexits::ExitCode
where X: Into<Self>
{
    fn from(value: ServerError<X>) -> Self {
        match value {
            ServerError::Resolve { authority: _, source: _ } => Self::NoHost,
            ServerError::Io { operation: _, address: _, source } | ServerError::Serve { source } => source.into(),
            ServerError::App(source) => source.into(),
            ServerError::Abandoned { reason: _, pending: _ } => Self::TempFail,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn bind<X>(listen: impl IntoIterator<Item = Authority>, port: u16) -> Result<Vec<(Authority, NEVec<(SocketAddr, TcpListener)>)>, NEVec<ServerError<X>>> {
    let resolved = join_all(listen.into_iter().map(async |authority| {
        let found = net::lookup_host(format!("{}:{}", authority.host(), authority.port_u16().unwrap_or(port)))
            .await
            .map_err(Some)
            .and_then(|found| found.try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>).ok_or(None))
            .map_err(|source| ServerError::Resolve { authority: authority.clone(), source })?;
        Ok((authority, found))
    }))
    .await
    .into_iter()
    .collect::<Validated<Vec<_>, _>>()
    .ok()?;
    let ipv4_ports = resolved.iter().flat_map(|(_, found)| found).filter(|address| address.is_ipv4()).map(SocketAddr::port).collect::<HashSet<_>>();
    let bind = |requested: SocketAddr, port: u16| {
        const BACKLOG: u32 = 128;
        let address = SocketAddr::new(requested.ip(), port);
        let failed = |operation: Operation| move |source: io::Error| ServerError::<X>::Io { operation, address, source };
        let listener = try bikeshed io::Result<TcpListener> {
            let socket = match address {
                SocketAddr::V4(_) => TcpSocket::new_v4()?,
                SocketAddr::V6(v6) => {
                    let socket = TcpSocket::new_v6()?;
                    SockRef::from(&socket).set_only_v6(v6.ip().is_unspecified() && ipv4_ports.contains(&requested.port()))?;
                    socket
                }
            };
            #[cfg(not(windows))]
            socket.set_reuseaddr(true)?;
            socket.bind(address)?;
            socket.listen(BACKLOG)?
        }
        .map_err(failed(Operation::Bind))?;
        let bound = listener.local_addr().map_err(failed(Operation::Address))?;
        tracing::info!({ SERVER_ADDRESS } = %bound.ip(), { SERVER_PORT } = bound.port(), "listening");
        Ok((bound, listener))
    };
    resolved
        .into_iter()
        .map(|(authority, found)| {
            let (&first, rest) = found.split_first();
            bind(first, first.port()).map_or_else(Validated::fail, |head @ (shared, _)| {
                rest.iter().map(|&requested| bind(requested, shared.port())).collect::<Validated<Vec<_>, _>>().map(|tail| (authority, NEVec::from((head, tail))))
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

pub async fn serve<A, L, S>(listeners: impl IntoIterator<Item = L>, app: Arc<A>, requests: S, grace: Duration) -> Result<(), NEVec<ServerError<A::Error>>>
where
    A: App,
    L: Listener,
    L::Addr: Debug + Clone + Sync + 'static,
    for<'stream> L::Addr: Connected<IncomingStream<'stream, L>>,
    S: Stream,
{
    let stopping = CancellationToken::new();
    let _stopped = stopping.drop_guard_ref();
    let service = Arc::clone(&app).router(stopping.child_token()).into_make_service_with_connect_info::<L::Addr>();
    let servers = listeners.into_iter().map(|listener| axum::serve(listener, service.clone()).with_graceful_shutdown(stopping.clone().cancelled_owned()).into_future()).collect::<JoinSet<_>>();
    let mut server = pin!(maybe_done(servers.join_all()));
    let mut lifetime = pin!(maybe_done(app.lifetime(stopping.child_token())));
    let mut requests = pin!(requests);
    let trigger = tokio::select! {
        Some(_) = requests.next() => Trigger::Request,
        () = lifetime.as_mut() => Trigger::Lifetime,
    };
    tracing::info!(%trigger, "shutting down");
    let mut drain = pin!(maybe_done(app.drain().inspect(|_| stopping.cancel())));
    let abandonment = tokio::select! {
        biased;
        ((), (), ()) = join3(drain.as_mut(), lifetime.as_mut(), server.as_mut()) => None,
        (Some(_), _) = requests.skip(match trigger { Trigger::Request => 0, Trigger::Lifetime => 1 }).into_future() => Some(Abandonment::Request),
        () = time::sleep(grace) => Some(Abandonment::Deadline(grace)),
    };
    let (drained, ended, closed) = (drain.as_mut().take_output(), lifetime.as_mut().take_output(), server.as_mut().take_output());
    let pending = [drained.is_none().then_some(Stage::Drain), ended.is_none().then_some(Stage::Lifetime), closed.is_none().then_some(Stage::Server)]
        .into_iter()
        .flatten()
        .try_into_nonempty_iter()
        .map(NonEmptyIterator::collect::<NEVec<_>>);
    iter::chain(drained, ended)
        .map(|outcome| outcome.map_err(ServerError::App))
        .chain(closed.into_iter().flatten().map(|outcome| outcome.map_err(|source| ServerError::Serve { source })))
        .chain(abandonment.zip(pending).map(|(reason, pending)| Err(ServerError::Abandoned { reason, pending })))
        .collect::<Validated<(), _>>()
        .ok()
}
