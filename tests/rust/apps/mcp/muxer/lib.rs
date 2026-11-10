use core::sync::atomic::{AtomicUsize, Ordering};
use std::io;
use std::path::{Path, PathBuf};
use std::process::Stdio;
use std::sync::Arc;

use rmcp::model::{CallToolRequestParams, CallToolResult, ClientCapabilities, ClientConfig, ElicitRequestParams, ElicitResult, ElicitationAction, Implementation, ProgressNotificationParam};
use rmcp::service::{NotificationContext, RequestContext, RunningService};
use rmcp::transport::TokioChildProcess;
use rmcp::{ClientHandler, ErrorData, RoleClient, ServiceExt as _};
use serde_json::Value;
use tempfile::TempDir;
use tokio::process::Command;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Isolated {
    pub root: TempDir,
    pub muxer: PathBuf,
    pub fixture: PathBuf,
}

impl Isolated {
    pub fn new(scratch: &str) -> io::Result<Self> {
        let built = |package: &str, binary: &str| {
            let build = escargot::CargoBuild::new().package(package).bin(binary).current_release().current_target().run().map_err(io::Error::other)?;
            Ok::<_, io::Error>(build.path().to_owned())
        };
        Ok(Self { root: tempfile::tempdir_in(scratch)?, muxer: built("mcp-muxer", "mcp-muxer")?, fixture: built("mcp-muxer-tests", "mcp-fixture")? })
    }

    #[must_use]
    pub fn folder(&self, kind: &str) -> PathBuf { self.root.path().join(kind) }

    fn isolated(&self, program: &Path) -> Command {
        let mut command = Command::new(program);
        command
            .env("HOME", self.root.path())
            .env("MCP_MUXER_RUNTIME_DIR", self.folder("r"))
            .env("MCP_MUXER_CACHE_DIR", self.folder("cache"))
            .env("MCP_MUXER_LOG_DIR", self.folder("logs"))
            .env("MCP_MUXER_LOG", "debug")
            .current_dir(self.root.path());
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

    pub async fn state(&self) -> io::Result<Value> {
        let output = self.isolated(&self.muxer).stdin(Stdio::null()).output().await?;
        serde_json::Deserializer::from_slice(&output.stdout).into_iter().next().transpose().map_err(io::Error::other)?.ok_or_else(|| io::ErrorKind::UnexpectedEof.into())
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

    async fn create_elicitation(&self, _: ElicitRequestParams, _: RequestContext<RoleClient>) -> Result<ElicitResult, ErrorData> {
        Ok(ElicitResult::new(ElicitationAction::Accept).with_content(serde_json::json!({ "answer": self.marker })))
    }

    async fn on_progress(&self, _: ProgressNotificationParam, _: NotificationContext<RoleClient>) { self.progress.fetch_add(1, Ordering::Relaxed); }

    async fn on_tool_list_changed(&self, _: NotificationContext<RoleClient>) { self.changes.fetch_add(1, Ordering::Relaxed); }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn session(agent: Agent, mut command: Command) -> io::Result<RunningService<RoleClient, Agent>> {
    command.stderr(Stdio::null());
    let (transport, _) = TokioChildProcess::builder(command).spawn()?;
    agent.serve(transport).await.map_err(io::Error::other)
}

#[must_use]
pub fn call(tool: &'static str, arguments: Value) -> CallToolRequestParams {
    match arguments {
        Value::Object(arguments) => CallToolRequestParams::new(tool).with_arguments(arguments),
        _ => CallToolRequestParams::new(tool),
    }
}

#[must_use]
pub fn text(result: &CallToolResult) -> String {
    result.content.iter().filter_map(|block| block.as_text().map(|text| text.text.as_str())).collect()
}
