use std::{
    ffi::{OsStr, OsString},
    fs, io,
    path::{Path, PathBuf},
};

use fs_err::tokio::read_dir;
use ignore::Walk;
use thiserror::Error;
use tokio::fs as tokio_fs;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
enum ListingError {
    #[error("cannot read the directory")]
    Read(#[from] io::Error),
    #[error("cannot walk the directory")]
    Walk(#[from] ignore::Error),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn std_names(root: &Path) -> io::Result<Vec<Option<OsString>>> { fs::read_dir(root)?.map(|entry| Ok(entry?.path().file_name().map(OsStr::to_os_string))).collect() }

fn walked_names(root: &Path) -> Result<Vec<Option<OsString>>, ignore::Error> { Walk::new(root).map(|entry| Ok(entry?.path().file_name().map(OsStr::to_os_string))).collect() }

fn bound_names(root: &Path) -> io::Result<Vec<Option<OsString>>> {
    fs::read_dir(root)?
        .map(|entry| {
            let entry = entry?;
            let path = entry.path();
            Ok(path.file_name().map(OsStr::to_os_string))
        })
        .collect()
}

fn reused_names(root: &Path) -> io::Result<Vec<(PathBuf, Option<OsString>)>> {
    fs::read_dir(root)?
        .map(|entry| {
            let path = entry?.path();
            let name = path.file_name().map(OsStr::to_os_string);
            Ok((path, name))
        })
        .collect()
}

fn manifest_count(root: &Path) -> io::Result<usize> { fs::read_dir(root)?.try_fold(0, |count, entry| Ok(count + usize::from(entry?.path().file_name() == Some(OsStr::new("Cargo.toml"))))) }

fn walked_manifest_count(root: &Path) -> Result<usize, ignore::Error> {
    Walk::new(root).try_fold(0, |count, entry| Ok(count + usize::from(entry?.path().file_name() == Some(OsStr::new("Cargo.toml")))))
}

fn checked_names(root: &Path) -> io::Result<Vec<Option<OsString>>> { fs_err::read_dir(root)?.map(|entry| Ok(entry?.path().file_name().map(OsStr::to_os_string))).collect() }

async fn tokio_names(root: &Path) -> io::Result<Vec<Option<OsString>>> {
    let mut entries = tokio_fs::read_dir(root).await?;
    let mut names = Vec::new();
    while let Some(entry) = entries.next_entry().await? {
        names.push(entry.path().file_name().map(OsStr::to_os_string));
    }
    Ok(names)
}

async fn checked_tokio_names(root: &Path) -> io::Result<Vec<Option<OsString>>> {
    let mut entries = read_dir(root).await?;
    let mut names = Vec::new();
    while let Some(entry) = entries.next_entry().await? {
        names.push(entry.path().file_name().map(OsStr::to_os_string));
    }
    Ok(names)
}

fn direct_names(root: &Path) -> Result<(Vec<OsString>, Vec<OsString>), ListingError> {
    let listed = fs::read_dir(root)?.map(|entry| Ok(entry?.file_name())).collect::<io::Result<_>>()?;
    let walked = Walk::new(root).map(|entry| Ok(entry?.file_name().to_os_string())).collect::<Result<_, ignore::Error>>()?;
    Ok((listed, walked))
}

fn path_name(root: &Path) -> Option<OsString> { PathBuf::from(root).as_path().file_name().map(OsStr::to_os_string) }

fn parent_count(root: &Path) -> Result<usize, ListingError> {
    let listed = fs::read_dir(root)?.try_fold(0, |count, entry| entry.map(|entry| count + usize::from(entry.path().parent().is_some() && entry.path().exists())))?;
    let walked = Walk::new(root).try_fold(0, |count, entry| entry.map(|entry| count + usize::from(entry.path().parent().is_some())))?;
    Ok(listed + walked)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() -> Result<(), ListingError> {
    let root = Path::new(".");
    std_names(root)?;
    walked_names(root)?;
    bound_names(root)?;
    reused_names(root)?;
    manifest_count(root)?;
    walked_manifest_count(root)?;
    checked_names(root)?;
    tokio_names(root).await?;
    checked_tokio_names(root).await?;
    direct_names(root)?;
    path_name(root);
    parent_count(root)?;
    Ok(())
}
