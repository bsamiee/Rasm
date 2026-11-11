#![feature(error_reporter)]

use core::error::Error;
use core::num::NonZero;
use core::pin::{Pin, pin};
use core::time::Duration;
use core::{fmt, iter};
use std::collections::VecDeque;
use std::error::Report;
use std::ffi::OsString;
use std::io;
#[cfg(unix)]
use std::os::fd::{AsFd, OwnedFd};
#[cfg(unix)]
use std::os::unix::fs::FileTypeExt as _;
#[cfg(unix)]
use std::os::unix::net::UnixStream as Socket;
use std::path::Path;
use std::process::{ExitStatus, Stdio};

use itertools::Itertools as _;
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use process_wrap::tokio::{ChildWrapper, CommandWrap, KillOnDrop};
#[cfg(unix)]
use process_wrap::tokio::{CommandWrapper, ProcessGroup, ProcessSession};
#[cfg(windows)]
use process_wrap::tokio::{CreationFlags, JobObject};
#[cfg(unix)]
use rustix::{io::Errno, process::Signal};
#[cfg(unix)]
use socket2::SockRef;
use thiserror::Error;
use tokio::io::{AsyncBufReadExt as _, AsyncRead, AsyncReadExt as _, AsyncWrite, BufReader};
#[cfg(unix)]
use tokio::net::UnixStream;
#[cfg(unix)]
use tokio::net::unix::pipe;
use tokio::process::ChildStderr;
use tokio::time::{Instant, sleep_until, timeout};
#[cfg(unix)]
use tokio_util::either::Either;
use tokio_util::sync::CancellationToken;
#[cfg(windows)]
use windows::Win32::System::Threading::{CREATE_NEW_PROCESS_GROUP, DETACHED_PROCESS};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum ProcessOperation {
    #[display("start the process")]
    Start,
    #[display("observe the process exit")]
    Wait,
    #[display("attach process streams")]
    Attach,
    #[display("release the process group or job")]
    Release,
}

pub type Input = Pin<Box<dyn AsyncWrite + Send>>;

pub type Output = Pin<Box<dyn AsyncRead + Send>>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(derive_more::Debug)]
pub struct Streams {
    pub id: NonZero<u32>,
    #[debug(skip)]
    pub stdin: Option<Input>,
    #[debug(skip)]
    pub stdout: Option<Output>,
    pub stderr: Option<ChildStderr>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
pub enum Failure {
    #[error("process cancelled")]
    Cancelled,
    #[error("process deadline expired")]
    Deadline,
    #[error("cannot {operation}")]
    Io { operation: ProcessOperation, source: io::Error },
}

#[derive(Debug, Error)]
#[error(
    "{} {}failed: {}",
    Path::new(program).display(),
    fmt::from_fn(|formatter| match process {
        Some((pid, Some(status))) => write!(formatter, "({pid}, {status}) "),
        Some((pid, None)) => write!(formatter, "({pid}) "),
        None => Ok(()),
    }),
    failures.iter().format_with("; ", |failure, emit| emit(&iter::successors(Some(failure as &dyn Error), |&cause| cause.source()).format(": ")))
)]
pub struct ProcessError {
    pub program: OsString,
    pub process: Option<(NonZero<u32>, Option<ExitStatus>)>,
    pub failures: NEVec<Failure>,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[cfg(unix)]
#[derive(Debug)]
struct KillOnParentDeath;

#[cfg(unix)]
impl CommandWrapper for KillOnParentDeath {
    fn wrap_child(&mut self, child: Box<dyn ChildWrapper>, _core: &CommandWrap) -> io::Result<Box<dyn ChildWrapper>> {
        const WATCH: &str = "exec 3<&0; { trap '' HUP INT TERM; read -r line; kill -s KILL 0; } <&3 3<&- &";
        let leader = child.id().ok_or_else(|| io::Error::other("spawned process has no PID"))?;
        let (watch, parent) = io::pipe()?;
        let mut watcher = tokio::process::Command::new("sh");
        watcher.args(["-c", WATCH]).stdin(watch).stdout(Stdio::null()).stderr(Stdio::null());
        drop(CommandWrap::from(watcher).wrap(ProcessGroup::attach_to(leader)).spawn()?);
        Ok(Box::new(KillOnParentDeathChild { inner: child, _parent: parent }))
    }
}

#[cfg(unix)]
#[derive(Debug)]
struct KillOnParentDeathChild {
    inner: Box<dyn ChildWrapper>,
    _parent: io::PipeWriter,
}

#[cfg(unix)]
impl ChildWrapper for KillOnParentDeathChild {
    fn inner(&self) -> &dyn ChildWrapper { self.inner.as_ref() }

    fn inner_mut(&mut self) -> &mut dyn ChildWrapper { self.inner.as_mut() }

    fn into_inner(self: Box<Self>) -> Box<dyn ChildWrapper> { self.inner }
}

#[derive(Debug)]
struct Released(Box<dyn ChildWrapper>);

impl Drop for Released {
    fn drop(&mut self) {
        if let Err(error) = self.0.start_kill() {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "process group already released");
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn grouped(command: impl Into<CommandWrap>) -> CommandWrap {
    let mut command = command.into();
    cfg_select! {
        unix => command.wrap(ProcessGroup::leader()).wrap(KillOnParentDeath),
        windows => command.wrap(KillOnDrop).wrap(JobObject),
    };
    command
}

#[must_use]
pub fn detached(command: impl Into<CommandWrap>) -> CommandWrap {
    let mut command = command.into();
    cfg_select! {
        unix => command.wrap(ProcessSession),
        windows => command.wrap(CreationFlags(DETACHED_PROCESS | CREATE_NEW_PROCESS_GROUP)),
    };
    command
}

#[cfg(unix)]
fn sized(socket: &Socket, buffer: usize) -> io::Result<()> {
    let options = SockRef::from(socket);
    options.set_send_buffer_size(buffer)?;
    options.set_recv_buffer_size(buffer)
}

pub fn standard(buffer: Option<usize>) -> io::Result<(impl AsyncRead + Send + Unpin + 'static, impl AsyncWrite + Send + Unpin + 'static)> {
    cfg_select! {
        unix => {{
            fn attached<P, B>(descriptor: impl AsFd, buffer: Option<usize>, piped: impl FnOnce(OwnedFd) -> io::Result<P>, blocking: B) -> io::Result<Either<UnixStream, Either<P, B>>> {
                let file = std::fs::File::from(descriptor.as_fd().try_clone_to_owned()?);
                let kind = file.metadata()?.file_type();
                match (kind.is_socket(), kind.is_fifo()) {
                    (true, _) => {
                        let socket = Socket::from(OwnedFd::from(file));
                        if let Some(buffer) = buffer {
                            sized(&socket, buffer)?;
                        }
                        socket.set_nonblocking(true)?;
                        UnixStream::from_std(socket).map(Either::Left)
                    }
                    (false, true) => piped(OwnedFd::from(file)).map(|pipe| Either::Right(Either::Left(pipe))),
                    (false, false) => Ok(Either::Right(Either::Right(blocking))),
                }
            }
            Ok((attached(io::stdin(), buffer, pipe::Receiver::from_owned_fd, tokio::io::stdin())?, attached(io::stdout(), buffer, pipe::Sender::from_owned_fd, tokio::io::stdout())?))
        }}
        _ => {{
            let _: Option<usize> = buffer;
            Ok((tokio::io::stdin(), tokio::io::stdout()))
        }}
    }
}

pub async fn tail<R>(reader: R, kept: usize, limit: u64, mut each: impl FnMut(&str)) -> VecDeque<String>
where R: AsyncRead + Unpin {
    let (mut reader, mut line, mut tail) = (BufReader::new(reader), Vec::new(), VecDeque::with_capacity(kept));
    loop {
        line.clear();
        match (&mut reader).take(limit).read_until(b'\n', &mut line).await {
            Ok(0) => break tail,
            Ok(_) => {
                let text = String::from_utf8_lossy(&line);
                each(text.trim_end());
                tail.push_back(text.trim_end().to_owned());
                tail.retain_back(kept);
            }
            Err(error) => {
                tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "process output unreadable");
                break tail;
            }
        }
    }
}

pub async fn run(
    command: impl Into<CommandWrap>,
    streams: Option<usize>,
    deadline: Option<Instant>,
    grace: Duration,
    stopping: &CancellationToken,
    attach: impl AsyncFnOnce(Streams) -> io::Result<()>,
) -> Result<ExitStatus, ProcessError> {
    let failed = |operation| move |source| Failure::Io { operation, source };
    let mut command = grouped(command);
    command.wrap(KillOnDrop);
    let program = command.command().as_std().get_program().to_owned();
    let refused = |failure| ProcessError { program: program.clone(), process: None, failures: NEVec::new(failure) };
    if stopping.is_cancelled() {
        return Err(refused(Failure::Cancelled));
    }
    if deadline.is_some_and(|deadline| Instant::now() >= deadline) {
        return Err(refused(Failure::Deadline));
    }

    #[cfg(unix)]
    let linked = streams.map(|buffer| {
        let (near, far) = Socket::pair()?;
        sized(&near, buffer)?;
        sized(&far, buffer)?;
        near.set_nonblocking(true)?;
        command.command_mut().stdin(OwnedFd::from(far.try_clone()?)).stdout(OwnedFd::from(far));
        UnixStream::from_std(near).map(UnixStream::into_split)
    });
    #[cfg(unix)]
    let linked = linked.transpose().map_err(|source| refused(failed(ProcessOperation::Start)(source)))?;
    #[cfg(windows)]
    if streams.is_some() {
        command.command_mut().stdin(Stdio::piped()).stdout(Stdio::piped());
    }
    let mut child = Released(command.spawn().map_err(|source| refused(failed(ProcessOperation::Start)(source)))?);
    drop(command);
    let pid = child.0.id().and_then(NonZero::new).ok_or_else(|| refused(failed(ProcessOperation::Start)(io::Error::other("spawned process has no PID"))))?;
    let (stdout, stdin) = (child.0.stdout().take().map(|pipe| Box::pin(pipe) as Output), child.0.stdin().take().map(|pipe| Box::pin(pipe) as Input));
    #[cfg(unix)]
    let (stdout, stdin) = linked.map_or((stdout, stdin), |(reader, writer)| (Some(Box::pin(reader) as Output), Some(Box::pin(writer) as Input)));
    let mut attach = pin!(attach(Streams { id: pid, stdin, stdout, stderr: child.0.stderr().take() }));
    let (ended, attached) = {
        let exited = cfg_select! {
            unix => child.0.wait(),
            windows => child.0.inner_mut().wait(),
        };
        let mut ending = pin!(async {
            tokio::select! {
                biased;
                status = exited => status.map(drop).map_err(failed(ProcessOperation::Wait)),
                Some(()) = async {
                    sleep_until(deadline?).await;
                    Some(())
                } => Err(Failure::Deadline),
                () = stopping.cancelled() => Err(Failure::Cancelled),
            }
        });
        tokio::select! {
            biased;
            ended = &mut ending => (ended, None),
            attached = &mut attach => match attached {
                Ok(()) => (ending.await, Some(Ok(()))),
                Err(error) => (Ok(()), Some(Err(error))),
            },
        }
    };

    let exited = timeout(grace, child.0.wait()).await;
    #[cfg(unix)]
    let exited = if let Err(tokio::time::error::Elapsed { .. }) = &exited {
        if let Err(error) = child.0.signal(Signal::TERM.as_raw()) {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "process group ended before its terminate signal");
        }
        timeout(grace, child.0.wait()).await
    } else {
        exited
    };
    let released = match child.0.start_kill() {
        Ok(()) => Ok(()),
        #[cfg(unix)]
        Err(error) if Errno::from_io_error(&error) == Some(Errno::SRCH) => Ok(()),
        Err(error) => Err(error),
    };
    let (reaped, attached) = tokio::join!(
        async {
            match exited {
                Ok(status) => status,
                Err(tokio::time::error::Elapsed { .. }) => timeout(grace, child.0.wait()).await?,
            }
        },
        async {
            match attached {
                Some(attached) => attached,
                None => timeout(grace, attach).await?,
            }
        }
    );
    let later = [ended.err(), released.err().map(failed(ProcessOperation::Release)), attached.err().map(failed(ProcessOperation::Attach))].into_iter().flatten().collect::<Vec<_>>();
    let error = |status, failures| ProcessError { program, process: Some((pid, status)), failures };
    match reaped {
        Ok(status) => NEVec::try_from_vec(later).map_or(Ok(status), |failures| Err(error(Some(status), failures))),
        Err(source) => Err(error(None, NEVec::from((failed(ProcessOperation::Wait)(source), later)))),
    }
}
