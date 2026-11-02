use core::{error::Error, future::ready, net::SocketAddr, pin::pin};
use std::{io, process::ExitCode, sync::Arc};

use axum::{Router, serve::ListenerExt as _};
use clap::{Args, CommandFactory, FromArgMatches};
use futures::{FutureExt as _, StreamExt as _, future};
use nonempty_collections::NEVec;
use rasm_command::{Augmented, CommandError, Process};
use rasm_telemetry::Defaults;
use tokio::net::TcpListener;
use tokio_util::sync::CancellationToken;
use tracing_subscriber::{EnvFilter, Layer, Registry, layer::Layered, reload};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Daemon {
    type Error: Error + Into<sysexits::ExitCode>;

    fn router(self: Arc<Self>) -> Router;

    fn serve(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send { stopping.cancelled_owned().map(Ok) }

    fn drain(&self) -> impl Future<Output = ()> + Send { ready(()) }
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args)]
#[group(skip)]
#[command(next_help_heading = "Daemon")]
struct Options {
    #[arg(long, env, value_name = "ADDRESS", help = "Socket address serving HTTP, port 0 for one the system assigns")]
    listen: SocketAddr,
}

#[derive(Debug, derive_more::Display)]
enum Operation {
    #[display("bind {_0}")]
    Bind(SocketAddr),
    #[display("read bound address")]
    Address,
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
    #[error("second shutdown signal arrived before drain finished")]
    Abandoned,
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
            DaemonError::Abandoned => Self::TempFail,
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<A, L, D>(defaults: Defaults, layer: L, start: impl AsyncFnOnce(A, SocketAddr, CancellationToken) -> Result<D, D::Error>) -> ExitCode
where
    A: CommandFactory + FromArgMatches,
    L: Layer<Layered<reload::Layer<EnvFilter, Registry>, Registry>> + Send + Sync + 'static,
    D: Daemon,
{
    rasm_command::run(defaults, io::stderr, layer, async |Process { args: Augmented { options: Options { listen }, args }, mut signals, filter: _ }| {
        let listener = TcpListener::bind(listen).await.map_err(|source| DaemonError::Io { operation: Operation::Bind(listen), source })?;
        let address = listener.local_addr().map_err(|source| DaemonError::Io { operation: Operation::Address, source })?;
        let stopping = CancellationToken::new();
        let future::Either::Left((started, _)) = future::select(pin!(start(args, address, stopping.child_token())), signals.next()).await else { return Ok(()) };
        let daemon = Arc::new(started.map_err(DaemonError::Daemon)?);
        tracing::info!(listen = %address, "listening");
        let accepted = listener.tap_io(|stream| {
            let (Ok(()) | Err(_)) = stream.set_nodelay(true);
        });
        let routed = Arc::clone(&daemon).router().into_make_service_with_connect_info::<SocketAddr>();
        let mut http = pin!(future::maybe_done(axum::serve(accepted, routed).with_graceful_shutdown(stopping.clone().cancelled_owned()).into_future()));
        let mut lifetime = pin!(future::maybe_done(daemon.serve(stopping.child_token())));
        future::select(signals.next(), future::select(lifetime.as_mut(), http.as_mut())).await;
        tracing::info!("shutting down");
        let drained = async {
            daemon.drain().await;
            stopping.cancel();
        };
        let finished = matches!(future::select(pin!(future::join3(drained, lifetime.as_mut(), http.as_mut())), signals.next()).await, future::Either::Left(_)).ok_or(DaemonError::Abandoned);
        let ended = lifetime.as_mut().take_output().unwrap_or(Ok(())).map_err(DaemonError::Daemon);
        let served = http.as_mut().take_output().unwrap_or(Ok(())).map_err(|source| DaemonError::Io { operation: Operation::Serve, source });
        Validated::from(ended).map3(served.into(), finished.into(), |(), (), ()| ()).ok()
    })
}
