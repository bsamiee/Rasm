use core::{
    fmt::{Display, Write as _},
    mem,
};

use indexmap::IndexMap;
use serde::{Serialize, ser};
use serde_json::ser::{CompactFormatter, Formatter as _};

use crate::CellError;

// --- [MODELS] --------------------------------------------------------------------------

pub(crate) struct Field {
    pub(crate) text: String,
    pub(crate) numeric: bool,
}

#[derive(Default)]
pub(crate) struct Row {
    pub(crate) named: bool,
    pub(crate) fields: IndexMap<String, Field>,
}

struct Column<'row> {
    path: Option<String>,
    row: &'row mut Row,
}

struct Record<'row> {
    parent: Column<'row>,
    key: String,
}

enum Sequence<'row> {
    Positional { parent: Column<'row>, index: usize },
    Json { parent: Column<'row>, first: bool, text: Vec<u8> },
}

impl<'row> ser::Serializer for Column<'row> {
    type Ok = ();
    type Error = CellError;
    type SerializeSeq = Sequence<'row>;
    type SerializeTuple = Sequence<'row>;
    type SerializeTupleStruct = Sequence<'row>;
    type SerializeTupleVariant = Sequence<'row>;
    type SerializeMap = Record<'row>;
    type SerializeStruct = Record<'row>;
    type SerializeStructVariant = Record<'row>;

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

    fn serialize_f32(self, v: f32) -> Result<(), Self::Error> { leaf(self, zmij::Buffer::new().format(v), true) }

    fn serialize_f64(self, v: f64) -> Result<(), Self::Error> { leaf(self, zmij::Buffer::new().format(v), true) }

    fn serialize_char(self, v: char) -> Result<(), Self::Error> { leaf(self, v, false) }

    fn serialize_str(self, v: &str) -> Result<(), Self::Error> { leaf(self, v, false) }

    fn serialize_bytes(self, v: &[u8]) -> Result<(), Self::Error> { leaf(self, String::from_utf8_lossy(v), false) }

    fn serialize_none(self) -> Result<(), Self::Error> { leaf(self, "", false) }

    fn serialize_some<T>(self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        value.serialize(self)
    }

    fn serialize_unit(self) -> Result<(), Self::Error> { leaf(self, "", false) }

    fn serialize_unit_struct(self, _name: &'static str) -> Result<(), Self::Error> { leaf(self, "", false) }

    fn serialize_unit_variant(self, _name: &'static str, _variant_index: u32, variant: &'static str) -> Result<(), Self::Error> { leaf(self, variant, false) }

    fn serialize_newtype_struct<T>(self, _name: &'static str, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        value.serialize(self)
    }

    fn serialize_newtype_variant<T>(self, _name: &'static str, _variant_index: u32, variant: &'static str, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        value.serialize(tagged(self, variant))
    }

    fn serialize_seq(self, _len: Option<usize>) -> Result<Sequence<'row>, Self::Error> { sequence(self) }

    fn serialize_tuple(self, _len: usize) -> Result<Sequence<'row>, Self::Error> { sequence(self) }

    fn serialize_tuple_struct(self, _name: &'static str, _len: usize) -> Result<Sequence<'row>, Self::Error> { sequence(self) }

    fn serialize_tuple_variant(self, _name: &'static str, _variant_index: u32, variant: &'static str, _len: usize) -> Result<Sequence<'row>, Self::Error> { sequence(tagged(self, variant)) }

    fn serialize_map(self, _len: Option<usize>) -> Result<Record<'row>, Self::Error> { Ok(record(self)) }

    fn serialize_struct(self, _name: &'static str, _len: usize) -> Result<Record<'row>, Self::Error> { Ok(record(self)) }

    fn serialize_struct_variant(self, _name: &'static str, _variant_index: u32, variant: &'static str, _len: usize) -> Result<Record<'row>, Self::Error> { Ok(record(tagged(self, variant))) }

    fn collect_str<T>(self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Display {
        leaf(self, value, false)
    }
}

impl ser::SerializeMap for Record<'_> {
    type Ok = ();
    type Error = CellError;

    fn serialize_key<T>(&mut self, key: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        let mut label = Row::default();
        key.serialize(Column { path: None, row: &mut label })?;
        self.key = match label.fields.pop() {
            Some((_, Field { text, numeric: _ })) if label.fields.is_empty() => text,
            Some(_) | None => return Err(CellError::CompoundKey),
        };
        Ok(())
    }

    fn serialize_value<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        value.serialize(child(&mut self.parent, &mem::take(&mut self.key)))
    }

    fn end(self) -> Result<(), Self::Error> { Ok(()) }
}

impl ser::SerializeStruct for Record<'_> {
    type Ok = ();
    type Error = CellError;

    fn serialize_field<T>(&mut self, key: &'static str, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        value.serialize(child(&mut self.parent, key))
    }

    fn end(self) -> Result<(), Self::Error> { Ok(()) }
}

impl ser::SerializeStructVariant for Record<'_> {
    type Ok = ();
    type Error = CellError;

    fn serialize_field<T>(&mut self, key: &'static str, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeStruct::serialize_field(self, key, value)
    }

    fn end(self) -> Result<(), Self::Error> { Ok(()) }
}

impl ser::SerializeSeq for Sequence<'_> {
    type Ok = ();
    type Error = CellError;

    fn serialize_element<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        match self {
            Sequence::Positional { parent, index } => {
                let key = index.to_string();
                *index = index.strict_add(1);
                value.serialize(child(parent, &key))
            }
            Sequence::Json { parent: _, first, text } => {
                CompactFormatter.begin_array_value(text, mem::replace(first, false))?;
                Ok(serde_json::to_writer(&mut *text, value)?)
            }
        }
    }

    fn end(self) -> Result<(), Self::Error> {
        match self {
            Sequence::Positional { parent: _, index: _ } => Ok(()),
            Sequence::Json { parent, first: _, mut text } => {
                CompactFormatter.end_array(&mut text)?;
                leaf(parent, String::from_utf8(text)?, false)
            }
        }
    }
}

impl ser::SerializeTuple for Sequence<'_> {
    type Ok = ();
    type Error = CellError;

    fn serialize_element<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeSeq::serialize_element(self, value)
    }

    fn end(self) -> Result<(), Self::Error> { ser::SerializeSeq::end(self) }
}

impl ser::SerializeTupleStruct for Sequence<'_> {
    type Ok = ();
    type Error = CellError;

    fn serialize_field<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeSeq::serialize_element(self, value)
    }

    fn end(self) -> Result<(), Self::Error> { ser::SerializeSeq::end(self) }
}

impl ser::SerializeTupleVariant for Sequence<'_> {
    type Ok = ();
    type Error = CellError;

    fn serialize_field<T>(&mut self, value: &T) -> Result<(), Self::Error>
    where T: ?Sized + Serialize {
        ser::SerializeSeq::serialize_element(self, value)
    }

    fn end(self) -> Result<(), Self::Error> { ser::SerializeSeq::end(self) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn leaf(Column { path, row }: Column<'_>, text: impl Display, numeric: bool) -> Result<(), CellError> {
    let mut written = String::new();
    write!(written, "{text}")?;
    row.fields.insert(path.unwrap_or_default(), Field { text: written, numeric });
    Ok(())
}

fn child<'parent>(parent: &'parent mut Column<'_>, key: &str) -> Column<'parent> {
    Column { path: Some(parent.path.as_ref().map_or_else(|| key.to_owned(), |path| format!("{path}.{key}"))), row: &mut *parent.row }
}

fn tagged<'row>(mut column: Column<'row>, variant: &str) -> Column<'row> {
    column.row.named = true;
    let path = child(&mut column, variant).path;
    Column { path, ..column }
}

const fn record(column: Column<'_>) -> Record<'_> {
    column.row.named = true;
    Record { parent: column, key: String::new() }
}

fn sequence(column: Column<'_>) -> Result<Sequence<'_>, CellError> {
    if column.path.is_none() {
        Ok(Sequence::Positional { parent: column, index: 0 })
    } else {
        let mut text = Vec::new();
        CompactFormatter.begin_array(&mut text)?;
        Ok(Sequence::Json { parent: column, first: true, text })
    }
}

pub(crate) fn flattened(value: &impl Serialize) -> Result<Row, CellError> {
    let mut row = Row::default();
    value.serialize(Column { path: None, row: &mut row })?;
    Ok(row)
}
