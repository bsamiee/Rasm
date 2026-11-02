use pdf_oxide::compliance::{
    ConversionConfig, ConversionResult, PdfAConverter, PdfALevel, UaValidationResult, ValidationResult, XValidationResult, validate_pdf_a, validate_pdf_ua, validate_pdf_x,
};
use validated::Validated;

use crate::{Document, PdfError, Standard, Subject, engine};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub enum Verdict {
    Archival(ValidationResult),
    Accessibility(UaValidationResult),
    Press(XValidationResult),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn validate(document: &Document, standards: impl IntoIterator<Item = Standard>) -> Result<Vec<Verdict>, PdfError> {
    let mut engine = engine(document)?;
    standards
        .into_iter()
        .map(|standard| {
            match standard {
                Standard::Archival(level) => validate_pdf_a(&mut engine, level).map(Verdict::Archival),
                Standard::Accessibility(level) => validate_pdf_ua(&mut engine, level).map(Verdict::Accessibility),
                Standard::Press(level) => validate_pdf_x(&mut engine, level).map(Verdict::Press),
            }
            .map_err(|source| (Subject::Standard(standard), PdfError::Engine(source)))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(PdfError::Refused)
}

pub fn convert(document: &Document, level: PdfALevel, config: ConversionConfig) -> Result<(Document, ConversionResult), PdfError> {
    let mut engine = engine(document)?;
    let result = PdfAConverter::new(level).with_config(config).convert(&mut engine)?;
    Document::new(engine.source_bytes, None).map(|converted| (converted, result))
}
