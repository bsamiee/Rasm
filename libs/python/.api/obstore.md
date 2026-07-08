# [PY_BRANCH_API_OBSTORE]

`obstore` is the branch object-store substrate over S3, GCS, Azure, HTTP, local filesystem, and memory stores. It supplies sync and async operations, store construction, credential providers, retry policy, zero-copy `Bytes`, Arrow-aware listing, conditional mutation, presigned URLs, buffered readers/writers, and an `fsspec` bridge.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `obstore`
- package: `obstore`
- import: `obstore`; `obstore.store`; `obstore.auth`; `obstore.fsspec`
- owner: `shared`
- rail: object storage
- version: `0.11.0`
- license: `MIT`
- capability: sync and async object-store IO, typed store constructors, `from_url` dispatch, credential-provider refresh, retry/backoff policy, get/put/delete/list/copy/rename/sign, range reads, conditional get/put, zero-copy byte buffers, Arrow list streaming, buffered file handles, and fsspec `AsyncFileSystem` adaptation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: store backends and config
- rail: object storage

| [INDEX] | [SYMBOL]                                                                     | [TYPE_FAMILY] | [RAIL]                                               |
| :-----: | :--------------------------------------------------------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `store.S3Store` / `GCSStore` / `AzureStore`                                  | store         | cloud object-store backends                          |
|  [02]   | `store.HTTPStore` / `LocalStore` / `MemoryStore`                             | store         | HTTP, local, and memory backends                     |
|  [03]   | `store.ObjectStore`                                                          | store alias   | canonical store-handle union returned by `from_url`  |
|  [04]   | `store.ObjectStoreMethods`                                                   | mixin base    | concrete bound-method surface every store subclasses |
|  [05]   | `S3Config` / `GCSConfig` / `AzureConfig`                                     | config dict   | provider-specific config                             |
|  [06]   | `ClientConfig` / `RetryConfig` / `BackoffConfig`                             | config dict   | HTTP client and retry/backoff policy                 |
|  [07]   | `S3CredentialProvider` / `GCSCredentialProvider` / `AzureCredentialProvider` | provider      | sync/async credential refresh callable               |

[PUBLIC_TYPE_SCOPE]: result, request, buffer, and faults
- rail: object storage

| [INDEX] | [SYMBOL]                                                                    | [TYPE_FAMILY] | [RAIL]                                                 |
| :-----: | :-------------------------------------------------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `Bytes`                                                                     | buffer        | zero-copy bytes wrapper with buffer protocol           |
|  [02]   | `GetResult`                                                                 | result        | metadata plus `.bytes()` and `.stream(min_chunk_size)` |
|  [03]   | `ObjectMeta` / `ListResult` / `ListStream`                                  | metadata      | object metadata and stream/list carriers               |
|  [04]   | `PutResult`                                                                 | result        | put response with `e_tag` and `version`                |
|  [05]   | `GetOptions` / `PutMode` / `UpdateVersion`                                  | request       | conditional get/put request policy                     |
|  [06]   | `OffsetRange` / `SuffixRange`                                               | range         | range request shapes                                   |
|  [07]   | `ReadableFile` / `AsyncReadableFile` / `WritableFile` / `AsyncWritableFile` | file          | buffered object file handles                           |
|  [08]   | `exceptions.BaseError` and subclasses                                       | fault         | object-store fault hierarchy                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: store construction, credentials, retry, and fsspec bridge
- rail: object storage

| [INDEX] | [SURFACE]                                                                                                                        | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `store.from_url(url, *, config=None, client_options=None, retry_config=None, credential_provider=None, **kwargs) -> ObjectStore` | construct      | URL-dispatched store construction |
|  [02]   | `S3Store(...)` / `GCSStore(...)` / `AzureStore(...)` / `HTTPStore(...)` / `LocalStore(...)` / `MemoryStore(...)`                 | construct      | typed store construction          |
|  [03]   | `parse_scheme(url) -> Literal["s3","gcs","http","local","memory","azure"]`                                                       | classify       | URL backend classification        |
|  [04]   | `auth.boto3.*` / `auth.google.*` / `auth.azure.*` / `auth.earthdata.*` / `auth.planetary_computer.*`                             | credential     | shipped credential providers      |
|  [05]   | `fsspec.register(protocols=None, *, asynchronous=False)` / `fsspec.FsspecStore` / `fsspec.BufferedFile`                          | bridge         | adapt obstore into fsspec         |

[ENTRYPOINT_SCOPE]: reads, mutation, listing, streaming, and signing
- rail: object storage

| [INDEX] | [SURFACE]                                                                                           | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `get` / `get_async` / `head` / `head_async`                                                         | read           | object and metadata reads                 |
|  [02]   | `get_range` / `get_range_async` / `get_ranges` / `get_ranges_async`                                 | range read     | single and coalesced range reads          |
|  [03]   | `put` / `put_async` / `delete` / `delete_async` / `copy` / `copy_async` / `rename` / `rename_async` | mutation       | object mutation                           |
|  [04]   | `list` / `list_with_delimiter` / async mirrors                                                      | listing        | recursive and delimited listings          |
|  [05]   | `open_reader` / `open_reader_async` / `open_writer` / `open_writer_async`                           | file           | buffered object stream handles            |
|  [06]   | `sign` / `sign_async`                                                                               | presign        | presigned URLs for signing-capable stores |

Every operation exists as both a module-level free function taking `store` first and a bound store method through `ObjectStoreMethods`; callers with a held store use the bound form, and callers threading stores as values use the free-function form.

## [04]-[IMPLEMENTATION_LAW]

[OBJECT_STORE_TOPOLOGY]:
- store law: `ObjectStore` is the canonical store-handle annotation; concrete stores subclass `ObjectStoreMethods`, but user code does not subclass it.
- credential law: stores accept sync or async credential providers returning typed credentials with expiration; obstore owns refresh and signing.
- retry law: `RetryConfig` and `BackoffConfig` travel into store construction; transient storage faults are governed at the store layer, and higher-level `stamina` rows wrap only the operation boundary.
- zero-copy law: `Bytes` implements the buffer protocol; callers materialize Python `bytes` only when an owning boundary requires ownership.
- conditional law: `GetOptions` carries `if_match`, `if_none_match`, time preconditions, ranges, versions, and head policy; `PutMode` carries create/overwrite/update-version compare-and-swap.
- range law: `get_range` and `get_ranges` accept explicit start/end/length or range dict shapes, and `get_ranges` coalesces nearby ranges.
- list law: `list(..., return_arrow=True)` emits Arrow `RecordBatch` chunks for direct Arrow/Polars ingestion.
- fsspec law: `obstore.fsspec` is the bridge for libraries requiring a filesystem handle; it is not a second object-store IO owner.

[STACK_LAW]:
- data mutation/object plane: data egress composes `put`, conditional `PutMode`, delete/copy/rename, Arrow listings, and presigned URLs for content-keyed object outputs.
- runtime transport plane: runtime roots construct stores with `from_url`, credential providers, retry config, and `sign_async`; read operations use sync or async variants according to the active server context.
- fsspec bridge: xarray, zarr, pyarrow, pandas, and DuckDB paths that accept fsspec filesystems receive an `obstore.fsspec` adapter when direct obstore integration is unavailable.
- external credential seams: Earthdata and Planetary Computer providers produce signed access for raster/STAC flows without reimplementing token refresh.

[EXCEPTIONS]:
- `NotFoundError`, `AlreadyExistsError`, `PreconditionError`, `NotModifiedError`, `PermissionDeniedError`, `UnauthenticatedError`, `NotSupportedError`, `InvalidPathError`, `UnknownConfigurationKeyError`, `GenericError`, and `JoinError` all subclass `exceptions.BaseError`.

[RAIL_LAW]:
- Package: `obstore`
- Owns: branch object-store construction, credentials, retry/backoff, read/list/range/mutation/sign operations, zero-copy buffers, buffered file handles, Arrow listing, and fsspec bridge
- Accept: `from_url` or typed constructors with `config`, `client_options`, `retry_config`, and `credential_provider`; free or bound operation forms; conditional mutation; Arrow listings; native credential providers; fsspec adaptation when required
- Reject: hand-rolled storage HTTP clients, duplicate token refresh, custom store protocols duplicating `ObjectStore`, parallel object IO paths beside the fsspec adapter, and folder-level package-surface duplication
