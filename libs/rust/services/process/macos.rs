use core::{future::pending, num::NonZero, pin::pin, time::Duration};
use std::{ffi::OsString, io, path::Path, process::Stdio};

use serde::{Deserialize, de::IgnoredAny};
use serde_json::value::RawValue;
use thiserror::Error;
use tokio::{
    process::{ChildStdout, Command},
    time::{Instant, sleep_until, timeout},
};
use tokio_stream::StreamExt as _;
use tokio_util::{
    codec::{FramedRead, LinesCodec},
    sync::CancellationToken,
};
use url::Url;

use crate::Stop;

// --- [CONSTANTS] -----------------------------------------------------------------------

const FRAME_BYTES: usize = 8 * 1024 * 1024;
const RELEASE: Duration = Duration::from_secs(25);
const CRASH_REPORT: Duration = Duration::from_secs(5);
const REAP: Duration = Duration::from_secs(5);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Launch<'path> {
    pub bundle: &'path Path,
    pub documents: &'path [&'path Path],
    pub arguments: &'path [OsString],
    pub environment: &'path [(&'path str, &'path str)],
}

#[derive(Deserialize)]
#[serde(rename_all = "lowercase")]
enum Frame<'wire> {
    Launched {
        pid: NonZero<u32>,
        created: bool,
    },
    State {
        pid: NonZero<u32>,
        url: Url,
        #[serde(borrow)]
        state: State<'wire>,
    },
    Prompts(Vec<IgnoredAny>),
    Failed(#[serde(borrow)] &'wire RawValue),
}

#[derive(Deserialize)]
#[serde(rename_all = "lowercase")]
enum State<'wire> {
    Running { status: Responsiveness, active: bool, windows: Vec<Window> },
    Exited(Termination),
    Failed(#[serde(borrow)] &'wire RawValue),
}

#[derive(Deserialize)]
#[serde(rename_all = "lowercase")]
enum Responsiveness {
    Launching,
    Responding,
    Unresponsive,
}

#[derive(Deserialize)]
struct Window {
    #[serde(default)]
    asking: bool,
}

#[derive(Clone, Copy, Deserialize)]
struct Termination {
    status: Option<i32>,
    signal: Option<i32>,
    #[serde(default)]
    reporting: bool,
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
#[error("{operation}; application release failed: {release}")]
struct ReleaseFailure {
    #[source]
    operation: io::Error,
    release: io::Error,
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Observation<'path, Observe> {
    frames: FramedRead<ChildStdout, LinesCodec>,
    bundle: &'path Path,
    observe: Observe,
    pid: Option<NonZero<u32>>,
    terminal: Option<Termination>,
}

impl<Observe: AsyncFnMut(&str) -> io::Result<()>> Observation<'_, Observe> {
    async fn next(&mut self) -> io::Result<Option<Progress>> {
        let Some(line) = self.frames.next().await.transpose().map_err(io::Error::other)? else { return Ok(None) };
        let mut decoder = serde_json::Deserializer::from_str(&line);
        let progress = (|| {
            let frame = serde_path_to_error::deserialize(&mut decoder).map_err(io::Error::other)?;
            decoder.end()?;
            self.advance(frame)
        })();
        let recorded = (self.observe)(&line).await;
        progress.and_then(|progress| recorded.map(|()| Some(progress)))
    }

    fn advance(&mut self, frame: Frame<'_>) -> io::Result<Progress> {
        match frame {
            Frame::Launched { pid, created: true } if self.pid.is_none() => self.pid = Some(pid),
            Frame::Launched { pid, created: false } => return Err(io::Error::new(io::ErrorKind::AlreadyExists, format!("Application {pid} is already running"))),
            Frame::Launched { pid, created: true } => return Err(io::Error::other(format!("Unexpected second application acknowledgment for {pid}"))),
            Frame::State { pid, url, state } => {
                if Some(pid) != self.pid || !url.to_file_path().is_ok_and(|bundle| bundle == self.bundle) {
                    return Err(io::Error::other(format!("Native state for {url} (PID {pid}) does not match {} ({:?})", self.bundle.display(), self.pid)));
                }
                return match state {
                    State::Running { status, active, windows } if !active && !windows.iter().any(|window| window.asking) => match status {
                        Responsiveness::Responding => Ok(Progress::Ready(pid)),
                        Responsiveness::Launching | Responsiveness::Unresponsive => Ok(Progress::Waiting),
                    },
                    State::Running { status: _, active, windows: _ } => Err(io::Error::other(format!("Application {pid} {}", if active { "became active" } else { "requested attention" }))),
                    State::Exited(termination) => {
                        self.terminal = Some(termination);
                        Ok(Progress::Exited(pid, termination))
                    }
                    State::Failed(error) => Err(io::Error::other(format!("Application {pid} failed: {error}"))),
                };
            }
            Frame::Prompts(prompts) if prompts.is_empty() => (),
            Frame::Prompts(_) => return Err(io::Error::other("Application launch requested attention")),
            Frame::Failed(error) => return Err(io::Error::other(format!("Native launch failed: {error}"))),
        }
        Ok(Progress::Waiting)
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

fn releasing<T>(outcome: io::Result<T>, release: io::Result<()>) -> io::Result<T> {
    match (outcome, release) {
        (Ok(value), Ok(())) => Ok(value),
        (Err(error), Ok(())) | (Ok(_), Err(error)) => Err(error),
        (Err(operation), Err(release)) => Err(io::Error::new(operation.kind(), ReleaseFailure { operation, release })),
    }
}

pub async fn launch<T>(
    mut watcher: Command,
    launch: Launch<'_>,
    deadline: Instant,
    stopping: &CancellationToken,
    observe: impl AsyncFnMut(&str) -> io::Result<()>,
    work: impl AsyncFnOnce(NonZero<u32>) -> io::Result<T>,
) -> io::Result<T> {
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
    watcher.args(["launch", "--owned"]).arg(&bundle).stdin(Stdio::piped()).stdout(Stdio::piped());
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
    let mut watcher = watcher.kill_on_drop(false).spawn()?;
    let watcher_pid = watcher.id().ok_or_else(|| io::Error::other("Native launch helper has no PID"))?;
    let mut input = Some(watcher.stdin.take().ok_or_else(|| io::Error::other("Native launch helper has no lease pipe"))?);
    let output = watcher.stdout.take().ok_or_else(|| io::Error::other("Native launch helper has no event pipe"))?;
    let mut native = Observation { frames: FramedRead::new(output, LinesCodec::new_with_max_length(FRAME_BYTES)), bundle: &bundle, observe, pid: None, terminal: None };
    let mut work = Some(work);
    let mut activity = pin!(None);
    let mut stop = pin!(stopping.cancelled());
    let mut limit = pin!(sleep_until(deadline));
    let (outcome, in_flight) = loop {
        let mut frame = pin!(native.next());
        let outcome = tokio::select! {
            biased;
            received = frame.as_mut() => {
                match received {
                    Ok(Some(Progress::Ready(pid))) if let Some(begin) = work.take() => {
                        if let Err(error) = check() { break (Err(error), None); }
                        tracing::debug!(%pid, "application ready");
                        activity.set(Some(begin(pid)));
                    }
                    Ok(Some(Progress::Ready(_) | Progress::Waiting)) => (),
                    Ok(Some(Progress::Exited(pid, exit))) => break (Err(exit.error(pid)), None),
                    Ok(None) => break (Err(io::Error::new(io::ErrorKind::UnexpectedEof, "Native application observation ended before release")), None),
                    Err(error) => break (Err(error), None),
                }
                continue;
            },
            () = stop.as_mut() => Err(cancelled()),
            () = limit.as_mut() => Err(expired()),
            status = watcher.wait() => match status {
                Ok(status) => Err(io::Error::other(format!("Native launch helper {watcher_pid} stopped during work: {status}"))),
                Err(error) => Err(error),
            },
            result = async {
                match activity.as_mut().as_pin_mut() {
                    Some(work) => work.await,
                    None => pending().await,
                }
            } => result,
        };
        activity.set(None);
        let released = Instant::now();
        drop(input.take());
        let recorded = timeout(RELEASE, frame).await.map_err(|error| io::Error::new(io::ErrorKind::TimedOut, error)).and_then(|frame| frame.map(|_| ()));
        break (outcome, Some((released, recorded)));
    };
    activity.set(None);
    if let Err(error) = &outcome {
        tracing::warn!(?native.pid, %error, "application session failed");
    }
    let report = if in_flight.is_none() && native.terminal.is_some_and(|exit| exit.reporting) {
        timeout(CRASH_REPORT, async {
            while native.terminal.is_some_and(|exit| exit.reporting) && native.next().await?.is_some() {}
            Ok(())
        })
        .await
        .unwrap_or(Ok(()))
    } else {
        Ok(())
    };
    let (released, recorded) = in_flight.unwrap_or_else(|| (Instant::now(), Ok(())));
    drop(input);
    let released = timeout(RELEASE.saturating_sub(released.elapsed()), async {
        let (captured, status) = tokio::join!(native.capture(), watcher.wait());
        let confirmed = status.and_then(|status| match (native.pid, native.terminal) {
            (Some(pid), Some(terminal)) if !terminal.clean() => Err(terminal.error(pid)),
            (Some(pid), None) => Err(io::Error::other(format!("Native launch helper returned {status} without confirming application {pid} exited"))),
            (Some(_), Some(_)) if !status.success() => Err(io::Error::other(format!("Native launch helper failed with {status}"))),
            (Some(_), Some(_)) | (None, _) => Ok(()),
        });
        releasing(captured, confirmed)
    })
    .await;
    let release = if let Ok(result) = released {
        result
    } else {
        let failed = Err(io::Error::new(io::ErrorKind::TimedOut, format!("Native launch helper {watcher_pid} did not confirm application {:?} quit", native.pid)));
        let reaped = timeout(REAP, watcher.kill()).await.unwrap_or_else(|error| Err(io::Error::other(error)));
        releasing(failed, reaped)
    };
    releasing(outcome, releasing(releasing(recorded, report), release))
}
