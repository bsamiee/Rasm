use std::collections::BTreeSet;

use claims::{assert_ok, assert_some};
use rasm_mcp_tests::{Answered, Everything, Id, Message, Transcript, everything, replies, wired};
use rstest::rstest;

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn replayed(transcript: Transcript) -> Vec<Message> {
    let (shared, _app) = assert_ok!(everything().await);
    let (mut wire, _serving) = wired(shared.connection());
    let seen = assert_ok!(wire.replayed(transcript.sent()).await);
    assert_eq!(replies(&seen), transcript.replies(), "{transcript:?}: each request is answered once under its own id, and no notification is answered: {seen:#?}");
    seen
}

fn answered(seen: &[Message], id: u64, nth: usize) -> &Answered {
    let results = seen.iter().filter(|message| message.id == Some(Id::Number(id))).filter_map(|message| message.result.as_ref());
    assert_some!(results.into_iter().nth(nth))
}

// --- [TRANSCRIPTS]

#[tokio::test(start_paused = true)]
async fn request_before_initialize_is_refused_and_ping_answers_empty() {
    let seen = replayed(Transcript::Lifecycle).await;
    assert_eq!(answered(&seen, 1, 0).protocol_version.as_deref(), Some("2024-11-05"), "initialize negotiates the version the client asked for");
    assert_eq!(answered(&seen, 1, 1), &Answered::default(), "ping returns an empty result");
    let listed = BTreeSet::from_iter(assert_some!(answered(&seen, 3, 0).tools.as_ref()).iter().map(|tool| tool.name.clone().into_owned()));
    let registered = BTreeSet::from_iter(Everything::tool_router().list_all().into_iter().map(|tool| tool.name.into_owned()));
    assert_eq!(listed, registered, "tools/list after notifications/initialized lists every registered tool");
}

#[rstest]
#[case::malformed_requests(Transcript::BadRequests)]
#[case::prompts(Transcript::Prompts)]
#[tokio::test(start_paused = true)]
async fn every_request_is_answered_once_as_its_outline_states(#[case] transcript: Transcript) { drop(replayed(transcript).await); }

#[tokio::test(start_paused = true)]
async fn tool_calls_answer_text_structure_or_a_tool_error() {
    let seen = replayed(Transcript::Tools).await;
    let text = |id: u64| answered(&seen, id, 0).content.as_ref().and_then(|content| content.first()).and_then(|block| block.text.as_deref());
    assert_eq!(text(6), Some("hi you"), "a text tool answers its text");
    assert_eq!(answered(&seen, 7, 0).structured_content.as_ref().and_then(|structured| structured.output.as_deref()), Some("Ack input"), "a structured tool answers its structure");
    assert_eq!(answered(&seen, 8, 0).is_error, Some(true), "arguments missing a required property are a tool error");
    assert_eq!(answered(&seen, 10, 0).is_error, Some(true), "absent arguments are a tool error");
}

#[tokio::test(start_paused = true)]
async fn static_resource_reads_its_text() {
    let seen = replayed(Transcript::Resources).await;
    let text = answered(&seen, 3, 0).contents.as_ref().and_then(|contents| contents.first()).and_then(|block| block.text.as_deref());
    assert_eq!(text, Some("This is the content of the static text resource."), "the read answers the resource's text");
}

#[rstest]
#[case::unknown_newer_version_falls_back_to_the_latest_with_initialize(Transcript::VersionLatest, "2025-11-25")]
#[case::known_older_version_is_kept(Transcript::VersionOlder, "2024-11-05")]
#[tokio::test(start_paused = true)]
async fn initialize_negotiates_a_version_the_server_speaks(#[case] transcript: Transcript, #[case] negotiated: &str) {
    let seen = replayed(transcript).await;
    assert_eq!(answered(&seen, 1, 0).protocol_version.as_deref(), Some(negotiated), "{transcript:?}");
}

#[tokio::test(start_paused = true)]
async fn discover_lists_versions_of_either_lifecycle_and_capabilities() {
    let seen = replayed(Transcript::Discover).await;
    let versions = assert_some!(answered(&seen, 1, 0).supported_versions.as_ref());
    assert!(versions.iter().any(|version| version == "2026-07-28") && versions.iter().any(|version| version == "2024-11-05"), "{versions:?}");
    let capabilities = BTreeSet::from_iter(assert_some!(answered(&seen, 1, 0).capabilities.as_ref()).keys().map(String::as_str));
    assert!(capabilities.is_superset(&BTreeSet::from(["completions", "prompts", "resources", "tools"])), "discover states the capability of each registered router: {capabilities:?}");
}
