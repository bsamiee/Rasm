use core::{num::NonZero, pin::pin, time::Duration};
#[cfg(unix)]
use std::os::unix::process::ExitStatusExt as _;
use std::{
    error,
    ffi::OsStr,
    io::{self, Write as _},
    path::{self, Path, PathBuf},
    process::ExitStatus,
    sync::Arc,
};

use fs_err::{File, PathExt as _, create_dir_all};
use jiff::Timestamp;
use nonempty_collections::NEVec;
use serde::Serialize;
use serde_json::{Map, json};
use tempfile::{Builder, NamedTempFile, TempDir};
use tokio::{
    process::Command,
    sync::Mutex,
    task,
    time::{Instant, sleep_until},
};
use tokio_util::sync::CancellationToken;
use walkdir::WalkDir;

use crate::{Failure, Stop};

// --- [CONSTANTS] -----------------------------------------------------------------------

const JOURNAL: &str = "lifecycle.jsonl";
const REPORT: &str = "execution.json";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Serialize)]
#[serde(tag = "status", content = "error", rename_all = "lowercase")]
enum Verdict<Message> {
    Passed,
    Failed(Message),
    Interrupted(Message),
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
#[serde(untagged, rename_all_fields = "camelCase")]
enum State<'report> {
    Running {
        status: &'static str,
    },
    Finished {
        #[serde(flatten)]
        verdict: Verdict<&'report str>,
        elapsed_ms: f64,
        attachments: Vec<PathBuf>,
    },
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct Report<'report> {
    started: Timestamp,
    deadline_ms: f64,
    journal: &'static str,
    #[serde(flatten)]
    state: State<'report>,
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
        let run = Self::open(output_root, budget).await.map_err(|source| NEVec::new(Failure::from(source)))?;
        let (result, stopped, reported) = {
            let active = Instant::now() < run.deadline;
            let mut work = pin!(work(&run));
            let selected = if active {
                tokio::select! {
                    biased;
                    result = &mut work => Ok(result),
                    () = sleep_until(run.deadline) => Err(Stop::Deadline),
                    () = interrupt => Err(Stop::Cancelled),
                }
            } else {
                Err(Stop::Deadline)
            };
            match selected {
                Ok(result) => (result, None, Ok(())),
                Err(reason) => {
                    let (name, kind) = match reason {
                        Stop::Cancelled => ("run interrupted", io::ErrorKind::Interrupted),
                        Stop::Deadline => ("run deadline expired", io::ErrorKind::TimedOut),
                    };
                    run.stopping.cancel();
                    let source = io::Error::from(kind);
                    let error = source.to_string();
                    let fault = run.fault(name, (), &error);
                    let (reported, work_error) = if active {
                        let (reported, result) = tokio::join!(fault, work);
                        (reported, result.err())
                    } else {
                        (fault.await, None)
                    };
                    (Err(work_error.unwrap_or_else(|| source.into())), Some(reason), reported)
                }
            }
        };
        let error = result.as_ref().err().map(|source| error::Report::new(source).to_string());
        let verdict = match (error, stopped) {
            (None, _) => Verdict::Passed,
            (Some(error), Some(Stop::Cancelled)) => Verdict::Interrupted(error),
            (Some(error), None | Some(Stop::Deadline)) => Verdict::Failed(error),
        };
        let finished = run.finish(verdict).await;
        let later = reported.err().into_iter().chain(finished.err()).map(Failure::from).collect::<Vec<_>>();
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
            let deadline_ms = budget.as_millis_f64();
            write_report(directory.path(), &Report { started, deadline_ms, journal: JOURNAL, state: State::Running { status: "running" } })?;
            let line = encoded(&Evidence { name: "run started", elapsed_ms: start.elapsed().as_millis_f64(), details: json!({ "deadlineMs": deadline_ms }), error: None })?;
            write_line(&mut journal, line)?;
            Ok::<_, io::Error>(Self { directory: directory.keep(), scratch, start, started, deadline, stopping: CancellationToken::new(), journal: Arc::new(Mutex::new(journal)) })
        })
        .await
        .map_err(io::Error::other)??;
        tracing::info!(directory = %run.directory.display(), "Run started");
        Ok(run)
    }

    async fn finish(self, verdict: Verdict<String>) -> io::Result<()> {
        let Self { directory, scratch, start, started, deadline, stopping: _, journal } = self;
        let mut journal = journal.lock_owned().await;
        task::spawn_blocking(move || {
            let scratch_path = scratch.path().to_owned();
            let released = scratch.close().or_else(|error| if error.kind() == io::ErrorKind::NotFound { Ok(()) } else { Err(error) }).and_then(|()| match scratch_path.fs_err_symlink_metadata() {
                Err(error) if error.kind() == io::ErrorKind::NotFound => Ok(()),
                Err(error) => Err(error),
                Ok(_) => Err(io::Error::other(format!("Scratch directory {} still exists", scratch_path.display()))),
            });
            let release_error = released.as_ref().err().map(ToString::to_string);
            let (name, details) = match &release_error {
                None => ("run scratch released", serde_json::json!({ "directory": scratch_path, "absent": true })),
                Some(_) => ("run scratch cleanup failed", serde_json::json!({ "directory": scratch_path })),
            };
            let recorded = encoded(&Evidence { name, elapsed_ms: start.elapsed().as_millis_f64(), details, error: release_error.as_deref() }).and_then(|line| write_line(&mut journal, line));
            let listed = attachments(&directory);
            let errors = release_error
                .map(|error| format!("Cannot remove scratch {}: {error}", scratch_path.display()))
                .into_iter()
                .chain(recorded.err().map(|error| format!("Cannot record scratch release: {error}")))
                .chain(listed.as_ref().err().map(|error| format!("Cannot list run attachments: {error}")))
                .collect::<Vec<_>>();
            let combined = (!errors.is_empty()).then(|| {
                let work_error = match &verdict {
                    Verdict::Passed => None,
                    Verdict::Failed(error) | Verdict::Interrupted(error) => Some(error),
                };
                work_error.into_iter().chain(&errors).map(String::as_str).collect::<Vec<_>>().join("\n")
            });
            let verdict = match (&combined, &verdict) {
                (Some(error), _) | (None, Verdict::Failed(error)) => Verdict::Failed(error.as_str()),
                (None, Verdict::Interrupted(error)) => Verdict::Interrupted(error.as_str()),
                (None, Verdict::Passed) => Verdict::Passed,
            };
            let state = State::Finished { verdict, elapsed_ms: start.elapsed().as_millis_f64(), attachments: listed.unwrap_or_default() };
            let published = write_report(&directory, &Report { started, deadline_ms: deadline.duration_since(start).as_millis_f64(), journal: JOURNAL, state });
            let errors = errors.into_iter().chain(published.err().map(|error| format!("Cannot publish final run report: {error}"))).collect::<Vec<_>>();
            if errors.is_empty() { Ok(()) } else { Err(io::Error::other(errors.join("\n"))) }
        })
        .await
        .map_err(io::Error::other)?
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
    pub async fn command(&self, mut command: Command, stem: &str, active_case: Option<&Path>) -> io::Result<NonZero<u32>> {
        if Path::new(stem).file_name() != Some(OsStr::new(stem)) {
            return Err(io::Error::new(io::ErrorKind::InvalidInput, "log name must be a single path component"));
        }
        let log = self.directory.join(format!("{stem}.log"));
        let stderr = self.directory.join(format!("{stem}-stderr.log"));
        let program = PathBuf::from(command.as_std().get_program());
        let open = async |path: &Path| Ok::<_, io::Error>(fs_err::tokio::File::create_new(path).await?.into_std().await.into_file());
        command.stdout(open(&log).await?).stderr(open(&stderr).await?);
        let process = |pid: Option<NonZero<u32>>| {
            [("pid".into(), json!(pid)), ("program".into(), json!(program)), ("log".into(), json!(log)), ("stderr".into(), json!(stderr))].into_iter().collect::<Map<_, _>>()
        };
        let started = async |pid, _, _| self.record("process started", process(Some(pid))).await;
        let exited = async |pid, outcome: Result<ExitStatus, Stop>| {
            let mut details = process(Some(pid));
            let (name, error) = match outcome {
                Ok(status) => {
                    details.extend([("code".into(), json!(status.code()))]);
                    #[cfg(unix)]
                    details.extend([("signal".into(), json!(status.signal())), ("coreDumped".into(), json!(status.core_dumped()))]);
                    ("process exited", status.exit_ok().err().map(|source| source.to_string()))
                }
                Err(reason) => ("process interrupted", Some(reason.to_string())),
            };
            let Some(error) = error else { return self.record(name, details).await };
            if let Some(path) = active_case {
                match fs_err::tokio::read_to_string(path).await {
                    Ok(case) => drop(details.insert("case".into(), json!(case))),
                    Err(source) if source.kind() == io::ErrorKind::NotFound => (),
                    Err(source) => drop(details.insert("caseError".into(), json!(source.to_string()))),
                }
            }
            self.fault(name, details, &error).await
        };
        let (pid, status) = match crate::run(command, self.deadline, &self.stopping, started, exited).await {
            Ok(process) => process,
            Err(mut failure) => {
                if failure.process.is_none()
                    && let Err(error) = self.fault("process did not start", process(None), &failure.to_string()).await
                {
                    failure.failures.push(Failure::Observe(error));
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
        self.record("application selected", json!({ "bundle": bundle, "log": log })).await?;
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
                self.record("application ready", json!({ "pid": pid, "bundle": bundle })).await?;
                work(pid).await
            },
        )
        .await
    }

    // --- [EVIDENCE]
    pub async fn record(&self, name: &str, details: impl Serialize) -> io::Result<()> { self.append(name, details, None).await }

    pub async fn fault(&self, name: &str, details: impl Serialize, error: &str) -> io::Result<()> {
        tracing::error!(directory = %self.directory.display(), %error, "{name}");
        self.append(name, details, Some(error)).await
    }

    async fn append(&self, name: &str, details: impl Serialize, error: Option<&str>) -> io::Result<()> {
        let mut journal = Arc::clone(&self.journal).lock_owned().await;
        let line = encoded(&Evidence { name, elapsed_ms: self.start.elapsed().as_millis_f64(), details, error })?;
        task::spawn_blocking(move || write_line(&mut journal, line)).await.map_err(io::Error::other)?
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [JOURNAL]
fn encoded(evidence: &Evidence<'_, impl Serialize>) -> io::Result<(Vec<u8>, bool)> {
    let mut line = serde_json::to_vec(evidence)?;
    line.push(b'\n');
    Ok((line, evidence.error.is_some()))
}

fn write_line(journal: &mut File, (line, durable): (Vec<u8>, bool)) -> io::Result<()> {
    journal.write_all(&line)?;
    if durable { journal.sync_data() } else { Ok(()) }
}

// --- [REPORT]
fn attachments(directory: &Path) -> io::Result<Vec<PathBuf>> {
    let mut files = WalkDir::new(directory)
        .min_depth(1)
        .into_iter()
        .filter_map(|entry| match entry {
            Ok(entry) if entry.file_type().is_file() => Some(entry.into_path().strip_prefix(directory).map(Path::to_owned).map_err(io::Error::other)),
            Ok(_) => None,
            Err(error) => Some(Err(error.into())),
        })
        .filter(|entry| !matches!(entry, Ok(path) if path == Path::new(REPORT) || path == Path::new(JOURNAL)))
        .collect::<io::Result<Vec<_>>>()?;
    files.sort_unstable();
    Ok(files)
}

fn write_report(directory: &Path, report: &Report<'_>) -> io::Result<()> {
    let mut temporary = NamedTempFile::new_in(directory)?;
    temporary.write_all(&serde_json::to_vec_pretty(report)?)?;
    temporary.as_file().sync_data()?;
    temporary.persist(directory.join(REPORT)).map(drop).map_err(io::Error::from)
}
