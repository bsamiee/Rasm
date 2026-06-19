# [PY_RUNTIME_API_FSSPEC]

`fsspec` supplies the protocol-agnostic filesystem abstraction: a registry-dispatched `AbstractFileSystem`, URL-to-filesystem resolution, mutable-mapping `FSMap` views, transparent caching/compression layers, async filesystem support, and progress callbacks. It is the runtime owner for resource-root dispatch across local, memory, and cloud backends.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fsspec`
- package: `fsspec`
- import: `fsspec`
- owner: `runtime`
- rail: resources
- namespaces: `fsspec`, `fsspec.asyn`, `fsspec.caching`, `fsspec.registry`, `fsspec.callbacks`
- capability: protocol-dispatched filesystems, URL resolution, mapper views, caching/compression, async filesystems, progress callbacks

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: filesystem family
- rail: resources

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                |
| :-----: | :--------------------- | :------------ | :------------------------------------ |
|   [1]   | `AbstractFileSystem`   | filesystem    | protocol-agnostic filesystem base     |
|   [2]   | `asyn.AsyncFileSystem` | filesystem    | async filesystem base                 |
|   [3]   | `FSMap`                | mapping       | dict-like key/value over a filesystem |
|   [4]   | `Callback`             | callback      | transfer-progress hook                |
|   [5]   | `caching.BaseCache`    | cache         | block-cache strategy base             |
|   [6]   | `registry`             | registry      | protocol→implementation map           |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: resources

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [RAIL]                    |
| :-----: | :--------------- | :------------ | :------------------------ |
|   [1]   | `FSTimeoutError` | fault         | backend operation timeout |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resolution and access operations
- rail: resources

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :------------------------ | :------------- | :--------------------------------- |
|   [1]   | `filesystem`              | resolve        | filesystem from protocol + options |
|   [2]   | `url_to_fs`               | resolve        | filesystem + path from a URL       |
|   [3]   | `get_fs_token_paths`      | resolve        | filesystem, cache token, and paths |
|   [4]   | `get_filesystem_class`    | resolve        | implementation class for protocol  |
|   [5]   | `open`                    | access         | open a single file                 |
|   [6]   | `open_files`              | access         | open a glob of files               |
|   [7]   | `open_local`              | access         | materialise remote to a local path |
|   [8]   | `get_mapper`              | access         | `FSMap` view over a root           |
|   [9]   | `register_implementation` | registry       | register a protocol backend        |
|  [10]   | `available_protocols`     | registry       | list registered protocols          |
|  [11]   | `available_compressions`  | registry       | list compression codecs            |

## [4]-[IMPLEMENTATION_LAW]

[RESOURCES_TOPOLOGY]:
- dispatch law: a resource root is a protocol URL resolved once through `url_to_fs`/`filesystem` with `storage_options`; the resolved `AbstractFileSystem` is the single access surface — no `os`/`open` branching by scheme.
- backend law: cloud backends (`s3fs`, `gcsfs`) register through the fsspec registry and are reached by protocol, never instantiated directly; a new backend is one registry entry.
- mapping law: key/value resource access uses `get_mapper`/`FSMap`; the content-identity owner reads bytes through the mapper, not a backend-specific client.
- caching law: read amplification is controlled by the fsspec block cache and compression codecs declared at open time, never a hand-rolled local cache.
- async law: concurrent resource I/O uses the `AsyncFileSystem` surface under the anyio lane, never blocking calls on the event loop.
- progress law: transfer progress flows through one `Callback` feeding the receipt surface, never ad hoc print/log.

[LOCAL_ADMISSION]:
- `ResourceRoot`/`ResourceRef` admit a protocol + `storage_options`; the runtime resolves filesystems through fsspec and owns no per-scheme client.
- `universal-pathlib` `UPath` is the path-object face over the same fsspec filesystem; this page owns the filesystem surface, that page owns the path surface.

[RAIL_LAW]:
- Package: `fsspec`
- Owns: protocol-dispatched filesystem resolution, file/mapper access, caching/compression, async filesystems, and progress callbacks
- Accept: `url_to_fs`/`filesystem` resolution, registry backends, `FSMap` mappers, block caching, `AsyncFileSystem`, `Callback` progress
- Reject: per-scheme `os`/client branching, direct cloud-client instantiation, hand-rolled caches, blocking I/O on the loop
