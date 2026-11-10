#![feature(exit_status_error)]

use core::fmt::Debug;
use core::num::TryFromIntError;
use core::panic::AssertUnwindSafe;
use std::io;
use std::path::PathBuf;
use std::process::{ExitCode, Termination};

use futures::FutureExt as _;
use tonic::Status;
use uuid::Uuid;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Fixture: Sized {
    type Within: ?Sized;
    type Kept: Debug;

    fn created(within: &Self::Within) -> impl Future<Output = Result<Self, FixtureError>>;

    fn dropped(self) -> impl Future<Output = Result<(), FixtureError>>;

    fn kept(self) -> impl Future<Output = Result<Self::Kept, FixtureError>>;

    fn scoped<T>(within: &Self::Within, test: impl AsyncFnOnce(&Self) -> T) -> impl Future<Output = Result<(), Failure<Self::Kept>>>
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

#[derive(Debug, Clone, PartialEq, Eq, derive_more::Display)]
pub enum Operation {
    #[display("end session {_0}")]
    End(Uuid),
    #[display("create a store in session {_0}")]
    Create(Uuid),
    #[display("open template {_0}")]
    Open(String),
    #[display("seal template {_0}")]
    Seal(String),
    #[display("clone template {_0}")]
    Clone(String),
    #[display("keep store {_0}")]
    Keep(String),
    #[display("drop store {_0}")]
    Drop(String),
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug)]
pub enum Failure<K> {
    Uncreated(Box<FixtureError>),
    Undropped(Box<FixtureError>),
    Unkept(Box<FixtureError>),
    Kept(Box<K>),
}

#[derive(Debug, thiserror::Error)]
pub enum FixtureError {
    #[error("store service at {endpoint} granted no session")]
    Silent { endpoint: http::Uri, source: Option<Box<Status>> },
    #[error("store service refused to {operation}")]
    Refused { operation: Operation, source: Box<Status> },
    #[error("store {store} is held by a client, {held} connections of its pool still checked out")]
    Leaked { store: String, held: usize, source: Box<Status> },
    #[error(transparent)]
    Endpoint(#[from] clap::Error),
    #[error(transparent)]
    Address(#[from] http::Error),
    #[error("store {store} states a port above 65535")]
    Port { store: String, source: TryFromIntError },
    #[error("server request states no `{variable}`")]
    Unstated { variable: &'static str },
    #[error("docker compose resolved no model of {}", file.display())]
    Compose { file: PathBuf, source: io::Error },
    #[error("docker compose printed a model of {} this crate cannot read", file.display())]
    Model { file: PathBuf, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("{} states no image for service {service}", file.display())]
    Unlisted { file: PathBuf, service: String },
    #[error(transparent)]
    Image(#[from] oci_spec::distribution::ParseError),
    #[error(transparent)]
    Server(#[from] testcontainers::TestcontainersError),
    #[error(transparent)]
    Postgres(#[from] sqlx::Error),
    #[error(transparent)]
    Clickhouse(#[from] clickhouse::error::Error),
    #[error(transparent)]
    Bucket(#[from] object_store::Error),
    #[error(transparent)]
    Cache(#[from] redis::RedisError),
    #[error(transparent)]
    Folder(#[from] assert_fs::fixture::FixtureError),
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod server;
mod sqlite;
mod store;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use server::{Hosted, defined};
pub use sqlite::{Folder, Sqlite};
pub use store::{Cache, Cloned, Endpoint, Kind, Session, Source, Step, Store, Template, Templated};
