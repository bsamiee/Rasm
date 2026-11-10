#![feature(array_try_map, default_field_values, error_reporter, option_get_or_try_insert_with, try_blocks_heterogeneous)]

use core::iter;
use core::time::Duration;
use std::error::Report;
use std::fmt;
use std::sync::Arc;

use arrow::array::{Array as _, ArrayRef, AsArray as _, MapArray, StructArray, make_array, new_null_array};
use arrow::compute::{CastOptions, cast_with_options};
use arrow::datatypes::{DataType, Field, FieldRef, Fields, Schema, SchemaRef, TimeUnit};
use arrow::error::ArrowError;
use arrow::record_batch::RecordBatch;
use arrow_schema::extension::{ExtensionType, Uuid};
use itertools::Itertools as _;
use jiff::Timestamp;
use newtype_uuid::{TypedUuid, TypedUuidKind};
use nonempty_collections::NEVec;
use parquet::errors::ParquetError;
use serde::de::DeserializeOwned;
use serde::{Deserializer, Serializer};
use serde_with::{Bytes, DeserializeAs, SerializeAs};
use uom::Conversion;
use uom::num::Num;
use uom::si::{self, Dimension, Quantity, Unit as _};
use uom::typenum::Integer;
use uuid::ClockSequence;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Record {
    fn schema() -> SchemaRef;
}

pub trait Column {
    fn field(name: String) -> Field;
}

impl<K> Column for TypedUuid<K>
where K: TypedUuidKind
{
    fn field(name: String) -> Field { Field::new(name, DataType::FixedSizeBinary(16), false).with_extension_type(Uuid) }
}

impl Column for Timestamp {
    fn field(name: String) -> Field { Field::new(name, DataType::Timestamp(TimeUnit::Nanosecond, Some(Arc::from("UTC"))), false) }
}

impl<D, U, V> Column for Quantity<D, U, V>
where
    D: Dimension + ?Sized,
    U: si::Units<V> + ?Sized,
    V: Num + Conversion<V>,
{
    fn field(name: String) -> Field {
        let units = [
            (<D::L as Integer>::I32, U::length::abbreviation()),
            (<D::M as Integer>::I32, U::mass::abbreviation()),
            (<D::T as Integer>::I32, U::time::abbreviation()),
            (<D::I as Integer>::I32, U::electric_current::abbreviation()),
            (<D::Th as Integer>::I32, U::thermodynamic_temperature::abbreviation()),
            (<D::N as Integer>::I32, U::amount_of_substance::abbreviation()),
            (<D::J as Integer>::I32, U::luminous_intensity::abbreviation()),
        ]
        .into_iter()
        .filter(|&(exponent, _)| exponent != 0)
        .format_with(" ", |(exponent, symbol), emit| match exponent {
            1 => emit(&symbol),
            ..=0 | 2.. => emit(&format_args!("{symbol}{exponent}")),
        })
        .to_string();
        Field::new(name, DataType::Float64, false).with_extension_type(Units(if units.is_empty() { "1".to_owned() } else { units }))
    }
}

impl<C> Column for Option<C>
where C: Column
{
    fn field(name: String) -> Field { C::field(name).with_nullable(true) }
}

impl<C> Column for Vec<C>
where C: Column
{
    fn field(name: String) -> Field { Field::new_list(name, C::field(Field::LIST_FIELD_DEFAULT_NAME.to_owned()), false) }
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Key;

impl<K> SerializeAs<TypedUuid<K>> for Key
where K: TypedUuidKind
{
    fn serialize_as<S>(source: &TypedUuid<K>, serializer: S) -> Result<S::Ok, S::Error>
    where S: Serializer {
        Bytes::serialize_as(source.as_bytes(), serializer)
    }
}

impl<'de, K> DeserializeAs<'de, TypedUuid<K>> for Key
where K: TypedUuidKind
{
    fn deserialize_as<D>(deserializer: D) -> Result<TypedUuid<K>, D::Error>
    where D: Deserializer<'de> {
        Bytes::deserialize_as(deserializer).map(TypedUuid::from_bytes)
    }
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Units(String);

impl ExtensionType for Units {
    const NAME: &'static str = "rasm.units";

    type Metadata = String;

    fn metadata(&self) -> &Self::Metadata { &self.0 }

    fn serialize_metadata(&self) -> Option<String> { Some(self.0.clone()) }

    fn deserialize_metadata(metadata: Option<&str>) -> Result<Self::Metadata, ArrowError> {
        metadata.map(str::to_owned).ok_or_else(|| ArrowError::InvalidArgumentError(format!("{} extension type expects unit text", Self::NAME)))
    }

    fn supports_data_type(&self, data_type: &DataType) -> Result<(), ArrowError> {
        match data_type {
            DataType::Float64 => Ok(()),
            found => Err(ArrowError::InvalidArgumentError(format!("{} extension type stores {}, found {found}", Self::NAME, DataType::Float64))),
        }
    }

    fn try_new(data_type: &DataType, metadata: Self::Metadata) -> Result<Self, ArrowError> {
        let units = Self(metadata);
        units.supports_data_type(data_type).map(|()| units)
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum TablesError {
    #[error("columns refuse their record types{}", fmt::from_fn(|formatter| .columns.iter().try_for_each(|(column, source)| write!(formatter, "\n{}: {}", column.name(), Report::new(source)))))]
    Columns { found: SchemaRef, columns: NEVec<(FieldRef, ArrowError)> },
    #[error("record holds no uuid key column {column}")]
    KeyColumn { column: String },
    #[error("instant {at} precedes the Unix epoch a version 7 key encodes")]
    Key { at: Timestamp, source: jiff::Error },
    #[error(transparent)]
    Records(#[from] serde_arrow::Error),
    #[error(transparent)]
    Parquet(#[from] ParquetError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn decode<T>(batch: &RecordBatch) -> Result<Vec<T>, TablesError>
where T: Record + DeserializeOwned {
    rows(&T::schema(), batch)
}

pub fn key<K, C>(context: &C, at: Timestamp) -> Result<TypedUuid<K>, TablesError>
where
    K: TypedUuidKind,
    C: ClockSequence<Output: Into<u128>> + ?Sized,
{
    Duration::try_from(at.as_duration()).map(|since| TypedUuid::new_v7(uuid::Timestamp::from_unix(context, since.as_secs(), since.subsec_nanos()))).map_err(|source| TablesError::Key { at, source })
}

fn rows<T>(schema: &Schema, batch: &RecordBatch) -> Result<Vec<T>, TablesError>
where T: DeserializeOwned {
    let options = CastOptions { safe: false, ..CastOptions::default() };
    let columns = schema
        .fields()
        .iter()
        .map(|field| {
            column(batch.schema_ref().fields(), batch.columns(), batch.num_rows(), field, &options)
                .and_then(|column| match (!field.is_nullable()).then(|| column.logical_null_count()) {
                    None | Some(0) => Ok(column),
                    Some(nulls) => Err(ArrowError::InvalidArgumentError(format!("required column holds nulls in {nulls} rows"))),
                })
                .map_err(|source| (Arc::clone(field), source))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(|columns| TablesError::Columns { found: batch.schema(), columns })?;
    Ok(serde_arrow::from_arrow(schema.fields(), &columns)?)
}

fn column(found: &Fields, columns: &[ArrayRef], rows: usize, expected: &Field, options: &CastOptions<'_>) -> Result<ArrayRef, ArrowError> {
    fn conformed(array: &ArrayRef, found: &Field, expected: &Field, options: &CastOptions<'_>) -> Result<ArrayRef, ArrowError> {
        let [into, from] = [expected, found].map(|side| (side.extension_type_name(), side.extension_type_metadata().filter(|metadata| !metadata.is_empty())));
        let refused = || ArrowError::CastError(format!("{} does not conform to {}", array.data_type(), expected.data_type()));
        match (expected.data_type(), array.data_type()) {
            _ if into != from => Err(ArrowError::SchemaError(format!("extension {from:?} differs from the record's {into:?}"))),
            (target, source) if target == source => Ok(Arc::clone(array)),
            (_, DataType::Dictionary(_, values)) if values.is_nested() => conformed(&cast_with_options(array, values, options)?, found, expected, options),
            (_, DataType::RunEndEncoded(_, values)) if values.data_type().is_nested() => conformed(&cast_with_options(array, values.data_type(), options)?, found, expected, options),
            (DataType::Struct(fields), DataType::Struct(_)) => {
                let structs = array.as_struct();
                let columns = fields.iter().map(|field| column(structs.fields(), structs.columns(), structs.len(), field, options)).collect::<Result<Vec<_>, _>>()?;
                Ok(Arc::new(StructArray::try_new_with_length(fields.clone(), columns, structs.nulls().cloned(), structs.len())?))
            }
            (DataType::Map(entries, sorted), DataType::Map(found_entries, order))
                if sorted == order
                    && let (DataType::Struct(fields), DataType::Struct(found_fields)) = (entries.data_type(), found_entries.data_type())
                    && let ([key, value], [found_key, found_value]) = (&**fields, &**found_fields) =>
            {
                let maps = array.as_map();
                let columns = vec![conformed(maps.keys(), found_key, key, options)?, conformed(maps.values(), found_value, value, options)?];
                Ok(Arc::new(MapArray::try_new(
                    Arc::clone(entries),
                    maps.offsets().clone(),
                    StructArray::try_new(fields.clone(), columns, maps.entries().nulls().cloned())?,
                    maps.nulls().cloned(),
                    *sorted,
                )?))
            }
            (
                DataType::List(item) | DataType::LargeList(item) | DataType::ListView(item) | DataType::LargeListView(item) | DataType::FixedSizeList(item, _),
                source @ (DataType::List(found_item) | DataType::LargeList(found_item) | DataType::ListView(found_item) | DataType::LargeListView(found_item) | DataType::FixedSizeList(found_item, _)),
            ) => {
                let data = array.to_data();
                let items = data.child_data().iter().map(|child| conformed(&make_array(child.clone()), found_item, item, options).map(|values| values.to_data())).collect::<Result<Vec<_>, _>>()?;
                let listed = match source {
                    DataType::LargeList(_) => DataType::LargeList(Arc::clone(item)),
                    DataType::ListView(_) => DataType::ListView(Arc::clone(item)),
                    DataType::LargeListView(_) => DataType::LargeListView(Arc::clone(item)),
                    DataType::FixedSizeList(_, size) => DataType::FixedSizeList(Arc::clone(item), *size),
                    _ => DataType::List(Arc::clone(item)),
                };
                cast_with_options(&make_array(data.into_builder().data_type(listed).child_data(items).build()?), expected.data_type(), options)
            }
            (_, source) if source.is_nested() => Err(refused()),
            (DataType::List(item) | DataType::LargeList(item) | DataType::ListView(item) | DataType::LargeListView(item) | DataType::FixedSizeList(item, _), source)
                if !source.is_null() && item.extension_type_name().is_some() =>
                Err(refused()),
            (target, _) => cast_with_options(array, target, options),
        }
    }
    match iter::zip(found, columns).find(|(field, _)| field.name() == expected.name()) {
        Some((field, array)) => conformed(array, field, expected, options),
        None if expected.is_nullable() => Ok(new_null_array(expected.data_type(), rows)),
        None => Err(ArrowError::SchemaError(format!("required column {} is absent", expected.name()))),
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod storage;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use storage::{Scan, lookup, properties, read, write};
