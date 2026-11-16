use claims::{assert_ok, assert_some};
use rasm_mcp_tests::{Answered, Id, Message, OUTLINES, TRANSCRIPTS, everything, replies, wired};
use rstest::rstest;

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn replayed(name: &str) -> Vec<Message> {
    let (shared, _app) = assert_ok!(everything().await);
    let transcript = assert_some!(TRANSCRIPTS.iter().find(|(known, _)| *known == name)).1;
    let (mut wire, _serving) = wired(shared.connection());
    let seen = assert_ok!(wire.replayed(transcript).await);
    let outline = assert_some!(OUTLINES.iter().find(|(known, _)| *known == name)).1;
    assert_eq!(
        replies(&seen),
        Vec::from_iter(outline.iter().map(|(id, reply)| (Some(Id::Number(*id)), *reply))),
        "each request is answered once under its own id, and no notification is answered: {seen:#?}"
    );
    seen
}

fn answered(seen: &[Message], id: u64, nth: usize) -> &Answered {
    let results = seen.iter().filter(|message| message.id == Some(Id::Number(id))).filter_map(|message| message.result.as_ref());
    assert_some!(results.into_iter().nth(nth))
}

// --- [TRANSCRIPTS]

#[tokio::test]
async fn lifecycle() {
    let seen = replayed("lifecycle").await;
    assert_eq!(answered(&seen, 1, 0).protocol_version.as_deref(), Some("2024-11-05"));
    assert_eq!(answered(&seen, 1, 1), &Answered::default(), "ping returns an empty result");
    assert_some!(answered(&seen, 3, 0).tools.as_ref());
}

#[rstest]
#[case::bad_requests("bad_requests")]
#[case::prompts("prompts")]
#[tokio::test]
async fn outline(#[case] name: &str) { drop(replayed(name).await); }

#[tokio::test]
async fn tools() {
    let seen = replayed("tools").await;
    let text = |id: u64| answered(&seen, id, 0).content.as_ref().and_then(|content| content.first()).and_then(|block| block.text.as_deref());
    assert_eq!(text(6), Some("hi you"));
    assert_eq!(answered(&seen, 7, 0).structured_content.as_ref().and_then(|structured| structured.output.as_deref()), Some("Ack input"));
    assert_eq!(answered(&seen, 8, 0).is_error, Some(true), "arguments missing a required property are a tool error");
    assert_eq!(answered(&seen, 10, 0).is_error, Some(true), "absent arguments are a tool error");
}

#[tokio::test]
async fn resources() {
    let seen = replayed("resources").await;
    let text = answered(&seen, 3, 0).contents.as_ref().and_then(|contents| contents.first()).and_then(|block| block.text.as_deref());
    assert_eq!(text, Some("This is the content of the static text resource."));
}

#[rstest]
#[case::version_latest("version-latest", "2025-11-25")]
#[case::version_older("version-older", "2024-11-05")]
#[tokio::test]
async fn version(#[case] name: &str, #[case] negotiated: &str) {
    let seen = replayed(name).await;
    assert_eq!(answered(&seen, 1, 0).protocol_version.as_deref(), Some(negotiated));
}

#[tokio::test]
async fn discover() {
    let seen = replayed("discover").await;
    let versions = assert_some!(answered(&seen, 1, 0).supported_versions.as_ref());
    assert!(versions.iter().any(|version| version == "2026-07-28") && versions.iter().any(|version| version == "2024-11-05"), "{versions:?}");
    assert_some!(answered(&seen, 1, 0).capabilities.as_ref());
}
