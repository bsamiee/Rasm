# [PY_RUNTIME_API_S3FS]

`s3fs` supplies the S3 fsspec backend: an async `S3FileSystem`, S3 file handles, an `S3Map` mapping view, and a customisable retryable-error surface. It registers under the `s3`/`s3a` protocols and is reached through fsspec dispatch, never instantiated as a standalone client. It is the runtime S3 transport row for shared-pull cloud resource roots.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `s3fs`
- package: `s3fs`
- import: `s3fs`
- owner: `runtime`
- rail: resources
- namespaces: `s3fs`, `s3fs.core`, `s3fs.mapping`, `s3fs.errors`, `s3fs.utils`
- installed: `2026.4.0`
- capability: async `AsyncFileSystem` S3 backend with sync mirrors, S3 file handles with multipart upload, an `S3Map` mapping view, presigned-URL generation, object versioning, server-side-encryption params, region caching, and a retryable-error / custom-error-handler surface

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: filesystem family
- rail: resources

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :---------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `S3FileSystem`                | filesystem    | async `AsyncFileSystem` S3 backend (`s3`/`s3a`) |
|  [02]   | `S3File`                      | file handle   | S3 object file handle with multipart upload     |
|  [03]   | `S3Map`                       | mapping       | factory returning an `fsspec.FSMap` over a prefix |
|  [04]   | `utils.SSEParams`             | value object  | server-side-encryption request params           |
|  [05]   | `utils.S3BucketRegionCache`   | cache         | per-bucket region resolution cache               |
|  [06]   | `utils.ParamKwargsHelper`     | adapter       | filters caller kwargs to the boto method's accepted set |
|  [07]   | `utils.FileExpired`           | fault         | `IOError` raised when a cached ETag no longer matches |
|  [08]   | `errors.translate_boto_error` | fault map     | botocore `ClientError` -> stdlib `OSError` subclass |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and credentials
- rail: resources
- the constructor params become the `storage_options` keys fsspec dispatch forwards; credentials and endpoint flow here, never a global client.

| [INDEX] | [SURFACE]                                                                                                       | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :-------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `S3FileSystem(anon=, key=, secret=, token=, endpoint_url=, use_ssl=, client_kwargs=, config_kwargs=, s3_additional_kwargs=, requester_pays=, default_block_size=, default_fill_cache=, version_aware=, cache_regions=, profile=, max_concurrency=, fixed_upload_size=)` | build | full constructor; reached via fsspec `storage_options` |
|  [02]   | `split_path(path) -> tuple[bucket, key, version_id | None]`                                                     | path parse     | decompose an `s3://` path including a version    |

[ENTRYPOINT_SCOPE]: async object/byte operations (sync mirror auto-generated)
- rail: resources
- defined as `_`-prefixed coroutines on `AsyncFileSystem`; fsspec exposes a same-named sync wrapper (e.g. `_cat_file` -> `cat_file`). Async callers await the `_` form directly.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :-------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `_cat_file(path, version_id=, start=, end=)` / `_cat_file_concurrent` | read           | full or ranged object bytes (concurrent multi-part GET) |
|  [02]   | `_pipe_file(path, data, ...)`                                         | write          | write bytes to an object (single or multipart)   |
|  [03]   | `_get_file(rpath, lpath, ...)` / `_put_file(lpath, rpath, ...)`       | transfer       | object <-> local file streaming transfer         |
|  [04]   | `_cp_file(path1, path2, preserve_etag=)` / `_copy_managed` / `_copy_etag_preserved` | copy | server-side copy, ETag-preserving for large objects |
|  [05]   | `_merge(path, filelist)`                                             | compose        | multipart-upload concatenation of existing objects |
|  [06]   | `_rm(path, recursive=)` / `_rm_file` / `_bulk_delete(pathlist)`       | delete         | single, recursive, and batched (1000-key) delete |
|  [07]   | `_ls(path, detail=, refresh=, versions=)` / `_lsdir` / `_lsbuckets`  | list           | prefix/bucket listing, version-aware             |
|  [08]   | `_info(path, version_id=, refresh=)` / `_exists` / `_checksum`       | stat           | object metadata, existence, ETag checksum        |
|  [09]   | `_find` / `_glob` / `_walk(path, maxdepth=)`                         | traverse       | recursive key discovery                          |
|  [10]   | `_mkdir` / `_makedirs(exist_ok=)` / `_rmdir`                         | bucket         | bucket create/remove (S3 has no real dirs)       |
|  [11]   | `_object_version_info(path)`                                        | versioning     | list object versions (requires `version_aware=True`) |

[ENTRYPOINT_SCOPE]: presign / metadata / resilience
- rail: resources

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `sign(path, expiration=100, **kwargs)` / `_url(path, expires=, client_method=)` | presign | presigned URL for an object (shareable hand-off) |
|  [02]   | `getxattr(xattr_name)` / `setxattr(copy_kwargs=, **kwargs)`     | metadata       | object user metadata read/write (via copy)       |
|  [03]   | `invalidate_cache(path=)` / `modified(path, version_id=, refresh=)` | cache | drop listing cache; last-modified instant         |
|  [04]   | `add_retryable_error(exc)`                                     | resilience     | append an exception type to the module `S3_RETRYABLE_ERRORS` tuple |
|  [05]   | `set_custom_error_handler(func)`                              | resilience     | install a module-level handler invoked on retryable failure |

[ENTRYPOINT_SCOPE]: mapping view
- rail: resources

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `S3Map(root, s3, check=False, create=False)`       | mapping        | mutable `MutableMapping` (`fsspec.FSMap`) over an `s3://` prefix |

## [04]-[IMPLEMENTATION_LAW]

[RESOURCES_TOPOLOGY]:
- dispatch law: S3 roots are reached through `fsspec.url_to_fs("s3://...", **storage_options)`; the runtime never constructs `S3FileSystem` directly outside the fsspec resolution path. The `s3` and `s3a` protocols both register this backend.
- async law: `S3FileSystem` is an `AsyncFileSystem` over an `aiobotocore` client; the real I/O is the `_`-prefixed coroutine (`_cat_file`, `_get_file`, `_pipe_file`), and the sync method of the same name without the underscore is the fsspec-generated wrapper. Async runtime owners await the `_` form on the shared event-loop client; never spin a second client per call.
- shared-pull law: the agent reads the same `s3://` tree a remote tool wrote, with zero byte transfer beyond the requested object — ranged `_cat_file(start=, end=)` reads a slice, `_cat_file_concurrent` parallelizes a large object, and the resource root is the shared bucket prefix, not a local copy.
- write law: large writes/copies use S3 multipart — `S3File` buffers to `blocksize` parts, `_copy_managed`/`_copy_etag_preserved` do server-side multipart copy, and `_merge` concatenates existing objects without re-download; `fixed_upload_size=True` is required for fixed-part backends (Cloudflare R2).
- versioning law: with `version_aware=True` the path grammar carries a `?versionId=`/version suffix decoded by `split_path`, and `_object_version_info`/`_info(version_id=)` address a specific revision; without it, S3 latest-version semantics apply.
- credential law: S3 credentials arrive as `storage_options` (`key`/`secret`/`token`/`endpoint_url`/`profile`/`client_kwargs`/`config_kwargs`) from the caller-owned `pydantic-settings` model, never hard-coded or read from a global client; `anon=True` is the unauthenticated public-bucket path. Server-side encryption is one `s3_additional_kwargs`/`SSEParams` row, not a per-call header.
- resilience law: transient S3 faults extend the module `S3_RETRYABLE_ERRORS` tuple through `add_retryable_error`, and `set_custom_error_handler` maps a provider fault; botocore `ClientError` is normalised to a stdlib `OSError` subclass by `errors.translate_boto_error`. Retry *scheduling* is the `stamina` owner, not a hand-rolled loop.

[LOCAL_ADMISSION]:
- s3fs is a fsspec backend row reached by protocol; the generic filesystem surface (`open`/`cat`/`ls`/`glob`/`pipe`/`get`/`put`) and dispatch law arrive settled from `.api/fsspec.md`; this page owns the S3-specific construction, multipart/versioning/presign/SSE behavior, and the boto error rail.
- The runtime owns no durable store; S3 is a transport-and-read resource root, never a product store engine.

[INTEGRATION_STACK]:
- fsspec leg: `fsspec.url_to_fs("s3://bucket/prefix", **storage_options)` from `.api/fsspec.md` returns this backend; `fsspec.open`/`open_files` and the `S3Map` (`fsspec.FSMap`) compose the generic surface, so callers code against fsspec and S3 is a protocol detail.
- aiobotocore leg: the underlying client is `aiobotocore`'s async S3 client; `client_kwargs`/`config_kwargs`/`profile` are forwarded to `aiobotocore.session.AioSession`, so AWS credential resolution, endpoint, and retry-config are botocore-native, not re-implemented.
- stamina leg: a transient `_cat_file`/`_put_file` failure registered via `add_retryable_error` is retried by a `.api/stamina.md` `retry_context`/`@retry` wrapping the resource read, with the attempt recorded on the surrounding observability span — s3fs declares *what* is retryable, stamina owns *when*.
- settings leg: the `storage_options` dict is a field on the `.api/pydantic-settings.md` `BaseSettings` model (or a nested-secrets / cloud-secret-manager source), so one validated config carries the credential set into the fsspec dispatch.
- obstore boundary: `.api/obstore.md` is the alternative Rust object-store path; s3fs is the fsspec-native S3 row for fsspec-shaped consumers — pick one transport per resource root, never double-dispatch the same prefix through both.

[RAIL_LAW]:
- Package: `s3fs`
- Owns: the async S3 fsspec backend (multipart, versioning, presign, SSE, region cache, boto error rail) for shared-pull cloud resource roots
- Accept: protocol-dispatched `S3FileSystem` via `storage_options`, awaited `_`-prefixed async ops on the shared client, ranged/concurrent reads, multipart writes/copies, `S3Map` views, `version_aware` revisions, presigned URLs, `add_retryable_error`/`set_custom_error_handler` resilience extension
- Reject: direct client instantiation outside fsspec, hard-coded credentials, S3 as a durable store, hand-rolled retry loops, a per-call client, hand-rolled multipart or varint range framing
