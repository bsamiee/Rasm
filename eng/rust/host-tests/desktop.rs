use core::num::NonZero;
use std::{env, io, path::PathBuf};

use fs_err::tokio::canonicalize;
use rasm_process::{Launch, Run};
use tokio::{net::UnixListener, process::Command};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn socket(run: &Run) -> io::Result<(PathBuf, UnixListener)> {
    let path = run.scratch().join("socket");
    UnixListener::bind(&path).map(|listener| (path, listener))
}

pub(crate) async fn with_app<T>(run: &Run, launch: Launch<'_>, work: impl AsyncFnOnce(NonZero<u32>) -> io::Result<T>) -> io::Result<T> {
    let build = env::var_os("SWIFTPM_BUILD_DIR").ok_or_else(|| io::Error::other("Missing SwiftPM build directory"))?;
    let mut watcher = Command::new(canonicalize(PathBuf::from(build).join("debug/watch-apps")).await?);
    watcher.env("WATCH_APPS_LOG", run.directory().join("watch-apps.log"));
    run.application(watcher, launch, work).await
}
