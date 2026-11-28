use core::panic::AssertUnwindSafe;
use std::path::{Path, PathBuf};
use std::process::{ExitCode, Termination};

use assert_fs::TempDir;
use assert_fs::fixture::FixtureError;
use derive_more::Deref;
use futures::FutureExt as _;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Deref)]
pub struct Folder(TempDir);

impl Folder {
    pub async fn scoped<T>(within: &Path, test: impl AsyncFnOnce(&Self) -> T) -> Result<(), Failure>
    where T: Termination {
        let folder = TempDir::new_in(within).map(Self).map_err(Failure::Uncreated)?;
        let passed = AssertUnwindSafe(test(&folder)).catch_unwind().await.is_ok_and(|outcome| outcome.report() == ExitCode::SUCCESS);
        if passed { folder.0.close().map_err(Failure::Undropped) } else { Err(Failure::Kept(folder.0.into_persistent().to_path_buf())) }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug)]
pub enum Failure {
    Uncreated(FixtureError),
    Undropped(FixtureError),
    Kept(PathBuf),
}
