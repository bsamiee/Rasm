#![feature(default_field_values, error_reporter)]

use std::error::Report;
use std::io;
use std::process::ExitCode;

use clap::Parser;
use futures::StreamExt as _;
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_cli::{CommandError, Invocation};
use rasm_mcp_tests::everything;
use rasm_telemetry::process::Program;
use rmcp::ServiceExt as _;
use tokio::runtime::Builder;
use tracing_subscriber::layer::Identity;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Parser)]
#[command(about = "Stdio MCP server with the tools, resources, and prompts the official conformance scenarios call")]
struct Arguments;

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    rasm_cli::run(
        Program { scopes: vec![(env!("CARGO_CRATE_NAME"), None)], .. },
        io::stderr,
        Identity::new(),
        |_: &Arguments| Builder::new_current_thread(),
        async |Invocation { args: Arguments, executable: _, styles: _, folders: _, filter: _, parent: _, settings: _, export: _ }| {
            let mut signals = rasm_cli::signals()?;
            let shared = match everything().await {
                Ok((shared, _)) => shared,
                Err(error) => {
                    tracing::error!({ EXCEPTION_MESSAGE } = %Report::new(&error), "everything server refused its routers");
                    return Ok::<_, NEVec<CommandError>>(ExitCode::from(4));
                }
            };
            let running = match shared.connection().serve(rmcp::transport::stdio()).await {
                Ok(running) => running,
                Err(error) => {
                    tracing::error!({ EXCEPTION_MESSAGE } = %Report::new(&error), "everything server handshake failed");
                    return Ok(ExitCode::from(4));
                }
            };
            tokio::select! {
                _ = signals.next() => (),
                _ = running.waiting() => (),
            }
            Ok(ExitCode::SUCCESS)
        },
    )
}
