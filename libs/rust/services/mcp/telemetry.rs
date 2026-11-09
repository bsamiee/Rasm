#![expect(
    deprecated,
    reason = "the MCP and GenAI names moved to the GenAI semantic-conventions repository, which publishes no Rust crate, with values unchanged, and legacy subscription requests name their resource"
)]

use std::{
    borrow::Cow,
    sync::{LazyLock, OnceLock},
};

use opentelemetry::{InstrumentationScope, KeyValue, global, metrics::Histogram, trace::Status};
use opentelemetry_semantic_conventions::{
    SCHEMA_URL,
    attribute::{
        ERROR_TYPE, EXCEPTION_MESSAGE, GEN_AI_OPERATION_NAME, GEN_AI_PROMPT_NAME, GEN_AI_TOOL_NAME, JSONRPC_REQUEST_ID, MCP_METHOD_NAME, MCP_PROTOCOL_VERSION, MCP_RESOURCE_URI,
        RPC_RESPONSE_STATUS_CODE,
    },
    metric::{MCP_SERVER_OPERATION_DURATION, MCP_SERVER_SESSION_DURATION},
};
use rmcp::{
    ErrorData, RoleServer, Service,
    model::{
        CallToolRequest, CallToolResult, ClientNotification, ClientRequest, GetPromptRequest, InitializeRequest, ProtocolVersion, ReadResourceRequest, ServerConfig, ServerResult, SubscribeRequest,
        UnsubscribeRequest,
    },
    service::{NotificationContext, RequestContext},
};
use tokio::time::Instant;
use tracing::{
    Instrument as _,
    field::{self, Empty},
};
use tracing_opentelemetry::{OpenTelemetrySpanExt as _, SetParentError};

use crate::Meta;

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Traced<S> {
    service: S,
    opened: OnceLock<(Instant, ProtocolVersion)>,
}

impl<S> Traced<S> {
    pub const fn new(service: S) -> Self { Self { service, opened: OnceLock::new() } }
}

impl<S> Service<RoleServer> for Traced<S>
where S: Service<RoleServer>
{
    async fn handle_request(&self, request: ClientRequest, context: RequestContext<RoleServer>) -> Result<ServerResult, ErrorData> {
        const EXECUTE_TOOL: &str = "execute_tool";
        static OPERATION: LazyLock<Histogram<f64>> = LazyLock::new(|| duration(MCP_SERVER_OPERATION_DURATION));
        let method = request.method().to_owned();
        let (tool, prompt, uri) = match &request {
            ClientRequest::CallToolRequest(CallToolRequest { params, .. }) => (Some(params.name.to_string()), None, None),
            ClientRequest::GetPromptRequest(GetPromptRequest { params, .. }) => (None, Some(params.name.clone()), None),
            ClientRequest::ReadResourceRequest(ReadResourceRequest { params, .. }) => (None, None, Some(params.uri.clone())),
            ClientRequest::SubscribeRequest(SubscribeRequest { params, .. }) => (None, None, Some(params.uri.clone())),
            ClientRequest::UnsubscribeRequest(UnsubscribeRequest { params, .. }) => (None, None, Some(params.uri.clone())),
            _ => (None, None, None),
        };
        let version = match &request {
            ClientRequest::InitializeRequest(InitializeRequest { params, .. }) => Some(params.protocol_version.clone()),
            _ => context.protocol_version(),
        };
        let span = tracing::info_span!(
            "request",
            otel.name = tool.as_ref().or(prompt.as_ref()).map_or_else(|| method.clone(), |name| format!("{method} {name}")),
            otel.kind = "server",
            { MCP_METHOD_NAME } = method,
            { GEN_AI_OPERATION_NAME } = tool.is_some().then_some(EXECUTE_TOOL),
            { GEN_AI_TOOL_NAME } = tool.as_deref(),
            { GEN_AI_PROMPT_NAME } = prompt.as_deref(),
            { MCP_RESOURCE_URI } = uri.as_deref(),
            { MCP_PROTOCOL_VERSION } = version.as_ref().map(ProtocolVersion::as_str),
            { JSONRPC_REQUEST_ID } = %context.id,
            { ERROR_TYPE } = Empty,
            { RPC_RESPONSE_STATUS_CODE } = Empty,
        );
        let parent = global::get_text_map_propagator(|propagator| propagator.extract(&Meta(&context.meta.0)));
        let (Ok(()) | Err(SetParentError::LayerNotFound | SetParentError::SpanDisabled | SetParentError::AlreadyStarted)) = span.set_parent(parent);
        if let (ClientRequest::InitializeRequest(_), Some(version)) = (&request, &version) {
            let (Ok(()) | Err((_, _))) = self.opened.set((Instant::now(), version.clone()));
        }
        let (started, cancelled) = (Instant::now(), context.ct.clone());
        let result = self.service.handle_request(request, context).instrument(span.clone()).await;
        let elapsed = started.elapsed();
        let (error_type, failure) = match (&result, cancelled.is_cancelled()) {
            (Ok(_) | Err(_), true) => (Some(Cow::Borrowed("cancelled")), None),
            (Ok(ServerResult::CallToolResult(CallToolResult { is_error: Some(true), .. })), false) => (Some(Cow::Borrowed("tool_error")), None),
            (Ok(_), false) => (None, None),
            (Err(error), false) => (Some(Cow::Owned(error.code.0.to_string())), Some(error)),
        };
        span.record(ERROR_TYPE, error_type.as_deref()).record(RPC_RESPONSE_STATUS_CODE, failure.map(|error| field::display(error.code.0)));
        if let Some(kind) = &error_type {
            span.set_status(Status::error(failure.map_or_else(|| kind.clone(), |error| error.message.clone())));
        }
        if let Some(error) = failure {
            tracing::warn!(parent: &span, elapsed = elapsed.as_secs_f64(), { EXCEPTION_MESSAGE } = %error.message, "request failed");
        } else {
            tracing::debug!(parent: &span, elapsed = elapsed.as_secs_f64(), "request completed");
        }
        let attributes = [
            Some(KeyValue::new(MCP_METHOD_NAME, method)),
            tool.is_some().then(|| KeyValue::new(GEN_AI_OPERATION_NAME, EXECUTE_TOOL)),
            tool.map(|name| KeyValue::new(GEN_AI_TOOL_NAME, name)),
            prompt.map(|name| KeyValue::new(GEN_AI_PROMPT_NAME, name)),
            version.map(|version| KeyValue::new(MCP_PROTOCOL_VERSION, version.as_str().to_owned())),
            failure.map(|error| KeyValue::new(RPC_RESPONSE_STATUS_CODE, error.code.0.to_string())),
            error_type.map(|kind| KeyValue::new(ERROR_TYPE, kind)),
        ];
        OPERATION.record(elapsed.as_secs_f64(), &attributes.into_iter().flatten().collect::<Vec<_>>());
        result
    }

    async fn handle_notification(&self, notification: ClientNotification, context: NotificationContext<RoleServer>) -> Result<(), ErrorData> {
        self.service.handle_notification(notification, context).await
    }

    fn get_info(&self) -> ServerConfig { self.service.get_info() }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> { self.service.supported_protocol_versions() }
}

impl<S> Drop for Traced<S> {
    fn drop(&mut self) {
        static SESSION: LazyLock<Histogram<f64>> = LazyLock::new(|| duration(MCP_SERVER_SESSION_DURATION));
        if let Some((opened, version)) = self.opened.take() {
            SESSION.record(opened.elapsed().as_secs_f64(), &[KeyValue::new(MCP_PROTOCOL_VERSION, version.as_str().to_owned())]);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn duration(name: &'static str) -> Histogram<f64> {
    const BOUNDARIES: [f64; 14] = [0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1.0, 2.0, 5.0, 10.0, 30.0, 60.0, 120.0, 300.0];
    global::meter_with_scope(InstrumentationScope::builder(env!("CARGO_PKG_NAME")).with_schema_url(SCHEMA_URL).build()).f64_histogram(name).with_unit("s").with_boundaries(BOUNDARIES.to_vec()).build()
}
