# [CSHARP_BRANCH_ARCHITECTURE]

The branch domain map of `libs/csharp` — the strata-ordered package roster and the one project-reference graph. Each package is a genuine higher-order domain; per-package structure, owners, and seams live in each folder `ARCHITECTURE.md`.

Each node is a package folder; the language's `.planning/` scaffold is authoring substrate, never part of the map.

## [1]-[PACKAGE_ROSTER]

```text codemap
libs/csharp/
├── Rasm/              # KERNEL        — RhinoCommon-aware geometry/numeric kernel
├── Rasm.Materials/    # AEC-DOMAIN    — Host-neutral profiles, appearance, and construction
├── Rasm.Bim/          # AEC-DOMAIN    — Host-neutral BIM object model and IFC/glTF/STEP exchange
├── Rasm.Fabrication/  # AEC-DOMAIN    — Host-neutral portable fabrication
├── Rasm.AppHost/      # APP-PLATFORM  — Runtime spine: lifecycle, clocks, config, ports, observability
├── Rasm.Compute/      # APP-PLATFORM  — Measured execution: tensor, model, solver, runtime
├── Rasm.Persistence/  # APP-PLATFORM  — Durable stores: store, schema, query, version, sync
├── Rasm.AppUi/        # APP-PLATFORM  — Avalonia product UI: shell, render, charts, editing, theme
├── Rasm.Rhino/        # HOST-BOUNDARY — RhinoCommon host APIs; references only Rasm
└── Rasm.Grasshopper/  # HOST-BOUNDARY — GH2 host APIs; references only Rasm
```

The eight planning-scoped packages carry a `.planning/` scaffold with the four index docs and design pages; `Rasm` keeps its scaffold only in the greenfield `Geometry/` robust-core because its `Vectors`/`Analysis`/`Domain` siblings are mature source. `Rasm.Rhino` and `Rasm.Grasshopper` are durable host-bound source with no `.planning/`; their open work the future app root composes rides this branch `TASKLOG.md`.

## [2]-[SEAMS]

```text seams
Rasm.AppHost      →  typescript:interchange  # [WIRE]: ReceiptEnvelope/HLC/Tenant + capability SDK
Rasm.Compute      →  typescript:interchange  # [WIRE]: proto suite wire + FaultDetail
Rasm.Persistence  →  typescript:interchange  # [WIRE]: OpLog/Snapshot CRDT wire
Rasm              →  python:runtime          # [CONTENT_KEY]: XxHash128 content-identity seed parity
Rasm.Bim          ⇄  python:geometry         # [TESSELLATION]: GLB/IFC tessellation rail — C# requests, Python evaluates
Rasm.AppHost      ⇄  python:runtime          # [WIRE]: gRPC companion server + capability invoke
Rasm.Compute      ←  python:compute          # [GRADUATION]: graduation evidence HandoffAxis
```

## [3]-[DEPENDENCY_DIRECTION]

Dependency is strictly upward through the strata; the graph is acyclic with the kernel at the base, `Rasm.AppHost` as the app-platform root, and the app shells at the future leaf. This is the project-reference graph the eight folders consume as settled vocabulary.

- `Rasm` references no sibling and is referenced by every C# stratum above it.
- The AEC-domain packages (`Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`) reference only `Rasm` and, minimally, each other. They are host-neutral and consume an app-platform capability only at a content-keyed wire seam (reproduced bit-identically), never through an upward project reference.
- `Rasm.AppHost` references nothing and is referenced by its app-platform siblings; `Rasm.Persistence` references `Rasm.AppHost`; `Rasm.Compute` references `Rasm`, `Rasm.AppHost`, and `Rasm.Persistence`; `Rasm.AppUi` references `Rasm`, `Rasm.AppHost`, `Rasm.Compute`, and `Rasm.Persistence`. `Rasm.Compute` references `Rasm.Persistence` for the cache/artifact/benchmark indexes; `Rasm.Persistence` never references `Rasm.Compute`.
- No host-neutral package (KERNEL, AEC-DOMAIN, APP-PLATFORM) references a HOST-BOUNDARY package. `Rasm.AppUi` is the app-platform consuming leaf and references no host boundary. `Rasm.Rhino` and `Rasm.Grasshopper` reference only `Rasm` and enter at the future app root that composes a live host.
- `Rasm.Bim` consumes the Compute content-identity seed and the tessellation rail as settled wire vocabulary, never through a `Rasm.Compute` project reference, so the AEC-domain never depends upward on the app-platform.
