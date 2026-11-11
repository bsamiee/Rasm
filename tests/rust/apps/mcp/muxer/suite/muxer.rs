use core::future::ready;
use core::sync::atomic::Ordering;
use core::time::Duration;
use std::error::Report;
use std::io;
use std::path::PathBuf;
use std::process::Stdio;

use claims::{assert_err, assert_ok, assert_some};
use futures::{StreamExt as _, future, stream};
use mcp_muxer_tests::{Agent, Ask, Echo, Isolated, Served, Sized, Slow, Variable, Whoami, call, decoded, exited, modern, session, spawned, text};
use rmcp::RoleClient;
use rmcp::model::{CallToolRequest, ClientRequest, NumberOrString, ProgressNotificationParam, ProgressToken};
use rmcp::service::{ClientInitializeError, ClientLifecycleMode, ClientServiceExt as _, PeerRequestOptions, RunningService};
use sysinfo::{Pid, ProcessesToUpdate, Signal, System};
use tokio::time::error::Elapsed;
use tokio::time::{self, Instant};

use crate::scoped;

// --- [CONSTANTS] -----------------------------------------------------------------------

const IDLE: [(&str, &str); 1] = [("MCP_MUXER_IDLE", "1s")];
const HOME: &str = "HOME";

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn whoami(running: &RunningService<RoleClient, Agent>, name: &str) -> Whoami {
    let result = assert_ok!(running.peer().call_tool(assert_ok!(call("whoami", &Variable { name: name.to_owned() }))).await);
    assert_ok!(decoded(text(&result).as_bytes()))
}

async fn echoed(running: &RunningService<RoleClient, Agent>, said: &str) -> String { text(&assert_ok!(running.peer().call_tool(assert_ok!(call("echo", &Echo { text: said.to_owned() }))).await)) }

async fn daemon(isolated: &Isolated, settings: &[(&str, &str)]) -> Option<u32> { assert_ok!(isolated.state(settings).await).daemon.map(|running| running.pid) }

async fn servers(isolated: &Isolated, settings: &[(&str, &str)]) -> Vec<Served> { assert_ok!(isolated.state(settings).await).daemon.map_or_default(|running| running.servers) }

// --- [SHARING]

#[tokio::test(flavor = "multi_thread")]
async fn sessions_share_one_server_per_key() {
    let passed = scoped(async |isolated: &Isolated| {
        let trace = "TRACEPARENT";
        let relay = |options: &[&str]| {
            let mut command = isolated.relay(options, &[]);
            command.env(trace, "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01");
            session(Agent::default(), command)
        };
        let (first, second, tagged, other, own) = (
            assert_ok!(relay(&[]).await),
            assert_ok!(relay(&[]).await),
            assert_ok!(relay(&["--env", "FIXTURE_TAG=one"]).await),
            assert_ok!(relay(&["--env", "FIXTURE_TAG=two"]).await),
            assert_ok!(relay(&["--per", "session"]).await),
        );
        let direct = assert_ok!(session(Agent::default(), isolated.direct(&[])).await);
        let (shared, same, one, two, private, unshared) =
            (whoami(&first, HOME).await, whoami(&second, HOME).await, whoami(&tagged, "FIXTURE_TAG").await, whoami(&other, "FIXTURE_TAG").await, whoami(&own, HOME).await, whoami(&direct, HOME).await);
        assert_eq!(shared.pid, same.pid, "two sessions with one key reach one server process");
        assert_eq!(shared.value, unshared.value, "the server receives the session environment an unshared run receives");
        assert_eq!(shared.directory, unshared.directory, "the server runs in the session working directory");
        assert_eq!((one.value.as_deref(), two.value.as_deref()), (Some("one"), Some("two")), "declared variables reach the server");
        assert_ne!(one.pid, two.pid, "sessions with differing declared values never share");
        assert_ne!(shared.pid, private.pid, "a per-session server is not shared");
        assert_eq!((whoami(&first, trace).await.value, whoami(&own, trace).await.value.is_some()), (None, true), "trace context reaches a per-session server and no shared one");
        let entries = |folder: PathBuf| assert_ok!(fs_err::read_dir(folder)).count();
        let state = assert_ok!(isolated.state(&[]).await);
        assert_eq!(state.cache, isolated.folder("cache"), "the state report names the overridden cache folder");
        assert!(entries(isolated.folder("cache")) >= 1, "stored handshakes sit in the overridden cache folder");
        assert!(entries(isolated.folder("logs")) >= 1, "log files sit in the overridden log folder");
        assert!(entries(isolated.folder("r")) >= 2, "socket and lock sit in the overridden runtime folder");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread")]
async fn racing_first_sessions_all_share() {
    let passed = scoped(async |isolated: &Isolated| {
        for round in 0..100 {
            let relay = || {
                let mut command = isolated.relay(&[], &[]);
                command.env("MCP_MUXER_RUNTIME_DIR", format!("r{round}")).env("MCP_MUXER_IDLE", "5s");
                session(Agent::default(), command)
            };
            let (first, second) = future::join(relay(), relay()).await;
            let (first, second) = (assert_ok!(first), assert_ok!(second));
            assert_eq!(whoami(&first, HOME).await.pid, whoami(&second, HOME).await.pid, "round {round}: neither racing session fell back to an unshared server");
        }
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread")]
async fn runtime_folder_past_the_socket_limit_runs_the_server_unshared() {
    let passed = scoped(async |isolated: &Isolated| {
        let mut command = isolated.relay(&[], &[]);
        command.env("MCP_MUXER_RUNTIME_DIR", isolated.folder(&"long".repeat(40)));
        let running = assert_ok!(session(Agent::default(), command).await);
        assert_eq!(echoed(&running, "alone").await, "alone", "a session no daemon can serve runs its own server");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread")]
async fn unshared_server_ends_with_its_session() {
    let passed = scoped(async |isolated: &Isolated| {
        for (server, ignored) in [([].as_slice(), false), (["--linger", "1m"].as_slice(), true)] {
            let mut command = isolated.relay(&[], server);
            command.env("MCP_MUXER_RUNTIME_DIR", isolated.folder(&"long".repeat(40))).stdin(Stdio::piped()).stdout(Stdio::piped()).stderr(Stdio::null()).kill_on_drop(true);
            let mut relay = assert_ok!(command.spawn());
            let transport = (assert_some!(relay.stdout.take()), assert_some!(relay.stdin.take()));
            let running = assert_ok!(Agent::default().serve_with_lifecycle(transport, ClientLifecycleMode::Initialize).await);
            assert_eq!(echoed(&running, "alone").await, "alone");
            let closed = Instant::now();
            assert_ok!(running.cancel().await);
            let status = assert_ok!(assert_ok!(time::timeout(Duration::from_secs(10), relay.wait()).await, "the relay ends once its session closed standard input: {server:?}"));
            assert!(status.success(), "the server ends on end of input, or on the terminate signal that follows when it ignores end of input: {server:?} {status}");
            assert_eq!(closed.elapsed() >= Duration::from_secs(3), ignored, "a server is signalled only after it ignored end of input for the grace period: {server:?}");
        }
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

// --- [RESILIENCE]

#[tokio::test(flavor = "multi_thread")]
async fn daemon_stops_past_a_server_that_stopped_reading() {
    let passed = scoped(async |isolated: &Isolated| {
        let running = assert_ok!(session(Agent::default(), isolated.relay(&[], &["--speaks", "legacy"])).await);
        let held = assert_some!(daemon(isolated, &[]).await);
        let request = ClientRequest::CallToolRequest(CallToolRequest::new(assert_ok!(call("stall", &Slow { millis: 600_000, steps: 600 }))));
        let _stalled = assert_ok!(running.peer().send_cancellable_request(request, PeerRequestOptions::no_options()).await);
        let update = ProgressNotificationParam::new(ProgressToken(NumberOrString::Number(0)), 0.0).with_message("x".repeat(4 << 10));
        let sends = stream::repeat(update).then(async |update| time::timeout(Duration::from_secs(1), running.peer().notify_progress(update)).await.map(|sent| assert_ok!(sent)));
        let taken = sends.take_while(|sent| {
            ready(match sent {
                Ok(()) => true,
                Err(Elapsed { .. }) => false,
            })
        });
        let backed = taken.count().await;
        assert!(backed > 256, "messages back up behind the server that reads none: {backed}");
        let mut system = assert_ok!(System::new());
        system.refresh_processes(ProcessesToUpdate::Some(&[Pid::from_u32(held)]), true);
        assert_eq!(assert_some!(system.process(Pid::from_u32(held))).kill_with(Signal::Term), Some(true), "the daemon is signalled");
        assert_ok!(assert_ok!(time::timeout(Duration::from_secs(10), exited(held)).await, "a signalled daemon stops though its server reads nothing"));
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread")]
async fn relay_survives_daemon_loss() {
    let passed = scoped(async |isolated: &Isolated| {
        let agent = Agent::default();
        let running = assert_ok!(session(agent.clone(), isolated.relay(&[], &[])).await);
        let before = whoami(&running, HOME).await;
        let lost = assert_some!(daemon(isolated, &[]).await);
        let slow = running.peer().call_tool(assert_ok!(call("slow", &Slow { millis: 5000, steps: 5 })));
        let killed = async {
            time::sleep(Duration::from_millis(300)).await;
            let mut system = assert_ok!(System::new());
            system.refresh_processes(ProcessesToUpdate::Some(&[Pid::from_u32(lost)]), true);
            assert!(assert_some!(system.process(Pid::from_u32(lost))).kill(), "the daemon is killed mid-call");
        };
        let (answered, ()) = future::join(slow, killed).await;
        assert_err!(answered, "the call in flight is answered with an error under its own id");
        let after = whoami(&running, HOME).await;
        assert_ne!(before.pid, after.pid, "the next call reaches a server under a new daemon");
        assert_eq!(agent.changes.load(Ordering::Relaxed), 1, "the session is told its tool list changed");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread")]
async fn newer_build_takes_over() {
    let passed = scoped(async |isolated: &Isolated| {
        let installed = isolated.root.join("installed");
        assert_ok!(fs_err::tokio::copy(&isolated.muxer, &installed).await);
        let relay = || {
            let command = isolated.relay(&[], &[]);
            let mut rebuilt = tokio::process::Command::new(&installed);
            rebuilt.args(command.as_std().get_args()).envs(command.as_std().get_envs().filter_map(|(name, value)| Some((name, value?)))).current_dir(&isolated.root);
            session(Agent::default(), rebuilt)
        };
        let old = assert_ok!(relay().await);
        let (before, first) = (whoami(&old, HOME).await, daemon(isolated, &[]).await);
        time::sleep(Duration::from_millis(50)).await;
        let staged = installed.with_extension("new");
        assert_ok!(fs_err::tokio::write(&staged, assert_ok!(fs_err::tokio::read(&isolated.muxer).await)).await);
        assert_ok!(fs_err::tokio::set_permissions(&staged, assert_ok!(fs_err::tokio::metadata(&isolated.muxer).await).permissions()).await);
        assert_ok!(fs_err::tokio::rename(&staged, &installed).await);
        let new = assert_ok!(relay().await);
        let after = whoami(&new, HOME).await;
        assert_ne!(first, daemon(isolated, &[]).await, "a relay from a newer file replaces the daemon");
        assert_eq!(whoami(&old, HOME).await.pid, after.pid, "the old session keeps working and shares the new daemon's server");
        assert_ne!(before.pid, after.pid, "the old daemon's server stopped with it");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread")]
async fn slow_start_waits_once_then_answers_from_the_stored_handshake() {
    let passed = scoped(async |isolated: &Isolated| {
        let relay = || {
            let mut command = isolated.relay(&[], &["--start-delay", "25s"]);
            command.envs(IDLE);
            session(Agent::default(), command)
        };
        let started = Instant::now();
        let cold = assert_ok!(relay().await);
        assert!(started.elapsed() >= Duration::from_secs(24), "with no stored handshake the session waits for the server");
        assert_eq!(echoed(&cold, "cold").await, "cold");
        assert_eq!(Vec::from_iter(servers(isolated, &IDLE).await.iter().map(|served| served.starts)), [1], "the slow server started once");
        let first = assert_some!(daemon(isolated, &IDLE).await);
        assert_ok!(cold.cancel().await);
        assert_ok!(assert_ok!(time::timeout(Duration::from_secs(30), exited(first)).await, "the daemon exits once it holds nothing"));
        let started = Instant::now();
        let warm = assert_ok!(relay().await);
        assert!(started.elapsed() < Duration::from_secs(5), "with a stored handshake the session is answered at once");
        assert_eq!(echoed(&warm, "warm").await, "warm", "the first call waits for the server");
        assert_eq!(Vec::from_iter(servers(isolated, &IDLE).await.iter().map(|served| served.starts)), [1], "the slow server started once more, with no restart");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread")]
async fn failed_start_reports_its_cause_and_backs_off() {
    let passed = scoped(async |isolated: &Isolated| {
        let relay = |server: &[&str]| {
            let mut command = isolated.relay(&[], server);
            command.envs(IDLE);
            session(Agent::default(), command)
        };
        let attached = assert_ok!(relay(&["--speaks", "legacy"]).await);
        let states = |refused: &io::Error, texts: &[&str]| match refused.get_ref().and_then(|inner| inner.downcast_ref::<ClientInitializeError>()) {
            Some(ClientInitializeError::JsonRpcError(answered)) => texts.iter().all(|text| answered.message.contains(text)),
            Some(_) | None => false,
        };
        let cause = assert_err!(relay(&["--fail-start"]).await);
        assert!(states(&cause, &["exit status: 7", "fixture refuses to start"]), "the error states exit status and stderr: {cause:?}");
        let again = assert_err!(relay(&["--fail-start"]).await);
        assert!(states(&again, &["server is down"]), "inside the backoff window the caller gets the stored cause: {again:?}");
        let failing = async || servers(isolated, &IDLE).await.into_iter().find(|served| served.arguments == ["--fail-start"]).map(|served| (served.starts, served.failures));
        assert_eq!(failing().await, Some((2, 1)), "the unknown server started once per handshake order, and a second session inside the backoff window starts no process");
        let first = assert_some!(daemon(isolated, &IDLE).await);
        drop(attached);
        assert_ok!(assert_ok!(time::timeout(Duration::from_secs(30), exited(first)).await, "the daemon exits once it holds nothing"));
        let _held = assert_ok!(relay(&["--speaks", "legacy"]).await);
        let stored = assert_err!(relay(&["--fail-start"]).await);
        assert!(states(&stored, &["server is down", "fixture refuses to start"]), "a new daemon answers from the stored failure: {stored:?}");
        let counted = failing().await;
        assert!(matches!(counted, Some((0, 1) | (2, 2))), "inside the stored window the new daemon starts no process, after it the stored failure count grows: {counted:?}");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread")]
async fn sessions_of_either_era_reach_servers_of_either_era() {
    let passed = scoped(async |isolated: &Isolated| {
        let agent = |marker: &str| Agent { marker: marker.to_owned(), ..Agent::default() };
        for (server, served) in [([].as_slice(), "modern"), (["--speaks", "legacy"].as_slice(), "legacy")] {
            let (old, new) = (agent("old"), agent("new"));
            let (legacy, _) = assert_ok!(spawned(old.clone(), isolated.relay(&[], server), ClientLifecycleMode::Initialize).await);
            let (current, _) = assert_ok!(spawned(new.clone(), isolated.relay(&[], server), modern()).await);
            assert_eq!(echoed(&legacy, "one").await, "one", "{served} server: a session that sends initialize is answered");
            assert_eq!(echoed(&current, "two").await, "two", "{served} server: a session that sends server/discover is answered");
            let asked = || call("ask", &Ask { question: "who".to_owned() });
            assert_eq!(text(&assert_ok!(legacy.call_tool(assert_ok!(asked())).await)), "old", "{served} server: the initialize session answers its own input request");
            assert_eq!(text(&assert_ok!(current.call_tool(assert_ok!(asked())).await)), "new", "{served} server: the discover session answers its own input request");
            assert_ok!(legacy.call_tool(assert_ok!(call("slow", &Slow { millis: 100, steps: 2 }))).await);
            assert_eq!(old.progress.load(Ordering::Relaxed), 2, "{served} server: progress reaches the initialize session");
            let eras = Vec::from_iter(servers(isolated, &[]).await.into_iter().filter_map(|held| held.era));
            assert!(eras.iter().any(|era| era == served), "the state report names a {served} server: {eras:?}");
            assert_eq!(whoami(&legacy, HOME).await.pid, whoami(&current, HOME).await.pid, "{served} server: sessions of both eras share one server process");
        }
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

// --- [ROUTING]

#[tokio::test(flavor = "multi_thread")]
async fn oversized_response_returns_an_error() {
    let passed = scoped(async |isolated: &Isolated| {
        let running = assert_ok!(session(Agent::default(), isolated.relay(&[], &[])).await);
        let refused = assert_err!(running.peer().call_tool(assert_ok!(call("sized", &Sized { bytes: 17 << 20 }))).await);
        assert!(Report::new(&refused).to_string().contains("exceeds"), "the caller is told the response was too large: {refused:?}");
        assert_eq!(echoed(&running, "next").await, "next", "the session stays usable");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread")]
async fn server_requests_progress_and_cancellation_reach_the_calling_session() {
    let passed = scoped(async |isolated: &Isolated| {
        let agent = |marker: &str| Agent { marker: marker.to_owned(), ..Agent::default() };
        let (left, right) = (agent("left"), agent("right"));
        let one = assert_ok!(session(left.clone(), isolated.relay(&[], &[])).await);
        let two = assert_ok!(session(right.clone(), isolated.relay(&[], &[])).await);
        let asked = || call("ask", &Ask { question: "who".to_owned() });
        assert_eq!(text(&assert_ok!(one.peer().call_tool(assert_ok!(asked())).await)), "left", "elicitation reaches the session whose call is in flight");
        assert_eq!(text(&assert_ok!(two.peer().call_tool(assert_ok!(asked())).await)), "right", "elicitation reaches the other session for its own call");
        assert_ok!(one.peer().call_tool(assert_ok!(call("slow", &Slow { millis: 200, steps: 4 }))).await);
        assert_eq!((left.progress.load(Ordering::Relaxed), right.progress.load(Ordering::Relaxed)), (4, 0), "progress reaches the calling session alone");
        let request = ClientRequest::CallToolRequest(CallToolRequest::new(assert_ok!(call("slow", &Slow { millis: 60_000, steps: 1 }))));
        let handle = assert_ok!(two.peer().send_cancellable_request(request, PeerRequestOptions::no_options()).await);
        time::sleep(Duration::from_millis(200)).await;
        assert_eq!(Vec::from_iter(servers(isolated, &[]).await.iter().map(|served| served.calls)), [1], "the slow call is in flight");
        assert_ok!(handle.cancel(None).await);
        time::sleep(Duration::from_millis(300)).await;
        assert_eq!(Vec::from_iter(servers(isolated, &[]).await.iter().map(|served| served.calls)), [0], "the cancelled call ends at the server");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}
