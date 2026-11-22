use std::process::Stdio;

use claims::{assert_ok, assert_some};
use mcp_muxer_tests::Isolated;
use rasm_mcp_tests::{Transcript, Wire, replies};
use rasm_testing::{Failure, Folder};
use rstest::rstest;

use crate::scoped;

// --- [TRANSCRIPTS]

#[rstest]
#[tokio::test(flavor = "multi_thread")]
async fn transcript_through_the_relay_is_answered_as_the_server_answers_it(
    #[values(Transcript::Lifecycle, Transcript::BadRequests, Transcript::Tools, Transcript::Resources, Transcript::Prompts, Transcript::VersionLatest, Transcript::VersionOlder, Transcript::Discover)]
    transcript: Transcript,
) -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let mut relay = isolated.serving(Some(&[]), &isolated.everything, &[]);
        let mut child = assert_ok!(relay.stdin(Stdio::piped()).stdout(Stdio::piped()).stderr(Stdio::null()).kill_on_drop(true).spawn());
        let mut wire = Wire::new(assert_some!(child.stdout.take()), assert_some!(child.stdin.take()));
        let seen = assert_ok!(wire.replayed(transcript.sent()).await);
        assert_eq!(replies(&seen), transcript.replies(), "{transcript:?}: each request is answered once under its own id: {seen:#?}");
    })
    .await
}
