use core::time::Duration;
use std::process::Stdio;

use claims::{assert_ok, assert_some};
use mcp_muxer_tests::{Agent, Isolated, exited, spawned};
use rmcp::service::{ClientLifecycleMode, ClientServiceExt as _};
use sysinfo::{Pid, ProcessStatus, ProcessesToUpdate, Signal, System};
use tokio::time::{self, Instant};

use crate::{HOME, echoed, scoped, whoami};

// --- [SHUTDOWN]

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

#[tokio::test(flavor = "multi_thread")]
async fn parent_only_sigterm_stops_its_server() {
    let passed = scoped(async |isolated: &Isolated| {
        let (running, relay) = assert_ok!(spawned(Agent::default(), isolated.relay(&["--per", "session"], &[]), ClientLifecycleMode::Initialize).await);
        let (server, relay) = (whoami(&running, HOME).await.pid, Pid::from_u32(assert_some!(relay)));
        let mut system = assert_ok!(System::new());
        system.refresh_processes(ProcessesToUpdate::Some(&[relay]), true);
        assert_eq!(assert_some!(system.process(relay)).kill_with(Signal::Term), Some(true), "the relay alone is signalled, never its process group");
        assert_ok!(assert_ok!(time::timeout(Duration::from_secs(10), exited(server)).await, "the server ends with the relay that owned it"));
        system.refresh_processes(ProcessesToUpdate::Some(&[Pid::from_u32(server)]), true);
        assert!(system.process(Pid::from_u32(server)).is_none_or(|left| left.status() != ProcessStatus::Zombie), "no zombie stays behind");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}
