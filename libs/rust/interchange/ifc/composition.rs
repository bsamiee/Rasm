use core::iter;
use std::sync::Arc;

use hashbrown::HashMap;
use indexmap::{IndexMap, IndexSet};
use itertools::{Either, Itertools as _};
use nonempty_collections::{NEVec, NonEmptyIterator as _};
use petgraph::{Direction, algo::kosaraju_scc, graphmap::DiGraphMap};
use validated::Validated;

use crate::{Composed, Header, IfcError, Layer, Model, Name, Node, Path, descend, distinct, schemas};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Stated<'layer> = Option<Either<&'layer Path, &'layer serde_json::Value>>;
pub type Opinions<'layer> = IndexMap<(&'layer Path, Subject<&'layer Name, &'layer str>), NEVec<(&'layer Header, Stated<'layer>)>>;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Subject<N = Name, A = Box<str>> {
    #[display("child {_0}")]
    Child(N),
    #[display("inherit {_0}")]
    Inherit(N),
    #[display("attribute {_0}")]
    Attribute(A),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FEDERATION]

#[must_use]
pub fn federate(layers: NEVec<Layer>) -> Layer {
    let header = layers.first().header.clone();
    let mut schemas = IndexMap::new();
    let data = flatten(layers.into_iter().rev().flat_map(|Layer { header: _, imports: _, schemas: definitions, data }| {
        schemas.extend(definitions);
        data
    }));
    Layer { header, imports: Vec::new(), schemas, data: data.into_values().collect() }
}

#[must_use]
pub fn opinions(layers: &NEVec<Layer>) -> Opinions<'_> {
    layers
        .iter()
        .flat_map(|layer| {
            layer.data.iter().rev().flat_map(move |node| {
                let children = node.children.iter().map(|(name, target)| (Subject::Child(name), target.as_ref().map(Either::Left)));
                let inherits = node.inherits.iter().map(|(name, target)| (Subject::Inherit(name), target.as_ref().map(Either::Left)));
                let attributes = node.attributes.iter().map(|(id, value)| (Subject::Attribute(&**id), value.as_ref().map(Either::Right)));
                children.chain(inherits).chain(attributes).map(move |(subject, stated)| ((&node.path, subject), (&layer.header, stated)))
            })
        })
        .fold(IndexMap::new(), |mut held: Opinions<'_>, (key, opinion)| {
            held.entry(key).and_modify(|stated| stated.push(opinion)).or_insert_with(|| NEVec::new(opinion));
            held
        })
}

pub fn conflicts(layers: &NEVec<Layer>) -> impl Iterator<Item = <Opinions<'_> as IntoIterator>::Item> {
    opinions(layers).into_iter().filter(|(_, stated)| stated.iter().any(|(_, opinion)| *opinion != stated.first().1))
}

#[must_use]
pub(crate) fn flatten<V, I>(nodes: I) -> IndexMap<Path, Node<V>>
where I: IntoIterator<Item = Node<V>> {
    nodes.into_iter().fold(IndexMap::new(), |mut held, Node { path, children, inherits, attributes }| {
        let opinions = held.entry(path).or_insert_with_key(|path| Node::new(path.clone()));
        opinions.children.extend(children);
        inherits.into_iter().for_each(|(name, target)| {
            let present = target.is_some();
            let (index, previous) = opinions.inherits.insert_full(name, target);
            if !present || previous.flatten().is_none() {
                opinions.inherits.move_index(index, opinions.inherits.len().strict_sub(1));
            }
        });
        opinions.attributes.extend(attributes);
        held
    })
}

pub(crate) fn transcoded<V, U>(nodes: impl IntoIterator<Item = (Path, Node<V>)>, value: impl Fn(&Path, &str, V) -> Result<U, IfcError>) -> (IndexMap<Path, Node<U>>, Vec<IfcError>) {
    nodes.into_iter().fold((IndexMap::new(), Vec::new()), |(mut held, mut refused), (key, Node { path, children, inherits, attributes })| {
        let (attributes, failures): (IndexMap<_, _>, Vec<_>) =
            attributes.into_iter().map(|(id, opinion)| opinion.map(|stated| value(&path, &id, stated)).transpose().map(|opinion| (id, opinion))).partition_result();
        refused.extend(failures);
        held.insert(key, Node { path, children, inherits, attributes });
        (held, refused)
    })
}

// --- [COMPOSITION]

pub fn compose(layers: NEVec<Layer>) -> Result<Model, NEVec<IfcError>> {
    let (model, failures) = compose_partial(layers);
    NEVec::try_from_vec(failures).map_or(Ok(model), Err)
}

#[must_use]
pub fn compose_partial(layers: NEVec<Layer>) -> (Model, Vec<IfcError>) {
    let Layer { header: _, imports: _, schemas, data } = federate(layers);
    let (heads, roots, nodes, failures) = {
        let decode = schemas::decoder(schemas.iter().map(|(id, schema)| (&**id, schema)));
        let (opinions, refused) = transcoded(data.into_iter().map(|node| (node.path.clone(), node)), |path, id, value: serde_json::Value| decode(path, id, &value).map(Arc::new));
        let stated = opinions
            .keys()
            .flat_map(|path| iter::successors(path.0.iter().as_slice().split_last(), |&(_, parent)| parent.split_last()).filter(|(_, parent)| !parent.is_empty()))
            .map(|(name, parent)| (parent, name))
            .into_grouping_map()
            .collect::<IndexSet<_>>();
        let occurrence = |path: Path, mut root: Arc<Composed>, heads: &IndexMap<Name, Arc<Composed>>| {
            let resolved = |target: &Path| match (descend(heads, target), target.0.split_first()) {
                (Some(node), _) => Ok(Arc::clone(node)),
                (None, (_, [])) => Ok(Arc::new(Composed { path: target.clone(), attributes: IndexMap::new(), children: IndexMap::new() })),
                (None, _) => Err(IfcError::Missing { path: target.clone() }),
            };
            let mut path = path.0;
            let mut pending = Vec::from([Some(Ok((None, &mut root)))]);
            let mut failures = Vec::new();
            while let Some(next) = pending.pop() {
                let Some(next) = next else {
                    drop(path.pop());
                    continue;
                };
                let branch = match next {
                    Ok((Some(name), branch)) => {
                        path.push(Name::clone(name));
                        pending.push(None);
                        branch
                    }
                    Ok((None, branch)) => branch,
                    Err(failure) => {
                        failures.push(failure);
                        continue;
                    }
                };
                let opinion = opinions.get(path.iter().as_slice());
                let below = stated.get(path.iter().as_slice());
                let sources = opinion.into_iter().flat_map(|opinion| opinion.inherits.values().flatten()).map(resolved).collect::<Validated<Vec<_>, _>>();
                let parents = opinion
                    .into_iter()
                    .flat_map(|opinion| &opinion.children)
                    .filter_map(|(name, target)| target.as_ref().map(|target| resolved(target).map(|child| (name.clone(), child))))
                    .collect::<Validated<Vec<_>, _>>();
                match sources
                    .map2(parents, |sources, parents| {
                        let node = Arc::make_mut(branch);
                        let kept_attribute = |id: &str| opinion.is_none_or(|opinion| opinion.attributes.get(id).is_none_or(Option::is_some));
                        let kept_child = |name: &Name| opinion.is_none_or(|opinion| opinion.children.get(name).is_none_or(Option::is_some));
                        node.attributes.retain(|id, _| kept_attribute(id));
                        node.attributes.extend(
                            sources
                                .iter()
                                .flat_map(|source| source.attributes.iter())
                                .filter(|(id, _)| kept_attribute(id))
                                .chain(opinion.into_iter().flat_map(|opinion| &opinion.attributes).filter_map(|(id, value)| value.as_ref().map(|value| (id, value))))
                                .map(|(id, value)| (id.clone(), Arc::clone(value))),
                        );
                        node.children.retain(|name, _| kept_child(name));
                        node.children.extend(
                            sources.iter().flat_map(|source| source.children.iter()).filter(|(name, _)| kept_child(name)).map(|(name, child)| (name.clone(), Arc::clone(child))).chain(parents),
                        );
                        node
                    })
                    .ok()
                {
                    Ok(node) => {
                        pending.extend(
                            below
                                .into_iter()
                                .flatten()
                                .rev()
                                .filter(|name| !node.children.contains_key(**name) && opinion.is_none_or(|opinion| !opinion.children.contains_key(**name)))
                                .map(|name| Some(Err(IfcError::Missing { path: Path(path.nonempty_iter().chain(iter::once(*name)).cloned().collect()) }))),
                        );
                        pending.extend(node.children.iter_mut().rev().filter(|(name, _)| below.is_some_and(|names| names.contains(name))).map(|(name, child)| Some(Ok((Some(name), child)))));
                    }
                    Err(errors) => failures.extend(errors),
                }
            }
            NEVec::try_from_vec(failures).map_or(Ok(root), Err)
        };
        let mut graph = opinions.keys().map(Path::head).fold(DiGraphMap::<&Name, ()>::new(), |mut graph, head| {
            graph.add_node(head);
            graph
        });
        opinions.iter().flat_map(|(path, opinion)| opinion.inherits.values().chain(opinion.children.values()).flatten().map(|target| (path.head(), target.head()))).for_each(|(from, target)| {
            if graph.contains_node(target) {
                graph.add_edge(from, target, ());
            }
        });
        let components = kosaraju_scc(&graph);
        let acyclic = |members: &&Vec<_>| matches!(members, [head] if !graph.contains_edge(*head, *head));
        let cycles = components.iter().filter(|members| !acyclic(members)).flatten().map(|head| IfcError::Cycle { head: (*head).clone() });
        let (heads, failures) =
            components.iter().filter(acyclic).flatten().fold((IndexMap::<Name, Arc<Composed>>::new(), iter::chain(refused, cycles).collect::<Vec<_>>()), |(mut heads, mut failures), &head| {
                let path = Path(NEVec::new(head.clone()));
                let base = Arc::new(Composed { path: path.clone(), attributes: IndexMap::new(), children: IndexMap::new() });
                match graph.neighbors(head).all(|dependency| heads.contains_key(dependency)).then(|| occurrence(path, base, &heads)) {
                    Some(Ok(node)) => drop(heads.insert(head.clone(), node)),
                    Some(Err(errors)) => failures.extend(errors),
                    None => {}
                }
                (heads, failures)
            });
        let roots = graph.nodes().filter(|head| graph.neighbors_directed(*head, Direction::Incoming).next().is_none()).filter_map(|head| heads.get(head).map(Arc::clone)).collect::<Vec<_>>();
        let nodes = distinct(graph.nodes().filter_map(|head| heads.get(head))).map(Arc::clone).collect::<Vec<_>>();
        (heads, roots, nodes, failures)
    };
    let holders = nodes.iter().flat_map(|holder| holder.children.iter().map(move |(name, child)| (child.path.clone(), (name.clone(), Arc::clone(holder))))).into_group_map();
    let holding = nodes.iter().flat_map(|node| node.attributes.keys().map(move |id| (id, node))).fold(HashMap::<Box<str>, Vec<_>>::new(), |mut held, (id, node)| {
        held.entry_ref(&**id).or_default().push(Arc::clone(node));
        held
    });
    (Model { schemas, heads, roots, nodes, holders, holding }, failures)
}
