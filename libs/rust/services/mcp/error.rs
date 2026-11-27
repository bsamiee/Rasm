use core::error::Error;
use core::fmt;
#[cfg(feature = "app")]
use core::str::Utf8Error;
#[cfg(feature = "app")]
use std::borrow::Cow;
use std::error::Report;

#[cfg(feature = "proxy")]
use bytesize::ByteSize;
#[cfg(feature = "proxy")]
use itertools::Itertools as _;
#[cfg(feature = "app")]
use jsonschema::{ValidationError, paths::Location};
#[cfg(any(feature = "app", feature = "proxy"))]
use nonempty_collections::NEVec;
use rmcp::ErrorData;
#[cfg(feature = "app")]
use rmcp::model::ElicitationAction;
use rmcp::model::{ConstString as _, ErrorCode, ListResourceTemplatesRequestMethod, ListResourcesRequestMethod};
#[cfg(feature = "proxy")]
use rmcp::model::{DiscoverRequestMethod, InitializeResultMethod, ProtocolVersion};
use serde::Serialize;
#[cfg(feature = "app")]
use serde::de::value;
#[cfg(feature = "app")]
use serde_json::Value;

#[cfg(feature = "app")]
use crate::Handle;
#[cfg(feature = "proxy")]
use crate::{Cause, Method};

// --- [TYPES] ---------------------------------------------------------------------------

#[cfg(feature = "proxy")]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, derive_more::Display)]
#[display(rename_all = "lowercase")]
#[serde(rename_all = "lowercase")]
pub enum Part {
    Request,
    Response,
}

#[cfg(feature = "proxy")]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, derive_more::Display)]
#[display(rename_all = "kebab-case")]
#[serde(rename_all = "kebab-case")]
pub enum Unrouted {
    CallerEnded,
    CallerAmbiguous,
    CapabilityUndeclared,
    SessionClosed,
    InputRequired,
    Idle,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, derive_more::From)]
pub struct ToolError<E>(pub E);

impl<E> From<ToolError<E>> for ErrorData
where E: Serialize + Error
{
    fn from(value: ToolError<E>) -> Self { serde_json::to_value(&value.0).map_or_else(internal, |data| Self::internal_error(Report::new(&value.0).to_string(), Some(data))) }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, derive_more::Display)]
#[serde(rename_all = "snake_case")]
pub enum Next {
    #[display("retry")]
    Retry,
    #[display("retry without {_0}")]
    Without(&'static str),
    #[display("{_0}")]
    Call(&'static str),
    #[cfg(feature = "proxy")]
    #[display("retry with smaller {_0}")]
    Smaller(Part),
}

#[derive(Serialize)]
pub(crate) struct Data<'error, E> {
    #[serde(flatten)]
    pub(crate) error: &'error E,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub(crate) next: Option<Next>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Serialize, thiserror::Error)]
#[serde(tag = "_tag")]
pub enum McpError {
    #[cfg(feature = "app")]
    #[error("resource {pattern} unroutable")]
    Route {
        pattern: String,
        #[serde(skip)]
        source: matchit::InsertError,
    },
    #[cfg(feature = "app")]
    #[error("resource template {template} expression {{{expression}}} unroutable, routable: {{x}}, {{/x}}, {{.x}}, {{?x}}, {{&x}}, final {{+x}}")]
    Template { template: String, expression: String },
    #[cfg(feature = "app")]
    #[error("tool {tool} input schema uncompilable")]
    Schema {
        tool: Cow<'static, str>,
        #[serde(skip)]
        source: ValidationError<'static>,
    },
    #[cfg(feature = "app")]
    #[error("tool {tool} name invalid")]
    ToolName { tool: Cow<'static, str> },
    #[cfg(feature = "app")]
    #[error("completion prompt {prompt} undeclared")]
    Completion { prompt: String },
    #[cfg(feature = "app")]
    #[error("{} declarations refused{}", .errors.len(), fmt::from_fn(|formatter| .errors.iter().try_for_each(|error| write!(formatter, "\n  {}", Report::new(error)))))]
    Declarations {
        #[serde(skip)]
        errors: NEVec<Self>,
    },
    #[cfg(feature = "app")]
    #[error("tool {tool} arguments break input schema")]
    Arguments { tool: Cow<'static, str>, violations: NEVec<(Location, String)> },
    #[error("resource {uri} not found")]
    ResourceNotFound { uri: String },
    #[cfg(feature = "app")]
    #[error("resource {uri} variable not UTF-8")]
    Encoding {
        uri: String,
        #[serde(skip)]
        source: Utf8Error,
    },
    #[cfg(feature = "app")]
    #[error("resource {uri} variables break template")]
    Variables {
        uri: String,
        #[serde(skip)]
        source: serde_path_to_error::Error<value::Error>,
    },
    #[cfg(feature = "app")]
    #[error("requestState matches no paused call")]
    RequestState,
    #[cfg(feature = "app")]
    #[error("handle {handle} unknown")]
    Handle { handle: Handle },
    #[cfg(feature = "app")]
    #[error("handle argument malformed")]
    HandleArgument {
        #[serde(skip)]
        source: serde_path_to_error::Error<serde_json::Error>,
    },
    #[cfg(feature = "app")]
    #[error("tool {tool} input schema properties malformed")]
    Properties {
        tool: Cow<'static, str>,
        #[serde(skip)]
        source: serde_path_to_error::Error<serde_json::Error>,
    },
    #[error("server stopping")]
    Stopping,
    #[error("task {task} owned by another connection")]
    Task { task: String },
    #[cfg(feature = "app")]
    #[error("elicitation {}", match action {
        ElicitationAction::Accept => "accepted empty",
        ElicitationAction::Decline => "declined",
        _ => "cancelled",
    })]
    Elicitation { action: ElicitationAction },
    #[cfg(feature = "app")]
    #[error("client elicitation unsupported")]
    Unsupported,
    #[cfg(feature = "app")]
    #[error("elicitation abandoned")]
    Abandoned,
    #[cfg(feature = "app")]
    #[error("elicitation form schema invalid")]
    Form {
        #[serde(skip)]
        source: serde_json::Error,
    },
    #[cfg(feature = "app")]
    #[error("elicitation answer breaks form")]
    Answer {
        data: Value,
        #[serde(skip)]
        source: serde_path_to_error::Error<serde_json::Error>,
    },
    #[cfg(feature = "app")]
    #[error("elicitation undelivered")]
    Service {
        #[serde(skip)]
        source: rmcp::ServiceError,
    },
    #[cfg(feature = "proxy")]
    #[error("{part} exceeds {}", ByteSize::b(limit.saturating_cast::<u64>()))]
    Exceeded { part: Part, limit: usize },
    #[cfg(feature = "proxy")]
    #[error("server starting")]
    Starting,
    #[cfg(feature = "proxy")]
    #[error("server down{}: {}", if *sent { ", call sent" } else { "" }, cause.iter().format(", "))]
    Down { cause: NEVec<Cause>, sent: bool },
    #[cfg(feature = "proxy")]
    #[error("server connection lost")]
    Lost,
    #[cfg(feature = "proxy")]
    #[error("{} unserved, protocol up to {}", DiscoverRequestMethod::VALUE, ProtocolVersion::LATEST_WITH_INITIALIZE)]
    Discover,
    #[cfg(feature = "proxy")]
    #[error("{} unrouted: {reason}", fmt::from_fn(|formatter| match method {
        Some(method) => write!(formatter, "{method}"),
        None => formatter.write_str("request"),
    }))]
    Unrouted { method: Option<Method>, reason: Unrouted },
}

impl From<McpError> for ErrorData {
    fn from(value: McpError) -> Self {
        let (code, next) = coded(&value);
        serde_json::to_value(Data { error: &value, next }).map_or_else(internal, |data| Self::new(code, reported(&value, next), Some(data)))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) const fn coded(error: &McpError) -> (ErrorCode, Option<Next>) {
    match error {
        McpError::ResourceNotFound { uri: _ } => (ErrorCode::RESOURCE_NOT_FOUND, Some(Next::Call(ListResourcesRequestMethod::VALUE))),
        McpError::Task { task: _ } => (ErrorCode::INVALID_PARAMS, None),
        McpError::Stopping => (ErrorCode::INTERNAL_ERROR, Some(Next::Retry)),
        #[cfg(feature = "app")]
        McpError::Arguments { tool: _, violations: _ } => (ErrorCode::INVALID_PARAMS, Some(Next::Retry)),
        #[cfg(feature = "app")]
        McpError::Variables { uri: _, source: _ } => (ErrorCode::INVALID_PARAMS, Some(Next::Call(ListResourceTemplatesRequestMethod::VALUE))),
        #[cfg(feature = "app")]
        McpError::RequestState => (ErrorCode::INVALID_PARAMS, Some(Next::Without("requestState"))),
        #[cfg(feature = "app")]
        McpError::Handle { handle: _ } | McpError::HandleArgument { source: _ } => (ErrorCode::INVALID_PARAMS, Some(Next::Without("handle"))),
        #[cfg(feature = "app")]
        McpError::Encoding { uri: _, source: _ } => (ErrorCode::INVALID_PARAMS, None),
        #[cfg(feature = "app")]
        McpError::Elicitation { action: ElicitationAction::Decline } | McpError::Unsupported | McpError::Form { source: _ } => (ErrorCode::INTERNAL_ERROR, None),
        #[cfg(feature = "app")]
        McpError::Elicitation { action: _ } | McpError::Abandoned | McpError::Answer { data: _, source: _ } | McpError::Service { source: _ } => (ErrorCode::INTERNAL_ERROR, Some(Next::Retry)),
        #[cfg(feature = "app")]
        McpError::Route { pattern: _, source: _ }
        | McpError::Template { template: _, expression: _ }
        | McpError::Schema { tool: _, source: _ }
        | McpError::Properties { tool: _, source: _ }
        | McpError::ToolName { tool: _ }
        | McpError::Completion { prompt: _ }
        | McpError::Declarations { errors: _ } => (ErrorCode::INTERNAL_ERROR, None),
        #[cfg(feature = "proxy")]
        McpError::Exceeded { part: Part::Request, limit: _ } => (ErrorCode::INVALID_REQUEST, Some(Next::Smaller(Part::Request))),
        #[cfg(feature = "proxy")]
        McpError::Exceeded { part: Part::Response, limit: _ } => (ErrorCode::INTERNAL_ERROR, Some(Next::Smaller(Part::Response))),
        #[cfg(feature = "proxy")]
        McpError::Starting | McpError::Down { cause: _, sent: _ } | McpError::Lost => (ErrorCode::INTERNAL_ERROR, Some(Next::Retry)),
        #[cfg(feature = "proxy")]
        McpError::Discover => (ErrorCode::METHOD_NOT_FOUND, Some(Next::Call(InitializeResultMethod::VALUE))),
        #[cfg(feature = "proxy")]
        McpError::Unrouted { method: _, reason: Unrouted::CallerAmbiguous } => (ErrorCode::METHOD_NOT_FOUND, Some(Next::Retry)),
        #[cfg(feature = "proxy")]
        McpError::Unrouted { method: _, reason: Unrouted::CallerEnded | Unrouted::CapabilityUndeclared | Unrouted::SessionClosed | Unrouted::InputRequired | Unrouted::Idle } =>
            (ErrorCode::METHOD_NOT_FOUND, None),
    }
}

pub(crate) fn reported(error: &impl Error, next: Option<Next>) -> String {
    let stated = fmt::from_fn(|formatter| next.map_or(Ok(()), |next| write!(formatter, "\nnext: {next}")));
    format!("{}{stated}", Report::new(error))
}

pub(crate) fn internal(error: impl Error) -> ErrorData { ErrorData::internal_error(Report::new(error).to_string(), None) }
