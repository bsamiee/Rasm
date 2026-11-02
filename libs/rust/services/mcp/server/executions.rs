use core::time::Duration;
use std::{borrow::Cow, collections::BTreeMap};

use futures::future::BoxFuture;
use rmcp::{
    ErrorData,
    model::{CallToolResponse, JsonObject, ServerResult},
};
use serde::Serialize;
use tokio::{sync::watch, time::Instant};
use uuid::Uuid;

// --- [TYPES] ---------------------------------------------------------------------------

pub(super) type Ledger = watch::Sender<BTreeMap<Uuid, Execution>>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Serialize)]
pub(super) struct Execution {
    pub(super) tool: Cow<'static, str>,
    pub(super) arguments: Option<JsonObject>,
    pub(super) outcome: Option<Outcome>,
    #[serde(skip)]
    pub(super) since: Instant,
}

#[derive(Debug, Clone, Serialize)]
#[serde(tag = "_tag")]
pub(super) enum Outcome {
    Returned { result: Box<ServerResult> },
    Failed { error: ErrorData },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(super) async fn record(ledger: &Ledger, retention: Duration, execution: Execution, call: BoxFuture<'static, Result<CallToolResponse, ErrorData>>) {
    let key = Uuid::new_v4();
    ledger.send_modify(|rows| {
        rows.retain(|_, held| held.outcome.is_none() || held.since.elapsed() < retention);
        rows.insert(key, execution);
    });
    let outcome = match call.await {
        Ok(response) => Outcome::Returned { result: Box::new(response.into()) },
        Err(error) => Outcome::Failed { error },
    };
    ledger.send_modify(|rows| {
        rows.entry(key).and_modify(|held| (held.outcome, held.since) = (Some(outcome), Instant::now()));
    });
}
