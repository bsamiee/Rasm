use core::pin::pin;
use core::sync::atomic::{AtomicU64, Ordering};
use core::time::Duration;
use std::collections::BTreeMap;
use std::error::Report;
use std::io::{self, Write as _};
use std::path::PathBuf;
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
use rasm_mcp::{History, IdleTimeout, Link, Liveness, Policy, Pool, Store};
use rasm_telemetry::metrics;
use rmcp::model::{ClientCapabilitiesBuilder, ElicitationCapability, Implementation, InitializeRequestParams, ProtocolVersion, RootsCapabilities, SamplingCapability};
use rmcp::transport::async_rw::DEFAULT_MAX_LINE_LENGTH;
use serde::de::IgnoredAny;
use tempfile::NamedTempFile;
use tokio::sync::{oneshot, watch};
use tokio::{task, time};
use tokio_util::sync::CancellationToken;
use tokio_util::task::{AbortOnDropHandle, TaskTracker};
use tracing::Instrument as _;

use crate::{BUFFER, Build, Command, Launch, MuxerError, OPENING, PROTOCOL, Per, Request, ServerKey, ServerState, State, Welcome, digest};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
struct Records(Utf8PathBuf);

impl Records {
    fn path(&self, command: &Command) -> io::Result<Utf8PathBuf> { Ok(self.0.join(digest(command)?).with_extension("json")) }
}

impl Store<ServerKey> for Records {
    async fn load(&self, key: &ServerKey) -> io::Result<Option<History>> {
        match fs_err::tokio::read(self.path(&key.0)?).await {
            Ok(held) => rasm_mcp::decode::<(Command, History)>(&held).map_err(io::Error::other).map(|(command, history)| (command == key.0).then_some(history)),
            Err(error) if error.kind() == io::ErrorKind::NotFound => Ok(None),
            Err(error) => Err(error),
        }
    }

    async fn save(&self, key: &ServerKey, history: History) -> io::Result<()> {
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

#[derive(derive_more::Debug)]
struct Daemon {
    built: Build,
    started: Timestamp,
    idle: Option<Duration>,
    pool: Pool<ServerKey, Records>,
    processes: watch::Sender<BTreeMap<ServerKey, u32>>,
    #[debug(skip)]
    memory: Option<Box<dyn Fn() -> BTreeMap<u32, u64> + Send + Sync>>,
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
                let Launch { command, per } = *launch;
                let own = match per {
                    Per::Session => Some(self.joined.fetch_add(1, Ordering::Relaxed)),
                    Per::Directory | Per::User => None,
                };
                let key = (command, own);
                let member = self.pool.join(&key, || self.start(&key)).await;
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
                if matches!(error, IpcError::Refused(Refusal::Superseded)) {
                    self.superseded.cancel();
                }
                tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "session refused");
            }
        }
    }

    fn start(
        &self,
        key: &ServerKey,
    ) -> (Policy, impl FnMut(CancellationToken) -> BoxFuture<'static, io::Result<Link<rasm_process::Output, rasm_process::Input, BoxFuture<'static, String>>>> + Send + use<>, tracing::Span) {
        const IDLE: IdleTimeout = IdleTimeout { ratio: 300, floor: Duration::from_mins(1), ceiling: Duration::from_hours(2) };
        const LOGIN: Duration = Duration::from_secs(10);
        const GRACE: Duration = Duration::from_millis(500);
        const TAIL: usize = 8;
        const LINE: u64 = 4 << 10;
        const PATH_TMP: &str = "/tmp";
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
            idle: self.idle.map_or(IDLE, |idle| IdleTimeout { ratio: 0, floor: idle, ceiling: idle }),
            line: DEFAULT_MAX_LINE_LENGTH,
            queue: 256,
        };
        let (key, processes) = (key.clone(), self.processes.clone());
        let span = tracing::info_span!(parent: None, "server", program = %key.0.program.display());
        let connect = move |stop: CancellationToken| {
            let (key, processes, (attached, attachment)) = (key.clone(), processes.clone(), oneshot::channel());
            let Command { program, file: _, arguments, directory, environment } = key.0.clone();
            let mut exited = async move {
                let temporary = [PathBuf::from(PATH_TMP)];
                let login = match rasm_process::login(&directory, &temporary, time::Instant::now().checked_add(LOGIN), GRACE, &stop).await {
                    Ok(login) => login,
                    Err(error) => return Report::new(&error).to_string(),
                };
                let mut process = tokio::process::Command::new(program);
                process.args(arguments).current_dir(directory).env_clear().envs(login).envs(environment).stderr(Stdio::piped());
                let attach = async |streams| attached.send(streams).map_err(|rasm_process::Streams { id: _, stdin: _, stdout: _, stderr: _ }| io::Error::from(io::ErrorKind::BrokenPipe));
                match rasm_process::run(process, Some(BUFFER), &temporary, None, GRACE, &stop, attach).await {
                    Ok(status) | Err(rasm_process::ProcessError { program: _, process: Some((_, Some(status))), failures: _ }) => status.to_string(),
                    Err(error) => Report::new(&error).to_string(),
                }
            }
            .boxed();
            async move {
                let streams = match future::select(attachment, &mut exited).await {
                    Either::Left((Ok(streams), _)) => streams,
                    Either::Left((Err(oneshot::error::RecvError { .. }), _)) => return Err(io::Error::other(exited.await)),
                    Either::Right((exit, _)) => return Err(io::Error::other(exit)),
                };
                let (reader, writer) = streams.stdout.zip(streams.stdin).ok_or(io::ErrorKind::BrokenPipe)?;
                processes.send_if_modified(|processes| processes.insert(key.clone(), streams.id.get()).is_none());
                let read = |stderr| AbortOnDropHandle::new(task::spawn(rasm_process::tail(stderr, TAIL, LINE, |line| tracing::debug!(line, "server stderr")).in_current_span()));
                let closed = future::join(exited, OptionFuture::from(streams.stderr.map(read))).inspect(move |_| _ = processes.send_if_modified(|processes| processes.remove(&key).is_some())).map(
                    |(exit, tail)| match tail {
                        Some(Ok(tail)) if !tail.is_empty() => format!("{exit}; stderr: {}", tail.iter().join(" | ")),
                        Some(Ok(_)) | None => exit,
                        Some(Err(error)) => format!("{exit}; stderr unread: {}", Report::new(&error)),
                    },
                );
                Ok(Link { reader, writer, closed: closed.boxed() })
            }
            .boxed()
        };
        (policy, connect, span)
    }

    fn state(&self) -> State {
        let (own, processes, readings) = (std::process::id(), self.processes.borrow().clone(), self.memory.as_ref().map_or_default(|read| read()));
        let memory = |pid: u32| readings.get(&pid).copied();
        let reported = |(key, member): (&ServerKey, &rasm_mcp::Proxy)| {
            let (status, pid) = (member.status().borrow().clone(), processes.get(key).copied());
            ServerState {
                program: key.0.program.clone(),
                arguments: key.0.arguments.clone(),
                directory: key.0.directory.clone(),
                phase: status.phase,
                lifecycle: status.profile.as_ref().map(|profile| profile.lifecycle),
                sessions: status.sessions,
                calls: member.in_flight(),
                starts: status.starts,
                failures: status.failures,
                cause: status.cause.as_deref().map(str::to_owned),
                start_cost: status.profile.as_ref().map(|profile| profile.cost),
                pid,
                memory: pid.and_then(memory),
            }
        };
        let servers = Vec::from_iter(self.pool.members().iter().map(reported));
        let (sessions, calls) = servers.iter().fold((0_usize, 0_usize), |(sessions, calls), server| (sessions.saturating_add(server.sessions), calls.saturating_add(server.calls)));
        State { pid: own, build: self.built, started: self.started, memory: memory(own), sessions, calls, servers }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn serve(endpoint: Endpoint, cache: &Utf8Path, idle: Option<Duration>, built: Build) -> Result<(), MuxerError> {
    const LINGER: Duration = Duration::from_secs(10);
    const SETTLE: Duration = Duration::from_millis(250);
    let mut signals = rasm_cli::signals()?;
    let claim = ExponentialBuilder::new().with_min_delay(Duration::from_millis(1)).with_max_delay(Duration::from_millis(100)).without_max_times().with_total_delay(Some(Duration::from_secs(30)));
    let Some(Instance { listener, held }) = Instance::claim(&endpoint, claim).await? else { return Ok(()) };
    let files = rlimit::increase_nofile_limit(u64::MAX).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "open file limit not raised")).ok();
    tracing::info!(pid = std::process::id(), ?built, files, "listening");
    let stale = async |path: std::path::PathBuf| {
        let (command, IgnoredAny) = rasm_mcp::decode::<(Command, IgnoredAny)>(&fs_err::tokio::read(&path).await?).map_err(io::Error::other)?;
        Ok::<_, io::Error>(crate::build(&fs_err::tokio::metadata(&command.program).await?)? != command.file)
    };
    let folder = cache.join("servers");
    let records =
        fs_err::tokio::read_dir(&folder).map_ok(|listing| stream::try_unfold(listing, async |mut records| Ok(records.next_entry().await?.map(|entry| (entry, records))))).try_flatten_stream();
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
    let (processes, own) = (watch::Sender::<BTreeMap<ServerKey, u32>>::default(), std::process::id());
    let watched = processes.subscribe();
    let memory = metrics::process(&*global::meter_provider(), move || Vec::from_iter(watched.borrow().values().copied().chain([own])));
    let daemon = Arc::new(Daemon {
        built,
        started: Timestamp::now(),
        idle,
        pool: Pool::new(Records(folder)),
        processes,
        memory: memory.inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "process metrics unavailable")).ok().map(|read| Box::new(read) as Box<_>),
        joined: AtomicU64::new(0),
        superseded: CancellationToken::new(),
        stopping: CancellationToken::new(),
    });
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
                Err(error) => tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "session not accepted"),
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
    tokio::select! {
        biased;
        _ = signals.next() => {}
        () = sessions.wait().then(|()| daemon.pool.stop()) => {}
    }
    tracing::info!(reason, "stopped");
    drop(held);
    Ok(())
}
