use core::hash::Hash;
use std::collections::HashSet;

use indexmap::{IndexMap, IndexSet};
use nonempty_collections::NEVec;
use petgraph::{
    graphmap::DiGraphMap,
    visit::{Dfs, Visitable as _, Walker as _},
};
use validated::Validated;

use crate::{Header, IfcError, Layer, Model, Node, Path, Schema, Value, composition, schemas};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn overlay<I>(model: &Model, header: Header, nodes: I) -> Result<Layer, NEVec<IfcError>>
where I: IntoIterator<Item = Node<Value>> {
    let inherited = schemas::inheritance(model.schemas());
    let encoded = |node: &Path, id: &str, value: &Value| {
        serde_path_to_error::serialize(value, serde_json::value::Serializer)
            .map_err(|source| IfcError::Invalid { node: node.clone(), attribute: id.into(), source })
            .and_then(|json| schemas::decode(model.schemas(), &inherited, node, id, &json).map(|_| json))
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
        .map(|data| Layer { header, imports: Vec::new(), schemas: carried(model.schemas(), &inherited, &data), data })
}

#[must_use]
pub fn diff(before: &Layer, after: &Layer) -> Layer {
    fn opinions<K, T>(before: Option<&IndexMap<K, Option<T>>>, after: Option<&IndexMap<K, Option<T>>>) -> IndexMap<K, Option<T>>
    where
        K: Hash + Eq + Clone,
        T: PartialEq + Clone,
    {
        let held = |opinions: Option<&IndexMap<K, Option<T>>>, key: &K| opinions.and_then(|opinions| opinions.get(key));
        before
            .into_iter()
            .chain(after)
            .flat_map(IndexMap::keys)
            .collect::<IndexSet<_>>()
            .into_iter()
            .filter(|key| held(before, key) != held(after, key))
            .map(|key| (key.clone(), held(after, key).cloned().flatten()))
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
    Layer { header: before.header.clone(), imports: Vec::new(), schemas: carried(&after.schemas, &schemas::inheritance(&after.schemas), &data), data }
}

fn carried<V>(schemas: &IndexMap<Box<str>, Schema>, inherited: &DiGraphMap<&str, ()>, data: &[Node<V>]) -> IndexMap<Box<str>, Schema> {
    let written = data.iter().flat_map(|node| node.attributes.iter().filter_map(|(id, value)| value.is_some().then_some(&**id))).collect::<Vec<_>>();
    let reached = Dfs::from_parts(written, inherited.visit_map()).iter(inherited).collect::<HashSet<_>>();
    schemas.iter().filter(|&(id, _)| reached.contains(&**id)).map(|(id, schema)| (id.clone(), schema.clone())).collect()
}
