# [PY_BRANCH_API_OBSTORE]

`obstore` is the branch object-store substrate: sync and async object IO across cloud, HTTP, local, and memory backends through one `ObjectStore` handle built by `from_url` or a typed store, carrying zero-copy `Bytes`, conditional `PutMode`/`GetOptions` mutation, coalesced range reads, Arrow-native listing, credential-provider refresh, and presigned URLs. It owns the object plane directly; `obstore.fsspec` adapts a store to an `AbstractFileSystem` only where a downstream library requires a filesystem handle, never as a second IO owner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `obstore`
- package: `obstore` (MIT)
- module: `obstore`
- asset: native wheel (Rust `object_store` core via pyo3)
- rail: object storage
- namespaces: `obstore`, `obstore.store`, `obstore.auth`, `obstore.fsspec`, `obstore.exceptions`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: store backends and config

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `store.S3Store` / `GCSStore` / `AzureStore`      | store         | cloud object-store backends                          |
|  [02]   | `store.HTTPStore` / `LocalStore` / `MemoryStore` | store         | HTTP, local, and memory backends                     |
|  [03]   | `store.ObjectStore`                              | store alias   | canonical store-handle union returned by `from_url`  |
|  [04]   | `store.ObjectStoreMethods`                       | mixin base    | concrete bound-method surface every store subclasses |
|  [05]   | `S3Config` / `GCSConfig` / `AzureConfig`         | config dict   | provider-specific config                             |
|  [06]   | `ClientConfig` / `RetryConfig` / `BackoffConfig` | config dict   | HTTP client and retry/backoff policy                 |
|  [07]   | `S3CredentialProvider`                           | provider      | S3 sync/async credential refresh callable            |
|  [08]   | `GCSCredentialProvider`                          | provider      | GCS sync/async credential refresh callable           |
|  [09]   | `AzureCredentialProvider`                        | provider      | Azure sync/async credential refresh callable         |

[PUBLIC_TYPE_SCOPE]: result, request, buffer, and faults

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :----------------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `Bytes`                                    | buffer        | zero-copy bytes wrapper with buffer protocol           |
|  [02]   | `GetResult`                                | result        | metadata plus `.bytes()` and `.stream(min_chunk_size)` |
|  [03]   | `ObjectMeta` / `ListResult` / `ListStream` | metadata      | object metadata and stream/list carriers               |
|  [04]   | `PutResult`                                | result        | put response with `e_tag` and `version`                |
|  [05]   | `GetOptions` / `PutMode` / `UpdateVersion` | request       | conditional get/put request policy                     |
|  [06]   | `OffsetRange` / `SuffixRange`              | range         | range request shapes                                   |
|  [07]   | `ReadableFile` / `AsyncReadableFile`       | file          | buffered object read handle (sync/async)               |
|  [08]   | `WritableFile` / `AsyncWritableFile`       | file          | buffered object write handle (sync/async)              |
|  [09]   | `exceptions.BaseError`                     | fault         | root of the object-store fault hierarchy               |

[exceptions] subclass `BaseError`: `NotFoundError` `AlreadyExistsError` `PreconditionError` `NotModifiedError` `PermissionDeniedError` `UnauthenticatedError` `NotSupportedError` `InvalidPathError` `UnknownConfigurationKeyError` `GenericError` `JoinError`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: store construction and credentials
- construct carry: `config`, `client_options`, `retry_config`, `credential_provider`; `from_url` adds `url` and `**kwargs`, returning `ObjectStore`

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `store.from_url(url, **kwargs) -> ObjectStore`                                     | construct      | URL-dispatched store construction  |
|  [02]   | `S3Store` / `GCSStore` / `AzureStore` / `HTTPStore` / `LocalStore` / `MemoryStore` | construct      | typed store construction           |
|  [03]   | `parse_scheme(url) -> Literal["s3","gcs","http","local","memory","azure"]`         | classify       | URL backend classification         |
|  [04]   | `auth.boto3.*`                                                                     | credential     | AWS boto3 credential provider      |
|  [05]   | `auth.google.*`                                                                    | credential     | GCS credential provider            |
|  [06]   | `auth.azure.*`                                                                     | credential     | Azure credential provider          |
|  [07]   | `auth.earthdata.*`                                                                 | credential     | NASA Earthdata credential provider |
|  [08]   | `auth.planetary_computer.*`                                                        | credential     | MS Planetary Computer provider     |

[ENTRYPOINT_SCOPE]: fsspec bridge

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `fsspec.register(protocols=None, *, asynchronous=False)` | bridge         | register obstore as fsspec protocols |
|  [02]   | `fsspec.FsspecStore`                                     | bridge         | fsspec `AsyncFileSystem` adapter     |
|  [03]   | `fsspec.BufferedFile`                                    | bridge         | fsspec buffered file handle          |

[ENTRYPOINT_SCOPE]: reads, mutation, listing, streaming, and signing
- every operation carries a sync form and an `_async` mirror; the table lists the sync name

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :----------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `get` / `head`                       | read           | object and metadata reads                 |
|  [02]   | `get_range` / `get_ranges`           | range read     | single and coalesced range reads          |
|  [03]   | `put` / `delete` / `copy` / `rename` | mutation       | object mutation                           |
|  [04]   | `list` / `list_with_delimiter`       | listing        | recursive and delimited listings          |
|  [05]   | `open_reader` / `open_writer`        | file           | buffered object stream handles            |
|  [06]   | `sign`                               | presign        | presigned URLs for signing-capable stores |

Every operation is both a module-level free function taking `store` first and a bound method through `ObjectStoreMethods`: a held store calls the bound form, a store threaded as a value calls the free function.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- store law: `ObjectStore` is the canonical store-handle annotation; concrete stores subclass `ObjectStoreMethods` for the bound-method surface, and a custom backend implements the `obspec` protocols rather than subclassing it.
- credential law: stores accept sync or async credential providers returning typed credentials with expiration; obstore owns refresh and signing.
- retry law: `RetryConfig` and `BackoffConfig` travel into store construction; the store layer governs transient storage faults, and `stamina` wraps only the operation boundary.
- zero-copy law: `Bytes` implements the buffer protocol; callers materialize Python `bytes` only where an owning boundary requires ownership.
- conditional law: `GetOptions` carries `if_match`, `if_none_match`, time preconditions, ranges, versions, and head policy; `PutMode` carries create/overwrite/update-version compare-and-swap.
- range law: `get_range` and `get_ranges` accept explicit start/end/length or range-dict shapes, and `get_ranges` coalesces nearby ranges.
- list law: `list(return_arrow=True)` emits Arrow `RecordBatch` chunks for direct Arrow/Polars ingestion.

[STACKING]:
- `fsspec`(`.api/fsspec.md`): `obstore.fsspec.register`/`FsspecStore` adapt a store to an `AbstractFileSystem` only where xarray, zarr, pyarrow, or DuckDB require a filesystem handle; direct `get_range`/`put` stay the primary object plane.
- branch composition: runtime roots build stores with `from_url`, credential providers, and `RetryConfig`, reading through sync or async ops per the active server context; the egress plane composes `put` under conditional `PutMode`, delete/copy/rename, Arrow `list`, and `sign` for content-keyed outputs; `auth.earthdata`/`auth.planetary_computer` mint signed access for raster/STAC flows without reimplementing token refresh.

[LOCAL_ADMISSION]:
- Accept `from_url` or a typed store constructor carrying the four config values as the branch object-store construction surface.
- Accept free-function or bound operation forms, conditional `GetOptions`/`PutMode`, coalesced `get_ranges`, Arrow `list(return_arrow=True)`, and native credential providers.
- Accept `obstore.fsspec` only where a downstream library requires an `AbstractFileSystem` handle.

[RAIL_LAW]:
- Package: `obstore`
- Owns: branch object-store construction, credentials, retry/backoff, read/list/range/mutation/sign operations, zero-copy buffers, buffered file handles, Arrow listing, and the fsspec bridge
- Accept: `from_url` or typed constructors with the four config carries; free or bound operation forms; conditional mutation; Arrow listings; native credential providers; fsspec adaptation where a filesystem handle is required
- Reject: hand-rolled storage HTTP clients, duplicate token refresh, custom store protocols duplicating `ObjectStore`, object IO paths parallel to the fsspec adapter, and folder-tier package-surface duplication
