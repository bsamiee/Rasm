#![feature(associated_type_defaults, default_field_values, error_reporter, iterator_try_collect, lock_value_accessors, nonpoison_mutex, sync_nonpoison, trait_alias)]

use core::{
    error::Error,
    fmt,
    future::{Ready, ready},
    pin::pin,
    str::Utf8Error,
};
use std::{borrow::Cow, error::Report, ffi::OsString, io};

use futures::{FutureExt as _, StreamExt as _, TryFutureExt as _, TryStreamExt as _, future, stream};
use jsonschema::{ValidationError, paths::Location};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::{
    ErrorData, RoleServer, ServerHandler, ServiceError,
    model::{
        ErrorCode, ProgressNotificationParam, ProgressToken, PromptListChangedNotification, ProtocolVersion, ResourceListChangedNotification, ResourceUpdatedNotification,
        ResourceUpdatedNotificationParam, ServerCapabilities, ServerNotification, SubscriptionFilter, ToolListChangedNotification,
    },
    service::{ClientInitializeError, RequestContext},
};
use serde::{Serialize, de::value};
use serde_json::Value;
use tokio::sync::{broadcast, watch};
use tokio_stream::wrappers::WatchStream;
use tokio_util::sync::CancellationToken;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Endpoint: ServerHandler {
    type Error: Error + Send + Sync + Into<sysexits::ExitCode> + 'static;

    fn watch(&self, uri: &str) -> Result<impl Future<Output = ()> + Send + 'static, McpError> { Err::<Ready<()>, _>(McpError::Unrouted { uri: uri.to_owned() }) }

    fn serve(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), Self::Error>> + Send { stopping.cancelled_owned().map(Ok) }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, derive_more::Display)]
#[serde(rename_all = "lowercase")]
pub enum Unanswered {
    #[display("user declined to answer, continue without answer")]
    Declined,
    #[display("user dismissed question, call again to ask again")]
    Dismissed,
    #[display("user answer is empty, call again to ask again")]
    Empty,
    #[display("client cannot ask user, ask in conversation and pass answer as arguments")]
    Unsupported,
    #[display("call ended before user answered, call again")]
    Abandoned,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Serialize, thiserror::Error)]
#[serde(tag = "_tag")]
pub enum McpError {
    #[error("cannot route resource {pattern}")]
    Route {
        pattern: String,
        #[serde(skip)]
        source: matchit::InsertError,
    },
    #[error("cannot compile tool {tool} input schema")]
    Schema {
        tool: Cow<'static, str>,
        #[serde(skip)]
        source: ValidationError<'static>,
    },
    #[error("{count} declarations failed{listed}", count = .errors.len(), listed = fmt::from_fn(|formatter| .errors.iter().try_for_each(|error| write!(formatter, "\n{}", Report::new(error)))))]
    Declarations {
        #[serde(skip)]
        errors: NEVec<Self>,
    },
    #[error("arguments break tool {tool} input schema, correct each violation at its path and call again")]
    Arguments { tool: Cow<'static, str>, violations: NEVec<(Location, String)> },
    #[error("{uri} names no resource, list resources and resource templates")]
    Unrouted { uri: String },
    #[error("{uri} variable percent-decodes to invalid UTF-8")]
    Unencoded {
        uri: String,
        #[serde(skip)]
        source: Utf8Error,
    },
    #[error("{uri} variables break template, read resources/templates/list")]
    Variables {
        uri: String,
        #[serde(skip)]
        source: serde_path_to_error::Error<value::Error>,
    },
    #[error("requestState matches no paused call, call again without requestState")]
    Unparked,
    #[error("server is shutting down, call again after restart")]
    Stopping,
    #[error("{reason}")]
    Elicitation { reason: Unanswered },
    #[error("requested form has no object schema, tool cannot ask question")]
    Unaskable {
        #[serde(skip)]
        source: serde_json::Error,
    },
    #[error("answer breaks requested form, call again to ask again")]
    Answer {
        data: Value,
        #[serde(skip)]
        source: serde_path_to_error::Error<serde_json::Error>,
    },
    #[error("question did not reach user, call again")]
    Unreached {
        #[serde(skip)]
        source: ServiceError,
    },
    #[error("cannot spawn upstream server {}", .command.display())]
    Spawn {
        #[serde(skip)]
        command: OsString,
        #[serde(skip)]
        source: io::Error,
    },
    #[error("upstream handshake failed")]
    Handshake {
        #[serde(skip)]
        source: Box<ClientInitializeError>,
    },
    #[error("upstream handshake sent no server info")]
    Unidentified,
    #[error("upstream server ended before shutdown")]
    Ended,
}

impl From<McpError> for ErrorData {
    fn from(value: McpError) -> Self {
        let code = match &value {
            McpError::Unrouted { uri: _ } => ErrorCode::RESOURCE_NOT_FOUND,
            McpError::Arguments { tool: _, violations: _ } | McpError::Unencoded { uri: _, source: _ } | McpError::Variables { uri: _, source: _ } | McpError::Unparked => ErrorCode::INVALID_PARAMS,
            McpError::Route { pattern: _, source: _ }
            | McpError::Schema { tool: _, source: _ }
            | McpError::Declarations { errors: _ }
            | McpError::Stopping
            | McpError::Elicitation { reason: _ }
            | McpError::Unaskable { source: _ }
            | McpError::Answer { data: _, source: _ }
            | McpError::Unreached { source: _ }
            | McpError::Spawn { command: _, source: _ }
            | McpError::Handshake { source: _ }
            | McpError::Unidentified
            | McpError::Ended => ErrorCode::INTERNAL_ERROR,
        };
        Self::new(code, Report::new(&value).to_string(), serde_json::to_value(&value).ok())
    }
}

impl From<McpError> for sysexits::ExitCode {
    fn from(value: McpError) -> Self {
        match value {
            McpError::Spawn { command: _, source } => source.into(),
            McpError::Handshake { source: _ } | McpError::Unidentified => Self::Protocol,
            McpError::Ended => Self::Unavailable,
            McpError::Stopping => Self::TempFail,
            McpError::Declarations { errors } => Self::from(errors.into_nonempty_iter().next().0),
            McpError::Route { pattern: _, source: _ }
            | McpError::Schema { tool: _, source: _ }
            | McpError::Arguments { tool: _, violations: _ }
            | McpError::Unrouted { uri: _ }
            | McpError::Unencoded { uri: _, source: _ }
            | McpError::Variables { uri: _, source: _ }
            | McpError::Unparked
            | McpError::Elicitation { reason: _ }
            | McpError::Unaskable { source: _ }
            | McpError::Answer { data: _, source: _ }
            | McpError::Unreached { source: _ } => Self::Software,
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug)]
pub struct Notifier(broadcast::Sender<ServerNotification>);

impl Notifier {
    pub fn notify(&self, notification: impl Into<ServerNotification>) { let (Ok(_) | Err(broadcast::error::SendError(_))) = self.0.send(notification.into()); }
}

#[derive(Clone, Debug, Default)]
pub struct Progress(watch::Sender<Option<ProgressNotificationParam>>);

impl Progress {
    pub fn report(&self, progress: f64, total: Option<f64>, message: Option<String>) {
        self.0.send_if_modified(|latest| {
            latest.as_mut().filter(|held| progress.is_finite() && progress > held.progress).map(|held| (held.progress, held.total, held.message) = (progress, total, message)).is_some()
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
        .filter_map(|latest| ready(latest.filter(|held| held.progress > 0.0_f64)));
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

fn versions() -> &'static [ProtocolVersion] {
    const FLOOR: ProtocolVersion = ProtocolVersion::V_2025_06_18;
    ProtocolVersion::KNOWN_VERSIONS.rsplit_once(|version| *version < FLOOR).map_or(ProtocolVersion::KNOWN_VERSIONS, |(_, served)| served)
}

fn listed(capabilities: &ServerCapabilities) -> SubscriptionFilter {
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

pub mod host;
pub mod proxy;
pub mod server;
