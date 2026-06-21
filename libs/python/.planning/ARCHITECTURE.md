# [PYTHON_BRANCH_ARCHITECTURE]

The branch domain map of `libs/python` — the host-free peer packages of the science/compute/data/geometry/IFC companion. `runtime` mints the shared value shapes; `compute`, `data`, `geometry`, and `artifacts` compose them at their boundary.

Each node is a package folder; the language's `.planning/` scaffold is authoring substrate, never part of the map.

## [01]-[PACKAGE_MAP]

```text codemap
libs/python/
├── runtime/    # Host-free execution foundation four siblings compose
├── compute/    # Offline scientific evidence that graduates through one rail
├── data/       # Portable data interchange: tabular, spatial, gridded, graph
├── geometry/   # Host-free geometry + IFC/BIM companion and cross-boundary owner
└── artifacts/  # Self-contained artifact-production utility under one ArtifactReceipt
```

## [02]-[SEAMS]

```text seams
runtime   ←  csharp:Rasm              # [CONTENT_KEY]: XxHash128 content-identity seed decode parity
runtime   ⇄  csharp:Rasm.AppHost      # [WIRE]: gRPC ServerHost + capability invoke + trace/OTLP egress
runtime   ←  csharp:Rasm.Persistence  # [WIRE]: CRDT MessagePack op-log decode
geometry  ⇄  csharp:Rasm.Bim          # [TESSELLATION]: GLB/IFC tessellation companion
geometry  ⇄  csharp:Rasm.Compute      # [WIRE]: ComputeService/ArtifactSync GLB rail
compute   →  csharp:Rasm.Compute      # [GRADUATION]: graduation evidence HandoffAxis
```

## [03]-[DEPENDENCY_DIRECTION]

The direction is stated once, here. `runtime` is the foundation: it mints `ContentIdentity`/`ContentKey`, `BoundaryFault`/`RuntimeRail`, `Retry`, `RuntimeContext`/`SettingsAdmission`, `ResourceRoot`/`TransportResource`, `LanePolicy`/`StagePlan`, `Receipt`/`ReceiptContributor`, and `ServerHost`/`Credential`, and references no sibling. `compute`, `data`, `geometry`, and `artifacts` compose those owners at their boundary as settled vocabulary and never re-mint a second content-identity, receipt, retry, transport, or wire owner. No package imports another package's interior.

Two consumer-to-consumer compositions exist and are named here so neither is read as an interior import: `compute` composes `data` dataset and labelled-array shapes as study inputs, and `compute` accepts `geometry` evidence through the graduation `HandoffAxis` geometry case. Both are boundary compositions of a published shape, not interior coupling — `compute` re-catalogues neither, and `geometry` evidence crosses only on the single graduation rail. Every other cross-folder fact rides a folder task, never a per-folder seam ledger.

The cross-language wire — the companion gRPC contract the geometry daemon serves, the content-identity seed parity with C#, the two-hop IFC/STEP tessellation rail, and the graduation-evidence seam — couples Python to C# only at the wire and lives on the owning folder tasks and the cross-`libs/` ledger, never on a Python-branch surface.

## [04]-[INTERPRETER_FLOOR]

The branch runs a `>=3.15` core on the normal-GIL CPython build (`Py_GIL_DISABLED=0`) for `runtime`, `compute`, `data`, and `artifacts`, with one sanctioned divergence: a `python_version<'3.13'` companion floor homing the native geometry/IFC and gRPC-codegen stack. The companion floor is provided by the Forge companion lane (`forge-companion-env`, python312, source-building the companion native libs), not a Rasm-owned environment gate to admit: it carries the compiled geometry/IFC cores (`ifcopenshell`, `open3d`, `small-gicp`, `topologicpy`, isolating the copyleft `ifcopenshell` wheel at the process boundary). The gRPC stack splits by provenance: `grpcio-tools` (the `protoc` compiler) is the gated `; python_version<'3.15'` codegen companion (its bundled `protoc` build lacks a cp315 wheel); `protobuf` is a direct cp315-clean core dependency (the wire runtime); and `grpcio` (the `grpc.aio` runtime) is a direct CORE dependency that resolves on cp315 (already pulled transitively via `google-cloud-storage`) — so a `runtime` page needing the protobuf wire or the `grpc.aio` server leg sits on the core, and only proto codegen assumes the companion interpreter. `specklepy` is no longer a branch dependency: its transitive closure is `gql`/`httpx`/`ujson`/`pydantic`, pulling neither `grpcio` nor `protobuf`, and Speckle interchange terminates C#-side, so the package is removed from core and never carried the gRPC stack. Two floors are Rasm-owned and manifest-declared, distinct from the Forge lane: the `python_version<'3.15'` gated band (`compas`/`compas_dr`/`compas_tna`/`manifold3d` and the scientific stack) and the `python_version<'3.13'` artifacts native render path (`vtk`/`pyvista`). The OCCT CAD-STEP reader (`pythonocc-core`) has no PyPI distribution and is an honest deferral. Every package consumes the floor as settled.
