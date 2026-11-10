use std::fs::TryLockError;
use std::io;
use std::path::{self, Path, PathBuf};

use backon::{BackoffBuilder, Retryable as _};
use interprocess::local_socket::tokio::{Listener, Stream};
use interprocess::local_socket::traits::tokio::Stream as _;
use interprocess::local_socket::{ListenerOptions, Name};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum IpcOperation {
    #[display("create the instance folder")]
    Folder,
    #[display("take the instance lock")]
    Lock,
    #[display("listen on the local socket")]
    Listen,
    #[display("reach the service")]
    Connect,
    #[display("start the service")]
    Start,
    #[display("send the opening message")]
    Send,
    #[display("receive the opening message")]
    Receive,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Kind {
    Runtime,
    Cache,
    Logs,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, Hash, serde::Serialize)]
pub struct Folders {
    pub runtime: PathBuf,
    pub cache: PathBuf,
    pub logs: PathBuf,
}

impl Folders {
    pub fn resolve(application: &str, runtime: Option<PathBuf>, cache: Option<PathBuf>, logs: Option<PathBuf>) -> Result<Self, IpcError> {
        let resolved = |kind: Kind, stated: Option<PathBuf>, platform: Option<PathBuf>| match stated {
            Some(folder) => path::absolute(folder).map_err(|source| IpcError::Io { operation: IpcOperation::Folder, source }),
            None => platform.map(|base| base.join(application)).ok_or(IpcError::Folder { kind }),
        };
        let journal = cfg_select! {
            target_vendor = "apple" => dirs::home_dir().map(|home| home.join("Library").join("Logs")),
            windows => dirs::data_local_dir().map(|data| data.join("logs")),
            _ => dirs::state_dir(),
        };
        Ok(Self {
            runtime: resolved(Kind::Runtime, runtime, dirs::runtime_dir().or_else(dirs::cache_dir))?,
            cache: resolved(Kind::Cache, cache, dirs::cache_dir())?,
            logs: resolved(Kind::Logs, logs, journal)?,
        })
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum IpcError {
    #[error("cannot {operation}")]
    Io { operation: IpcOperation, source: io::Error },
    #[error("platform names no per-user {kind} folder, state one")]
    Folder { kind: Kind },
    #[error("cannot {operation} before the deadline")]
    Deadline { operation: IpcOperation },
    #[error("cannot encode the opening message")]
    Encode { source: serde_json::Error },
    #[error("cannot decode the opening message")]
    Decode { source: serde_path_to_error::Error<serde_json::Error> },
    #[error("opening message exceeds {limit} bytes")]
    Oversized { limit: usize },
    #[error("service closed the connection before answering")]
    Closed,
    #[error(transparent)]
    Refused(#[from] Refusal),
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Instance(fs_err::File);

impl Instance {
    pub async fn acquire<B>(lock: &Path, wait: B) -> Result<Self, IpcError>
    where B: BackoffBuilder {
        let failed = |operation| move |source| IpcError::Io { operation, source };
        if let Some(folder) = lock.parent() {
            let mut builder = fs_err::tokio::DirBuilder::new();
            builder.recursive(true);
            #[cfg(unix)]
            builder.mode(0o700);
            builder.create(folder).await.map_err(failed(IpcOperation::Folder))?;
        }
        let file = fs_err::tokio::File::create(lock).await.map_err(failed(IpcOperation::Lock))?.into_std().await;
        let held = (|| core::future::ready(file.try_lock())).retry(wait).when(|refused| matches!(refused, TryLockError::WouldBlock)).await;
        match held {
            Ok(()) => Ok(Self(file)),
            Err(TryLockError::WouldBlock) => Err(IpcError::Deadline { operation: IpcOperation::Lock }),
            Err(TryLockError::Error(source)) => Err(failed(IpcOperation::Lock)(source)),
        }
    }

    pub async fn listen(self, name: Name<'_>) -> Result<Option<Listener>, IpcError> {
        let Self(_held) = self;
        match Stream::connect(name.borrow()).await {
            Ok(_serving) => Ok(None),
            Err(error) if absent(&error) => ListenerOptions::new().name(name).try_overwrite(true).create_tokio().map(Some).map_err(|source| IpcError::Io { operation: IpcOperation::Listen, source }),
            Err(source) => Err(IpcError::Io { operation: IpcOperation::Connect, source }),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn absent(error: &io::Error) -> bool { matches!(error.kind(), io::ErrorKind::NotFound | io::ErrorKind::ConnectionRefused) }

pub async fn connect<B>(name: &Name<'_>, retry: B, start: impl FnOnce() -> io::Result<()>) -> Result<Stream, IpcError>
where B: BackoffBuilder {
    match Stream::connect(name.borrow()).await {
        Ok(stream) => Ok(stream),
        Err(error) if absent(&error) => {
            start().map_err(|source| IpcError::Io { operation: IpcOperation::Start, source })?;
            (|| Stream::connect(name.borrow())).retry(retry).when(absent).await.map_err(|source| IpcError::Io { operation: IpcOperation::Connect, source })
        }
        Err(source) => Err(IpcError::Io { operation: IpcOperation::Connect, source }),
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod hello;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use hello::{Hello, Limits, Opened, Refusal, accept, greet, reply};
