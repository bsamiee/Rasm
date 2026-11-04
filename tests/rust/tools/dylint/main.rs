#![feature(exit_status_error)]

use std::{
    collections::BTreeMap,
    io, panic,
    process::{Command, ExitStatusError, Stdio},
};

use cargo_metadata::{Message, diagnostic::DiagnosticLevel};
use nonempty_collections::NEVec;
use thiserror::Error;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, derive_more::Display)]
enum Step {
    #[display("failed to start")]
    Spawn,
    #[display("output is unreadable")]
    Decode,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
enum Failure {
    #[error("cargo dylint {step}")]
    Io { step: Step, source: io::Error },
    #[error("cargo dylint exited with failure\n{errors}")]
    Exit { errors: String, source: ExitStatusError },
    #[error("cargo dylint produced no `{lint}` diagnostic")]
    MissingDiagnostic { lint: &'static str },
    #[error("`{lint}` diagnostics differ from its snapshot\n{message}")]
    Snapshot { lint: &'static str, message: String },
}

impl From<Failure> for NEVec<Failure> {
    fn from(value: Failure) -> Self { Self::new(value) }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

macro_rules! snapshots {
    ($(fn $lint:ident($found:ident: &str) $body:block)+) => {
        $(fn $lint($found: &str) $body)+
        pub(crate) const SNAPSHOTS: &[(&str, fn(&str))] = &[$((stringify!($lint), $lint)),+];
    };
}

mod concurrency;
mod duplication;
mod errors;
mod expressions;
mod interior_mutability;
mod items;
mod macros;
mod manual;
mod patterns;
mod representation;

#[test]
fn ui() -> Result<(), NEVec<Failure>> {
    let package = env!("CARGO_PKG_NAME");
    let output = Command::new(env!("CARGO"))
        .args([
            "dylint",
            "--lib",
            package,
            "-p",
            package,
            "--fail-on-no-libraries",
            "--",
            "--locked",
            "--profile",
            "dylint",
            "--examples",
            "--test",
            package,
            "--message-format=json",
            "--config",
            "build.warnings='warn'",
        ])
        .stderr(Stdio::inherit())
        .output()
        .map_err(|source| Failure::Io { step: Step::Spawn, source })?;
    let (found, errors) = Message::parse_stream(output.stdout.as_slice()).try_fold((BTreeMap::<String, BTreeMap<String, String>>::new(), String::new()), |(mut found, mut errors), message| {
        if let Message::CompilerMessage(message) = message.map_err(|source| Failure::Io { step: Step::Decode, source })?
            && let Some(rendered) = message.message.rendered
        {
            if matches!(message.message.level, DiagnosticLevel::Error | DiagnosticLevel::Ice) {
                errors.extend([rendered.trim_end(), "\n\n"]);
            }
            if (message.target.is_example() || message.target.is_test())
                && !message.message.spans.is_empty()
                && let Some(code) = message.message.code
            {
                let applicabilities = message
                    .message
                    .children
                    .iter()
                    .filter_map(|child| Some(format!("applicability {:?}: {}\n", child.spans.iter().find_map(|span| span.suggestion_applicability.as_ref())?, child.message)))
                    .collect::<String>();
                found.entry(code.code).or_default().entry(message.target.name).or_default().extend([rendered.trim_end(), "\n", &applicabilities, "\n"]);
            }
        }
        Ok::<_, Failure>((found, errors))
    })?;
    output.status.exit_ok().map_err(|source| Failure::Exit { errors, source })?;
    [
        concurrency::SNAPSHOTS,
        duplication::SNAPSHOTS,
        errors::SNAPSHOTS,
        expressions::SNAPSHOTS,
        interior_mutability::SNAPSHOTS,
        items::SNAPSHOTS,
        macros::SNAPSHOTS,
        manual::SNAPSHOTS,
        patterns::SNAPSHOTS,
        representation::SNAPSHOTS,
    ]
    .into_iter()
    .flatten()
    .map(|&(lint, snapshot)| {
        let diagnostics = found.get(lint).ok_or(Failure::MissingDiagnostic { lint })?;
        let rendered = diagnostics.values().map(String::as_str).collect::<String>();
        panic::catch_unwind(|| snapshot(&rendered)).map_err(|payload| Failure::Snapshot { lint, message: *payload.downcast::<String>().unwrap_or_else(|other| panic::resume_unwind(other)) })
    })
    .collect::<Validated<(), Failure>>()
    .ok()
}
