#![feature(exit_status_error)]

use std::{
    collections::BTreeMap,
    io,
    path::PathBuf,
    process::{Command, ExitStatusError, Stdio},
};

use cargo_metadata::{Message, diagnostic::DiagnosticLevel};
use thiserror::Error;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, derive_more::Display)]
#[display(rename_all = "lowercase")]
enum Stage {
    Start,
    Output,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
enum Failure {
    #[error("cargo dylint {stage} failed")]
    Io { stage: Stage, source: io::Error },
    #[error("cargo dylint exited with failure\n{errors}")]
    Exit { errors: String, source: ExitStatusError },
    #[error("diagnostics in targets no fixture pattern names\n{found}")]
    Unmatched { found: String },
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn ui() -> Result<(), Failure> {
    let output = Command::new(env!("CARGO"))
        .args(["dylint", "--all", "-p", env!("CARGO_PKG_NAME"), "--fail-on-no-libraries", "--", "--locked", "--examples", "--test", "*", "--message-format=json", "--config", "build.warnings='warn'"])
        .stderr(Stdio::inherit())
        .output()
        .map_err(|source| Failure::Io { stage: Stage::Start, source })?;
    let (mut found, errors) = Message::parse_stream(output.stdout.as_slice()).try_fold((BTreeMap::<PathBuf, String>::new(), String::new()), |(mut found, mut errors), message| {
        if let Message::CompilerMessage(message) = message.map_err(|source| Failure::Io { stage: Stage::Output, source })?
            && let Some(rendered) = &message.message.rendered
        {
            if matches!(message.message.level, DiagnosticLevel::Error | DiagnosticLevel::Ice) {
                errors.extend([rendered.trim_end(), "\n\n"]);
            }
            if (message.target.is_example() || message.target.is_test()) && !message.message.spans.is_empty() && message.message.code.is_some() {
                let applicabilities = message
                    .message
                    .children
                    .iter()
                    .filter_map(|child| Some(format!("applicability {:?}: {}\n", child.spans.iter().find_map(|span| span.suggestion_applicability.as_ref())?, child.message)))
                    .collect::<String>();
                found.entry(message.target.src_path.into_std_path_buf()).or_default().extend([rendered.trim_end(), "\n", &applicabilities, "\n"]);
            }
        }
        Ok::<_, Failure>((found, errors))
    })?;
    output.status.exit_ok().map_err(|source| Failure::Exit { errors, source })?;
    insta::with_settings!({ prepend_module_to_snapshot => false, omit_expression => true }, {
        insta::glob!("../..", "{examples/*.rs,tests/expansions/*.rs}", |path| insta::assert_snapshot!(found.remove(path).unwrap_or_default()));
    });
    found.is_empty().ok_or_else(|| Failure::Unmatched { found: found.into_values().collect() })
}
