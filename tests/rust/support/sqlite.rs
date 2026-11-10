use core::ffi::{c_char, c_int};
use core::future::ready;
use core::{mem, ptr};
use std::path::{Path, PathBuf};

use assert_fs::TempDir;
use derive_more::Deref;
use libsqlite3_sys::{SQLITE_OK, sqlite3, sqlite3_api_routines, sqlite3_free};
use sqlx::SqlitePool;
use sqlx::sqlite::{SqliteConnectOptions, SqlitePoolOptions};

use crate::{Fixture, FixtureError};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Deref)]
pub struct Folder(TempDir);

impl Fixture for Folder {
    type Within = Path;
    type Kept = PathBuf;

    fn created(within: &Path) -> impl Future<Output = Result<Self, FixtureError>> { ready(TempDir::new_in(within).map(Self).map_err(FixtureError::from)) }

    fn dropped(self) -> impl Future<Output = Result<(), FixtureError>> { ready(self.0.close().map_err(FixtureError::from)) }

    fn kept(self) -> impl Future<Output = Result<PathBuf, FixtureError>> { ready(Ok(self.0.into_persistent().to_path_buf())) }
}

#[derive(Debug, Deref)]
pub struct Sqlite {
    #[deref]
    pool: SqlitePool,
    folder: Folder,
}

impl Fixture for Sqlite {
    type Within = (PathBuf, SqliteConnectOptions);
    type Kept = PathBuf;

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
