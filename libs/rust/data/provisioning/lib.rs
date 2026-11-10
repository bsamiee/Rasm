#![cfg_attr(feature = "bucket", feature(iterator_try_collect))]
#![feature(deref_patterns, error_reporter)]

use core::str::FromStr;

use newtype_uuid::{GenericUuid as _, TypedUuidKind as _};
use newtype_uuid_macros::impl_typed_uuid_kinds;
use nutype::nutype;
use serde::{Deserialize, Serialize};
use uuid::fmt::Simple;

// --- [TYPES] ---------------------------------------------------------------------------

impl_typed_uuid_kinds! {
    kinds = {
        Store = {},
    },
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, enum_map::Enum, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Kind {
    Postgres,
    Bucket,
    Clickhouse,
    Cache,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Removal {
    Idle,
    Forced,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, derive_more::Display)]
#[display("{}{}", StoreKind::tag(), _0.as_untyped_uuid().as_simple())]
pub struct StoreId(StoreUuid);

impl StoreId {
    #[must_use]
    pub fn new(at: jiff::Timestamp) -> Option<Self> {
        let seconds = u64::try_from(at.as_second()).ok()?;
        let nanoseconds = u32::try_from(at.subsec_nanosecond()).ok()?;
        Some(Self(StoreUuid::new_v7(uuid::Timestamp::from_unix(uuid::NoContext, seconds, nanoseconds))))
    }
}

impl FromStr for StoreId {
    type Err = StoreIdError;

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        let digits = s.strip_prefix(StoreKind::tag().as_str()).ok_or(StoreIdError { source: None })?;
        let simple = digits.parse::<Simple>().map_err(|source| StoreIdError { source: Some(source) })?;
        (simple.encode_lower(&mut uuid::Uuid::encode_buffer()) == digits).then(|| Self(StoreUuid::from_untyped_uuid(simple.into_uuid()))).ok_or(StoreIdError { source: None })
    }
}

#[nutype(
    validate(predicate = |name: &str| (1..=64).contains(&name.len()) && !name.chars().any(char::is_control)),
    derive(Debug, Clone, PartialEq, Eq, PartialOrd, Ord, Hash, Display, AsRef, FromStr, Serialize, Deserialize)
)]
pub struct SessionName(String);

#[nutype(
    validate(predicate = |digest: &str| (1..=64).contains(&digest.len()) && !digest.chars().any(char::is_control)),
    derive(Debug, Clone, PartialEq, Eq, PartialOrd, Ord, Hash, Display, AsRef, FromStr, Serialize, Deserialize)
)]
pub struct Digest(String);

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(tag = "state", rename_all = "snake_case")]
pub enum Label {
    Held { session: SessionName },
    Kept { session: SessionName },
    Build { session: SessionName, digest: Digest },
    Template { digest: Digest },
    Ready { digest: Digest },
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, thiserror::Error)]
#[error("store id is `{}` and 32 lowercase hex digits", StoreKind::tag())]
pub struct StoreIdError {
    pub(crate) source: Option<uuid::Error>,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[cfg(feature = "bucket")]
mod bucket;
#[cfg(feature = "cache")]
mod cache;
#[cfg(feature = "clickhouse")]
mod clickhouse;
mod error;
mod ledger;
#[cfg(feature = "postgres")]
mod postgres;
mod registry;
mod stores;

// --- [EXPORTS] -------------------------------------------------------------------------

#[cfg(feature = "bucket")]
pub use bucket::{Bucket, Granted};
#[cfg(feature = "cache")]
pub use cache::{Cache, Numbered};
#[cfg(feature = "clickhouse")]
pub use clickhouse::Clickhouse;
pub use error::{Claim, Operation, ProvisioningError, Refusal};
#[cfg(feature = "postgres")]
pub use postgres::Postgres;
pub use registry::{Policy, Registry};
pub use stores::{Create, Login, Server, Stores, Template};
