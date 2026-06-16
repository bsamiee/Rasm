# [PY_RUNTIME_API_GCSFS]

`gcsfs` supplies the Google Cloud Storage fsspec backend: an async `GCSFileSystem` and a `GCSMap` mapping view, registered under the `gs`/`gcs` protocols and reached through fsspec dispatch. It is the runtime GCS transport row for shared-pull cloud resource roots.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `gcsfs`
- package: `gcsfs`
- import: `gcsfs`
- version: `2026.4.0`
- owner: `runtime`
- rail: resources
- namespaces: `gcsfs`, `gcsfs.core`, `gcsfs.credentials`, `gcsfs.mapping`
- capability: async GCS filesystem, GCS mapping view, credential admission

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: filesystem family
- rail: resources

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :-------------- | :------------ | :------------------------------- |
|   [1]   | `GCSFileSystem` | filesystem    | async GCS fsspec filesystem      |
|   [2]   | `GCSMap`        | mapping       | dict-like view over a GCS prefix |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: backend operations
- rail: resources

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------------- | :------------- | :------------------------------------- |
|   [1]   | `GCSFileSystem`             | build          | construct via fsspec `storage_options` |
|   [2]   | `GCSFileSystem.credentials` | auth           | resolved credential surface            |

## [4]-[IMPLEMENTATION_LAW]

[RESOURCES_TOPOLOGY]:
- dispatch law: GCS roots are reached through `fsspec.url_to_fs("gs://...", **storage_options)`; the runtime never constructs `GCSFileSystem` directly outside the fsspec resolution path.
- shared-pull law: the agent reads the same `gs://`/`gcs://` tree a remote tool wrote, with zero byte transfer beyond the requested object.
- credential law: GCS credentials arrive as `storage_options` (token/project/anon) from the caller-owned settings model, resolved through the gcsfs credential surface, never a global default client.

[LOCAL_ADMISSION]:
- gcsfs is a fsspec backend row reached by protocol; the filesystem surface and dispatch law arrive settled from `.api/api-fsspec.md`.
- The runtime owns no durable store; GCS is a transport-and-read resource root, never a product store engine.

[RAIL_LAW]:
- Package: `gcsfs`
- Owns: the GCS fsspec backend for shared-pull cloud resource roots
- Accept: protocol-dispatched `GCSFileSystem`, `storage_options` credentials, `GCSMap` views
- Reject: direct client instantiation outside fsspec, global default credentials, GCS as a durable store
