# [PY_DATA_API_FSSPEC]

Full surface and stacking: `libs/python/.api/fsspec.md` (shared-tier canonical owner).

`fsspec` enters data only as the resolved filesystem behind a `UPath` and as the DuckDB remote-glob scan seam. Data does not construct filesystems, register protocols, or admit cloud backend extras.

## [01]-[DATA_READ_SLICE]

[LOCAL_ADMISSION]:
- `UPath(ref).fs` is the only fsspec handle the data folder owns.
- `DuckDBPyConnection.register_filesystem(fs)` binds that handle into a request-scoped DuckDB scan session for remote glob reads.
- The scan session receives a whole resolved backend; data never re-registers protocol classes and never instantiates `s3fs`, `gcsfs`, or a custom filesystem directly.
- Read caching stays with DuckDB `httpfs`, object-store caching, or the owning chunked-array backend; data does not select fsspec block-cache policies.

[MINE_OR_DECLINE]:
- `transaction` / `start_transaction` / `end_transaction` — DECLINE: egress atomicity is `obstore` conditional write or a table-format commit.
- `cat_ranges(paths, starts, ends, max_gap=None, on_error="return")` — DECLINE: chunk-range reads ride `obstore.get_range`/`get_ranges`, DuckDB `httpfs`, or a native reader.
- `BlockCache` / `BackgroundBlockCache` / `ReadAheadCache` / `MMapCache` / `FirstChunkCache` — DECLINE: read caching is owner-specific and never a data-global policy.
- `FSMap` / `get_mapper(url, check=False, create=False)` — DECLINE: mutable chunk stores are `zarr`, `tensorstore`, or `icechunk` owners.
- `fsspec.generic.rsync` — DECLINE: bulk movement is out of the data interchange charter; object egress is content-keyed `obstore` put, not directory sync.

[RAIL_LAW]:
- Package: `fsspec` (data overlay)
- Owns: the data read-slice over `UPath.fs` and DuckDB `register_filesystem`
- Accept: `UPath(ref).fs` handed to `DuckDBPyConnection.register_filesystem(fs)` for remote-glob scans
- Reject: direct filesystem construction, protocol registration, cloud-extra re-admission, cache policy selection, transaction usage, mapping-store usage, and bulk directory sync inside data
