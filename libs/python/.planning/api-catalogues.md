# [PYTHON_API_CATALOGUES]

`.api` folders prevent underuse of external packages and forbid phantom members. A planning fence or future source module names an external API member only after the package owner captures verified evidence under `libs/python/<package>/.api/api-<distribution>.md`. The catalogue is decompile/reflection truth, never assumed-from-training.

## [1]-[EVIDENCE_RECORD]

Each distribution record carries:
- Distribution: root manifest package key.
- Import name: module path the owner imports (distribution-to-module remaps recorded where they differ, e.g. `grpcio` to `grpc`, `small-gicp` to `small_gicp`, `universal-pathlib` to `upath`).
- Owner: one package owner from `runtime`, `data`, `geometry`, `compute`, or `artifacts`.
- Version: installed reflection version, or the marker-gated interpreter floor when the project venv carries no wheel.
- Public types, entrypoints, implementation law: the verified member surface the owner composes — classes, functions, builders, codecs, engines, exporters, protocols — at full useful depth.
- Local admission: the owner-specific rail law preventing wrapper-renames and cross-package leakage.
- Capture gap: the interpreter-floor or wheel-availability note when the surface is verified out-of-band rather than from the project venv.

## [2]-[PACKAGE_FOLDERS]

- runtime: `libs/python/runtime/.api/` — concurrency, validation, rails, resilience, observability, resources, transport, automation, the companion server wire, content-identity inputs, and structural-parsing evidence.
- data: `libs/python/data/.api/` — columnar/query engines, vector + raster geospatial, graph, schema-contract validation, and mesh-file exchange.
- geometry: `libs/python/geometry/.api/` — the IfcOpenShell companion, point-cloud registration, and AEC computational geometry (companion floor `python_version<'3.13'`).
- compute: `libs/python/compute/.api/` — the numeric trio, accelerators, units/uncertainty, and model assets.
- artifacts: `libs/python/artifacts/.api/` — document/PDF/Office/structured-text, 2D + 3D visualization, preview, and compression.

## [3]-[CAPTURE_ORDER]

1. Admit or hold the dependency in the root `pyproject.toml`.
2. Reflect the installed distribution; for a marker-gated pin absent from the project venv, capture out-of-band on the matching interpreter floor and record the capture gap.
3. Record import names and the distribution-to-module remap; bind the distribution to one package owner in [API owners](region-map/api-owners.md).
4. Capture public types, entrypoints, and implementation law at the depth the owner composes; member spellings are verified, never invented.
5. Update the package-local planning page only with the owner consequence; the `.api` page owns the member table.

## [4]-[UNDERUSE_SCAN]

Reject any local function that renames a package API without adding owner policy, receipt projection, boundary admission, or cross-package translation. Reject any package claim that names a broad library but uses only the weakest stdlib-shaped subset. An admitted capability no owner exploits is a named gap, not an acceptable baseline.

## [5]-[CAPTURE_FLOOR_LAW]

The project venv runs `requires-python='>=3.15'`. Distributions with a cp315 wheel reflect in place via `assay api query`. Marker-gated distributions reflect out-of-band on their interpreter floor (`python_version<'3.15'` for the accelerator/raster set, `python_version<'3.13'` for the geometry-companion + VTK native set); their `.api` pages carry verified members plus the capture-gap note recording that the default `>=3.15` `assay api query` resolves no source against that interpreter until a fork/companion lock scope admits the lower environment.
