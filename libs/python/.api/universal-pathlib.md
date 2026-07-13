# [PY_RUNTIME_API_UNIVERSAL_PATHLIB]

`universal-pathlib` supplies `UPath`: a `pathlib`-shaped path object over any fsspec backend built on the `pathlib_abc` `JoinablePath`/`ReadablePath`/`WritablePath` protocol stack. Protocol and `storage_options` resolve at construction so local, memory, S3, GCS, HTTP, zip, tar, ftp, sftp, smb, webdav, github, hdfs, and data-uri roots share one path API. A metaclass dispatches `UPath(url)` to the registered subclass for the detected protocol, `upath.registry` owns the protocol-to-class table, `upath.extensions.ProxyUPath` is the backend-agnostic extension base, and `UPath.__get_pydantic_core_schema__` makes any `UPath` a first-class pydantic field. It is the runtime path-object face over the fsspec filesystem surface owned by `libs/python/.api/fsspec.md`, with data-specific scan law carried by `libs/python/data/.api/fsspec.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `universal-pathlib`
- package: `universal-pathlib`
- version: `0.3.10`
- license: MIT
- import: `upath`
- owner: `runtime` (resources), `compute` (`experiments/model` `UPath` model-asset resolution)
- rail: resources
- depends-on: `fsspec` (filesystem surface), `pathlib_abc` (`JoinablePath`/`ReadablePath`/`WritablePath` protocol stack); `pydantic`/`pydantic_core` are optional (schema hook is import-deferred), `s3fs`/`gcsfs`/`adlfs`/`paramiko`/`smbprotocol` are per-protocol extras pulled by the backend filesystem, not by `upath`.
- namespaces: `upath`, `upath.registry`, `upath.extensions`, `upath.types`, `upath.implementations`
- capability: fsspec-backed `pathlib`-shaped path objects with protocol/`storage_options` resolution at construction, metaclass protocol dispatch, a registry of protocol implementations, a `ProxyUPath` extension base, pydantic-core schema integration, and the modern `info`/`walk`/`copy`/`move` `pathlib_abc` surface

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: path family
- rail: resources

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]       | [RAIL]                                                     |
| :-----: | :---------------------- | :------------------ | :--------------------------------------------------------- |
|  [01]   | `UPath`                 | path                | fsspec-backed universal path; protocol-dispatched subclass |
|  [02]   | `extensions.ProxyUPath` | path-extension base | wrap-and-extend base virtually registered against `UPath`  |
|  [03]   | `UnsupportedOperation`  | fault               | unsupported per-backend path operation                     |

[PUBLIC_TYPE_SCOPE]: protocol typing (`upath.types`)
- rail: resources
- The `pathlib_abc` protocol stack and the upath flavour duck-type; downstream code types path-like inputs as `JoinablePathLike` and the flavour as `UPathParser` rather than `str`/`Path`.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [RAIL]                                                    |
| :-----: | :------------------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `{Joinable,Readable,Writable}Path`     | protocol      | `pathlib_abc` capability tiers `UPath` implements         |
|  [02]   | `{Joinable,Readable,Writable}PathLike` | type alias    | `Union[<tier>, SupportsPathLike, str]` input edges        |
|  [03]   | `SupportsPathLike`                     | protocol      | `__vfspath__`- or `__fspath__`-bearing objects            |
|  [04]   | `PathInfo`                             | protocol      | type returned by `UPath.info` (a `UPathInfo`)             |
|  [05]   | `StatResultType`                       | protocol      | `os.stat_result` duck-type from `stat`/`lstat`            |
|  [06]   | `UPathParser`                          | protocol      | path flavour: `split`/`join`/`strip_protocol`/`splitroot` |
|  [07]   | `OnNameCollisionFunc`                  | type alias    | copy-collision callback `(paths) -> (file?, dir?)`        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and resolution; surfaces are `UPath.*` unless qualified
- rail: resources
- `UPath(*args, protocol=None, chain_parser=DEFAULT_CHAIN_PARSER, **storage_options)` detects the protocol from `args[0]` (or `protocol=`), parses inline url `storage_options`, and dispatches to the registered subclass; `args[0]` may be a `str`, a `UPath`, or any `__fspath__`/`__vfspath__` object. `protocol=`/`**storage_options` carry credentials and endpoints with the path value. `scheme=` is deprecated in favour of `protocol=`. The metaclass collapses `UPath(existing_upath)` to a copy.

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :----------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `UPath(*args, protocol=None, **storage_options)` | build          | construct a backend-resolved path (dispatched)          |
|  [02]   | `from_uri(uri, **storage_options)`               | build          | classmethod constructor from a uri                      |
|  [03]   | `cwd()` / `home()`                               | build          | local cwd/home; subclasses raise `UnsupportedOperation` |
|  [04]   | `protocol` / `storage_options` / `path`          | metadata       | resolved protocol, read-only options, backend-rel path  |
|  [05]   | `fs`                                             | filesystem     | cached `AbstractFileSystem` (lazy on first access)      |
|  [06]   | `parser`                                         | flavour        | `UPathParser` flavour (`LazyFlavourDescriptor`)         |
|  [07]   | `info`                                           | metadata       | `PathInfo` wrapping one `fs.info` call                  |
|  [08]   | `as_uri()` / `as_posix()`                        | metadata       | uri / posix-separated string forms                      |

[ENTRYPOINT_SCOPE]: path arithmetic and traversal; surfaces are `UPath.*`
- rail: resources
- Path arithmetic (`/`, `joinpath`, `with_segments`, `with_name`/`with_stem`/`with_suffix`, `parent`/`parents`/`parts`) is backend-agnostic and identical across local and cloud roots. `joinuri` applies urljoin semantics (a fresh protocol short-circuits to a new `UPath`). `glob`/`rglob`/`walk` enumerate via `fs.glob`/`fs.listdir`; `case_sensitive`/`recurse_symlinks` are currently warned-and-ignored.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :---------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `joinpath(*segments)` / `UPath / segment`                   | compose        | child path composition (`with_segments`)      |
|  [02]   | `joinuri(uri)`                                              | compose        | urljoin-semantics child (protocol-aware)      |
|  [03]   | `with_segments(*segments)`                                  | compose        | rebuild preserving protocol/options/cached fs |
|  [04]   | `relative_to(other, *, walk_up=False)` / `is_relative_to`   | compose        | relative view; `walk_up=True` unimplemented   |
|  [05]   | `iterdir()`                                                 | traverse       | directory listing yielding `Self`             |
|  [06]   | `glob(pattern)` / `rglob(pattern)`                          | traverse       | pattern enumeration via `fs.glob`             |
|  [07]   | `walk(top_down=True, on_error=None, follow_symlinks=False)` | traverse       | tree walk yielding `(Self, dirs, files)`      |
|  [08]   | `full_match(pattern)` / `match(pattern)`                    | traverse       | glob-style match against the path             |
|  [09]   | `resolve(strict=False)`                                     | compose        | absolutize + normalize (resolve symlinks)     |

[ENTRYPOINT_SCOPE]: byte access and mutation; surfaces are `UPath.*`
- rail: resources
- `open`/`read_bytes`/`read_text`/`write_bytes`/`write_text` route to `fs.open(self.path, ...)`; `buffering` maps to the fsspec `block_size`, and extra `**fsspec_kwargs` pass through to the backend open. `stat`/`exists`/`is_file`/`is_dir`/`is_symlink` route to `fs.info`/`fs.exists`/`fs.isdir`; `follow_symlinks=False` is warned-and-ignored. `copy`/`copy_into`/`move`/`move_into` are the modern `pathlib_abc` transfer surface (`copy` streams via `_copy_from`, `move` = copy then `fs.rm`).

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :--------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `open(mode='r', buffering=-1, **fsspec_kwargs)`            | access         | open the target on the backend filesystem |
|  [02]   | `read_bytes()` / `read_text(encoding, errors, newline)`    | access         | whole-file read                           |
|  [03]   | `write_bytes(data)` / `write_text(data, ...)`              | mutate         | whole-file write returning byte count     |
|  [04]   | `stat(*, follow_symlinks=True)` / `lstat()`                | metadata       | `UPathStatResult` from `fs.info`          |
|  [05]   | `exists()` / `is_file()` / `is_dir()` / `is_symlink()`     | metadata       | existence/type predicates via `fs`        |
|  [06]   | `mkdir(mode, parents, exist_ok)` / `touch(mode, exist_ok)` | mutate         | directory/file creation                   |
|  [07]   | `unlink(missing_ok=False)` / `rmdir()`                     | mutate         | file/dir removal                          |
|  [08]   | `rename(target)` / `replace(target)`                       | mutate         | rename (`recursive`/`maxdepth` extras)    |
|  [09]   | `copy(target, **kw)` / `copy_into(target_dir, **kw)`       | transfer       | recursive copy (cross-protocol streaming) |
|  [10]   | `move(target, **kw)` / `move_into(target_dir, **kw)`       | transfer       | copy-then-remove move (cross-protocol)    |

[ENTRYPOINT_SCOPE]: registry and extension (`upath.registry`, `upath.extensions`)
- rail: resources
- The registry is a `ChainMap` over `_Registry.known_implementations` (the protocol-to-FQN table); a protocol resolves to a `UPath` subclass lazily, and an unregistered-but-fsspec-known protocol falls back to a dynamically generated `_<Proto>Path` (with a `UserWarning`). `ProxyUPath` wraps a `UPath` in `__wrapped__`, mirrors the full path interface returning `Self`, and is virtually registered (`UPath.register(ProxyUPath)`) so `isinstance(proxy, UPath)` holds; it supersedes the deprecated `_protocol_dispatch = False` subclassing route.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `registry.get_upath_class(protocol, *, fallback=True)` | registry       | protocol → `UPath` subclass (overloaded per literal)    |
|  [02]   | `registry.register_implementation(protocol, cls, ...)` | registry       | register a `UPath` subclass or FQN for a protocol       |
|  [03]   | `registry.available_implementations(...)`              | registry       | registered protocols; `fallback=True` adds fsspec-known |
|  [04]   | `extensions.ProxyUPath(*args, **storage_options)`      | extend         | wrap+extend base; add methods over every backend        |
|  [05]   | `extensions.ProxyUPath._from_upath(upath)`             | extend         | classmethod lift of a `UPath` into the proxy            |

## [04]-[IMPLEMENTATION_LAW]

[RESOURCES_TOPOLOGY]:
- path law: a resource reference is one `UPath` carrying its protocol and `storage_options`; path arithmetic (`/`, `joinpath`, `with_segments`, `glob`, `walk`) is backend-agnostic and the same code serves local, memory, zip, and cloud roots. No per-scheme `os.path`/`Path` branching.
- dispatch law: protocol selection is the `_UPathMeta` metaclass plus `upath.registry`; `UPath(url)` resolves the subclass once at construction. A new backend is one `register_implementation` entry or a `universal_pathlib.implementations` entry-point, never a hand-rolled scheme switch in the consumer.
- resolution law: `UPath.fs` is the cached fsspec `AbstractFileSystem`; the filesystem surface and dispatch law arrive settled from `libs/python/.api/fsspec.md`, with data-specific DuckDB registration captured by `libs/python/data/.api/fsspec.md`. `UPath` is the path face, fsspec is the filesystem face — one backend, two views. The cloud backends (`s3fs`/`gcsfs`) are reached only through their protocol, never instantiated alongside the path.
- option law: `protocol=`/`storage_options` resolution at construction is load-bearing; credentials and endpoints travel with the path value as a read-only `MappingProxyType`, never a separate global filesystem handle. Inline url query parameters fold into `storage_options` via `_parse_storage_options`.
- info law: filesystem metadata is one `UPath.info` (`PathInfo`) backed by a single `fs.info` call, not repeated `exists`/`is_dir`/`stat` round-trips; type predicates and `stat` reuse the same backend info.
- transfer law: cross-root copies use `copy`/`move`/`copy_into`/`move_into`; cross-protocol transfer streams through `_copy_from` (`vfsopen` read/write), never a backend-specific bulk client the consumer must select.

[LOCAL_ADMISSION]:
- `ResourceRef` is a `UPath`; the runtime composes path arithmetic and traversal over it and resolves the filesystem only when bytes are accessed. `libs/python/.api/fsspec.md` owns the `AbstractFileSystem` surface — this page never re-documents filesystem resolution.
- typed path edges admit `JoinablePathLike`/`ReadablePathLike`/`WritablePathLike` from `upath.types`, never bare `str`; the flavour is `UPathParser`, the metadata carrier is `PathInfo`/`StatResultType`.
- a `UPath` is a first-class pydantic field: `UPath.__get_pydantic_core_schema__` validates from a `str` or a `{path, protocol, storage_options}` mapping and serialises back to that mapping, so a settings/config model (`.api/pydantic-settings.md`) carries a `UPath` directly and the artifact-store root deserialises with its `storage_options` intact — no `str`-field-then-reconstruct seam.
- a backend-agnostic capability extension (a method that must work over every protocol) subclasses `ProxyUPath`, not `UPath`; the deprecated `_protocol_dispatch = False` route is never used.
- `UPath` never replaces `pathlib.Path` for purely local, performance-critical paths where no protocol indirection is needed; the registry resolves the empty protocol to the local `PosixUPath`/`WindowsUPath` for the base class.

[RAIL_LAW]:
- Package: `universal-pathlib`
- Owns: fsspec-backed universal path objects with protocol/`storage_options` resolution, metaclass protocol dispatch, the protocol-implementation registry, the `ProxyUPath` extension base, pydantic-core schema integration, and the `pathlib_abc` `info`/`walk`/`copy`/`move` surface
- Accept: `UPath` with `protocol`/`storage_options`, backend-agnostic path arithmetic, `UPath.fs` filesystem access, `UPath.info` single-call metadata, `get_upath_class`/`register_implementation` dispatch, `ProxyUPath` for cross-backend extensions, `UPath` as a pydantic field, typed `JoinablePathLike` edges
- Reject: per-scheme path branching, a separate global filesystem handle alongside the path, repeated `exists`/`is_dir`/`stat` instead of one `info`, the deprecated `_protocol_dispatch = False` subclassing, `scheme=` instead of `protocol=`, a `str` pydantic field reconstructed into a path, `UPath` where plain `Path` suffices
