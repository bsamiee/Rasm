# [PY_DATA_API_ICECHUNK]

`icechunk` supplies a transactional, versioned Zarr store with Git-like branching, tagging, and snapshot history over object-store backends (S3, GCS, Azure, local, in-memory, HTTP, R2, Tigris). `Repository` owns lifecycle, branch and tag management, ancestry, and garbage collection; `Session` owns read, write, commit, rebase, fork, and merge; `IcechunkStore` is the Zarr-compatible store handle reached through `session.store`. Storage descriptors and credentials are built through module-level factory functions, and every `Repository`/`Session` method has an `_async` coroutine variant.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `icechunk`
- package: `icechunk`
- module: `icechunk`
- asset: native Rust extension (pyo3); CPython floor `<3.15`
- owner: `data`
- rail: versioned-store

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core lifecycle types
- rail: versioned-store

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [ROLE]                                             |
| :-----: | :----------------- | :-------------- | :------------------------------------------------- |
|   [1]   | `Repository`       | lifecycle owner | create/open, branch/tag, ancestry, GC, sessions    |
|   [2]   | `Session`          | write unit      | read, write, commit, rebase, fork, merge           |
|   [3]   | `ForkSession`      | divergent write | fork-side write unit merged via `Session.merge`    |
|   [4]   | `IcechunkStore`    | Zarr store      | Zarr `Store`-compatible handle via `session.store` |
|   [5]   | `Storage`          | storage handle  | opaque backend descriptor from factory functions   |
|   [6]   | `RepositoryConfig` | configuration   | caching, compression, manifest, storage settings   |

[PUBLIC_TYPE_SCOPE]: record and config types
- rail: versioned-store

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]   | [ROLE]                                                    |
| :-----: | :------------------------------------------------------- | :-------------- | :-------------------------------------------------------- |
|   [1]   | `SnapshotInfo`                                           | history record  | `id`, `parent_id`, `message`, `written_at`, `metadata`    |
|   [2]   | `Diff`                                                   | changeset       | new/deleted/updated arrays, groups, chunks, moved_nodes   |
|   [3]   | `Conflict`                                               | conflict record | `conflict_type`, `path`, `conflicted_chunks`              |
|   [4]   | `GCSummary`                                              | GC result       | chunks/manifests/snapshots/transaction_logs/bytes deleted |
|   [5]   | `Update`                                                 | ops-log entry   | `kind`, `updated_at`, `backup_path`                       |
|   [6]   | `BasicConflictSolver` / `ConflictDetector`               | conflict solver | `ConflictSolver` implementations for rebase               |
|   [7]   | `CachingConfig` / `CompressionConfig` / `ManifestConfig` | config          | nested `RepositoryConfig` settings                        |
|   [8]   | `VirtualChunkContainer` / `VirtualChunkSpec`             | virtual ref     | external chunk addressing descriptors                     |

[PUBLIC_TYPE_SCOPE]: enums
- rail: versioned-store

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [MEMBERS]                                               |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------ |
|   [1]   | `SessionMode`          | mode enum     | `readonly`, `writable`, `rearrange`                     |
|   [2]   | `VersionSelection`     | rebase policy | `Fail`, `UseOurs`, `UseTheirs`                          |
|   [3]   | `ChunkType`            | chunk kind    | `native`, `virtual`, `inline`, `uninitialized`          |
|   [4]   | `ConflictType`         | conflict kind | `ChunkDoubleUpdate`, `ZarrMetadataDoubleUpdate`, 9 more |
|   [5]   | `ChecksumAlgorithm`    | checksum kind | `Crc32`, `Crc32c`, `Crc64Nvme`, `Sha1`, `Sha256`        |
|   [6]   | `CompressionAlgorithm` | codec kind    | `Zstd`                                                  |
|   [7]   | `CommitMethod`         | rewrite mode  | `Literal["new_commit", "amend"]`                        |
|   [8]   | `SpecVersion`          | format ver    | `v1`, `v2`                                              |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: storage and credential factories (`icechunk`)
- rail: versioned-store

| [INDEX] | [SURFACE]                                                                                                                       | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------ | :------------- | :------------------------------- |
|   [1]   | `local_filesystem_storage(path) -> Storage`                                                                                     | storage        | local directory backend          |
|   [2]   | `in_memory_storage() -> Storage`                                                                                                | storage        | ephemeral memory backend         |
|   [3]   | `s3_storage(*, bucket, prefix, region, endpoint_url, allow_http, access_key_id, secret_access_key, anonymous, from_env, ...)`   | storage        | S3 backend                       |
|   [4]   | `gcs_storage(*, bucket, prefix, service_account_file, bearer_token, anonymous, from_env, ...)`                                  | storage        | GCS backend                      |
|   [5]   | `azure_storage(*, account, container, prefix, access_key, sas_token, bearer_token, from_env, anonymous, ...)`                   | storage        | Azure Blob backend               |
|   [6]   | `http_storage(base_url, opts)` / `redirect_storage(base_url)`                                                                   | storage        | HTTP and redirect backends       |
|   [7]   | `r2_storage(*, bucket, prefix, account_id, endpoint_url, ...)` / `tigris_storage(*, bucket, prefix, use_weak_consistency, ...)` | storage        | R2 and Tigris backends           |
|   [8]   | `s3_credentials(*, access_key_id, secret_access_key, session_token, anonymous, from_env, get_credentials, ...)`                 | credential     | S3 credential variants           |
|   [9]   | `s3_static_credentials` / `s3_from_env_credentials` / `s3_anonymous_credentials` / `s3_refreshable_credentials`                 | credential     | explicit S3 credential kinds     |
|  [10]   | `gcs_credentials(...)` / `azure_credentials(...)` and their `_static`/`_from_env`/`_refreshable`/`_anonymous` variants          | credential     | GCS and Azure credential kinds   |
|  [11]   | `containers_credentials(m)`                                                                                                     | credential     | virtual-container credential map |

[ENTRYPOINT_SCOPE]: repository lifecycle, branches, and sessions (`Repository`)
- rail: versioned-store

| [INDEX] | [SURFACE]                                                                                                                                 | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `Repository.create(storage, config=None, authorize_virtual_chunk_access=None, spec_version=None, check_clean_root=True)`                  | lifecycle      | create new repository             |
|   [2]   | `Repository.open(storage, config=None, authorize_virtual_chunk_access=None)`                                                              | lifecycle      | open existing repository          |
|   [3]   | `Repository.open_or_create(storage, config=None, ..., create_version=None, check_clean_root=True)`                                        | lifecycle      | open or create                    |
|   [4]   | `Repository.exists(storage)` / `Repository.fetch_config(storage)`                                                                         | lifecycle      | probe existence or config         |
|   [5]   | `Repository.create_branch(branch, snapshot_id)` / `delete_branch(branch)` / `reset_branch(branch, snapshot_id, *, from_snapshot_id=None)` | branch         | branch management                 |
|   [6]   | `Repository.list_branches()` / `lookup_branch(branch)` / `list_tags()` / `lookup_tag(tag)`                                                | branch         | branch and tag queries            |
|   [7]   | `Repository.create_tag(tag, snapshot_id)` / `delete_tag(tag)`                                                                             | tag            | tag management                    |
|   [8]   | `Repository.writable_session(branch) -> Session`                                                                                          | session        | open a writable session           |
|   [9]   | `Repository.readonly_session(branch=None, *, tag=None, snapshot_id=None, as_of=None) -> Session`                                          | session        | open a read-only session          |
|  [10]   | `Repository.rearrange_session(branch) -> Session`                                                                                         | session        | open a rearrange session          |
|  [11]   | `Repository.transaction(branch, *, message, metadata=None, rebase_with=None, rebase_tries=1000)`                                          | session        | context-managed write transaction |
|  [12]   | `Repository.ancestry(*, branch=None, tag=None, snapshot_id=None) -> Iterator[SnapshotInfo]`                                               | history        | walk snapshot ancestry            |
|  [13]   | `Repository.diff(*, from_branch=None, ..., to_branch=None, ...) -> Diff`                                                                  | history        | diff two refs                     |
|  [14]   | `Repository.expire_snapshots(older_than, *, delete_expired_branches=False, delete_expired_tags=False) -> set[str]`                        | maintenance    | expire old snapshots              |
|  [15]   | `Repository.garbage_collect(delete_object_older_than, *, dry_run=False, ...) -> GCSummary`                                                | maintenance    | reclaim unreferenced objects      |
|  [16]   | `Repository.rewrite_manifests(message, *, branch, metadata=None, commit_method='new_commit') -> str`                                      | maintenance    | rewrite manifest files            |

[ENTRYPOINT_SCOPE]: session and store operations (`Session`, `IcechunkStore`)
- rail: versioned-store

| [INDEX] | [SURFACE]                                                                                                                          | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `Session.commit(message, metadata=None, *, rebase_with=None, rebase_tries=1000, allow_empty=False) -> str`                         | commit         | commit pending writes             |
|   [2]   | `Session.amend(message, *, metadata=None, allow_empty=False)` / `Session.flush(message, *, metadata=None)`                         | commit         | amend or flush a commit           |
|   [3]   | `Session.rebase(solver)` / `Session.discard_changes()`                                                                             | rebase         | resolve conflicts or drop writes  |
|   [4]   | `Session.fork() -> ForkSession` / `Session.merge(*others)`                                                                         | branch         | fork and merge divergent writes   |
|   [5]   | `Session.status() -> Diff`                                                                                                         | query          | uncommitted changeset             |
|   [6]   | `Session.store` / `Session.snapshot_id` / `Session.branch` / `Session.mode` / `Session.has_uncommitted_changes`                    | query          | session state accessors           |
|   [7]   | `Session.move(from_path, to_path)` / `reindex_array(array_path, forward, backward=None)` / `shift_array(array_path, chunk_offset)` | rearrange      | rearrange-session node operations |
|   [8]   | `IcechunkStore.get(key, prototype, byte_range=None)` / `set(key, value)` / `delete(key)`                                           | store          | Zarr buffer read/write/delete     |
|   [9]   | `IcechunkStore.list()` / `list_prefix(prefix)` / `list_dir(prefix)` / `exists(key)` / `is_empty(prefix)`                           | store          | listing and existence queries     |
|  [10]   | `IcechunkStore.set_virtual_ref(key, location, *, offset, length, checksum=None, validate_container=True)`                          | virtual        | register one external chunk ref   |
|  [11]   | `IcechunkStore.set_virtual_refs(array_path, chunks, *, validate_containers=True)`                                                  | virtual        | register many external chunk refs |
|  [12]   | `Session.chunk_type(array_path, chunk_coordinates)` / `all_virtual_chunk_locations()`                                              | query          | chunk kind and virtual locations  |

## [4]-[IMPLEMENTATION_LAW]

[VERSIONED_TOPOLOGY]:
- namespace: `icechunk` (single module); storage and credential factories are module-level functions, `Storage.new_*` class methods exist but are not canonical
- every writable operation flows through a `Session` from `Repository.writable_session(branch)`; the `IcechunkStore` handle is reached via `session.store` and handed to Zarr
- `Repository.transaction(branch, *, message, ...)` is the context-manager shorthand wrapping `writable_session` plus `commit`, yielding an `IcechunkStore`, not a separate code path
- both sync and async variants exist for every `Repository` and `Session` method; async variants carry the `_async` suffix and return coroutines, except `ancestry`/`ops_log` whose async forms are `async_ancestry`/`ops_log_async`
- conflict resolution is passed at commit time as `rebase_with=BasicConflictSolver(...)` or `rebase_with=ConflictDetector()`; `VersionSelection` (`Fail`/`UseOurs`/`UseTheirs`) drives the solver per `ConflictType`
- `rewrite_manifests(..., commit_method=...)` uses the `CommitMethod` literal `"new_commit"` or `"amend"`; `CompressionAlgorithm` exposes `Zstd` and `ChecksumAlgorithm` covers Crc32/Crc32c/Crc64Nvme/Sha1/Sha256
- virtual chunk references point to external object-store locations; `set_virtual_ref`/`set_virtual_refs` register them without copying data, gated by `authorize_virtual_chunk_access` credentials

[LOCAL_ADMISSION]:
- `Repository` is the lifecycle owner, `Session` is the write unit, and `session.store` is the only Zarr store handle that crosses into array code.
- Storage is constructed via the module-level factory functions; credentials use the matching `*_credentials` factory or `containers_credentials` for virtual containers.
- Conflict solvers (`BasicConflictSolver`, `ConflictDetector`) are passed at commit time as `rebase_with=`; do not run rebase as a separate out-of-band step when auto-rebase is configured.
- Choose either the sync or the async method for one operation; never run parallel sync/async call sites for the same write.

[RAIL_LAW]:
- Package: `icechunk`
- Owns: transactional Zarr store, branch/tag/snapshot version control, multi-backend storage, conflict detection and rebase, garbage collection, and virtual chunk addressing
- Accept: `Repository` as the lifecycle owner, `Session` as the write unit, `session.store` as the Zarr handle, module-level storage/credential factories, and `rebase_with` solvers at commit time
- Reject: direct `Storage.new_*` call sites in domain code, out-of-band rebase under auto-rebase, parallel async/sync call sites for one operation, and imperative chunk iteration outside the `IcechunkStore` protocol
