# [PY_RUNTIME_API_OBSTORE]

`obstore` supplies a unified object-storage interface over S3, GCS, Azure Blob, HTTP, and local/memory stores via Rust-backed store classes (`S3Store`, `GCSStore`, `AzureStore`, `HTTPStore`, `LocalStore`, `MemoryStore`) and module-level sync/async dispatch functions (`get`, `put`, `head`, `delete`, `list`, `copy`, `rename`, `sign`, `get_range`, `get_ranges`, `list_with_delimiter`, `open_reader`, `open_writer`), with an Arrow-native listing path and a typed exception hierarchy.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `obstore`
- package: `obstore`
- module: `obstore`
- asset: Rust extension (PyO3 wheel)
- rail: object-storage
- namespaces: `obstore`, `obstore.store`, `obstore.exceptions`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: store classes
- rail: object-storage

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------- | :------------ | :------------------------------------- |
|  [01]   | `store.S3Store`            | store         | AWS S3 / S3-compatible                 |
|  [02]   | `store.GCSStore`           | store         | Google Cloud Storage                   |
|  [03]   | `store.AzureStore`         | store         | Azure Blob Storage                     |
|  [04]   | `store.HTTPStore`          | store         | generic HTTP object store              |
|  [05]   | `store.LocalStore`         | store         | local filesystem                       |
|  [06]   | `store.MemoryStore`        | store         | in-process volatile memory store       |
|  [07]   | `store.ObjectStoreMethods` | protocol      | structural protocol all stores satisfy |

[PUBLIC_TYPE_SCOPE]: result and data types
- rail: object-storage
- `Bytes` is a top-level runtime export; `GetResult`/`ObjectMeta`/`PutResult`/`ListResult` are typing return-types carried by the operation signatures, imported for annotation, not constructed as top-level names.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [RAIL]                                                                                  |
| :-----: | :----------- | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `Bytes`      | buffer        | zero-copy bytes wrapper with `to_bytes()` method                                        |
|  [02]   | `GetResult`  | result        | streamed get result; `.meta`, `.range`, `.attributes`, `.bytes()`, `.stream`, `.buffer` |
|  [03]   | `ObjectMeta` | dict          | `path`, `last_modified`, `size`, `e_tag`, `version`                                     |
|  [04]   | `PutResult`  | dict          | `e_tag`, `version`                                                                      |
|  [05]   | `ListResult` | dict          | `common_prefixes`, `objects`                                                            |

[PUBLIC_TYPE_SCOPE]: exceptions family
- rail: object-storage

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]  | [RAIL]                                |
| :-----: | :---------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `exceptions.BaseError`                    | base exception | all obstore errors                    |
|  [02]   | `exceptions.NotFoundError`                | exception      | path not found                        |
|  [03]   | `exceptions.AlreadyExistsError`           | exception      | path collision on exclusive write     |
|  [04]   | `exceptions.PermissionDeniedError`        | exception      | access denied                         |
|  [05]   | `exceptions.UnauthenticatedError`         | exception      | missing or invalid credentials        |
|  [06]   | `exceptions.PreconditionError`            | exception      | conditional write failed              |
|  [07]   | `exceptions.NotModifiedError`             | exception      | conditional get returned not-modified |
|  [08]   | `exceptions.NotSupportedError`            | exception      | operation not supported by backend    |
|  [09]   | `exceptions.InvalidPathError`             | exception      | invalid object path                   |
|  [10]   | `exceptions.UnknownConfigurationKeyError` | exception      | unknown config key at construction    |
|  [11]   | `exceptions.GenericError`                 | exception      | unclassified backend error            |
|  [12]   | `exceptions.JoinError`                    | exception      | async task join failure               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: object operations (sync and async)
- rail: object-storage
- Each operation has a matching `_async` variant (e.g. `get_async`, `put_async`); signatures are identical.

| [INDEX] | [SURFACE]                                                                                                    | [ENTRY_FAMILY]   | [RAIL]                                |
| :-----: | :----------------------------------------------------------------------------------------------------------- | :--------------- | :------------------------------------ |
|  [01]   | `get(store, path, *, options=None) -> GetResult`                                                             | object read      | fetch object; stream or buffer result |
|  [02]   | `put(store, path, file, *, attributes, tags, mode, use_multipart, chunk_size, max_concurrency) -> PutResult` | object write     | upload object                         |
|  [03]   | `head(store, path) -> ObjectMeta`                                                                            | metadata read    | fetch object metadata dict            |
|  [04]   | `delete(store, paths) -> None`                                                                               | object delete    | delete one or multiple paths          |
|  [05]   | `copy(store, from_, to, *, overwrite=True) -> None`                                                          | object copy      | server-side copy                      |
|  [06]   | `rename(store, from_, to, *, overwrite=True) -> None`                                                        | object move      | server-side rename/move               |
|  [07]   | `get_range(store, path, *, start, end=None, length=None) -> Bytes`                                           | partial read     | read byte range from object           |
|  [08]   | `get_ranges(store, path, *, starts, ends=None, lengths=None, coalesce) -> list[Bytes]`                       | multi-range read | read multiple byte ranges             |
|  [09]   | `sign(store, method, paths, expires_in)`                                                                     | presign          | generate presigned URL(s)             |

[ENTRYPOINT_SCOPE]: listing operations
- rail: object-storage

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :----------------------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `list(store, prefix=None, *, offset=None, chunk_size=50, return_arrow=False)`  | list stream    | chunked listing; Arrow or ObjectMeta stream |
|  [02]   | `list_with_delimiter(store, prefix=None, *, return_arrow=False) -> ListResult` | delimited list | returns `common_prefixes` + `objects`       |

[ENTRYPOINT_SCOPE]: streaming IO
- rail: object-storage

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :----------------------------------------------------------------------------------- | :------------- | :-------------------------- |
|  [01]   | `open_reader(store, path, *, buffer_size=..., size=None)`                            | reader         | async/sync streaming reader |
|  [02]   | `open_writer(store, path, *, attributes, buffer_size=..., tags, max_concurrency=12)` | writer         | async/sync streaming writer |

[ENTRYPOINT_SCOPE]: store construction
- rail: object-storage

| [INDEX] | [SURFACE]                                                                                                                        | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `store.from_url(url, *, config=None, client_options=None, retry_config=None, credential_provider=None, **kwargs) -> ObjectStore` | URL factory    | dispatch store by URL scheme  |
|  [02]   | `store.S3Store.from_url(url, *, config, client_options, retry_config, credential_provider, **kwargs)`                            | class factory  | construct S3 store from URL   |
|  [03]   | `store.GCSStore.from_url(url, ...)` / `store.AzureStore.from_url(url, ...)`                                                      | class factory  | per-provider URL construction |
|  [04]   | `store.HTTPStore.from_url(url, *, client_options, retry_config)`                                                                 | class factory  | HTTP store from base URL      |
|  [05]   | `store.LocalStore.from_url(url, *, automatic_cleanup=False, mkdir=False)`                                                        | class factory  | local store from file:// URL  |
|  [06]   | `parse_scheme(url) -> str`                                                                                                       | utility        | extract URL scheme string     |

## [04]-[IMPLEMENTATION_LAW]

[STORAGE_TOPOLOGY]:
- `ObjectStore` type alias: `S3Store | GCSStore | HTTPStore | AzureStore | LocalStore | MemoryStore`
- `ObjectStoreMethods` is a structural Protocol; all store classes satisfy it without explicit inheritance
- module-level functions accept any `ObjectStore` as the first argument; they do not require subclassing
- `GetResult.bytes()` materializes the full body; `.stream` returns an async iterator of `Bytes` chunks
- `ObjectMeta` is a plain `dict` with keys: `path`, `last_modified`, `size`, `e_tag`, `version`
- `list()` returns a `ListStream` iterable; `return_arrow=True` yields Arrow `RecordBatch` instead of `Sequence[ObjectMeta]`
- `delete` accepts a single path string or a sequence of path strings
- presigned URL generation via `sign` requires the store to support it; `NotSupportedError` is raised otherwise
- `chunk_size` in `put` defaults to 5 MiB (5242880 bytes); `max_concurrency` defaults to 12 for multipart uploads
- `coalesce` in `get_ranges` defaults to 1 MiB (1048576 bytes) merge window

[LOCAL_ADMISSION]:
- `from_url` is the canonical store construction path in composition roots; direct class constructors accept keyword config
- async variants (`*_async`) are used inside asyncio contexts; sync variants are used in blocking/thread-pool contexts
- `open_reader`/`open_writer` own large-object streaming; `get`/`put` own single-buffer transfers

[RAIL_LAW]:
- Package: `obstore`
- Owns: cloud and local object-storage reads, writes, listings, and presigned URLs
- Accept: `from_url` construction; module-level dispatch functions for all operations; `open_reader`/`open_writer` for streaming
- Reject: direct HTTP calls to storage APIs; hand-rolled S3/GCS request signing; blocking reads of large objects via full `bytes()` when streaming is available
