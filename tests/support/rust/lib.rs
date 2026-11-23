use core::fmt::Debug;
use core::future::ready;
use core::panic::AssertUnwindSafe;
use std::path::{Path, PathBuf};
use std::process::{ExitCode, Termination};

use assert_fs::TempDir;
use assert_fs::fixture::FixtureError;
use derive_more::Deref;
use derive_where::derive_where;
use futures::FutureExt as _;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Fixture: Sized {
    type Within: ?Sized;
    type Kept: Debug;
    type Error: Debug;

    fn created(within: &Self::Within) -> impl Future<Output = Result<Self, Self::Error>>;

    fn dropped(self) -> impl Future<Output = Result<(), Self::Error>>;

    fn kept(self) -> impl Future<Output = Result<Self::Kept, Self::Error>>;

    fn scoped<T>(within: &Self::Within, test: impl AsyncFnOnce(&Self) -> T) -> impl Future<Output = Result<(), Failure<Self>>>
    where T: Termination {
        async move {
            let fixture = Self::created(within).await.map_err(|error| Failure::Uncreated(Box::new(error)))?;
            let passed = AssertUnwindSafe(test(&fixture)).catch_unwind().await.is_ok_and(|outcome| outcome.report() == ExitCode::SUCCESS);
            if passed {
                fixture.dropped().await.map_err(|error| Failure::Undropped(Box::new(error)))
            } else {
                Err(fixture.kept().await.map_or_else(|error| Failure::Unkept(Box::new(error)), |kept| Failure::Kept(Box::new(kept))))
            }
        }
    }
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Deref)]
pub struct Folder(TempDir);

impl Fixture for Folder {
    type Within = Path;
    type Kept = PathBuf;
    type Error = FixtureError;

    fn created(within: &Path) -> impl Future<Output = Result<Self, FixtureError>> { ready(TempDir::new_in(within).map(Self)) }

    fn dropped(self) -> impl Future<Output = Result<(), FixtureError>> { ready(self.0.close()) }

    fn kept(self) -> impl Future<Output = Result<PathBuf, FixtureError>> { ready(Ok(self.0.into_persistent().to_path_buf())) }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive_where(Debug)]
pub enum Failure<F>
where F: Fixture
{
    Uncreated(Box<F::Error>),
    Undropped(Box<F::Error>),
    Unkept(Box<F::Error>),
    Kept(Box<F::Kept>),
}
