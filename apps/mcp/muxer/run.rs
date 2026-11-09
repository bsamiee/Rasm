use std::{
    collections::BTreeMap,
    env::{self, VarError},
    error::Report,
    io,
    os::{
        fd::{AsFd, OwnedFd},
        unix::{self, fs::MetadataExt as _, process::CommandExt as _},
    },
    process::Stdio,
};

use camino::Utf8PathBuf;
use clap::{Args, ValueEnum, ValueHint, error::ErrorKind};
use futures::StreamExt as _;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_command::Signals;
use rustix::fs::{FileType, fstat};
use tokio::{
    io::{AsyncBufReadExt as _, AsyncWriteExt as _, BufReader},
    net::{UnixStream, unix::pipe},
    process::Command,
};
use tokio_util::either::Either;

use crate::{
    MuxerError, Operation,
    daemon::{Launch, Server, logs, socket},
    decoded,
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, Default, ValueEnum)]
enum Share {
    #[default]
    #[value(help = "Sessions of one project share the server, which runs in the project root")]
    Project,
    #[value(help = "Every session of the user shares the server, which runs in the home folder")]
    User,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Args)]
pub(crate) struct Run {
    #[arg(long, value_enum, default_value_t, help = "Sessions sharing one server process")]
    share: Share,
    #[arg(long, value_name = "NAME[=VALUE]", value_parser = declared)]
    #[arg(help = "Variable the server receives, a literal or this session's value; sessions with differing values never share a server")]
    env: Vec<(String, Option<String>)>,
    #[arg(long, env = "CLAUDE_PROJECT_DIR", value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "Project root a project-shared server runs in, the working directory when unset")]
    project: Option<Utf8PathBuf>,
    #[arg(value_hint = ValueHint::CommandName, help = "Stdio MCP server program, searched on PATH")]
    program: String,
    #[arg(trailing_var_arg = true, allow_hyphen_values = true, help = "Arguments the server program receives")]
    arguments: Vec<String>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn declared(text: &str) -> Result<(String, Option<String>), clap::Error> {
    let (name, value) = text.split_once('=').map_or((text, None), |(name, value)| (name, Some(value.to_owned())));
    (!name.is_empty()).then(|| (name.to_owned(), value)).ok_or_else(|| clap::Error::raw(ErrorKind::InvalidValue, "variable names no NAME"))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn relayed(run: Run, mut signals: Signals) -> Result<(), MuxerError> {
    fn attached<P>(fd: impl AsFd, piped: impl FnOnce(OwnedFd) -> io::Result<P>) -> io::Result<Either<UnixStream, P>> {
        let owned = fd.as_fd().try_clone_to_owned()?;
        match FileType::from_raw_mode(fstat(&owned)?.st_mode) {
            FileType::Socket => {
                let socket = unix::net::UnixStream::from(owned);
                socket.set_nonblocking(true)?;
                UnixStream::from_std(socket).map(Either::Left)
            }
            FileType::Fifo | FileType::RegularFile | FileType::Directory | FileType::Symlink | FileType::CharacterDevice | FileType::BlockDevice | FileType::Unknown => piped(owned).map(Either::Right),
        }
    }
    const INHERITED: [&str; 11] = ["HOME", "LOGNAME", "PATH", "SHELL", "USER", "__CF_USER_TEXT_ENCODING", "LANG", "LC_ALL", "TERM", "TMPDIR", "TZ"];
    let failed = |operation| move |source| MuxerError::Io { operation, source };
    #[expect(clippy::disallowed_methods, reason = "a server receives the variables its harness passes every stdio server and the ones its row names, which no parser declares")]
    let read = |name: &str| match env::var(name) {
        Ok(value) => Ok(Some(value)),
        Err(VarError::NotPresent) => Ok(None),
        Err(VarError::NotUnicode(_)) => Err(failed(Operation::Variable(name.to_owned()))(io::ErrorKind::InvalidData.into())),
    };
    let Run { share, env: variables, project, program, arguments } = run;
    let resolved = which::which(&program).map_err(|source| failed(Operation::Resolve(program.clone()))(io::Error::new(io::ErrorKind::NotFound, source)))?;
    let utf8 = |path| Utf8PathBuf::from_path_buf(path).map_err(|_| failed(Operation::Directory)(io::ErrorKind::InvalidFilename.into()));
    let metadata = fs_err::tokio::metadata(&resolved).await.map_err(failed(Operation::Resolve(program.clone())))?;
    #[expect(clippy::disallowed_methods, reason = "a stdio server's project is the folder its harness starts it in")]
    let working = || env::current_dir().map_err(failed(Operation::Directory));
    let directory = match (share, project) {
        (Share::Project, Some(project)) => project,
        (Share::Project, None) => utf8(working()?)?,
        (Share::User, _) => dirs::home_dir().ok_or_else(|| failed(Operation::Locate)(io::ErrorKind::NotFound.into())).and_then(utf8)?,
    };
    let launch = Launch {
        server: Server {
            program: utf8(resolved)?,
            identity: (metadata.dev(), metadata.ino(), metadata.modified().map_err(failed(Operation::Resolve(program.clone())))?),
            arguments: arguments.clone(),
            directory,
            declared: variables
                .into_iter()
                .filter_map(|(name, value)| value.map_or_else(|| read(&name), |value| Ok(Some(value))).map(|value| value.map(|value| (name, value))).transpose())
                .try_collect()?,
        },
        environment: INHERITED.iter().filter_map(|name| read(name).map(|value| value.map(|value| ((*name).to_owned(), value))).transpose()).try_collect::<BTreeMap<_, _>>()?,
    };
    let opened = async {
        let socket = socket()?;
        let stream = match UnixStream::connect(&socket).await {
            Ok(stream) => stream,
            Err(error) if matches!(error.kind(), io::ErrorKind::NotFound | io::ErrorKind::ConnectionRefused) => {
                #[expect(clippy::disallowed_methods, reason = "the session starts the daemon from its own installed executable")]
                let executable = env::current_exe().map_err(failed(Operation::Start))?;
                let mut daemon = Command::new(executable);
                daemon.arg("--log-directory").arg(logs()?).arg("daemon").stdin(Stdio::null()).stdout(Stdio::piped()).stderr(Stdio::null()).as_std_mut().setsid(true);
                let mut listening = (daemon.spawn().map_err(failed(Operation::Start))?.stdout.take().map(BufReader::new), String::new());
                if let (Some(stdout), line) = &mut listening {
                    stdout.read_line(line).await.map_err(failed(Operation::Start))?;
                }
                UnixStream::connect(&socket).await.map_err(failed(Operation::Connect(socket.clone())))?
            }
            Err(source) => return Err(failed(Operation::Connect(socket))(source)),
        };
        let mut stream = BufReader::new(stream);
        let mut line = serde_json::to_vec(&launch)?;
        line.push(b'\n');
        stream.write_all(&line).await.map_err(failed(Operation::Opening))?;
        let mut answer = String::new();
        stream.read_line(&mut answer).await.map_err(failed(Operation::Opening))?;
        decoded::<()>(&answer).map(|()| stream).map_err(MuxerError::from)
    };
    match opened.await {
        Ok(mut stream) => {
            let mut stdio = attached(io::stdin(), pipe::Receiver::from_owned_fd)
                .and_then(|input| attached(io::stdout(), pipe::Sender::from_owned_fd).map(|output| tokio::io::join(input, output)))
                .map_err(failed(Operation::Relay))?;
            tokio::select! {
                copied = tokio::io::copy_bidirectional(&mut stdio, &mut stream) => copied.map(drop).map_err(failed(Operation::Relay)),
                _ = signals.next() => Ok(()),
            }
        }
        Err(error) => {
            tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(&error), program, "daemon unavailable, running the server directly");
            Err(failed(Operation::Exec(program.clone()))(std::process::Command::new(&program).args(&arguments).exec()))
        }
    }
}
