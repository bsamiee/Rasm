use std::path::Path;

use claims::assert_ok;
use rasm_mcp_tests::{conformed, everything, served};
use rasm_testing::{Fixture as _, Folder};
use rstest::rstest;

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(test)]
async fn conforms(scenario: &str, revision: &str) {
    let passed = Folder::scoped(Path::new(env!("CARGO_TARGET_TMPDIR")), async |folder: &Folder| {
        let (shared, _app) = assert_ok!(everything().await);
        let (url, _stop) = assert_ok!(served(move || shared.connection()).await);
        let baseline = Path::new(concat!(env!("CARGO_MANIFEST_DIR"), "/expected-failures.yaml"));
        let outcome = assert_ok!(conformed(&url, scenario, revision, baseline, folder.path()).await);
        assert_eq!((outcome.code, outcome.checks.is_empty()), (Some(0), false), "{scenario} at {revision}:\n{}", outcome.output);
    });
    assert_ok!(passed.await, "a failed scenario keeps its results folder");
}

// --- [REVISIONS]

#[rstest]
#[tokio::test(flavor = "multi_thread")]
async fn revision_2025_11_25(
    #[values(
        "server-initialize",
        "logging-set-level",
        "ping",
        "completion-complete",
        "tools-list",
        "tools-call-simple-text",
        "tools-call-image",
        "tools-call-audio",
        "tools-call-embedded-resource",
        "tools-call-mixed-content",
        "tools-call-with-logging",
        "tools-call-error",
        "tools-call-with-progress",
        "tools-call-sampling",
        "tools-call-elicitation",
        "elicitation-sep1034-defaults",
        "server-sse-multiple-streams",
        "elicitation-sep1330-enums",
        "resources-list",
        "resources-read-text",
        "resources-read-binary",
        "resources-templates-read",
        "resources-subscribe",
        "resources-unsubscribe",
        "prompts-list",
        "prompts-get-simple",
        "prompts-get-with-args",
        "prompts-get-embedded-resource",
        "prompts-get-with-image",
        "dns-rebinding-protection",
        "server-session-lifecycle"
    )]
    scenario: &str,
) {
    conforms(scenario, "2025-11-25").await;
}

#[rstest]
#[tokio::test(flavor = "multi_thread")]
async fn revision_2026_07_28(
    #[values(
        "server-stateless",
        "completion-complete",
        "tools-list",
        "tools-call-simple-text",
        "tools-call-image",
        "tools-call-audio",
        "tools-call-embedded-resource",
        "tools-call-mixed-content",
        "tools-call-error",
        "tools-call-with-progress",
        "server-sse-multiple-streams",
        "resources-list",
        "resources-read-text",
        "resources-read-binary",
        "resources-templates-read",
        "sep-2164-resource-not-found",
        "prompts-list",
        "prompts-get-simple",
        "prompts-get-with-args",
        "prompts-get-embedded-resource",
        "prompts-get-with-image",
        "dns-rebinding-protection",
        "caching"
    )]
    scenario: &str,
) {
    conforms(scenario, "2026-07-28").await;
}
