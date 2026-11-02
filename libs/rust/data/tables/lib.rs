#![feature(error_reporter)]

use std::{convert, error::Report, fmt, sync::Arc, time::Duration};

use arrow::{
    array::new_null_array,
    compute::{CastOptions, can_cast_types, cast_with_options},
    datatypes::{DataType, Field, FieldRef, Schema, SchemaRef, TimeUnit},
    error::ArrowError,
    record_batch::{RecordBatch, RecordBatchOptions},
};
use arrow_schema::extension::{ExtensionType, Uuid};
use itertools::{Either, Itertools as _};
use jiff::Timestamp;
use newtype_uuid::{TypedUuid, TypedUuidKind};
use nonempty_collections::NEVec;
use parquet::errors::ParquetError;
use serde::{Deserializer, Serializer, de::DeserializeOwned};
use serde_with::{Bytes, DeserializeAs, SerializeAs};
use uom::{
    si::{self, Dimension, SI, Unit},
    typenum::Integer,
};
use uuid::ClockSequence;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Record {
    fn schema() -> SchemaRef;
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Column {
    Key,
    Instant,
    Quantity(Units),
}

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

impl Units {
    #[must_use]
    pub fn of<D>() -> Self
    where D: Dimension + ?Sized {
        let factors = [
            (<D::L as Integer>::I32, <<SI<f64> as si::Units<f64>>::length as Unit>::abbreviation()),
            (<D::M as Integer>::I32, <<SI<f64> as si::Units<f64>>::mass as Unit>::abbreviation()),
            (<D::T as Integer>::I32, <<SI<f64> as si::Units<f64>>::time as Unit>::abbreviation()),
            (<D::I as Integer>::I32, <<SI<f64> as si::Units<f64>>::electric_current as Unit>::abbreviation()),
            (<D::Th as Integer>::I32, <<SI<f64> as si::Units<f64>>::thermodynamic_temperature as Unit>::abbreviation()),
            (<D::N as Integer>::I32, <<SI<f64> as si::Units<f64>>::amount_of_substance as Unit>::abbreviation()),
            (<D::J as Integer>::I32, <<SI<f64> as si::Units<f64>>::luminous_intensity as Unit>::abbreviation()),
        ]
        .into_iter()
        .filter(|&(exponent, _)| exponent != 0);
        Self(match factors.clone().next() {
            None => "1".to_owned(),
            Some(_) => factors
                .format_with(" ", |(exponent, symbol), emit| match exponent {
                    1 => emit(&symbol),
                    ..=0 | 2.. => emit(&format_args!("{symbol}{exponent}")),
                })
                .to_string(),
        })
    }
}

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
    #[error("columns {} differ from the record schema", .columns.iter().map(|field| field.name()).format(", "))]
    Schema { found: SchemaRef, columns: NEVec<FieldRef> },
    #[error("column {} holds values its record type refuses", .column.name())]
    Values { column: FieldRef, source: ArrowError },
    #[error("columns refuse their record types{}", fmt::from_fn(|formatter| .errors.iter().try_for_each(|error| write!(formatter, "\n{}", Report::new(error)))))]
    Columns { errors: NEVec<Self> },
    #[error("instant {at} precedes the Unix epoch a version 7 key encodes")]
    Key { at: Timestamp, source: jiff::Error },
    #[error(transparent)]
    Records(#[from] serde_arrow::Error),
    #[error(transparent)]
    Arrow(#[from] ArrowError),
    #[error(transparent)]
    Parquet(#[from] ParquetError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn field(name: String, column: Column, nullable: bool) -> Field {
    match column {
        Column::Key => Field::new(name, DataType::FixedSizeBinary(16), nullable).with_extension_type(Uuid),
        Column::Instant => Field::new(name, DataType::Timestamp(TimeUnit::Microsecond, Some(Arc::from("UTC"))), nullable),
        Column::Quantity(units) => Field::new(name, DataType::Float64, nullable).with_extension_type(units),
    }
}

pub fn decode<T>(batch: &RecordBatch) -> Result<Vec<T>, TablesError>
where T: Record + DeserializeOwned {
    let schema = T::schema();
    let found = batch.project(&projection(&schema, batch.schema_ref())?)?;
    let options = CastOptions { safe: false, ..CastOptions::default() };
    let columns = schema
        .fields()
        .iter()
        .map(|field| {
            found
                .column_by_name(field.name())
                .map_or_else(|| Ok(new_null_array(field.data_type(), found.num_rows())), |column| cast_with_options(column, field.data_type(), &options))
                .map_err(|source| TablesError::Values { column: Arc::clone(field), source })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(|errors| TablesError::Columns { errors })?;
    Ok(serde_arrow::from_record_batch(&RecordBatch::try_new_with_options(schema, columns, &RecordBatchOptions::new().with_row_count(Some(found.num_rows())))?)?)
}

pub fn key<K, C>(context: &C, at: Timestamp) -> Result<TypedUuid<K>, TablesError>
where
    K: TypedUuidKind,
    C: ClockSequence<Output: Into<u128>> + ?Sized,
{
    Duration::try_from(at.as_duration()).map(|since| TypedUuid::new_v7(uuid::Timestamp::from_unix(context, since.as_secs(), since.subsec_nanos()))).map_err(|source| TablesError::Key { at, source })
}

fn projection(expected: &Schema, found: &SchemaRef) -> Result<Vec<usize>, TablesError> {
    fn identical(expected: &Field, found: &Field) -> bool {
        let [expected_extension, found_extension] = [expected, found].map(|side| (side.extension_type_name(), side.extension_type_metadata().filter(|metadata| !metadata.is_empty())));
        expected_extension == found_extension && nested(expected.data_type(), found.data_type())
    }
    fn nested(expected: &DataType, found: &DataType) -> bool {
        match (expected, found) {
            (_, DataType::Dictionary(_, values)) => nested(expected, values),
            (_, DataType::RunEndEncoded(_, values)) => nested(expected, values.data_type()),
            (DataType::Struct(fields), DataType::Struct(columns)) => fields.iter().all(|field| columns.find(field.name()).is_some_and(|(_, column)| identical(field, column))),
            (DataType::Union(fields, _), DataType::Union(columns, _)) =>
                fields.iter().all(|(_, field)| columns.iter().find(|(_, column)| column.name() == field.name()).is_some_and(|(_, column)| identical(field, column))),
            (
                DataType::List(field) | DataType::LargeList(field) | DataType::ListView(field) | DataType::LargeListView(field) | DataType::FixedSizeList(field, _),
                DataType::List(column) | DataType::LargeList(column) | DataType::ListView(column) | DataType::LargeListView(column) | DataType::FixedSizeList(column, _),
            )
            | (DataType::Map(field, _), DataType::Map(column, _)) => identical(field, column),
            _ => can_cast_types(found, expected),
        }
    }
    let (positions, refused): (Vec<_>, Vec<_>) = expected
        .fields()
        .iter()
        .filter_map(|field| match found.column_with_name(field.name()) {
            Some((position, column)) if identical(field, column) => Some(Either::Left(position)),
            None if field.is_nullable() => None,
            Some(_) | None => Some(Either::Right(Arc::clone(field))),
        })
        .partition_map(convert::identity);
    NEVec::try_from_vec(refused).map_or(Ok(positions), |columns| Err(TablesError::Schema { found: Arc::clone(found), columns }))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod storage;
