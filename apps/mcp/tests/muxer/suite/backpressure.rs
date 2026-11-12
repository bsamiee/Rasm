use core::future::ready;
use core::time::Duration;
use std::error::Report;

use claims::{assert_err, assert_ok, assert_some};
use futures::{StreamExt as _, stream};
use mcp_muxer_tests::{Agent, Isolated, Sized, Slow, call, exited, session};
use rmcp::model::{CallToolRequest, ClientRequest, NumberOrString, ProgressNotificationParam, ProgressToken};
use rmcp::service::PeerRequestOptions;
use sysinfo::{Pid, ProcessesToUpdate, Signal, System};
use tokio::time;
use tokio::time::error::Elapsed;

use crate::{daemon, echoed, scoped};

// --- [STALLED_SERVER]

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

// --- [OVERSIZED]

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
