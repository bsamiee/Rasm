use core::{hash::Hash, iter};
use std::collections::HashSet;

use indexmap::{IndexMap, IndexSet};
use nonempty_collections::NEVec;
use petgraph::{
    graphmap::DiGraphMap,
    visit::{Dfs, Visitable as _, Walker as _},
};
use serde_json::value::Serializer;
use validated::Validated;

use crate::{DataType, Header, IfcError, Layer, Model, Node, Path, Schema, Value, ValueDescription, composition, schemas};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn overlay<I>(model: &Model, header: Header, schemas: &IndexMap<Box<str>, Schema>, nodes: I) -> Result<Layer, NEVec<IfcError>>
where I: IntoIterator<Item = Node<Value>> {
    let described = iter::chain(model.schemas(), schemas).map(|(id, schema)| (id.clone(), schema.clone())).collect::<IndexMap<_, _>>();
    let inherited = schemas::inheritance(&described);
    let encoded = |node: &Path, id: &str, value: &Value| {
        serde_path_to_error::serialize(value, Serializer)
            .map_err(|source| IfcError::Invalid { node: node.clone(), attribute: id.into(), source })
            .and_then(|json| schemas::decode(&described, &inherited, node, id, &json).map(|_| json))
    };
    composition::collapse(nodes)
        .into_values()
        .map(|Node { path, children, inherits, attributes }| {
            attributes
                .into_iter()
                .map(|(id, value)| value.map(|value| encoded(&path, &id, &value)).transpose().map(|value| (id, value)))
                .collect::<Validated<IndexMap<_, _>, _>>()
                .map(|attributes| Node { path, children, inherits, attributes })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map(|data| Layer { header, imports: Vec::new(), schemas: carried(&described, &data, schemas.keys().map(AsRef::as_ref)), data })
}

#[must_use]
pub fn diff(before: &Layer, after: &Layer) -> Layer {
    fn opinions<K, T>(before: Option<&IndexMap<K, Option<T>>>, after: Option<&IndexMap<K, Option<T>>>) -> IndexMap<K, Option<T>>
    where
        K: Hash + Eq + Clone,
        T: PartialEq + Clone,
    {
        iter::chain(before, after)
            .flat_map(IndexMap::keys)
            .collect::<IndexSet<_>>()
            .into_iter()
            .filter_map(|key| {
                let restated = after.and_then(|opinions| opinions.get(key));
                (before.and_then(|opinions| opinions.get(key)) != restated).then(|| (key.clone(), restated.cloned().flatten()))
            })
            .collect()
    }
    let (base, revised) = (composition::collapse(before.data.iter().cloned()), composition::collapse(after.data.iter().cloned()));
    let data = base
        .keys()
        .chain(revised.keys())
        .collect::<IndexSet<_>>()
        .into_iter()
        .filter_map(|path| {
            let (stated, restated) = (base.get(path), revised.get(path));
            let children = opinions(stated.map(|node| &node.children), restated.map(|node| &node.children));
            let inherits = opinions(stated.map(|node| &node.inherits), restated.map(|node| &node.inherits));
            let attributes = opinions(stated.map(|node| &node.attributes), restated.map(|node| &node.attributes));
            (!(children.is_empty() && inherits.is_empty() && attributes.is_empty())).then(|| Node { path: path.clone(), children, inherits, attributes })
        })
        .collect::<Vec<_>>();
    let restated = after.schemas.iter().filter(|&(id, schema)| before.schemas.get(id) != Some(schema)).map(|(id, _)| &**id);
    Layer { header: after.header.clone(), imports: after.imports.iter().filter(|import| !before.imports.contains(import)).cloned().collect(), schemas: carried(&after.schemas, &data, restated), data }
}

fn carried<'held, V>(schemas: &'held IndexMap<Box<str>, Schema>, data: &'held [Node<V>], restated: impl IntoIterator<Item = &'held str>) -> IndexMap<Box<str>, Schema> {
    fn named(description: &ValueDescription) -> Vec<&str> {
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
        description.inherits.iter().map(AsRef::as_ref).chain(nested.into_iter().flat_map(named)).collect()
    }
    let references = schemas.iter().flat_map(|(id, schema)| named(&schema.value).into_iter().map(move |named| (&**id, named))).collect::<DiGraphMap<&str, ()>>();
    let written = data.iter().flat_map(|node| node.attributes.iter().filter_map(|(id, value)| value.is_some().then_some(&**id)));
    let reached = Dfs::from_parts(written.chain(restated).collect(), references.visit_map()).iter(&references).collect::<HashSet<_>>();
    schemas.iter().filter(|&(id, _)| reached.contains(&**id)).map(|(id, schema)| (id.clone(), schema.clone())).collect()
}
