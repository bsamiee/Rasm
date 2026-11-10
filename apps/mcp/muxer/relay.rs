use core::time::Duration;
use std::collections::BTreeSet;
use std::io::{self, Write as _};
use std::path::PathBuf;
use std::process::{ExitCode, Stdio};

use backon::ExponentialBuilder;
use bytes::Bytes;
use futures::StreamExt as _;
use rasm_ipc::{IpcError, Opened, Refusal};
use rasm_mcp::{Envelope, Line, Lines, Message};
use rmcp::ErrorData;
use rmcp::model::{
    ConstString as _, ErrorCode, InitializeResultMethod, InitializedNotificationMethod, JsonRpcVersion2_0, PromptListChangedNotificationMethod, ResourceListChangedNotificationMethod,
    ToolListChangedNotificationMethod,
};
use rmcp::transport::async_rw::DEFAULT_MAX_LINE_LENGTH;
use serde::Deserialize;
use serde_json::value::RawValue;
use tokio::io::{AsyncRead, AsyncWrite, AsyncWriteExt as _, BufWriter};
use tokio_util::codec::FramedRead;
use tokio_util::sync::CancellationToken;

use crate::{Build, Identity, Launch, MuxerError, OPENING, Operation, Request, Welcome, failed, hello};

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUFFER: usize = 8 << 10;
const GRACE: Duration = Duration::from_secs(3);
const ATTEMPTS: u32 = 3;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Ended {
    Client,
    Link,
}

#[derive(Debug, Default)]
struct Session {
    initialize: Option<(Bytes, Bytes)>,
    initialized: Option<Bytes>,
    lists: Vec<&'static str>,
    pending: BTreeSet<Bytes>,
    swallow: bool,
    delivered: bool,
}

impl Session {
    fn sent(&mut self, line: &Bytes) {
        match Envelope::read(line) {
            Ok(Envelope { id: Some(id), method: Some(method), .. }) => {
                let id = line.slice_ref(id.get().as_bytes());
                if method == InitializeResultMethod::VALUE {
                    self.initialize = Some((id.clone(), line.clone()));
                }
                self.pending.insert(id);
            }
            Ok(Envelope { id: None, method: Some(method), .. }) if method == InitializedNotificationMethod::VALUE => self.initialized = Some(line.clone()),
            Ok(_) | Err(_) => {}
        }
    }

    fn received(&mut self, line: &[u8]) -> bool {
        #[derive(Deserialize)]
        struct Change {
            #[serde(rename = "listChanged")]
            list_changed: Option<bool>,
        }
        #[derive(Deserialize)]
        struct Capabilities {
            tools: Option<Change>,
            prompts: Option<Change>,
            resources: Option<Change>,
        }
        #[derive(Deserialize)]
        struct Initialized {
            capabilities: Capabilities,
        }
        let Ok(Envelope { id: Some(id), method: None, result, .. }) = Envelope::read(line) else { return true };
        let answered = self.pending.remove(id.get().as_bytes());
        let greeting = self.initialize.as_ref().is_some_and(|(held, _)| held == id.get().as_bytes());
        match (greeting, self.swallow) {
            (true, true) => {
                self.swallow = false;
                false
            }
            (true, false) => {
                let declared = result.and_then(|result| serde_path_to_error::deserialize::<_, Initialized>(&mut serde_json::Deserializer::from_str(result.get())).ok());
                self.lists = declared.into_iter().flat_map(|Initialized { capabilities: Capabilities { tools, prompts, resources } }| {
                    [(tools, ToolListChangedNotificationMethod::VALUE), (prompts, PromptListChangedNotificationMethod::VALUE), (resources, ResourceListChangedNotificationMethod::VALUE)]
                        .into_iter()
                        .filter_map(|(change, method)| change?.list_changed?.then_some(method))
                })
                .collect();
                true
            }
            (false, _) => answered,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn failure(id: &[u8], message: String) -> Option<Bytes> {
    let id = serde_path_to_error::deserialize::<_, &RawValue>(&mut serde_json::Deserializer::from_slice(id)).ok()?;
    let error = ErrorData::new(ErrorCode::INTERNAL_ERROR, message, None);
    Message::<&RawValue, (), ()> { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: None, params: None, result: None, error: Some(&error) }.line().ok()
}

fn stdio() -> io::Result<(impl AsyncRead + Unpin, impl AsyncWrite + Unpin)> {
    cfg_select! {
        unix => {{
            use std::os::fd::{AsFd, OwnedFd};
            use std::os::unix::fs::FileTypeExt as _;
            use tokio::net::UnixStream;
            use tokio::net::unix::pipe;
            use tokio_util::either::Either;
            fn attached<P, B>(descriptor: impl AsFd, piped: impl FnOnce(OwnedFd) -> io::Result<P>, blocking: B) -> io::Result<Either<UnixStream, Either<P, B>>> {
                let file = std::fs::File::from(descriptor.as_fd().try_clone_to_owned()?);
                let kind = file.metadata()?.file_type();
                if kind.is_socket() {
                    let socket = std::os::unix::net::UnixStream::from(OwnedFd::from(file));
                    socket.set_nonblocking(true)?;
                    UnixStream::from_std(socket).map(Either::Left)
                } else if kind.is_fifo() {
                    piped(OwnedFd::from(file)).map(|pipe| Either::Right(Either::Left(pipe)))
                } else {
                    Ok(Either::Right(Either::Right(blocking)))
                }
            }
            Ok((attached(io::stdin(), pipe::Receiver::from_owned_fd, tokio::io::stdin())?, attached(io::stdout(), pipe::Sender::from_owned_fd, tokio::io::stdout())?))
        }}
        _ => Ok((tokio::io::stdin(), tokio::io::stdout())),
    }
}

async fn connect(identity: &Identity, built: Build, executable: &PathBuf, launch: &Launch) -> Result<Opened<interprocess::local_socket::tokio::Stream, Welcome>, MuxerError> {
    let name = identity.socket()?;
    let schedule = ExponentialBuilder::new().with_min_delay(Duration::from_millis(2)).with_max_delay(Duration::from_millis(250)).without_max_times().with_total_delay(Some(Duration::from_secs(20)));
    let start = || {
        let mut daemon = tokio::process::Command::new(executable);
        daemon.args(identity.arguments()).env_clear().stdin(Stdio::null()).stdout(Stdio::null()).stderr(Stdio::null());
        rasm_process::detached(daemon).spawn().map(drop)
    };
    let request = hello(Request::Serve(Box::new(Launch { command: launch.command.clone(), per: launch.per, environment: launch.environment.clone() })), built);
    let mut attempts = 0_u32;
    loop {
        let stream = rasm_ipc::connect(&name, schedule, start).await?;
        match rasm_ipc::greet(stream, &request, OPENING).await {
            Err(IpcError::Refused(Refusal::Superseded) | IpcError::Closed) if attempts < ATTEMPTS => attempts = attempts.saturating_add(1),
            opened => return opened.map_err(MuxerError::from),
        }
    }
}

async fn pump<I, O, R, W>(client: &mut FramedRead<I, Lines>, answers: &mut BufWriter<O>, reader: R, writer: W, session: &mut Session) -> io::Result<Ended>
where
    I: AsyncRead + Unpin,
    O: AsyncWrite + Unpin,
    R: AsyncRead + Unpin,
    W: AsyncWrite + Unpin, {
    let mut link = FramedRead::with_capacity(reader, Lines::new(usize::MAX, BUFFER), BUFFER);
    let mut requests = BufWriter::with_capacity(BUFFER, writer);
    if let Some((id, line)) = &session.initialize {
        session.swallow = !session.pending.contains(id);
        let replayed = async {
            requests.write_all(line).await?;
            if let Some(line) = &session.initialized {
                requests.write_all(line).await?;
            }
            requests.flush().await
        };
        if replayed.await.is_err() {
            return Ok(Ended::Link);
        }
    }
    loop {
        tokio::select! {
            line = link.next() => match line {
                Some(Ok(Line::Whole(line))) => if session.received(&line) {
                    session.delivered = true;
                    answers.write_all(&line).await?;
                    if link.read_buffer().is_empty() {
                        answers.flush().await?;
                    }
                },
                Some(Ok(Line::Oversized(_))) => {}
                Some(Err(_)) | None => return Ok(Ended::Link),
            },
            line = client.next() => match line {
                Some(Ok(Line::Whole(line))) => {
                    session.sent(&line);
                    let forwarded = async {
                        requests.write_all(&line).await?;
                        if client.read_buffer().is_empty() { requests.flush().await } else { Ok(()) }
                    };
                    if forwarded.await.is_err() {
                        return Ok(Ended::Link);
                    }
                }
                Some(Ok(Line::Oversized(head))) => {
                    let refused = rasm_mcp::identity(&head).and_then(|id| failure(&id, format!("request exceeds {DEFAULT_MAX_LINE_LENGTH} bytes, send less")));
                    if let Some(refused) = refused {
                        answers.write_all(&refused).await?;
                        answers.flush().await?;
                    }
                }
                Some(Err(error)) => return Err(error),
                None => return Ok(Ended::Client),
            },
        }
    }
}

async fn recovered<O>(answers: &mut BufWriter<O>, session: &mut Session) -> io::Result<()>
where O: AsyncWrite + Unpin {
    let greeting = session.initialize.as_ref().map(|(id, _)| id.clone());
    let lost = Vec::from_iter(session.pending.extract_if(.., |id| Some(&*id) != greeting.as_ref()));
    for line in lost.iter().filter_map(|id| failure(id, "connection to the server was lost before it answered; call again".to_owned())) {
        answers.write_all(&line).await?;
    }
    for method in session.lists.iter().filter(|_| session.initialized.is_some()) {
        if let Ok(line) = (Message::<u64, (), ()> { jsonrpc: JsonRpcVersion2_0, id: None, method: Some(method), params: None, result: None, error: None }).line() {
            answers.write_all(&line).await?;
        }
    }
    answers.flush().await
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn relay(identity: &Identity, built: Build, executable: PathBuf, launch: Launch) -> Result<ExitCode, MuxerError> {
    let mut opened = connect(identity, built, &executable, &launch).await;
    let (input, output) = stdio().map_err(failed(Operation::Relay))?;
    let mut client = FramedRead::with_capacity(input, Lines::new(DEFAULT_MAX_LINE_LENGTH, BUFFER), BUFFER);
    let mut answers = BufWriter::with_capacity(BUFFER, output);
    let mut session = Session::default();
    let mut losses = 0_u32;
    loop {
        match opened {
            Ok(Opened { body: Welcome::Serving | Welcome::State(_), reader, writer }) if losses < ATTEMPTS => {
                session.delivered = false;
                match pump(&mut client, &mut answers, reader, writer, &mut session).await.map_err(failed(Operation::Relay))? {
                    Ended::Client => return Ok(ExitCode::SUCCESS),
                    Ended::Link => {
                        losses = if session.delivered { 0 } else { losses.saturating_add(1) };
                        recovered(&mut answers, &mut session).await.map_err(failed(Operation::Relay))?;
                        opened = connect(identity, built, &executable, &launch).await;
                    }
                }
            }
            unavailable => {
                let cause = unavailable.err().map_or_else(|| "daemon closed the session repeatedly".to_owned(), |error| {
                    core::iter::successors(Some(&error as &dyn core::error::Error), |&cause| cause.source()).map(ToString::to_string).collect::<Vec<_>>().join(": ")
                });
                match writeln!(io::stderr().lock(), "{}: daemon unavailable, running the server unshared: {cause}", env!("CARGO_PKG_NAME")) {
                    Ok(()) | Err(_) => {}
                }
                let mut server = tokio::process::Command::new(&launch.command.program);
                server
                    .args(&launch.command.arguments)
                    .current_dir(&launch.command.directory)
                    .env_clear()
                    .envs(launch.environment.iter().map(|(name, value)| (name, value)))
                    .envs(&launch.command.environment)
                    .stdin(Stdio::piped())
                    .stdout(Stdio::piped())
                    .stderr(Stdio::inherit());
                let status = rasm_process::run(server, None, GRACE, &CancellationToken::new(), async |pipes| match (pipes.stdout, pipes.stdin) {
                    (Some(reader), Some(writer)) => pump(&mut client, &mut answers, reader, writer, &mut session).await.map(drop),
                    (_, _) => Err(io::ErrorKind::BrokenPipe.into()),
                })
                .await?;
                return Ok(status.code().and_then(|code| u8::try_from(code).ok()).map_or(ExitCode::FAILURE, ExitCode::from));
            }
        }
    }
}
