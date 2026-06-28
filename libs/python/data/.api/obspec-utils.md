# [PY_DATA_API_OBSPEC_UTILS]

`obspec-utils` is the companion layer over `obspec` (the structural store-protocol package) and `obstore` (the Rust object-store engine): it adds the pieces `obstore` deliberately leaves out — a longest-prefix multi-store URL router (`ObjectStoreRegistry`), file-handle readers that adapt a byte-range store into a seekable handle for libraries that demand a file object (`h5py`, `zarr`, `netCDF4`), transparent read-path wrappers (LRU caching, concurrent request splitting, request tracing), object-store `glob`, and a pure-`aiohttp` HTTP store for endpoints where `obstore`'s `HTTPStore` is the wrong fit. Every reader, wrapper, and store is typed against `obspec` protocols, so `obstore`'s `S3Store`/`GCSStore`/`AzureStore`/`MemoryStore`, the `AiohttpStore`, and the wrappers all compose interchangeably through one registry; it never re-implements the multi-cloud I/O engine `obstore` owns or the protocol algebra `obspec` owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `obspec-utils`
- package: `obspec-utils`
- version: `0.9.0`
- license: Apache-2.0
- module: `obspec_utils`
- owner: `data`
- rail: object-store
- depends: `obspec` (protocol surface — `Get`/`GetRange`/`GetRanges`/`Head`/`List` and async mirrors, `ObjectMeta`/`GetResult`/`GetOptions` TypedDicts), `obstore` (concrete stores + `MemoryStore` used as the cache/wrap substrate)
- import: `from obspec_utils.registry import ObjectStoreRegistry; from obspec_utils.readers import BufferedStoreReader, EagerStoreReader, BlockStoreReader; from obspec_utils.wrappers import CachingReadableStore, SplittingReadableStore, TracingReadableStore, RequestTrace, RequestRecord; from obspec_utils.protocols import ReadableStore, ReadableFile; from obspec_utils.stores import AiohttpStore; from obspec_utils.glob import glob, glob_objects, glob_async, glob_objects_async`
- import-law: the top-level `obspec_utils.__all__` is ONLY `{__version__, glob, glob_objects, glob_async, glob_objects_async}`. `ObjectStoreRegistry`, the readers, the wrappers, the protocols, and `AiohttpStore` are NOT re-exported at top level — import each from its subpackage; a top-level `from obspec_utils import ObjectStoreRegistry` raises `ImportError` (only the glob functions are re-exported — the package's own internal code reaches the registry/stores only under `TYPE_CHECKING`). The flat `obspec_utils.{aiohttp,obspec,cache,tracing,splitting}` modules are deprecated shims that emit `DeprecationWarning`; use the subpackages.
- capability: multi-store URL registry with longest-path-segment resolution and async-context store lifecycle; three file-handle readers (sequential-buffered, eager-whole-file, block-LRU-sparse) over byte-range stores; transparent read wrappers (LRU full-object cache, large-`get` request splitting, request tracing to a pandas-able trace); object-store `glob` (sync/async, paths or `ObjectMeta`); a pure-`aiohttp` `ReadableStore` for generic HTTPS/THREDDS/NASA endpoints

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: protocols
- rail: object-store

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [ROLE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `obspec_utils.protocols.ReadableStore` | `@runtime_checkable Protocol` | full read interface — composes `obspec.{Get,GetAsync,GetRange,GetRangeAsync,GetRanges,GetRangesAsync,Head,HeadAsync}`; the structural type every wrapper accepts and `obstore`'s read stores satisfy; upstream warns it may change, so a consumer SHOULD pin its own narrower protocol for stable typing |
|  [02]   | `obspec_utils.protocols.ReadableFile` | `@runtime_checkable Protocol` | minimal seekable handle — `read(size=-1, /)`, `seek(offset, whence=0, /)`, `tell()`; the contract every reader satisfies, so a reader drops into any API expecting a file object (`h5py.File`, `zarr`, `netCDF4`) |
|  [03]   | `obspec_utils.typing.Url` / `obspec_utils.typing.Path` | `TypeAlias = str` | registry key and trailing-path string aliases (semantic, not branded) |

[PUBLIC_TYPE_SCOPE]: registry
- rail: object-store

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [ROLE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `obspec_utils.registry.ObjectStoreRegistry[T]` | generic registry | `Generic[T]`, `T` bound by `obspec.Get`; maps `(scheme, netloc)` + a path-segment tree to stores; longest-prefix `resolve`; async context manager that opens/closes session-bearing stores |
|  [02]   | `obspec_utils.registry.UrlKey` | `namedtuple(scheme, netloc)` | the primary registry key (URL scheme + authority); built by `get_url_key` |
|  [03]   | `obspec_utils.registry.PathEntry[T]` | path-segment tree node | `Generic[T]` node holding an optional `store` plus `children`; `iter_stores()` and `lookup(path) -> (store, depth) | None` implement longest-match traversal (internal to the registry, importable) |

[PUBLIC_TYPE_SCOPE]: concrete store
- rail: object-store

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [ROLE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `obspec_utils.stores.AiohttpStore` | `ReadableStore` impl | `aiohttp`-backed HTTP/HTTPS store for generic endpoints (THREDDS, NASA outside AWS region) where `obstore.store.HTTPStore` WebDAV/S3 semantics are wrong; async context manager reuses one `ClientSession`; `base_url`/`headers`/`timeout` |
|  [02]   | `obspec_utils.stores.AiohttpGetResult` | `@dataclass(obspec.GetResult)` | sync get result — `buffer()`, `meta`, `attributes`, `range`, `__iter__` |
|  [03]   | `obspec_utils.stores.AiohttpGetResultAsync` | `@dataclass(obspec.GetResultAsync)` | async get result — `buffer_async()`, `meta`, `attributes`, `range`, `__aiter__` |

[PUBLIC_TYPE_SCOPE]: trace records
- rail: observability

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [ROLE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `obspec_utils.wrappers.RequestTrace` | `@dataclass` | collection of `RequestRecord` with analysis — `add(...)`, `clear()`, `to_dataframe()` (lazy `pandas`), `summary() -> dict`, `total_bytes`, `total_requests`, `requests` |
|  [02]   | `obspec_utils.wrappers.RequestRecord` | `@dataclass` | one range request — `path`, `start`, `length`, `end`, `timestamp`, `duration`, `method` (`get`/`get_range`/`get_ranges`/`head`), `range_style` (`end`/`length`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registry operations
- rail: object-store

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `ObjectStoreRegistry(stores: dict[Url, T] | None = None)` | construct | seed the registry from a `{url: store}` map; each entry is `register`ed |
|  [02]   | `registry.register(url: Url, store: T) -> None` | bind | register/replace a store under a URL; navigates the path-segment tree |
|  [03]   | `registry.resolve(url: Url) -> tuple[T, Path]` | route | longest-prefix match by `(scheme, netloc)` then path segments; strips the store's `prefix`/`url` prefix; raises `ValueError` on no match |
|  [04]   | `async with registry: ...` | lifecycle | `__aenter__`/`__aexit__` open/close every registered store implementing the async-context protocol (e.g. `AiohttpStore` session); non-session stores (`S3Store`) pass through untouched |
|  [05]   | `get_url_key(url: Url) -> UrlKey` | key | parse a URL to its `(scheme, netloc)` key; raises `ValueError` if schemeless |
|  [06]   | `path_segments(path: str) -> Iterator[str]` | split | non-empty path segments (drops empties, unlike `urllib.parse`) |

[ENTRYPOINT_SCOPE]: glob discovery
- rail: object-store

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `glob(store: obspec.List, pattern: str) -> Iterator[str]` | sync paths | match object paths against a glob pattern; yields paths |
|  [02]   | `glob_objects(store: obspec.List, pattern: str) -> Iterator[ObjectMeta]` | sync meta | same match; yields full `ObjectMeta` (`path`/`last_modified`/`size`/`e_tag`/`version`) |
|  [03]   | `glob_async(store: obspec.ListAsync, pattern: str) -> AsyncIterator[str]` | async paths | async mirror over `list_async` |
|  [04]   | `glob_objects_async(store: obspec.ListAsync, pattern: str) -> AsyncIterator[ObjectMeta]` | async meta | async mirror yielding `ObjectMeta` |

`pattern` supports `*` (within a segment), `**` (recursive across segments), `?` (one char), `[abc]`/`[a-z]`/`[!abc]` (class/range/negation). The literal leading prefix is extracted and passed to `store.list(prefix=...)` so listing is server-side narrowed before regex filtering — never a full-bucket scan when the pattern is anchored.

[ENTRYPOINT_SCOPE]: file-handle readers
- rail: object-store

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `BufferedStoreReader(store, path, buffer_size=1 MiB)` | sequential | on-demand `get_range` reads with read-ahead buffer; best for forward streaming with rare back-seeks; `Store = Get+GetRange+Head` |
|  [02]   | `EagerStoreReader(store, path, request_size=12 MiB, file_size=None, max_concurrent_requests=18)` | whole-file | fetches the entire file at construction via concurrent `get_ranges` (Icechunk strategy), serves all reads from RAM; pass `file_size` to skip the `head`; `Store = Get+GetRanges+Head` |
|  [03]   | `BlockStoreReader(store, path, block_size=1 MiB, max_cached_blocks=64)` | sparse | fixed-size blocks fetched via concurrent `get_ranges` with bounded LRU cache (`block_size × max_cached_blocks` memory cap); best for many non-contiguous reads of a large file; `Store = Get+GetRanges+Head` |
|  [04]   | `ParallelStoreReader(store, path, chunk_size=1 MiB, max_cached_chunks=64)` | DEPRECATED | `BlockStoreReader` alias emitting `DeprecationWarning`; removed after v0.12 — do not use |

Every reader implements `ReadableFile` plus `readall()`, `readable()`/`seekable()`/`writable()`, `close()`, and the context-manager protocol, so it is a drop-in seekable handle for `h5py.File(reader)`, `zarr`, or `netCDF4`.

[ENTRYPOINT_SCOPE]: read-path wrappers (each a transparent `ReadableStore`)
- rail: object-store

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `CachingReadableStore(store, max_size=256 MiB)` | LRU cache | caches whole objects in an `obstore.MemoryStore` on first access; thread-safe (`threading.Lock`); serves later `get`/`get_range`/`get_ranges` from cache; `clear_cache()`, `cache_size`, `cached_paths`; picklable (`__reduce__` ships a fresh empty cache for `ProcessPool`/Dask workers); context manager clears on exit; transparent `__getattr__` forwarding of non-cached methods |
|  [02]   | `SplittingReadableStore(store, request_size=12 MiB, max_concurrent_requests=18)` | request splitting | rewrites a large `get`/`get_async` into concurrent `get_ranges` (Icechunk strategy); `head`s the size, splits, concatenates; small files fall through to a single `get`; range methods pass through unchanged |
|  [03]   | `TracingReadableStore(store, trace, *, on_request=None)` | tracing | records every `get`/`get_range`/`get_ranges`/`head` into a `RequestTrace` with auto-timing (records even on exception); optional `on_request(RequestRecord)` callback for live logging/telemetry; transparent delegate |

[ENTRYPOINT_SCOPE]: AiohttpStore operations (sync + async mirror)
- rail: object-store

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `store.get(path, *, options=None)` / `store.get_async(...)` | get | download object; `options` carries `range` (tuple, `OffsetRange`, `SuffixRange`) and conditionals; sync wraps async via `asyncio.run` |
|  [02]   | `store.get_range(path, *, start, end=None, length=None)` / `..._async` | range | HTTP `Range` request (obspec end-exclusive → HTTP inclusive); either `end` or `length` required |
|  [03]   | `store.get_ranges(path, *, starts, ends=None, lengths=None)` / `..._async` | multi-range | concurrent ranges via `asyncio.gather` over the managed session |
|  [04]   | `store.head(path)` / `store.head_async(path)` | head | `HEAD` request parsed into `ObjectMeta` (size from `Content-Length`/`Content-Range`, `e_tag`, `last_modified`) |

## [04]-[IMPLEMENTATION_LAW]

[LAYERED_TOPOLOGY]:
- three-package stack: `obspec` defines the structural protocols (`Get`/`GetRange`/`GetRanges`/`Head`/`List` + async mirrors and the `ObjectMeta`/`GetResult`/`GetOptions` TypedDicts); `obstore` supplies the Rust-backed concrete stores AND the `MemoryStore` that the cache/split wrappers reuse as a temporary buffer; `obspec_utils` adds registry + readers + wrappers + glob + `AiohttpStore` ON TOP, all keyed to the `obspec` protocols so an `obstore` store, an `AiohttpStore`, and a stack of wrappers are mutually substitutable
- annotation law: annotate stores with `obstore.store.ObjectStore` (the concrete handle union) OR a project-pinned `obspec` protocol composition; `ReadableStore` is the broadest read contract but upstream may change it, so a stable consumer pins its own `class Store(Get, GetRange, GetRanges, Head, Protocol)`
- registry resolution: `resolve` keys on `(scheme, netloc)` then walks the path-segment tree for the LONGEST registered prefix; on a hit it strips the store's own `prefix` (obstore stores) or parsed `url` path (`AiohttpStore`) so the returned trailing `Path` is store-relative — register `s3://bucket` → resolve `s3://bucket/a/b` yields `(store, "a/b")`, and a store built with `prefix="data/"` further strips that prefix
- reader selection is a real decision axis, not three flavors of one thing: `EagerStoreReader` when the whole file is needed or random access dominates (RAM-bound); `BlockStoreReader` for sparse partial reads of a large file (memory-capped LRU); `BufferedStoreReader` for forward streaming with rare back-seeks (simplest, single read-ahead buffer)

[WRAPPER_COMPOSITION]:
- wrappers are transparent `ReadableStore`s that wrap a `ReadableStore`, so they nest; the canonical accelerator is `CachingReadableStore(SplittingReadableStore(base))` — a large `get` becomes concurrent `get_ranges`, the concatenated result is cached, and the second access serves from the in-process `MemoryStore` with zero network
- `TracingReadableStore` wraps anywhere in the stack to measure that layer; place it OUTSIDE the cache to count only real network requests, INSIDE to count logical reads
- the cache is per-process and never shared across `ProcessPool`/Dask/Lithops workers (`__reduce__` deliberately ships an empty cache); partition workloads by file so each worker caches a disjoint set, or front a shared external cache for cross-worker reuse
- `SplittingReadableStore` only rewrites `get`/`get_async`; explicit `get_range`/`get_ranges` pass through because the caller already sized them

[INTEGRATION]:
- obstore seam (companion spine): `obstore.store.from_url(...)`/`S3Store(...)`/`GCSStore(...)` build the concrete stores; register them in one `ObjectStoreRegistry` so the egress/catalog/virtual rails resolve heterogeneous asset URLs (`s3://`, `gs://`, `https://`) through a single boundary; the wrappers consume those same stores, and `obstore.MemoryStore` IS the cache substrate — never hand-roll an HTTP client or a cache dict beside obstore
- gridded/field/virtual seam: the readers hand a SEEKABLE FILE HANDLE to libraries that need one rather than an obstore store — `h5py.File(BlockStoreReader(store, path))`, a `netCDF4.Dataset` over `EagerStoreReader`, or a zarr chunk read — so the data gridded (`zarr`/`tensorstore`/`icechunk`), field (`netcdf4`/HDF5), and virtual (`virtualizarr`) rails reach object-store bytes through the same byte-range engine the rest of the package uses
- catalog seam: `glob`/`glob_objects` discover STAC asset hrefs and gridded chunk inventories under a prefix without a full-bucket scan, feeding the catalog asset-href fold and the virtual-reference manifest discovery
- anyio seam: the `*_async` reader/wrapper/store mirrors and the `AiohttpStore` async-context session run under the structured-concurrency rail; the registry async context manager opens/closes session-bearing stores inside one task scope
- observability seam (`structlog`/`opentelemetry`): `TracingReadableStore`'s `on_request(RequestRecord)` callback emits a structured event per range request (path/offset/length/duration/method) into the runtime observability rail; one `RequestRecord` maps to one span attribute set, and `RequestTrace.summary()`/`to_dataframe()` roll the access pattern up for profiling
- tabular/profile seam: `RequestTrace.to_dataframe()` yields a `pandas` frame (lazy import) that flows straight into the data profile/quality rail to grade cloud-read efficiency (request count, byte volume, request-size distribution)
- stamina/ContentIdentity seam: wrap `registry.resolve(url)` + read in a `stamina` retry for transient cloud failures (obstore stores also carry native `RetryConfig`; the wrappers do not, so retry lives at the rail); key the cached/fetched object by the runtime `ContentIdentity` so the egress reuse ledger dedupes across stores
- msgspec/pydantic seam: `obspec.ObjectMeta`/`PutResult`/`ListResult` are TypedDicts; decode them into `msgspec.Struct`/`pydantic` models at the boundary when a typed receipt is needed downstream

[EXCEPTIONS]:
- `ObjectStoreRegistry.resolve` raises `ValueError` when no registered store matches the URL's `(scheme, netloc)` + prefix
- `get_url_key` raises `ValueError` for a schemeless URL
- `BufferedStoreReader`/`BlockStoreReader`/`AiohttpStore` raise `ValueError` for an invalid `whence` (readers) or a range request missing both `end` and `length`
- `AiohttpStore` calls `response.raise_for_status()`, so HTTP errors surface as `aiohttp.ClientResponseError`; the underlying obstore stores raise `obstore.exceptions.*`
- the deprecated flat modules (`obspec_utils.aiohttp`/`obspec`/`cache`/`tracing`/`splitting`) and `ParallelStoreReader` emit `DeprecationWarning`

[RAIL_LAW]:
- Package: `obspec-utils`
- Owns: the multi-store URL registry, file-handle readers over byte-range stores, transparent read-path wrappers (cache/split/trace), object-store glob, and a pure-`aiohttp` `ReadableStore` — all typed on `obspec` protocols and composed with `obstore` stores
- Accept: `obstore` stores (and `AiohttpStore`) registered in one `ObjectStoreRegistry`; readers handed to file-object-consuming libraries (`h5py`/`zarr`/`netCDF4`); `CachingReadableStore(SplittingReadableStore(base))` as the cloud-read accelerator; `TracingReadableStore` + `RequestTrace` into the observability/profile rail; subpackage imports (`obspec_utils.registry`/`readers`/`wrappers`/`protocols`/`stores`)
- Reject: a hand-rolled URL→store dispatch dict, a hand-rolled HTTP object client, or a hand-rolled byte cache when the registry/`AiohttpStore`/`CachingReadableStore` own them; the phantom top-level `from obspec_utils import ObjectStoreRegistry` import; the deprecated flat-module and `ParallelStoreReader` surfaces; a second I/O path when an obstore store already serves the protocol
