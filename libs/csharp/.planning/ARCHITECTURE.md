# [CSHARP_BRANCH_ARCHITECTURE]

The branch domain map of `libs/csharp` — the strata-ordered package roster and the one project-reference graph. Each package is a genuine higher-order domain; per-package structure, owners, and seams live in each folder `ARCHITECTURE.md`.

Each node is a package folder; the language's `.planning/` scaffold is authoring substrate, never part of the map.

## [01]-[PACKAGE_ROSTER]

```text codemap
libs/csharp/
├── Rasm/              # [KERNEL]        — RhinoCommon-aware geometry/numeric kernel
├── Rasm.Element/      # [AEC_DOMAIN]    — Lowest-AEC element seam: the ElementGraph property-graph + IElementProjection/IGraphConstraint contracts
├── Rasm.Materials/    # [AEC_DOMAIN]    — Host-neutral profiles, appearance, and construction
├── Rasm.Bim/          # [AEC_DOMAIN]    — Host-neutral BIM object model and IFC/glTF/STEP exchange
├── Rasm.Fabrication/  # [AEC_DOMAIN]    — Host-neutral HLR, CAM toolpath, kinematics, and nesting over a Clipper2 polygon floor
├── Rasm.AppHost/      # [APP_PLATFORM]  — Runtime spine: lifecycle, clocks, config, ports, observability
├── Rasm.Compute/      # [APP_PLATFORM]  — Measured execution: tensor, model, solver, runtime
├── Rasm.Persistence/  # [APP_PLATFORM]  — Durable stores: store, schema, query, version, sync
├── Rasm.AppUi/        # [APP_PLATFORM]  — Avalonia product UI: shell, render, charts, editing, theme
├── Rasm.Rhino/        # [HOST_BOUNDARY] — RhinoCommon host APIs; references only Rasm
└── Rasm.Grasshopper/  # [HOST_BOUNDARY] — GH2 host APIs; references only Rasm
```

The planning-scoped packages carry a `.planning/` scaffold with the four index docs and design pages; `Rasm.Element` is the lowest-AEC element seam the AEC peers and the app-platform stores depend up on. `Rasm.Rhino` and `Rasm.Grasshopper` are planning-scoped HOST-BOUNDARY packages like every sibling: each carries the four index docs, one package-root `.planning/`, and a folder `.api/` tier over its host assemblies (RhinoCommon + Eto; Grasshopper2 + Eto), referencing only the `Rasm` kernel.

## [02]-[SEAMS]

```text seams
Rasm.AppHost      →  typescript:core/interchange  # [WIRE]: ReceiptEnvelope/HLC/Tenant + capability SDK
Rasm.Compute      →  typescript:core/interchange  # [WIRE]: proto suite wire + FaultDetail
Rasm.Persistence  →  typescript:core/interchange  # [WIRE]: OpLog/Snapshot CRDT wire
Rasm              →  python:runtime               # [CONTENT_KEY]: XxHash128 content-identity seed parity
Rasm.Element      ⇄  python:geometry              # [WIRE]: ElementGraph content-key (one XxHash128 seed) + vocabulary the companion decodes, never re-mints
Rasm.Element      ⇄  typescript:core/interchange  # [WIRE]: ElementGraph/Node/Relationship content-keyed wire the TypeScript peer decodes
Rasm.Bim          ⇄  python:geometry              # [TESSELLATION]: GLB/IFC tessellation rail — C# requests, Python evaluates
Rasm.AppHost      ⇄  python:runtime               # [WIRE]: gRPC companion server + capability invoke
Rasm.Compute      ←  python:compute               # [GRADUATION]: graduation evidence HandoffAxis
```

## [03]-[DEPENDENCY_DIRECTION]

Dependency is strictly upward through the strata; the graph is acyclic with the kernel at the base, `Rasm.AppHost` as the app-platform root, and the app shells at the future leaf. This is the project-reference graph the planning-scoped folders consume as settled vocabulary.

- `Rasm` references no sibling and is referenced by every C# stratum above it.
- `Rasm.Element` (the lowest-AEC element seam) references only `Rasm` and names no IFC or provider package; it is referenced as the shared lower stratum by the AEC peers and by `Rasm.Persistence`/`Rasm.Compute`.
- The AEC-domain peers (`Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`) reference `{Rasm, Rasm.Element}` and never reference each other — alignment travels through the `IElementProjection`/`IGraphConstraint` seam contracts and the content-keyed wire, never an AEC-peer project reference. They are host-neutral and consume an app-platform capability only at a content-keyed wire seam (reproduced bit-identically), never through an upward project reference.
- `Rasm.AppHost` references nothing and is referenced by `Rasm.Compute` and `Rasm.AppUi`; `Rasm.Persistence` references only `Rasm` and `Rasm.Element` (it persists the `ElementGraph` as its system of record) — AppHost is a PORT peer whose adapters decode Persistence-owned coordination, cache, and frame shapes, never a Persistence downward reference; `Rasm.Compute` references `Rasm`, `Rasm.AppHost`, `Rasm.Persistence`, and `Rasm.Element` (its `Analysis` rail reads the concrete `ElementGraph` and writes `Assessment` nodes back); `Rasm.AppUi` references `Rasm`, `Rasm.AppHost`, `Rasm.Compute`, and `Rasm.Persistence` and stays pure-UI with no AEC-domain reference. `Rasm.Compute` references `Rasm.Persistence` for the cache/artifact/benchmark indexes; `Rasm.Persistence` never references `Rasm.Compute`.
- No host-neutral package (KERNEL, AEC-DOMAIN, APP-PLATFORM) references a HOST-BOUNDARY package. `Rasm.AppUi` is the app-platform consuming leaf and references no host boundary. `Rasm.Rhino` and `Rasm.Grasshopper` reference only `Rasm` and enter at the future app root that composes a live host.
- `Rasm.Bim` consumes the `Rasm` kernel content-identity seed (the seed-zero `XxHash128` content-hash entry the kernel owns) and the tessellation rail as settled wire vocabulary, never through a `Rasm.Compute` project reference, so the AEC-domain never depends upward on the app-platform.
