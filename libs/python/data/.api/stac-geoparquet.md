# [PY_DATA_API_STAC_GEOPARQUET]

`stac-geoparquet` owns the STAC<->GeoParquet/Arrow interchange on the STAC-catalog domain: its `stac_geoparquet.arrow.*` surface parses `pystac.Item` objects or STAC NDJSON into a `pyarrow.RecordBatchReader`, writes that stream to GeoParquet keyed by a STAC schema version, and rehydrates the table back to item dicts or NDJSON. Composition folds the arrow parse/write functions and `stac_table_to_items` into the `data` STAC item-table owner and pairs rehydration with `pystac.Item.from_dict`, never re-implementing the STAC->Arrow schema mapping this package owns.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: interchange carriers and schema axis
- concern: STAC catalog

`ACCEPTED_SCHEMA_OPTIONS` is a `pa.Schema`, an `InferredSchema`, or a `Literal['FirstBatch', 'FullFile', 'ChunksToDisk']`; `SUPPORTED_PARQUET_SCHEMA_VERSIONS` is `Literal['1.0.0', '1.1.0']` (default `'1.1.0'`). Grid symbols drop the `arrow.` prefix.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]    | [CAPABILITY]                                                                  |
| :-----: | :---------------------------------- | :--------------- | :---------------------------------------------------------------------------- |
|  [01]   | `pa.RecordBatchReader`              | Arrow carrier    | the streamed STAC item table (from/to the parse functions)                    |
|  [02]   | `ACCEPTED_SCHEMA_OPTIONS`           | schema axis      | `pa.Schema`/`InferredSchema`/inference `Literal` union (values in the lead)   |
|  [03]   | `InferredSchema`                    | schema builder   | two-pass accumulator: `update_from_items`/`update_from_json`/`manual_updates` |
|  [04]   | `SUPPORTED_PARQUET_SCHEMA_VERSIONS` | version constant | `Literal['1.0.0','1.1.0']` set                                                |
|  [05]   | `DEFAULT_PARQUET_SCHEMA_VERSION`    | version constant | default `'1.1.0'` GeoParquet schema version                                   |
|  [06]   | `DEFAULT_JSON_CHUNK_SIZE`           | chunk constant   | default NDJSON/item batch size (`65536`)                                      |
|  [07]   | `pgstac_reader.PgstacRowFactory`    | pgstac reader    | build STAC items from pgstac database rows (`psycopg` extra)                  |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Arrow parse and GeoParquet write (`stac_geoparquet.arrow.*`)
- concern: STAC catalog
- write carry: `chunk_size`, `schema` (`ACCEPTED_SCHEMA_OPTIONS`), `schema_version` (`SUPPORTED_PARQUET_SCHEMA_VERSIONS`), `collections`, `collection_metadata`, `filesystem`, `**kwargs`

`parse_stac_items_to_arrow` takes `schema` over the full `ACCEPTED_SCHEMA_OPTIONS` union (default `'FullFile'`); `parse_stac_ndjson_to_arrow` takes an explicit `pa.Schema` or `None`.

| [INDEX] | [SURFACE]                                                             | [SHAPE] | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `parse_stac_items_to_arrow(items, *, tmpdir) -> RecordBatchReader`    | static  | `pystac.Item`/dict iterable -> Arrow |
|  [02]   | `parse_stac_ndjson_to_arrow(path, *, limit) -> RecordBatchReader`     | static  | STAC NDJSON file(s) -> Arrow         |
|  [03]   | `to_parquet(table, output_path)`                                      | static  | Arrow table/reader -> GeoParquet     |
|  [04]   | `parse_stac_items_to_parquet(items, *, output_path, tmpdir) -> str`   | static  | items -> GeoParquet in one call      |
|  [05]   | `parse_stac_ndjson_to_parquet(input_path, output_path)`               | static  | NDJSON -> GeoParquet                 |
|  [06]   | `parse_stac_ndjson_to_delta_lake(input_path, table_or_uri, *, limit)` | static  | NDJSON -> Delta (`deltalake` extra)  |

[ENTRYPOINT_SCOPE]: rehydrate, NDJSON readers, and geopandas trio
- concern: STAC catalog

`stac_table_to_items` generates plain item dicts (pair with `pystac.Item.from_dict`). `json_reader.read_json`/`read_json_chunked` are the NDJSON-to-dict readers feeding the parse functions. Top-level `to_dict`/`to_geodataframe`/`to_item_collection` form the geopandas `GeoDataFrame` round-trip.

| [INDEX] | [SURFACE]                                                   | [SHAPE] | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `arrow.stac_table_to_items(table) -> Iterable[dict]`        | static  | Arrow STAC table -> item dicts     |
|  [02]   | `arrow.stac_table_to_ndjson(table, dest)`                   | static  | Arrow STAC table -> NDJSON file    |
|  [03]   | `json_reader.read_json(path) -> Iterable[dict]`             | static  | STAC NDJSON file(s) -> dict stream |
|  [04]   | `json_reader.read_json_chunked(path, chunk_size, *, limit)` | static  | chunked NDJSON -> dict batches     |
|  [05]   | `to_geodataframe(items) -> GeoDataFrame`                    | static  | items -> geopandas                 |
|  [06]   | `to_item_collection(df) -> ItemCollection`                  | static  | `GeoDataFrame` -> STAC             |
|  [07]   | `to_dict(record) -> dict`                                   | static  | a parquet row -> a STAC item dict  |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- surface: the dist is `stac-geoparquet`, the import is `stac_geoparquet`, and the canonical interchange surface is `stac_geoparquet.arrow.*` — the `arrow.*` functions are the one interchange path, never parallel table engines.
- carrier axis: `parse_stac_items_to_arrow` (in-memory `pystac.Item`/dicts) and `parse_stac_ndjson_to_arrow` (NDJSON file) are source rows on one `pyarrow.RecordBatchReader` carrier (zero-copy, streamed).
- write axis: `to_parquet`/`parse_stac_*_to_parquet` write GeoParquet keyed by a `schema_version` from `SUPPORTED_PARQUET_SCHEMA_VERSIONS`; `parse_stac_ndjson_to_delta_lake` is the Delta sink (`deltalake` extra) — output sink is a function row, never a hand-rolled parquet writer.
- schema axis: schema resolves through `ACCEPTED_SCHEMA_OPTIONS` — `'FullFile'` scans all batches, `'FirstBatch'` infers from the first, `'ChunksToDisk'` spills oversized inputs to `tmpdir`, an explicit `pa.Schema` pins it, and an `InferredSchema` accumulator (`update_from_items`/`update_from_json`) builds a first-pass schema for a second-pass write — never a hand-built Arrow schema where the inference options apply.
- rehydration axis: `stac_table_to_items` yields plain item dicts rebuilt through `pystac.Item.from_dict`; `json_reader.read_json`/`read_json_chunked` feed the parse functions — never re-mint a STAC item `pystac` already models.
- geopandas axis: the top-level `to_geodataframe`/`to_item_collection`/`to_dict` trio is the `GeoDataFrame` round-trip; the zero-copy Arrow item-table path is default, the trio the boundary where a `GeoDataFrame` is required downstream.
- evidence: each interchange result retains item count, schema version, batch/chunk size, and output byte/row count.

[STACKING]:
- `pystac`(`.api/pystac.md`): `pystac.Item` iterables feed `parse_stac_items_to_arrow`, and `pystac.Item.from_dict` rehydrates the `stac_table_to_items` dicts — the `Item.to_dict()` GeoJSON is the bridge shape.
- `pystac-client`(`.api/pystac-client.md`): `Client.search(...).items()` is the live-search source feeding the parse functions.
- `pyarrow`(`.api/pyarrow.md`): the `RecordBatchReader` carrier is a `pyarrow` stream; `to_parquet` emits the GeoArrow geometry column.
- `deltalake`(`.api/deltalake.md`): `parse_stac_ndjson_to_delta_lake` sinks the stream to a Delta table.
- `pyiceberg`(`.api/pyiceberg.md`): the written GeoParquet folds into the Iceberg/lakehouse table owner.
- `geopandas`(`.api/geopandas.md`): `to_geodataframe`/`to_item_collection` bridge to a `GeoDataFrame` where a vector-dataframe consumer requires one.
- `odc-stac`(`.api/odc-stac.md`), `rioxarray`(`.api/rioxarray.md`): consume the rehydrated items for xarray-cube assembly.
- `shapely`(`.api/shapely.md`), `geoarrow-rust-compute`(`.api/geoarrow-rust-compute.md`): consume the GeoArrow geometry column the GeoParquet write produces.
- within-lib: the `data` STAC owner composes the arrow parse/write functions and `stac_table_to_items` as the sole STAC<->GeoParquet interchange, threading `ACCEPTED_SCHEMA_OPTIONS` and `schema_version` through one item-table carrier.

[LOCAL_ADMISSION]:
- admit `stac_geoparquet.arrow.*` as the sole STAC<->GeoParquet interchange; the `deltalake` and `psycopg` extras admit only where a Delta sink or a pgstac read is composed, never as a default dependency.
