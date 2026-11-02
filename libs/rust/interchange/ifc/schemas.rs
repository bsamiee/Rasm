use core::{
    cell::{LazyCell, OnceCell},
    cmp::Ordering,
    fmt, iter,
    num::TryFromIntError,
};

use data_encoding::BASE64;
use indexmap::{IndexMap, IndexSet};
use jiff::fmt::temporal::Pieces;
use petgraph::{
    graphmap::DiGraphMap,
    visit::{Dfs, Visitable as _, Walker as _},
};
use serde::de::{self, DeserializeOwned, DeserializeSeed, Deserializer, MapAccess, SeqAccess, Unexpected, Visitor};

use crate::{ArrayRestrictions, Composed, DataType, Fault, IfcError, ObjectRestrictions, Path, QuantityKind, Schema, Value, ValueDescription, stored};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Attribute: DeserializeOwned {
    const ID: &'static str;
}

// --- [MODELS] --------------------------------------------------------------------------

struct Descriptions<'schema> {
    described: Vec<&'schema ValueDescription>,
    undefined: Option<&'schema str>,
    schemas: &'schema IndexMap<Box<str>, Schema>,
    inherited: &'schema DiGraphMap<&'schema str, ()>,
    element: OnceCell<Box<Self>> = OnceCell::new(),
    fields: OnceCell<IndexMap<&'schema str, Self>> = OnceCell::new(),
}

impl<'schema> Descriptions<'schema> {
    fn new<I>(own: I, schemas: &'schema IndexMap<Box<str>, Schema>, inherited: &'schema DiGraphMap<&'schema str, ()>) -> Self
    where I: IntoIterator<Item = &'schema ValueDescription, IntoIter: Clone> {
        let own = own.into_iter();
        let ancestors = Dfs::from_parts(own.clone().flat_map(|description| &description.inherits).map(AsRef::as_ref).collect(), inherited.visit_map()).iter(inherited).collect::<Vec<_>>();
        Self {
            described: own.chain(ancestors.iter().filter_map(|id| schemas.get(*id)).map(|schema| &schema.value)).collect(),
            undefined: ancestors.into_iter().find(|id| !schemas.contains_key(*id)),
            schemas,
            inherited,
            ..
        }
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
            (
                None
                | Some(
                    DataType::Real { quantity_kind: _ }
                    | DataType::Boolean
                    | DataType::Integer
                    | DataType::String
                    | DataType::DateTime
                    | DataType::Enum { restrictions: _ }
                    | DataType::Array { restrictions: _ }
                    | DataType::Object { restrictions: _ }
                    | DataType::Reference
                    | DataType::Blob,
                ),
                _,
            ) => Err(E::invalid_type(visited, &self)),
        };
        let refinement = |value: &Value| match value {
            Value::Real(_) | Value::String(_) => 0_u8,
            Value::Integer(_) => 1,
            Value::Boolean(_) | Value::Quantity(_) | Value::DateTime(_) | Value::Enum(_) | Value::Array(_) | Value::Object(_) | Value::Reference(_) | Value::Blob(_) => 2,
        };
        self.described
            .iter()
            .try_fold(None, |kept: Option<Value>, description| {
                let next = decoded(Some(&description.data_type))?;
                match kept.map(|kept| (refinement(&kept).cmp(&refinement(&next)), kept)) {
                    None | Some((Ordering::Less, _)) => Ok(Some(next)),
                    Some((Ordering::Greater, kept)) => Ok(Some(kept)),
                    Some((Ordering::Equal, kept)) => (kept == next).then_some(Some(kept)).ok_or_else(|| E::invalid_value(visited, &self)),
                }
            })?
            .map_or_else(|| decoded(None), Ok)
    }

    fn restricted<R, E>(&self, visited: Unexpected<'_>, project: impl Fn(&'schema DataType) -> Option<R> + Clone) -> Result<impl Iterator<Item = R> + Clone, E>
    where E: de::Error {
        self.described
            .iter()
            .copied()
            .all(|description| project(&description.data_type).is_some())
            .ok_or_else(|| E::invalid_type(visited, &self))
            .map(|()| self.described.iter().copied().filter_map(move |description| project(&description.data_type)))
    }
}

impl<'de> DeserializeSeed<'de> for &Descriptions<'_> {
    type Value = Value;

    fn deserialize<D>(self, deserializer: D) -> Result<Value, D::Error>
    where D: Deserializer<'de> {
        match self.undefined {
            Some(id) => Err(de::Error::custom(format_args!("inherits {id}, which no schema defines"))),
            None => deserializer.deserialize_any(self),
        }
    }
}

impl<'de> Visitor<'de> for &Descriptions<'_> {
    type Value = Value;

    fn expecting(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self.described.split_first() {
            None => formatter.write_str("an IFCX value"),
            Some((first, rest)) => iter::once(("", first)).chain(rest.iter().map(|description| (" and ", description))).try_for_each(|(separator, description)| {
                formatter.write_str(separator)?;
                match &description.data_type {
                    DataType::Real { quantity_kind: _ } => formatter.write_str("a real number"),
                    DataType::Boolean => formatter.write_str("a boolean"),
                    DataType::Integer => formatter.write_str("an integer"),
                    DataType::String => formatter.write_str("a string"),
                    DataType::DateTime => formatter.write_str("an ISO 8601 date"),
                    DataType::Enum { restrictions } => write!(formatter, "one of {}", restrictions.options.join(", ")),
                    DataType::Array { restrictions } => match (restrictions.min, restrictions.max) {
                        (None, None) => formatter.write_str("an array"),
                        (Some(min), None) => write!(formatter, "an array of at least {min} items"),
                        (None, Some(max)) => write!(formatter, "an array of at most {max} items"),
                        (Some(min), Some(max)) => write!(formatter, "an array of {min} to {max} items"),
                    },
                    DataType::Object { restrictions: _ } => formatter.write_str("an object"),
                    DataType::Reference => formatter.write_str("a node path"),
                    DataType::Blob => formatter.write_str("base64 text"),
                }
            }),
        }
    }

    fn visit_bool<E>(self, v: bool) -> Result<Value, E>
    where E: de::Error {
        self.scalar(Unexpected::Bool(v))
    }

    fn visit_i64<E>(self, v: i64) -> Result<Value, E>
    where E: de::Error {
        self.scalar(Unexpected::Signed(v))
    }

    fn visit_u64<E>(self, v: u64) -> Result<Value, E>
    where E: de::Error {
        self.scalar(match i64::try_from(v) {
            Ok(signed) => Unexpected::Signed(signed),
            Err(TryFromIntError { .. }) => Unexpected::Unsigned(v),
        })
    }

    fn visit_f64<E>(self, v: f64) -> Result<Value, E>
    where E: de::Error {
        self.scalar(Unexpected::Float(v))
    }

    fn visit_str<E>(self, v: &str) -> Result<Value, E>
    where E: de::Error {
        self.scalar(Unexpected::Str(v))
    }

    fn visit_seq<A>(self, mut seq: A) -> Result<Value, A::Error>
    where A: SeqAccess<'de> {
        let arrays = self.restricted::<&ArrayRestrictions, A::Error>(Unexpected::Seq, |data_type| match data_type {
            DataType::Array { restrictions } => Some(restrictions.as_ref()),
            DataType::Real { quantity_kind: _ }
            | DataType::Boolean
            | DataType::Integer
            | DataType::String
            | DataType::DateTime
            | DataType::Enum { restrictions: _ }
            | DataType::Object { restrictions: _ }
            | DataType::Reference
            | DataType::Blob => None,
        })?;
        let element = self.element.get_or_init(|| Box::new(Descriptions::new(arrays.clone().map(|restrictions| &restrictions.value), self.schemas, self.inherited)));
        let values = iter::from_fn(|| seq.next_element_seed(&**element).transpose()).try_collect::<Vec<_>>()?;
        arrays
            .into_iter()
            .all(|restrictions| restrictions.min.is_none_or(|min| min <= values.len()) && restrictions.max.is_none_or(|max| values.len() <= max))
            .ok_or_else(|| de::Error::invalid_length(values.len(), &self))
            .map(|()| Value::Array(values))
    }

    fn visit_map<A>(self, mut map: A) -> Result<Value, A::Error>
    where A: MapAccess<'de> {
        let objects = self
            .restricted::<Option<&ObjectRestrictions>, A::Error>(Unexpected::Map, |data_type| match data_type {
                DataType::Object { restrictions } => Some(restrictions.as_ref()),
                DataType::Real { quantity_kind: _ }
                | DataType::Boolean
                | DataType::Integer
                | DataType::String
                | DataType::DateTime
                | DataType::Enum { restrictions: _ }
                | DataType::Array { restrictions: _ }
                | DataType::Reference
                | DataType::Blob => None,
            })?
            .flatten();
        let fields = self.fields.get_or_init(|| {
            objects
                .clone()
                .flat_map(|restrictions| restrictions.values.keys())
                .map(AsRef::as_ref)
                .collect::<IndexSet<&str>>()
                .into_iter()
                .map(|key| (key, Descriptions::new(objects.clone().filter_map(move |restrictions| restrictions.values.get(key)), self.schemas, self.inherited)))
                .collect()
        });
        let untyped = LazyCell::new(|| Descriptions::new(iter::empty(), self.schemas, self.inherited));
        let decoded = iter::from_fn(|| map.next_key::<Box<str>>().transpose().map(|key| key.and_then(|key| map.next_value_seed(fields.get(&*key).unwrap_or(&*untyped)).map(|value| (key, value)))))
            .try_collect::<IndexMap<_, _>>()?;
        let missing = fields
            .keys()
            .copied()
            .filter(|key| !decoded.contains_key(*key) && objects.clone().filter_map(|restrictions| restrictions.values.get(*key)).any(|description| !description.optional))
            .collect::<Vec<_>>();
        missing.is_empty().ok_or_else(|| de::Error::custom(format_args!("missing `{}`", missing.join("`, `")))).map(|()| Value::Object(decoded))
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
    let schema = schemas.get(id).ok_or_else(|| IfcError::Attribute { node: node.clone(), attribute: id.into(), reason: Fault::Unschematized })?;
    let mut track = serde_path_to_error::Track::new();
    Descriptions::new(iter::once(&schema.value), schemas, inherited).deserialize(serde_path_to_error::Deserializer::new(value, &mut track)).map_err(|inner| IfcError::Invalid {
        node: node.clone(),
        attribute: id.into(),
        source: serde_path_to_error::Error::new(track.path(), inner),
    })
}
