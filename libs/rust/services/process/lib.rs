use core::error::Error;
use core::num::NonZero;
use core::pin::pin;
use core::time::Duration;
use core::{fmt, iter};
use std::ffi::OsString;
use std::io;
use std::path::Path;
use std::process::ExitStatus;
#[cfg(unix)]
use std::process::Stdio;

use itertools::Itertools as _;
use nonempty_collections::NEVec;
#[cfg(windows)]
use process_wrap::tokio::{CreationFlags, JobObject};
use process_wrap::tokio::{ChildWrapper, CommandWrap, KillOnDrop};
#[cfg(unix)]
use process_wrap::tokio::{CommandWrapper, ProcessGroup, ProcessSession};
#[cfg(unix)]
use rustix::{io::Errno, process::Signal};
use thiserror::Error;
use tokio::process::{ChildStderr, ChildStdin, ChildStdout};
use tokio::time::{Instant, sleep_until, timeout};
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
    #[display("attach process pipes")]
    Attach,
    #[display("release the process group or job")]
    Release,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Pipes {
    pub id: NonZero<u32>,
    pub stdin: Option<ChildStdin>,
    pub stdout: Option<ChildStdout>,
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
            tracing::debug!(%error, "process group already released");
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

pub async fn run(
    command: impl Into<CommandWrap>,
    deadline: Option<Instant>,
    grace: Duration,
    stopping: &CancellationToken,
    attach: impl AsyncFnOnce(Pipes) -> io::Result<()>,
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

    let mut child = Released(command.spawn().map_err(|source| refused(failed(ProcessOperation::Start)(source)))?);
    let pid = child.0.id().and_then(NonZero::new).ok_or_else(|| refused(failed(ProcessOperation::Start)(io::Error::other("spawned process has no PID"))))?;
    let pipes = Pipes { id: pid, stdin: child.0.stdin().take(), stdout: child.0.stdout().take(), stderr: child.0.stderr().take() };
    let mut attach = pin!(attach(pipes));
    let (ended, attached) = {
        let exited = cfg_select! {
            unix => child.0.wait(),
            windows => child.0.inner_mut().wait(),
        };
        let mut ending = pin!(async {
            tokio::select! {
                biased;
                status = exited => status.map(drop).map_err(failed(ProcessOperation::Wait)),
                Some(()) = async { Some(sleep_until(deadline?).await) } => Err(Failure::Deadline),
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
    let exited = match exited {
        Ok(status) => Ok(status),
        Err(tokio::time::error::Elapsed { .. }) => {
            if let Err(error) = child.0.signal(Signal::TERM.as_raw()) {
                tracing::debug!(%error, "process group ended before its terminate signal");
            }
            timeout(grace, child.0.wait()).await
        }
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
