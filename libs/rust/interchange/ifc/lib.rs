#![feature(default_field_values, iterator_try_collect, try_blocks_heterogeneous)]

use core::{fmt, num::ParseFloatError, str::FromStr};
use std::{io, sync::Arc};

use indexmap::IndexMap;
use jiff::fmt::temporal::Pieces;
use nonempty_collections::NEVec;
use parry3d_f64::shape::TriMeshBuilderError;
use serde::{
    Deserialize, Deserializer, Serialize, Serializer,
    de::{
        self, IntoDeserializer, Visitor,
        value::{BorrowedStrDeserializer, MapDeserializer, SeqDeserializer},
    },
};
use serde_with::{DefaultOnNull, DeserializeFromStr, DisplayFromStr, SerializeDisplay, base64::Base64, serde_as};
use uom::si;
use url::Url;
use zip::result::ZipError;

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(
    validate(not_empty, predicate = |name: &str| !name.contains('/')),
    derive(Debug, Clone, PartialEq, Eq, Hash, PartialOrd, Ord, Display, AsRef, Deref, Borrow, FromStr, Serialize, Deserialize)
)]
pub struct Name(String);

#[derive(Debug, Clone, PartialEq, Eq, Hash, PartialOrd, Ord, SerializeDisplay, DeserializeFromStr)]
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

impl fmt::Display for Path {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result {
        let (head, names) = self.0.split_first();
        write!(formatter, "{head}")?;
        names.iter().try_for_each(|name| write!(formatter, "/{name}"))
    }
}

impl FromStr for Path {
    type Err = NameError;

    fn from_str(text: &str) -> Result<Self, Self::Err> {
        let (head, rest) = text.split_once('/').map_or((text, None), |(head, rest)| (head, Some(rest)));
        Ok(Self(NEVec::from((Name::try_new(head)?, rest.into_iter().flat_map(|names| names.split('/')).map(Name::try_new).try_collect::<Vec<_>>()?))))
    }
}

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

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(tag = "dataType")]
pub enum DataType {
    Real {
        #[serde(rename = "quantityKind", default, skip_serializing_if = "Option::is_none")]
        quantity_kind: Option<QuantityKind>,
    },
    Boolean,
    Integer,
    String,
    DateTime,
    Enum {
        #[serde(rename = "enumRestrictions")]
        restrictions: EnumRestrictions,
    },
    Array {
        #[serde(rename = "arrayRestrictions")]
        restrictions: Box<ArrayRestrictions>,
    },
    Object {
        #[serde(rename = "objectRestrictions", default, skip_serializing_if = "Option::is_none")]
        restrictions: Option<ObjectRestrictions>,
    },
    Reference,
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

        impl Serialize for Quantity {
            fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
            where S: Serializer {
                serializer.serialize_f64(match self {
                    $(Self::$kind(value) => value.get::<si::$module::$unit>(),)+
                })
            }
        }

        fn stored(kind: QuantityKind, value: f64) -> Quantity {
            match kind {
                $(QuantityKind::$kind => Quantity::$kind(si::f64::$quantity::new::<si::$module::$unit>(value)),)+
            }
        }

        const fn base(quantity: Quantity) -> f64 {
            match quantity {
                $(Quantity::$kind(value) => value.value,)+
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

#[serde_as]
#[derive(Debug, Clone, PartialEq, Serialize)]
#[serde(untagged)]
pub enum Value {
    Boolean(bool),
    Integer(i64),
    Real(f64),
    Quantity(Quantity),
    String(Box<str>),
    DateTime(#[serde_as(as = "DisplayFromStr")] Pieces<'static>),
    Enum(Box<str>),
    Array(Vec<Self>),
    Object(IndexMap<Box<str>, Self>),
    Reference(Path),
    Blob(#[serde_as(as = "Base64")] Box<[u8]>),
}

impl<'de> Deserializer<'de> for &'de Value {
    type Error = de::value::Error;

    fn deserialize_any<V>(self, visitor: V) -> Result<V::Value, Self::Error>
    where V: Visitor<'de> {
        match self {
            Value::Boolean(value) => visitor.visit_bool(*value),
            Value::Integer(value) => visitor.visit_i64(*value),
            Value::Real(value) => visitor.visit_f64(*value),
            Value::Quantity(quantity) => visitor.visit_f64(base(*quantity)),
            Value::String(text) | Value::Enum(text) => visitor.visit_borrowed_str(text),
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
            Value::String(text) | Value::Enum(text) => visitor.visit_enum(BorrowedStrDeserializer::new(text)),
            Value::Boolean(_) | Value::Integer(_) | Value::Real(_) | Value::Quantity(_) | Value::DateTime(_) | Value::Array(_) | Value::Object(_) | Value::Reference(_) | Value::Blob(_) =>
                self.deserialize_any(visitor),
        }
    }

    serde::forward_to_deserialize_any! {
        bool i8 i16 i32 i64 i128 u8 u16 u32 u64 u128 f32 f64 char str string
        bytes byte_buf unit unit_struct seq tuple tuple_struct map struct identifier ignored_any
    }
}

impl<'de> IntoDeserializer<'de, de::value::Error> for &'de Value {
    type Deserializer = Self;

    fn into_deserializer(self) -> Self { self }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Composed {
    pub node: Path,
    pub attributes: IndexMap<Box<str>, Value>,
    pub children: IndexMap<Name, Arc<Self>>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Model {
    schemas: IndexMap<Box<str>, Schema>,
    nodes: IndexMap<Name, Arc<Composed>>,
    roots: Vec<Arc<Composed>>,
}

impl Model {
    #[must_use]
    pub const fn schemas(&self) -> &IndexMap<Box<str>, Schema> { &self.schemas }

    pub fn roots(&self) -> impl Iterator<Item = &Composed> { self.roots.iter().map(Arc::as_ref) }

    pub fn nodes(&self) -> impl Iterator<Item = &Composed> { self.nodes.values().map(Arc::as_ref) }

    #[must_use]
    pub fn get(&self, path: &Path) -> Option<&Composed> {
        let (head, names) = path.0.split_first();
        names.iter().try_fold(self.nodes.get(head).map(Arc::as_ref)?, |node, name| node.children.get(name).map(Arc::as_ref))
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

#[derive(Debug, thiserror::Error)]
pub enum IfcError {
    #[error(transparent)]
    Decode(#[from] serde_path_to_error::Error<serde_json::Error>),
    #[error(transparent)]
    Request(#[from] reqwest::Error),
    #[error(transparent)]
    Read(#[from] io::Error),
    #[error("import {uri} is no URL")]
    Url { uri: Box<str>, source: url::ParseError },
    #[error("import {url} holds no IFCX layer")]
    Import { url: Url, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("import {url}: {reason}")]
    Refused { url: Url, reason: Refusal },
    #[error("{head} composes through itself")]
    Cycle { head: Name },
    #[error("{path} names no node")]
    Unknown { path: Path },
    #[error("{node} holds {attribute} with no schema")]
    Unschematized { node: Path, attribute: Box<str> },
    #[error("{node} holds {attribute} outside its schema")]
    Invalid { node: Path, attribute: Box<str>, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("{node} holds {attribute} unlike its record")]
    Record { node: Path, attribute: Box<str>, source: serde_path_to_error::Error<de::value::Error> },
    #[error("{node} holds a malformed {attribute}")]
    Malformed { node: Path, attribute: Box<str> },
    #[error("{node} holds no triangle mesh")]
    Mesh { node: Path, source: TriMeshBuilderError },
    #[error("{path} holds no horizontal layout")]
    Layout { path: Path },
    #[error("BCF {found} is no BCF 3.0 archive")]
    Version { found: Box<str> },
    #[error("BCF entry {entry} is unreadable")]
    Archive { entry: Box<str>, source: ZipError },
    #[error("BCF entry {entry} is no XML")]
    Parse { entry: Box<str>, source: xot::ParseError },
    #[error("BCF entry {entry} is unwritable")]
    Xml { entry: Box<str>, source: xot::Error },
    #[error("BCF entry {entry} lacks a readable {element}")]
    Element { entry: Box<str>, element: &'static str },
    #[error("BCF entry {entry} holds a malformed date in {element}")]
    Date { entry: Box<str>, element: &'static str, source: jiff::Error },
    #[error("BCF entry {entry} holds a malformed guid in {element}")]
    Guid { entry: Box<str>, element: &'static str, source: newtype_uuid::ParseError },
    #[error("BCF entry {entry} holds a malformed number in {element}")]
    Number { entry: Box<str>, element: &'static str, source: ParseFloatError },
    #[error("BCF entry {entry} holds a malformed color in {element}")]
    Color { entry: Box<str>, element: &'static str, source: data_encoding::DecodeError },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn read(bytes: &[u8]) -> Result<Layer, IfcError> {
    let mut json = serde_json::Deserializer::from_slice(bytes);
    let layer = serde_path_to_error::deserialize(&mut json)?;
    json.end().map(|()| layer).map_err(|source| IfcError::Decode(serde_path_to_error::Error::new(serde_path_to_error::Track::new().path(), source)))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod alignment;
pub mod composition;
pub mod edits;
pub mod geometry;
pub mod imports;
pub mod issues;
pub mod schemas;
pub mod spatial;
