use core::{cell::RefCell, hash::Hash, iter};
use std::{
    collections::HashMap,
    io::{Read, Seek, Write},
    path::{Component, Path},
};

use aws_lc_rs::digest::{self, Digest};
use color::{AlphaColor, Rgba8, Srgb};
use data_encoding::{HEXLOWER, HEXUPPER, HEXUPPER_PERMISSIVE};
use indexmap::{IndexMap, IndexSet};
use itertools::Itertools as _;
use jiff::{Timestamp, fmt::temporal::Pieces, tz::Offset};
use newtype_uuid::{TypedUuid, TypedUuidKind};
use newtype_uuid_macros::impl_typed_uuid_kinds;
use nonempty_collections::{NEVec, nev};
use strum::ParseError;
use uom::si::{f64::Length, length::meter};
use validated::Validated;
use xot::{
    Node, Xot,
    output::xml::{Declaration, Parameters},
};
use zip::{CompressionMethod, ZIP64_BYTES_THR, ZipArchive, ZipWriter, result::ZipError, write::SimpleFileOptions};

use crate::IfcError;

// --- [TYPES] ---------------------------------------------------------------------------

type Open<'archive> = dyn Fn(&str) -> Result<Box<[u8]>, ZipError> + 'archive;
type Store<'archive> = dyn Fn(&str, &[u8], CompressionMethod) -> Result<(), IfcError> + 'archive;

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

trait Kind: TypedUuidKind + Sized {
    fn guid(guid: TypedUuid<Self>) -> Guid;
}

impl Kind for TopicKind {
    fn guid(guid: TypedUuid<Self>) -> Guid { Guid::Topic(guid) }
}

impl Kind for CommentKind {
    fn guid(guid: TypedUuid<Self>) -> Guid { Guid::Comment(guid) }
}

impl Kind for ViewpointKind {
    fn guid(guid: TypedUuid<Self>) -> Guid { Guid::Viewpoint(guid) }
}

impl Kind for DocumentKind {
    fn guid(guid: TypedUuid<Self>) -> Guid { Guid::Document(guid) }
}

impl Kind for DocumentReferenceKind {
    fn guid(guid: TypedUuid<Self>) -> Guid { Guid::DocumentReference(guid) }
}

trait Simple: Sized {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError>;
    fn encode(&self) -> String;
}

impl Simple for f64 {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> {
        text.parse::<Self>()
            .map_err(|source| IfcError::Number { entry: entry.into(), element, source })
            .and_then(|value| Some(value).filter(|number| number.is_finite()).ok_or_else(|| IfcError::Element { entry: entry.into(), element }))
    }

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
where K: Kind
{
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> { text.parse().map_err(|source| IfcError::Guid { entry: entry.into(), element, source }) }

    fn encode(&self) -> String { self.to_string() }
}

impl Simple for NodeUuid {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> {
        let malformed = || IfcError::Element { entry: entry.into(), element };
        let index = |byte: u8| match byte {
            b'0'..=b'9' => Some(byte.strict_sub(b'0')),
            b'A'..=b'Z' => Some(byte.strict_sub(b'A').strict_add(10)),
            b'a'..=b'z' => Some(byte.strict_sub(b'a').strict_add(36)),
            b'_' => Some(62),
            b'$' => Some(63),
            _ => None,
        };
        text.as_bytes()
            .as_array::<IFC_GUID_DIGITS>()
            .ok_or_else(malformed)?
            .iter()
            .try_fold(0_u128, |value, &byte| value.checked_mul(64)?.checked_add(u128::from(index(byte)?)))
            .map(Self::from_u128)
            .ok_or_else(malformed)
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
        match *HEXUPPER_PERMISSIVE.decode(text.as_bytes()).map_err(|source| IfcError::Color { entry: entry.into(), element, source })? {
            [r, g, b] => Ok(Self::from_rgb8(r, g, b)),
            [alpha, r, g, b] => Ok(Self::from_rgba8(r, g, b, alpha)),
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

impl Simple for Length {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> { f64::decode(text, entry, element).map(Self::new::<meter>) }

    fn encode(&self) -> String { self.get::<meter>().encode() }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, strum::EnumString, strum::IntoStaticStr)]
#[strum(serialize_all = "lowercase")]
pub enum ImageFormat {
    Png,
    Jpg,
}

impl Simple for ImageFormat {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> { text.parse().map_err(|ParseError::VariantNotFound| IfcError::Element { entry: entry.into(), element }) }

    fn encode(&self) -> String { <&str>::from(self).to_owned() }
}

trait Codec: Sized {
    fn read(reader: &Reader<'_>, node: Node, element: &'static str) -> Validated<Self, IfcError>;
    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError>;
    fn present(_: &Xot, _: Node) -> bool { true }
}

impl<T> Codec for T
where T: Simple
{
    fn read(reader: &Reader<'_>, node: Node, element: &'static str) -> Validated<Self, IfcError> { Validated::from(Self::decode(reader.xot.string_value(node).trim(), reader.entry, element)) }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { writer.xot.append_text(node, &self.encode()).map_err(writer.xml()) }

    fn present(xot: &Xot, node: Node) -> bool { !xot.string_value(node).trim().is_empty() }
}

trait Single {}

impl<T> Single for T where T: Codec {}

trait Occurs: Sized {
    const MANY: bool;
    type Item;
    fn occurs(items: Vec<Self::Item>) -> Option<Self>;
    fn items(&self) -> impl Iterator<Item = &Self::Item>;
}

impl<T> Occurs for T
where T: Single
{
    const MANY: bool = false;
    type Item = Self;

    fn occurs(items: Vec<Self>) -> Option<Self> { items.into_iter().next() }

    fn items(&self) -> impl Iterator<Item = &Self> { iter::once(self) }
}

impl<T> Occurs for Option<T>
where T: Single
{
    const MANY: bool = false;
    type Item = T;

    fn occurs(items: Vec<T>) -> Option<Self> { Some(items.into_iter().next()) }

    fn items(&self) -> impl Iterator<Item = &T> { self.iter() }
}

impl<T> Occurs for Vec<T>
where T: Codec
{
    const MANY: bool = true;
    type Item = T;

    fn occurs(items: Self) -> Option<Self> { Some(items) }

    fn items(&self) -> impl Iterator<Item = &T> { self.iter() }
}

impl<T> Occurs for NEVec<T>
where T: Codec
{
    const MANY: bool = true;
    type Item = T;

    fn occurs(items: Vec<T>) -> Option<Self> { Self::try_from_vec(items) }

    fn items(&self) -> impl Iterator<Item = &T> { self.iter() }
}

impl<T> Occurs for IndexSet<T>
where T: Codec + Hash + Eq
{
    const MANY: bool = true;
    type Item = T;

    fn occurs(items: Vec<T>) -> Option<Self> { Some(items.into_iter().collect()) }

    fn items(&self) -> impl Iterator<Item = &T> { self.iter() }
}

trait Keyed<K>: Sized {
    fn read_keyed(reader: &Reader<'_>, node: Node, element: &'static str, key: K) -> Validated<Self, IfcError>;
    fn write_keyed(&self, key: K, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError>;
}

impl<K, T> Keyed<K> for T
where T: Codec
{
    fn read_keyed(reader: &Reader<'_>, node: Node, element: &'static str, _: K) -> Validated<Self, IfcError> { Self::read(reader, node, element) }

    fn write_keyed(&self, _: K, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { self.write(writer, node) }
}

macro_rules! codec {
    (@read $reader:ident $node:ident inline $field:ident) => { Codec::read($reader, $node, stringify!($field)) };
    (@read $reader:ident $node:ident $method:ident $field:ident ($($argument:expr),+)) => { $reader.$method($node, $($argument),+) };
    (@read $reader:ident $node:ident $method:ident $field:ident ($($argument:expr),+) = default) => {
        $reader.$method::<Option<_>>($node, $($argument),+).map(Option::unwrap_or_default)
    };
    (@write $writer:ident $node:ident inline $field:ident) => { Codec::write($field, $writer, $node)? };
    (@write $writer:ident $node:ident $method:ident $field:ident ($($argument:expr),+)) => { $writer.$method($node, $($argument,)+ $field)? };
    (@write $writer:ident $node:ident $method:ident $field:ident ($($argument:expr),+) = default) => {
        if <_ as PartialEq>::ne($field, &Default::default()) {
            $writer.$method($node, $($argument,)+ $field)?
        }
    };
    (@join $value:expr) => { $value };
    (@join $value:expr, $($rest:expr),+) => { $value.map2(codec!(@join $($rest),+), |head, tail| (head, tail)) };
    (@pattern $field:ident) => { $field };
    (@pattern $field:ident, $($rest:ident),+) => { ($field, codec!(@pattern $($rest),+)) };
    ($($record:ty { $($method:ident $field:ident $(($($argument:expr),+))? $(= $default:tt)?),+ $(,)? })+) => {$(
        impl Codec for $record {
            fn read(reader: &Reader<'_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
                codec!(@join $(codec!(@read reader node $method $field $(($($argument),+))? $(= $default)?)),+)
                    .map(|codec!(@pattern $($field),+)| Self { $($field),+ })
            }

            fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
                let Self { $($field),+ } = self;
                $(codec!(@write writer node $method $field $(($($argument),+))? $(= $default)?);)+
                Ok(())
            }
        }
    )+};
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const GUID: &str = "Guid";
const TOPIC: &str = "Topic";
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
const IS_EXTERNAL: &str = "IsExternal";
const REFERENCE: &str = "Reference";
const MARKUP_ENTRY: &str = "markup.bcf";
const MARKUP_ROOT: &str = "Markup";

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(sanitize(trim), validate(not_empty), derive(Debug, Clone, PartialEq, Eq, Hash, PartialOrd, Ord, Display, AsRef, Deref, Borrow, FromStr, TryFrom, Into))]
pub struct Text(String);

impl Simple for Text {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> {
        Self::try_new(text).map_err(|TextError::NotEmptyViolated| IfcError::Element { entry: entry.into(), element })
    }

    fn encode(&self) -> String { String::clone(self) }
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

impl Single for Image {}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
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

#[derive(Debug, Clone, PartialEq, Default)]
pub struct Issues {
    pub project: Option<Project>,
    pub extensions: Extensions,
    pub documents: IndexMap<DocumentUuid, Document>,
    pub topics: IndexMap<TopicUuid, markup::Topic>,
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
    fn read_keyed(reader: &Reader<'_>, node: Node, _: &'static str, key: DocumentUuid) -> Validated<Self, IfcError> {
        reader.element(node, &[], FILENAME).map3(reader.element(node, &[], DESCRIPTION), Validated::from(reader.file(&document_entry(key))), |filename, description, content| Self {
            filename,
            description,
            content,
        })
    }

    fn write_keyed(&self, key: DocumentUuid, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { filename, description, content } = self;
        writer.element(node, &[], FILENAME, filename)?;
        writer.element(node, &[], DESCRIPTION, description)?;
        writer.file(&document_entry(key), content, CompressionMethod::Deflated)
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Reader<'xml> {
    xot: &'xml Xot,
    entry: &'xml str,
    open: &'xml Open<'xml>,
}

impl<'xml> Reader<'xml> {
    fn lacks(&self, element: &'static str) -> IfcError { IfcError::Element { entry: self.entry.into(), element } }

    fn named(&self, node: Node, name: &'static str) -> impl Iterator<Item = Node> + use<'xml> {
        let xot = self.xot;
        xot.children(node).filter(move |&child| xot.element(child).is_some_and(|element| xot.local_name_str(element.name()) == name))
    }

    fn elements(&self, node: Node, holders: &[&'static str]) -> Vec<Node> { holders.iter().fold(vec![node], |nodes, name| nodes.into_iter().flat_map(|holder| self.named(holder, name)).collect()) }

    fn occurs<T>(&self, items: Validated<Vec<T::Item>, IfcError>, element: &'static str) -> Validated<T, IfcError>
    where T: Occurs {
        items.ok().map_or_else(Validated::Fail, |found| T::occurs(found).map_or_else(|| Validated::fail(self.lacks(element)), Validated::Good))
    }

    fn element<T>(&self, node: Node, holders: &[&'static str], element: &'static str) -> Validated<T, IfcError>
    where T: Occurs<Item: Codec> {
        self.occurs(
            self.elements(node, holders)
                .into_iter()
                .flat_map(|holder| self.named(holder, element))
                .filter(|&child| T::Item::present(self.xot, child))
                .map(|child| T::Item::read(self, child, element))
                .collect(),
            element,
        )
    }

    fn attribute<T>(&self, node: Node, holders: &[&'static str], attribute: &'static str) -> Validated<T, IfcError>
    where T: Occurs<Item: Simple> {
        let xot = self.xot;
        self.occurs(
            self.elements(node, holders)
                .into_iter()
                .filter_map(|holder| xot.name(attribute).and_then(|id| xot.get_attribute(holder, id)))
                .map(str::trim)
                .filter(|text| !text.is_empty())
                .map(|text| T::Item::decode(text, self.entry, attribute))
                .collect(),
            attribute,
        )
    }

    fn keyed<K, V>(&self, node: Node, holders: &[&'static str], element: &'static str, index: Option<&'static str>) -> Validated<IndexMap<TypedUuid<K>, V>, IfcError>
    where
        K: Kind,
        V: Keyed<TypedUuid<K>>,
    {
        self.elements(node, holders)
            .into_iter()
            .flat_map(|holder| self.named(holder, element))
            .map(|child| {
                self.attribute::<TypedUuid<K>>(child, &[], GUID).ok().map_or_else(Validated::Fail, |key| {
                    index.map_or(Validated::Good(None), |rank| self.element::<Option<f64>>(child, &[], rank)).map2(V::read_keyed(self, child, element, key), |rank, value| (key, (rank, value)))
                })
            })
            .collect::<Validated<Vec<_>, _>>()
            .ok()
            .and_then(|entries| unique(self.entry, entries))
            .map_or_else(Validated::Fail, |ranked| {
                Validated::Good(
                    ranked
                        .sorted_by(|_, (left, _), _, (right, _)| match (left, right) {
                            (Some(earlier), Some(later)) => earlier.total_cmp(later),
                            (earlier, later) => earlier.is_some().cmp(&later.is_some()),
                        })
                        .map(|(key, (_, value))| (key, value))
                        .collect(),
                )
            })
    }

    fn resource<T>(&self, node: Node, external: bool) -> Validated<T, IfcError>
    where T: Occurs<Item = markup::Resource> {
        let found = self
            .attribute::<Option<bool>>(node, &[], IS_EXTERNAL)
            .map2(self.element::<Option<Text>>(node, &[], REFERENCE), |stated, reference| reference.map(|held| (stated.unwrap_or(external), held)))
            .ok()
            .map_or_else(Validated::Fail, |held| {
                Validated::from(
                    held.into_iter()
                        .map(
                            |(outside, reference)| {
                                if outside { Ok(markup::Resource::External(reference)) } else { self.file(&reference).map(|content| markup::Resource::Archived { reference, content }) }
                            },
                        )
                        .collect::<Result<Vec<_>, _>>(),
                )
            });
        self.occurs(found, REFERENCE)
    }

    fn image<T>(&self, node: Node, element: &'static str) -> Validated<T, IfcError>
    where T: Occurs<Item = Image> {
        let found = self.element::<Option<Text>>(node, &[], element).ok().map_or_else(Validated::Fail, |reference| {
            Validated::from(reference.into_iter().map(|name| self.file(&name).and_then(|content| Image::try_from(content).map_err(|_: Box<[u8]>| self.lacks(element)))).collect::<Result<Vec<_>, _>>())
        });
        self.occurs(found, element)
    }

    fn file(&self, name: &str) -> Result<Box<[u8]>, IfcError> {
        let entry = resolve(self.entry, name);
        (self.open)(&entry).map_err(archive_error(&entry))
    }

    fn document<T>(&self, name: &str, root: &'static str) -> Validated<T, IfcError>
    where T: Codec {
        document(self.open, &resolve(self.entry, name), root, |reader, element| T::read(reader, element, root))
    }
}

struct Writer<'xml> {
    xot: &'xml mut Xot,
    entry: &'xml str,
    store: &'xml Store<'xml>,
}

impl<'xml> Writer<'xml> {
    fn xml(&self) -> impl Fn(xot::Error) -> IfcError + use<'xml> {
        let entry = self.entry;
        move |source| IfcError::Xml { entry: entry.into(), source }
    }

    fn append(&mut self, parent: Node, name: &str) -> Result<Node, IfcError> {
        let id = self.xot.add_name(name);
        let child = self.xot.new_element(id);
        self.xot.append(parent, child).map(|()| child).map_err(self.xml())
    }

    fn text(&mut self, node: Node, element: &'static str, text: &str) -> Result<(), IfcError> {
        let child = self.append(node, element)?;
        self.xot.append_text(child, text).map_err(self.xml())
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
        value
            .items()
            .try_fold(None, |parent, item| {
                let held = parent.map_or_else(|| self.holders(node, holders), Ok)?;
                let child = self.append(held, element)?;
                item.write(self, child).map(|()| Some(held))
            })
            .map(drop)
    }

    fn attribute<T>(&mut self, node: Node, holders: &[&'static str], attribute: &'static str, value: &T) -> Result<(), IfcError>
    where T: Occurs<Item: Simple> {
        let name = self.xot.add_name(attribute);
        let (containers, repeated) = match (T::MANY, holders.split_last()) {
            (true, Some((holder, containers))) => (containers, Some(*holder)),
            (true, None) | (false, _) => (holders, None),
        };
        value
            .items()
            .try_fold(None, |parent, item| {
                let held = parent.map_or_else(|| self.holders(node, containers), Ok)?;
                let target = repeated.map_or(Ok(held), |holder| self.append(held, holder))?;
                self.xot.set_attribute(target, name, item.encode());
                Ok(Some(held))
            })
            .map(drop)
    }

    fn keyed<K, V>(&mut self, node: Node, holders: &[&'static str], element: &'static str, index: Option<&'static str>, entries: &IndexMap<TypedUuid<K>, V>) -> Result<(), IfcError>
    where
        K: Kind,
        V: Keyed<TypedUuid<K>>,
    {
        iter::zip((0_u32..).map(f64::from), entries)
            .try_fold(None, |parent, (position, (&key, value))| {
                let held = parent.map_or_else(|| self.holders(node, holders), Ok)?;
                let child = self.append(held, element)?;
                self.attribute(child, &[], GUID, &key)?;
                value.write_keyed(key, self, child)?;
                index.map_or(Ok(()), |rank| self.element(child, &[], rank, &position)).map(|()| Some(held))
            })
            .map(drop)
    }

    fn resource<T>(&mut self, node: Node, external: bool, value: &T) -> Result<(), IfcError>
    where T: Occurs<Item = markup::Resource> {
        value.items().try_for_each(|resource| match resource {
            markup::Resource::External(reference) => {
                self.attribute(node, &[], IS_EXTERNAL, &(!external).then_some(true))?;
                self.element(node, &[], REFERENCE, reference)
            }
            markup::Resource::Archived { reference, content } => {
                self.attribute(node, &[], IS_EXTERNAL, &external.then_some(false))?;
                self.element(node, &[], REFERENCE, reference)?;
                self.file(reference, content, CompressionMethod::Deflated)
            }
        })
    }

    fn image<T>(&mut self, node: Node, element: &'static str, value: &T) -> Result<(), IfcError>
    where T: Occurs<Item = Image> {
        value.items().try_for_each(|Image { format, content }| {
            let name = format!("{}.{}", HEXLOWER.encode(digest::digest(&digest::SHA256, content).as_ref()), <&str>::from(format));
            self.text(node, element, &name)?;
            self.file(&name, content, CompressionMethod::Stored)
        })
    }

    fn file(&self, name: &str, content: &[u8], method: CompressionMethod) -> Result<(), IfcError> { (self.store)(&resolve(self.entry, name), content, method) }

    fn document(&self, name: &str, root: &'static str, write: impl FnOnce(&mut Writer<'_>, Node) -> Result<(), IfcError>) -> Result<(), IfcError> {
        compose(self.store, &resolve(self.entry, name), root, write)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ARCHIVES]

pub fn read<R>(source: R) -> Result<Issues, NEVec<IfcError>>
where R: Read + Seek {
    fn optional<T>(open: &Open<'_>, entry: &str, root: &'static str, read: impl FnOnce(&Reader<'_>, Node) -> Validated<T, IfcError>) -> Validated<Option<T>, IfcError> {
        match open(entry) {
            Ok(bytes) => parse(open, entry, &bytes, root, read).map(Some),
            Err(ZipError::FileNotFound) => Validated::Good(None),
            Err(source) => Validated::fail(archive_error(entry)(source)),
        }
    }
    let zip = ZipArchive::new(source).map_err(|source| nev![archive_error(CENTRAL_DIRECTORY)(source)])?;
    let listed = zip.file_names().enumerate().map(|(index, name)| name.map(|raw| (resolve("", &raw), index)).map_err(archive_error(CENTRAL_DIRECTORY))).collect::<Validated<Vec<_>, _>>().ok()?;
    let entries = listed.iter().map(|(name, index)| (name.to_ascii_lowercase(), *index)).collect::<HashMap<_, _>>();
    let zip = RefCell::new(zip);
    let open = |entry: &str| -> Result<Box<[u8]>, ZipError> {
        let index = *entries.get(&entry.to_ascii_lowercase()).ok_or(ZipError::FileNotFound)?;
        let mut archive = zip.borrow_mut();
        let mut file = archive.by_index(index)?;
        let mut content = Vec::new();
        file.read_to_end(&mut content).map_err(ZipError::Io)?;
        Ok(content.into_boxed_slice())
    };
    let found = document(&open, VERSION_ENTRY, VERSION_ROOT, |reader, root| reader.attribute::<Text>(root, &[], VERSION_ID)).ok()?;
    (*found == *RELEASE).ok_or_else(|| nev![IfcError::Version { found: found.into_inner().into_boxed_str() }])?;
    let topics = listed
        .iter()
        .filter(|(name, _)| name.split_once('/').is_some_and(|(_, file)| file.eq_ignore_ascii_case(MARKUP_ENTRY)))
        .map(|(entry, _)| {
            document(&open, entry, MARKUP_ROOT, |reader, root| reader.attribute::<TopicUuid>(root, &[TOPIC], GUID).map2(markup::Topic::read(reader, root, MARKUP_ROOT), |guid, topic| (guid, topic)))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .and_then(|held| unique(CENTRAL_DIRECTORY, held))
        .map_or_else(Validated::Fail, Validated::Good);
    optional(&open, PROJECT_ENTRY, PROJECT_ROOT, |reader, root| reader.element(root, &[], PROJECT))
        .map4(
            optional(&open, EXTENSIONS_ENTRY, EXTENSIONS_ROOT, |reader, root| Extensions::read(reader, root, EXTENSIONS_ROOT)),
            optional(&open, DOCUMENTS_ENTRY, DOCUMENTS_ROOT, |reader, root| reader.keyed(root, &[DOCUMENTS], DOCUMENT, None)),
            topics,
            |project, extensions, documents, topics| Issues { project, extensions: extensions.unwrap_or_default(), documents: documents.unwrap_or_default(), topics },
        )
        .ok()
}

pub fn write<W>(issues: &Issues, sink: W) -> Result<W, IfcError>
where W: Write + Seek {
    let Issues { project, extensions, documents, topics } = issues;
    let zip = RefCell::new(ZipWriter::new(sink));
    let written = RefCell::new(HashMap::<String, Digest>::new());
    let store = |entry: &str, content: &[u8], method: CompressionMethod| -> Result<(), IfcError> {
        let held = digest::digest(&digest::SHA256, content);
        match written.borrow_mut().insert(entry.to_owned(), held) {
            Some(previous) if previous.as_ref() == held.as_ref() => Ok(()),
            Some(_) | None => {
                let large = nalgebra::convert::<usize, f64>(content.len()) >= nalgebra::convert::<u64, f64>(ZIP64_BYTES_THR);
                let mut archive = zip.borrow_mut();
                archive.start_file(entry, SimpleFileOptions::DEFAULT.compression_method(method).large_file(large)).map_err(archive_error(entry))?;
                archive.write_all(content).map_err(|source| archive_error(entry)(ZipError::Io(source)))
            }
        }
    };
    let directory = |folder: &str| zip.borrow_mut().add_directory(folder, SimpleFileOptions::DEFAULT).map_err(archive_error(folder));
    compose(&store, VERSION_ENTRY, VERSION_ROOT, |writer, root| {
        let name = writer.xot.add_name(VERSION_ID);
        writer.xot.set_attribute(root, name, RELEASE);
        Ok(())
    })?;
    if *extensions != Extensions::default() {
        compose(&store, EXTENSIONS_ENTRY, EXTENSIONS_ROOT, |writer, root| extensions.write(writer, root))?;
    }
    if let Some(info) = project {
        compose(&store, PROJECT_ENTRY, PROJECT_ROOT, |writer, root| writer.element(root, &[], PROJECT, info))?;
    }
    if !documents.is_empty() {
        directory(DOCUMENTS)?;
        compose(&store, DOCUMENTS_ENTRY, DOCUMENTS_ROOT, |writer, root| writer.keyed(root, &[DOCUMENTS], DOCUMENT, None, documents))?;
    }
    topics.iter().try_for_each(|(guid, topic)| {
        let folder = guid.to_string();
        directory(&folder)?;
        compose(&store, &format!("{folder}/{MARKUP_ENTRY}"), MARKUP_ROOT, |writer, root| {
            topic.write(writer, root)?;
            writer.attribute(root, &[TOPIC], GUID, guid)
        })
    })?;
    zip.into_inner().finish().map_err(archive_error(CENTRAL_DIRECTORY))
}

// --- [ENTRIES]

fn document<T>(open: &Open<'_>, entry: &str, root: &'static str, read: impl FnOnce(&Reader<'_>, Node) -> Validated<T, IfcError>) -> Validated<T, IfcError> {
    open(entry).map_err(archive_error(entry)).map_or_else(Validated::fail, |bytes| parse(open, entry, &bytes, root, read))
}

fn parse<T>(open: &Open<'_>, entry: &str, bytes: &[u8], root: &'static str, read: impl FnOnce(&Reader<'_>, Node) -> Validated<T, IfcError>) -> Validated<T, IfcError> {
    let mut xot = Xot::new();
    xot.parse_bytes(bytes).map_err(|source| IfcError::Parse { entry: entry.into(), source }).map_or_else(Validated::fail, |document| {
        let reader = Reader { xot: &xot, entry, open };
        reader.named(document, root).next().map_or_else(|| Validated::fail(reader.lacks(root)), |element| read(&reader, element))
    })
}

fn compose(store: &Store<'_>, entry: &str, root: &'static str, write: impl FnOnce(&mut Writer<'_>, Node) -> Result<(), IfcError>) -> Result<(), IfcError> {
    let mut xot = Xot::new();
    let name = xot.add_name(root);
    let element = xot.new_element(name);
    let mut writer = Writer { xot: &mut xot, entry, store };
    let document = writer.xot.new_document_with_element(element).map_err(writer.xml())?;
    write(&mut writer, element)?;
    let text = writer
        .xot
        .serialize_xml_string(Parameters { declaration: Some(Declaration { encoding: Some("UTF-8".to_owned()), standalone: None }), ..Parameters::default() }, document)
        .map_err(writer.xml())?;
    store(entry, text.as_bytes(), CompressionMethod::Deflated)
}

fn resolve(entry: &str, name: &str) -> String {
    let (folder, _) = entry.rsplit_once('/').unwrap_or_default();
    Path::new(folder)
        .join(name.replace('\\', "/"))
        .components()
        .fold(Vec::new(), |mut segments, component| {
            match component {
                Component::ParentDir => drop(segments.pop()),
                Component::Normal(segment) => segments.extend(segment.to_str()),
                Component::Prefix(_) | Component::RootDir | Component::CurDir => {}
            }
            segments
        })
        .join("/")
}

fn document_entry(guid: DocumentUuid) -> String { format!("{DOCUMENTS}/{guid}") }

fn unique<K, V>(entry: &str, entries: Vec<(TypedUuid<K>, V)>) -> Result<IndexMap<TypedUuid<K>, V>, NEVec<IfcError>>
where K: Kind {
    NEVec::try_from_vec(entries.iter().map(|&(key, _)| key).duplicates().map(|key| IfcError::Duplicate { entry: entry.into(), guid: K::guid(key) }).collect())
        .map_or_else(|| Ok(entries.into_iter().collect()), Err)
}

fn archive_error(entry: &str) -> impl Fn(ZipError) -> IfcError { move |source| IfcError::Archive { entry: entry.into(), source } }

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod markup;
pub mod visualization;
