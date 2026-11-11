#![feature(error_reporter, lock_value_accessors, nonpoison_mutex, sync_nonpoison)]
#![cfg_attr(feature = "app", feature(associated_type_defaults, default_field_values, integer_casts))]

use core::future::{Ready, ready};
use core::pin::pin;
use std::error::Report;

use futures::{StreamExt as _, TryFutureExt as _, TryStreamExt as _, future, stream};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
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
        self.0.send_if_modified(|latest| match latest {
            Some(held) if update.progress.is_finite() && update.progress >= held.progress && (update.progress, update.total, &update.message) != (held.progress, held.total, &held.message) => {
                (held.progress, held.total, held.message) = (update.progress, update.total, update.message);
                true
            }
            None if update.progress.is_finite() => {
                *latest = Some(update);
                true
            }
            Some(_) | None => false,
        });
    }

    pub fn relay<F>(&self, context: &RequestContext<RoleServer>, call: F) -> impl Future<Output = F::Output> + Send
    where F: Future<Output: Send> + Send {
        let peer = context.peer.clone();
        let stamped = |token: ProgressToken| {
            WatchStream::new(self.0.subscribe()).map(move |latest| {
                latest.map(|mut held| {
                    held.progress_token = token.clone();
                    held
                })
            })
        };
        let updates = stream::iter(context.meta.get_progress_token().map(stamped))
            .flatten()
            .scan(None, |sent, latest| ready(Some(latest.filter(|held| sent.is_none_or(|sent| held.progress > sent)).inspect(|held| *sent = Some(held.progress)))))
            .filter_map(ready);
        async move {
            let relayed = updates.map(Ok).try_for_each(|held| peer.notify_progress(held)).unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "progress relay failed"));
            match future::select(pin!(call), pin!(relayed)).await {
                future::Either::Left((output, _)) => output,
                future::Either::Right(((), call)) => call.await,
            }
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[cfg(feature = "app")]
mod app;
mod error;
#[cfg(feature = "proxy")]
mod proxy;
mod shared;
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
pub use proxy::{
    Catalog, Era, History, Key, Link, Liveness, Phase, Policy, Proxy, Retention, Status,
    jsonrpc::{Envelope, Line, Lines, Message, Meta, Method, Params, Severity, decode, encoded, identity, refusal},
    pool::{Pool, Store},
    session::{Ended, Resumable},
};
pub use shared::{Connection, Log, Shared};
#[cfg(feature = "telemetry")]
pub use telemetry::Traced;
