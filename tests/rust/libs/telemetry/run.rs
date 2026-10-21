use std::{io, process::ExitCode};

use clap::{Args as _, Command, FromArgMatches as _};
use nonempty_collections::NEVec;
use opentelemetry::global;
use rasm_telemetry::{Telemetry, options::Options};
use tracing::subscriber;
use tracing_subscriber::{EnvFilter, Registry, layer::Identity};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn refused_install_keeps_process_globals() -> Result<(), clap::Error> {
    let mut command = Options::augment_args(Command::new(env!("CARGO_CRATE_NAME")));
    let options = Options::from_arg_matches(&command.try_get_matches_from_mut([env!("CARGO_CRATE_NAME")])?)?;
    let (held, mut worked) = (subscriber::set_global_default(Registry::default()), false);
    let code = Telemetry::run(options, &command, [], EnvFilter::default(), io::sink, Identity::new(), |_| {
        worked = true;
        Ok::<_, NEVec<!>>(ExitCode::SUCCESS)
    });
    assert!(
        held.is_ok() && code == ExitCode::FAILURE && !worked && global::get_text_map_propagator(|propagator| propagator.fields().next().is_none()),
        "refused install ran the work or replaced the process propagator"
    );
    Ok(())
}
