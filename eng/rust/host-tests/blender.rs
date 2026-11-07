use std::{ffi::OsString, io, path::PathBuf};

use clap::{Args, ValueHint};
use rasm_process::report::Run;
use tokio::process::Command;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args)]
pub(crate) struct Suite {
    #[arg(value_hint = ValueHint::FilePath)]
    script: PathBuf,
    #[arg(trailing_var_arg = true, allow_hyphen_values = true)]
    arguments: Vec<OsString>,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn test(Suite { script, arguments }: Suite, run: &Run) -> io::Result<()> {
    let active = run.directory().join("active-case");
    let profile = run.directory().file_name().ok_or_else(|| io::Error::other("Run directory has no name"))?;
    let mut command = Command::new("python");
    command
        .args(["-m", "eng.python.blender", "run"])
        .arg(profile)
        .args(["--", "--background", "--factory-startup", "--offline-mode", "--disable-autoexec", "--console-crash-handler", "--python-exit-code", "1", "--python"])
        .arg(script)
        .arg("--")
        .args(arguments)
        .env("RASM_HOST_RUN_DIRECTORY", run.directory())
        .env("RASM_HOST_RUN_SCRATCH", run.scratch())
        .env("RASM_HOST_ACTIVE_CASE", &active);
    run.command(command, Some(&active)).await.map(drop)
}
