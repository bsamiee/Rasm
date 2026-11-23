// --- [EXPORTS] -------------------------------------------------------------------------

pub use futures::future::join; //~ reexported_dependency
pub use itertools; //~ reexported_dependency
pub use nonempty_collections::{NEVec, nev}; //~ reexported_dependency
pub use serde_json::{Map, Number, Value};
pub use tokio::sync::watch::*; //~ reexported_dependency
pub use uuid::Uuid;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Record {
    pub fields: Map<String, Value>,
    count: Number,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn identified(id: Uuid) -> NEVec<Uuid> { nev![id] }

#[must_use]
pub const fn counted(record: &Record) -> &Number { &record.count }
