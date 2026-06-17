# [PYTHON_BRANCH_TASKLOG]

Open work only — branch-altitude items and the install/catalogue/floor gaps no single package owns. Closed items never appear; their closure is read where the owning package `ARCHITECTURE.md` `[OWNER_REGISTRY]` `[STATE]` reads `FINALIZED`. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`.

## [1]-[MANIFEST_FLOORS]

| [INDEX] | [ITEM]                                                                                                                                                                                                                                                                                          | [PAGE#CLUSTER]                       | [STATUS] |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :----------------------------------- | :------: |
|   [1]   | Companion lock-scope decision: admit the sub-3.13/sub-3.15 companion environment (via `tool.uv.environments` fork or a dedicated companion lock) so `assay api` reflects grpcio/grpcio-tools/protobuf and the five geometry pins (ifcopenshell/open3d/small-gicp/topologicpy/compas) — blocks the geometry + runtime SPIKE owners | `runtime/server-host#SERVE` · `geometry/ifc-companion#DAEMON` | BLOCKED  |
|   [2]   | Two-distribution split: a core `>=3.15` project and an isolated companion project each carry a `[build-system]` (hatchling) so the core ships an installable wheel and the companion registers the daemon console entry under its lowered floor; resolves the `package = false` packaging gap | `runtime/evidence#ENTRY`             | BLOCKED  |
|   [3]   | cp315 wheel floor: data + compute scientific distributions lack cp315 wheels (numpy/scipy/pint/numba/jax/onnx/scikit-learn; deploy-asset set scikit-fem/python-flint/optimistix/pymc/arviz) — blocks the data/compute `.api` catalogues | `data/columnar-query#SCAN` · `compute/array-solver#SOLVER` | BLOCKED  |
|   [4]   | artifacts image toolchain: pillow has no cp315 wheel, transitively blocking pikepdf/reportlab/weasyprint/python-pptx/matplotlib | `artifacts/visual-export#PREVIEW`    | BLOCKED  |
|   [5]   | ideation-page pin admission: the new distributions absent from the root `pyproject.toml` (rigor/differentiable/simframe/inference/lakehouse/tensorstore/parametric/interchange/conformance/color/imaging/orchestration/notebook/interop/graph), each carrying its interpreter-floor + license posture — gates every ideation MISSING row | `compute/rigor#CERTIFY` · `geometry/interchange#INGEST` | BLOCKED  |

## [2]-[CONTENT_IDENTITY]

| [INDEX] | [ITEM]                                                                                                                                                                                                            | [PAGE#CLUSTER]                      | [STATUS] |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :---------------------------------- | :------: |
|   [1]   | An XxHash128 distribution reproducing the digest byte-identically is not yet pinned; `ContentIdentity` derives the seed over the format/deflection/tolerance string and proves byte-identity at the Tier-0 content-identity seam | `runtime/content-identity#IDENTITY` |  SPIKE   |
|   [2]   | provider-fingerprint canonicalization (BLAS vendor/thread-count/SIMD-level/library-version/RNG-state byte layout) proves member byte-identity at the Tier-0 determinism seam; absent the proof the determinism closure federates by content key rather than extending the same domain | `runtime/content-identity#IDENTITY` |  SPIKE   |

## [3]-[API_CATALOGUE_GAPS]

| [INDEX] | [ITEM]                                                                                                                                                                                                          | [PAGE#CLUSTER]                       | [STATUS] |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------------------------------- | :------: |
|   [1]   | runtime companion wire (grpcio/grpcio-tools/protobuf) catalogue fills once the companion floor admits the sub-3.13 environment and the `server-host#SERVE` members confirm against the inbound contract | `runtime/server-host#SERVE`          | BLOCKED  |
|   [2]   | data floor catalogue: pyarrow/geopandas/shapely/pyogrio/pyproj/rasterio/h5py resolve on `>=3.15` via the scientific source-build env; connectorx/rhino3dm stay fenced until upstream wheels or the companion floor land | `data/columnar-query#SCAN`           | BLOCKED  |
|   [3]   | compute floor catalogue: scipy is a default-floor source build; numba/jax/onnx/onnxruntime/scikit-learn ride the `python_version<'3.15'` marker on an upstream CPython ceiling, catalogued separately on the marker floor | `compute/array-solver#SOLVER`        | BLOCKED  |
|   [4]   | `.api` catalogue gap: recently admitted pins lacking `api-*.md` (arro3-core, ibis-framework, sparse, pdal, nanoarrow, narwhals, zarr, icechunk, laspy, pye57, xxhash, scikit-fem, scikit-image, numpyro, optimistix, awkward, cubed, arviz, netcdf4, rustworkx, python-flint, lonboard, pyhanko, colour-science, fonttools, papermill, nbclient) — dev/test tooling stays uncatalogued; reconcile small-gicp/small_gicp naming | `api-catalogues.md#[1]`              | QUEUED   |

## [4]-[PAGE_GRADE]

| [INDEX] | [ITEM]                                                                                                                                                                                         | [PAGE#CLUSTER]              | [STATUS] |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------- | :------: |
|   [1]   | cold-grade every package-local planning page against the standard and the Python stack doctrine after the marker-floor catalogues fill; drive every owner-registry `SPIKE` to `FINALIZED` by folding verified member spellings back into the fences | `README.md#[2]`            | QUEUED   |
