# [PY_DATA_COLUMNAR_QUERY]

Columnar/query ownership centers on portable scan plans and egress receipts. The package uses external engines for their full capability instead of wrapping them into a lowest-common dataframe facade.

## [1]-[DATASET_OWNER]

[DATASET_REF]:
- Owns: dataset URI, dataset kind, media type, declared engine preferences, and caller-owned provenance.
- Cases: CSV, Parquet, Arrow IPC, Arrow dataset, Delta table, Pandas-compatible file, Polars-compatible file, `.3dm`, mesh, and HDF-backed payload.
- API routes: `.api/api-polars.md`, `.api/api-pyarrow.md`, `.api/api-pandas.md`, `.api/api-xarray.md`, `.api/api-deltalake.md`, `.api/api-h5py.md`.
- Boundary: dataset references are not product store identities.

[COLUMNAR_EGRESS]:
- Owns: Arrow Table, Parquet, IPC, and lazy scan export decisions.
- Packages: `pyarrow`, `polars`, `duckdb`.
- Output: egress plan plus receipt fields for format, row count, schema hash, and content digest route.
- Boundary: no app schema migration or durable write.

## [2]-[SCAN_OWNER]

[SCAN_PLAN]:
- Owns: scan engine, projection, predicates, input partitions, and explicit connection policy.
- Cases: Polars lazy scan, DuckDB relation, PyArrow dataset scanner.
- API routes: `.api/api-polars.md`, `.api/api-duckdb.md`, `.api/api-pyarrow.md`.
- Boundary: DuckDB connections are explicit resources; global connections and process-wide SQL state are rejected.

[QUERY_RECEIPT]:
- Owns: scan, transform, and egress fault classification for offline query work.
- Output: receipt rows with engine, source, selected columns, predicate count, row count, and artifact reference.
- Boundary: product query receipts and persistence timelines remain `Rasm.Persistence` concerns.

## [3]-[RED_TEAM]

- Reject a generic dataframe wrapper that erases Polars, Arrow, DuckDB, Pandas, Dask, and Xarray distinctions.
- Reject direct SQL strings without a `ScanPlan` owner and explicit connection policy.
- Reject egress code that writes product store state.
- Reject package-local schema law that belongs to Persistence.
