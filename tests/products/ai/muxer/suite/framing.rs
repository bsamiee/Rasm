use claims::assert_ok;
use mcp_muxer_tests::{Agent, Isolated, session};
use rstest::rstest;

use crate::{echoed, scoped};

// --- [TEXT]

#[rstest]
#[case::utf8_of_every_width("ą€🙂漢")]
#[case::line_and_paragraph_separators("before\u{2028}middle\u{2029}after")]
#[tokio::test(flavor = "multi_thread")]
async fn text_survives_the_relay(#[case] said: &str) {
    let passed = scoped(async |isolated: &Isolated| {
        let relayed = assert_ok!(session(Agent::default(), isolated.relay(&[], &[])).await);
        let direct = assert_ok!(session(Agent::default(), isolated.direct(&[])).await);
        assert_eq!((echoed(&relayed, said).await, echoed(&direct, said).await), (said.to_owned(), said.to_owned()), "the relayed result equals the direct one");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}
