use core::time::Duration;

use claims::assert_ok;
use rasm_mcp_tests::{Recorder, Seen, everything, paired};
use rmcp::model::{CallToolRequest, CallToolRequestParams, ClientRequest, NumberOrString, ProgressToken, ProtocolVersion, RequestMetaObject};
use rmcp::service::{ClientLifecycleMode, PeerRequestOptions};
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
