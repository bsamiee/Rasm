#![feature(error_reporter)]
#![cfg_attr(unix, feature(exit_status_error))]

use core::{fmt, num::NonZero, pin::pin, time::Duration};
#[cfg(unix)]
use std::process::{Command, Stdio};
use std::{error::Report, ffi::OsString, io, path::Path, process::ExitStatus};

use itertools::Itertools as _;
use nonempty_collections::NEVec;
#[cfg(windows)]
use process_wrap::tokio::JobObject;
#[cfg(unix)]
use process_wrap::tokio::{ChildWrapper, CommandWrapper, ProcessGroup};
use process_wrap::tokio::{CommandWrap, KillOnDrop};
#[cfg(unix)]
use rustix::{
    io::Errno,
    process::{Pid, getpgid},
};
use thiserror::Error;
use tokio::{
    process::{ChildStderr, ChildStdout},
    time::{Instant, sleep_until, timeout},
};
use tokio_util::sync::CancellationToken;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Operation {
    #[display("start the process")]
    Start,
    #[display("observe the process exit")]
    Wait,
    #[display("capture process output")]
    Capture,
    #[display("release the process group or job")]
    Release,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
pub enum Failure {
    #[error("process cancelled")]
    Cancelled,
    #[error("process deadline expired")]
    Deadline,
    #[error("cannot {operation}")]
    Io { operation: Operation, source: io::Error },
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
    failures.iter().format_with("; ", |failure, emit| emit(&Report::new(failure)))
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
        let leader = child.id().and_then(|pid| Pid::from_raw(pid.cast_signed())).ok_or_else(|| io::Error::other("spawned process has no PID"))?;
        let (watch, parent) = io::pipe()?;
        let mut watcher = Command::new("sh");
        watcher.args(["-c", WATCH]).stdin(watch).stdout(Stdio::null()).stderr(Stdio::null());
        process_wrap::std::CommandWrap::from(watcher)
            .wrap(process_wrap::std::ProcessGroup::attach_to(getpgid(Some(leader))?.as_raw_pid().cast_unsigned()))
            .spawn()?
            .inner_mut()
            .wait()?
            .exit_ok()
            .map_err(io::Error::other)?;
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

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn owned(command: impl Into<CommandWrap>) -> CommandWrap {
    let mut command = command.into();
    cfg_select! {
        unix => command.wrap(KillOnDrop).wrap(ProcessGroup::leader()).wrap(KillOnParentDeath),
        windows => command.wrap(KillOnDrop).wrap(JobObject),
    };
    command
}

pub async fn run(
    command: impl Into<CommandWrap>,
    deadline: Instant,
    stopping: &CancellationToken,
    capture: impl AsyncFnOnce(NonZero<u32>, Option<ChildStdout>, Option<ChildStderr>) -> io::Result<()>,
) -> Result<ExitStatus, ProcessError> {
    const RELEASE: Duration = Duration::from_secs(5);
    let failed = |operation| move |source| Failure::Io { operation, source };
    let mut command = owned(command);
    let program = command.command().as_std().get_program().to_owned();
    let refused = |failure| ProcessError { program: program.clone(), process: None, failures: NEVec::new(failure) };
    if stopping.is_cancelled() {
        return Err(refused(Failure::Cancelled));
    }
    if Instant::now() >= deadline {
        return Err(refused(Failure::Deadline));
    }

    let mut child = command.spawn().map_err(|source| refused(failed(Operation::Start)(source)))?;
    let pid = child.id().and_then(NonZero::new).ok_or_else(|| refused(failed(Operation::Start)(io::Error::other("spawned process has no PID"))))?;
    drop(child.stdin().take());
    let mut capture = pin!(capture(pid, child.stdout().take(), child.stderr().take()));
    let (ended, captured) = {
        let exited = cfg_select! {
            unix => child.wait(),
            windows => child.inner_mut().wait(),
        };
        let mut ending = pin!(async {
            tokio::select! {
                biased;
                status = exited => status.map(drop).map_err(failed(Operation::Wait)),
                () = sleep_until(deadline) => Err(Failure::Deadline),
                () = stopping.cancelled() => Err(Failure::Cancelled),
            }
        });
        tokio::select! {
            biased;
            ended = &mut ending => (ended, None),
            captured = &mut capture => match captured {
                Ok(()) => (ending.await, Some(Ok(()))),
                Err(error) => (Ok(()), Some(Err(error))),
            },
        }
    };

    let released = cfg_select! {
        unix => child.start_kill().or_else(|error| (Errno::from_io_error(&error) == Some(Errno::SRCH)).ok_or(error)),
        windows => child.start_kill(),
    };
    let (reaped, captured) = tokio::join!(async { timeout(RELEASE, child.wait()).await? }, async {
        match captured {
            Some(captured) => captured,
            None => timeout(RELEASE, capture).await?,
        }
    });
    let later = [ended.err(), released.err().map(failed(Operation::Release)), captured.err().map(failed(Operation::Capture))].into_iter().flatten().collect::<Vec<_>>();
    let error = |status, failures| ProcessError { program, process: Some((pid, status)), failures };
    match reaped {
        Ok(status) => NEVec::try_from_vec(later).map_or(Ok(status), |failures| Err(error(Some(status), failures))),
        Err(source) => Err(error(None, NEVec::from((failed(Operation::Wait)(source), later)))),
    }
}
