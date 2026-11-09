#![feature(default_field_values, deref_patterns, float_conversions, gen_blocks)]
#![cfg_attr(feature = "geometry", feature(iterator_try_collect, vec_into_chunks))]

use core::{
    borrow::Borrow,
    hash::Hash,
    mem,
    num::{ParseIntError, TryFromIntError},
    ptr,
    str::FromStr,
};
use std::{
    collections::{HashMap, HashSet},
    sync::Arc,
};

use indexmap::IndexMap;
use itertools::{Either, Itertools as _};
use jiff::fmt::temporal::Pieces;
use nonempty_collections::{NEVec, NonEmptyIterator as _, iter};
use serde::{
    Deserialize, Deserializer, Serialize, Serializer,
    de::{
        IntoDeserializer, Visitor,
        value::{BorrowedStrDeserializer, MapAccessDeserializer, MapDeserializer, SeqDeserializer},
    },
};
use serde_with::{DefaultOnNull, DeserializeFromStr, DisplayFromStr, SerializeDisplay, base64::Base64, serde_as};
use uom::si::{self, f64::Length};
use url::Url;

// --- [MODELS] --------------------------------------------------------------------------

// --- [PATHS]

#[nutype::nutype(
    validate(with = |name: &Arc<str>| -> Result<(), IfcError> { (!name.is_empty() && !name.contains('/')).ok_or_else(|| IfcError::Constrained(Constraint::Name(Arc::clone(name)))) }, error = IfcError),
    derive(Debug, Clone, PartialEq, Eq, Hash, PartialOrd, Ord, Display, AsRef, Deref, Borrow, Serialize, Deserialize)
)]
pub struct Name(Arc<str>);

impl Borrow<str> for Name {
    fn borrow(&self) -> &str { self }
}

#[derive(Debug, Clone, PartialEq, Eq, Hash, PartialOrd, Ord, derive_more::AsRef, derive_more::Display, SerializeDisplay, DeserializeFromStr)]
#[display("{}", _0.iter().format("/"))]
pub struct Path(NEVec<Name>);

impl Path {
    #[must_use]
    pub fn head(&self) -> &Name { self.0.first() }

    #[must_use]
    pub fn child(&self, name: Name) -> Self {
        let mut names = self.0.clone();
        names.push(name);
        Self(names)
    }
}

impl Borrow<[Name]> for Path {
    fn borrow(&self) -> &[Name] { self.0.iter().as_slice() }
}

impl FromStr for Path {
    type Err = IfcError;

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        let (head, names) = s.split_once('/').map_or((s, None), |(head, names)| (head, Some(names)));
        iter::once(head).chain(names.into_iter().flat_map(|names| names.split('/'))).map(|name| Name::try_new(Arc::<str>::from(name))).collect::<Result<NEVec<_>, _>>().map(Self)
    }
}

// --- [WIRES]

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct Layer {
    pub header: Header,
    pub imports: Vec<Import>,
    pub schemas: IndexMap<Box<str>, Schema>,
    pub data: Vec<Node>,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Header {
    pub id: Box<str>,
    pub ifcx_version: IfcxVersion,
    pub data_version: Box<str>,
    pub author: Box<str>,
    pub timestamp: Box<str>,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum IfcxVersion {
    #[serde(rename = "ifcx_alpha")]
    Alpha,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct Import {
    pub uri: Box<str>,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub integrity: Option<Box<str>>,
}

#[serde_as]
#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(bound(serialize = "V: Serialize", deserialize = "V: Deserialize<'de>"))]
pub struct Node<V = serde_json::Value> {
    pub path: Path,
    #[serde_as(as = "DefaultOnNull")]
    #[serde(default, skip_serializing_if = "IndexMap::is_empty")]
    pub children: IndexMap<Name, Option<Path>>,
    #[serde_as(as = "DefaultOnNull")]
    #[serde(default, skip_serializing_if = "IndexMap::is_empty")]
    pub inherits: IndexMap<Name, Option<Path>>,
    #[serde_as(as = "DefaultOnNull")]
    #[serde(default, skip_serializing_if = "IndexMap::is_empty")]
    pub attributes: IndexMap<Box<str>, Option<V>>,
}

impl<V> Node<V> {
    #[must_use]
    pub fn new(path: Path) -> Self { Self { path, children: IndexMap::new(), inherits: IndexMap::new(), attributes: IndexMap::new() } }
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct Schema {
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub uri: Option<Box<str>>,
    pub value: ValueDescription,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct ValueDescription {
    #[serde(flatten)]
    pub data_type: DataType,
    #[serde(default, skip_serializing_if = "core::ops::Not::not")]
    pub optional: bool,
    #[serde(default, skip_serializing_if = "Vec::is_empty")]
    pub inherits: Vec<Box<str>>,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize, derive_more::Display)]
#[serde(tag = "dataType")]
pub enum DataType {
    #[display("a real number")]
    Real {
        #[serde(rename = "quantityKind", default, skip_serializing_if = "Option::is_none")]
        quantity_kind: Option<QuantityKind>,
    },
    #[display("a boolean")]
    Boolean,
    #[display("an integer")]
    Integer,
    #[display("a string")]
    String,
    #[display("an ISO 8601 date")]
    DateTime,
    #[display("one of {}", restrictions.options.join(", "))]
    Enum {
        #[serde(rename = "enumRestrictions")]
        restrictions: EnumRestrictions,
    },
    #[display("an array of {} to {} items", restrictions.min.unwrap_or_default(), restrictions.max.map_or(Either::Left("any"), Either::Right))]
    Array {
        #[serde(rename = "arrayRestrictions")]
        restrictions: Box<ArrayRestrictions>,
    },
    #[display("an object")]
    Object {
        #[serde(rename = "objectRestrictions", default, skip_serializing_if = "Option::is_none")]
        restrictions: Option<ObjectRestrictions>,
    },
    #[display("a node path")]
    Reference,
    #[display("base64 text")]
    Blob,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct EnumRestrictions {
    pub options: Vec<Box<str>>,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct ArrayRestrictions {
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub min: Option<usize>,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub max: Option<usize>,
    pub value: ValueDescription,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct ObjectRestrictions {
    pub values: IndexMap<Box<str>, ValueDescription>,
}

// --- [QUANTITIES]

#[nutype::nutype(
    validate(
        with = |length: &Length| -> Result<(), IfcError> {
            (length.is_normal() && length.is_sign_positive()).ok_or(IfcError::Constrained(Constraint::Length(length.get::<si::length::meter>())))
        },
        error = IfcError
    ),
    derive(Debug, Clone, Copy, PartialEq, PartialOrd, Deref)
)]
pub struct PositiveLength(Length);

macro_rules! quantities {
    ($($kind:ident($quantity:ident, $module:ident::$unit:ident, $name:literal $(, $alias:literal)?)),+ $(,)?) => {
        #[derive(Debug, Clone, Copy, PartialEq, strum::EnumDiscriminants)]
        #[strum_discriminants(name(QuantityKind), derive(Hash, Serialize, Deserialize))]
        pub enum Quantity {
            $(
                #[strum_discriminants(serde(rename = $name $(, alias = $alias)?))]
                $kind(si::f64::$quantity),
            )+
        }

        impl From<Quantity> for f64 {
            fn from(value: Quantity) -> Self {
                match value {
                    $(Quantity::$kind(quantity) => quantity.get::<si::$module::$unit>(),)+
                }
            }
        }

        fn stored(kind: QuantityKind, value: f64) -> Quantity {
            match kind {
                $(QuantityKind::$kind => Quantity::$kind(si::f64::$quantity::new::<si::$module::$unit>(value)),)+
            }
        }
    };
}

quantities! {
    PlaneAngle(Angle, angle::degree, "Plane angle", "Angle"),
    ThermodynamicTemperature(ThermodynamicTemperature, thermodynamic_temperature::kelvin, "Thermodynamic temperature", "Temperature"),
    ElectricCurrent(ElectricCurrent, electric_current::ampere, "Electric current", "ElectricCurrent"),
    Time(Time, time::second, "Time"),
    Frequency(Frequency, frequency::hertz, "Frequency"),
    Mass(Mass, mass::kilogram, "Mass"),
    Length(Length, length::meter, "Length"),
    LinearVelocity(Velocity, velocity::meter_per_second, "Linear velocity", "Velocity"),
    Force(Force, force::newton, "Force"),
    Pressure(Pressure, pressure::pascal, "Pressure"),
    Area(Area, area::square_meter, "Area"),
    Energy(Energy, energy::joule, "Energy"),
    Power(Power, power::watt, "Power"),
    Volume(Volume, volume::cubic_meter, "Volume"),
}

impl Serialize for Quantity {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where S: Serializer {
        serializer.serialize_f64(f64::from(*self))
    }
}

// --- [VALUES]

#[serde_as]
#[derive(Debug, Clone, PartialEq, Serialize)]
#[serde(untagged)]
pub enum Value {
    Boolean(bool),
    Integer(i64),
    Real(f64),
    Quantity(Quantity),
    String(Box<str>),
    DateTime(#[serde_as(as = "Box<DisplayFromStr>")] Box<Pieces<'static>>),
    Array(Vec<Self>),
    Object(Box<IndexMap<Box<str>, Self>>),
    Reference(Path),
    Blob(#[serde_as(as = "Base64")] Box<[u8]>),
}

impl<'de> Deserializer<'de> for &'de Value {
    type Error = serde_json::Error;

    fn deserialize_any<V>(self, visitor: V) -> Result<V::Value, Self::Error>
    where V: Visitor<'de> {
        match self {
            Value::Boolean(value) => visitor.visit_bool(*value),
            Value::Integer(value) => visitor.visit_i64(*value),
            Value::Real(value) => visitor.visit_f64(*value),
            Value::Quantity(quantity) => visitor.visit_f64(f64::from(*quantity)),
            Value::String(text) => visitor.visit_borrowed_str(text),
            Value::DateTime(pieces) => visitor.visit_string(pieces.to_string()),
            Value::Reference(path) => visitor.visit_string(path.to_string()),
            Value::Blob(bytes) => visitor.visit_borrowed_bytes(bytes),
            Value::Array(items) => {
                let mut elements = SeqDeserializer::new(items.iter());
                let decoded = visitor.visit_seq(&mut elements)?;
                elements.end().map(|()| decoded)
            }
            Value::Object(entries) => {
                let mut fields = MapDeserializer::new(entries.iter().map(|(key, value)| (BorrowedStrDeserializer::new(key), value)));
                let decoded = visitor.visit_map(&mut fields)?;
                fields.end().map(|()| decoded)
            }
        }
    }

    fn deserialize_option<V>(self, visitor: V) -> Result<V::Value, Self::Error>
    where V: Visitor<'de> {
        visitor.visit_some(self)
    }

    fn deserialize_newtype_struct<V>(self, _: &'static str, visitor: V) -> Result<V::Value, Self::Error>
    where V: Visitor<'de> {
        visitor.visit_newtype_struct(self)
    }

    fn deserialize_enum<V>(self, _: &'static str, _: &'static [&'static str], visitor: V) -> Result<V::Value, Self::Error>
    where V: Visitor<'de> {
        match self {
            Value::String(text) => visitor.visit_enum(BorrowedStrDeserializer::new(text)),
            Value::Object(entries) if entries.len() == 1 =>
                visitor.visit_enum(MapAccessDeserializer::new(MapDeserializer::new(entries.iter().map(|(key, value)| (BorrowedStrDeserializer::new(key), value))))),
            Value::Boolean(_) | Value::Integer(_) | Value::Real(_) | Value::Quantity(_) | Value::DateTime(_) | Value::Array(_) | Value::Object(_) | Value::Reference(_) | Value::Blob(_) =>
                self.deserialize_any(visitor),
        }
    }

    serde::forward_to_deserialize_any! {
        bool i8 i16 i32 i64 i128 u8 u16 u32 u64 u128 f32 f64 char str string
        bytes byte_buf unit unit_struct seq tuple tuple_struct map struct identifier ignored_any
    }
}

impl<'de> IntoDeserializer<'de, serde_json::Error> for &'de Value {
    type Deserializer = Self;

    fn into_deserializer(self) -> Self { self }
}

// --- [MODEL]

#[derive(Debug, Clone)]
pub struct Composed {
    pub path: Path,
    pub attributes: IndexMap<Box<str>, Arc<Value>>,
    pub children: IndexMap<Name, Arc<Self>>,
}

impl PartialEq for Composed {
    fn eq(&self, other: &Self) -> bool {
        let mut pending = Vec::new();
        let mut seen = HashSet::new();
        let mut pair = (self, other);
        loop {
            let (left, right) = pair;
            if left.children.is_empty() || seen.insert((ptr::from_ref(left), ptr::from_ref(right))) {
                if left.path != right.path || left.attributes != right.attributes || left.children.len() != right.children.len() {
                    return false;
                }
                if left
                    .children
                    .iter()
                    .try_for_each(|(name, child)| {
                        pending.push((child.as_ref(), right.children.get(name)?.as_ref()));
                        Some(())
                    })
                    .is_none()
                {
                    return false;
                }
            }
            let Some(next) = pending.pop() else { return true };
            pair = next;
        }
    }
}

impl Drop for Composed {
    fn drop(&mut self) {
        let mut pending = mem::take(&mut self.children).into_values().collect::<Vec<_>>();
        while let Some(child) = pending.pop() {
            if let Some(mut child) = Arc::into_inner(child) {
                pending.extend(mem::take(&mut child.children).into_values());
            }
        }
    }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Model {
    schemas: IndexMap<Box<str>, Schema>,
    heads: IndexMap<Name, Arc<Composed>>,
    roots: Vec<Arc<Composed>>,
    nodes: Vec<Arc<Composed>>,
    holders: HashMap<Path, Vec<(Name, Arc<Composed>)>>,
    holding: hashbrown::HashMap<String, Vec<Arc<Composed>>>,
}

impl Model {
    #[must_use]
    pub const fn schemas(&self) -> &IndexMap<Box<str>, Schema> { &self.schemas }

    pub fn roots(&self) -> impl Iterator<Item = &Composed> { self.roots.iter().map(Arc::as_ref) }

    pub fn nodes(&self) -> impl Iterator<Item = &Composed> { self.nodes.iter().map(Arc::as_ref) }

    pub fn head(&self, name: &str) -> Option<&Composed> { self.heads.get(name).map(Arc::as_ref) }

    pub fn get(&self, path: &Path) -> Result<&Composed, IfcError> { descend(&self.heads, path).map(Arc::as_ref).ok_or_else(|| IfcError::Missing { path: path.clone() }) }

    pub fn holding(&self, id: &str) -> impl Iterator<Item = &Composed> { self.holding.get(id).into_iter().flatten().map(Arc::as_ref) }

    pub fn parents(&self, path: &Path) -> impl Iterator<Item = &Composed> { self.holders.get(path).into_iter().flatten().map(|(_, holder)| holder.as_ref()) }

    pub fn ancestors<'model>(&'model self, path: &Path) -> impl Iterator<Item = &'model Composed> {
        reached(self.parents(path), |node: &'model Composed| &node.path, move |node: &'model Composed| self.parents(&node.path))
    }

    #[must_use]
    pub fn occurrences(&self, path: &Path) -> Vec<Path> {
        let Some(holders) = self.holders.get(path) else { return Vec::from([path.clone()]) };
        let reachable = reached(holders.iter().rev().map(|(name, holder)| holder.children[name].as_ref()), ptr::from_ref, |node: &Composed| {
            self.holders.get(&node.path).into_iter().flatten().rev().filter(move |(name, holder)| ptr::eq(holder.children[name].as_ref(), node)).map(|(_, holder)| holder.as_ref())
        })
        .collect::<Vec<_>>();
        let relevant = reachable.iter().copied().map(ptr::from_ref).collect::<HashSet<_>>();
        let mut paths = Vec::new();
        let mut names = Vec::new();
        let mut pending = reachable.into_iter().filter(|node| !self.holders.contains_key(&node.path)).rev().map(|node| Some((node, &node.path, None))).collect::<Vec<_>>();
        while let Some(next) = pending.pop() {
            let (node, root) = match next {
                Some((node, root, Some(name))) => {
                    names.push(name);
                    pending.push(None);
                    (node, root)
                }
                Some((node, root, None)) => (node, root),
                None => {
                    names.truncate(names.len().strict_sub(1));
                    continue;
                }
            };
            if node.path == *path {
                paths.push(Path(root.0.nonempty_iter().chain(names.iter().copied()).cloned().collect()));
            }
            pending.extend(node.children.iter().rev().filter(|(_, child)| relevant.contains(&Arc::as_ptr(child))).map(|(name, child)| Some((child.as_ref(), root, Some(name)))));
        }
        paths
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Refusal {
    #[display("unsupported scheme")]
    Scheme,
    #[display("integrity mismatch")]
    Integrity,
}

#[derive(Debug, Clone, Copy, PartialEq, thiserror::Error)]
pub enum Fault {
    #[error("has no schema")]
    Undeclared,
    #[error("is absent")]
    Absent,
    #[cfg(feature = "pcd")]
    #[error("lacks PCD field {0}")]
    Column(geometry::Column),
    #[error("holds {colors} colors for {points} points")]
    Colors { colors: usize, points: usize },
    #[error("holds {count} values outside whole tuples")]
    Partial { count: usize },
    #[error("counts curve vertices other than its {points} points")]
    Counts { points: usize },
    #[error("names vertex {index} of {points} points")]
    Face { index: u32, points: usize },
    #[error("holds {value} outside its range")]
    Range { value: f64 },
    #[error("is singular or projective")]
    Singular,
    #[error("cants {cant} past rail head distance {distance}")]
    Cant { cant: f64, distance: f64 },
    #[error("names a segment type IFC 4.3 does not define")]
    Unknown,
    #[error("lacks the values that determine its geometry")]
    Underdetermined,
}

#[derive(Debug, Clone, PartialEq, derive_more::Display)]
pub enum Constraint {
    #[display("node name {_0} is empty or holds a slash")]
    Name(Arc<str>),
    #[display("length {_0} m is not positive")]
    Length(f64),
    #[display("fraction {_0} lies outside 0 to 1")]
    Fraction(f32),
}

#[derive(Debug, thiserror::Error)]
pub enum IfcError {
    #[error("{} holds no IFCX layer", .url.as_ref().map_or("input", Url::as_str))]
    Decode { url: Option<Url>, source: serde_path_to_error::Error<serde_json::Error> },
    #[cfg(feature = "fetch")]
    #[error("import {url} is unreachable")]
    Request { url: Url, source: reqwest::Error },
    #[cfg(feature = "fetch")]
    #[error("import {url} is unreadable")]
    Read { url: Url, source: std::io::Error },
    #[error("import {uri} is no URL")]
    Url { uri: Box<str>, source: url::ParseError },
    #[error("import {url}: {reason}")]
    Refused { url: Url, reason: Refusal },
    #[error("{head} composes through itself")]
    Cycle { head: Name },
    #[error("no node at {path}")]
    Missing { path: Path },
    #[error("{attribute} on {node} {reason}")]
    Attribute { node: Path, attribute: Box<str>, reason: Fault },
    #[error("{node} holds {attribute} outside its schema")]
    Invalid { node: Path, attribute: Box<str>, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("{0}")]
    Constrained(Constraint),
    #[error("{node} names {attribute} with no EPSG code")]
    Epsg { node: Path, attribute: Box<str>, source: ParseIntError },
    #[error("{node} holds a curve of {count} vertices, past the u32 vertex count range")]
    Vertices { node: Path, count: usize, source: TryFromIntError },
    #[error("{path} holds conflicting opinions on {opinion}")]
    Conflict { path: Path, opinion: composition::Subject },
    #[cfg(feature = "bcf")]
    #[error("{node} is named by no GUID")]
    Guid { node: Path, source: rasm_bcf::BcfError },
    #[cfg(feature = "geometry")]
    #[error("{node} holds no triangle mesh")]
    Mesh { node: Path, source: parry3d_f64::shape::TriMeshBuilderError },
    #[cfg(feature = "pcd")]
    #[error("{node} holds an unreadable PCD point cloud")]
    Cloud { node: Path, source: pcd_rs::Error },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn read(bytes: &[u8], url: Option<&Url>) -> Result<Layer, IfcError> {
    let mut json = serde_json::Deserializer::from_slice(bytes);
    serde_path_to_error::deserialize(&mut json)
        .and_then(|layer| json.end().map(|()| layer).map_err(|source| serde_path_to_error::Error::new(serde_path_to_error::Track::new().path(), source)))
        .map_err(|source| IfcError::Decode { url: url.cloned(), source })
}

fn descend<'nodes>(heads: &'nodes IndexMap<Name, Arc<Composed>>, path: &Path) -> Option<&'nodes Arc<Composed>> {
    let (head, names) = path.0.split_first();
    names.iter().try_fold(heads.get(head)?, |node, name| node.children.get(name))
}

fn reached<T, K, I>(starts: impl IntoIterator<Item = T>, key: impl Fn(T) -> K, next: impl Fn(T) -> I) -> impl Iterator<Item = T>
where
    T: Copy,
    K: Hash + Eq,
    I: IntoIterator<Item = T>,
{
    gen move {
        let mut pending = starts.into_iter().collect::<Vec<_>>();
        let mut seen = HashSet::new();
        while let Some(node) = pending.pop() {
            if seen.insert(key(node)) {
                pending.extend(next(node));
                yield node;
            }
        }
    }
}

fn distinct<'model>(starts: impl IntoIterator<Item = &'model Arc<Composed>>) -> impl Iterator<Item = &'model Arc<Composed>> { reached(starts, Arc::as_ptr, |node| node.children.values()) }

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod alignment;
pub mod composition;
pub mod edits;
#[cfg(feature = "geometry")]
pub mod geometry;
pub mod imports;
pub mod schemas;
pub mod spatial;
#[cfg(feature = "bcf")]
pub mod viewpoints;
