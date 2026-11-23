use core::time::Duration;

use claims::{assert_ok, assert_some};
use mcp_muxer_tests::{Agent, Isolated, exited, session, spawned};
use rasm_testing::{Failure, Folder};
use rmcp::service::ClientLifecycleMode;
use sysinfo::{Pid, ProcessStatus, ProcessesToUpdate, Signal, System};
use tokio::time;

use crate::{HOME, scoped, whoami};

// --- [SHUTDOWN]

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
async fn parent_only_sigterm_stops_its_server() -> Result<(), Failure<Folder>> {
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
