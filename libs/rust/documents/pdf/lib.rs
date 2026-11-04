#![feature(array_try_map, default_field_values, deref_patterns, error_reporter, float_conversions, gen_blocks, integer_casts, iterator_try_collect)]

use core::{borrow::Borrow, fmt, str::Utf8Error};
use std::{
    collections::{BTreeMap, HashSet},
    error::Report,
    sync::Arc,
};

use hayro::hayro_interpret::TransformExt as _;
use hayro_syntax::{
    DecryptionError, LoadPdfError, Pdf,
    object::dict::keys::{LAST_MODIFIED, PIECE_INFO, PRIVATE, USER_UNIT},
    page::Page,
};
use jiff::Zoned;
use krilla::{
    configure::{Accessibility, Archival, ConfigurationError, Validators},
    error::KrillaError,
    geom,
    icc::ICCProfile,
    pdf,
};
use kurbo::{Affine, Rect, Size};
use lopdf::{Dictionary, LoadOptions, Object, ObjectId, decode_text_string, dictionary, text_string};
use moxcms::{CmsError, ColorProfile, DataColorSpace, ProfileClass};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use pdf_oxide::{
    PdfDocument,
    compliance::{PdfALevel, PdfUaLevel, PdfXLevel},
    editor::{DocumentEditor, SaveOptions},
    ocr::OcrError,
};
use rasm_linework::{LineworkError, WidthError};
use rasn::{
    error::{DecodeError, EncodeError},
    types::Enumerated as _,
};
use rasn_cms::tsp::PkiStatus;
use secrecy::{ExposeSecret as _, SecretString};
use uom::si::{
    f64::{Length, LinearNumberDensity},
    length::{inch, millimeter},
};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub type Fields = BTreeMap<Arc<str>, Arc<str>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PDFAID_NAMESPACE: &str = "http://www.aiim.org/pdfa/ns/id/";
const PDFUAID_NAMESPACE: &str = "http://www.aiim.org/pdfua/ns/id/";
const PDFXID_NAMESPACE: &str = "http://www.npes.org/pdfx/ns/id/";
const GTS_PDFX_VERSION: &str = "GTS_PDFXVersion";
const PART: &str = "part";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, derive_more::Debug, derive_more::Deref)]
#[deref(forward)]
pub struct Document(#[debug(skip)] Arc<Pdf>);

impl Document {
    pub fn new(bytes: Vec<u8>, password: Option<&SecretString>) -> Result<Self, PdfError> {
        let plain = if lopdf::Document::load_metadata_mem(&bytes)?.encrypted {
            let mut decrypted = Vec::new();
            lopdf::Document::load_mem_with_options(&bytes, LoadOptions { password: password.map(|secret| secret.expose_secret().to_owned()), ..LoadOptions::default() })?
                .save_to(&mut decrypted)
                .map_err(lopdf::Error::from)?;
            decrypted
        } else {
            bytes
        };
        Self::from_plain_bytes(plain)
    }

    fn from_plain_bytes(bytes: Vec<u8>) -> Result<Self, PdfError> { Pdf::new(bytes).map(|pdf| Self(Arc::new(pdf))).map_err(PdfError::Load) }
}

#[nutype::nutype(validate(predicate = |density: &LinearNumberDensity| density.is_normal() && density.is_sign_positive()), derive(Debug, Clone, Copy, PartialEq, Deref))]
pub struct Resolution(LinearNumberDensity);

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct OutputIntent {
    #[as_ref([u8])]
    data: Arc<[u8]>,
    #[as_ref(ColorProfile)]
    profile: Arc<ColorProfile>,
    icc: ICCProfile<4>,
}

impl OutputIntent {
    pub fn new(data: Arc<[u8]>) -> Result<Self, PdfError> {
        let profile = ColorProfile::new_from_slice(&data)?;
        (profile.profile_class == ProfileClass::OutputDevice && profile.color_space == DataColorSpace::Cmyk).ok_or(PdfError::Intent)?;
        ICCProfile::new(&data).ok_or(PdfError::Intent).map(|icc| Self { data, profile: Arc::new(profile), icc })
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display, strum::EnumDiscriminants)]
#[strum_discriminants(name(Family), derive(Hash, derive_more::Display))]
pub enum Standard {
    Archival(PdfALevel),
    Accessibility(PdfUaLevel),
    Press(PdfXLevel),
}

#[derive(Debug, Clone, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Subject {
    #[display("element {index} of layer {layer} on page {page}")]
    Element {
        page: usize,
        layer: Arc<str>,
        index: usize,
    },
    #[display("page {_0}")]
    Page(usize),
    #[display("part {_0}")]
    Part(usize),
    #[display("metadata")]
    Metadata,
    #[display("outline")]
    Outline,
    #[display("optional content configuration")]
    Configuration,
    #[display("optional content group {_0}")]
    Group(usize),
    Standard(Standard),
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Refusal {
    #[display("raster fails to decode")]
    Decode,
    #[display("text reaches the writer unshaped")]
    Unshaped,
    #[display("metadata holds no element")]
    Metadata,
    #[display("label names two pages")]
    Label,
    #[display("outline entry names no page and no label")]
    Destination,
    #[display("attachment is embedded twice")]
    Attachment,
    #[display("measure unit names no length")]
    Unit,
    #[display("layer prints and views differently under a standard forbidding usage application")]
    Usage,
}

#[derive(Debug, thiserror::Error)]
pub enum PdfError {
    #[error(transparent)]
    Structure(#[from] lopdf::Error),
    #[error(transparent)]
    Engine(#[from] pdf_oxide::Error),
    #[error(transparent)]
    Writer(#[from] KrillaError),
    #[error("{}", fmt::from_fn(|formatter| {
        let standards = |validators: Validators| fmt::from_fn(move |list| {
            let archival = validators.archival().map_or("no archival standard", Archival::as_str);
            let accessibility = validators.accessibility().map_or("no accessibility standard", Accessibility::as_str);
            write!(list, "{archival} and {accessibility}")
        });
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
    Width(#[from] WidthError),
    #[error(transparent)]
    Svg(#[from] usvg::Error),
    #[error(transparent)]
    Image(#[from] image::ImageError),
    #[error(transparent)]
    Tiff(#[from] tiff::TiffError),
    #[error(transparent)]
    Resize(#[from] fast_image_resize::ResizeError),
    #[error(transparent)]
    ImageBuffer(#[from] fast_image_resize::ImageBufferError),
    #[error(transparent)]
    Profile(#[from] CmsError),
    #[error(transparent)]
    Compare(#[from] image_compare::CompareError),
    #[error(transparent)]
    Xml(#[from] xot::Error),
    #[error(transparent)]
    Decode(#[from] DecodeError),
    #[error(transparent)]
    Encode(#[from] EncodeError),
    #[error(transparent)]
    Text(#[from] Utf8Error),
    #[error(transparent)]
    Http(#[from] reqwest::Error),
    #[error(transparent)]
    Recognition(#[from] OcrError),
    #[error("{refusal}: {name}")]
    Named { name: Arc<str>, refusal: Refusal },
    #[error("page {index} lies past the {count} pages of the document")]
    Page { index: usize, count: usize },
    #[error("area of {} by {} encloses nothing", .0.width(), .0.height())]
    Area(Rect),
    #[error("font data of {family} holds no face {index}")]
    Font { family: Arc<str>, index: u32 },
    #[error("document holds no optional content group {} {} R", .0.0, .0.1)]
    Group(ObjectId),
    #[error("PDF metadata cannot represent year {0}")]
    Year(i16),
    #[error("profile describes no CMYK output device")]
    Intent,
    #[error("{0} needs output the writer cannot produce")]
    Press(PdfXLevel),
    #[error("{press} and {accessibility} require different PDF versions")]
    Versions { press: PdfXLevel, accessibility: PdfUaLevel },
    #[error("document claims {family} conformance {claim}, which no validator checks")]
    Claim { family: Family, claim: Arc<str> },
    #[error("page label range at page {0} numbers from zero, where ISO 32000 starts at one")]
    Numbering(usize),
    #[error("timestamp authority answered {}", fmt::from_fn(|formatter| match .0 {
        Some(status) => write!(formatter, "status {}", status.discriminant()),
        None => formatter.write_str("no RFC 3161 token"),
    }))]
    Timestamp(Option<PkiStatus>),
    #[error("timestamp token stamps other bytes")]
    Imprint,
    #[error("raster of {width} by {height} pixels exceeds addressable memory")]
    Raster { width: u64, height: u64 },
    #[error("refused{}", fmt::from_fn(|formatter| .0.iter().try_for_each(|(subject, error)| write!(formatter, "\n{subject}: {}", Report::new(error)))))]
    Refused(NEVec<(Subject, Self)>),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn save(document: &Document, options: SaveOptions) -> Result<Vec<u8>, PdfError> {
    match options {
        SaveOptions { incremental: true, compress: _, linearize: false, garbage_collect: _, encryption: None } => Ok(document.data().as_ref().to_vec()),
        rewrite => editor(document).and_then(|mut editor| editor.save_to_bytes_with_options(rewrite).map_err(PdfError::from)),
    }
}

pub fn fields(document: &Document) -> Result<Vec<Fields>, PdfError> {
    let structure = structure(document)?;
    let path = [PIECE_INFO, env!("CARGO_PKG_NAME").as_bytes(), PRIVATE];
    let entry = |(key, value): (&Vec<u8>, &Object)| {
        Validated::from(str::from_utf8(key).map(Arc::from).map_err(PdfError::from))
            .map2(Validated::from(resolved(&structure, value).and_then(decode_text_string).map(Arc::from).map_err(PdfError::from)), |key, value| (key, value))
    };
    structure
        .page_iter()
        .enumerate()
        .map(|(index, page)| {
            structure
                .get_dictionary(page)
                .and_then(|sheet| nested(&structure, sheet, &path))
                .map_or_else(|error| Validated::fail(PdfError::from(error)), |found| found.into_iter().flat_map(Dictionary::iter).map(entry).collect::<Validated<Fields, _>>())
                .map_err(|errors| errors.into_nonempty_iter().map(|error| (Subject::Page(index), error)).collect())
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(PdfError::Refused)
}

pub fn set_fields<I, F>(document: &Document, pages: I, modified: &Zoned) -> Result<Document, PdfError>
where
    I: IntoIterator<Item = (usize, F)>,
    F: Borrow<Fields>,
{
    let mut structure = structure(document)?;
    let sheets = structure.page_iter().collect::<Vec<_>>();
    let placed = pages
        .into_iter()
        .map(|(index, fields)| sheets.get(index).map(|&sheet| (sheet, fields)).ok_or((Subject::Page(index), PdfError::Page { index, count: sheets.len() })))
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(PdfError::Refused)?;
    placed.iter().try_for_each(|(sheet, fields)| write_fields(&mut structure, *sheet, fields.borrow(), modified))?;
    saved(structure)
}

fn structure(document: &Document) -> Result<lopdf::Document, PdfError> { lopdf::Document::load_mem(document.data().as_ref()).map_err(PdfError::from) }

fn engine(document: &Document) -> Result<PdfDocument, PdfError> { PdfDocument::from_bytes(document.data().as_ref().to_vec()).map_err(PdfError::from) }

fn editor(document: &Document) -> Result<DocumentEditor, PdfError> { DocumentEditor::from_bytes(document.data().as_ref().to_vec()).map_err(PdfError::from) }

fn saved(mut structure: lopdf::Document) -> Result<Document, PdfError> {
    let mut bytes = Vec::new();
    structure.save_to(&mut bytes).map_err(lopdf::Error::from)?;
    Document::from_plain_bytes(bytes)
}

fn extent(size: Size) -> Result<geom::Size, PdfError> { geom::Size::from_wh(size.width.cast::<f32>(), size.height.cast::<f32>()).ok_or(PdfError::Area(size.to_rect())) }

fn embedded(document: &Document) -> Result<pdf::PdfDocument, PdfError> {
    pdf::Pdf::new(Arc::new(document.data().clone())).ok().map(|source| pdf::PdfDocument::new(Arc::new(source))).ok_or(PdfError::Load(LoadPdfError::Invalid))
}

fn page(document: &Document, index: usize) -> Result<&Page<'_>, PdfError> {
    let pages = document.pages();
    pages.get(index).ok_or(PdfError::Page { index, count: pages.len() })
}

fn points_per_millimeter() -> f64 {
    const POINTS_PER_INCH: f64 = 72.0;
    Length::new::<millimeter>(1.0).get::<inch>() * POINTS_PER_INCH
}

fn frame(page: &Page<'_>) -> (Affine, Size) {
    let unit = page.raw().get::<f64>(USER_UNIT).filter(|unit| unit.is_normal() && unit.is_sign_positive()).unwrap_or(1.0) / points_per_millimeter();
    let (width, height) = page.render_dimensions();
    (Affine::scale(unit) * page.initial_transform(true).to_kurbo(), Size::new(f64::from(width) * unit, f64::from(height) * unit))
}

fn resolved<'held>(structure: &'held lopdf::Document, object: &'held Object) -> Result<&'held Object, lopdf::Error> {
    static NULL: Object = Object::Null;
    match structure.dereference(object) {
        Err(lopdf::Error::ObjectNotFound(_)) => Ok(&NULL),
        found => found.map(|(_, target)| target),
    }
}

fn entry<'held>(structure: &'held lopdf::Document, holder: &'held Dictionary, key: impl AsRef<[u8]>) -> Result<Option<&'held Object>, lopdf::Error> {
    Ok(holder.as_hashmap().get(key.as_ref()).map(|value| resolved(structure, value)).transpose()?.filter(|value| !value.is_null()))
}

fn nested<'held>(structure: &'held lopdf::Document, holder: &'held Dictionary, path: &[&[u8]]) -> Result<Option<&'held Dictionary>, lopdf::Error> {
    path.iter().try_fold(Some(holder), |found, key| found.map(|parent| entry(structure, parent, key)?.map(Object::as_dict).transpose()).transpose().map(Option::flatten))
}

fn leaves<K, F>(structure: &lopdf::Document, root: &Dictionary, list: &str, key: &F) -> Result<Vec<(K, Object)>, lopdf::Error>
where F: Fn(&Object) -> Result<K, lopdf::Error> {
    enum Step<'node> {
        Node(&'node Dictionary),
        Child(&'node Object),
        Leave(ObjectId),
    }
    let mut found = Vec::new();
    let mut active = HashSet::new();
    let mut pending = vec![Step::Node(root)];
    while let Some(step) = pending.pop() {
        match step {
            Step::Node(node) => {
                entry(structure, node, list)?
                    .map(Object::as_array)
                    .transpose()?
                    .into_iter()
                    .flat_map(|pairs| pairs.as_chunks::<2>().0)
                    .try_for_each(|[name, value]| key(name).map(|name| found.push((name, value.clone()))))?;
                pending.extend(entry(structure, node, b"Kids")?.map(Object::as_array).transpose()?.into_iter().flatten().rev().map(Step::Child));
            }
            Step::Child(child) => {
                let id = child.as_reference()?;
                active.insert(id).ok_or(lopdf::Error::ReferenceCycle(id))?;
                pending.extend([Step::Leave(id), Step::Node(structure.get_dictionary(id)?)]);
            }
            Step::Leave(id) => {
                active.remove(&id);
            }
        }
    }
    Ok(found)
}

fn write_fields(structure: &mut lopdf::Document, page: ObjectId, fields: &Fields, modified: &Zoned) -> Result<(), lopdf::Error> {
    let application = env!("CARGO_PKG_NAME").as_bytes();
    let stamp = || Object::from(modified.clone());
    let mut pieces = nested(structure, structure.get_dictionary(page)?, &[PIECE_INFO])?.cloned().unwrap_or_default();
    let written = (!fields.is_empty()).then(|| dictionary! { LAST_MODIFIED => stamp(), PRIVATE => fields.iter().map(|(key, value)| (key.as_bytes(), text_string(value))).collect::<Dictionary>() });
    if pieces.remove(application).is_none() && written.is_none() {
        return Ok(());
    }
    if let Some(data) = written {
        pieces.set(application, data);
    }
    let sheet = structure.get_dictionary_mut(page)?;
    if pieces.is_empty() {
        sheet.remove(PIECE_INFO);
    } else {
        sheet.set(PIECE_INFO, pieces);
    }
    sheet.set(LAST_MODIFIED, stamp());
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
