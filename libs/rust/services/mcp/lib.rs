#![cfg_attr(feature = "app", feature(associated_type_defaults, default_field_values, integer_casts, lock_value_accessors, nonpoison_mutex, sync_nonpoison))]

use core::future::{Ready, ready};
use core::pin::pin;

use futures::{StreamExt as _, TryFutureExt as _, TryStreamExt as _, future, stream};
use rmcp::model::{ProgressNotificationParam, ProgressToken};
use rmcp::service::RequestContext;
use rmcp::{RoleServer, Service};
use tokio::sync::watch;
use tokio_stream::wrappers::WatchStream;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Subscribe: Service<RoleServer> {
    fn subscribe(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, McpError> { Err::<Ready<()>, _>(McpError::ResourceNotFound { uri: uri.to_owned() }) }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug, Default)]
pub struct Progress(watch::Sender<Option<ProgressNotificationParam>>);

impl Progress {
    pub fn report(&self, update: ProgressNotificationParam) {
        self.0.send_if_modified(|latest| {
            latest
                .as_mut()
                .filter(|held| update.progress.is_finite() && update.progress >= held.progress && (update.progress, update.total, &update.message) != (held.progress, held.total, &held.message))
                .map(|held| (held.progress, held.total, held.message) = (update.progress, update.total, update.message))
                .is_some()
        });
    }

    pub fn relay<F>(&self, context: &RequestContext<RoleServer>, call: F) -> impl Future<Output = F::Output> + Send
    where F: Future<Output: Send> + Send {
        let peer = context.peer.clone();
        let updates = stream::iter(context.meta.get_progress_token().map(|token| {
            self.seed(token);
            WatchStream::new(self.0.subscribe())
        }))
        .flatten()
        .scan(0.0_f64, |sent, latest| ready(Some(latest.filter(|held| held.progress > *sent).inspect(|held| *sent = held.progress))))
        .filter_map(ready);
        async move {
            let relayed = updates.map(Ok).try_for_each(|held| peer.notify_progress(held)).unwrap_or_else(|error| tracing::debug!(error = %error::report(&error), "progress relay failed"));
            match future::select(pin!(call), pin!(relayed)).await {
                future::Either::Left((output, _)) => output,
                future::Either::Right(((), call)) => call.await,
            }
        }
    }

    fn seed(&self, token: ProgressToken) {
        self.0.send_if_modified(|latest| {
            match latest {
                Some(held) => held.progress_token = token,
                None => *latest = Some(ProgressNotificationParam::new(token, 0.0_f64)),
            }
            false
        });
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[cfg(feature = "app")]
mod app;
mod error;
#[cfg(feature = "proxy")]
mod proxy;
mod shared;
#[cfg(feature = "proxy")]
mod stdio;
#[cfg(feature = "telemetry")]
mod telemetry;

// --- [EXPORTS] -------------------------------------------------------------------------

#[cfg(feature = "app")]
pub use app::{
    CompletionRouter, Router, Routers, Server, Settings,
    resources::{IntoResourceContents, ResourceRouter},
    tools::Elicitation,
};
pub use error::{McpError, ToolError};
#[cfg(feature = "proxy")]
pub use proxy::{Catalog, Link, Liveness, Phase, Policy, Proxy, Retention, Status};
pub use shared::{Connection, Shared};
#[cfg(feature = "proxy")]
pub use stdio::{Envelope, Line, Lines, Message, Meta, Params, identity};
#[cfg(feature = "telemetry")]
pub use telemetry::Traced;
