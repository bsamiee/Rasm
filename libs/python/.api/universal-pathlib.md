# [PY_BRANCH_API_UNIVERSAL_PATHLIB]

`universal-pathlib` owns `UPath`, the `pathlib`-shaped path face over the `fsspec` filesystem surface on the `pathlib_abc` protocol stack. Protocol and `storage_options` resolve at construction, so one arithmetic/traversal/byte-access API serves every fsspec root from local through cloud, and the runtime resources rail composes it as its reference type.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `universal-pathlib`
- package: `universal-pathlib`
- import: `upath`
- owner: `runtime` (resources), `compute` (`experiments/model` asset resolution)
- rail: resources
- license: `MIT`
- depends: `fsspec` (filesystem surface), `pathlib_abc` (`Joinable`/`Readable`/`WritablePath` protocol stack); `pydantic`/`pydantic_core` optional via the import-deferred schema hook; `s3fs`/`gcsfs`/`adlfs`/`paramiko`/`smbprotocol` per-protocol extras the backend filesystem pulls, never `upath`
- namespaces: `upath`, `upath.registry`, `upath.extensions`, `upath.types`, `upath.implementations`
- capability: fsspec-backed `pathlib`-shaped paths resolving protocol/`storage_options` at construction, metaclass protocol dispatch, the protocol-implementation registry, a `ProxyUPath` extension base, pydantic-core schema integration, and the `pathlib_abc` `info`/`walk`/`copy`/`move` surface

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: path family

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]       | [CAPABILITY]                                               |
| :-----: | :---------------------- | :------------------ | :--------------------------------------------------------- |
|  [01]   | `UPath`                 | path                | fsspec-backed universal path; protocol-dispatched subclass |
|  [02]   | `extensions.ProxyUPath` | path-extension base | wrap-and-extend base virtually registered against `UPath`  |
|  [03]   | `UnsupportedOperation`  | fault               | unsupported per-backend path operation                     |

[PUBLIC_TYPE_SCOPE]: protocol typing (`upath.types`)

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                              |
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

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `UPath(*args, protocol=None, **storage_options)` | ctor     | construct a backend-resolved path (dispatched)          |
|  [02]   | `from_uri(uri, **storage_options)`               | factory  | classmethod constructor from a uri                      |
|  [03]   | `cwd()` / `home()`                               | factory  | local cwd/home; subclasses raise `UnsupportedOperation` |
|  [04]   | `protocol` / `storage_options` / `path`          | property | resolved protocol, read-only options, backend-rel path  |
|  [05]   | `fs`                                             | property | cached `AbstractFileSystem` (lazy on first access)      |
|  [06]   | `parser`                                         | property | `UPathParser` flavour (`LazyFlavourDescriptor`)         |
|  [07]   | `info`                                           | property | `PathInfo` wrapping one `fs.info` call                  |
|  [08]   | `as_uri()` / `as_posix()`                        | instance | uri / posix-separated string forms                      |

- `UPath(...)`: `args[0]` accepts a `str`, a `UPath`, or any `__fspath__`/`__vfspath__` object, and an existing `UPath` collapses to a copy.

[ENTRYPOINT_SCOPE]: path arithmetic and traversal

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `joinpath(*segments)` / `UPath / segment`                   | instance | child path composition (`with_segments`)      |
|  [02]   | `joinuri(uri)`                                              | instance | urljoin-semantics child (protocol-aware)      |
|  [03]   | `with_segments(*segments)`                                  | instance | rebuild preserving protocol/options/cached fs |
|  [04]   | `relative_to(other, *, walk_up=False)` / `is_relative_to`   | instance | relative view; `walk_up=True` unimplemented   |
|  [05]   | `iterdir()`                                                 | instance | directory listing yielding `Self`             |
|  [06]   | `glob(pattern)` / `rglob(pattern)`                          | instance | pattern enumeration via `fs.glob`             |
|  [07]   | `walk(top_down=True, on_error=None, follow_symlinks=False)` | instance | tree walk yielding `(Self, dirs, files)`      |
|  [08]   | `full_match(pattern)` / `match(pattern)`                    | instance | glob-style match against the path             |
|  [09]   | `resolve(strict=False)`                                     | instance | absolutize + normalize (resolve symlinks)     |

- `glob`/`rglob`/`walk`: `case_sensitive`/`recurse_symlinks` are warned-and-ignored.

[ENTRYPOINT_SCOPE]: byte access and mutation

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `open(mode='r', buffering=-1, **fsspec_kwargs)`            | instance | open the target on the backend filesystem |
|  [02]   | `read_bytes()` / `read_text(encoding, errors, newline)`    | instance | whole-file read                           |
|  [03]   | `write_bytes(data)` / `write_text(data, ...)`              | instance | whole-file write returning byte count     |
|  [04]   | `stat(*, follow_symlinks=True)` / `lstat()`                | instance | `UPathStatResult` from `fs.info`          |
|  [05]   | `exists()` / `is_file()` / `is_dir()` / `is_symlink()`     | instance | existence/type predicates via `fs`        |
|  [06]   | `mkdir(mode, parents, exist_ok)` / `touch(mode, exist_ok)` | instance | directory/file creation                   |
|  [07]   | `unlink(missing_ok=False)` / `rmdir()`                     | instance | file/dir removal                          |
|  [08]   | `rename(target)` / `replace(target)`                       | instance | rename (`recursive`/`maxdepth` extras)    |
|  [09]   | `copy(target, **kw)` / `copy_into(target_dir, **kw)`       | instance | recursive copy (cross-protocol streaming) |
|  [10]   | `move(target, **kw)` / `move_into(target_dir, **kw)`       | instance | copy-then-remove move (cross-protocol)    |

- `open`: `buffering` maps to the fsspec `block_size`, and `**fsspec_kwargs` pass through to the backend open.
- `stat`/`is_symlink`: `follow_symlinks=False` is warned-and-ignored.

[ENTRYPOINT_SCOPE]: registry and extension (`upath.registry`, `upath.extensions`)

| [INDEX] | [SURFACE]                                              | [SHAPE] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------- | :------ | :------------------------------------------------------ |
|  [01]   | `registry.get_upath_class(protocol, *, fallback=True)` | static  | protocol → `UPath` subclass (overloaded per literal)    |
|  [02]   | `registry.register_implementation(protocol, cls, ...)` | static  | register a `UPath` subclass or FQN for a protocol       |
|  [03]   | `registry.available_implementations(...)`              | static  | registered protocols; `fallback=True` adds fsspec-known |
|  [04]   | `extensions.ProxyUPath(*args, **storage_options)`      | ctor    | wrap+extend base; add methods over every backend        |
|  [05]   | `extensions.ProxyUPath._from_upath(upath)`             | factory | classmethod lift of a `UPath` into the proxy            |

- `registry.get_upath_class`: an unregistered but fsspec-known protocol falls back to a generated `_<Proto>Path` with a `UserWarning`.
- `extensions.ProxyUPath`: wraps its `UPath` in `__wrapped__`, mirrors the path interface returning `Self`, and is virtually registered so `isinstance(proxy, UPath)` holds.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- path law: a resource reference is one `UPath` carrying its protocol and `storage_options`; arithmetic and traversal are backend-agnostic, so one code path serves local, memory, zip, and cloud roots with no per-scheme `os.path`/`Path` branching.
- dispatch law: the `_UPathMeta` metaclass resolves the subclass once at construction through `upath.registry`; a new backend is one `register_implementation` call or a `universal_pathlib.implementations` entry-point, never a consumer scheme switch.
- resolution law: `UPath.fs` caches the fsspec `AbstractFileSystem` — `UPath` the path face, fsspec the filesystem face over one backend; cloud drivers (`s3fs`/`gcsfs`) are reached only through their protocol.
- option law: credentials and endpoints travel with the path value as a read-only `MappingProxyType`, never a separate global filesystem handle; inline url query parameters fold into `storage_options` via `_parse_storage_options`.
- info law: filesystem metadata is one `UPath.info` (`PathInfo`) over a single `fs.info` call, and type predicates and `stat` reuse that info rather than repeating `exists`/`is_dir`/`stat` round-trips.
- transfer law: cross-root movement uses `copy`/`move`/`copy_into`/`move_into`, and cross-protocol transfer streams through `_copy_from` (`vfsopen` read/write), never a backend-specific bulk client.

[STACKING]:
- `fsspec`(`libs/python/.api/fsspec.md`): `UPath.fs` is the cached `AbstractFileSystem` this catalog's dispatch resolves, and a data-folder DuckDB scan registers over that same filesystem.
- `pydantic`(`libs/python/.api/pydantic.md`): `UPath.__get_pydantic_core_schema__` validates from a `str` or a `{path, protocol, storage_options}` mapping and serialises back to it, so a config model carries a `UPath` field that round-trips with its `storage_options` intact.

[LOCAL_ADMISSION]:
- `ResourceRef` is a `UPath`; the runtime threads arithmetic and traversal over it and resolves the filesystem only when bytes are accessed.
- typed path edges admit `JoinablePathLike`/`ReadablePathLike`/`WritablePathLike` from `upath.types`, the flavour is `UPathParser`, and the metadata carrier is `PathInfo`/`StatResultType`.
- a capability that must work over every protocol subclasses `ProxyUPath`, inheriting the wrapped dispatch rather than reimplementing it.
- `PosixUPath`/`WindowsUPath` answer the empty protocol and own purely local performance-critical paths where protocol indirection buys nothing.

[RAIL_LAW]:
- Package: `universal-pathlib`
- Owns: fsspec-backed universal path objects — protocol/`storage_options` resolution at construction, metaclass protocol dispatch, the protocol-implementation registry, the `ProxyUPath` extension base, pydantic-core schema integration, and the `pathlib_abc` `info`/`walk`/`copy`/`move` surface
- Accept: `UPath` with `protocol`/`storage_options`, backend-agnostic arithmetic, `UPath.fs` access, single-call `UPath.info`, `get_upath_class`/`register_implementation` dispatch, `ProxyUPath` cross-backend extension, `UPath` as a pydantic field, typed `JoinablePathLike` edges
- Reject: per-scheme path branching, a global filesystem handle held beside the path, repeated `exists`/`is_dir`/`stat` in place of one `info`, a `str` pydantic field reconstructed into a path, `UPath` where plain `Path` suffices
