use claims::assert_ok;
use rasm_mcp_tests::{Recorder, Seen, everything, paired};
use rmcp::model::ProtocolVersion;
use rmcp::service::ClientLifecycleMode;

use crate::seen;

// --- [LIST_CHANGED]

#[tokio::test]
async fn disable_enable_sends_tool_list_changed() {
    let (shared, app) = assert_ok!(everything().await);
    let (recorder, mut events) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Initialize).await);
    let listed = async || Vec::from_iter(assert_ok!(running.peer().list_tools(None).await).tools.into_iter().map(|tool| tool.name.into_owned()));
    let all = listed().await;
    assert!(all.iter().any(|name| name == "greet"));
    app.routers.disable("greet");
    assert!(matches!(seen(&mut events).await, Seen::ToolsChanged), "disabling a tool notifies the client");
    assert_eq!(listed().await, Vec::from_iter(all.iter().filter(|name| *name != "greet").cloned()), "a disabled tool leaves the list");
    app.routers.enable("greet");
    assert!(matches!(seen(&mut events).await, Seen::ToolsChanged), "enabling a tool notifies the client");
    assert_eq!(listed().await, all, "an enabled tool returns to the list");
}
