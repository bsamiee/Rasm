use std::collections::BTreeMap;

use derive_where::derive_where;
use futures::{FutureExt as _, TryFuture, TryFutureExt as _, future::BoxFuture};
use rmcp::{
    ErrorData,
    model::{ArgumentInfo, CompletionContext},
};

// --- [TYPES] ---------------------------------------------------------------------------

pub(super) trait Completer<A> = Fn(A, ArgumentInfo, Option<CompletionContext>) -> BoxFuture<'static, Result<Vec<String>, ErrorData>> + Send + Sync;

// --- [SERVICES] ------------------------------------------------------------------------

#[derive_where(Debug, Default)]
pub struct CompletionRouter<A> {
    #[derive_where(skip(Debug))]
    pub(super) prompts: BTreeMap<String, Box<dyn Completer<A>>>,
    #[derive_where(skip(Debug))]
    pub(super) resources: BTreeMap<String, Box<dyn Completer<A>>>,
}

impl<A> CompletionRouter<A>
where A: 'static
{
    #[must_use]
    pub fn prompt<Q>(mut self, name: impl Into<String>, complete: impl Fn(A, ArgumentInfo, Option<CompletionContext>) -> Q + Send + Sync + 'static) -> Self
    where Q: TryFuture<Ok = Vec<String>, Error: Into<ErrorData>> + Send + 'static {
        self.prompts.insert(name.into(), Box::new(move |app, argument, context| complete(app, argument, context).map_err(Into::into).boxed()));
        self
    }

    #[must_use]
    pub fn resource<Q>(mut self, uri_template: impl Into<String>, complete: impl Fn(A, ArgumentInfo, Option<CompletionContext>) -> Q + Send + Sync + 'static) -> Self
    where Q: TryFuture<Ok = Vec<String>, Error: Into<ErrorData>> + Send + 'static {
        self.resources.insert(uri_template.into(), Box::new(move |app, argument, context| complete(app, argument, context).map_err(Into::into).boxed()));
        self
    }
}
