# [PY_RUNTIME_API_FSSPEC]

`fsspec` is the registry-dispatched filesystem substrate `universal-pathlib`'s `UPath` resolves against: one protocol URL resolves once to a cached `AbstractFileSystem`, and roots' `Transfer` aspect reads through it — the blocking sync surface offloaded to a thread and the `AsyncFileSystem` `_`-coroutine mirror driven natively. The runtime slice is that shared resolution plus the read leg. The transaction, `cat_ranges`, block-cache, `FSMap`, and cross-protocol `rsync` surfaces are the data folder's to mine (`libs/python/data`); this catalog carries only what a runtime fence consumes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fsspec`
- package: `fsspec`
- import: `fsspec`
- owner: `runtime`
- rail: transport
- version: `2026.4.0`
- license: `BSD-3-Clause`
- namespaces: `fsspec`, `fsspec.asyn`, `fsspec.registry`, `fsspec.spec`
- capability: registry-cached protocol->filesystem resolution shared with `UPath`, the read surface (sync offloaded + async `_`-mirror) under roots' `Transfer`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: filesystem family (read slice)
- rail: transport
- one resolved `AbstractFileSystem` is the shared access surface `UPath` composes; the async mirror is the native-read face under `Transfer`.

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `AbstractFileSystem`             | filesystem    | protocol-agnostic filesystem base; registry-cached (`cachable=True`) so one root+options resolves once |
|  [02]   | `asyn.AsyncFileSystem`           | filesystem    | async filesystem base (`_`-prefixed coroutine read mirror + `open_async`) |
|  [03]   | `spec.AbstractBufferedFile`      | file          | block-buffered read handle yielded by `fs.open` (`read`/`seek`/`readinto`) |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: transport

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                            |
| :-----: | :----------------- | :------------ | :-------------------------------- |
|  [01]   | `FSTimeoutError`   | fault         | backend operation timeout (async) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resolution (module-level)
- rail: transport
- one pass yields the cached, registry-shared `AbstractFileSystem` that `UPath` reuses; no per-scheme `os`/`open` branching.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `filesystem(protocol, **storage_options) -> AbstractFileSystem`            | resolve        | filesystem from protocol + options (instance-cached) |
|  [02]   | `url_to_fs(url, **kwargs) -> (AbstractFileSystem, str)`                     | resolve        | filesystem + stripped path from a URL        |
|  [03]   | `get_filesystem_class(protocol) -> type[AbstractFileSystem]`               | resolve        | implementation class for a protocol          |

[ENTRYPOINT_SCOPE]: read surface (on the resolved filesystem)
- rail: transport
- the sync methods run under an anyio thread offload inside `Transfer`; `AsyncFileSystem` mirrors each as a `_`-coroutine for the native-async leg.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `fs.open(path, mode='rb', block_size=) -> AbstractBufferedFile`            | open           | open one file for reading (block-buffered)   |
|  [02]   | `fs.cat_file(path, start=, end=)` / `fs.cat(path)`                         | bytes read     | whole-file / single byte-range read into memory |
|  [03]   | `fs.info(path)` / `fs.exists` / `fs.isfile` / `fs.isdir` / `fs.ls(path, detail=True)` | inspect | metadata + single-level enumeration for a read plan |
|  [04]   | `await afs._cat_file(path, start=, end=)` / `await afs._open(...)` / `await afs._info(...)` | async read | coroutine mirror of the read methods under `Transfer` |
|  [05]   | `await afs.open_async(path, mode='rb') -> AbstractAsyncStreamedFile`       | async open     | async streamed read handle                   |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- resolution law: a resource root is a protocol URL resolved once through `url_to_fs`/`filesystem` with settings-model `storage_options`; the resolved `AbstractFileSystem` is registry-cached, and `UPath` shares that exact instance — resolution never forks a second filesystem for the same root+options.
- read law: roots' `Transfer` aspect reads through the resolved filesystem — the blocking sync surface (`cat_file`/`open`/`info`) offloaded via the anyio thread band (never blocking the event loop), or the `AsyncFileSystem` `_`-coroutine mirror when the backend is natively async. This page owns the read leg only.
- scope law: writes, `with fs.transaction`, `cat_ranges` batched multi-range reads, named block-cache strategies, compression codecs, `FSMap`/`get_mapper` key-value views, `GenericFileSystem`/`rsync`, and instance serialization are the data folder's slice (`libs/python/data`) — a runtime fence never consumes them, so this catalog never carries them.

[LOCAL_ADMISSION]:
- `universal-pathlib` `UPath` is the path-object face over the same fsspec filesystem: that page owns the path surface, this page owns the filesystem-resolution + read surface, and the two share one resolved `AbstractFileSystem` via the registry cache (`roots.md:68`).
- Object-store roots do NOT route through fsspec cloud backends: `obstore` is the runtime object-store transport (`roots.md:207,248`), and `UPath`+fsspec covers file/memory/local resolution — there is no `s3fs`/`gcsfs` backend admission in runtime.
- `storage_options` originate in the `pydantic-settings` settings model, never inline literals; secrets never appear in a resolution call.

[STACK_LAW]:
- `url_to_fs(root_url, **storage_options)` -> the resolved `AbstractFileSystem` backs `UPath(root, protocol=scheme)` (`roots.md:68`) -> roots' `Transfer` reads through `fs.cat_file`/`_cat_file` under the anyio thread band (`roots.md:124-125`): one resolution, one shared filesystem, no per-scheme client branching.
- consumers: roots' `Transfer` read leg (runtime) and compute's `UPath` model-asset resolution (`experiments/model.md`, compute campaign) both consume this shared resolution — the re-scope holds the read slice for both and drops nothing either needs.

[RAIL_LAW]:
- Package: `fsspec`
- Owns: registry-cached protocol->filesystem resolution shared with `UPath`, and the read surface (sync offloaded + async `_`-mirror) under roots' `Transfer`
- Accept: `url_to_fs`/`filesystem` resolution with settings-model `storage_options`, the shared registry-cached `AbstractFileSystem`, `fs.cat_file`/`open`/`info`/`ls` reads offloaded via anyio, the `AsyncFileSystem` `_`-coroutine read mirror and `open_async`
- Reject: per-scheme `os`/client branching, a second filesystem for an already-resolved root, blocking reads on the event loop, inline `storage_options` secrets, `s3fs`/`gcsfs` cloud-backend admission (obstore owns object stores), and cataloguing the write/transaction/`cat_ranges`/block-cache/`FSMap`/`rsync` surfaces the data folder owns
