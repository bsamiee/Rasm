#![feature(default_field_values)]

use std::{ffi::OsString, process::ExitCode};

use clap::{Arg, ArgMatches, CommandFactory, FromArgMatches, ValueHint, error::ErrorKind, value_parser};
use rasm_mcp::{host, proxy::Proxy};
use rasm_telemetry::Defaults;
use tokio::process::Command;
use tracing_subscriber::layer::Identity;

// --- [CONSTANTS] -----------------------------------------------------------------------

const COMMAND: &str = "COMMAND";

// --- [MODELS] --------------------------------------------------------------------------

struct Upstream(Command);

impl CommandFactory for Upstream {
    fn command() -> clap::Command {
        clap::command!().arg(
            Arg::new(COMMAND)
                .value_parser(value_parser!(OsString))
                .num_args(1..)
                .required(true)
                .trailing_var_arg(true)
                .value_hint(ValueHint::CommandWithArguments)
                .help("Stdio MCP server executable, all following tokens passed as arguments"),
        )
    }

    fn command_for_update() -> clap::Command { Self::command().mut_arg(COMMAND, |arg| arg.required(false)) }
}

impl FromArgMatches for Upstream {
    fn from_arg_matches(matches: &ArgMatches) -> Result<Self, clap::Error> {
        let mut words = matches.get_many::<OsString>(COMMAND).into_iter().flatten();
        let mut command = Command::new(words.next().ok_or_else(|| clap::Error::new(ErrorKind::MissingRequiredArgument))?);
        command.args(words);
        Ok(Self(command))
    }

    fn update_from_arg_matches(&mut self, matches: &ArgMatches) -> Result<(), clap::Error> {
        if matches.contains_id(COMMAND) {
            *self = Self::from_arg_matches(matches)?;
        }
        Ok(())
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode { host::run(Defaults { .. }, Identity::new(), |Upstream(command), host| Proxy::spawn(command, host)) }
