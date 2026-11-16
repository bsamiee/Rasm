use claims::{assert_err, assert_ok};
use rasm_mcp_tests::{Recorder, everything, paired};
use rmcp::ServiceError;
use rmcp::model::{ErrorCode, ProtocolVersion, ReadResourceRequestParams};
use rmcp::service::ClientLifecycleMode;
use rstest::rstest;

// --- [NOT_FOUND]

#[rstest]
#[case::legacy_version_gets_resource_not_found_code(ProtocolVersion::V_2025_11_25, ClientLifecycleMode::Initialize, ErrorCode::RESOURCE_NOT_FOUND)]
#[case::sep_2164_version_gets_invalid_params_code(ProtocolVersion::V_2026_07_28, ClientLifecycleMode::Discover { preferred_versions: vec![ProtocolVersion::V_2026_07_28] }, ErrorCode::INVALID_PARAMS)]
#[tokio::test]
async fn not_found_code(#[case] version: ProtocolVersion, #[case] lifecycle: ClientLifecycleMode, #[case] code: ErrorCode) {
    let (shared, _app) = assert_ok!(everything().await);
    let (recorder, _events) = Recorder::new(version);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, lifecycle).await);
    let missing = assert_err!(running.peer().read_resource(ReadResourceRequestParams::new("missing://resource")).await, "missing resource should error");
    assert!(matches!(&missing, ServiceError::McpError(data) if data.code == code), "expected {code:?}, got: {missing:?}");
}
