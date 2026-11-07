#![feature(duration_millis_float, error_reporter, exit_status_error)]

use core::{error, fmt, num::NonZero, pin::pin, time::Duration};
use std::{ffi::OsString, io, path::Path, process::ExitStatus};

#[cfg(target_os = "macos")]
use libproc::{
    bsd_info::BSDInfo,
    proc_pid::pidinfo,
    processes::{ProcFilter, pids_by_type},
};
use nonempty_collections::NEVec;
#[cfg(windows)]
use process_wrap::tokio::{CommandWrap, JobObject, KillOnDrop};
#[cfg(unix)]
use rustix::{
    io::Errno,
    process::{Pid, Signal, WaitId, WaitIdOptions, kill_process_group, waitid},
};
use thiserror::Error;
#[cfg(unix)]
use tokio::signal::unix::{self, SignalKind, signal};
use tokio::{
    process::{ChildStderr, ChildStdout, Command},
    time::{Instant, error::Elapsed, sleep_until, timeout_at},
};
use tokio_util::sync::CancellationToken;

// --- [CONSTANTS] -----------------------------------------------------------------------

const RELEASE: Duration = Duration::from_secs(5);

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
    #[error("cannot start process: {0}")]
    Start(#[source] io::Error),
    #[error("cannot observe process exit: {0}")]
    Wait(#[source] io::Error),
    #[error("cannot capture process output: {0}")]
    Capture(#[source] io::Error),
    #[error("cannot record process evidence: {0}")]
    Observe(#[source] io::Error),
    #[error("cannot release the process group or job: {0}")]
    Release(#[source] io::Error),
    #[error("terminal process evidence exceeded its deadline")]
    ObservationDeadline(#[source] Elapsed),
    #[error("the process did not reap before the release deadline")]
    ReapDeadline(#[source] Elapsed),
    #[error("process output did not drain before the release deadline")]
    DrainDeadline(#[source] Elapsed),
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
        f.write_str(" failed")?;
        self.failures.iter().try_for_each(|failure| write!(f, ": {failure}"))
    }
}

impl error::Error for ProcessError {
    fn source(&self) -> Option<&(dyn error::Error + 'static)> { Some(self.failures.first()) }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[cfg(unix)]
struct Group(Option<Pid>);

#[cfg(unix)]
impl Drop for Group {
    fn drop(&mut self) {
        if let Some(pid) = self.0 {
            drop(terminate(pid));
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [UNIX]
#[cfg(unix)]
fn terminate(pid: Pid) -> io::Result<()> {
    match kill_process_group(pid, Signal::KILL) {
        Ok(()) | Err(Errno::SRCH) => Ok(()),
        #[cfg(target_os = "macos")]
        Err(Errno::PERM) if no_live_group_members(pid.as_raw_nonzero().get().cast_unsigned()) => Ok(()),
        Err(error) => Err(error.into()),
    }
}

#[cfg(target_os = "macos")]
fn no_live_group_members(pgrpid: u32) -> bool {
    const INCLUDE_ZOMBIES: u64 = 1;
    pids_by_type(ProcFilter::ByProgramGroup { pgrpid }).is_ok_and(|members| {
        members
            .into_iter()
            .all(|pid| i32::try_from(pid).ok().and_then(|pid| pidinfo::<BSDInfo>(pid, INCLUDE_ZOMBIES).ok()).is_some_and(|info| info.pbi_pgid != pgrpid || info.pbi_status == libc::SZOMB))
    })
}

#[cfg(unix)]
async fn exited(changes: &mut unix::Signal, pid: Pid) -> io::Result<()> {
    let options = WaitIdOptions::EXITED | WaitIdOptions::NOHANG | WaitIdOptions::NOWAIT;
    loop {
        match waitid(WaitId::Pid(pid), options) {
            Ok(Some(_)) => return Ok(()),
            Ok(None) =>
                if changes.recv().await.is_none() {
                    return Err(io::Error::other("SIGCHLD observation ended"));
                },
            Err(Errno::INTR) => (),
            Err(error) => return Err(error.into()),
        }
    }
}

// --- [SUPERVISION]
pub async fn run(
    command: Command,
    deadline: Instant,
    stopping: &CancellationToken,
    capture: impl AsyncFnOnce(NonZero<u32>, Option<ChildStdout>, Option<ChildStderr>) -> io::Result<()>,
    mut observe: impl AsyncFnMut(NonZero<u32>, Result<ExitStatus, Stop>) -> io::Result<()>,
) -> Result<(NonZero<u32>, ExitStatus), ProcessError> {
    let started = Instant::now();
    let program = command.as_std().get_program().to_owned();
    let failed = |failure| ProcessError { program: program.clone(), process: None, failures: NEVec::new(failure) };
    if started >= deadline {
        return Err(failed(Failure::Stopped(Stop::Deadline)));
    }
    if stopping.is_cancelled() {
        return Err(failed(Failure::Stopped(Stop::Cancelled)));
    }

    #[cfg(unix)]
    let mut changes = signal(SignalKind::child()).map_err(|error| failed(Failure::Wait(error)))?;
    #[cfg(unix)]
    let mut command = command;
    #[cfg(unix)]
    #[expect(clippy::disallowed_methods, reason = "Group signals before the leader PID can be reused and on drop")]
    let child = command.process_group(0).kill_on_drop(true).spawn();
    #[cfg(windows)]
    let child = CommandWrap::from(command).wrap(KillOnDrop).wrap(JobObject).spawn();
    let mut child = child.map_err(|error| failed(Failure::Start(error)))?;
    let pid = child.id().and_then(NonZero::new).ok_or_else(|| failed(Failure::Start(io::Error::other("spawned process has no PID"))))?;
    #[cfg(unix)]
    let group_id = i32::try_from(pid.get()).ok().and_then(Pid::from_raw).ok_or_else(|| failed(Failure::Start(io::Error::other("process ID does not fit pid_t"))))?;
    #[cfg(unix)]
    let mut group = Group(Some(group_id));
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
            unix => exited(&mut changes, group_id),
            windows => async { child.inner_mut().wait().await.map(drop) },
        };
        let mut ending = pin!(async {
            tokio::select! {
                biased;
                status = native => status.map_err(Failure::Wait),
                () = sleep_until(deadline) => Err(Failure::Stopped(Stop::Deadline)),
                () = stopping.cancelled() => Err(Failure::Stopped(Stop::Cancelled)),
            }
        });
        tokio::select! {
            biased;
            outcome = &mut ending => outcome,
            result = &mut capture => {
                captured = true;
                match result {
                    Ok(()) => ending.await,
                    Err(error) => Err(Failure::Capture(error)),
                }
            }
        }
    };

    let release = Instant::now().checked_add(RELEASE).ok_or_else(|| ProcessError {
        program: program.clone(),
        process: Some((pid, None)),
        failures: NEVec::new(Failure::Release(io::Error::other("release deadline exceeds the monotonic clock range"))),
    })?;
    #[cfg(unix)]
    let terminated = {
        group.0 = None;
        terminate(group_id)
    };
    #[cfg(windows)]
    let terminated = child.start_kill();
    let stopping = match &outcome {
        Err(Failure::Stopped(reason)) => Some(*reason),
        _ => None,
    };
    let captured = async {
        if captured {
            return Ok(());
        }
        timeout_at(release, capture).await.map_err(Failure::DrainDeadline)?.map_err(Failure::Capture)
    };
    let released = async {
        let stopped = async {
            match stopping {
                Some(reason) => timeout_at(release, observe(pid, Err(reason))).await.map_err(Failure::ObservationDeadline)?.map_err(Failure::Observe),
                None => Ok(()),
            }
        };
        let wait = cfg_select! {
            unix => child.wait(),
            windows => child.inner_mut().wait(),
        };
        let (stopped, reaped) = tokio::join!(biased; stopped, timeout_at(release, wait));
        let reaped = reaped.map_err(Failure::ReapDeadline).and_then(|result| result.map_err(Failure::Wait));
        let observed = async {
            match reaped {
                Ok(status) => timeout_at(release, observe(pid, Ok(status))).await.map_err(Failure::ObservationDeadline)?.map_err(Failure::Observe),
                Err(_) => Ok(()),
            }
        };
        #[cfg(windows)]
        let (observed, job) = tokio::join!(biased; observed, timeout_at(release, child.wait()));
        #[cfg(windows)]
        let job = job.map_err(Failure::ReapDeadline).and_then(|result| result.map(drop).map_err(Failure::Wait));
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
    let failures = outcome.err().into_iter().chain(terminated.err().map(Failure::Release)).chain(observed.into_iter().filter_map(Result::err)).chain(captured.err()).collect();
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
pub mod macos;
pub mod report;
