use core::time::Duration;
use std::io;
use std::path::{Path, PathBuf};

use claims::{assert_err, assert_ok, assert_some};
use futures::future;
use mcp_muxer_tests::{Agent, Isolated, Slow, call, exited, reached, session, spawned};
use rasm_testing::{Failure, Folder};
use rmcp::model::ProtocolVersion;
use rmcp::service::{ClientInitializeError, ClientLifecycleMode};
use rstest::rstest;
use sysinfo::{Pid, ProcessesToUpdate, System};
use tokio::time::{self, Instant};

use crate::{HOME, IDLE, daemon, echoed, scoped, servers, settled, whoami};

// --- [FALLBACK]

#[tokio::test(flavor = "multi_thread")]
async fn runtime_folder_past_the_socket_limit_runs_the_server_unshared() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let mut command = isolated.relay(&[], &[]);
        command.env("MCP_MUXER_RUNTIME_DIR", isolated.folder(&"long".repeat(40)));
        let running = assert_ok!(session(Agent::default(), command).await);
        assert_eq!(echoed(&running, "alone").await, "alone", "a session no daemon can serve runs its own server");
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
    const DELAY: Duration = Duration::from_secs(4);
    scoped(async |isolated: &Isolated| {
        let relay = || {
            let mut command = isolated.relay(&[], &["--start-delay", "4s"]);
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
    })
    .await
}

#[rstest]
#[case::initialize(ClientLifecycleMode::Initialize)]
#[case::discover_then_initialize(ClientLifecycleMode::Auto { preferred_versions: vec![ProtocolVersion::LATEST], legacy_version: Some(ProtocolVersion::LATEST_WITH_INITIALIZE) })]
#[tokio::test(flavor = "multi_thread")]
async fn program_missing_from_path_is_named_to_the_client_and_starts_once_found(#[case] lifecycle: ClientLifecycleMode) -> Result<(), Failure<Folder>> {
    fn answered(refused: &ClientInitializeError) -> Option<&str> {
        match refused {
            ClientInitializeError::JsonRpcError(answer) => Some(&answer.message),
            ClientInitializeError::LegacyFallbackFailed { discover: _, fallback } => answered(fallback),
            _ => None,
        }
    }
    scoped(async |isolated: &Isolated| {
        let relay = |path: PathBuf| {
            let mut command = isolated.serving(Some(&[]), Path::new("mcp-fixture"), &[]);
            command.env("PATH", path);
            spawned(Agent::default(), command, lifecycle.clone())
        };
        let refused = assert_err!(relay(isolated.folder("empty")).await);
        let message = refused.get_ref().and_then(|inner| inner.downcast_ref::<ClientInitializeError>()).and_then(answered);
        assert!(message.is_some_and(|message| message.contains("mcp-fixture") && message.contains("PATH")), "the client's request is answered with the missing program: {refused:?}");
        let (found, _) = assert_ok!(relay(isolated.root.clone()).await);
        assert_eq!(echoed(&found, "found").await, "found", "a later session whose PATH holds the program reaches its server");
    })
    .await
}
