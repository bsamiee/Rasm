use std::{borrow::Borrow, sync::Arc};

use hayro_syntax::object::dict::keys::{INFO, METADATA};
use lopdf::{Stream, decode_text_string};
use pdf_oxide::compliance::{
    ConversionConfig, ConversionResult, PdfAConverter, PdfALevel, PdfUaLevel, PdfXLevel, UaValidationResult, ValidationResult, XValidationResult, validate_pdf_a, validate_pdf_ua, validate_pdf_x,
};
use validated::Validated;
use xot::Xot;

use crate::{Document, GTS_PDFX_VERSION, PART, PDFAID_NAMESPACE, PDFUAID_NAMESPACE, PDFXID_NAMESPACE, PdfError, Standard, Subject, engine, entry, structure};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub enum Verdict {
    Archival(ValidationResult),
    Accessibility(UaValidationResult),
    Press(XValidationResult),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn claims(document: &Document) -> Result<Vec<Standard>, PdfError> { declared(&structure(document)?)?.into_iter().try_collect::<Vec<_>>() }

pub(crate) fn declared(structure: &lopdf::Document) -> Result<Vec<Result<Standard, PdfError>>, PdfError> {
    let packet = entry(structure, structure.catalog()?, METADATA)?.map(|metadata| metadata.as_stream().and_then(Stream::get_plain_content)).transpose()?;
    let mut xot = Xot::new();
    let root = packet
        .map(|bytes| {
            str::from_utf8(&bytes).map_err(PdfError::from).and_then(|text| xot.parse(text).map_err(|source| PdfError::Xml(source.into())))
        })
        .transpose()?;
    let property = |namespace: &str, local: &str| {
        let name = xot.namespace(namespace).and_then(|uri| xot.name_ns(local, uri))?;
        root.into_iter()
            .flat_map(|held| xot.descendants(held))
            .find_map(|node| xot.get_attribute(node, name).or_else(|| xot.element(node).filter(|element| element.name() == name).and_then(|_| xot.text_content_str(node))))
            .map(str::trim)
    };
    let press = match property(PDFXID_NAMESPACE, GTS_PDFX_VERSION) {
        Some(version) => Some(version.to_owned()),
        None => crate::nested(structure, &structure.trailer, &[INFO])?.and_then(|information| entry(structure, information, GTS_PDFX_VERSION).transpose()).map(|version| version.and_then(decode_text_string)).transpose()?,
    };
    let unsupported = |claim: String| PdfError::Claim(Arc::from(claim));
    Ok([
        property(PDFAID_NAMESPACE, PART).map(|part| {
            let conformance = property(PDFAID_NAMESPACE, "conformance").unwrap_or_default();
            PdfALevel::from_xmp(part, conformance).map(Standard::Archival).ok_or_else(|| unsupported(format!("pdfaid:part {part} conformance {conformance}")))
        }),
        property(PDFUAID_NAMESPACE, PART)
            .map(|part| [PdfUaLevel::Ua1, PdfUaLevel::Ua2].into_iter().find(|level| level.xmp_part() == part).map(Standard::Accessibility).ok_or_else(|| unsupported(format!("pdfuaid:part {part}")))),
        press.map(|version| PdfXLevel::from_gts_version(&version).map(Standard::Press).ok_or_else(|| unsupported(format!("{GTS_PDFX_VERSION} {version}")))),
    ]
    .into_iter()
    .flatten()
    .collect())
}

pub fn validate<S>(document: &Document, standards: S) -> Result<Vec<Verdict>, PdfError>
where S: IntoIterator<Item: Borrow<Standard>> {
    let mut engine = engine(document)?;
    standards
        .into_iter()
        .map(|standard| {
            let standard = *standard.borrow();
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
