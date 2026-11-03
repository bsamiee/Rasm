use pdf_oxide::editor::DocumentEditor;

use crate::{Document, PdfError};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn edited<T>(document: &Document, edit: impl FnOnce(&mut DocumentEditor) -> pdf_oxide::Result<T>) -> Result<(Document, T), PdfError> {
    let mut editor = crate::editor(document)?;
    let outcome = edit(&mut editor)?;
    Ok((Document::new(editor.save_to_bytes()?, None)?, outcome))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod annotations;
pub mod assembly;
pub mod signing;
