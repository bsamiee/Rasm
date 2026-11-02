use core::{fmt, iter};

use data_encoding::BASE64;
use indexmap::IndexMap;
use jiff::fmt::temporal::Pieces;
use petgraph::{
    graphmap::DiGraphMap,
    visit::{Dfs, Visitable as _, Walker as _},
};
use serde::de::{self, DeserializeOwned, DeserializeSeed, Deserializer, MapAccess, SeqAccess, Unexpected, Visitor};

use crate::{Composed, DataType, IfcError, Path, QuantityKind, Schema, Value, ValueDescription, stored};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Attribute: DeserializeOwned {
    const ID: &'static str;
}

// --- [MODELS] --------------------------------------------------------------------------

struct Descriptions<'schema> {
    described: Vec<&'schema ValueDescription>,
    schemas: &'schema IndexMap<Box<str>, Schema>,
    inherited: &'schema DiGraphMap<&'schema str, ()>,
}

impl<'schema> Descriptions<'schema> {
    fn new<E>(described: impl IntoIterator<Item = &'schema ValueDescription>, schemas: &'schema IndexMap<Box<str>, Schema>, inherited: &'schema DiGraphMap<&'schema str, ()>) -> Result<Self, E>
    where E: de::Error {
        let own = described.into_iter().collect::<Vec<_>>();
        Dfs::from_parts(own.iter().flat_map(|description| &description.inherits).map(AsRef::as_ref).collect(), inherited.visit_map())
            .iter(inherited)
            .map(|id| schemas.get(id).map(|schema| &schema.value).ok_or_else(|| E::custom(format_args!("inherits {id}, which no schema defines"))))
            .try_collect::<Vec<_>>()
            .map(|ancestors| Self { described: [own, ancestors].concat(), schemas, inherited })
    }

    fn scalar<E>(&self, visited: Unexpected<'_>) -> Result<Value, E>
    where E: de::Error {
        let real = |quantity_kind: Option<QuantityKind>, value: f64| quantity_kind.map_or(Value::Real(value), |kind| Value::Quantity(stored(kind, value)));
        let decoded = |data_type: Option<&DataType>| match (data_type, visited) {
            (None | Some(DataType::Boolean), Unexpected::Bool(value)) => Ok(Value::Boolean(value)),
            (None | Some(DataType::Integer), Unexpected::Signed(value)) => Ok(Value::Integer(value)),
            (Some(DataType::Integer), Unexpected::Unsigned(_)) => Err(E::invalid_value(visited, &self)),
            (None, Unexpected::Unsigned(value)) => Ok(Value::Real(nalgebra::convert::<u64, f64>(value))),
            (None, Unexpected::Float(value)) => Ok(Value::Real(value)),
            (Some(DataType::Real { quantity_kind }), Unexpected::Signed(value)) => Ok(real(*quantity_kind, nalgebra::convert::<i64, f64>(value))),
            (Some(DataType::Real { quantity_kind }), Unexpected::Unsigned(value)) => Ok(real(*quantity_kind, nalgebra::convert::<u64, f64>(value))),
            (Some(DataType::Real { quantity_kind }), Unexpected::Float(value)) => Ok(real(*quantity_kind, value)),
            (None | Some(DataType::String), Unexpected::Str(text)) => Ok(Value::String(text.into())),
            (Some(DataType::DateTime), Unexpected::Str(text)) => Pieces::parse(text).map(|pieces| Value::DateTime(pieces.into_owned())).map_err(E::custom),
            (Some(DataType::Enum { restrictions }), Unexpected::Str(text)) =>
                restrictions.options.iter().any(|option| **option == *text).then(|| Value::Enum(text.into())).ok_or_else(|| E::invalid_value(visited, &self)),
            (Some(DataType::Reference), Unexpected::Str(text)) => text.parse().map(Value::Reference).map_err(E::custom),
            (Some(DataType::Blob), Unexpected::Str(text)) => BASE64.decode(text.as_bytes()).map(|bytes| Value::Blob(bytes.into_boxed_slice())).map_err(E::custom),
            _ => Err(E::invalid_type(visited, &self)),
        };
        match self.described.as_slice() {
            [] => decoded(None),
            [first, rest @ ..] => rest.iter().try_fold(decoded(Some(&first.data_type))?, |value, description| decoded(Some(&description.data_type)).map(|_| value)),
        }
    }

    fn restricted<R, E>(&self, visited: Unexpected<'_>, project: impl Fn(&'schema DataType) -> Option<R>) -> Result<Vec<R>, E>
    where E: de::Error {
        self.described.iter().map(|description| project(&description.data_type).ok_or_else(|| E::invalid_type(visited, &self))).try_collect::<Vec<_>>()
    }
}

impl<'de> DeserializeSeed<'de> for &Descriptions<'_> {
    type Value = Value;

    fn deserialize<D>(self, deserializer: D) -> Result<Value, D::Error>
    where D: Deserializer<'de> {
        deserializer.deserialize_any(self)
    }
}

impl<'de> Visitor<'de> for &Descriptions<'_> {
    type Value = Value;

    fn expecting(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self.described.as_slice() {
            [] => formatter.write_str("an IFCX value"),
            [first, rest @ ..] => iter::once(("", first)).chain(rest.iter().map(|description| (" and ", description))).try_for_each(|(separator, description)| {
                formatter.write_str(separator)?;
                match &description.data_type {
                    DataType::Real { quantity_kind: _ } => formatter.write_str("a real number"),
                    DataType::Boolean => formatter.write_str("a boolean"),
                    DataType::Integer => formatter.write_str("an integer"),
                    DataType::String => formatter.write_str("a string"),
                    DataType::DateTime => formatter.write_str("an ISO 8601 date"),
                    DataType::Enum { restrictions } => write!(formatter, "one of {:?}", restrictions.options),
                    DataType::Array { restrictions } => {
                        formatter.write_str("an array")?;
                        if let Some(min) = restrictions.min {
                            write!(formatter, ", at least {min} items")?;
                        }
                        if let Some(max) = restrictions.max {
                            write!(formatter, ", at most {max} items")?;
                        }
                        Ok(())
                    }
                    DataType::Object { restrictions: _ } => formatter.write_str("an object"),
                    DataType::Reference => formatter.write_str("a node path"),
                    DataType::Blob => formatter.write_str("base64 text"),
                }
            }),
        }
    }

    fn visit_bool<E>(self, value: bool) -> Result<Value, E>
    where E: de::Error {
        self.scalar(Unexpected::Bool(value))
    }

    fn visit_i64<E>(self, value: i64) -> Result<Value, E>
    where E: de::Error {
        self.scalar(Unexpected::Signed(value))
    }

    fn visit_u64<E>(self, value: u64) -> Result<Value, E>
    where E: de::Error {
        self.scalar(i64::try_from(value).ok().map_or(Unexpected::Unsigned(value), Unexpected::Signed))
    }

    fn visit_f64<E>(self, value: f64) -> Result<Value, E>
    where E: de::Error {
        self.scalar(Unexpected::Float(value))
    }

    fn visit_str<E>(self, text: &str) -> Result<Value, E>
    where E: de::Error {
        self.scalar(Unexpected::Str(text))
    }

    fn visit_seq<A>(self, mut items: A) -> Result<Value, A::Error>
    where A: SeqAccess<'de> {
        let arrays = self.restricted::<_, A::Error>(Unexpected::Seq, |data_type| match data_type {
            DataType::Array { restrictions } => Some(restrictions.as_ref()),
            _ => None,
        })?;
        let element = Descriptions::new::<A::Error>(arrays.iter().map(|restrictions| &restrictions.value), self.schemas, self.inherited)?;
        let values = iter::from_fn(|| items.next_element_seed(&element).transpose()).try_collect::<Vec<_>>()?;
        arrays
            .iter()
            .all(|restrictions| restrictions.min.is_none_or(|min| min <= values.len()) && restrictions.max.is_none_or(|max| values.len() <= max))
            .ok_or_else(|| de::Error::invalid_length(values.len(), &self))
            .map(|()| Value::Array(values))
    }

    fn visit_map<A>(self, mut entries: A) -> Result<Value, A::Error>
    where A: MapAccess<'de> {
        let objects = self.restricted::<_, A::Error>(Unexpected::Map, |data_type| match data_type {
            DataType::Object { restrictions } => Some(restrictions.as_ref()),
            _ => None,
        })?;
        let decoded = iter::from_fn(|| {
            entries.next_key::<Box<str>>().transpose().map(|key| {
                key.and_then(|key| {
                    Descriptions::new::<A::Error>(objects.iter().flatten().filter_map(|restrictions| restrictions.values.get(&key)), self.schemas, self.inherited)
                        .and_then(|seed| entries.next_value_seed(&seed))
                        .map(|value| (key, value))
                })
            })
        })
        .try_collect::<IndexMap<_, _>>()?;
        objects
            .iter()
            .flatten()
            .flat_map(|restrictions| &restrictions.values)
            .find(|(key, description)| !description.optional && !decoded.contains_key(*key))
            .map_or(Ok(Value::Object(decoded)), |(key, _)| Err(de::Error::custom(format_args!("missing field `{key}`"))))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RECORDS]

pub fn attribute<T>(node: &Composed) -> Result<Option<T>, IfcError>
where T: Attribute {
    record(node, T::ID)
}

pub(crate) fn record<T>(node: &Composed, id: &str) -> Result<Option<T>, IfcError>
where T: DeserializeOwned {
    node.attributes.get(id).map(|value| serde_path_to_error::deserialize(value).map_err(|source| IfcError::Record { node: node.node.clone(), attribute: id.into(), source })).transpose()
}

// --- [DECODE]

#[must_use]
pub(crate) fn inheritance(schemas: &IndexMap<Box<str>, Schema>) -> DiGraphMap<&str, ()> {
    schemas.iter().flat_map(|(id, schema)| schema.value.inherits.iter().map(move |inherited| (id.as_ref(), inherited.as_ref()))).collect()
}

pub(crate) fn decode(schemas: &IndexMap<Box<str>, Schema>, inherited: &DiGraphMap<&str, ()>, node: &Path, id: &str, value: &serde_json::Value) -> Result<Value, IfcError> {
    let schema = schemas.get(id).ok_or_else(|| IfcError::Unschematized { node: node.clone(), attribute: id.into() })?;
    let mut track = serde_path_to_error::Track::new();
    Descriptions::new(iter::once(&schema.value), schemas, inherited).and_then(|seed| seed.deserialize(serde_path_to_error::Deserializer::new(value, &mut track))).map_err(|inner| IfcError::Invalid {
        node: node.clone(),
        attribute: id.into(),
        source: serde_path_to_error::Error::new(track.path(), inner),
    })
}
