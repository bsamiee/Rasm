#![feature(default_field_values)]

use std::{io, process::ExitCode};

use clap::{Args as _, Command, FromArgMatches as _};
use nonempty_collections::NEVec;
use rasm_telemetry::{Defaults, Options};
use tracing::Level;
use tracing_subscriber::{EnvFilter, filter::LevelFilter, layer::Identity};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn empty_directives_preserve_root_level_and_target_overrides() -> Result<(), clap::Error> {
    const TARGET: &str = "root_level_target";
    let name = env!("CARGO_PKG_NAME");
    let mut command = Options::augment_args(Command::new(name));
    let mut matches = command.try_get_matches_from_mut([name, "--log=", "--sdk-disabled=true"])?;
    let options = Options::from_arg_matches_mut(&mut matches)?;
    let status = options.run(&command, Defaults { directives: format!("warn,{TARGET}=trace,").into(), .. }, io::sink, Identity::new(), |filter, level| {
        assert_eq!(level, LevelFilter::WARN);
        assert!(tracing::enabled!(Level::WARN));
        assert!(!tracing::enabled!(Level::INFO));
        assert!(tracing::enabled!(target: TARGET, Level::TRACE));
        let maximum = filter.with_current(EnvFilter::max_level_hint);
        assert!(matches!(maximum, Ok(Some(LevelFilter::TRACE))), "{maximum:?}");
        Ok::<_, NEVec<io::Error>>(())
    });
    assert_eq!(status, ExitCode::SUCCESS);
    Ok(())
}
