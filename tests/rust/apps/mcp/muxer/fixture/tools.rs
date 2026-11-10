use core::future::ready;
use core::time::Duration;
use std::borrow::Cow;
use std::collections::BTreeMap;

use clap::ValueEnum;
use mcp_muxer_tests::{Answer, Ask, Echo, Sized, Slow, Variable, Whoami};
use rmcp::handler::server::tool::{InputResponses, ToolCallContext, ToolRouter};
use rmcp::handler::server::wrapper::Parameters;
use rmcp::model::{
    CallToolRequestParams, CallToolResponse, CallToolResult, ContentBlock, ElicitRequest, ElicitRequestParams, ElicitResult, ElicitationAction, ElicitationSchema, InputRequest, InputRequiredResult,
    ListToolsResult, PaginatedRequestParams, ProgressNotificationParam, ProtocolVersion, ServerCapabilities, ServerConfig,
};
use rmcp::service::RequestContext;
use rmcp::{ErrorData, RoleServer, ServerHandler, tool, tool_router};
use tokio::time;
use tokio_util::sync::CancellationToken;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, Default, ValueEnum)]
pub(crate) enum Speaks {
    #[default]
    Both,
    Legacy,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub(crate) struct Fixture {
    tools: ToolRouter<Self>,
    speaks: Speaks,
    pub(crate) crashed: CancellationToken,
}

#[tool_router]
impl Fixture {
    pub(crate) fn new(speaks: Speaks) -> Self { Self { tools: Self::tool_router(), speaks, crashed: CancellationToken::new() } }

    #[tool(description = "Return the text")]
    fn echo(Parameters(Echo { text }): Parameters<Echo>) -> String { text }

    #[tool(description = "Return a string of the given length")]
    fn sized(Parameters(Sized { bytes }): Parameters<Sized>) -> String { "x".repeat(bytes) }

    #[tool(description = "Sleep in steps, reporting progress after each")]
    async fn slow(Parameters(Slow { millis, steps }): Parameters<Slow>, context: RequestContext<RoleServer>) -> Result<String, ErrorData> {
        let failed = |error: &dyn core::error::Error| ErrorData::internal_error(error.to_string(), None);
        let token = context.meta.get_progress_token();
        for step in 1..=steps {
            context.ct.run_until_cancelled(time::sleep(Duration::from_millis(millis).checked_div(steps).unwrap_or_default())).await.ok_or_else(|| ErrorData::internal_error("cancelled", None))?;
            if let Some(token) = token.clone() {
                context.peer.notify_progress(ProgressNotificationParam::new(token, f64::from(step)).with_total(f64::from(steps))).await.map_err(|error| failed(&error))?;
            }
        }
        Ok("done".to_owned())
    }

    #[tool(description = "Ask the client a question and return its answer: an input-required result on 2026-07-28, an elicitation request before it")]
    async fn ask(Parameters(Ask { question }): Parameters<Ask>, InputResponses(responses): InputResponses, context: RequestContext<RoleServer>) -> Result<CallToolResponse, ErrorData> {
        const KEY: &str = "answer";
        let failed = |error: &dyn core::error::Error| ErrorData::internal_error(error.to_string(), None);
        let requested_schema = ElicitationSchema::builder().required_string(KEY).build().map_err(|error| ErrorData::internal_error(error, None))?;
        let asked = ElicitRequestParams::FormElicitationParams { meta: None, message: question, requested_schema };
        let modern = context.protocol_version().is_some_and(|version| !version.has_initialize());
        let answered = match (responses.and_then(|mut responses| responses.remove(KEY)), modern) {
            (Some(response), _) => serde_path_to_error::deserialize::<_, ElicitResult>(response).map_err(|error| failed(&error))?,
            (None, true) => return Ok(InputRequiredResult::from_input_requests(BTreeMap::from([(KEY.to_owned(), InputRequest::Elicitation(ElicitRequest::new(asked)))])).into()),
            (None, false) => context.peer.create_elicitation(asked).await.map_err(|error| failed(&error))?,
        };
        let text = match (answered.action, answered.content) {
            (ElicitationAction::Accept, Some(content)) => serde_path_to_error::deserialize::<_, Answer>(content).map(|Answer { answer }| answer).map_err(|error| failed(&error))?,
            (_, _) => "unanswered".to_owned(),
        };
        Ok(CallToolResult::success(vec![ContentBlock::text(text)]).into())
    }

    #[tool(description = "Return the server process id, working directory, and one environment variable")]
    fn whoami(Parameters(Variable { name }): Parameters<Variable>) -> Result<String, ErrorData> {
        #[expect(clippy::disallowed_methods, reason = "the fixture reports the environment and directory its parent gave it")]
        let (value, directory) = (std::env::var(&name).ok(), std::env::current_dir().ok());
        serde_json::to_string(&Whoami { pid: std::process::id(), directory, value }).map_err(|error| ErrorData::internal_error(error.to_string(), None))
    }

    #[tool(description = "End the server process with status 3")]
    fn crash(&self) -> String {
        self.crashed.cancel();
        "crashing".to_owned()
    }
}

impl ServerHandler for Fixture {
    fn get_info(&self) -> ServerConfig { ServerConfig::new(ServerCapabilities::builder().enable_tools().enable_tool_list_changed().build()) }

    fn supported_protocol_versions(&self) -> Cow<'static, [ProtocolVersion]> {
        Cow::Borrowed(match self.speaks {
            Speaks::Both => ProtocolVersion::KNOWN_VERSIONS,
            Speaks::Legacy => ProtocolVersion::known_up_to(&ProtocolVersion::LATEST_WITH_INITIALIZE),
        })
    }

    async fn call_tool(&self, request: CallToolRequestParams, context: RequestContext<RoleServer>) -> Result<CallToolResponse, ErrorData> {
        self.tools.call(ToolCallContext::new(self, request, context)).await
    }

    fn list_tools(&self, _: Option<PaginatedRequestParams>, _: RequestContext<RoleServer>) -> impl Future<Output = Result<ListToolsResult, ErrorData>> + Send {
        ready(Ok(ListToolsResult::with_all_items(self.tools.list_all())))
    }
}
