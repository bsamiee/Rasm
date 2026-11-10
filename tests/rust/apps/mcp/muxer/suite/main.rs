#![feature(error_reporter)]

use std::path::{Path, PathBuf};
use std::process::Termination;

use mcp_muxer_tests::Isolated;
use rasm_test_support::{Failure, Fixture as _, Folder};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(test)]
async fn scoped<T>(test: impl AsyncFnOnce(&Isolated) -> T) -> Result<(), Failure<PathBuf>>
where T: Termination {
    let ended = async |folder: &Folder| {
        let isolated = Isolated::new(folder.path())?;
        let outcome = test(&isolated).await;
        isolated.ended().await.map(|_ended| outcome)
    };
    Folder::scoped(Path::new(env!("CARGO_TARGET_TMPDIR")), ended).await
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod load;
mod muxer;
mod proxy;
