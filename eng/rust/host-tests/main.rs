#![feature(default_field_values)]

use core::time::Duration;
use std::{io, path::PathBuf, process::ExitCode};

use clap::{Parser, Subcommand};
use futures::{FutureExt as _, StreamExt as _};
use jiff::fmt::friendly::SpanParser;
use rasm_command::{CommandError, Process, telemetry::Defaults};
use rasm_process::Run;
use tracing_subscriber::layer::Identity;

#[cfg(target_os = "macos")]
mod adobe;
mod blender;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Parser)]
#[command(about)]
struct Tests {
    output: PathBuf,
    #[arg(long, value_name = "DURATION", value_parser = |text: &str| SpanParser::new().parse_unsigned_duration(text), help = "Time the run takes before its processes stop")]
    budget: Duration,
    #[command(subcommand)]
    host: Host,
}

#[derive(Subcommand)]
enum Host {
    Blender(blender::Suite),
    #[cfg(target_os = "macos")]
    Adobe(adobe::Suite),
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    rasm_command::run(Defaults { .. }, io::stderr, Identity::new(), async |Process { args: Tests { output, budget, host }, styles: _, mut signals, filter: _, level: _ }| {
        Run::execute(&output, budget, signals.next().map(drop), async |run| {
            match host {
                Host::Blender(suite) => blender::test(suite, run).await,
                #[cfg(target_os = "macos")]
                Host::Adobe(suite) => adobe::test(suite, run).await,
            }
            .map_err(CommandError::File)
        })
        .await
    })
}
