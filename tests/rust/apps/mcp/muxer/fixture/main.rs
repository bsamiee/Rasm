#![feature(default_field_values, error_reporter)]

use core::time::Duration;
use std::error::Report;
use std::io;
use std::process::ExitCode;

use clap::Parser;
use futures::StreamExt as _;
use jiff::fmt::friendly::SpanParser;
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_command::{CommandError, Invocation};
use rasm_observability::process::Program;
use rmcp::ServiceExt as _;
use tokio::runtime::Builder;
use tokio::time;
use tracing_subscriber::layer::Identity;

use self::tools::{Fixture, Speaks};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Parser)]
#[command(about = "Stdio MCP server with settable protocol versions, start delay, start failure, crash, progress, input requests, and result size")]
struct Arguments {
    #[arg(long, value_name = "DURATION", default_value = "0s", value_parser = |text: &str| SpanParser::new().parse_unsigned_duration(text), help = "Time before the server reads its first message")]
    start_delay: Duration,
    #[arg(long, help = "Exit with status 7 before serving, after one event on standard error")]
    fail_start: bool,
    #[arg(long, value_enum, default_value_t, help = "Protocol versions the server answers")]
    speaks: Speaks,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod tools;

fn main() -> ExitCode {
    rasm_command::run(
        Program { scopes: vec![(env!("CARGO_CRATE_NAME"), None)], .. },
        io::stderr,
        Identity::new(),
        |_: &Arguments| Builder::new_current_thread(),
        async |Invocation { args: Arguments { start_delay, fail_start, speaks }, executable: _, styles: _, folders: _, filter: _, parent: _, settings: _, export: _ }| {
            let mut signals = rasm_command::signals()?;
            if fail_start {
                tracing::error!("fixture refuses to start");
                return Ok::<_, NEVec<CommandError>>(ExitCode::from(7));
            }
            time::sleep(start_delay).await;
            let fixture = Fixture::new(speaks);
            let crashed = fixture.crashed.clone();
            let running = match fixture.serve(rmcp::transport::stdio()).await {
                Ok(running) => running,
                Err(error) => {
                    tracing::error!({ EXCEPTION_MESSAGE } = %Report::new(&error), "fixture handshake failed");
                    return Ok(ExitCode::from(4));
                }
            };
            Ok(tokio::select! {
                () = crashed.cancelled() => ExitCode::from(3),
                _ = signals.next() => ExitCode::SUCCESS,
                _ = running.waiting() => ExitCode::SUCCESS,
            })
        },
    )
}
