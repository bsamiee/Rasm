use core::{cell::RefCell, iter};
use std::{
    collections::HashMap,
    io::{Read, Seek, Write},
};

use aws_lc_rs::digest::{self, Digest};
use data_encoding::{Encoding, HEXLOWER};
use indexmap::{IndexMap, IndexSet};
use newtype_uuid_macros::impl_typed_uuid_kinds;
use nonempty_collections::{NEVec, nev};
use uuid::Uuid;
use validated::Validated;
use xot::{Node, Xot};
use zip::{CompressionMethod, ZIP64_BYTES_THR, ZipArchive, ZipWriter, result::ZipError, write::SimpleFileOptions};

use self::xml::{Codec, Keyed, Reader, Simple, Writer, archive_error, codec, document, local_name, required, resolve, stated, stored, unique};
use crate::{Alphabet, Constraint, IfcError, bind};

// --- [TYPES] ---------------------------------------------------------------------------

// --- [IDENTIFIERS]

impl_typed_uuid_kinds! {
    kinds = {
        Topic = {},
        Comment = {},
        Viewpoint = {},
        Document = {},
        DocumentReference = {},
    },
}

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

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::From, derive_more::Into)]
pub struct NodeUuid(Uuid);

impl Simple for NodeUuid {
    fn decode(text: &str) -> Result<Self, IfcError> {
        let parsed = |uuid: Result<Uuid, uuid::Error>| uuid.map(Self).map_err(|source| IfcError::Uuid { text: text.into(), source });
        if text.len() != IFC_GUID_DIGITS {
            return parsed(text.parse());
        }
        match &IFC_GUID.decode(format!("00{text}").as_bytes()).map_err(|source| IfcError::Encoded { text: text.into(), alphabet: Alphabet::IfcGuid, source })? {
            [0, 0, uuid @ ..] => parsed(Uuid::from_slice(uuid)),
            _ => Err(IfcError::Constrained(Constraint::IfcGuid(text.into()))),
        }
    }

    fn encode(&self) -> String { IFC_GUID.encode(&[[0; 2].as_slice(), self.0.as_bytes()].concat()).split_off(2) }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const IFC_GUID: Encoding = data_encoding_macro::new_encoding! {
    symbols: "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_$",
};
const IFC_GUID_DIGITS: usize = 22;
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
    validate(with = |text: &str| -> Result<(), IfcError> { (!text.is_empty()).ok_or(IfcError::Constrained(Constraint::Text)) }, error = IfcError),
    derive(Debug, Clone, PartialEq, Eq, Hash, PartialOrd, Ord, Display, AsRef, Deref, Borrow, FromStr, TryFrom, Into)
)]
pub struct Text(String);

impl Simple for Text {
    fn decode(text: &str) -> Result<Self, IfcError> { Self::try_new(text) }

    fn encode(&self) -> String { String::clone(self) }
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
        match *value {
            [0x89, b'P', b'N', b'G', b'\r', b'\n', 0x1A, b'\n', ..] => Ok(Self { format: ImageFormat::Png, content: value }),
            [0xFF, 0xD8, 0xFF, ..] => Ok(Self { format: ImageFormat::Jpg, content: value }),
            _ => Err(value),
        }
    }
}

impl Codec for Image {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> {
        let text = reader.xot.string_value(node);
        Validated::from(reader.file(text.trim()).and_then(|content| Self::try_from(content).map_err(|_: Box<[u8]>| reader.refused(local_name(reader.xot, node), Some(text.trim())))))
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { format, content } = self;
        let held = digest::digest(&digest::SHA256, content);
        let name = format!("{}.{}", HEXLOWER.encode(held.as_ref()), <&str>::from(format));
        writer.xot.append_text(node, &name).map_err(writer.xml())?;
        (writer.store)(&resolve(writer.entry, &name), content, held, CompressionMethod::Stored)
    }

    fn present(xot: &Xot, node: Node) -> bool { stated(xot, node) }
}

#[derive(Debug, Clone, PartialEq, Default, derive_more::AsRef)]
pub struct Issues {
    #[as_ref]
    project: Option<Project>,
    #[as_ref]
    extensions: Extensions,
    #[as_ref]
    documents: IndexMap<DocumentUuid, Document>,
    #[as_ref]
    topics: IndexMap<TopicUuid, markup::Topic>,
}

impl Issues {
    pub fn new(project: Option<Project>, extensions: Extensions, documents: IndexMap<DocumentUuid, Document>, topics: IndexMap<TopicUuid, markup::Topic>) -> Result<Self, NEVec<IfcError>> {
        let dangling = topics
            .iter()
            .flat_map(|(&topic, body)| {
                let viewpoints = body.comments.values().filter_map(|comment| comment.remark.as_ref().right()).filter(|guid| !body.viewpoints.contains_key(*guid)).map(move |&guid| IfcError::Dangling { topic, guid: guid.into() });
                let references = body.document_references.values().filter_map(|reference| reference.target.as_ref().left()).filter(|guid| !documents.contains_key(*guid)).map(move |&guid| IfcError::Dangling { topic, guid: guid.into() });
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

#[derive(Debug, Clone, PartialEq, Eq, Default)]
pub struct Extensions {
    pub topic_types: IndexSet<Text>,
    pub topic_statuses: IndexSet<Text>,
    pub priorities: IndexSet<Text>,
    pub topic_labels: IndexSet<Text>,
    pub users: IndexSet<Text>,
    pub snippet_types: IndexSet<Text>,
    pub stages: IndexSet<Text>,
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
    Extensions {
        element topic_types(&["TopicTypes"], "TopicType"),
        element topic_statuses(&["TopicStatuses"], "TopicStatus"),
        element priorities(&["Priorities"], "Priority"),
        element topic_labels(&["TopicLabels"], "TopicLabel"),
        element users(&["Users"], "User"),
        element snippet_types(&["SnippetTypes"], "SnippetType"),
        element stages(&["Stages"], "Stage"),
    }
}

impl Keyed<DocumentUuid> for Document {
    fn read_keyed(reader: &Reader<'_>, node: Node, key: DocumentUuid) -> Validated<Self, IfcError> {
        reader.element(node, &[], FILENAME).map3(reader.element(node, &[], DESCRIPTION), Validated::from(reader.file(&document_entry(key))), |filename, description, content| Self { filename, description, content })
    }

    fn write_keyed(&self, key: DocumentUuid, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { filename, description, content } = self;
        writer.element(node, &[], FILENAME, filename)?;
        writer.element(node, &[], DESCRIPTION, description)?;
        writer.file(&document_entry(key), content, CompressionMethod::Deflated)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ARCHIVES]

pub fn read<R>(source: R) -> Result<Issues, NEVec<IfcError>>
where R: Read + Seek {
    const UPGRADED_RELEASE: &str = "2.1";
    let zip = ZipArchive::new(source).map_err(|source| nev![archive_error(CENTRAL_DIRECTORY)(source)])?;
    let entries = zip.file_names().enumerate().map(|(index, name)| name.map(|raw| (resolve("", &raw).to_ascii_lowercase(), index)).map_err(archive_error(CENTRAL_DIRECTORY))).collect::<Validated<IndexMap<_, _>, _>>().ok()?;
    let zip = RefCell::new(zip);
    let open = |entry: &str| -> Result<Box<[u8]>, ZipError> {
        let index = *entries.get(&entry.to_ascii_lowercase()).ok_or(ZipError::FileNotFound)?;
        let mut archive = zip.borrow_mut();
        let mut file = archive.by_index(index)?;
        let mut content = Vec::new();
        file.read_to_end(&mut content)?;
        Ok(content.into_boxed_slice())
    };
    let found = required(document(&open, false, VERSION, |reader, root| reader.attribute::<Text>(root, &[], VERSION_ID)), VERSION.0).ok()?;
    let upgrading = match found.as_str() {
        RELEASE => false,
        UPGRADED_RELEASE => true,
        _ => return Err(nev![IfcError::Version { found: found.into_inner().into_boxed_str() }]),
    };
    let topics = entries
        .keys()
        .filter(|name| name.split_once('/').is_some_and(|(_, file)| file == MARKUP.0))
        .map(|entry| required(document(&open, upgrading, (entry, MARKUP.1), |reader, root| reader.attribute::<TopicUuid>(root, &[TOPIC], GUID).map2(markup::Topic::read(reader, root), |guid, topic| (guid, topic))), entry))
        .collect();
    document(&open, upgrading, PROJECT_INFO, |reader, root| reader.element(root, &[], PROJECT))
        .map4(
            document(&open, upgrading, EXTENSIONS, Extensions::read),
            document(&open, upgrading, DOCUMENT_INFO, |reader, root| reader.keyed(root, &[DOCUMENTS], DOCUMENT, None)),
            bind(topics, |held| unique(CENTRAL_DIRECTORY, held)),
            |project, extensions, documents, topics| (project, extensions.unwrap_or_default(), documents.unwrap_or_default(), topics),
        )
        .ok()
        .and_then(|(project, extensions, documents, topics)| Issues::new(project, extensions, documents, topics))
}

pub fn write<W>(issues: &Issues, sink: W) -> Result<W, IfcError>
where W: Write + Seek {
    let Issues { project, extensions, documents, topics } = issues;
    let zip = RefCell::new(ZipWriter::new(sink));
    let written = RefCell::new(HashMap::<String, Digest>::new());
    let store = |entry: &str, content: &[u8], held: Digest, method: CompressionMethod| -> Result<(), IfcError> {
        match written.borrow_mut().insert(entry.to_owned(), held) {
            Some(previous) if previous.as_ref() == held.as_ref() => Ok(()),
            Some(_) | None => {
                let large = u64::try_from(content.len()).map_err(|source| IfcError::Size { entry: entry.into(), source })? >= ZIP64_BYTES_THR;
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
    let listed = || topics.values();
    let predefined = joined(
        extensions.clone(),
        Extensions {
            topic_types: listed().map(|topic| topic.r#type.clone()).collect(),
            topic_statuses: listed().map(|topic| topic.status.clone()).collect(),
            priorities: listed().filter_map(|topic| topic.priority.clone()).collect(),
            topic_labels: listed().flat_map(|topic| topic.labels.iter().cloned()).collect(),
            users: listed()
                .flat_map(|topic| {
                    iter::once(&topic.creation_author)
                        .chain(&topic.modified_author)
                        .chain(&topic.assigned_to)
                        .chain(topic.comments.values().flat_map(|comment| iter::once(&comment.author).chain(&comment.modified_author)))
                })
                .cloned()
                .collect(),
            snippet_types: listed().filter_map(|topic| topic.bim_snippet.as_ref().map(|snippet| snippet.snippet_type.clone())).collect(),
            stages: listed().filter_map(|topic| topic.stage.clone()).collect(),
        },
    );
    if predefined != Extensions::default() {
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

pub fn merge(left: Issues, right: Issues) -> Result<Issues, NEVec<IfcError>> {
    let Issues { project, extensions, mut documents, mut topics } = left;
    let Issues { project: other_project, extensions: other_extensions, documents: other_documents, topics: other_topics } = right;
    documents.extend(other_documents);
    other_topics.into_iter().for_each(|(guid, topic)| {
        let merged = match topics.swap_remove(&guid) {
            Some(held) => markup::merged(held, topic),
            None => topic,
        };
        topics.insert(guid, merged);
    });
    Issues::new(project.or(other_project), joined(extensions, other_extensions), documents, topics)
}

// --- [EXTENSIONS]

fn joined(mut held: Extensions, other: Extensions) -> Extensions {
    let Extensions { topic_types, topic_statuses, priorities, topic_labels, users, snippet_types, stages } = other;
    held.topic_types.extend(topic_types);
    held.topic_statuses.extend(topic_statuses);
    held.priorities.extend(priorities);
    held.topic_labels.extend(topic_labels);
    held.users.extend(users);
    held.snippet_types.extend(snippet_types);
    held.stages.extend(stages);
    held
}

// --- [ENTRIES]

fn document_entry(guid: DocumentUuid) -> String { format!("{DOCUMENTS}/{guid}") }

// --- [COMPOSITION] ---------------------------------------------------------------------

mod xml;

pub mod markup;
pub mod visualization;
