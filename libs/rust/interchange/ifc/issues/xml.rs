use core::{hash::Hash, iter};

use aws_lc_rs::digest::{self, Digest};
use indexmap::{IndexMap, IndexSet};
use itertools::{EitherOrBoth, Itertools as _};
use jiff::{Timestamp, fmt::temporal::Pieces, tz::Offset};
use newtype_uuid::{GenericUuid as _, TypedUuid, TypedUuidKind};
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use typed_path::Utf8UnixPath;
use uuid::Uuid;
use validated::Validated;
use xot::{
    Node, Xot,
    output::xml::{Declaration, Parameters},
};
use zip::{CompressionMethod, result::ZipError};

use super::{ASPECT_RATIO, FILE, GUID, Guid, IS_EXTERNAL, ORTHOGONAL_CAMERA, PERSPECTIVE_CAMERA, PROJECT_INFO, TOPIC};
use crate::{Constraint, IfcError, bind};

// --- [TYPES] ---------------------------------------------------------------------------

// --- [ENTRIES]

pub(super) type Open<'archive> = dyn Fn(&str) -> Result<Box<[u8]>, ZipError> + 'archive;
pub(super) type Store<'archive> = dyn Fn(&str, &[u8], Digest, CompressionMethod) -> Result<(), IfcError> + 'archive;

// --- [VALUES]

pub(super) trait Simple: Sized {
    fn decode(text: &str) -> Result<Self, IfcError>;
    fn encode(&self) -> String;
}

impl Simple for f64 {
    fn decode(text: &str) -> Result<Self, IfcError> {
        text.parse::<Self>()
            .map_err(|source| IfcError::Number { text: text.into(), source })
            .and_then(|number| number.is_finite().ok_or(IfcError::Constrained(Constraint::Finite(number))).map(|()| number))
    }

    fn encode(&self) -> String { self.to_string() }
}

impl Simple for i32 {
    fn decode(text: &str) -> Result<Self, IfcError> { text.parse().map_err(|source| IfcError::Integer { text: text.into(), source }) }

    fn encode(&self) -> String { self.to_string() }
}

impl Simple for bool {
    fn decode(text: &str) -> Result<Self, IfcError> {
        match text {
            "true" | "1" => Ok(true),
            "false" | "0" => Ok(false),
            _ => Err(IfcError::Constrained(Constraint::Boolean(text.into()))),
        }
    }

    fn encode(&self) -> String { self.to_string() }
}

impl Simple for Timestamp {
    fn decode(text: &str) -> Result<Self, IfcError> {
        Pieces::parse(text)
            .and_then(|pieces| pieces.to_numeric_offset().unwrap_or(Offset::UTC).to_timestamp(pieces.date().to_datetime(pieces.time().unwrap_or_default())))
            .map_err(|source| IfcError::Timestamp { text: text.into(), source })
    }

    fn encode(&self) -> String { self.to_string() }
}

impl<K> Simple for TypedUuid<K>
where K: TypedUuidKind
{
    fn decode(text: &str) -> Result<Self, IfcError> { text.parse::<Uuid>().map(Self::from_untyped_uuid).map_err(|source| IfcError::Uuid { text: text.into(), source }) }

    fn encode(&self) -> String { self.to_string() }
}

// --- [OCCURRENCE]

pub(super) trait Codec: Sized {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError>;
    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError>;
    fn present(_: &Xot, _: Node) -> bool { true }
}

impl<T> Codec for T
where T: Simple
{
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> {
        let text = reader.xot.string_value(node);
        Validated::from(Self::decode(text.trim()).map_err(reader.malformed(local_name(reader.xot, node))))
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { writer.xot.append_text(node, &self.encode()).map_err(writer.xml()) }

    fn present(xot: &Xot, node: Node) -> bool { stated(xot, node) }
}

impl Codec for () {
    fn read(_: &Reader<'_>, _: Node) -> Validated<Self, IfcError> { Validated::Good(()) }

    fn write(&self, _: &mut Writer<'_>, _: Node) -> Result<(), IfcError> { Ok(()) }
}

pub(super) trait Occurs: Sized {
    type Item;
    fn occurs(items: Vec<Self::Item>) -> Option<Self>;
    fn items(&self) -> impl Iterator<Item = &Self::Item>;
}

impl<T> Occurs for T
where T: Codec
{
    type Item = Self;

    fn occurs(items: Vec<Self>) -> Option<Self> { items.into_iter().next() }

    fn items(&self) -> impl Iterator<Item = &Self> { iter::once(self) }
}

impl<T> Occurs for Option<T>
where T: Codec
{
    type Item = T;

    fn occurs(items: Vec<T>) -> Option<Self> { Some(items.into_iter().next()) }

    fn items(&self) -> impl Iterator<Item = &T> { self.iter() }
}

impl<T> Occurs for Vec<T>
where T: Codec
{
    type Item = T;

    fn occurs(items: Self) -> Option<Self> { Some(items) }

    fn items(&self) -> impl Iterator<Item = &T> { self.iter() }
}

impl<T> Occurs for NEVec<T>
where T: Codec
{
    type Item = T;

    fn occurs(items: Vec<T>) -> Option<Self> { Self::try_from_vec(items) }

    fn items(&self) -> impl Iterator<Item = &T> { self.iter() }
}

impl<T> Occurs for IndexSet<T>
where T: Codec + Hash + Eq
{
    type Item = T;

    fn occurs(items: Vec<T>) -> Option<Self> { Some(items.into_iter().collect()) }

    fn items(&self) -> impl Iterator<Item = &T> { self.iter() }
}

// --- [RECORDS]

pub(super) trait Keyed<K>: Sized {
    fn read_keyed(reader: &Reader<'_>, node: Node, key: K) -> Validated<Self, IfcError>;
    fn write_keyed(&self, key: K, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError>;
}

impl<K, T> Keyed<K> for T
where T: Codec
{
    fn read_keyed(reader: &Reader<'_>, node: Node, _: K) -> Validated<Self, IfcError> { Self::read(reader, node) }

    fn write_keyed(&self, _: K, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { self.write(writer, node) }
}

macro_rules! codec {
    (@read $reader:ident $node:ident inline $field:ident) => { Codec::read($reader, $node) };
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
            fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> {
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

pub(super) use codec;

// --- [SERVICES] ------------------------------------------------------------------------

pub(super) struct Reader<'xml> {
    pub(super) xot: &'xml Xot,
    pub(super) entry: &'xml str,
    pub(super) open: &'xml Open<'xml>,
    pub(super) upgrading: bool,
}

impl Reader<'_> {
    pub(super) fn refused(&self, element: &str, text: Option<&str>) -> IfcError { IfcError::Element { entry: self.entry.into(), element: element.into(), text: text.map(Into::into) } }

    fn malformed(&self, element: &str) -> impl FnOnce(IfcError) -> IfcError { move |source| IfcError::Content { entry: self.entry.into(), element: element.into(), source: Box::new(source) } }

    fn occurs<T>(&self, items: Validated<Vec<T::Item>, IfcError>, element: &str) -> Validated<T, IfcError>
    where T: Occurs {
        bind(items, |found| T::occurs(found).map_or_else(|| Validated::fail(self.refused(element, None)), Validated::Good))
    }

    pub(super) fn element<T>(&self, node: Node, ancestors: &[&'static str], element: &'static str) -> Validated<T, IfcError>
    where T: Occurs<Item: Codec> {
        self.occurs(elements(self.xot, node, ancestors).into_iter().flat_map(|ancestor| named(self.xot, ancestor, element)).filter(|&child| T::Item::present(self.xot, child)).map(|child| T::Item::read(self, child)).collect(), element)
    }

    pub(super) fn attribute<T>(&self, node: Node, ancestors: &[&'static str], attribute: &'static str) -> Validated<T, IfcError>
    where T: Occurs<Item: Simple> {
        let xot = self.xot;
        self.occurs(
            elements(xot, node, ancestors)
                .into_iter()
                .filter_map(|ancestor| xot.name(attribute).and_then(|id| xot.get_attribute(ancestor, id)))
                .map(str::trim)
                .filter(|text| !text.is_empty())
                .map(|text| T::Item::decode(text).map_err(self.malformed(attribute)))
                .collect(),
            attribute,
        )
    }

    pub(super) fn keyed<K, V>(&self, node: Node, ancestors: &[&'static str], element: &'static str, index: Option<&'static str>) -> Validated<IndexMap<TypedUuid<K>, V>, IfcError>
    where
        K: TypedUuidKind,
        TypedUuid<K>: Into<Guid>,
        V: Keyed<TypedUuid<K>>,
    {
        let entries = elements(self.xot, node, ancestors)
            .into_iter()
            .flat_map(|ancestor| named(self.xot, ancestor, element))
            .map(|child| {
                bind(self.attribute::<TypedUuid<K>>(child, &[], GUID), |key| {
                    index.map_or(Validated::Good(None), |rank| self.element::<Option<i32>>(child, &[], rank)).map2(V::read_keyed(self, child, key), |rank, value| (key, (rank, value)))
                })
            })
            .collect::<Validated<Vec<_>, _>>();
        bind(entries, |entries| unique(self.entry, entries)).map(|ranked| ranked.sorted_by(|_, (left, _), _, (right, _)| left.cmp(right)).map(|(key, (_, value))| (key, value)).collect())
    }

    pub(super) fn file(&self, name: &str) -> Result<Box<[u8]>, IfcError> {
        let entry = resolve(self.entry, name);
        (self.open)(&entry).map_err(archive_error(&entry))
    }
}

pub(super) struct Writer<'xml> {
    pub(super) xot: &'xml mut Xot,
    pub(super) entry: &'xml str,
    pub(super) store: &'xml Store<'xml>,
}

impl<'xml> Writer<'xml> {
    pub(super) fn xml(&self) -> impl Fn(xot::Error) -> IfcError + use<'xml> {
        let entry = self.entry;
        move |source| IfcError::Xml { entry: entry.into(), source }
    }

    pub(super) fn append(&mut self, parent: Node, name: &str) -> Result<Node, IfcError> { appended(self.xot, parent, name).map_err(self.xml()) }

    pub(super) fn text(&mut self, node: Node, element: &'static str, text: &str) -> Result<(), IfcError> {
        let child = self.append(node, element)?;
        self.xot.append_text(child, text).map_err(self.xml())
    }

    fn ancestors(&mut self, node: Node, ancestors: &[&str]) -> Result<Node, IfcError> { ancestor(self.xot, node, ancestors).map_err(self.xml()) }

    pub(super) fn element<T>(&mut self, node: Node, ancestors: &[&'static str], element: &'static str, value: &T) -> Result<(), IfcError>
    where T: Occurs<Item: Codec> {
        value.items().try_for_each(|item| {
            let held = self.ancestors(node, ancestors)?;
            let child = self.append(held, element)?;
            item.write(self, child)
        })
    }

    pub(super) fn attribute<T>(&mut self, node: Node, ancestors: &[&'static str], attribute: &'static str, value: &T) -> Result<(), IfcError>
    where T: Occurs<Item: Simple> {
        let name = self.xot.add_name(attribute);
        value.items().try_for_each(|item| {
            let target = self.ancestors(node, ancestors)?;
            self.xot.set_attribute(target, name, item.encode());
            Ok(())
        })
    }

    pub(super) fn keyed<K, V>(&mut self, node: Node, ancestors: &[&'static str], element: &'static str, index: Option<&'static str>, entries: &IndexMap<TypedUuid<K>, V>) -> Result<(), IfcError>
    where
        K: TypedUuidKind,
        V: Keyed<TypedUuid<K>>,
    {
        iter::zip(0_i32.., entries).try_for_each(|(position, (&key, value))| {
            let held = self.ancestors(node, ancestors)?;
            let child = self.append(held, element)?;
            self.attribute(child, &[], GUID, &key)?;
            value.write_keyed(key, self, child)?;
            index.map_or(Ok(()), |rank| self.element(child, &[], rank, &position))
        })
    }

    pub(super) fn file(&self, name: &str, content: &[u8], method: CompressionMethod) -> Result<(), IfcError> { (self.store)(&resolve(self.entry, name), content, digest::digest(&digest::SHA256, content), method) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ENTRIES]

pub(super) fn required<T>(found: Validated<Option<T>, IfcError>, entry: &str) -> Validated<T, IfcError> { bind(found, |held| held.map_or_else(|| Validated::fail(archive_error(entry)(ZipError::FileNotFound)), Validated::Good)) }

pub(super) fn sides<L, R>(reader: &Reader<'_>, left_found: Validated<Option<L>, IfcError>, right_found: Validated<Option<R>, IfcError>, element: &str) -> Validated<EitherOrBoth<L, R>, IfcError> {
    bind(left_found.map2(right_found, |left, right| (left, right)), |stated| match stated {
        (Some(left), Some(right)) => Validated::Good(EitherOrBoth::Both(left, right)),
        (Some(left), None) => Validated::Good(EitherOrBoth::Left(left)),
        (None, Some(right)) => Validated::Good(EitherOrBoth::Right(right)),
        (None, None) => Validated::fail(reader.refused(element, None)),
    })
}

pub(super) fn document<T>(open: &Open<'_>, upgrading: bool, (entry, root): (&str, &'static str), read: impl FnOnce(&Reader<'_>, Node) -> Validated<T, IfcError>) -> Validated<Option<T>, IfcError> {
    let bytes = match open(entry) {
        Ok(bytes) => bytes,
        Err(ZipError::FileNotFound) => return Validated::Good(None),
        Err(source) => return Validated::fail(archive_error(entry)(source)),
    };
    let upgraded = |xot: &mut Xot, document: Node| -> Result<(), xot::Error> {
        const UPGRADES: [(&[&str], &str, &str, &[&str]); 9] = [
            (&["Header"], FILE, FILE, &["Header", "Files"]),
            (&[TOPIC], "ReferenceLink", "ReferenceLink", &[TOPIC, "ReferenceLinks"]),
            (&[TOPIC], "Labels", "Label", &[TOPIC, "Labels"]),
            (&[TOPIC], "DocumentReference", "DocumentReference", &[TOPIC, "DocumentReferences"]),
            (&[TOPIC], "RelatedTopic", "RelatedTopic", &[TOPIC, "RelatedTopics"]),
            (&[], "Comment", "Comment", &[TOPIC, "Comments"]),
            (&[], "Viewpoints", "ViewPoint", &[TOPIC, "Viewpoints"]),
            (&[], "Bitmap", "Bitmap", &["Bitmaps"]),
            (&["Components"], "ViewSetupHints", "ViewSetupHints", &["Components", "Visibility"]),
        ];
        let document_element = xot.document_element(document)?;
        if local_name(xot, document_element) == "ProjectExtension" {
            rename(xot, document_element, PROJECT_INFO.1);
        }
        UPGRADES.iter().try_for_each(|&(parents, child, renamed, ancestors)| -> Result<(), xot::Error> {
            let moved = elements(xot, document_element, parents).into_iter().flat_map(|parent| named(xot, parent, child).collect::<Vec<_>>()).collect::<Vec<_>>();
            moved.iter().try_for_each(|&node| xot.detach(node))?;
            if moved.is_empty() {
                return Ok(());
            }
            let target = ancestor(xot, document_element, ancestors)?;
            moved.into_iter().try_for_each(|node| {
                rename(xot, node, renamed);
                xot.append(target, node)
            })
        })?;
        let (external, is_external, aspect) = (xot.add_name("isExternal"), xot.add_name(IS_EXTERNAL), xot.add_name(ASPECT_RATIO));
        xot.descendants(document_element).collect::<Vec<_>>().into_iter().try_for_each(|node| -> Result<(), xot::Error> {
            if let Some(value) = xot.get_attribute(node, external).map(str::to_owned) {
                xot.set_attribute(node, is_external, value);
            }
            match local_name(xot, node) {
                "ReferencedDocument" => {
                    rename(xot, node, "Url");
                    Ok(())
                }
                ORTHOGONAL_CAMERA | PERSPECTIVE_CAMERA if xot.children(node).all(|child| xot.element(child).is_none_or(|element| element.name() != aspect)) => {
                    let ratio = appended(xot, node, ASPECT_RATIO)?;
                    xot.append_text(ratio, "1")
                }
                _ => Ok(()),
            }
        })
    };
    let mut xot = Xot::new();
    let parsed = xot.parse_bytes(&bytes).map_err(xot::Error::from).and_then(|document| if upgrading { upgraded(&mut xot, document).map(|()| document) } else { Ok(document) });
    let reader = Reader { xot: &xot, entry, open, upgrading };
    bind(Validated::from(parsed.map_err(|source| IfcError::Xml { entry: entry.into(), source })), |document| named(reader.xot, document, root).next().map_or_else(|| Validated::fail(reader.refused(root, None)), |element| read(&reader, element).map(Some)))
}

pub(super) fn stored(store: &Store<'_>, (entry, root): (&str, &'static str), write: impl FnOnce(&mut Writer<'_>, Node) -> Result<(), IfcError>) -> Result<(), IfcError> {
    let mut xot = Xot::new();
    let name = xot.add_name(root);
    let element = xot.new_element(name);
    let mut writer = Writer { xot: &mut xot, entry, store };
    let document = writer.xot.new_document_with_element(element).map_err(writer.xml())?;
    write(&mut writer, element)?;
    let text = writer.xot.serialize_xml_string(Parameters { declaration: Some(Declaration { encoding: Some("UTF-8".to_owned()), standalone: None }), ..Parameters::default() }, document).map_err(writer.xml())?;
    store(entry, text.as_bytes(), digest::digest(&digest::SHA256, text.as_bytes()), CompressionMethod::Deflated)
}

pub(super) fn resolve(entry: &str, name: &str) -> String {
    let (folder, _) = entry.rsplit_once('/').unwrap_or_default();
    Utf8UnixPath::new(folder).join(name.replace('\\', "/")).normalize().as_str().trim_start_matches('/').to_owned()
}

pub(super) fn unique<K, V>(entry: &str, entries: Vec<(TypedUuid<K>, V)>) -> Validated<IndexMap<TypedUuid<K>, V>, IfcError>
where
    K: TypedUuidKind,
    TypedUuid<K>: Into<Guid>,
{
    entries
        .iter()
        .map(|&(key, _)| key)
        .duplicates()
        .map(|key| IfcError::Duplicate { entry: entry.into(), guid: key.into() })
        .try_into_nonempty_iter()
        .map(NonEmptyIterator::collect::<NEVec<_>>)
        .map_or_else(|| Validated::Good(entries.into_iter().collect()), Validated::Fail)
}

pub(super) fn archive_error(entry: &str) -> impl Fn(ZipError) -> IfcError { move |source| IfcError::Archive { entry: entry.into(), source } }

// --- [TREE]

pub(super) fn local_name(xot: &Xot, node: Node) -> &str { xot.element(node).map_or("", |element| xot.local_name_str(element.name())) }

pub(super) fn stated(xot: &Xot, node: Node) -> bool { !xot.string_value(node).trim().is_empty() }

pub(super) fn named<'xml>(xot: &'xml Xot, node: Node, name: &str) -> impl Iterator<Item = Node> + use<'xml> {
    let name = xot.name(name);
    xot.children(node).filter(move |&child| xot.element(child).is_some_and(|element| Some(element.name()) == name))
}

fn elements(xot: &Xot, node: Node, ancestors: &[&str]) -> Vec<Node> { ancestors.iter().fold(vec![node], |nodes, name| nodes.into_iter().flat_map(|parent| named(xot, parent, name)).collect()) }

fn rename(xot: &mut Xot, node: Node, name: &str) {
    let id = xot.add_name(name);
    if let Some(element) = xot.element_mut(node) {
        element.set_name(id);
    }
}

fn appended(xot: &mut Xot, parent: Node, name: &str) -> Result<Node, xot::Error> {
    let id = xot.add_name(name);
    let child = xot.new_element(id);
    xot.append(parent, child).map(|()| child)
}

fn ancestor(xot: &mut Xot, node: Node, names: &[&str]) -> Result<Node, xot::Error> {
    names.iter().try_fold(node, |parent, name| {
        let id = xot.add_name(name);
        let found = xot.children(parent).find(|&child| xot.element(child).is_some_and(|element| element.name() == id));
        found.map_or_else(|| appended(xot, parent, name), Ok)
    })
}
