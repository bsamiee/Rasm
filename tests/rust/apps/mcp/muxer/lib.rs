use core::future::ready;
use core::sync::atomic::{AtomicUsize, Ordering};
use std::io;
use std::path::{Path, PathBuf};
use std::process::{ExitStatus, Stdio};
use std::sync::Arc;

use rmcp::model::{
    CallToolRequestParams, CallToolResult, ClientCapabilities, ClientConfig, ElicitRequestParams, ElicitResult, ElicitationAction, Implementation, ProgressNotificationParam, ProtocolVersion,
};
use rmcp::service::{ClientLifecycleMode, ClientServiceExt as _, NotificationContext, RequestContext, RunningService};
use rmcp::transport::TokioChildProcess;
use rmcp::{ClientHandler, ErrorData, RoleClient};
use schemars::JsonSchema;
use serde::de::DeserializeOwned;
use serde::{Deserialize, Serialize};
use serde_path_to_error::Track;
use sysinfo::{Pid, ProcessRefreshKind, ProcessesToUpdate, Signal, System, UpdateKind};
use tokio::process::Command;
use tokio::task;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Serialize, Deserialize, JsonSchema)]
pub struct Echo {
    pub text: String,
}

#[derive(Debug, Clone, Copy, Serialize, Deserialize, JsonSchema)]
pub struct Sized {
    pub bytes: usize,
}

#[derive(Debug, Clone, Copy, Serialize, Deserialize, JsonSchema)]
pub struct Slow {
    pub millis: u64,
    pub steps: u32,
}

#[derive(Debug, Clone, Serialize, Deserialize, JsonSchema)]
pub struct Ask {
    pub question: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, JsonSchema)]
pub struct Variable {
    pub name: String,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Answer {
    pub answer: String,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct Whoami {
    pub pid: u32,
    pub directory: Option<PathBuf>,
    pub value: Option<String>,
}

#[derive(Debug, Clone, PartialEq, Eq, Deserialize)]
pub struct Served {
    pub arguments: Vec<String>,
    pub era: Option<String>,
    pub calls: usize,
    pub starts: u32,
    pub failures: usize,
}

#[derive(Debug, Clone, PartialEq, Eq, Deserialize)]
pub struct Running {
    pub pid: u32,
    pub sessions: usize,
    pub calls: usize,
    pub servers: Vec<Served>,
}

#[derive(Debug, Clone, PartialEq, Eq, Deserialize)]
pub struct Reported {
    pub daemon: Option<Running>,
    pub runtime: PathBuf,
    pub cache: PathBuf,
}

#[derive(Debug, Clone)]
pub struct Isolated {
    pub root: PathBuf,
    pub muxer: PathBuf,
    pub fixture: PathBuf,
    pub level: &'static str,
}

impl Isolated {
    pub fn new(root: &Path) -> io::Result<Self> {
        let built = |package: &str, binary: &str| {
            let build = escargot::CargoBuild::new().package(package).bin(binary).current_release().current_target().run().map_err(io::Error::other)?;
            let held = root.join(binary);
            fs_err::hard_link(build.path(), &held).map(|()| held)
        };
        let building = fs_err::File::create(root.with_file_name("build.lock"))?;
        building.lock()?;
        let muxer = env!("CARGO_PKG_NAME").trim_end_matches("-tests");
        Ok(Self { root: root.to_owned(), muxer: built(muxer, muxer)?, fixture: built(env!("CARGO_PKG_NAME"), "mcp-fixture")?, level: "debug" })
    }

    #[must_use]
    pub fn folder(&self, kind: &str) -> PathBuf { self.root.join(kind) }

    fn isolated(&self, program: &Path) -> Command {
        let mut command = Command::new(program);
        command
            .env("HOME", &self.root)
            .env("MCP_MUXER_RUNTIME_DIR", "r")
            .env("MCP_MUXER_CACHE_DIR", self.folder("cache"))
            .env("MCP_MUXER_LOG_DIR", self.folder("logs"))
            .env("MCP_MUXER_LOG_LEVEL", self.level)
            .current_dir(&self.root);
        command
    }

    #[must_use]
    pub fn relay(&self, options: &[&str], server: &[&str]) -> Command {
        let mut command = self.isolated(&self.muxer);
        command.args(options).arg("--").arg(&self.fixture).args(server);
        command
    }

    #[must_use]
    pub fn direct(&self, server: &[&str]) -> Command {
        let mut command = self.isolated(&self.fixture);
        command.args(server);
        command
    }

    pub async fn state(&self, settings: &[(&str, &str)]) -> io::Result<Reported> {
        let output = self.isolated(&self.muxer).envs(settings.iter().copied()).stdin(Stdio::null()).output().await?;
        decoded(&output.stdout)
    }

    pub async fn ended(&self) -> io::Result<usize> {
        let root = fs_err::tokio::canonicalize(&self.root).await?;
        let waited = task::spawn_blocking(move || {
            let mut system = System::new().map_err(io::Error::other)?;
            system.refresh_processes_specifics(ProcessesToUpdate::All, true, ProcessRefreshKind::nothing().with_cwd(UpdateKind::Always));
            let started = Vec::from_iter(system.processes().values().filter(|process| process.cwd() == Some(&root)));
            let signalled = started.iter().filter(|process| process.kill_with(Signal::Term) == Some(true));
            Ok::<_, io::Error>(Vec::from_iter(signalled).into_iter().filter_map(|process| process.wait()).count())
        });
        waited.await.map_err(io::Error::other)?
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone, Default)]
pub struct Agent {
    pub marker: String,
    pub progress: Arc<AtomicUsize>,
    pub changes: Arc<AtomicUsize>,
}

impl ClientHandler for Agent {
    fn get_info(&self) -> ClientConfig { ClientConfig::new(ClientCapabilities::builder().enable_elicitation().build(), Implementation::new("agent", "0.0.0")) }

    fn create_elicitation(&self, _: ElicitRequestParams, _: RequestContext<RoleClient>) -> impl Future<Output = Result<ElicitResult, ErrorData>> + Send + '_ {
        let content = serde_json::to_value(Answer { answer: self.marker.clone() }).map_err(|error| ErrorData::internal_error(error.to_string(), None));
        ready(content.map(|content| ElicitResult::new(ElicitationAction::Accept).with_content(content)))
    }

    fn on_progress(&self, _: ProgressNotificationParam, _: NotificationContext<RoleClient>) -> impl Future<Output = ()> + Send + '_ {
        self.progress.fetch_add(1, Ordering::Relaxed);
        ready(())
    }

    fn on_tool_list_changed(&self, _: NotificationContext<RoleClient>) -> impl Future<Output = ()> + Send + '_ {
        self.changes.fetch_add(1, Ordering::Relaxed);
        ready(())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn decoded<T>(bytes: &[u8]) -> io::Result<T>
where T: DeserializeOwned {
    let mut decoder = serde_json::Deserializer::from_slice(bytes);
    let value = serde_path_to_error::deserialize(&mut decoder).map_err(io::Error::other)?;
    decoder.end().map(|()| value).map_err(|source| io::Error::other(serde_path_to_error::Error::new(Track::new().path(), source)))
}

pub async fn spawned(agent: Agent, command: Command, lifecycle: ClientLifecycleMode) -> io::Result<(RunningService<RoleClient, Agent>, Option<u32>)> {
    let (transport, _) = TokioChildProcess::builder(command).stderr(Stdio::null()).spawn()?;
    let process = transport.id();
    Ok((agent.serve_with_lifecycle(transport, lifecycle).await.map_err(io::Error::other)?, process))
}

pub async fn session(agent: Agent, command: Command) -> io::Result<RunningService<RoleClient, Agent>> { spawned(agent, command, ClientLifecycleMode::Initialize).await.map(|(running, _)| running) }

#[must_use]
pub fn modern() -> ClientLifecycleMode { ClientLifecycleMode::Auto { preferred_versions: vec![ProtocolVersion::LATEST], legacy_version: None } }

pub fn call(tool: &'static str, arguments: &impl Serialize) -> io::Result<CallToolRequestParams> {
    let arguments = serde_path_to_error::deserialize(serde_json::to_value(arguments).map_err(io::Error::other)?).map_err(io::Error::other)?;
    Ok(CallToolRequestParams::new(tool).with_arguments(arguments))
}

#[must_use]
pub fn text(result: &CallToolResult) -> String { result.content.iter().filter_map(|block| block.as_text().map(|text| text.text.as_str())).collect() }

pub async fn exited(process: u32) -> io::Result<Option<ExitStatus>> {
    let waited = task::spawn_blocking(move || {
        let (mut system, pid) = (System::new().map_err(io::Error::other)?, Pid::from_u32(process));
        system.refresh_processes(ProcessesToUpdate::Some(&[pid]), true);
        Ok::<_, io::Error>(system.process(pid).and_then(sysinfo::Process::wait))
    });
    waited.await.map_err(io::Error::other)?
}
