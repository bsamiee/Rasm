use core::{
    iter,
    num::NonZero,
    pin::{Pin, pin},
    time::Duration,
};
#[cfg(unix)]
use std::os::unix::process::ExitStatusExt as _;
use std::{
    error,
    ffi::{OsStr, OsString},
    io::{self, Write as _},
    path::{self, Path, PathBuf},
    process::ExitStatus,
    sync::Arc,
};

use fs_err::{File, create_dir_all};
use itertools::Itertools as _;
use jiff::Timestamp;
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use serde::Serialize;
use tempfile::{Builder, NamedTempFile, TempDir};
use tokio::{
    process::Command,
    sync::Mutex,
    task,
    time::{Instant, sleep_until},
};
use tokio_util::sync::CancellationToken;
use walkdir::WalkDir;

use crate::{Failure, Operation, Stop};

// --- [CONSTANTS] -----------------------------------------------------------------------

const JOURNAL: &str = "lifecycle.jsonl";
const REPORT: &str = "execution.json";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Serialize)]
#[serde(tag = "status", content = "error", rename_all = "lowercase")]
enum Verdict {
    Passed,
    Failed(String),
    Interrupted(String),
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct Evidence<'event, Details> {
    name: &'event str,
    elapsed_ms: f64,
    details: Details,
    #[serde(skip_serializing_if = "Option::is_none")]
    error: Option<&'event str>,
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct Exit {
    code: Option<i32>,
    #[cfg(unix)]
    signal: Option<i32>,
    #[cfg(unix)]
    core_dumped: bool,
}

#[derive(Serialize)]
#[serde(untagged)]
enum Subject<'subject> {
    Process {
        pid: Option<NonZero<u32>>,
        program: &'subject Path,
        log: &'subject Path,
        stderr: &'subject Path,
        #[serde(flatten)]
        exit: Option<Exit>,
        #[serde(skip_serializing_if = "Option::is_none")]
        case: Option<String>,
    },
    Application {
        #[serde(skip_serializing_if = "Option::is_none")]
        pid: Option<NonZero<u32>>,
        bundle: &'subject Path,
        log: &'subject Path,
    },
    Scratch {
        directory: &'subject Path,
    },
}

#[derive(Serialize)]
#[serde(untagged, rename_all_fields = "camelCase")]
enum State {
    Running {
        status: &'static str,
    },
    Finished {
        #[serde(flatten)]
        verdict: Verdict,
        elapsed_ms: f64,
        attachments: Vec<PathBuf>,
    },
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct Report {
    started: Timestamp,
    deadline_ms: f64,
    journal: &'static str,
    #[serde(flatten)]
    state: State,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Run {
    directory: PathBuf,
    scratch: TempDir,
    start: Instant,
    started: Timestamp,
    deadline: Instant,
    stopping: CancellationToken,
    journal: Arc<Mutex<File>>,
}

impl Run {
    // --- [LIFETIME]
    pub async fn execute<Output, Failure>(
        output_root: &Path,
        budget: Duration,
        interrupt: impl Future<Output = ()>,
        work: impl AsyncFnOnce(&Self) -> Result<Output, Failure>,
    ) -> Result<Output, NEVec<Failure>>
    where
        Failure: error::Error + From<io::Error>,
    {
        async fn stopped<Work, Output, Failure>(run: &Run, reason: Stop, work: Pin<&mut Work>) -> (Result<Output, Failure>, Verdict, io::Result<()>)
        where
            Work: Future<Output = Result<Output, Failure>>,
            Failure: error::Error + From<io::Error>,
        {
            let (name, kind, verdict): (_, _, fn(String) -> Verdict) = match reason {
                Stop::Cancelled => ("run interrupted", io::ErrorKind::Interrupted, Verdict::Interrupted),
                Stop::Deadline => ("run deadline expired", io::ErrorKind::TimedOut, Verdict::Failed),
            };
            run.stopping.cancel();
            let source = io::Error::from(kind);
            let message = source.to_string();
            let (reported, result) = tokio::join!(run.fault(name, (), &message), work);
            let error = result.err().unwrap_or_else(|| source.into());
            let verdict = verdict(error::Report::new(&error).to_string());
            (Err(error), verdict, reported)
        }
        let run = Self::open(output_root, budget).await.map_err(|source| NEVec::new(Failure::from(source)))?;
        let (result, verdict, reported) = {
            let mut work = pin!(work(&run));
            tokio::select! {
                biased;
                result = &mut work => {
                    let verdict = result.as_ref().map_or_else(|error| Verdict::Failed(error::Report::new(error).to_string()), |_| Verdict::Passed);
                    (result, verdict, Ok(()))
                }
                () = sleep_until(run.deadline) => stopped(&run, Stop::Deadline, work).await,
                () = interrupt => stopped(&run, Stop::Cancelled, work).await,
            }
        };
        let finished = run.finish(verdict).await;
        let later = iter::chain(reported.err(), finished.err().into_iter().flatten()).map(Failure::from).collect::<Vec<_>>();
        match result {
            Ok(value) => NEVec::try_from_vec(later).map_or(Ok(value), Err),
            Err(error) => Err(NEVec::from((error, later))),
        }
    }

    async fn open(output_root: &Path, budget: Duration) -> io::Result<Self> {
        let start = Instant::now();
        let started = Timestamp::now();
        let deadline = start.checked_add(budget).filter(|_| !budget.is_zero()).ok_or_else(|| io::Error::new(io::ErrorKind::InvalidInput, "Invalid run deadline"))?;
        let output_root = path::absolute(output_root)?;
        let run = task::spawn_blocking(move || {
            create_dir_all(&output_root)?;
            let directory = Builder::new().prefix("run-").tempdir_in(&output_root)?;
            let scratch = Builder::new().prefix("scratch-").tempdir_in(output_root)?;
            let mut journal = File::create_new(directory.path().join(JOURNAL))?;
            write_report(directory.path(), &Report { started, deadline_ms: budget.as_millis_f64(), journal: JOURNAL, state: State::Running { status: "running" } })?;
            write_line(&mut journal, encoded(&Evidence { name: "run started", elapsed_ms: start.elapsed().as_millis_f64(), details: (), error: None })?)?;
            Ok::<_, io::Error>(Self { directory: directory.keep(), scratch, start, started, deadline, stopping: CancellationToken::new(), journal: Arc::new(Mutex::new(journal)) })
        })
        .await
        .map_err(io::Error::other)??;
        tracing::info!(directory = %run.directory.display(), "Run started");
        Ok(run)
    }

    async fn finish(self, verdict: Verdict) -> Result<(), NEVec<io::Error>> {
        let Self { directory, scratch, start, started, deadline, stopping: _, journal } = self;
        let mut journal = journal.lock_owned().await;
        task::spawn_blocking(move || {
            let scratch_path = scratch.path().to_owned();
            let released =
                scratch.close().or_else(|error| (error.kind() == io::ErrorKind::NotFound).ok_or(error)).err().map(|error| format!("Cannot remove scratch {}: {error}", scratch_path.display()));
            let details = Subject::Scratch { directory: &scratch_path };
            let recorded = encoded(&Evidence { name: "run scratch released", elapsed_ms: start.elapsed().as_millis_f64(), details, error: released.as_deref() })
                .and_then(|line| write_line(&mut journal, line))
                .err()
                .map(|error| format!("Cannot record scratch release: {error}"));
            let listed = WalkDir::new(&directory)
                .min_depth(1)
                .sort_by_file_name()
                .into_iter()
                .filter_map(|entry| match entry {
                    Ok(entry) if entry.file_type().is_file() => Some(entry.into_path().strip_prefix(&directory).map(Path::to_owned).map_err(io::Error::other)),
                    Ok(_) => None,
                    Err(error) => Some(Err(error.into())),
                })
                .filter(|entry| !matches!(entry, Ok(path) if path == Path::new(REPORT) || path == Path::new(JOURNAL)))
                .collect::<io::Result<Vec<_>>>();
            let (attachments, unlisted) = match listed {
                Ok(attachments) => (attachments, None),
                Err(error) => (Vec::new(), Some(format!("Cannot list run attachments: {error}"))),
            };
            let failures = [released, recorded, unlisted].into_iter().flatten().collect::<Vec<_>>();
            let verdict = match (verdict, failures.is_empty()) {
                (Verdict::Passed, true) => Verdict::Passed,
                (Verdict::Interrupted(error), true) => Verdict::Interrupted(error),
                (Verdict::Passed, false) => Verdict::Failed(failures.iter().join("\n")),
                (Verdict::Failed(error), _) | (Verdict::Interrupted(error), false) => Verdict::Failed(iter::chain([&error], &failures).join("\n")),
            };
            let state = State::Finished { verdict, elapsed_ms: start.elapsed().as_millis_f64(), attachments };
            let published = write_report(&directory, &Report { started, deadline_ms: deadline.duration_since(start).as_millis_f64(), journal: JOURNAL, state });
            let failures = iter::chain(failures, published.err().map(|error| format!("Cannot publish final run report: {error}"))).map(io::Error::other).collect();
            NEVec::try_from_vec(failures).map_or(Ok(()), Err)
        })
        .await
        .map_err(|error| NEVec::new(io::Error::other(error)))?
    }

    // --- [PATHS_AND_TIME]
    #[must_use]
    pub fn directory(&self) -> &Path { &self.directory }

    #[must_use]
    pub fn scratch(&self) -> &Path { self.scratch.path() }

    #[must_use]
    pub const fn deadline(&self) -> Instant { self.deadline }

    pub async fn cancelled(&self) { self.stopping.cancelled().await; }

    // --- [PROCESSES]
    pub async fn command(&self, mut command: Command, active_case: Option<&Path>) -> io::Result<NonZero<u32>> {
        let program = PathBuf::from(command.as_std().get_program());
        let stem = program.file_stem().ok_or_else(|| io::Error::new(io::ErrorKind::InvalidInput, "program has no file name"))?;
        let named = |suffix: &str| self.directory.join([stem, OsStr::new(suffix)].into_iter().collect::<OsString>());
        let (log, stderr) = (named(".log"), named("-stderr.log"));
        let open = async |path: &Path| Ok::<_, io::Error>(fs_err::tokio::File::create_new(path).await?.into_std().await.into_file());
        command.stdout(open(&log).await?).stderr(open(&stderr).await?);
        let process = |pid, exit, case| Subject::Process { pid, program: &program, log: &log, stderr: &stderr, exit, case };
        let started = async |pid, _, _| self.record("process started", process(Some(pid), None, None)).await;
        let exited = async |pid, outcome: Result<ExitStatus, Stop>| {
            let (name, exit, error) = match outcome {
                Ok(status) => (
                    "process exited",
                    Some(Exit {
                        code: status.code(),
                        #[cfg(unix)]
                        signal: status.signal(),
                        #[cfg(unix)]
                        core_dumped: status.core_dumped(),
                    }),
                    status.exit_ok().err().map(|source| source.to_string()),
                ),
                Err(reason) => ("process interrupted", None, Some(reason.to_string())),
            };
            let Some(error) = error else { return self.record(name, process(Some(pid), exit, None)).await };
            let case = match active_case {
                Some(path) => fs_err::tokio::read_to_string(path).await.map(Some).or_else(|source| (source.kind() == io::ErrorKind::NotFound).ok_or(source).map(|()| None))?,
                None => None,
            };
            self.fault(name, process(Some(pid), exit, case), &error).await
        };
        let (pid, status) = match crate::run(command, self.deadline, &self.stopping, started, exited).await {
            Ok(process) => process,
            Err(mut failure) => {
                if failure.process.is_none()
                    && let Err(source) = self.fault("process did not start", process(None, None, None), &failure.to_string()).await
                {
                    failure.failures.push(Failure::Io { operation: Operation::Observe, source });
                }
                return Err(io::Error::other(failure));
            }
        };
        status.exit_ok().map_err(io::Error::other).map(|()| pid)
    }

    #[cfg(target_os = "macos")]
    pub async fn application<T>(&self, watcher: Command, launch: crate::macos::Launch<'_>, work: impl AsyncFnOnce(NonZero<u32>) -> io::Result<T>) -> io::Result<T> {
        let log = self.directory.join("watch-apps.jsonl");
        let frames = Arc::new(Mutex::new(fs_err::tokio::OpenOptions::new().create(true).append(true).open(&log).await?.into_std().await));
        let bundle = launch.bundle;
        let application = |pid| Subject::Application { pid, bundle, log: &log };
        self.record("application selected", application(None)).await?;
        crate::macos::launch(
            watcher,
            launch,
            self.deadline,
            &self.stopping,
            async |line: &str| {
                let mut row = line.as_bytes().to_vec();
                row.push(b'\n');
                let frames = Arc::clone(&frames);
                task::spawn_blocking(move || frames.blocking_lock().write_all(&row)).await.map_err(io::Error::other)?
            },
            async |pid| {
                self.record("application ready", application(Some(pid))).await?;
                work(pid).await
            },
        )
        .await
    }

    // --- [EVIDENCE]
    pub async fn record(&self, name: &str, details: impl Serialize) -> io::Result<()> { self.append(name, details, None).await }

    pub async fn fault(&self, name: &str, details: impl Serialize, error: &str) -> io::Result<()> {
        tracing::error!(directory = %self.directory.display(), name, { EXCEPTION_MESSAGE } = error, "run fault");
        self.append(name, details, Some(error)).await
    }

    async fn append(&self, name: &str, details: impl Serialize, error: Option<&str>) -> io::Result<()> {
        let mut journal = Arc::clone(&self.journal).lock_owned().await;
        let line = encoded(&Evidence { name, elapsed_ms: self.start.elapsed().as_millis_f64(), details, error })?;
        task::spawn_blocking(move || write_line(&mut journal, line)).await.map_err(io::Error::other)?
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn encoded(evidence: &Evidence<'_, impl Serialize>) -> io::Result<(Vec<u8>, bool)> {
    let mut line = serde_json::to_vec(evidence)?;
    line.push(b'\n');
    Ok((line, evidence.error.is_some()))
}

fn write_line(journal: &mut File, (line, durable): (Vec<u8>, bool)) -> io::Result<()> {
    journal.write_all(&line)?;
    if durable { journal.sync_data() } else { Ok(()) }
}

fn write_report(directory: &Path, report: &Report) -> io::Result<()> {
    let mut temporary = NamedTempFile::new_in(directory)?;
    temporary.write_all(&serde_json::to_vec_pretty(report)?)?;
    temporary.as_file().sync_data()?;
    temporary.persist(directory.join(REPORT)).map(drop).map_err(io::Error::from)
}
