#![expect(
    deprecated,
    reason = "the MCP and GenAI names moved to the GenAI semantic-conventions repository, which publishes no Rust crate, with values unchanged, and legacy subscription requests name their resource"
)]

use core::ops::Deref;
use std::borrow::Cow;
use std::sync::{LazyLock, OnceLock};

use opentelemetry::KeyValue;
use opentelemetry::metrics::Histogram;
use opentelemetry::propagation::Extractor;
use opentelemetry_semantic_conventions::attribute::{
    ERROR_TYPE, EXCEPTION_MESSAGE, GEN_AI_OPERATION_NAME, GEN_AI_PROMPT_NAME, GEN_AI_TOOL_NAME, JSONRPC_REQUEST_ID, MCP_METHOD_NAME, MCP_PROTOCOL_VERSION, MCP_RESOURCE_URI, RPC_RESPONSE_STATUS_CODE,
};
use opentelemetry_semantic_conventions::metric::{MCP_SERVER_OPERATION_DURATION, MCP_SERVER_SESSION_DURATION};
use rasm_observability::Call;
use rmcp::model::{
    CallToolRequest, CallToolResult, ClientNotification, ClientRequest, GetPromptRequest, InitializeRequest, MetaObject, ProtocolVersion, ReadResourceRequest, ServerConfig, ServerResult,
    SubscribeRequest, UnsubscribeRequest,
};
use rmcp::service::{NotificationContext, RequestContext};
use rmcp::{ErrorData, RoleServer, Service};
use serde_json::Value;
use tokio::time::Instant;
use tracing::Instrument as _;
use tracing::field::Empty;

// --- [CONSTANTS] -----------------------------------------------------------------------

const BOUNDARIES: [f64; 14] = [0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1.0, 2.0, 5.0, 10.0, 30.0, 60.0, 120.0, 300.0];

// --- [MODELS] --------------------------------------------------------------------------

struct Carrier<M>(M);

impl<M> Extractor for Carrier<M>
where M: Deref<Target = MetaObject>
{
    fn get(&self, key: &str) -> Option<&str> { self.0.get(key).and_then(Value::as_str) }

    fn keys(&self) -> Vec<&str> { self.0.keys().map(String::as_str).collect() }
}

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
        static OPERATION: LazyLock<Histogram<f64>> = LazyLock::new(|| rasm_observability::duration(env!("CARGO_PKG_NAME"), MCP_SERVER_OPERATION_DURATION, BOUNDARIES.to_vec()));
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
        let attributes = [
            Some(KeyValue::new(MCP_METHOD_NAME, method)),
            tool.is_some().then(|| KeyValue::new(GEN_AI_OPERATION_NAME, EXECUTE_TOOL)),
            tool.map(|name| KeyValue::new(GEN_AI_TOOL_NAME, name)),
            prompt.map(|name| KeyValue::new(GEN_AI_PROMPT_NAME, name)),
            version.as_ref().map(|version| KeyValue::new(MCP_PROTOCOL_VERSION, version.as_str().to_owned())),
        ];
        let call = Call::start(span, Some(&Carrier(&context.meta.0)), attributes.into_iter().flatten());
        if let (ClientRequest::InitializeRequest(_), Some(version)) = (&request, version) {
            let (Ok(()) | Err((_, _))) = self.opened.set((call.started, version));
        }
        let cancelled = context.ct.clone();
        let result = self.service.handle_request(request, context).instrument(call.span.clone()).await;
        let (error_type, failure) = match (&result, cancelled.is_cancelled()) {
            (Ok(_) | Err(_), true) => (Some(Cow::Borrowed("cancelled")), None),
            (Ok(ServerResult::CallToolResult(CallToolResult { is_error: Some(true), .. })), false) => (Some(Cow::Borrowed("tool_error")), None),
            (Ok(_), false) => (None, None),
            (Err(error), false) => (Some(Cow::Owned(error.code.0.to_string())), Some(error)),
        };
        let (span, elapsed) = (call.span.clone(), call.started.elapsed().as_secs_f64());
        call.finish(&OPERATION, failure.map(|error| error.code.0.to_string().into()), error_type.map(|kind| (kind.clone().into(), failure.map_or(kind, |error| error.message.clone()))));
        if let Some(error) = failure {
            tracing::debug!(parent: &span, elapsed, { EXCEPTION_MESSAGE } = %error.message, "request failed");
        } else {
            tracing::debug!(parent: &span, elapsed, "request completed");
        }
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
        static SESSION: LazyLock<Histogram<f64>> = LazyLock::new(|| rasm_observability::duration(env!("CARGO_PKG_NAME"), MCP_SERVER_SESSION_DURATION, BOUNDARIES.to_vec()));
        if let Some((opened, version)) = self.opened.take() {
            SESSION.record(opened.elapsed().as_secs_f64(), &[KeyValue::new(MCP_PROTOCOL_VERSION, version.as_str().to_owned())]);
        }
    }
}
