# [PY_RUNTIME_API_FSSPEC]

`fsspec` supplies the protocol-agnostic filesystem abstraction: a registry-dispatched `AbstractFileSystem` with a full POSIX-shaped operation surface, an `AsyncFileSystem` async mirror, URL-to-filesystem resolution, mutable-mapping `FSMap` views, a pluggable block-cache strategy family, a compression-codec registry, transactional writes, batch byte ops (`cat_ranges`/`pipe`), a cross-protocol `GenericFileSystem`/`rsync`, instance serialization, and progress callbacks with concrete subclasses. It is the runtime owner for resource-root dispatch across local, memory, and cloud backends.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fsspec`
- package: `fsspec`
- import: `fsspec`
- owner: `runtime`
- rail: resources
- version: `2026.6.0` (CalVer; the package versions on a date scheme, so the floor is "newest stable", never a SemVer pin)
- license: `BSD-3-Clause`
- floor: `Requires-Python>=3.10`; wheel `py3-none-any` (pure-Python, no ABI pin); the core has zero hard runtime deps — backends and features are extras (`s3` -> `s3fs`, `gcs`/`gs` -> `gcsfs`, `abfs` -> `adlfs`, `http` -> `aiohttp`, `sftp`/`ssh` -> `paramiko`, `smb` -> `smbprotocol`, `git`/`github`, `dropbox`, `arrow`/`hdfs` -> `pyarrow`, `libarchive`, `tqdm` for progress)
- namespaces: `fsspec`, `fsspec.asyn`, `fsspec.caching`, `fsspec.registry`, `fsspec.callbacks`, `fsspec.core`, `fsspec.generic`, `fsspec.compression`, `fsspec.transaction`, `fsspec.exceptions`
- capability: protocol-dispatched filesystems, URL resolution, mapper views, block caching, compression codecs, transactional writes, batch byte ranges, cross-protocol copy/rsync, async filesystems, instance JSON round-trip, progress callbacks

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: filesystem family
- rail: resources

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------ |
|  [01]   | `AbstractFileSystem`      | filesystem    | protocol-agnostic filesystem base (POSIX-shaped sync surface) |
|  [02]   | `asyn.AsyncFileSystem`    | filesystem    | async filesystem base (`_`-prefixed coroutine mirror + `open_async`) |
|  [03]   | `generic.GenericFileSystem` | filesystem  | cross-protocol filesystem (copy/move between two backends) |
|  [04]   | `FSMap`                   | mapping       | dict-like key/value over a filesystem (`getitems`/`setitems` batch) |
|  [05]   | `spec.AbstractBufferedFile` | file        | block-buffered file object (`read`/`write`/`seek`/`readinto`/`readuntil`) |
|  [06]   | `asyn.AbstractAsyncStreamedFile` | file   | async file object yielded by `open_async`               |

[PUBLIC_TYPE_SCOPE]: cache, callback, and transaction family
- rail: resources
- block-cache strategies are selected by `cache_type` (their registry name) at open time; `Callback` subclasses feed transfer progress to the receipt surface; `Transaction` groups deferred writes.

| [INDEX] | [SYMBOL]                                                                | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :---------------------------------------------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `caching.BaseCache`                                                     | cache base    | block-cache strategy base                       |
|  [02]   | `caching.BlockCache` / `caching.BackgroundBlockCache`                   | cache         | fixed-block LRU caching (sync / background-prefetch) |
|  [03]   | `caching.MMapCache`                                                     | cache         | memory-mapped on-disk block cache               |
|  [04]   | `caching.ReadAheadCache` / `caching.FirstChunkCache` / `caching.BytesCache` / `caching.AllBytes` / `caching.KnownPartsOfAFile` | cache | streaming / first-chunk / whole-file / known-range strategies |
|  [05]   | `caching.register_cache(cls, clobber=)` / `caching.caches`             | cache registry | register / look up a named cache strategy       |
|  [06]   | `Callback` / `callbacks.NoOpCallback` / `callbacks.DEFAULT_CALLBACK` / `callbacks.TqdmCallback` / `callbacks.DotPrinterCallback` | callback | transfer-progress hook (no-op default; tqdm bar; dot printer) |
|  [07]   | `transaction.Transaction`                                              | transaction   | deferred-write grouping committed on `__exit__` |
|  [08]   | `registry`                                                            | registry      | protocol->implementation map                    |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: resources

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                              |
| :-----: | :-------------------------------- | :------------ | :---------------------------------- |
|  [01]   | `FSTimeoutError`                  | fault         | backend operation timeout (async)   |
|  [02]   | `exceptions.BlocksizeMismatchError` | fault       | cached block size disagrees with reopen |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resolution and open (module-level)
- rail: resources
- one resolution pass yields the cached, registry-shared `AbstractFileSystem`; `open`/`open_files` return lazy `OpenFile`/`OpenFiles` that materialize on context entry and layer compression/caching at that point.

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `filesystem(protocol, **storage_options) -> AbstractFileSystem`                            | resolve        | filesystem from protocol + options (instance-cached) |
|  [02]   | `url_to_fs(url, **kwargs) -> (AbstractFileSystem, str)`                                     | resolve        | filesystem + stripped path from a URL        |
|  [03]   | `get_fs_token_paths(urlpath, mode='rb', num=1, name_function=, storage_options=, protocol=, expand=True) -> (fs, token, paths)` | resolve | filesystem, cache token, and expanded paths |
|  [04]   | `get_filesystem_class(protocol) -> type[AbstractFileSystem]`                                | resolve        | implementation class for a protocol          |
|  [05]   | `open(urlpath, mode='rb', compression=None, encoding=, protocol=, **kwargs) -> OpenFile`   | access         | open a single file (lazy; compression-layered) |
|  [06]   | `open_files(urlpath, mode='rb', compression=, num=1, auto_mkdir=True, expand=True, **kwargs) -> OpenFiles` | access | open a glob/list of files                  |
|  [07]   | `open_local(url, mode='rb', **storage_options) -> str \| list[str]`                        | access         | materialize remote file(s) to a local path   |
|  [08]   | `get_mapper(url='', check=False, create=False, alternate_root=None, **kwargs) -> FSMap`    | access         | `FSMap` mutable-mapping view over a root      |

[ENTRYPOINT_SCOPE]: filesystem operations (on `AbstractFileSystem`)
- rail: resources
- defined on `AbstractFileSystem` (PUBLIC_TYPES filesystem [01]); `AsyncFileSystem` mirrors each as a `_`-prefixed coroutine (`_cat`/`_get`/`_ls`/...).

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `fs.open(path, mode='rb', block_size=, cache_options=, compression=) -> AbstractBufferedFile` | open        | open one file (cache strategy via `cache_options['cache_type']`) |
|  [02]   | `fs.cat(path, recursive=False, on_error='raise')` / `fs.cat_file(path, start=, end=)`      | bytes read     | whole-file / byte-range read into memory     |
|  [03]   | `fs.cat_ranges(paths, starts, ends, max_gap=, on_error='return')`                          | bytes read     | batched multi-range read (gap-coalesced)     |
|  [04]   | `fs.pipe(path, value)` / `fs.pipe_file(path, value, mode='overwrite')` / `fs.write_bytes` / `fs.write_text` | bytes write | write bytes/text directly                    |
|  [05]   | `fs.get(rpath, lpath, recursive=, callback=, maxdepth=)` / `fs.put(lpath, rpath, ...)`     | transfer       | download / upload (recursive, callback-tracked) |
|  [06]   | `fs.copy` / `fs.cp` / `fs.mv` / `fs.rm(path, recursive=, maxdepth=)` / `fs.rm_file`        | mutate         | server-side copy/move/delete                 |
|  [07]   | `fs.ls(path, detail=True)` / `fs.find` / `fs.glob(path, maxdepth=)` / `fs.walk(path, topdown=)` / `fs.du` | traverse | enumerate / glob / recurse / size           |
|  [08]   | `fs.info` / `fs.exists` / `fs.isfile` / `fs.isdir` / `fs.checksum` / `fs.modified` / `fs.created` / `fs.head` / `fs.tail` / `fs.sign` | inspect | metadata + signed-URL minting       |
|  [09]   | `fs.mkdir(path, create_parents=)` / `fs.makedirs(path, exist_ok=)` / `fs.rmdir` / `fs.touch` | dir ops      | directory lifecycle                          |
|  [10]   | `fs.transaction` / `fs.start_transaction()` / `fs.end_transaction()`                       | transaction    | group deferred writes (commit on context exit) |
|  [11]   | `fs.get_mapper(root='', check=, create=)` / `fs.invalidate_cache(path=)` / `fs.clear_instance_cache()` | access | mapper view / dircache control               |
|  [12]   | `fs.to_dict(include_password=)` / `fs.from_dict(dct)` / `fs.to_json()` / `fs.from_json(blob)` | serialize    | round-trip a filesystem instance for offload/transport |

[ENTRYPOINT_SCOPE]: async, registry, cross-protocol, and codecs
- rail: resources

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `await afs._cat(path, recursive=, batch_size=)` / `await afs._get(...)` / `await afs._ls(...)` / `await afs._pipe(...)` | async | coroutine mirror of every sync op (`batch_size` bounds concurrency) |
|  [02]   | `await afs.open_async(path, mode='rb') -> AbstractAsyncStreamedFile`                        | async open     | async streamed file handle                   |
|  [03]   | `register_implementation(name, cls, clobber=False, errtxt=None)`                           | registry       | register a protocol backend                  |
|  [04]   | `available_protocols()` / `registry.known_implementations`                                 | registry       | list registered / known-by-spec protocols    |
|  [05]   | `available_compressions()` / `compression.register_compression(name, callback, extensions=)` | codec        | list / register a compression codec (`gzip`/`bz2`/`lzma`/`xz`/`zstd`/`zip`) |
|  [06]   | `generic.rsync(source, destination, delete_missing=, source_field='size', update_cond='different', fs=)` | sync | cross-protocol differential sync via `GenericFileSystem` |

## [04]-[IMPLEMENTATION_LAW]

[RESOURCES_TOPOLOGY]:
- dispatch law: a resource root is a protocol URL resolved once through `url_to_fs`/`filesystem` with `storage_options`; the resolved `AbstractFileSystem` is the single access surface — no `os`/`open` branching by scheme. Instances are registry-cached (`cachable=True`), so repeated resolution of the same root+options reuses one filesystem.
- backend law: cloud backends (`s3fs`, `gcsfs`, `adlfs`) register through the fsspec registry (declared as extras) and are reached by protocol, never instantiated directly; a new backend is one `register_implementation` entry.
- mapping law: key/value resource access uses `get_mapper`/`FSMap`; the content-identity owner reads bytes through the mapper (`getitems`/`setitems` for batch), not a backend-specific client.
- caching law: read amplification is controlled by a named fsspec block-cache strategy passed as `cache_options={'cache_type': 'mmap'|'background'|'readahead'|...}` at open time, plus compression codecs declared via `compression=`; never a hand-rolled local cache. `BackgroundBlockCache` prefetches the next block off-thread for sequential scans; `MMapCache` persists blocks to disk for re-open.
- batch law: multi-range reads use `cat_ranges` (gap-coalesced, one round trip) rather than a per-range `cat_file` loop; the `AsyncFileSystem` mirror bounds fan-out with `batch_size`.
- transaction law: grouped writes use `with fs.transaction:`; partial writes are not committed until context exit, so a failure mid-batch leaves no half-written objects.
- async law: concurrent resource I/O uses the `AsyncFileSystem` `_`-coroutine surface under the anyio lane, never blocking calls on the event loop; `open_async` yields an `AbstractAsyncStreamedFile`.
- progress law: transfer progress flows through one `Callback` (`TqdmCallback` for interactive, a custom subclass feeding the receipt surface for headless), passed as `callback=` to `get`/`put`; never ad hoc print/log.
- serialization law: moving a filesystem instance across an offload boundary uses `to_dict`/`from_dict` (or `to_json`/`from_json`), preserving `storage_options`; `include_password=False` strips secrets from the serialized form.

[LOCAL_ADMISSION]:
- `ResourceRoot`/`ResourceRef` admit a protocol + `storage_options`; the runtime resolves filesystems through fsspec and owns no per-scheme client.
- `universal-pathlib` `UPath` is the path-object face over the same fsspec filesystem; this page owns the filesystem surface, that page owns the path surface. The two share one resolved `AbstractFileSystem` instance via the registry cache.
- Cloud-backend extras (`s3fs`/`gcsfs`/`adlfs`) are admitted as fsspec registry backends, not as parallel SDK clients; `GenericFileSystem`/`rsync` is the cross-protocol copy owner (e.g. `s3://` -> `gcs://`), not a hand-rolled download-then-upload.

[STACK_LAW]:
- `url_to_fs(root_url, **storage_options)` -> `fs.cat_ranges(paths, starts, ends)` batched read -> bytes feed the `msgspec`/content-identity decode owner: one resolution, one batched I/O round trip, no per-scheme client branching. `storage_options` originate in the `pydantic-settings` settings model, not inline literals.
- `with fs.transaction:` wraps a `pipe_file`/`put` batch so a `BoundaryFault` mid-write rolls the whole group back; the `stamina.retry_context` owner retries the transaction on a transient `FSTimeoutError`, while the OTel span reads the `Callback` byte totals at commit.
- `Callback` (a receipt-feeding subclass) passed to `fs.get(..., callback=cb)` is the single seam where transfer-progress facts are emitted; `TqdmCallback` is swapped in only at an interactive boundary, never inside the headless rail.

[RAIL_LAW]:
- Package: `fsspec`
- Owns: protocol-dispatched filesystem resolution, the full POSIX-shaped operation surface, file/mapper access, the block-cache strategy family, compression codecs, transactional writes, batch byte-range reads, cross-protocol copy/rsync, async filesystems, instance serialization, and progress callbacks
- Accept: `url_to_fs`/`filesystem` resolution with settings-model `storage_options`, registry backends, `FSMap` mappers with batch `getitems`/`setitems`, named block-cache strategies via `cache_options`, declared `compression=` codecs, `cat_ranges` batched reads, `with fs.transaction`, the `AsyncFileSystem` `_`-coroutine mirror, `Callback` progress, `GenericFileSystem`/`rsync` cross-protocol copy, `to_dict`/`from_dict` offload serialization
- Reject: per-scheme `os`/client branching, direct cloud-SDK instantiation, hand-rolled caches or per-range `cat_file` loops, blocking I/O on the loop, ad hoc progress print/log, inline `storage_options` secrets, download-then-upload instead of `rsync`/`GenericFileSystem`
