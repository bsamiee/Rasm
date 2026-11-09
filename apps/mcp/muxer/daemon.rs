use core::{future::ready, mem::DropGuard, pin::pin, time::Duration};
use std::{
    collections::{BTreeMap, HashMap},
    error::Report,
    fs::TryLockError,
    io,
    path::PathBuf,
    process::Stdio,
    sync::{Arc, nonpoison::Mutex},
    time::SystemTime,
};

use camino::Utf8PathBuf;
use futures::{StreamExt as _, TryFutureExt as _, TryStreamExt as _, future, stream};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_command::Signals;
use rasm_mcp::{Host, Proxy, Traced};
use rmcp::{ServiceExt as _, model::Implementation, transport::TokioChildProcess};
use serde::{Deserialize, Serialize};
use tokio::{
    io::{AsyncBufReadExt as _, AsyncWriteExt as _, BufReader},
    net::{UnixListener, UnixStream},
    process::Command,
    sync::{OnceCell, watch},
    time,
};
use tokio_stream::wrappers::LinesStream;
use tokio_util::{sync::CancellationToken, task::TaskTracker};
use tracing::Instrument as _;

use crate::{MuxerError, Operation, decoded};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, Hash, Serialize, Deserialize)]
pub(crate) struct Server {
    pub(crate) program: Utf8PathBuf,
    pub(crate) identity: (u64, u64, SystemTime),
    pub(crate) arguments: Vec<String>,
    pub(crate) directory: Utf8PathBuf,
    pub(crate) declared: BTreeMap<String, String>,
}

#[derive(Debug, Serialize, Deserialize)]
pub(crate) struct Launch {
    pub(crate) server: Server,
    pub(crate) environment: BTreeMap<String, String>,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
struct Upstream {
    host: Arc<Host<Proxy>>,
    sessions: watch::Sender<usize>,
}

#[derive(Debug)]
struct Daemon {
    idle: Duration,
    upstreams: Mutex<HashMap<Server, Arc<OnceCell<Upstream>>>>,
    connections: watch::Sender<usize>,
    tasks: TaskTracker,
    stopping: CancellationToken,
}

impl Daemon {
    async fn connected(self: Arc<Self>, stream: UnixStream) -> Result<(), MuxerError> {
        let failed = |operation| move |source| MuxerError::Io { operation, source };
        let counted = |count: watch::Sender<usize>| {
            count.send_modify(|held| *held = held.saturating_add(1));
            DropGuard::new(count, |count| count.send_modify(|held| *held = held.saturating_sub(1)))
        };
        let _connected = counted(self.connections.clone());
        let (reader, mut writer) = stream.into_split();
        let mut reader = BufReader::new(reader);
        let mut line = String::new();
        reader.read_line(&mut line).await.map_err(failed(Operation::Opening))?;
        let Launch { server, environment } = decoded(&line)?;
        let cell = self.upstreams.with_mut(|upstreams| Arc::clone(upstreams.entry(server.clone()).or_default()));
        let Upstream { host, sessions } = cell.get_or_try_init(|| Arc::clone(&self).linked(server, environment)).await?;
        let mut answer = serde_json::to_vec(&())?;
        answer.push(b'\n');
        writer.write_all(&answer).await.map_err(failed(Operation::Opening))?;
        let _session = counted(sessions.clone());
        let running = Box::pin(Traced::new(host.session()).serve_with_ct((reader, writer), self.stopping.child_token()))
            .await
            .map_err(|error| MuxerError::Io { operation: Operation::Relay, source: io::Error::other(error) })?;
        let quit = running.waiting().await.map_err(|error| MuxerError::Io { operation: Operation::Relay, source: io::Error::other(error) })?;
        tracing::debug!(?quit, "session closed");
        Ok(())
    }

    async fn linked(self: Arc<Self>, server: Server, environment: BTreeMap<String, String>) -> Result<Upstream, MuxerError> {
        let Server { program, identity: _, arguments, directory, declared } = server.clone();
        let tasks = self.tasks.clone();
        let spawn = move || {
            let mut command = Command::new(&program);
            command.args(&arguments).current_dir(&directory).env_clear().envs(&environment).envs(&declared);
            let spawned = TokioChildProcess::builder(rasm_process::owned(command)).stderr(Stdio::piped()).spawn().map(|(transport, stderr)| {
                let lines = stream::iter(stderr).flat_map(|stderr| LinesStream::new(BufReader::new(stderr).lines()));
                let logged = lines.map_ok(|line| tracing::debug!(line, "upstream stderr")).try_collect::<()>();
                drop(tasks.spawn(
                    logged.unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "upstream stderr closed")).instrument(tracing::debug_span!("upstream", %program)),
                ));
                transport
            });
            ready(spawned)
        };
        let identity = Implementation::new(env!("CARGO_PKG_NAME"), env!("CARGO_PKG_VERSION"));
        let host = Host::new(async |notifier| Proxy::connect(identity, notifier, spawn).await).await?;
        let sessions = watch::Sender::new(0);
        let idled = idled(sessions.subscribe(), self.idle);
        let daemon = Arc::clone(&self);
        let held = Arc::clone(&host);
        self.tasks.spawn(self.stopping.clone().run_until_cancelled_owned(async move {
            idled.await;
            daemon.upstreams.with_mut(|upstreams| upstreams.remove(&server));
            held.drain().await;
            tracing::info!(program = %server.program, "upstream idle, stopped");
        }));
        Ok(Upstream { host, sessions })
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn logs() -> Result<Utf8PathBuf, MuxerError> {
    let platform = cfg_select! {
        target_vendor = "apple" => dirs::home_dir().map(|home| home.join("Library").join("Logs")),
        _ => dirs::state_dir(),
    };
    per_user(platform)
}

pub(crate) fn socket() -> Result<Utf8PathBuf, MuxerError> { folder().map(|folder| folder.join("daemon.sock")) }

fn folder() -> Result<Utf8PathBuf, MuxerError> { per_user(dirs::runtime_dir().or_else(dirs::cache_dir)).map(|folder| folder.join(env!("CARGO_PKG_VERSION"))) }

fn per_user(base: Option<PathBuf>) -> Result<Utf8PathBuf, MuxerError> {
    base.and_then(|base| Utf8PathBuf::from_path_buf(base.join(env!("CARGO_PKG_NAME"))).ok()).ok_or(MuxerError::Io { operation: Operation::Locate, source: io::ErrorKind::NotFound.into() })
}

async fn idled(mut count: watch::Receiver<usize>, idle: Duration) {
    loop {
        match count.wait_for(|held| *held == 0).await.map(drop) {
            Ok(()) => {}
            Err(watch::error::RecvError { .. }) => break,
        }
        match time::timeout(idle, count.wait_for(|held| *held > 0)).await.map(|waited| waited.map(drop)) {
            Ok(Ok(())) => {}
            Ok(Err(watch::error::RecvError { .. })) | Err(time::error::Elapsed { .. }) => break,
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn served(idle: Duration, mut signals: Signals) -> Result<(), MuxerError> {
    let (folder, socket) = (folder()?, socket()?);
    let failed = |operation| move |source| MuxerError::Io { operation, source };
    fs_err::tokio::DirBuilder::new().recursive(true).mode(0o700).create(&folder).await.map_err(failed(Operation::Listen(socket.clone())))?;
    let lock = fs_err::tokio::File::create(folder.join("daemon.lock")).await.map_err(failed(Operation::Listen(socket.clone())))?.into_std().await;
    match lock.try_lock() {
        Ok(()) => {}
        Err(TryLockError::WouldBlock) => return Ok(()),
        Err(TryLockError::Error(source)) => return Err(failed(Operation::Listen(socket))(source)),
    }
    let listener = match UnixListener::bind(&socket) {
        Err(error) if error.kind() == io::ErrorKind::AddrInUse => {
            fs_err::tokio::remove_file(&socket).await.map_err(failed(Operation::Listen(socket.clone())))?;
            UnixListener::bind(&socket)
        }
        bound => bound,
    }
    .map_err(failed(Operation::Listen(socket.clone())))?;
    tokio::io::stdout().write_all(format!("{socket}\n").as_bytes()).await.map_err(failed(Operation::Listen(socket.clone())))?;
    tracing::info!(%socket, pid = std::process::id(), "listening");
    let daemon = Arc::new(Daemon { idle, upstreams: Mutex::default(), connections: watch::Sender::new(0), tasks: TaskTracker::new(), stopping: CancellationToken::new() });
    let mut unused = pin!(idled(daemon.connections.subscribe(), idle));
    loop {
        tokio::select! {
            biased;
            _ = signals.next() => break,
            () = &mut unused => break,
            accepted = listener.accept() => match accepted.map(|(stream, _)| Arc::clone(&daemon).connected(stream)) {
                Ok(session) => drop(daemon.tasks.spawn(session.unwrap_or_else(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "session failed")))),
                Err(error) => tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "accept failed"),
            },
        }
    }
    drop(listener);
    fs_err::tokio::remove_file(&socket).await.map_err(failed(Operation::Listen(socket.clone())))?;
    daemon.stopping.cancel();
    let hosts = daemon.upstreams.with_mut(|upstreams| upstreams.drain().filter_map(|(_, cell)| cell.get().map(|upstream| Arc::clone(&upstream.host))).collect::<Vec<_>>());
    future::join_all(hosts.iter().map(|host| host.drain())).await;
    drop(hosts);
    daemon.tasks.close();
    daemon.tasks.wait().await;
    tracing::info!("stopped");
    Ok(())
}
