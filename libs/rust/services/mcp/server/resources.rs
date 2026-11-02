use core::{future::ready, iter};

use derive_where::derive_where;
use futures::{FutureExt as _, Stream, StreamExt as _, TryFuture, TryFutureExt as _, future::BoxFuture, stream::BoxStream};
use rmcp::{
    ErrorData, Json,
    model::{Resource, ResourceContents, ResourceTemplate},
    transport::common::http_header::JSON_MIME_TYPE,
};
use serde::{Serialize, de::DeserializeOwned};
use serde_urlencoded::Deserializer;
use tokio_stream::wrappers::WatchStream;
use validated::Validated;

use super::Changes;
use crate::{McpError, internal};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait IntoResourceContents {
    fn into_resource_contents(self, uri: String, mime: Option<String>) -> Result<Vec<ResourceContents>, ErrorData>;
}

impl IntoResourceContents for Vec<ResourceContents> {
    fn into_resource_contents(self, _: String, _: Option<String>) -> Result<Vec<ResourceContents>, ErrorData> { Ok(self) }
}

impl IntoResourceContents for String {
    fn into_resource_contents(self, uri: String, mime: Option<String>) -> Result<Vec<ResourceContents>, ErrorData> {
        Ok(vec![mime.into_iter().fold(ResourceContents::text(self, uri), ResourceContents::with_mime_type)])
    }
}

impl<T> IntoResourceContents for Json<T>
where T: Serialize
{
    fn into_resource_contents(self, uri: String, mime: Option<String>) -> Result<Vec<ResourceContents>, ErrorData> {
        serde_json::to_string(&self.0).map_err(internal)?.into_resource_contents(uri, mime.or_else(|| Some(JSON_MIME_TYPE.to_owned())))
    }
}

impl<T, E> IntoResourceContents for Result<T, E>
where
    T: IntoResourceContents,
    ErrorData: From<E>,
{
    fn into_resource_contents(self, uri: String, mime: Option<String>) -> Result<Vec<ResourceContents>, ErrorData> { self?.into_resource_contents(uri, mime) }
}

pub(super) trait Routed<A, T> = Fn(A, String, String) -> Result<T, McpError> + Send + Sync;

pub(super) trait List<A> = Fn(A) -> BoxFuture<'static, Result<Vec<Resource>, ErrorData>> + Send + Sync;

// --- [MODELS] --------------------------------------------------------------------------

pub(super) struct Route<A> {
    pub(super) read: Box<dyn Routed<A, BoxFuture<'static, Result<Vec<ResourceContents>, ErrorData>>>>,
    pub(super) watch: Box<dyn Routed<A, BoxStream<'static, ()>>>,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive_where(Debug, Default)]
pub struct ResourceRouter<A> {
    #[derive_where(skip(Debug))]
    resources: Vec<(String, Route<A>)>,
    #[derive_where(skip(Debug))]
    templates: Vec<(ResourceTemplate, Route<A>)>,
    #[derive_where(skip(Debug))]
    lists: Vec<Box<dyn List<A>>>,
}

impl<A> ResourceRouter<A>
where A: 'static
{
    #[must_use]
    pub fn listed<Q>(mut self, list: impl Fn(A) -> Q + Send + Sync + 'static) -> Self
    where Q: TryFuture<Ok = Vec<Resource>, Error: Into<ErrorData>> + Send + 'static {
        self.lists.push(Box::new(move |app| list(app).map_err(Into::into).boxed()));
        self
    }

    #[must_use]
    pub fn resource<R, S>(mut self, resource: Resource, read: impl Fn(A) -> R + Send + Sync + 'static, watch: impl Fn(A) -> S + Send + Sync + 'static) -> Self
    where
        R: Future<Output: IntoResourceContents> + Send + 'static,
        S: Stream<Item = ()> + Send + 'static,
    {
        self.resources.push((resource.uri.clone(), route(resource.mime_type.clone(), |_, _| Ok(()), move |app, ()| read(app), move |app, ()| watch(app))));
        self.listed(move |_| ready(Ok::<_, ErrorData>(vec![resource.clone()])))
    }

    #[must_use]
    pub fn template<P, R, S>(mut self, template: ResourceTemplate, read: impl Fn(A, P) -> R + Send + Sync + 'static, watch: impl Fn(A, P) -> S + Send + Sync + 'static) -> Self
    where
        P: DeserializeOwned + 'static,
        R: Future<Output: IntoResourceContents> + Send + 'static,
        S: Stream<Item = ()> + Send + 'static,
    {
        let decoded = |uri: &str, variables: &str| {
            serde_path_to_error::deserialize(Deserializer::new(form_urlencoded::parse(variables.as_bytes()))).map_err(|source| McpError::Variables { uri: uri.to_owned(), source })
        };
        let routed = route(template.mime_type.clone(), decoded, read, watch);
        self.templates.push((template, routed));
        self
    }

    #[must_use]
    pub fn executions(self, resource: Resource, changes: &Changes<A>) -> Self {
        let (ledger, watched) = (changes.ledger.clone(), changes.ledger.clone());
        self.resource(resource.with_mime_type(JSON_MIME_TYPE), move |_| ready(Json(ledger.borrow().clone())), move |_| WatchStream::from_changes(watched.subscribe()).map(drop))
    }
}

pub(super) struct Resources<A> {
    uris: matchit::Router<Route<A>>,
    patterns: matchit::Router<Route<A>>,
    pub(super) lists: Vec<Box<dyn List<A>>>,
    pub(super) templates: Vec<ResourceTemplate>,
}

impl<A> TryFrom<ResourceRouter<A>> for Resources<A> {
    type Error = McpError;

    fn try_from(ResourceRouter { resources, templates, lists }: ResourceRouter<A>) -> Result<Self, McpError> {
        let (mut uris, mut patterns) = (matchit::Router::new(), matchit::Router::new());
        let inserted = |router: &mut matchit::Router<Route<A>>, pattern: String, route| router.insert(&pattern, route).map_err(|source| McpError::Route { pattern, source });
        let (templates, routes) = templates
            .into_iter()
            .map(|(template, route)| {
                let pattern = template.uri_template.split_once('?').map_or(template.uri_template.as_str(), |(path, _)| path.trim_suffix('{')).replace("{+", "{*");
                (template, (pattern, route))
            })
            .unzip::<_, _, Vec<_>, Vec<_>>();
        resources
            .into_iter()
            .map(|(uri, route)| inserted(&mut uris, uri.chars().flat_map(|character| iter::once(character).chain(matches!(character, '{' | '}').then_some(character))).collect(), route))
            .chain(routes.into_iter().map(|(pattern, route)| inserted(&mut patterns, pattern, route)))
            .collect::<Validated<(), _>>()
            .ok()
            .map(|()| Self { uris, patterns, lists, templates })
            .map_err(|errors| McpError::Declarations { errors })
    }
}

impl<A> Resources<A> {
    pub(super) fn routed(&self, uri: &str) -> Result<(&Route<A>, String), McpError> {
        match self.uris.at(uri) {
            Ok(matchit::Match { value, params: _ }) => Ok((value, String::new())),
            Err(matchit::MatchError::NotFound) => {
                let (path, query) = uri.split_once('?').unwrap_or((uri, ""));
                let matchit::Match { value, params } = self.patterns.at(path).map_err(|matchit::MatchError::NotFound| McpError::Unrouted { uri: uri.to_owned() })?;
                params
                    .iter()
                    .map(|(name, variable)| percent_encoding::percent_decode_str(variable).decode_utf8().map(|decoded| (name, decoded)))
                    .try_collect::<Vec<_>>()
                    .map(|variables| (value, form_urlencoded::Serializer::new(String::new()).extend_pairs(variables).extend_pairs(form_urlencoded::parse(query.as_bytes())).finish()))
                    .map_err(|source| McpError::Unencoded { uri: uri.to_owned(), source })
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn route<A, P, R, S>(
    mime: Option<String>,
    decode: impl Fn(&str, &str) -> Result<P, McpError> + Copy + Send + Sync + 'static,
    read: impl Fn(A, P) -> R + Send + Sync + 'static,
    watch: impl Fn(A, P) -> S + Send + Sync + 'static,
) -> Route<A>
where
    R: Future<Output: IntoResourceContents> + Send + 'static,
    S: Stream<Item = ()> + Send + 'static,
{
    Route {
        read: Box::new(move |app, uri, variables| {
            let mime = mime.clone();
            decode(&uri, &variables).map(|variables| read(app, variables).map(move |contents| contents.into_resource_contents(uri, mime)).boxed())
        }),
        watch: Box::new(move |app, uri, variables| decode(&uri, &variables).map(|variables| watch(app, variables).boxed())),
    }
}
