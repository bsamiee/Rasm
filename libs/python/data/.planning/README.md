# [PY_DATA_PLANNING]

`data` owns portable datasets, columnar scans, schema claims, geospatial claims, graph payloads, and AEC/file exchange bundles. It is an offline interchange and analysis package, not durable Persistence.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE] | [OWNS] |
| :-----: | :----- | :----- |
|   [1]   | [columnar query](columnar-query.md) | datasets, scans, Arrow, DuckDB, and columnar egress |
|   [2]   | [schema geo AEC](schema-geo-aec.md) | schema claims, CRS/unit evidence, geospatial and AEC exchange |
|   [3]   | [graph bundle](graph-bundle.md) | graph payloads, mesh/HDF exchange, bundle receipts, and split candidates |

## [2]-[OWNER_CLUSTERS]

[DATASET]:
- Owner symbols: `DatasetRef`, `DatasetKind`.
- API routes: `.api/api-polars.md`, `.api/api-pyarrow.md`, `.api/api-pandas.md`, `.api/api-xarray.md`, `.api/api-rhino3dm.md`, `.api/api-meshio.md`, `.api/api-trimesh.md`.
- Boundary: no product identity, no product repository, no host document mutation.

[SCAN]:
- Owner symbols: `ScanPlan`, `QueryReceipt`.
- API routes: `.api/api-duckdb.md`, `.api/api-polars.md`, `.api/api-pyarrow.md`, `.api/api-adbc-driver-manager.md`, `.api/api-connectorx.md`, `.api/api-deltalake.md`.
- Boundary: no durable query rails and no global DuckDB connection.

[SCHEMA_GEO]:
- Owner symbols: `SchemaClaim`, `GeoClaim`.
- API routes: `.api/api-geopandas.md`, `.api/api-shapely.md`, `.api/api-pyogrio.md`, `.api/api-pyproj.md`.
- Boundary: no Persistence migration law and no live Rhino/GH mutation.

[GRAPH_AEC]:
- Owner symbols: `ExchangeBundle`, graph and mesh payload records.
- API routes: `.api/api-networkx.md`, `.api/api-h5py.md`, `.api/api-rhino3dm.md`, `.api/api-meshio.md`, `.api/api-trimesh.md`.
- Boundary: no geometry kernel, no bridge lifecycle, no product collaboration store.

## [3]-[TRANSCRIPTION_LAW]

- Future source lands directly under `libs/python/data`.
- Every external API member used by source must appear in the package `.api` folder first.
- Data imports runtime contracts only after runtime source exists and only for rails, receipts, and resource roots.
- Data emits portable files, records, and receipts; Persistence owns durable stores.
