use indexmap::{IndexMap, IndexSet};
use jiff::Timestamp;
use validated::Validated;
use xot::Node;

use super::{Codec, CommentUuid, DocumentReferenceUuid, DocumentUuid, GUID, Image, Keyed, NodeUuid, Reader, Single, TOPIC, Text, TopicUuid, ViewpointUuid, Writer, visualization::Visualization};
use crate::IfcError;

// --- [CONSTANTS] -----------------------------------------------------------------------

const COMMENT: &str = "Comment";
const DOCUMENT_GUID: &str = "DocumentGuid";
const URL: &str = "Url";
const VIEWPOINT: &str = "Viewpoint";
const SNAPSHOT: &str = "Snapshot";
const VISUALIZATION_INFO: &str = "VisualizationInfo";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Topic {
    pub files: Vec<File>,
    pub server_assigned_id: Option<Text>,
    pub kind: Text,
    pub status: Text,
    pub reference_links: Vec<Text>,
    pub title: Text,
    pub priority: Option<Text>,
    pub labels: IndexSet<Text>,
    pub creation_date: Timestamp,
    pub creation_author: Text,
    pub modified_date: Option<Timestamp>,
    pub modified_author: Option<Text>,
    pub due_date: Option<Timestamp>,
    pub assigned_to: Option<Text>,
    pub stage: Option<Text>,
    pub description: Option<Text>,
    pub bim_snippet: Option<BimSnippet>,
    pub document_references: IndexMap<DocumentReferenceUuid, DocumentReference>,
    pub related_topics: IndexSet<TopicUuid>,
    pub comments: IndexMap<CommentUuid, Comment>,
    pub viewpoints: IndexMap<ViewpointUuid, Viewpoint>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct File {
    pub ifc_project: Option<NodeUuid>,
    pub ifc_spatial_structure_element: Option<NodeUuid>,
    pub filename: Option<Text>,
    pub date: Option<Timestamp>,
    pub reference: Option<Resource>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct BimSnippet {
    pub snippet_type: Text,
    pub reference: Resource,
    pub reference_schema: Text,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Resource {
    External(Text),
    Archived { reference: Text, content: Box<[u8]> },
}

impl Single for Resource {}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct DocumentReference {
    pub target: Target,
    pub description: Option<Text>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Target {
    Document(DocumentUuid),
    Url(Text),
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Comment {
    pub date: Timestamp,
    pub author: Text,
    pub remark: Remark,
    pub modified_date: Option<Timestamp>,
    pub modified_author: Option<Text>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Remark {
    Text(Text),
    Viewpoint(ViewpointUuid),
    Both { text: Text, viewpoint: ViewpointUuid },
}

#[derive(Debug, Clone, PartialEq)]
pub struct Viewpoint {
    pub visualization: Option<Visualization>,
    pub snapshot: Option<Image>,
}

codec! {
    Topic {
        element files(&["Header", "Files"], "File"),
        attribute server_assigned_id(&[TOPIC], "ServerAssignedId"),
        attribute kind(&[TOPIC], "TopicType"),
        attribute status(&[TOPIC], "TopicStatus"),
        element reference_links(&[TOPIC, "ReferenceLinks"], "ReferenceLink"),
        element title(&[TOPIC], "Title"),
        element priority(&[TOPIC], "Priority"),
        element labels(&[TOPIC, "Labels"], "Label"),
        element creation_date(&[TOPIC], "CreationDate"),
        element creation_author(&[TOPIC], "CreationAuthor"),
        element modified_date(&[TOPIC], "ModifiedDate"),
        element modified_author(&[TOPIC], "ModifiedAuthor"),
        element due_date(&[TOPIC], "DueDate"),
        element assigned_to(&[TOPIC], "AssignedTo"),
        element stage(&[TOPIC], "Stage"),
        element description(&[TOPIC], "Description"),
        element bim_snippet(&[TOPIC], "BimSnippet"),
        keyed document_references(&[TOPIC, "DocumentReferences"], "DocumentReference", None),
        attribute related_topics(&[TOPIC, "RelatedTopics", "RelatedTopic"], GUID),
        keyed comments(&[TOPIC, "Comments"], COMMENT, None),
        keyed viewpoints(&[TOPIC, "Viewpoints"], "ViewPoint", Some("Index")),
    }
    File {
        attribute ifc_project(&[], "IfcProject"),
        attribute ifc_spatial_structure_element(&[], "IfcSpatialStructureElement"),
        element filename(&[], "Filename"),
        element date(&[], "Date"),
        resource reference(true),
    }
    BimSnippet {
        attribute snippet_type(&[], "SnippetType"),
        resource reference(false),
        element reference_schema(&[], "ReferenceSchema"),
    }
    DocumentReference {
        inline target,
        element description(&[], "Description"),
    }
    Comment {
        element date(&[], "Date"),
        element author(&[], "Author"),
        inline remark,
        element modified_date(&[], "ModifiedDate"),
        element modified_author(&[], "ModifiedAuthor"),
    }
}

impl Codec for Target {
    fn read(reader: &Reader<'_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        match reader.element::<Option<DocumentUuid>>(node, &[], DOCUMENT_GUID).map2(reader.element::<Option<Text>>(node, &[], URL), |guid, link| (guid, link)) {
            Validated::Good((Some(guid), _)) => Validated::Good(Self::Document(guid)),
            Validated::Good((None, Some(link))) => Validated::Good(Self::Url(link)),
            Validated::Good((None, None)) => Validated::fail(reader.lacks(DOCUMENT_GUID)),
            Validated::Fail(failures) => Validated::Fail(failures),
        }
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        match self {
            Self::Document(guid) => writer.element(node, &[], DOCUMENT_GUID, guid),
            Self::Url(link) => writer.element(node, &[], URL, link),
        }
    }
}

impl Codec for Remark {
    fn read(reader: &Reader<'_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        match reader.element::<Option<Text>>(node, &[], COMMENT).map2(reader.attribute::<Option<ViewpointUuid>>(node, &[VIEWPOINT], GUID), |text, viewpoint| (text, viewpoint)) {
            Validated::Good((Some(text), Some(viewpoint))) => Validated::Good(Self::Both { text, viewpoint }),
            Validated::Good((Some(text), None)) => Validated::Good(Self::Text(text)),
            Validated::Good((None, Some(viewpoint))) => Validated::Good(Self::Viewpoint(viewpoint)),
            Validated::Good((None, None)) => Validated::fail(reader.lacks(COMMENT)),
            Validated::Fail(failures) => Validated::Fail(failures),
        }
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        match self {
            Self::Text(text) => writer.element(node, &[], COMMENT, text),
            Self::Viewpoint(viewpoint) => writer.attribute(node, &[VIEWPOINT], GUID, viewpoint),
            Self::Both { text, viewpoint } => {
                writer.element(node, &[], COMMENT, text)?;
                writer.attribute(node, &[VIEWPOINT], GUID, viewpoint)
            }
        }
    }
}

impl Keyed<ViewpointUuid> for Viewpoint {
    fn read_keyed(reader: &Reader<'_>, node: Node, _: &'static str, _: ViewpointUuid) -> Validated<Self, IfcError> {
        reader
            .element::<Option<Text>>(node, &[], VIEWPOINT)
            .ok()
            .map_or_else(Validated::Fail, |file| file.map_or(Validated::Good(None), |name| reader.document(&name, VISUALIZATION_INFO).map(Some)))
            .map2(reader.image(node, SNAPSHOT), |visualization, snapshot| Self { visualization, snapshot })
    }

    fn write_keyed(&self, key: ViewpointUuid, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { visualization, snapshot } = self;
        if let Some(shown) = visualization {
            let name = format!("{key}.bcfv");
            writer.text(node, VIEWPOINT, &name)?;
            writer.document(&name, VISUALIZATION_INFO, |nested, root| {
                shown.write(nested, root)?;
                nested.attribute(root, &[], GUID, &key)
            })?;
        }
        writer.image(node, SNAPSHOT, snapshot)
    }
}
