use core::time::Duration;
use std::collections::BTreeMap;

use claims::{assert_err, assert_ok, assert_some};
use rasm_mcp::decode;
use rasm_mcp_tests::{Everything, Recorder, Seen, everything, paired};
use rmcp::model::{CallToolRequestParams, CallToolResult, JsonObject, ProtocolVersion};
use rmcp::service::ClientLifecycleMode;
use serde::{Deserialize, Serialize};
use serde_json::{Value, json};
use tokio::time;

use crate::seen;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Serialize)]
struct Named<'handle> {
    handle: &'handle str,
}

#[derive(Debug, Deserialize)]
struct Typed {
    #[serde(rename = "type")]
    kind: Option<String>,
}

#[derive(Debug, Deserialize)]
struct Declared {
    properties: BTreeMap<String, Typed>,
    #[serde(default)]
    required: Vec<String>,
}

#[derive(Debug, PartialEq, Eq, Deserialize)]
struct Refused {
    #[serde(rename = "_tag")]
    tag: String,
    handle: String,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn texts(result: &CallToolResult) -> Vec<&str> { Vec::from_iter(result.content.iter().filter_map(|block| block.as_text()).map(|text| text.text.as_str())) }

fn handled(handle: &str) -> CallToolRequestParams {
    let arguments = assert_ok!(serde_json::to_vec(&Named { handle }));
    CallToolRequestParams::new("test_handle").with_arguments(assert_ok!(decode::<JsonObject>(&arguments)))
}

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

#[tokio::test]
async fn storing_an_identical_tool_router_sends_no_tool_list_changed() {
    let (shared, app) = assert_ok!(everything().await);
    let (recorder, mut events) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Initialize).await);
    let before = assert_ok!(running.peer().list_tools(None).await).tools;
    assert_ok!(app.routers.tools(Everything::tool_router()));
    assert_eq!(assert_ok!(running.peer().list_tools(None).await).tools, before, "an identical tool router lists the same tools");
    app.routers.disable("greet");
    assert!(matches!(seen(&mut events).await, Seen::ToolsChanged), "the disable after the store is told");
    assert_err!(time::timeout(Duration::from_millis(200), events.recv()).await, "an identical tool router changes no listing, so the disable is told alone");
}

// --- [HANDLES]

#[tokio::test]
async fn every_tool_lists_one_optional_string_handle() {
    let (shared, _app) = assert_ok!(everything().await);
    let (recorder, _events) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Initialize).await);
    let tools = assert_ok!(running.peer().list_tools(None).await).tools;
    assert!(tools.len() > 1);
    for tool in tools {
        let Declared { properties, required } = assert_ok!(decode::<Declared>(&assert_ok!(serde_json::to_vec(&tool.input_schema))));
        assert_eq!(properties.get("handle").and_then(|handle| handle.kind.as_deref()), Some("string"), "{} lists handle as a string: {properties:?}", tool.name);
        assert!(!required.iter().any(|name| name == "handle"), "{} leaves handle optional: {required:?}", tool.name);
    }
}

#[tokio::test]
async fn initialize_era_calls_without_handle_share_their_connection_state() {
    let (shared, _app) = assert_ok!(everything().await);
    let (recorder, _events) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Initialize).await);
    let (other, _unrelated) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (bystander, _other) = assert_ok!(paired(shared.connection(), other, ClientLifecycleMode::Initialize).await);
    let first = assert_ok!(running.peer().call_tool(CallToolRequestParams::new("test_handle")).await);
    let second = assert_ok!(running.peer().call_tool(CallToolRequestParams::new("test_handle")).await);
    let elsewhere = assert_ok!(bystander.peer().call_tool(CallToolRequestParams::new("test_handle")).await);
    let [first] = assert_ok!(<[&str; 1]>::try_from(texts(&first)), "a session handle is named in no block");
    let (handle, calls) = assert_some!(first.split_once(' '));
    assert_eq!(calls, "1");
    assert_eq!(texts(&second), [format!("{handle} 2")], "the second call of a connection reads the state of its first");
    let [elsewhere] = assert_ok!(<[&str; 1]>::try_from(texts(&elsewhere)), "a session handle is named in no block");
    assert!(elsewhere.ends_with(" 1") && !elsewhere.starts_with(handle), "another connection holds its own state: {elsewhere}");
}

#[tokio::test]
async fn handle_argument_reaches_the_state_it_names_over_the_connection_state() {
    let (shared, _app) = assert_ok!(everything().await);
    let (recorder, _events) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Initialize).await);
    let (other, _unrelated) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (bystander, _other) = assert_ok!(paired(shared.connection(), other, ClientLifecycleMode::Initialize).await);
    let first = assert_ok!(running.peer().call_tool(CallToolRequestParams::new("test_handle")).await);
    let handle = assert_some!(assert_some!(texts(&first).first()).split_once(' ')).0.to_owned();
    let named = assert_ok!(bystander.peer().call_tool(handled(&handle)).await);
    assert_eq!(texts(&named), [format!("{handle} 2")], "a named handle reaches the state another connection built");
}

#[tokio::test]
async fn stateless_call_without_handle_mints_one_and_names_it_in_its_result() {
    let (shared, _app) = assert_ok!(everything().await);
    let (recorder, _events) = Recorder::new(ProtocolVersion::V_2026_07_28);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Discover { preferred_versions: vec![ProtocolVersion::V_2026_07_28] }).await);
    let minted = assert_ok!(running.peer().call_tool(CallToolRequestParams::new("test_handle")).await);
    let [counted, appended] = assert_ok!(<[&str; 2]>::try_from(texts(&minted)), "a minted handle is named in one block");
    let (handle, calls) = assert_some!(counted.split_once(' '));
    assert_eq!(calls, "1");
    assert_eq!(assert_ok!(decode::<Value>(appended.as_bytes())), json!({ "handle": handle }), "the result names the handle it minted");
    let named = assert_ok!(running.peer().call_tool(handled(handle)).await);
    assert_eq!(texts(&named), [format!("{handle} 2")], "a call naming its handle reaches its state and mints nothing");
    let fresh = assert_ok!(running.peer().call_tool(CallToolRequestParams::new("test_handle")).await);
    let [restarted, _] = assert_ok!(<[&str; 2]>::try_from(texts(&fresh)), "a minted handle is named in one block");
    assert!(restarted.ends_with(" 1") && !restarted.starts_with(handle), "a stateless call without handle starts new state: {restarted}");
}

#[tokio::test]
async fn unknown_handle_is_refused_by_name() {
    const UNKNOWN: &str = "1f0e7c52-8a4b-4d3e-9c61-2b5a7d904e38";
    let (shared, _app) = assert_ok!(everything().await);
    let (recorder, _events) = Recorder::new(ProtocolVersion::V_2026_07_28);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Discover { preferred_versions: vec![ProtocolVersion::V_2026_07_28] }).await);
    let refused = assert_ok!(running.peer().call_tool(handled(UNKNOWN)).await);
    assert_eq!(refused.is_error, Some(true));
    let structured = assert_ok!(decode::<Refused>(&assert_ok!(serde_json::to_vec(&assert_some!(refused.structured_content)))));
    assert_eq!(structured, Refused { tag: "Handle".to_owned(), handle: UNKNOWN.to_owned() });
}
