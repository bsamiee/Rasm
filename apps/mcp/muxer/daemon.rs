use core::pin::{Pin, pin};
use core::sync::atomic::{AtomicU64, Ordering};
use core::time::Duration;
use std::collections::BTreeMap;
use std::error::Report;
use std::io::{self, Write as _};
use std::process::Stdio;
use std::sync::Arc;

use backon::ExponentialBuilder;
use camino::{Utf8Path, Utf8PathBuf};
use futures::future::{self, BoxFuture, Either, OptionFuture};
use futures::{FutureExt as _, StreamExt as _, TryFutureExt as _, TryStreamExt as _, stream};
use interprocess::local_socket::tokio::Stream;
use interprocess::local_socket::traits::tokio::Listener as _;
use itertools::Itertools as _;
use jiff::Timestamp;
use opentelemetry::global;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_ipc::{Endpoint, Instance, IpcError, Opened, Refusal};
use rasm_mcp::{History, Link, Liveness, Policy, Pool, Retention, Store};
use rasm_observability::metrics;
use rmcp::model::{ClientCapabilitiesBuilder, ElicitationCapability, Implementation, InitializeRequestParams, ProtocolVersion, RootsCapabilities, SamplingCapability};
use rmcp::transport::async_rw::DEFAULT_MAX_LINE_LENGTH;
use serde::de::{DeserializeOwned, IgnoredAny};
use serde_path_to_error::Track;
use sysinfo::{Pid, ProcessRefreshKind, ProcessesToUpdate, System};
use tempfile::NamedTempFile;
use tokio::io::{AsyncRead, AsyncWrite};
use tokio::sync::{oneshot, watch};
use tokio::{task, time};
use tokio_util::sync::CancellationToken;
use tokio_util::task::{AbortOnDropHandle, TaskTracker};
use tracing::Instrument as _;

use crate::{BUFFER, Build, Command, Key, Launch, MuxerError, OPENING, PROTOCOL, Per, Request, Server, State, Welcome, digest, server};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
struct Records(Utf8PathBuf);

impl Records {
    fn path(&self, command: &Command) -> io::Result<Utf8PathBuf> { Ok(self.0.join(digest(command)?).with_extension("json")) }
}

impl Store<Key> for Records {
    async fn load(&self, key: &Key) -> io::Result<Option<History>> {
        match fs_err::tokio::read(self.path(&key.0)?).await {
            Ok(held) => record::<(Command, History)>(&held).map(|(command, history)| (command == key.0).then_some(history)),
            Err(error) if error.kind() == io::ErrorKind::NotFound => Ok(None),
            Err(error) => Err(error),
        }
    }

    async fn save(&self, key: &Key, history: History) -> io::Result<()> {
        let (folder, path, record) = (self.0.clone(), self.path(&key.0)?, serde_json::to_vec(&(&key.0, history))?);
        let stored = task::spawn_blocking(move || {
            fs_err::create_dir_all(&folder)?;
            let mut staged = NamedTempFile::new_in(&folder)?;
            staged.write_all(&record)?;
            staged.persist(&path).map(drop).map_err(io::Error::from)
        });
        stored.await?
    }
}

#[derive(Debug)]
struct Daemon {
    built: Build,
    started: Timestamp,
    idle: Option<Duration>,
    pool: Pool<Key, Records>,
    processes: watch::Sender<BTreeMap<Key, u32>>,
    joined: AtomicU64,
    superseded: CancellationToken,
    stopping: CancellationToken,
}

impl Daemon {
    async fn accepted(self: Arc<Self>, stream: Stream) {
        match rasm_ipc::accept::<_, Build, Request>(stream, PROTOCOL, &self.built, OPENING).await {
            Ok(Opened { body: Request::Report, reader: _, mut writer }) =>
                if let Err(error) = rasm_ipc::reply(&mut writer, Ok::<_, Refusal>(Welcome::State(Box::new(self.state())))).await {
                    tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "state report not delivered");
                },
            Ok(Opened { body: Request::Serve(launch), reader, mut writer }) => {
                let Launch { command, per, environment } = *launch;
                let own = match per {
                    Per::Session => Some(self.joined.fetch_add(1, Ordering::Relaxed)),
                    Per::Directory | Per::User => None,
                };
                let key = (command, own);
                let member = self.pool.join(&key, || self.start(&key, environment.into())).await;
                let served = async {
                    rasm_ipc::reply(&mut writer, Ok::<_, Refusal>(Welcome::Serving)).await.map_err(io::Error::other)?;
                    member.serve(reader, writer).await
                };
                if let Err(error) = served.await {
                    tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "session ended");
                }
                if own.is_some() {
                    self.pool.remove(&key).await;
                }
            }
            Err(error) => {
                if let IpcError::Refused(Refusal::Superseded) = error {
                    self.superseded.cancel();
                }
                tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "session refused");
            }
        }
    }

    fn start(
        &self,
        key: &Key,
        environment: Arc<[(String, String)]>,
    ) -> (
        Policy,
        impl FnMut(CancellationToken) -> BoxFuture<'static, io::Result<Link<Pin<Box<dyn AsyncRead + Send>>, Pin<Box<dyn AsyncWrite + Send>>, BoxFuture<'static, String>>>> + Send + use<>,
        tracing::Span,
    ) {
        const RETENTION: Retention = Retention { ratio: 300, floor: Duration::from_mins(1), ceiling: Duration::from_hours(2) };
        const GRACE: Duration = Duration::from_millis(500);
        const TAIL: usize = 8;
        const LINE: u64 = 4 << 10;
        let declared = ClientCapabilitiesBuilder {
            roots: Some(RootsCapabilities::default()),
            sampling: Some(SamplingCapability::default()),
            elicitation: Some(ElicitationCapability::default()),
            ..rmcp::model::ClientCapabilities::builder()
        };
        let policy = Policy {
            client: InitializeRequestParams::new(declared.build(), Implementation::new(env!("CARGO_PKG_NAME"), env!("CARGO_PKG_VERSION")))
                .with_protocol_version(ProtocolVersion::LATEST_WITH_INITIALIZE),
            restart: ExponentialBuilder::new().with_min_delay(Duration::from_secs(1)).with_max_delay(Duration::from_mins(5)).without_max_times(),
            handshake: Duration::from_secs(30),
            probation: Duration::from_secs(2),
            liveness: Some(Liveness { quiet: Duration::from_mins(2), grace: Duration::from_mins(1) }),
            retention: self.idle.map_or(RETENTION, |idle| Retention { ratio: 0, floor: idle, ceiling: idle }),
            line: DEFAULT_MAX_LINE_LENGTH,
            queue: 256,
        };
        let (key, processes) = (key.clone(), self.processes.clone());
        let span = tracing::info_span!(parent: None, "server", program = %key.0.program.display());
        let connect = move |stop: CancellationToken| {
            let (key, environment, processes) = (key.clone(), Arc::clone(&environment), processes.clone());
            async move {
                let (attached, streams) = oneshot::channel();
                let process = server(&key.0, &environment, Stdio::piped());
                let mut exited = async move {
                    let attach = async |streams| attached.send(streams).map_err(|rasm_process::Streams { id: _, stdin: _, stdout: _, stderr: _ }| io::Error::from(io::ErrorKind::BrokenPipe));
                    match rasm_process::run(process, Some(BUFFER), None, GRACE, &stop, attach).await {
                        Ok(status) | Err(rasm_process::ProcessError { program: _, process: Some((_, Some(status))), failures: _ }) => status.to_string(),
                        Err(error) => Report::new(&error).to_string(),
                    }
                }
                .boxed();
                let streams = match future::select(streams, &mut exited).await {
                    Either::Left((Ok(streams), _)) => streams,
                    Either::Left((Err(oneshot::error::RecvError { .. }), _)) => return Err(io::Error::other(exited.await)),
                    Either::Right((exit, _)) => return Err(io::Error::other(exit)),
                };
                let (Some(reader), Some(writer)) = (streams.stdout, streams.stdin) else { return Err(io::ErrorKind::BrokenPipe.into()) };
                processes.send_if_modified(|processes| processes.insert(key.clone(), streams.id.get()).is_none());
                let read = |stderr| AbortOnDropHandle::new(task::spawn(rasm_process::tail(stderr, TAIL, LINE, |line| tracing::debug!(line, "server stderr")).in_current_span()));
                let closed = future::join(exited, OptionFuture::from(streams.stderr.map(read))).map(move |(exit, tail)| {
                    processes.send_if_modified(|processes| processes.remove(&key).is_some());
                    match tail {
                        Some(Ok(tail)) if !tail.is_empty() => format!("{exit}; stderr: {}", tail.iter().join(" | ")),
                        Some(Ok(_)) | None => exit,
                        Some(Err(error)) => format!("{exit}; stderr unread: {}", Report::new(&error)),
                    }
                });
                Ok(Link { reader, writer, closed: closed.boxed() })
            }
            .boxed()
        };
        (policy, connect, span)
    }

    fn state(&self) -> State {
        let (own, processes) = (std::process::id(), self.processes.borrow().clone());
        let system = match System::new() {
            Ok(mut system) => {
                let watched = Vec::from_iter(processes.values().copied().chain([own]).map(Pid::from_u32));
                system.refresh_processes_specifics(ProcessesToUpdate::Some(&watched), true, ProcessRefreshKind::nothing().with_memory());
                Some(system)
            }
            Err(error) => {
                tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "process readings unavailable");
                None
            }
        };
        let memory = |pid: u32| system.as_ref().and_then(|system| system.process(Pid::from_u32(pid))).map(sysinfo::Process::memory);
        let reported = |(key, member): (&Key, &rasm_mcp::Proxy)| {
            let (status, pid) = (member.status().borrow().clone(), processes.get(key).copied());
            Server {
                program: key.0.program.clone(),
                arguments: key.0.arguments.clone(),
                directory: key.0.directory.clone(),
                phase: status.phase,
                era: status.catalog.as_ref().map(|catalog| catalog.era),
                sessions: status.sessions,
                calls: member.in_flight(),
                starts: status.starts,
                failures: status.failures,
                cause: status.cause.as_deref().map(str::to_owned),
                start_cost: status.catalog.as_ref().map(|catalog| catalog.cost),
                pid,
                memory: pid.and_then(memory),
            }
        };
        let servers = Vec::from_iter(self.pool.members().iter().map(reported));
        let (sessions, calls) = servers.iter().fold((0_usize, 0_usize), |(sessions, calls), server| (sessions.saturating_add(server.sessions), calls.saturating_add(server.calls)));
        State { pid: own, build: self.built, started: self.started, memory: memory(own), sessions, calls, servers }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn record<T>(held: &[u8]) -> io::Result<T>
where T: DeserializeOwned {
    let mut decoder = serde_json::Deserializer::from_slice(held);
    let value = serde_path_to_error::deserialize(&mut decoder).map_err(io::Error::other)?;
    decoder.end().map(|()| value).map_err(|source| io::Error::other(serde_path_to_error::Error::new(Track::new().path(), source)))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn serve(endpoint: Endpoint, cache: &Utf8Path, idle: Option<Duration>, built: Build) -> Result<(), MuxerError> {
    const LINGER: Duration = Duration::from_secs(10);
    const SETTLE: Duration = Duration::from_millis(250);
    let mut signals = rasm_command::signals()?;
    let claim = ExponentialBuilder::new().with_min_delay(Duration::from_millis(1)).with_max_delay(Duration::from_millis(100)).without_max_times().with_total_delay(Some(Duration::from_secs(30)));
    let Some(Instance { listener, held }) = Instance::claim(&endpoint, claim).await? else { return Ok(()) };
    let files = rlimit::increase_nofile_limit(u64::MAX).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "open file limit not raised")).ok();
    tracing::info!(pid = std::process::id(), ?built, files, "listening");
    let stale = async |path: std::path::PathBuf| {
        let (command, IgnoredAny) = record::<(Command, IgnoredAny)>(&fs_err::tokio::read(&path).await?)?;
        Ok::<_, io::Error>(crate::build(&fs_err::tokio::metadata(&command.program).await?)? != command.file)
    };
    let records = fs_err::tokio::read_dir(cache).map_ok(|records| stream::try_unfold(records, async |mut records| Ok(records.next_entry().await?.map(|entry| (entry, records))))).try_flatten_stream();
    let pruned = records.try_for_each(async |entry| match stale(entry.path()).await {
        Ok(false) => Ok(()),
        Ok(true) => fs_err::tokio::remove_file(entry.path()).await,
        Err(error) => {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server record unreadable, removed");
            fs_err::tokio::remove_file(entry.path()).await
        }
    });
    if let Err(error) = pruned.await {
        tracing::trace!({ EXCEPTION_MESSAGE } = %Report::new(&error), "stored server records not pruned");
    }
    let daemon = Arc::new(Daemon {
        built,
        started: Timestamp::now(),
        idle,
        pool: Pool::new(Records(cache.to_owned())),
        processes: watch::Sender::default(),
        joined: AtomicU64::new(0),
        superseded: CancellationToken::new(),
        stopping: CancellationToken::new(),
    });
    let processes = daemon.processes.subscribe();
    if let Err(error) = metrics::process(&*global::meter_provider(), move || Vec::from_iter(processes.borrow().values().copied())) {
        tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server process metrics unavailable");
    }
    let (sessions, mut unused) = (TaskTracker::new(), pin!(time::sleep(LINGER).then(|()| daemon.pool.unused())));
    let reason = loop {
        tokio::select! {
            biased;
            _ = signals.next() => break "signalled",
            () = daemon.superseded.cancelled() => break "a newer build connected",
            () = &mut unused => break "nothing held",
            accepted = listener.accept() => match accepted {
                Ok(stream) => {
                    let session = Arc::clone(&daemon).accepted(stream).instrument(tracing::info_span!(parent: None, "session"));
                    drop(sessions.spawn(daemon.stopping.clone().run_until_cancelled_owned(session)));
                }
                Err(error) => {
                    tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "session not accepted");
                    time::sleep(SETTLE).await;
                }
            },
        }
    };
    drop(listener);
    tokio::select! {
        biased;
        _ = signals.next() => {}
        () = daemon.pool.drain(SETTLE) => {}
    }
    daemon.stopping.cancel();
    sessions.close();
    sessions.wait().await;
    daemon.pool.stop().await;
    tracing::info!(reason, "stopped");
    drop(held);
    Ok(())
}
