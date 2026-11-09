#![feature(duration_millis_float, error_reporter, exit_status_error)]

#[cfg(unix)]
use core::mem::DropGuard;
use core::{error, fmt, iter, num::NonZero, pin::pin, time::Duration};
use std::{error::Report, ffi::OsString, io, path::Path, process::ExitStatus};

#[cfg(target_os = "macos")]
use libproc::{
    bsd_info::BSDInfo,
    proc_pid::pidinfo,
    processes::{ProcFilter, pids_by_type},
};
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
#[cfg(windows)]
use process_wrap::tokio::{CommandWrap, JobObject, KillOnDrop};
#[cfg(unix)]
use rustix::{
    io::{Errno, retry_on_intr},
    process::{Pid, Signal, WaitId, WaitIdOptions, kill_process_group, waitid},
};
use thiserror::Error;
#[cfg(unix)]
use tokio::task;
use tokio::{
    process::{ChildStderr, ChildStdout, Command},
    time::{Instant, error::Elapsed, sleep_until, timeout_at},
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
    #[display("record process evidence")]
    Observe,
    #[display("release the process group or job")]
    Release,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Error)]
pub enum Stop {
    #[error("process canceled")]
    Cancelled,
    #[error("process deadline expired")]
    Deadline,
}

#[derive(Debug, Error)]
pub enum Failure {
    #[error("cannot {operation}")]
    Io {
        operation: Operation,
        source: io::Error,
    },
    #[error("cannot {operation} before the release deadline")]
    Expired {
        operation: Operation,
        source: Elapsed,
    },
    #[error(transparent)]
    Stopped(Stop),
}

#[derive(Debug)]
pub struct ProcessError {
    pub program: OsString,
    pub process: Option<(NonZero<u32>, Option<ExitStatus>)>,
    pub failures: NEVec<Failure>,
}

impl fmt::Display for ProcessError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", Path::new(&self.program).display())?;
        if let Some((pid, status)) = self.process {
            write!(f, " ({pid})")?;
            if let Some(status) = status {
                write!(f, ", {status}")?;
            }
        }
        let (first, rest) = self.failures.split_first();
        write!(f, " failed: {first}")?;
        rest.iter().try_for_each(|failure| write!(f, "; {}", Report::new(failure)))
    }
}

impl error::Error for ProcessError {
    fn source(&self) -> Option<&(dyn error::Error + 'static)> { Some(self.failures.first()) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn run(
    command: Command,
    deadline: Instant,
    stopping: &CancellationToken,
    capture: impl AsyncFnOnce(NonZero<u32>, Option<ChildStdout>, Option<ChildStderr>) -> io::Result<()>,
    mut observe: impl AsyncFnMut(NonZero<u32>, Result<ExitStatus, Stop>) -> io::Result<()>,
) -> Result<(NonZero<u32>, ExitStatus), ProcessError> {
    const RELEASE: Duration = Duration::from_secs(5);
    let io_failure = |operation| move |source| Failure::Io { operation, source };
    let expired = |operation| move |source| Failure::Expired { operation, source };
    let started = Instant::now();
    let program = command.as_std().get_program().to_owned();
    let failed = |failure| ProcessError { program: program.clone(), process: None, failures: NEVec::new(failure) };
    if started >= deadline {
        return Err(failed(Failure::Stopped(Stop::Deadline)));
    }
    if stopping.is_cancelled() {
        return Err(failed(Failure::Stopped(Stop::Cancelled)));
    }

    #[cfg_attr(unix, expect(clippy::disallowed_methods, reason = "Group signals before the leader PID can be reused and on drop"))]
    let child = cfg_select! {
        unix => {{
            let mut command = command;
            command.process_group(0).kill_on_drop(true).spawn()
        }}
        windows => CommandWrap::from(command).wrap(KillOnDrop).wrap(JobObject).spawn(),
    };
    let mut child = child.map_err(|source| failed(io_failure(Operation::Start)(source)))?;
    let pid = child.id().and_then(NonZero::new).ok_or_else(|| failed(io_failure(Operation::Start)(io::Error::other("spawned process has no PID"))))?;
    #[cfg(unix)]
    let group_id = Pid::from_raw(pid.get().cast_signed()).ok_or_else(|| failed(io_failure(Operation::Start)(io::Error::other("process ID does not fit pid_t"))))?;
    #[cfg(unix)]
    let terminate = |group: Pid| {
        #[cfg(target_os = "macos")]
        let vacated = |pgrpid: u32| {
            const INCLUDE_ZOMBIES: u64 = 1;
            let live = pids_by_type(ProcFilter::ByProgramGroup { pgrpid })?.into_iter().any(|member| match pidinfo::<BSDInfo>(member.cast_signed(), INCLUDE_ZOMBIES) {
                Ok(info) => info.pbi_pgid == pgrpid && info.pbi_status != libc::SZOMB,
                Err(reason) => {
                    tracing::debug!(member, reason, "process group member unreadable");
                    true
                }
            });
            (!live).ok_or(io::Error::from(Errno::PERM))
        };
        match kill_process_group(group, Signal::KILL) {
            Ok(()) | Err(Errno::SRCH) => Ok(()),
            #[cfg(target_os = "macos")]
            Err(Errno::PERM) => vacated(group.as_raw_nonzero().get().cast_unsigned()),
            Err(error) => Err(io::Error::from(error)),
        }
    };
    #[cfg(unix)]
    let group = DropGuard::new(group_id, move |group| {
        if let Err(error) = terminate(group) {
            tracing::warn!(%pid, { EXCEPTION_MESSAGE } = %Report::new(error), "process group release failed");
        }
    });
    let (stdin, stdout, stderr) = cfg_select! {
        unix => (child.stdin.take(), child.stdout.take(), child.stderr.take()),
        windows => (child.stdin().take(), child.stdout().take(), child.stderr().take()),
    };
    drop(stdin);
    tracing::debug!(?program, %pid, "process started");

    let mut capture = pin!(capture(pid, stdout, stderr));
    let mut captured = false;
    let outcome = {
        let native = cfg_select! {
            unix =>
                async move {
                    task::spawn_blocking(move || retry_on_intr(|| waitid(WaitId::Pid(group_id), WaitIdOptions::EXITED | WaitIdOptions::NOWAIT)).map(drop))
                        .await
                        .map_err(io::Error::other)?
                        .map_err(io::Error::from)
                },
            windows => async { child.inner_mut().wait().await.map(drop) },
        };
        let mut ending = pin!(async {
            tokio::select! {
                biased;
                status = native => status.map(|()| None).map_err(io_failure(Operation::Wait)),
                () = sleep_until(deadline) => Ok(Some(Stop::Deadline)),
                () = stopping.cancelled() => Ok(Some(Stop::Cancelled)),
            }
        });
        tokio::select! {
            biased;
            outcome = &mut ending => outcome,
            result = &mut capture => {
                captured = true;
                match result {
                    Ok(()) => ending.await,
                    Err(source) => Err(io_failure(Operation::Capture)(source)),
                }
            }
        }
    };

    let release = Instant::now().checked_add(RELEASE).ok_or_else(|| ProcessError {
        program: program.clone(),
        process: Some((pid, None)),
        failures: NEVec::new(io_failure(Operation::Release)(io::Error::other("release deadline exceeds the monotonic clock range"))),
    })?;
    let terminated = cfg_select! {
        unix => terminate(DropGuard::dismiss(group)),
        windows => child.start_kill(),
    };
    let (stopped, failure) = outcome.map_or_else(|failure| (None, Some(failure)), |stopped| (stopped, None));
    let captured = async {
        if captured {
            return Ok(());
        }
        timeout_at(release, capture).await.map_err(expired(Operation::Capture))?.map_err(io_failure(Operation::Capture))
    };
    let released = async {
        let stopped = async {
            match stopped {
                Some(reason) => timeout_at(release, observe(pid, Err(reason))).await.map_err(expired(Operation::Observe))?.map_err(io_failure(Operation::Observe)),
                None => Ok(()),
            }
        };
        let wait = cfg_select! {
            unix => child.wait(),
            windows => child.inner_mut().wait(),
        };
        let (stopped, reaped) = tokio::join!(biased; stopped, timeout_at(release, wait));
        let reaped = reaped.map_err(expired(Operation::Wait)).and_then(|result| result.map_err(io_failure(Operation::Wait)));
        let observed = async {
            match reaped {
                Ok(status) => timeout_at(release, observe(pid, Ok(status))).await.map_err(expired(Operation::Observe))?.map_err(io_failure(Operation::Observe)),
                Err(_) => Ok(()),
            }
        };
        #[cfg(windows)]
        let (observed, job) = tokio::join!(biased; observed, timeout_at(release, child.wait()));
        #[cfg(windows)]
        let job = job.map_err(expired(Operation::Wait)).and_then(|result| result.map(drop).map_err(io_failure(Operation::Wait)));
        #[cfg(unix)]
        let observed = observed.await;
        (
            reaped,
            [
                stopped,
                observed,
                #[cfg(windows)]
                job,
            ],
        )
    };
    let ((status, observed), captured) = tokio::join!(biased; released, captured);
    let failures = iter::chain([stopped.map(Failure::Stopped), failure, terminated.err().map(io_failure(Operation::Release))].into_iter().flatten(), observed.into_iter().filter_map(Result::err))
        .chain(captured.err())
        .collect();
    tracing::debug!(%pid, ?status, elapsed = ?started.elapsed(), "process released");
    let failed = |status, failures| ProcessError { program, process: Some((pid, status)), failures };
    match (status, NEVec::try_from_vec(failures)) {
        (Ok(status), None) => Ok((pid, status)),
        (Ok(status), Some(failures)) => Err(failed(Some(status), failures)),
        (Err(error), None) => Err(failed(None, NEVec::new(error))),
        (Err(error), Some(mut failures)) => {
            failures.push(error);
            Err(failed(None, failures))
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[cfg(target_os = "macos")]
mod macos;
mod report;

// --- [EXPORTS] -------------------------------------------------------------------------

#[cfg(target_os = "macos")]
pub use macos::{Launch, launch};
pub use report::Run;
pub use nonempty_collections;
pub use serde;
pub use tokio;
pub use tokio_util;
