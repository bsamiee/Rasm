use core::hint::black_box;
use std::{
    io,
    path::{Path, PathBuf},
};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn method(path: &Path) -> io::Result<PathBuf> { path.canonicalize() }

fn owned(path: &PathBuf) -> io::Result<PathBuf> { path.canonicalize() }

fn qualified(path: &Path) -> io::Result<PathBuf> { Path::canonicalize(path) }

fn type_relative(path: &PathBuf) -> io::Result<PathBuf> { <Path>::canonicalize(path) }

fn mapped(paths: &[&Path]) -> Vec<io::Result<PathBuf>> { paths.iter().copied().map(Path::canonicalize).collect() }

fn asserted(path: &Path) -> bool { matches!(path.canonicalize(), Ok(_)) }

fn tracked(path: &Path) -> io::Result<PathBuf> { fs_err::canonicalize(path) }

fn function(path: &Path) -> io::Result<PathBuf> { std::fs::canonicalize(path) }

// --- [COMPOSITION] ---------------------------------------------------------------------

macro_rules! canonical {
    ($path:expr) => {
        $path.canonicalize()
    };
}

fn main() {
    let path = Path::new(".");
    let owned_path = path.to_path_buf();
    black_box(&(method(path), owned(&owned_path), qualified(path), type_relative(&owned_path), mapped(&[path]), asserted(path), tracked(path), function(path), canonical!(path)));
}
