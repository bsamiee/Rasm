use core::{borrow::Borrow, iter, ops::Bound};
use std::{
    collections::{BTreeSet, HashSet},
    sync::Arc,
};

use arrow::{
    array::{AsArray as _, BooleanArray},
    compute::cast,
    datatypes::Schema,
    record_batch::RecordBatch,
};
use arrow_schema::extension::Uuid;
use futures::{Stream, StreamExt as _, TryStream, TryStreamExt as _, future::OptionFuture, stream};
use itertools::Itertools as _;
use newtype_uuid::{TypedUuid, TypedUuidKind};
use parquet::{
    arrow::{
        ArrowSchemaConverter, AsyncArrowWriter, ParquetRecordBatchStreamBuilder, ProjectionMask,
        arrow_reader::{ArrowPredicate, ArrowPredicateFn, RowFilter, RowSelection, RowSelector, statistics::StatisticsConverter},
        async_reader::AsyncFileReader,
        async_writer::AsyncFileWriter,
        parquet_column,
    },
    basic::{LogicalType, SortOrder},
    bloom_filter::Sbbf,
    errors::ParquetError,
    file::{metadata::ParquetMetaData, properties::WriterProperties},
};
use serde::{Serialize, de::DeserializeOwned};

use crate::{Record, TablesError, rows};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Default)]
pub struct Scan {
    pub row_groups: Option<Vec<usize>> = None,
    pub selection: Option<RowSelection> = None,
    pub filter: Option<RowFilter> = None,
}

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

pub async fn write<T, W, S, E>(async_writer: W, rows: S, settings: &WriterProperties) -> Result<ParquetMetaData, E>
where
    T: Record + Serialize,
    W: AsyncFileWriter,
    S: TryStream<Ok: Borrow<[T]>, Error = E>,
    E: From<TablesError>,
{
    let schema = T::schema();
    rows.try_fold(AsyncArrowWriter::try_new(async_writer, Arc::clone(&schema), Some(properties(&schema, settings)?)).map_err(TablesError::from)?, async |mut writer, chunk| {
        let written = try bikeshed Result<_, TablesError> { writer.write(&serde_arrow::to_record_batch(schema.fields(), &<S::Ok as Borrow<[T]>>::borrow(&chunk))?).await? };
        written.map(|()| writer).map_err(E::from)
    })
    .await?
    .close()
    .await
    .map_err(|source| E::from(TablesError::from(source)))
}

// --- [READ]

pub fn read<T, R>(builder: ParquetRecordBatchStreamBuilder<R>) -> Result<impl Stream<Item = Result<Vec<T>, TablesError>>, TablesError>
where
    T: Record + DeserializeOwned,
    R: AsyncFileReader + Unpin + 'static,
{
    let schema = T::schema();
    rows::<T>(&schema, &RecordBatch::new_empty(Arc::clone(builder.schema())))?;
    let mask = ProjectionMask::roots(builder.parquet_schema(), schema.fields().iter().filter_map(|field| builder.schema().column_with_name(field.name()).map(|(position, _)| position)));
    Ok(builder.with_projection(mask).build()?.map(move |batch| rows(&schema, &batch?)))
}

pub async fn lookup<T, R, K, I>(builder: ParquetRecordBatchStreamBuilder<R>, scan: Scan, column: &str, keys: I) -> Result<impl Stream<Item = Result<Vec<T>, TablesError>>, TablesError>
where
    T: Record + DeserializeOwned,
    R: AsyncFileReader + Unpin + 'static,
    K: TypedUuidKind,
    I: IntoIterator<Item: Borrow<TypedUuid<K>>>,
{
    let schema = T::schema();
    let field = schema.fields().find(column).map(|(_, field)| field).filter(|field| field.try_extension_type::<Uuid>().is_ok()).ok_or_else(|| TablesError::KeyColumn { column: column.to_owned() })?;
    let Some((leaf, _)) = parquet_column(builder.parquet_schema(), builder.schema(), column) else {
        return read(builder.with_row_groups(Vec::new()));
    };
    let metadata = Arc::clone(builder.metadata());
    let groups = scan.row_groups.unwrap_or_else(|| (0..metadata.num_row_groups()).collect());
    let row_groups = groups.iter().map(|group| metadata.row_groups().get(*group).ok_or(ParquetError::IndexOutOfBound(*group, metadata.num_row_groups()))).collect::<Result<Vec<_>, _>>()?;
    let keys = keys.into_iter().map(|key| *key.borrow().as_bytes()).collect::<BTreeSet<_>>();
    let statistics = StatisticsConverter::from_column_index(leaf, field, metadata.file_metadata().schema_descr())?;
    let ordered = metadata.file_metadata().column_order(leaf).sort_order() == SortOrder::UNSIGNED;
    let held = |min: Option<&[u8]>, max: Option<&[u8]>, bloom: Option<&Sbbf>| {
        let [min, max] = [min, max].map(|bound| bound.filter(|_| ordered));
        keys.range::<[u8], _>((min.map_or(Bound::Unbounded, Bound::Included), Bound::Unbounded))
            .take_while(|key| max.is_none_or(|max| key.as_slice() <= max))
            .any(|key| bloom.is_none_or(|bloom| bloom.check(key.as_slice())))
    };
    let [mins, maxes] = [StatisticsConverter::row_group_mins, StatisticsConverter::row_group_maxes].try_map(|bounds| bounds(&statistics, row_groups.iter().copied()))?;
    let pages = metadata.column_index().zip(metadata.offset_index());
    let (builder, selectors) = stream::iter(iter::zip(iter::zip(&groups, &row_groups), iter::zip(mins.as_fixed_size_binary(), maxes.as_fixed_size_binary())))
        .map(Ok::<_, TablesError>)
        .try_fold((builder, Vec::new()), async |(mut reader, mut selectors), ((group, row_group), (min, max))| {
            let rows = usize::try_from(row_group.num_rows()).map_err(ParquetError::from)?;
            let bloom = OptionFuture::from(held(min, max, None).then(|| reader.get_row_group_column_bloom_filter(*group, leaf))).await.transpose()?;
            match (bloom.filter(|bloom| held(min, max, bloom.as_ref())), pages) {
                (Some(bloom), Some((column_index, offset_index))) if let Some(counts) = statistics.data_page_row_counts(offset_index, metadata.row_groups(), iter::once(group))? => {
                    let [page_mins, page_maxes] =
                        [StatisticsConverter::data_page_mins, StatisticsConverter::data_page_maxes].try_map(|bounds| bounds(&statistics, column_index, offset_index, iter::once(group)))?;
                    iter::zip(iter::zip(page_mins.as_fixed_size_binary(), page_maxes.as_fixed_size_binary()), counts.values())
                        .map(|((page_min, page_max), count)| usize::try_from(*count).map(|row_count| RowSelector { row_count, skip: !held(page_min, page_max, bloom.as_ref()) }))
                        .process_results(|spans| selectors.extend(spans))
                        .map_err(ParquetError::from)?;
                }
                (bloom, _) => selectors.push(RowSelector { row_count: rows, skip: bloom.is_none() }),
            }
            Ok((reader, selectors))
        })
        .await?;
    let selection = RowSelection::from(selectors);
    let keys = keys.into_iter().collect::<HashSet<_>>();
    let target = field.data_type().clone();
    let predicate = ArrowPredicateFn::new(ProjectionMask::leaves(metadata.file_metadata().schema_descr(), [leaf]), move |batch: RecordBatch| {
        Ok(cast(batch.column(0), &target)?.as_fixed_size_binary().iter().map(|key| key.map(|key| keys.contains(key))).collect::<BooleanArray>())
    });
    read(
        builder
            .with_row_groups(groups)
            .with_row_selection(scan.selection.map(|caller| caller.intersection(&selection)).unwrap_or(selection))
            .with_row_filter(RowFilter::new(iter::once::<Box<dyn ArrowPredicate>>(Box::new(predicate)).chain(scan.filter.into_iter().flat_map(RowFilter::into_predicates)).collect())),
    )
}
