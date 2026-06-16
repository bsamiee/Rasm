# [PY_RUNTIME_API_UNIVERSAL_PATHLIB]

`universal-pathlib` supplies `UPath`: a `pathlib.Path`-shaped object over any fsspec backend, resolving protocol and `storage_options` at construction so local, memory, S3, GCS, and HTTP roots share one path API. It is the runtime path-object face over the fsspec filesystem surface.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `universal-pathlib`
- package: `universal-pathlib`
- import: `upath`
- version: `0.3.10`
- owner: `runtime`
- rail: resources
- namespaces: `upath`, `upath.implementations`, `upath.registry`
- capability: fsspec-backed `pathlib`-shaped path objects with protocol and `storage_options` resolution

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: path family
- rail: resources

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `UPath` | path | fsspec-backed universal path |
| [2] | `UnsupportedOperation` | fault | unsupported path operation |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: path operations
- rail: resources

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `UPath(url, protocol=..., **storage_options)` | build | construct a backend-resolved path |
| [2] | `UPath.fs` | filesystem | underlying fsspec filesystem |
| [3] | `UPath.path` | path | backend-relative path string |
| [4] | `UPath.protocol` | metadata | resolved protocol |
| [5] | `UPath.storage_options` | metadata | resolved backend options |
| [6] | `UPath.open` | access | open the target |
| [7] | `UPath.glob` / `UPath.rglob` | traverse | pattern enumeration |
| [8] | `UPath.iterdir` | traverse | directory listing |
| [9] | `UPath.joinpath` / `/` | compose | child path composition |

## [4]-[IMPLEMENTATION_LAW]

[RESOURCES_TOPOLOGY]:
- path law: a resource reference is one `UPath` carrying its protocol and `storage_options`; path arithmetic (`/`, `joinpath`, `glob`) is backend-agnostic and the same code serves local and cloud roots.
- resolution law: `UPath.fs` exposes the fsspec filesystem; the filesystem surface and dispatch law arrive settled from `.api/api-fsspec.md`. `UPath` is the path face, fsspec is the filesystem face — one backend, two views.
- option law: `storage_options`/`protocol=` resolution at construction is load-bearing; credentials and endpoints travel with the path value, never a separate global filesystem handle.

[LOCAL_ADMISSION]:
- `ResourceRef` is a `UPath`; the runtime composes path arithmetic and traversal over it and resolves the filesystem only when bytes are accessed.
- `UPath` never replaces `pathlib.Path` for purely local, performance-critical paths where no protocol indirection is needed.

[RAIL_LAW]:
- Package: `universal-pathlib`
- Owns: fsspec-backed universal path objects with protocol and `storage_options` resolution
- Accept: `UPath` with `protocol`/`storage_options`, backend-agnostic path arithmetic, `UPath.fs` filesystem access
- Reject: per-scheme path branching, a separate global filesystem handle alongside the path, `UPath` where plain `Path` suffices
