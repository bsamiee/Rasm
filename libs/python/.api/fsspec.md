# [PY_BRANCH_API_FSSPEC]

`fsspec` is the branch filesystem substrate beneath `universal-pathlib`: one protocol URL resolves through the registry to one cached `AbstractFileSystem`, and every runtime, data, and compute consumer composes that filesystem rather than constructing per-scheme clients.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fsspec`
- package: `fsspec`
- import: `fsspec`
- owner: `shared`
- rail: transport
- license: `BSD-3-Clause`
- asset: pure Python; backend extras (`s3fs`/`gcsfs`/`adlfs`/`aiohttp`) pull concrete drivers
- namespaces: `fsspec`, `fsspec.asyn`, `fsspec.registry`, `fsspec.spec`, `fsspec.mapping`, `fsspec.caching`, `fsspec.generic`
- capability: registry-cached protocol filesystem resolution, sync/async reads, byte-range and multi-range reads, mutation and transactions, block/read-ahead caches, mapping views, generic sync

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: filesystem family

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                                      |
| :-----: | :-------------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `AbstractFileSystem`        | filesystem    | protocol-agnostic filesystem base; registry-cached by protocol, root, and options |
|  [02]   | `asyn.AsyncFileSystem`      | filesystem    | async filesystem base with `_`-prefixed coroutine mirrors and `open_async`        |
|  [03]   | `spec.AbstractBufferedFile` | file          | block-buffered handle returned by `fs.open`                                       |
|  [04]   | `mapping.FSMap`             | mapping       | mutable key-value mapping view over a filesystem root                             |
|  [05]   | `FSTimeoutError`            | fault         | backend timeout fault raised by async filesystem operations                       |

[PUBLIC_TYPE_SCOPE]: read-cache and transfer policy

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :---------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `BlockCache`                  | cache         | fixed-size block LRU over the remote file                          |
|  [02]   | `BackgroundBlockCache`        | cache         | block cache prefetching the next block in a thread                 |
|  [03]   | `ReadAheadCache`              | cache         | contiguous forward read-ahead buffer                               |
|  [04]   | `MMapCache`                   | cache         | memory-mapped local sparse-file cache                              |
|  [05]   | `FirstChunkCache`             | cache         | caches the file's leading block for header reads                   |
|  [06]   | filesystem transaction object | transaction   | `fs.transaction` context manager plus explicit start/end lifecycle |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resolution and registry

| [INDEX] | [SURFACE]                                                        | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `filesystem(protocol, **storage_options) -> AbstractFileSystem`  | factory | construct or reuse the cached filesystem instance   |
|  [02]   | `url_to_fs(url, **kwargs) -> (AbstractFileSystem, str)`          | static  | split URL into filesystem and backend-relative path |
|  [03]   | `get_filesystem_class(protocol) -> type[AbstractFileSystem]`     | static  | fetch the implementation class for a protocol       |
|  [04]   | `register_implementation(name, cls, clobber=False, errtxt=None)` | static  | bind a custom implementation to a protocol          |

[ENTRYPOINT_SCOPE]: read, metadata, mutation, and range access

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `fs.open(path, mode='rb', block_size=...) -> AbstractBufferedFile`              | instance | stream a file through the backend  |
|  [02]   | `fs.cat_file(path, start=, end=)` / `fs.cat(path)`                              | instance | whole-file or single-range read    |
|  [03]   | `fs.cat_ranges(paths, starts, ends, max_gap=None, on_error="return")`           | instance | concurrent byte-range batch read   |
|  [04]   | `fs.info` / `fs.exists` / `fs.isfile` / `fs.isdir` / `fs.ls(path, detail=True)` | instance | metadata and directory enumeration |
|  [05]   | `fs.put` / `fs.get` / `fs.copy` / `fs.mv` / `fs.rm` / `fs.mkdir`                | instance | filesystem mutation surface        |
|  [06]   | `afs._cat_file` / `afs._open` / `afs._info` / `afs.open_async`                  | instance | coroutine mirror of the surface    |

[ENTRYPOINT_SCOPE]: map, cache, transaction, and generic transfer

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `get_mapper(url, check=False, create=False, **kwargs)`     | factory  | filesystem-backed mutable mapping |
|  [02]   | `fs.transaction` / `start_transaction` / `end_transaction` | instance | atomic-write staging              |
|  [03]   | `fsspec.open_files(...)` / `open_local(...)`               | static   | multi-file open and local staging |
|  [04]   | `fsspec.generic.rsync(source, destination, ...)`           | static   | backend-generic directory sync    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- resolution: `url_to_fs(root_url, **storage_options)` or `filesystem(protocol, **storage_options)` is the single filesystem construction boundary; the resulting `AbstractFileSystem` serves every consumer of that root.
- cache: cached filesystem identity is a function of protocol, root, and storage options; no consumer creates a second filesystem for an already-resolved root.
- read: sync methods run under the owning concurrency offload when called from async code; `AsyncFileSystem` `_`-methods and `open_async` are the native coroutine path.

[STACKING]:
- `universal-pathlib`(`libs/python/.api/universal-pathlib.md`): `UPath.fs` caches the `url_to_fs`/`filesystem`-resolved `AbstractFileSystem` per protocol, root, and options; runtime resource roots and compute model-asset reads compose that one cached handle for path resolution, never a backend-specific file client.
- `obstore`(`libs/python/.api/obstore.md`): object-store roots use `obstore` directly and bridge through `obstore.fsspec` only when a downstream library requires a filesystem handle.
- data scan seam: the resolved `UPath.fs` is the whole backend `DuckDBPyConnection.register_filesystem(fs)` receives; every read slice stays downstream of that handle and read caching stays with DuckDB `httpfs`, object-store caching, or the owning chunked-array backend.

[LOCAL_ADMISSION]:
- Accept `UPath(...).fs`, `url_to_fs`, and `filesystem` as the branch construction and registration surfaces.
- Accept `fs.open`, `cat_file`, `cat_ranges`, `info`, and async mirrors where the owning page names a byte/read-range rail.
- Accept `FSMap`, transactions, and generic transfer only behind an owner declaring mutable mapping, atomic write, or bulk sync.
- Data declines these at its boundary: egress atomicity is `obstore` conditional write or a table-format commit, chunk-range reads ride `obstore.get_range`/`get_ranges` or a native reader, mutable chunk stores are `zarr`/`tensorstore`/`icechunk`, and bulk movement is content-keyed `obstore` put.
- Reject per-scheme `os`/cloud-client branching, inline credentials, direct construction of cloud extra drivers in folder code, blocking reads on an event loop, and wrapper-renames of filesystem operations.

[RAIL_LAW]:
- Package: `fsspec`
- Owns: branch-wide protocol filesystem resolution, cached `AbstractFileSystem` handles, read/mutation/range/map/transaction surfaces, cache policies, and async filesystem mirrors
- Accept: one resolved filesystem shared by `UPath`, runtime roots, DuckDB scan registration, and compute asset reads
- Reject: duplicated filesystem handles, hand-rolled backend clients, per-folder re-catalogues of package surface, and scheme switches where the registry owns dispatch
