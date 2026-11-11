use core::fmt;
use std::path::{Path, PathBuf};

use claims::{assert_err, assert_matches, assert_ok, assert_some};
use futures::future;
use rasm_testing::{Failure, Fixture as _, Folder, Sqlite};
use sqlx::sqlite::SqliteConnectOptions;
use tokio::sync::oneshot;

// --- [OPERATIONS] ----------------------------------------------------------------------

#[tokio::test]
async fn sqlite_store_is_one_file_every_connection_shares() -> Result<(), Failure<Sqlite>> {
    Sqlite::scoped(&(PathBuf::from(env!("CARGO_TARGET_TMPDIR")), SqliteConnectOptions::new()), async |store| {
        let (writer, reader) = future::join(store.acquire(), store.acquire()).await;
        let (mut writer, mut reader) = (assert_ok!(writer), assert_ok!(reader));
        assert_ok!(sqlx::query("create table own (value integer)").execute(&mut *writer).await);
        assert_ok!(sqlx::query("insert into own values (42)").execute(&mut *writer).await);
        let read: i64 = assert_ok!(sqlx::query_scalar("select value from own").fetch_one(&mut *reader).await);
        assert_eq!(read, 42, "a second connection reads the row the first wrote");
    })
    .await
}

#[tokio::test]
async fn sqlite_store_ranks_vectors_on_every_connection() -> Result<(), Failure<Sqlite>> {
    Sqlite::scoped(&(PathBuf::from(env!("CARGO_TARGET_TMPDIR")), SqliteConnectOptions::new()), async |store| {
        let (writer, reader) = future::join(store.acquire(), store.acquire()).await;
        let (mut writer, mut reader) = (assert_ok!(writer), assert_ok!(reader));
        assert_ok!(sqlx::query("create virtual table held using vec0(embedding float[2])").execute(&mut *writer).await);
        assert_ok!(sqlx::query("insert into held (rowid, embedding) values (1, '[0, 0]'), (2, '[3, 4]'), (3, '[1, 1]')").execute(&mut *writer).await);
        let nearest: Vec<i64> = assert_ok!(sqlx::query_scalar("select rowid from held where embedding match '[1, 1]' and k = 2 order by distance").fetch_all(&mut *reader).await);
        assert_eq!(nearest, [3, 1], "a second connection of the pool ranks the rows by distance through `vec0`");
    })
    .await
}

#[tokio::test]
async fn passed_body_has_its_file_removed() {
    let (seen, file) = oneshot::channel();
    let within = (PathBuf::from(env!("CARGO_TARGET_TMPDIR")), SqliteConnectOptions::new());
    let passed = Sqlite::scoped(&within, async |store| {
        assert_ok!(sqlx::query("create table own (value integer)").execute(&**store).await);
        assert_ok!(seen.send(store.connect_options().get_filename().to_owned()));
    });
    assert_ok!(passed.await, "a passed body removes its folder");
    assert_err!(fs_err::tokio::metadata(assert_ok!(file.await)).await, "the file of a passed body is removed with its folder");
}

#[tokio::test]
async fn panicked_body_has_its_file_kept() {
    let (seen, file) = oneshot::channel();
    let within = (PathBuf::from(env!("CARGO_TARGET_TMPDIR")), SqliteConnectOptions::new());
    let body = Sqlite::scoped(&within, async |store| {
        assert_ok!(sqlx::query("create table own (value integer)").execute(&**store).await);
        assert_ok!(seen.send(store.connect_options().get_filename().to_owned()));
        assert!(store.is_closed(), "this body fails once its file exists");
    });
    assert_matches!(body.await, Err(Failure::Kept(_)), "a panicked body keeps its folder");
    let file = assert_ok!(file.await);
    assert!(assert_ok!(fs_err::tokio::metadata(&file).await).is_file(), "the file of a failed body is kept");
    assert_ok!(fs_err::tokio::remove_dir_all(assert_some!(file.parent())).await);
}

#[tokio::test]
async fn refused_body_has_its_folder_kept() {
    let (seen, folder) = oneshot::channel();
    let refused = Folder::scoped(Path::new(env!("CARGO_TARGET_TMPDIR")), async |folder| {
        assert_ok!(fs_err::tokio::write(folder.join("log"), "line").await);
        assert_ok!(seen.send(folder.to_path_buf()));
        Err::<(), _>(fmt::Error)
    });
    assert_matches!(refused.await, Err(Failure::Kept(_)), "a body returning `Err` keeps its folder");
    let folder = assert_ok!(folder.await);
    assert_eq!(assert_ok!(fs_err::tokio::read_to_string(folder.join("log")).await), "line", "the folder of a failed body keeps its files");
    assert_ok!(fs_err::tokio::remove_dir_all(folder).await);
}
