#![feature(error_reporter, try_blocks_heterogeneous)]

use core::{cell::LazyCell, iter, net::SocketAddr, pin::pin, time::Duration};
use std::{collections::HashSet, error::Report, io, process::ExitCode, sync::Arc};

use axum::{Router, serve::ListenerExt as _};
use clap::{Args, CommandFactory, FromArgMatches};
use futures::{FutureExt as _, StreamExt as _, future};
use http::uri::Authority;
use itertools::Itertools as _;
use jiff::fmt::friendly::{SpanParser, SpanPrinter};
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use opentelemetry_semantic_conventions::attribute::{EXCEPTION_MESSAGE, SERVER_ADDRESS, SERVER_PORT};
use rasm_command::{Augmented, CommandError, Process};
use rasm_telemetry::{Defaults, Failure, Handle, Layer};
use socket2::SockRef;
use tokio::{
    net::{self, TcpSocket},
    time,
};
use tokio_util::sync::CancellationToken;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Lifetime {
    type Error: Failure;

    fn lifetime(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send { stopping.cancelled_owned().map(Ok) }
}

pub trait Daemon: Lifetime {
    fn router(self: Arc<Self>) -> Router;

    fn drain(&self) -> impl Future<Output = Result<(), Self::Error>> + Send { async { Ok(()) } }
}

#[derive(Debug, Clone, Copy, derive_more::Display)]
#[display(rename_all = "lowercase")]
enum Trigger {
    Signal,
    Lifetime,
    Server,
}

#[derive(Debug, Clone, Copy, derive_more::Display)]
#[display(rename_all = "lowercase")]
enum Stage {
    Drain,
    Lifetime,
    Server,
}

#[derive(Debug, Clone, Copy, derive_more::Display)]
#[display(rename_all = "lowercase")]
enum Operation {
    Bind,
    #[display("read address")]
    Address,
    Listen,
    Serve,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args)]
#[group(skip)]
#[command(next_help_heading = "Daemon")]
struct Options {
    #[arg(long, env, required = true, value_delimiter = ',', value_name = "HOST:PORT", help = "Host and port serving HTTP on each address the host resolves to, port 0 for one the system assigns")]
    listen: Vec<Authority>,
    #[arg(
        long,
        env,
        value_name = "DURATION",
        value_parser = |text: &str| SpanParser::new().parse_unsigned_duration(text),
        help = "Drain time after shutdown starts before the process abandons it, unbounded when unset"
    )]
    grace: Option<Duration>,
}

#[derive(Debug, Clone, Copy, derive_more::Display)]
enum Abandonment {
    #[display("second shutdown signal")]
    Signal,
    #[display("grace deadline {}", SpanPrinter::new().unsigned_duration_to_string(_0))]
    Deadline(Duration),
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum DaemonError<X> {
    #[error(transparent)]
    Command(#[from] CommandError),
    #[error("cannot resolve {authority}")]
    Resolve { authority: Authority, source: Option<io::Error> },
    #[error("cannot {operation} {address}")]
    Io { operation: Operation, address: SocketAddr, source: io::Error },
    #[error(transparent)]
    Daemon(X),
    #[error("{reason} abandoned shutdown with {} running", .pending.iter().format(", "))]
    Abandoned { reason: Abandonment, pending: NEVec<Stage> },
}

impl<X> From<DaemonError<X>> for NEVec<DaemonError<X>> {
    fn from(value: DaemonError<X>) -> Self { Self::new(value) }
}

impl<X> From<DaemonError<X>> for sysexits::ExitCode
where X: Into<Self>
{
    fn from(value: DaemonError<X>) -> Self {
        match value {
            DaemonError::Command(source) => source.into(),
            DaemonError::Resolve { authority: _, source: _ } => Self::NoHost,
            DaemonError::Io { operation: _, address: _, source } => source.into(),
            DaemonError::Daemon(source) => source.into(),
            DaemonError::Abandoned { reason: _, pending: _ } => Self::TempFail,
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<A, L, D>(defaults: Defaults, layer: L, start: impl AsyncFnOnce(A, &[(Authority, NEVec<SocketAddr>)], Handle, CancellationToken) -> Result<D, D::Error>) -> ExitCode
where
    A: CommandFactory + FromArgMatches,
    L: Layer,
    D: Daemon,
{
    rasm_command::run(defaults, io::stderr, layer, async |Process { args: Augmented { options: Options { listen, grace }, args }, styles: _, mut signals, filter, level: _ }| {
        let failed = |operation: Operation, address: SocketAddr| move |source: io::Error| DaemonError::<D::Error>::Io { operation, address, source };
        let resolved = future::join_all(listen.into_iter().map(async |authority| {
            let found = net::lookup_host(authority.as_str()).await.map_err(Some).and_then(|found| found.try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>).ok_or(None));
            found.map_err(|source| DaemonError::<D::Error>::Resolve { authority: authority.clone(), source }).map(|found| (authority, found))
        }))
        .await
        .into_iter()
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
        let ipv4_ports = LazyCell::new(|| resolved.iter().flat_map(|(_, found)| found).filter(|address| address.is_ipv4()).map(SocketAddr::port).collect::<HashSet<_>>());
        let bind = |requested: SocketAddr, port: u16| {
            let mut address = requested;
            address.set_port(port);
            let paired = requested.is_ipv6() && requested.ip().is_unspecified() && ipv4_ports.contains(&requested.port());
            let socket = try bikeshed io::Result<TcpSocket> {
                let socket = if address.is_ipv4() { TcpSocket::new_v4() } else { TcpSocket::new_v6() }?;
                if paired {
                    SockRef::from(&socket).set_only_v6(true)?;
                }
                #[cfg(not(windows))]
                socket.set_reuseaddr(true)?;
                socket.bind(address)?;
                socket
            }
            .map_err(failed(Operation::Bind, address))?;
            let bound = socket.local_addr().map_err(failed(Operation::Address, address))?;
            socket.listen(u32::MAX).map(|listener| (bound, listener)).map_err(failed(Operation::Listen, bound))
        };
        let rows = resolved
            .iter()
            .map(|(_, found)| {
                let (&first, rest) = found.split_first();
                bind(first, first.port()).map_or_else(Validated::fail, |head @ (shared, _)| {
                    rest.iter().map(|&requested| bind(requested, shared.port())).collect::<Validated<Vec<_>, _>>().map(|tail| NEVec::from((head, tail)))
                })
            })
            .collect::<Validated<Vec<_>, _>>()
            .ok()?;
        drop(ipv4_ports);
        let addresses = iter::zip(resolved, &rows).map(|((authority, _), listeners)| (authority, listeners.nonempty_iter().map(|&(address, _)| address).collect::<NEVec<_>>())).collect::<Vec<_>>();
        let stopping = CancellationToken::new();
        let _stopped = stopping.drop_guard_ref();
        let daemon = Arc::new(tokio::select! {
            started = start(args, &addresses, filter, stopping.child_token()) => started.map_err(DaemonError::Daemon)?,
            _ = signals.next() => return Ok(()),
        });
        drop(addresses);
        let service = Arc::clone(&daemon).router().into_make_service_with_connect_info::<SocketAddr>();
        let serving = future::join_all(rows.into_iter().flatten().map(|(address, listener)| {
            tracing::info!({ SERVER_ADDRESS } = %address.ip(), { SERVER_PORT } = address.port(), "listening");
            let accepted = listener.tap_io(|stream| stream.set_nodelay(true).unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "TCP_NODELAY refused")));
            axum::serve(accepted, service.clone()).with_graceful_shutdown(stopping.clone().cancelled_owned()).into_future().map(move |outcome| outcome.map_err(failed(Operation::Serve, address)))
        }));
        let mut server = pin!(future::maybe_done(serving));
        let mut lifetime = pin!(future::maybe_done(daemon.lifetime(stopping.child_token())));
        let trigger = tokio::select! {
            _ = signals.next() => Trigger::Signal,
            () = lifetime.as_mut() => Trigger::Lifetime,
            () = server.as_mut() => Trigger::Server,
        };
        tracing::info!(%trigger, "shutting down");
        let mut drain = pin!(future::maybe_done(daemon.drain().inspect(|_| stopping.cancel())));
        let abandonment = tokio::select! {
            biased;
            ((), (), ()) = future::join3(drain.as_mut(), lifetime.as_mut(), server.as_mut()) => None,
            _ = signals.skip(match trigger { Trigger::Signal => 0, Trigger::Lifetime | Trigger::Server => 1 }).into_future() => Some(Abandonment::Signal),
            Some(()) = future::OptionFuture::from(grace.map(time::sleep)) => grace.map(Abandonment::Deadline),
        };
        let (drained, ended, closed) = (drain.as_mut().take_output(), lifetime.as_mut().take_output(), server.as_mut().take_output());
        let pending = [drained.is_none().then_some(Stage::Drain), ended.is_none().then_some(Stage::Lifetime), closed.is_none().then_some(Stage::Server)].into_iter().flatten().collect::<Vec<_>>();
        let abandoned = abandonment.zip(NEVec::try_from_vec(pending)).map(|(reason, pending)| DaemonError::Abandoned { reason, pending });
        iter::chain(drained, ended).map(|outcome| outcome.map_err(DaemonError::Daemon)).chain(closed.into_iter().flatten()).chain(abandoned.map(Err)).collect::<Validated<(), _>>().ok()
    })
}
