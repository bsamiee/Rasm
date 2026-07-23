# [PY_DATA_API_ICECHUNK]

`icechunk` owns a transactional, versioned Zarr store — Git-like branch, tag, and snapshot history over any object-store backend. `Repository` owns lifecycle, branches, tags, ancestry, and garbage collection; `Session` owns read, write, commit, rebase, fork, and merge; `IcechunkStore`, reached through `session.store`, is the Zarr-compatible handle every array consumer binds. Module-level factories build storage descriptors and credentials, and the synchronous surface mirrors onto an async rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `icechunk`
- package: `icechunk` (Apache-2.0)
- module: `icechunk`
- owner: data
- rail: versioned-store

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core lifecycle types

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [ROLE]                                             |
| :-----: | :----------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `Repository`       | lifecycle owner | create/open, branch/tag, ancestry, GC, sessions    |
|  [02]   | `Session`          | write unit      | read, write, commit, rebase, fork, merge           |
|  [03]   | `ForkSession`      | divergent write | fork-side write unit merged via `Session.merge`    |
|  [04]   | `IcechunkStore`    | Zarr store      | Zarr `Store`-compatible handle via `session.store` |
|  [05]   | `Storage`          | storage handle  | opaque backend descriptor from factory functions   |
|  [06]   | `RepositoryConfig` | configuration   | caching, compression, manifest, storage settings   |

[PUBLIC_TYPE_SCOPE]: record and config types

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]   | [ROLE]                                                               |
| :-----: | :------------------------------------------- | :-------------- | :------------------------------------------------------------------- |
|  [01]   | `SnapshotInfo`                               | history record  | `id`, `parent_id`, `message`, `written_at`, `metadata`               |
|  [02]   | `Diff`                                       | changeset       | new/deleted/updated arrays, groups, chunks, moved_nodes              |
|  [03]   | `Conflict`                                   | conflict record | `conflict_type`, `path`, `conflicted_chunks`                         |
|  [04]   | `GCSummary`                                  | GC result       | chunks/manifests/snapshots/attributes/transaction_logs/bytes deleted |
|  [05]   | `Update` / `UpdateType`                      | ops-log entry   | `kind`, `updated_at`, `backup_path`; `UpdateType` enum of kinds      |
|  [06]   | `AncestryGraph`                              | history graph   | DAG of `SnapshotInfo` from `Repository.ancestry_graph`               |
|  [07]   | `ConflictSolver`                             | solver base     | abstract solver passed as `rebase_with`                              |
|  [08]   | `BasicConflictSolver` / `ConflictDetector`   | conflict solver | `ConflictSolver` implementations for rebase                          |
|  [09]   | `RepoStatus` / `RepoAvailability`            | repo status     | repository health/status read via `get_status`                       |
|  [10]   | `VirtualChunkContainer` / `VirtualChunkSpec` | virtual ref     | external chunk addressing descriptors                                |

[PUBLIC_TYPE_SCOPE]: configuration types

`RepositoryConfig` is the root config; the caching/compression/manifest/storage families are nested settings. `FeatureFlag` gates active store behavior. `ManifestSplittingConfig`/`ManifestPreloadConfig` tune manifest sharding and preload, keyed by the `ManifestSplit*Condition`/`ManifestPreloadCondition` predicate enums.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [ROLE]                                               |
| :-----: | :-------------------------------- | :---------------- | :--------------------------------------------------- |
|  [01]   | `RepositoryConfig`                | root config       | caching/compression/manifest/storage/inline settings |
|  [02]   | `CachingConfig`                   | nested config     | chunk cache behavior                                 |
|  [03]   | `CompressionConfig`               | nested config     | snapshot compression behavior                        |
|  [04]   | `ManifestConfig`                  | nested config     | manifest behavior                                    |
|  [05]   | `ManifestSplittingConfig`         | split config      | manifest sharding root                               |
|  [06]   | `ManifestSplitCondition`          | split predicate   | node split predicate                                 |
|  [07]   | `ManifestSplitDimCondition`       | split predicate   | dimension split predicate                            |
|  [08]   | `ManifestPreloadConfig`           | preload config    | manifest preload budget                              |
|  [09]   | `ManifestPreloadCondition`        | preload predicate | manifest preload predicate                           |
|  [10]   | `StorageSettings`                 | storage tuning    | per-backend storage policy root                      |
|  [11]   | `StorageConcurrencySettings`      | storage tuning    | concurrency policy                                   |
|  [12]   | `StorageRetriesSettings`          | storage tuning    | retry policy                                         |
|  [13]   | `StorageTimeoutSettings`          | storage tuning    | timeout policy                                       |
|  [14]   | `S3Options` / `ObjectStoreConfig` | backend config    | S3/object-store endpoint and connection options      |
|  [15]   | `FeatureFlag`                     | feature gate      | store-behavior flag set on the repository            |

[PUBLIC_TYPE_SCOPE]: enums

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

`IcechunkError` is the root exception; `ConflictError` and `RebaseFailedError` are the rebase/commit failures the data tier maps to its typed error. `RebaseFailedError` carries the unresolved `Conflict` list for solver-driven retry.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [ROLE]                                                      |
| :-----: | :------------------ | :------------ | :---------------------------------------------------------- |
|  [01]   | `IcechunkError`     | error root    | base exception for all icechunk failures                    |
|  [02]   | `ConflictError`     | commit error  | raised on commit when the branch tip moved under the writer |
|  [03]   | `RebaseFailedError` | rebase error  | carries unresolved `Conflict` list after a failed rebase    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: storage and credential factories (`icechunk`)

Storage factories return an opaque `Storage`; cloud backends share the keyword-only `*, bucket, prefix, anonymous, from_env` shape and differ by endpoint/account and credential params. Object-store cloud backends (`s3_storage`/`gcs_storage`/`r2_storage`/`tigris_storage`) accept a `headers` dict attached to every request, `s3_storage` splitting `read_headers`/`write_headers`. Credential factories share `*, access_key_id, secret_access_key, session_token, anonymous, from_env, get_credentials`; virtual-store factories produce an `ObjectStoreConfig` fed to `VirtualChunkContainer`.

| [INDEX] | [SURFACE]                                                                               | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `local_filesystem_storage(path) -> Storage`                                             | local directory backend                      |
|  [02]   | `in_memory_storage() -> Storage`                                                        | ephemeral memory backend                     |
|  [03]   | `s3_storage(*, region, endpoint_url, allow_http, ...)`                                  | S3 backend                                   |
|  [04]   | `gcs_storage(*, service_account_file, bearer_token, ...)`                               | GCS backend                                  |
|  [05]   | `azure_storage(*, account, container, access_key, sas_token, ...)`                      | Azure Blob backend                           |
|  [06]   | `http_storage(base_url, opts, headers)`                                                 | HTTP backend                                 |
|  [07]   | `redirect_storage(base_url)`                                                            | redirect backend                             |
|  [08]   | `r2_storage(*, account_id, endpoint_url, ...)`                                          | R2 backend                                   |
|  [09]   | `tigris_storage(*, use_weak_consistency, ...)`                                          | Tigris backend                               |
|  [10]   | `s3_credentials(*, session_token, get_credentials, ...)`                                | variant-dispatching S3 credential factory    |
|  [11]   | `s3_static_credentials` / `s3_from_env_credentials`                                     | explicit static / from-env S3 credentials    |
|  [12]   | `s3_anonymous_credentials` / `s3_refreshable_credentials`                               | anonymous / refreshable S3 credentials       |
|  [13]   | `gcs_credentials(...)` / `azure_credentials(...)`                                       | GCS / Azure credential factories             |
|  [14]   | `gcs_credentials`/`azure_credentials` `_static`/`_from_env`/`_refreshable`/`_anonymous` | explicit GCS/Azure credential kinds          |
|  [15]   | `containers_credentials(m)`                                                             | virtual-container credential map             |
|  [16]   | `s3_store(region, endpoint_url, allow_http, s3_compatible, force_path_style, ...)`      | S3 `ObjectStoreConfig` factory               |
|  [17]   | `gcs_store(opts)` / `http_store(opts, headers)` / `local_filesystem_store(path)`        | GCS/HTTP/local `ObjectStoreConfig` factories |

[ENTRYPOINT_SCOPE]: repository lifecycle, branches, and sessions (`Repository`)

Every surface is a `Repository` method (leading `Repository.` elided) except rows [23]-[24], module-level functions; optional keyword args default `None` unless a default is shown.

| [INDEX] | [SURFACE]                                                                                             | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------------------------------ |
|  [01]   | `create(storage, config, authorize_virtual_chunk_access, spec_version, check_clean_root=True)`        | create new repository           |
|  [02]   | `open(storage, config=None, authorize_virtual_chunk_access=None)`                                     | open existing repository        |
|  [03]   | `open_or_create(storage, config=None, ..., create_version=None, check_clean_root=True)`               | open or create                  |
|  [04]   | `exists(storage)` / `fetch_config(storage)`                                                           | probe existence or config       |
|  [05]   | `create_branch(branch, snapshot_id)` / `delete_branch(branch)`                                        | branch create / delete          |
|  [06]   | `reset_branch(branch, snapshot_id, *, from_snapshot_id=None)`                                         | move a branch tip               |
|  [07]   | `list_branches()` / `lookup_branch(branch)` / `list_tags()` / `lookup_tag(tag)`                       | branch and tag queries          |
|  [08]   | `create_tag(tag, snapshot_id)` / `delete_tag(tag)`                                                    | tag management                  |
|  [09]   | `writable_session(branch) -> Session`                                                                 | open a writable session         |
|  [10]   | `readonly_session(branch=None, *, tag=None, snapshot_id=None, as_of=None) -> Session`                 | open a read-only session        |
|  [11]   | `rearrange_session(branch) -> Session`                                                                | open a rearrange session        |
|  [12]   | `transaction(branch, *, message, metadata=None, rebase_with=None, rebase_tries=1000)`                 | context-managed transaction     |
|  [13]   | `ancestry(*, branch=None, tag=None, snapshot_id=None) -> Iterator[SnapshotInfo]`                      | walk snapshot ancestry          |
|  [14]   | `ancestry_graph(*, branch=None, tag=None, snapshot_id=None, plain=False) -> AncestryGraph`            | full ancestry DAG               |
|  [15]   | `lookup_snapshot(snapshot_id) -> SnapshotInfo`                                                        | resolve one snapshot            |
|  [16]   | `diff(*, from_branch=None, ..., to_branch=None, ...) -> Diff`                                         | diff two refs                   |
|  [17]   | `get_status() -> RepoStatus` / `set_status(...)`                                                      | read/write repository status    |
|  [18]   | `get_metadata()` / `set_metadata(m)` / `update_metadata(m)`                                           | repository-level metadata       |
|  [19]   | `expire_snapshots(older_than, *, delete_expired_branches, delete_expired_tags) -> set[str]`           | expire old snapshots            |
|  [20]   | `garbage_collect(delete_object_older_than, *, dry_run, max_snapshots_in_memory=50, ...) -> GCSummary` | reclaim unreferenced objects    |
|  [21]   | `rewrite_manifests(message, *, branch, metadata=None, commit_method='new_commit') -> str`             | rewrite manifest files          |
|  [22]   | `chunk_storage_stats()` / `list_manifest_files()`                                                     | storage stats and manifest list |
|  [23]   | `supported_spec_versions() -> list[SpecVersion]`                                                      | spec-version roster             |
|  [24]   | `upgrade_icechunk_repository(repo, *, dry_run, delete_unused_v1_files=True, prefetch_concurrency)`    | in-place repository upgrade     |

[ENTRYPOINT_SCOPE]: session and store operations (`Session`, `IcechunkStore`)

Every surface is a `Session` method (leading `Session.` elided); optional keyword args default `None` unless a default is shown.

| [INDEX] | [SURFACE]                                                                                     | [CAPABILITY]                     |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------------------------- |
|  [01]   | `commit(message, metadata, *, rebase_with, rebase_tries=1000, allow_empty=False) -> str`      | commit pending writes            |
|  [02]   | `amend(message, *, metadata, allow_empty=False)` / `flush(message, *, metadata)`              | amend or flush a commit          |
|  [03]   | `rebase(solver)` / `discard_changes()`                                                        | resolve conflicts or drop writes |
|  [04]   | `fork() -> ForkSession` / `merge(*others)`                                                    | fork and merge divergent writes  |
|  [05]   | `status() -> Diff`                                                                            | uncommitted changeset            |
|  [06]   | `store` / `snapshot_id` / `branch` / `mode`                                                   | session state accessors          |
|  [07]   | `has_uncommitted_changes`                                                                     | pending-write predicate          |
|  [08]   | `move(from_path, to_path)` / `get_node_id(path)`                                              | rearrange node move / id         |
|  [09]   | `reindex_array(array_path, forward, backward=None)` / `shift_array(array_path, chunk_offset)` | rearrange array reindex / shift  |
|  [10]   | `chunk_type(array_path, chunk_coordinates)` / `all_virtual_chunk_locations()`                 | chunk kind and virtual locations |

Store operations run on `IcechunkStore` (leading `IcechunkStore.` elided).

| [INDEX] | [SURFACE]                                                                                  | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `get(key, prototype, byte_range=None)` / `set(key, value)` / `delete(key)`                 | Zarr buffer read/write/delete     |
|  [02]   | `list()` / `list_prefix(prefix)` / `list_dir(prefix)` / `exists(key)` / `is_empty(prefix)` | listing and existence queries     |
|  [03]   | `set_virtual_ref(key, location, *, offset, length, checksum, validate_container=True)`     | register one external chunk ref   |
|  [04]   | `set_virtual_refs(array_path, chunks, *, validate_containers=True)`                        | register many external chunk refs |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `icechunk` is one module; storage and credential factories are module-level functions.
- Every writable operation flows through a `Session` from `Repository.writable_session(branch)`; `session.store` yields the `IcechunkStore` handed to Zarr.
- `Repository.transaction(branch, *, message, ...)` fuses `writable_session` and `commit`, yielding an `IcechunkStore` inside the context — one path, not a separate code branch.
- Async variants suffix `_async` and return coroutines; the two iterator-returning methods instead prefix as `async_ancestry` and `async_ops_log`, each returning an `AsyncCloseableIterator`.
- Conflict resolution passes at commit time as `rebase_with=BasicConflictSolver(...)` or `rebase_with=ConflictDetector()`, `VersionSelection` (`Fail`/`UseOurs`/`UseTheirs`) driving the solver per `ConflictType`. A bare `commit` on a moved branch tip raises `ConflictError`; an unresolved auto-rebase raises `RebaseFailedError` carrying the `Conflict` list — both map to the data tier's typed error, never swallowed.
- Virtual chunk references address external object-store locations without copying data, each gated by `authorize_virtual_chunk_access` credentials; `containers_credentials` lowers a per-prefix credential-factory map into that keyword, and `set_virtual_ref`/`set_virtual_refs` register one chunk or a `VirtualChunkSpec` batch under fully-qualified Zarr-v3 keys.

[STACKING]:
- `zarr`(`.api/zarr.md`): `session.store` is a `zarr.abc.store.Store`, so `zarr.open_group(store=session.store, ...)`/`create_array(store=...)` read and write the versioned store with no separate code path — icechunk owns commit/branch/snapshot, `zarr` owns array semantics over the same handle.
- `xarray`(`libs/python/.api/xarray.md`): `xarray.open_zarr(session.store)` reads and `Dataset.to_zarr(session.store, ...)` then `session.commit(message)` writes one snapshot per dataset write; wrapping the `to_zarr` inside `Repository.transaction(branch, message=...)` commits on clean exit.
- `tensorstore`(`.api/tensorstore.md`): reads the same on-disk zarr-v3 format icechunk writes through the `zarr3` driver for high-throughput async access.
- `virtualizarr`(`.api/virtualizarr.md`): aggregates archival references into a virtual zarr icechunk persists via `set_virtual_refs`.
- within-lib: icechunk is the transactional snapshot layer beneath the zarr/xarray array layer; `Repository.readonly_session(*, tag=/snapshot_id=/as_of=).store` opens any committed snapshot in zarr/xarray for time-travel reads.

[LOCAL_ADMISSION]:
- Construct `Storage` through the module-level factories and credentials through the matching `*_credentials` factory or `containers_credentials` for virtual containers.
- Pass conflict solvers at commit time as `rebase_with=`; never run `rebase` out of band when auto-rebase is configured.
- Choose the sync or the async method for one operation; never run parallel sync/async call sites for the same write.

[RAIL_LAW]:
- Package: `icechunk`
- Owns: transactional Zarr store, branch/tag/snapshot version control, multi-backend storage, conflict detection and rebase, garbage collection, and virtual chunk addressing
- Accept: `Repository` as the lifecycle owner, `Session` as the write unit, `session.store` as the Zarr handle handed to `zarr`/`xarray`/`tensorstore`, module-level storage/credential factories, `rebase_with` solvers at commit time, and `IcechunkError`/`ConflictError`/`RebaseFailedError` mapped to the data tier's typed error
- Reject: direct `Storage.new_*` call sites in domain code, out-of-band rebase under auto-rebase, parallel async/sync call sites for one operation, imperative chunk iteration outside the `IcechunkStore` protocol, and a parallel array/dataset container that re-owns the zarr/xarray semantics layered over `session.store`
