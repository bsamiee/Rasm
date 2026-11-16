use std::process::Stdio;

use claims::{assert_ok, assert_some};
use mcp_muxer_tests::Isolated;
use rasm_mcp_tests::{Id, Message, OUTLINES, TRANSCRIPTS, Wire, replies};
use rstest::rstest;
use tokio::process::Command;

use crate::scoped;

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn replayed(mut command: Command, transcript: &str) -> Vec<Message> {
    let mut child = assert_ok!(command.stdin(Stdio::piped()).stdout(Stdio::piped()).stderr(Stdio::null()).kill_on_drop(true).spawn());
    let mut wire = Wire::new(assert_some!(child.stdout.take()), assert_some!(child.stdin.take()));
    assert_ok!(wire.replayed(transcript).await)
}

// --- [TRANSCRIPTS]

#[rstest]
#[tokio::test(flavor = "multi_thread")]
async fn transcript_replays_through_the_relay_as_it_does_directly(#[values("lifecycle", "bad_requests", "tools", "resources", "prompts", "version-latest", "version-older", "discover")] name: &str) {
    let passed = scoped(async |isolated: &Isolated| {
        let transcript = assert_some!(TRANSCRIPTS.iter().find(|(known, _)| *known == name)).1;
        let outline = assert_some!(OUTLINES.iter().find(|(known, _)| *known == name)).1;
        let through = replayed(isolated.serving(Some(&[]), &isolated.everything, &[]), transcript).await;
        let direct = replayed(isolated.serving(None, &isolated.everything, &[]), transcript).await;
        assert_eq!(replies(&through), Vec::from_iter(outline.iter().map(|(id, reply)| (Some(Id::Number(*id)), *reply))), "each request is answered once under its own id: {through:#?}");
        assert_eq!(replies(&through), replies(&direct), "the relay answers what the server answers");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}
