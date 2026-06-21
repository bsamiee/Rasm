# [PY_DATA_API_ICECHUNK]

`icechunk` supplies a transactional, versioned Zarr store with Git-like branching, tagging, and snapshot history over object-store backends (S3, GCS, Azure, local, in-memory, HTTP, R2, Tigris). `Repository` owns lifecycle, branch and tag management, ancestry, and garbage collection; `Session` owns read, write, commit, rebase, fork, and merge; `IcechunkStore` is the Zarr-compatible store handle reached through `session.store`. Storage descriptors and credentials are built through module-level factory functions, and every `Repository`/`Session` method has an `_async` coroutine variant.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `icechunk`
- package: `icechunk`
- version: `2.0.6`
- license: Apache-2.0
- module: `icechunk`
- asset: native Rust extension (pyo3, `icechunk._icechunk_python`); upstream `requires-python >=3.12`, but no `cp315` wheel ships, so admitted only on the `python_version<'3.15'` companion band
- owner: `data`
- rail: versioned-store

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core lifecycle types
- rail: versioned-store

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [ROLE]                                             |
| :-----: | :----------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `Repository`       | lifecycle owner | create/open, branch/tag, ancestry, GC, sessions    |
|  [02]   | `Session`          | write unit      | read, write, commit, rebase, fork, merge           |
|  [03]   | `ForkSession`      | divergent write | fork-side write unit merged via `Session.merge`    |
|  [04]   | `IcechunkStore`    | Zarr store      | Zarr `Store`-compatible handle via `session.store` |
|  [05]   | `Storage`          | storage handle  | opaque backend descriptor from factory functions   |
|  [06]   | `RepositoryConfig` | configuration   | caching, compression, manifest, storage settings   |

[PUBLIC_TYPE_SCOPE]: record and config types
- rail: versioned-store

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]   | [ROLE]                                                          |
| :-----: | :------------------------------------------------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `SnapshotInfo`                                           | history record  | `id`, `parent_id`, `message`, `written_at`, `metadata`          |
|  [02]   | `Diff`                                                   | changeset       | new/deleted/updated arrays, groups, chunks, moved_nodes         |
|  [03]   | `Conflict`                                               | conflict record | `conflict_type`, `path`, `conflicted_chunks`                    |
|  [04]   | `GCSummary`                                              | GC result       | chunks/manifests/snapshots/transaction_logs/bytes deleted       |
|  [05]   | `Update` / `UpdateType`                                  | ops-log entry   | `kind`, `updated_at`, `backup_path`; `UpdateType` enum of kinds |
|  [06]   | `AncestryGraph`                                          | history graph   | DAG of `SnapshotInfo` from `Repository.ancestry_graph`          |
|  [07]   | `ConflictSolver`                                         | solver base     | abstract solver passed as `rebase_with`                         |
|  [08]   | `BasicConflictSolver` / `ConflictDetector`               | conflict solver | `ConflictSolver` implementations for rebase                     |
|  [09]   | `RepoStatus` / `RepoAvailability`                        | repo status     | repository health/status read via `get_status`                 |
|  [10]   | `VirtualChunkContainer` / `VirtualChunkSpec`             | virtual ref     | external chunk addressing descriptors                           |

[PUBLIC_TYPE_SCOPE]: configuration types
- rail: versioned-store

`RepositoryConfig` is the root config; the caching/compression/manifest/storage families are nested settings. `FeatureFlag` gates experimental store behavior. `ManifestSplittingConfig`/`ManifestPreloadConfig` tune manifest sharding and preload, keyed by the `ManifestSplit*Condition`/`ManifestPreloadCondition` predicate enums.

| [INDEX] | [SYMBOL]                                                                                                          | [TYPE_FAMILY] | [ROLE]                                                |
| :-----: | :---------------------------------------------------------------------------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `RepositoryConfig`                                                                                                | root config   | caching/compression/manifest/storage/inline settings  |
|  [02]   | `CachingConfig` / `CompressionConfig` / `ManifestConfig`                                                          | nested config | chunk cache, snapshot compression, manifest behavior   |
|  [03]   | `ManifestSplittingConfig` / `ManifestSplitCondition` / `ManifestSplitDimCondition`                               | split config  | manifest sharding by node/dimension predicate          |
|  [04]   | `ManifestPreloadConfig` / `ManifestPreloadCondition`                                                             | preload config| manifest preload predicate and budget                 |
|  [05]   | `StorageSettings` / `StorageConcurrencySettings` / `StorageRetriesSettings` / `StorageTimeoutSettings`           | storage tuning| concurrency, retry, and timeout policy per backend     |
|  [06]   | `S3Options` / `ObjectStoreConfig`                                                                                | backend config| S3/object-store endpoint and connection options       |
|  [07]   | `FeatureFlag`                                                                                                     | feature gate  | experimental-behavior flag set on the repository       |

[PUBLIC_TYPE_SCOPE]: enums
- rail: versioned-store

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [MEMBERS]                                               |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `SessionMode`          | mode enum     | `readonly`, `writable`, `rearrange`                     |
|  [02]   | `VersionSelection`     | rebase policy | `Fail`, `UseOurs`, `UseTheirs`                          |
|  [03]   | `ChunkType`            | chunk kind    | `native`, `virtual`, `inline`, `uninitialized`          |
|  [04]   | `ConflictType`         | conflict kind | `ChunkDoubleUpdate`, `ZarrMetadataDoubleUpdate`, 9 more |
|  [05]   | `ChecksumAlgorithm`    | checksum kind | `Crc32`, `Crc32c`, `Crc64Nvme`, `Sha1`, `Sha256`        |
|  [06]   | `CompressionAlgorithm` | codec kind    | `Zstd`                                                  |
|  [07]   | `CommitMethod`         | rewrite mode  | `Literal["new_commit", "amend"]`                        |
|  [08]   | `SpecVersion`          | format ver    | `v1`, `v2`                                              |

[PUBLIC_TYPE_SCOPE]: error rail
- rail: versioned-store

`IcechunkError` is the root exception; `ConflictError` and `RebaseFailedError` are the rebase/commit failures the data tier maps to its typed error. `RebaseFailedError` carries the unresolved `Conflict` list for solver-driven retry.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [ROLE]                                                      |
| :-----: | :------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `IcechunkError`      | error root    | base exception for all icechunk failures                   |
|  [02]   | `ConflictError`      | commit error  | raised on commit when the branch tip moved under the writer |
|  [03]   | `RebaseFailedError`  | rebase error  | carries unresolved `Conflict` list after a failed rebase    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: storage and credential factories (`icechunk`)
- rail: versioned-store

| [INDEX] | [SURFACE]                                                                                                                       | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------ | :------------- | :------------------------------- |
|  [01]   | `local_filesystem_storage(path) -> Storage`                                                                                     | storage        | local directory backend          |
|  [02]   | `in_memory_storage() -> Storage`                                                                                                | storage        | ephemeral memory backend         |
|  [03]   | `s3_storage(*, bucket, prefix, region, endpoint_url, allow_http, access_key_id, secret_access_key, anonymous, from_env, ...)`   | storage        | S3 backend                       |
|  [04]   | `gcs_storage(*, bucket, prefix, service_account_file, bearer_token, anonymous, from_env, ...)`                                  | storage        | GCS backend                      |
|  [05]   | `azure_storage(*, account, container, prefix, access_key, sas_token, bearer_token, from_env, anonymous, ...)`                   | storage        | Azure Blob backend               |
|  [06]   | `http_storage(base_url, opts)` / `redirect_storage(base_url)`                                                                   | storage        | HTTP and redirect backends       |
|  [07]   | `r2_storage(*, bucket, prefix, account_id, endpoint_url, ...)` / `tigris_storage(*, bucket, prefix, use_weak_consistency, ...)` | storage        | R2 and Tigris backends           |
|  [08]   | `s3_credentials(*, access_key_id, secret_access_key, session_token, anonymous, from_env, get_credentials, ...)`                 | credential     | S3 credential variants           |
|  [09]   | `s3_static_credentials` / `s3_from_env_credentials` / `s3_anonymous_credentials` / `s3_refreshable_credentials`                 | credential     | explicit S3 credential kinds     |
|  [10]   | `gcs_credentials(...)` / `azure_credentials(...)` and their `_static`/`_from_env`/`_refreshable`/`_anonymous` variants          | credential     | GCS and Azure credential kinds   |
|  [11]   | `containers_credentials(m)`                                                                                                     | credential     | virtual-container credential map |

[ENTRYPOINT_SCOPE]: repository lifecycle, branches, and sessions (`Repository`)
- rail: versioned-store

| [INDEX] | [SURFACE]                                                                                                                                 | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `Repository.create(storage, config=None, authorize_virtual_chunk_access=None, spec_version=None, check_clean_root=True)`                  | lifecycle      | create new repository             |
|  [02]   | `Repository.open(storage, config=None, authorize_virtual_chunk_access=None)`                                                              | lifecycle      | open existing repository          |
|  [03]   | `Repository.open_or_create(storage, config=None, ..., create_version=None, check_clean_root=True)`                                        | lifecycle      | open or create                    |
|  [04]   | `Repository.exists(storage)` / `Repository.fetch_config(storage)`                                                                         | lifecycle      | probe existence or config         |
|  [05]   | `Repository.create_branch(branch, snapshot_id)` / `delete_branch(branch)` / `reset_branch(branch, snapshot_id, *, from_snapshot_id=None)` | branch         | branch management                 |
|  [06]   | `Repository.list_branches()` / `lookup_branch(branch)` / `list_tags()` / `lookup_tag(tag)`                                                | branch         | branch and tag queries            |
|  [07]   | `Repository.create_tag(tag, snapshot_id)` / `delete_tag(tag)`                                                                             | tag            | tag management                    |
|  [08]   | `Repository.writable_session(branch) -> Session`                                                                                          | session        | open a writable session           |
|  [09]   | `Repository.readonly_session(branch=None, *, tag=None, snapshot_id=None, as_of=None) -> Session`                                          | session        | open a read-only session          |
|  [10]   | `Repository.rearrange_session(branch) -> Session`                                                                                         | session        | open a rearrange session          |
|  [11]   | `Repository.transaction(branch, *, message, metadata=None, rebase_with=None, rebase_tries=1000)`                                          | session        | context-managed write transaction |
|  [12]   | `Repository.ancestry(*, branch=None, tag=None, snapshot_id=None) -> Iterator[SnapshotInfo]`                                               | history        | walk snapshot ancestry            |
|  [13]   | `Repository.ancestry_graph(*, branch=None, tag=None, snapshot_id=None, plain=False) -> AncestryGraph`                                     | history        | full ancestry DAG                 |
|  [14]   | `Repository.lookup_snapshot(snapshot_id) -> SnapshotInfo`                                                                                 | history        | resolve one snapshot              |
|  [15]   | `Repository.diff(*, from_branch=None, ..., to_branch=None, ...) -> Diff`                                                                  | history        | diff two refs                     |
|  [16]   | `Repository.get_status() -> RepoStatus` / `set_status(...)`                                                                               | status         | read/write repository status      |
|  [17]   | `Repository.get_metadata()` / `set_metadata(m)` / `update_metadata(m)`                                                                    | metadata       | repository-level metadata         |
|  [18]   | `Repository.expire_snapshots(older_than, *, delete_expired_branches=False, delete_expired_tags=False) -> set[str]`                        | maintenance    | expire old snapshots              |
|  [19]   | `Repository.garbage_collect(delete_object_older_than, *, dry_run=False, max_snapshots_in_memory=50, ...) -> GCSummary`                    | maintenance    | reclaim unreferenced objects      |
|  [20]   | `Repository.rewrite_manifests(message, *, branch, metadata=None, commit_method='new_commit') -> str`                                      | maintenance    | rewrite manifest files            |
|  [21]   | `Repository.chunk_storage_stats()` / `list_manifest_files()`                                                                              | introspection  | storage stats and manifest list   |

[ENTRYPOINT_SCOPE]: session and store operations (`Session`, `IcechunkStore`)
- rail: versioned-store

| [INDEX] | [SURFACE]                                                                                                                          | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `Session.commit(message, metadata=None, *, rebase_with=None, rebase_tries=1000, allow_empty=False) -> str`                         | commit         | commit pending writes             |
|  [02]   | `Session.amend(message, *, metadata=None, allow_empty=False)` / `Session.flush(message, *, metadata=None)`                         | commit         | amend or flush a commit           |
|  [03]   | `Session.rebase(solver)` / `Session.discard_changes()`                                                                             | rebase         | resolve conflicts or drop writes  |
|  [04]   | `Session.fork() -> ForkSession` / `Session.merge(*others)`                                                                         | branch         | fork and merge divergent writes   |
|  [05]   | `Session.status() -> Diff`                                                                                                         | query          | uncommitted changeset             |
|  [06]   | `Session.store` / `Session.snapshot_id` / `Session.branch` / `Session.mode` / `Session.has_uncommitted_changes`                    | query          | session state accessors           |
|  [07]   | `Session.move_node(from_path, to_path)` / `reindex_array(array_path, forward, backward=None)` / `shift_array(array_path, chunk_offset)` / `get_node_id(path)` | rearrange      | rearrange-session node operations |
|  [08]   | `IcechunkStore.get(key, prototype, byte_range=None)` / `set(key, value)` / `delete(key)`                                           | store          | Zarr buffer read/write/delete     |
|  [09]   | `IcechunkStore.list()` / `list_prefix(prefix)` / `list_dir(prefix)` / `exists(key)` / `is_empty(prefix)`                           | store          | listing and existence queries     |
|  [10]   | `IcechunkStore.set_virtual_ref(key, location, *, offset, length, checksum=None, validate_container=True)`                          | virtual        | register one external chunk ref   |
|  [11]   | `IcechunkStore.set_virtual_refs(array_path, chunks, *, validate_containers=True)`                                                  | virtual        | register many external chunk refs |
|  [12]   | `Session.chunk_type(array_path, chunk_coordinates)` / `all_virtual_chunk_locations()`                                              | query          | chunk kind and virtual locations  |

## [04]-[IMPLEMENTATION_LAW]

[VERSIONED_TOPOLOGY]:
- namespace: `icechunk` (single module); storage and credential factories are module-level functions, `Storage.new_*` class methods exist but are not canonical
- every writable operation flows through a `Session` from `Repository.writable_session(branch)`; the `IcechunkStore` handle is reached via `session.store` and handed to Zarr
- `Repository.transaction(branch, *, message, ...)` is the context-manager shorthand wrapping `writable_session` plus `commit`, yielding an `IcechunkStore`, not a separate code path
- both sync and async variants exist for every `Repository` and `Session` method; async variants carry the `_async` suffix and return coroutines, except the iterator pair `ancestry`/`ops_log` whose async forms are the prefix-named `async_ancestry`/`async_ops_log` (returning `AsyncIterator`)
- conflict resolution is passed at commit time as `rebase_with=BasicConflictSolver(...)` or `rebase_with=ConflictDetector()`; `VersionSelection` (`Fail`/`UseOurs`/`UseTheirs`) drives the solver per `ConflictType`. A bare `commit` on a moved branch tip raises `ConflictError`; an unresolved auto-rebase raises `RebaseFailedError` carrying the `Conflict` list — both map to the data tier's typed error, never swallowed
- `rewrite_manifests(..., commit_method=...)` uses the `CommitMethod` literal `"new_commit"` or `"amend"`; `CompressionAlgorithm` exposes `Zstd` and `ChecksumAlgorithm` covers Crc32/Crc32c/Crc64Nvme/Sha1/Sha256
- virtual chunk references point to external object-store locations; `set_virtual_ref`/`set_virtual_refs` register them without copying data, gated by `authorize_virtual_chunk_access` credentials

[STACK]:
- `session.store` is the `zarr.abc.store.Store` handle: `zarr.open_group(store=session.store, ...)` / `xarray.open_zarr(session.store)` read/write the versioned store with no separate code path. icechunk owns version control; `zarr`/`xarray` (`.api/zarr.md`, `.api/xarray.md`) own array/dataset semantics over the same handle.
- write rail: `xarray.Dataset.to_zarr(session.store, ...)` then `session.commit(message)` produces one snapshot per dataset write; the `Repository.transaction(branch, message=...)` context manager fuses `writable_session`+`commit` so the `to_zarr` happens inside the `with` and commits on clean exit.
- read rail: `tensorstore` (`.api/tensorstore.md`) reads the same on-disk zarr-v3 format icechunk writes for high-throughput async access, and `virtualizarr` (`.api/virtualizarr.md`) aggregates archival references into a virtual zarr that icechunk persists via `set_virtual_refs` — icechunk is the transactional snapshot layer beneath the zarr/xarray array layer, never a parallel array container.
- a read-only history view is `Repository.readonly_session(tag=...)`/`(snapshot_id=...)`/`(as_of=...)` -> `session.store` opened in zarr/xarray for time-travel reads against any committed snapshot.

[LOCAL_ADMISSION]:
- `Repository` is the lifecycle owner, `Session` is the write unit, and `session.store` is the only Zarr store handle that crosses into array code.
- Storage is constructed via the module-level factory functions; credentials use the matching `*_credentials` factory or `containers_credentials` for virtual containers.
- Conflict solvers (`BasicConflictSolver`, `ConflictDetector`) are passed at commit time as `rebase_with=`; do not run rebase as a separate out-of-band step when auto-rebase is configured.
- Choose either the sync or the async method for one operation; never run parallel sync/async call sites for the same write.

[RAIL_LAW]:
- Package: `icechunk`
- Owns: transactional Zarr store, branch/tag/snapshot version control, multi-backend storage, conflict detection and rebase, garbage collection, and virtual chunk addressing
- Accept: `Repository` as the lifecycle owner, `Session` as the write unit, `session.store` as the Zarr handle handed to `zarr`/`xarray`/`tensorstore`, module-level storage/credential factories, `rebase_with` solvers at commit time, and `IcechunkError`/`ConflictError`/`RebaseFailedError` mapped to the data tier's typed error
- Reject: direct `Storage.new_*` call sites in domain code, out-of-band rebase under auto-rebase, parallel async/sync call sites for one operation, imperative chunk iteration outside the `IcechunkStore` protocol, and a parallel array/dataset container that re-owns the zarr/xarray semantics layered over `session.store`
