# [PYTHON_TASKLOG]

Open work only. Closed rows move into the owning page or disappear. Owner state lives on each package charter DENSITY_BAR, never here.

## [1]-[INTERPRETER_FLOOR]

[PY_FLOOR_001]:
- Owner: branch / manifest
- Work: `requires-python='>=3.15'` makes every `python_version<'3.13'` and `<'3.15'` marker DEAD under uv universal resolution — all marker-gated pins are pruned from `uv.lock`, so the geometry-companion own-lock-scope the topology mandates is unreachable under a single floor. The named resolution (`architecture.md#INTERPRETER_FLOORS`) is two distributions: a core `>=3.15` project plus an isolated companion project with its own `[build-system]`, a lowered `requires-python`, and `tool.uv.required-environments` forks, so the marker-gated and sub-3.13 pins resolve in the companion lock while the core `>=3.15` floor and the ty/ruff 3.15 configs hold exactly. This row and `PY_FLOOR_002` are one decision (core/companion split), executed together.
- Exit: the two-distribution split lands, the sub-3.13 and sub-3.15 environments resolve their marker-gated pins into the companion lock, and `assay api query` reflects them in place.

[PY_FLOOR_002]:
- Owner: branch / packaging
- Work: `[tool.uv] package = false` produces no installable distribution; a branch positioned as adoptable-if-public ships no wheel, no `[build-system]`, no entry-point registration, and the companion daemon cannot ship as a console entry. The two-distribution split named at `architecture.md#INTERPRETER_FLOORS` adds a `[build-system]` (hatchling) to each project so the core ships an installable wheel and the companion project registers the daemon console entry under its own lower floor. Executed jointly with `PY_FLOOR_001`.
- Exit: both distributions produce installable artifacts and the companion `Entrypoint` registers a console entry.

## [2]-[CONTENT_IDENTITY]

[PY_HASH_001]:
- Owner: runtime
- Work: an XxHash128 distribution reproducing the C# `System.IO.Hashing.XxHash128.HashToUInt128(bytes, seed)` digest byte-identically is not yet pinned in the root manifest; `content-identity#IDENTITY` `_digest128`/`_digest64` bind to it at admission. The seed-derivation (a 64-bit XxHash3 digest over the format/deflection/tolerance string) is proven against the C# `InterchangeIdentity.Seed` so a cross-setting hit never collides.
- Exit: the XxHash128 pin is admitted, catalogued, and the seed parity is proven against the C# output.

## [3]-[API_CATALOGUE]

[PY_API_001]:
- Owner: runtime
- Work: the companion server wire (`grpcio`, `grpcio-tools`, `protobuf`) is catalogued but rides the `python_version<'3.13'` floor; under `>=3.15` `grpcio`/`protobuf` are present only as marker-free transitive deps of `specklepy` (major 6, not the pinned major 7) and `grpcio-tools` is absent. The `server-host#SERVE` fence members confirm against the C# `ComputeService`/`ArtifactSync` descriptors once the floor decision admits the sub-3.13 environment.
- Exit: `PY_FLOOR_001` admits the floor, `grpcio-tools` installs, and `assay api query` fills the full member surface.

[PY_API_002a]:
- Owner: data
- Work: 19 data distributions are absent from the `>=3.15` lock (polars, pyarrow, pandas, deltalake, duckdb, adbc-driver-manager, connectorx, dask, xarray, geopandas, shapely, pyogrio, pyproj, rasterio, rhino3dm, meshio, trimesh, h5py, pandera); `networkx` is captured. Each page carries the import name and the wheel-floor capture gap; no members are invented while the cp315 wheel is missing. `rasterio` additionally rides the `python_version<'3.15'` marker.
- Exit: a cp315 wheel or the marker-floor environment admits each distribution, then `assay api query` fills public types, entrypoints, and implementation law.

[PY_API_003]:
- Owner: compute
- Work: `numba`/`jax`/`onnx`/`onnxruntime`/`scikit-learn` ride the `python_version<'3.15'` marker (no cp315 wheel); `numpy`/`scipy`/`pint`/`uncertainties` carry no cp315 wheel; `sympy` is cp315-reflected. Fill the marker-floor catalogues once the environment installs them.
- Exit: the marker-floor environment admits each distribution and `assay api query` fills the catalogue.

[PY_API_004]:
- Owner: artifacts
- Work: `pillow` has no cp315 wheel and a blocked source build (missing llvm-ar then absent libjpeg/zlib headers); it transitively blocks `pikepdf`/`reportlab`/`weasyprint`/`python-pptx`/`matplotlib`. `pyvista`/`vtk` ride the `python_version<'3.13'` native floor. The other 19 are cp315-reflected.
- Exit: `pillow` installs (a cp315 wheel publishes or the host provisions libjpeg/zlib + an archiver) and the native VTK floor is admitted, then the six and the two re-reflect.

## [4]-[PAGE_GRADE]

[PY_DOC_001]:
- Owner: package planning
- Work: cold-grade every package-local planning page against the suite standard and the upgraded `docs/stacks/python/` after the marker-floor catalogues fill; drive every DENSITY_BAR `SPIKE` to `FINALIZED` by folding verified member spellings back into the fences.
- Exit: a cold read of each package corpus surfaces nothing and the residual `SPIKE` set is exactly the live-host/companion-floor probes.

## [5]-[FORWARD_FRONTIER]

[PY_NEXT_001]:
- Owner: branch (next loop)
- Work: the object-graph DAG diff concern (Speckle-class) warrants its own owned page once a named consumer and the C# Persistence diff algebra land; it is forward-emitted, not forced into this topology because the diff algebra sits one boundary over in C# Persistence.
- Exit: a named consumer admits the concern as a new owned page.
