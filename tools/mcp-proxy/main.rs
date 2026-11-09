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
    #[arg(value_hint = ValueHint::CommandName, help = "Stdio MCP server program")]
    program: OsString,
    #[arg(trailing_var_arg = true, allow_hyphen_values = true, help = "Arguments the program receives")]
    arguments: Vec<OsString>,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    host::run(Defaults { .. }, Identity::new(), |Upstream { program, arguments }, host| {
        let mut upstream = Command::new(program);
        upstream.args(arguments);
        Proxy::spawn(upstream, host)
    })
}
