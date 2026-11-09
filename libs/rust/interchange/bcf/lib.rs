#![feature(deref_patterns)]

use core::{
    cell::RefCell,
    convert,
    fmt::{self, Write as _},
    iter,
    num::{ParseFloatError, ParseIntError, TryFromIntError},
    str::FromStr,
};
use std::{
    collections::HashMap,
    io::{Read, Seek, Write},
};

use data_encoding::{DecodeError, Encoding, HEXLOWER};
use enum_map::EnumMap;
use imagesize::{ImageError, ImageType};
use indexmap::{IndexMap, IndexSet};
use itertools::Either;
use jiff::{Timestamp, civil};
use newtype_uuid_macros::impl_typed_uuid_kinds;
use nonempty_collections::{NEVec, nev};
use sha2::{Digest as _, Sha256, digest::Output};
use url::Url;
use validated::Validated;
use xot::{Node, Xot};
use zip::{CompressionMethod, ZIP64_BYTES_THR, ZipArchive, ZipWriter, result::ZipError, write::SimpleFileOptions};

use self::xml::{Codec, Keyed, Reader, Simple, Upgrade, Writer, archive_error, codec, document, holds_text, normalized, required, resolve, stored, unique};

// --- [TYPES] ---------------------------------------------------------------------------

impl_typed_uuid_kinds! {
    kinds = {
        Topic = {},
        Comment = {},
        Viewpoint = {},
        Document = {},
        DocumentReference = {},
    },
}

pub type DateTime = Either<Timestamp, civil::DateTime>;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::From, derive_more::Display)]
pub enum Guid {
    #[display("topic {_0}")]
    Topic(TopicUuid),
    #[display("comment {_0}")]
    Comment(CommentUuid),
    #[display("viewpoint {_0}")]
    Viewpoint(ViewpointUuid),
    #[display("document {_0}")]
    Document(DocumentUuid),
    #[display("document reference {_0}")]
    DocumentReference(DocumentReferenceUuid),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, enum_map::Enum, strum::IntoStaticStr)]
pub enum Extension {
    TopicType,
    TopicStatus,
    Priority,
    TopicLabel,
    User,
    SnippetType,
    Stage,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const IFC_GUID: Encoding = data_encoding_macro::new_encoding! {
    symbols: "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_$",
};
const GUID: &str = "Guid";
const TOPIC: &str = "Topic";
const FILE: &str = "File";
const CENTRAL_DIRECTORY: &str = "central directory";
const VERSION: (&str, &str) = ("bcf.version", "Version");
const EXTENSIONS: (&str, &str) = ("extensions.xml", "Extensions");
const PROJECT_INFO: (&str, &str) = ("project.bcfp", "ProjectInfo");
const DOCUMENT_INFO: (&str, &str) = ("documents.xml", "DocumentInfo");
const MARKUP: (&str, &str) = ("markup.bcf", "Markup");
const VERSION_ID: &str = "VersionId";
const RELEASE: &str = "3.0";
const PROJECT: &str = "Project";
const DOCUMENTS: &str = "Documents";
const DOCUMENT: &str = "Document";
const FILENAME: &str = "Filename";
const DESCRIPTION: &str = "Description";
const IS_EXTERNAL: &str = "IsExternal";
const REFERENCE: &str = "Reference";
const ORTHOGONAL_CAMERA: &str = "OrthogonalCamera";
const PERSPECTIVE_CAMERA: &str = "PerspectiveCamera";
const ASPECT_RATIO: &str = "AspectRatio";

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(
    sanitize(trim),
    validate(with = |text: &str| -> Result<(), BcfError> { (!text.is_empty()).ok_or(BcfError::Constrained(Constraint::Text)) }, error = BcfError),
    derive(Debug, Clone, PartialEq, Eq, Hash, PartialOrd, Ord, Display, AsRef, Deref, Borrow, FromStr, TryFrom, Into)
)]
pub struct Text(String);

impl Simple for Text {
    fn decode(text: &str) -> Result<Self, BcfError> { Self::try_new(text) }

    fn encode(&self) -> String { String::clone(self) }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::From, derive_more::Into)]
pub struct IfcGuid(Uuid);

impl IfcGuid {
    pub const LENGTH: usize = 22;
}

impl FromStr for IfcGuid {
    type Err = BcfError;

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        if s.len() != Self::LENGTH {
            return s.parse().map(Self).map_err(|source| BcfError::Uuid { text: s.into(), source });
        }
        let mut input = [b'0'; 24];
        input[2..].copy_from_slice(s.as_bytes());
        let mut bytes = [0; 18];
        IFC_GUID.decode_mut(&input, &mut bytes).map_err(|source| BcfError::Encoded { text: s.into(), alphabet: Alphabet::IfcGuid, source: source.error })?;
        match bytes {
            [0, 0, uuid @ ..] => Ok(Self(Uuid::from_bytes(uuid))),
            _ => Err(BcfError::Constrained(Constraint::IfcGuid(s.into()))),
        }
    }
}

impl fmt::Display for IfcGuid {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let mut bytes = [0; 18];
        bytes[2..].copy_from_slice(self.0.as_bytes());
        IFC_GUID.encode_mut_str(&bytes, &mut [0; 24]).chars().skip(2).try_for_each(|symbol| f.write_char(symbol))
    }
}

impl Simple for IfcGuid {
    fn decode(text: &str) -> Result<Self, BcfError> { text.parse() }

    fn encode(&self) -> String { self.to_string() }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, strum::IntoStaticStr)]
#[strum(serialize_all = "lowercase")]
pub enum ImageFormat {
    Png,
    Jpg,
}

#[derive(Debug, Clone, PartialEq, Eq, derive_more::AsRef)]
pub struct Image {
    #[as_ref]
    format: ImageFormat,
    #[as_ref([u8])]
    content: Box<[u8]>,
}

impl TryFrom<Box<[u8]>> for Image {
    type Error = Box<[u8]>;

    fn try_from(value: Box<[u8]>) -> Result<Self, Self::Error> {
        match imagesize::image_type(&value) {
            Ok(ImageType::Png) => Ok(Self { format: ImageFormat::Png, content: value }),
            Ok(ImageType::Jpeg) => Ok(Self { format: ImageFormat::Jpg, content: value }),
            Ok(_) | Err(ImageError::NotSupported | ImageError::CorruptedImage | ImageError::IoError(_)) => Err(value),
        }
    }
}

impl Codec for Image {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, BcfError> {
        let text = reader.xot.string_value(node);
        Validated::from(
            reader
                .file(text.trim())
                .and_then(|content| Self::try_from(content).map_err(|_: Box<[u8]>| reader.local_name(node).map_or_else(convert::identity, |element| reader.refused(element, Some(text.trim()))))),
        )
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), BcfError> {
        let Self { format, content } = self;
        let held = Sha256::digest(content);
        let name = format!("{}.{}", HEXLOWER.encode_display(held.as_ref()), <&str>::from(format));
        writer.xot.append_text(node, &name).map_err(writer.xml())?;
        (writer.store)(&resolve(writer.entry, &name), content, held, CompressionMethod::Stored)
    }

    fn present(xot: &Xot, node: Node) -> bool { holds_text(xot, node) }
}

#[derive(Debug, Clone, PartialEq, Default, derive_more::AsRef)]
pub struct Issues {
    #[as_ref]
    project: Option<Project>,
    #[as_ref]
    extensions: EnumMap<Extension, IndexSet<Text>>,
    #[as_ref]
    documents: IndexMap<DocumentUuid, Document>,
    #[as_ref]
    topics: IndexMap<TopicUuid, Topic>,
}

impl Issues {
    pub fn new(
        project: Option<Project>,
        extensions: EnumMap<Extension, IndexSet<Text>>,
        documents: IndexMap<DocumentUuid, Document>,
        topics: IndexMap<TopicUuid, Topic>,
    ) -> Result<Self, NEVec<BcfError>> {
        let dangling = topics
            .iter()
            .flat_map(|(&topic, body)| {
                let viewpoints = body
                    .comments
                    .values()
                    .filter_map(|comment| comment.remark.as_ref().right())
                    .filter(|guid| !body.viewpoints.contains_key(*guid))
                    .map(move |&guid| BcfError::Dangling { topic, guid: guid.into() });
                let references = body
                    .document_references
                    .values()
                    .filter_map(|reference| reference.target.as_ref().left())
                    .filter(|guid| !documents.contains_key(*guid))
                    .map(move |&guid| BcfError::Dangling { topic, guid: guid.into() });
                viewpoints.chain(references)
            })
            .collect::<Vec<_>>();
        NEVec::try_from_vec(dangling).map_or(Ok(Self { project, extensions, documents, topics }), Err)
    }
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Project {
    pub id: Text,
    pub name: Option<Text>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Document {
    pub filename: Text,
    pub description: Option<Text>,
    pub content: Box<[u8]>,
}

codec! {
    Project {
        attribute id(&[], "ProjectId"),
        element name(&[], "Name"),
    }
}

impl Codec for EnumMap<Extension, IndexSet<Text>> {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, BcfError> {
        EnumMap::from_fn(|kind| {
            let [plural, _] = names(kind);
            reader.element(node, &[plural], kind.into()).map(|values| (kind, values))
        })
        .into_values()
        .collect()
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), BcfError> {
        self.iter().try_for_each(|(kind, values)| {
            let [plural, _] = names(kind);
            writer.element(node, &[plural], kind.into(), values)
        })
    }
}

impl Keyed<DocumentUuid> for Document {
    fn read_keyed(reader: &Reader<'_>, node: Node, key: DocumentUuid) -> Validated<Self, BcfError> {
        reader.element(node, &[], FILENAME).map3(reader.element(node, &[], DESCRIPTION), Validated::from(reader.file(&document_entry(key))), |filename, description, content| Self {
            filename,
            description,
            content,
        })
    }

    fn write_keyed(&self, key: DocumentUuid, writer: &mut Writer<'_>, node: Node) -> Result<(), BcfError> {
        let Self { filename, description, content } = self;
        writer.element(node, &[], FILENAME, filename)?;
        writer.element(node, &[], DESCRIPTION, description)?;
        writer.file(&document_entry(key), content, CompressionMethod::Deflated)
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, derive_more::Display)]
pub enum Constraint {
    #[display("number {_0} is not finite")]
    Finite(f64),
    #[display("length {_0} m is not positive")]
    Length(f64),
    #[display("aspect ratio {_0} is not positive")]
    AspectRatio(f64),
    #[display("field of view {_0}° lies outside 0° to 180°")]
    FieldOfView(f64),
    #[display("BCF text is empty")]
    Text,
    #[display("{_0} is no boolean")]
    Boolean(Box<str>),
    #[display("color holds {_0} bytes")]
    Color(usize),
    #[display("IfcGuid {_0} holds more than 128 bits")]
    IfcGuid(Box<str>),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Alphabet {
    #[display("IfcGuid")]
    IfcGuid,
    #[display("hexadecimal")]
    Hexadecimal,
}

#[derive(Debug, thiserror::Error)]
pub enum BcfError {
    #[error("{0}")]
    Constrained(Constraint),
    #[error("{text} is no number")]
    Number { text: Box<str>, source: ParseFloatError },
    #[error("{text} is no integer")]
    Integer { text: Box<str>, source: ParseIntError },
    #[error("{text} is no timestamp")]
    Timestamp { text: Box<str>, source: jiff::Error },
    #[error("{text} is no UUID")]
    Uuid { text: Box<str>, source: uuid::Error },
    #[error("{text} is no URI")]
    Uri { text: Box<str>, source: url::ParseError },
    #[error("{text} is no {alphabet} text")]
    Encoded { text: Box<str>, alphabet: Alphabet, source: DecodeError },
    #[error("BCF entry {entry} exceeds the ZIP64 size range")]
    Size { entry: Box<str>, source: TryFromIntError },
    #[error("BCF {found} is no BCF release this crate reads")]
    Version { found: Box<str> },
    #[error("BCF entry {entry} is unreadable")]
    Archive { entry: Box<str>, source: ZipError },
    #[error("BCF entry {entry} holds malformed XML")]
    Xml { entry: Box<str>, source: xot::Error },
    #[error("BCF entry {entry} holds {element} as {}", .text.as_deref().unwrap_or("nothing"))]
    Element { entry: Box<str>, element: Box<str>, text: Option<Box<str>> },
    #[error("BCF entry {entry} holds a malformed {element}")]
    Content { entry: Box<str>, element: Box<str>, source: Box<Self> },
    #[error("BCF entry {entry} repeats {guid}")]
    Duplicate { entry: Box<str>, guid: Guid },
    #[error("BCF topic {topic} names absent {guid}")]
    Dangling { topic: TopicUuid, guid: Guid },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ARCHIVES]

pub fn read<R>(source: R) -> Result<Issues, NEVec<BcfError>>
where R: Read + Seek {
    const UPGRADED_RELEASE: &str = "2.1";
    let zip = ZipArchive::new(source).map_err(|source| nev![archive_error(CENTRAL_DIRECTORY)(source)])?;
    let entries = zip
        .file_names()
        .enumerate()
        .map(|(index, name)| name.map(|raw| (normalized(&raw).to_ascii_lowercase(), index)).map_err(archive_error(CENTRAL_DIRECTORY)))
        .collect::<Validated<IndexMap<_, _>, _>>()
        .ok()?;
    let zip = RefCell::new(zip);
    let open = |entry: &str| -> Result<Box<[u8]>, ZipError> {
        let index = *entries.get(&entry.to_ascii_lowercase()).ok_or(ZipError::FileNotFound)?;
        let mut archive = zip.borrow_mut();
        let mut file = archive.by_index(index)?;
        let mut content = Vec::new();
        file.read_to_end(&mut content)?;
        Ok(content.into_boxed_slice())
    };
    let found = required(document(&open, None, VERSION, |reader, root| reader.attribute::<Text>(root, &[], VERSION_ID)), VERSION.0).ok()?;
    let upgraded = Upgrade::default();
    let upgrade = match found.as_str() {
        RELEASE => None,
        UPGRADED_RELEASE => Some(&upgraded),
        _ => return Err(nev![BcfError::Version { found: found.into_inner().into_boxed_str() }]),
    };
    let topics = entries
        .keys()
        .filter(|name| name.split_once('/').is_some_and(|(_, file)| file == MARKUP.0))
        .map(|entry| {
            required(
                document(&open, upgrade, (entry, MARKUP.1), |reader, root| reader.attribute::<TopicUuid>(root, &[TOPIC], GUID).map2(Topic::read(reader, root), |guid, topic| (guid, topic))),
                entry,
            )
        })
        .collect::<Validated<Vec<_>, _>>();
    let listed = |schema: &Text| -> Validated<EnumMap<Extension, IndexSet<Text>>, BcfError> {
        let mut xot = Xot::new();
        let (name, value, simple_type, enumeration) = (xot.add_name("name"), xot.add_name("value"), xot.add_name("simpleType"), xot.add_name("enumeration"));
        let parsed = match Url::parse(schema) {
            Ok(_) => Ok(None),
            Err(url::ParseError::RelativeUrlWithoutBase) => {
                let entry = resolve(PROJECT_INFO.0, schema);
                open(&entry).map_err(archive_error(&entry)).and_then(|bytes| xot.parse_bytes(&bytes).map(Some).map_err(|source| BcfError::Xml { entry: entry.as_str().into(), source: source.into() }))
            }
            Err(source) => Err(BcfError::Uri { text: schema.as_str().into(), source }),
        };
        parsed.map_or_else(Validated::fail, |root| {
            EnumMap::from_fn(|kind| {
                let [_, restricted] = names(kind);
                root.into_iter()
                    .flat_map(|root| xot.descendants(root))
                    .filter(|&node| xot.node_name(node) == Some(simple_type) && xot.get_attribute(node, name) == Some(restricted))
                    .flat_map(|node| xot.descendants(node))
                    .filter(|&node| xot.node_name(node) == Some(enumeration))
                    .filter_map(|node| xot.get_attribute(node, value))
                    .map(|text| Validated::from(Text::try_new(text)))
                    .collect::<Validated<IndexSet<_>, _>>()
                    .map(|values| (kind, values))
            })
            .into_values()
            .collect()
        })
    };
    let project = document(&open, upgrade, PROJECT_INFO, |reader, root| {
        reader.element::<Option<Project>>(root, &[], PROJECT).map2(reader.element::<Option<Text>>(root, &[], "ExtensionSchema"), |project, schema| (project, schema))
    })
    .ok()
    .map_or_else(Validated::Fail, |info| {
        let (project, schema) = info.unwrap_or_default();
        schema.map_or(Validated::Good(EnumMap::default()), |schema| listed(&schema)).map(|listed| (project, listed))
    });
    let read = project
        .map4(
            document(&open, upgrade, EXTENSIONS, EnumMap::read),
            document(&open, upgrade, DOCUMENT_INFO, |reader, root| reader.keyed(root, &[DOCUMENTS], DOCUMENT, None)),
            topics.ok().map_or_else(Validated::Fail, |held| unique(CENTRAL_DIRECTORY, held)),
            |(project, mut listed), extensions, documents, topics| {
                extensions.into_iter().flatten().for_each(|(kind, values)| listed[kind].extend(values));
                (project, listed, documents.unwrap_or_default(), topics)
            },
        )
        .ok();
    read.and_then(|(project, extensions, mut documents, topics)| {
        documents.extend(upgraded.into_inner());
        Issues::new(project, extensions, documents, topics)
    })
}

pub fn write<W>(issues: &Issues, sink: W) -> Result<W, BcfError>
where W: Write + Seek {
    let Issues { project, extensions, documents, topics } = issues;
    let zip = RefCell::new(ZipWriter::new(sink));
    let written = RefCell::new(HashMap::<String, Output<Sha256>>::new());
    let store = |entry: &str, content: &[u8], held: Output<Sha256>, method: CompressionMethod| -> Result<(), BcfError> {
        match written.borrow_mut().insert(entry.to_owned(), held) {
            Some(previous) if previous == held => Ok(()),
            Some(_) | None => {
                let large = u64::try_from(content.len()).map_err(|source| BcfError::Size { entry: entry.into(), source })? >= ZIP64_BYTES_THR;
                let mut archive = zip.borrow_mut();
                archive.start_file(entry, SimpleFileOptions::DEFAULT.compression_method(method).large_file(large)).map_err(archive_error(entry))?;
                archive.write_all(content).map_err(|source| archive_error(entry)(source.into()))
            }
        }
    };
    let directory = |folder: &str| zip.borrow_mut().add_directory(folder, SimpleFileOptions::DEFAULT).map_err(archive_error(folder));
    stored(&store, VERSION, |writer, root| {
        let name = writer.xot.add_name(VERSION_ID);
        writer.xot.set_attribute(root, name, RELEASE);
        Ok(())
    })?;
    let predefined = topics
        .values()
        .flat_map(|topic| {
            let users = iter::once(&topic.creation_author).chain(&topic.modified_author).chain(&topic.assigned_to);
            iter::chain([(Extension::TopicType, &topic.r#type), (Extension::TopicStatus, &topic.status)], topic.priority.iter().map(|priority| (Extension::Priority, priority)))
                .chain(topic.labels.iter().map(|label| (Extension::TopicLabel, label)))
                .chain(users.chain(topic.comments.values().flat_map(|comment| iter::once(&comment.author).chain(&comment.modified_author))).map(|user| (Extension::User, user)))
                .chain(topic.bim_snippet.iter().map(|snippet| (Extension::SnippetType, &snippet.snippet_type)))
                .chain(topic.stage.iter().map(|stage| (Extension::Stage, stage)))
        })
        .fold(extensions.clone(), |mut listed, (kind, value)| {
            listed[kind].insert(value.clone());
            listed
        });
    if predefined.values().any(|values| !values.is_empty()) {
        stored(&store, EXTENSIONS, |writer, root| predefined.write(writer, root))?;
    }
    if let Some(info) = project {
        stored(&store, PROJECT_INFO, |writer, root| writer.element(root, &[], PROJECT, info))?;
    }
    if !documents.is_empty() {
        directory(DOCUMENTS)?;
        stored(&store, DOCUMENT_INFO, |writer, root| writer.keyed(root, &[DOCUMENTS], DOCUMENT, None, documents))?;
    }
    topics.iter().try_for_each(|(guid, topic)| {
        let folder = guid.to_string();
        directory(&folder)?;
        stored(&store, (&format!("{folder}/{}", MARKUP.0), MARKUP.1), |writer, root| {
            topic.write(writer, root)?;
            writer.attribute(root, &[TOPIC], GUID, guid)
        })
    })?;
    zip.into_inner().finish().map_err(archive_error(CENTRAL_DIRECTORY))
}

#[must_use]
pub fn merge<F>(left: Issues, right: Issues, mut topic: F) -> Issues
where F: FnMut(Topic, Topic) -> Topic {
    let Issues { project, mut extensions, mut documents, mut topics } = left;
    let Issues { project: other_project, extensions: other_extensions, documents: other_documents, topics: other_topics } = right;
    documents.extend(other_documents);
    other_topics.into_iter().for_each(|(guid, incoming)| match topics.swap_remove_full(&guid) {
        Some((index, guid, held)) => {
            let (last, _) = topics.insert_full(guid, topic(held, incoming));
            topics.swap_indices(index, last);
        }
        None => drop(topics.insert(guid, incoming)),
    });
    other_extensions.into_iter().for_each(|(kind, values)| extensions[kind].extend(values));
    Issues { project: project.or(other_project), extensions, documents, topics }
}

// --- [NAMES]

fn document_entry(guid: DocumentUuid) -> String { format!("{DOCUMENTS}/{guid}") }

const fn names(kind: Extension) -> [&'static str; 2] {
    match kind {
        Extension::TopicType => ["TopicTypes", "TopicType"],
        Extension::TopicStatus => ["TopicStatuses", "TopicStatus"],
        Extension::Priority => ["Priorities", "Priority"],
        Extension::TopicLabel => ["TopicLabels", "TopicLabel"],
        Extension::User => ["Users", "UserIdType"],
        Extension::SnippetType => ["SnippetTypes", "SnippetType"],
        Extension::Stage => ["Stages", "Stage"],
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod xml;

mod markup;
mod visualization;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use markup::{BimSnippet, Comment, DocumentReference, File, Remark, Resource, Target, Topic, Viewpoint, newest};
pub use uuid::Uuid;
pub use visualization::{
    AspectRatio, Bitmap, Camera, ClippingPlane, Coloring, Component, Components, FieldOfView, Line, PositiveLength, Presentation, Projection, Reference, ViewSetupHint, Visibility, Visualization,
    presentation, visibility,
};
