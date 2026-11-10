use core::sync::atomic::{AtomicU32, AtomicU64, Ordering};
use core::time::Duration;
use std::collections::{BTreeMap, VecDeque};
use std::io;
use std::path::{Path, PathBuf};
use std::process::Stdio;
use std::sync::Arc;

use backon::ExponentialBuilder;
use futures::future::{self, Either};
use futures::{FutureExt as _, StreamExt as _};
use interprocess::local_socket::tokio::Stream;
use interprocess::local_socket::traits::tokio::Listener as _;
use jiff::Timestamp;
use rasm_ipc::{Instance, IpcError, Opened, Refusal};
use rasm_mcp::{Catalog, Link, Liveness, Phase, Policy, Proxy, Retention};
use rmcp::model::{ClientCapabilities, Implementation, InitializeRequestParams, ProtocolVersion};
use rmcp::transport::async_rw::DEFAULT_MAX_LINE_LENGTH;
use serde::{Deserialize, Serialize};
use sysinfo::{Pid, ProcessRefreshKind, ProcessesToUpdate, System};
use tokio::io::{AsyncBufReadExt as _, BufReader};
use tokio::process::{ChildStdin, ChildStdout};
use tokio::sync::{Mutex, oneshot, watch};
use tokio::time;
use tokio_util::sync::CancellationToken;
use tokio_util::task::TaskTracker;
use tracing::Instrument as _;

use crate::{Build, Command, Identity, Launch, MuxerError, OPENING, Operation, PROTOCOL, Per, Request, Server, State, Welcome, failed};

// --- [CONSTANTS] -----------------------------------------------------------------------

const GRACE: Duration = Duration::from_secs(2);
const DRAIN: Duration = Duration::from_secs(5);
const STARTUP: Duration = Duration::from_secs(10);
const TAIL: usize = 8;
const QUEUE: usize = 256;
const BUFFER: usize = 8 << 10;
const RETENTION: Retention = Retention { ratio: 300, floor: Duration::from_mins(1), ceiling: Duration::from_hours(2) };
const LIVENESS: Liveness = Liveness { quiet: Duration::from_mins(2), grace: Duration::from_mins(1) };

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Serialize, Deserialize)]
struct Stored {
    command: Command,
    catalog: Catalog,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
struct Upstream {
    command: Command,
    proxy: Proxy,
    pid: Arc<AtomicU32>,
}

#[derive(Debug)]
struct Daemon {
    identity: Identity,
    built: Build,
    started: Timestamp,
    upstreams: Mutex<BTreeMap<(Command, Option<u64>), Arc<Upstream>>>,
    holding: watch::Sender<usize>,
    joined: AtomicU64,
    tasks: TaskTracker,
    superseded: CancellationToken,
}

impl Daemon {
    async fn accepted(self: Arc<Self>, stream: Stream) {
        match rasm_ipc::accept::<_, Build, Request>(stream, PROTOCOL, &self.built, OPENING).await {
            Ok(Opened { body: Request::Report, reader: _, mut writer }) => {
                let state = self.state().await;
                if let Err(error) = rasm_ipc::reply(&mut writer, Ok::<_, Refusal>(Welcome::State(Box::new(state)))).await {
                    tracing::debug!(%error, "state report not delivered");
                }
            }
            Ok(Opened { body: Request::Serve(launch), reader, mut writer }) => {
                let upstream = Arc::clone(&self).upstream(*launch).await;
                let served = async {
                    rasm_ipc::reply(&mut writer, Ok::<_, Refusal>(Welcome::Serving)).await.map_err(io::Error::other)?;
                    upstream.proxy.serve(reader, writer).await
                };
                if let Err(error) = served.await {
                    tracing::debug!(%error, "session ended");
                }
            }
            Err(IpcError::Refused(Refusal::Superseded)) => {
                tracing::info!("a newer build connected, draining");
                self.superseded.cancel();
            }
            Err(error) => tracing::debug!(%error, "session refused"),
        }
    }

    async fn upstream(self: Arc<Self>, launch: Launch) -> Arc<Upstream> {
        let Launch { command, per, environment } = launch;
        let key = (command.clone(), matches!(per, Per::Session).then(|| self.joined.fetch_add(1, Ordering::Relaxed)));
        let mut upstreams = self.upstreams.lock().await;
        if let Some(upstream) = upstreams.get(&key) {
            return Arc::clone(upstream);
        }
        let stored = self.identity.folders.cache.join(format!("{:016x}.json", xxhash_rust::xxh3::xxh3_64(&serde_json::to_vec(&command).unwrap_or_default())));
        let catalog = match fs_err::tokio::read(&stored).await.map(|held| serde_path_to_error::deserialize::<_, Stored>(&mut serde_json::Deserializer::from_slice(&held))) {
            Ok(Ok(held)) if held.command == command => Some(held.catalog),
            Ok(Ok(_) | Err(_)) | Err(_) => None,
        };
        const DECLARED: &str = r#"{"roots":{},"sampling":{},"elicitation":{}}"#;
        let capabilities = serde_path_to_error::deserialize::<_, ClientCapabilities>(&mut serde_json::Deserializer::from_str(DECLARED)).unwrap_or_default();
        let policy = Policy {
            client: InitializeRequestParams::new(capabilities, Implementation::new(env!("CARGO_PKG_NAME"), env!("CARGO_PKG_VERSION")))
                .with_protocol_version(ProtocolVersion::LATEST_WITH_INITIALIZE),
            restart: ExponentialBuilder::new().with_min_delay(Duration::from_secs(1)).with_max_delay(Duration::from_mins(5)).without_max_times(),
            liveness: Some(LIVENESS),
            retention: self.identity.idle.map_or(RETENTION, |idle| Retention { ratio: 0, floor: idle, ceiling: idle }),
            line: DEFAULT_MAX_LINE_LENGTH,
            queue: QUEUE,
            buffer: BUFFER,
        };
        let (pid, span) = (Arc::new(AtomicU32::new(0)), tracing::info_span!(parent: None, "server", program = %command.program.display()));
        let (proxy, actor) = Proxy::new(policy, catalog, connector(command.clone(), Arc::new(environment), Arc::clone(&pid)));
        let upstream = Arc::new(Upstream { command, proxy, pid });
        upstreams.insert(key.clone(), Arc::clone(&upstream));
        self.holding.send_modify(|holding| *holding = holding.saturating_add(1));
        drop(self.tasks.spawn(actor.instrument(span.clone())));
        drop(self.tasks.spawn(Arc::clone(&self).kept(key, Arc::clone(&upstream), stored).instrument(span)));
        upstream
    }

    async fn kept(self: Arc<Self>, key: (Command, Option<u64>), upstream: Arc<Upstream>, stored: PathBuf) {
        let mut status = upstream.proxy.status();
        let (mut written, mut served) = (status.borrow().catalog.clone(), false);
        loop {
            let (phase, sessions, catalog) = {
                let held = status.borrow_and_update();
                (held.phase, held.sessions, held.catalog.clone())
            };
            served |= sessions > 0;
            if let Some(catalog) = catalog.filter(|catalog| written.as_ref().is_none_or(|held| !Arc::ptr_eq(held, catalog))) {
                let record = serde_json::to_vec(&Stored { command: upstream.command.clone(), catalog: Catalog::clone(&catalog) }).map_err(io::Error::other);
                let saved = async {
                    fs_err::tokio::create_dir_all(&self.identity.folders.cache).await?;
                    let staged = stored.with_extension("staged");
                    fs_err::tokio::write(&staged, record?).await?;
                    fs_err::tokio::rename(&staged, &stored).await
                };
                if let Err(error) = saved.await {
                    tracing::warn!(%error, "server handshake not stored");
                }
                written = Some(catalog);
            }
            if sessions == 0 && (key.1.is_some_and(|_| served) || matches!(phase, Phase::Idle | Phase::Down | Phase::Stopped)) {
                let mut upstreams = self.upstreams.lock().await;
                if upstream.proxy.status().borrow().sessions == 0 {
                    upstreams.remove(&key);
                    break;
                }
            }
            if status.changed().await.is_err() {
                break;
            }
        }
        upstream.proxy.stop().await;
        self.holding.send_modify(|holding| *holding = holding.saturating_sub(1));
    }

    async fn state(&self) -> State {
        let upstreams = Vec::from_iter(self.upstreams.lock().await.values().cloned());
        let own = std::process::id();
        let pids = Vec::from_iter(upstreams.iter().map(|upstream| upstream.pid.load(Ordering::Relaxed)).chain([own]).filter(|pid| *pid != 0).map(Pid::from_u32));
        let system = System::new().map(|mut system| {
            system.refresh_processes_specifics(ProcessesToUpdate::Some(&pids), true, ProcessRefreshKind::nothing().with_memory());
            system
        });
        let memory = |pid: u32| system.as_ref().ok().and_then(|system| system.process(Pid::from_u32(pid))).map(sysinfo::Process::memory);
        let servers = upstreams.iter().map(|upstream| {
            let pid = Some(upstream.pid.load(Ordering::Relaxed)).filter(|pid| *pid != 0);
            Server::new(&upstream.command, &upstream.proxy.status().borrow(), upstream.proxy.in_flight(), pid.map(|pid| (pid, memory(pid))))
        });
        State { pid: own, build: self.built, started: self.started, memory: memory(own), connections: self.holding.borrow().saturating_sub(upstreams.len()), servers: servers.collect() }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn connector(command: Command, environment: Arc<Vec<(String, String)>>, pid: Arc<AtomicU32>) -> impl FnMut(CancellationToken) -> future::BoxFuture<'static, io::Result<Link<ChildStdout, ChildStdin, future::BoxFuture<'static, String>>>> + Send {
    move |stop| {
        let (command, environment, pid) = (command.clone(), Arc::clone(&environment), Arc::clone(&pid));
        async move {
            let mut process = tokio::process::Command::new(&command.program);
            process
                .args(&command.arguments)
                .current_dir(&command.directory)
                .env_clear()
                .envs(environment.iter().map(|(name, value)| (name, value)))
                .envs(&command.environment)
                .stdin(Stdio::piped())
                .stdout(Stdio::piped())
                .stderr(Stdio::piped());
            let (attached, pipes) = oneshot::channel();
            let mut exited = async move {
                let attach = async |pipes| attached.send(pipes).map_err(|_| io::Error::from(io::ErrorKind::BrokenPipe));
                rasm_process::run(process, None, GRACE, &stop, attach).await.map_or_else(|error| error.to_string(), |status| status.to_string())
            }
            .boxed();
            let pipes = match future::select(pipes, &mut exited).await {
                Either::Left((Ok(pipes), _)) => pipes,
                Either::Left((Err(oneshot::error::RecvError { .. }), _)) => return Err(io::Error::other(exited.await)),
                Either::Right((exit, _)) => return Err(io::Error::other(exit)),
            };
            pid.store(pipes.id.get(), Ordering::Relaxed);
            let (Some(reader), Some(writer)) = (pipes.stdout, pipes.stdin) else { return Err(io::ErrorKind::BrokenPipe.into()) };
            let said = async move {
                let mut tail = VecDeque::with_capacity(TAIL);
                let mut lines = BufReader::new(pipes.stderr?).lines();
                while let Ok(Some(line)) = lines.next_line().await {
                    tracing::info!(line, "server stderr");
                    if tail.len() == TAIL {
                        tail.pop_front();
                    }
                    tail.push_back(line);
                }
                Some(tail)
            };
            let closed = future::join(exited, said).map(move |(exit, tail)| {
                pid.store(0, Ordering::Relaxed);
                match tail.filter(|tail| !tail.is_empty()) {
                    Some(tail) => format!("{exit}; stderr: {}", Vec::from(tail).join(" | ")),
                    None => exit,
                }
            });
            Ok(Link { reader, writer, closed: closed.boxed() })
        }
        .boxed()
    }
}

async fn pruned(cache: &Path) {
    let Ok(mut entries) = fs_err::tokio::read_dir(cache).await else { return };
    while let Ok(Some(entry)) = entries.next_entry().await {
        let path = entry.path();
        let held = fs_err::tokio::read(&path).await.map(|held| serde_path_to_error::deserialize::<_, Stored>(&mut serde_json::Deserializer::from_slice(&held)));
        let current = match held {
            Ok(Ok(Stored { command, catalog: _ })) => fs_err::tokio::metadata(&command.program).await.and_then(|file| crate::build(&file)).is_ok_and(|file| file == command.file),
            Ok(Err(_)) | Err(_) => false,
        };
        if !current && let Err(error) = fs_err::tokio::remove_file(&path).await {
            tracing::debug!(%error, "stale stored handshake not removed");
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn serve(identity: Identity, built: Build) -> Result<(), MuxerError> {
    let wait = || ExponentialBuilder::new().with_min_delay(Duration::from_millis(1)).with_max_delay(Duration::from_millis(100)).without_max_times().with_total_delay(Some(Duration::from_secs(30)));
    let (lock, socket) = (identity.lock()?, identity.socket()?);
    let Some(listener) = Instance::acquire(&lock, wait()).await?.listen(socket).await? else { return Ok(()) };
    let mut signals = signals().map_err(failed(Operation::Signal))?;
    tracing::info!(pid = std::process::id(), ?built, runtime = %identity.folders.runtime.display(), "listening");
    pruned(&identity.folders.cache).await;
    let daemon = Arc::new(Daemon {
        identity,
        built,
        started: Timestamp::now(),
        upstreams: Mutex::default(),
        holding: watch::Sender::new(0),
        joined: AtomicU64::new(0),
        tasks: TaskTracker::new(),
        superseded: CancellationToken::new(),
    });
    let mut holding = daemon.holding.subscribe();
    let mut served = false;
    loop {
        tokio::select! {
            biased;
            _ = signals.next() => break tracing::info!("signalled, draining"),
            () = daemon.superseded.cancelled() => break,
            Ok(_) = holding.wait_for(|holding| *holding == 0), if served => break tracing::info!("nothing held, stopping"),
            () = time::sleep(STARTUP), if !served => break tracing::info!("no session arrived, stopping"),
            accepted = listener.accept() => match accepted {
                Ok(stream) => {
                    served = true;
                    let (session, counted) = (Arc::clone(&daemon).accepted(stream), daemon.holding.clone());
                    counted.send_modify(|holding| *holding = holding.saturating_add(1));
                    drop(daemon.tasks.spawn(session.map(move |()| counted.send_modify(|holding| *holding = holding.saturating_sub(1)))));
                }
                Err(error) => tracing::warn!(%error, "session not accepted"),
            },
        }
    }
    let closing = Instance::acquire(&lock, wait()).await;
    drop(listener);
    let upstreams = Vec::from_iter(daemon.upstreams.lock().await.values().cloned());
    let drained = future::join_all(upstreams.iter().map(|upstream| upstream.proxy.drain()));
    tokio::select! {
        biased;
        _ = signals.next() => tracing::info!("signalled again, drain skipped"),
        _ = time::timeout(DRAIN, drained) => {}
    }
    future::join_all(upstreams.iter().map(|upstream| upstream.proxy.stop())).await;
    daemon.tasks.close();
    daemon.tasks.wait().await;
    tracing::info!("stopped");
    drop(closing);
    Ok(())
}

fn signals() -> io::Result<futures::stream::SelectAll<futures::stream::BoxStream<'static, ()>>> {
    cfg_select! {
        unix => {{
            use tokio::signal::unix::{SignalKind, signal};
            use tokio_stream::wrappers::SignalStream;
            [SignalKind::interrupt(), SignalKind::terminate(), SignalKind::hangup()].into_iter().map(|kind| signal(kind).map(|signal| SignalStream::new(signal).boxed())).collect()
        }}
        _ => Ok(futures::stream::select_all([futures::stream::once(tokio::signal::ctrl_c()).map(drop).boxed()])),
    }
}
