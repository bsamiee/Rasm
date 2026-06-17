# [PY_DATA_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`.

## [1]-[WHEEL_FLOOR_PROBES]

Probes gated on a cp315 wheel publishing or the marker-floor environment being admitted; each is named in its page RESEARCH cluster.

| [INDEX] | [ITEM]                                                                                          | [PAGE#CLUSTER]         | [STATUS] |
| :-----: | :--------------------------------------------------------------------------------------------- | :--------------------- | :------: |
|   [1]   | The Arrow C Data Interface zero-copy path verifies against the real engines (`nanoarrow` deploy-gated) | schema-geo#INTEROP     | SPIKE    |
|   [2]   | ADBC/ConnectorX remote sourcing verifies against a live driver                                  | columnar-query#QUERY   | SPIKE    |
|   [3]   | GeoParquet/CRS-normalized egress verifies against geopandas/pyproj                              | schema-geo#GEO         | SPIKE    |
|   [4]   | The Delta lakehouse lifecycle (time-travel/optimize/vacuum/CDF) verifies against deltalake      | columnar-query#LAKEHOUSE | SPIKE    |
|   [5]   | The chunked tensor store rows (zarr/cubed/awkward/icechunk) verify on the marker floor          | graph-mesh#TENSOR      | SPIKE    |
|   [6]   | Mesh-file round-trip verifies against meshio/trimesh                                            | graph-mesh#MESH        | SPIKE    |

## [2]-[CATALOGUE_GATES]

Manifest and `.api` gaps; the catalogue fills once each distribution installs and `assay api` reflects it.

| [INDEX] | [ITEM]                                                                                          | [PAGE#CLUSTER]         | [STATUS] |
| :-----: | :--------------------------------------------------------------------------------------------- | :--------------------- | :------: |
|   [1]   | columnar/query distributions (polars, pyarrow, pandas, dask, xarray, duckdb, adbc, connectorx, deltalake) resolve to `.api` rows once their cp315 wheels install | columnar-query#DATASET | BLOCKED  |
|   [2]   | geospatial/schema distributions (geopandas, shapely, pyogrio, pyproj, rasterio, pandera) resolve to `.api` rows | schema-geo#GEO         | BLOCKED  |
|   [3]   | AEC mesh distributions (rhino3dm, meshio, trimesh, h5py) resolve to `.api` rows                 | graph-mesh#MESH        | BLOCKED  |

## [3]-[TRANSCRIPTION]

The implementation sequence is the `ARCHITECTURE.md` `[SOURCE_TREE]` build order (`datasets.py` through `graph_mesh.py`); each file transcribes its page clusters verbatim and resolves the RESEARCH rows those pages carry. Production source is absent.

| [INDEX] | [ITEM]                                                                          | [PAGE#CLUSTER]         | [STATUS] |
| :-----: | :----------------------------------------------------------------------------- | :--------------------- | :------: |
|   [1]   | Transcribe the BUILD_ORDER files per `ARCHITECTURE.md` `[SOURCE_TREE]`         | columnar-query#DATASET | QUEUED   |
