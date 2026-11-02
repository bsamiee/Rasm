use core::{fmt::Display, future::ready, pin::pin};
use std::{borrow::Cow, collections::BTreeMap};

use futures::{
    FutureExt as _, StreamExt as _,
    future::{self, BoxFuture, Either},
    stream,
};
use jsonschema::validator_for;
use nonempty_collections::NEVec;
use rmcp::{
    ErrorData, RoleServer, ServiceError,
    handler::server::tool::{IntoCallToolResult, ToolCallContext, ToolRoute},
    model::{
        CallToolRequestMethod, CallToolRequestParams, CallToolResponse, CallToolResult, ContentBlock, CreateTaskResult, ElicitRequest, ElicitRequestParams, ElicitationAction, ElicitationSchema,
        InputRequest, InputRequiredResult, JsonObject, NumberOrString, ProgressToken, ServerResult,
    },
    service::{ElicitationSafe, RequestContext},
    task_manager::{TaskContext, TaskExit, TaskOptions},
};
use serde::{Serialize, de::DeserializeOwned};
use serde_json::Value;
use tokio::{
    sync::{mpsc, oneshot},
    time::Instant,
};
use tokio_stream::wrappers::{UnboundedReceiverStream, WatchStream};
use tokio_util::sync::CancellationToken;
use uuid::Uuid;

use super::{App, Server};
use crate::{McpError, Progress, Unanswered, internal};

// --- [TYPES] ---------------------------------------------------------------------------

type Asked = (InputRequest, oneshot::Sender<Value>);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, derive_more::From)]
pub struct Failure<E>(pub E);

impl<E> IntoCallToolResult for Failure<E>
where E: Serialize + Display
{
    fn into_call_tool_result(self) -> Result<CallToolResponse, ErrorData> {
        #[derive(Serialize)]
        struct Rendered<'error, E> {
            #[serde(flatten)]
            error: &'error E,
            message: String,
        }
        ContentBlock::json(Rendered { error: &self.0, message: self.0.to_string() }).map(|block| CallToolResult::error(vec![block]).into())
    }
}

impl<E> From<Failure<E>> for ErrorData
where E: Serialize + Display
{
    fn from(value: Failure<E>) -> Self { serde_json::to_value(&value.0).map_or_else(internal, |data| Self::internal_error(value.0.to_string(), Some(data))) }
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

pub(super) struct Running {
    call: BoxFuture<'static, Result<CallToolResponse, ErrorData>>,
    questions: mpsc::UnboundedReceiver<(String, Asked)>,
    pending: BTreeMap<String, Asked>,
    progress: Progress,
    execution: Execution,
    destructive: bool,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug)]
pub struct Elicitation {
    context: RequestContext<RoleServer>,
    asked: mpsc::UnboundedSender<(String, Asked)>,
}

impl Elicitation {
    pub async fn elicit<T>(&self, message: impl Into<String>) -> Result<T, McpError>
    where T: ElicitationSafe + DeserializeOwned {
        let abandoned = || McpError::Elicitation { reason: Unanswered::Abandoned };
        self.context
            .client_capabilities()
            .and_then(|capabilities| capabilities.elicitation)
            .is_some_and(|modes| modes.form.is_some() || modes.url.is_none())
            .ok_or(McpError::Elicitation { reason: Unanswered::Unsupported })?;
        let requested_schema = ElicitationSchema::from_type::<T>().map_err(|source| McpError::Unaskable { source })?;
        let params = ElicitRequestParams::FormElicitationParams { meta: None, message: message.into(), requested_schema };
        let result = if self.context.protocol_version().is_none_or(|version| version.has_initialize()) {
            self.context.peer.create_elicitation(params).await.map_err(|source| McpError::Unreached { source })?
        } else {
            let (responder, answered) = oneshot::channel();
            self.asked.send((Uuid::new_v4().to_string(), (InputRequest::Elicitation(ElicitRequest::new(params)), responder))).map_err(|mpsc::error::SendError(_)| abandoned())?;
            let answer = answered.await.map_err(|oneshot::error::RecvError { .. }| abandoned())?;
            serde_path_to_error::deserialize(&answer).map_err(|source| McpError::Answer { data: answer, source })?
        };
        match (result.action, result.content) {
            (ElicitationAction::Accept, Some(content)) => serde_path_to_error::deserialize(&content).map_err(|source| McpError::Answer { data: content, source }),
            (ElicitationAction::Accept, None) => Err(McpError::Elicitation { reason: Unanswered::Empty }),
            (ElicitationAction::Decline, _) => Err(McpError::Elicitation { reason: Unanswered::Declined }),
            (_, _) => Err(McpError::Elicitation { reason: Unanswered::Dismissed }),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(super) fn validated<A>(route: ToolRoute<A>) -> Result<ToolRoute<A>, McpError>
where A: Send + Sync + 'static {
    let ToolRoute { call, attr, .. } = route;
    let validator = validator_for(&attr.schema_as_json_value()).map_err(|source| McpError::Schema { tool: attr.name.clone(), source })?;
    Ok(ToolRoute::new_dyn(attr, move |context: ToolCallContext<'_, A>| {
        let arguments = Value::Object(context.arguments.clone().unwrap_or_default());
        let violations = validator.evaluate(&arguments).iter_errors().map(|entry| (entry.instance_location.clone(), entry.error.message().to_owned())).collect();
        match NEVec::try_from_vec(violations) {
            None => call(context),
            Some(violations) => ready(Failure(McpError::Arguments { tool: context.name, violations }).into_call_tool_result()).boxed(),
        }
    }))
}

pub(super) async fn call<A>(server: &Server<A>, request: CallToolRequestParams, context: RequestContext<RoleServer>) -> Result<CallToolResponse, ErrorData>
where A: App {
    fn deliver(key: &str, responder: oneshot::Sender<Value>, answer: Value) {
        responder.send(answer).unwrap_or_else(|unawaited| tracing::debug!(question = key, %unawaited, "question no longer awaits its answer"));
    }
    async fn tasked(running: Running, task: TaskContext) -> Result<CallToolResult, TaskExit> {
        let Running { mut call, questions, pending, progress, execution: _, destructive } = running;
        progress.seed(ProgressToken(NumberOrString::String(task.task_id().into())));
        let asked = stream::iter(pending).chain(UnboundedReceiverStream::new(questions)).map(Either::Left);
        let reported = WatchStream::new(progress.0.subscribe()).filter_map(|latest| ready(latest.and_then(|held| held.message))).map(Either::Right);
        let side = stream::select(asked, reported).for_each_concurrent(None, |item| {
            let task = task.clone();
            async move {
                match item {
                    Either::Left((key, (request, responder))) =>
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
            }
        };
        match future::select(pin!(settled), pin!(side)).await {
            Either::Left((settled, _)) => settled,
            Either::Right(((), settled)) => settled.await,
        }
    }
    let tools = server.changes.tools.load_full().ok_or_else(ErrorData::method_not_found::<CallToolRequestMethod>)?;
    let resumed = server.parked.with_mut(|waiting| {
        waiting.retain(|_, held| held.execution.since.elapsed() < server.retention);
        let retried = |_: &String, held: &mut Running| (&held.execution.tool, &held.execution.arguments) == (&request.name, &request.arguments);
        request.request_state.as_ref().map(|state| waiting.extract_if(state..=state, retried).next().ok_or(McpError::Unparked)).transpose()
    })?;
    let mut running = if let Some((_, mut held)) = resumed {
        request
            .input_responses
            .into_iter()
            .flatten()
            .filter_map(|(key, answer)| held.pending.remove(&key).map(|(_, responder)| (key, responder, answer)))
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
    let park = |mut parked: Running| {
        parked.execution.since = Instant::now();
        let state = Uuid::new_v4().to_string();
        let required = InputRequiredResult::new(Some(parked.pending.iter().map(|(key, (asked, _))| (key.clone(), asked.clone())).collect()), Some(state.clone()));
        server.parked.with_mut(|waiting| waiting.insert(state, parked));
        CallToolResponse::from(required)
    };
    if context.client_capabilities().is_some_and(|capabilities| capabilities.supports_tasks()) && server.info.capabilities.supports_tasks() {
        let options = TaskOptions::default().with_ttl_ms((!running.destructive).then(|| u64::try_from(server.retention.as_millis()).unwrap_or(u64::MAX)));
        Ok(CreateTaskResult::new(server.tasks.spawn(options, |task| tasked(running, task).boxed())).into())
    } else if running.pending.is_empty() {
        let mut asked = Vec::new();
        tokio::select! {
            biased;
            1.. = running.questions.recv_many(&mut asked, usize::MAX) => {
                running.pending.extend(asked);
                Ok(park(running))
            }
            completed = running.progress.relay(&context, &mut running.call) => completed,
            () = context.ct.cancelled() => {
                if let Running { call, questions: _, pending: _, progress: _, execution, destructive: true } = running {
                    let mut row = 0;
                    server.changes.ledger.send_modify(|rows| {
                        rows.retain(|_, held| held.outcome.is_none() || held.since.elapsed() < server.retention);
                        row = rows.last_key_value().map_or(0, |(last, _)| last.saturating_add(1));
                        rows.insert(row, execution);
                    });
                    let outcome = match call.await {
                        Ok(response) => Outcome::Returned { result: Box::new(response.into()) },
                        Err(error) => Outcome::Failed { error },
                    };
                    server.changes.ledger.send_modify(|rows| {
                        rows.entry(row).and_modify(|held| (held.outcome, held.since) = (Some(outcome), Instant::now()));
                    });
                }
                Err(internal(ServiceError::Cancelled { reason: None }))
            }
        }
    } else {
        Ok(park(running))
    }
}
