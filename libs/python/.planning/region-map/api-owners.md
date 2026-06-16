# [PYTHON_API_OWNERS]

One package owner per distribution. Versions live only in the root `pyproject.toml`; this map carries the distribution-to-owner-to-rail routing, never a pin.

## [1]-[RUNTIME]

- concurrency + lanes: `anyio`, `watchfiles`, `aiocron`
- validation: `pydantic`, `pydantic-settings`, `msgspec`, `beartype`
- rails + resilience: `expression`, `stamina`
- observability + telemetry: `structlog`, `opentelemetry-api`, `opentelemetry-sdk`, `opentelemetry-exporter-otlp-proto-http`, `psutil`
- resources: `fsspec`, `s3fs`, `gcsfs`, `universal-pathlib`
- transport: `httpx`, `asyncssh`, `specklepy`
- companion server wire: `grpcio`, `grpcio-tools`, `protobuf`
- entrypoint grammar: `cyclopts`
- structural-parsing evidence: `tree-sitter`, `tree-sitter-python`, `tree-sitter-typescript`

## [2]-[DATA]

- columnar: `polars`, `pyarrow`, `pandas`, `dask`, `xarray`
- query: `duckdb`, `adbc-driver-manager`, `connectorx`, `deltalake`
- vector geospatial: `geopandas`, `shapely`, `pyogrio`, `pyproj`
- raster geospatial: `rasterio`
- graph: `networkx`
- mesh-file exchange: `rhino3dm`, `meshio`, `trimesh`, `h5py`
- contract gate: `pandera`

## [3]-[GEOMETRY]

- IFC companion + analysis: `ifcopenshell`
- scan processing: `open3d`, `small-gicp`
- geometry algebra: `topologicpy`, `compas`

## [4]-[COMPUTE]

- arrays: `numpy` (composes data `xarray`/`dask` as study-input shapes)
- solvers + symbolic: `scipy`, `sympy`
- accelerators: `numba`, `jax`
- units + uncertainty: `pint`, `uncertainties`
- model assets: `onnx`, `onnxruntime`, `scikit-learn`

## [5]-[ARTIFACTS]

- document/PDF: `pymupdf`, `pypdf`, `pikepdf`, `pypdfium2`, `reportlab`, `weasyprint`
- Office + structured text: `python-docx`, `python-pptx`, `openpyxl`, `lxml`, `ruamel-yaml`, `tomlkit`
- report templating: `jinja2`
- 2D visualization: `altair`, `plotly`, `matplotlib`, `vl-convert-python`, `kaleido`
- 3D scientific visualization: `pyvista`, `vtk`
- preview: `pillow`, `qrcode`, `python-magic`
- compression: `zstandard`, `lz4`, `brotli`, `py7zr`

## [6]-[PROPOSED] — next-loop ideation distributions (2026-06-16)

Distribution-to-owner routing reserved for the ideation pages (`.artifacts/planning-briefs/python-ideation.md`). State `[PROPOSED]`: the distribution is NOT yet admitted to the root `pyproject.toml`; the admission task is `PY_FLOOR_003` in the TASKLOG. Versions still live only in the root manifest once admitted; this map carries routing, never a pin.

- compute `rigor` (`PY_RIGOR_001`): `python-flint` (arb ball arithmetic + univariate Taylor series ONLY — multivariate Taylor models / verified ODE / rigorous global-opt have NO pinned PyPI owner, deferred) — LGPL posture review required (FLINT/Arb is LGPL-2.1+); marker-floor gating likely (no cp315 wheel — confirm at admission).
- compute `differentiable` (`PY_DIFF_001`): `optimistix` (Gauss-Newton/LM root + minimisation for the discrete-adjoint outer loop) — new; rides the jax marker floor (`PY_API_003`, no cp315 wheel). The forward is `PY_SIM_001`'s scikit-fem (numpy/scipy, NOT jax-traceable) via a hand-implemented discrete adjoint; a jax-native FEM (future jax-FEM pin) would be a separate forward object, not a trace through scikit-fem.
- compute `simframe` (`PY_SIM_001`): `scikit-fem` (weak-form assembly + transposed-form assembly for the `PY_DIFF_001` adjoint), `netcdf4` (CF container) — new; `meshio`/`h5py` already catalogued under data. ParaView Catalyst v2 / VTK-m in-situ rides the native VTK floor (`python_version<'3.13'`, same floor as `pyvista`/`vtk`).
- compute `inference` (`PY_INFER_001`): `numpyro` (MCMC/HMC/NUTS/VI, jax marker floor `PY_API_003`), `pymc`, `arviz` (convergence diagnostics + model comparison) — new. Classical inferential statistics, not the excluded generative/deep-learning AI frontier.
- compute `arrays` gpu Substrate row (`PY_FLOOR_003`): `cupy` (CUDA device arrays) — new, marker-floor. The `gpu` `Substrate` enum value stays OUT of the closed union until this pin lands (unbacked enum = FENCE_LAW UNDONE).
- data `tensorstore` (`PY_TENSOR_001`): `icechunk` (transactional Zarr store), `zarr` (v3 sharding), `cubed` (out-of-core blockwise), `awkward` (ragged arrays) — new; `xarray`/`dask` already catalogued under data. STAC/DGG cases gated on `pystac`+`pystac-client`+`odc-stac` (STAC EO collections) and `h3`+`s2sphere` (DGG binning) — new, OUT of the `cube` surface until admitted. ICECHUNK MATURITY: transaction/conflict-resolution semantics on concurrent commit are still hardening; the `ContentIdentity`-keyed conflict-detection path is the maturity risk — capture-gap note alongside the LGPL/marker-floor pins.
- geometry `parametric` (`PY_PARA_001`): `python-solvespace` (DOF constraint solver) — new, native build; `specklepy` object-graph transport already a runtime `TransportResource` row.
- geometry `interchange` (`PY_INGEST_001`): `pythonocc-core` (native OpenCascade STEP/IGES/AP242 B-rep), `laspy`+`pye57` (E57/LAS/LAZ point clouds), `pdal` (point-cloud pipeline) — new, native builds + marker-floor likely. Revit/Navisworks bridges and IFC5 parsing ride this owner. The in-process native-CAD/scan-ingest plane the C# stack delegates to Python (`aggressive-ideation.md#[4]`).
- artifacts `conformance` (`PY_CONF_001`): `pyhanko` (PAdES signing + RFC-3161 timestamp), `fonttools` (font-subset completeness) — new; veraPDF is a JVM tool reached over a process boundary (no Python pin — record as an external-tool seam, not a distribution). `pikepdf`/`pymupdf` already catalogued under artifacts.
- runtime `orchestration`/`StagePlan` (`PY_ORCH_001`): `croniter` (cron `Trigger` row) — new; `watchfiles` already catalogued under runtime concurrency.
- runtime `notebook` (`PY_NB_001`): `papermill`/`nbclient` (parametrized headless execution) — new.
- data `columnar-query` interchange carrier (`PY_INTEROP_001`): `narwhals` (backend-agnostic dataframe ops) + `nanoarrow` (Arrow PyCapsule C Data Interface) — new; `pyarrow` already catalogued.
- data `graph-mesh` accelerated network science (`PY_GRAPH_001`): `rustworkx` (accelerated networkx backend) — new; `networkx` already captured.
- artifacts `imaging` (`PY_IMG_001`): `scikit-image` — new; `scipy.ndimage` rides the compute `scipy` row. Marker-floor gating likely (confirm cp315 wheel at admission).

## [7]-[REASSIGNMENT_RECORD]

- `specklepy`: data to runtime `TransportResource` (remote transport, not a data owner).
- `pyvista`, `vtk`: data to artifacts `VisualSpec` 3D cluster.
- `rasterio`: data `RasterGeoClaim` axis, split from the vector `GeoClaim`.
- `xarray`, `dask`: owned by data (ingress catalogue); compute composes them and carries no duplicate `.api` page.
- `opentelemetry-instrumentation-logging`: removed (self-negating; diagnostics ride structlog to stderr).
- `cyclopts`, `psutil`, `tree-sitter*`: assigned to runtime owners (Entrypoint, Receipt observability, ApiEvidence structural parsing); no new public command surface.
