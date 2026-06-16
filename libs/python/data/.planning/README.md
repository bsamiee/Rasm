# [PY_DATA_PLANNING]

`data` owns portable data interchange: typed dataset refs, columnar lazy/streaming scan + egress, query plans across engines, schema claims + a data-contract validation gate, the full vector AND raster geospatial surface as two axes, graph payloads, and mesh-file exchange. It has zero consumers today; implementation is full-capability. Content identity and the egress bundle spine are consumed from runtime `ContentIdentity`, never re-minted. It is an offline interchange and analysis package, not durable Persistence.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                              | [OWNS]                                                    | [STATE]   |
| :-----: | :---------------------------------- | :-------------------------------------------------------- | :-------- |
|   [1]   | [columnar-query](columnar-query.md) | dataset refs, scan plans, columnar egress, query receipts | finalized |
|   [2]   | [schema-geo](schema-geo.md)         | schema claims, contract gate, vector + raster geospatial  | finalized |
|   [3]   | [graph-mesh](graph-mesh.md)         | graph payloads and mesh-file exchange                     | finalized |

## [2]-[CATALOGUE_PENDING]

- `rasterio` carries a `python_version<'3.15'` marker (no cp315 wheel); the project `>=3.15` venv prunes it. The `RasterGeoClaim` fence members verify out-of-band on the marker floor until a cp315 wheel publishes (suite TASKLOG `PY_API_002a`).
- The remaining 18 data distributions (polars, pyarrow, pandas, deltalake, duckdb, adbc-driver-manager, connectorx, dask, xarray, geopandas, shapely, pyogrio, pyproj, rhino3dm, meshio, trimesh, h5py, pandera) carry no cp315 wheel; `networkx` is cp315-installed and reflected.

## [3]-[DENSITY_BAR]

Implementation collapses to one owner per axis. A new data source is a `DatasetKind` row, a new engine is a `ScanPlan` case, a new geometry family is a column. `[STATE]` carries `SPIKE` where the fence is complete but its proof carries a residual cp315-wheel or marker-floor probe named in the page RESEARCH cluster.

| [INDEX] | [AXIS/CONCERN]    | [OWNER]          | [KIND]                  | [CASES]                                   |  [STATE]  |
| :-----: | :---------------- | :--------------- | :---------------------- | :---------------------------------------- | :-------: |
|   [1]   | Dataset identity  | `DatasetRef`     | frozen owner + kind row | csv/parquet/arrow/delta/3dm/mesh/hdf      |   SPIKE   |
|   [2]   | Scan plan         | `ScanPlan`       | tagged union            | polars-lazy/duckdb/pyarrow-dataset        |   SPIKE   |
|   [3]   | Columnar egress   | `ColumnarEgress` | static surface          | arrow/parquet/ipc/lazy-scan               |   SPIKE   |
|   [4]   | Query receipt     | `QueryReceipt`   | receipt                 | engine/source/columns/predicate/row-count |   SPIKE   |
|   [5]   | Schema claim      | `SchemaClaim`    | value object            | field/logical-type/required/nullable      |   SPIKE   |
|   [6]   | Contract gate     | `ContractGate`   | rail                    | schema/quality rules over pandera         |   SPIKE   |
|   [7]   | Vector geospatial | `VectorGeoClaim` | value object            | crs/units/axis-order/geometry-family      |   SPIKE   |
|   [8]   | Raster geospatial | `RasterGeoClaim` | value object            | coverage/band/resampling/nodata           |   SPIKE   |
|   [9]   | Graph payload     | `GraphPayload`   | frozen owner            | kind/nodes/edges/attrs/directionality     | FINALIZED |
|  [10]   | Mesh payload      | `MeshPayload`    | frozen owner            | identity/cell-block/units/metadata        |   SPIKE   |

## [4]-[BUILD_ORDER]

| [INDEX] | [FILE]          | [TRANSCRIBES]           | [GATE]         |
| :-----: | :-------------- | :---------------------- | :------------- |
|   [1]   | `datasets.py`   | columnar-query#DATASET  | static         |
|   [2]   | `scan.py`       | columnar-query#SCAN     | static         |
|   [3]   | `schema_geo.py` | schema-geo#SCHEMA, #GEO | static + floor |
|   [4]   | `graph_mesh.py` | graph-mesh#GRAPH, #MESH | static         |

## [5]-[PROOF_GATES]

| [GATE] | [RAIL]            | [EVIDENCE]                                          |
| :----: | :---------------- | :-------------------------------------------------- |
|  [G1]  | `uv lock --check` | data pins resolve against the root manifest         |
|  [G2]  | `.api` catalogue  | every fence member resolves to an `.api` row        |
|  [G3]  | wheel floor       | cp315/marker-floor wheels install before re-reflect |

## [6]-[PROHIBITIONS]

- [NEVER] re-mint content identity; the egress bundle key is one runtime `ContentIdentity` key.
- [NEVER] own a durable store, schema migration, product repository, query rail, or Rhino/GH document mutation; data emits portable bundles.
- [NEVER] own IFC tessellation/registration/topology (geometry), the numeric trio or labelled-array compute (compute), or remote-stream transport (runtime `TransportResource`).
- [NEVER] create a `get`/`get_many`/`scan`/`list` family; `DatasetRef` discriminates by source shape.
- [NEVER] under-collapse the geospatial axis into one claim; vector and raster are two value objects because band/resampling differs from CRS/axis-order.

## [7]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]                                          | [PAGE]         | [CATALOGUE]          | [STATUS]          |
| :-----: | :------------------------------------------------- | :------------- | :------------------- | :---------------- |
|   [1]   | polars, pyarrow, pandas, dask, xarray              | columnar-query | api-polars.md ...    | catalogue-pending |
|   [2]   | duckdb, adbc-driver-manager, connectorx, deltalake | columnar-query | api-duckdb.md ...    | catalogue-pending |
|   [3]   | geopandas, shapely, pyogrio, pyproj                | schema-geo     | api-geopandas.md ... | catalogue-pending |
|   [4]   | rasterio                                           | schema-geo     | api-rasterio.md      | catalogue-pending |
|   [5]   | pandera                                            | schema-geo     | api-pandera.md       | catalogue-pending |
|   [6]   | networkx                                           | graph-mesh     | api-networkx.md      | admitted          |
|   [7]   | rhino3dm, meshio, trimesh, h5py                    | graph-mesh     | api-meshio.md ...    | catalogue-pending |

## [8]-[REFINEMENT_HORIZON]

Entry for the next deepening session: `libs/python/.planning/campaign-method.md`, then the suite `TASKLOG.md`, then this charter. Every `SPIKE` owner finalizes once its distribution installs (a cp315 wheel publishes or the marker-floor environment is admitted) and `assay api query` fills the catalogue. The Arrow C Data Interface zero-copy path, the ADBC/ConnectorX remote sourcing, and the GeoParquet/CRS-normalized egress deepen against the real engines. The bar: any portable data pipeline a flagship app ships — lazy scan, predicate pushdown, cross-engine egress, geospatial reprojection, graph algorithm, mesh-file round-trip — is buildable from these pages alone, content-addressed by one runtime owner.
