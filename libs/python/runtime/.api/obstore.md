# [PY_RUNTIME_API_OBSTORE]

`obstore` supplies a unified object-storage interface over S3, GCS, Azure Blob, HTTP, and local/memory stores via Rust-backed store classes (`S3Store`, `GCSStore`, `AzureStore`, `HTTPStore`, `LocalStore`, `MemoryStore`) bound by the `ObjectStore` union, with module-level sync/async dispatch functions (`get`/`get_async`, `put`/`put_async`, `head`, `delete`, `list`, `copy`, `rename`, `sign`, `get_range`/`get_ranges`, `list_with_delimiter`, `open_reader`/`open_writer`), conditional-write preconditions (`PutMode`, `UpdateVersion`, `GetOptions`), typed config/retry/credential TypedDicts, an Arrow-native listing path, a zero-copy `Bytes` buffer, and a typed exception hierarchy. It is the Rust `object_store` backend the data tier composes while pure-Python cloud-storage packages lag on CPython 3.15.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `obstore`
- package: `obstore`
- import: `obstore`
- owner: `runtime`
- rail: object-storage
- namespaces: `obstore`, `obstore.store`, `obstore.exceptions`, `obstore.auth`, `obstore.fsspec`
- installed: `0.11.0`
- capability: unified multi-cloud object storage, sync/async operation pairs, conditional writes, byte-range and multi-range reads, streaming reader/writer, Arrow listing, presigned URLs, typed config/retry/credential providers, pluggable auth providers (`boto3`/Azure/Google/Earthdata/Planetary-Computer), and an `fsspec` `AbstractFileSystem` adapter

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: store classes and union
- rail: object-storage
- `ObjectStore` and per-store config/credential types are import-time `TYPE_CHECKING` annotations, not runtime constructors.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                                                   |
| :-----: | :------------------------ | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `store.S3Store`           | store         | AWS S3 / S3-compatible                                                   |
|  [02]   | `store.GCSStore`          | store         | Google Cloud Storage                                                     |
|  [03]   | `store.AzureStore`        | store         | Azure Blob Storage                                                       |
|  [04]   | `store.HTTPStore`         | store         | generic HTTP object store                                                |
|  [05]   | `store.LocalStore`        | store         | local filesystem                                                         |
|  [06]   | `store.MemoryStore`       | store         | in-process volatile memory store                                         |
|  [07]   | `store.ObjectStore`       | union alias   | the canonical store-handle type — runtime `UnionType` alias `S3Store \| GCSStore \| AzureStore \| HTTPStore \| LocalStore \| MemoryStore`, reflection-importable (absent from `store.__all__`), the type `from_url(...) -> ObjectStore` returns; annotate every store handle with this |
|  [08]   | `store.ObjectStoreMethods` | mixin base    | concrete mixin base class (a real `type`, NOT a `Protocol`, NOT `runtime_checkable`, absent from `store.__all__`) carrying the shared store surface (`copy`/`get`/`list`/`put`/`prefix`, pickle `__getstate__`/`__setstate__`); every store in the `ObjectStore` union subclasses it, so it is the structural superset that also admits a custom store, while the `ObjectStore` union is the idiomatic store-handle annotation `from_url` returns |

[PUBLIC_TYPE_SCOPE]: store config, retry, and credential providers
- rail: object-storage
- `*Config`/`*Credential`/`RetryConfig`/`ClientConfig` are `TypedDict`s; `*CredentialProvider` are sync/async callables returning a refreshed credential.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :-------------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `store.S3Config` / `store.GCSConfig` / `store.AzureConfig` | config dict   | provider-keyed config (region, endpoint, bucket, etc.)       |
|  [02]   | `store.ClientConfig`                                | config dict   | HTTP client knobs (timeouts, proxy, TLS, redirects)          |
|  [03]   | `store.RetryConfig`                                 | config dict   | retry policy (`max_retries`, `retry_timeout`, `backoff`)     |
|  [04]   | `store.BackoffConfig`                               | config dict   | exponential backoff (`init_backoff`, `max_backoff`, `base`)  |
|  [05]   | `store.S3CredentialProvider` / `GCSCredentialProvider` / `AzureCredentialProvider` | provider | sync/async callable yielding a refreshable credential        |

[PUBLIC_TYPE_SCOPE]: result, request, and buffer types
- rail: object-storage
- `Bytes` and the operation functions are top-level runtime exports; `GetResult`/`ObjectMeta`/`PutResult`/`ListResult`/`GetOptions` are `TYPE_CHECKING` return/option annotations.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                                                                |
| :-------: | :------------------------------ | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `Bytes`                           | buffer        | zero-copy bytes wrapper (buffer protocol; `to_bytes()` materializes)                  |
|  [02]   | `GetResult`                       | result        | streamed get; `.meta`, `.range`, `.attributes`, `.bytes()`, `.stream(min_chunk_size)` |
|  [03]   | `ObjectMeta`                      | dict          | `path`, `last_modified`, `size`, `e_tag`, `version`                                   |
|  [04]   | `PutResult`                       | dict          | `e_tag`, `version`                                                                    |
|  [05]   | `ListResult`                      | dict          | `common_prefixes`, `objects`                                                          |
|  [06]   | `GetOptions`                      | request dict  | `if_match`, `if_none_match`, `if_modified_since`, `if_unmodified_since`, `range`, `version`, `head` |
|  [07]   | `PutMode`                         | precond alias | `"create" \| "overwrite" \| UpdateVersion` — atomic-write precondition                |
|  [08]   | `UpdateVersion`                   | precond dict  | `e_tag` + `version` for conditional compare-and-set put                               |
|  [09]   | `OffsetRange` / `SuffixRange`     | range dict    | `{offset}` (from N) / `{suffix}` (last N bytes) range selectors                       |
|  [10]   | `Attributes` / `Attribute`        | metadata      | object attribute map (content-type, content-encoding, cache-control, custom)          |
|  [11]   | `BytesStream`                     | async stream  | async iterator of `Bytes` chunks from `GetResult.stream`                              |
|  [12]   | `ReadableFile` / `WritableFile`   | file handle   | sync streaming file handle from `open_reader`/`open_writer`                           |
|  [13]   | `AsyncReadableFile` / `AsyncWritableFile` | file handle | async streaming file handle                                                       |

[PUBLIC_TYPE_SCOPE]: exceptions family
- rail: object-storage
- all subclass `exceptions.BaseError` (itself an `Exception`); the conditional-write/get errors map to the precondition rails.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]  | [RAIL]                                          |
| :-----: | :---------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `exceptions.BaseError`                    | base exception | root of all obstore errors                      |
|  [02]   | `exceptions.NotFoundError`                | exception      | path not found                                  |
|  [03]   | `exceptions.AlreadyExistsError`           | exception      | path collision on `PutMode="create"`            |
|  [04]   | `exceptions.PreconditionError`            | exception      | conditional write/`UpdateVersion` compare failed |
|  [05]   | `exceptions.NotModifiedError`             | exception      | conditional get (`if_none_match`) not-modified  |
|  [06]   | `exceptions.PermissionDeniedError`        | exception      | access denied                                   |
|  [07]   | `exceptions.UnauthenticatedError`         | exception      | missing or invalid credentials                  |
|  [08]   | `exceptions.NotSupportedError`            | exception      | operation not supported by backend (e.g. `sign`) |
|  [09]   | `exceptions.InvalidPathError`             | exception      | invalid object path                             |
|  [10]   | `exceptions.UnknownConfigurationKeyError` | exception      | unknown config key at construction              |
|  [11]   | `exceptions.GenericError`                 | exception      | unclassified backend error                      |
|  [12]   | `exceptions.JoinError`                    | exception      | async task join failure                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: object operations (sync and async)
- rail: object-storage
- Each operation has a matching `_async` variant (`get_async`, `put_async`, `copy_async`, `delete_async`, `head_async`, `rename_async`, `get_range_async`, `get_ranges_async`, `sign_async`); signatures are identical.

| [INDEX] | [SURFACE]                                                                                                    | [ENTRY_FAMILY]   | [RAIL]                                          |
| :-----: | :----------------------------------------------------------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `get(store, path, *, options=None) -> GetResult`                                                             | object read      | fetch object; `options` carries conditional get |
|  [02]   | `put(store, path, file, *, attributes, tags, mode, use_multipart, chunk_size, max_concurrency) -> PutResult` | object write     | upload; `mode` is the atomic-write precondition |
|  [03]   | `head(store, path) -> ObjectMeta`                                                                            | metadata read    | fetch object metadata dict                      |
|  [04]   | `delete(store, paths) -> None`                                                                               | object delete    | delete one path or a sequence of paths          |
|  [05]   | `copy(store, from_, to, *, overwrite=True) -> None`                                                          | object copy      | server-side copy                                |
|  [06]   | `rename(store, from_, to, *, overwrite=True) -> None`                                                        | object move      | server-side rename/move                         |
|  [07]   | `get_range(store, path, *, start, end=None, length=None) -> Bytes`                                           | partial read     | read a byte range from an object                |
|  [08]   | `get_ranges(store, path, *, starts, ends=None, lengths=None) -> list[Bytes]`                                 | multi-range read | coalesced concurrent multi-range read           |
|  [09]   | `sign(store, method, paths, expires_in: timedelta) -> str \| list[str]`                                       | presign          | presigned URL(s); `store` is restricted to `SignCapableStore` (`S3Store \| GCSStore \| AzureStore`), `method` is `HTTP_METHOD` (`GET`/`PUT`/`POST`/`DELETE`/`HEAD`...), `expires_in` is a `timedelta`; a single path returns `str`, a sequence returns `list[str]` |

[ENTRYPOINT_SCOPE]: listing operations
- rail: object-storage

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `list(store, prefix=None, *, offset=None, chunk_size=50, return_arrow=False) -> ListStream`        | list stream    | chunked listing; Arrow `RecordBatch` or `ObjectMeta` stream |
|  [02]   | `list_with_delimiter(store, prefix=None, *, return_arrow=False) -> ListResult`                     | delimited list | one level; `common_prefixes` + `objects`    |

[ENTRYPOINT_SCOPE]: streaming IO
- rail: object-storage
- Each has an `_async` variant (`open_reader_async`, `open_writer_async`).

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :----------------------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `open_reader(store, path, *, buffer_size=..., size=None) -> ReadableFile`            | reader         | seekable streaming reader           |
|  [02]   | `open_writer(store, path, *, attributes, buffer_size=..., tags, max_concurrency=12) -> WritableFile` | writer | buffered multipart streaming writer |

[ENTRYPOINT_SCOPE]: store construction
- rail: object-storage
- `from_url` is overloaded per scheme; the bare module-level `store.from_url` dispatches to the matching class. `parse_scheme` is the top-level `obstore.parse_scheme` (not `obstore.store.parse_scheme`) and returns the bare backend-scheme `Literal`, never a `(scheme, path)` tuple.

| [INDEX] | [SURFACE]                                                                                                                                  | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `store.from_url(url, *, config=None, client_options=None, retry_config=None, credential_provider=None, **kwargs) -> ObjectStore`            | URL factory    | dispatch store by URL scheme  |
|  [02]   | `store.S3Store.from_url(url, *, config, client_options, retry_config, credential_provider, **kwargs)`                                       | class factory  | construct S3 store from URL   |
|  [03]   | `store.GCSStore.from_url(...)` / `store.AzureStore.from_url(...)`                                                                           | class factory  | per-provider URL construction |
|  [04]   | `store.HTTPStore.from_url(url, *, client_options, retry_config)`                                                                            | class factory  | HTTP store from base URL      |
|  [05]   | `store.LocalStore(prefix=None, *, automatic_cleanup=False, mkdir=False)` / `store.LocalStore.from_url(url, *, automatic_cleanup=False, mkdir=False)` | class factory | local store; `prefix` scopes all paths to a directory root, `.prefix` reads it back |
|  [06]   | `obstore.parse_scheme(url) -> Literal["s3", "gcs", "http", "local", "memory", "azure"]`                                                     | utility        | top-level backend-scheme classifier |

[ENTRYPOINT_SCOPE]: pluggable credential providers and fsspec adapter
- rail: object-storage
- `auth.*` providers are sync/async callables matching the per-store `*CredentialProvider` shape; `obstore.fsspec` bridges a store into the `fsspec` filesystem ecosystem the sibling `fsspec.md`/`s3fs.md`/`gcsfs.md` catalogs own.

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `auth.boto3.Boto3CredentialProvider(...)` / `auth.boto3.StsCredentialProvider(...)`        | credential     | `S3Store` provider over a `boto3.session.Session` / STS `assume_role`, `__call__ -> S3Credential` |
|  [02]   | `auth.google.GoogleCredentialProvider(...)` / `auth.google.GoogleAsyncCredentialProvider(...)` | credential | `GCSStore` provider over `google.auth`, sync/async `__call__ -> GCSCredential` |
|  [03]   | `auth.azure.AzureCredentialProvider(...)` / `auth.azure.AzureAsyncCredentialProvider(...)` | credential     | `AzureStore` provider over `azure.identity`, sync/async `__call__ -> AzureCredential` |
|  [04]   | `auth.earthdata.*` / `auth.planetary_computer.*`                                           | credential     | domain credential providers (NASA Earthdata, MS Planetary Computer token signing) |
|  [05]   | `fsspec.register(protocols=None, *, asynchronous=False)` / `fsspec.FsspecStore` / `fsspec.BufferedFile` | fsspec adapter | register obstore as an `fsspec` `AsyncFileSystem` so Arrow/pandas/zarr readers consume a store by URL |

## [04]-[IMPLEMENTATION_LAW]

[STORAGE_TOPOLOGY]:
- `ObjectStore` is the canonical store-handle type — the runtime `UnionType` alias `S3Store | GCSStore | AzureStore | HTTPStore | LocalStore | MemoryStore` `from_url(...) -> ObjectStore` returns; annotate every store handle with it. `store.ObjectStoreMethods` is the concrete mixin base class (a real `type`, NOT a `Protocol`, NOT `runtime_checkable`) capturing the shared store surface (`copy`/`get`/`list`/`put`/`prefix`/pickle `__getstate__`/`__setstate__`) every store in the union subclasses — the structural superset that also admits a hypothetical custom store, not the idiomatic handle annotation. Module-level functions accept any `ObjectStore` member as the first positional argument; the Rust core dispatches structurally rather than through Python method calls, so the union is for type-checking, not runtime virtual dispatch. The runtime composes the existing `store.ObjectStore`/`store.ObjectStoreMethods` surface and never re-declares a parallel store base class.
- conditional-write law: `put(..., mode=...)` is the atomic precondition — `"overwrite"` (default), `"create"` (raises `AlreadyExistsError` on collision), or an `UpdateVersion` (`{e_tag, version}`) compare-and-set that raises `PreconditionError` on mismatch; conditional get rides `GetOptions.if_match`/`if_none_match`/`if_modified_since` and surfaces `NotModifiedError`/`PreconditionError`. Optimistic concurrency rides these, never a read-then-write race.
- byte-range law: `get_range`/`get_ranges` and `GetOptions.range` accept `(start, end)` tuples, `OffsetRange` (`{offset}`, from N to end), or `SuffixRange` (`{suffix}`, last N bytes); `get_ranges` coalesces nearby ranges into concurrent fetches.
- streaming law: `GetResult.bytes()` materializes the full body; `GetResult.stream(min_chunk_size)` returns a `BytesStream` async iterator; `open_reader`/`open_writer` own large-object seekable streaming and multipart upload, `get`/`put` own single-buffer transfers.
- retry/credential law: transient faults (5xx, connection drops, timeouts on read-only requests) are governed by `RetryConfig` + `BackoffConfig` passed at construction, and rotating credentials by a `*CredentialProvider` — either a bare sync/async callable or an `obstore.auth` provider (`Boto3CredentialProvider`/`StsCredentialProvider` for S3, `Google[Async]CredentialProvider` for GCS, `Azure[Async]CredentialProvider` for Azure) the Rust core re-invokes on expiry; retry scheduling for storage faults lives in the store config, never a hand-rolled loop, and credential refresh lives in the provider, never a hand-rolled refresh thread.
- fsspec-bridge law: `obstore.fsspec.register(...)` installs an `fsspec` `AsyncFileSystem` so Arrow/pandas/zarr/parquet readers that speak `fsspec` consume an obstore-backed URL directly; this is the single bridge into the `fsspec` ecosystem (see `fsspec.md`/`s3fs.md`/`gcsfs.md`), not a parallel filesystem shim.
- listing law: `list()` returns a `ListStream`; `return_arrow=True` yields Arrow `RecordBatch` for zero-copy bulk metadata into the Arrow/data tier instead of `Sequence[ObjectMeta]`.
- defaults: `put` `chunk_size` defaults to 5 MiB, `max_concurrency` to 12 for multipart; `list` `chunk_size` to 50.

[LOCAL_ADMISSION]:
- `from_url` is the canonical store construction path in composition roots, threading `config`/`client_options`/`retry_config`/`credential_provider`; direct class constructors accept the same keyword config for explicit wiring.
- async variants (`*_async`) are used inside asyncio contexts and the `grpc.aio` serve leg; sync variants in blocking/thread-pool contexts.
- integration rail: `get(..., options=GetOptions(if_none_match=etag))` returns `Bytes` (buffer-protocol, zero-copy into Arrow/NumPy or a `msgspec.Decoder`), the conditional get/put preconditions back content-keyed idempotent writes, and the `*CredentialProvider` is fed from a `pydantic-settings`-validated credential model — one validated settings layer, never a hard-coded key. Transient `BaseError` subclasses lift to `BoundaryFault` at the data egress; `NotModifiedError`/`PreconditionError` are control-flow signals on the conditional rail, not faults.

[RAIL_LAW]:
- Package: `obstore`
- Owns: cloud and local object-storage reads, writes, listings, conditional writes, byte-range reads, streaming, and presigned URLs over the Rust `object_store` core
- Accept: `from_url` construction with `config`/`retry_config`/`credential_provider`; `LocalStore(prefix=...)` path-scoped roots; module-level sync/async dispatch functions for all operations; `PutMode`/`UpdateVersion` conditional writes; `GetOptions` conditional/range gets; `open_reader`/`open_writer` for streaming; `return_arrow=True` for bulk listing; the `Bytes` zero-copy buffer; `obstore.auth` credential providers for rotating cloud credentials; `obstore.fsspec.register` to expose a store through the `fsspec` ecosystem
- Reject: a re-declared store base class or store-surface protocol duplicating `store.ObjectStoreMethods`; direct HTTP calls to storage APIs; hand-rolled S3/GCS request signing; a hand-rolled credential-refresh thread where `obstore.auth` providers or a `*CredentialProvider` callable already rotate; read-then-write races where a conditional `PutMode` applies; blocking `bytes()` reads of large objects when `stream`/`open_reader` is available; hand-rolled retry loops where `RetryConfig` governs
