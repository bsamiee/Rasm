use core::fmt;
use std::io;
use std::path::{Path, PathBuf};

use assert_fs::TempDir;
use claims::{assert_err, assert_matches, assert_ok, assert_some};
use rasm_testing::{Failure, Fixture as _, Folder};
use rstest::rstest;

// --- [OPERATIONS] ----------------------------------------------------------------------

#[tokio::test]
async fn passed_body_has_its_folder_removed() {
    let mut seen = None;
    let passed = Folder::scoped(Path::new(env!("CARGO_TARGET_TMPDIR")), async |folder| {
        assert_ok!(fs_err::tokio::write(folder.join("log"), "line").await);
        seen = Some(folder.to_path_buf());
    });
    assert_ok!(passed.await, "a passed body removes its folder");
    let removed = assert_err!(fs_err::tokio::metadata(assert_some!(seen)).await);
    assert_eq!(removed.kind(), io::ErrorKind::NotFound, "the folder of a passed body is gone with its files");
}

#[rstest]
#[case::returned_err(false)]
#[case::panicked(true)]
#[tokio::test]
async fn failed_body_has_its_folder_kept_with_its_files(#[case] panics: bool) {
    let mut seen = None;
    let failed = Folder::scoped(Path::new(env!("CARGO_TARGET_TMPDIR")), async |folder| {
        assert_ok!(fs_err::tokio::write(folder.join("log"), "line").await);
        seen = Some(folder.to_path_buf());
        assert!(!panics, "this body fails by panicking once its file exists");
        Err::<(), _>(fmt::Error)
    });
    let kept = match failed.await {
        Err(Failure::Kept(kept)) => Some(*kept),
        Ok(()) | Err(Failure::Uncreated(_) | Failure::Undropped(_) | Failure::Unkept(_)) => None,
    };
    let kept = assert_some!(kept, "a failed body keeps its folder");
    assert_eq!(Some(&kept), seen.as_ref(), "the kept path is the folder the body wrote to");
    assert_eq!(assert_ok!(fs_err::tokio::read_to_string(kept.join("log")).await), "line", "the folder of a failed body keeps its files");
    assert_ok!(fs_err::tokio::remove_dir_all(kept).await);
}

#[tokio::test]
async fn folder_under_a_missing_parent_is_uncreated_and_its_body_never_runs() {
    let within = assert_ok!(TempDir::new_in(env!("CARGO_TARGET_TMPDIR")));
    let (mut ran, missing): (bool, PathBuf) = (false, within.join("missing/parent"));
    let uncreated = Folder::scoped(&missing, async |_| ran = true);
    assert_matches!(uncreated.await, Err(Failure::Uncreated(_)), "a folder whose parent is missing is never created");
    assert!(!ran, "a body without its fixture never runs");
}
