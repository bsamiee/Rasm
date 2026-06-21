# [PY_DATA_API_OBSTORE]

`obstore` supplies a sync/async object-store abstraction over S3, GCS, Azure, HTTP, local filesystem, and in-memory storage for the data object-store rail. The package owner exposes a uniform `get`/`put`/`delete`/`list` operation surface backed by the Rust `object_store` crate with zero-copy `Bytes` returns and Arrow-aware list streaming; it never re-implements the multi-cloud storage engine obstore owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `obstore`
- package: `obstore`
- version: `0.10.1` (manifest floor `>=0.10.1`)
- license: MIT
- import: `import obstore as obs; from obstore.store import S3Store, GCSStore, AzureStore, HTTPStore, LocalStore, MemoryStore, from_url`
- abi: `maturin` Rust extension shipping the `cp311-abi3` stable-ABI wheel (`_obstore.abi3.so`) — one wheel runs on every CPython `>=3.10` including cp315; the bundled `object_store` Rust crate version is exposed as `obstore._object_store_version`
- owner: `data`
- rail: object-store
- capability: sync and async object-store I/O — get/put/delete/list/copy/rename/sign over S3, GCS, Azure, HTTP, local, and memory backends with zero-copy Bytes, Arrow list streaming, buffered file readers/writers, presigned URLs, native credential providers, and an `fsspec` `AsyncFileSystem` bridge

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: store backends
- rail: object-store

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE] | [CAPABILITY]                              |
| :-----: | :-------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `obstore.store.S3Store`     | store backend  | Amazon S3 and S3-compatible object stores |
|  [02]   | `obstore.store.GCSStore`    | store backend  | Google Cloud Storage                      |
|  [03]   | `obstore.store.AzureStore`  | store backend  | Azure Blob Storage                        |
|  [04]   | `obstore.store.HTTPStore`   | store backend  | generic HTTP object store                 |
|  [05]   | `obstore.store.LocalStore`  | store backend  | local filesystem with optional prefix     |
|  [06]   | `obstore.store.MemoryStore` | store backend  | fully in-memory store for testing         |

[PUBLIC_TYPE_SCOPE]: result and metadata types
- rail: object-store

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]    | [CAPABILITY]                                                         |
| :-----: | :-------------------- | :---------------- | :------------------------------------------------------------------- |
|  [01]   | `obstore.Bytes`       | zero-copy buffer  | Rust-backed bytes implementing buffer protocol; `.to_bytes()`, slicing, `+` concat |
|  [02]   | `obstore.GetResult`   | get response      | `.bytes()`/`.bytes_async()` materialize, `.stream(min_chunk_size=)` streams, `.meta` carries `ObjectMeta`, `.attributes`, `.range` |
|  [03]   | `obstore.BytesStream` | chunk stream      | sync/async iterator of `Bytes` chunks                                |
|  [04]   | `obstore.ObjectMeta`  | object metadata   | `TypedDict` with `path`, `last_modified`, `size`, `e_tag`, `version` |
|  [05]   | `obstore.ListResult`  | delimiter listing | `TypedDict` with `common_prefixes` and `objects`                     |
|  [06]   | `obstore.ListStream`  | list stream       | sync/async iterator of `list[ObjectMeta]` or `RecordBatch` chunks    |
|  [07]   | `obstore.PutResult`   | put response      | `TypedDict` with `e_tag` and `version`                               |
|  [08]   | `obstore.GetOptions`  | get request opts  | `TypedDict(total=False)`: `if_match`, `if_none_match`, `if_modified_since`, `if_unmodified_since`, `range`, `version`, `head` |
|  [09]   | `obstore.PutMode`     | put precondition  | `"create" | "overwrite" | UpdateVersion` type alias                  |
|  [10]   | `obstore.UpdateVersion` | conditional put | `TypedDict(total=False)` with `e_tag`/`version` for compare-and-swap put |
|  [11]   | `obstore.Attributes`  | object attributes | `dict` of `Attribute` keys (`Content-Type`, `Content-Encoding`, `Content-Language`, `Content-Disposition`, `Cache-Control`, or `("Metadata", str)`) |
|  [12]   | `obstore.OffsetRange` / `obstore.SuffixRange` | range request | `TypedDict` range shapes for `GetOptions.range` (offset-from-start, suffix-from-end) |
|  [13]   | `obstore.ReadableFile` / `obstore.AsyncReadableFile` | buffered reader | seekable file object: `read`/`readall`/`readline`/`readlines`/`seek`/`tell`/`size` (async mirror is awaitable) |
|  [14]   | `obstore.WritableFile` / `obstore.AsyncWritableFile` | buffered writer | context-manager file object: `write`/`flush`/`close`/`closed` |

## [03]-[ENTRYPOINTS]

Every operation has two equivalent forms backed by the same Rust call: a module-level free function taking the store as first argument (`obs.get(store, path)`) and a bound store-instance method (`store.get(path)`) supplied by the `ObjectStoreMethods` mixin on every store class. The bound form is preferred at call sites that already hold a store; the free form composes when the store is threaded as data. Async mirrors (`*_async`) exist for both forms. The signatures below list the free-function form; the bound form drops the leading `store` argument.

[ENTRYPOINT_SCOPE]: get operations
- rail: object-store

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY]    | [RAIL]                         |
| :-----: | :--------------------------------------------------------------------------- | :---------------- | :----------------------------- |
|  [01]   | `get(store, path, *, options) -> GetResult`                                  | sync get          | retrieve full object           |
|  [02]   | `get_async(store, path, *, options) -> GetResult`                            | async get         | retrieve full object async     |
|  [03]   | `get_range(store, path, *, start, end, length) -> Bytes`                     | sync range        | retrieve byte range            |
|  [04]   | `get_range_async(store, path, *, start, end, length) -> Bytes`               | async range       | retrieve byte range async      |
|  [05]   | `get_ranges(store, path, *, starts, ends, lengths, coalesce) -> list[Bytes]` | sync multi-range  | retrieve multiple ranges       |
|  [06]   | `get_ranges_async(store, path, *, starts, ends, coalesce) -> list[Bytes]`    | async multi-range | retrieve multiple ranges async |
|  [07]   | `head(store, path) -> ObjectMeta`                                            | sync head         | fetch object metadata          |
|  [08]   | `head_async(store, path) -> ObjectMeta`                                      | async head        | fetch object metadata async    |

[ENTRYPOINT_SCOPE]: write and mutation operations
- rail: object-store

| [INDEX] | [SURFACE]                                                                                                    | [ENTRY_FAMILY] | [RAIL]                 |
| :-----: | :----------------------------------------------------------------------------------------------------------- | :------------- | :--------------------- |
|  [01]   | `put(store, path, file, *, attributes, tags, mode, use_multipart, chunk_size, max_concurrency) -> PutResult` | sync put       | write object           |
|  [02]   | `put_async(store, path, file, *, mode, use_multipart, chunk_size, max_concurrency) -> PutResult`             | async put      | write object async     |
|  [03]   | `delete(store, path)`                                                                                        | sync delete    | delete object          |
|  [04]   | `delete_async(store, path)`                                                                                  | async delete   | delete object async    |
|  [05]   | `copy(store, from_, to)`                                                                                     | sync copy      | server-side copy       |
|  [06]   | `copy_async(store, from_, to)`                                                                               | async copy     | server-side copy async |
|  [07]   | `rename(store, from_, to)`                                                                                   | sync rename    | atomic rename          |
|  [08]   | `rename_async(store, from_, to)`                                                                             | async rename   | atomic rename async    |

[ENTRYPOINT_SCOPE]: list and streaming operations
- rail: object-store

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY]  | [RAIL]                    |
| :-----: | :------------------------------------------------------------------------ | :-------------- | :------------------------ |
|  [01]   | `list(store, prefix, *, offset, chunk_size, return_arrow) -> ListStream`  | recursive list  | lazy recursive listing    |
|  [02]   | `list_with_delimiter(store, prefix, *, return_arrow) -> ListResult`       | flat list       | list with common prefixes |
|  [03]   | `list_with_delimiter_async(store, prefix, *, return_arrow) -> ListResult` | async flat list | flat list async           |
|  [04]   | `open_reader(store, path) -> ReadableFile`                                | buffered read   | buffered sync reader      |
|  [05]   | `open_reader_async(store, path) -> AsyncReadableFile`                     | buffered read   | buffered async reader     |
|  [06]   | `open_writer(store, path) -> WritableFile`                                | buffered write  | buffered sync writer      |
|  [07]   | `open_writer_async(store, path) -> AsyncWritableFile`                     | buffered write  | buffered async writer     |

[ENTRYPOINT_SCOPE]: presigned URLs and scheme parsing
- rail: object-store

| [INDEX] | [SURFACE]                                                                                 | [ENTRY_FAMILY]   | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------------------------------- | :--------------- | :------------------------------------------------ |
|  [01]   | `sign(store, method, paths, expires_in) -> str | Sequence[str]`                            | sync presign     | presigned URL(s); `paths` str -> str, seq -> seq  |
|  [02]   | `sign_async(store, method, paths, expires_in) -> str | Sequence[str]`                      | async presign    | presigned URL(s) async                            |
|  [03]   | `parse_scheme(url) -> Literal["s3","gcs","http","local","memory","azure"]`                 | scheme dispatch  | classify a URL to its backend without constructing a store |

`sign` accepts only the `SignCapableStore` union (`S3Store | GCSStore | AzureStore`); `method` is an `HTTP_METHOD` literal (`GET`/`PUT`/`POST`/`DELETE`/`HEAD`); `expires_in` is a `datetime.timedelta`. The str-vs-sequence `paths` overload discriminates single-URL from batch return.

[ENTRYPOINT_SCOPE]: store construction
- rail: object-store

| [INDEX] | [SYMBOL]                                                               | [PACKAGE_ROLE] | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `obstore.store.from_url(url, *, config, client_options, retry_config)` | factory        | dispatch store construction from URL |
|  [02]   | `S3Store(bucket, *, prefix, config, client_options, retry_config, credential_provider)` | constructor | typed S3 store (mirrored `GCSStore`/`AzureStore`/`HTTPStore`/`LocalStore`/`MemoryStore`) |

## [04]-[IMPLEMENTATION_LAW]

[OBJECT_STORE_TOPOLOGY]:
- dual form: every operation is both a free function `obs.op(store, ...)` and a bound method `store.op(...)` via `ObjectStoreMethods`; `obstore.store.ObjectStoreMethods` is the mixin, not a user base class (do not subclass to build a custom store)
- zero-copy: `Bytes` implements the Python buffer protocol (`memoryview`, slicing, `+`); call `.to_bytes()`/`bytes()` only when a Python-owned copy is required
- multipart: `put` auto-uses multipart upload when file length exceeds `chunk_size` (default `5 MiB`), with `max_concurrency` (default `12`) concurrent parts; disable via `use_multipart=False`
- conditional ops: `GetOptions` carries `if_match`/`if_none_match`/`if_modified_since`/`if_unmodified_since`/`range`/`version`/`head`; `PutMode` carries `"create"`/`"overwrite"`/`UpdateVersion` for compare-and-swap; failures surface as `PreconditionError`/`NotModifiedError`/`AlreadyExistsError`
- range: `GetOptions.range` accepts `(start, end)`, an int sequence, `OffsetRange`, or `SuffixRange`; `get_ranges` coalesces nearby ranges for one round trip
- list stream: `list` returns a `ListStream` usable with both `for` and `async for`; `list_with_delimiter` is the eager flat-listing form
- `return_arrow=True`: `list`/`list_with_delimiter` return `arro3.core.RecordBatch` instead of `list[ObjectMeta]`; `arro3-core` must be installed — this is the columnar handoff into the Arrow/Polars rail
- `from_url` dispatch: URL scheme selects backend; `s3://`, `gs://`, `az://`, `file:///`, `memory:///`, `http://`, `https://` supported; `parse_scheme(url)` classifies without constructing
- config types: `S3Config`, `GCSConfig`, `AzureConfig`, `ClientConfig`, `RetryConfig`, and `BackoffConfig` are `TypedDict` shapes in `obstore.store`; `RetryConfig` embeds `BackoffConfig` for exponential backoff with jitter

[CREDENTIAL_PROVIDERS]:
- each store class accepts `credential_provider=<callable>` returning a typed credential (`S3Credential`/`GCSCredential`/`AzureCredential` TypedDicts carrying token + `expires_at`); obstore caches and refreshes when expiry approaches
- the `*CredentialProvider` protocols are `S3CredentialProvider`/`GCSCredentialProvider`/`AzureCredentialProvider`; obstore invokes the provider as a zero-arg callable (sync `__call__` or `async __call__`) returning the credential TypedDict — the shipped `auth` providers implement that callable form
- `obstore.auth` ships first-class providers (each with a sync and an async class), gated on their optional backing SDK: `auth.boto3.Boto3CredentialProvider`/`StsCredentialProvider` (boto3 session / STS assume-role), `auth.google.GoogleCredentialProvider`/`GoogleAsyncCredentialProvider` (google-auth), `auth.azure.AzureCredentialProvider`/`AzureAsyncCredentialProvider` (azure-identity), `auth.earthdata.NasaEarthdataCredentialProvider`/`NasaEarthdataAsyncCredentialProvider` (NASA Earthdata S3 STS), `auth.planetary_computer.PlanetaryComputerCredentialProvider`/`PlanetaryComputerAsyncCredentialProvider` (Planetary Computer SAS token signing)
- the credentialed store is constructed once at boundary scope and threaded; the data rail never re-implements token refresh or signing

[FSSPEC_BRIDGE]:
- `obstore.fsspec` adapts any store into an fsspec `AsyncFileSystem`: `FsspecStore(protocol, *, ...)` is a concrete `fsspec.asyn.AsyncFileSystem`, `BufferedFile` an `AbstractBufferedFile`, and `register(protocol, *, asynchronous=False)` dynamically registers the subclass so `fsspec`-consuming libraries (`xarray`, `zarr`, `pyarrow.dataset`, `pandas`) read/write through obstore by URL
- the bridge is the seam for any admitted library that takes an `fsspec` filesystem rather than an obstore store directly; it is not a second I/O path

[INTEGRATION]:
- odc-stac seam: a `PlanetaryComputerCredentialProvider` (or `sign`) supplies the `patch_url=` callable `odc.stac.load` threads onto asset hrefs, so the lazy Dask cube reads SAS-signed cloud rasters; the same store credentializes the GDAL/rasterio session `configure_s3_access` sets up
- netcdf4 seam: `get` returns `Bytes` that feed `netCDF4.Dataset('inmem', memory=<bytes>)` for an in-RAM CF read, and a `Dataset(..., memory=N)` write whose `close()` `memoryview` is `put` back — a netCDF round trip with no temp file
- columnar seam: `list(..., return_arrow=True)` hands object inventories to the Arrow/Polars rail as `RecordBatch`; presigned `sign` URLs hand browser/CDN-facing access to the web tier without leaking credentials
- zarr/tensorstore seam: the `fsspec` bridge (or a store passed to `zarr`'s obstore-native backend) is the object-store substrate for the versioned-tensor (`zarr`/`tensorstore`/`icechunk`) read/write rail

[EXCEPTIONS]:
- `obstore.exceptions.BaseError` — root of all obstore exceptions
- `obstore.exceptions.AlreadyExistsError` — object already exists at path
- `obstore.exceptions.PreconditionError` — conditional put/get precondition failed
- `obstore.exceptions.NotModifiedError` — ETag match returned not-modified
- `obstore.exceptions.PermissionDeniedError` — credentials lack required permission
- `obstore.exceptions.UnauthenticatedError` — credentials not valid
- `obstore.exceptions.UnknownConfigurationKeyError` — invalid config key for the store

[RAIL_LAW]:
- Package: `obstore`
- Owns: sync/async object-store I/O (free-function and bound-method forms), store construction, zero-copy `Bytes`, buffered file readers/writers, conditional get/put, list streaming (incl. Arrow), presigned URLs, native credential providers, and the `fsspec` bridge for S3/GCS/Azure/HTTP/local/memory
- Accept: store instances via `from_url` or typed constructors with `credential_provider`/`retry_config`/`config`; bound-method calls where a store is held; `Bytes` passed zero-copy; `obstore.auth` providers for cloud credentials; `obstore.fsspec.register` to expose stores to fsspec consumers; `sign` for credential-free presigned access
- Reject: hand-rolled HTTP object-store clients; thin wrappers that rename or partially expose operations obstore owns; re-implemented token refresh when an `obstore.auth` provider exists; a second I/O path when the `fsspec` bridge already adapts the store
