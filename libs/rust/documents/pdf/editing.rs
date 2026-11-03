use lopdf::{Dictionary, Object};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn resolved<'structure>(structure: &'structure lopdf::Document, object: &'structure Object) -> Result<&'structure Object, lopdf::Error> {
    static NULL: Object = Object::Null;
    match structure.dereference(object) {
        Err(lopdf::Error::ObjectNotFound(_)) => Ok(&NULL),
        found => found.map(|(_, target)| target),
    }
}

fn entry<'structure>(structure: &'structure lopdf::Document, holder: &'structure Dictionary, key: &str) -> Result<Option<&'structure Object>, lopdf::Error> {
    Ok(holder.has(key.as_bytes()).then(|| holder.get(key.as_bytes()).and_then(|value| resolved(structure, value))).transpose()?.filter(|value| !value.is_null()))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod annotations;
pub mod assembly;
pub mod signing;
