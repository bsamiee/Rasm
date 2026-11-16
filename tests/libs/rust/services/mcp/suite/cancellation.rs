use core::time::Duration;

use claims::{assert_err, assert_ok, assert_some};
use rasm_mcp_tests::{Id, Recorder, everything, paired, wired};
use rmcp::model::{CallToolRequestParams, ProtocolVersion};
use rmcp::service::ClientLifecycleMode;
use tokio::time;

// --- [RESPONSES]

#[tokio::test]
async fn cancelled_request_receives_no_response() {
    let (shared, app) = assert_ok!(everything().await);
    let (mut wire, _serving) = wired(shared.connection());
    assert_ok!(wire.send(r#"{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"raw","version":"1.0.0"}}}"#).await);
    assert_eq!(assert_some!(assert_ok!(wire.next().await)).id, Some(Id::Number(1)));
    assert_ok!(wire.send(r#"{"jsonrpc":"2.0","method":"notifications/initialized"}"#).await);
    assert_ok!(wire.send(r#"{"jsonrpc":"2.0","id":7,"method":"tools/call","params":{"name":"test_held","arguments":{}}}"#).await);
    assert_ok!(assert_ok!(time::timeout(Duration::from_secs(5), app.entered.acquire()).await, "the call is in flight")).forget();
    assert_ok!(wire.send(r#"{"jsonrpc":"2.0","method":"notifications/cancelled","params":{"requestId":7,"reason":"test"}}"#).await);
    assert_ok!(wire.send(r#"{"jsonrpc":"2.0","id":8,"method":"ping"}"#).await);
    let next = assert_some!(assert_ok!(wire.next().await));
    assert_eq!(next.id, Some(Id::Number(8)), "a receiver sends no response for a request it was told to cancel: {next:?}");
    assert_ok!(assert_ok!(time::timeout(Duration::from_secs(5), app.cancelled.acquire()).await, "a receiver stops processing the request it was told to cancel")).forget();
}

#[tokio::test]
async fn abandoned_tool_call_is_cancelled() {
    let (shared, app) = assert_ok!(everything().await);
    let (recorder, _events) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Initialize).await);
    let abandoned = time::timeout(Duration::from_millis(200), running.peer().call_tool(CallToolRequestParams::new("test_held"))).await;
    assert_err!(abandoned, "the held call outlives the wait");
    assert_ok!(assert_ok!(time::timeout(Duration::from_secs(5), app.entered.acquire()).await)).forget();
    assert_ok!(assert_ok!(time::timeout(Duration::from_secs(5), app.cancelled.acquire()).await, "dropping the call future cancels the request at the server")).forget();
}
