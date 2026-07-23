# [PY_DATA_API_OBSPEC_UTILS]

`obspec-utils` folds the pieces `obstore` leaves out into one `obspec`-typed companion layer: a longest-prefix multi-store URL router (`ObjectStoreRegistry`), file-handle readers over byte-range stores, transparent read-path wrappers (cache, split, trace), object-store `glob`, and a pure-`aiohttp` `ReadableStore`. Every reader, wrapper, and store types against the `obspec` protocols, so `obstore` stores, `AiohttpStore`, and wrapper stacks compose interchangeably through one registry, re-implementing neither the multi-cloud engine `obstore` owns nor the protocol algebra `obspec` owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `obspec-utils`
- package: `obspec-utils` (Apache-2.0)
- module: `obspec_utils`
- namespaces: `registry`, `readers`, `wrappers`, `protocols`, `stores`, `glob`
- owner: `data`
- rail: object-store
- depends: `obspec` (protocol algebra — `Get`/`GetRange`/`GetRanges`/`Head`/`List` and async mirrors, `ObjectMeta`/`GetResult`/`GetOptions` TypedDicts), `obstore` (concrete stores and the `MemoryStore` reused as cache substrate)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: protocols (`obspec_utils.protocols`, with the `typing` aliases)

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]                 | [CAPABILITY]                               |
| :-----: | :-------------- | :---------------------------- | :----------------------------------------- |
|  [01]   | `ReadableStore` | `@runtime_checkable Protocol` | full read interface over the `obspec` gets |
|  [02]   | `ReadableFile`  | `@runtime_checkable Protocol` | minimal seekable handle                    |
|  [03]   | `Url` / `Path`  | `TypeAlias = str`             | registry-key and trailing-path aliases     |

- `ReadableFile`: `read(size=-1, /)`, `seek(offset, whence=0, /)`, `tell()` — every reader satisfies it, dropping into `h5py.File`, `zarr`, or `netCDF4`.

[PUBLIC_TYPE_SCOPE]: registry (`obspec_utils.registry`)

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]                | [CAPABILITY]                                 |
| :-----: | :----------------------- | :--------------------------- | :------------------------------------------- |
|  [01]   | `ObjectStoreRegistry[T]` | generic registry             | multi-store router; longest-prefix `resolve` |
|  [02]   | `UrlKey`                 | `namedtuple(scheme, netloc)` | primary registry key from `get_url_key`      |
|  [03]   | `PathEntry[T]`           | path-tree node               | longest-match tree node                      |

- `ObjectStoreRegistry[T]`: `T` bound by `obspec.Get`.
- `PathEntry[T]`: holds an optional `store` and `children`; `iter_stores()` and `lookup(path) -> (store, depth) | None` walk the longest match.

[PUBLIC_TYPE_SCOPE]: concrete store (`obspec_utils.stores`)

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]                       | [CAPABILITY]                                  |
| :-----: | :---------------------- | :---------------------------------- | :-------------------------------------------- |
|  [01]   | `AiohttpStore`          | `ReadableStore` impl                | `aiohttp` HTTP/HTTPS store, generic endpoints |
|  [02]   | `AiohttpGetResult`      | `@dataclass(obspec.GetResult)`      | sync get result                               |
|  [03]   | `AiohttpGetResultAsync` | `@dataclass(obspec.GetResultAsync)` | async get result                              |

- `AiohttpGetResult` carries `buffer()`, `meta`, `attributes`, `range`, `iter`; the async mirror carries `buffer_async()` and `aiter`.

[PUBLIC_TYPE_SCOPE]: trace records (`obspec_utils.wrappers`)

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :-------------- | :------------ | :--------------------------------------- |
|  [01]   | `RequestTrace`  | `@dataclass`  | `RequestRecord` collection with analysis |
|  [02]   | `RequestRecord` | `@dataclass`  | one range-request record                 |

- `RequestTrace`: `add(...)`, `clear()`, `to_dataframe()` (lazy `pandas`), `summary() -> dict`, `total_bytes`, `total_requests`, `requests`.
- `[RequestRecord]`: `path` `start` `length` `end` `timestamp` `duration` `method` `range_style`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registry operations (`obspec_utils.registry`)

| [INDEX] | [SURFACE]                                                  | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `ObjectStoreRegistry(stores: dict[Url, T] \| None = None)` | seed the router from a `{url: store}` map       |
|  [02]   | `registry.register(url, store) -> None`                    | bind or replace a store under a URL in the tree |
|  [03]   | `registry.resolve(url) -> tuple[T, Path]`                  | longest-prefix route to a store-relative `Path` |
|  [04]   | `async with registry: ...`                                 | open/close session-bearing stores on enter/exit |
|  [05]   | `get_url_key(url) -> UrlKey`                               | URL to its `(scheme, netloc)` key               |
|  [06]   | `path_segments(path) -> Iterator[str]`                     | non-empty path segments, empties dropped        |

- `registry.resolve` raises `ValueError` on no match; `get_url_key` raises `ValueError` on a schemeless URL.

[ENTRYPOINT_SCOPE]: glob discovery (`obspec_utils.glob`)

Every glob takes `(store, pattern)` — `store` an `obspec.List` (sync) or `obspec.ListAsync` (async) — and yields paths or full `ObjectMeta` (`path`/`last_modified`/`size`/`e_tag`/`version`).

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------- | :----------------------------------- |
|  [01]   | `glob(...) -> Iterator[str]`                           | match object paths against a glob    |
|  [02]   | `glob_objects(...) -> Iterator[ObjectMeta]`            | same match; yields full `ObjectMeta` |
|  [03]   | `glob_async(...) -> AsyncIterator[str]`                | async mirror over `list_async`       |
|  [04]   | `glob_objects_async(...) -> AsyncIterator[ObjectMeta]` | async mirror yielding `ObjectMeta`   |

`pattern` admits `*` (within a segment), `**` (recursive across segments), `?` (one char), and `[abc]`/`[a-z]`/`[!abc]` (class/range/negation); the literal leading prefix passes to `store.list(prefix=...)` so an anchored pattern narrows server-side before regex filtering, never a full-bucket scan.

[ENTRYPOINT_SCOPE]: file-handle readers (`obspec_utils.readers`)

Every reader constructs over `(store, path, …)` and its tuning kwargs, implementing `ReadableFile` with `readall()`, `readable()`/`seekable()`/`writable()`, `close()`, and the context-manager protocol — a drop-in seekable handle for `h5py.File`, `zarr`, or `netCDF4`.

| [INDEX] | [SURFACE]                                                                                        | [CAPABILITY]                  |
| :-----: | :----------------------------------------------------------------------------------------------- | :---------------------------- |
|  [01]   | `BufferedStoreReader(store, path, buffer_size=1 MiB)`                                            | sequential read-ahead buffer  |
|  [02]   | `EagerStoreReader(store, path, request_size=12 MiB, file_size=None, max_concurrent_requests=18)` | eager whole-file fetch to RAM |
|  [03]   | `BlockStoreReader(store, path, block_size=1 MiB, max_cached_blocks=64)`                          | sparse LRU block cache        |

- `BufferedStoreReader`: on-demand `get_range` reads behind a read-ahead buffer; best for forward streaming with rare back-seeks; requires `Store = Get + GetRange + Head`.
- `EagerStoreReader`: fetches the whole file at construction via concurrent `get_ranges` and serves reads from RAM; `file_size` skips the `head`; requires `Store = Get + GetRanges + Head`.
- `BlockStoreReader`: fixed-size blocks via concurrent `get_ranges` under a bounded LRU (`block_size × max_cached_blocks` memory cap); best for many non-contiguous reads of a large file; requires `Store = Get + GetRanges + Head`.

[ENTRYPOINT_SCOPE]: read-path wrappers (`obspec_utils.wrappers`, each a transparent `ReadableStore`)

Each wrapper takes a `ReadableStore` as its first `store` argument.

| [INDEX] | [SURFACE]                                                                        | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `CachingReadableStore(store, max_size=256 MiB)`                                  | whole-object LRU cache                |
|  [02]   | `SplittingReadableStore(store, request_size=12 MiB, max_concurrent_requests=18)` | split a large `get` into `get_ranges` |
|  [03]   | `TracingReadableStore(store, trace, *, on_request=None)`                         | record requests to a `RequestTrace`   |

- `CachingReadableStore`: caches whole objects in an `obstore.MemoryStore` on first access, serving later `get`/`get_range`/`get_ranges` from cache; thread-safe (`threading.Lock`), picklable (`__reduce__` ships a fresh empty cache for pool workers); exposes `clear_cache()`, `cache_size`, `cached_paths`; clears on context exit.
- `SplittingReadableStore`: rewrites a large `get`/`get_async` into concurrent `get_ranges` (heads the size, splits, concatenates); small files and explicit range methods pass through unchanged.
- `TracingReadableStore`: records every `get`/`get_range`/`get_ranges`/`head` into the `RequestTrace` with auto-timing on success and exception; `on_request(RequestRecord)` fires per request for live telemetry.

[ENTRYPOINT_SCOPE]: AiohttpStore operations (`obspec_utils.stores`, sync with `..._async` mirror)

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `get(path, *, options=None)`                           | download object; `options` range/conditionals    |
|  [02]   | `get_range(path, *, start, end=None, length=None)`     | HTTP `Range` request; `end` or `length` required |
|  [03]   | `get_ranges(path, *, starts, ends=None, lengths=None)` | concurrent ranges via `asyncio.gather`           |
|  [04]   | `head(path)`                                           | `HEAD` parsed into `ObjectMeta`                  |

- `AiohttpStore(base_url, *, headers=None, timeout=30.0)`: an async context manager reusing one `ClientSession`; `get_range` maps obspec end-exclusive to HTTP inclusive; `head` fills `ObjectMeta` size from `Content-Length`/`Content-Range` with `e_tag`/`last_modified`.
- `AiohttpStore` calls `response.raise_for_status()`, so HTTP errors surface as `aiohttp.ClientResponseError`; underlying `obstore` stores raise `obstore.exceptions.*`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Three-package stack: `obspec` owns the structural protocols, `obstore` the Rust-backed concrete stores and the `MemoryStore` the cache/split wrappers reuse as a buffer, `obspec_utils` the registry, readers, wrappers, glob, and `AiohttpStore` on top — all keyed to the `obspec` protocols, so an `obstore` store, an `AiohttpStore`, and a wrapper stack are mutually substitutable.
- Registry resolution keys on `(scheme, netloc)` then walks the path-segment tree for the longest registered prefix, stripping the store's own `prefix` (obstore) or parsed `url` path (`AiohttpStore`) so the returned `Path` is store-relative: register `s3://bucket`, resolve `s3://bucket/a/b` yields `(store, "a/b")`.
- Reader selection is a decision axis: `EagerStoreReader` when the whole file is needed or random access dominates (RAM-bound), `BlockStoreReader` for sparse partial reads of a large file (memory-capped LRU), `BufferedStoreReader` for forward streaming with rare back-seeks.

[STACKING]:
- `obstore`(`.api/obstore.md`): `from_url`/`S3Store`/`GCSStore` build the concrete stores registered in one `ObjectStoreRegistry`, and `obstore.MemoryStore` is the cache and split-buffer substrate.
- within-lib: the transparent `ReadableStore` wrappers nest — `CachingReadableStore(SplittingReadableStore(base))` turns a large `get` into concurrent `get_ranges`, caches the concatenation in the in-process `MemoryStore`, and serves the second read with zero network; `TracingReadableStore` outside the cache counts network requests, inside counts logical reads.
- `zarr`(`.api/zarr.md`) / `netcdf4`(`.api/netcdf4.md`) / `h5py`(`.api/h5py.md`): a reader hands a seekable handle — `h5py.File(BlockStoreReader(store, path))`, a `netCDF4.Dataset` over `EagerStoreReader`, a zarr chunk read — so the gridded, field, and virtual rails reach object-store bytes through one byte-range engine.
- `virtualizarr`(`.api/virtualizarr.md`) / `icechunk`(`.api/icechunk.md`) / `pystac-client`(`.api/pystac-client.md`): `glob`/`glob_objects` discover STAC asset hrefs and chunk inventories under a prefix, feeding the catalog asset-href fold and the virtual-reference manifest discovery without a full-bucket scan.
- `anyio`: `*_async` reader/wrapper/store mirrors, the `AiohttpStore` session, and the registry async context manager run inside one structured-concurrency task scope.
- `opentelemetry-api` / `structlog`: `TracingReadableStore.on_request(RequestRecord)` emits one structured event per range request (path/offset/length/duration/method) into the observability rail, one record mapping to one span attribute set.
- `pandas`(`.api/pandas.md`): `RequestTrace.to_dataframe()` yields a lazy-imported frame into the data-profile rail to grade cloud-read efficiency by request count, byte volume, and request-size distribution.
- `stamina` / `ContentIdentity`: wrap `registry.resolve(url)` and the read in a `stamina` retry for transient cloud failures — obstore stores carry native `RetryConfig`, the wrappers do not, so retry lives at the rail — keying the object by the runtime `ContentIdentity` so the egress reuse ledger dedupes across stores.
- `msgspec` / `pydantic`: decode the `obspec` `ObjectMeta`/`PutResult`/`ListResult` TypedDicts into `msgspec.Struct`/`pydantic` models at the boundary when a typed receipt is needed downstream.

[LOCAL_ADMISSION]:
- Import each owner from its subpackage (`obspec_utils.registry`/`readers`/`wrappers`/`protocols`/`stores`); the top-level surface re-exports only `glob`/`glob_objects`/`glob_async`/`glob_objects_async`, so `from obspec_utils import ObjectStoreRegistry` raises `ImportError`.
- Annotate a store with `obstore.store.ObjectStore` or a project-pinned `obspec` protocol composition; pin a narrower `class Store(Get, GetRange, GetRanges, Head, Protocol)` rather than the upstream-mutable `ReadableStore` for stable typing.
- One `ObjectStoreRegistry` routes heterogeneous asset URLs (`s3://`, `gs://`, `https://`) through a single boundary; a reader hands a seekable handle to a file-object library; `CachingReadableStore(SplittingReadableStore(base))` is the cloud-read accelerator.

[RAIL_LAW]:
- Package: `obspec-utils`
- Owns: the multi-store URL registry, file-handle readers over byte-range stores, transparent cache/split/trace read wrappers, object-store glob, and a pure-`aiohttp` `ReadableStore`, all typed on `obspec` protocols and composed with `obstore` stores.
- Accept: `obstore` stores and `AiohttpStore` registered in one `ObjectStoreRegistry`; readers handed to file-object libraries (`h5py`/`zarr`/`netCDF4`); `CachingReadableStore(SplittingReadableStore(base))` as the accelerator; `TracingReadableStore` with `RequestTrace` into the observability and profile rails.
- Reject: a hand-rolled URL-to-store dispatch dict, HTTP object client, or byte cache when the registry, `AiohttpStore`, and `CachingReadableStore` own them; the top-level `from obspec_utils import ObjectStoreRegistry` import; a second I/O path when an `obstore` store already serves the protocol.
