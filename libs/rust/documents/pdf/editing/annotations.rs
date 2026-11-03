use core::borrow::Borrow;

use color::{OpaqueColor, Srgb};
use kurbo::Rect;
use pdf_oxide::{
    editor::{DocumentEditor, FormFieldValue, FormFieldWrapper},
    redaction::{RedactionOptions, RedactionReport},
};
use validated::Validated;

use crate::{Document, PdfError, Subject};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy)]
pub struct Redaction {
    pub page: usize,
    pub area: Rect,
    pub fill: Option<OpaqueColor<Srgb>>,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Finish {
    Interactive,
    Flattened,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [REDACTION]

pub fn redact<I>(document: &Document, redactions: I, options: RedactionOptions) -> Result<(Document, RedactionReport), PdfError>
where I: IntoIterator<Item: Borrow<Redaction>> {
    let regions = redactions
        .into_iter()
        .map(|redaction| {
            let &Redaction { page, area, fill } = Borrow::<Redaction>::borrow(&redaction);
            crate::page(document, page)
                .map(|sheet| {
                    let placed = crate::frame(sheet).0.inverse().transform_rect_bbox(area);
                    (page, [placed.x0, placed.y0, placed.x1, placed.y1].map(f64::cast::<f32>), fill.map(|color| color.components))
                })
                .map_err(|error| (Subject::Page(page), error))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(PdfError::Refused)?;
    edited(document, |editor| {
        regions.into_iter().try_for_each(|(page, rect, fill)| editor.add_redaction(page, rect, fill))?;
        editor.apply_redactions_destructive(options).and_then(|applied| {
            editor.sanitize_document(options).map(|scrubbed| {
                let mut report = applied;
                report.annotations_removed = applied.annotations_removed.strict_add(scrubbed.annotations_removed);
                report.bytes_removed = applied.bytes_removed.strict_add(scrubbed.bytes_removed);
                report
            })
        })
    })
}

// --- [FORMS]

pub fn fill<K>(document: &Document, values: impl IntoIterator<Item = (K, FormFieldValue)>, finish: Finish) -> Result<Document, PdfError>
where K: AsRef<str> {
    let held = values.into_iter().collect::<Vec<_>>();
    let set = |editor: &mut DocumentEditor| held.iter().try_for_each(|(name, value)| editor.set_form_field_value(name.as_ref(), value.clone()));
    let (filled, ()) = edited(document, set)?;
    match finish {
        Finish::Interactive => Ok(filled),
        Finish::Flattened => edited(&filled, |editor| {
            set(editor)?;
            editor.flatten_forms()
        })
        .map(|(flattened, ())| flattened),
    }
}

pub fn form_fields(document: &Document) -> Result<Vec<FormFieldWrapper>, PdfError> {
    DocumentEditor::from_bytes(document.data().as_ref().to_vec()).and_then(|mut editor| editor.get_form_fields()).map_err(PdfError::from)
}

fn edited<T>(document: &Document, edit: impl FnOnce(&mut DocumentEditor) -> pdf_oxide::Result<T>) -> Result<(Document, T), PdfError> {
    let mut editor = DocumentEditor::from_bytes(document.data().as_ref().to_vec())?;
    let outcome = edit(&mut editor)?;
    Ok((Document::new(editor.save_to_bytes()?, None)?, outcome))
}
