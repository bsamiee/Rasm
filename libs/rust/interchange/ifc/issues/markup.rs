use jiff::Timestamp;
use validated::Validated;
use xot::Node;

use super::{Codec, CommentUuid, DocumentReferenceUuid, DocumentUuid, GUID, ImageFormat, NodeUuid, Reader, Simple, TopicUuid, ViewpointUuid, Writer, bind, visualization::Visualization};
use crate::IfcError;

// --- [CONSTANTS] -----------------------------------------------------------------------

const DOCUMENT_GUID: &str = "DocumentGuid";
const URL: &str = "Url";
const VIEWPOINT: &str = "Viewpoint";
const SNAPSHOT: &str = "Snapshot";
const VISUALIZATION_INFO: &str = "VisualizationInfo";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Topic {
    pub files: Vec<File>,
    pub guid: TopicUuid,
    pub server_assigned_id: Option<Box<str>>,
    pub kind: Box<str>,
    pub status: Box<str>,
    pub reference_links: Vec<Box<str>>,
    pub title: Box<str>,
    pub priority: Option<Box<str>>,
    pub labels: Vec<Box<str>>,
    pub creation_date: Timestamp,
    pub creation_author: Box<str>,
    pub modified_date: Option<Timestamp>,
    pub modified_author: Option<Box<str>>,
    pub due_date: Option<Timestamp>,
    pub assigned_to: Option<Box<str>>,
    pub stage: Option<Box<str>>,
    pub description: Option<Box<str>>,
    pub bim_snippet: Option<BimSnippet>,
    pub document_references: Vec<DocumentReference>,
    pub related_topics: Vec<TopicUuid>,
    pub comments: Vec<Comment>,
    pub viewpoints: Vec<Viewpoint>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct File {
    pub ifc_project: Option<NodeUuid>,
    pub ifc_spatial_structure_element: Option<NodeUuid>,
    pub is_external: bool,
    pub filename: Option<Box<str>>,
    pub date: Option<Timestamp>,
    pub reference: Option<Box<str>>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct BimSnippet {
    pub snippet_type: Box<str>,
    pub is_external: bool,
    pub reference: Box<str>,
    pub reference_schema: Box<str>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct DocumentReference {
    pub guid: DocumentReferenceUuid,
    pub target: Target,
    pub description: Option<Box<str>>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Target {
    Document(DocumentUuid),
    Url(Box<str>),
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Comment {
    pub guid: CommentUuid,
    pub date: Timestamp,
    pub author: Box<str>,
    pub text: Option<Box<str>>,
    pub viewpoint: Option<ViewpointUuid>,
    pub modified_date: Option<Timestamp>,
    pub modified_author: Option<Box<str>>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Viewpoint {
    pub guid: ViewpointUuid,
    pub visualization: Option<Visualization>,
    pub snapshot: Option<Snapshot>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Snapshot {
    pub format: ImageFormat,
    pub content: Box<[u8]>,
}

codec! {
    Topic {
        element files(&["Header", "Files"], "File"),
        attribute guid(&["Topic"], "Guid"),
        attribute server_assigned_id(&["Topic"], "ServerAssignedId"),
        attribute kind(&["Topic"], "TopicType"),
        attribute status(&["Topic"], "TopicStatus"),
        element reference_links(&["Topic", "ReferenceLinks"], "ReferenceLink"),
        element title(&["Topic"], "Title"),
        element priority(&["Topic"], "Priority"),
        element labels(&["Topic", "Labels"], "Label"),
        element creation_date(&["Topic"], "CreationDate"),
        element creation_author(&["Topic"], "CreationAuthor"),
        element modified_date(&["Topic"], "ModifiedDate"),
        element modified_author(&["Topic"], "ModifiedAuthor"),
        element due_date(&["Topic"], "DueDate"),
        element assigned_to(&["Topic"], "AssignedTo"),
        element stage(&["Topic"], "Stage"),
        element description(&["Topic"], "Description"),
        element bim_snippet(&["Topic"], "BimSnippet"),
        element document_references(&["Topic", "DocumentReferences"], "DocumentReference"),
        attribute related_topics(&["Topic", "RelatedTopics", "RelatedTopic"], "Guid"),
        element comments(&["Topic", "Comments"], "Comment"),
        element viewpoints(&["Topic", "Viewpoints"], "ViewPoint"),
    }
    File {
        attribute ifc_project(&[], "IfcProject"),
        attribute ifc_spatial_structure_element(&[], "IfcSpatialStructureElement"),
        attribute is_external(&[], "IsExternal") = true,
        element filename(&[], "Filename"),
        element date(&[], "Date"),
        element reference(&[], "Reference"),
    }
    BimSnippet {
        attribute snippet_type(&[], "SnippetType"),
        attribute is_external(&[], "IsExternal") = default,
        element reference(&[], "Reference"),
        element reference_schema(&[], "ReferenceSchema"),
    }
    DocumentReference {
        attribute guid(&[], "Guid"),
        inline target,
        element description(&[], "Description"),
    }
    Comment {
        attribute guid(&[], "Guid"),
        element date(&[], "Date"),
        element author(&[], "Author"),
        element text(&[], "Comment"),
        attribute viewpoint(&["Viewpoint"], "Guid"),
        element modified_date(&[], "ModifiedDate"),
        element modified_author(&[], "ModifiedAuthor"),
    }
}

impl Codec for Target {
    fn read(reader: &Reader<'_, '_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        bind(reader.element::<Option<DocumentUuid>>(node, &[], DOCUMENT_GUID).map2(reader.element::<Option<Box<str>>>(node, &[], URL), |guid, link| (guid, link)), |target| match target {
            (Some(guid), _) => Validated::Good(Self::Document(guid)),
            (None, Some(link)) => Validated::Good(Self::Url(link)),
            (None, None) => Validated::fail(reader.lacks(DOCUMENT_GUID)),
        })
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        match self {
            Self::Document(guid) => writer.element(node, &[], DOCUMENT_GUID, guid),
            Self::Url(link) => writer.element(node, &[], URL, link),
        }
    }
}

impl Codec for Viewpoint {
    fn read(reader: &Reader<'_, '_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        reader.attribute(node, &[], GUID).map3(
            bind(reader.element::<Option<Box<str>>>(node, &[], VIEWPOINT), |file| file.map_or(Validated::Good(None), |name| reader.document(&name, VISUALIZATION_INFO).map(Some))),
            bind(reader.element::<Option<Box<str>>>(node, &[], SNAPSHOT), |file| {
                file.map_or(Validated::Good(None), |name| {
                    Validated::from(name.rsplit_once('.').map_or_else(|| Err(reader.lacks(SNAPSHOT)), |(_, extension)| ImageFormat::decode(extension, reader.entry, SNAPSHOT)))
                        .map2(Validated::from(reader.file(&name)), |format, content| Some(Snapshot { format, content }))
                })
            }),
            |guid, visualization, snapshot| Self { guid, visualization, snapshot },
        )
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { guid, visualization, snapshot } = self;
        writer.attribute(node, &[], GUID, guid)?;
        if let Some(shown) = visualization {
            let name: Box<str> = format!("{guid}.bcfv").into();
            writer.element(node, &[], VIEWPOINT, &name)?;
            writer.document(&name, VISUALIZATION_INFO, |nested, root| {
                nested.attribute(root, &[], GUID, guid)?;
                shown.write(nested, root)
            })?;
        }
        if let Some(Snapshot { format, content }) = snapshot {
            let name: Box<str> = format!("{guid}.{}", format.encode()).into();
            writer.element(node, &[], SNAPSHOT, &name)?;
            writer.file(&name, content)?;
        }
        Ok(())
    }
}
