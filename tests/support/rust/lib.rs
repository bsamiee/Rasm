#[cfg(feature = "sqlite")]
use core::ffi::{c_char, c_int};
use core::fmt::Debug;
use core::future::ready;
use core::panic::AssertUnwindSafe;
#[cfg(feature = "sqlite")]
use core::{mem, ptr};
use std::path::{Path, PathBuf};
use std::process::{ExitCode, Termination};

use assert_fs::TempDir;
use assert_fs::fixture::FixtureError;
use derive_more::Deref;
use derive_where::derive_where;
use futures::FutureExt as _;
#[cfg(feature = "sqlite")]
use libsqlite3_sys::{SQLITE_OK, sqlite3, sqlite3_api_routines, sqlite3_free};
#[cfg(feature = "sqlite")]
use sqlx::SqlitePool;
#[cfg(feature = "sqlite")]
use sqlx::sqlite::{SqliteConnectOptions, SqlitePoolOptions};

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

#[cfg(feature = "sqlite")]
#[derive(Debug, Deref)]
pub struct Sqlite {
    #[deref]
    pool: SqlitePool,
    folder: Folder,
}

#[cfg(feature = "sqlite")]
impl Fixture for Sqlite {
    type Within = (PathBuf, SqliteConnectOptions);
    type Kept = PathBuf;
    type Error = FixtureError;

    async fn created((parent, options): &(PathBuf, SqliteConnectOptions)) -> Result<Self, FixtureError> {
        let folder = Folder::created(parent).await?;
        let file = options.clone().filename(folder.path().join("store.sqlite")).create_if_missing(true);
        let vectored = SqlitePoolOptions::new().after_connect(|connection, _| {
            Box::pin(async move {
                type Entry = unsafe extern "C" fn(*mut sqlite3, *mut *mut c_char, *const sqlite3_api_routines) -> c_int;
                let mut locked = connection.lock_handle().await?;
                #[expect(
                    unsafe_code,
                    reason = "sqlite-vec builds with SQLITE_CORE: its entry point registers `vec0` on this one live, locked connection, reads no API table, and its error text is freed here"
                )]
                let code = unsafe {
                    let mut text = ptr::null_mut::<c_char>();
                    let code = mem::transmute::<unsafe extern "C" fn(), Entry>(sqlite_vec::sqlite3_vec_init)(locked.as_raw_handle().as_ptr(), &raw mut text, ptr::null());
                    sqlite3_free(text.cast());
                    code
                };
                (code == SQLITE_OK).ok_or_else(|| sqlx::Error::Configuration(Box::new(libsqlite3_sys::Error::new(code))))
            })
        });
        Ok(Self { pool: vectored.connect_lazy_with(file), folder })
    }

    async fn dropped(self) -> Result<(), FixtureError> {
        let Self { pool, folder } = self;
        pool.close().await;
        folder.dropped().await
    }

    async fn kept(self) -> Result<PathBuf, FixtureError> { self.folder.kept().await }
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
