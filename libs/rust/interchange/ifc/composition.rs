use core::iter;
use std::{
    collections::{HashMap, HashSet},
    sync::Arc,
};

use indexmap::IndexMap;
use nonempty_collections::NEVec;
use petgraph::{Direction, algo::tarjan_scc, graphmap::DiGraphMap};
use validated::Validated;

use crate::{Composed, IfcError, Layer, Model, Name, Node, Part, Path, Value, schemas};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn federate(layers: NEVec<Layer>) -> Layer {
    let header = layers.first().header.clone();
    let (schemas, data): (Vec<_>, Vec<_>) = layers.into_iter().rev().map(|Layer { header: _, imports: _, schemas, data }| (schemas, data)).unzip();
    Layer { header, imports: Vec::new(), schemas: schemas.into_iter().flatten().collect(), data: collapse(data.into_iter().flatten()).into_values().collect() }
}

#[must_use]
pub(crate) fn collapse<V, I>(nodes: I) -> IndexMap<Path, Node<V>>
where I: IntoIterator<Item = Node<V>> {
    nodes.into_iter().fold(IndexMap::new(), |mut held, Node { path, children, inherits, attributes }| {
        let opinions = held.entry(path.clone()).or_insert_with(|| Node { path, children: IndexMap::new(), inherits: IndexMap::new(), attributes: IndexMap::new() });
        opinions.children.extend(children);
        inherits.into_iter().for_each(|(name, target)| {
            let kept = opinions.inherits.get_full(&name).and_then(|(index, _, stated)| (stated.is_some() && target.is_some()).then_some(index));
            opinions.inherits.insert_before(kept.unwrap_or(opinions.inherits.len()), name, target);
        });
        opinions.attributes.extend(attributes);
        held
    })
}

pub fn compose(layer: Layer) -> Result<Model, NEVec<IfcError>> {
    fn occurrence(
        path: &Path,
        base: Composed,
        opinions: &IndexMap<Path, Node<Value>>,
        stated: &HashSet<(&Name, &[Name], &Name)>,
        composed: &HashMap<&Name, Arc<Composed>>,
    ) -> Result<Composed, NEVec<IfcError>> {
        let descend = |target: &Path| {
            let (head, names) = target.0.split_first();
            composed.get(head).and_then(|defined| names.iter().try_fold(defined, |node, name| node.children.get(name))).ok_or_else(|| IfcError::Missing { path: target.clone(), part: Part::Node })
        };
        let opinion = opinions.get(path);
        let sources = opinion.into_iter().flat_map(|opinion| opinion.inherits.values().flatten()).map(descend).collect::<Validated<Vec<_>, _>>();
        let parents = opinion
            .into_iter()
            .flat_map(|opinion| &opinion.children)
            .filter_map(|(name, target)| target.as_ref().map(|target| descend(target).map(|child| (name, child))))
            .collect::<Validated<Vec<_>, _>>();
        let Composed { node, mut attributes, mut children } = base;
        let (head, names) = path.0.split_first();
        sources
            .map2(parents, |sources, parents| {
                children.extend(sources.iter().flat_map(|source| &source.children).chain(parents).map(|(name, child)| (name.clone(), Arc::clone(child))));
                children.retain(|name, _| opinion.is_none_or(|opinion| opinion.children.get(name).is_none_or(Option::is_some)));
                attributes.extend(
                    sources
                        .iter()
                        .flat_map(|source| &source.attributes)
                        .chain(opinion.into_iter().flat_map(|opinion| &opinion.attributes).filter_map(|(id, value)| value.as_ref().map(|value| (id, value))))
                        .map(|(id, value)| (id.clone(), value.clone())),
                );
                attributes.retain(|id, _| opinion.is_none_or(|opinion| opinion.attributes.get(id).is_none_or(Option::is_some)));
                children
            })
            .ok()?
            .into_iter()
            .map(|(name, child)| {
                if stated.contains(&(head, names, &name)) {
                    occurrence(&path.child(name.clone()), Arc::unwrap_or_clone(child), opinions, stated, composed).map(|grown| (name, Arc::new(grown))).map_or_else(Validated::Fail, Validated::Good)
                } else {
                    Validated::Good((name, child))
                }
            })
            .collect::<Validated<IndexMap<_, _>, _>>()
            .ok()
            .map(|children| Composed { node, attributes, children })
    }
    let Layer { header: _, imports: _, schemas, data } = layer;
    let inherited = schemas::inheritance(&schemas);
    let opinions = collapse(data)
        .into_iter()
        .map(|(key, Node { path, children, inherits, attributes })| {
            attributes
                .into_iter()
                .map(|(id, value)| value.map(|value| schemas::decode(&schemas, &inherited, &path, &id, &value)).transpose().map(|value| (id, value)))
                .collect::<Validated<IndexMap<_, _>, _>>()
                .map(|attributes| (key, Node { path, children, inherits, attributes }))
        })
        .collect::<Validated<IndexMap<_, _>, _>>()
        .ok()?;
    let defined = opinions.keys().map(Path::head).collect::<HashSet<_>>();
    let stated = opinions
        .keys()
        .flat_map(|path| {
            let (head, names) = path.0.split_first();
            iter::successors(names.split_last(), |&(_, parent)| parent.split_last()).map(move |(name, parent)| (head, parent, name))
        })
        .collect::<HashSet<_>>();
    let mut heads = DiGraphMap::<&Name, ()>::new();
    opinions.keys().for_each(|path| {
        heads.add_node(path.head());
    });
    heads.extend(opinions.iter().flat_map(|(path, opinion)| opinion.inherits.values().chain(opinion.children.values()).flatten().map(|target| (path.head(), target.head()))));
    let (composed, failures) = tarjan_scc(&heads).into_iter().fold((HashMap::new(), Vec::new()), |(mut composed, mut failures), members| {
        match <[_; 1]>::try_from(members) {
            Ok([head]) if heads.contains_edge(head, head) => failures.push(IfcError::Cycle { head: head.clone() }),
            Ok([head]) if defined.contains(head) && heads.neighbors(head).all(|dependency| composed.contains_key(dependency) || !defined.contains(dependency)) => {
                let path = Path(NEVec::new(head.clone()));
                match occurrence(&path, Composed { node: path.clone(), attributes: IndexMap::new(), children: IndexMap::new() }, &opinions, &stated, &composed) {
                    Ok(node) => {
                        composed.insert(head, Arc::new(node));
                    }
                    Err(errors) => failures.extend(errors),
                }
            }
            Ok([_]) => {}
            Err(cycle) => failures.extend(cycle.into_iter().map(|head| IfcError::Cycle { head: head.clone() })),
        }
        (composed, failures)
    });
    let nodes = heads.nodes().filter_map(|head| composed.get(head).map(|node| (head.clone(), Arc::clone(node)))).collect::<IndexMap<_, _>>();
    let roots = heads.nodes().filter(|head| heads.neighbors_directed(head, Direction::Incoming).next().is_none()).filter_map(|head| nodes.get(head).map(Arc::clone)).collect();
    NEVec::try_from_vec(failures).map_or_else(|| Ok(Model { schemas, nodes, roots }), Err)
}
