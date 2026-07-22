# [PY_DATA_API_ARRO3_IO]

`arro3.io` supplies the pyarrow-free codec rail for the carrier leg: native Rust readers parse Arrow IPC (file and stream framing), Parquet, CSV, JSON, and newline-delimited JSON into a lazy `arro3.core.RecordBatchReader`, and matching writers lower any PyCapsule producer back to each format. Readers intake a path, `pathlib.Path`, or binary buffer; writers accept `data` through the `arro3.core.types` `ArrowStreamExportable`/`ArrowArrayExportable` protocols, so a `pyarrow`, `nanoarrow`, `polars`, ADBC, or `arro3` frame flows in by capsule with no producer-named branch.

`read_parquet_async` fronts an `ObjectStore` (S3/GCS/Azure/HTTP/local/memory) to pull a cloud object into a materialized `arro3.core.Table` without a local copy, and a typed `exceptions` hierarchy surfaces store and codec faults. Every function stays on the `arro3.core` memory model, closing the `pyarrow` C++ bridge a `pyarrow`-free carrier path must not drag in.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arro3-io`
- package: `arro3-io`
- owner: `data`
- module: `arro3.io`
- asset: native extension (Rust/PyO3) over `arrow-rs` `arrow-ipc`, `parquet`, `arrow-csv`, `arrow-json`
- dependency: `arro3-core` (sole runtime requirement; supplies the `RecordBatchReader`/`Table`/`Schema` memory model)
- license: `MIT OR Apache-2.0`
- rail: arrow-io

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Parquet encoding and compression vocabulary (`arro3.io._parquet`)
- rail: arrow-io

`write_parquet` discriminates codec and layout on three bounded aliases; a per-column `dict` keyed by `ParquetColumnPath` overrides the file-wide default.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]     | [ROLE]                                                                  |
| :-----: | :------------------- | :---------------- | :---------------------------------------------------------------------- |
|  [01]   | `ParquetCompression` | `Literal` \| str  | `uncompressed`/`snappy`/`gzip`/`brotli`/`lz4`/`zstd`; `"zstd(3)"` level |
|  [02]   | `ParquetEncoding`    | `Literal`         | encoding vocabulary for column pages                                    |
|  [03]   | `ParquetColumnPath`  | `str` \| Sequence | column key for the per-column override maps                             |

[PUBLIC_TYPE_SCOPE]: object-store carriers (`arro3.io.store`, `arro3.io._pyo3_object_store`)
- rail: arrow-io

`store` re-exports the `ObjectStore` variants; each constructs through `from_env`/`from_url`, `S3Store.from_session` builds off a boto3/botocore session, and `RetryConfig`/`BackoffConfig` are `TypedDict` request-policy carriers.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [ROLE]                                                     |
| :-----: | :------------------------------------ | :------------ | :--------------------------------------------------------- |
|  [01]   | `S3Store` / `GCSStore` / `AzureStore` | class         | cloud object stores; `from_env`/`from_url` construction    |
|  [02]   | `HTTPStore`                           | class         | HTTP object store; `from_url` only                         |
|  [03]   | `LocalStore` / `MemoryStore`          | class         | local-disk (`prefix`) and in-memory stores                 |
|  [04]   | `RetryConfig` / `BackoffConfig`       | `TypedDict`   | retry-count/timeout and exponential-backoff request policy |

[PUBLIC_TYPE_SCOPE]: typed failure rail (`arro3.io.exceptions`)
- rail: arrow-io

`BaseError` roots the exception hierarchy that surfaces store and codec faults as typed Python exceptions rather than an opaque `OSError`.

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY] | [ROLE]                           |
| :-----: | :---------------------------------------------------------- | :------------ | :------------------------------- |
|  [01]   | `BaseError`                                                 | exception     | hierarchy root; catch-all fault  |
|  [02]   | `NotFoundError` / `AlreadyExistsError` / `InvalidPathError` | exception     | object-lifecycle and path faults |
|  [03]   | `PermissionDeniedError` / `UnauthenticatedError`            | exception     | store access faults              |
|  [04]   | `PreconditionError` / `NotModifiedError`                    | exception     | conditional-request faults       |
|  [05]   | `NotSupportedError` / `JoinError` / `GenericError`          | exception     | capability and async-join faults |
|  [06]   | `UnknownConfigurationKeyError`                              | exception     | store config-key fault           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schema inference (`arro3.io._csv`, `_json`)
- rail: arrow-io

CSV and JSON carry no embedded schema, so production inference passes a positive `max_records` sampling bound and returns an `arro3.core.Schema` the matching `read_*` intakes; IPC and Parquet self-describe and skip inference. Only the explicit unbounded-scan profile passes `max_records=None`, and policy admits that profile for a trusted, size-bounded local input. `infer_csv_schema` also takes the `has_header`/`delimiter`/`escape`/`quote`/`terminator`/`comment` dialect keywords.

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [CAPABILITY]                                |
| :-----: | :----------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `infer_csv_schema(file, ...)`  | schema probe   | `Schema` inferred from CSV dialect and rows |
|  [02]   | `infer_json_schema(file, ...)` | schema probe   | `Schema` inferred from sampled JSON records |

[ENTRYPOINT_SCOPE]: streaming readers (`arro3.io._csv`, `_ipc`, `_json`, `_parquet`)
- rail: arrow-io

Every synchronous reader returns a lazy `arro3.core.RecordBatchReader` over a path or binary buffer; CSV and JSON require the caller-supplied `schema` and an optional `batch_size`, `read_csv` also taking the full dialect keyword set. `read_ipc` reads random-access file framing, `read_ipc_stream` reads the streaming framing.

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [CAPABILITY]                             |
| :-----: | :----------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `read_csv(file, schema, ...)`  | dialect reader | CSV → `RecordBatchReader`, known schema  |
|  [02]   | `read_json(file, schema, ...)` | reader         | JSON → `RecordBatchReader`, known schema |
|  [03]   | `read_ipc(file)`               | reader         | Arrow IPC file → `RecordBatchReader`     |
|  [04]   | `read_ipc_stream(file)`        | reader         | Arrow IPC stream → `RecordBatchReader`   |
|  [05]   | `read_parquet(file)`           | reader         | Parquet → `RecordBatchReader`            |

[ENTRYPOINT_SCOPE]: async object-store read (`arro3.io._parquet`)
- rail: arrow-io

`read_parquet_async` pulls a Parquet object at `path` from a `store` `ObjectStore` and awaits a materialized `arro3.core.Table`, the cloud-native ingress that never lands a local file.

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]     | [CAPABILITY]                          |
| :-----: | :----------------------------------------- | :----------------- | :------------------------------------ |
|  [01]   | `await read_parquet_async(path, *, store)` | async store reader | Parquet object → materialized `Table` |

[ENTRYPOINT_SCOPE]: writers (`arro3.io._csv`, `_ipc`, `_json`, `_parquet`)
- rail: arrow-io

Writers take `data` as any `ArrowStreamExportable`/`ArrowArrayExportable` producer and a path or buffer sink, returning `None`. `write_ipc`/`write_ipc_stream` select `LZ4`/`ZSTD` block `compression`; `write_json`/`write_ndjson` take `explicit_nulls`; `write_csv` takes the dialect and `date_format`/`datetime_format`/`time_format`/`timestamp_format`/`timestamp_tz_format`/`null` keywords.

`write_parquet` exposes the full `arrow-rs` writer-property surface: `compression`, `encoding`, `dictionary_enabled`, the per-column `column_compression`/`column_encoding`/`column_dictionary_enabled`/`column_max_statistics_size` maps, `bloom_filter_enabled`/`bloom_filter_fpp`/`bloom_filter_ndv`, `max_row_group_size`, `data_page_size_limit`/`data_page_row_count_limit`, `dictionary_page_size_limit`, `write_batch_size`, `max_statistics_size`, `key_value_metadata`, `created_by`, `skip_arrow_metadata`, and `writer_version` (`parquet_1_0`/`parquet_2_0`).

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :---------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `write_csv(data, file, ...)`        | dialect writer | CSV with dialect and temporal-format control      |
|  [02]   | `write_json(data, file, ...)`       | writer         | JSON array; `explicit_nulls` keeps null keys      |
|  [03]   | `write_ndjson(data, file, ...)`     | writer         | newline-delimited JSON                            |
|  [04]   | `write_ipc(data, file, ...)`        | writer         | Arrow IPC file; `LZ4`/`ZSTD` blocks               |
|  [05]   | `write_ipc_stream(data, file, ...)` | writer         | Arrow IPC stream                                  |
|  [06]   | `write_parquet(data, file, ...)`    | writer         | Parquet across the full `arrow-rs` writer surface |

## [04]-[IMPLEMENTATION_LAW]

[ARROW_IO_TOPOLOGY]:
- namespace: `arro3.io` re-exports free functions from the internal `_csv`/`_ipc`/`_json`/`_parquet` extension modules beside the `store` object-store submodule and the `exceptions` failure hierarchy
- read dispatch: every synchronous reader returns a lazy `RecordBatchReader`, so a read composes onto a downstream `arro3.compute` kernel and stays streaming until a terminal `read_all`; only `read_parquet_async` materializes an `arro3.core.Table`, awaiting a whole cloud object off an `ObjectStore`
- write intake: writers accept `ArrowStreamExportable | ArrowArrayExportable`, so a `Table`, `RecordBatchReader`, or `RecordBatch` from any producer lowers by capsule with no copy and no producer library named
- schema axis: CSV and JSON carry no schema and pair an `infer_*_schema` probe with a schema-taking `read_*`; IPC and Parquet self-describe and read schema-free
- inference policy: production probes pass a positive `max_records` bound; policy refuses `None` for remote or unbounded inputs, and only the explicit bounded-local profile admits a full scan

[LOCAL_ADMISSION]:
- Read and write the carrier leg through `arro3.io` where the payload is IPC/Parquet/CSV/JSON, never re-importing `pyarrow.parquet`/`pyarrow.csv`/`pyarrow.feather` to move bytes the `arro3.core` model already holds.
- Keep the read lazy: hold the returned `RecordBatchReader` and compose `arro3.compute` `filter`/`cast`/`date_part` onto it, deferring materialization to a terminal drain.
- Front cloud Parquet with an `arro3.io.store` `ObjectStore` and `read_parquet_async`, not a local download step; select the store variant by scheme and carry a `RetryConfig` for flaky transports.

[INTEGRATION_RAILS]:
- carrier-leg codec: a slim leg that only serializes and deserializes Arrow reaches for `arro3.io` and its `arro3-core` dependency alone, never dragging the `pyarrow` C++ bridge into a `pyarrow`-free carrier path.
- arro3-core ↔ arro3-io ↔ arro3-compute: a read yields the `RecordBatchReader` the `arro3.compute` kernels consume, and a kernel `Array`/`ArrayReader` lowers back through a writer, so a frame round-trips codec → compute → codec on one memory model with no producer named.
- typed store faults: catch `arro3.io.exceptions.NotFoundError`/`PermissionDeniedError`/`PreconditionError` at the async store boundary and route them onto the domain error rail rather than an opaque `OSError`.

[RAIL_LAW]:
- Package: `arro3-io`
- Owns: pyarrow-free Arrow IPC/Parquet/CSV/JSON/NDJSON readers-writers, cloud object-store carriers, and the typed IO failure hierarchy over the `arro3.core` memory model
- Accept: a path/`Path`/binary buffer on read; any PyCapsule producer through `arro3.core.types` on write
- Reject: a `pyarrow.parquet`/`pyarrow.csv`/`pyarrow.feather` re-import on a carrier leg the `arro3` model already serves; the format parsers `arrow-rs` already owns
