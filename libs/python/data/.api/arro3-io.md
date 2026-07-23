# [PY_DATA_API_ARRO3_IO]

`arro3.io` owns the pyarrow-free codec rail for the carrier leg: native Rust readers parse Arrow IPC file and stream framing, Parquet, CSV, JSON, and NDJSON into a lazy `arro3.core.RecordBatchReader`, and writers lower any `arro3.core.types` PyCapsule producer back to each format with no producer-named branch. `read_parquet_async` streams a cloud Parquet object off an `ObjectStore` into a materialized `arro3.core.Table` with no local copy, and a typed `exceptions` hierarchy surfaces store and codec faults — every surface on the `arro3.core` model that keeps the `pyarrow` C++ bridge out.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arro3-io`
- package: `arro3-io` (MIT OR Apache-2.0)
- owner: `data`
- module: `arro3.io`
- asset: native extension (Rust/PyO3) over `arrow-rs` `arrow-ipc`, `parquet`, `arrow-csv`, `arrow-json`
- dependency: `arro3-core` (sole runtime requirement)
- rail: arrow-io

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Parquet encoding and compression vocabulary (`arro3.io._parquet`)

`write_parquet` discriminates codec and layout on three bounded aliases; a per-column `dict` keyed by `ParquetColumnPath` overrides the file-wide default.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]     | [CAPABILITY]                                                            |
| :-----: | :------------------- | :---------------- | :---------------------------------------------------------------------- |
|  [01]   | `ParquetCompression` | `Literal` \| str  | `uncompressed`/`snappy`/`gzip`/`brotli`/`lz4`/`zstd`; `"zstd(3)"` level |
|  [02]   | `ParquetEncoding`    | `Literal`         | encoding vocabulary for column pages                                    |
|  [03]   | `ParquetColumnPath`  | `str` \| Sequence | column key for the per-column override maps                             |

[PUBLIC_TYPE_SCOPE]: object-store carriers (`arro3.io.store`, `arro3.io._pyo3_object_store`)

`store` re-exports the `ObjectStore` variants; each constructs through `from_env`/`from_url`, `S3Store.from_session` builds off a boto3/botocore session, and `RetryConfig`/`BackoffConfig` are `TypedDict` request-policy carriers.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :------------------------------------ | :------------ | :--------------------------------------------------------- |
|  [01]   | `S3Store` / `GCSStore` / `AzureStore` | class         | cloud object stores; `from_env`/`from_url` construction    |
|  [02]   | `HTTPStore`                           | class         | HTTP object store; `from_url` only                         |
|  [03]   | `LocalStore` / `MemoryStore`          | class         | local-disk (`prefix`) and in-memory stores                 |
|  [04]   | `RetryConfig` / `BackoffConfig`       | `TypedDict`   | retry-count/timeout and exponential-backoff request policy |

[PUBLIC_TYPE_SCOPE]: typed failure rail (`arro3.io.exceptions`)

`BaseError` roots the hierarchy that surfaces every store and codec fault as a typed exception.

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------- | :------------ | :------------------------------- |
|  [01]   | `BaseError`                                                 | exception     | hierarchy root; catch-all fault  |
|  [02]   | `NotFoundError` / `AlreadyExistsError` / `InvalidPathError` | exception     | object-lifecycle and path faults |
|  [03]   | `PermissionDeniedError` / `UnauthenticatedError`            | exception     | store access faults              |
|  [04]   | `PreconditionError` / `NotModifiedError`                    | exception     | conditional-request faults       |
|  [05]   | `NotSupportedError` / `JoinError` / `GenericError`          | exception     | capability and async-join faults |
|  [06]   | `UnknownConfigurationKeyError`                              | exception     | store config-key fault           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schema inference (`arro3.io._csv`, `_json`)

CSV and JSON carry no embedded schema; an inference probe passes a positive `max_records` bound and returns the `arro3.core.Schema` the matching `read_*` intakes, while IPC and Parquet self-describe and skip inference. Policy admits `max_records=None` only for a trusted, size-bounded local input. `infer_csv_schema` also takes the `has_header`/`delimiter`/`escape`/`quote`/`terminator`/`comment` dialect keywords.

| [INDEX] | [SURFACE]                      | [CAPABILITY]                                |
| :-----: | :----------------------------- | :------------------------------------------ |
|  [01]   | `infer_csv_schema(file, ...)`  | `Schema` inferred from CSV dialect and rows |
|  [02]   | `infer_json_schema(file, ...)` | `Schema` inferred from sampled JSON records |

[ENTRYPOINT_SCOPE]: streaming readers (`arro3.io._csv`, `_ipc`, `_json`, `_parquet`)

Every synchronous reader returns a lazy `arro3.core.RecordBatchReader` over a path or binary buffer; CSV and JSON require the caller-supplied `schema` and an optional `batch_size`, `read_csv` also taking the full dialect keyword set. `read_ipc` reads random-access file framing, `read_ipc_stream` reads the streaming framing.

| [INDEX] | [SURFACE]                      | [CAPABILITY]                             |
| :-----: | :----------------------------- | :--------------------------------------- |
|  [01]   | `read_csv(file, schema, ...)`  | CSV → `RecordBatchReader`, known schema  |
|  [02]   | `read_json(file, schema, ...)` | JSON → `RecordBatchReader`, known schema |
|  [03]   | `read_ipc(file)`               | Arrow IPC file → `RecordBatchReader`     |
|  [04]   | `read_ipc_stream(file)`        | Arrow IPC stream → `RecordBatchReader`   |
|  [05]   | `read_parquet(file)`           | Parquet → `RecordBatchReader`            |

[ASYNC_STORE_READ]: `arro3.io._parquet`
- `await read_parquet_async(path, *, store)`: pulls a Parquet object at `path` from a `store` `ObjectStore` and awaits a materialized `arro3.core.Table`, the cloud-native ingress that lands no local file.

[ENTRYPOINT_SCOPE]: writers (`arro3.io._csv`, `_ipc`, `_json`, `_parquet`)

Writers take `data` as any `ArrowStreamExportable`/`ArrowArrayExportable` producer and a path or buffer sink, returning `None`; `write_ipc`/`write_ipc_stream` select `LZ4`/`ZSTD` block `compression`, `write_json`/`write_ndjson` take `explicit_nulls`, and `write_csv` takes the dialect and temporal-format keywords. `write_parquet` exposes the full `arrow-rs` writer-property surface: file-wide and per-`ParquetColumnPath` compression/encoding/dictionary control, bloom-filter tuning, row-group and page sizing, statistics and metadata, and the `parquet_1_0`/`parquet_2_0` `writer_version`.

| [INDEX] | [SURFACE]                           | [CAPABILITY]                                      |
| :-----: | :---------------------------------- | :------------------------------------------------ |
|  [01]   | `write_csv(data, file, ...)`        | CSV with dialect and temporal-format control      |
|  [02]   | `write_json(data, file, ...)`       | JSON array; `explicit_nulls` keeps null keys      |
|  [03]   | `write_ndjson(data, file, ...)`     | newline-delimited JSON                            |
|  [04]   | `write_ipc(data, file, ...)`        | Arrow IPC file; `LZ4`/`ZSTD` blocks               |
|  [05]   | `write_ipc_stream(data, file, ...)` | Arrow IPC stream                                  |
|  [06]   | `write_parquet(data, file, ...)`    | Parquet across the full `arrow-rs` writer surface |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `arro3.io` re-exports free functions from the `_csv`/`_ipc`/`_json`/`_parquet` extension modules beside the `store` object-store submodule and the `exceptions` failure hierarchy.
- Every synchronous reader returns a lazy `RecordBatchReader` that composes onto an `arro3.compute` kernel and stays streaming to a terminal drain; `read_parquet_async` alone materializes an `arro3.core.Table`, awaiting a whole cloud object off an `ObjectStore`.
- CSV and JSON pair an `infer_*_schema` probe with a schema-taking `read_*`; IPC and Parquet self-describe and read schema-free.

[STACKING]:
- `arro3-core`(`.api/arro3-core.md`): a reader's `RecordBatchReader` and a writer's `ArrowStreamExportable`/`ArrowArrayExportable` intake ride the `arro3.core.types` PyCapsule protocols, so any producer frame lowers with no copy and no producer named.
- `arro3-compute`(`.api/arro3-compute.md`): a read yields the `RecordBatchReader` the compute kernels consume and a kernel `Array`/`ArrayReader` lowers back through a writer, so a frame round-trips codec → compute → codec on one memory model.
- carrier-leg codec owner: a slim serialize/deserialize leg reaches for `arro3.io` over its `arro3-core` dependency alone, catching `exceptions.NotFoundError`/`PermissionDeniedError`/`PreconditionError` at the async store boundary and routing them onto the domain error rail.

[LOCAL_ADMISSION]:
- Read and write the carrier leg through `arro3.io` where the payload is IPC/Parquet/CSV/JSON.
- Hold the returned `RecordBatchReader` and compose `arro3.compute` `filter`/`cast`/`date_part` onto it, deferring materialization to a terminal drain.
- Front cloud Parquet with an `arro3.io.store` `ObjectStore` and `read_parquet_async`, selecting the store variant by scheme and carrying a `RetryConfig` for flaky transports.

[RAIL_LAW]:
- Package: `arro3-io`
- Owns: pyarrow-free Arrow IPC/Parquet/CSV/JSON/NDJSON readers and writers, cloud object-store carriers, and the typed IO failure hierarchy over the `arro3.core` memory model
- Accept: a path/`Path`/binary buffer on read; any PyCapsule producer through `arro3.core.types` on write
- Reject: a `pyarrow.parquet`/`pyarrow.csv`/`pyarrow.feather` re-import on a carrier leg the `arro3` model already serves
