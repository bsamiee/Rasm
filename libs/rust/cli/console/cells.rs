use core::mem;
use std::borrow::Cow;

use indexmap::{IndexMap, map::RawEntryApiV1 as _};
use itertools::Itertools as _;
use serde::Serialize;
use serde_content::{Data, Enum, Number, Serializer, Struct, Value};

use crate::EncodingError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, PartialEq, Eq, Hash)]
pub(crate) enum Segment {
    Name(Cow<'static, str>),
    Key { kind: mem::Discriminant<Value<'static>>, json: String, label: String },
    Index(usize),
}

pub(crate) struct Field {
    pub(crate) text: String,
    pub(crate) numeric: bool,
}

#[derive(Default)]
pub(crate) struct Row {
    pub(crate) named: bool,
    pub(crate) fields: Vec<Option<Field>>,
}

#[derive(Default)]
pub(crate) struct Columns {
    pub(crate) names: IndexMap<Box<[Segment]>, Box<str>>,
    path: Vec<Segment>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn rendered(value: Value<'static>) -> Result<Option<Field>, serde_json::Error> {
    match value {
        Value::Option(Some(inner)) => rendered(*inner),
        Value::Struct(Struct { data: Data::NewType { value }, .. }) => rendered(value),
        Value::Unit | Value::Option(None) | Value::Struct(Struct { data: Data::Unit, .. }) => Ok(None),
        Value::Number(Number::F32(number)) if !number.is_finite() => Ok(None),
        Value::Number(Number::F64(number)) if !number.is_finite() => Ok(None),
        Value::Number(number) => serde_json::to_string(&number).map(|text| Some(Field { text, numeric: true })),
        Value::Bool(flag) => Ok(Some(Field { text: flag.to_string(), numeric: false })),
        Value::Char(character) => Ok(Some(Field { text: String::from(character), numeric: false })),
        Value::String(text) => Ok(Some(Field { text: text.into_owned(), numeric: false })),
        Value::Bytes(bytes) => Ok(Some(Field { text: String::from_utf8_lossy_owned(bytes.into_owned()), numeric: false })),
        Value::Enum(Enum { variant, data: Data::Unit, .. }) => Ok(Some(Field { text: variant.into_owned(), numeric: false })),
        compound @ (Value::Seq(_) | Value::Map(_) | Value::Struct(_) | Value::Enum(_) | Value::Tuple(_)) =>
            serde_json::to_string(&compound).map(|text| Some(Field { text, numeric: false })),
    }
}

fn walked(value: Value<'static>, columns: &mut Columns, row: &mut Row) -> Result<(), serde_json::Error> {
    fn entered(segment: Segment, value: Value<'static>, columns: &mut Columns, row: &mut Row) -> Result<(), serde_json::Error> {
        let depth = columns.path.len();
        columns.path.push(segment);
        let walked = walked(value, columns, row);
        columns.path.truncate(depth);
        walked
    }
    fn keyed(key: Value<'static>) -> Result<Segment, serde_json::Error> {
        let Value::String(name) = key else {
            let (kind, json) = (mem::discriminant(&key), serde_json::to_string(&key)?);
            return rendered(key).map(|field| Segment::Key { kind, label: field.map_or_else(|| json.clone(), |Field { text, numeric: _ }| text), json });
        };
        Ok(Segment::Name(name))
    }
    fn cell(value: Value<'static>, Columns { names, path }: &mut Columns, Row { named, fields }: &mut Row) -> Result<(), serde_json::Error> {
        let field = rendered(value)?;
        *named |= path.iter().any(|segment| !matches!(segment, Segment::Index(_)));
        let entry = names.raw_entry_mut_v1().from_key(path.as_slice());
        let index = entry.index();
        entry.or_insert_with(|| {
            let joined = path.iter().format_with(".", |segment, emit| match segment {
                Segment::Name(name) => emit(name),
                Segment::Key { kind: _, json: _, label } => emit(label),
                Segment::Index(position) => emit(position),
            });
            (path.clone().into_boxed_slice(), joined.to_string().into_boxed_str())
        });
        if let Some(slot) = fields.get_mut(index) {
            *slot = field;
        } else {
            fields.resize_with(index, || None);
            fields.push(field);
        }
        Ok(())
    }
    match value {
        Value::Option(Some(inner)) => walked(*inner, columns, row),
        Value::Struct(Struct { data: Data::NewType { value }, .. }) => walked(value, columns, row),
        Value::Seq(values) | Value::Tuple(values) | Value::Struct(Struct { data: Data::Tuple { values }, .. }) if columns.path.is_empty() =>
            values.into_iter().enumerate().try_for_each(|(index, value)| entered(Segment::Index(index), value, columns, row)),
        Value::Struct(Struct { data: Data::Struct { fields }, .. }) => fields.into_iter().try_for_each(|(name, value)| entered(Segment::Name(name), value, columns, row)),
        Value::Map(entries) => entries.into_iter().try_for_each(|(key, value)| entered(keyed(key)?, value, columns, row)),
        Value::Enum(Enum { name, variant, data: data @ (Data::NewType { .. } | Data::Tuple { .. } | Data::Struct { .. }), .. }) =>
            entered(Segment::Name(variant), Value::Struct(Box::new(Struct { name, data })), columns, row),
        leaf @ (Value::Unit
        | Value::Bool(_)
        | Value::Number(_)
        | Value::Char(_)
        | Value::String(_)
        | Value::Bytes(_)
        | Value::Option(None)
        | Value::Seq(_)
        | Value::Tuple(_)
        | Value::Struct(_)
        | Value::Enum(_)) => cell(leaf, columns, row),
    }
}

pub(crate) fn flattened(value: &impl Serialize, columns: &mut Columns) -> Result<Row, EncodingError> {
    let (known, mut row) = (columns.names.len(), Row::default());
    let walked = try bikeshed Result<(), EncodingError> { walked(Serializer::new().human_readable().serialize(value)?, columns, &mut row)? };
    walked.map(|()| row).inspect_err(|_| columns.names.truncate(known))
}
