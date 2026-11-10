#![feature(default_field_values, error_reporter)]

use core::iter;
use core::time::Duration;
use std::collections::BTreeMap;
use std::error::Report;
use std::ffi::OsString;
use std::path::{self, PathBuf};
use std::process::{ExitCode, Stdio};
use std::{env, io};

use backon::ExponentialBuilder;
use camino::Utf8PathBuf;
use clap::{Parser, ValueEnum, ValueHint};
use itertools::Itertools as _;
use jiff::Timestamp;
use jiff::fmt::friendly::SpanParser;
use nonempty_collections::NEVec;
use opentelemetry::global;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_command::{CommandError, Folder, Invocation};
use rasm_ipc::{Endpoint, Hello, IpcError, Limits, Opened};
use rasm_mcp::{Ended, Phase, Resumable};
use rasm_observability::Environment;
use rasm_observability::process::Program;
use rasm_process::ProcessError;
use rmcp::transport::async_rw::DEFAULT_MAX_LINE_LENGTH;
use serde::{Deserialize, Serialize};
use tokio::io::{AsyncRead, AsyncWrite, AsyncWriteExt as _};
use tokio::runtime::Builder;
use tokio_util::sync::CancellationToken;
use tracing_subscriber::layer::Identity;

// --- [TYPES] ---------------------------------------------------------------------------

type Build = (i64, i32, u64);

type Key = (Command, Option<u64>);

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
    #[display("relay messages")]
    Relay,
    #[display("write the state report")]
    Report,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOL: u32 = 3;
const OPENING: Limits = Limits { line: 4 << 20, deadline: Duration::from_secs(10) };
const BUFFER: usize = 256 << 10;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Parser)]
#[command(version, about = "Shares one stdio MCP server process across every agent session that configures the same server", long_about = None)]
#[command(override_usage = "mcp-muxer [OPTIONS] -- <COMMAND> [ARGUMENTS]...\n       mcp-muxer [OPTIONS]")]
#[command(after_help = "With a COMMAND, serves that stdio MCP server to this session through the shared daemon. With none, prints the daemon state as JSON.\n\
Environment: a shared server starts with the variables of the session that started it, without trace context; a --per session server and an unshared run receive the whole session environment.\n\
The daemon exports telemetry under the OTEL_ variables of the session that started it.\n\
Requests from a server opened with initialize: calls of a tool not yet seen to finish without asking run one at a time, so a request reaches the session that called; other calls run concurrently.")]
struct Cli {
    #[arg(long, value_enum, default_value_t, help = "Sessions that share one server process")]
    per: Per,
    #[arg(long, value_name = "NAME[=VALUE]")]
    #[arg(help = "Variable the server receives, a literal or this session's value; sessions with differing values never share a server")]
    env: Vec<String>,
    #[arg(long, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "Directory the server runs in, default this session's working directory")]
    directory: Option<PathBuf>,
    #[arg(long, env = "MCP_MUXER_IDLE", value_name = "DURATION", help_heading = "Daemon", value_parser = |text: &str| SpanParser::new().parse_unsigned_duration(text))]
    #[arg(help = "Time an unused server stays up, default 300 times its start cost between 1m and 2h")]
    idle: Option<Duration>,
    #[arg(long, hide = true, value_name = "INSTANCE")]
    daemon: Option<String>,
    #[arg(last = true, value_name = "COMMAND", value_hint = ValueHint::CommandWithArguments, help = "Stdio MCP server program and its arguments")]
    command: Vec<String>,
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
    sessions: usize,
    calls: usize,
    servers: Vec<Server>,
}

#[derive(Debug, Serialize, Deserialize)]
struct Server {
    program: PathBuf,
    arguments: Vec<String>,
    directory: PathBuf,
    phase: Phase,
    era: Option<rasm_mcp::Era>,
    sessions: usize,
    calls: usize,
    starts: u32,
    failures: usize,
    cause: Option<String>,
    start_cost: Option<Duration>,
    pid: Option<u32>,
    memory: Option<u64>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum MuxerError {
    #[error("cannot {operation}")]
    Io { operation: Operation, source: io::Error },
    #[error(transparent)]
    Entry(#[from] CommandError),
    #[error(transparent)]
    Ipc(#[from] IpcError),
    #[error(transparent)]
    Process(#[from] ProcessError),
    #[error("cannot encode a message")]
    Encode(#[source] serde_json::Error),
    #[error("environment variable {} is not Unicode", name.display())]
    Environment { name: OsString },
    #[error("--env {text} names no NAME")]
    Variable { text: String },
    #[error("daemon closed the session before any answer")]
    Unanswered,
}

impl From<MuxerError> for sysexits::ExitCode {
    fn from(value: MuxerError) -> Self {
        match value {
            MuxerError::Io { operation: _, source } => source.into(),
            MuxerError::Entry(error) => error.into(),
            MuxerError::Ipc(_) | MuxerError::Process(_) | MuxerError::Unanswered => Self::Unavailable,
            MuxerError::Encode(_) => Self::Software,
            MuxerError::Environment { name: _ } => Self::DataErr,
            MuxerError::Variable { text: _ } => Self::Usage,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn build(file: &std::fs::Metadata) -> io::Result<Build> {
    let modified = Timestamp::try_from(file.modified()?).map_err(io::Error::other)?;
    Ok((modified.as_second(), modified.subsec_nanosecond(), file.len()))
}

fn digest(value: &impl Serialize) -> serde_json::Result<String> { Ok(format!("{:016x}", xxhash_rust::xxh3::xxh3_64(&serde_json::to_vec(value)?))) }

fn server(command: &Command, environment: &[(String, String)], stderr: Stdio) -> tokio::process::Command {
    let mut server = tokio::process::Command::new(&command.program);
    server.args(&command.arguments).current_dir(&command.directory).env_clear().envs(environment.iter().map(|(name, value)| (name, value))).envs(&command.environment).stderr(stderr);
    server
}

async fn run(invocation: Invocation<Cli>) -> Result<ExitCode, MuxerError> {
    const REACH: ExponentialBuilder =
        ExponentialBuilder::new().with_min_delay(Duration::from_millis(2)).with_max_delay(Duration::from_millis(250)).without_max_times().with_total_delay(Some(Duration::from_secs(12)));
    const UNANSWERED: u8 = 3;
    #[derive(Serialize)]
    struct Printed<'held> {
        daemon: Option<State>,
        runtime: &'held Utf8PathBuf,
        cache: &'held Utf8PathBuf,
    }
    async fn unshared<I, O>(unreached: MuxerError, mut session: Resumable<I, O>, command: &Command, environment: &[(String, String)]) -> Result<ExitCode, MuxerError>
    where
        I: AsyncRead + Unpin,
        O: AsyncWrite + Unpin,
    {
        const GRACE: Duration = Duration::from_secs(3);
        tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(&unreached), "daemon unavailable, running the server unshared");
        let status = rasm_process::run(server(command, environment, Stdio::inherit()), Some(BUFFER), None, GRACE, &CancellationToken::new(), async |streams| match (streams.stdout, streams.stdin) {
            (Some(reader), Some(writer)) => session.over(reader, writer).await.map(drop),
            (_, _) => Err(io::ErrorKind::BrokenPipe.into()),
        })
        .await?;
        Ok(match status.code().map(u8::try_from) {
            Some(Ok(code)) => ExitCode::from(code),
            Some(Err(error)) => {
                tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "server exit status outside the exit code range");
                ExitCode::FAILURE
            }
            None => ExitCode::FAILURE,
        })
    }
    let failed = |operation: Operation| move |source: io::Error| MuxerError::Io { operation, source };
    let Invocation { args: Cli { per, env: declared, directory, idle, daemon, command }, executable, styles: _, folders, filter: _, parent, settings, export } = invocation;
    let folder = |folder: Folder| folders[folder].as_ref().ok_or(CommandError::Folder { folder });
    let (runtime, cache) = (folder(Folder::Runtime)?, folder(Folder::Cache)?);
    let instance = match &daemon {
        Some(instance) => instance.clone(),
        None => format!("{}-{}", env!("CARGO_PKG_NAME"), digest(&settings).map_err(MuxerError::Encode)?),
    };
    let endpoint = Endpoint::new(runtime.as_std_path(), &instance, Some(BUFFER));
    let built = fs_err::tokio::metadata(&executable).await.and_then(|file| build(&file)).map_err(failed(Operation::Executable))?;
    let hello = |body: Request| Hello { protocol: PROTOCOL, build: built, body };
    if daemon.is_some() {
        return daemon::serve(endpoint?, cache, idle, built).await.map(|()| ExitCode::SUCCESS);
    }
    let Some(command) = NEVec::try_from_vec(command) else {
        let daemon = match rasm_ipc::attach::<_, _, Welcome>(&endpoint?, &hello(Request::Report), OPENING).await? {
            Some(Opened { body: Welcome::State(state), reader: _, writer: _ }) => Some(*state),
            Some(Opened { body: Welcome::Serving, reader: _, writer: _ }) | None => None,
        };
        let mut printed = serde_json::to_vec_pretty(&Printed { daemon, runtime, cache }).map_err(MuxerError::Encode)?;
        printed.push(b'\n');
        tokio::io::stdout().write_all(&printed).await.map_err(failed(Operation::Report))?;
        return Ok(ExitCode::SUCCESS);
    };
    #[expect(clippy::disallowed_methods, reason = "the server receives this session's whole environment, as a run without the muxer would, and no argument parser declares its names")]
    let variables = env::vars_os().map(|(name, value)| match (name.into_string(), value.into_string()) {
        (Ok(name), Ok(value)) => Ok((name, value)),
        (Err(name), _) | (Ok(_), Err(name)) => Err(MuxerError::Environment { name }),
    });
    let environment = variables.collect::<Result<Vec<_>, _>>()?;
    let held = |name: &str| environment.iter().find(|(known, _)| known == name).map(|(_, value)| value.clone());
    let stated = declared.into_iter().map(|text| match text.split_once('=') {
        Some(("", _)) => Err(MuxerError::Variable { text }),
        Some((name, value)) => Ok(Some((name.to_owned(), value.to_owned()))),
        None if text.is_empty() => Err(MuxerError::Variable { text }),
        None => Ok(held(&text).map(|value| (text, value))),
    });
    let declared = stated.flatten_ok().collect::<Result<BTreeMap<_, _>, _>>()?;
    let (program, arguments) = command.split_first();
    let program = which::which(program).map_err(|source| failed(Operation::Resolve)(io::Error::new(io::ErrorKind::NotFound, source)))?;
    let file = fs_err::tokio::metadata(&program).await.and_then(|file| build(&file)).map_err(failed(Operation::Inspect))?;
    let directory = match (per, directory) {
        (Per::User, None) => dirs::home_dir().ok_or_else(|| io::Error::from(io::ErrorKind::NotFound)),
        (Per::Session | Per::Directory | Per::User, stated) => path::absolute(stated.unwrap_or_else(|| PathBuf::from("."))),
    }
    .map_err(failed(Operation::Directory))?;
    let command = Command { program, file, arguments: arguments.to_vec(), directory, environment: declared };
    let carried = global::get_text_map_propagator(|propagator| Vec::from_iter(propagator.fields().map(str::to_owned)));
    let shared = environment.iter().filter(|(name, _)| per == Per::Session || !carried.iter().any(|field| field.eq_ignore_ascii_case(name)));
    let request = hello(Request::Serve(Box::new(Launch { command: command.clone(), per, environment: shared.cloned().collect() })));
    let start = || {
        let located = folders.iter().filter_map(|(folder, path)| Some([format!("--{folder}-dir"), path.as_ref()?.to_string()])).flatten();
        let mut daemon = tokio::process::Command::new(&executable);
        daemon.args(["--daemon", &instance]).args(located).env_clear().envs(environment.iter().filter(|(name, _)| name.starts_with("OTEL_")).map(|(name, value)| (name, value)));
        daemon.envs(iter::chain(&settings, &export).map(|(name, value)| (name, value)));
        daemon.stdin(Stdio::null()).stdout(Stdio::null()).stderr(Stdio::null());
        #[cfg(target_vendor = "apple")]
        daemon.env("MallocSpaceEfficient", "1");
        global::get_text_map_propagator(|propagator| propagator.inject_context(&parent, &mut Environment(daemon.as_std_mut())));
        rasm_process::detached(daemon).spawn().map(drop)
    };
    let (input, output) = rasm_process::standard(Some(BUFFER)).map_err(failed(Operation::Relay))?;
    let mut session = Resumable::new(input, output, DEFAULT_MAX_LINE_LENGTH);
    let endpoint = match endpoint {
        Ok(endpoint) => endpoint,
        Err(error) => return unshared(MuxerError::Ipc(error), session, &command, &environment).await,
    };
    let mut spare = 0..UNANSWERED;
    let unreached = loop {
        let (reader, writer) = match rasm_ipc::connect::<_, _, _, Welcome>(&endpoint, &request, OPENING, REACH, &start).await {
            Ok(Opened { body: Welcome::Serving | Welcome::State(_), reader, writer }) => (reader, writer),
            Err(error) => break MuxerError::Ipc(error),
        };
        match session.over(reader, writer).await.map_err(failed(Operation::Relay))? {
            Ended::Client => return Ok(ExitCode::SUCCESS),
            Ended::Link { delivered: true } => spare = 0..UNANSWERED,
            Ended::Link { delivered: false } => match spare.next() {
                Some(_) => {}
                None => break MuxerError::Unanswered,
            },
        }
    };
    unshared(unreached, session, &command, &environment).await
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod daemon;

fn main() -> ExitCode {
    rasm_command::run(
        Program { scopes: Vec::from_iter([env!("CARGO_CRATE_NAME"), "rasm_ipc", "rasm_mcp", "rasm_process"].map(|scope| (scope, None))), .. },
        io::stderr,
        Identity::new(),
        |cli: &Cli| if cli.daemon.is_some() { Builder::new_multi_thread() } else { Builder::new_current_thread() },
        async |invocation| run(invocation).await.map_err(NEVec::new),
    )
}
