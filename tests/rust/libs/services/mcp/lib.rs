#![expect(deprecated, reason = "the 2025-11-25 scenarios exercise sampling, roots, and logging, which SEP-2577 deprecates")]

use core::future::{pending, ready};
use core::num::NonZero;
use core::time::Duration;
use std::io;
use std::path::Path;
use std::process::Stdio;
use std::sync::Arc;

use futures::{FutureExt as _, StreamExt as _, stream};
use rasm_mcp::{CompletionRouter, Elicitation, Log, McpError, Progress, ResourceRouter, Router, Routers, Server, Settings, Shared, decode};
use rmcp::handler::server::common::Extension;
use rmcp::handler::server::wrapper::{Json, Parameters};
use rmcp::model::{
    ArgumentInfo, CallToolResult, CancelledNotificationParam, ClientCapabilities, ClientConfig, CompletionContext, ContentBlock, CreateMessageRequestParams, CreateMessageResult, ElicitRequestParams,
    ElicitResult, ElicitationAction, Implementation, LoggingLevel, LoggingMessageNotificationParam, ProgressNotificationParam, PromptMessage, ProtocolVersion, Resource, ResourceContents,
    ResourceTemplate, ResourceUpdatedNotificationParam, Role, SamplingMessage, ServerNotification,
};
use rmcp::service::{ClientLifecycleMode, ClientServiceExt as _, NotificationContext, RequestContext, RunningService, ServiceExt as _};
use rmcp::transport::streamable_http_server::session::local::LocalSessionManager;
use rmcp::transport::{StreamableHttpServerConfig, StreamableHttpService};
use rmcp::{ClientHandler, ErrorData, RoleClient, RoleServer, ServerHandler, Service, prompt, prompt_router, tool, tool_router};
use schemars::JsonSchema;
use serde::{Deserialize, Serialize};
use serde_json::{Map, Value};
use tokio::io::{AsyncBufReadExt as _, AsyncRead, AsyncWrite, AsyncWriteExt as _, BufReader, DuplexStream, Lines, ReadHalf, WriteHalf};
use tokio::net::TcpListener;
use tokio::process::Command;
use tokio::sync::{Semaphore, broadcast, mpsc, watch};
use tokio::time;
use tokio_stream::wrappers::WatchStream;
use tokio_util::task::AbortOnDropHandle;

// --- [CONSTANTS] -----------------------------------------------------------------------

pub const IMAGE: &str = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==";
pub const AUDIO: &str = "UklGRiYAAABXQVZFZm10IBAAAAABAAEAQB8AAAB9AAACABAAZGF0YQIAAAA=";
pub const WATCHED: &str = "test://watched-resource";
const TEXT: &str = "text/plain";
pub const TRANSCRIPTS: [(&str, &str); 8] = [
    ("lifecycle", include_str!("transcripts/lifecycle.jsonl")),
    ("bad_requests", include_str!("transcripts/bad_requests.jsonl")),
    ("tools", include_str!("transcripts/tools.jsonl")),
    ("resources", include_str!("transcripts/resources.jsonl")),
    ("prompts", include_str!("transcripts/prompts.jsonl")),
    ("version-latest", include_str!("transcripts/version-latest.jsonl")),
    ("version-older", include_str!("transcripts/version-older.jsonl")),
    ("discover", include_str!("transcripts/discover.jsonl")),
];
pub const OUTLINES: [(&str, &[(u64, Reply)]); 8] = [
    ("lifecycle", &[(2, Reply::Error), (1, Reply::Result), (1, Reply::Result), (2, Reply::Result), (3, Reply::Result)]),
    ("bad_requests", &[(1, Reply::Error), (2, Reply::Result), (3, Reply::Error), (4, Reply::Error), (5, Reply::Error), (4, Reply::Error)]),
    ("tools", &[(1, Reply::Result), (2, Reply::Result), (3, Reply::Result), (4, Reply::Result), (5, Reply::Error), (6, Reply::Result), (7, Reply::Result), (8, Reply::Result), (10, Reply::Result)]),
    ("resources", &[(1, Reply::Result), (2, Reply::Result), (3, Reply::Result), (4, Reply::Error), (5, Reply::Error)]),
    ("prompts", &[(1, Reply::Result), (2, Reply::Result), (4, Reply::Result), (5, Reply::Error)]),
    ("version-latest", &[(1, Reply::Result)]),
    ("version-older", &[(1, Reply::Result)]),
    ("discover", &[(1, Reply::Result)]),
];
// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Reply {
    Result,
    Error,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Deserialize, JsonSchema)]
pub struct Prompted {
    pub prompt: String,
}

#[derive(Debug, Clone, Deserialize, JsonSchema)]
pub struct Asked {
    pub message: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, JsonSchema)]
pub struct Identity {
    #[schemars(description = "User's response")]
    pub username: String,
    #[schemars(description = "User's email address")]
    pub email: String,
}

rmcp::elicit_safe!(Identity);

#[derive(Debug, Clone, Deserialize, JsonSchema)]
pub struct Arguments {
    pub arg1: String,
    pub arg2: String,
}

#[derive(Debug, Clone, Deserialize, JsonSchema)]
pub struct Embedded {
    #[serde(rename = "resourceUri")]
    pub resource_uri: String,
}

#[derive(Debug, Clone, Deserialize, JsonSchema)]
pub struct Greeted {
    #[serde(rename = "Name")]
    pub name: String,
}

#[derive(Debug, Clone, Deserialize, JsonSchema)]
pub struct Input {
    #[serde(rename = "In")]
    pub input: String,
}

#[derive(Debug, Clone, Serialize, JsonSchema)]
pub struct Output {
    #[serde(rename = "Out")]
    pub output: String,
}

#[derive(Debug, Clone, Deserialize)]
pub struct Row {
    pub id: String,
}

#[derive(Debug, Clone, Serialize)]
pub struct Templated {
    pub id: String,
    #[serde(rename = "templateTest")]
    pub template_test: bool,
    pub data: String,
}

#[derive(Debug, Clone, PartialEq, Eq, Deserialize)]
pub struct Fault {
    pub code: i32,
    pub message: String,
    #[serde(default)]
    pub data: Option<Value>,
}

#[derive(Debug, Clone, PartialEq, Eq, Deserialize)]
#[serde(untagged)]
pub enum Id {
    Number(u64),
    Signed(i64),
    Text(String),
}

#[derive(Debug, Clone, Default, PartialEq, Eq, Deserialize)]
#[serde(default)]
pub struct Block {
    pub text: Option<String>,
}

#[derive(Debug, Clone, Default, PartialEq, Eq, Deserialize)]
#[serde(default)]
pub struct Structured {
    #[serde(rename = "Out")]
    pub output: Option<String>,
}

#[derive(Debug, Clone, Default, PartialEq, Eq, Deserialize)]
#[serde(default, rename_all = "camelCase")]
pub struct Answered {
    pub protocol_version: Option<String>,
    pub supported_versions: Option<Vec<String>>,
    pub capabilities: Option<Map<String, Value>>,
    pub tools: Option<Vec<Map<String, Value>>>,
    pub content: Option<Vec<Block>>,
    pub contents: Option<Vec<Block>>,
    pub structured_content: Option<Structured>,
    pub is_error: Option<bool>,
}

#[derive(Debug, Clone, PartialEq, Eq, Deserialize)]
pub struct Message {
    #[serde(default)]
    pub id: Option<Id>,
    #[serde(default)]
    pub method: Option<String>,
    #[serde(default)]
    pub result: Option<Answered>,
    #[serde(default)]
    pub error: Option<Fault>,
}

#[derive(Debug, Clone, PartialEq, Eq, Deserialize)]
pub struct Check {
    pub id: String,
    pub status: String,
    #[serde(default, rename = "errorMessage")]
    pub error_message: Option<String>,
}

#[derive(Debug, Clone)]
pub struct Conformed {
    pub code: Option<i32>,
    pub checks: Vec<Check>,
    pub output: String,
}

#[derive(Debug, Clone)]
pub enum Seen {
    Progress(ProgressNotificationParam),
    Logged(LoggingMessageNotificationParam),
    Updated(String),
    Cancelled(CancelledNotificationParam),
    Elicited(ElicitRequestParams),
    Sampled(CreateMessageRequestParams),
    ToolsChanged,
    PromptsChanged,
    ResourcesChanged,
}

#[derive(Debug)]
pub struct Wire<R, W> {
    lines: Lines<BufReader<R>>,
    writer: W,
}

impl<R, W> Wire<R, W>
where
    R: AsyncRead + Unpin,
    W: AsyncWrite + Unpin,
{
    pub fn new(reader: R, writer: W) -> Self { Self { lines: BufReader::new(reader).lines(), writer } }

    pub async fn send(&mut self, line: &str) -> io::Result<()> {
        self.writer.write_all(line.as_bytes()).await?;
        self.writer.write_all(b"\n").await?;
        self.writer.flush().await
    }

    pub async fn next(&mut self) -> io::Result<Option<Message>> {
        let line = self.lines.next_line().await?;
        line.map(|line| decode(line.as_bytes()).map_err(io::Error::other)).transpose()
    }

    pub async fn replayed(&mut self, transcript: &str) -> io::Result<Vec<Message>> {
        const QUIET: Duration = Duration::from_millis(300);
        let mut seen = Vec::new();
        for line in transcript.lines() {
            let sent = decode::<Message>(line.as_bytes()).map_err(io::Error::other)?;
            match self.send(line).await {
                Ok(()) => (),
                Err(closed) if closed.kind() == io::ErrorKind::BrokenPipe => break,
                Err(unsent) => return Err(unsent),
            }
            let awaited = sent.id.filter(|_| sent.method.is_some());
            while let Some(id) = awaited.as_ref()
                && let Some(message) = self.next().await?
            {
                let answered = message.method.is_none() && message.id.as_ref() == Some(id);
                seen.push(message);
                if answered {
                    break;
                }
            }
        }
        while let Ok(late) = time::timeout(QUIET, self.next()).await
            && let Some(message) = late?
        {
            seen.push(message);
        }
        Ok(seen)
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
struct Left(Arc<Semaphore>);

impl Drop for Left {
    fn drop(&mut self) { self.0.add_permits(1); }
}

#[derive(Debug, Clone)]
pub struct Everything {
    pub routers: Routers<Self>,
    pub entered: Arc<Semaphore>,
    pub cancelled: Arc<Semaphore>,
    pub watched: watch::Sender<u64>,
}

impl Server for Everything {}

#[tool_router]
#[prompt_router]
impl Everything {
    #[tool(description = "Tests simple text response")]
    fn test_simple_text() -> String { "This is a simple text response for testing.".to_owned() }

    #[tool(description = "Tests image content response")]
    fn test_image_content() -> CallToolResult { CallToolResult::success(vec![ContentBlock::image(IMAGE, "image/png")]) }

    #[tool(description = "Tests audio content response")]
    fn test_audio_content() -> CallToolResult { CallToolResult::success(vec![ContentBlock::audio(AUDIO, "audio/wav")]) }

    #[tool(description = "Tests embedded resource content response")]
    fn test_embedded_resource() -> CallToolResult {
        let resource = ResourceContents::text("This is an embedded resource content.", "test://embedded-resource");
        CallToolResult::success(vec![ContentBlock::resource(resource)])
    }

    #[tool(description = "Tests response with multiple content types (text, image, resource)")]
    fn test_multiple_content_types() -> CallToolResult {
        let resource = ResourceContents::text(r#"{"test":"data","value":123}"#, "test://mixed-content-resource").with_mime_type("application/json");
        CallToolResult::success(vec![ContentBlock::text("Multiple content types test:"), ContentBlock::image(IMAGE, "image/png"), ContentBlock::resource(resource)])
    }

    #[tool(description = "Tests tool that emits log messages during execution")]
    async fn test_tool_with_logging(Extension(log): Extension<Log>, context: RequestContext<RoleServer>) -> Result<String, ErrorData> {
        for message in ["Tool execution started", "Tool processing data", "Tool execution completed"] {
            log.send(&context, LoggingMessageNotificationParam::new(LoggingLevel::Info, Value::from(message))).await.map_err(|error| ErrorData::internal_error(error.to_string(), None))?;
            time::sleep(Duration::from_millis(50)).await;
        }
        Ok("Logging complete".to_owned())
    }

    #[tool(description = "Tests error response handling")]
    fn test_error_handling() -> CallToolResult { CallToolResult::error(vec![ContentBlock::text("This tool intentionally returns an error for testing")]) }

    #[tool(description = "Tests tool that reports progress notifications")]
    async fn test_tool_with_progress(Extension(progress): Extension<Progress>, context: RequestContext<RoleServer>) -> String {
        const TOTAL: f64 = 100.0;
        if let Some(token) = context.meta.get_progress_token() {
            for (done, message) in [(0.0, "Starting"), (50.0, "Halfway"), (TOTAL, "Complete")] {
                progress.report(ProgressNotificationParam::new(token.clone(), done).with_total(TOTAL).with_message(message));
                time::sleep(Duration::from_millis(50)).await;
            }
        }
        "Progress complete".to_owned()
    }

    #[tool(description = "Tests server-initiated sampling (LLM completion request)")]
    async fn test_sampling(Parameters(Prompted { prompt }): Parameters<Prompted>, context: RequestContext<RoleServer>) -> Result<String, ErrorData> {
        let sampled = context.peer.create_message(CreateMessageRequestParams::new(vec![SamplingMessage::user_text(prompt)], 100)).await;
        let answered = sampled.map_err(|error| ErrorData::internal_error(error.to_string(), None))?;
        let text = answered.message.content.first().and_then(|block| block.as_text()).ok_or_else(|| ErrorData::internal_error("sampling answered no text", None))?;
        Ok(format!("LLM response: {}", text.text))
    }

    #[tool(description = "Tests server-initiated elicitation (user input request)")]
    async fn test_elicitation(Parameters(Asked { message }): Parameters<Asked>, Extension(elicitation): Extension<Elicitation>) -> Result<String, ErrorData> {
        let answered = elicitation.elicit::<Identity>(message).await?;
        let content = serde_json::to_string(&answered).map_err(|error| ErrorData::internal_error(error.to_string(), None))?;
        Ok(format!("User response: action=accept, content={content}"))
    }

    #[tool(description = "Tests elicitation with default values per SEP-1034")]
    async fn test_elicitation_sep1034_defaults(context: RequestContext<RoleServer>) -> Result<String, ErrorData> {
        asked(context, "Please provide values (all have defaults)", include_str!("fixture/sep1034-defaults.json")).await
    }

    #[tool(description = "Tests elicitation with enum schema improvements per SEP-1330")]
    async fn test_elicitation_sep1330_enums(context: RequestContext<RoleServer>) -> Result<String, ErrorData> {
        asked(context, "Test enum schema improvements", include_str!("fixture/sep1330-enums.json")).await
    }

    #[tool(description = "Fails unless the client declared the sampling capability")]
    fn test_missing_capability(RequestContext { meta, .. }: RequestContext<RoleServer>) -> Result<String, ErrorData> {
        let declared = meta.client_capabilities().unwrap_or_default();
        declared.sampling.map(|_| "sampling is declared".to_owned()).ok_or_else(|| ErrorData::missing_required_client_capability(ClientCapabilities::builder().enable_sampling().build()))
    }

    #[tool(description = "Asks for input inside its own result stream")]
    async fn test_streaming_elicitation(Extension(elicitation): Extension<Elicitation>) -> Result<String, ErrorData> {
        let Identity { username, email: _ } = elicitation.elicit("Provide a value").await?;
        Ok(username)
    }

    #[tool(description = "Logs one message when the request set a log level")]
    async fn test_logging_tool(context: RequestContext<RoleServer>) -> Result<String, ErrorData> {
        if let Some(level) = context.meta.log_level() {
            context
                .peer
                .notify_logging_message(LoggingMessageNotificationParam::new(level, Value::from("logLevel was requested")))
                .await
                .map_err(|error| ErrorData::internal_error(error.to_string(), None))?;
        }
        Ok("logged".to_owned())
    }

    #[tool(description = "Changes the tool list")]
    fn test_trigger_tool_change(&self) -> String {
        const TOGGLED: &str = "greet";
        self.routers.disable(TOGGLED);
        self.routers.enable(TOGGLED);
        "tools changed".to_owned()
    }

    #[tool(description = "Changes the prompt list")]
    fn test_trigger_prompt_change(&self) -> String {
        self.routers.prompts(Self::prompt_router());
        "prompts changed".to_owned()
    }

    #[tool(description = "say hi")]
    fn greet(Parameters(Greeted { name }): Parameters<Greeted>) -> String { format!("hi {name}") }

    #[tool(description = "structured output")]
    fn structured(Parameters(Input { input }): Parameters<Input>) -> Json<Output> { Json(Output { output: format!("Ack {input}") }) }

    #[tool(description = "Holds its call open until the client cancels it")]
    async fn test_held(&self, context: RequestContext<RoleServer>) -> Result<String, ErrorData> {
        self.entered.add_permits(1);
        let _left = Left(Arc::clone(&self.cancelled));
        context.ct.run_until_cancelled(pending::<()>()).await;
        Err(ErrorData::internal_error("cancelled", None))
    }
    #[prompt(name = "test_simple_prompt", description = "A simple prompt without arguments")]
    async fn test_simple_prompt(&self) -> Vec<PromptMessage> { vec![PromptMessage::new_text(Role::User, "This is a simple prompt for testing.")] }

    #[prompt(name = "test_prompt_with_arguments", description = "A prompt with required arguments")]
    async fn test_prompt_with_arguments(&self, Parameters(Arguments { arg1, arg2 }): Parameters<Arguments>) -> Vec<PromptMessage> {
        vec![PromptMessage::new_text(Role::User, format!("Prompt with arguments: arg1='{arg1}', arg2='{arg2}'"))]
    }

    #[prompt(name = "test_prompt_with_embedded_resource", description = "A prompt that includes an embedded resource")]
    async fn test_prompt_with_embedded_resource(&self, Parameters(Embedded { resource_uri }): Parameters<Embedded>) -> Vec<PromptMessage> {
        let embedded = PromptMessage::new_resource(Role::User, resource_uri, Some(TEXT.to_owned()), Some("Embedded resource content for testing.".to_owned()), None, None, None);
        vec![embedded, PromptMessage::new_text(Role::User, "Please process the embedded resource above.")]
    }

    #[prompt(name = "test_prompt_with_image", description = "A prompt that includes image content")]
    async fn test_prompt_with_image(&self) -> Vec<PromptMessage> {
        vec![PromptMessage::new(Role::User, ContentBlock::image(IMAGE, "image/png")), PromptMessage::new_text(Role::User, "Please analyze the image above.")]
    }
}

#[derive(Debug, Clone)]
pub struct Recorder {
    pub seen: mpsc::UnboundedSender<Seen>,
    pub action: ElicitationAction,
    pub content: Option<Value>,
    pub version: ProtocolVersion,
}

impl Recorder {
    #[must_use]
    pub fn new(version: ProtocolVersion) -> (Self, mpsc::UnboundedReceiver<Seen>) {
        let (seen, events) = mpsc::unbounded_channel();
        (Self { seen, action: ElicitationAction::Accept, content: serde_json::to_value(Identity { username: "testuser".to_owned(), email: "test@example.com".to_owned() }).ok(), version }, events)
    }

    fn noted(&self, seen: Seen) -> impl Future<Output = ()> + Send + use<> {
        if let Err(mpsc::error::SendError(unread)) = self.seen.send(seen) {
            drop(unread);
        }
        ready(())
    }
}

impl ClientHandler for Recorder {
    fn get_info(&self) -> ClientConfig {
        let mut info = ClientConfig::new(ClientCapabilities::builder().enable_elicitation().enable_sampling().enable_roots().build(), Implementation::new("recorder", "1.0.0"));
        info.protocol_version = self.version.clone();
        info
    }

    fn create_elicitation(&self, request: ElicitRequestParams, _: RequestContext<RoleClient>) -> impl Future<Output = Result<ElicitResult, ErrorData>> + Send + '_ {
        let answer = ElicitResult::new(self.action.clone());
        self.noted(Seen::Elicited(request)).map(move |()| Ok(self.content.clone().into_iter().fold(answer, ElicitResult::with_content)))
    }

    fn create_message(&self, params: CreateMessageRequestParams, _: RequestContext<RoleClient>) -> impl Future<Output = Result<CreateMessageResult, ErrorData>> + Send + '_ {
        let answer = CreateMessageResult::new(SamplingMessage::assistant_text("This is a test response from the client"), "test-model".to_owned());
        self.noted(Seen::Sampled(params)).map(|()| Ok(answer.with_stop_reason(CreateMessageResult::STOP_REASON_END_TURN)))
    }

    fn on_cancelled(&self, params: CancelledNotificationParam, _: NotificationContext<RoleClient>) -> impl Future<Output = ()> + Send + '_ { self.noted(Seen::Cancelled(params)) }

    fn on_progress(&self, params: ProgressNotificationParam, _: NotificationContext<RoleClient>) -> impl Future<Output = ()> + Send + '_ { self.noted(Seen::Progress(params)) }

    fn on_logging_message(&self, params: LoggingMessageNotificationParam, _: NotificationContext<RoleClient>) -> impl Future<Output = ()> + Send + '_ { self.noted(Seen::Logged(params)) }

    fn on_resource_updated(&self, params: ResourceUpdatedNotificationParam, _: NotificationContext<RoleClient>) -> impl Future<Output = ()> + Send + '_ { self.noted(Seen::Updated(params.uri)) }

    fn on_resource_list_changed(&self, _: NotificationContext<RoleClient>) -> impl Future<Output = ()> + Send + '_ { self.noted(Seen::ResourcesChanged) }

    fn on_tool_list_changed(&self, _: NotificationContext<RoleClient>) -> impl Future<Output = ()> + Send + '_ { self.noted(Seen::ToolsChanged) }

    fn on_prompt_list_changed(&self, _: NotificationContext<RoleClient>) -> impl Future<Output = ()> + Send + '_ { self.noted(Seen::PromptsChanged) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn asked(context: RequestContext<RoleServer>, message: &str, schema: &str) -> Result<String, ErrorData> {
    let requested_schema = decode(schema.as_bytes()).map_err(|error| ErrorData::internal_error(error.to_string(), None))?;
    let params = ElicitRequestParams::FormElicitationParams { meta: None, message: message.to_owned(), requested_schema };
    let answered = context.peer.create_elicitation(params).await.map_err(|error| ErrorData::internal_error(error.to_string(), None))?;
    Ok(format!("Elicitation completed: action={:?}, content={:?}", answered.action, answered.content))
}

pub async fn routed(notifier: broadcast::Sender<ServerNotification>) -> Result<(Router<Everything>, Everything), McpError> {
    let settings = Settings {
        instructions: Some("Everything server for the MCP suites".to_owned()),
        page: const { NonZero::new(50).unwrap() },
        retention: Duration::from_mins(1),
        versions: ProtocolVersion::KNOWN_VERSIONS,
    };
    let listed = |uri: &str, name: &str, mime: &str| Resource::new(uri, name).with_mime_type(mime);
    let binary = || ready(vec![ResourceContents::BlobResourceContents { uri: "test://static-binary".to_owned(), mime_type: Some("image/png".to_owned()), blob: IMAGE.to_owned(), meta: None }]);
    let matching = |values: [&str; 3], argument: &ArgumentInfo| ready(Ok::<_, ErrorData>(Vec::from_iter(values.into_iter().filter(|value| value.starts_with(&argument.value)).map(str::to_owned))));
    let resources = ResourceRouter::default()
        .resource(listed("test://static-text", "Static Text Resource", TEXT), |_| ready("This is the content of the static text resource.".to_owned()), |_| stream::pending())
        .resource(listed("test://static-binary", "Static Binary Resource", "image/png"), move |_| binary(), |_| stream::pending())
        .resource(listed(WATCHED, "Watched Resource", TEXT), |_| ready("Watched resource content".to_owned()), |app: Everything| WatchStream::from_changes(app.watched.subscribe()).map(drop))
        .template(
            ResourceTemplate::new("test://template/{id}/data", "Dynamic Resource").with_mime_type("application/json"),
            |_, Row { id }: Row| {
                let data = format!("Data for ID: {id}");
                ready(Json(Templated { id, template_test: true, data }))
            },
            |_, _: Row| stream::pending(),
            move |_: Everything, argument: ArgumentInfo, _: Option<CompletionContext>| matching(["1", "2", "3"], &argument),
        );
    let completions = CompletionRouter::default()
        .prompt("test_prompt_with_arguments", move |_: Everything, argument: ArgumentInfo, _: Option<CompletionContext>| matching(["testing", "tested", "other"], &argument));
    let mut built = None;
    let registered = async |routers: Routers<Everything>| {
        routers.tools(Everything::tool_router())?;
        routers.prompts(Everything::prompt_router());
        routers.resources(resources)?;
        routers.completions(completions)?;
        let app = Everything { routers, entered: Arc::new(Semaphore::new(0)), cancelled: Arc::new(Semaphore::new(0)), watched: watch::Sender::new(0) };
        built = Some(app.clone());
        Ok(app)
    };
    let router = Router::new(Implementation::new("everything", "1.0.0"), settings, notifier, registered).await?;
    built.map(|app| (router, app)).ok_or(McpError::Stopping)
}

pub async fn everything() -> Result<(Arc<Shared<Router<Everything>>>, Everything), McpError> {
    let mut built = None;
    let shared = Shared::new(64, async |notifier| {
        let (router, app) = routed(notifier).await?;
        built = Some(app);
        Ok::<_, McpError>(router)
    });
    let shared = shared.await?;
    built.map(|app| (shared, app)).ok_or(McpError::Stopping)
}

#[must_use]
pub fn replies(seen: &[Message]) -> Vec<(Option<Id>, Reply)> {
    let answered = seen.iter().filter(|message| message.method.is_none());
    answered.map(|message| (message.id.clone(), if message.error.is_some() { Reply::Error } else { Reply::Result })).collect()
}

pub fn accepted<S>(service: S) -> (DuplexStream, AbortOnDropHandle<io::Result<()>>)
where S: Service<RoleServer> + Send + 'static {
    let (near, far) = tokio::io::duplex(64 << 10);
    let serving = async move {
        let running = service.serve(far).await.map_err(io::Error::other)?;
        running.waiting().await.map(drop).map_err(io::Error::other)
    };
    (near, AbortOnDropHandle::new(tokio::spawn(serving)))
}

pub async fn paired<S>(service: S, client: Recorder, lifecycle: ClientLifecycleMode) -> io::Result<(RunningService<RoleClient, Recorder>, AbortOnDropHandle<io::Result<()>>)>
where S: Service<RoleServer> + Send + 'static {
    let (near, serving) = accepted(service);
    client.serve_with_lifecycle(near, lifecycle).await.map(|running| (running, serving)).map_err(io::Error::other)
}

pub fn wired<S>(service: S) -> (Wire<ReadHalf<DuplexStream>, WriteHalf<DuplexStream>>, AbortOnDropHandle<io::Result<()>>)
where S: Service<RoleServer> + Send + 'static {
    let (near, serving) = accepted(service);
    let (reader, writer) = tokio::io::split(near);
    (Wire::new(reader, writer), serving)
}

pub async fn served<S>(factory: impl Fn() -> S + Send + Sync + 'static) -> io::Result<(String, AbortOnDropHandle<io::Result<()>>)>
where S: ServerHandler + Send + 'static {
    let listener = TcpListener::bind("127.0.0.1:0").await?;
    let address = listener.local_addr()?;
    let config = StreamableHttpServerConfig::default().with_allowed_origins([format!("http://{address}")]).enforce_origin_validation();
    let service = StreamableHttpService::new(move || Ok(factory()), LocalSessionManager::default().into(), config);
    let serving = axum::serve(listener, axum::Router::new().nest_service("/mcp", service)).into_future();
    Ok((format!("http://{address}/mcp"), AbortOnDropHandle::new(tokio::spawn(serving))))
}

pub async fn conformed(url: &str, scenario: &str, revision: &str, baseline: &Path, results: &Path) -> io::Result<Conformed> {
    let mut command = Command::new("conformance");
    command.args(["server", "--url", url, "--scenario", scenario, "--spec-version", revision, "--expected-failures"]).arg(baseline).arg("--output-dir").arg(results);
    let output = command.current_dir(results).stdin(Stdio::null()).output().await?;
    let mut checks = Vec::new();
    let mut runs = fs_err::tokio::read_dir(results).await?;
    while let Some(run) = runs.next_entry().await? {
        match fs_err::tokio::read(run.path().join("checks.json")).await {
            Ok(written) => checks.extend(decode::<Vec<Check>>(&written).map_err(io::Error::other)?),
            Err(absent) if absent.kind() == io::ErrorKind::NotFound => (),
            Err(unread) => return Err(unread),
        }
    }
    Ok(Conformed { code: output.status.code(), checks, output: format!("{}{}", String::from_utf8_lossy(&output.stdout), String::from_utf8_lossy(&output.stderr)) })
}
