use core::str::Utf8Error;
use std::borrow::Borrow;

use lopdf::{Dictionary, Object, Stream, decode_text_string};
use pdf_oxide::compliance::{
    ConversionConfig, ConversionResult, PdfAConverter, PdfALevel, PdfUaLevel, PdfXLevel, UaValidationResult, ValidationResult, XValidationResult, validate_pdf_a, validate_pdf_ua, validate_pdf_x,
};
use validated::Validated;
use xot::Xot;

use crate::{Document, PdfError, Standard, Subject, engine, structure};

// --- [CONSTANTS] -----------------------------------------------------------------------

const PDFA_NAMESPACE: &str = "http://www.aiim.org/pdfa/ns/id/";
const PDFUA_NAMESPACE: &str = "http://www.aiim.org/pdfua/ns/id/";
const PDFX_NAMESPACE: &str = "http://www.npes.org/pdfx/ns/id/";
const PDFX_VERSION: &str = "GTS_PDFXVersion";
const PART: &str = "part";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub enum Verdict {
    Archival(ValidationResult),
    Accessibility(UaValidationResult),
    Press(XValidationResult),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn claims(document: &Document) -> Result<Vec<Standard>, PdfError> {
    let structure = structure(document)?;
    let packet = entry(&structure, structure.catalog()?, "Metadata")?.map(|metadata| metadata.as_stream().and_then(Stream::get_plain_content)).transpose()?;
    let mut xot = Xot::new();
    let root = packet
        .map(|bytes| {
            str::from_utf8(&bytes).map_err(|Utf8Error { .. }| PdfError::Structure(lopdf::Error::CharacterEncoding)).and_then(|text| xot.parse(text).map_err(|source| PdfError::Xml(source.into())))
        })
        .transpose()?;
    let property = |namespace: &str, local: &str| {
        let name = xot.namespace(namespace).and_then(|uri| xot.name_ns(local, uri))?;
        root.into_iter()
            .flat_map(|held| xot.descendants(held))
            .find_map(|node| xot.get_attribute(node, name).or_else(|| xot.element(node).filter(|element| element.name() == name).and_then(|_| xot.text_content_str(node))))
            .map(str::trim)
    };
    let stated = || -> Result<Option<String>, lopdf::Error> {
        let Some(information) = entry(&structure, &structure.trailer, "Info")? else {
            return Ok(None);
        };
        entry(&structure, information.as_dict()?, PDFX_VERSION)?.map(decode_text_string).transpose()
    };
    let press = property(PDFX_NAMESPACE, PDFX_VERSION).map(str::to_owned).map_or_else(stated, |version| Ok(Some(version)))?;
    let unsupported = |claim: String| PdfError::Engine(pdf_oxide::Error::Unsupported(format!("validation against the claimed {claim}")));
    [
        property(PDFA_NAMESPACE, PART).map(|part| {
            let conformance = property(PDFA_NAMESPACE, "conformance").unwrap_or_default();
            PdfALevel::from_xmp(part, conformance).map(Standard::Archival).ok_or_else(|| unsupported(format!("pdfaid:part {part} conformance {conformance}")))
        }),
        property(PDFUA_NAMESPACE, PART)
            .map(|part| [PdfUaLevel::Ua1, PdfUaLevel::Ua2].into_iter().find(|level| level.xmp_part() == part).map(Standard::Accessibility).ok_or_else(|| unsupported(format!("pdfuaid:part {part}")))),
        press.map(|version| PdfXLevel::from_gts_version(&version).map(Standard::Press).ok_or_else(|| unsupported(format!("{PDFX_VERSION} {version}")))),
    ]
    .into_iter()
    .flatten()
    .try_collect::<Vec<_>>()
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

fn entry<'held>(structure: &'held lopdf::Document, dictionary: &'held Dictionary, key: &str) -> Result<Option<&'held Object>, lopdf::Error> {
    dictionary.has(key.as_bytes()).then(|| dictionary.get_deref(key.as_bytes(), structure)).transpose()
}
