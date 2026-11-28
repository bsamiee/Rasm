use core::time::Duration;
use std::io;
use std::path::Path;
use std::process::Termination;

use backon::{ConstantBuilder, Retryable as _};
use claims::{assert_ok, assert_some};
use mcp_muxer_tests::{Agent, Echo, Isolated, Served, Whoami, call, decoded, text};
use rasm_testing::{Failure, Folder};
use rmcp::model::CallToolRequestParams;
use rmcp::service::{ClientInitializeError, RunningService};
use rmcp::{ErrorData, RoleClient};
use sysinfo::{Pid, ProcessRefreshKind, ProcessesToUpdate, System, UpdateKind};

// --- [CONSTANTS] -----------------------------------------------------------------------

const IDLE: [(&str, &str); 1] = [("MCP_MUXER_IDLE", "1s")];
const HOME: &str = "HOME";

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(test)]
async fn scoped<T>(test: impl AsyncFnOnce(&Isolated) -> T) -> Result<(), Failure>
where T: Termination {
    let ended = async |folder: &Folder| {
        let isolated = Isolated::new(folder.path(), env!("CARGO_BIN_FILE_MCP_MUXER"), env!("CARGO_BIN_EXE_mcp-fixture"))?;
        let outcome = test(&isolated).await;
        isolated.ended().await.map(|_ended| outcome)
    };
    Folder::scoped(Path::new(env!("CARGO_TARGET_TMPDIR")), ended).await
}

async fn whoami(running: &RunningService<RoleClient, Agent>, name: &str) -> Whoami {
    let pid = assert_ok!(decoded::<u32>(text(&assert_ok!(running.peer().call_tool(CallToolRequestParams::new("whoami")).await)).as_bytes()));
    let mut system = assert_ok!(System::new());
    system.refresh_processes_specifics(ProcessesToUpdate::Some(&[Pid::from_u32(pid)]), true, ProcessRefreshKind::nothing().with_cwd(UpdateKind::Always).with_environ(UpdateKind::Always));
    let process = assert_some!(system.process(Pid::from_u32(pid)));
    let value = process.environ().iter().find_map(|entry| entry.to_str()?.split_once('=').filter(|&(held, _)| held == name).map(|(_, value)| value.to_owned()));
    Whoami { pid, directory: process.cwd().map(Path::to_path_buf), value }
}

fn refusal(refused: &io::Error) -> Option<&ErrorData> {
    fn answered(refused: &ClientInitializeError) -> Option<&ErrorData> {
        match refused {
            ClientInitializeError::JsonRpcError(answer) => Some(answer),
            ClientInitializeError::LegacyFallbackFailed { discover: _, fallback } => answered(fallback),
            _ => None,
        }
    }
    refused.get_ref().and_then(|inner| inner.downcast_ref()).and_then(answered)
}

async fn echoed(running: &RunningService<RoleClient, Agent>, said: &str) -> String { text(&assert_ok!(running.peer().call_tool(assert_ok!(call("echo", &Echo { text: said.to_owned() }))).await)) }

async fn daemon(isolated: &Isolated, settings: &[(&str, &str)]) -> Option<u32> { assert_ok!(isolated.state(settings).await).daemon.map(|running| running.pid) }

async fn servers(isolated: &Isolated, settings: &[(&str, &str)]) -> Vec<Served> { assert_ok!(isolated.state(settings).await).daemon.map_or_default(|running| running.servers) }

async fn settled(isolated: &Isolated, reached: impl Fn(&[Served]) -> bool) -> Vec<Served> {
    let polled = async || {
        let held = servers(isolated, &[]).await;
        if reached(&held) { Ok(held) } else { Err(held) }
    };
    polled.retry(ConstantBuilder::new().with_delay(Duration::from_millis(50)).with_max_times(100)).await.unwrap_or_else(|last| last)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod daemon;
mod sharing;
