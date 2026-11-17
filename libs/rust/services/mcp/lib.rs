#![feature(error_reporter, lock_value_accessors, nonpoison_mutex, sync_nonpoison)]
#![cfg_attr(feature = "app", feature(associated_type_defaults, default_field_values, integer_casts))]

use core::future::{Ready, ready};
use core::pin::pin;
use std::error::Report;

use futures::{FutureExt as _, Stream, StreamExt as _, TryFutureExt as _, TryStreamExt as _, future, stream};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::model::ProgressNotificationParam;
use rmcp::service::RequestContext;
use rmcp::{RoleServer, Service, ServiceError};
use tokio::sync::{oneshot, watch};
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
        let (latest, (ended, ending)) = (self.0.subscribe(), oneshot::channel::<()>());
        let updates = WatchStream::new(latest.clone()).take_until(ending).chain(stream::once(async move { latest.borrow().clone() })).filter_map(ready);
        relayed(
            context,
            updates,
            call.map(move |output| {
                drop(ended);
                output
            }),
        )
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn relayed<F>(context: &RequestContext<RoleServer>, updates: impl Stream<Item = ProgressNotificationParam> + Send, call: F) -> impl Future<Output = F::Output> + Send
where F: Future<Output: Send> + Send {
    let (peer, cancelled, token) = (context.peer.clone(), context.ct.clone(), context.meta.get_progress_token());
    let stamped = updates.filter_map(move |mut held| {
        ready(token.clone().map(|token| {
            held.progress_token = token;
            held
        }))
    });
    let increasing = stamped.scan(None, |sent, held| ready(Some(Some(held).filter(|held| sent.is_none_or(|sent| held.progress > sent)).inspect(|held| *sent = Some(held.progress))))).filter_map(ready);
    async move {
        let relayed = increasing
            .map(Ok)
            .try_for_each(|held| cancelled.run_until_cancelled(peer.notify_progress(held)).map(|sent| sent.unwrap_or(Err(ServiceError::Cancelled { reason: None }))))
            .unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "progress relay failed"));
        match future::select(pin!(call), pin!(relayed)).await {
            future::Either::Left((output, relayed)) => {
                relayed.await;
                output
            }
            future::Either::Right(((), call)) => call.await,
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
    History, IdleTimeout, Key, Lifecycle, Link, Liveness, Phase, Policy, Profile, Proxy, Status,
    jsonrpc::{Envelope, Line, Lines, Message, Meta, Method, Params, decode, encoded, identity, refusal},
    pool::{Pool, Store},
    session::{Ended, Resumable},
};
pub use shared::{Connection, Handle, Logging, Shared};
#[cfg(feature = "telemetry")]
pub use telemetry::Traced;
