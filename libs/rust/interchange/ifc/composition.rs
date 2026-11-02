use std::{collections::HashSet, sync::Arc};

use indexmap::IndexMap;
use nonempty_collections::NEVec;
use petgraph::{Direction, algo::toposort, graphmap::DiGraphMap};
use validated::Validated;

use crate::{Composed, IfcError, Layer, Model, Name, Node, Path, Value, schemas};

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
        opinions.inherits.extend(inherits);
        opinions.attributes.extend(attributes);
        held
    })
}

pub fn compose(layer: Layer) -> Result<Model, NEVec<IfcError>> {
    fn occurrence(path: &Path, node: Composed, opinions: &IndexMap<Path, Node<Value>>, overlaid: &HashSet<Path>, composed: &IndexMap<Name, Arc<Composed>>) -> Result<Composed, IfcError> {
        let descend = |target: &Path| {
            let (head, names) = target.0.split_first();
            composed.get(head).and_then(|defined| names.iter().try_fold(defined, |node, name| node.children.get(name))).ok_or_else(|| IfcError::Unknown { path: target.clone() })
        };
        let opinion = opinions.get(path);
        let inherited = opinion.into_iter().flat_map(|opinion| opinion.inherits.values().flatten()).try_fold(node, |mut node, target| {
            descend(target).map(|source| {
                node.children.extend(source.children.iter().map(|(name, child)| (name.clone(), Arc::clone(child))));
                node.attributes.extend(source.attributes.iter().map(|(id, value)| (id.clone(), value.clone())));
                node
            })
        })?;
        let parented = opinion.into_iter().flat_map(|opinion| &opinion.children).try_fold(inherited, |mut node, (name, target)| match target {
            Some(target) => descend(target).map(|child| {
                node.children.insert(name.clone(), Arc::clone(child));
                node
            }),
            None => {
                node.children.shift_remove(name);
                Ok(node)
            }
        })?;
        let Composed { node, attributes, children } = opinion.into_iter().flat_map(|opinion| &opinion.attributes).fold(parented, |mut node, (id, value)| {
            match value {
                Some(value) => {
                    node.attributes.insert(id.clone(), value.clone());
                }
                None => {
                    node.attributes.shift_remove(id);
                }
            }
            node
        });
        let children = children
            .into_iter()
            .map(|(name, child)| {
                let at = path.child(name.clone());
                if overlaid.contains(&at) { occurrence(&at, Arc::unwrap_or_clone(child), opinions, overlaid, composed).map(|grown| (name, Arc::new(grown))) } else { Ok((name, child)) }
            })
            .try_collect::<IndexMap<_, _>>()?;
        Ok(Composed { node, attributes, children })
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
    let mut heads = DiGraphMap::<&Name, ()>::new();
    opinions.keys().for_each(|path| {
        heads.add_node(path.head());
    });
    heads.extend(opinions.iter().flat_map(|(path, opinion)| opinion.inherits.values().chain(opinion.children.values()).flatten().map(|target| (path.head(), target.head()))));
    let order = toposort(&heads, None).map_err(|cycle| NEVec::new(IfcError::Cycle { head: cycle.node_id().clone() }))?;
    let roots = heads.nodes().filter(|head| heads.neighbors_directed(*head, Direction::Incoming).next().is_none()).collect::<Vec<_>>();
    let overlaid = opinions
        .keys()
        .flat_map(|path| {
            let (head, names) = path.0.split_first();
            (1..=names.len()).map(move |count| Path(NEVec::from((head.clone(), names.iter().take(count).cloned().collect()))))
        })
        .collect::<HashSet<_>>();
    let composed = order
        .into_iter()
        .rev()
        .try_fold(IndexMap::new(), |mut composed, head| {
            let path = Path(NEVec::new(head.clone()));
            occurrence(&path, Composed { node: path.clone(), attributes: IndexMap::new(), children: IndexMap::new() }, &opinions, &overlaid, &composed).map(|node| {
                composed.insert(head.clone(), Arc::new(node));
                composed
            })
        })
        .map_err(NEVec::new)?;
    Ok(Model { schemas, roots: roots.into_iter().filter_map(|head| composed.get(head).map(Arc::clone)).collect(), nodes: composed })
}
