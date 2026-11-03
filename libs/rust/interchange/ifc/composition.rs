use core::iter;
use std::{collections::HashMap, sync::Arc};

use indexmap::{IndexMap, IndexSet};
use itertools::{Either, Itertools as _};
use nonempty_collections::NEVec;
use petgraph::{Direction, algo::tarjan_scc, graphmap::DiGraphMap};
use validated::Validated;

use crate::{Composed, Header, IfcError, Layer, Model, Name, Node, Path, Value, descend, distinct, schemas};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Stated<'layer> = Option<Either<&'layer Path, &'layer serde_json::Value>>;
pub type Opinions<'layer> = IndexMap<(&'layer Path, Subject<'layer>), NEVec<(&'layer Header, Stated<'layer>)>>;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Subject<'layer> {
    #[display("child {_0}")]
    Child(&'layer Name),
    #[display("inherit {_0}")]
    Inherit(&'layer Name),
    #[display("attribute {_0}")]
    Attribute(&'layer str),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FEDERATION]

#[must_use]
pub fn federate(layers: NEVec<Layer>) -> Layer {
    let header = layers.first().header.clone();
    let (schemas, data): (Vec<_>, Vec<_>) = layers.into_iter().rev().map(|Layer { header: _, imports: _, schemas, data }| (schemas, data)).unzip();
    Layer { header, imports: Vec::new(), schemas: schemas.into_iter().flatten().collect(), data: flatten(data.into_iter().flatten()).into_values().collect() }
}

#[must_use]
pub fn opinions(layers: &NEVec<Layer>) -> Opinions<'_> {
    layers
        .iter()
        .flat_map(|layer| {
            layer.data.iter().rev().flat_map(move |node| {
                let children = node.children.iter().map(|(name, target)| (Subject::Child(name), target.as_ref().map(Either::Left)));
                let inherits = node.inherits.iter().map(|(name, target)| (Subject::Inherit(name), target.as_ref().map(Either::Left)));
                let attributes = node.attributes.iter().map(|(id, value)| (Subject::Attribute(id), value.as_ref().map(Either::Right)));
                children.chain(inherits).chain(attributes).map(move |(subject, stated)| ((&node.path, subject), (&layer.header, stated)))
            })
        })
        .fold(IndexMap::new(), |mut held: Opinions<'_>, (key, opinion)| {
            held.entry(key).and_modify(|stated| stated.push(opinion)).or_insert_with(|| NEVec::new(opinion));
            held
        })
}

pub fn conflicts(layers: &NEVec<Layer>) -> impl Iterator<Item = ((&Path, Subject<'_>), NEVec<(&Header, Stated<'_>)>)> {
    opinions(layers).into_iter().filter(|(_, stated)| stated.iter().any(|(_, opinion)| *opinion != stated.first().1))
}

#[must_use]
pub(crate) fn flatten<V, I>(nodes: I) -> IndexMap<Path, Node<V>>
where I: IntoIterator<Item = Node<V>> {
    nodes.into_iter().fold(IndexMap::new(), |mut held, Node { path, children, inherits, attributes }| {
        let opinions = held.entry(path).or_insert_with_key(|path| Node::new(path.clone()));
        opinions.children.extend(children);
        inherits.into_iter().for_each(|(name, target)| {
            let kept = opinions.inherits.get_full(&name).and_then(|(index, _, stated)| (stated.is_some() && target.is_some()).then_some(index));
            opinions.inherits.insert_before(kept.unwrap_or(opinions.inherits.len()), name, target);
        });
        opinions.attributes.extend(attributes);
        held
    })
}

pub(crate) fn transcoded<V, U>(nodes: IndexMap<Path, Node<V>>, value: impl Fn(&Path, &str, V) -> Result<U, IfcError>) -> (IndexMap<Path, Node<U>>, Vec<IfcError>) {
    nodes.into_iter().fold((IndexMap::new(), Vec::new()), |(mut held, mut refused), (key, Node { path, children, inherits, attributes })| {
        let (attributes, failures): (IndexMap<_, _>, Vec<_>) = attributes.into_iter().map(|(id, opinion)| opinion.map(|stated| value(&path, &id, stated)).transpose().map(|opinion| (id, opinion))).partition_result();
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
    fn occurrence(path: &Path, base: Composed, opinions: &IndexMap<Path, Node<Arc<Value>>>, stated: &HashMap<&[Name], IndexSet<&Name>>, heads: &IndexMap<Name, Arc<Composed>>) -> Result<Composed, NEVec<IfcError>> {
        let absent = Node::new(path.clone());
        let opinion = opinions.get(path).unwrap_or(&absent);
        let below = stated.get(path.0.iter().as_slice());
        let resolved = |target: &Path| match (descend(heads, target), target.0.split_first()) {
            (Some(node), _) => Ok(Arc::clone(node)),
            (None, (_, [])) => Ok(Arc::new(Composed { path: target.clone(), attributes: IndexMap::new(), children: IndexMap::new() })),
            (None, _) => Err(IfcError::Missing { path: target.clone() }),
        };
        let sources = opinion.inherits.values().flatten().map(resolved).collect::<Validated<Vec<_>, _>>();
        let parents = opinion.children.iter().filter_map(|(name, target)| target.as_ref().map(|target| resolved(target).map(|child| (name.clone(), child)))).collect::<Validated<Vec<_>, _>>();
        let Composed { path: definition, attributes, children } = base;
        let (attributes, children) = sources
            .map2(parents, |sources, parents| {
                let inherited = sources.iter().flat_map(|source| source.children.iter().map(|(name, child)| (name.clone(), Arc::clone(child))));
                let copied = sources.iter().flat_map(|source| source.attributes.iter().map(|(id, value)| (id.clone(), Arc::clone(value))));
                let own = opinion.attributes.iter().filter_map(|(id, value)| value.as_ref().map(|value| (id.clone(), Arc::clone(value))));
                (
                    iter::chain(attributes, copied).chain(own).filter(|(id, _)| opinion.attributes.get(id).is_none_or(Option::is_some)).collect::<IndexMap<_, _>>(),
                    iter::chain(children, inherited).chain(parents).filter(|(name, _)| opinion.children.get(name).is_none_or(Option::is_some)).collect::<IndexMap<_, _>>(),
                )
            })
            .ok()?;
        let vanished = below
            .into_iter()
            .flatten()
            .filter(|name| !children.contains_key(**name) && !opinion.children.contains_key(**name))
            .map(|name| Err::<(), _>(IfcError::Missing { path: path.child((*name).clone()) }))
            .collect::<Validated<Vec<_>, _>>();
        children
            .into_iter()
            .map(|(name, child)| match below.filter(|names| names.contains(&name)) {
                Some(_) => occurrence(&path.child(name.clone()), Arc::unwrap_or_clone(child), opinions, stated, heads).map_or_else(Validated::Fail, |grown| Validated::Good((name, Arc::new(grown)))),
                None => Validated::Good((name, child)),
            })
            .collect::<Validated<IndexMap<_, _>, _>>()
            .map2(vanished, |children, _| Composed { path: definition, attributes, children })
            .ok()
    }
    let Layer { header: _, imports: _, schemas, data } = federate(layers);
    let (heads, roots, failures) = {
        let decode = schemas::decoder(schemas.iter().map(|(id, schema)| (&**id, schema)));
        let (opinions, refused) = transcoded(flatten(data), |path, id, value: serde_json::Value| decode(path, id, &value).map(Arc::new));
        let stated = opinions
            .keys()
            .flat_map(|path| iter::successors(path.0.iter().as_slice().split_last(), |&(_, parent)| parent.split_last()).filter(|(_, parent)| !parent.is_empty()))
            .map(|(name, parent)| (parent, name))
            .into_grouping_map()
            .collect::<IndexSet<_>>();
        let mut graph = opinions.keys().map(Path::head).fold(DiGraphMap::<&Name, ()>::new(), |mut graph, head| {
            graph.add_node(head);
            graph
        });
        let edges = opinions
            .iter()
            .flat_map(|(path, opinion)| opinion.inherits.values().chain(opinion.children.values()).flatten().map(|target| (path.head(), target.head())))
            .filter(|&(_, target)| graph.contains_node(target))
            .collect::<Vec<_>>();
        graph.extend(edges);
        let components = tarjan_scc(&graph);
        let acyclic = |members: &&Vec<_>| matches!(members.as_slice(), [head] if !graph.contains_edge(*head, *head));
        let cycles = components.iter().filter(|members| !acyclic(members)).flatten().map(|head| IfcError::Cycle { head: (*head).clone() });
        let (heads, failures) = components.iter().filter(acyclic).flatten().fold((IndexMap::<Name, Arc<Composed>>::new(), iter::chain(refused, cycles).collect::<Vec<_>>()), |(mut heads, mut failures), &head| {
            let path = Path(NEVec::new(head.clone()));
            let base = Composed { path: path.clone(), attributes: IndexMap::new(), children: IndexMap::new() };
            match graph.neighbors(head).all(|dependency| heads.contains_key(dependency)).then(|| occurrence(&path, base, &opinions, &stated, &heads)) {
                Some(Ok(node)) => drop(heads.insert(head.clone(), Arc::new(node))),
                Some(Err(errors)) => failures.extend(errors),
                None => {}
            }
            (heads, failures)
        });
        let roots = graph.nodes().filter(|head| graph.neighbors_directed(*head, Direction::Incoming).next().is_none()).filter_map(|head| heads.get(head).map(Arc::clone)).collect::<Vec<_>>();
        (heads, roots, failures)
    };
    let nodes = distinct(heads.values()).map(Arc::clone).collect::<Vec<_>>();
    let holders = nodes.iter().flat_map(|holder| holder.children.iter().map(move |(name, child)| (child.path.clone(), (name.clone(), Arc::clone(holder))))).into_group_map();
    let holding = nodes.iter().flat_map(|node| node.attributes.keys().map(move |id| (id, node))).fold(HashMap::<Box<str>, Vec<_>>::new(), |mut held, (id, node)| {
        match held.get_mut(&**id) {
            Some(holding) => holding.push(Arc::clone(node)),
            None => drop(held.insert(id.clone(), Vec::from([Arc::clone(node)]))),
        }
        held
    });
    (Model { schemas, heads, roots, nodes, holders, holding }, failures)
}
