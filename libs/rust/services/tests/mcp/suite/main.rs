#![expect(deprecated, reason = "the legacy revisions keep resources/subscribe, which SEP-2577 deprecates")]

use core::time::Duration;

use claims::{assert_ok, assert_some};
use rasm_mcp_tests::Seen;
use tokio::sync::mpsc;
use tokio::time;

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn seen(events: &mut mpsc::UnboundedReceiver<Seen>) -> Seen { assert_some!(assert_ok!(time::timeout(Duration::from_secs(5), events.recv()).await, "the client is told within five seconds")) }

// --- [COMPOSITION] ---------------------------------------------------------------------

mod cancellation;
mod conformance;
mod logging;
mod progress;
mod proxy;
mod resources;
mod subscriptions;
mod tools;
mod wire;
