use claims::assert_ok;
use mcp_muxer_tests::{Agent, Isolated, session};
use rasm_testing::{Failure, Folder};
use rstest::rstest;

use crate::{echoed, scoped};

// --- [TEXT]

#[rstest]
#[case::utf8_of_every_width("ą€🙂漢")]
#[case::line_and_paragraph_separators("before\u{2028}middle\u{2029}after")]
#[case::escaped_newline_and_quote("one\ntwo \"three\"")]
#[tokio::test(flavor = "multi_thread")]
async fn text_crosses_the_relay_unchanged(#[case] said: &str) -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let relayed = assert_ok!(session(Agent::default(), isolated.relay(&[], &[])).await);
        assert_eq!(echoed(&relayed, said).await, said, "the relayed echo returns the text sent");
    })
    .await
}
