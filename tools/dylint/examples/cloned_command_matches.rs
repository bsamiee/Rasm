use core::hint::black_box;

use clap::{ArgMatches, Command};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Cli {
    command: Command,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn parsed(command: &Command, arguments: &[&str]) -> Result<ArgMatches, clap::Error> { command.clone().try_get_matches_from(arguments) }

fn fielded(cli: &Cli, arguments: &[&str]) -> Result<ArgMatches, clap::Error> { cli.command.clone().try_get_matches_from(arguments) }

fn local(arguments: &[&str]) -> Result<ArgMatches, clap::Error> {
    let command = Command::new("local");
    command.clone().try_get_matches_from(arguments)
}

fn borrowed(command: &mut Command, arguments: &[&str]) -> Result<ArgMatches, clap::Error> { command.try_get_matches_from_mut(arguments) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let mut command = Command::new("fixture");
    let cli = Cli { command: command.clone() };
    black_box(&(parsed(&command, &["fixture"]), fielded(&cli, &["fixture"]), local(&["fixture"]), borrowed(&mut command, &["fixture"])));
}
