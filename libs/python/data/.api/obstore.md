# [PY_DATA_API_OBSTORE]

`obstore` supplies a sync/async object-store abstraction over S3, GCS, Azure, HTTP, local filesystem, and in-memory storage for the data object-store rail. The package owner exposes a uniform `get`/`put`/`delete`/`list` operation surface backed by the Rust `object_store` crate with zero-copy `Bytes` returns and Arrow-aware list streaming; it never re-implements the multi-cloud storage engine obstore owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `obstore`
- package: `obstore`
- import: `import obstore as obs; from obstore.store import S3Store, GCSStore, AzureStore, HTTPStore, LocalStore, MemoryStore, from_url`
- owner: `data`
- rail: object-store
- capability: sync and async object-store I/O — get/put/delete/list/copy/rename/sign over S3, GCS, Azure, HTTP, local, and memory backends with zero-copy Bytes and Arrow list streaming

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
|  [01]   | `obstore.Bytes`       | zero-copy buffer  | Rust-backed bytes implementing buffer protocol                       |
|  [02]   | `obstore.GetResult`   | get response      | materializes via `bytes()` or streams via `stream()`                 |
|  [03]   | `obstore.BytesStream` | chunk stream      | sync/async iterator of `bytes` chunks                                |
|  [04]   | `obstore.ObjectMeta`  | object metadata   | `TypedDict` with `path`, `last_modified`, `size`, `e_tag`, `version` |
|  [05]   | `obstore.ListResult`  | delimiter listing | `TypedDict` with `common_prefixes` and `objects`                     |
|  [06]   | `obstore.ListStream`  | list stream       | sync/async iterator of `ObjectMeta` or `RecordBatch` chunks          |
|  [07]   | `obstore.PutResult`   | put response      | `TypedDict` with `e_tag` and `version`                               |
|  [08]   | `obstore.GetOptions`  | get request opts  | `TypedDict` for conditional get, range, version                      |
|  [09]   | `obstore.PutMode`     | put precondition  | `"create"`, `"overwrite"`, or `UpdateVersion` dict                   |

## [03]-[ENTRYPOINTS]

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

[ENTRYPOINT_SCOPE]: store construction
- rail: object-store

| [INDEX] | [SYMBOL]                                                               | [PACKAGE_ROLE] | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `obstore.store.from_url(url, *, config, client_options, retry_config)` | factory        | dispatch store construction from URL |

## [04]-[IMPLEMENTATION_LAW]

[OBJECT_STORE_TOPOLOGY]:
- zero-copy: `Bytes` implements the Python buffer protocol; pass to `bytes()` only when Python-side copy is required
- multipart: `put` auto-uses multipart upload when file length exceeds `chunk_size`; disable via `use_multipart=False`
- list stream: `list` returns a `ListStream` that is not async under the hood; both `for` and `async for` work on the same stream object
- `return_arrow=True`: list operations return `arro3.core.RecordBatch` instead of `list[ObjectMeta]`; `arro3-core` must be installed
- `from_url` dispatch: URL scheme selects backend; `s3://`, `gs://`, `az://`, `file:///`, `memory:///`, `http://`, `https://` are all supported
- credential providers: each store class accepts a `credential_provider` callable for token refresh
- config types: `S3Config`, `GCSConfig`, `AzureConfig`, `ClientConfig`, `RetryConfig`, and `BackoffConfig` are all `TypedDict` shapes in `obstore.store`

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
- Owns: sync/async object-store I/O, store construction, zero-copy Bytes, list streaming, and credential providers for S3/GCS/Azure/HTTP/local/memory
- Accept: store instances constructed via `from_url` or store-specific constructors; `Bytes` passed zero-copy where possible
- Reject: hand-rolled HTTP object-store clients; thin wrappers that rename or partially expose operations obstore owns
