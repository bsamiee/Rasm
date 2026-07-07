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
runtime    ⇄  csharp:Rasm              # [CONTENT_KEY]: XxHash128 seed parity — both peers reproduce the one Domain/Identity seed
runtime    ⇄  csharp:Rasm.AppHost      # [WIRE]: gRPC ServerHost + capability invoke + W3C trace / OTLP egress
runtime    ⇄  csharp:Rasm.Persistence  # [WIRE]: CrdtOp / OpLogEntry MessagePack CRDT-delta op-log over the one wire vocabulary
compute    →  csharp:Rasm.Compute      # [GRADUATION]: HandoffAxis graduation evidence crosses outward only
data       ⇄  csharp:Rasm.Persistence  # [WIRE]: Substrait portable plan + Arrow record-batch query interchange
data       →  csharp:Rasm.Compute      # [SHAPE]: DOE dataset + GeoArrow study inputs
geometry   ⇄  csharp:Rasm.Bim          # [TESSELLATION]: GLB/IFC tessellation companion
geometry   ⇄  csharp:Rasm.Compute      # [WIRE]: ComputeService/ArtifactSync GLB rail
artifacts  →  csharp:Rasm.Persistence  # [CONTENT_KEY]: exchange/credential signed-artifact binding; durable warm-fill elision GATED on the AppHost wire
```

## [03]-[DEPENDENCY_DIRECTION]

The direction is stated once, here. `runtime` is the foundation: it mints `ContentIdentity`/`ContentKey`, `BoundaryFault`/`RuntimeRail`, `Retry`, `RuntimeContext`/`SettingsAdmission`, `ResourceRoot`/`TransportResource`, `LanePolicy`/`StagePlan`, `Receipt`/`ReceiptContributor`, and `ServerHost`/`Credential`, and references no sibling. `compute`, `data`, `geometry`, and `artifacts` compose those owners at their boundary as settled vocabulary and never re-mint a second content-identity, receipt, retry, transport, or wire owner. No package imports another package's interior.

Two consumer-to-consumer compositions exist and are named here so neither is read as an interior import: `compute` composes the `data` DOE-frame admission arm (`FrameAdmission`/`FrameInterop`) as a study input — labelled arrays are `xarray` carriers `compute`'s own array owner admits from any producer, with no data seam owed — and `compute` accepts `geometry` evidence through the graduation `HandoffAxis` geometry case. Both are boundary compositions of a published shape, not interior coupling — `compute` re-catalogues neither, and `geometry` evidence crosses only on the single graduation rail. Every other cross-folder fact rides a folder task, never a per-folder seam ledger.

The cross-language wire — the companion gRPC contract the geometry daemon serves, the content-identity seed parity with C#, the two-hop IFC/STEP tessellation rail, the Substrait/Arrow query interchange, and the graduation-evidence seam — couples Python to C# only at the wire. `[2]-[SEAMS]` records the package-level aggregate; the file-level detail lives on the owning folder `ARCHITECTURE.md`, its tasks, and the cross-`libs/` ledger.

## [04]-[ADMISSION_POLICY]

The root manifest owns interpreter admission, dependency groups, version bounds, and `python_version` markers. The branch default is a `>=3.15` normal-GIL CPython core; worker-lane exceptions stay in the root manifest until resolver evidence permits removal. Package-local docs name capability, entrypoints, boundaries, and exclusions, not installation rationale.

`protobuf` and `grpcio` are core runtime dependencies. `grpcio-tools` is codegen-only. Native rendering and OCCT/STEP concerns stay on their owning geometry/artifacts tasks and root-manifest admissions. `specklepy` is not a branch dependency.
