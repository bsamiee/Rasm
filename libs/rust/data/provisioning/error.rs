use jiff::Timestamp;
use tokio::task::JoinError;

use crate::{Digest, Kind, StoreId};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Operation {
    #[display("read the live limit")]
    Limit,
    #[display("list stores")]
    List,
    #[display("create {_0}")]
    Create(StoreId),
    #[display("label {_0}")]
    Label(StoreId),
    #[display("copy a template into {_0}")]
    Copy(StoreId),
    #[display("grant a login on {_0}")]
    Grant(StoreId),
    #[display("issue credentials for {_0}")]
    Issue(StoreId),
    #[display("remove {_0}")]
    Remove(StoreId),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Refusal {
    #[display("no store holds")]
    Missing,
    #[display("a client holds a connection to")]
    InUse,
    #[cfg(feature = "bucket")]
    #[display("security token service returned no credentials for")]
    Unissued,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Claim {
    #[display("a sealed template or an open build holds")]
    Claimed,
    #[display("no sealed template holds")]
    Unsealed,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum ProvisioningError {
    #[error("{kind} holds its limit of live stores")]
    Exhausted { kind: Kind },
    #[error("{refusal} {id}")]
    Store { id: StoreId, refusal: Refusal },
    #[error("{claim} {digest}")]
    Template { digest: Digest, claim: Claim },
    #[error("clock reads {at}, before the Unix epoch")]
    Clock { at: Timestamp },
    #[error("registry is stopping")]
    Stopping,
    #[error("runtime ended the operation")]
    Interrupted { source: JoinError },
    #[error("cannot encode a record of {id}")]
    Encode { id: StoreId, source: serde_json::Error },
    #[cfg(feature = "postgres")]
    #[error("postgres refused to {operation}")]
    Postgres { operation: Operation, source: sqlx::Error },
    #[cfg(feature = "bucket")]
    #[error("object storage refused to {operation}")]
    Bucket { operation: Operation, source: Box<aws_sdk_s3::Error> },
    #[cfg(feature = "bucket")]
    #[error("security token service refused to {operation}")]
    Credentials { operation: Operation, source: Box<aws_sdk_sts::Error> },
    #[cfg(feature = "clickhouse")]
    #[error("clickhouse refused to {operation}")]
    Clickhouse { operation: Operation, source: ::clickhouse::error::Error },
    #[cfg(feature = "cache")]
    #[error("valkey refused to {operation}")]
    Cache { operation: Operation, source: redis::RedisError },
}
