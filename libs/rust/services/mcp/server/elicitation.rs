use rmcp::{
    RoleServer,
    model::{ElicitRequest, ElicitRequestParams, ElicitationAction, ElicitationCapability, ElicitationSchema, InputRequest},
    service::{ElicitationSafe, RequestContext},
};
use serde::{Serialize, de::DeserializeOwned};
use serde_json::Value;
use tokio::sync::{mpsc, oneshot};
use url::Url;
use uuid::Uuid;

use crate::McpError;

// --- [TYPES] ---------------------------------------------------------------------------

pub(super) type Asked = (InputRequest, oneshot::Sender<Value>);

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, derive_more::Display)]
#[serde(rename_all = "lowercase")]
pub enum Unanswered {
    #[display("user declined to answer, continue without answer")]
    Declined,
    #[display("user cancelled question, call again to ask again")]
    Cancelled,
    #[display("user answer is empty, call again to ask again")]
    Empty,
    #[display("client cannot ask user, ask in conversation and pass answer as arguments")]
    Unsupported,
    #[display("call ended before user answered, call again")]
    Abandoned,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug)]
pub struct Elicitation {
    pub(super) context: RequestContext<RoleServer>,
    pub(super) asked: mpsc::UnboundedSender<(String, Asked)>,
}

impl Elicitation {
    pub async fn elicit<T>(&self, message: impl Into<String>) -> Result<T, McpError>
    where T: ElicitationSafe + DeserializeOwned {
        let requested_schema = ElicitationSchema::from_type::<T>().map_err(|source| McpError::Form { source })?;
        let content = self.ask(|modes| modes.form.is_some() || modes.url.is_none(), ElicitRequestParams::FormElicitationParams { meta: None, message: message.into(), requested_schema }).await?;
        let content = content.ok_or(McpError::Elicitation { reason: Unanswered::Empty })?;
        serde_path_to_error::deserialize(&content).map_err(|source| McpError::Answer { data: content, source })
    }

    pub async fn elicit_url(&self, message: impl Into<String>, url: Url) -> Result<(), McpError> {
        let params = ElicitRequestParams::UrlElicitationParams { meta: None, message: message.into(), url: url.into(), elicitation_id: Uuid::new_v4().to_string() };
        self.ask(|modes| modes.url.is_some(), params).await.map(drop)
    }

    async fn ask(&self, supported: impl FnOnce(&ElicitationCapability) -> bool, params: ElicitRequestParams) -> Result<Option<Value>, McpError> {
        let abandoned = || McpError::Elicitation { reason: Unanswered::Abandoned };
        self.context.client_capabilities().and_then(|capabilities| capabilities.elicitation).is_some_and(|modes| supported(&modes)).ok_or(McpError::Elicitation { reason: Unanswered::Unsupported })?;
        let result = if self.context.protocol_version().is_none_or(|version| version.has_initialize()) {
            self.context.peer.create_elicitation(params).await.map_err(|source| McpError::Service { source })?
        } else {
            let (responder, answered) = oneshot::channel();
            self.asked.send((Uuid::new_v4().to_string(), (InputRequest::Elicitation(ElicitRequest::new(params)), responder))).map_err(|mpsc::error::SendError(_)| abandoned())?;
            let answer = answered.await.map_err(|oneshot::error::RecvError { .. }| abandoned())?;
            serde_path_to_error::deserialize(&answer).map_err(|source| McpError::Answer { data: answer, source })?
        };
        match result.action {
            ElicitationAction::Accept => Ok(result.content),
            ElicitationAction::Decline => Err(McpError::Elicitation { reason: Unanswered::Declined }),
            _ => Err(McpError::Elicitation { reason: Unanswered::Cancelled }),
        }
    }
}
