# [PY_DATA]

`data` owns portable data interchange: typed dataset refs, columnar lazy/streaming scan and egress, query plans across engines, schema claims and a data-contract validation gate, the vector and raster geospatial axes, graph payloads, and mesh-file exchange. It has zero consumers today and implementation is full-capability. Content identity and the egress bundle spine are consumed from the runtime `ContentIdentity` owner, never re-minted. Owner state and the axis registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`. The design pages in `.planning/` are decision-complete blueprints an implementation agent transcribes; the package catalogues in `.api/` carry the external-surface evidence each page consumes.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                        | [OWNS]                                                     |
| :-----: | :-------------------------------------------- | :-------------------------------------------------------- |
|   [1]   | [columnar-query](.planning/columnar-query.md) | dataset refs, scan plans, lakehouse, query engine, quality |
|   [2]   | [schema-geo](.planning/schema-geo.md)         | dataframe-agnostic interop, frame admission, geospatial    |
|   [3]   | [graph-mesh](.planning/graph-mesh.md)         | graph payloads, chunked tensor stores, mesh-file exchange  |

## [2]-[ADMISSIONS_RECORD]

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in the root manifest; this table never carries a pin. `[STATUS]` is one of `admitted`, `catalogue-pending`. Distributions without a cp315 wheel carry `catalogue-pending` until a wheel publishes or the marker-floor environment is admitted and `assay api` fills the catalogue.

| [INDEX] | [PACKAGE]                                          | [PAGE]         | [CATALOGUE]                                                              | [STATUS]          |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------------------------------------- | :---------------- |
|   [1]   | polars, pyarrow, pandas, dask, xarray             | columnar-query | api-polars.md, api-pyarrow.md, api-pandas.md, api-dask.md, api-xarray.md | catalogue-pending |
|   [2]   | duckdb, adbc-driver-manager, connectorx, deltalake | columnar-query | api-duckdb.md, api-adbc-driver-manager.md, api-connectorx.md, api-deltalake.md | catalogue-pending |
|   [3]   | geopandas, shapely, pyogrio, pyproj               | schema-geo     | api-geopandas.md, api-shapely.md, api-pyogrio.md, api-pyproj.md          | catalogue-pending |
|   [4]   | rasterio                                          | schema-geo     | api-rasterio.md                                                          | catalogue-pending |
|   [5]   | pandera                                           | schema-geo     | api-pandera.md                                                          | catalogue-pending |
|   [6]   | networkx                                          | graph-mesh     | api-networkx.md                                                         | admitted          |
|   [7]   | rhino3dm, meshio, trimesh, h5py                   | graph-mesh     | api-rhino3dm.md, api-meshio.md, api-trimesh.md, api-h5py.md              | catalogue-pending |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                | [RAIL]      | [EVIDENCE]                                          |
| :-----: | :-------------------- | :---------- | :------------------------------------------------- |
|  [G1]   | locked restore        | uv          | data pins resolve against the root manifest         |
|  [G2]   | API catalogue resolve | assay api   | every fence member resolves to an `.api` row        |
|  [G3]   | type check            | ty          | typed-signature transcription resolves clean        |
|  [G4]   | lint and format       | ruff        | routed closure, zero diagnostics                    |
|  [G5]   | spec law-matrix       | pytest      | data law-matrix specs pass                          |
|  [G6]   | wheel floor           | uv          | cp315/marker-floor wheels install before re-reflect |
|  [G7]   | page diagram render   | mermaid-cli | page diagrams render through the local renderer      |
