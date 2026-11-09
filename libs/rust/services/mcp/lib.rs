#![feature(default_field_values, error_reporter, lock_value_accessors, nonpoison_mutex, sync_nonpoison)]
#![cfg_attr(feature = "app", feature(associated_type_defaults, integer_casts))]

use core::{
    error::Error,
    future::{Ready, ready},
    ops::{Deref, DerefMut},
    pin::pin,
};
#[cfg(feature = "app")]
use core::{fmt, str::Utf8Error};
#[cfg(feature = "app")]
use std::borrow::Cow;
use std::error::Report;

use futures::{StreamExt as _, TryFutureExt as _, TryStreamExt as _, future, stream};
#[cfg(feature = "app")]
use jsonschema::{ValidationError, paths::Location};
#[cfg(feature = "app")]
use nonempty_collections::NEVec;
use opentelemetry::propagation::{Extractor, Injector};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::{
    ErrorData, RoleServer, Service,
    model::{
        ErrorCode, MetaObject, ProgressNotificationParam, ProgressToken, PromptListChangedNotification, ResourceListChangedNotification, ResourceUpdatedNotification, ResourceUpdatedNotificationParam,
        ServerCapabilities, ServerNotification, SubscriptionFilter, ToolListChangedNotification,
    },
    service::RequestContext,
};
use serde::Serialize;
#[cfg(feature = "app")]
use serde::de::value;
use serde_json::Value;
use tokio::sync::watch;
use tokio_stream::wrappers::WatchStream;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Endpoint: Service<RoleServer> {
    fn watch(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, McpError> { Err::<Ready<()>, _>(McpError::ResourceNotFound { uri: uri.to_owned() }) }
}

// --- [MODELS] --------------------------------------------------------------------------

struct Meta<M>(M);

impl<M> Extractor for Meta<M>
where M: Deref<Target = MetaObject>
{
    fn get(&self, key: &str) -> Option<&str> { self.0.get(key).and_then(Value::as_str) }

    fn keys(&self) -> Vec<&str> { self.0.keys().map(String::as_str).collect() }
}

impl<M> Injector for Meta<M>
where M: DerefMut<Target = MetaObject>
{
    fn set(&mut self, key: &str, value: String) { self.0.insert(key.to_owned(), value.into()); }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Update {
    pub progress: f64,
    pub total: Option<f64> = None,
    pub message: Option<String> = None,
}

#[derive(Debug, derive_more::From)]
pub struct ToolError<E>(pub E);

impl<E> From<ToolError<E>> for ErrorData
where E: Serialize + Error
{
    fn from(value: ToolError<E>) -> Self { serde_json::to_value(&value.0).map_or_else(internal, |data| Self::internal_error(Report::new(&value.0).to_string(), Some(data))) }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[cfg(feature = "app")]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, derive_more::Display)]
#[serde(rename_all = "lowercase")]
pub enum Unanswered {
    #[display("user declined to answer, continue without answer")]
    Declined,
    #[display("user cancelled question, call again to ask again")]
    Cancelled,
    #[display("user answer is empty, call again to ask again")]
    Empty,
    #[display("client cannot ask user, ask in conversation and pass answer as arguments")]
    Unsupported,
    #[display("call ended before user answered, call again")]
    Abandoned,
}

#[derive(Debug, Serialize, thiserror::Error)]
#[serde(tag = "_tag")]
pub enum McpError {
    #[cfg(feature = "app")]
    #[error("cannot route resource {pattern}")]
    Route {
        pattern: String,
        #[serde(skip)]
        source: matchit::InsertError,
    },
    #[cfg(feature = "app")]
    #[error("resource template {template} expression {{{expression}}} cannot route, use {{x}}, {{/x}}, {{.x}}, a final {{+x}}, {{?x}} or {{&x}}")]
    Template { template: String, expression: String },
    #[cfg(feature = "app")]
    #[error("cannot compile tool {tool} input schema")]
    Schema {
        tool: Cow<'static, str>,
        #[serde(skip)]
        source: ValidationError<'static>,
    },
    #[cfg(feature = "app")]
    #[error("tool name {tool} breaks MCP tool name format")]
    ToolName { tool: Cow<'static, str> },
    #[cfg(feature = "app")]
    #[error("completion names undeclared prompt {prompt}, register prompt before its completion")]
    Completion { prompt: String },
    #[cfg(feature = "app")]
    #[error("cannot register {} declarations{}", .errors.len(), fmt::from_fn(|formatter| .errors.iter().try_for_each(|error| write!(formatter, "\n{}", Report::new(error)))))]
    Declarations {
        #[serde(skip)]
        errors: NEVec<Self>,
    },
    #[cfg(feature = "app")]
    #[error("arguments break tool {tool} input schema, correct each violation at its path and call again")]
    Arguments { tool: Cow<'static, str>, violations: NEVec<(Location, String)> },
    #[error("{uri} names no resource, list resources and resource templates")]
    ResourceNotFound { uri: String },
    #[cfg(feature = "app")]
    #[error("{uri} variable percent-decodes to invalid UTF-8")]
    Encoding {
        uri: String,
        #[serde(skip)]
        source: Utf8Error,
    },
    #[cfg(feature = "app")]
    #[error("{uri} variables break template, read resources/templates/list")]
    Variables {
        uri: String,
        #[serde(skip)]
        source: serde_path_to_error::Error<value::Error>,
    },
    #[cfg(feature = "app")]
    #[error("requestState matches no paused call, call again without requestState")]
    RequestState,
    #[error("server is shutting down, call again after restart")]
    Stopping,
    #[error("call asks for input, which needs negotiated protocol version 2026-07-28 or newer")]
    InputRequired,
    #[cfg(feature = "proxy")]
    #[error("upstream server exited before answering, call again")]
    Ended,
    #[cfg(feature = "proxy")]
    #[error("cannot start upstream server")]
    Connect {
        #[serde(skip)]
        source: std::io::Error,
    },
    #[cfg(feature = "proxy")]
    #[error("upstream server refused the handshake")]
    Handshake {
        #[serde(skip)]
        source: Box<rmcp::service::ClientInitializeError>,
    },
    #[error("task {task} belongs to another session")]
    Task { task: String },
    #[cfg(feature = "app")]
    #[error("{reason}")]
    Elicitation { reason: Unanswered },
    #[cfg(feature = "app")]
    #[error("requested form has no object schema, tool cannot ask question")]
    Form {
        #[serde(skip)]
        source: serde_json::Error,
    },
    #[cfg(feature = "app")]
    #[error("answer breaks requested form, call again to ask again")]
    Answer {
        data: Value,
        #[serde(skip)]
        source: serde_path_to_error::Error<serde_json::Error>,
    },
    #[cfg(feature = "app")]
    #[error("question did not reach user, call again")]
    Service {
        #[serde(skip)]
        source: rmcp::ServiceError,
    },
}

impl From<McpError> for ErrorData {
    fn from(value: McpError) -> Self {
        let code = match &value {
            McpError::ResourceNotFound { uri: _ } => ErrorCode::RESOURCE_NOT_FOUND,
            McpError::Task { task: _ } => ErrorCode::INVALID_PARAMS,
            #[cfg(feature = "app")]
            McpError::Arguments { tool: _, violations: _ } | McpError::Encoding { uri: _, source: _ } | McpError::Variables { uri: _, source: _ } | McpError::RequestState => ErrorCode::INVALID_PARAMS,
            McpError::Stopping => ErrorCode::INTERNAL_ERROR,
            McpError::InputRequired => ErrorCode::INVALID_REQUEST,
            #[cfg(feature = "proxy")]
            McpError::Ended => ErrorCode::INTERNAL_ERROR,
            #[cfg(feature = "proxy")]
            McpError::Connect { source: _ } | McpError::Handshake { source: _ } => ErrorCode::INTERNAL_ERROR,
            #[cfg(feature = "app")]
            McpError::Route { pattern: _, source: _ }
            | McpError::Template { template: _, expression: _ }
            | McpError::Schema { tool: _, source: _ }
            | McpError::ToolName { tool: _ }
            | McpError::Completion { prompt: _ }
            | McpError::Declarations { errors: _ }
            | McpError::Elicitation { reason: _ }
            | McpError::Form { source: _ }
            | McpError::Answer { data: _, source: _ }
            | McpError::Service { source: _ } => ErrorCode::INTERNAL_ERROR,
        };
        Self { code, ..ToolError(value).into() }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug, Default)]
pub struct Progress(watch::Sender<Option<ProgressNotificationParam>>);

impl Progress {
    pub fn report(&self, update: Update) {
        let Update { progress, total, message } = update;
        self.0.send_if_modified(|latest| {
            latest
                .as_mut()
                .filter(|held| progress.is_finite() && progress >= held.progress && (progress, total, &message) != (held.progress, held.total, &held.message))
                .map(|held| (held.progress, held.total, held.message) = (progress, total, message))
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
            let relayed = updates.map(Ok).try_for_each(|held| peer.notify_progress(held)).unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "progress relay failed"));
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

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(any(feature = "app", feature = "proxy"))]
fn versions() -> &'static [rmcp::model::ProtocolVersion] {
    use rmcp::model::ProtocolVersion;
    const FLOOR: ProtocolVersion = ProtocolVersion::V_2025_06_18;
    ProtocolVersion::KNOWN_VERSIONS.rsplit_once(|version| *version < FLOOR).map_or(ProtocolVersion::KNOWN_VERSIONS, |(_, served)| served)
}

fn list_changes(capabilities: &ServerCapabilities) -> SubscriptionFilter {
    SubscriptionFilter::builder().tools_list_changed().prompts_list_changed().resources_list_changed().build().supported_by(capabilities)
}

fn resynced<U>(filter: &SubscriptionFilter, uris: U) -> impl Iterator<Item = ServerNotification> + use<U>
where U: IntoIterator<Item = String> {
    [
        (filter.tools_list_changed, ToolListChangedNotification::default().into()),
        (filter.prompts_list_changed, PromptListChangedNotification::default().into()),
        (filter.resources_list_changed, ResourceListChangedNotification::default().into()),
    ]
    .into_iter()
    .filter_map(|(listed, notification)| (listed == Some(true)).then_some(notification))
    .chain(uris.into_iter().map(|uri| ResourceUpdatedNotification::new(ResourceUpdatedNotificationParam::new(uri)).into()))
}

fn internal(error: impl Error) -> ErrorData { ErrorData::internal_error(Report::new(error).to_string(), None) }

// --- [COMPOSITION] ---------------------------------------------------------------------

mod host;
#[cfg(feature = "proxy")]
mod proxy;
#[cfg(feature = "app")]
mod server;
mod telemetry;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use host::{Host, Session};
#[cfg(feature = "proxy")]
pub use proxy::Proxy;
#[cfg(feature = "app")]
pub use server::{App, CompletionRouter, Elicitation, IntoResourceContents, Registry, ResourceRouter, Server, Settings};
pub use telemetry::Traced;
