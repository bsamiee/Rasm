use std::collections::BTreeSet;

use hayro_syntax::object::dict::keys::{FORM, RESOURCES, SUBTYPE, XOBJECT};
use lopdf::{Dictionary, Object, ObjectId};
use pdf_oxide::editor::DocumentEditor;

use crate::{Document, PdfError};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn edited<T>(document: &Document, edit: impl FnOnce(&mut DocumentEditor) -> pdf_oxide::Result<T>) -> Result<(Document, T), PdfError> {
    let mut editor = crate::editor(document)?;
    let outcome = edit(&mut editor)?;
    Ok((crate::sealed(document, editor.save_to_bytes()?)?, outcome))
}

fn forms<'document>(structure: &'document lopdf::Document, resources: &'document Dictionary, mut found: BTreeSet<ObjectId>) -> Result<BTreeSet<ObjectId>, lopdf::Error> {
    let entries = |holder: &'document Dictionary, path: &[&[u8]]| crate::nested(structure, holder, path).map(|held| held.into_iter().flat_map(Dictionary::iter));
    let mut pending = vec![entries(resources, &[XOBJECT])?];
    while let Some(siblings) = pending.last_mut() {
        let Some((_, object)) = siblings.next() else {
            pending.pop();
            continue;
        };
        if let (Some(id), Object::Stream(form)) = structure.dereference(object)?
            && !found.contains(&id)
            && matches!(crate::entry(structure, &form.dict, SUBTYPE)?, Some(Object::Name(subtype)) if subtype == FORM)
        {
            found.insert(id);
            pending.push(entries(&form.dict, &[RESOURCES, XOBJECT])?);
        }
    }
    Ok(found)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod annotations;
mod assembly;
mod overlay;
mod signing;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use annotations::{Finish, Redaction, fill, form_fields, redact};
pub use assembly::{Part, assemble, split};
pub use overlay::overlay;
pub use signing::{Authority, Level, sign};
