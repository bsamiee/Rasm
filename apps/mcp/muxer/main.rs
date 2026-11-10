use core::num::NonZero;
use core::time::Duration;
use std::collections::BTreeMap;
use std::env;
use std::io::{self, Write as _};
use std::path::PathBuf;
use std::process::ExitCode;

use clap::{Args, Parser, ValueEnum, ValueHint};
use interprocess::local_socket::Name;
use jiff::Timestamp;
use jiff::fmt::friendly::{SpanParser, SpanPrinter};
use json_subscriber::JsonLayer;
use rasm_ipc::{Folders, Hello, IpcError, Limits};
use rasm_mcp::{Phase, Status};
use rasm_process::ProcessError;
use serde::{Deserialize, Serialize};
use tokio::runtime::Builder;
use tracing_appender::non_blocking::NonBlockingBuilder;
use tracing_appender::rolling::{InitError, RollingFileAppender, Rotation};
use tracing_subscriber::filter::EnvFilter;
use tracing_subscriber::layer::SubscriberExt as _;
use tracing_subscriber::{Layer as _, Registry};

// --- [TYPES] ---------------------------------------------------------------------------

type Build = (i64, i32, u64);

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default, ValueEnum, Serialize, Deserialize)]
enum Per {
    #[value(help = "Each session runs its own server, for a server holding per-session state")]
    Session,
    #[default]
    #[value(help = "Sessions with the same working directory share one server")]
    Directory,
    #[value(help = "Every session of the user shares one server, which runs in the home folder")]
    User,
}

#[derive(Debug, Clone, Copy, derive_more::Display)]
enum Operation {
    #[display("read this executable")]
    Executable,
    #[display("find the server command on PATH")]
    Resolve,
    #[display("read the server command file")]
    Inspect,
    #[display("resolve the server directory")]
    Directory,
    #[display("read a non-Unicode environment variable")]
    Environment,
    #[display("name the local socket")]
    Socket,
    #[display("start the runtime")]
    Runtime,
    #[display("watch for signals")]
    Signal,
    #[display("relay messages")]
    Relay,
    #[display("write the state report")]
    Report,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOL: u32 = 1;
const OPENING: Limits = Limits { line: 4 << 20, deadline: Duration::from_secs(10) };

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Parser)]
#[command(version, about = "Shares one stdio MCP server process across every agent session that configures the same server", long_about = None)]
#[command(override_usage = "mcp-muxer [OPTIONS] -- <COMMAND> [ARGUMENTS]...\n       mcp-muxer [OPTIONS]")]
#[command(after_help = "With a COMMAND, serves that stdio MCP server to this session through the shared daemon. With none, prints the daemon state as JSON.")]
struct Cli {
    #[arg(long, value_enum, default_value_t, help = "Sessions that share one server process")]
    per: Per,
    #[arg(long = "env", value_name = "NAME[=VALUE]", value_parser = variable)]
    #[arg(help = "Variable the server receives, a literal or this session's value; sessions with differing values never share a server")]
    environment: Vec<(String, Option<String>)>,
    #[arg(long, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "Directory the server runs in, default this session's working directory")]
    directory: Option<PathBuf>,
    #[command(flatten)]
    settings: Settings,
    #[arg(long, hide = true)]
    daemon: bool,
    #[arg(last = true, value_name = "COMMAND", value_hint = ValueHint::CommandWithArguments, help = "Stdio MCP server program and its arguments")]
    command: Vec<String>,
}

#[derive(Debug, Clone, Args)]
#[command(next_help_heading = "Daemon")]
struct Settings {
    #[arg(long, env = "MCP_MUXER_RUNTIME_DIR", value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "Folder holding the daemon socket and lock")]
    runtime_dir: Option<PathBuf>,
    #[arg(long, env = "MCP_MUXER_CACHE_DIR", value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "Folder holding each server's stored handshake")]
    cache_dir: Option<PathBuf>,
    #[arg(long, env = "MCP_MUXER_LOG_DIR", value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "Folder holding daily JSON line log files")]
    log_dir: Option<PathBuf>,
    #[arg(long, env = "MCP_MUXER_LOG", value_name = "DIRECTIVES", default_value = "info", help = "Log filter directives, a level or target=level rows")]
    log: String,
    #[arg(long, env = "MCP_MUXER_LOG_FILES", value_name = "COUNT", default_value = "7", help = "Daily log files kept, oldest removed first")]
    log_files: NonZero<usize>,
    #[arg(long, env = "MCP_MUXER_IDLE", value_name = "DURATION", value_parser = |text: &str| SpanParser::new().parse_unsigned_duration(text))]
    #[arg(help = "Time an unused server stays up, default 300 times its start cost between 1m and 2h")]
    idle: Option<Duration>,
}

#[derive(Debug, Clone, PartialEq, Eq, Hash, Serialize)]
struct Identity {
    folders: Folders,
    log: String,
    log_files: NonZero<usize>,
    idle: Option<Duration>,
}

impl Identity {
    fn resolve(settings: Settings) -> Result<Self, MuxerError> {
        let Settings { runtime_dir, cache_dir, log_dir, log, log_files, idle } = settings;
        Ok(Self { folders: Folders::resolve(env!("CARGO_PKG_NAME"), runtime_dir, cache_dir, log_dir)?, log, log_files, idle })
    }

    fn instance(&self) -> Result<String, MuxerError> { Ok(format!("{:016x}", xxhash_rust::xxh3::xxh3_64(&serde_json::to_vec(self).map_err(MuxerError::Encode)?))) }

    fn lock(&self) -> Result<PathBuf, MuxerError> { Ok(self.folders.runtime.join(self.instance()?).with_extension("lock")) }

    fn socket(&self) -> Result<Name<'static>, MuxerError> {
        let instance = self.instance()?;
        let named = cfg_select! {
            windows => {{
                use interprocess::local_socket::{GenericNamespaced, ToNsName as _};
                format!("{}-{instance}", env!("CARGO_PKG_NAME")).to_ns_name::<GenericNamespaced>()
            }}
            _ => {{
                use interprocess::local_socket::{GenericFilePath, ToFsName as _};
                self.folders.runtime.join(instance).with_extension("sock").to_fs_name::<GenericFilePath>()
            }}
        };
        named.map_err(failed(Operation::Socket))
    }

    fn arguments(&self) -> impl Iterator<Item = std::ffi::OsString> {
        let Self { folders: Folders { runtime, cache, logs }, log, log_files, idle } = self;
        let idle = idle.map(|idle| ["--idle".into(), SpanPrinter::new().unsigned_duration_to_string(&idle).into()]);
        [
            "--daemon".into(),
            "--runtime-dir".into(),
            runtime.clone().into_os_string(),
            "--cache-dir".into(),
            cache.clone().into_os_string(),
            "--log-dir".into(),
            logs.clone().into_os_string(),
            "--log".into(),
            log.clone().into(),
            "--log-files".into(),
            log_files.to_string().into(),
        ]
        .into_iter()
        .chain(idle.into_iter().flatten())
    }
}

#[derive(Debug, Clone, PartialEq, Eq, Hash, PartialOrd, Ord, Serialize, Deserialize)]
struct Command {
    program: PathBuf,
    file: Build,
    arguments: Vec<String>,
    directory: PathBuf,
    environment: BTreeMap<String, String>,
}

#[derive(Debug, Serialize, Deserialize)]
struct Launch {
    command: Command,
    per: Per,
    environment: Vec<(String, String)>,
}

#[derive(Debug, Serialize, Deserialize)]
enum Request {
    Serve(Box<Launch>),
    Report,
}

#[derive(Debug, Serialize, Deserialize)]
enum Welcome {
    Serving,
    State(Box<State>),
}

#[derive(Debug, Serialize, Deserialize)]
struct State {
    pid: u32,
    build: Build,
    started: Timestamp,
    memory: Option<u64>,
    connections: usize,
    servers: Vec<Server>,
}

#[derive(Debug, Serialize, Deserialize)]
struct Server {
    program: PathBuf,
    arguments: Vec<String>,
    directory: PathBuf,
    phase: String,
    sessions: usize,
    calls: usize,
    starts: u32,
    failures: u32,
    cause: Option<String>,
    start_cost: Option<Duration>,
    pid: Option<u32>,
    memory: Option<u64>,
}

impl Server {
    fn new(command: &Command, status: &Status, calls: usize, process: Option<(u32, Option<u64>)>) -> Self {
        let phase = match status.phase {
            Phase::Starting => "starting",
            Phase::Up => "up",
            Phase::Idle => "idle",
            Phase::Down => "down",
            Phase::Stopped => "stopped",
        };
        Self {
            program: command.program.clone(),
            arguments: command.arguments.clone(),
            directory: command.directory.clone(),
            phase: phase.to_owned(),
            sessions: status.sessions,
            calls,
            starts: status.starts,
            failures: status.failures,
            cause: status.cause.as_deref().map(str::to_owned),
            start_cost: status.catalog.as_ref().map(|catalog| catalog.cost),
            pid: process.map(|(pid, _)| pid),
            memory: process.and_then(|(_, memory)| memory),
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum MuxerError {
    #[error("cannot {operation}")]
    Io { operation: Operation, source: io::Error },
    #[error(transparent)]
    Ipc(#[from] IpcError),
    #[error(transparent)]
    Process(#[from] ProcessError),
    #[error("cannot encode a message")]
    Encode(#[source] serde_json::Error),
    #[error("cannot open the log folder")]
    Logs(#[from] InitError),
    #[error("log filter directives do not parse")]
    Filter(#[from] tracing_subscriber::filter::ParseError),
    #[error("a tracing subscriber is already installed")]
    Subscriber(#[from] tracing::subscriber::SetGlobalDefaultError),
}

impl From<MuxerError> for sysexits::ExitCode {
    fn from(value: MuxerError) -> Self {
        match value {
            MuxerError::Io { operation: _, source } => source.into(),
            MuxerError::Ipc(_) | MuxerError::Process(_) => Self::Unavailable,
            MuxerError::Encode(_) | MuxerError::Subscriber(_) => Self::Software,
            MuxerError::Logs(_) => Self::CantCreat,
            MuxerError::Filter(_) => Self::Usage,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn failed(operation: Operation) -> impl FnOnce(io::Error) -> MuxerError { move |source| MuxerError::Io { operation, source } }

fn variable(text: &str) -> Result<(String, Option<String>), String> {
    let (name, value) = text.split_once('=').map_or((text, None), |(name, value)| (name, Some(value.to_owned())));
    if name.is_empty() { Err("variable names no NAME".to_owned()) } else { Ok((name.to_owned(), value)) }
}

fn build(file: &std::fs::Metadata) -> io::Result<Build> {
    let modified = Timestamp::try_from(file.modified()?).map_err(io::Error::other)?;
    Ok((modified.as_second(), modified.subsec_nanosecond(), file.len()))
}

fn hello(body: Request, built: Build) -> Hello<Build, Request> { Hello { protocol: PROTOCOL, build: built, body } }

async fn launch(per: Per, declared: Vec<(String, Option<String>)>, directory: Option<PathBuf>, command: Vec<String>) -> Result<Launch, MuxerError> {
    #[expect(clippy::disallowed_methods, reason = "the server receives this session's whole environment, as a run without the muxer would, which no argument parser declares")]
    let environment = env::vars_os().map(|(name, value)| Some((name.into_string().ok()?, value.into_string().ok()?))).collect::<Option<Vec<_>>>();
    let environment = environment.ok_or_else(|| failed(Operation::Environment)(io::ErrorKind::InvalidData.into()))?;
    let held = |name: &str| environment.iter().find(|(known, _)| known == name).map(|(_, value)| value.clone());
    let declared = declared.into_iter().filter_map(|(name, value)| Some((value.or_else(|| held(&name))?, name))).map(|(value, name)| (name, value)).collect();
    let mut words = command.into_iter();
    let program = which::which(words.next().unwrap_or_default()).map_err(|source| failed(Operation::Resolve)(io::Error::new(io::ErrorKind::NotFound, source)))?;
    let file = build(&fs_err::tokio::metadata(&program).await.map_err(failed(Operation::Inspect))?).map_err(failed(Operation::Inspect))?;
    let home = || dirs::home_dir().ok_or_else(|| io::Error::from(io::ErrorKind::NotFound));
    let directory = match (per, directory) {
        (Per::User, None) => home(),
        (Per::Session | Per::Directory | Per::User, stated) => std::path::absolute(stated.unwrap_or_else(|| PathBuf::from("."))),
    }
    .map_err(failed(Operation::Directory))?;
    Ok(Launch { command: Command { program, file, arguments: words.collect(), directory, environment: declared }, per, environment })
}

async fn report(identity: &Identity, built: Build) -> Result<(), MuxerError> {
    use interprocess::local_socket::tokio::Stream;
    use interprocess::local_socket::traits::tokio::Stream as _;
    #[derive(Serialize)]
    struct Printed<'held> {
        daemon: Option<State>,
        folders: &'held Folders,
    }
    let daemon = match Stream::connect(identity.socket()?).await {
        Ok(stream) => match rasm_ipc::greet::<_, _, _, Welcome>(stream, &hello(Request::Report, built), OPENING).await?.body {
            Welcome::State(state) => Some(*state),
            Welcome::Serving => None,
        },
        Err(error) if rasm_ipc::absent(&error) => None,
        Err(source) => return Err(failed(Operation::Report)(source)),
    };
    let mut printed = serde_json::to_vec_pretty(&Printed { daemon, folders: &identity.folders }).map_err(MuxerError::Encode)?;
    printed.push(b'\n');
    io::stdout().lock().write_all(&printed).map_err(failed(Operation::Report))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod daemon;
mod relay;

fn run(cli: Cli) -> Result<ExitCode, MuxerError> {
    let Cli { per, environment, directory, settings, daemon, command } = cli;
    let identity = Identity::resolve(settings)?;
    #[expect(clippy::disallowed_methods, reason = "the relay starts the daemon from its own installed executable, and its file states the build")]
    let executable = env::current_exe().map_err(failed(Operation::Executable))?;
    let built = fs_err::metadata(&executable).and_then(|file| build(&file)).map_err(failed(Operation::Executable))?;
    if daemon {
        let appender = RollingFileAppender::builder().rotation(Rotation::DAILY).filename_prefix(env!("CARGO_PKG_NAME")).filename_suffix("jsonl").max_log_files(identity.log_files.get()).build(&identity.folders.logs)?;
        let (writer, _flushed) = NonBlockingBuilder::default().lossy(false).finish(appender);
        let mut lines = JsonLayer::new(writer);
        lines.with_timer("timestamp", tracing_subscriber::fmt::time::SystemTime).with_level("level").with_target("target").with_top_level_flattened_span_list().with_flattened_event();
        tracing::subscriber::set_global_default(Registry::default().with(lines.with_filter(EnvFilter::try_new(&identity.log)?)))?;
        let runtime = Builder::new_multi_thread().worker_threads(2).enable_all().build().map_err(failed(Operation::Runtime))?;
        runtime.block_on(daemon::serve(identity, built)).map(|()| ExitCode::SUCCESS)
    } else {
        let runtime = Builder::new_current_thread().enable_all().build().map_err(failed(Operation::Runtime))?;
        let served = runtime.block_on(async {
            if command.is_empty() {
                report(&identity, built).await.map(|()| ExitCode::SUCCESS)
            } else {
                relay::relay(&identity, built, executable, launch(per, environment, directory, command).await?).await
            }
        });
        runtime.shutdown_background();
        served
    }
}

fn main() -> ExitCode {
    run(Cli::parse()).unwrap_or_else(|error| {
        let chain = core::iter::successors(Some(&error as &dyn core::error::Error), |&cause| cause.source()).map(ToString::to_string).collect::<Vec<_>>().join(": ");
        match writeln!(io::stderr().lock(), "{}: {chain}", env!("CARGO_PKG_NAME")) {
            Ok(()) | Err(_) => sysexits::ExitCode::from(error).into(),
        }
    })
}
