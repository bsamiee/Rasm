use core::{future::ready, iter, num::NonZero};

use bytes::Bytes;
use data_encoding::BASE64;
use derive_where::derive_where;
use futures::{FutureExt as _, Stream, StreamExt as _, TryFuture, TryFutureExt as _, future::BoxFuture, stream::BoxStream};
use itertools::Itertools as _;
use rmcp::{
    ErrorData, Json,
    model::{Resource, ResourceContents, ResourceTemplate},
    transport::common::http_header::JSON_MIME_TYPE,
};
use serde::{Serialize, de::DeserializeOwned};
use serde_urlencoded::Deserializer;
use tokio_stream::wrappers::WatchStream;
use validated::Validated;

use super::Registry;
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

impl IntoResourceContents for Bytes {
    fn into_resource_contents(self, uri: String, mime: Option<String>) -> Result<Vec<ResourceContents>, ErrorData> {
        Ok(vec![mime.into_iter().fold(ResourceContents::blob(BASE64.encode(&self), uri), ResourceContents::with_mime_type)])
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

pub(super) trait List<A> = Fn(A, Option<String>, NonZero<usize>) -> BoxFuture<'static, Result<Vec<Resource>, ErrorData>> + Send + Sync;

// --- [MODELS] --------------------------------------------------------------------------

pub(super) struct Route<A> {
    pub(super) read: Box<dyn Routed<A, BoxFuture<'static, Result<Vec<ResourceContents>, ErrorData>>>>,
    pub(super) watch: Box<dyn Routed<A, BoxStream<'static, ()>>>,
}

#[derive(Default)]
struct Query {
    variables: Vec<String>,
    literals: Vec<(String, String)>,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive_where(Debug, Default)]
pub struct ResourceRouter<A> {
    #[derive_where(skip(Debug))]
    resources: Vec<(Resource, Route<A>)>,
    #[derive_where(skip(Debug))]
    templates: Vec<(ResourceTemplate, Route<A>)>,
    #[derive_where(skip(Debug))]
    lists: Vec<Box<dyn List<A>>>,
}

impl<A> ResourceRouter<A>
where A: 'static
{
    #[must_use]
    pub fn list<Q>(mut self, list: impl Fn(A, Option<String>, NonZero<usize>) -> Q + Send + Sync + 'static) -> Self
    where Q: TryFuture<Ok = Vec<Resource>, Error: Into<ErrorData>> + Send + 'static {
        self.lists.push(Box::new(move |app, after, limit| list(app, after, limit).map_err(Into::into).boxed()));
        self
    }

    #[must_use]
    pub fn resource<R, S>(mut self, resource: Resource, read: impl Fn(A) -> R + Send + Sync + 'static, watch: impl Fn(A) -> S + Send + Sync + 'static) -> Self
    where
        R: Future<Output: IntoResourceContents> + Send + 'static,
        S: Stream<Item = ()> + Send + 'static,
    {
        let routed = route(resource.mime_type.clone(), |_, _| Ok(()), move |app, ()| read(app), move |app, ()| watch(app));
        self.resources.push((resource, routed));
        self
    }

    #[must_use]
    pub fn executions(self, resource: Resource, registry: &Registry<A>) -> Self {
        let (ledger, watched) = (registry.ledger.clone(), registry.ledger.clone());
        self.resource(
            resource.with_mime_type(JSON_MIME_TYPE),
            move |_| ready(serde_json::to_string(&*ledger.borrow()).map_err(internal)),
            move |_| WatchStream::from_changes(watched.subscribe()).map(drop),
        )
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
}

pub(super) struct Resources<A> {
    uris: matchit::Router<Route<A>>,
    patterns: matchit::Router<(Route<A>, Query)>,
    pub(super) lists: Vec<Box<dyn List<A>>>,
    pub(super) declared: Box<[Resource]>,
    pub(super) templates: Box<[ResourceTemplate]>,
}

impl<A> TryFrom<ResourceRouter<A>> for Resources<A> {
    type Error = McpError;

    fn try_from(ResourceRouter { resources, templates, lists }: ResourceRouter<A>) -> Result<Self, McpError> {
        fn inserted<V>(router: &mut matchit::Router<V>, pattern: String, value: V) -> Result<(), McpError> { router.insert(&pattern, value).map_err(|source| McpError::Route { pattern, source }) }
        let (mut uris, mut patterns) = (matchit::Router::new(), matchit::Router::new());
        let resources = resources
            .into_iter()
            .map(|(resource, route)| {
                inserted(&mut uris, resource.uri.chars().flat_map(|character| iter::once(character).chain(matches!(character, '{' | '}').then_some(character))).collect(), route).map(|()| resource)
            })
            .collect::<Validated<Vec<_>, _>>();
        let templates = templates
            .into_iter()
            .map(|(template, route)| translated(&template.uri_template).and_then(|(pattern, query)| inserted(&mut patterns, pattern, (route, query))).map(|()| template))
            .collect::<Validated<Vec<_>, _>>();
        resources
            .map2(templates, |resources, templates| Self {
                uris,
                patterns,
                lists,
                declared: resources.into_iter().sorted_unstable_by(|left, right| left.uri.cmp(&right.uri)).collect(),
                templates: templates.into_iter().sorted_unstable_by(|left, right| left.uri_template.cmp(&right.uri_template)).collect(),
            })
            .ok()
            .map_err(|errors| McpError::Declarations { errors })
    }
}

impl<A> Resources<A> {
    pub(super) fn routed(&self, uri: &str) -> Result<(&Route<A>, String), McpError> {
        match self.uris.at(uri) {
            Ok(matchit::Match { value, params: _ }) => Ok((value, String::new())),
            Err(matchit::MatchError::NotFound) => {
                let not_found = || McpError::ResourceNotFound { uri: uri.to_owned() };
                let (path, query) = uri.split_once('?').unwrap_or((uri, ""));
                let matchit::Match { value: (route, Query { variables, literals }), params } = self.patterns.at(path).map_err(|matchit::MatchError::NotFound| not_found())?;
                let pairs = form_urlencoded::parse(query.as_bytes()).collect::<Vec<_>>();
                literals.iter().all(|(name, value)| pairs.iter().any(|(held, given)| (&**held, &**given) == (name.as_str(), value.as_str()))).ok_or_else(not_found)?;
                params
                    .iter()
                    .map(|(name, variable)| percent_encoding::percent_decode_str(variable).decode_utf8().map(|decoded| (name, decoded)))
                    .collect::<Result<Vec<_>, _>>()
                    .map(|decoded| {
                        let queried = pairs.iter().filter(|(name, _)| variables.iter().any(|variable| variable == name));
                        (route, form_urlencoded::Serializer::new(String::new()).extend_pairs(decoded).extend_pairs(queried).finish())
                    })
                    .map_err(|source| McpError::Encoding { uri: uri.to_owned(), source })
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

fn translated(template: &str) -> Result<(String, Query), McpError> {
    let refused = |expression: &str| McpError::Template { template: template.to_owned(), expression: expression.to_owned() };
    let mut pieces = template.split('{');
    let head = pieces.next().map(|literal| Ok((None, literal)));
    let mut tokens = iter::chain(head, pieces.map(|piece| piece.split_once('}').map(|(expression, literal)| (Some(expression), literal)).ok_or_else(|| refused(piece))));
    let (pattern, query, _) = tokens.try_fold((String::new(), None::<Query>, None::<&str>), |(mut pattern, mut query, closed), token| {
        let (braced, literal) = token?;
        let operated =
            braced.map(|expression| (expression, expression.split_at_checked(1).filter(|(operator, _)| operator.starts_with(['+', '#', '.', '/', ';', '?', '&'])).unwrap_or(("", expression))));
        let closed = match operated {
            None => Ok(closed),
            Some((expression, (operator, names))) => {
                let (single, listed) = (!names.is_empty() && !names.contains([',', '*', ':']), !names.is_empty() && !names.contains(['*', ':']));
                match (operator, &mut query, closed) {
                    ("" | "/" | ".", None, None) if single => {
                        pattern.extend([operator, "{", names, "}"]);
                        Ok(None)
                    }
                    ("+", None, None) if single => {
                        pattern.extend(["{*", names, "}"]);
                        Ok(Some(expression))
                    }
                    ("?", None, _) if listed => {
                        query = Some(Query { variables: names.split(',').map(str::to_owned).collect(), literals: Vec::new() });
                        Ok(closed)
                    }
                    ("&", Some(Query { variables, literals: _ }), _) if listed => {
                        variables.extend(names.split(',').map(str::to_owned));
                        Ok(closed)
                    }
                    (_, _, _) => Err(refused(expression)),
                }
            }
        }?;
        match (&mut query, closed, literal.split_once('?')) {
            (Some(Query { variables: _, literals }), _, _) => literals.extend(form_urlencoded::parse(literal.as_bytes()).into_owned()),
            (None, _, _) if literal.is_empty() => {}
            (None, Some(open), _) => return Err(refused(open)),
            (None, None, Some((path, pairs))) => {
                pattern.push_str(&path.replace('}', "}}"));
                query = Some(Query { variables: Vec::new(), literals: form_urlencoded::parse(pairs.as_bytes()).into_owned().collect() });
            }
            (None, None, None) => pattern.push_str(&literal.replace('}', "}}")),
        }
        Ok((pattern, query, closed))
    })?;
    Ok((pattern, query.unwrap_or_default()))
}
