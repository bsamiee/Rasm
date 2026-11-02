use core::{error::Error, future::ready, pin::pin};
use std::{borrow::Cow, collections::BTreeMap, error::Report, sync::Arc};

use futures::{
    FutureExt as _, StreamExt as _,
    future::{self, BoxFuture, Either},
    stream,
};
use jsonschema::validator_for;
use nonempty_collections::NEVec;
use rmcp::{
    ErrorData, RoleServer, ServiceError,
    handler::server::{
        tool::{IntoCallToolResult, ToolCallContext, ToolRoute},
        tool_name_validation::validate_and_warn_tool_name,
    },
    model::{CallToolRequestMethod, CallToolRequestParams, CallToolResponse, CallToolResult, CreateTaskResult, InputRequiredResult, NumberOrString, ProgressToken},
    service::RequestContext,
    task_manager::{TaskContext, TaskExit, TaskOptions},
};
use serde::Serialize;
use serde_json::Value;
use tokio::{
    sync::{mpsc, oneshot},
    time::{self, Instant},
};
use tokio_stream::wrappers::{UnboundedReceiverStream, WatchStream};
use tokio_util::sync::CancellationToken;
use uuid::Uuid;
use validated::Validated;

use super::{
    App, Server,
    elicitation::{Asked, Elicitation},
    executions::{self, Execution},
};
use crate::{McpError, Progress, internal};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, derive_more::From)]
pub struct ToolError<E>(pub E);

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

impl<E> From<ToolError<E>> for ErrorData
where E: Serialize + Error
{
    fn from(value: ToolError<E>) -> Self { serde_json::to_value(&value.0).map_or_else(internal, |data| Self::internal_error(Report::new(&value.0).to_string(), Some(data))) }
}

pub(super) struct Running {
    call: BoxFuture<'static, Result<CallToolResponse, ErrorData>>,
    questions: mpsc::UnboundedReceiver<(String, Asked)>,
    pending: BTreeMap<String, Asked>,
    progress: Progress,
    execution: Execution,
    destructive: bool,
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
    let tools = server.registry.tools.load_full().ok_or_else(ErrorData::method_not_found::<CallToolRequestMethod>)?;
    let resumed = server.parked.with_mut(|waiting| {
        let retried = |_: &String, held: &mut Running| (&held.execution.tool, &held.execution.arguments) == (&request.name, &request.arguments);
        request.request_state.as_ref().map(|state| waiting.extract_if(state..=state, retried).next().ok_or(McpError::RequestState)).transpose()
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
    let park = |parked: Running| {
        let state = Uuid::new_v4().to_string();
        let required = InputRequiredResult::new(Some(parked.pending.iter().map(|(key, (asked, _))| (key.clone(), asked.clone())).collect()), Some(state.clone()));
        let (calls, expiring) = (Arc::clone(&server.parked), state.clone());
        server.parked.with_mut(|waiting| waiting.insert(state, parked));
        server.tracker.spawn(server.halted.clone().run_until_cancelled_owned(time::sleep(server.retention)).map(move |expired| {
            if expired.is_some() {
                drop(calls.with_mut(|held| held.remove(&expiring)));
            }
        }));
        CallToolResponse::from(required)
    };
    if context.client_capabilities().is_some_and(|capabilities| capabilities.supports_tasks()) && server.info.capabilities.supports_tasks() {
        let options = TaskOptions::default().with_ttl_ms((!running.destructive).then(|| u64::try_from(server.retention.as_millis()).unwrap_or(u64::MAX)));
        Ok(CreateTaskResult::new(server.tasks.spawn(options, |task| server.tracker.track_future(tasked(running, task)).boxed())).into())
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
                if running.destructive {
                    executions::record(&server.registry.ledger, server.retention, running.execution, running.call).await;
                }
                Err(internal(ServiceError::Cancelled { reason: None }))
            }
        }
    } else {
        Ok(park(running))
    }
}
