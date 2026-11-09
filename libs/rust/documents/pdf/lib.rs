#![feature(array_try_map, default_field_values, deref_patterns, error_reporter, float_conversions, gen_blocks, integer_casts, iterator_try_collect)]

use core::{borrow::Borrow, cell::LazyCell, fmt, num::NonZero, str::Utf8Error};
use std::{
    collections::{BTreeMap, HashSet},
    error::Report,
    sync::Arc,
};

use hayro::hayro_interpret::TransformExt as _;
use hayro_syntax::{
    DecryptionError, LoadPdfError, Pdf,
    object::dict::keys::{ID, KIDS, LAST_MODIFIED, PIECE_INFO, PRIVATE, STD_CF, USER_UNIT},
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
use lopdf::{
    Dictionary, EncryptionState, EncryptionVersion, IncrementalDocument, LoadOptions, Object, ObjectId, Permissions, StringFormat, decode_text_string, dictionary,
    encryption::crypt_filters::{Aes128CryptFilter, Aes256CryptFilter, CryptFilter},
    text_string,
};
use moxcms::{CmsError, ColorProfile, DataColorSpace, ProfileClass};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use pdf_oxide::{
    PdfDocument,
    compliance::{PdfALevel, PdfUaLevel, PdfXLevel, XComplianceError},
    editor::{DocumentEditor, EncryptionAlgorithm, EncryptionConfig, SaveOptions},
    encryption::generate_file_id,
    ocr::OcrError,
};
use rand::{CryptoRng, RngExt as _};
use rasm_drawing::{LineworkError, WidthError};
use rasn::{
    error::{DecodeError, EncodeError},
    types::Enumerated as _,
};
use rasn_cms::tsp::PkiStatus;
use secrecy::{ExposeSecret as _, SecretString};
use uom::si::{
    f64::{Length, LinearNumberDensity},
    length::point_computer,
    ratio::ratio,
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
pub struct Document {
    #[deref(forward)]
    #[debug(skip)]
    pdf: Arc<Pdf>,
    #[debug(skip)]
    password: Option<SecretString>,
}

impl Document {
    pub fn new(bytes: Vec<u8>, password: Option<&SecretString>) -> Result<Self, PdfError> {
        Pdf::new_with_password(bytes, password.map_or("", |secret| secret.expose_secret())).map(|pdf| Self { pdf: Arc::new(pdf), password: password.cloned() }).map_err(PdfError::Load)
    }
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
    Named {
        name: Arc<str>,
        refusal: Refusal,
    },
    #[error("page {index} lies past the {count} pages of the document")]
    Page {
        index: usize,
        count: usize,
    },
    #[error("area of {} by {} encloses nothing", .0.width(), .0.height())]
    Area(Rect),
    #[error("font data of {family} holds no face {index}")]
    Font {
        family: Arc<str>,
        index: u32,
    },
    #[error("document holds no optional content group {} {} R", .0.0, .0.1)]
    Group(ObjectId),
    #[error("PDF metadata cannot represent year {0}")]
    Year(i16),
    #[error("profile describes no CMYK output device")]
    Intent,
    #[error("{0} needs output the writer cannot produce")]
    Press(PdfXLevel),
    #[error("document claims {family} conformance {claim}, which no validator checks")]
    Claim {
        family: Family,
        claim: Arc<str>,
    },
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
    Raster {
        width: u64,
        height: u64,
    },
    #[error("document is encrypted, and the signer appends its signature dictionary unencrypted")]
    Encrypted,
    #[error("document holds {0} signatures, and filling rewrites the whole file, which invalidates them")]
    Signed(NonZero<usize>),
    #[error("{level} validator refused the claim{}", fmt::from_fn(|formatter| errors.iter().try_for_each(|error| write!(formatter, "\n{error}"))))]
    Nonconforming {
        level: PdfXLevel,
        errors: Vec<XComplianceError>,
    },
    #[error("refused{}", fmt::from_fn(|formatter| .0.iter().try_for_each(|(subject, error)| write!(formatter, "\n{subject}: {}", Report::new(error)))))]
    Refused(NEVec<(Subject, Self)>),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn save<R>(document: &Document, options: SaveOptions, generator: &mut R) -> Result<Vec<u8>, PdfError>
where R: CryptoRng + ?Sized {
    match &options {
        SaveOptions { incremental: true, compress: false, linearize: false, garbage_collect: false, encryption: None } => Ok(document.data().as_ref().to_vec()),
        SaveOptions { encryption: None, .. } => Ok(editor(document)?.save_to_bytes_with_options(options)?),
        SaveOptions { encryption: Some(EncryptionConfig { user_password, owner_password, algorithm, permissions }), .. } => {
            let key = LazyCell::new(|| generator.random::<[u8; 32]>());
            let permissions = Permissions::from_bits_truncate(u64::from(permissions.to_bits().cast_unsigned()));
            let filters = |filter: Arc<dyn CryptFilter>| BTreeMap::from([(STD_CF.to_vec(), filter)]);
            encrypted(&editor(document)?.save_to_bytes_with_options(SaveOptions { encryption: None, ..options })?, None, |structure| {
                EncryptionState::try_from(match algorithm {
                    EncryptionAlgorithm::Rc4_40 => EncryptionVersion::V1 { document: structure, owner_password, user_password, permissions },
                    EncryptionAlgorithm::Rc4_128 => EncryptionVersion::V2 { document: structure, owner_password, user_password, key_length: 128, permissions },
                    EncryptionAlgorithm::Aes128 => EncryptionVersion::V4 {
                        document: structure,
                        encrypt_metadata: true,
                        crypt_filters: filters(Arc::new(Aes128CryptFilter)),
                        stream_filter: STD_CF.to_vec(),
                        string_filter: STD_CF.to_vec(),
                        owner_password,
                        user_password,
                        permissions,
                    },
                    EncryptionAlgorithm::Aes256 => EncryptionVersion::V5 {
                        encrypt_metadata: true,
                        crypt_filters: filters(Arc::new(Aes256CryptFilter)),
                        file_encryption_key: &*key,
                        stream_filter: STD_CF.to_vec(),
                        string_filter: STD_CF.to_vec(),
                        owner_password,
                        user_password,
                        permissions,
                    },
                })
            })
        }
    }
}

pub fn fields(document: &Document, application: &str) -> Result<Vec<Fields>, PdfError> {
    let structure = structure(document)?;
    let path = [PIECE_INFO, application.as_bytes(), PRIVATE];
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

pub fn set_fields<I, F>(document: &Document, application: &str, pages: I, modified: &Zoned) -> Result<Document, PdfError>
where
    I: IntoIterator<Item = (usize, F)>,
    F: Borrow<Fields>,
{
    revised(document, |structure| {
        let sheets = structure.page_iter().collect::<Vec<_>>();
        let placed = pages
            .into_iter()
            .map(|(index, fields)| sheets.get(index).map(|&sheet| (sheet, fields)).ok_or((Subject::Page(index), PdfError::Page { index, count: sheets.len() })))
            .collect::<Validated<Vec<_>, _>>()
            .ok()
            .map_err(PdfError::Refused)?;
        placed.iter().try_for_each(|(sheet, fields)| write_fields(structure, application, *sheet, fields.borrow(), modified)).map_err(PdfError::from)
    })
}

fn structure(document: &Document) -> Result<lopdf::Document, PdfError> {
    lopdf::Document::load_mem_with_options(document.data().as_ref(), LoadOptions { password: document.password.as_ref().map(|secret| secret.expose_secret().to_owned()), ..LoadOptions::default() })
        .map_err(PdfError::from)
}

fn plain(document: &Document) -> Result<Vec<u8>, PdfError> {
    if lopdf::Document::load_metadata_mem(document.data().as_ref())?.encrypted {
        let mut bytes = Vec::new();
        structure(document)?.save_to(&mut bytes).map_err(lopdf::Error::from)?;
        Ok(bytes)
    } else {
        Ok(document.data().as_ref().to_vec())
    }
}

fn engine(document: &Document) -> Result<PdfDocument, PdfError> { PdfDocument::from_bytes(plain(document)?).map_err(PdfError::from) }

fn editor(document: &Document) -> Result<DocumentEditor, PdfError> { DocumentEditor::from_bytes(plain(document)?).map_err(PdfError::from) }

fn saved(mut structure: lopdf::Document) -> Result<Document, PdfError> {
    let mut bytes = Vec::new();
    structure.save_to(&mut bytes).map_err(lopdf::Error::from)?;
    Document::new(bytes, None)
}

fn revised(document: &Document, edit: impl FnOnce(&mut lopdf::Document) -> Result<(), PdfError>) -> Result<Document, PdfError> {
    let prior = structure(document)?;
    let mut edited = prior.clone();
    edit(&mut edited)?;
    let changed = edited.objects.into_iter().filter(|(id, object)| prior.objects.get(id) != Some(object)).collect::<Vec<_>>();
    let mut update = IncrementalDocument::create_from(document.data().as_ref().to_vec(), prior);
    update.new_document.objects.extend(changed);
    update.new_document.max_id = edited.max_id;
    let mut bytes = Vec::new();
    update.save_to(&mut bytes).map_err(lopdf::Error::from)?;
    Document::new(bytes, document.password.as_ref())
}

fn sealed(document: &Document, bytes: Vec<u8>) -> Result<Document, PdfError> {
    let source = structure(document)?;
    match source.encryption_state {
        None => Document::new(bytes, None),
        Some(state) => Document::new(encrypted(&bytes, source.trailer.as_hashmap().get(ID), |_| Ok(state))?, document.password.as_ref()),
    }
}

fn encrypted(bytes: &[u8], identifier: Option<&Object>, state: impl FnOnce(&lopdf::Document) -> Result<EncryptionState, lopdf::Error>) -> Result<Vec<u8>, PdfError> {
    let mut structure = lopdf::Document::load_mem(bytes)?;
    structure.trailer.set(ID, identifier.cloned().unwrap_or_else(|| Object::Array(<[_; 2]>::from(generate_file_id()).map(|part| Object::String(part, StringFormat::Hexadecimal)).into())));
    let state = state(&structure)?;
    structure.encrypt(&state)?;
    let mut encrypted = Vec::new();
    structure.save_to(&mut encrypted).map_err(lopdf::Error::from)?;
    Ok(encrypted)
}

fn extent(size: Size) -> Result<geom::Size, PdfError> { geom::Size::from_wh(size.width.cast::<f32>(), size.height.cast::<f32>()).ok_or(PdfError::Area(size.to_rect())) }

fn embedded(document: &Document) -> Result<pdf::PdfDocument, PdfError> {
    pdf::Pdf::new(Arc::new(plain(document)?)).ok().map(|source| pdf::PdfDocument::new(Arc::new(source))).ok_or(PdfError::Load(LoadPdfError::Invalid))
}

fn page(document: &Document, index: usize) -> Result<&Page<'_>, PdfError> {
    let pages = document.pages();
    pages.get(index).ok_or(PdfError::Page { index, count: pages.len() })
}

fn per_point(resolution: Resolution) -> f64 { (Length::new::<point_computer>(1.0) * *resolution).get::<ratio>() }

fn frame(page: &Page<'_>) -> (Affine, Size) {
    let unit = page.raw().get::<f64>(USER_UNIT).filter(|unit| unit.is_normal() && unit.is_sign_positive()).unwrap_or(1.0);
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

fn leaves<K, F>(structure: &lopdf::Document, root: &Dictionary, list: &[u8], key: &F) -> Result<Vec<(K, Object)>, lopdf::Error>
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
                pending.extend(entry(structure, node, KIDS)?.map(Object::as_array).transpose()?.into_iter().flatten().rev().map(Step::Child));
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

fn write_fields(structure: &mut lopdf::Document, application: &str, page: ObjectId, fields: &Fields, modified: &Zoned) -> Result<(), lopdf::Error> {
    let application = application.as_bytes();
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

mod authoring;
mod conformance;
mod editing;
mod extraction;
mod layers;
mod measurement;
mod navigation;
mod proofs;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use authoring::{Options, Press, Standards, author, scan};
pub use color;
pub use conformance::{Verdict, claims, convert, validate};
pub use editing::{Authority, Finish, Level, Part, Redaction, assemble, fill, form_fields, overlay, redact, sign, split};
pub use extraction::{Paths, extract, paths, recognize};
pub use fast_image_resize;
pub use fontdb;
pub use hayro;
pub use hayro_syntax;
pub use image;
pub use image_compare;
pub use jiff;
pub use krilla;
pub use kurbo;
pub use layers::{Group, groups, set};
pub use lopdf;
pub use measurement::{Measure, locate, measures};
pub use moxcms;
pub use navigation::{Navigation, label, read};
pub use nonempty_collections;
pub use pdf_oxide;
pub use proofs::{Appearance, Raster, compare, render, svg};
pub use rand;
pub use rasm_drawing;
pub use rasn;
pub use rasn_cms;
pub use reqwest;
pub use secrecy;
pub use tiff;
pub use uom;
pub use usvg;
pub use xot;
