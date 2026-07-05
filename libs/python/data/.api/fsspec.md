# [PY_DATA_API_FSSPEC]

`fsspec` is the filesystem-abstraction spine beneath `universal-pathlib` (`UPath`) and every cloud/archive backend the platform reaches: one `AbstractFileSystem` protocol (local, memory, http, s3, gcs, az, zip, tar, cached, ...) resolved by protocol string through a global registry, with byte-range reads, globbing, directory listing, a mutable-mapping view (`FSMap`), a pluggable block-cache layer, and atomic-write transactions. In the data plane its role is narrow and delegated: transport ownership is runtime's (`TransportResource`, `obstore` for object egress, `universal-pathlib` for path resolution), so data mines exactly one read-slice — the `.fs` filesystem handle off a resolved `UPath`, handed to DuckDB's `register_filesystem` so a `httpfs`/remote-glob scan reads through the same backend that resolved the ref. Every other fsspec surface (transactions, concurrent range fetch, block caching, `FSMap`, bulk `rsync`) is runtime- or engine-owned and DECLINED here per the mine-or-decline rows below.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fsspec`
- package: `fsspec`
- import: `import fsspec` (reached in-plane through `UPath.fs`, never a direct construction)
- owner: `data`
- rail: transport (the `UPath.fs` read-slice into the DuckDB scan session; branch-owned substrate, folder-tier read-slice catalogue per the domain-consumption law)
- version: `2026.4.0`
- license: `BSD-3-Clause`
- asset: pure Python; backend extras (`s3fs`/`gcsfs`/`adlfs`/`aiohttp`) pull the concrete drivers — none re-admitted at the data tier (the runtime `s3fs`/`gcsfs` strike is final track-wide; `obstore` owns object transport)
- depends-on: stdlib only at the core; `universal-pathlib` (`UPath`) is the in-plane accessor, `duckdb` the consumer of the resolved handle
- capability: protocol-registry filesystem resolution, byte-range and block reads, glob/ls/find directory walks, `FSMap` mapping view, a pluggable read-cache family, and atomic-write transactions across every registered backend

## [02]-[CAPTURE]

[PUBLIC_TYPES]: the resolution + registry surface (the mined slice)
- `AbstractFileSystem` — the backend protocol; the data plane touches it only as the object `UPath.fs` returns. Read/list members (`cat`/`cat_file`/`open`/`ls`/`glob`/`find`/`info`/`exists`/`walk`/`read_block`) are the DuckDB-driven surface; the handle is passed whole, never method-mined here.
- `filesystem(protocol, **storage_options)` / `url_to_fs(url, **kw) -> (fs, path)` / `get_filesystem_class(protocol)` — protocol-string resolution; in-plane this is `universal-pathlib`'s job (`UPath(url).fs`), so these are reached transitively, not called directly.
- `register_implementation(name, cls, clobber=False, errtxt=None)` — the registry write hook a custom backend uses to bind a protocol; the DuckDB seam consumes the resolved instance, not the registry.

[IMPLEMENTATION_LAW]:
- the ONE live data seam is `con.register_filesystem(dataset.ref.path.fs)` (`tabular/columnar.md`, the `remote_glob` scan arm): the resolved `UPath` exposes its backing `AbstractFileSystem` through `.fs`, and DuckDB registers that instance so `read_parquet(glob, hive_partitioning=...)` reads remote/archive globs through the exact backend that resolved the ref — one filesystem object, resolved once by `universal-pathlib`, shared into the scan session, never re-opened.
- data NEVER constructs an `fsspec` filesystem directly and never re-registers a protocol: `TransportResource`/`obstore`/`universal-pathlib` own construction, credentials, and object transport; fsspec enters only as the already-resolved `.fs` handle.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `fsspec`
- Owns: protocol-registry filesystem resolution beneath `universal-pathlib`; the resolved `AbstractFileSystem` the DuckDB scan session registers
- Accept: `UPath(ref).fs` -> `DuckDBPyConnection.register_filesystem(fs)` as the remote-glob scan seam — the resolved backend handed whole into the scan session
- Reject: constructing an `fsspec` filesystem directly (resolution is `universal-pathlib`'s); re-admitting `s3fs`/`gcsfs` at the data tier (the runtime strike is final; `obstore` owns object transport); mining fsspec method-by-method where the DuckDB `httpfs`/scan layer drives the handle
- MINE-OR-DECLINE (the runtime-`[V13]`-delegated surfaces):
  - `transaction` / `start_transaction` / `end_transaction` (atomic-write staging) — DECLINE: egress atomicity is `obstore` `PutMode`/conditional-write owned (`tabular/egress.md`); the data plane never stages fsspec transactions.
  - `cat_ranges(paths, starts, ends, max_gap=None, on_error="return")` (concurrent byte-range fetch) — DECLINE: chunk-range reads ride `obstore` `get_ranges` and the engine-native readers (`tensorstore`/`icechunk`); no data fence fetches ranges through fsspec.
  - block-cache family (`BlockCache`/`BackgroundBlockCache`/`ReadAheadCache`/`MMapCache`/`FirstChunkCache`) — DECLINE: read caching is DuckDB `httpfs`- and engine-owned; the plane hands a raw handle and lets the consumer cache.
  - `FSMap` / `get_mapper(url, check=False, create=False)` (mutable-mapping store view) — DECLINE: the chunked-array store layer is `zarr` v3 store / `icechunk` / `obstore` owned; the legacy `FSMap` mapping is superseded and unused here.
  - `fsspec.generic.rsync` (bulk directory sync) — DECLINE: bulk movement is out of the interchange charter; egress is content-keyed `obstore` put, not a mirror sync.
