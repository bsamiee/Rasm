use core::time::Duration;

use claims::{assert_err, assert_ok};
use rasm_mcp_tests::{Recorder, Seen, everything, paired};
use rmcp::model::{CallToolRequestParams, LoggingLevel, ProtocolVersion, SetLevelRequestParams};
use rmcp::service::ClientLifecycleMode;
use tokio::time;

use crate::seen;

// --- [LEVEL]

#[tokio::test]
async fn log_messages_follow_the_level_the_client_set() {
    let (shared, _app) = assert_ok!(everything().await);
    let (recorder, mut events) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Initialize).await);
    assert_ok!(running.peer().set_level(SetLevelRequestParams::new(LoggingLevel::Error)).await);
    assert_ok!(running.peer().call_tool(CallToolRequestParams::new("test_tool_with_logging")).await);
    assert_err!(time::timeout(Duration::from_millis(200), events.recv()).await, "messages below the level the client set are not sent");
    assert_ok!(running.peer().set_level(SetLevelRequestParams::new(LoggingLevel::Debug)).await);
    assert_ok!(running.peer().call_tool(CallToolRequestParams::new("test_tool_with_logging")).await);
    for _ in 0..3 {
        assert!(matches!(seen(&mut events).await, Seen::Logged(logged) if logged.level == LoggingLevel::Info), "messages at or above the level reach the client");
    }
}
