use core::{borrow::Borrow, future::ready};
use std::{collections::HashSet, sync::Arc};

use arrow::{
    array::{AsArray as _, BooleanArray},
    compute::cast,
    datatypes::Schema,
    record_batch::RecordBatch,
};
use arrow_schema::extension::Uuid;
use futures::{Stream, StreamExt as _, TryStream, TryStreamExt as _, stream};
use newtype_uuid::{TypedUuid, TypedUuidKind};
use parquet::{
    arrow::{
        ArrowSchemaConverter, AsyncArrowWriter, ParquetRecordBatchStreamBuilder, ProjectionMask,
        arrow_reader::{ArrowPredicateFn, RowFilter},
        async_reader::AsyncFileReader,
        async_writer::AsyncFileWriter,
        parquet_column,
    },
    basic::{LogicalType, SortOrder},
    file::{metadata::ParquetMetaData, properties::WriterProperties},
};
use serde::{Serialize, de::DeserializeOwned};

use crate::{Record, TablesError, decode, projection};

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [WRITE]

pub fn properties(schema: &Schema, settings: &WriterProperties) -> Result<WriterProperties, TablesError> {
    Ok(ArrowSchemaConverter::new()
        .with_coerce_types(settings.coerce_types())
        .convert(schema)?
        .columns()
        .iter()
        .filter(|column| column.logical_type_ref() == Some(&LogicalType::Uuid) && settings.bloom_filter_properties(column.path()).is_none())
        .fold(settings.clone().into_builder(), |builder, column| builder.set_column_bloom_filter_enabled(column.path().clone(), true))
        .build())
}

pub async fn write<T, W, S>(async_writer: W, rows: S, settings: &WriterProperties) -> Result<ParquetMetaData, TablesError>
where
    T: Record + Serialize,
    W: AsyncFileWriter,
    S: TryStream<Ok: Borrow<[T]>, Error: Into<TablesError>>,
{
    let schema = T::schema();
    Ok(rows
        .err_into::<TablesError>()
        .and_then(|chunk| ready(serde_arrow::to_record_batch(schema.fields(), &chunk.borrow()).map_err(TablesError::from)))
        .try_fold(AsyncArrowWriter::try_new(async_writer, Arc::clone(&schema), Some(properties(&schema, settings)?))?, async |mut writer, batch| {
            writer.write(&batch).await?;
            Ok::<_, TablesError>(writer)
        })
        .await?
        .close()
        .await?)
}

// --- [READ]

pub fn read<T, R>(builder: ParquetRecordBatchStreamBuilder<R>) -> Result<impl Stream<Item = Result<Vec<T>, TablesError>>, TablesError>
where
    T: Record + DeserializeOwned,
    R: AsyncFileReader + Unpin + 'static,
{
    let mask = ProjectionMask::roots(builder.parquet_schema(), projection(&T::schema(), builder.schema())?);
    Ok(builder.with_projection(mask).build()?.map(|batch| decode(&batch?)))
}

pub async fn lookup<T, R, K, I>(builder: ParquetRecordBatchStreamBuilder<R>, column: &str, keys: I) -> Result<impl Stream<Item = Result<Vec<T>, TablesError>>, TablesError>
where
    T: Record + DeserializeOwned,
    R: AsyncFileReader + Unpin + 'static,
    K: TypedUuidKind,
    I: IntoIterator<Item: Borrow<TypedUuid<K>>>,
{
    let schema = T::schema();
    projection(&schema, builder.schema())?;
    let field = schema.field_with_name(column)?;
    field.try_extension_type::<Uuid>()?;
    let Some((leaf, _)) = parquet_column(builder.parquet_schema(), builder.schema(), column) else {
        return read(builder.with_row_groups(Vec::new()));
    };
    let target = field.data_type().clone();
    let metadata = Arc::clone(builder.metadata());
    let keys = keys.into_iter().map(|key| *key.borrow().as_bytes()).collect::<HashSet<_>>();
    let ordered = metadata.file_metadata().column_order(leaf).sort_order() == SortOrder::UNSIGNED;
    let (builder, groups) = stream::iter(
        metadata
            .row_groups()
            .iter()
            .map(|group| group.column(leaf).statistics().filter(|_| ordered).and_then(|statistics| statistics.min_bytes_opt().zip(statistics.max_bytes_opt())))
            .enumerate()
            .map(|(group, bounds)| (group, keys.iter().filter(|key| bounds.is_none_or(|(min, max)| (min..=max).contains(&key.as_slice()))).collect::<Vec<_>>()))
            .filter(|(_, within)| !within.is_empty()),
    )
    .map(Ok::<_, TablesError>)
    .try_fold((builder, Vec::new()), async |(mut reader, mut groups), (group, within)| {
        let filter = reader.get_row_group_column_bloom_filter(group, leaf).await?;
        groups.extend(within.iter().any(|key| filter.as_ref().is_none_or(|filter| filter.check(key.as_slice()))).then_some(group));
        Ok((reader, groups))
    })
    .await?;
    let predicate = ArrowPredicateFn::new(ProjectionMask::leaves(metadata.file_metadata().schema_descr(), [leaf]), move |batch: RecordBatch| {
        Ok(cast(batch.column(0), &target)?.as_fixed_size_binary().iter().map(|key| key.map(|key| keys.contains(key))).collect::<BooleanArray>())
    });
    read(builder.with_row_groups(groups).with_row_filter(RowFilter::new(vec![Box::new(predicate)])))
}
