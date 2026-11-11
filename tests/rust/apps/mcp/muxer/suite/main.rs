#![feature(error_reporter)]

use std::path::Path;
use std::process::Termination;

use claims::assert_ok;
use mcp_muxer_tests::{Agent, Echo, Isolated, Served, Variable, Whoami, call, decoded, text};
use rasm_testing::{Failure, Fixture as _, Folder};
use rmcp::RoleClient;
use rmcp::service::RunningService;

// --- [CONSTANTS] -----------------------------------------------------------------------

const IDLE: [(&str, &str); 1] = [("MCP_MUXER_IDLE", "1s")];
const HOME: &str = "HOME";

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(test)]
async fn scoped<T>(test: impl AsyncFnOnce(&Isolated) -> T) -> Result<(), Failure<Folder>>
where T: Termination {
    let ended = async |folder: &Folder| {
        let isolated = Isolated::new(folder.path())?;
        let outcome = test(&isolated).await;
        isolated.ended().await.map(|_ended| outcome)
    };
    Folder::scoped(Path::new(env!("CARGO_TARGET_TMPDIR")), ended).await
}

async fn whoami(running: &RunningService<RoleClient, Agent>, name: &str) -> Whoami {
    let result = assert_ok!(running.peer().call_tool(assert_ok!(call("whoami", &Variable { name: name.to_owned() }))).await);
    assert_ok!(decoded(text(&result).as_bytes()))
}

async fn echoed(running: &RunningService<RoleClient, Agent>, said: &str) -> String { text(&assert_ok!(running.peer().call_tool(assert_ok!(call("echo", &Echo { text: said.to_owned() }))).await)) }

async fn daemon(isolated: &Isolated, settings: &[(&str, &str)]) -> Option<u32> { assert_ok!(isolated.state(settings).await).daemon.map(|running| running.pid) }

async fn servers(isolated: &Isolated, settings: &[(&str, &str)]) -> Vec<Served> { assert_ok!(isolated.state(settings).await).daemon.map_or_default(|running| running.servers) }

// --- [COMPOSITION] ---------------------------------------------------------------------

mod backpressure;
mod cli;
mod daemon;
mod framing;
mod load;
mod process;
mod sharing;
mod wire;
