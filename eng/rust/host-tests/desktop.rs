use core::num::NonZero;
use std::{
    env, io,
    path::{Path, PathBuf},
};

use fs_err::tokio::{canonicalize, create_dir_all};
use nix::unistd::{User, getuid};
use rasm_process::{macos::Launch, report::Run};
use tempfile::TempDir;
use tokio::{net::UnixListener, process::Command};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn login_home() -> io::Result<PathBuf> { User::from_uid(getuid()).map_err(io::Error::other)?.map(|user| user.dir).ok_or_else(|| io::Error::other("No desktop user home directory")) }

pub(crate) fn socket(scratch: &Path) -> io::Result<(TempDir, PathBuf, UnixListener)> {
    let temporary = env::var_os("TMPDIR");
    let directory = tempfile::tempdir_in(temporary.as_deref().map_or(scratch, Path::new))?;
    let path = directory.path().join("socket");
    let listener = UnixListener::bind(&path)?;
    Ok((directory, path, listener))
}

pub(crate) async fn with_app<T>(run: &Run, launch: Launch<'_>, work: impl AsyncFnOnce(NonZero<u32>) -> io::Result<T>) -> io::Result<T> {
    let build = env::var_os("SWIFTPM_BUILD_DIR").ok_or_else(|| io::Error::other("Missing SwiftPM build directory"))?;
    let helper = canonicalize(PathBuf::from(build).join("debug/watch-apps")).await?;
    let cache = login_home()?.join("Library/Caches/Rasm");
    create_dir_all(&cache).await?;
    let mut watcher = Command::new("/usr/bin/lockf");
    watcher.args(["-t", "0", "-k"]).arg(cache.join("desktop-tests.lock")).arg(helper).env("WATCH_APPS_LOG", run.directory().join("watch-apps.log"));
    run.application(watcher, launch, work).await
}
