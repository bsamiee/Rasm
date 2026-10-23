use std::process::ExitCode;

use clap::{Args as _, Command, FromArgMatches as _};
use nonempty_collections::NEVec;
use opentelemetry::global;
use rasm_telemetry::Options;
use tracing::subscriber;
use tracing_subscriber::{Registry, layer::Identity};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn refused_install_keeps_process_globals() -> Result<(), clap::Error> {
    let mut command = Options::augment_args(Command::new(env!("CARGO_CRATE_NAME")));
    let options = Options::from_arg_matches(&command.try_get_matches_from_mut([
        env!("CARGO_CRATE_NAME"),
        "--propagators=tracecontext,baggage",
        "--log=info",
        "--traces-exporter=none",
        "--metrics-exporter=none",
        "--logs-exporter=none",
    ])?)?;
    let (held, mut worked) = (subscriber::set_global_default(Registry::default()), false);
    let code = options.run(&command, [], "", Identity::new(), |_| {
        worked = true;
        Ok::<_, NEVec<!>>(ExitCode::SUCCESS)
    });
    assert_eq!(
        (held.is_ok(), code, worked, global::get_text_map_propagator(|propagator| propagator.fields().count()), log::max_level()),
        (true, ExitCode::FAILURE, false, 0, log::LevelFilter::Off),
        "refused install changed process globals"
    );
    Ok(())
}
