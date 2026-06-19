# [PY_DATA_API_STAC_GEOPARQUET]

`stac-geoparquet` (dist `stac-geoparquet`, import `stac_geoparquet`) supplies the STAC<->GeoParquet/Arrow interchange for the data STAC-catalog rail: an Arrow-centric surface (`stac_geoparquet.arrow.*`) that parses `pystac.Item` objects or STAC NDJSON into a `pyarrow.RecordBatchReader`, writes that table to GeoParquet (with STAC schema versioning), and rehydrates a STAC table back to item dicts, plus the legacy top-level geopandas trio (`to_geodataframe`/`to_item_collection`/`to_dict`). The package owner composes the `arrow` parse/write functions and `stac_table_to_items` into the STAC item-table owner; it never re-implements the STAC->Arrow schema mapping stac-geoparquet already owns.

## [01]-[PACKAGE_SURFACE]

- package: `stac-geoparquet`
- import: `import stac_geoparquet` (dist name `stac-geoparquet`, import name `stac_geoparquet`); the Arrow surface lives under `stac_geoparquet.arrow.*`
- owner: `data`
- rail: STAC catalog
- asset: pure-Python runtime library (`py3-none-any` wheel)
- installed: present in the lockfile but not yet synced into the active venv — RESEARCH-capture-pending-on-uv-sync; the member surface below is authored from the canonical source (`stac-utils/stac-geoparquet` `0.8.0`) and official documentation, and reflection-verifies on uv sync (pure-Python, imports on the cp315 core)
- entry points: console script `stac-geoparquet` (CLI); library use is import-only
- capability: STAC item <-> Arrow/GeoParquet interchange — parse `pystac.Item`/STAC-NDJSON into a `pyarrow.RecordBatchReader`, write GeoParquet with the STAC GeoParquet schema versions, rehydrate a STAC table to item dicts and NDJSON, optional Delta Lake sink, a pgstac row reader, and a legacy geopandas `GeoDataFrame` round-trip

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: interchange carriers and schema constants
- rail: STAC catalog

The interchange carrier is `pyarrow.RecordBatchReader` (zero-copy streaming); the schema axis is the string-literal `ACCEPTED_SCHEMA_OPTIONS` (`"FullFile"`/`"FirstBatch"`/an explicit `pa.Schema`) and the `*_PARQUET_SCHEMA_VERSION` GeoParquet version constants. The arrow internals are underscore-private; the public entry is the `stac_geoparquet.arrow` namespace.

| [INDEX] | [SYMBOL]                                                                     | [TYPE_FAMILY]    | [CAPABILITY]                                               |
| :-----: | :--------------------------------------------------------------------------- | :--------------- | :--------------------------------------------------------- |
|  [01]   | `pa.RecordBatchReader`                                                       | Arrow carrier    | the streamed STAC item table (from/to the parse functions) |
|  [02]   | `arrow.ACCEPTED_SCHEMA_OPTIONS`                                              | schema axis      | `"FullFile"`/`"FirstBatch"`/explicit `pa.Schema` inference |
|  [03]   | `arrow.DEFAULT_PARQUET_SCHEMA_VERSION` / `SUPPORTED_PARQUET_SCHEMA_VERSIONS` | version constant | the STAC GeoParquet schema version set                     |
|  [04]   | `arrow.DEFAULT_JSON_CHUNK_SIZE`                                              | chunk constant   | default NDJSON/item batch size                             |
|  [05]   | `pgstac_reader.PgstacRowFactory`                                             | pgstac reader    | build STAC items from pgstac database rows                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Arrow parse and GeoParquet write (`stac_geoparquet.arrow.*`)
- rail: STAC catalog

The Arrow functions are reachable only as `stac_geoparquet.arrow.X` (none are top-level). `parse_stac_items_to_arrow` takes the schema as a string literal; `parse_stac_ndjson_to_arrow` takes an explicit `pa.Schema`.

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]                                                                                                                                                                                   | [CAPABILITY]                         |
| :-----: | :-------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `arrow.parse_stac_items_to_arrow`       | `parse_stac_items_to_arrow(items, chunk_size=DEFAULT_JSON_CHUNK_SIZE, schema='FullFile', tmpdir=None)` -> `RecordBatchReader`                                                                  | `pystac.Item`/dict iterable -> Arrow |
|  [02]   | `arrow.parse_stac_ndjson_to_arrow`      | `parse_stac_ndjson_to_arrow(path, *, chunk_size=..., schema=None, limit=None)` -> `RecordBatchReader`                                                                                          | STAC NDJSON file -> Arrow            |
|  [03]   | `arrow.to_parquet`                      | `to_parquet(table, output_path, *, schema_version=DEFAULT_PARQUET_SCHEMA_VERSION, collections=None, collection_metadata=None, filesystem=None, **kwargs)`                                      | Arrow table -> GeoParquet            |
|  [04]   | `arrow.parse_stac_items_to_parquet`     | `parse_stac_items_to_parquet(items, *, chunk_size=..., schema='FirstBatch', output_path, tmpdir=None, schema_version=..., filesystem=None, **kwargs)` -> `str`                                 | items -> GeoParquet in one call      |
|  [05]   | `arrow.parse_stac_ndjson_to_parquet`    | `parse_stac_ndjson_to_parquet(input_path, output_path, *, chunk_size=..., schema=None, limit=None, schema_version=..., collections=None, collection_metadata=None, filesystem=None, **kwargs)` | NDJSON -> GeoParquet                 |
|  [06]   | `arrow.parse_stac_ndjson_to_delta_lake` | `parse_stac_ndjson_to_delta_lake(input_path, table_or_uri, ...)`                                                                                                                               | NDJSON -> Delta Lake (optional dep)  |

[ENTRYPOINT_SCOPE]: rehydrate and legacy geopandas trio
- rail: STAC catalog

`stac_table_to_items` is a generator over plain item dicts (pair with `pystac.Item.from_dict` to rebuild the model). The geopandas trio (`to_dict`/`to_geodataframe`/`to_item_collection`) is the top-level re-export and remains supported.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                                                                 | [CAPABILITY]                      |
| :-----: | :--------------------------- | :----------------------------------------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `arrow.stac_table_to_items`  | `stac_table_to_items(table)` -> `Iterable[dict]`                                                             | Arrow STAC table -> item dicts    |
|  [02]   | `arrow.stac_table_to_ndjson` | `stac_table_to_ndjson(table, dest)`                                                                          | Arrow STAC table -> NDJSON file   |
|  [03]   | `to_geodataframe`            | `to_geodataframe(items, add_self_link=False, dtype_backend=None, datetime_precision='ns')` -> `GeoDataFrame` | items -> geopandas (legacy)       |
|  [04]   | `to_item_collection`         | `to_item_collection(df)` -> `pystac.ItemCollection`                                                          | `GeoDataFrame` -> STAC (legacy)   |
|  [05]   | `to_dict`                    | `to_dict(record)` -> `dict`                                                                                  | a parquet row -> a STAC item dict |

## [04]-[IMPLEMENTATION_LAW]

[STAC_GEOPARQUET]:
- import: `import stac_geoparquet` at boundary scope only; module-level import is banned by the manifest import policy. The dist name is `stac-geoparquet`; the import name is `stac_geoparquet`; the canonical interchange surface is `stac_geoparquet.arrow.*`.
- carrier axis: the Arrow surface carries STAC items as a `pyarrow.RecordBatchReader` (zero-copy, streamed) — `parse_stac_items_to_arrow` (in-memory `pystac.Item`/dicts) and `parse_stac_ndjson_to_arrow` (NDJSON file) are source rows on the same carrier, never parallel table engines.
- write axis: `to_parquet`/`parse_stac_*_to_parquet` write GeoParquet keyed by a `schema_version` from `SUPPORTED_PARQUET_SCHEMA_VERSIONS`; `parse_stac_ndjson_to_delta_lake` is the optional Delta sink — output sink is a function row, never a hand-rolled parquet writer.
- schema axis: schema inference is the `ACCEPTED_SCHEMA_OPTIONS` string literal (`"FullFile"` scans all batches, `"FirstBatch"` infers from the first) or an explicit `pa.Schema`; never a hand-built Arrow schema where the inference options apply.
- rehydration axis: `stac_table_to_items` yields plain item dicts; rebuild the in-memory model with `pystac.Item.from_dict` — never re-mint a STAC item the `pystac` owner already models.
- legacy axis: the top-level `to_geodataframe`/`to_item_collection`/`to_dict` trio is the geopandas round-trip retained for the `geopandas` seam; prefer the Arrow surface for the zero-copy item-table path, and use the geopandas trio only where a `GeoDataFrame` is required downstream.
- evidence: each interchange captures item count, schema version, batch/chunk size, and output byte/row count as a table receipt.
- boundary: stac-geoparquet owns the STAC<->Arrow/GeoParquet schema mapping; `pystac` owns the in-memory item model on input and rehydration; `pystac-client` owns the live search feeding items in; `pyarrow`/`geopandas` own the carriers; the GeoParquet table folds into the tensor virtual cube and cloud-egress owners; live UI stays outside this package.

[RAIL_LAW]:
- Package: `stac-geoparquet`
- Owns: STAC item <-> Arrow/GeoParquet interchange — `pystac.Item`/NDJSON to `RecordBatchReader`, GeoParquet write with STAC schema versioning, table-to-item-dict rehydration, an optional Delta Lake sink, a pgstac reader, and the legacy geopandas round-trip
- Accept: the `stac_geoparquet.arrow.*` parse/write functions over a `RecordBatchReader`, `ACCEPTED_SCHEMA_OPTIONS` schema inference, `schema_version`-keyed GeoParquet output, `stac_table_to_items` + `pystac.Item.from_dict` rehydration, the geopandas trio only for a required `GeoDataFrame` seam
- Reject: wrapper-renames of the arrow parse/write functions; a hand-rolled STAC->Arrow schema mapping; a hand-built parquet writer where `to_parquet` versions the schema; re-minting a STAC item where `pystac.Item.from_dict` rehydrates; defaulting to the geopandas trio where the zero-copy Arrow item-table path applies
