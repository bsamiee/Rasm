use std::process::Stdio;

use claims::assert_ok;
use mcp_muxer_tests::Isolated;
use rstest::rstest;

use crate::scoped;

// --- [ARGUMENTS]

#[tokio::test]
async fn help_goes_to_standard_output() {
    let passed = scoped(async |isolated: &Isolated| {
        let mut command = isolated.relay(&["--help"], &[]);
        let output = assert_ok!(command.stdin(Stdio::null()).output().await);
        assert!(output.status.success(), "help is no failure: {}", output.status);
        assert!(!output.stdout.is_empty() && output.stderr.is_empty(), "help text is the output, with nothing on standard error");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[rstest]
#[case::unknown_flag(&["--no-such-flag"], "--no-such-flag")]
#[case::invalid_duration(&["--idle", "invalid"], "--idle")]
#[case::not_a_number(&["--idle", "nan"], "--idle")]
#[case::unknown_choice(&["--per", "nobody"], "--per")]
#[tokio::test]
async fn invalid_arguments_are_rejected(#[case] options: &[&str], #[case] named: &str) {
    let passed = scoped(async |isolated: &Isolated| {
        let mut command = isolated.relay(options, &[]);
        let output = assert_ok!(command.stdin(Stdio::null()).output().await);
        let reported = String::from_utf8_lossy(&output.stderr);
        assert!(!output.status.success(), "an invalid argument ends the program with a failure status");
        assert!(output.stdout.is_empty(), "no MCP message is written for an invalid argument: {}", String::from_utf8_lossy(&output.stdout));
        assert!(reported.contains(named), "standard error names the argument: {reported}");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}
