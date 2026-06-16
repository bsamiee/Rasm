# [PYTHON_ARCHITECTURE]

`libs/python` is a root-manifest Python library branch of five first-class packages re-derived from first principles around the load-bearing cross-boundary concern (geometry + IFC interchange). Package folders carry README, `.planning`, `.api`, and future source directly under the package root; the geometry package and the companion-server + accelerator stack ride a separate interpreter floor.

## [1]-[TOPOLOGY]

The codemap shows the no-source campaign layout and the admitted future source roots.

```text codemap
libs/python/
├── .planning/
│   ├── README.md
│   ├── architecture.md
│   ├── campaign-method.md
│   ├── api-catalogues.md
│   ├── FEATURES.md
│   ├── TASKLOG.md
│   └── region-map/
│       ├── page-regions.md
│       ├── owner-symbols.md
│       ├── seam-splits.md
│       └── api-owners.md
├── runtime/          (>=3.15 core floor)
│   ├── README.md · .planning/ · .api/
├── data/             (>=3.15 core; rasterio marker <3.15)
│   ├── README.md · .planning/ · .api/
├── geometry/         (companion floor python_version<'3.13')
│   ├── README.md · .planning/ · .api/
├── compute/          (>=3.15 core; accelerator/model rows marker <3.15)
│   ├── README.md · .planning/ · .api/
└── artifacts/        (>=3.15 core; pyvista/vtk marker <3.13)
    ├── README.md · .planning/ · .api/
```

Text equivalent: `libs/python/.planning` owns branch law and the region ledger; each package owns its local `.planning` and `.api`; future Python source lands directly in `libs/python/<package>` rather than behind a nested source namespace.

## [2]-[INTERPRETER_FLOORS]

- Core floor `requires-python='>=3.15'`: `runtime`, `data` core, `compute` core, `artifacts` core — pure-Python and cp315-wheel surfaces.
- Marker `python_version<'3.15'`: `rasterio` (data), `numba`/`jax`/`onnx`/`onnxruntime`/`scikit-learn` (compute) — no cp315 wheel published.
- Marker `python_version<'3.13'`: the `geometry-companion` group (`ifcopenshell`, `open3d`, `small-gicp`, `topologicpy`, `compas`, `grpcio`, `grpcio-tools`, `protobuf`) and `artifacts` `pyvista`/`vtk` — the native/OCCT/VTK interpreter floor. This is the one sanctioned divergence; the isolated companion environment also keeps the LGPL-3.0-or-later `ifcopenshell` wheel out of the MIT/Apache library lock and homes the gRPC server stack.
- Floor-realization decision (named, open at `PY_FLOOR_001`/`PY_FLOOR_002`): a single `requires-python='>=3.15'` floor prunes every `python_version<'3.15'` and `<'3.13'` marker under uv universal resolution, so the marker-gated pins never enter `uv.lock` and the companion's own lock scope is unreachable as written. The realization is two distributions — a core `>=3.15` project and an isolated companion project carrying its own `[build-system]`, a lowered `requires-python`, and `tool.uv.required-environments` forks — which simultaneously resolves the LGPL boundary (the `ifcopenshell` wheel stays in the companion lock, never the core MIT/Apache lock) and the `package = false` packaging gap. The `>=3.15` floor and the ty/ruff 3.15 configs hold exactly until this decision lands; the topology above is the target shape, gated on that decision.

## [3]-[DEPENDENCY_DIRECTION]

- `runtime` imports no first-wave Python package; it is the foundation every sibling consumes.
- `data` consumes runtime `ContentIdentity`, `ResourceRoot`, rails, and `ReceiptContributor`; it rejects compute/geometry/artifacts interiors.
- `geometry` consumes runtime `ServerHost`/`Credential` (the companion serve), `ContentIdentity` (the C# `InterchangeIdentity` seed), rails, lanes, and `ReceiptContributor`; it consumes data mesh-file shapes only as inputs.
- `compute` consumes runtime rails/receipts, data bundle shapes as study inputs, and geometry evidence through its own geometry handoff case; it composes the data `xarray`/`dask` catalogues and never re-catalogues them.
- `artifacts` consumes runtime `ContentIdentity`/rails/`ReceiptContributor` and accepts data/compute output as immutable bundle inputs.
- No Python package imports C# interiors, product host lifecycles, durable store repositories, bridge lifecycle code, or TypeScript UI state.

## [4]-[CROSS_BOUNDARY_SEAMS]

- Companion home of record: the IfcOpenShell tessellation companion is owned by `libs/python/geometry` (`geometry/.planning/ifc-companion.md#DAEMON`), riding the `python_version<'3.13'` companion floor; this supersedes the `libs/python/compute` location the C# interchange page assumed, so the C# `Rasm.Compute/.planning/interchange.md` companion references point at the geometry folder.
- Companion seam: geometry `IfcCompanion` is the inbound gRPC server (hosted by runtime `ServerHost`) speaking the existing C# `ComputeService`/`ArtifactSync` contract over the remote-lane `TRANSPORT_AXIS` UDS/InProcess leg; mechanics at `geometry/.planning/ifc-companion.md`, consequence at C# `remote-lane#TRANSPORT_AXIS`.
- Content seam: runtime `ContentIdentity` reproduces the C# `InterchangeIdentity` XxHash128 seed with deflection/tolerance folded into the key; data/geometry/artifacts consume it, never re-minting.
- Graduation seam: compute `GraduationReceipt` carries a geometry handoff case so geometry-package evidence (registration transform, reconstructed mesh, topology graph, form-finding) reaches the C# owner system through the one graduation rail.

## [5]-[MODULE_RULES]

- Package folders are direct future source roots; empty package markers are absent; public symbols live in the file that owns implementation.
- Heavy scientific, document, visualization, geometry, and native packages enter through owner-named boundary adapters only after `.api` evidence names their relevant surfaces.
- The root `pyproject.toml` owns dependency admission; package-local manifests stay absent.
