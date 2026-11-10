use core::error::Error;
#[cfg(feature = "app")]
use core::{fmt, str::Utf8Error};
#[cfg(feature = "app")]
use std::borrow::Cow;
use std::error::Report;

#[cfg(feature = "app")]
use jsonschema::{ValidationError, paths::Location};
#[cfg(feature = "app")]
use nonempty_collections::NEVec;
use rmcp::ErrorData;
#[cfg(feature = "app")]
use rmcp::model::ElicitationAction;
use rmcp::model::ErrorCode;
use serde::Serialize;
#[cfg(feature = "app")]
use serde::de::value;
#[cfg(feature = "app")]
use serde_json::Value;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, derive_more::From)]
pub struct ToolError<E>(pub E);

impl<E> From<ToolError<E>> for ErrorData
where E: Serialize + Error
{
    fn from(value: ToolError<E>) -> Self { serde_json::to_value(&value.0).map_or_else(internal, |data| Self::internal_error(Report::new(&value.0).to_string(), Some(data))) }
}

// --- [ERRORS] --------------------------------------------------------------------------

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
    #[error("task {task} belongs to another connection")]
    Task { task: String },
    #[cfg(feature = "app")]
    #[error("{}", match action {
        ElicitationAction::Accept => "user answer is empty, call again to ask again",
        ElicitationAction::Decline => "user declined to answer, continue without answer",
        _ => "user cancelled question, call again to ask again",
    })]
    Elicitation { action: ElicitationAction },
    #[cfg(feature = "app")]
    #[error("client cannot ask user, ask in conversation and pass answer as arguments")]
    Unsupported,
    #[cfg(feature = "app")]
    #[error("call ended before user answered, call again")]
    Abandoned,
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
            #[cfg(feature = "app")]
            McpError::Route { pattern: _, source: _ }
            | McpError::Template { template: _, expression: _ }
            | McpError::Schema { tool: _, source: _ }
            | McpError::ToolName { tool: _ }
            | McpError::Completion { prompt: _ }
            | McpError::Declarations { errors: _ }
            | McpError::Elicitation { action: _ }
            | McpError::Unsupported
            | McpError::Abandoned
            | McpError::Form { source: _ }
            | McpError::Answer { data: _, source: _ }
            | McpError::Service { source: _ } => ErrorCode::INTERNAL_ERROR,
        };
        Self { code, ..ToolError(value).into() }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn internal(error: impl Error) -> ErrorData { ErrorData::internal_error(Report::new(error).to_string(), None) }
