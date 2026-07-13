# [PY_DATA_API_STAC_GEOPARQUET]

`stac-geoparquet` supplies the STAC<->GeoParquet/Arrow interchange for the data STAC-catalog rail: an Arrow-centric surface (`stac_geoparquet.arrow.*`) that parses `pystac.Item` objects or STAC NDJSON into a `pyarrow.RecordBatchReader`, writes that stream to GeoParquet keyed by a STAC GeoParquet schema version, rehydrates a STAC table back to item dicts or NDJSON, sinks to Delta Lake (optional `deltalake` extra), reads pgstac database rows (optional `psycopg` extra), plus the legacy top-level geopandas trio (`to_geodataframe`/`to_item_collection`/`to_dict`). The package owner composes the `arrow` parse/write functions and `stac_table_to_items` into the STAC item-table owner, pairs rehydration with `pystac.Item.from_dict`, and never re-implements the STAC->Arrow schema mapping stac-geoparquet already owns.

## [01]-[PACKAGE_SURFACE]

- package: `stac-geoparquet`
- version: `0.8.0`
- license: MIT (permissive)
- module: `import stac_geoparquet` (dist name `stac-geoparquet`, import name `stac_geoparquet`; the Arrow surface lives under `stac_geoparquet.arrow.*`)
- owner: `data`
- rail: STAC catalog
- entry points: console script `stac-geoparquet` (CLI); library use is import-only
- capability: STAC item <-> Arrow/GeoParquet interchange — parse `pystac.Item`/STAC-NDJSON into a `pyarrow.RecordBatchReader`, write GeoParquet with a STAC GeoParquet schema version (`'1.0.0'`/`'1.1.0'`), rehydrate a STAC table to item dicts and NDJSON, an optional Delta Lake sink, a pgstac row reader, a two-pass `InferredSchema` builder, NDJSON readers (`json_reader.read_json`/`read_json_chunked`), and a legacy geopandas `GeoDataFrame` round-trip

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: interchange carriers and schema axis
- rail: STAC catalog

The interchange carrier is `pyarrow.RecordBatchReader` (zero-copy streaming). The schema axis is `ACCEPTED_SCHEMA_OPTIONS` — a `pa.Schema`, an `InferredSchema` instance, or a `Literal['FirstBatch', 'FullFile', 'ChunksToDisk']`; the GeoParquet output version is `SUPPORTED_PARQUET_SCHEMA_VERSIONS = Literal['1.0.0', '1.1.0']` (default `'1.1.0'`). The arrow internals are underscore-private; the public entries are the `stac_geoparquet.arrow`, `stac_geoparquet.json_reader`, and `stac_geoparquet.pgstac_reader` namespaces. The `arrow.` prefix drops from the `arrow.*` symbols in the grid.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]    | [CAPABILITY]                                                                  |
| :-----: | :---------------------------------- | :--------------- | :---------------------------------------------------------------------------- |
|  [01]   | `pa.RecordBatchReader`              | Arrow carrier    | the streamed STAC item table (from/to the parse functions)                    |
|  [02]   | `ACCEPTED_SCHEMA_OPTIONS`           | schema axis      | `pa.Schema`/`InferredSchema`/inference `Literal` union (values in the lead)   |
|  [03]   | `InferredSchema`                    | schema builder   | two-pass accumulator: `update_from_items`/`update_from_json`/`manual_updates` |
|  [04]   | `SUPPORTED_PARQUET_SCHEMA_VERSIONS` | version constant | `Literal['1.0.0','1.1.0']` set                                                |
|  [05]   | `DEFAULT_PARQUET_SCHEMA_VERSION`    | version constant | default `'1.1.0'` GeoParquet schema version                                   |
|  [06]   | `DEFAULT_JSON_CHUNK_SIZE`           | chunk constant   | default NDJSON/item batch size (`65536`)                                      |
|  [07]   | `pgstac_reader.PgstacRowFactory`    | pgstac reader    | build STAC items from pgstac database rows (requires the `psycopg` extra)     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Arrow parse and GeoParquet write (`stac_geoparquet.arrow.*`)
- rail: STAC catalog

The Arrow functions are reachable only as `stac_geoparquet.arrow.X` (none are top-level). `parse_stac_items_to_arrow` takes the schema as the `ACCEPTED_SCHEMA_OPTIONS` union (default `'FullFile'`); `parse_stac_ndjson_to_arrow` takes an explicit `pa.Schema` (or `None`).
- call: `arrow.parse_stac_items_to_arrow(items, chunk_size=65536, schema='FullFile', tmpdir=None) -> RecordBatchReader`
- call: `arrow.parse_stac_ndjson_to_arrow(path, *, chunk_size=65536, schema=None, limit=None) -> RecordBatchReader`
- call: `arrow.to_parquet(table, output_path, *, schema_version='1.1.0', collections=None, collection_metadata=None, filesystem=None, **kwargs) -> None`
- call: `arrow.parse_stac_items_to_parquet(items, *, chunk_size=65536, schema='FirstBatch', output_path, tmpdir=None, schema_version='1.1.0', filesystem=None, **kwargs) -> str`
- call: `arrow.parse_stac_ndjson_to_parquet(input_path, output_path, *, chunk_size=65536, schema=None, limit=None, schema_version='1.1.0', collections=None, collection_metadata=None, filesystem=None, **kwargs) -> None`
- call: `arrow.parse_stac_ndjson_to_delta_lake(input_path, table_or_uri, *, chunk_size=65536, schema=None, limit=None, schema_version='1.1.0', **kwargs) -> None`

| [INDEX] | [SURFACE]                               | [CAPABILITY]                             |
| :-----: | :-------------------------------------- | :--------------------------------------- |
|  [01]   | `arrow.parse_stac_items_to_arrow`       | `pystac.Item`/dict iterable -> Arrow     |
|  [02]   | `arrow.parse_stac_ndjson_to_arrow`      | STAC NDJSON file(s) -> Arrow             |
|  [03]   | `arrow.to_parquet`                      | Arrow table/reader -> GeoParquet         |
|  [04]   | `arrow.parse_stac_items_to_parquet`     | items -> GeoParquet in one call          |
|  [05]   | `arrow.parse_stac_ndjson_to_parquet`    | NDJSON -> GeoParquet                     |
|  [06]   | `arrow.parse_stac_ndjson_to_delta_lake` | NDJSON -> Delta Lake (`deltalake` extra) |

[ENTRYPOINT_SCOPE]: rehydrate, NDJSON readers, and legacy geopandas trio
- rail: STAC catalog

`stac_table_to_items` is a generator over plain item dicts (pair with `pystac.Item.from_dict` to rebuild the model). `json_reader.read_json`/`read_json_chunked` are the NDJSON-to-dict readers that feed the parse functions. The geopandas trio (`to_dict`/`to_geodataframe`/`to_item_collection`) is the top-level re-export and remains supported.
- call: `arrow.stac_table_to_items(table) -> Iterable[dict]`; `arrow.stac_table_to_ndjson(table, dest) -> None`
- call: `json_reader.read_json(path) -> Iterable[dict]`; `read_json_chunked(path, chunk_size, limit=None)`
- call: `to_geodataframe(items, add_self_link=False, dtype_backend=None, datetime_precision='ns') -> GeoDataFrame`
- call: `to_item_collection(df) -> pystac.ItemCollection`; `to_dict(record) -> dict`

| [INDEX] | [SURFACE]                    | [CAPABILITY]                       |
| :-----: | :--------------------------- | :--------------------------------- |
|  [01]   | `arrow.stac_table_to_items`  | Arrow STAC table -> item dicts     |
|  [02]   | `arrow.stac_table_to_ndjson` | Arrow STAC table -> NDJSON file    |
|  [03]   | `json_reader.read_json`      | STAC NDJSON file(s) -> dict stream |
|  [04]   | `to_geodataframe`            | items -> geopandas (legacy)        |
|  [05]   | `to_item_collection`         | `GeoDataFrame` -> STAC (legacy)    |
|  [06]   | `to_dict`                    | a parquet row -> a STAC item dict  |

## [04]-[IMPLEMENTATION_LAW]

[STAC_GEOPARQUET]:
- import: `import stac_geoparquet` at boundary scope only; module-level import is banned by the manifest import policy. The dist name is `stac-geoparquet`; the import name is `stac_geoparquet`; the canonical interchange surface is `stac_geoparquet.arrow.*`.
- carrier axis: the Arrow surface carries STAC items as a `pyarrow.RecordBatchReader` (zero-copy, streamed) — `parse_stac_items_to_arrow` (in-memory `pystac.Item`/dicts) and `parse_stac_ndjson_to_arrow` (NDJSON file) are source rows on the same carrier, never parallel table engines.
- write axis: `to_parquet`/`parse_stac_*_to_parquet` write GeoParquet keyed by a `schema_version` from `SUPPORTED_PARQUET_SCHEMA_VERSIONS` (`'1.0.0'`/`'1.1.0'`, default `'1.1.0'`); `parse_stac_ndjson_to_delta_lake` is the optional Delta sink (`deltalake` extra) — output sink is a function row, never a hand-rolled parquet writer.
- schema axis: schema inference is the `ACCEPTED_SCHEMA_OPTIONS` value — `'FullFile'` scans all batches, `'FirstBatch'` infers from the first, `'ChunksToDisk'` spills oversized inputs to `tmpdir`, an explicit `pa.Schema` pins the schema, and an `InferredSchema` accumulator (`update_from_items`/`update_from_json`) builds the schema across a first pass for a second-pass write. Never hand-build an Arrow schema where the inference options apply.
- rehydration axis: `stac_table_to_items` yields plain item dicts; rebuild the in-memory model with `pystac.Item.from_dict` — never re-mint a STAC item the `pystac` owner already models. `json_reader.read_json`/`read_json_chunked` are the NDJSON-to-dict readers feeding the parse functions.
- legacy axis: the top-level `to_geodataframe`/`to_item_collection`/`to_dict` trio is the geopandas round-trip retained for the `geopandas` seam; prefer the Arrow surface for the zero-copy item-table path, and use the geopandas trio only where a `GeoDataFrame` is required downstream.
- evidence: each interchange captures item count, schema version, batch/chunk size, and output byte/row count as a table receipt.

[SIBLING_STACK]:
- boundary: stac-geoparquet owns the STAC<->Arrow/GeoParquet schema mapping; `pystac` owns the in-memory item model on input and rehydration; `pystac-client` owns the live search feeding items in; `pyarrow`/`geopandas` own the carriers; the GeoParquet table folds into the `pyiceberg`/`deltalake`/cloud-egress owners; live UI stays outside this package.
- typical rail: `pystac-client` search -> `pystac.Item` iterable -> `arrow.parse_stac_items_to_arrow` (or `arrow.parse_stac_ndjson_to_arrow` from NDJSON) -> `RecordBatchReader` -> `arrow.to_parquet(schema_version='1.1.0')` (or `parse_stac_ndjson_to_delta_lake`) -> the GeoParquet/Delta lakehouse; rehydrate with `arrow.stac_table_to_items` + `pystac.Item.from_dict`.
- `odc-stac`/`rioxarray` (siblings) consume the rehydrated items for xarray cube assembly; `shapely`/`geoarrow-rust-compute` consume the GeoArrow geometry column the GeoParquet write produces.

[RAIL_LAW]:
- Package: `stac-geoparquet`
- Owns: STAC item <-> Arrow/GeoParquet interchange — `pystac.Item`/NDJSON to `RecordBatchReader`, GeoParquet write with STAC schema versioning, table-to-item-dict and NDJSON rehydration, an optional Delta Lake sink, a pgstac reader, the `InferredSchema` two-pass builder, NDJSON readers, and the legacy geopandas round-trip
- Accept: the `stac_geoparquet.arrow.*` parse/write functions over a `RecordBatchReader`; `ACCEPTED_SCHEMA_OPTIONS` schema inference (`'FullFile'`/`'FirstBatch'`/`'ChunksToDisk'`/`pa.Schema`/`InferredSchema`); `schema_version`-keyed GeoParquet output; `stac_table_to_items` + `pystac.Item.from_dict` rehydration; `json_reader.read_json` for NDJSON ingest; the geopandas trio only for a required `GeoDataFrame` seam
- Reject: wrapper-renames of the arrow parse/write functions; a hand-rolled STAC->Arrow schema mapping; a hand-built parquet writer where `to_parquet` versions the schema; re-minting a STAC item where `pystac.Item.from_dict` rehydrates; defaulting to the geopandas trio where the zero-copy Arrow item-table path applies; assuming the Delta or pgstac readers are available without their optional extra
