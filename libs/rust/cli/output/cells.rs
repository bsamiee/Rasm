use core::{
    fmt::{Display, Write as _},
    mem,
};

use indexmap::IndexSet;
use serde::{
    Serialize,
    ser::{self, Error as _},
};
use serde_json::ser::{CompactFormatter, Formatter as _};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Place {
    Root,
    Field,
    Key,
}

// --- [MODELS] --------------------------------------------------------------------------

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
    pub(crate) names: IndexSet<Box<str>>,
    path: String,
}

struct State<'columns> {
    columns: &'columns mut Columns,
    row: Row,
    compound: bool,
}

struct Serializer<'state, 'columns> {
    state: &'state mut State<'columns>,
    place: Place,
}

enum Record<'state, 'columns> {
    Fields { state: &'state mut State<'columns>, root: bool, base: usize, end: usize },
    Key,
}

enum Sequence<'state, 'columns> {
    Positional { state: &'state mut State<'columns>, index: usize },
    Json { state: &'state mut State<'columns>, end: usize, first: bool, text: Vec<u8> },
    Key,
}

impl<'state, 'columns> ser::Serializer for Serializer<'state, 'columns> {
    type Ok = ();
    type Error = serde_json::Error;
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
        match self.place {
            Place::Key => self.state.compound = true,
            Place::Root | Place::Field => {
                let end = tagged(self.state, self.place, variant);
                value.serialize(Serializer { state: &mut *self.state, place: Place::Field })?;
                self.state.columns.path.truncate(end);
            }
        }
        Ok(())
    }

    fn serialize_seq(self, _len: Option<usize>) -> Result<Sequence<'state, 'columns>, Self::Error> {
        let end = self.state.columns.path.len();
        sequence(self, end)
    }

    fn serialize_tuple(self, len: usize) -> Result<Sequence<'state, 'columns>, Self::Error> { self.serialize_seq(Some(len)) }

    fn serialize_tuple_struct(self, _name: &'static str, len: usize) -> Result<Sequence<'state, 'columns>, Self::Error> { self.serialize_seq(Some(len)) }

    fn serialize_tuple_variant(self, _name: &'static str, _variant_index: u32, variant: &'static str, _len: usize) -> Result<Sequence<'state, 'columns>, Self::Error> {
        match self.place {
            Place::Key => sequence(self, 0),
            Place::Root | Place::Field => {
                let end = tagged(self.state, self.place, variant);
                sequence(Serializer { state: self.state, place: Place::Field }, end)
            }
        }
    }

    fn serialize_map(self, _len: Option<usize>) -> Result<Record<'state, 'columns>, Self::Error> {
        let end = self.state.columns.path.len();
        Ok(record(self, end))
    }

    fn serialize_struct(self, _name: &'static str, len: usize) -> Result<Record<'state, 'columns>, Self::Error> { self.serialize_map(Some(len)) }

    fn serialize_struct_variant(self, _name: &'static str, _variant_index: u32, variant: &'static str, _len: usize) -> Result<Record<'state, 'columns>, Self::Error> {
        match self.place {
            Place::Key => Ok(record(self, 0)),
            Place::Root | Place::Field => {
                let end = tagged(self.state, self.place, variant);
                Ok(record(Serializer { state: self.state, place: Place::Field }, end))
            }
        }
    }

    fn collect_str<T>(self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Display {
        leaf(self, value, false)
    }
}

impl ser::SerializeMap for Record<'_, '_> {
    type Ok = ();
    type Error = serde_json::Error;

    fn serialize_key<T>(&mut self, key: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        match self {
            Record::Fields { state, root, base: _, end: _ } => {
                if !*root {
                    state.columns.path.push('.');
                }
                let start = state.columns.path.len();
                key.serialize(Serializer { state, place: Place::Key })?;
                if mem::take(&mut state.compound) {
                    state.columns.path.truncate(start);
                    state.columns.path.push_str(&serde_json::to_string(key)?);
                }
                Ok(())
            }
            Record::Key => Ok(()),
        }
    }

    fn serialize_value<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        match self {
            Record::Fields { state, root: _, base, end: _ } => {
                value.serialize(Serializer { state, place: Place::Field })?;
                state.columns.path.truncate(*base);
                Ok(())
            }
            Record::Key => Ok(()),
        }
    }

    fn end(self) -> Result<(), Self::Error> {
        ended(self);
        Ok(())
    }
}

impl ser::SerializeStruct for Record<'_, '_> {
    type Ok = ();
    type Error = serde_json::Error;

    fn serialize_field<T>(&mut self, key: &'static str, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeMap::serialize_entry(self, key, value)
    }

    fn end(self) -> Result<(), Self::Error> {
        ended(self);
        Ok(())
    }
}

impl ser::SerializeStructVariant for Record<'_, '_> {
    type Ok = ();
    type Error = serde_json::Error;

    fn serialize_field<T>(&mut self, key: &'static str, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeStruct::serialize_field(self, key, value)
    }

    fn end(self) -> Result<(), Self::Error> {
        ended(self);
        Ok(())
    }
}

impl ser::SerializeSeq for Sequence<'_, '_> {
    type Ok = ();
    type Error = serde_json::Error;

    fn serialize_element<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        match self {
            Sequence::Positional { state, index } => {
                state.columns.path.push_str(itoa::Buffer::new().format(*index));
                *index = index.strict_add(1);
                value.serialize(Serializer { state, place: Place::Field })?;
                state.columns.path.clear();
                Ok(())
            }
            Sequence::Json { state: _, end: _, first, text } => {
                CompactFormatter.begin_array_value(text, mem::replace(first, false)).map_err(serde_json::Error::io)?;
                serde_json::to_writer(&mut *text, value)
            }
            Sequence::Key => Ok(()),
        }
    }

    fn end(self) -> Result<(), Self::Error> {
        match self {
            Sequence::Positional { state: _, index: _ } | Sequence::Key => Ok(()),
            Sequence::Json { state, end, first: _, mut text } => {
                CompactFormatter.end_array(&mut text).map_err(serde_json::Error::io)?;
                cell(state, String::from_utf8(text).map_err(serde_json::Error::custom)?, false);
                state.columns.path.truncate(end);
                Ok(())
            }
        }
    }
}

impl ser::SerializeTuple for Sequence<'_, '_> {
    type Ok = ();
    type Error = serde_json::Error;

    fn serialize_element<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeSeq::serialize_element(self, value)
    }

    fn end(self) -> Result<(), Self::Error> { ser::SerializeSeq::end(self) }
}

impl ser::SerializeTupleStruct for Sequence<'_, '_> {
    type Ok = ();
    type Error = serde_json::Error;

    fn serialize_field<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeSeq::serialize_element(self, value)
    }

    fn end(self) -> Result<(), Self::Error> { ser::SerializeSeq::end(self) }
}

impl ser::SerializeTupleVariant for Sequence<'_, '_> {
    type Ok = ();
    type Error = serde_json::Error;

    fn serialize_field<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeSeq::serialize_element(self, value)
    }

    fn end(self) -> Result<(), Self::Error> { ser::SerializeSeq::end(self) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn cell(state: &mut State<'_>, text: String, numeric: bool) {
    let Columns { names, path } = &mut *state.columns;
    let index = names.get_index_of(path.as_str()).unwrap_or_else(|| names.insert_full(Box::from(path.as_str())).0);
    let field = Some(Field { text, numeric });
    if let Some(slot) = state.row.fields.get_mut(index) {
        *slot = field;
    } else {
        state.row.fields.resize_with(index, || None);
        state.row.fields.push(field);
    }
}

fn leaf(Serializer { state, place }: Serializer<'_, '_>, text: impl Display, numeric: bool) -> Result<(), serde_json::Error> {
    match place {
        Place::Key => write!(state.columns.path, "{text}").map_err(serde_json::Error::custom),
        Place::Root | Place::Field => {
            let mut written = String::new();
            write!(written, "{text}").map_err(serde_json::Error::custom)?;
            cell(state, written, numeric);
            Ok(())
        }
    }
}

fn tagged(state: &mut State<'_>, place: Place, variant: &str) -> usize {
    let end = state.columns.path.len();
    if !matches!(place, Place::Root) {
        state.columns.path.push('.');
    }
    state.columns.path.push_str(variant);
    state.row.named = true;
    end
}

const fn record<'state, 'columns>(Serializer { state, place }: Serializer<'state, 'columns>, end: usize) -> Record<'state, 'columns> {
    match place {
        Place::Key => {
            state.compound = true;
            Record::Key
        }
        Place::Root | Place::Field => {
            state.row.named = true;
            let base = state.columns.path.len();
            Record::Fields { state, root: matches!(place, Place::Root), base, end }
        }
    }
}

fn ended(record: Record<'_, '_>) {
    if let Record::Fields { state, root: _, base: _, end } = record {
        state.columns.path.truncate(end);
    }
}

fn sequence<'state, 'columns>(Serializer { state, place }: Serializer<'state, 'columns>, end: usize) -> Result<Sequence<'state, 'columns>, serde_json::Error> {
    match place {
        Place::Root => Ok(Sequence::Positional { state, index: 0 }),
        Place::Field => {
            let mut text = Vec::new();
            CompactFormatter.begin_array(&mut text).map_err(serde_json::Error::io)?;
            Ok(Sequence::Json { state, end, first: true, text })
        }
        Place::Key => {
            state.compound = true;
            Ok(Sequence::Key)
        }
    }
}

pub(crate) fn flattened(value: &impl Serialize, columns: &mut Columns) -> Result<Row, serde_json::Error> {
    let known = columns.names.len();
    columns.path.clear();
    let mut state = State { columns, row: Row::default(), compound: false };
    match value.serialize(Serializer { state: &mut state, place: Place::Root }) {
        Ok(()) => Ok(state.row),
        Err(source) => {
            state.columns.names.truncate(known);
            Err(source)
        }
    }
}
