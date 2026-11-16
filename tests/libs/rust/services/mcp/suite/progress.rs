use core::time::Duration;

use claims::{assert_ok, assert_some};
use rasm_mcp_tests::{Id, Recorder, Seen, everything, paired, wired};
use rmcp::model::{CallToolRequest, CallToolRequestParams, ClientRequest, NumberOrString, ProgressToken, ProtocolVersion, RequestMetaObject};
use rmcp::service::{ClientLifecycleMode, PeerRequestOptions};
use serde_json::Number;
use tokio::time;

// --- [ROUTING]

#[tokio::test]
async fn progress_subscriber() {
    let (shared, _app) = assert_ok!(everything().await);
    let (recorder, mut events) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Initialize).await);
    let token = ProgressToken(NumberOrString::String("progress-test-1".into()));
    let mut meta = RequestMetaObject::new();
    meta.set_progress_token(token.clone());
    let request = ClientRequest::CallToolRequest(CallToolRequest::new(CallToolRequestParams::new("test_tool_with_progress")));
    let handle = assert_ok!(running.peer().send_cancellable_request(request, PeerRequestOptions::no_options().with_meta(meta)).await);
    let sent = handle.progress_token.clone();
    assert_ok!(handle.await_response().await);
    time::sleep(Duration::from_millis(200)).await;
    let mut arrived = Vec::new();
    assert_ok!(time::timeout(Duration::from_secs(5), events.recv_many(&mut arrived, 16)).await, "progress reaches the caller");
    let reported = Vec::from_iter(arrived.into_iter().filter_map(|seen| match seen {
        Seen::Progress(progress) => Some((progress.progress_token, progress.progress, progress.total)),
        Seen::Logged(_) | Seen::Updated(_) | Seen::Cancelled(_) | Seen::Elicited(_) | Seen::Sampled(_) | Seen::ToolsChanged | Seen::PromptsChanged | Seen::ResourcesChanged => None,
    }));
    assert_eq!(reported, [(sent.clone(), 0.0, Some(100.0)), (sent.clone(), 50.0, Some(100.0)), (sent, 100.0, Some(100.0))], "each progress value reaches the request that set the token, in order");
}

// --- [ORDER]

#[tokio::test(start_paused = true)]
async fn progress_reported_as_a_tool_returns_is_written_before_its_result() {
    let (shared, _app) = assert_ok!(everything().await);
    let (mut wire, _serving) = wired(shared.connection());
    assert_ok!(wire.send(r#"{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"raw","version":"1.0.0"}}}"#).await);
    assert_eq!(assert_some!(assert_ok!(wire.next().await)).id, Some(Id::Number(1)));
    assert_ok!(wire.send(r#"{"jsonrpc":"2.0","method":"notifications/initialized"}"#).await);
    assert_ok!(wire.send(r#"{"jsonrpc":"2.0","id":7,"method":"tools/call","params":{"name":"test_tool_with_progress","arguments":{},"_meta":{"progressToken":"p"}}}"#).await);
    let mut written = Vec::new();
    loop {
        let message = assert_some!(assert_ok!(wire.next().await));
        written.push(message.params.and_then(|params| params.progress).as_ref().and_then(Number::as_f64));
        if message.id == Some(Id::Number(7)) {
            break;
        }
    }
    assert_eq!(written, [Some(0.0), Some(50.0), Some(100.0), None], "each value a tool reports is written before its result, one reported as the tool returns included");
}
