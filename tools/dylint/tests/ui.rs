#![feature(exit_status_error)]

use std::{io, process::ExitStatusError};

use thiserror::Error;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
enum Failure {
    #[error("cargo dylint did not start")]
    Spawn(#[source] io::Error),
    #[error("cargo dylint wrote a line no message decodes from")]
    Message(#[source] io::Error),
    #[error("cargo dylint failed")]
    Exit(#[source] ExitStatusError),
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[cfg(test)]
mod tests {
    use std::{
        collections::BTreeMap,
        process::{Command, Stdio},
    };

    use cargo_metadata::Message;

    use super::Failure;

    #[test]
    fn examples() -> Result<(), Failure> {
        let output = Command::new(env!("CARGO"))
            .args(["dylint", "--lib", env!("CARGO_PKG_NAME"), "-p", env!("CARGO_PKG_NAME"), "--", "--examples", "--message-format=json", "--config", "build.warnings='warn'"])
            .stderr(Stdio::inherit())
            .output()
            .map_err(Failure::Spawn)?;
        let findings = Message::parse_stream(output.stdout.as_slice()).try_fold(BTreeMap::<String, String>::new(), |mut findings, message| {
            if let Message::CompilerMessage(message) = message.map_err(Failure::Message)?
                && !message.message.spans.is_empty()
                && let Some(rendered) = message.message.rendered
            {
                let applicabilities = message
                    .message
                    .children
                    .iter()
                    .filter_map(|child| Some(format!("applicability {:?}: {}\n", child.spans.iter().find_map(|span| span.suggestion_applicability.as_ref())?, child.message)))
                    .collect::<String>();
                findings.entry(message.target.name).or_default().extend([rendered.trim_end(), "\n", &applicabilities, "\n"]);
            }
            Ok(findings)
        })?;
        insta::with_settings!({ snapshot_path => ".", prepend_module_to_snapshot => false }, { insta::assert_snapshot!("ui", findings.into_values().collect::<String>()) });
        output.status.exit_ok().map_err(Failure::Exit)
    }
}
