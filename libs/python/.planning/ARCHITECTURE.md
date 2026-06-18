# [PYTHON_BRANCH_ARCHITECTURE]

The Python branch domain map: five host-free peer packages of the science/compute/data/geometry/IFC companion, the dependency direction across them stated once, and the interpreter floor the whole branch sits below. The per-package sub-domain structure, owners, and charters live on each package `ARCHITECTURE.md` and are not restated here; this map carries only the branch-altitude topology. Cross-language wires live on the folder tasks that build them and in the cross-`libs/` `IDEAS.md`/`TASKLOG.md`, never in a branch seam ledger.

## [1]-[PACKAGE_MAP]

Five packages, one foundation and four consumers. `runtime` mints the shared value shapes; `compute`, `data`, `geometry`, and `artifacts` are independent peer producers that compose those shapes at their boundary.

```text codemap
libs/python/
├── runtime/      # the host-free execution foundation: the one content-identity owner, the one boundary-fault + Result/Option rail, the one resilience policy, caller-owned context/settings admission, resource roots + bounded anyio lanes, local receipts + the contributor port, the inbound companion gRPC server-runtime + credential axis, external-API/structural-parsing evidence, the private daemon entrypoint
├── compute/      # offline scientific evidence that graduates: Array-API array admission, one route-discriminated solver folding one receipt, autodiff sensitivity, certified-enclosure validated numerics, signal processing, symbolic codegen, unit-bearing uncertainty, design-of-experiments + run history, model-asset validation, Bayesian inference, the graduation rail + the C# stub codegen
├── data/         # portable data interchange: typed dataset refs, columnar lazy/streaming scan + egress, the transactional table-format lakehouse, cross-engine relational query, a data-contract gate, dataframe-agnostic interop over a pyarrow-free Arrow carrier, vector + raster geospatial, graph payloads, chunked tensor stores, mesh-file exchange
├── geometry/     # the host-free geometry + IFC/BIM companion and the load-bearing cross-boundary owner: the IfcOpenShell GLB tessellation daemon, IFC analysis + buildingSMART validation, point-cloud/3D-scan registration, non-manifold topology, AEC computational geometry, the planned CAD-STEP hop and the shared mesh-utility
└── artifacts/    # the self-contained artifact-production utility: documents/PDF/Office/structured-text, reproducible notebook reports, publication tables, 2D charts + offscreen 3D scientific visuals, archival signed PDFs, color-managed assets, raster previews, compressed bundles, all under one kind-discriminated ArtifactReceipt
```

## [2]-[DEPENDENCY_DIRECTION]

The direction is stated once, here. `runtime` is the foundation: it mints `ContentIdentity`/`ContentKey`, `BoundaryFault`/`RuntimeRail`, `Retry`, `RuntimeContext`/`SettingsAdmission`, `ResourceRoot`/`TransportResource`, `LanePolicy`/`StagePlan`, `Receipt`/`ReceiptContributor`, and `ServerHost`/`Credential`, and references no sibling. `compute`, `data`, `geometry`, and `artifacts` compose those owners at their boundary as settled vocabulary and never re-mint a second content-identity, receipt, retry, transport, or wire owner. No package imports another package's interior.

Two consumer-to-consumer compositions exist and are named here so neither is read as an interior import: `compute` composes `data` dataset and labelled-array shapes as study inputs, and `compute` accepts `geometry` evidence through the graduation `HandoffAxis` geometry case. Both are boundary compositions of a published shape, not interior coupling — `compute` re-catalogues neither, and `geometry` evidence crosses only on the single graduation rail. Every other cross-folder fact rides a folder task, never a per-folder seam ledger.

The cross-language wire — the companion gRPC contract the geometry daemon serves, the content-identity seed parity with C#, the two-hop IFC/STEP tessellation rail, and the graduation-evidence seam — couples Python to C# only at the wire and lives on the owning folder tasks and the cross-`libs/` ledger, never on a Python-branch surface.

## [3]-[INTERPRETER_FLOOR]

The branch runs a `>=3.15` core on the normal-GIL CPython build for `runtime`, `compute`, `data`, and `artifacts`, with one sanctioned divergence: a `python_version<'3.13'` companion floor homing the native geometry/IFC and gRPC-server stack. The companion floor is provided by the Forge companion lane (`forge-companion-env`, python312, source-building the companion native libs), not a Rasm-owned environment gate to admit: it carries the geometry compiled geometry/IFC cores (`ifcopenshell`, `open3d`, `small-gicp`, `topologicpy`, isolating the copyleft `ifcopenshell` wheel at the process boundary) and the `runtime` inbound `grpcio.aio` server-host leg (`grpcio`/`grpcio-tools`/`protobuf`). Two floors are Rasm-owned and manifest-declared, distinct from the Forge lane: the `python_version<'3.15'` gated band (`compas`/`compas_dr`/`compas_tna`/`manifold3d` and the scientific stack) and the `python_version<'3.13'` artifacts native render path (`vtk`/`pyvista`). The OCCT CAD-STEP reader (`pythonocc-core`) has no PyPI distribution and is an honest deferral. Every package consumes the floor as settled.
