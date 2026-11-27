use core::future::ready;
use core::time::Duration;
use std::io;
use std::path::{Path, PathBuf};

use claims::{assert_err, assert_ok, assert_some};
use futures::{StreamExt as _, future, stream};
use mcp_muxer_tests::{Agent, Down, Isolated, Slow, call, decoded, exited, reached, session, spawned};
use rasm_testing::{Failure, Folder};
use rmcp::model::{CallToolRequest, ClientRequest, NumberOrString, ProgressNotificationParam, ProgressToken, ProtocolVersion};
use rmcp::service::{ClientLifecycleMode, PeerRequestOptions};
use rstest::rstest;
use sysinfo::{Pid, ProcessStatus, ProcessesToUpdate, Signal, System};
use tokio::time::{self, Instant};

use crate::{HOME, IDLE, daemon, echoed, refusal, scoped, servers, settled, whoami};

// --- [REFUSAL]

#[tokio::test(flavor = "multi_thread")]
async fn runtime_folder_past_the_socket_limit_refuses_the_session_with_the_cause() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let mut command = isolated.relay(&[], &[]);
        command.env("MCP_MUXER_RUNTIME_DIR", isolated.folder(&"long".repeat(40)));
        let refused = assert_err!(session(Agent::default(), command).await);
        assert!(refusal(&refused).is_some_and(|answer| answer.message.contains("socket")), "a session no daemon can serve is answered with the cause and runs no server of its own: {refused:?}");
    })
    .await
}

// --- [REPLACEMENT]

#[tokio::test(flavor = "multi_thread")]
async fn relay_survives_daemon_loss() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let agent = Agent::default();
        let running = assert_ok!(session(agent.clone(), isolated.relay(&[], &[])).await);
        let before = whoami(&running, HOME).await;
        let lost = assert_some!(daemon(isolated, &[]).await);
        let slow = running.peer().call_tool(assert_ok!(call("slow", &Slow { millis: 5000, steps: 5 })));
        let killed = async {
            let held = settled(isolated, |servers| servers.iter().any(|served| served.calls == 1)).await;
            assert!(held.iter().any(|served| served.calls == 1), "the slow call is in flight before the daemon dies: {held:?}");
            let mut system = assert_ok!(System::new());
            system.refresh_processes(ProcessesToUpdate::Some(&[Pid::from_u32(lost)]), true);
            assert!(assert_some!(system.process(Pid::from_u32(lost))).kill(), "the daemon is killed mid-call");
        };
        let (answered, ()) = future::join(slow, killed).await;
        assert_err!(answered, "the call in flight is answered with an error under its own id");
        let after = whoami(&running, HOME).await;
        assert_ne!(before.pid, after.pid, "the next call reaches a server under a new daemon");
        assert_eq!(assert_ok!(reached(&agent.changes, 1).await), 1, "the session is told its tool list changed");
    })
    .await
}

#[tokio::test(flavor = "multi_thread")]
async fn newer_build_takes_over() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
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
    })
    .await
}

// --- [STARTS]

#[tokio::test(flavor = "multi_thread")]
async fn slow_start_waits_once_then_answers_from_the_stored_handshake() -> Result<(), Failure<Folder>> {
    const DELAY: Duration = Duration::from_secs(2);
    scoped(async |isolated: &Isolated| {
        let relay = || {
            let mut command = isolated.relay(&[], &["--start-delay", "2s"]);
            command.envs(IDLE);
            session(Agent::default(), command)
        };
        let started = Instant::now();
        let cold = assert_ok!(relay().await);
        assert!(started.elapsed() >= DELAY, "with no stored handshake the session waits for the server: {:?}", started.elapsed());
        assert_eq!(echoed(&cold, "cold").await, "cold", "the cold session is served");
        assert_eq!(Vec::from_iter(servers(isolated, &IDLE).await.iter().map(|served| served.starts)), [1], "the slow server started once");
        let first = assert_some!(daemon(isolated, &IDLE).await);
        assert_ok!(cold.cancel().await);
        assert_ok!(assert_ok!(time::timeout(Duration::from_secs(30), exited(first)).await, "the daemon exits once it holds nothing"));
        let reopened = Instant::now();
        let warm = assert_ok!(relay().await);
        assert!(reopened.elapsed() < DELAY / 2, "with a stored handshake the session is answered before the server could: {:?}", reopened.elapsed());
        assert_eq!(echoed(&warm, "warm").await, "warm", "the first call waits for the server");
        assert_eq!(Vec::from_iter(servers(isolated, &IDLE).await.iter().map(|served| served.starts)), [1], "the new daemon started the slow server once, with no restart");
    })
    .await
}

#[tokio::test(flavor = "multi_thread")]
async fn failed_start_reports_its_cause_and_backs_off() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let relay = |server: &[&str]| {
            let mut command = isolated.relay(&[], server);
            command.envs(IDLE);
            session(Agent::default(), command)
        };
        let attached = assert_ok!(relay(&["--speaks", "legacy"]).await);
        let down = |refused: &io::Error| {
            let data = assert_some!(refusal(refused).filter(|answer| answer.code.0 == -32603).and_then(|answer| answer.data.clone()), "an internal error carries its typed data: {refused:?}");
            assert_ok!(decoded::<Down>(&assert_ok!(serde_json::to_vec(&data))))
        };
        let ended = |stated: &Down| stated.cause.first().is_some_and(|end| end.status == "exit status: 7" && end.stderr.iter().any(|line| line.contains("fixture refuses to start")));
        let cause = assert_err!(relay(&["--fail-start"]).await);
        assert!(ended(&down(&cause)), "the error states exit status and stderr: {cause:?}");
        let again = assert_err!(relay(&["--fail-start"]).await);
        let stated = down(&again);
        assert!(stated.next == "retry" && !stated.sent, "inside the backoff window the caller gets the stored cause: {again:?}");
        let failing = async || servers(isolated, &IDLE).await.into_iter().find(|served| served.arguments == ["--fail-start"]).map(|served| (served.starts, served.failures));
        assert_eq!(failing().await, Some((2, 1)), "the unknown server started once per handshake order, and a second session inside the backoff window starts no process");
        let first = assert_some!(daemon(isolated, &IDLE).await);
        drop(attached);
        assert_ok!(assert_ok!(time::timeout(Duration::from_secs(30), exited(first)).await, "the daemon exits once it holds nothing"));
        let _held = assert_ok!(relay(&["--speaks", "legacy"]).await);
        let stored = assert_err!(relay(&["--fail-start"]).await);
        assert!(ended(&down(&stored)), "a new daemon answers from the stored failure: {stored:?}");
        let counted = failing().await;
        assert!(matches!(counted, Some((0, 1) | (2, 2))), "inside the stored window the new daemon starts no process, after it the stored failure count grows: {counted:?}");
    })
    .await
}

// --- [SHUTDOWN]

#[tokio::test(flavor = "multi_thread")]
async fn daemon_stops_past_a_server_that_stopped_reading() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let running = assert_ok!(session(Agent::default(), isolated.relay(&[], &["--speaks", "legacy"])).await);
        let held = assert_some!(daemon(isolated, &[]).await);
        let request = ClientRequest::CallToolRequest(CallToolRequest::new(assert_ok!(call("stall", &Slow { millis: 600_000, steps: 600 }))));
        let _stalled = assert_ok!(running.peer().send_cancellable_request(request, PeerRequestOptions::no_options()).await);
        let progress = ProgressNotificationParam::new(ProgressToken(NumberOrString::Number(0)), 0.0).with_message("x".repeat(4 << 10));
        let sends = stream::repeat(progress).then(async |update| time::timeout(Duration::from_secs(1), running.peer().notify_progress(update)).await.map(|sent| assert_ok!(sent)));
        sends.take_while(|sent| ready(sent.is_ok())).for_each(|_| ready(())).await;
        let mut system = assert_ok!(System::new());
        system.refresh_processes(ProcessesToUpdate::Some(&[Pid::from_u32(held)]), true);
        assert_eq!(assert_some!(system.process(Pid::from_u32(held))).kill_with(Signal::Term), Some(true), "the daemon is signalled");
        assert_ok!(assert_ok!(time::timeout(Duration::from_secs(10), exited(held)).await, "a signalled daemon stops though its server reads nothing"));
    })
    .await
}

#[tokio::test(flavor = "multi_thread")]
async fn server_that_ignores_end_of_input_ends_on_the_terminate_signal_after_its_session_left() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let running = assert_ok!(session(Agent::default(), isolated.relay(&["--per", "session"], &["--linger", "1m"])).await);
        let server = whoami(&running, HOME).await.pid;
        assert_ok!(running.cancel().await);
        assert_ok!(assert_ok!(time::timeout(Duration::from_secs(10), exited(server)).await, "a server that stays a minute past end of input ends on the signal that follows"));
    })
    .await
}

#[tokio::test(flavor = "multi_thread")]
async fn relay_alone_signalled_stops_its_server() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let (running, relay) = assert_ok!(spawned(Agent::default(), isolated.relay(&["--per", "session"], &[]), ClientLifecycleMode::Initialize).await);
        let (server, relay) = (whoami(&running, HOME).await.pid, Pid::from_u32(assert_some!(relay)));
        let mut system = assert_ok!(System::new());
        system.refresh_processes(ProcessesToUpdate::Some(&[relay]), true);
        assert_eq!(assert_some!(system.process(relay)).kill_with(Signal::Term), Some(true), "the relay alone is signalled, never its process group");
        assert_ok!(assert_ok!(time::timeout(Duration::from_secs(10), exited(server)).await, "the server ends with the relay that owned it"));
        system.refresh_processes(ProcessesToUpdate::Some(&[Pid::from_u32(server)]), true);
        assert!(system.process(Pid::from_u32(server)).is_none_or(|left| left.status() != ProcessStatus::Zombie), "no zombie stays behind");
    })
    .await
}

#[rstest]
#[case::initialize(ClientLifecycleMode::Initialize)]
#[case::discover_then_initialize(ClientLifecycleMode::Auto { preferred_versions: vec![ProtocolVersion::LATEST], legacy_version: Some(ProtocolVersion::LATEST_WITH_INITIALIZE) })]
#[tokio::test(flavor = "multi_thread")]
async fn program_missing_from_path_is_named_to_the_client_and_starts_once_found(#[case] lifecycle: ClientLifecycleMode) -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let relay = |path: PathBuf| {
            let mut command = isolated.serving(&[], Path::new("mcp-fixture"), &[]);
            command.env("PATH", path);
            spawned(Agent::default(), command, lifecycle.clone())
        };
        let refused = assert_err!(relay(isolated.folder("empty")).await);
        assert!(
            refusal(&refused).is_some_and(|answer| answer.message.contains("mcp-fixture") && answer.message.contains("PATH")),
            "the client's request is answered with the missing program: {refused:?}"
        );
        let (found, _) = assert_ok!(relay(isolated.root.clone()).await);
        assert_eq!(echoed(&found, "found").await, "found", "a later session whose PATH holds the program reaches its server");
    })
    .await
}
