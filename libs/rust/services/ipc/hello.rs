use core::time::Duration;

use serde::de::DeserializeOwned;
use serde::{Deserialize, Serialize};
use tokio::io::{AsyncBufReadExt as _, AsyncRead, AsyncWrite, AsyncWriteExt as _, BufReader, ReadHalf, WriteHalf};
use tokio::time;

use crate::{IpcError, IpcOperation};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy)]
pub struct Limits {
    pub line: usize,
    pub deadline: Duration,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct Hello<B, T> {
    pub protocol: u32,
    pub build: B,
    pub body: T,
}

#[derive(Debug, Serialize, Deserialize)]
enum Reply<W> {
    Welcome(W),
    Refused(Refusal),
}

#[derive(Debug)]
pub struct Opened<S, T> {
    pub body: T,
    pub reader: BufReader<ReadHalf<S>>,
    pub writer: WriteHalf<S>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize, thiserror::Error)]
pub enum Refusal {
    #[error("service speaks opening protocol {served}, this client {offered}")]
    Protocol { offered: u32, served: u32 },
    #[error("a newer build replaces the service, connect again")]
    Superseded,
    #[error("{reason}")]
    Service { reason: String },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn send<W>(writer: &mut W, message: &impl Serialize) -> Result<(), IpcError>
where W: AsyncWrite + Unpin {
    let mut line = serde_json::to_vec(message).map_err(|source| IpcError::Encode { source })?;
    line.push(b'\n');
    writer.write_all(&line).await.map_err(|source| IpcError::Io { operation: IpcOperation::Send, source })
}

async fn receive<R, M>(reader: &mut BufReader<R>, limit: usize) -> Result<M, IpcError>
where
    R: AsyncRead + Unpin,
    M: DeserializeOwned, {
    let mut line = Vec::new();
    let read = tokio::io::AsyncReadExt::take(&mut *reader, u64::try_from(limit).unwrap_or(u64::MAX)).read_until(b'\n', &mut line).await;
    match (read.map_err(|source| IpcError::Io { operation: IpcOperation::Receive, source })?, line.last()) {
        (0, _) => Err(IpcError::Closed),
        (_, Some(b'\n')) => serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_slice(&line)).map_err(|source| IpcError::Decode { source }),
        (_, _) => Err(IpcError::Oversized { limit }),
    }
}

pub async fn greet<S, B, T, W>(stream: S, hello: &Hello<B, T>, limits: Limits) -> Result<Opened<S, W>, IpcError>
where
    S: AsyncRead + AsyncWrite,
    B: Serialize,
    T: Serialize,
    W: DeserializeOwned, {
    let (reader, mut writer) = tokio::io::split(stream);
    let mut reader = BufReader::new(reader);
    let exchanged = async {
        send(&mut writer, hello).await?;
        receive::<_, Reply<W>>(&mut reader, limits.line).await
    };
    match time::timeout(limits.deadline, exchanged).await {
        Ok(Ok(Reply::Welcome(body))) => Ok(Opened { body, reader, writer }),
        Ok(Ok(Reply::Refused(refusal))) => Err(refusal.into()),
        Ok(Err(error)) => Err(error),
        Err(time::error::Elapsed { .. }) => Err(IpcError::Deadline { operation: IpcOperation::Receive }),
    }
}

pub async fn accept<S, B, T>(stream: S, protocol: u32, build: &B, limits: Limits) -> Result<Opened<S, T>, IpcError>
where
    S: AsyncRead + AsyncWrite,
    B: DeserializeOwned + Ord,
    T: DeserializeOwned, {
    let (reader, mut writer) = tokio::io::split(stream);
    let mut reader = BufReader::new(reader);
    let Hello { protocol: offered, build: offering, body } = match time::timeout(limits.deadline, receive::<_, Hello<B, T>>(&mut reader, limits.line)).await {
        Ok(received) => received?,
        Err(time::error::Elapsed { .. }) => return Err(IpcError::Deadline { operation: IpcOperation::Receive }),
    };
    let refusal = if offered != protocol {
        Some(Refusal::Protocol { offered, served: protocol })
    } else if offering > *build {
        Some(Refusal::Superseded)
    } else {
        None
    };
    match refusal {
        Some(refusal) => {
            send(&mut writer, &Reply::<()>::Refused(refusal.clone())).await?;
            Err(refusal.into())
        }
        None => Ok(Opened { body, reader, writer }),
    }
}

pub async fn reply<W, M>(writer: &mut W, answer: Result<M, Refusal>) -> Result<(), IpcError>
where
    W: AsyncWrite + Unpin,
    M: Serialize, {
    send(writer, &answer.map_or_else(Reply::Refused, Reply::Welcome)).await
}
