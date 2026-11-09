#![feature(default_field_values)]

use std::{ffi::OsString, process::ExitCode};

use clap::{Parser, ValueHint};
use rasm_command::telemetry::Defaults;
use rasm_mcp::{Proxy, host};
use tokio::process::Command;
use tracing_subscriber::layer::Identity;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Parser)]
#[command(version, about)]
struct Upstream {
    #[arg(required = true, num_args = 1.., trailing_var_arg = true, allow_hyphen_values = true, value_hint = ValueHint::CommandWithArguments, help = "Stdio MCP server command line")]
    command: Vec<OsString>,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    host::run(Defaults { .. }, Identity::new(), |Upstream { command }, host| {
        let mut parts = command.into_iter();
        let mut upstream = Command::new(parts.next().unwrap_or_default());
        upstream.args(parts);
        Proxy::spawn(upstream, host)
    })
}
