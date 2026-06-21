# [PY_RUNTIME_API_GCSFS]

`gcsfs` supplies the Google Cloud Storage fsspec backend: an async `GCSFileSystem` (a full `fsspec.asyn.AsyncFileSystem` with `async_impl=True`, `protocol=('gs','gcs')`, sync-mirrored methods) plus a `GCSMap` mapping view, a Google-credential admission surface, and a built-in retry policy over the GCS JSON API. It is the runtime GCS transport row for shared-pull cloud resource roots: the agent reads the same `gs://` tree a remote tool wrote, composing the fsspec async file surface, the credential model, and the retry/checksum policy rather than re-learning the GCS REST API.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `gcsfs`
- package: `gcsfs`
- import: `gcsfs`
- owner: `runtime`
- rail: resources
- version: `2026.6.0`
- license: `BSD-3-Clause`
- floor: `Requires-Python>=3.10`; wheel `py3-none-any` (pure-Python, no ABI pin); version-locked in step with `fsspec`/`s3fs` (calendar-versioned together)
- namespaces: `gcsfs`, `gcsfs.core`, `gcsfs.credentials`, `gcsfs.mapping`, `gcsfs.retry`
- capability: async GCS fsspec filesystem with sync-mirrored file ops, ranged/concurrent reads, signed URLs, object metadata (`setxattrs`/`getxattr`), `GCSMap` mapping view, Google-credential admission, requester-pays + version-aware + consistency policy, built-in retry/checksum policy over the GCS JSON API

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: filesystem family
- rail: resources

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                                          |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------------- |
|  [01]   | `GCSFileSystem`           | filesystem    | async GCS fsspec filesystem (`async_impl=True`, `protocol=('gs','gcs')`) |
|  [02]   | `GCSMap`                  | mapping       | dict-like view over a GCS prefix (`MutableMapping`)             |
|  [03]   | `credentials.GoogleCredentials` | credential | credential resolver (`token`/`access`/`project`/`on_google`)   |

[PUBLIC_TYPE_SCOPE]: retry and fault vocabulary
- rail: resources

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :-------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `retry.HttpError`                 | fault         | GCS JSON API HTTP error (carries `code`/`message`)          |
|  [02]   | `retry.ChecksumError`             | fault         | crc32c/md5 integrity-check failure on read/write            |
|  [03]   | `retry.NonRetryableError`         | fault         | terminal fault the retry policy must not re-attempt         |
|  [04]   | `retry.RETRIABLE_EXCEPTIONS`      | tuple         | transient-fault set (`ConnectionError`/`ReadTimeout`/`SSLError`/`RefreshError`/…) |
|  [05]   | `retry.DEFAULT_RETRY_CONFIG`      | dict          | default backoff/jitter config consumed by `retry_request`   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: backend construction and credential admission
- rail: resources

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `GCSFileSystem(project='', access='full_control', token=None, *, consistency='none', requester_pays=False, version_aware=False, requests_timeout=None, endpoint_url=None, default_location=None, asynchronous=False, loop=None, session_kwargs=None, **kwargs)` | build | construct via fsspec `storage_options` (never hand-built outside dispatch) |
|  [02]   | `retry.is_retriable(exception) -> bool`                                                            | policy         | classify a fault against `RETRIABLE_EXCEPTIONS` for retry routing |

[ENTRYPOINT_SCOPE]: async file operations (fsspec `AsyncFileSystem` surface)
- rail: resources
- defined on `GCSFileSystem` (PUBLIC_TYPES [01]); each `_`-prefixed coroutine has a sync-mirrored public name (`_cat`/`cat`, `_get`/`get`, `_find`/`find`) via fsspec `mirror_sync_methods`. The runtime awaits the `_`-coroutines under the anyio lane; the sync mirrors are boundary-script only.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `_cat_file(path, start=None, end=None)` / `cat_file`                       | read           | fetch a byte range of one object (start/end window)         |
|  [02]   | `_cat_ranges(paths, starts, ends, max_gap=None, on_error='return')` / `cat_ranges` | read   | coalesced multi-range concurrent fetch (`max_gap` merges adjacent ranges) |
|  [03]   | `_pipe_file(path, value)` / `pipe_file`; `_pipe(path, value)` / `pipe`     | write          | write bytes to one/many objects without a file handle       |
|  [04]   | `_get(rpath, lpath)` / `get`; `_get_file` / `get_file`                     | download       | recursive/single object download to local                  |
|  [05]   | `_put(lpath, rpath)` / `put`; `_put_file` / `put_file`                     | upload         | recursive/single local upload (resumable for large objects) |
|  [06]   | `_ls(path, detail=True)` / `ls`; `_info(path)` / `info`                    | list           | list / stat a prefix or object (detail = full metadata)     |
|  [07]   | `_find(path, maxdepth=None, withdirs=False, detail=False)` / `find`; `walk`; `glob` | list  | recursive enumeration / tree walk / glob match              |
|  [08]   | `_rm(path)` / `rm`; `_rm_file` / `rm_file`; `_cp_file`/`copy`; `_mv`/`mv`  | mutate         | delete / copy / move objects (batched delete)               |
|  [09]   | `open_async(path, mode='rb')`; `open(path, mode)`                          | stream         | async/sync file-object handle for streaming read/write      |
|  [10]   | `get_mapper(root='', check=False, create=False)` -> `GCSMap`              | map            | dict-like `MutableMapping` view over a prefix               |
|  [11]   | `sign(path, expiration=100, **kwargs)`                                     | url            | mint a time-limited signed GCS URL (no credential leak)     |
|  [12]   | `setxattrs(path, content_type=None, content_encoding=None, fixed_key_metadata=None, **kwargs)`; `getxattr(path, attr)` | metadata | set/read object metadata (content-type, custom fixed-key)   |

## [04]-[IMPLEMENTATION_LAW]

[RESOURCES_TOPOLOGY]:
- dispatch law: GCS roots are reached through `fsspec.url_to_fs("gs://...", **storage_options)`; the runtime never constructs `GCSFileSystem` directly outside the fsspec resolution path. The `cachable` instance cache means one `(token, project, …)` tuple yields one shared filesystem — never one client per call.
- async law: the runtime awaits the `_`-prefixed coroutines (`_cat_file`, `_cat_ranges`, `_pipe_file`, `_find`) under the anyio lane; `_cat_ranges` coalesces adjacent windows via `max_gap` and fetches concurrently, so columnar/partial reads issue one batched round-trip rather than N sequential GETs. The sync mirrors are boundary-script only.
- shared-pull law: the agent reads the same `gs://`/`gcs://` tree a remote tool wrote, transferring zero bytes beyond the requested object/range; `cat_ranges` is the partial-read seam for large columnar artifacts.
- credential law: GCS credentials arrive as `storage_options` (`token`/`access`/`project`/`requester_pays`/`endpoint_url`) from the caller-owned settings model, resolved through `gcsfs.credentials.GoogleCredentials`, never a global default client. `token='anon'` selects unauthenticated public-bucket access; `token='google_default'` defers to ADC; `requester_pays=True` charges the named `project`.
- url-signing law: ephemeral shared access is minted through `sign(path, expiration=...)`; a signed URL is the only credential-free hand-off, never an inline service-account key in a URL or config.
- metadata law: object content-type and custom metadata are read/written through `getxattr`/`setxattrs`; `version_aware=True` exposes generation-numbered object versions for content-keyed reads.

[RETRY_TOPOLOGY]:
- The GCS JSON API transient-fault set is owned by `gcsfs.retry` (`RETRIABLE_EXCEPTIONS`, `is_retriable`, `DEFAULT_RETRY_CONFIG`); gcsfs already retries idempotent operations internally with exponential backoff (`GCSFileSystem.retries`, default 6).
- `ChecksumError` is a non-retryable integrity fault (crc32c/md5 mismatch) surfacing data corruption, not a transient transport blip; it lifts to a boundary fault, never silently re-attempted.

[LOCAL_ADMISSION]:
- gcsfs is an fsspec backend row reached by protocol; the generic filesystem surface and dispatch law arrive settled from `.api/fsspec.md`. This catalog documents only the GCS-specific delta (credentials, signed URLs, requester-pays, version-aware, the GCS retry/checksum policy).
- The runtime owns no durable store; GCS is a transport-and-read resource root, never a product store engine.
- `gcsfs` is pure-Python; the crc32c integrity check is delegated to `google-crc32c`, whose C extension accelerates checksums when present and falls back to a slower pure-Python path otherwise — a stack-acceleration note, not a gcsfs ABI requirement.

[STACK_LAW]:
- `fsspec.url_to_fs("gs://...")` -> `GCSFileSystem` (cached instance) -> `_cat_ranges` coalesced read -> bytes handed to the codec/wire-model decoder: one rail, no per-call client, no full-object read for a partial fetch.
- `GCSFileSystem.sign(...)` mints a signed URL that an `httpx.AsyncClient` (the `.api/httpx.md` owner) fetches credential-free; the GCS credential never crosses into the HTTP transport leg.
- `is_retriable`/`RETRIABLE_EXCEPTIONS` classify a transient fault that an outer `stamina.retry_context` may re-attempt at the operation grain; gcsfs owns connection-level retry, `stamina` owns operation-level backoff — never two overlapping retry ladders for the same blip.

[RAIL_LAW]:
- Package: `gcsfs`
- Owns: the GCS fsspec backend for shared-pull cloud resource roots, ranged/concurrent reads, signed URLs, object metadata, Google-credential admission, and the GCS JSON API retry/checksum policy
- Accept: protocol-dispatched cached `GCSFileSystem`, `storage_options` credentials (`token`/`access`/`project`/`requester_pays`/`version_aware`), awaited `_cat_ranges`/`_pipe_file`/`_find` coroutines under anyio, `sign` signed URLs, `getxattr`/`setxattrs` metadata, `GCSMap` views, `retry.is_retriable`/`RETRIABLE_EXCEPTIONS` fault classification
- Reject: direct client instantiation outside fsspec, per-call filesystem construction (defeats the instance cache), global default credentials, inline service-account keys in URLs, full-object reads for a partial fetch when `cat_ranges` fits, GCS as a durable store, a retry ladder overlapping gcsfs's own connection-level retries
