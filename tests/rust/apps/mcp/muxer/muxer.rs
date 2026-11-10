use core::sync::atomic::Ordering;
use core::time::Duration;

use claims::{assert_err, assert_ok, assert_some};
use futures::future;
use mcp_muxer_tests::{Agent, Isolated, call, session, text};
use rmcp::model::{CallToolRequest, ClientRequest};
use rmcp::service::PeerRequestOptions;
use serde_json::{Value, json};
use sysinfo::{Pid, ProcessesToUpdate, System};
use tokio::time::{self, Instant};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn isolated() -> Isolated { assert_ok!(Isolated::new(env!("CARGO_TARGET_TMPDIR"))) }

async fn whoami(running: &rmcp::service::RunningService<rmcp::RoleClient, Agent>, name: &str) -> Value {
    let result = assert_ok!(running.peer().call_tool(call("whoami", json!({ "name": name }))).await);
    assert_ok!(serde_json::Deserializer::from_str(&text(&result)).into_iter::<Value>().next().transpose()).unwrap_or_default()
}

async fn stopped(isolated: &Isolated) {
    let waited = time::timeout(Duration::from_secs(30), async {
        while assert_ok!(isolated.state().await)["daemon"] != Value::Null {
            time::sleep(Duration::from_millis(100)).await;
        }
    });
    assert_ok!(waited.await, "daemon exits once it holds nothing");
}

// --- [SHARING]

#[tokio::test(flavor = "multi_thread")]
async fn sessions_share_one_server_per_key() {
    let isolated = isolated();
    let relay = |options: &[&str]| session(Agent::default(), isolated.relay(options, &[]));
    let (first, second, tagged, other, own) = (
        assert_ok!(relay(&[]).await),
        assert_ok!(relay(&[]).await),
        assert_ok!(relay(&["--env", "FIXTURE_TAG=one"]).await),
        assert_ok!(relay(&["--env", "FIXTURE_TAG=two"]).await),
        assert_ok!(relay(&["--per", "session"]).await),
    );
    let direct = assert_ok!(session(Agent::default(), isolated.direct(&[])).await);
    let (shared, same, one, two, private, unshared) = (
        whoami(&first, "HOME").await,
        whoami(&second, "HOME").await,
        whoami(&tagged, "FIXTURE_TAG").await,
        whoami(&other, "FIXTURE_TAG").await,
        whoami(&own, "HOME").await,
        whoami(&direct, "HOME").await,
    );
    assert_eq!(shared["pid"], same["pid"], "two sessions with one key reach one server process");
    assert_eq!(shared["value"], unshared["value"], "the server receives the session environment an unshared run receives");
    assert_eq!(shared["directory"], unshared["directory"], "the server runs in the session working directory");
    assert_eq!((&one["value"], &two["value"]), (&json!("one"), &json!("two")), "declared variables reach the server");
    assert_ne!(one["pid"], two["pid"], "sessions with differing declared values never share");
    assert_ne!(shared["pid"], private["pid"], "a per-session server is not shared");
    let state = assert_ok!(isolated.state().await);
    assert_eq!(state["folders"]["cache"], json!(isolated.folder("cache")), "the state report names the overridden cache folder");
    assert!(assert_ok!(std::fs::read_dir(isolated.folder("cache"))).count() >= 1, "stored handshakes sit in the overridden cache folder");
    assert!(assert_ok!(std::fs::read_dir(isolated.folder("logs"))).count() >= 1, "log files sit in the overridden log folder");
    assert!(assert_ok!(std::fs::read_dir(isolated.folder("r"))).count() >= 2, "socket and lock sit in the overridden runtime folder");
}

#[tokio::test(flavor = "multi_thread")]
async fn racing_first_sessions_all_share() {
    let isolated = isolated();
    for round in 0..100 {
        let relay = || {
            let mut command = isolated.relay(&[], &[]);
            command.env("MCP_MUXER_RUNTIME_DIR", isolated.folder(&format!("r{round}"))).env("MCP_MUXER_IDLE", "1s");
            session(Agent::default(), command)
        };
        let (first, second) = future::join(relay(), relay()).await;
        let (first, second) = (assert_ok!(first), assert_ok!(second));
        assert_eq!(whoami(&first, "HOME").await["pid"], whoami(&second, "HOME").await["pid"], "round {round}: neither racing session fell back to an unshared server");
    }
}

// --- [RESILIENCE]

#[tokio::test(flavor = "multi_thread")]
async fn relay_survives_daemon_loss() {
    let isolated = isolated();
    let agent = Agent::default();
    let running = assert_ok!(session(agent.clone(), isolated.relay(&[], &[])).await);
    let before = whoami(&running, "HOME").await;
    let daemon = assert_some!(assert_ok!(isolated.state().await)["daemon"]["pid"].as_u64().and_then(|pid| u32::try_from(pid).ok()));
    let slow = running.peer().call_tool(call("slow", json!({ "millis": 5000, "steps": 5 })));
    let killed = async {
        time::sleep(Duration::from_millis(300)).await;
        let mut system = assert_ok!(System::new());
        system.refresh_processes(ProcessesToUpdate::Some(&[Pid::from_u32(daemon)]), true);
        assert!(assert_some!(system.process(Pid::from_u32(daemon))).kill(), "the daemon is killed mid-call");
    };
    let (answered, ()) = future::join(slow, killed).await;
    assert_err!(answered, "the call in flight is answered with an error under its own id");
    let after = whoami(&running, "HOME").await;
    assert_ne!(before["pid"], after["pid"], "the next call reaches a server under a new daemon");
    assert_eq!(agent.changes.load(Ordering::Relaxed), 1, "the session is told its tool list changed");
}

#[tokio::test(flavor = "multi_thread")]
async fn newer_build_takes_over() {
    let isolated = isolated();
    let installed = isolated.root.path().join("mcp-muxer");
    assert_ok!(std::fs::copy(&isolated.muxer, &installed));
    let relay = || {
        let command = isolated.relay(&[], &[]);
        let mut rebuilt = tokio::process::Command::new(&installed);
        rebuilt.args(command.as_std().get_args()).envs(command.as_std().get_envs().filter_map(|(name, value)| Some((name, value?)))).current_dir(isolated.root.path());
        session(Agent::default(), rebuilt)
    };
    let old = assert_ok!(relay().await);
    let (before, first) = (whoami(&old, "HOME").await, assert_ok!(isolated.state().await)["daemon"]["pid"].clone());
    time::sleep(Duration::from_millis(50)).await;
    let staged = installed.with_extension("new");
    assert_ok!(std::fs::copy(&isolated.muxer, &staged));
    assert_ok!(std::fs::rename(&staged, &installed));
    let new = assert_ok!(relay().await);
    let after = whoami(&new, "HOME").await;
    let second = assert_ok!(isolated.state().await)["daemon"]["pid"].clone();
    assert_ne!(first, second, "a relay from a newer file replaces the daemon");
    assert_eq!(whoami(&old, "HOME").await["pid"], after["pid"], "the old session keeps working and shares the new daemon's server");
    assert_ne!(before["pid"], after["pid"], "the old daemon's server stopped with it");
}

#[tokio::test(flavor = "multi_thread")]
async fn slow_start_waits_once_then_answers_from_the_stored_handshake() {
    let isolated = isolated();
    let relay = || {
        let mut command = isolated.relay(&[], &["--start-delay", "25s"]);
        command.env("MCP_MUXER_IDLE", "1s");
        session(Agent::default(), command)
    };
    let started = Instant::now();
    let cold = assert_ok!(relay().await);
    assert!(started.elapsed() >= Duration::from_secs(24), "with no stored handshake the session waits for the server");
    assert_eq!(text(&assert_ok!(cold.peer().call_tool(call("echo", json!({ "text": "cold" }))).await)), "cold");
    assert_eq!(assert_ok!(isolated.state().await)["daemon"]["servers"][0]["starts"], json!(1), "the slow server started once");
    assert_ok!(cold.cancel().await);
    stopped(&isolated).await;
    let started = Instant::now();
    let warm = assert_ok!(relay().await);
    assert!(started.elapsed() < Duration::from_secs(5), "with a stored handshake the session is answered at once");
    assert_eq!(text(&assert_ok!(warm.peer().call_tool(call("echo", json!({ "text": "warm" }))).await)), "warm", "the first call waits for the server");
    assert_eq!(assert_ok!(isolated.state().await)["daemon"]["servers"][0]["starts"], json!(1), "the slow server started once more, with no restart");
}

#[tokio::test(flavor = "multi_thread")]
async fn failed_start_reports_its_cause_and_backs_off() {
    let isolated = isolated();
    let refused = assert_err!(session(Agent::default(), isolated.relay(&[], &["--fail-start"])).await);
    let cause = refused.to_string();
    assert!(cause.contains("exit status: 7") && cause.contains("fixture refuses to start"), "the error states exit status and stderr: {cause}");
    let again = assert_err!(session(Agent::default(), isolated.relay(&[], &["--fail-start"])).await).to_string();
    assert!(again.contains("server is down"), "inside the backoff window the caller gets the stored cause: {again}");
    assert_eq!(assert_ok!(isolated.state().await)["daemon"]["servers"][0]["starts"], json!(1), "a second session inside the backoff window starts no process");
}

// --- [ROUTING]

#[tokio::test(flavor = "multi_thread")]
async fn oversized_response_returns_an_error() {
    let isolated = isolated();
    let running = assert_ok!(session(Agent::default(), isolated.relay(&[], &[])).await);
    let refused = assert_err!(running.peer().call_tool(call("sized", json!({ "bytes": 17 << 20 }))).await).to_string();
    assert!(refused.contains("exceeds"), "the caller is told the response was too large: {refused}");
    assert_eq!(text(&assert_ok!(running.peer().call_tool(call("echo", json!({ "text": "next" }))).await)), "next", "the session stays usable");
}

#[tokio::test(flavor = "multi_thread")]
async fn server_requests_progress_and_cancellation_reach_the_calling_session() {
    let isolated = isolated();
    let agent = |marker: &str| Agent { marker: marker.to_owned(), ..Agent::default() };
    let (left, right) = (agent("left"), agent("right"));
    let one = assert_ok!(session(left.clone(), isolated.relay(&[], &[])).await);
    let two = assert_ok!(session(right.clone(), isolated.relay(&[], &[])).await);
    let asked = || call("ask", json!({ "question": "who" }));
    assert!(text(&assert_ok!(one.peer().call_tool(asked()).await)).contains("left"), "elicitation reaches the session whose call is in flight");
    assert!(text(&assert_ok!(two.peer().call_tool(asked()).await)).contains("right"), "elicitation reaches the other session for its own call");
    assert_ok!(one.peer().call_tool(call("slow", json!({ "millis": 200, "steps": 4 }))).await);
    assert_eq!((left.progress.load(Ordering::Relaxed), right.progress.load(Ordering::Relaxed)), (4, 0), "progress reaches the calling session alone");
    let request = ClientRequest::CallToolRequest(CallToolRequest::new(call("slow", json!({ "millis": 60_000, "steps": 1 }))));
    let handle = assert_ok!(two.peer().send_cancellable_request(request, PeerRequestOptions::no_options()).await);
    time::sleep(Duration::from_millis(200)).await;
    assert_eq!(assert_ok!(isolated.state().await)["daemon"]["servers"][0]["calls"], json!(1), "the slow call is in flight");
    assert_ok!(handle.cancel(None).await);
    time::sleep(Duration::from_millis(300)).await;
    assert_eq!(assert_ok!(isolated.state().await)["daemon"]["servers"][0]["calls"], json!(0), "the cancelled call ends at the server");
}
