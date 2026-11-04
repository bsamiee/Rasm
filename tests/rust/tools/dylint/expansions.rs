use core::future::ready;

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::test]
async fn generated_pin() { ready(()).await }

#[tokio::test]
async fn explicit_pin() {
    let future = ready(());
    tokio::pin!(future);
    future.await
}

#[cfg_attr(dylint_lib = "lints", expect(tokio_pin), forbid(unfulfilled_lint_expectations))]
#[tokio::test]
async fn expected_pin() {
    let future = ready(());
    tokio::pin!(future);
    future.await
}

#[cfg_attr(dylint_lib = "lints", allow(tokio_pin))]
#[tokio::test]
async fn allowed_pin() {
    let future = ready(());
    tokio::pin!(future);
    future.await
}

mod assertions_outside_tests;
mod interior_mutable_static;
mod macro_matching;

mod error_assertions {
    use core::{error::Error, fmt, hint::black_box};

    use anyhow::Context as _;

    // --- [MODELS] ----------------------------------------------------------------------

    #[derive(Debug, derive_more::Display)]
    enum Mode {
        Ready,
    }

    // --- [OPERATIONS] ------------------------------------------------------------------

    fn boxed_helper() -> Result<(), Box<dyn Error>> { Ok(()) }

    fn report_helper() -> anyhow::Result<()> { Ok(()) }

    // --- [COMPOSITION] -----------------------------------------------------------------

    #[test]
    fn observed_error_text() {
        let error = fmt::Error;
        assert!(error.to_string().ends_with("argument"));
        assert!(format!("{error:?}").contains("Error"));
        assert!(!error.to_string().is_empty() && format!("{error:?}").starts_with("Error"));
        assert!(matches!(Some(&error), Some(cause) if cause.to_string().contains("formatting")));
        debug_assert!(error.to_string().starts_with("an"));
        assert_eq!(error.to_string(), "an error occurred when formatting an argument");
    }

    #[test]
    fn resolved_assertion_forms() {
        use core::assert as require;

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
        if error.to_string().contains("argument") {
            black_box(&error);
        }
        black_box(match error.to_string().contains("formatting") {
            true => 1,
            false => 0,
        });
        assert!(if error.to_string().contains("argument") { true } else { false });
        let message = error.to_string();
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
        assert!(error.to_string().contains("formatting"));
    }

    #[test]
    fn value_text_assertion() {
        assert!(Mode::Ready.to_string().contains("Ready"));
    }

    #[test]
    fn spawned_rendering_assertions() {
        core::assert_matches!(
            std::thread::spawn(|| {
                let mode = Mode::Ready;
                assert!(mode.to_string().contains("Ready"));
                assert!(format!("{mode:?}").contains("Ready"));
                let error = fmt::Error;
                assert!(error.to_string().ends_with("argument"));
                assert!(format!("{error:?}").contains("Error"));
            })
            .join(),
            Ok(())
        );
    }

    #[test]
    fn spawned_rendering_control() {
        core::assert_matches!(
            std::thread::spawn(|| {
                let mode = Mode::Ready;
                if mode.to_string().contains("Ready") {
                    black_box(&mode);
                }
                let error = fmt::Error;
                black_box(format!("{error:?}").contains("Error"));
            })
            .join(),
            Ok(())
        );
    }

    #[test]
    fn boxed_test_envelope() -> Result<(), Box<dyn Error>> { Ok(()) }

    #[test]
    fn report_test_envelope() -> anyhow::Result<()> { Ok(()) }

    #[tokio::test]
    async fn async_report_test_envelope() -> anyhow::Result<()> {
        core::future::ready(()).await;
        Ok(())
    }

    #[test]
    fn nested_report_helper() {
        fn nested_report_helper() -> anyhow::Result<()> { Ok(()) }
        black_box(&nested_report_helper());
    }

    #[test]
    fn string_test_error() -> Result<(), String> { Ok(()) }

    #[test]
    fn test_introduces_report() -> anyhow::Result<()> {
        black_box(anyhow::Error::from(fmt::Error));
        Ok(())
    }

    #[tokio::test]
    async fn async_test_introduces_report() -> anyhow::Result<()> {
        core::future::ready(()).await;
        black_box(anyhow::Error::from(fmt::Error));
        Ok(())
    }

    #[test]
    fn test_introduces_context() -> anyhow::Result<()> {
        Err::<(), _>(fmt::Error).context("rendering")?;
        Ok(())
    }
}
