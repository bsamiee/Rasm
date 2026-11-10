use core::cell::Cell;
use std::fs::TryLockError;
use std::io;
#[cfg(unix)]
use std::os::fd::{AsFd, OwnedFd};
use std::path::{Path, PathBuf};

use backon::{BackoffBuilder, Retryable as _};
use interprocess::local_socket::tokio::{Listener, Stream};
use interprocess::local_socket::traits::tokio::Stream as _;
#[cfg(not(windows))]
use interprocess::local_socket::{GenericFilePath, ToFsName as _};
#[cfg(windows)]
use interprocess::local_socket::{GenericNamespaced, ToNsName as _};
use interprocess::local_socket::{ListenerOptions, Name};
use serde::Serialize;
use serde::de::DeserializeOwned;
#[cfg(unix)]
use socket2::SockRef;
use tokio::time::error::Elapsed;

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

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error, derive_more::IsVariant)]
pub enum IpcError {
    #[error("cannot {operation}")]
    Io { operation: IpcOperation, source: io::Error },
    #[error("another process held the instance lock past the wait")]
    Busy,
    #[error("cannot {operation} before the deadline")]
    Deadline { operation: IpcOperation, source: Elapsed },
    #[error("cannot encode the opening message")]
    Encode { source: serde_json::Error },
    #[error("cannot decode the opening message")]
    Decode { source: serde_path_to_error::Error<serde_json::Error> },
    #[error("opening message exceeds {limit} bytes")]
    Oversized { limit: u64 },
    #[error("service closed the connection before answering")]
    Closed,
    #[error(transparent)]
    Refused(#[from] Refusal),
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Endpoint {
    pub lock: PathBuf,
    pub name: Name<'static>,
    pub buffer: Option<usize>,
}

impl Endpoint {
    pub fn new(folder: &Path, instance: &str, buffer: Option<usize>) -> Result<Self, IpcError> {
        let name = cfg_select! {
            windows => instance.to_owned().to_ns_name::<GenericNamespaced>(),
            _ => folder.join(instance).with_added_extension("sock").to_fs_name::<GenericFilePath>(),
        };
        Ok(Self { lock: folder.join(instance).with_added_extension("lock"), name: name.map_err(|source| IpcError::Io { operation: IpcOperation::Listen, source })?, buffer })
    }
}

#[derive(Debug)]
pub struct Instance {
    pub listener: Listener,
    pub held: fs_err::File,
}

impl Instance {
    pub async fn claim<B>(endpoint: &Endpoint, wait: B) -> Result<Option<Self>, IpcError>
    where B: BackoffBuilder {
        let failed = |operation| move |source| IpcError::Io { operation, source };
        if let Some(folder) = endpoint.lock.parent() {
            let mut builder = fs_err::tokio::DirBuilder::new();
            builder.recursive(true);
            #[cfg(unix)]
            builder.mode(0o700);
            builder.create(folder).await.map_err(failed(IpcOperation::Folder))?;
        }
        let held = fs_err::tokio::File::create(&endpoint.lock).await.map_err(failed(IpcOperation::Lock))?.into_std().await;
        let vacant = async || match held.try_lock() {
            Ok(()) => Ok(true),
            Err(TryLockError::Error(source)) => Err(failed(IpcOperation::Lock)(source)),
            Err(TryLockError::WouldBlock) => match Stream::connect(endpoint.name.borrow()).await {
                Ok(_serving) => Ok(false),
                Err(error) if absent(&error) => Err(IpcError::Busy),
                Err(source) => Err(failed(IpcOperation::Connect)(source)),
            },
        };
        vacant
            .retry(wait)
            .when(IpcError::is_busy)
            .await?
            .then(|| {
                let listener = ListenerOptions::new().name(endpoint.name.borrow()).try_overwrite(true).create_tokio()?;
                #[cfg(unix)]
                if let (Listener::UdSocket(socket), Some(buffer)) = (&listener, endpoint.buffer) {
                    sized(socket, buffer)?;
                }
                Ok(Self { listener, held })
            })
            .transpose()
            .map_err(failed(IpcOperation::Listen))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(unix)]
fn sized(socket: &impl AsFd, buffer: usize) -> io::Result<()> {
    let options = SockRef::from(socket);
    options.set_send_buffer_size(buffer)?;
    options.set_recv_buffer_size(buffer)
}

fn absent(error: &io::Error) -> bool { matches!(error.kind(), io::ErrorKind::NotFound | io::ErrorKind::ConnectionRefused) }

pub async fn attach<B, T, W>(endpoint: &Endpoint, hello: &Hello<B, T>, limits: Limits) -> Result<Option<Opened<Stream, W>>, IpcError>
where
    B: Serialize,
    T: Serialize,
    W: DeserializeOwned,
{
    let connected = Stream::connect(endpoint.name.borrow()).await;
    #[cfg(unix)]
    let connected = match (connected, endpoint.buffer) {
        (Ok(Stream::UdSocket(socket)), Some(buffer)) => OwnedFd::try_from(socket).and_then(|socket| {
            sized(&socket, buffer)?;
            socket.try_into().map(Stream::UdSocket)
        }),
        (connected @ (Ok(Stream::UdSocket(_)) | Err(_)), None) | (connected @ Err(_), Some(_)) => connected,
    };
    match connected {
        Ok(stream) => greet(stream, hello, limits).await.map(Some),
        Err(error) if absent(&error) => Ok(None),
        Err(source) => Err(IpcError::Io { operation: IpcOperation::Connect, source }),
    }
}

pub async fn connect<R, B, T, W>(endpoint: &Endpoint, hello: &Hello<B, T>, limits: Limits, retry: R, start: impl Fn() -> io::Result<()>) -> Result<Opened<Stream, W>, IpcError>
where
    R: BackoffBuilder,
    B: Serialize,
    T: Serialize,
    W: DeserializeOwned,
{
    let started = Cell::new(false);
    let opened = async || match attach(endpoint, hello, limits).await {
        Ok(Some(opened)) => Ok(opened),
        Ok(None) => {
            if !started.replace(true) {
                start().map_err(|source| IpcError::Io { operation: IpcOperation::Start, source })?;
            }
            Err(IpcError::Io { operation: IpcOperation::Connect, source: io::ErrorKind::NotFound.into() })
        }
        Err(error) => {
            started.set(false);
            Err(error)
        }
    };
    let replaced = |refused: &IpcError| match refused {
        IpcError::Refused(Refusal::Superseded) | IpcError::Closed => true,
        IpcError::Io { operation: IpcOperation::Connect | IpcOperation::Send | IpcOperation::Receive, source } => source.kind() != io::ErrorKind::InvalidInput,
        IpcError::Refused(Refusal::Protocol { offered: _, served: _ } | Refusal::Service { reason: _ })
        | IpcError::Io { operation: IpcOperation::Folder | IpcOperation::Lock | IpcOperation::Listen | IpcOperation::Start, source: _ }
        | IpcError::Busy
        | IpcError::Deadline { operation: _, source: _ }
        | IpcError::Encode { source: _ }
        | IpcError::Decode { source: _ }
        | IpcError::Oversized { limit: _ } => false,
    };
    opened.retry(retry).when(replaced).await
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod hello;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use hello::{Hello, Limits, Opened, Refusal, accept, greet, reply};
