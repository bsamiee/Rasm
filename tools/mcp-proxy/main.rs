#![feature(default_field_values)]

use std::{ffi::OsString, process::ExitCode};

use clap::{Parser, ValueHint};
use rasm_command::telemetry::Defaults;
use rasm_mcp::{Proxy, host};
use tokio::process::Command;
use tracing_subscriber::layer::Identity;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Parser)]
#[command(version = option_env!("RASM_VERSION").unwrap_or(env!("CARGO_PKG_VERSION")), about)]
struct Upstream {
    #[arg(value_hint = ValueHint::CommandName, help = "Stdio MCP server executable")]
    program: OsString,
    #[arg(trailing_var_arg = true, allow_hyphen_values = true, help = "Arguments the server receives")]
    arguments: Vec<OsString>,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    host::run(Defaults { .. }, Identity::new(), |Upstream { program, arguments }, host| {
        let mut command = Command::new(program);
        command.args(arguments);
        Proxy::spawn(command, host)
    })
}
