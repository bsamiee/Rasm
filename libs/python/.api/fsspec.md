# [PY_BRANCH_API_FSSPEC]

`fsspec` is the branch filesystem substrate beneath `universal-pathlib`: one protocol URL resolves through the registry to one cached `AbstractFileSystem`, and every runtime, data, and compute consumer composes that filesystem instead of constructing per-scheme clients.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fsspec`
- package: `fsspec`
- import: `fsspec`
- owner: `shared`
- rail: transport
- version: `2026.4.0`
- license: `BSD-3-Clause`
- asset: pure Python; backend extras (`s3fs`/`gcsfs`/`adlfs`/`aiohttp`) pull concrete drivers and are never re-admitted by folder overlays
- namespaces: `fsspec`, `fsspec.asyn`, `fsspec.registry`, `fsspec.spec`, `fsspec.mapping`, `fsspec.caching`, `fsspec.generic`
- capability: registry-cached protocol filesystem resolution, sync and async read surfaces, byte-range and multi-range reads, filesystem mutation and transactions, block/read-ahead caches, mapping views, and generic sync primitives

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: filesystem family
- rail: transport

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                                                            |
| :-----: | :-------------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `AbstractFileSystem`        | filesystem    | protocol-agnostic filesystem base; registry-cached by protocol, root, and options |
|  [02]   | `asyn.AsyncFileSystem`      | filesystem    | async filesystem base with `_`-prefixed coroutine mirrors and `open_async`        |
|  [03]   | `spec.AbstractBufferedFile` | file          | block-buffered handle returned by `fs.open`                                       |
|  [04]   | `mapping.FSMap`             | mapping       | mutable key-value mapping view over a filesystem root                             |
|  [05]   | `FSTimeoutError`            | fault         | backend timeout fault raised by async filesystem operations                       |

[PUBLIC_TYPE_SCOPE]: cache and transfer policy
- rail: transport

| [INDEX] | [SYMBOL]                                                                                   | [TYPE_FAMILY] | [RAIL]                                                            |
| :-----: | :----------------------------------------------------------------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `BlockCache` / `BackgroundBlockCache` / `ReadAheadCache` / `MMapCache` / `FirstChunkCache` | cache         | read-cache strategies behind file handles                         |
|  [02]   | filesystem transaction object                                                              | transaction   | `fs.transaction` context manager and explicit start/end lifecycle |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resolution and registry
- rail: transport

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :--------------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `filesystem(protocol, **storage_options) -> AbstractFileSystem`  | resolve        | construct or reuse the cached filesystem instance   |
|  [02]   | `url_to_fs(url, **kwargs) -> (AbstractFileSystem, str)`          | resolve        | split URL into filesystem and backend-relative path |
|  [03]   | `get_filesystem_class(protocol) -> type[AbstractFileSystem]`     | registry       | fetch the implementation class for a protocol       |
|  [04]   | `register_implementation(name, cls, clobber=False, errtxt=None)` | registry       | bind a custom implementation to a protocol          |

[ENTRYPOINT_SCOPE]: read, metadata, mutation, and range access
- rail: transport

| [INDEX] | [SURFACE]                                                                                                  | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :--------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `fs.open(path, mode='rb', block_size=...) -> AbstractBufferedFile`                                         | open           | stream a file through the backend          |
|  [02]   | `fs.cat_file(path, start=, end=)` / `fs.cat(path)`                                                         | bytes read     | whole-file or single-range read            |
|  [03]   | `fs.cat_ranges(paths, starts, ends, max_gap=None, on_error="return")`                                      | range read     | concurrent byte-range batch read           |
|  [04]   | `fs.info(path)` / `fs.exists` / `fs.isfile` / `fs.isdir` / `fs.ls(path, detail=True)`                      | inspect        | metadata and directory enumeration         |
|  [05]   | `fs.put` / `fs.get` / `fs.copy` / `fs.mv` / `fs.rm` / `fs.mkdir`                                           | mutate         | filesystem mutation surface                |
|  [06]   | `await afs._cat_file(...)` / `await afs._open(...)` / `await afs._info(...)` / `await afs.open_async(...)` | async          | coroutine mirror of the filesystem surface |

[ENTRYPOINT_SCOPE]: map, cache, transaction, and generic transfer
- rail: transport

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :--------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `get_mapper(url, check=False, create=False, **kwargs)`     | mapping        | filesystem-backed mutable mapping |
|  [02]   | `fs.transaction` / `start_transaction` / `end_transaction` | transaction    | atomic-write staging              |
|  [03]   | `fsspec.open_files(...)` / `open_local(...)`               | batch open     | multi-file open and local staging |
|  [04]   | `fsspec.generic.rsync(source, destination, ...)`           | transfer       | backend-generic directory sync    |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- resolution law: `url_to_fs(root_url, **storage_options)` or `filesystem(protocol, **storage_options)` is the single filesystem construction boundary; the resulting `AbstractFileSystem` is shared by `UPath.fs`, runtime resource roots, DuckDB scan registration, and compute model-asset reads.
- cache law: cached filesystem identity is a function of protocol, root, and storage options; no consumer creates a second filesystem for an already-resolved root.
- read law: sync methods run under the owning concurrency offload when used from async code; `AsyncFileSystem` `_` methods and `open_async` are the native coroutine path.
- mutation law: writes, `transaction`, `FSMap`, cache strategy, and generic transfer are canonical fsspec surfaces, but folder overlays admit them only when their owner names a direct mutation or mapping rail.

[STACK_LAW]:
- runtime roots read through `UPath` plus this filesystem surface; object-store roots use `obstore` directly and only bridge through `obstore.fsspec` when a downstream library requires a filesystem handle.
- data scan sessions admit only the resolved `UPath.fs` handle passed to `DuckDBPyConnection.register_filesystem(fs)`; every data read slice stays downstream of that handle.
- compute model assets compose `UPath` and fsspec for path resolution only; solver state never grows backend-specific file clients.

[LOCAL_ADMISSION]:
- Accept `UPath(...).fs`, `url_to_fs`, and `filesystem` as the branch construction and registration surfaces.
- Accept `fs.open`, `cat_file`, `cat_ranges`, `info`, and async mirrors where the owning page names a byte/read-range rail.
- Accept `FSMap`, transactions, and generic transfer only behind the owner that declares mutable mapping, atomic write, or bulk sync semantics.
- Reject per-scheme `os`/cloud-client branching, inline credentials, direct construction of cloud extra drivers in folder code, blocking reads on an event loop, and wrapper-renames of filesystem operations.

[RAIL_LAW]:
- Package: `fsspec`
- Owns: branch-wide protocol filesystem resolution, cached `AbstractFileSystem` handles, read/mutation/range/map/transaction surfaces, cache policies, and async filesystem mirrors
- Accept: one resolved filesystem shared by `UPath`, runtime roots, DuckDB scan registration, and compute asset reads
- Reject: duplicated filesystem handles, hand-rolled backend clients, per-folder re-catalogues of package surface, and scheme switches where the registry owns dispatch
