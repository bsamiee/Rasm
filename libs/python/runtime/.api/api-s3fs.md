# [PY_RUNTIME_API_S3FS]

`s3fs` supplies the S3 fsspec backend: an async `S3FileSystem`, S3 file handles, an `S3Map` mapping view, and a customisable retryable-error surface. It registers under the `s3`/`s3a` protocols and is reached through fsspec dispatch, never instantiated as a standalone client. It is the runtime S3 transport row for shared-pull cloud resource roots.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `s3fs`
- package: `s3fs`
- import: `s3fs`
- version: `2026.4.0`
- owner: `runtime`
- rail: resources
- namespaces: `s3fs`, `s3fs.core`, `s3fs.mapping`, `s3fs.errors`, `s3fs.utils`
- capability: async S3 filesystem, S3 file handles, S3 mapping view, retryable-error customisation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: filesystem family
- rail: resources

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `S3FileSystem` | filesystem | async S3 fsspec filesystem |
| [2] | `S3File` | file handle | S3 object file handle |
| [3] | `S3Map` | mapping | dict-like view over an S3 prefix |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: backend operations
- rail: resources

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `S3FileSystem` | build | construct via fsspec `storage_options` |
| [2] | `add_retryable_error` | resilience | extend the retryable-error set |
| [3] | `set_custom_error_handler` | resilience | install a custom error handler |

## [4]-[IMPLEMENTATION_LAW]

[RESOURCES_TOPOLOGY]:
- dispatch law: S3 roots are reached through `fsspec.url_to_fs("s3://...", **storage_options)`; the runtime never constructs `S3FileSystem` directly outside the fsspec resolution path.
- shared-pull law: the agent reads the same `s3://` tree a remote tool wrote, with zero byte transfer beyond the requested object — the resource root is the shared bucket prefix, not a local copy.
- credential law: S3 credentials arrive as `storage_options` (key/secret/token/endpoint) from the caller-owned settings model, never hard-coded or read from a global client.
- resilience law: transient S3 faults extend through `add_retryable_error`; retry scheduling is the `stamina` owner, not a hand-rolled loop.

[LOCAL_ADMISSION]:
- s3fs is a fsspec backend row reached by protocol; the filesystem surface and dispatch law arrive settled from `.api/api-fsspec.md`.
- The runtime owns no durable store; S3 is a transport-and-read resource root, never a product store engine.

[RAIL_LAW]:
- Package: `s3fs`
- Owns: the S3 fsspec backend for shared-pull cloud resource roots
- Accept: protocol-dispatched `S3FileSystem`, `storage_options` credentials, `S3Map` views, retryable-error extension
- Reject: direct client instantiation outside fsspec, hard-coded credentials, S3 as a durable store, hand-rolled retries
