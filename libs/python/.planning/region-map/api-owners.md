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

## [6]-[REASSIGNMENT_RECORD]

- `specklepy`: data to runtime `TransportResource` (remote transport, not a data owner).
- `pyvista`, `vtk`: data to artifacts `VisualSpec` 3D cluster.
- `rasterio`: data `RasterGeoClaim` axis, split from the vector `GeoClaim`.
- `xarray`, `dask`: owned by data (ingress catalogue); compute composes them and carries no duplicate `.api` page.
- `opentelemetry-instrumentation-logging`: removed (self-negating; diagnostics ride structlog to stderr).
- `cyclopts`, `psutil`, `tree-sitter*`: assigned to runtime owners (Entrypoint, Receipt observability, ApiEvidence structural parsing); no new public command surface.
