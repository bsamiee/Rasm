#![feature(error_reporter, try_blocks_heterogeneous)]

use core::{future::ready, iter, net::SocketAddr, pin::pin, time::Duration};
use std::{error::Report, io, process::ExitCode, sync::Arc};

use axum::{Router, serve::ListenerExt as _};
use clap::{Args, CommandFactory, FromArgMatches};
use futures::{FutureExt as _, StreamExt as _, future};
use jiff::fmt::friendly::SpanParser;
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::{EXCEPTION_MESSAGE, SERVER_ADDRESS, SERVER_PORT};
use rasm_command::{Augmented, CommandError, Process};
use rasm_telemetry::{Defaults, Failure, Handle, Layer};
use tokio::{net::TcpSocket, time};
use tokio_util::sync::CancellationToken;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Daemon {
    type Error: Failure;

    fn router(self: Arc<Self>) -> Router;

    fn serve(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send { stopping.cancelled_owned().map(Ok) }

    fn drain(&self) -> impl Future<Output = Result<(), Self::Error>> + Send { ready(()).map(Ok) }
}

#[derive(Debug, derive_more::Display)]
enum Abandonment {
    #[display("second shutdown signal")]
    Signal,
    #[display("grace deadline")]
    Deadline,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args)]
#[group(skip)]
#[command(next_help_heading = "Daemon")]
struct Options {
    #[arg(long, env, value_name = "ADDRESS", help = "Socket address serving HTTP, port 0 for one the system assigns")]
    listen: SocketAddr,
    #[arg(
        long,
        env,
        value_name = "DURATION",
        value_parser = |text: &str| SpanParser::new().parse_unsigned_duration(text),
        help = "Drain time after shutdown starts before the process abandons it, unbounded when unset"
    )]
    grace: Option<Duration>,
}

#[derive(Debug, derive_more::Display)]
enum Operation {
    #[display("bind {_0}")]
    Bind(SocketAddr),
    #[display("read bound address")]
    Address,
    #[display("listen on {_0}")]
    Listen(SocketAddr),
    #[display("serve HTTP")]
    Serve,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum DaemonError<X> {
    #[error(transparent)]
    Command(#[from] CommandError),
    #[error("cannot {operation}")]
    Io { operation: Operation, source: io::Error },
    #[error(transparent)]
    Daemon(X),
    #[error("{reason} arrived before drain finished")]
    Abandoned { reason: Abandonment },
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
            DaemonError::Io { operation: _, source } => source.into(),
            DaemonError::Daemon(source) => source.into(),
            DaemonError::Abandoned { reason: _ } => Self::TempFail,
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<A, L, D>(defaults: Defaults, layer: L, start: impl AsyncFnOnce(A, SocketAddr, Handle, CancellationToken) -> Result<D, D::Error>) -> ExitCode
where
    A: CommandFactory + FromArgMatches,
    L: Layer,
    D: Daemon,
{
    rasm_command::run(defaults, io::stderr, layer, async |Process { args: Augmented { options: Options { listen, grace }, args }, styles: _, mut signals, filter }| {
        let failed = |operation: Operation| move |source: io::Error| DaemonError::<D::Error>::Io { operation, source };
        let socket = try bikeshed io::Result<TcpSocket> {
            let socket = if listen.is_ipv4() { TcpSocket::new_v4() } else { TcpSocket::new_v6() }?;
            #[cfg(not(windows))]
            socket.set_reuseaddr(true)?;
            socket.bind(listen)?;
            socket
        }
        .map_err(failed(Operation::Bind(listen)))?;
        let address = socket.local_addr().map_err(failed(Operation::Address))?;
        let listener = socket.listen(u32::MAX).map_err(failed(Operation::Listen(address)))?;
        let stopping = CancellationToken::new();
        let _stopped = stopping.drop_guard_ref();
        let future::Either::Left((started, _)) = future::select(pin!(start(args, address, filter, stopping.child_token())), signals.next()).await else { return Ok(()) };
        let daemon = Arc::new(started.map_err(DaemonError::Daemon)?);
        tracing::info!({ SERVER_ADDRESS } = %address.ip(), { SERVER_PORT } = address.port(), "listening");
        let accepted = listener.tap_io(|stream| stream.set_nodelay(true).unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "TCP_NODELAY refused")));
        let routed = Arc::clone(&daemon).router().into_make_service_with_connect_info::<SocketAddr>();
        let mut http = pin!(future::maybe_done(axum::serve(accepted, routed).with_graceful_shutdown(stopping.clone().cancelled_owned()).into_future()));
        let mut lifetime = pin!(future::maybe_done(daemon.serve(stopping.child_token())));
        let skipped = tokio::select! {
            _ = signals.next() => 0,
            _ = future::select(lifetime.as_mut(), http.as_mut()) => 1,
        };
        tracing::info!("shutting down");
        let finished = tokio::select! {
            biased;
            (drained, (), ()) = future::join3(daemon.drain().inspect(|_| stopping.cancel()), lifetime.as_mut(), http.as_mut()) => drained.map_err(DaemonError::Daemon),
            _ = signals.skip(skipped).into_future() => Err(DaemonError::Abandoned { reason: Abandonment::Signal }),
            Some(()) = future::OptionFuture::from(grace.map(time::sleep)) => Err(DaemonError::Abandoned { reason: Abandonment::Deadline }),
        };
        let ended = lifetime.as_mut().take_output().map(|ended| ended.map_err(DaemonError::Daemon));
        let served = http.as_mut().take_output().map(|served| served.map_err(failed(Operation::Serve)));
        iter::chain(ended, served).chain([finished]).collect::<Validated<(), _>>().ok()
    })
}
