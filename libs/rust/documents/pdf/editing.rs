use std::collections::BTreeSet;

use hayro_syntax::object::dict::keys::{FORM, RESOURCES, SUBTYPE, XOBJECT};
use lopdf::{Dictionary, Object, ObjectId};
use pdf_oxide::editor::DocumentEditor;

use crate::{Document, PdfError};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn edited<T>(document: &Document, edit: impl FnOnce(&mut DocumentEditor) -> pdf_oxide::Result<T>) -> Result<(Document, T), PdfError> {
    let mut editor = crate::editor(document)?;
    let outcome = edit(&mut editor)?;
    Ok((Document::new(editor.save_to_bytes()?, None)?, outcome))
}

fn forms(structure: &lopdf::Document, resources: &Dictionary, found: BTreeSet<ObjectId>) -> Result<BTreeSet<ObjectId>, lopdf::Error> {
    crate::nested(structure, resources, &[XOBJECT])?.into_iter().flat_map(Dictionary::iter).try_fold(found, |mut found, (_, entry)| match structure.dereference(entry)? {
        (Some(id), Object::Stream(form)) if !found.contains(&id) && matches!(crate::entry(structure, &form.dict, SUBTYPE)?, Some(Object::Name(subtype)) if subtype == FORM) => {
            found.insert(id);
            match crate::nested(structure, &form.dict, &[RESOURCES])? {
                Some(held) => forms(structure, held, found),
                None => Ok(found),
            }
        }
        _ => Ok(found),
    })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod annotations;
pub mod assembly;
pub mod overlay;
pub mod signing;
