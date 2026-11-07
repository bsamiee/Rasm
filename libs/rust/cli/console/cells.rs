use core::{
    fmt::{Display, Write as _},
    iter, mem,
};
use std::borrow::Cow;

use indexmap::{IndexMap, map::RawEntryApiV1 as _};
use serde::{
    Serialize,
    ser::{self, Error as _},
};
use serde_content::{Data, Enum, Number, Struct, Value};
use serde_json::ser::{CompactFormatter, Formatter as _};

use crate::EncodingError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, PartialEq, Eq, Hash)]
pub(crate) enum Segment {
    Name(Cow<'static, str>),
    Key { kind: mem::Discriminant<Value<'static>>, json: Box<str> },
    Index(usize),
}

pub(crate) struct Field {
    pub(crate) text: Vec<u8>,
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
    text: String,
}

impl Columns {
    const fn position(&self) -> (usize, usize) { (self.path.len(), self.text.len()) }

    fn truncate(&mut self, (depth, end): (usize, usize)) {
        self.path.truncate(depth);
        self.text.truncate(end);
    }
}

struct State<'columns> {
    columns: &'columns mut Columns,
    row: Row,
}

struct Record<'state, 'columns> {
    state: &'state mut State<'columns>,
    base: (usize, usize),
    end: (usize, usize),
}

enum Sequence<'state, 'columns> {
    Positional { state: &'state mut State<'columns>, index: usize },
    Json { state: &'state mut State<'columns>, end: (usize, usize), first: bool, text: Vec<u8> },
}

impl<'state, 'columns> ser::Serializer for &'state mut State<'columns> {
    type Ok = ();
    type Error = EncodingError;
    type SerializeSeq = Sequence<'state, 'columns>;
    type SerializeTuple = Sequence<'state, 'columns>;
    type SerializeTupleStruct = Sequence<'state, 'columns>;
    type SerializeTupleVariant = Sequence<'state, 'columns>;
    type SerializeMap = Record<'state, 'columns>;
    type SerializeStruct = Record<'state, 'columns>;
    type SerializeStructVariant = Record<'state, 'columns>;

    fn serialize_bool(self, v: bool) -> Result<(), Self::Error> { leaf(self, v, false) }

    fn serialize_i8(self, v: i8) -> Result<(), Self::Error> { leaf(self, itoa::Buffer::new().format(v), true) }

    fn serialize_i16(self, v: i16) -> Result<(), Self::Error> { leaf(self, itoa::Buffer::new().format(v), true) }

    fn serialize_i32(self, v: i32) -> Result<(), Self::Error> { leaf(self, itoa::Buffer::new().format(v), true) }

    fn serialize_i64(self, v: i64) -> Result<(), Self::Error> { leaf(self, itoa::Buffer::new().format(v), true) }

    fn serialize_i128(self, v: i128) -> Result<(), Self::Error> { leaf(self, itoa::Buffer::new().format(v), true) }

    fn serialize_u8(self, v: u8) -> Result<(), Self::Error> { leaf(self, itoa::Buffer::new().format(v), true) }

    fn serialize_u16(self, v: u16) -> Result<(), Self::Error> { leaf(self, itoa::Buffer::new().format(v), true) }

    fn serialize_u32(self, v: u32) -> Result<(), Self::Error> { leaf(self, itoa::Buffer::new().format(v), true) }

    fn serialize_u64(self, v: u64) -> Result<(), Self::Error> { leaf(self, itoa::Buffer::new().format(v), true) }

    fn serialize_u128(self, v: u128) -> Result<(), Self::Error> { leaf(self, itoa::Buffer::new().format(v), true) }

    fn serialize_f32(self, v: f32) -> Result<(), Self::Error> { if v.is_finite() { leaf(self, zmij::Buffer::new().format_finite(v), true) } else { self.serialize_none() } }

    fn serialize_f64(self, v: f64) -> Result<(), Self::Error> { if v.is_finite() { leaf(self, zmij::Buffer::new().format_finite(v), true) } else { self.serialize_none() } }

    fn serialize_char(self, v: char) -> Result<(), Self::Error> { leaf(self, v, false) }

    fn serialize_str(self, v: &str) -> Result<(), Self::Error> { leaf(self, v, false) }

    fn serialize_bytes(self, v: &[u8]) -> Result<(), Self::Error> { leaf(self, String::from_utf8_lossy(v), false) }

    fn serialize_none(self) -> Result<(), Self::Error> { leaf(self, "", false) }

    fn serialize_some<T>(self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        value.serialize(self)
    }

    fn serialize_unit(self) -> Result<(), Self::Error> { leaf(self, "", false) }

    fn serialize_unit_struct(self, _name: &'static str) -> Result<(), Self::Error> { self.serialize_unit() }

    fn serialize_unit_variant(self, _name: &'static str, _variant_index: u32, variant: &'static str) -> Result<(), Self::Error> { leaf(self, variant, false) }

    fn serialize_newtype_struct<T>(self, _name: &'static str, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        value.serialize(self)
    }

    fn serialize_newtype_variant<T>(self, _name: &'static str, _variant_index: u32, variant: &'static str, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        let end = tagged(self, variant);
        value.serialize(&mut *self)?;
        self.columns.truncate(end);
        Ok(())
    }

    fn serialize_seq(self, _len: Option<usize>) -> Result<Sequence<'state, 'columns>, Self::Error> {
        let end = self.columns.position();
        sequence(self, end)
    }

    fn serialize_tuple(self, len: usize) -> Result<Sequence<'state, 'columns>, Self::Error> { self.serialize_seq(Some(len)) }

    fn serialize_tuple_struct(self, _name: &'static str, len: usize) -> Result<Sequence<'state, 'columns>, Self::Error> { self.serialize_seq(Some(len)) }

    fn serialize_tuple_variant(self, _name: &'static str, _variant_index: u32, variant: &'static str, _len: usize) -> Result<Sequence<'state, 'columns>, Self::Error> {
        let end = tagged(self, variant);
        sequence(self, end)
    }

    fn serialize_map(self, _len: Option<usize>) -> Result<Record<'state, 'columns>, Self::Error> {
        let end = self.columns.position();
        Ok(record(self, end))
    }

    fn serialize_struct(self, _name: &'static str, len: usize) -> Result<Record<'state, 'columns>, Self::Error> { self.serialize_map(Some(len)) }

    fn serialize_struct_variant(self, _name: &'static str, _variant_index: u32, variant: &'static str, _len: usize) -> Result<Record<'state, 'columns>, Self::Error> {
        let end = tagged(self, variant);
        Ok(record(self, end))
    }

    fn collect_str<T>(self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Display {
        leaf(self, value, false)
    }
}

impl ser::SerializeMap for Record<'_, '_> {
    type Ok = ();
    type Error = EncodingError;

    fn serialize_key<T>(&mut self, key: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        fn inner<'value>(value: &'value Value<'static>) -> Option<&'value Value<'static>> {
            match value {
                Value::Option(Some(value)) => Some(value),
                Value::Struct(Struct { data: Data::NewType { value }, .. }) => Some(value),
                Value::Unit
                | Value::Bool(_)
                | Value::Number(_)
                | Value::Char(_)
                | Value::String(_)
                | Value::Bytes(_)
                | Value::Seq(_)
                | Value::Map(_)
                | Value::Option(None)
                | Value::Struct(_)
                | Value::Enum(_)
                | Value::Tuple(_) => None,
            }
        }
        let value = serde_content::Serializer::new().human_readable().serialize(key)?;
        if self.base.0 != 0 {
            self.state.columns.text.push('.');
        }
        let segment = match value {
            Value::String(name) => {
                self.state.columns.text.push_str(&name);
                Segment::Name(name)
            }
            value => {
                let json = serde_json::to_string(&value)?;
                let terminal = iter::successors(inner(&value), |&value| inner(value)).last().unwrap_or(&value);
                match terminal {
                    Value::Unit | Value::Option(None) | Value::Struct(Struct { data: Data::Unit, .. }) => (),
                    Value::Number(Number::F32(value)) if !value.is_finite() => (),
                    Value::Number(Number::F64(value)) if !value.is_finite() => (),
                    Value::Char(value) => self.state.columns.text.push(*value),
                    Value::String(value) => self.state.columns.text.push_str(value),
                    Value::Bytes(value) => self.state.columns.text.push_str(&String::from_utf8_lossy(value)),
                    Value::Enum(Enum { variant, data: Data::Unit, .. }) => self.state.columns.text.push_str(variant),
                    Value::Bool(_) | Value::Number(_) | Value::Seq(_) | Value::Map(_) | Value::Option(Some(_)) | Value::Struct(_) | Value::Enum(_) | Value::Tuple(_) => {
                        self.state.columns.text.push_str(&json);
                    }
                }
                Segment::Key { kind: mem::discriminant(&value), json: json.into_boxed_str() }
            }
        };
        self.state.columns.path.push(segment);
        Ok(())
    }

    fn serialize_value<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        value.serialize(&mut *self.state)?;
        self.state.columns.truncate(self.base);
        Ok(())
    }

    fn end(self) -> Result<(), Self::Error> {
        self.state.columns.truncate(self.end);
        Ok(())
    }
}

impl ser::SerializeStruct for Record<'_, '_> {
    type Ok = ();
    type Error = EncodingError;

    fn serialize_field<T>(&mut self, key: &'static str, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        if self.base.0 != 0 {
            self.state.columns.text.push('.');
        }
        self.state.columns.text.push_str(key);
        self.state.columns.path.push(Segment::Name(Cow::Borrowed(key)));
        ser::SerializeMap::serialize_value(self, value)
    }

    fn end(self) -> Result<(), Self::Error> {
        self.state.columns.truncate(self.end);
        Ok(())
    }
}

impl ser::SerializeStructVariant for Record<'_, '_> {
    type Ok = ();
    type Error = EncodingError;

    fn serialize_field<T>(&mut self, key: &'static str, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeStruct::serialize_field(self, key, value)
    }

    fn end(self) -> Result<(), Self::Error> {
        self.state.columns.truncate(self.end);
        Ok(())
    }
}

impl ser::SerializeSeq for Sequence<'_, '_> {
    type Ok = ();
    type Error = EncodingError;

    fn serialize_element<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        match self {
            Sequence::Positional { state, index } => {
                state.columns.text.push_str(itoa::Buffer::new().format(*index));
                state.columns.path.push(Segment::Index(*index));
                *index = index.strict_add(1);
                value.serialize(&mut **state)?;
                state.columns.truncate((0, 0));
                Ok(())
            }
            Sequence::Json { state: _, end: _, first, text } => {
                CompactFormatter.begin_array_value(text, mem::replace(first, false)).map_err(serde_json::Error::io)?;
                serde_json::to_writer(text, value).map_err(EncodingError::from)
            }
        }
    }

    fn end(self) -> Result<(), Self::Error> {
        match self {
            Sequence::Positional { state: _, index: _ } => Ok(()),
            Sequence::Json { state, end, first: _, mut text } => {
                CompactFormatter.end_array(&mut text).map_err(serde_json::Error::io)?;
                cell(state, text, false);
                state.columns.truncate(end);
                Ok(())
            }
        }
    }
}

impl ser::SerializeTuple for Sequence<'_, '_> {
    type Ok = ();
    type Error = EncodingError;

    fn serialize_element<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeSeq::serialize_element(self, value)
    }

    fn end(self) -> Result<(), Self::Error> { ser::SerializeSeq::end(self) }
}

impl ser::SerializeTupleStruct for Sequence<'_, '_> {
    type Ok = ();
    type Error = EncodingError;

    fn serialize_field<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeSeq::serialize_element(self, value)
    }

    fn end(self) -> Result<(), Self::Error> { ser::SerializeSeq::end(self) }
}

impl ser::SerializeTupleVariant for Sequence<'_, '_> {
    type Ok = ();
    type Error = EncodingError;

    fn serialize_field<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeSeq::serialize_element(self, value)
    }

    fn end(self) -> Result<(), Self::Error> { ser::SerializeSeq::end(self) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn cell(state: &mut State<'_>, text: Vec<u8>, numeric: bool) {
    let Columns { names, path, text: name } = &mut *state.columns;
    let entry = names.raw_entry_mut_v1().from_key(path.as_slice());
    let index = entry.index();
    entry.or_insert_with(|| (path.clone().into_boxed_slice(), Box::from(name.as_str())));
    let field = Some(Field { text, numeric });
    if let Some(slot) = state.row.fields.get_mut(index) {
        *slot = field;
    } else {
        state.row.fields.resize_with(index, || None);
        state.row.fields.push(field);
    }
}

fn leaf(state: &mut State<'_>, text: impl Display, numeric: bool) -> Result<(), EncodingError> {
    let mut written = String::new();
    write!(written, "{text}").map_err(EncodingError::custom)?;
    cell(state, written.into_bytes(), numeric);
    Ok(())
}

fn tagged(state: &mut State<'_>, variant: &'static str) -> (usize, usize) {
    let end = state.columns.position();
    if !state.columns.path.is_empty() {
        state.columns.text.push('.');
    }
    state.columns.text.push_str(variant);
    state.columns.path.push(Segment::Name(Cow::Borrowed(variant)));
    state.row.named = true;
    end
}

const fn record<'state, 'columns>(state: &'state mut State<'columns>, end: (usize, usize)) -> Record<'state, 'columns> {
    state.row.named = true;
    let base = state.columns.position();
    Record { state, base, end }
}

fn sequence<'state, 'columns>(state: &'state mut State<'columns>, end: (usize, usize)) -> Result<Sequence<'state, 'columns>, EncodingError> {
    if state.columns.path.is_empty() {
        Ok(Sequence::Positional { state, index: 0 })
    } else {
        let mut text = Vec::new();
        CompactFormatter.begin_array(&mut text).map_err(serde_json::Error::io)?;
        Ok(Sequence::Json { state, end, first: true, text })
    }
}

pub(crate) fn flattened(value: &impl Serialize, columns: &mut Columns) -> Result<Row, EncodingError> {
    let known = columns.names.len();
    columns.truncate((0, 0));
    let mut state = State { columns, row: Row::default() };
    match value.serialize(&mut state) {
        Ok(()) => Ok(state.row),
        Err(source) => {
            state.columns.names.truncate(known);
            Err(source)
        }
    }
}
