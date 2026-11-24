#![feature(deref_patterns)]

use core::future::ready;

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::test]
async fn generated_pin() { ready(()).await }

#[tokio::test]
async fn explicit_pin() {
    let future = ready(());
    tokio::pin!(future); //~ tokio_pin
    future.await
}

#[expect(tokio_pin)]
#[tokio::test]
async fn expected_pin() {
    let future = ready(());
    tokio::pin!(future);
    future.await
}

mod assertions_outside_tests;
pub mod interior_mutable_static;
mod macro_matching;

mod error_assertions {
    use core::error::Error;
    use core::hint::black_box;
    use core::{assert_matches, debug_assert_matches, fmt};

    use anyhow::Context as _;

    // --- [MODELS]

    #[derive(Debug, derive_more::Display)]
    enum Mode {
        Ready,
    }

    // --- [OPERATIONS]

    fn boxed_helper() -> Result<(), Box<dyn Error>> { Ok(()) } //~ untyped_error

    fn report_helper() -> anyhow::Result<()> { Ok(()) } //~ untyped_error

    // --- [COMPOSITION]

    #[test]
    fn observed_error_text() {
        let error = fmt::Error;
        assert!(error.to_string().ends_with("argument"));
        assert!(!error.to_string().is_empty() && format!("{error:?}").starts_with("Error"));
        assert!(matches!(Some(&error), Some(cause) if cause.to_string().contains("formatting")));
        assert_eq!(error.to_string(), "an error occurred when formatting an argument");
        assert_ne!(error.to_string(), "");
    }

    #[test]
    fn matched_error_text() {
        let error = fmt::Error;
        assert_matches!(Some(&error), Some(cause) if cause.to_string() == "an error occurred when formatting an argument");
        assert_matches!(Some(&error), Some(cause) if cause.to_string().contains("formatting"), "the cause of {error:?}");
        debug_assert_matches!(Some(&error), Some(cause) if !cause.to_string().is_empty() && cause.to_string().ends_with("argument"));
        assert_matches!(error.to_string(), "an error occurred when formatting an argument");
    }

    #[test]
    fn resolved_assertion_forms() {
        use core::assert as require; //~ needless_import_rename

        macro_rules! observed {
            ($condition:expr) => {
                core::assert!($condition);
            };
        }
        let error = fmt::Error;
        require!(error.to_string().starts_with("an"));
        observed!(error.to_string().contains("formatting"));
    }

    #[test]
    fn error_text_controls_branches() {
        let error = fmt::Error;
        //~v stringified_error
        black_box(match error.to_string().contains("formatting") {
            true => 1,
            false => 0,
        });
        assert!(if error.to_string().contains("argument") { true } else { false }); //~ stringified_error
        let message = error.to_string(); //~ stringified_error
        assert!(message.contains("formatting"));
        if message.ends_with("argument") {
            black_box(message.len());
        }
    }

    #[test]
    fn shadowed_assertion() {
        macro_rules! assert {
            ($condition:expr) => {
                black_box($condition);
            };
        }
        let error = fmt::Error;
        assert!(error.to_string().contains("formatting")); //~ stringified_error
    }

    #[test]
    fn value_text_assertion() {
        assert!(Mode::Ready.to_string().contains("Ready"));
    }

    #[test]
    fn boxed_test_envelope() -> Result<(), Box<dyn Error>> { black_box((boxed_helper(), report_helper())).0 }

    #[test]
    fn report_test_envelope() -> anyhow::Result<()> { black_box((report_helper(), boxed_helper())).0 }

    #[tokio::test]
    async fn async_report_test_envelope() -> anyhow::Result<()> {
        core::future::ready(()).await;
        Ok(())
    }

    #[test]
    fn nested_report_helper() {
        fn nested_report_helper() -> anyhow::Result<()> { Ok(()) } //~ untyped_error
        black_box(&nested_report_helper());
    }

    #[test]
    fn string_test_error() -> Result<(), String> { Ok(()) } //~ untyped_error

    #[test]
    fn introduces_report() -> anyhow::Result<()> {
        black_box(anyhow::Error::from(fmt::Error)); //~ untyped_error
        Ok(())
    }

    #[tokio::test]
    async fn async_introduces_report() -> anyhow::Result<()> {
        core::future::ready(()).await;
        black_box(anyhow::Error::from(fmt::Error)); //~ untyped_error
        Ok(())
    }

    #[test]
    fn introduces_context() -> anyhow::Result<()> {
        Err::<(), _>(fmt::Error).context("rendering")?; //~ untyped_error
        Ok(())
    }
}
