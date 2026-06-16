# [PYTHON_API_OWNERS]

Runtime:
- concurrency: `anyio`
- validation: `pydantic`, `pydantic-settings`, `msgspec`, `beartype`
- rails: `expression`, `stamina`
- observability: `structlog`, `opentelemetry-api`, `opentelemetry-sdk`, `opentelemetry-exporter-otlp-proto-http`, `opentelemetry-instrumentation-logging`
- resources: `fsspec`, `s3fs`, `gcsfs`, `universal-pathlib`
- transport: `httpx`, `asyncssh`
- automation: `watchfiles`, `aiocron`

Data:
- columnar: `polars`, `pyarrow`, `pandas`, `dask`, `xarray`
- query: `duckdb`, `adbc-driver-manager`, `connectorx`, `deltalake`
- geospatial: `geopandas`, `shapely`, `pyogrio`, `pyproj`
- graph: `networkx`
- AEC and mesh: `rhino3dm`, `h5py`, `meshio`, `trimesh`
- pending exchange: `ifcopenshell`, `topologicpy`, `open3d`, `vtk`, `pyvista`, `rasterio`, `compas`, `specklepy`

Compute:
- arrays: `numpy`, `xarray`, `dask`
- solvers: `scipy`, `sympy`
- units and uncertainty: `pint`, `uncertainties`
- pending accelerators: `jax`, `numba`
- pending model assets: `onnx`, `onnxruntime`, `scikit-learn`

Artifacts:
- PDF: `pymupdf`, `pypdf`, `pikepdf`, `pypdfium2`, `reportlab`, `weasyprint`
- Office and structured documents: `python-docx`, `python-pptx`, `openpyxl`, `lxml`, `ruamel-yaml`, `tomlkit`
- visuals: `altair`, `vl-convert-python`, `plotly`, `kaleido`, `matplotlib`
- images and codes: `pillow`, `qrcode`, `python-magic`
- compression: `zstandard`, `lz4`, `brotli`, `py7zr`
