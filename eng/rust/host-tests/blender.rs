use std::{ffi::OsString, io, path::PathBuf};

use clap::{Args, ValueHint};
use fs_err::tokio::canonicalize;
use futures::future::join;
use rasm_process::report::Run;
use tokio::process::Command;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args)]
pub(crate) struct Suite {
    #[arg(long, env = "BLENDER_PATH", value_hint = ValueHint::ExecutablePath)]
    executable: PathBuf,
    #[arg(value_hint = ValueHint::FilePath)]
    script: PathBuf,
    #[arg(trailing_var_arg = true, allow_hyphen_values = true)]
    arguments: Vec<OsString>,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn test(Suite { executable, script, arguments }: Suite, run: &Run) -> io::Result<()> {
    let (executable, script) = match join(canonicalize(executable), canonicalize(script)).await {
        (Ok(executable), Ok(script)) => (executable, script),
        (Err(executable), Err(script)) => return Err(io::Error::other(format!("{executable}\n{script}"))),
        (Err(error), _) | (_, Err(error)) => return Err(error),
    };
    let active = run.directory().join("active-case");
    let mut command = Command::new(executable);
    command
        .current_dir(run.scratch())
        .args(["--background", "--factory-startup", "--offline-mode", "--disable-autoexec", "--console-crash-handler", "--python-exit-code", "1", "--python"])
        .arg(script)
        .arg("--")
        .args(arguments)
        .env("BLENDER_USER_RESOURCES", run.scratch())
        .env("TMPDIR", run.scratch())
        .env("TEMP", run.scratch())
        .env("TMP", run.scratch())
        .env("RASM_HOST_RUN_DIRECTORY", run.directory())
        .env("RASM_HOST_RUN_SCRATCH", run.scratch())
        .env("RASM_HOST_ACTIVE_CASE", &active);
    for name in ["BLENDER_USER_CONFIG", "BLENDER_USER_DATAFILES", "BLENDER_USER_EXTENSIONS", "BLENDER_USER_SCRIPTS"] {
        command.env_remove(name);
    }
    for kind in ["CACHE", "CONFIG", "DATA", "STATE"] {
        command.env(format!("XDG_{kind}_HOME"), run.scratch().join(kind.to_ascii_lowercase()));
    }
    run.command(command, "blender", Some(&active)).await.map(drop)
}
