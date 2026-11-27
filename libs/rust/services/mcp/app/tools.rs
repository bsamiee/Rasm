use core::error::Error;
use core::future::ready;
use core::pin::pin;
use std::borrow::Cow;
use std::collections::BTreeMap;
use std::sync::Arc;
use std::sync::nonpoison::Mutex;

use derive_where::derive_where;
use futures::future::{self, BoxFuture, Either};
use futures::{FutureExt as _, StreamExt as _, stream};
use itertools::Itertools as _;
use jsonschema::validator_for;
use nonempty_collections::NEVec;
use rmcp::handler::server::tool::{IntoCallToolResult, ToolCallContext, ToolRoute};
use rmcp::handler::server::tool_name_validation::validate_and_warn_tool_name;
use rmcp::model::{
    CallToolRequestMethod, CallToolRequestParams, CallToolResponse, CallToolResult, ContentBlock, CreateTaskResult, ElicitRequest, ElicitRequestParams, ElicitationAction, ElicitationSchema,
    InputRequest, InputRequiredResult, JsonObject, ServerResult,
};
use rmcp::schemars::SchemaGenerator;
use rmcp::service::{ElicitationSafe, RequestContext};
use rmcp::task_manager::{TaskContext, TaskExit, TaskOptions};
use rmcp::{ErrorData, RoleServer, ServiceError};
use serde::de::DeserializeOwned;
use serde::{Deserialize, Serialize};
use serde_json::Value;
use serde_path_to_error::Track;
use tokio::sync::{mpsc, oneshot, watch};
use tokio::time;
use tokio_stream::wrappers::{UnboundedReceiverStream, WatchStream};
use tokio_util::sync::{CancellationToken, DropGuard};
use tokio_util::task::AbortOnDropHandle;
use url::Url;
use uuid::Uuid;
use validated::Validated;

use super::{Router, Server};
use crate::error::{Data, coded, internal, reported};
use crate::{Handle, McpError, Next, Progress, ToolError};

// --- [TYPES] ---------------------------------------------------------------------------

pub(super) type Paused = BTreeMap<String, (Running, AbortOnDropHandle<()>)>;

type Executions = Arc<Mutex<BTreeMap<usize, Execution>>>;

// --- [MODELS] --------------------------------------------------------------------------

impl<E> IntoCallToolResult for ToolError<E>
where E: Serialize + Error
{
    fn into_call_tool_result(self) -> Result<CallToolResponse, ErrorData> { structured(&self.0, None) }
}

impl IntoCallToolResult for McpError {
    fn into_call_tool_result(self) -> Result<CallToolResponse, ErrorData> { structured(&self, coded(&self).1) }
}

#[derive(Deserialize, Serialize)]
struct Declared {
    #[serde(default)]
    properties: JsonObject,
    #[serde(flatten)]
    rest: JsonObject,
}

#[derive(Deserialize)]
struct Addressed {
    handle: Option<Handle>,
    #[serde(flatten)]
    arguments: JsonObject,
}

#[derive_where(Clone)]
#[derive_where(Default; S: Default)]
pub(super) struct Record<S> {
    state: watch::Sender<S>,
    executions: Executions,
}

#[derive(Debug, Clone, Serialize)]
struct Execution {
    tool: Cow<'static, str>,
    arguments: Option<JsonObject>,
    outcome: Outcome,
}

#[derive(Debug, Clone, Serialize)]
#[serde(tag = "_tag")]
enum Outcome {
    Running,
    Returned { result: Box<ServerResult> },
    Failed { error: ErrorData },
}

#[derive(Debug, Serialize)]
struct Appended {
    #[serde(skip_serializing_if = "Option::is_none")]
    handle: Option<Handle>,
    #[serde(skip_serializing_if = "Vec::is_empty")]
    executions: Vec<Execution>,
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
    released: DropGuard,
    executions: Executions,
    minted: Option<Handle>,
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

fn structured<E>(error: &E, next: Option<Next>) -> Result<CallToolResponse, ErrorData>
where E: Serialize + Error {
    let mut result = CallToolResult::error(vec![ContentBlock::text(reported(error, next))]);
    result.structured_content = Some(serde_json::to_value(Data { error, next }).map_err(internal)?);
    Ok(result.into())
}

pub(super) fn validated<A>(route: ToolRoute<A>) -> Validated<(Cow<'static, str>, ToolRoute<A>), McpError>
where A: Send + Sync + 'static {
    let ToolRoute { call, mut attr, .. } = route;
    let declared = serde_path_to_error::deserialize::<_, Declared>(Value::Object(JsonObject::clone(&attr.input_schema)))
        .and_then(|Declared { mut properties, rest }| {
            properties.insert("handle".to_owned(), SchemaGenerator::default().subschema_for::<Handle>().into());
            let encoded = serde_json::to_value(Declared { properties, rest }).map_err(|source| serde_path_to_error::Error::new(Track::new().path(), source))?;
            serde_path_to_error::deserialize::<_, JsonObject>(encoded)
        })
        .map(|schema| attr.input_schema = Arc::new(schema))
        .map_err(|source| McpError::Properties { tool: attr.name.clone(), source });
    let named = validate_and_warn_tool_name(&attr.name).ok_or_else(|| McpError::ToolName { tool: attr.name.clone() });
    let compiled = validator_for(&attr.schema_as_json_value()).map_err(|source| McpError::Schema { tool: attr.name.clone(), source });
    Validated::from(declared).map3(Validated::from(named), Validated::from(compiled), |(), (), validator| {
        let name = attr.name.clone();
        let routed = ToolRoute::new_dyn(attr, move |context: ToolCallContext<'_, A>| {
            let arguments = Value::Object(context.arguments.clone().unwrap_or_default());
            let violations = validator.evaluate(&arguments).iter_errors().map(|entry| (entry.instance_location.clone(), entry.error.message().to_owned())).collect();
            match NEVec::try_from_vec(violations) {
                None => call(context),
                Some(violations) => ready(McpError::Arguments { tool: context.name, violations }.into_call_tool_result()).boxed(),
            }
        });
        (name, routed)
    })
}

pub(super) async fn call<A>(server: &Router<A>, mut request: CallToolRequestParams, context: RequestContext<RoleServer>) -> Result<CallToolResponse, ErrorData>
where A: Server {
    fn deliver(key: &str, responder: oneshot::Sender<Value>, answer: Value) {
        responder.send(answer).unwrap_or_else(|unawaited| tracing::debug!(question = key, %unawaited, "question no longer awaits its answer"));
    }
    fn appended(executions: &Executions, minted: Option<Handle>, mut result: CallToolResult) -> Result<CallToolResult, ErrorData> {
        let listed = executions.with_mut(|rows| {
            let finished = rows.extract_if(.., |_, held| !matches!(held.outcome, Outcome::Running)).map(|(_, held)| held).collect_vec();
            finished.into_iter().chain(rows.values().cloned()).collect_vec()
        });
        let appended = Appended { handle: minted, executions: listed };
        if appended.handle.is_some() || !appended.executions.is_empty() {
            result.content.push(ContentBlock::json(&appended)?);
        }
        Ok(result)
    }
    async fn tasked(running: Running, task: TaskContext, halted: CancellationToken) -> Result<CallToolResult, TaskExit> {
        let Running { mut call, questions, pending, progress, execution: _, destructive, released, executions, minted } = running;
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
            let completed = tokio::select! {
                biased;
                completed = &mut call => completed,
                Some(()) | None = halted.run_until_cancelled(task.cancelled()) => {
                    drop(released);
                    if destructive { call.await } else { return Err(TaskExit::Cancelled) }
                }
            };
            match completed? {
                CallToolResponse::Complete(result) => appended(&executions, minted, result).map_err(TaskExit::Error),
                _ => Err(TaskExit::Error(internal(ServiceError::UnexpectedResponse))),
            }
        };
        match future::select(pin!(settled), pin!(side)).await {
            Either::Left((settled, _)) => settled,
            Either::Right(((), settled)) => settled.await,
        }
    }
    async fn recorded(executions: &Executions, execution: Execution, call: BoxFuture<'static, Result<CallToolResponse, ErrorData>>) {
        let key = executions.with_mut(|rows| {
            let key = rows.last_key_value().map_or_default(|(last, _)| last.saturating_add(1));
            rows.insert(key, execution);
            key
        });
        let outcome = match call.await {
            Ok(response) => Outcome::Returned { result: Box::new(response.into()) },
            Err(error) => Outcome::Failed { error },
        };
        executions.with_mut(|rows| {
            rows.entry(key).and_modify(|held| held.outcome = outcome);
        });
    }
    let tools = server.routers.tools.load_full().ok_or_else(ErrorData::method_not_found::<CallToolRequestMethod>)?;
    let addressed = request.arguments.take().map(|arguments| serde_path_to_error::deserialize::<_, Addressed>(Value::Object(arguments)));
    let given = match addressed {
        Some(Ok(Addressed { handle, arguments })) => {
            request.arguments = Some(arguments);
            Ok(handle)
        }
        Some(Err(source)) => Err(McpError::HandleArgument { source }),
        None => Ok(None),
    };
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
        let connection = context.extensions.get::<Handle>().copied().filter(|_| context.protocol_version().is_none_or(|version| version.has_initialize()));
        let resolved = given.and_then(|given| match (given, connection) {
            (Some(handle), _) => server.records.get(&handle).map(|record| (handle, record, None)).ok_or(McpError::Handle { handle }),
            (None, connection) => {
                let handle = connection.unwrap_or_else(|| Handle(Uuid::new_v4()));
                Ok((handle, server.records.get_with(handle, Record::default), connection.is_none().then_some(handle)))
            }
        });
        let (handle, Record { state, executions }, minted) = match resolved {
            Ok(resolved) => resolved,
            Err(refused) => return refused.into_call_tool_result(),
        };
        let destructive = tools.get(&request.name).is_some_and(|tool| tool.annotations.as_ref().is_none_or(|hints| hints.read_only_hint != Some(true) && hints.is_destructive()));
        let execution = Execution { tool: request.name.clone(), arguments: request.arguments.clone(), outcome: Outcome::Running };
        let (progress, (asked, questions), mut called) = (Progress::default(), mpsc::unbounded_channel(), context.clone());
        called.ct = CancellationToken::new();
        called.extensions.insert(progress.clone());
        called.extensions.insert(handle);
        called.extensions.insert(state);
        called.extensions.insert(Elicitation { context: called.clone(), asked });
        let (app, released) = (server.app.clone(), called.ct.clone().drop_guard());
        let call = async move { tools.call(ToolCallContext::new(&app, request, called)).await };
        Running { call: call.boxed(), questions, pending: BTreeMap::new(), progress, execution, destructive, released, executions, minted }
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
            completed = running.progress.relay(&context, &mut running.call) => match completed? {
                CallToolResponse::Complete(result) => appended(&running.executions, running.minted, result).map(CallToolResponse::from),
                response => Ok(response),
            },
            () = context.ct.cancelled() => {
                drop(running.released);
                if running.destructive {
                    recorded(&running.executions, running.execution, running.call).await;
                }
                Err(internal(ServiceError::Cancelled { reason: None }))
            }
        }
    } else {
        Ok(pause(running))
    }
}
