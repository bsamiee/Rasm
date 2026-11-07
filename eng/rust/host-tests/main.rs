#![feature(default_field_values)]

use core::time::Duration;
use std::{io, path::PathBuf, process::ExitCode};

use clap::{Parser, Subcommand};
use futures::{FutureExt as _, StreamExt as _};
use rasm_command::{CommandError, Process};
use rasm_process::report::Run;
use rasm_telemetry::Defaults;
use tracing_subscriber::layer::Identity;

#[cfg(target_os = "macos")]
mod adobe;
mod blender;
#[cfg(target_os = "macos")]
mod desktop;
#[cfg(target_os = "macos")]
mod rhino;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Parser)]
#[command(about)]
struct Tests {
    output: PathBuf,
    #[command(subcommand)]
    host: Host,
}

#[derive(Subcommand)]
enum Host {
    Blender(blender::Suite),
    #[cfg(target_os = "macos")]
    Rhino(rhino::Suite),
    #[cfg(target_os = "macos")]
    Adobe(adobe::Suite),
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    rasm_command::run(Defaults { .. }, io::stderr, Identity::new(), async |Process { args: Tests { output, host }, mut signals, .. }| {
        let minutes = match &host {
            Host::Blender(_) => 4,
            #[cfg(target_os = "macos")]
            Host::Rhino(_) => 2,
            #[cfg(target_os = "macos")]
            Host::Adobe(_) => 15,
        };
        Run::execute(&output, Duration::from_mins(minutes), signals.next().map(drop), async |run| {
            match host {
                Host::Blender(suite) => blender::test(suite, run).await,
                #[cfg(target_os = "macos")]
                Host::Rhino(suite) => rhino::test(suite, run).await,
                #[cfg(target_os = "macos")]
                Host::Adobe(suite) => adobe::test(suite, run).await,
            }
            .map_err(CommandError::File)
        })
        .await
    })
}
