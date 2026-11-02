use core::{cell::RefCell, slice};
use std::{
    borrow::Cow,
    io::{Cursor, Read as _, Write as _},
};

use color::{AlphaColor, Rgba8, Srgb};
use data_encoding::{HEXUPPER, HEXUPPER_PERMISSIVE};
use jiff::{Timestamp, fmt::temporal::Pieces, tz::Offset};
use newtype_uuid::{TypedUuid, TypedUuidKind};
use newtype_uuid_macros::impl_typed_uuid_kinds;
use nonempty_collections::{NEVec, nev};
use strum::ParseError;
use uom::si::{
    angle::degree,
    f64::{Angle, Length},
    length::meter,
};
use validated::Validated;
use xot::{
    Node, Xot,
    output::xml::{Declaration, Parameters},
};
use zip::{ZipArchive, ZipWriter, result::ZipError, write::SimpleFileOptions};

use crate::IfcError;

// --- [TYPES] ---------------------------------------------------------------------------

impl_typed_uuid_kinds! {
    kinds = {
        Topic = {},
        Comment = {},
        Viewpoint = {},
        Document = {},
        DocumentReference = {},
        Node = {},
    },
}

type Source<'bytes> = RefCell<ZipArchive<Cursor<&'bytes [u8]>>>;
type Sink = RefCell<ZipWriter<Cursor<Vec<u8>>>>;

trait Guid: TypedUuidKind {}

impl Guid for TopicKind {}

impl Guid for CommentKind {}

impl Guid for ViewpointKind {}

impl Guid for DocumentKind {}

impl Guid for DocumentReferenceKind {}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, strum::EnumString, strum::IntoStaticStr)]
#[strum(serialize_all = "lowercase", ascii_case_insensitive)]
pub enum ImageFormat {
    Png,
    #[strum(to_string = "jpg", serialize = "jpeg")]
    Jpg,
}

trait Simple: Sized {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError>;
    fn encode(&self) -> String;
}

impl Simple for Box<str> {
    fn decode(text: &str, _: &str, _: &'static str) -> Result<Self, IfcError> { Ok(text.into()) }

    fn encode(&self) -> String { String::from(&**self) }
}

impl Simple for f64 {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> { text.parse().map_err(|source| IfcError::Number { entry: entry.into(), element, source }) }

    fn encode(&self) -> String { self.to_string() }
}

impl Simple for bool {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> {
        match text {
            "true" | "1" => Ok(true),
            "false" | "0" => Ok(false),
            _ => Err(IfcError::Element { entry: entry.into(), element }),
        }
    }

    fn encode(&self) -> String { self.to_string() }
}

impl Simple for Timestamp {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> {
        let malformed = |source| IfcError::Date { entry: entry.into(), element, source };
        let pieces = Pieces::parse(text).map_err(malformed)?;
        pieces.to_numeric_offset().unwrap_or(Offset::UTC).to_timestamp(pieces.date().to_datetime(pieces.time().unwrap_or_default())).map_err(malformed)
    }

    fn encode(&self) -> String { self.to_string() }
}

impl<K> Simple for TypedUuid<K>
where K: Guid
{
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> { text.parse().map_err(|source| IfcError::Guid { entry: entry.into(), element, source }) }

    fn encode(&self) -> String { self.to_string() }
}

impl Simple for NodeUuid {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> {
        let index = |byte: u8| match byte {
            b'0'..=b'9' => Some(byte.strict_sub(b'0')),
            b'A'..=b'Z' => Some(byte.strict_sub(b'A').strict_add(10)),
            b'a'..=b'z' => Some(byte.strict_sub(b'a').strict_add(36)),
            b'_' => Some(62),
            b'$' => Some(63),
            _ => None,
        };
        <[u8; IFC_GUID_DIGITS]>::try_from(text.as_bytes())
            .ok()
            .and_then(|digits| digits.into_iter().try_fold(0_u128, |value, byte| value.checked_mul(64)?.checked_add(u128::from(index(byte)?))))
            .map(Self::from_u128)
            .ok_or_else(|| IfcError::Element { entry: entry.into(), element })
    }

    fn encode(&self) -> String {
        let digit = |sextet: u8| match sextet {
            0..=9 => char::from(b'0'.strict_add(sextet)),
            10..=35 => char::from(b'A'.strict_add(sextet.strict_sub(10))),
            36..=61 => char::from(b'a'.strict_add(sextet.strict_sub(36))),
            62 => '_',
            _ => '$',
        };
        let value = self.as_u128();
        (0..IFC_GUID_DIGITS)
            .rev()
            .map(|place| {
                let [.., byte] = (value >> place.strict_mul(6)).to_be_bytes();
                digit(byte & 63)
            })
            .collect()
    }
}

impl Simple for AlphaColor<Srgb> {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> {
        match HEXUPPER_PERMISSIVE.decode(text.as_bytes()).map_err(|source| IfcError::Color { entry: entry.into(), element, source })?.as_slice() {
            &[r, g, b] => Ok(Self::from_rgb8(r, g, b)),
            &[alpha, r, g, b] => Ok(Self::from_rgba8(r, g, b, alpha)),
            _ => Err(IfcError::Element { entry: entry.into(), element }),
        }
    }

    fn encode(&self) -> String {
        let Rgba8 { r, g, b, a: alpha } = self.to_rgba8();
        match alpha {
            u8::MAX => HEXUPPER.encode(&[r, g, b]),
            _ => HEXUPPER.encode(&[alpha, r, g, b]),
        }
    }
}

impl Simple for ImageFormat {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> {
        text.parse().map_err(|ParseError::VariantNotFound| IfcError::Element { entry: entry.into(), element })
    }

    fn encode(&self) -> String { <&str>::from(self).to_owned() }
}

impl Simple for Length {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> { f64::decode(text, entry, element).map(Self::new::<meter>) }

    fn encode(&self) -> String { self.get::<meter>().encode() }
}

impl Simple for Angle {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> { f64::decode(text, entry, element).map(Self::new::<degree>) }

    fn encode(&self) -> String { self.get::<degree>().encode() }
}

trait Codec: Sized {
    fn read(reader: &Reader<'_, '_>, node: Node, element: &'static str) -> Validated<Self, IfcError>;
    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError>;
}

impl<T> Codec for T
where T: Simple
{
    fn read(reader: &Reader<'_, '_>, node: Node, element: &'static str) -> Validated<Self, IfcError> { Validated::from(Self::decode(reader.xot.string_value(node).trim(), reader.entry, element)) }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { writer.xot.append_text(node, &self.encode()).map_err(writer.xml()) }
}

trait Occurs: Sized {
    const MANY: bool;
    type Item;
    fn occurs(items: Vec<Self::Item>) -> Option<Self>;
    fn items(&self) -> &[Self::Item];
}

impl<T> Occurs for T
where T: Codec
{
    const MANY: bool = false;
    type Item = Self;

    fn occurs(items: Vec<Self>) -> Option<Self> { items.into_iter().next() }

    fn items(&self) -> &[Self] { slice::from_ref(self) }
}

impl<T> Occurs for Option<T>
where T: Codec
{
    const MANY: bool = false;
    type Item = T;

    fn occurs(items: Vec<T>) -> Option<Self> { Some(items.into_iter().next()) }

    fn items(&self) -> &[T] { self.as_slice() }
}

impl<T> Occurs for Vec<T>
where T: Codec
{
    const MANY: bool = true;
    type Item = T;

    fn occurs(items: Vec<T>) -> Option<Self> { Some(items) }

    fn items(&self) -> &[T] { self }
}

impl<T> Occurs for NEVec<T>
where T: Codec
{
    const MANY: bool = true;
    type Item = T;

    fn occurs(items: Vec<T>) -> Option<Self> { NEVec::try_from_vec(items) }

    fn items(&self) -> &[T] { self.iter().as_slice() }
}

macro_rules! codec {
    (@read $reader:ident $node:ident inline $field:ident) => { Codec::read($reader, $node, stringify!($field)) };
    (@read $reader:ident $node:ident $method:ident $field:ident ($($argument:expr),+)) => { $reader.$method($node, $($argument),+) };
    (@read $reader:ident $node:ident $method:ident $field:ident ($($argument:expr),+) = default) => {
        $reader.$method::<Option<_>>($node, $($argument),+).map(Option::unwrap_or_default)
    };
    (@read $reader:ident $node:ident $method:ident $field:ident ($($argument:expr),+) = $default:tt) => {
        $reader.$method::<Option<_>>($node, $($argument),+).map(|value| value.unwrap_or($default))
    };
    (@write $writer:ident $node:ident inline $field:ident) => { Codec::write($field, $writer, $node)? };
    (@write $writer:ident $node:ident $method:ident $field:ident ($($argument:expr),+)) => { $writer.$method($node, $($argument,)+ $field)? };
    (@join $value:expr) => { $value };
    (@join $value:expr, $($rest:expr),+) => { $value.map2(codec!(@join $($rest),+), |head, tail| (head, tail)) };
    (@pattern $field:ident) => { $field };
    (@pattern $field:ident, $($rest:ident),+) => { ($field, codec!(@pattern $($rest),+)) };
    ($($record:ty { $($method:ident $field:ident $(($($argument:expr),+))? $(= $default:tt)?),+ $(,)? })+) => {$(
        impl Codec for $record {
            fn read(reader: &Reader<'_, '_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
                codec!(@join $(codec!(@read reader node $method $field $(($($argument),+))? $(= $default)?)),+)
                    .map(|codec!(@pattern $($field),+)| Self { $($field),+ })
            }

            fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
                let Self { $($field),+ } = self;
                $(codec!(@write writer node $method $field $(($($argument),+))?);)+
                Ok(())
            }
        }
    )+};
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const GUID: &str = "Guid";
const IFC_GUID_DIGITS: usize = 22;
const CENTRAL_DIRECTORY: &str = "central directory";
const VERSION_ENTRY: &str = "bcf.version";
const VERSION_ROOT: &str = "Version";
const VERSION_ID: &str = "VersionId";
const RELEASE: &str = "3.0";
const EXTENSIONS_ENTRY: &str = "extensions.xml";
const EXTENSIONS_ROOT: &str = "Extensions";
const PROJECT_ENTRY: &str = "project.bcfp";
const PROJECT_ROOT: &str = "ProjectInfo";
const PROJECT: &str = "Project";
const DOCUMENTS_ENTRY: &str = "documents.xml";
const DOCUMENTS_ROOT: &str = "DocumentInfo";
const DOCUMENTS: &str = "Documents";
const DOCUMENT: &str = "Document";
const FILENAME: &str = "Filename";
const DESCRIPTION: &str = "Description";
const MARKUP_ENTRY: &str = "markup.bcf";
const MARKUP_ROOT: &str = "Markup";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Default)]
pub struct Issues {
    pub project: Option<Project>,
    pub extensions: Extensions,
    pub documents: Vec<Document>,
    pub topics: Vec<markup::Topic>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Project {
    pub id: Box<str>,
    pub name: Option<Box<str>>,
}

#[derive(Debug, Clone, PartialEq, Eq, Default)]
pub struct Extensions {
    pub topic_types: Vec<Box<str>>,
    pub topic_statuses: Vec<Box<str>>,
    pub priorities: Vec<Box<str>>,
    pub topic_labels: Vec<Box<str>>,
    pub users: Vec<Box<str>>,
    pub snippet_types: Vec<Box<str>>,
    pub stages: Vec<Box<str>>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Document {
    pub guid: DocumentUuid,
    pub filename: Box<str>,
    pub description: Option<Box<str>>,
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

impl Codec for Document {
    fn read(reader: &Reader<'_, '_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        bind(reader.attribute::<DocumentUuid>(node, &[], GUID), |guid| Validated::from(reader.file(&document_entry(guid)).map(|content| (guid, content)))).map3(
            reader.element(node, &[], FILENAME),
            reader.element(node, &[], DESCRIPTION),
            |(guid, content), filename, description| Self { guid, filename, description, content },
        )
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { guid, filename, description, content } = self;
        writer.attribute(node, &[], GUID, guid)?;
        writer.element(node, &[], FILENAME, filename)?;
        writer.element(node, &[], DESCRIPTION, description)?;
        writer.file(&document_entry(*guid), content)
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Reader<'xml, 'bytes> {
    xot: &'xml Xot,
    entry: &'xml str,
    archive: &'xml Source<'bytes>,
}

impl<'xml> Reader<'xml, '_> {
    fn lacks(&self, element: &'static str) -> IfcError { IfcError::Element { entry: self.entry.into(), element } }

    fn named(&self, node: Node, name: &str) -> impl Iterator<Item = Node> + use<'xml> {
        let xot = self.xot;
        let id = xot.name(name);
        xot.children(node).filter(move |&child| xot.element(child).is_some_and(|element| Some(element.name()) == id))
    }

    fn elements(&self, node: Node, holders: &[&'static str]) -> Vec<Node> { holders.iter().fold(vec![node], |nodes, name| nodes.into_iter().flat_map(|holder| self.named(holder, name)).collect()) }

    fn occurs<T>(&self, items: Validated<Vec<T::Item>, IfcError>, element: &'static str) -> Validated<T, IfcError>
    where T: Occurs {
        bind(items, |found| T::occurs(found).map_or_else(|| Validated::fail(self.lacks(element)), Validated::Good))
    }

    fn element<T>(&self, node: Node, holders: &[&'static str], element: &'static str) -> Validated<T, IfcError>
    where T: Occurs<Item: Codec> {
        self.occurs(self.elements(node, holders).into_iter().flat_map(|holder| self.named(holder, element)).map(|child| T::Item::read(self, child, element)).collect(), element)
    }

    fn attribute<T>(&self, node: Node, holders: &[&'static str], attribute: &'static str) -> Validated<T, IfcError>
    where T: Occurs<Item: Simple> {
        let xot = self.xot;
        self.occurs(
            self.elements(node, holders)
                .into_iter()
                .filter_map(|holder| xot.name(attribute).and_then(|id| xot.get_attribute(holder, id)))
                .map(|text| T::Item::decode(text.trim(), self.entry, attribute))
                .collect(),
            attribute,
        )
    }

    fn file(&self, name: &str) -> Result<Box<[u8]>, IfcError> {
        let entry = sibling(self.entry, name);
        open(self.archive, &entry).map(Vec::into_boxed_slice).map_err(zip_error(&entry))
    }

    fn document<T>(&self, name: &str, root: &'static str) -> Validated<T, IfcError>
    where T: Codec {
        document(self.archive, &sibling(self.entry, name), root, |reader, element| T::read(reader, element, root))
    }
}

struct Writer<'xml> {
    xot: &'xml mut Xot,
    entry: &'xml str,
    archive: &'xml Sink,
}

impl<'xml> Writer<'xml> {
    const fn xml(&self) -> impl Fn(xot::Error) -> IfcError + use<'xml> {
        let entry = self.entry;
        move |source| IfcError::Xml { entry: entry.into(), source }
    }

    fn append(&mut self, parent: Node, name: &str) -> Result<Node, IfcError> {
        let id = self.xot.add_name(name);
        let child = self.xot.new_element(id);
        self.xot.append(parent, child).map(|()| child).map_err(self.xml())
    }

    fn holders(&mut self, node: Node, holders: &[&'static str]) -> Result<Node, IfcError> {
        holders.iter().try_fold(node, |parent, name| {
            let id = self.xot.add_name(name);
            let found = self.xot.children(parent).find(|&child| self.xot.element(child).is_some_and(|element| element.name() == id));
            found.map_or_else(|| self.append(parent, name), Ok)
        })
    }

    fn element<T>(&mut self, node: Node, holders: &[&'static str], element: &'static str, value: &T) -> Result<(), IfcError>
    where T: Occurs<Item: Codec> {
        match value.items() {
            [] => Ok(()),
            items => {
                let parent = self.holders(node, holders)?;
                items.iter().try_for_each(|item| self.append(parent, element).and_then(|child| item.write(self, child)))
            }
        }
    }

    fn attribute<T>(&mut self, node: Node, holders: &[&'static str], attribute: &'static str, value: &T) -> Result<(), IfcError>
    where T: Occurs<Item: Simple> {
        let name = self.xot.add_name(attribute);
        match (value.items(), T::MANY, holders.split_last()) {
            ([], _, _) => Ok(()),
            (items, true, Some((holder, containers))) => {
                let parent = self.holders(node, containers)?;
                items.iter().try_for_each(|item| self.append(parent, holder).map(|child| self.xot.set_attribute(child, name, item.encode())))
            }
            (items, _, _) => self.holders(node, holders).map(|holder| items.iter().for_each(|item| self.xot.set_attribute(holder, name, item.encode()))),
        }
    }

    fn file(&self, name: &str, content: &[u8]) -> Result<(), IfcError> {
        let entry = sibling(self.entry, name);
        let mut zip = self.archive.borrow_mut();
        zip.start_file(&entry, SimpleFileOptions::DEFAULT).map_err(zip_error(&entry))?;
        zip.write_all(content).map_err(ZipError::Io).map_err(zip_error(&entry))
    }

    fn document(&self, name: &str, root: &'static str, write: impl FnOnce(&mut Writer<'_>, Node) -> Result<(), IfcError>) -> Result<(), IfcError> {
        compose(self.archive, &sibling(self.entry, name), root, write)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn read(bytes: &[u8]) -> Result<Issues, NEVec<IfcError>> {
    fn optional<T>(archive: &Source<'_>, entry: &str, root: &'static str, read: impl FnOnce(&Reader<'_, '_>, Node) -> Validated<T, IfcError>) -> Validated<Option<T>, IfcError> {
        match open(archive, entry) {
            Ok(bytes) => parse(archive, entry, &bytes, root, read).map(Some),
            Err(ZipError::FileNotFound) => Validated::Good(None),
            Err(source) => Validated::fail(zip_error(entry)(source)),
        }
    }
    let archive = RefCell::new(ZipArchive::new(Cursor::new(bytes)).map_err(zip_error(CENTRAL_DIRECTORY)).map_err(NEVec::new)?);
    let found = document(&archive, VERSION_ENTRY, VERSION_ROOT, |reader, root| reader.attribute::<Box<str>>(root, &[], VERSION_ID)).ok()?;
    (*found == *RELEASE).ok_or_else(|| nev![IfcError::Version { found }])?;
    let names = archive.borrow().file_names().map(|name| name.map(Cow::into_owned).map_err(zip_error(CENTRAL_DIRECTORY))).collect::<Validated<Vec<_>, _>>();
    let topics = bind(names, |listed| {
        listed
            .iter()
            .filter(|name| matches!(name.split_once('/'), Some((_, MARKUP_ENTRY))))
            .map(|entry| document(&archive, entry, MARKUP_ROOT, |reader, root| markup::Topic::read(reader, root, MARKUP_ROOT)))
            .collect()
    });
    optional(&archive, PROJECT_ENTRY, PROJECT_ROOT, |reader, root| reader.element(root, &[], PROJECT))
        .map4(
            optional(&archive, EXTENSIONS_ENTRY, EXTENSIONS_ROOT, |reader, root| Extensions::read(reader, root, EXTENSIONS_ROOT)),
            optional(&archive, DOCUMENTS_ENTRY, DOCUMENTS_ROOT, |reader, root| reader.element(root, &[DOCUMENTS], DOCUMENT)),
            topics,
            |project, extensions, documents, topics| Issues { project, extensions: extensions.unwrap_or_default(), documents: documents.unwrap_or_default(), topics },
        )
        .ok()
}

pub fn write(issues: &Issues) -> Result<Vec<u8>, IfcError> {
    let Issues { project, extensions, documents, topics } = issues;
    let archive = RefCell::new(ZipWriter::new(Cursor::new(Vec::new())));
    compose(&archive, VERSION_ENTRY, VERSION_ROOT, |writer, root| writer.attribute(root, &[], VERSION_ID, &Box::<str>::from(RELEASE)))?;
    compose(&archive, EXTENSIONS_ENTRY, EXTENSIONS_ROOT, |writer, root| extensions.write(writer, root))?;
    if let Some(info) = project {
        compose(&archive, PROJECT_ENTRY, PROJECT_ROOT, |writer, root| writer.element(root, &[], PROJECT, info))?;
    }
    if !documents.is_empty() {
        archive.borrow_mut().add_directory(DOCUMENTS, SimpleFileOptions::DEFAULT).map_err(zip_error(DOCUMENTS))?;
        compose(&archive, DOCUMENTS_ENTRY, DOCUMENTS_ROOT, |writer, root| writer.element(root, &[DOCUMENTS], DOCUMENT, documents))?;
    }
    topics.iter().try_for_each(|topic| {
        let folder = topic.guid.to_string();
        let entry = format!("{folder}/{MARKUP_ENTRY}");
        archive.borrow_mut().add_directory(folder.as_str(), SimpleFileOptions::DEFAULT).map_err(zip_error(&folder))?;
        compose(&archive, &entry, MARKUP_ROOT, |writer, root| topic.write(writer, root))
    })?;
    archive.into_inner().finish().map(Cursor::into_inner).map_err(zip_error(CENTRAL_DIRECTORY))
}

fn document<T>(archive: &Source<'_>, entry: &str, root: &'static str, read: impl FnOnce(&Reader<'_, '_>, Node) -> Validated<T, IfcError>) -> Validated<T, IfcError> {
    open(archive, entry).map_err(zip_error(entry)).map_or_else(Validated::fail, |bytes| parse(archive, entry, &bytes, root, read))
}

fn open(archive: &Source<'_>, entry: &str) -> Result<Vec<u8>, ZipError> {
    let mut zip = archive.borrow_mut();
    let mut file = zip.by_name(entry)?;
    let mut bytes = Vec::new();
    file.read_to_end(&mut bytes).map(|_| bytes).map_err(ZipError::Io)
}

fn parse<T>(archive: &Source<'_>, entry: &str, bytes: &[u8], root: &'static str, read: impl FnOnce(&Reader<'_, '_>, Node) -> Validated<T, IfcError>) -> Validated<T, IfcError> {
    let mut xot = Xot::new();
    xot.parse_bytes(bytes).map_err(|source| IfcError::Parse { entry: entry.into(), source }).map_or_else(Validated::fail, |document| {
        let reader = Reader { xot: &xot, entry, archive };
        reader.named(document, root).next().map_or_else(|| Validated::fail(reader.lacks(root)), |element| read(&reader, element))
    })
}

fn compose(archive: &Sink, entry: &str, root: &'static str, write: impl FnOnce(&mut Writer<'_>, Node) -> Result<(), IfcError>) -> Result<(), IfcError> {
    let mut xot = Xot::new();
    let name = xot.add_name(root);
    let element = xot.new_element(name);
    let mut writer = Writer { xot: &mut xot, entry, archive };
    let document = writer.xot.new_document_with_element(element).map_err(writer.xml())?;
    write(&mut writer, element)?;
    let mut zip = archive.borrow_mut();
    zip.start_file(entry, SimpleFileOptions::DEFAULT).map_err(zip_error(entry))?;
    writer
        .xot
        .serialize_xml_write(Parameters { declaration: Some(Declaration { encoding: Some("UTF-8".to_owned()), standalone: None }), ..Parameters::default() }, document, &mut *zip)
        .map_err(writer.xml())
}

fn sibling(entry: &str, name: &str) -> String { entry.rsplit_once('/').map_or_else(|| name.to_owned(), |(folder, _)| format!("{folder}/{name}")) }

fn document_entry(guid: DocumentUuid) -> String { format!("{DOCUMENTS}/{guid}") }

fn bind<T, U>(value: Validated<T, IfcError>, next: impl FnOnce(T) -> Validated<U, IfcError>) -> Validated<U, IfcError> {
    match value {
        Validated::Good(held) => next(held),
        Validated::Fail(failures) => Validated::Fail(failures),
    }
}

const fn zip_error(entry: &str) -> impl Fn(ZipError) -> IfcError + use<'_> { move |source| IfcError::Archive { entry: entry.into(), source } }

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod markup;
pub mod visualization;
