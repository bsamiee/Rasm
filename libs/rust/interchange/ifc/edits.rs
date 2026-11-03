use core::{hash::Hash, iter};
use std::{collections::HashSet, sync::Arc};

use indexmap::IndexMap;
use itertools::Itertools as _;
use nonempty_collections::NEVec;
use petgraph::{
    graphmap::DiGraphMap,
    visit::{Dfs, Visitable as _, Walker as _},
};
use serde_json::value::Serializer;
use validated::Validated;

use crate::{
    Composed, DataType, Header, IfcError, Import, Layer, Model, Name, Node, Path, Schema, Value, ValueDescription,
    composition::{self, Subject},
    descend, schemas,
};

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [OVERLAYS]

pub fn overlay<I>(model: &Model, header: Header, schemas: &IndexMap<Box<str>, Schema>, imports: Vec<Import>, nodes: I) -> Result<Layer, NEVec<IfcError>>
where I: IntoIterator<Item = Node<Value>> {
    let described = iter::chain(model.schemas(), schemas).map(|(id, schema)| (&**id, schema)).collect::<IndexMap<_, _>>();
    let decode = schemas::decoder(described.iter().map(|(&id, &schema)| (id, schema)));
    let (data, refused) = composition::transcoded(composition::flatten(nodes), |node, id, value: Value| {
        serde_path_to_error::serialize(&value, Serializer)
            .map_err(|source| IfcError::Invalid { node: node.clone(), attribute: id.into(), source })
            .and_then(|json| decode(node, id, &json).map(|_| json))
    });
    NEVec::try_from_vec(refused).map_or_else(
        || {
            let data = data.into_values().collect::<Vec<_>>();
            Ok(Layer { header, imports, schemas: reachable(&described, &data, schemas.keys().map(|id| &**id)), data })
        },
        Err,
    )
}

pub fn merge(header: Header, base: &Layer, left: &Layer, right: &Layer) -> Result<Layer, NEVec<IfcError>> {
    fn merged<K, T>(
        path: &Path,
        subject: impl Fn(&K) -> String,
        base: Option<&IndexMap<K, Option<T>>>,
        left: Option<&IndexMap<K, Option<T>>>,
        right: Option<&IndexMap<K, Option<T>>>,
    ) -> Validated<IndexMap<K, Option<T>>, IfcError>
    where
        K: Hash + Eq + Clone,
        T: PartialEq + Clone,
    {
        iter::chain(left, right)
            .flat_map(IndexMap::keys)
            .unique()
            .filter_map(|key| {
                let (stated, ours, theirs) = (base.and_then(|held| held.get(key)), left.and_then(|held| held.get(key)), right.and_then(|held| held.get(key)));
                match (ours, theirs) {
                    (Some(chosen), Some(other)) if chosen == other || stated == Some(other) => Ok(Some(chosen)),
                    (Some(other), Some(chosen)) if stated == Some(other) => Ok(Some(chosen)),
                    (Some(chosen), None) | (None, Some(chosen)) if stated.is_none() => Ok(Some(chosen)),
                    (Some(kept), None) | (None, Some(kept)) if stated == Some(kept) => Ok(None),
                    (Some(_) | None, Some(_) | None) => Err(IfcError::Conflict { path: path.clone(), opinion: subject(key).into_boxed_str() }),
                }
                .map(|chosen| chosen.map(|chosen| (key.clone(), chosen.clone())))
                .transpose()
            })
            .collect()
    }
    let (held, ours, theirs) = (composition::flatten(borrowed(base)), composition::flatten(borrowed(left)), composition::flatten(borrowed(right)));
    let data = ours
        .keys()
        .chain(theirs.keys())
        .unique()
        .map(|path| {
            let (stated, left_node, right_node) = (held.get(path), ours.get(path), theirs.get(path));
            let children = merged(path, |name| Subject::Child(name).to_string(), stated.map(|node| &node.children), left_node.map(|node| &node.children), right_node.map(|node| &node.children));
            let inherits = merged(path, |name| Subject::Inherit(name).to_string(), stated.map(|node| &node.inherits), left_node.map(|node| &node.inherits), right_node.map(|node| &node.inherits));
            let attributes = merged(path, |id| Subject::Attribute(id).to_string(), stated.map(|node| &node.attributes), left_node.map(|node| &node.attributes), right_node.map(|node| &node.attributes));
            children.map3(inherits, attributes, |children, inherits, attributes| owned(Node { path: path.clone(), children, inherits, attributes }))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    Ok(Layer {
        header,
        imports: iter::chain(&left.imports, &right.imports).unique_by(|import| &import.uri).cloned().collect(),
        schemas: iter::chain(&left.schemas, &right.schemas).map(|(id, schema)| (id.clone(), schema.clone())).collect(),
        data,
    })
}

// --- [DIFFS]

#[must_use]
pub fn diff(before: &Layer, after: &Layer) -> Layer {
    fn changed<K, T>(before: Option<&IndexMap<K, Option<T>>>, after: Option<&IndexMap<K, Option<T>>>) -> IndexMap<K, Option<T>>
    where
        K: Hash + Eq + Clone,
        T: PartialEq + Clone,
    {
        iter::chain(before, after)
            .flat_map(IndexMap::keys)
            .unique()
            .filter_map(|key| {
                let restated = after.and_then(|opinions| opinions.get(key));
                (before.and_then(|opinions| opinions.get(key)) != restated).then(|| (key.clone(), restated.cloned().flatten()))
            })
            .collect()
    }
    let (base, revised) = (composition::flatten(borrowed(before)), composition::flatten(borrowed(after)));
    let data = base
        .keys()
        .chain(revised.keys())
        .unique()
        .flat_map(|path| {
            let (stated, restated) = (base.get(path), revised.get(path));
            let children = changed(stated.map(|node| &node.children), restated.map(|node| &node.children));
            let mut inherits = changed(stated.map(|node| &node.inherits), restated.map(|node| &node.inherits));
            let attributes = changed(stated.map(|node| &node.attributes), restated.map(|node| &node.attributes));
            let (held, listed) = (stated.map(|node| &node.inherits), restated.map(|node| &node.inherits));
            let kept = iter::zip(held.into_iter().flatten(), listed.into_iter().flatten()).take_while(|((left, _), (right, _))| left == right).count();
            let moved = listed.into_iter().flatten().skip(kept).map(|(name, target)| (name.clone(), target.clone())).collect::<IndexMap<_, _>>();
            let cleared = moved.keys().filter(|name| held.is_some_and(|names| names.contains_key(*name))).map(|name| (name.clone(), None)).collect::<IndexMap<_, _>>();
            inherits.retain(|name, _| !moved.contains_key(name));
            inherits.extend(moved);
            [Node { inherits: cleared, ..Node::new(path.clone()) }, Node { children, inherits, attributes, ..Node::new(path.clone()) }]
        })
        .filter(|node| !(node.children.is_empty() && node.inherits.is_empty() && node.attributes.is_empty()))
        .map(owned)
        .collect::<Vec<_>>();
    let revised_schemas = after.schemas.iter().map(|(id, schema)| (&**id, schema)).collect::<IndexMap<_, _>>();
    let restated = after.schemas.iter().filter(|&(id, schema)| before.schemas.get(id) != Some(schema)).map(|(id, _)| &**id);
    Layer { header: after.header.clone(), imports: after.imports.iter().filter(|import| !before.imports.contains(import)).cloned().collect(), schemas: reachable(&revised_schemas, &data, restated), data }
}

#[must_use]
pub fn effective_diff(before: &Model, after: &Model) -> Vec<Node<Value>> {
    fn shared(model: &Model, child: &Arc<Composed>) -> bool { descend(&model.heads, &child.path).is_some_and(|definition| Arc::ptr_eq(definition, child)) }
    let none = (IndexMap::<Box<str>, Arc<Value>>::new(), IndexMap::<Name, Arc<Composed>>::new());
    let none = &none;
    gen move {
        let mut pending = before
            .heads
            .keys()
            .chain(after.heads.keys())
            .unique()
            .map(|head| (Path(NEVec::new(head.clone())), [before, after].map(|model| model.heads.get(head).map_or((&none.0, &none.1), |node| (&node.attributes, &node.children)))))
            .collect::<Vec<_>>();
        while let Some((path, [(attributes, children), (revised, listed)])) = pending.pop() {
            let changed = attributes
                .keys()
                .chain(revised.keys())
                .unique()
                .filter(|id| attributes.get(*id) != revised.get(*id))
                .map(|id| (id.clone(), revised.get(id).map(|value| Value::clone(value))))
                .collect::<IndexMap<_, _>>();
            let names = children.keys().chain(listed.keys()).unique().collect::<Vec<_>>();
            let targets = names
                .iter()
                .filter_map(|&name| match (children.get(name), listed.get(name)) {
                    (Some(left), Some(right)) if left.path == right.path => None,
                    (_, right) => Some((name.clone(), right.map(|child| child.path.clone()))),
                })
                .collect::<IndexMap<_, _>>();
            pending.extend(names.into_iter().filter_map(|name| {
                let (left, right) = (children.get(name)?, listed.get(name)?);
                (!(shared(before, left) && shared(after, right))).then(|| (path.child(name.clone()), [(&left.attributes, &left.children), (&right.attributes, &right.children)]))
            }));
            yield Node { children: targets, attributes: changed, ..Node::new(path) };
        }
    }
    .filter(|node| !(node.children.is_empty() && node.attributes.is_empty()))
    .collect()
}

// --- [SHARED]

fn borrowed(layer: &Layer) -> impl Iterator<Item = Node<&serde_json::Value>> {
    layer.data.iter().map(|node| Node { path: node.path.clone(), children: node.children.clone(), inherits: node.inherits.clone(), attributes: node.attributes.iter().map(|(id, value)| (id.clone(), value.as_ref())).collect() })
}

fn owned(Node { path, children, inherits, attributes }: Node<&serde_json::Value>) -> Node {
    Node { path, children, inherits, attributes: attributes.into_iter().map(|(id, value)| (id, value.cloned())).collect() }
}

fn reachable<'held, V>(schemas: &IndexMap<&'held str, &'held Schema>, data: &'held [Node<V>], restated: impl IntoIterator<Item = &'held str>) -> IndexMap<Box<str>, Schema> {
    fn referenced(description: &ValueDescription) -> Vec<&str> {
        let nested = match &description.data_type {
            DataType::Array { restrictions } => vec![&restrictions.value],
            DataType::Object { restrictions } => restrictions.iter().flat_map(|restrictions| restrictions.values.values()).collect(),
            DataType::Real { quantity_kind: _ }
            | DataType::Boolean
            | DataType::Integer
            | DataType::String
            | DataType::DateTime
            | DataType::Enum { restrictions: _ }
            | DataType::Reference
            | DataType::Blob => Vec::new(),
        };
        description.inherits.iter().map(|id| &**id).chain(nested.into_iter().flat_map(referenced)).collect()
    }
    let references = schemas.iter().flat_map(|(&id, schema)| referenced(&schema.value).into_iter().map(move |named| (id, named))).collect::<DiGraphMap<&str, ()>>();
    let written = data.iter().flat_map(|node| node.attributes.iter().filter_map(|(id, value)| value.is_some().then_some(&**id)));
    let reached = Dfs::from_parts(written.chain(restated).collect(), references.visit_map()).iter(&references).collect::<HashSet<_>>();
    schemas.iter().filter(|&(id, _)| reached.contains(*id)).map(|(&id, &schema)| (Box::from(id), schema.clone())).collect()
}
