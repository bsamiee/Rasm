use core::error::Error;
use core::future::ready;
use core::pin::pin;
use core::time::Duration;
use std::borrow::Cow;
use std::collections::BTreeMap;
use std::error::Report;
use std::sync::Arc;

use futures::future::{self, BoxFuture, Either};
use futures::{FutureExt as _, StreamExt as _, stream};
use jsonschema::validator_for;
use nonempty_collections::NEVec;
use rmcp::handler::server::tool::{IntoCallToolResult, ToolCallContext, ToolRoute};
use rmcp::handler::server::tool_name_validation::validate_and_warn_tool_name;
use rmcp::model::{
    CallToolRequestMethod, CallToolRequestParams, CallToolResponse, CallToolResult, CreateTaskResult, ElicitRequest, ElicitRequestParams, ElicitationAction, ElicitationSchema, InputRequest,
    InputRequiredResult, JsonObject, NumberOrString, ProgressToken, ServerResult,
};
use rmcp::service::{ElicitationSafe, RequestContext};
use rmcp::task_manager::{TaskContext, TaskExit, TaskOptions};
use rmcp::{ErrorData, RoleServer, ServiceError};
use serde::Serialize;
use serde::de::DeserializeOwned;
use serde_json::Value;
use tokio::sync::{mpsc, oneshot, watch};
use tokio::time::{self, Instant};
use tokio_stream::wrappers::{UnboundedReceiverStream, WatchStream};
use tokio_util::sync::CancellationToken;
use tokio_util::task::AbortOnDropHandle;
use url::Url;
use uuid::Uuid;
use validated::Validated;

use super::{Router, Server};
use crate::error::internal;
use crate::{McpError, Progress, ToolError};

// --- [TYPES] ---------------------------------------------------------------------------

pub(super) type Paused = BTreeMap<String, (Running, AbortOnDropHandle<()>)>;

pub(super) type Executions = watch::Sender<BTreeMap<usize, Execution>>;

// --- [MODELS] --------------------------------------------------------------------------

impl<E> IntoCallToolResult for ToolError<E>
where E: Serialize + Error
{
    fn into_call_tool_result(self) -> Result<CallToolResponse, ErrorData> {
        #[derive(Serialize)]
        struct Rendered<'error, E> {
            #[serde(flatten)]
            error: &'error E,
            message: String,
        }
        serde_json::to_value(Rendered { error: &self.0, message: Report::new(&self.0).to_string() }).map(|value| CallToolResult::structured_error(value).into()).map_err(internal)
    }
}

#[derive(Debug, Clone, Serialize)]
pub(super) struct Execution {
    tool: Cow<'static, str>,
    arguments: Option<JsonObject>,
    outcome: Option<Outcome>,
    #[serde(skip)]
    since: Instant,
}

#[derive(Debug, Clone, Serialize)]
#[serde(tag = "_tag")]
enum Outcome {
    Returned { result: Box<ServerResult> },
    Failed { error: ErrorData },
}

struct AwaitingInput {
    request: InputRequest,
    responder: oneshot::Sender<Value>,
}

pub(super) struct Running {
    call: BoxFuture<'static, Result<CallToolResponse, ErrorData>>,
    questions: mpsc::UnboundedReceiver<(String, AwaitingInput)>,
    pending: BTreeMap<String, AwaitingInput>,
    progress: Progress,
    execution: Execution,
    destructive: bool,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug)]
pub struct Elicitation {
    context: RequestContext<RoleServer>,
    asked: mpsc::UnboundedSender<(String, AwaitingInput)>,
}

impl Elicitation {
    pub async fn elicit<T>(&self, message: impl Into<String>) -> Result<T, McpError>
    where T: ElicitationSafe + DeserializeOwned {
        let requested_schema = ElicitationSchema::from_type::<T>().map_err(|source| McpError::Form { source })?;
        let content = self.ask(ElicitRequestParams::FormElicitationParams { meta: None, message: message.into(), requested_schema }).await?;
        let content = content.ok_or(McpError::Elicitation { action: ElicitationAction::Accept })?;
        serde_path_to_error::deserialize(&content).map_err(|source| McpError::Answer { data: content, source })
    }

    pub async fn elicit_url(&self, message: impl Into<String>, url: Url, elicitation_id: impl Into<String>) -> Result<(), McpError> {
        self.ask(ElicitRequestParams::UrlElicitationParams { meta: None, message: message.into(), url: url.into(), elicitation_id: elicitation_id.into() }).await.map(drop)
    }

    async fn ask(&self, params: ElicitRequestParams) -> Result<Option<Value>, McpError> {
        let abandoned = || McpError::Abandoned;
        let supported = self.context.client_capabilities().and_then(|capabilities| capabilities.elicitation).is_some_and(|modes| match &params {
            ElicitRequestParams::FormElicitationParams { .. } => modes.form.is_some() || modes.url.is_none(),
            ElicitRequestParams::UrlElicitationParams { .. } => modes.url.is_some(),
            _ => false,
        });
        supported.ok_or(McpError::Unsupported)?;
        let result = if self.context.protocol_version().is_none_or(|version| version.has_initialize()) {
            self.context.peer.create_elicitation(params).await.map_err(|source| McpError::Service { source })?
        } else {
            let (responder, answered) = oneshot::channel();
            self.asked
                .send((Uuid::new_v4().to_string(), AwaitingInput { request: InputRequest::Elicitation(ElicitRequest::new(params)), responder }))
                .map_err(|mpsc::error::SendError(_)| abandoned())?;
            let answer = answered.await.map_err(|oneshot::error::RecvError { .. }| abandoned())?;
            serde_path_to_error::deserialize(&answer).map_err(|source| McpError::Answer { data: answer, source })?
        };
        match result.action {
            ElicitationAction::Accept => Ok(result.content),
            action => Err(McpError::Elicitation { action }),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(super) fn validated<A>(route: ToolRoute<A>) -> Validated<(Cow<'static, str>, ToolRoute<A>), McpError>
where A: Send + Sync + 'static {
    let ToolRoute { call, attr, .. } = route;
    let named = validate_and_warn_tool_name(&attr.name).ok_or_else(|| McpError::ToolName { tool: attr.name.clone() });
    let compiled = validator_for(&attr.schema_as_json_value()).map_err(|source| McpError::Schema { tool: attr.name.clone(), source });
    Validated::from(named).map2(Validated::from(compiled), |(), validator| {
        let name = attr.name.clone();
        let routed = ToolRoute::new_dyn(attr, move |context: ToolCallContext<'_, A>| {
            let arguments = Value::Object(context.arguments.clone().unwrap_or_default());
            let violations = validator.evaluate(&arguments).iter_errors().map(|entry| (entry.instance_location.clone(), entry.error.message().to_owned())).collect();
            match NEVec::try_from_vec(violations) {
                None => call(context),
                Some(violations) => ready(ToolError(McpError::Arguments { tool: context.name, violations }).into_call_tool_result()).boxed(),
            }
        });
        (name, routed)
    })
}

pub(super) async fn call<A>(server: &Router<A>, request: CallToolRequestParams, context: RequestContext<RoleServer>) -> Result<CallToolResponse, ErrorData>
where A: Server {
    fn deliver(key: &str, responder: oneshot::Sender<Value>, answer: Value) {
        responder.send(answer).unwrap_or_else(|unawaited| tracing::debug!(question = key, %unawaited, "question no longer awaits its answer"));
    }
    async fn tasked(running: Running, task: TaskContext, halted: CancellationToken) -> Result<CallToolResult, TaskExit> {
        let Running { mut call, questions, pending, progress, execution: _, destructive } = running;
        progress.seed(ProgressToken(NumberOrString::String(task.task_id().into())));
        let asked = stream::iter(pending).chain(UnboundedReceiverStream::new(questions)).map(Either::Left);
        let reported = WatchStream::new(progress.0.subscribe()).filter_map(|latest| ready(latest.and_then(|held| held.message))).map(Either::Right);
        let side = stream::select(asked, reported).for_each_concurrent(None, |item| {
            let task = task.clone();
            async move {
                match item {
                    Either::Left((key, AwaitingInput { request, responder })) =>
                        task.request_input(key.clone(), request).await.map_or_else(|exit| tracing::debug!(?exit, "task input ended unanswered"), |answer| deliver(&key, responder, answer)),
                    Either::Right(message) => task.set_status_message(message),
                }
            }
        });
        let settled = async {
            tokio::select! {
                biased;
                completed = &mut call => match completed? {
                    CallToolResponse::Complete(result) => Ok(result),
                    _ => Err(TaskExit::Error(internal(ServiceError::UnexpectedResponse))),
                },
                () = task.cancelled(), if !destructive => Err(TaskExit::Cancelled),
                () = halted.cancelled(), if !destructive => Err(TaskExit::Cancelled),
            }
        };
        match future::select(pin!(settled), pin!(side)).await {
            Either::Left((settled, _)) => settled,
            Either::Right(((), settled)) => settled.await,
        }
    }
    async fn recorded(executions: &Executions, retention: Duration, execution: Execution, call: BoxFuture<'static, Result<CallToolResponse, ErrorData>>) {
        let mut key = 0;
        executions.send_modify(|rows| {
            rows.retain(|_, held| held.outcome.is_none() || held.since.elapsed() < retention);
            key = rows.last_key_value().map_or_default(|(last, _)| last.saturating_add(1));
            rows.insert(key, execution);
        });
        let outcome = match call.await {
            Ok(response) => Outcome::Returned { result: Box::new(response.into()) },
            Err(error) => Outcome::Failed { error },
        };
        executions.send_modify(|rows| {
            rows.entry(key).and_modify(|held| (held.outcome, held.since) = (Some(outcome), Instant::now()));
        });
    }
    let tools = server.routers.tools.load_full().ok_or_else(ErrorData::method_not_found::<CallToolRequestMethod>)?;
    let resumed = server.paused.with_mut(|waiting| {
        let retried = |_: &String, (held, _): &mut (Running, AbortOnDropHandle<()>)| (&held.execution.tool, &held.execution.arguments) == (&request.name, &request.arguments);
        request.request_state.as_ref().map(|state| waiting.extract_if(state..=state, retried).next().map(|(_, (held, _))| held).ok_or(McpError::RequestState)).transpose()
    })?;
    let mut running = if let Some(mut held) = resumed {
        request
            .input_responses
            .into_iter()
            .flatten()
            .filter_map(|(key, answer)| held.pending.remove(&key).map(|AwaitingInput { request: _, responder }| (key, responder, answer)))
            .for_each(|(key, responder, answer)| deliver(&key, responder, answer));
        held
    } else {
        let destructive = tools.get(&request.name).is_some_and(|tool| tool.annotations.as_ref().is_none_or(|hints| hints.read_only_hint != Some(true) && hints.is_destructive()));
        let execution = Execution { tool: request.name.clone(), arguments: request.arguments.clone(), outcome: None, since: Instant::now() };
        let (progress, (asked, questions), mut called) = (Progress::default(), mpsc::unbounded_channel(), context.clone());
        called.ct = CancellationToken::new();
        called.extensions.insert(progress.clone());
        called.extensions.insert(Elicitation { context: called.clone(), asked });
        let (app, released) = (server.app.clone(), called.ct.clone().drop_guard());
        let call = async move { tools.call(ToolCallContext::new(&app, request, called)).await };
        Running { call: call.inspect(move |_| drop(released)).boxed(), questions, pending: BTreeMap::new(), progress, execution, destructive }
    };
    let pause = |paused: Running| {
        let state = Uuid::new_v4().to_string();
        let required = InputRequiredResult::new(Some(paused.pending.iter().map(|(key, AwaitingInput { request: input, responder: _ })| (key.clone(), input.clone())).collect()), Some(state.clone()));
        let (calls, expiring) = (Arc::clone(&server.paused), state.clone());
        let expiry = AbortOnDropHandle::new(server.tracker.spawn(time::sleep(server.retention).map(move |()| drop(calls.with_mut(|held| held.remove(&expiring))))));
        drop(server.paused.with_mut(|waiting| waiting.insert(state, (paused, expiry))));
        CallToolResponse::from(required)
    };
    if context.client_capabilities().is_some_and(|capabilities| capabilities.supports_tasks()) && server.info.capabilities.supports_tasks() {
        let options = TaskOptions::default().with_ttl_ms((!running.destructive).then(|| server.retention.as_millis().saturating_cast::<u64>()));
        Ok(CreateTaskResult::new(server.tasks.spawn(options, |task| server.tracker.track_future(tasked(running, task, server.halted.clone())).boxed())).into())
    } else if running.pending.is_empty() {
        let mut asked = Vec::new();
        tokio::select! {
            biased;
            1.. = running.questions.recv_many(&mut asked, usize::MAX) => {
                running.pending.extend(asked);
                Ok(pause(running))
            }
            completed = running.progress.relay(&context, &mut running.call) => completed,
            () = context.ct.cancelled() => {
                if running.destructive {
                    recorded(&server.routers.executions, server.retention, running.execution, running.call).await;
                }
                Err(internal(ServiceError::Cancelled { reason: None }))
            }
        }
    } else {
        Ok(pause(running))
    }
}
