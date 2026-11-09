use core::{future::pending, mem::DropGuard, num::NonZero, pin::pin};
use std::{collections::BTreeMap, error::Report, ffi::OsString, io, path::Path, process::Stdio};

use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rustix::{
    io::Errno,
    process::{Pid, Signal, kill_process},
};
use serde::{Deserialize, de::IgnoredAny};
use serde_with::{DisplayFromStr, serde_as};
use thiserror::Error;
use tokio::{
    process::{Child, ChildStdout, Command},
    time::{Instant, sleep_until},
};
use tokio_stream::StreamExt as _;
use tokio_util::{
    codec::{FramedRead, LinesCodec},
    sync::CancellationToken,
};

use crate::Stop;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Launch<'path> {
    pub bundle: &'path Path,
    pub documents: &'path [&'path Path],
    pub arguments: &'path [OsString],
    pub environment: &'path [(&'path str, &'path str)],
}

#[serde_as]
#[derive(Deserialize)]
#[serde(untagged)]
enum Line {
    Failed { fail: serde_json::Value },
    Instances(#[serde_as(as = "BTreeMap<DisplayFromStr, _>")] BTreeMap<NonZero<u32>, State>),
}

#[derive(Deserialize)]
#[serde(rename_all = "lowercase")]
enum State {
    Launched { created: bool },
    Running { status: Responsiveness, active: bool, asking: Vec<IgnoredAny> },
    Exited(Termination),
    Failed(IgnoredAny),
}

#[derive(Deserialize)]
#[serde(rename_all = "lowercase")]
enum Responsiveness {
    Launching,
    Responding,
    Unresponsive,
}

#[derive(Clone, Copy, Deserialize)]
struct Termination {
    status: Option<i32>,
    signal: Option<i32>,
}

impl Termination {
    const fn clean(self) -> bool { matches!((self.status, self.signal), (Some(0), None)) }

    fn error(self, pid: NonZero<u32>) -> io::Error {
        let reason = match (self.status, self.signal) {
            (Some(status), None) => format!("exit code {status}"),
            (None, Some(signal)) => format!("signal {signal}"),
            (Some(_), Some(_)) | (None, None) => String::from("unknown native exit status"),
        };
        io::Error::other(format!("Application {pid} exited with {reason}"))
    }
}

enum Progress {
    Waiting,
    Ready(NonZero<u32>),
    Exited(NonZero<u32>, Termination),
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
#[error("cannot release the application: {release}")]
struct ReleaseFailure {
    #[source]
    operation: io::Error,
    release: io::Error,
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Observation<Observe> {
    frames: FramedRead<ChildStdout, LinesCodec>,
    observe: Observe,
    pid: Option<NonZero<u32>>,
    terminal: Option<Termination>,
}

impl<Observe> Observation<Observe>
where Observe: AsyncFnMut(&str) -> io::Result<()>
{
    async fn next(&mut self) -> io::Result<Option<Progress>> {
        let Some(line) = self.frames.next().await.transpose().map_err(io::Error::other)? else { return Ok(None) };
        let mut decoder = serde_json::Deserializer::from_str(&line);
        let progress = (|| {
            let frame: BTreeMap<String, Line> = serde_path_to_error::deserialize(&mut decoder).map_err(io::Error::other)?;
            decoder.end()?;
            frame.into_values().try_fold(Progress::Waiting, |held, entry| match entry {
                Line::Failed { fail } => Err(io::Error::other(format!("Native launch failed: {fail}"))),
                Line::Instances(states) => states.into_iter().try_fold(held, |held, (pid, state)| {
                    self.advance(pid, state, &line).map(|next| match next {
                        Progress::Waiting => held,
                        Progress::Ready(_) | Progress::Exited(..) => next,
                    })
                }),
            })
        })();
        let recorded = (self.observe)(&line).await;
        progress.and_then(|progress| recorded.map(|()| Some(progress)))
    }

    fn advance(&mut self, pid: NonZero<u32>, state: State, line: &str) -> io::Result<Progress> {
        let owned = self.pid == Some(pid);
        match state {
            State::Launched { created: true } if self.pid.is_none() => {
                self.pid = Some(pid);
                Ok(Progress::Waiting)
            }
            State::Launched { created: false } => Err(io::Error::new(io::ErrorKind::AlreadyExists, format!("Application {pid} is already running"))),
            State::Launched { created: true } => Err(io::Error::other(format!("Unexpected second application acknowledgment for {pid}"))),
            State::Running { status: _, active: _, asking } if !asking.is_empty() => Err(io::Error::other(format!("Application {pid} requested attention: {line}"))),
            State::Running { status: _, active: true, asking: _ } if owned => Err(io::Error::other(format!("Application {pid} became active"))),
            State::Running { status: Responsiveness::Responding, active: false, asking: _ } if owned => Ok(Progress::Ready(pid)),
            State::Exited(termination) if owned => {
                self.terminal = Some(termination);
                Ok(Progress::Exited(pid, termination))
            }
            State::Running { status: Responsiveness::Launching | Responsiveness::Responding | Responsiveness::Unresponsive, active: _, asking: _ } | State::Exited(_) => Ok(Progress::Waiting),
            State::Failed(_) => Err(io::Error::other(format!("Application {pid} failed: {line}"))),
        }
    }

    async fn capture(&mut self) -> io::Result<()> {
        let mut first = Ok(());
        loop {
            match self.next().await {
                Ok(Some(_)) => (),
                Err(error) => first = first.and(Err(error)),
                Ok(None) => return first,
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn launch<T>(
    mut watcher: Command,
    launch: Launch<'_>,
    deadline: Instant,
    stopping: &CancellationToken,
    observe: impl AsyncFnMut(&str) -> io::Result<()>,
    work: impl AsyncFnOnce(NonZero<u32>) -> io::Result<T>,
) -> io::Result<T> {
    const FRAME_BYTES: usize = 8 * 1024 * 1024;
    fn releasing<T>(outcome: io::Result<T>, release: io::Result<()>) -> io::Result<T> {
        match (outcome, release) {
            (Ok(value), Ok(())) => Ok(value),
            (Err(error), Ok(())) | (Ok(_), Err(error)) => Err(error),
            (Err(operation), Err(release)) => Err(io::Error::new(operation.kind(), ReleaseFailure { operation, release })),
        }
    }
    fn end(watcher: &Child) -> io::Result<()> {
        match watcher.id().and_then(|pid| Pid::from_raw(pid.cast_signed())).map(|pid| kill_process(pid, Signal::TERM)) {
            None | Some(Ok(()) | Err(Errno::SRCH)) => Ok(()),
            Some(Err(error)) => Err(io::Error::from(error)),
        }
    }
    let cancelled = || io::Error::new(io::ErrorKind::Interrupted, Stop::Cancelled);
    let expired = || io::Error::new(io::ErrorKind::TimedOut, Stop::Deadline);
    let check = || {
        if stopping.is_cancelled() {
            Err(cancelled())
        } else if Instant::now() >= deadline {
            Err(expired())
        } else {
            Ok(())
        }
    };
    let bundle = tokio::select! {
        biased;
        () = stopping.cancelled() => return Err(cancelled()),
        () = sleep_until(deadline) => return Err(expired()),
        path = fs_err::tokio::canonicalize(launch.bundle) => path?,
    };
    watcher.args(["launch", "--wait"]).arg(&bundle).stdin(Stdio::null()).stdout(Stdio::piped());
    for document in launch.documents {
        watcher.arg("--document").arg(document);
    }
    for argument in launch.arguments {
        watcher.arg("--arguments").arg(argument);
    }
    for (key, value) in launch.environment {
        watcher.arg("--environment").arg(format!("{key}={value}"));
    }

    check()?;
    #[expect(clippy::disallowed_methods, reason = "watch-apps leaves the owner's group so a group kill reaches the owner alone and watch-apps quits the application it created")]
    let child = watcher.process_group(0).spawn()?;
    let mut watcher = DropGuard::new(child, |watcher| {
        if let Err(error) = end(&watcher) {
            tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "native launch helper release failed");
        }
    });
    let watcher_pid = watcher.id().ok_or_else(|| io::Error::other("Native launch helper has no PID"))?;
    let output = watcher.stdout.take().ok_or_else(|| io::Error::other("Native launch helper has no event pipe"))?;
    let mut native = Observation { frames: FramedRead::new(output, LinesCodec::new_with_max_length(FRAME_BYTES)), observe, pid: None, terminal: None };
    let mut work = Some(work);
    let mut activity = pin!(None);
    let mut stop = pin!(stopping.cancelled());
    let mut limit = pin!(sleep_until(deadline));
    let (outcome, recorded) = loop {
        let mut frame = pin!(native.next());
        let (outcome, in_flight) = tokio::select! {
            biased;
            received = frame.as_mut() => match received {
                Ok(Some(Progress::Ready(pid))) if let Some(begin) = work.take() => match check() {
                    Ok(()) => {
                        tracing::debug!(%pid, "application ready");
                        activity.set(Some(begin(pid)));
                        continue;
                    }
                    Err(error) => (Err(error), false),
                },
                Ok(Some(Progress::Ready(_) | Progress::Waiting)) => continue,
                Ok(Some(Progress::Exited(pid, exit))) => break (Err(exit.error(pid)), Ok(())),
                Ok(None) => (Err(io::Error::new(io::ErrorKind::UnexpectedEof, "Native application observation ended before release")), false),
                Err(error) => (Err(error), false),
            },
            () = stop.as_mut() => (Err(cancelled()), true),
            () = limit.as_mut() => (Err(expired()), true),
            status = watcher.wait() => (status.and_then(|status| Err(io::Error::other(format!("Native launch helper {watcher_pid} stopped during work: {status}")))), true),
            result = async {
                match activity.as_mut().as_pin_mut() {
                    Some(work) => work.await,
                    None => pending().await,
                }
            } => (result, true),
        };
        activity.set(None);
        let ended = end(&watcher);
        let recorded = if in_flight { frame.await.map(drop) } else { Ok(()) };
        break (outcome, ended.and(recorded));
    };
    activity.set(None);
    let (captured, status) = tokio::join!(native.capture(), watcher.wait());
    let confirmed = status.and_then(|status| match (native.pid, native.terminal) {
        (Some(pid), Some(terminal)) if !terminal.clean() => Err(terminal.error(pid)),
        (Some(pid), None) => Err(io::Error::other(format!("Native launch helper returned {status} without confirming application {pid} exited"))),
        (Some(_), Some(_)) if !status.success() => Err(io::Error::other(format!("Native launch helper failed with {status}"))),
        (Some(_), Some(_)) | (None, _) => Ok(()),
    });
    releasing(outcome, releasing(recorded, releasing(captured, confirmed)))
}
