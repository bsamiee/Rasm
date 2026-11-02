#![feature(default_field_values, error_reporter, float_conversions, iterator_try_collect)]

use core::{fmt, str::Utf8Error};
use std::{collections::BTreeMap, error::Report, sync::Arc};

use hayro::hayro_interpret::TransformExt as _;
use hayro_syntax::{DecryptionError, LoadPdfError, Pdf, page::Page};
use jiff::Zoned;
use krilla::{
    configure::{Accessibility, Archival, ConfigurationError, Validators},
    error::KrillaError,
    icc::ICCProfile,
};
use kurbo::Affine;
use lopdf::{Dictionary, LoadOptions, Object, ObjectId, decode_text_string, dictionary, text_string};
use moxcms::{CmsError, ColorProfile, DataColorSpace};
use nonempty_collections::NEVec;
use pdf_oxide::{
    PdfDocument,
    compliance::{PdfALevel, PdfUaLevel, PdfXLevel},
    editor::{DocumentEditor, SaveOptions},
    ocr::OcrError,
};
use rasm_linework::LineworkError;
use uom::si::{
    f64::{Length, LinearNumberDensity},
    length::{inch, millimeter},
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const POINTS_PER_INCH: f64 = 72.0;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, derive_more::Debug, derive_more::Deref)]
#[deref(forward)]
pub struct Document(#[debug(skip)] Arc<Pdf>);

impl Document {
    pub fn new(bytes: Vec<u8>, password: Option<&str>) -> Result<Self, PdfError> {
        let plain = if lopdf::Document::load_metadata_mem(&bytes)?.encrypted {
            let mut decrypted = Vec::new();
            lopdf::Document::load_mem_with_options(&bytes, LoadOptions { password: password.map(str::to_owned), ..LoadOptions::default() })?.save_to(&mut decrypted)?;
            decrypted
        } else {
            bytes
        };
        Pdf::new(plain).map(|pdf| Self(Arc::new(pdf))).map_err(PdfError::Load)
    }
}

#[nutype::nutype(validate(predicate = |density: &LinearNumberDensity| density.is_normal() && density.is_sign_positive()), derive(Debug, Clone, Copy, PartialEq, Deref))]
pub struct Resolution(LinearNumberDensity);

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct OutputIntent {
    #[as_ref([u8])]
    data: Arc<[u8]>,
    #[as_ref]
    profile: ColorProfile,
}

impl OutputIntent {
    pub fn new(data: Arc<[u8]>) -> Result<Self, PdfError> {
        let profile = ColorProfile::new_from_slice(&data)?;
        (profile.data_color_space == DataColorSpace::Cmyk && ICCProfile::<4>::new(&data).is_some()).ok_or(PdfError::Intent).map(|()| Self { data, profile })
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Standard {
    Archival(PdfALevel),
    Accessibility(PdfUaLevel),
    Press(PdfXLevel),
}

#[derive(Debug, Clone, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Subject {
    #[display("element {index} of layer {layer} on page {page}")]
    Element { page: usize, layer: Arc<str>, index: usize },
    #[display("page {_0}")]
    Page(usize),
    #[display("part {_0}")]
    Part(usize),
    Standard(Standard),
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum PdfError {
    #[error(transparent)]
    Structure(#[from] lopdf::Error),
    #[error(transparent)]
    Engine(#[from] pdf_oxide::Error),
    #[error(transparent)]
    Writer(#[from] KrillaError),
    #[error("{}", fmt::from_fn(|formatter| {
        let standards = |validators: Validators| fmt::from_fn(move |list| write!(list, "{} and {}", validators.archival().map_or("no archival standard", Archival::as_str), validators.accessibility().map_or("no accessibility standard", Accessibility::as_str)));
        match .0 {
            ConfigurationError::NoOverlappingValidatorsRange(validators) => write!(formatter, "{} share no PDF version", standards(*validators)),
            ConfigurationError::VersionDoesNotMatchValidatorsRange(version, validators) => write!(formatter, "{} lies outside the versions {} allow", version.as_str(), standards(*validators)),
        }
    }))]
    Configuration(ConfigurationError),
    #[error("{}", match .0 {
        LoadPdfError::Decryption(DecryptionError::MissingIDEntry) => "encrypted document lacks its file identifier",
        LoadPdfError::Decryption(DecryptionError::PasswordProtected) => "document opens only with its password",
        LoadPdfError::Decryption(DecryptionError::InvalidEncryption) => "document encryption dictionary is malformed",
        LoadPdfError::Decryption(DecryptionError::UnsupportedAlgorithm) => "document encryption uses an unsupported algorithm",
        LoadPdfError::Invalid => "bytes hold no readable PDF",
    })]
    Load(LoadPdfError),
    #[error(transparent)]
    Linework(#[from] LineworkError),
    #[error(transparent)]
    Svg(#[from] usvg::Error),
    #[error(transparent)]
    Image(#[from] image::ImageError),
    #[error(transparent)]
    Tiff(#[from] tiff::TiffError),
    #[error(transparent)]
    Resize(#[from] fast_image_resize::ResizeError),
    #[error(transparent)]
    Profile(#[from] CmsError),
    #[error(transparent)]
    Compare(#[from] image_compare::CompareError),
    #[error(transparent)]
    Xml(#[from] xot::Error),
    #[error(transparent)]
    Http(#[from] reqwest::Error),
    #[error(transparent)]
    Recognition(#[from] OcrError),
    #[error("raster fails to decode: {0}")]
    Decode(Arc<str>),
    #[error("page {index} lies past the {count} pages of the document")]
    Page { index: usize, count: usize },
    #[error("text {0} reaches the writer unshaped")]
    Unshaped(Arc<str>),
    #[error("profile describes no CMYK output device")]
    Intent,
    #[error("{0} needs output the writer cannot produce")]
    Press(PdfXLevel),
    #[error("{press} and {accessibility} require different PDF versions")]
    Versions { press: PdfXLevel, accessibility: PdfUaLevel },
    #[error("label {0} names two pages")]
    Label(Arc<str>),
    #[error("outline entry {0} names no page and no label")]
    Destination(Arc<str>),
    #[error("attachment {0} is embedded twice")]
    Attachment(Arc<str>),
    #[error("measure unit {0} names no length")]
    Unit(Arc<str>),
    #[error("layer {0} prints and views differently under a standard forbidding usage application")]
    Usage(Arc<str>),
    #[error("timestamp authority answered status {0}")]
    Timestamp(u8),
    #[error("timestamp token stamps other bytes")]
    Imprint,
    #[error("raster of {width} by {height} pixels exceeds addressable memory")]
    Raster { width: u64, height: u64 },
    #[error("document holds no layer {0}")]
    Group(Arc<str>),
    #[error("refused{}", fmt::from_fn(|formatter| .0.iter().try_for_each(|(subject, error)| write!(formatter, "\n{subject}: {}", Report::new(error)))))]
    Refused(NEVec<(Subject, PdfError)>),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn save(document: &Document, options: SaveOptions) -> Result<Vec<u8>, PdfError> {
    DocumentEditor::from_bytes(document.data().as_ref().to_vec()).and_then(|mut editor| editor.save_to_bytes_with_options(options)).map_err(PdfError::from)
}

pub fn fields(document: &Document, index: usize) -> Result<BTreeMap<Arc<str>, Arc<str>>, PdfError> {
    let structure = structure(document)?;
    let pages = structure.page_iter().collect::<Vec<_>>();
    let sheet = structure.get_dictionary(*pages.get(index).ok_or(PdfError::Page { index, count: pages.len() })?)?;
    [b"PieceInfo".as_slice(), env!("CARGO_PKG_NAME").as_bytes(), b"Private".as_slice()]
        .into_iter()
        .try_fold(Some(sheet), |found, key| found.filter(|parent| parent.has(key)).map(|parent| structure.get_dict_in_dict(parent, key)).transpose())?
        .into_iter()
        .flat_map(Dictionary::iter)
        .map(|(key, value)| Ok::<_, lopdf::Error>((Arc::from(str::from_utf8(key).map_err(|Utf8Error { .. }| lopdf::Error::CharacterEncoding)?), Arc::from(decode_text_string(structure.dereference(value)?.1)?))))
        .collect::<Result<BTreeMap<_, _>, _>>()
        .map_err(PdfError::from)
}

fn structure(document: &Document) -> Result<lopdf::Document, PdfError> { lopdf::Document::load_mem(document.data().as_ref()).map_err(PdfError::from) }

fn engine(document: &Document) -> Result<PdfDocument, PdfError> { PdfDocument::from_bytes(document.data().as_ref().to_vec()).map_err(PdfError::from) }

fn page(document: &Document, index: usize) -> Result<&Page<'_>, PdfError> {
    let pages = document.pages();
    pages.get(index).ok_or(PdfError::Page { index, count: pages.len() })
}

fn frame(page: &Page<'_>) -> Affine { Affine::scale(Length::new::<inch>(POINTS_PER_INCH.recip()).get::<millimeter>()) * page.initial_transform(true).to_kurbo() }

fn write_fields(structure: &mut lopdf::Document, page: ObjectId, fields: &BTreeMap<Arc<str>, Arc<str>>, created: &Zoned) -> Result<(), PdfError> {
    let modified = "LastModified";
    let stamp = || Object::from(created.clone());
    let sheet = structure.get_dictionary_mut(page)?;
    if !fields.is_empty() {
        sheet.set("PieceInfo", dictionary! { env!("CARGO_PKG_NAME") => dictionary! { modified => stamp(), "Private" => fields.iter().map(|(key, value)| (key.as_bytes(), text_string(value))).collect::<Dictionary>() } });
        sheet.set(modified, stamp());
    }
    Ok(())
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod authoring;
pub mod conformance;
pub mod editing;
pub mod extraction;
pub mod layers;
pub mod measurement;
pub mod navigation;
pub mod proofs;
