use core::{
    cell::{LazyCell, OnceCell},
    cmp::Ordering,
    fmt, iter,
    num::TryFromIntError,
    ops::RangeInclusive,
};
use std::rc::Rc;

use data_encoding::BASE64;
use indexmap::IndexMap;
use itertools::Itertools as _;
use jiff::fmt::temporal::Pieces;
use petgraph::{
    graphmap::DiGraphMap,
    visit::{Dfs, Visitable as _, Walker as _},
};
use serde::{
    Deserialize, Serialize,
    de::{self, DeserializeOwned, DeserializeSeed, Deserializer, MapAccess, SeqAccess, Unexpected, Visitor},
};
use serde_json::value::Serializer;
use uom::si::f64 as si;

use crate::{Composed, DataType, Fault, IfcError, Path, Schema, Value, ValueDescription, stored};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Attribute: DeserializeOwned {
    const ID: &'static str;
}

type Scope<'schema> = Rc<(IndexMap<&'schema str, &'schema Schema>, DiGraphMap<&'schema str, ()>)>;

// --- [MODELS] --------------------------------------------------------------------------

// --- [PROPERTIES]

macro_rules! properties {
    ($($namespace:literal { $($name:ident($value:ty) $(: $derived:path)?),+ $(,)? })+) => {$($(
        #[derive(Debug, Clone, PartialEq, Serialize, Deserialize $(, $derived)?)]
        pub struct $name(pub $value);

        impl Attribute for $name {
            const ID: &'static str = concat!($namespace, "::", stringify!($name));
        }
    )+)+};
}

properties! {
    "bsi::ifc::prop" {
        Name(Box<str>): Eq,
        Description(Box<str>): Eq,
        UsageType(Box<str>): Eq,
        TypeName(Box<str>): Eq,
        IsExternal(bool): Eq,
        NumberOfStoreys(i64): Eq,
        Station(si::Length),
        RefElevation(si::Length),
        ElevationOfRefHeight(si::Length),
        ElevationOfTerrain(si::Length),
        Height(si::Length),
        Width(si::Length),
        Length(si::Length),
        Depth(si::Length),
        NetArea(si::Area),
        NetSideArea(si::Area),
        CrossSectionArea(si::Area),
        Volume(si::Volume),
        NetVolume(si::Volume),
    }
    "bsi::ifc-mat::prop" {
        StrengthClass(Box<str>): Eq,
        MoistureContent(f64),
        MassDensity(f64),
    }
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub struct GlobalWarmingPotential(pub IndexMap<Box<str>, f64>);

impl Attribute for GlobalWarmingPotential {
    const ID: &'static str = "bsi::ifc-mat::prop::GWP";
}

// --- [TARGETS]

#[derive(Deserialize)]
pub(crate) struct Target {
    #[serde(rename = "ref")]
    pub(crate) reference: Path,
}

// --- [DESCRIPTIONS]

struct Descriptions<'schema> {
    described: Vec<&'schema ValueDescription>,
    undefined: Option<&'schema str>,
    scope: Scope<'schema>,
    element: OnceCell<Option<(Box<Self>, RangeInclusive<usize>)>> = OnceCell::new(),
    fields: OnceCell<Option<IndexMap<&'schema str, (Self, bool)>>> = OnceCell::new(),
}

impl<'schema> Descriptions<'schema> {
    fn new<I>(own: I, scope: &Scope<'schema>) -> Self
    where I: IntoIterator<Item = &'schema ValueDescription, IntoIter: Clone> {
        let own = own.into_iter();
        let (schemas, inherited) = &**scope;
        let ancestors = Dfs::from_parts(own.clone().flat_map(|description| &description.inherits).map(|id| &**id).collect(), inherited.visit_map()).iter(inherited).collect::<Vec<_>>();
        Self {
            described: own.chain(ancestors.iter().filter_map(|id| schemas.get(*id)).map(|schema| &schema.value)).collect(),
            undefined: ancestors.into_iter().find(|id| !schemas.contains_key(*id)),
            scope: Rc::clone(scope),
            ..
        }
    }

    fn scalar<E>(&self, visited: Unexpected<'_>) -> Result<Value, E>
    where E: de::Error {
        let number = match visited {
            Unexpected::Signed(value) => Some(nalgebra::convert::<i64, f64>(value)),
            Unexpected::Unsigned(value) => Some(nalgebra::convert::<u64, f64>(value)),
            Unexpected::Float(value) => Some(value),
            Unexpected::Bool(_)
            | Unexpected::Str(_)
            | Unexpected::Char(_)
            | Unexpected::Bytes(_)
            | Unexpected::Unit
            | Unexpected::Option
            | Unexpected::NewtypeStruct
            | Unexpected::Seq
            | Unexpected::Map
            | Unexpected::Enum
            | Unexpected::UnitVariant
            | Unexpected::NewtypeVariant
            | Unexpected::TupleVariant
            | Unexpected::StructVariant
            | Unexpected::Other(_) => None,
        };
        let decoded = |data_type: Option<&DataType>| match (data_type, visited, number) {
            (None | Some(DataType::Boolean), Unexpected::Bool(value), _) => Ok(Value::Boolean(value)),
            (None | Some(DataType::Integer), Unexpected::Signed(value), _) => Ok(Value::Integer(value)),
            (Some(DataType::Integer), Unexpected::Unsigned(_), _) => Err(E::invalid_value(visited, &self)),
            (None, _, Some(value)) => Ok(Value::Real(value)),
            (Some(DataType::Real { quantity_kind }), _, Some(value)) => Ok(quantity_kind.map_or(Value::Real(value), |kind| Value::Quantity(stored(kind, value)))),
            (None | Some(DataType::String), Unexpected::Str(text), _) => Ok(Value::String(text.into())),
            (Some(DataType::DateTime), Unexpected::Str(text), _) => Pieces::parse(text).map(|pieces| Value::DateTime(Box::new(pieces.into_owned()))).map_err(E::custom),
            (Some(DataType::Enum { restrictions }), Unexpected::Str(text), _) =>
                restrictions.options.iter().any(|option| **option == *text).then(|| Value::String(text.into())).ok_or_else(|| E::invalid_value(visited, &self)),
            (Some(DataType::Reference), Unexpected::Str(text), _) => text.parse().map(Value::Reference).map_err(E::custom),
            (Some(DataType::Blob), Unexpected::Str(text), _) => BASE64.decode(text.as_bytes()).map(|bytes| Value::Blob(bytes.into_boxed_slice())).map_err(E::custom),
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
                _,
            ) => Err(E::invalid_type(visited, &self)),
        };
        let specificity = |value: &Value| match value {
            Value::Real(_) | Value::String(_) => 0_u8,
            Value::Integer(_) => 1,
            Value::Boolean(_) | Value::Quantity(_) | Value::DateTime(_) | Value::Array(_) | Value::Object(_) | Value::Reference(_) | Value::Blob(_) => 2,
        };
        self.described
            .iter()
            .try_fold(None, |kept: Option<Value>, description| {
                let next = decoded(Some(&description.data_type))?;
                match kept.map(|kept| (specificity(&kept).cmp(&specificity(&next)), kept)) {
                    None | Some((Ordering::Less, _)) => Ok(Some(next)),
                    Some((Ordering::Greater, kept)) => Ok(Some(kept)),
                    Some((Ordering::Equal, kept)) => (kept == next).then_some(Some(kept)).ok_or_else(|| E::invalid_value(visited, &self)),
                }
            })?
            .map_or_else(|| decoded(None), Ok)
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
        if self.described.is_empty() { formatter.write_str("an IFCX value") } else { write!(formatter, "{}", self.described.iter().map(|description| &description.data_type).format(" and ")) }
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
        let Some((element, lengths)) = self.element.get_or_init(|| {
            let arrays = self
                .described
                .iter()
                .map(|description| {
                    let DataType::Array { restrictions } = &description.data_type else { return None };
                    Some(&**restrictions)
                })
                .collect::<Option<Vec<_>>>()?;
            let lengths = arrays.iter().filter_map(|restrictions| restrictions.min).max().unwrap_or_default()..=arrays.iter().filter_map(|restrictions| restrictions.max).min().unwrap_or(usize::MAX);
            Some((Box::new(Descriptions::new(arrays.iter().map(|restrictions| &restrictions.value), &self.scope)), lengths))
        }) else {
            return Err(de::Error::invalid_type(Unexpected::Seq, &self));
        };
        let capacity = seq.size_hint().unwrap_or_default();
        let values = iter::from_fn(|| seq.next_element_seed(&**element).transpose()).try_fold(Vec::with_capacity(capacity), |mut values, value| {
            value.map(|value| {
                values.push(value);
                values
            })
        })?;
        lengths.contains(&values.len()).ok_or_else(|| de::Error::invalid_length(values.len(), &self)).map(|()| Value::Array(values))
    }

    fn visit_map<A>(self, mut map: A) -> Result<Value, A::Error>
    where A: MapAccess<'de> {
        let Some(fields) = self.fields.get_or_init(|| {
            let objects = self
                .described
                .iter()
                .map(|description| {
                    let DataType::Object { restrictions } = &description.data_type else { return None };
                    Some(restrictions.as_ref())
                })
                .collect::<Option<Vec<_>>>()?
                .into_iter()
                .flatten()
                .collect::<Vec<_>>();
            Some(
                objects
                    .iter()
                    .copied()
                    .flat_map(|restrictions| restrictions.values.keys())
                    .map(|key| &**key)
                    .unique()
                    .map(|key| {
                        let mut described = objects.iter().copied().filter_map(move |restrictions| restrictions.values.get(key));
                        (key, (Descriptions::new(described.clone(), &self.scope), described.any(|description| !description.optional)))
                    })
                    .collect(),
            )
        }) else {
            return Err(de::Error::invalid_type(Unexpected::Map, &self));
        };
        let untyped = LazyCell::new(|| Descriptions::new(iter::empty(), &self.scope));
        let capacity = map.size_hint().unwrap_or_default();
        let decoded = iter::from_fn(|| {
            map.next_key::<Box<str>>().transpose().map(|key| key.and_then(|key| map.next_value_seed(fields.get(&*key).map_or_else(|| &*untyped, |(field, _)| field)).map(|value| (key, value))))
        })
        .try_fold(IndexMap::with_capacity(capacity), |mut held, entry| {
            entry.map(|(key, value)| {
                held.insert(key, value);
                held
            })
        })?;
        let missing = fields.iter().filter_map(|(&key, (_, required))| (*required && !decoded.contains_key(key)).then_some(key)).collect::<Vec<_>>();
        missing.is_empty().ok_or_else(|| de::Error::custom(format_args!("missing `{}`", missing.join("`, `")))).map(|()| Value::Object(Box::new(decoded)))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RECORDS]

pub fn attribute<T>(node: &Composed) -> Result<Option<T>, IfcError>
where T: Attribute {
    record(node, T::ID)
}

pub fn encoded<T>(node: &Path, value: &T) -> Result<(Box<str>, Option<Value>), IfcError>
where T: Attribute + Serialize {
    let invalid = |source| IfcError::Invalid { node: node.clone(), attribute: T::ID.into(), source };
    let json = serde_path_to_error::serialize(value, Serializer).map_err(invalid)?;
    let mut track = serde_path_to_error::Track::new();
    Descriptions::new(iter::empty(), &Rc::new((IndexMap::new(), DiGraphMap::new())))
        .deserialize(serde_path_to_error::Deserializer::new(&json, &mut track))
        .map(|decoded| (T::ID.into(), Some(decoded)))
        .map_err(|inner| invalid(serde_path_to_error::Error::new(track.path(), inner)))
}

pub(crate) fn record<T>(node: &Composed, id: &str) -> Result<Option<T>, IfcError>
where T: DeserializeOwned {
    node.attributes.get(id).map(|value| serde_path_to_error::deserialize(&**value).map_err(|source| IfcError::Invalid { node: node.path.clone(), attribute: id.into(), source })).transpose()
}

// --- [DECODING]

pub(crate) fn decoder<'schema>(described: impl IntoIterator<Item = (&'schema str, &'schema Schema)>) -> impl Fn(&Path, &str, &serde_json::Value) -> Result<Value, IfcError> {
    let schemas = described.into_iter().collect::<IndexMap<_, _>>();
    let inherited = schemas.iter().flat_map(|(&id, &schema)| schema.value.inherits.iter().map(move |ancestor| (id, &**ancestor))).collect::<DiGraphMap<&str, ()>>();
    let scope = Rc::new((schemas, inherited));
    let seeds = scope.0.iter().map(|(&id, &schema)| (id, Descriptions::new(iter::once(&schema.value), &scope))).collect::<IndexMap<_, _>>();
    move |node: &Path, id: &str, value: &serde_json::Value| {
        let seed = seeds.get(id).ok_or_else(|| IfcError::Attribute { node: node.clone(), attribute: id.into(), reason: Fault::Undeclared })?;
        let mut track = serde_path_to_error::Track::new();
        seed.deserialize(serde_path_to_error::Deserializer::new(value, &mut track)).map_err(|inner| IfcError::Invalid {
            node: node.clone(),
            attribute: id.into(),
            source: serde_path_to_error::Error::new(track.path(), inner),
        })
    }
}
