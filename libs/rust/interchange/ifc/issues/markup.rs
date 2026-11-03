use indexmap::{IndexMap, IndexSet};
use itertools::{Either, EitherOrBoth};
use jiff::Timestamp;
use validated::Validated;
use xot::Node;

use super::{
    CommentUuid, DocumentReferenceUuid, DocumentUuid, FILE, GUID, IS_EXTERNAL, Image, NodeUuid, REFERENCE, TOPIC, Text, TopicUuid, ViewpointUuid,
    visualization::Visualization,
    xml::{Codec, Keyed, Reader, Writer, codec, document, local_name, required, resolve, sides, stated, stored},
};
use crate::{IfcError, bind};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Target = Either<DocumentUuid, Text>;
pub type Remark = EitherOrBoth<Text, ViewpointUuid>;

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
    pub r#type: Text,
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
    pub related_topics: IndexMap<TopicUuid, ()>,
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
pub struct Resource {
    pub reference: Text,
    pub content: Option<Box<[u8]>>,
}

impl Codec for Resource {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> {
        let holder = reader.xot.parent(node).unwrap_or(node);
        let stated_external = reader.attribute::<Option<bool>>(holder, &[], IS_EXTERNAL).map2(Text::read(reader, node), |external, reference| (external.unwrap_or_else(|| local_name(reader.xot, holder) == FILE), reference));
        bind(stated_external, |(external, reference)| Validated::from(if external { Ok(None) } else { reader.file(&reference).map(Some) }.map(|content| Self { reference, content })))
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { reference, content } = self;
        let holder = writer.xot.parent(node).unwrap_or(node);
        let external = content.is_none();
        writer.attribute(holder, &[], IS_EXTERNAL, &(external != (local_name(writer.xot, holder) == FILE)).then_some(external))?;
        reference.write(writer, node)?;
        content.iter().try_for_each(|content| writer.file(reference, content, zip::CompressionMethod::Deflated))
    }

    fn present(xot: &xot::Xot, node: Node) -> bool { stated(xot, node) }
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct DocumentReference {
    pub target: Target,
    pub description: Option<Text>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Comment {
    pub date: Timestamp,
    pub author: Text,
    pub remark: Remark,
    pub modified_date: Option<Timestamp>,
    pub modified_author: Option<Text>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Viewpoint {
    pub visualization: Option<Visualization>,
    pub snapshot: Option<Image>,
}

codec! {
    Topic {
        element files(&["Header", "Files"], FILE),
        attribute server_assigned_id(&[TOPIC], "ServerAssignedId"),
        attribute r#type(&[TOPIC], "TopicType"),
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
        keyed related_topics(&[TOPIC, "RelatedTopics"], "RelatedTopic", None),
        keyed comments(&[TOPIC, "Comments"], COMMENT, None),
        keyed viewpoints(&[TOPIC, "Viewpoints"], "ViewPoint", Some("Index")),
    }
    File {
        attribute ifc_project(&[], "IfcProject"),
        attribute ifc_spatial_structure_element(&[], "IfcSpatialStructureElement"),
        element filename(&[], "Filename"),
        element date(&[], "Date"),
        element reference(&[], REFERENCE),
    }
    BimSnippet {
        attribute snippet_type(&[], "SnippetType"),
        element reference(&[], REFERENCE),
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
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> {
        bind(reader.element::<Option<DocumentUuid>>(node, &[], DOCUMENT_GUID).map2(reader.element::<Option<Text>>(node, &[], URL), |guid, link| (guid, link)), |stated| match stated {
            (Some(guid), _) => Validated::Good(Self::Left(guid)),
            (None, Some(link)) => Validated::Good(Self::Right(link)),
            (None, None) => Validated::fail(reader.refused(DOCUMENT_GUID, None)),
        })
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        match self {
            Self::Left(guid) => writer.element(node, &[], DOCUMENT_GUID, guid),
            Self::Right(link) => writer.element(node, &[], URL, link),
        }
    }
}

impl Codec for Remark {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> { sides(reader, reader.element(node, &[], COMMENT), reader.attribute(node, &[VIEWPOINT], GUID), COMMENT) }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let (text, viewpoint) = self.clone().left_and_right();
        writer.element(node, &[], COMMENT, &text)?;
        writer.attribute(node, &[VIEWPOINT], GUID, &viewpoint)
    }
}

impl Keyed<ViewpointUuid> for Viewpoint {
    fn read_keyed(reader: &Reader<'_>, node: Node, _: ViewpointUuid) -> Validated<Self, IfcError> {
        bind(reader.element::<Option<Text>>(node, &[], VIEWPOINT), |file| {
            file.map_or(Validated::Good(None), |name| {
                let entry = resolve(reader.entry, &name);
                required(document(reader.open, reader.upgrading, (&entry, VISUALIZATION_INFO), Visualization::read), &entry).map(Some)
            })
        })
        .map2(reader.element(node, &[], SNAPSHOT), |visualization, snapshot| Self { visualization, snapshot })
    }

    fn write_keyed(&self, key: ViewpointUuid, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { visualization, snapshot } = self;
        if let Some(shown) = visualization {
            let name = format!("{key}.bcfv");
            writer.text(node, VIEWPOINT, &name)?;
            stored(writer.store, (&resolve(writer.entry, &name), VISUALIZATION_INFO), |nested, root| {
                shown.write(nested, root)?;
                nested.attribute(root, &[], GUID, &key)
            })?;
        }
        writer.element(node, &[], SNAPSHOT, snapshot)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(super) fn merged(left: Topic, right: Topic) -> Topic {
    let (newer, older) = if left.modified_date >= right.modified_date { (left, right) } else { (right, left) };
    let mut comments = older.comments;
    newer.comments.into_iter().for_each(|(guid, comment)| {
        if comments.get(&guid).is_none_or(|held| held.modified_date <= comment.modified_date) {
            comments.insert(guid, comment);
        }
    });
    let mut viewpoints = older.viewpoints;
    viewpoints.extend(newer.viewpoints);
    let mut document_references = older.document_references;
    document_references.extend(newer.document_references);
    let mut related_topics = older.related_topics;
    related_topics.extend(newer.related_topics);
    Topic { document_references, related_topics, comments, viewpoints, ..newer }
}
